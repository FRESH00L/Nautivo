using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Nautivo.Tests;

[Collection("api")]
public class BusinessRuleTests(ApiFixture api)
{
    private static Task<HttpResponseMessage> TryEnroll(HttpClient client, Guid cruise, string name, decimal? total = null) =>
        client.PostAsJsonAsync($"/api/cruises/{cruise}/enrollments", new { participant = new { fullName = name }, total });

    private static async Task<JsonElement> Enroll(HttpClient client, Guid cruise, string name, decimal? total = null)
    {
        var response = await TryEnroll(client, cruise, name, total);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static async Task<JsonElement> CrewMember(HttpClient client, Guid cruise, Guid enrollment)
    {
        var detail = await client.GetFromJsonAsync<JsonElement>($"/api/cruises/{cruise}");
        return detail.GetProperty("enrollments").EnumerateArray().Single(e => e.GetProperty("id").GetGuid() == enrollment);
    }

    [Fact]
    public async Task Enrollment_over_capacity_is_rejected()
    {
        var (client, _, _) = await api.NewOrganization();
        var cruise = await client.CreateId("/api/cruises", TestData.Cruise(capacity: 1));

        await Enroll(client, cruise, "Pierwsza Osoba");
        Assert.Equal(HttpStatusCode.Conflict, (await TryEnroll(client, cruise, "Druga Osoba")).StatusCode);
    }

    [Fact]
    public async Task Concurrent_enrollments_never_exceed_capacity()
    {
        var (client, _, _) = await api.NewOrganization();
        var cruise = await client.CreateId("/api/cruises", TestData.Cruise(capacity: 3));

        var results = await Task.WhenAll(Enumerable.Range(0, 10).Select(i => TryEnroll(client, cruise, $"Osoba {i}")));

        Assert.Equal(3, results.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.Equal(7, results.Count(r => r.StatusCode == HttpStatusCode.Conflict));
    }

    [Fact]
    public async Task Route_needs_named_start_and_end_and_valid_coordinates()
    {
        var (client, _, _) = await api.NewOrganization();

        var onePort = await client.PostAsJsonAsync("/api/cruises", new { name = "Rejs", startDate = "2030-06-01", endDate = "2030-06-08", capacity = 4, price = 1000m, routePoints = TestData.Route("Gdynia") });
        Assert.Equal(HttpStatusCode.BadRequest, onePort.StatusCode);

        var badCoords = await client.PostAsJsonAsync("/api/cruises", new { name = "Rejs", startDate = "2030-06-01", endDate = "2030-06-08", capacity = 4, price = 1000m, routePoints = new object[] { new { name = "Gdynia", lat = 120.0, lng = 18.5 }, new { name = "Hel" } } });
        Assert.Equal(HttpStatusCode.BadRequest, badCoords.StatusCode);

        var route = new object[] { new { name = "Gdynia", lat = 54.519, lng = 18.553 }, new { name = "Hel" }, new { name = "Jastarnia" } };
        var id = await client.CreateId("/api/cruises", new { name = "Rejs", startDate = "2030-06-01", endDate = "2030-06-08", capacity = 4, price = 1000m, routePoints = route, yachtName = "Zefir", yachtModel = "Delphia 40", yachtLengthMeters = 12m });
        var detail = await client.GetFromJsonAsync<JsonElement>($"/api/cruises/{id}");

        Assert.Equal("Gdynia", detail.GetProperty("cruise").GetProperty("departurePort").GetString());
        Assert.Equal("Jastarnia", detail.GetProperty("cruise").GetProperty("arrivalPort").GetString());
        Assert.Equal("Zefir", detail.GetProperty("cruise").GetProperty("yachtName").GetString());
        Assert.Equal(3, detail.GetProperty("routePoints").GetArrayLength());
        Assert.Equal(54.519, detail.GetProperty("routePoints")[0].GetProperty("lat").GetDouble());
        // Bez terminu dopłaty terminem jest dzień startu.
        Assert.Equal("2030-06-01", detail.GetProperty("cruise").GetProperty("balanceDueDate").GetString());
    }

    [Fact]
    public async Task Same_participant_cannot_enroll_twice()
    {
        var (client, _, _) = await api.NewOrganization();
        var cruise = await client.CreateId("/api/cruises", TestData.Cruise());
        var person = await client.CreateId("/api/participants", new { fullName = "Jedna Osoba" });

        await client.CreateId($"/api/cruises/{cruise}/enrollments", new { participantId = person });
        var again = await client.PostAsJsonAsync($"/api/cruises/{cruise}/enrollments", new { participantId = person });
        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
    }

    [Fact]
    public async Task Payment_states_cover_none_deposit_paid_and_overpaid()
    {
        var (client, _, _) = await api.NewOrganization();
        var cruise = await client.CreateId("/api/cruises", TestData.Cruise(price: 1000m));
        var enrollment = (await Enroll(client, cruise, "Płacąca Osoba")).GetProperty("id").GetGuid();

        Assert.Equal("None", (await CrewMember(client, cruise, enrollment)).GetProperty("state").GetString());
        await client.CreateId($"/api/enrollments/{enrollment}/payments", new { amount = 300m });
        Assert.Equal("Partial", (await CrewMember(client, cruise, enrollment)).GetProperty("state").GetString());
        await client.CreateId($"/api/enrollments/{enrollment}/payments", new { amount = 700m });
        Assert.Equal("Paid", (await CrewMember(client, cruise, enrollment)).GetProperty("state").GetString());
        await client.CreateId($"/api/enrollments/{enrollment}/payments", new { amount = 50m });
        var member = await CrewMember(client, cruise, enrollment);
        Assert.Equal("Overpaid", member.GetProperty("state").GetString());
        Assert.Equal(1050m, member.GetProperty("paid").GetDecimal());
    }

    [Fact]
    public async Task Unpaid_after_due_date_is_overdue()
    {
        var (client, _, _) = await api.NewOrganization();
        var cruise = await client.CreateId("/api/cruises", TestData.Cruise(start: "2020-06-01", end: "2020-06-08"));
        var enrollment = (await Enroll(client, cruise, "Spóźniona Osoba")).GetProperty("id").GetGuid();
        Assert.Equal("Overdue", (await CrewMember(client, cruise, enrollment)).GetProperty("state").GetString());
    }

    [Fact]
    public async Task Crew_member_can_be_edited_and_removed()
    {
        var (client, _, _) = await api.NewOrganization();
        var cruise = await client.CreateId("/api/cruises", TestData.Cruise(price: 1000m));
        var enrollment = (await Enroll(client, cruise, "Stare Nazwisko", total: 800m)).GetProperty("id").GetGuid();
        await client.CreateId($"/api/enrollments/{enrollment}/payments", new { amount = 100m });
        Assert.Equal(800m, (await CrewMember(client, cruise, enrollment)).GetProperty("total").GetDecimal());

        (await client.PutAsJsonAsync($"/api/enrollments/{enrollment}", new { fullName = "Nowe Nazwisko", email = "nowy@example.com", phone = "+48 600", total = 900m })).EnsureSuccessStatusCode();
        var member = await CrewMember(client, cruise, enrollment);
        Assert.Equal("Nowe Nazwisko", member.GetProperty("fullName").GetString());
        Assert.Equal(900m, member.GetProperty("total").GetDecimal());

        (await client.DeleteAsync($"/api/enrollments/{enrollment}")).EnsureSuccessStatusCode();
        var detail = await client.GetFromJsonAsync<JsonElement>($"/api/cruises/{cruise}");
        Assert.Equal(0, detail.GetProperty("enrollments").GetArrayLength());
        Assert.Equal(0m, detail.GetProperty("cruise").GetProperty("collected").GetDecimal());
        // Osoba zostaje w bazie uczestników.
        Assert.Contains("Nowe Nazwisko", await client.GetStringAsync("/api/participants"));
    }

    [Fact]
    public async Task Price_change_updates_standard_price_but_keeps_discounts()
    {
        var (client, _, _) = await api.NewOrganization();
        var cruise = await client.CreateId("/api/cruises", TestData.Cruise(price: 1000m));
        var standard = (await Enroll(client, cruise, "Cena Standardowa")).GetProperty("id").GetGuid();
        var discounted = (await Enroll(client, cruise, "Cena Z Rabatem", total: 700m)).GetProperty("id").GetGuid();

        (await client.PutAsJsonAsync($"/api/cruises/{cruise}", TestData.Cruise(price: 1200m))).EnsureSuccessStatusCode();

        Assert.Equal(1200m, (await CrewMember(client, cruise, standard)).GetProperty("total").GetDecimal());
        Assert.Equal(700m, (await CrewMember(client, cruise, discounted)).GetProperty("total").GetDecimal());
    }

    [Fact]
    public async Task Costs_are_listed_summed_edited_and_deleted()
    {
        var (client, _, _) = await api.NewOrganization();
        var cruise = await client.CreateId("/api/cruises", TestData.Cruise());
        var charter = await client.CreateId($"/api/cruises/{cruise}/costs", new { category = "Czarter jachtu", description = "Delphia 40", amount = 5000m });
        await client.CreateId($"/api/cruises/{cruise}/costs", new { category = "Paliwo", amount = 450.50m });

        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync($"/api/cruises/{cruise}/costs", new { category = "Paliwo", amount = -1m })).StatusCode);

        var list = await client.GetFromJsonAsync<JsonElement>("/api/cruises");
        Assert.Equal(5450.50m, list[0].GetProperty("costs").GetDecimal());

        (await client.PutAsJsonAsync($"/api/costs/{charter}", new { category = "Czarter jachtu", amount = 4000m })).EnsureSuccessStatusCode();
        (await client.DeleteAsync($"/api/costs/{charter}")).EnsureSuccessStatusCode();
        var detail = await client.GetFromJsonAsync<JsonElement>($"/api/cruises/{cruise}");
        Assert.Equal(1, detail.GetProperty("costs").GetArrayLength());
        Assert.Equal(450.50m, detail.GetProperty("cruise").GetProperty("costs").GetDecimal());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Payment_must_be_positive(decimal amount)
    {
        var (client, _, _) = await api.NewOrganization();
        var cruise = await client.CreateId("/api/cruises", TestData.Cruise());
        var enrollment = (await Enroll(client, cruise, "Osoba Testowa")).GetProperty("id").GetGuid();

        var response = await client.PostAsJsonAsync($"/api/enrollments/{enrollment}/payments", new { amount });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Vessel_cannot_sail_two_overlapping_cruises_but_can_change_crew_same_day()
    {
        var (client, _, _) = await api.NewOrganization();
        var vessel = await client.CreateId("/api/vessels", TestData.Vessel());
        await client.CreateId("/api/cruises", TestData.Cruise(vessel, "2030-06-01", "2030-06-08"));

        var overlap = await client.PostAsJsonAsync("/api/cruises", TestData.Cruise(vessel, "2030-06-05", "2030-06-12"));
        Assert.Equal(HttpStatusCode.Conflict, overlap.StatusCode);

        var sameDayChange = await client.PostAsJsonAsync("/api/cruises", TestData.Cruise(vessel, "2030-06-08", "2030-06-15"));
        Assert.Equal(HttpStatusCode.Created, sameDayChange.StatusCode);
    }

    [Fact]
    public async Task Vessel_block_prevents_cruise_and_cruise_prevents_block()
    {
        var (client, _, _) = await api.NewOrganization();
        var vessel = await client.CreateId("/api/vessels", TestData.Vessel());
        (await client.PostAsJsonAsync($"/api/vessels/{vessel}/blocks", new { startDate = "2030-05-01", endDate = "2030-05-10", reason = "Serwis" })).EnsureSuccessStatusCode();

        var cruiseInBlock = await client.PostAsJsonAsync("/api/cruises", TestData.Cruise(vessel, "2030-05-05", "2030-05-12"));
        Assert.Equal(HttpStatusCode.Conflict, cruiseInBlock.StatusCode);

        await client.CreateId("/api/cruises", TestData.Cruise(vessel, "2030-07-01", "2030-07-08"));
        var blockInCruise = await client.PostAsJsonAsync($"/api/vessels/{vessel}/blocks", new { startDate = "2030-07-03", endDate = "2030-07-04", reason = "Serwis" });
        Assert.Equal(HttpStatusCode.Conflict, blockInCruise.StatusCode);
    }

    [Fact]
    public async Task Capacity_cannot_exceed_berths_or_drop_below_enrolled()
    {
        var (client, _, _) = await api.NewOrganization();
        var vessel = await client.CreateId("/api/vessels", TestData.Vessel(berths: 4));

        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/cruises", TestData.Cruise(vessel, capacity: 5))).StatusCode);

        var cruise = await client.CreateId("/api/cruises", TestData.Cruise(vessel, capacity: 2));
        await Enroll(client, cruise, "Pierwsza Osoba");
        await Enroll(client, cruise, "Druga Osoba");
        Assert.Equal(HttpStatusCode.Conflict, (await client.PutAsJsonAsync($"/api/cruises/{cruise}", TestData.Cruise(vessel, capacity: 1))).StatusCode);
    }

    [Fact]
    public async Task Records_in_use_cannot_be_deleted_but_cruise_can()
    {
        var (client, _, _) = await api.NewOrganization();
        var vessel = await client.CreateId("/api/vessels", TestData.Vessel());
        var cruise = await client.CreateId("/api/cruises", TestData.Cruise(vessel));
        var person = await client.CreateId("/api/participants", new { fullName = "Jedna Osoba" });
        var enrollment = await client.CreateId($"/api/cruises/{cruise}/enrollments", new { participantId = person });
        await client.CreateId($"/api/enrollments/{enrollment}/payments", new { amount = 100m });
        await client.CreateId($"/api/cruises/{cruise}/costs", new { category = "Paliwo", amount = 100m });

        Assert.Equal(HttpStatusCode.Conflict, (await client.DeleteAsync($"/api/vessels/{vessel}")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.DeleteAsync($"/api/participants/{person}")).StatusCode);

        // Rejs znika razem z zapisami, wpłatami i kosztami; osoba zostaje w bazie.
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/cruises/{cruise}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/cruises/{cruise}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/participants/{person}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/participants/{person}")).StatusCode);
    }

    [Fact]
    public async Task Dashboard_sums_payments_and_cruises()
    {
        var (client, _, _) = await api.NewOrganization();
        var cruise = await client.CreateId("/api/cruises", TestData.Cruise(price: 1000m, capacity: 4));
        var enrollment = (await Enroll(client, cruise, "Osoba Testowa")).GetProperty("id").GetGuid();
        await client.CreateId($"/api/enrollments/{enrollment}/payments", new { amount = 300m });

        var dashboard = await client.GetFromJsonAsync<JsonElement>("/api/dashboard");

        Assert.Equal(1, dashboard.GetProperty("cruisesTotal").GetInt32());
        Assert.Equal(300m, dashboard.GetProperty("earnedTotal").GetDecimal());
        Assert.Equal(300m, dashboard.GetProperty("earnedThisMonth").GetDecimal());
        Assert.Equal(700m, dashboard.GetProperty("toCollect").GetDecimal());
        Assert.Equal(1, dashboard.GetProperty("seatsTaken").GetInt32());
        Assert.Equal(4, dashboard.GetProperty("seatsTotal").GetInt32());
    }

    [Fact]
    public async Task Validation_errors_are_reported_in_polish()
    {
        var (client, _, _) = await api.NewOrganization();
        var response = await client.PostAsJsonAsync("/api/vessels", new { name = "", berths = 2, lengthMeters = 5, status = "InService" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Podaj nazwę jednostki", await response.Content.ReadAsStringAsync());
    }
}

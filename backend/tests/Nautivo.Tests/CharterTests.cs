using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Nautivo.Tests;

[Collection("api")]
public class CharterTests(ApiFixture api)
{
    public static object Unit(string name = "Skuter 1", string kind = "Jetski", decimal? rate = 500m) =>
        new { name, model = "Sea-Doo", lengthMeters = 3.5m, berths = 2, status = "InService", kind, charterDayRate = rate };

    public static object Booking(Guid vessel, string start, string end, decimal? total = null, string client = "Klient Czarteru") =>
        new { vesselId = vessel, startDate = start, endDate = end, total, participant = new { fullName = client, phone = "500600700" } };

    private static async Task<JsonElement> Charter(HttpClient client, Guid id) =>
        (await client.GetFromJsonAsync<JsonElement>("/api/charters")).EnumerateArray().Single(c => c.GetProperty("id").GetGuid() == id);

    [Fact]
    public async Task Price_is_day_rate_times_days_and_can_be_discounted()
    {
        var (client, _, _) = await api.NewOrganization();
        var unit = await client.CreateId("/api/vessels", Unit(rate: 400m));

        // Sobota–sobota: 7 płatnych dni, dzień zwrotu się nie liczy.
        var week = await client.CreateId("/api/charters", Booking(unit, "2031-06-07", "2031-06-14"));
        var charter = await Charter(client, week);
        Assert.Equal(7, charter.GetProperty("days").GetInt32());
        Assert.Equal(2800m, charter.GetProperty("total").GetDecimal());

        // Jeden dzień i kwota z rabatem.
        var day = await client.CreateId("/api/charters", Booking(unit, "2031-07-01", "2031-07-01", total: 350m));
        charter = await Charter(client, day);
        Assert.Equal(1, charter.GetProperty("days").GetInt32());
        Assert.Equal(350m, charter.GetProperty("total").GetDecimal());
        Assert.Equal(400m, charter.GetProperty("dayRate").GetDecimal());
    }

    [Fact]
    public async Task Overlapping_rental_is_rejected_but_handover_on_last_day_is_allowed()
    {
        var (client, _, _) = await api.NewOrganization();
        var unit = await client.CreateId("/api/vessels", Unit());
        await client.CreateId("/api/charters", Booking(unit, "2031-06-07", "2031-06-14", client: "Pierwszy"));

        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/charters", Booking(unit, "2031-06-10", "2031-06-12"))).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/charters", Booking(unit, "2031-06-01", "2031-06-20"))).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/charters", Booking(unit, "2031-06-07", "2031-06-07"))).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/charters", Booking(unit, "2031-06-13", "2031-06-15"))).StatusCode);

        // Odbiór w dniu zwrotu poprzedniego klienta i zwrot w dniu odbioru następnego.
        await client.CreateId("/api/charters", Booking(unit, "2031-06-14", "2031-06-21", client: "Drugi"));
        await client.CreateId("/api/charters", Booking(unit, "2031-06-01", "2031-06-07", client: "Wcześniejszy"));

        // Dwa jednodniowe wynajmy tego samego dnia to konflikt.
        await client.CreateId("/api/charters", Booking(unit, "2031-08-01", "2031-08-01"));
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/charters", Booking(unit, "2031-08-01", "2031-08-01"))).StatusCode);
    }

    [Fact]
    public async Task Editing_a_rental_does_not_clash_with_itself_but_checks_others()
    {
        var (client, _, _) = await api.NewOrganization();
        var unit = await client.CreateId("/api/vessels", Unit(rate: 100m));
        var first = await client.CreateId("/api/charters", Booking(unit, "2031-06-01", "2031-06-05"));
        await client.CreateId("/api/charters", Booking(unit, "2031-06-10", "2031-06-12"));
        var personId = (await Charter(client, first)).GetProperty("participantId").GetGuid();

        var longer = await client.PutAsJsonAsync($"/api/charters/{first}", new { vesselId = unit, startDate = "2031-06-01", endDate = "2031-06-08", participantId = personId });
        Assert.Equal(HttpStatusCode.NoContent, longer.StatusCode);
        Assert.Equal(700m, (await Charter(client, first)).GetProperty("total").GetDecimal());

        var clash = await client.PutAsJsonAsync($"/api/charters/{first}", new { vesselId = unit, startDate = "2031-06-01", endDate = "2031-06-11", participantId = personId });
        Assert.Equal(HttpStatusCode.Conflict, clash.StatusCode);
    }

    [Fact]
    public async Task Charter_cruise_and_service_block_exclude_each_other()
    {
        var (client, _, _) = await api.NewOrganization();
        var yacht = await client.CreateId("/api/vessels", Unit("Jacht", "Yacht", 900m));
        await client.CreateId("/api/cruises", TestData.Cruise(yacht, "2031-06-01", "2031-06-08"));
        await client.CreateId("/api/charters", Booking(yacht, "2031-06-20", "2031-06-27"));

        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/charters", Booking(yacht, "2031-06-05", "2031-06-10"))).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/cruises", TestData.Cruise(yacht, "2031-06-22", "2031-06-29"))).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync($"/api/vessels/{yacht}/blocks", new { startDate = "2031-06-25", endDate = "2031-06-26", reason = "Serwis" })).StatusCode);

        await client.CreateId($"/api/vessels/{yacht}/blocks", new { startDate = "2031-07-10", endDate = "2031-07-12", reason = "Serwis" });
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/charters", Booking(yacht, "2031-07-12", "2031-07-15"))).StatusCode);

        // Jednostki z charterem nie da się usunąć.
        Assert.Equal(HttpStatusCode.Conflict, (await client.DeleteAsync($"/api/vessels/{yacht}")).StatusCode);
    }

    [Fact]
    public async Task Unit_without_price_or_out_of_service_cannot_be_rented()
    {
        var (client, _, _) = await api.NewOrganization();
        var noPrice = await client.CreateId("/api/vessels", Unit(rate: null));
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/charters", Booking(noPrice, "2031-06-01", "2031-06-02"))).StatusCode);

        var broken = await client.CreateId("/api/vessels", new { name = "Zepsuty", berths = 2, status = "OutOfService", kind = "Motorboat", charterDayRate = 300m });
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/charters", Booking(broken, "2031-06-01", "2031-06-02"))).StatusCode);

        var unit = await client.CreateId("/api/vessels", Unit());
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/charters", Booking(unit, "2031-06-05", "2031-06-01"))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/charters", Booking(unit, "2031-06-01", "2031-06-02", total: -5m))).StatusCode);
    }

    [Fact]
    public async Task Checklist_is_saved_and_counted_in_summary()
    {
        var (client, _, _) = await api.NewOrganization();
        var unit = await client.CreateId("/api/vessels", Unit(rate: 200m));
        var items = new[] { new { name = "Kamizelki", isOk = true, note = (string?)null }, new { name = "Gaśnica", isOk = false, note = (string?)"Po terminie" } };
        Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsJsonAsync($"/api/vessels/{unit}/checklist", new { items })).StatusCode);

        var vessel = (await client.GetFromJsonAsync<JsonElement>("/api/vessels"))[0];
        Assert.Equal(2, vessel.GetProperty("checklist").GetArrayLength());
        Assert.Equal("Po terminie", vessel.GetProperty("checklist")[1].GetProperty("note").GetString());

        await client.CreateId("/api/charters", Booking(unit, "2031-03-01", "2031-03-04"));
        await client.CreateId("/api/charters", Booking(unit, "2031-03-10", "2031-03-12", total: 300m, client: "Inny"));
        await client.CreateId("/api/charters", Booking(unit, "2031-05-01", "2031-05-02"));

        var summary = await client.GetFromJsonAsync<JsonElement>("/api/charters/summary?year=2031");
        Assert.Equal(1, summary.GetProperty("units").GetInt32());
        // Każda rezerwacja z nowymi danymi dodaje osobę do bazy klientów.
        Assert.Equal(3, summary.GetProperty("clients").GetInt32());
        Assert.Equal(1100m, summary.GetProperty("revenueYear").GetDecimal());
        Assert.Equal(1, summary.GetProperty("checklistMissing").GetInt32());
        var march = summary.GetProperty("months")[2];
        Assert.Equal(2, march.GetProperty("bookings").GetInt32());
        Assert.Equal(5, march.GetProperty("days").GetInt32());
        Assert.Equal(900m, march.GetProperty("revenue").GetDecimal());
        var perUnit = summary.GetProperty("perUnit")[0];
        Assert.Equal(1200m, perUnit.GetProperty("listValue").GetDecimal());
        Assert.Equal(100m, perUnit.GetProperty("discounts").GetDecimal());
    }

    [Fact]
    public async Task Calendar_shows_charters_and_courses()
    {
        var (client, _, _) = await api.NewOrganization();
        var unit = await client.CreateId("/api/vessels", Unit());
        await client.CreateId("/api/charters", Booking(unit, "2035-05-02", "2035-05-04", client: "Kalendarzowy"));
        await client.CreateId("/api/courses", CourseTests.Course());

        var calendar = await client.GetFromJsonAsync<JsonElement>("/api/calendar?from=2035-05-01&to=2035-05-31");
        var item = calendar.GetProperty("vessels")[0].GetProperty("items")[0];
        Assert.Equal("charter", item.GetProperty("type").GetString());
        Assert.Equal("Kalendarzowy", item.GetProperty("title").GetString());
        Assert.Equal("course", calendar.GetProperty("courses")[0].GetProperty("type").GetString());
    }

    [Fact]
    public async Task Charters_are_isolated_between_organizations()
    {
        var (victim, _, _) = await api.NewOrganization("Ofiara");
        var unit = await victim.CreateId("/api/vessels", Unit());
        var charter = await victim.CreateId("/api/charters", Booking(unit, "2031-06-01", "2031-06-03", client: "Tajny Czarterowicz"));
        var person = (await Charter(victim, charter)).GetProperty("participantId").GetGuid();

        var (attacker, _, _) = await api.NewOrganization("Napastnik");
        Assert.Equal("[]", await attacker.GetStringAsync("/api/charters"));
        Assert.Equal("[]", await attacker.GetStringAsync("/api/charters/clients"));
        var summary = await attacker.GetFromJsonAsync<JsonElement>("/api/charters/summary?year=2031");
        Assert.Equal(0m, summary.GetProperty("revenueYear").GetDecimal());

        Assert.Equal(HttpStatusCode.NotFound, (await attacker.PutAsJsonAsync($"/api/charters/{charter}", Booking(unit, "2031-06-01", "2031-06-03"))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await attacker.DeleteAsync($"/api/charters/{charter}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await attacker.PutAsJsonAsync($"/api/vessels/{unit}/checklist", new { items = Array.Empty<object>() })).StatusCode);

        // Cudza jednostka i cudzy klient nie dadzą się użyć we własnej rezerwacji.
        Assert.Equal(HttpStatusCode.BadRequest, (await attacker.PostAsJsonAsync("/api/charters", Booking(unit, "2031-07-01", "2031-07-03"))).StatusCode);
        var own = await attacker.CreateId("/api/vessels", Unit());
        Assert.Equal(HttpStatusCode.NotFound, (await attacker.PostAsJsonAsync("/api/charters", new { vesselId = own, startDate = "2031-07-01", endDate = "2031-07-03", participantId = person })).StatusCode);

        Assert.Single((await victim.GetFromJsonAsync<JsonElement>("/api/charters")).EnumerateArray());
    }
}

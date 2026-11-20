using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Nautivo.Tests;

[Collection("api")]
public class DashboardAndSettingsTests(ApiFixture api)
{
    private static string Today => DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd");

    [Fact]
    public async Task Dashboard_reports_last_30_days_and_cruise_details()
    {
        var (client, _, _) = await api.NewOrganization();
        var cruise = await client.PostAsJsonAsync("/api/cruises", new
        {
            name = "Rejs testowy", routePoints = TestData.Route("Giżycko", "Mikołajki"), isConfirmed = true,
            startDate = "2035-06-01", endDate = "2035-06-08", price = 1000m, deposit = 300m, balanceDueDate = "2035-05-01", capacity = 4, status = "Planned",
        });
        var cruiseId = (await cruise.Content.ReadFromJsonAsync<IdDto>())!.Id;
        var enrollment = await client.CreateId($"/api/cruises/{cruiseId}/enrollments", TestData.NewPerson("Osoba Testowa"));
        await client.CreateId($"/api/enrollments/{enrollment}/payments", new { amount = 300m });

        var dashboard = await client.GetFromJsonAsync<JsonElement>("/api/dashboard");
        var period = dashboard.GetProperty("last30Days");

        Assert.Equal(1, period.GetProperty("reservations").GetInt32());
        Assert.Equal(300m, period.GetProperty("income").GetDecimal());
        Assert.Equal(1, period.GetProperty("newClients").GetInt32());

        var next = dashboard.GetProperty("nextCruises")[0];
        Assert.Equal("Giżycko", next.GetProperty("location").GetString());
        Assert.True(next.GetProperty("isConfirmed").GetBoolean());
        Assert.Equal(1, next.GetProperty("confirmed").GetInt32());
        Assert.Equal(1, dashboard.GetProperty("yourCruises")[0].GetProperty("reservations").GetInt32());
    }

    [Theory]
    [InlineData(7, 7)]
    [InlineData(30, 10)]
    [InlineData(90, 13)]
    [InlineData(365, 13)]
    public async Task Trend_returns_buckets_ending_today_and_sums_match(int days, int minimumPoints)
    {
        var (client, _, _) = await api.NewOrganization();
        var cruise = await client.CreateId("/api/cruises", TestData.Cruise(price: 1000m, capacity: 4));
        var enrollment = await client.CreateId($"/api/cruises/{cruise}/enrollments", TestData.NewPerson("Osoba Testowa"));
        await client.CreateId($"/api/enrollments/{enrollment}/payments", new { amount = 250m });

        var trend = await client.GetFromJsonAsync<JsonElement>($"/api/dashboard/trend?days={days}");
        var points = trend.GetProperty("points").EnumerateArray().ToList();

        Assert.True(points.Count >= minimumPoints);
        Assert.Equal(Today, points[^1].GetProperty("end").GetString());
        Assert.Equal(1, points.Sum(p => p.GetProperty("reservations").GetInt32()));
        Assert.Equal(250m, points.Sum(p => p.GetProperty("income").GetDecimal()));
    }

    [Fact]
    public async Task Trend_rejects_unknown_range()
    {
        var (client, _, _) = await api.NewOrganization();
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/dashboard/trend?days=12")).StatusCode);
    }

    [Fact]
    public async Task Trend_contains_only_own_organization_data()
    {
        var (victim, _, _) = await api.NewOrganization();
        var cruise = await victim.CreateId("/api/cruises", TestData.Cruise());
        var enrollment = await victim.CreateId($"/api/cruises/{cruise}/enrollments", TestData.NewPerson("Tajna Osoba"));
        await victim.CreateId($"/api/enrollments/{enrollment}/payments", new { amount = 100m });

        var (attacker, _, _) = await api.NewOrganization();
        var trend = await attacker.GetFromJsonAsync<JsonElement>("/api/dashboard/trend?days=30");

        Assert.All(trend.GetProperty("points").EnumerateArray(), p =>
        {
            Assert.Equal(0, p.GetProperty("reservations").GetInt32());
            Assert.Equal(0m, p.GetProperty("income").GetDecimal());
        });
    }

    [Fact]
    public async Task Owner_can_update_company_settings()
    {
        var (client, _, _) = await api.NewOrganization("Stara nazwa");
        var body = new { name = "Nowa nazwa", contactEmail = "biuro@example.com", phone = "+48 500", city = "Gdynia", taxId = "1234567890", website = "https://example.com" };
        Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsJsonAsync("/api/organization", body)).StatusCode);

        var settings = await client.GetFromJsonAsync<JsonElement>("/api/organization");
        Assert.Equal("Nowa nazwa", settings.GetProperty("name").GetString());
        Assert.Equal("Gdynia", settings.GetProperty("city").GetString());
        Assert.True(settings.GetProperty("canEdit").GetBoolean());

        var me = await client.GetFromJsonAsync<MeDto>("/api/auth/me");
        Assert.Equal("Nowa nazwa", me!.Organizations[0].Name);
    }

    [Fact]
    public async Task Cannot_change_settings_of_another_organization()
    {
        var victim = await api.NewOrganization("Ofiara");
        var (attacker, _, _) = await api.NewOrganization("Napastnik");
        attacker.DefaultRequestHeaders.Add("X-Organization-Id", victim.OrganizationId.ToString());

        var response = await attacker.PutAsJsonAsync("/api/organization", new { name = "Przejęta" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var settings = await victim.Client.GetFromJsonAsync<JsonElement>("/api/organization");
        Assert.Equal("Ofiara", settings.GetProperty("name").GetString());
    }
}

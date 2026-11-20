using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Nautivo.Tests;

/// <summary>Użytkownik jednej organizacji nie może odczytać ani zmienić danych drugiej.</summary>
[Collection("api")]
public class TenantIsolationTests(ApiFixture api)
{
    private async Task<(Guid Vessel, Guid Cruise, Guid Participant, Guid Enrollment, Guid Payment, Guid Cost, Guid Activity, Guid Task, Guid Course, Guid Trainee, Guid Session, Guid Test, Guid Material, Guid OrganizationId)> SeedVictim()
    {
        var (client, org, _) = await api.NewOrganization("Ofiara");
        var vessel = await client.CreateId("/api/vessels", TestData.Vessel());
        var cruise = await client.CreateId("/api/cruises", TestData.Cruise(vessel));
        var participant = await client.CreateId("/api/participants", new { fullName = "Tajny Uczestnik" });
        var enrollment = await client.CreateId($"/api/cruises/{cruise}/enrollments", new { participantId = participant });
        var payment = await client.CreateId($"/api/enrollments/{enrollment}/payments", new { amount = 100m });
        var cost = await client.CreateId($"/api/cruises/{cruise}/costs", new { category = "Czarter jachtu", amount = 5000m });
        var activity = await client.CreateId($"/api/participants/{participant}/activities", new { type = "Call", text = "Tajna notatka" });
        var task = await client.CreateId($"/api/participants/{participant}/tasks", new { title = "Tajne zadanie", dueDate = "2030-01-01" });
        var course = await client.CreateId("/api/courses", CourseTests.Course());
        var trainee = await client.CreateId($"/api/courses/{course}/enrollments", TestData.NewPerson("Tajny Kursant"));
        var session = await client.CreateId($"/api/courses/{course}/sessions", new { date = "2035-05-02", kind = "Theory", topic = "Tajne zajęcia" });
        var test = await client.CreateId($"/api/courses/{course}/tests", new { title = "Tajny test", date = "2035-05-03", maxScore = 10, passScore = 5 });
        var material = await client.CreateId($"/api/courses/{course}/materials", new { title = "Tajny konspekt" });
        return (vessel, cruise, participant, enrollment, payment, cost, activity, task, course, trainee, session, test, material, org);
    }

    [Fact]
    public async Task Cannot_select_another_organization_through_the_header()
    {
        var victim = await SeedVictim();
        var (attacker, _, _) = await api.NewOrganization("Napastnik");
        attacker.DefaultRequestHeaders.Add("X-Organization-Id", victim.OrganizationId.ToString());

        foreach (var url in new[] { "/api/dashboard", "/api/cruises", "/api/vessels", "/api/participants" })
        {
            Assert.Equal(HttpStatusCode.Forbidden, (await attacker.GetAsync(url)).StatusCode);
        }
    }

    [Fact]
    public async Task Lists_contain_only_own_data()
    {
        await SeedVictim();
        var (attacker, _, _) = await api.NewOrganization("Napastnik");

        Assert.Equal("[]", await attacker.GetStringAsync("/api/cruises"));
        Assert.Equal("[]", await attacker.GetStringAsync("/api/vessels"));
        Assert.Equal("[]", await attacker.GetStringAsync("/api/participants"));
        Assert.Equal("[]", await attacker.GetStringAsync("/api/participants?search=Tajny"));
        Assert.Equal("[]", await attacker.GetStringAsync("/api/tasks"));
        Assert.Equal("[]", await attacker.GetStringAsync("/api/courses"));

        var dashboard = await attacker.GetFromJsonAsync<JsonElement>("/api/dashboard");
        Assert.Equal(0, dashboard.GetProperty("cruisesTotal").GetInt32());
        Assert.Equal(0m, dashboard.GetProperty("earnedTotal").GetDecimal());

        var calendar = await attacker.GetFromJsonAsync<JsonElement>("/api/calendar?from=2030-01-01&to=2030-12-31");
        Assert.Equal(0, calendar.GetProperty("vessels").GetArrayLength());
    }

    [Fact]
    public async Task Cannot_read_or_change_foreign_records_by_id()
    {
        var v = await SeedVictim();
        var (attacker, _, _) = await api.NewOrganization("Napastnik");

        var responses = new[]
        {
            await attacker.GetAsync($"/api/cruises/{v.Cruise}"),
            await attacker.GetAsync($"/api/participants/{v.Participant}"),
            await attacker.PutAsJsonAsync($"/api/cruises/{v.Cruise}", TestData.Cruise()),
            await attacker.DeleteAsync($"/api/cruises/{v.Cruise}"),
            await attacker.PutAsJsonAsync($"/api/vessels/{v.Vessel}", TestData.Vessel("Przejęty")),
            await attacker.DeleteAsync($"/api/vessels/{v.Vessel}"),
            await attacker.PostAsJsonAsync($"/api/vessels/{v.Vessel}/blocks", new { startDate = "2031-01-01", endDate = "2031-01-02", reason = "x" }),
            await attacker.PutAsJsonAsync($"/api/participants/{v.Participant}", new { fullName = "Zmieniony" }),
            await attacker.DeleteAsync($"/api/participants/{v.Participant}"),
            await attacker.GetAsync($"/api/participants/{v.Participant}/export"),
            await attacker.PostAsync($"/api/participants/{v.Participant}/anonymize", null),
            await attacker.PostAsJsonAsync($"/api/participants/{v.Participant}/activities", new { type = "Note", text = "x" }),
            await attacker.PostAsJsonAsync($"/api/participants/{v.Participant}/tasks", new { title = "x", dueDate = "2030-01-01" }),
            await attacker.DeleteAsync($"/api/activities/{v.Activity}"),
            await attacker.PutAsJsonAsync($"/api/tasks/{v.Task}/done", new { done = true }),
            await attacker.DeleteAsync($"/api/tasks/{v.Task}"),
            await attacker.GetAsync($"/api/courses/{v.Course}"),
            await attacker.PutAsJsonAsync($"/api/courses/{v.Course}", CourseTests.Course()),
            await attacker.DeleteAsync($"/api/courses/{v.Course}"),
            await attacker.PostAsJsonAsync($"/api/courses/{v.Course}/enrollments", TestData.NewPerson("Intruz")),
            await attacker.PostAsJsonAsync($"/api/courses/{v.Course}/sessions", new { date = "2035-05-02", kind = "Theory", topic = "x" }),
            await attacker.PutAsJsonAsync($"/api/courses/{v.Course}/attendance", new { enrollmentId = v.Trainee, sessionId = v.Session, present = true }),
            await attacker.PostAsJsonAsync($"/api/courses/{v.Course}/tests", new { title = "x", date = "2035-05-02", maxScore = 10, passScore = 5 }),
            await attacker.PostAsJsonAsync($"/api/courses/{v.Course}/materials", new { title = "x" }),
            await attacker.PostAsJsonAsync($"/api/courses/{v.Course}/costs", new { category = "x", amount = 1m }),
            await attacker.PutAsJsonAsync($"/api/sessions/{v.Session}", new { date = "2035-05-02", kind = "Theory", topic = "x" }),
            await attacker.DeleteAsync($"/api/sessions/{v.Session}"),
            await attacker.PutAsJsonAsync($"/api/sessions/{v.Session}/complete", new { completed = true }),
            await attacker.PutAsJsonAsync($"/api/courses/{v.Course}/complete", new { completed = true }),
            await attacker.PutAsJsonAsync($"/api/tests/{v.Test}/results", new { enrollmentId = v.Trainee, score = 1 }),
            await attacker.DeleteAsync($"/api/tests/{v.Test}"),
            await attacker.DeleteAsync($"/api/materials/{v.Material}"),
            await attacker.PutAsJsonAsync($"/api/enrollments/{v.Trainee}/progress", new { examResult = "Passed" }),
            await attacker.PostAsJsonAsync($"/api/cruises/{v.Cruise}/enrollments", TestData.NewPerson("Intruz")),
            await attacker.PutAsJsonAsync($"/api/enrollments/{v.Enrollment}", new { fullName = "Zmieniony", total = 1m }),
            await attacker.DeleteAsync($"/api/enrollments/{v.Enrollment}"),
            await attacker.PostAsJsonAsync($"/api/cruises/{v.Cruise}/costs", new { category = "Paliwo", amount = 1m }),
            await attacker.PutAsJsonAsync($"/api/costs/{v.Cost}", new { category = "Paliwo", amount = 1m }),
            await attacker.DeleteAsync($"/api/costs/{v.Cost}"),
            await attacker.PostAsJsonAsync($"/api/enrollments/{v.Enrollment}/payments", new { amount = 1m }),
            await attacker.DeleteAsync($"/api/payments/{v.Payment}"),
        };

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.NotFound, r.StatusCode));
    }

    [Fact]
    public async Task Cannot_attach_foreign_vessel_or_participant_to_own_records()
    {
        var v = await SeedVictim();
        var (attacker, _, _) = await api.NewOrganization("Napastnik");

        var foreignVessel = await attacker.PostAsJsonAsync("/api/cruises", TestData.Cruise(v.Vessel));
        Assert.Equal(HttpStatusCode.BadRequest, foreignVessel.StatusCode);

        var ownCruise = await attacker.CreateId("/api/cruises", TestData.Cruise());
        var foreignPerson = await attacker.PostAsJsonAsync($"/api/cruises/{ownCruise}/enrollments", new { participantId = v.Participant });
        Assert.Equal(HttpStatusCode.NotFound, foreignPerson.StatusCode);
    }

    [Fact]
    public async Task Victim_data_is_unchanged_after_attack()
    {
        var v = await SeedVictim();
        var (attacker, _, _) = await api.NewOrganization("Napastnik");
        await attacker.DeleteAsync($"/api/payments/{v.Payment}");
        await attacker.DeleteAsync($"/api/enrollments/{v.Enrollment}");
        await attacker.DeleteAsync($"/api/costs/{v.Cost}");
        await attacker.DeleteAsync($"/api/cruises/{v.Cruise}");

        var admin = await api.AdminClient(v.OrganizationId);

        var detail = await admin.GetFromJsonAsync<JsonElement>($"/api/cruises/{v.Cruise}");
        var enrollment = detail.GetProperty("enrollments")[0];
        Assert.Equal("Confirmed", enrollment.GetProperty("status").GetString());
        Assert.Equal(100m, enrollment.GetProperty("paid").GetDecimal());
        Assert.Equal(5000m, detail.GetProperty("costs")[0].GetProperty("amount").GetDecimal());
    }
}

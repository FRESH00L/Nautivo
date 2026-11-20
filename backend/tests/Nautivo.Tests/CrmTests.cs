using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Nautivo.Tests;

[Collection("api")]
public class CrmTests(ApiFixture api)
{
    private static async Task<JsonElement> Contact(HttpClient client, Guid id) =>
        await client.GetFromJsonAsync<JsonElement>($"/api/participants/{id}");

    private static async Task<JsonElement> ListItem(HttpClient client, Guid id) =>
        (await client.GetFromJsonAsync<JsonElement>("/api/participants")).EnumerateArray().Single(p => p.GetProperty("id").GetGuid() == id);

    [Fact]
    public async Task Stage_follows_cruise_history()
    {
        var (client, _, _) = await api.NewOrganization();
        var person = await client.CreateId("/api/participants", new { fullName = "Nowy Kontakt" });
        Assert.Equal("Lead", (await ListItem(client, person)).GetProperty("stage").GetString());

        var old = await client.CreateId("/api/cruises", TestData.Cruise(start: "2020-06-01", end: "2020-06-08"));
        await client.CreateId($"/api/cruises/{old}/enrollments", new { participantId = person });
        // Jeden rejs ponad rok temu i nic w planach: klient do odzyskania.
        Assert.Equal("Lapsed", (await ListItem(client, person)).GetProperty("stage").GetString());

        var next = await client.CreateId("/api/cruises", TestData.Cruise(start: "2035-06-01", end: "2035-06-08"));
        await client.CreateId($"/api/cruises/{next}/enrollments", new { participantId = person });
        var item = await ListItem(client, person);
        Assert.Equal("Regular", item.GetProperty("stage").GetString());
        Assert.Equal("2035-06-01", item.GetProperty("nextCruiseStart").GetString());
        Assert.Equal("Regular", (await Contact(client, person)).GetProperty("stage").GetString());
    }

    [Fact]
    public async Task Tags_source_and_consent_are_saved_and_consent_change_is_logged()
    {
        var (client, _, _) = await api.NewOrganization();
        var person = await client.CreateId("/api/participants", new { fullName = "Zgoda Testowa", source = "Instagram", tags = new[] { "rodzina", "Rodzina", " VIP " }, marketingConsent = true });

        var contact = await Contact(client, person);
        Assert.Equal("Instagram", contact.GetProperty("source").GetString());
        Assert.Equal(["rodzina", "VIP"], contact.GetProperty("tags").EnumerateArray().Select(t => t.GetString()));
        Assert.True(contact.GetProperty("marketingConsent").GetBoolean());
        Assert.NotEqual(JsonValueKind.Null, contact.GetProperty("marketingConsentAt").ValueKind);

        (await client.PutAsJsonAsync($"/api/participants/{person}", new { fullName = "Zgoda Testowa", marketingConsent = false })).EnsureSuccessStatusCode();
        contact = await Contact(client, person);
        Assert.False(contact.GetProperty("marketingConsent").GetBoolean());
        Assert.Equal(JsonValueKind.Null, contact.GetProperty("marketingConsentAt").ValueKind);
        var log = contact.GetProperty("activities").EnumerateArray().Select(a => a.GetProperty("text").GetString()).ToList();
        Assert.Contains(log, t => t!.Contains("wycofanie zgody"));
        Assert.Contains(log, t => t!.Contains("zgodę na informacje"));
        var systemEntry = contact.GetProperty("activities").EnumerateArray().First(a => a.GetProperty("isSystem").GetBoolean()).GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.Conflict, (await client.DeleteAsync($"/api/activities/{systemEntry}")).StatusCode);

        var tooMany = await client.PostAsJsonAsync("/api/participants", new { fullName = "Za Dużo", tags = Enumerable.Range(0, 11).Select(i => $"t{i}") });
        Assert.Equal(HttpStatusCode.BadRequest, tooMany.StatusCode);
    }

    [Fact]
    public async Task Activities_and_tasks_work_and_open_tasks_are_listed()
    {
        var (client, _, _) = await api.NewOrganization();
        var person = await client.CreateId("/api/participants", new { fullName = "Osoba Z Zadaniem" });

        var activity = await client.CreateId($"/api/participants/{person}/activities", new { type = "Call", text = "Pytał o Chorwację." });
        var future = await client.PostAsJsonAsync($"/api/participants/{person}/activities", new { type = "Note", text = "x", occurredAt = DateTimeOffset.UtcNow.AddDays(2) });
        Assert.Equal(HttpStatusCode.BadRequest, future.StatusCode);

        var task = await client.CreateId($"/api/participants/{person}/tasks", new { title = "Oddzwonić", dueDate = "2030-01-10" });
        var open = await client.GetFromJsonAsync<JsonElement>("/api/tasks");
        Assert.Contains(open.EnumerateArray(), t => t.GetProperty("id").GetGuid() == task && t.GetProperty("participantName").GetString() == "Osoba Z Zadaniem");
        Assert.Equal(1, (await ListItem(client, person)).GetProperty("openTasks").GetInt32());

        (await client.PutAsJsonAsync($"/api/tasks/{task}/done", new { done = true })).EnsureSuccessStatusCode();
        Assert.DoesNotContain((await client.GetFromJsonAsync<JsonElement>("/api/tasks")).EnumerateArray(), t => t.GetProperty("id").GetGuid() == task);
        Assert.Equal(0, (await ListItem(client, person)).GetProperty("openTasks").GetInt32());

        (await client.DeleteAsync($"/api/activities/{activity}")).EnsureSuccessStatusCode();
        (await client.DeleteAsync($"/api/tasks/{task}")).EnsureSuccessStatusCode();
        var contact = await Contact(client, person);
        Assert.Equal(0, contact.GetProperty("activities").GetArrayLength());
        Assert.Equal(0, contact.GetProperty("tasks").GetArrayLength());
    }

    [Fact]
    public async Task Anonymization_removes_personal_data_but_keeps_payments()
    {
        var (client, _, _) = await api.NewOrganization();
        var person = await client.CreateId("/api/participants", new { fullName = "Jan Do Usunięcia", email = "jan@example.com", phone = "+48 600", notes = "Lubi kawę", tags = new[] { "VIP" }, marketingConsent = true });
        var cruise = await client.CreateId("/api/cruises", TestData.Cruise(price: 1000m));
        var enrollment = await client.CreateId($"/api/cruises/{cruise}/enrollments", new { participantId = person });
        await client.CreateId($"/api/enrollments/{enrollment}/payments", new { amount = 400m });
        await client.CreateId($"/api/participants/{person}/tasks", new { title = "Zadzwonić", dueDate = "2030-01-10" });

        Assert.Equal(HttpStatusCode.Conflict, (await client.DeleteAsync($"/api/participants/{person}")).StatusCode);
        (await client.PostAsync($"/api/participants/{person}/anonymize", null)).EnsureSuccessStatusCode();

        var contact = await Contact(client, person);
        Assert.Equal("Osoba zanonimizowana", contact.GetProperty("fullName").GetString());
        Assert.Equal(JsonValueKind.Null, contact.GetProperty("email").ValueKind);
        Assert.Equal(JsonValueKind.Null, contact.GetProperty("notes").ValueKind);
        Assert.Equal(0, contact.GetProperty("tags").GetArrayLength());
        Assert.Equal(0, contact.GetProperty("tasks").GetArrayLength());
        Assert.Equal(0, contact.GetProperty("activities").GetArrayLength());
        Assert.Equal(400m, contact.GetProperty("history")[0].GetProperty("paid").GetDecimal());
        Assert.DoesNotContain("Jan Do Usunięcia", await client.GetStringAsync("/api/participants"));
        Assert.Equal(HttpStatusCode.Conflict, (await client.PutAsJsonAsync($"/api/participants/{person}", new { fullName = "Powrót" })).StatusCode);
    }

    [Fact]
    public async Task Export_contains_all_data_of_the_person()
    {
        var (client, _, _) = await api.NewOrganization();
        var person = await client.CreateId("/api/participants", new { fullName = "Eksport Osoby", email = "eksport@example.com", tags = new[] { "rodzina" } });
        await client.CreateId($"/api/participants/{person}/activities", new { type = "Email", text = "Wysłany program." });

        var export = await client.GetFromJsonAsync<JsonElement>($"/api/participants/{person}/export");
        var data = export.GetProperty("person");
        Assert.Equal("eksport@example.com", data.GetProperty("email").GetString());
        Assert.Equal("Wysłany program.", data.GetProperty("activities")[0].GetProperty("text").GetString());
        Assert.True(export.TryGetProperty("exportedAt", out _));
    }
}

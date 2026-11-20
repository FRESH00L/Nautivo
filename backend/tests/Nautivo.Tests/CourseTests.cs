using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nautivo.Api.Data;

namespace Nautivo.Tests;

[Collection("api")]
public class CourseTests(ApiFixture api)
{
    public static object Course(string type = "Zj", int capacity = 4, decimal price = 1500m, string start = "2035-05-01", string end = "2035-05-10", string[]? documents = null) =>
        new { type, name = "Kurs testowy", startDate = start, endDate = end, capacity, price, location = "Giżycko", requiredDocuments = documents ?? ["Zdjęcie", "Kopia patentu"] };

    private static Task<JsonElement> Detail(HttpClient client, Guid course) => client.GetFromJsonAsync<JsonElement>($"/api/courses/{course}");

    private static async Task<JsonElement> Trainee(HttpClient client, Guid course, Guid enrollment) =>
        (await Detail(client, course)).GetProperty("trainees").EnumerateArray().Single(t => t.GetProperty("enrollment").GetProperty("id").GetGuid() == enrollment);

    [Fact]
    public async Task Enrollment_respects_capacity_and_payments_work()
    {
        var (client, _, _) = await api.NewOrganization();
        var course = await client.CreateId("/api/courses", Course(capacity: 1, price: 1000m));
        var first = await client.CreateId($"/api/courses/{course}/enrollments", TestData.NewPerson("Kursant Pierwszy"));
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync($"/api/courses/{course}/enrollments", TestData.NewPerson("Kursant Drugi"))).StatusCode);

        await client.CreateId($"/api/enrollments/{first}/payments", new { amount = 300m });
        var trainee = await Trainee(client, course, first);
        Assert.Equal("Partial", trainee.GetProperty("enrollment").GetProperty("state").GetString());
        var list = await client.GetFromJsonAsync<JsonElement>("/api/courses");
        Assert.Equal(300m, list[0].GetProperty("collected").GetDecimal());
    }

    [Fact]
    public async Task Schedule_attendance_and_progress_are_tracked()
    {
        var (client, _, _) = await api.NewOrganization();
        var course = await client.CreateId("/api/courses", Course());
        var enrollment = await client.CreateId($"/api/courses/{course}/enrollments", TestData.NewPerson("Kursant Testowy"));
        var session = await client.CreateId($"/api/courses/{course}/sessions", new { date = "2035-05-02", startTime = "09:00", endTime = "13:00", kind = "Theory", topic = "Locja", notes = "Mapy i znaki" });

        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync($"/api/courses/{course}/sessions", new { date = "2035-05-02", startTime = "13:00", endTime = "09:00", kind = "Theory", topic = "Zła godzina" })).StatusCode);

        (await client.PutAsJsonAsync($"/api/courses/{course}/attendance", new { enrollmentId = enrollment, sessionId = session, present = true })).EnsureSuccessStatusCode();
        Assert.Equal(session, (await Trainee(client, course, enrollment)).GetProperty("attendedSessions")[0].GetGuid());
        (await client.PutAsJsonAsync($"/api/courses/{course}/attendance", new { enrollmentId = enrollment, sessionId = session, present = false })).EnsureSuccessStatusCode();
        Assert.Equal(0, (await Trainee(client, course, enrollment)).GetProperty("attendedSessions").GetArrayLength());

        var badDocument = await client.PutAsJsonAsync($"/api/enrollments/{enrollment}/progress", new { examResult = "Pending", documentsDelivered = new[] { "Dowód" } });
        Assert.Equal(HttpStatusCode.BadRequest, badDocument.StatusCode);
        (await client.PutAsJsonAsync($"/api/enrollments/{enrollment}/progress", new { examResult = "Passed", certificateNumber = "ŻJ/1234", documentsDelivered = new[] { "Zdjęcie" } })).EnsureSuccessStatusCode();
        var trainee = await Trainee(client, course, enrollment);
        Assert.Equal("Passed", trainee.GetProperty("examResult").GetString());
        Assert.Equal("ŻJ/1234", trainee.GetProperty("certificateNumber").GetString());
        Assert.Equal("Zdjęcie", trainee.GetProperty("documentsDelivered")[0].GetString());
        Assert.Equal("Mapy i znaki", (await Detail(client, course)).GetProperty("sessions")[0].GetProperty("notes").GetString());
    }

    [Fact]
    public async Task Passed_exam_becomes_a_qualification_in_crm()
    {
        var (client, _, _) = await api.NewOrganization();
        var person = await client.CreateId("/api/participants", new { fullName = "Przyszły Sternik", manualQualifications = new[] { "Src" } });
        var course = await client.CreateId("/api/courses", Course(type: "Zj"));
        var enrollment = await client.CreateId($"/api/courses/{course}/enrollments", new { participantId = person });

        var contact = await client.GetFromJsonAsync<JsonElement>($"/api/participants/{person}");
        Assert.Equal(["Src"], contact.GetProperty("manualQualifications").EnumerateArray().Select(q => q.GetString()));
        Assert.Equal(0, contact.GetProperty("earnedQualifications").GetArrayLength());

        (await client.PutAsJsonAsync($"/api/enrollments/{enrollment}/progress", new { examResult = "Passed", certificateNumber = "ŻJ/77" })).EnsureSuccessStatusCode();
        contact = await client.GetFromJsonAsync<JsonElement>($"/api/participants/{person}");
        var earned = contact.GetProperty("earnedQualifications")[0];
        Assert.Equal("Zj", earned.GetProperty("type").GetString());
        Assert.Equal("ŻJ/77", earned.GetProperty("certificateNumber").GetString());
        // Szkolenie liczy się jako zakup: osoba nie jest już potencjalnym klientem.
        Assert.NotEqual("Lead", contact.GetProperty("stage").GetString());

        var item = (await client.GetFromJsonAsync<JsonElement>("/api/participants")).EnumerateArray().Single(p => p.GetProperty("id").GetGuid() == person);
        Assert.Equal(["Zj", "Src"], item.GetProperty("qualifications").EnumerateArray().Select(q => q.GetString()));
        Assert.Equal("Kurs testowy", contact.GetProperty("history")[0].GetProperty("cruiseName").GetString());
        Assert.Equal(course, contact.GetProperty("history")[0].GetProperty("courseId").GetGuid());
    }

    [Fact]
    public async Task Tests_materials_and_costs_work()
    {
        var (client, _, _) = await api.NewOrganization();
        var course = await client.CreateId("/api/courses", Course(price: 1000m));
        var enrollment = await client.CreateId($"/api/courses/{course}/enrollments", TestData.NewPerson("Kursant Testowy"));

        var test = await client.CreateId($"/api/courses/{course}/tests", new { title = "Test z locji", date = "2035-05-05", maxScore = 75, passScore = 50 });
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsJsonAsync($"/api/tests/{test}/results", new { enrollmentId = enrollment, score = 80 })).StatusCode);
        (await client.PutAsJsonAsync($"/api/tests/{test}/results", new { enrollmentId = enrollment, score = 62.5 })).EnsureSuccessStatusCode();
        Assert.Equal(62.5m, (await Detail(client, course)).GetProperty("tests")[0].GetProperty("results")[0].GetProperty("score").GetDecimal());

        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync($"/api/courses/{course}/materials", new { title = "Zły link", url = "javascript:alert(1)" })).StatusCode);
        var material = await client.CreateId($"/api/courses/{course}/materials", new { title = "Prezentacja", url = "https://example.com/locja" });

        var cost = await client.CreateId($"/api/courses/{course}/costs", new { category = "Wynajem sali", amount = 400m });
        Assert.Equal(400m, (await client.GetFromJsonAsync<JsonElement>("/api/courses"))[0].GetProperty("costs").GetDecimal());
        (await client.PutAsJsonAsync($"/api/costs/{cost}", new { category = "Wynajem sali", amount = 450m })).EnsureSuccessStatusCode();

        var detail = await Detail(client, course);
        Assert.Equal(450m, detail.GetProperty("costs")[0].GetProperty("amount").GetDecimal());
        Assert.Equal(material, detail.GetProperty("materials")[0].GetProperty("id").GetGuid());

        // Usunięcie kursanta zabiera też jego wyniki testów.
        (await client.DeleteAsync($"/api/enrollments/{enrollment}")).EnsureSuccessStatusCode();
        Assert.Equal(0, (await Detail(client, course)).GetProperty("tests")[0].GetProperty("results").GetArrayLength());
    }

    [Fact]
    public async Task Deleting_a_course_removes_everything_but_keeps_people()
    {
        var (client, _, _) = await api.NewOrganization();
        var course = await client.CreateId("/api/courses", Course());
        var enrollment = await client.CreateId($"/api/courses/{course}/enrollments", TestData.NewPerson("Kursant Do Usunięcia"));
        var session = await client.CreateId($"/api/courses/{course}/sessions", new { date = "2035-05-02", kind = "Practice", topic = "Manewry" });
        await client.PutAsJsonAsync($"/api/courses/{course}/attendance", new { enrollmentId = enrollment, sessionId = session, present = true });
        await client.CreateId($"/api/enrollments/{enrollment}/payments", new { amount = 100m });
        var test = await client.CreateId($"/api/courses/{course}/tests", new { title = "Test", date = "2035-05-05", maxScore = 10, passScore = 5 });
        await client.PutAsJsonAsync($"/api/tests/{test}/results", new { enrollmentId = enrollment, score = 7 });
        await client.CreateId($"/api/courses/{course}/costs", new { category = "Sala", amount = 100m });
        await client.CreateId($"/api/courses/{course}/materials", new { title = "Konspekt" });

        (await client.DeleteAsync($"/api/courses/{course}")).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/courses/{course}")).StatusCode);
        Assert.Contains("Kursant Do Usunięcia", await client.GetStringAsync("/api/participants"));
    }

    [Fact]
    public async Task Sessions_and_course_can_be_completed_manually()
    {
        var (client, _, _) = await api.NewOrganization();
        var course = await client.CreateId("/api/courses", Course());
        var first = await client.CreateId($"/api/courses/{course}/sessions", new { date = "2035-05-02", kind = "Theory", topic = "Locja" });
        await client.CreateId($"/api/courses/{course}/sessions", new { date = "2035-05-03", kind = "Practice", topic = "Manewry" });

        // Postęp liczy się z oznaczonych zajęć, nie z dat.
        Assert.Equal(0, (await client.GetFromJsonAsync<JsonElement>("/api/courses"))[0].GetProperty("sessionsDone").GetInt32());
        (await client.PutAsJsonAsync($"/api/sessions/{first}/complete", new { completed = true })).EnsureSuccessStatusCode();
        var detail = await Detail(client, course);
        Assert.Equal(1, detail.GetProperty("course").GetProperty("sessionsDone").GetInt32());
        Assert.NotEqual(JsonValueKind.Null, detail.GetProperty("sessions")[0].GetProperty("completedAt").ValueKind);
        (await client.PutAsJsonAsync($"/api/sessions/{first}/complete", new { completed = false })).EnsureSuccessStatusCode();
        Assert.Equal(0, (await Detail(client, course)).GetProperty("course").GetProperty("sessionsDone").GetInt32());

        (await client.PutAsJsonAsync($"/api/courses/{course}/complete", new { completed = true, completeSessions = true })).EnsureSuccessStatusCode();
        detail = await Detail(client, course);
        Assert.NotEqual(JsonValueKind.Null, detail.GetProperty("course").GetProperty("completedAt").ValueKind);
        Assert.Equal(2, detail.GetProperty("course").GetProperty("sessionsDone").GetInt32());
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync($"/api/courses/{course}/enrollments", TestData.NewPerson("Spóźniony Kursant"))).StatusCode);

        (await client.PutAsJsonAsync($"/api/courses/{course}/complete", new { completed = false })).EnsureSuccessStatusCode();
        Assert.Equal(JsonValueKind.Null, (await Detail(client, course)).GetProperty("course").GetProperty("completedAt").ValueKind);
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync($"/api/courses/{course}/enrollments", TestData.NewPerson("Spóźniony Kursant"))).StatusCode);
    }

    [Fact]
    public async Task Course_module_can_be_switched_off()
    {
        var (client, organization, _) = await api.NewOrganization();
        using (var scope = api.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var org = await db.Organizations.SingleAsync(o => o.Id == organization);
            org.EnabledModules = ["rejsy"];
            await db.SaveChangesAsync();
        }
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/courses")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/cruises")).StatusCode);
    }
}

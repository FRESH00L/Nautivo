using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Nautivo.Api.Data;
using Nautivo.Api.Domain;
using Nautivo.Api.Infrastructure;

namespace Nautivo.Api.Features;

public record CourseRequest(
    CourseType Type,
    [property: Required(ErrorMessage = "Podaj nazwę szkolenia."), MaxLength(200)] string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    [property: Range(1, 500, ErrorMessage = "Podaj liczbę miejsc od 1 do 500.")] int Capacity,
    [property: Range(typeof(decimal), "0", "9999999", ErrorMessage = "Podaj poprawną cenę.")] decimal Price,
    [property: MaxLength(200)] string? Location = null,
    [property: MaxLength(200)] string? Instructor = null,
    [property: Range(typeof(decimal), "0", "9999999", ErrorMessage = "Podaj poprawną zaliczkę.")] decimal Deposit = 0,
    DateOnly? BalanceDueDate = null,
    DateOnly? ExamDate = null,
    [property: MaxLength(1000)] string? ExamInfo = null,
    string[]? RequiredDocuments = null,
    CruiseStatus Status = CruiseStatus.Planned,
    [property: MaxLength(4000)] string? Description = null);

public record SessionRequest(
    DateOnly Date,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    SessionKind Kind,
    [property: Required(ErrorMessage = "Podaj temat zajęć."), MaxLength(200)] string Topic,
    [property: MaxLength(4000)] string? Notes = null);

public record MaterialRequest(
    [property: Required(ErrorMessage = "Podaj nazwę materiału."), MaxLength(200)] string Title,
    [property: MaxLength(1000)] string? Url,
    [property: MaxLength(1000)] string? Note);

public record TestRequest(
    [property: Required(ErrorMessage = "Podaj nazwę testu."), MaxLength(200)] string Title,
    DateOnly Date,
    [property: Range(typeof(decimal), "1", "10000", ErrorMessage = "Maksymalna liczba punktów musi być od 1 do 10000.")] decimal MaxScore,
    [property: Range(typeof(decimal), "0", "10000", ErrorMessage = "Podaj próg zaliczenia.")] decimal PassScore);

/// <summary>Wynik kursanta z testu. Score = null usuwa wynik (np. pomyłka).</summary>
public record TestResultRequest(Guid EnrollmentId, decimal? Score);

public record CompleteRequest(bool Completed, bool CompleteSessions = false);

public record AttendanceRequest(Guid EnrollmentId, Guid SessionId, bool Present);

public record ProgressRequest(ExamResult ExamResult, [property: MaxLength(60)] string? CertificateNumber, string[]? DocumentsDelivered);

public record CourseListItem(
    Guid Id, CourseType Type, string Name, DateOnly StartDate, DateOnly EndDate, string Location, string? Instructor,
    DateOnly? ExamDate, decimal Price, decimal Deposit, DateOnly BalanceDueDate, int Capacity, CruiseStatus Status,
    int Confirmed, decimal Collected, decimal Expected, int Overdue, int Sessions, int SessionsDone, int Passed, decimal Costs, DateTimeOffset? CompletedAt);

public record SessionDto(Guid Id, DateOnly Date, TimeOnly? StartTime, TimeOnly? EndTime, SessionKind Kind, string Topic, string? Notes, DateTimeOffset? CompletedAt);
public record MaterialDto(Guid Id, string Title, string? Url, string? Note);
public record TestScoreDto(Guid EnrollmentId, decimal Score);
public record TestDto(Guid Id, string Title, DateOnly Date, decimal MaxScore, decimal PassScore, IReadOnlyList<TestScoreDto> Results);

/// <summary>Kursant: dane zapisu i wpłat jak na rejsie oraz postęp w szkoleniu.</summary>
public record TraineeDto(EnrollmentDto Enrollment, ExamResult ExamResult, string? CertificateNumber, string[] DocumentsDelivered, Guid[] AttendedSessions);

public record CourseDetail(
    CourseListItem Course, string? Description, string? ExamInfo, string[] RequiredDocuments,
    IReadOnlyList<SessionDto> Sessions, IReadOnlyList<TraineeDto> Trainees, IReadOnlyList<MaterialDto> Materials,
    IReadOnlyList<TestDto> Tests, IReadOnlyList<CostDto> Costs);

public static class CourseEndpoints
{
    private const int MaxDocuments = 15;

    public static void MapCourseEndpoints(this RouteGroupBuilder api)
    {
        var courses = api.MapGroup("/courses").RequireModule(Modules.Courses);

        courses.MapGet("/", async (AppDbContext db, TimeProvider time, CancellationToken ct) =>
            await ListItems(db.Courses, time.Today()).ToListAsync(ct));

        courses.MapGet("/{id:guid}", async (Guid id, AppDbContext db, TimeProvider time, CancellationToken ct) =>
        {
            var today = time.Today();
            var course = await ListItems(db.Courses.Where(c => c.Id == id), today).FirstOrDefaultAsync(ct);
            if (course is null)
            {
                return Problems.NotFound("Nie znaleziono szkolenia.");
            }
            var extra = await db.Courses.AsNoTracking().Where(c => c.Id == id).Select(c => new { c.Description, c.ExamInfo, c.RequiredDocuments }).FirstAsync(ct);
            var sessions = await db.CourseSessions.AsNoTracking().Where(s => s.CourseId == id).OrderBy(s => s.Date).ThenBy(s => s.StartTime)
                .Select(s => new SessionDto(s.Id, s.Date, s.StartTime, s.EndTime, s.Kind, s.Topic, s.Notes, s.CompletedAt)).ToListAsync(ct);
            var enrollments = await db.Enrollments.AsNoTracking().Where(e => e.CourseId == id && e.Status == EnrollmentStatus.Confirmed)
                .Include(e => e.Participant).Include(e => e.Payments).OrderBy(e => e.CreatedAt).ToListAsync(ct);
            var enrollmentIds = enrollments.Select(e => e.Id).ToList();
            var attendance = (await db.CourseAttendance.AsNoTracking().Where(a => enrollmentIds.Contains(a.EnrollmentId)).ToListAsync(ct))
                .ToLookup(a => a.EnrollmentId, a => a.SessionId);
            var materials = await db.CourseMaterials.AsNoTracking().Where(m => m.CourseId == id).OrderBy(m => m.CreatedAt)
                .Select(m => new MaterialDto(m.Id, m.Title, m.Url, m.Note)).ToListAsync(ct);
            var tests = await db.CourseTests.AsNoTracking().Where(t => t.CourseId == id).OrderBy(t => t.Date)
                .Select(t => new TestDto(t.Id, t.Title, t.Date, t.MaxScore, t.PassScore, t.Results.Select(r => new TestScoreDto(r.EnrollmentId, r.Score)).ToList())).ToListAsync(ct);
            var costs = await db.CruiseCosts.AsNoTracking().Where(c => c.CourseId == id).OrderBy(c => c.CreatedAt)
                .Select(c => new CostDto(c.Id, c.Category, c.Description, c.Amount)).ToListAsync(ct);

            return Results.Ok(new CourseDetail(
                course, extra.Description, extra.ExamInfo, extra.RequiredDocuments, sessions,
                enrollments.Select(e => new TraineeDto(
                    CruiseEndpoints.ToDto(e, course.BalanceDueDate, today), e.ExamResult, e.CertificateNumber,
                    // Dokumenty usunięte z wymagań szkolenia nie liczą się już do postępu.
                    e.DocumentsDelivered.Where(extra.RequiredDocuments.Contains).ToArray(),
                    attendance[e.Id].ToArray())).ToList(),
                materials, tests, costs));
        });

        courses.MapPost("/", async (CourseRequest request, AppDbContext db, CancellationToken ct) =>
        {
            if (Validate(request) is { } error)
            {
                return error;
            }
            var course = new Course { Name = request.Name.Trim() };
            Apply(course, request);
            db.Courses.Add(course);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/courses/{course.Id}", new { course.Id });
        }).Validate<CourseRequest>();

        courses.MapPut("/{id:guid}", async (Guid id, CourseRequest request, AppDbContext db, CancellationToken ct) =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (course is null)
            {
                return Problems.NotFound("Nie znaleziono szkolenia.");
            }
            await LockCourse(db, id, ct);
            if (Validate(request) is { } error)
            {
                return error;
            }
            var trainees = await db.Enrollments.Where(e => e.CourseId == id && e.Status == EnrollmentStatus.Confirmed).ToListAsync(ct);
            if (request.Capacity < trainees.Count)
            {
                return Problems.Conflict($"Na szkolenie zapisanych jest {trainees.Count} osób. Nie można zmniejszyć limitu poniżej tej liczby.");
            }
            // Jak przy rejsach: nowa cena obejmuje osoby z ceną standardową, indywidualne kwoty zostają.
            foreach (var trainee in trainees.Where(e => e.Total == course.Price))
            {
                trainee.Total = request.Price;
            }
            Apply(course, request);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return Results.NoContent();
        }).Validate<CourseRequest>();

        // Usuwa szkolenie razem z harmonogramem, obecnościami, testami, materiałami, kosztami, zapisami i wpłatami. Osoby zostają w bazie klientów.
        courses.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (course is null)
            {
                return Problems.NotFound("Nie znaleziono szkolenia.");
            }
            await LockCourse(db, id, ct);
            db.CourseAttendance.RemoveRange(await db.CourseAttendance.Where(a => a.Session!.CourseId == id).ToListAsync(ct));
            db.CourseTestResults.RemoveRange(await db.CourseTestResults.Where(r => r.Test!.CourseId == id).ToListAsync(ct));
            db.CourseTests.RemoveRange(await db.CourseTests.Where(t => t.CourseId == id).ToListAsync(ct));
            db.CourseMaterials.RemoveRange(await db.CourseMaterials.Where(m => m.CourseId == id).ToListAsync(ct));
            db.CruiseCosts.RemoveRange(await db.CruiseCosts.Where(c => c.CourseId == id).ToListAsync(ct));
            db.Payments.RemoveRange(await db.Payments.Where(p => p.Enrollment!.CourseId == id).ToListAsync(ct));
            db.Enrollments.RemoveRange(await db.Enrollments.Where(e => e.CourseId == id).ToListAsync(ct));
            db.CourseSessions.RemoveRange(await db.CourseSessions.Where(s => s.CourseId == id).ToListAsync(ct));
            db.Courses.Remove(course);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return Results.NoContent();
        });

        courses.MapPost("/{id:guid}/enrollments", Enroll);

        // Ręczne zakończenie kursu przez prowadzącego. Opcjonalnie oznacza też wszystkie pozostałe zajęcia jako ukończone.
        courses.MapPut("/{id:guid}/complete", async (Guid id, CompleteRequest request, AppDbContext db, TimeProvider time, CancellationToken ct) =>
        {
            var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (course is null)
            {
                return Problems.NotFound("Nie znaleziono szkolenia.");
            }
            if (request.Completed && course.Status == CruiseStatus.Cancelled)
            {
                return Problems.Conflict("Odwołanego szkolenia nie można zakończyć.");
            }
            var now = time.GetUtcNow();
            course.CompletedAt = request.Completed ? course.CompletedAt ?? now : null;
            if (request.Completed && request.CompleteSessions)
            {
                foreach (var session in await db.CourseSessions.Where(s => s.CourseId == id && s.CompletedAt == null).ToListAsync(ct))
                {
                    session.CompletedAt = now;
                }
            }
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        courses.MapPost("/{id:guid}/sessions", async (Guid id, SessionRequest request, AppDbContext db, CancellationToken ct) =>
        {
            if (!await db.Courses.AnyAsync(c => c.Id == id, ct))
            {
                return Problems.NotFound("Nie znaleziono szkolenia.");
            }
            if (request.StartTime is { } from && request.EndTime is { } to && to <= from)
            {
                return Problems.Bad("Godzina końca musi być późniejsza niż początek.");
            }
            var session = new CourseSession { CourseId = id, Topic = request.Topic.Trim() };
            ApplySession(session, request);
            db.CourseSessions.Add(session);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/sessions/{session.Id}", new { session.Id });
        }).Validate<SessionRequest>();

        courses.MapPut("/{id:guid}/attendance", async (Guid id, AttendanceRequest request, AppDbContext db, CancellationToken ct) =>
        {
            var session = await db.CourseSessions.AnyAsync(s => s.Id == request.SessionId && s.CourseId == id, ct);
            var enrollment = await db.Enrollments.AnyAsync(e => e.Id == request.EnrollmentId && e.CourseId == id, ct);
            if (!session || !enrollment)
            {
                return Problems.NotFound("Nie znaleziono zajęć lub kursanta w tym szkoleniu.");
            }
            var existing = await db.CourseAttendance.FirstOrDefaultAsync(a => a.SessionId == request.SessionId && a.EnrollmentId == request.EnrollmentId, ct);
            if (request.Present && existing is null)
            {
                db.CourseAttendance.Add(new CourseAttendance { SessionId = request.SessionId, EnrollmentId = request.EnrollmentId });
            }
            else if (!request.Present && existing is not null)
            {
                db.CourseAttendance.Remove(existing);
            }
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        courses.MapPost("/{id:guid}/materials", async (Guid id, MaterialRequest request, AppDbContext db, CancellationToken ct) =>
        {
            if (!await db.Courses.AnyAsync(c => c.Id == id, ct))
            {
                return Problems.NotFound("Nie znaleziono szkolenia.");
            }
            var url = Clean(request.Url);
            // Tylko zwykłe linki: javascript: i podobne mogłyby wykonać kod po kliknięciu.
            if (url is not null && !(Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)))
            {
                return Problems.Bad("Link musi zaczynać się od https:// lub http://.");
            }
            var material = new CourseMaterial { CourseId = id, Title = request.Title.Trim(), Url = url, Note = Clean(request.Note) };
            db.CourseMaterials.Add(material);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/materials/{material.Id}", new { material.Id });
        }).Validate<MaterialRequest>();

        courses.MapPost("/{id:guid}/tests", async (Guid id, TestRequest request, AppDbContext db, CancellationToken ct) =>
        {
            if (!await db.Courses.AnyAsync(c => c.Id == id, ct))
            {
                return Problems.NotFound("Nie znaleziono szkolenia.");
            }
            if (request.PassScore > request.MaxScore)
            {
                return Problems.Bad("Próg zaliczenia nie może być wyższy niż maksimum punktów.");
            }
            var test = new CourseTest { CourseId = id, Title = request.Title.Trim(), Date = request.Date, MaxScore = request.MaxScore, PassScore = request.PassScore };
            db.CourseTests.Add(test);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/tests/{test.Id}", new { test.Id });
        }).Validate<TestRequest>();

        courses.MapPost("/{id:guid}/costs", async (Guid id, CostRequest request, AppDbContext db, CancellationToken ct) =>
        {
            if (!await db.Courses.AnyAsync(c => c.Id == id, ct))
            {
                return Problems.NotFound("Nie znaleziono szkolenia.");
            }
            if (decimal.Round(request.Amount, 2) != request.Amount)
            {
                return Problems.Bad("Kwota może mieć najwyżej dwa miejsca po przecinku.");
            }
            var cost = new CruiseCost { CourseId = id, Category = request.Category.Trim(), Description = Clean(request.Description), Amount = request.Amount };
            db.CruiseCosts.Add(cost);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/costs/{cost.Id}", new { cost.Id });
        }).Validate<CostRequest>();

        var materialsGroup = api.MapGroup("/materials").RequireModule(Modules.Courses);
        materialsGroup.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
        {
            var material = await db.CourseMaterials.FirstOrDefaultAsync(m => m.Id == id, ct);
            if (material is null)
            {
                return Problems.NotFound("Nie znaleziono materiału.");
            }
            db.CourseMaterials.Remove(material);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        var testsGroup = api.MapGroup("/tests").RequireModule(Modules.Courses);
        testsGroup.MapPut("/{id:guid}/results", async (Guid id, TestResultRequest request, AppDbContext db, CancellationToken ct) =>
        {
            var test = await db.CourseTests.FirstOrDefaultAsync(t => t.Id == id, ct);
            if (test is null || !await db.Enrollments.AnyAsync(e => e.Id == request.EnrollmentId && e.CourseId == test.CourseId, ct))
            {
                return Problems.NotFound("Nie znaleziono testu lub kursanta w tym szkoleniu.");
            }
            if (request.Score is < 0 || request.Score > test.MaxScore)
            {
                return Problems.Bad($"Wynik musi być od 0 do {test.MaxScore:0.##} pkt.");
            }
            var existing = await db.CourseTestResults.FirstOrDefaultAsync(r => r.TestId == id && r.EnrollmentId == request.EnrollmentId, ct);
            if (request.Score is { } score)
            {
                if (existing is null)
                {
                    db.CourseTestResults.Add(new CourseTestResult { TestId = id, EnrollmentId = request.EnrollmentId, Score = score });
                }
                else
                {
                    existing.Score = score;
                }
            }
            else if (existing is not null)
            {
                db.CourseTestResults.Remove(existing);
            }
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
        testsGroup.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
        {
            var test = await db.CourseTests.FirstOrDefaultAsync(t => t.Id == id, ct);
            if (test is null)
            {
                return Problems.NotFound("Nie znaleziono testu.");
            }
            db.CourseTestResults.RemoveRange(await db.CourseTestResults.Where(r => r.TestId == id).ToListAsync(ct));
            db.CourseTests.Remove(test);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        var sessions = api.MapGroup("/sessions").RequireModule(Modules.Courses);
        sessions.MapPut("/{id:guid}/complete", async (Guid id, CompleteRequest request, AppDbContext db, TimeProvider time, CancellationToken ct) =>
        {
            var session = await db.CourseSessions.FirstOrDefaultAsync(s => s.Id == id, ct);
            if (session is null)
            {
                return Problems.NotFound("Nie znaleziono zajęć.");
            }
            session.CompletedAt = request.Completed ? session.CompletedAt ?? time.GetUtcNow() : null;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
        sessions.MapPut("/{id:guid}", async (Guid id, SessionRequest request, AppDbContext db, CancellationToken ct) =>
        {
            var session = await db.CourseSessions.FirstOrDefaultAsync(s => s.Id == id, ct);
            if (session is null)
            {
                return Problems.NotFound("Nie znaleziono zajęć.");
            }
            if (request.StartTime is { } from && request.EndTime is { } to && to <= from)
            {
                return Problems.Bad("Godzina końca musi być późniejsza niż początek.");
            }
            ApplySession(session, request);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).Validate<SessionRequest>();
        sessions.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
        {
            var session = await db.CourseSessions.FirstOrDefaultAsync(s => s.Id == id, ct);
            if (session is null)
            {
                return Problems.NotFound("Nie znaleziono zajęć.");
            }
            db.CourseAttendance.RemoveRange(await db.CourseAttendance.Where(a => a.SessionId == id).ToListAsync(ct));
            db.CourseSessions.Remove(session);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        // Postęp kursanta: dostarczone dokumenty, wynik egzaminu i numer patentu lub świadectwa.
        api.MapGroup("/enrollments").RequireModule(Modules.Courses).MapPut("/{id:guid}/progress", async (Guid id, ProgressRequest request, AppDbContext db, CancellationToken ct) =>
        {
            var enrollment = await db.Enrollments.Include(e => e.Course).FirstOrDefaultAsync(e => e.Id == id && e.CourseId != null, ct);
            if (enrollment is null)
            {
                return Problems.NotFound("Nie znaleziono kursanta.");
            }
            var documents = (request.DocumentsDelivered ?? []).Distinct().ToArray();
            if (documents.Any(d => !enrollment.Course!.RequiredDocuments.Contains(d)))
            {
                return Problems.Bad("Można odhaczyć tylko dokumenty wymagane na tym szkoleniu.");
            }
            enrollment.ExamResult = request.ExamResult;
            enrollment.CertificateNumber = string.IsNullOrWhiteSpace(request.CertificateNumber) ? null : request.CertificateNumber.Trim();
            enrollment.DocumentsDelivered = documents;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).Validate<ProgressRequest>();
    }

    private static async Task<IResult> Enroll(Guid id, EnrollRequest request, AppDbContext db, CancellationToken ct)
    {
        if (EnrollmentRules.ValidateRequest(request) is { } invalid)
        {
            return invalid;
        }
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (course is null)
        {
            return Problems.NotFound("Nie znaleziono szkolenia.");
        }
        if (course.Status == CruiseStatus.Cancelled)
        {
            return Problems.Conflict("To szkolenie jest odwołane.");
        }
        if (course.CompletedAt is not null)
        {
            return Problems.Conflict("To szkolenie jest zakończone. Wznów je, aby dopisać kursanta.");
        }
        // Blokada szkolenia: równoczesne zapisy nie przekroczą limitu miejsc.
        await LockCourse(db, id, ct);
        var confirmed = await db.Enrollments.CountAsync(e => e.CourseId == id && e.Status == EnrollmentStatus.Confirmed, ct);
        if (confirmed >= course.Capacity)
        {
            return Problems.Conflict($"Na szkoleniu nie ma wolnych miejsc (limit: {course.Capacity}).");
        }
        var (participant, error) = await EnrollmentRules.ResolveParticipantAsync(db, request,
            participantId => db.Enrollments.AnyAsync(e => e.CourseId == id && e.ParticipantId == participantId && e.Status != EnrollmentStatus.Cancelled, ct), ct);
        if (error is not null)
        {
            return error;
        }
        var enrollment = new Enrollment { CourseId = id, Participant = participant, Total = request.Total ?? course.Price, Status = EnrollmentStatus.Confirmed };
        db.Enrollments.Add(enrollment);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return Results.Created($"/api/enrollments/{enrollment.Id}", new { enrollment.Id, enrollment.Status });
    }

    private static IQueryable<CourseListItem> ListItems(IQueryable<Course> courses, DateOnly today) =>
        courses.AsNoTracking().OrderBy(c => c.StartDate).Select(c => new CourseListItem(
            c.Id, c.Type, c.Name, c.StartDate, c.EndDate, c.Location, c.Instructor, c.ExamDate,
            c.Price, c.Deposit, c.BalanceDueDate, c.Capacity, c.Status,
            c.Enrollments.Count(e => e.Status == EnrollmentStatus.Confirmed),
            c.Enrollments.Where(e => e.Status == EnrollmentStatus.Confirmed).SelectMany(e => e.Payments).Sum(p => (decimal?)p.Amount) ?? 0,
            c.Enrollments.Where(e => e.Status == EnrollmentStatus.Confirmed).Sum(e => (decimal?)e.Total) ?? 0,
            c.BalanceDueDate < today
                ? c.Enrollments.Count(e => e.Status == EnrollmentStatus.Confirmed && (e.Payments.Sum(p => (decimal?)p.Amount) ?? 0) < e.Total)
                : 0,
            c.Sessions.Count,
            c.Sessions.Count(s => s.CompletedAt != null),
            c.Enrollments.Count(e => e.Status == EnrollmentStatus.Confirmed && e.ExamResult == ExamResult.Passed),
            c.Costs.Sum(x => (decimal?)x.Amount) ?? 0,
            c.CompletedAt));

    private static Task LockCourse(AppDbContext db, Guid id, CancellationToken ct) =>
        db.Database.ExecuteSqlAsync($"""SELECT 1 FROM "Courses" WHERE "Id" = {id} FOR UPDATE""", ct);

    private static IResult? Validate(CourseRequest request)
    {
        if (request.EndDate < request.StartDate)
        {
            return Problems.Bad("Data końca nie może być wcześniejsza niż początek.");
        }
        if (request.Deposit > request.Price)
        {
            return Problems.Bad("Zaliczka nie może być wyższa niż cena.");
        }
        if (request.BalanceDueDate > request.StartDate)
        {
            return Problems.Bad("Termin dopłaty nie może być późniejszy niż początek szkolenia.");
        }
        if (request.ExamDate < request.StartDate)
        {
            return Problems.Bad("Egzamin nie może być przed początkiem szkolenia.");
        }
        var documents = request.RequiredDocuments ?? [];
        if (documents.Length > MaxDocuments || documents.Any(d => string.IsNullOrWhiteSpace(d) || d.Trim().Length > 100))
        {
            return Problems.Bad($"Podaj najwyżej {MaxDocuments} dokumentów, każdy do 100 znaków.");
        }
        return null;
    }

    private static void Apply(Course course, CourseRequest request)
    {
        course.Type = request.Type;
        course.Name = request.Name.Trim();
        course.StartDate = request.StartDate;
        course.EndDate = request.EndDate;
        course.Capacity = request.Capacity;
        course.Price = request.Price;
        course.Location = request.Location?.Trim() ?? "";
        course.Instructor = Clean(request.Instructor);
        course.Deposit = request.Deposit;
        course.BalanceDueDate = request.BalanceDueDate ?? request.StartDate;
        course.ExamDate = request.ExamDate;
        course.ExamInfo = Clean(request.ExamInfo);
        course.RequiredDocuments = (request.RequiredDocuments ?? []).Select(d => d.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        course.Status = request.Status;
        course.Description = Clean(request.Description);
    }

    private static void ApplySession(CourseSession session, SessionRequest request)
    {
        session.Date = request.Date;
        session.StartTime = request.StartTime;
        session.EndTime = request.EndTime;
        session.Kind = request.Kind;
        session.Topic = request.Topic.Trim();
        session.Notes = Clean(request.Notes);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

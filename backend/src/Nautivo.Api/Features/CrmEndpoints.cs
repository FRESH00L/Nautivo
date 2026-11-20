using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Nautivo.Api.Data;
using Nautivo.Api.Domain;
using Nautivo.Api.Infrastructure;

namespace Nautivo.Api.Features;

/// <summary>
/// Etap relacji z klientem, liczony z historii rejsów (nie wpisywany ręcznie, więc się nie rozjeżdża):
/// Lead – kontakt bez rejsu, Customer – jeden rejs, Regular – co najmniej dwa,
/// Lapsed – ostatni rejs ponad rok temu i nic zaplanowanego (warto się odezwać).
/// </summary>
public enum ContactStage { Lead, Customer, Regular, Lapsed }

public record ParticipantRequest(
    [property: Required(ErrorMessage = "Podaj imię i nazwisko."), MaxLength(200)] string FullName,
    [property: EmailAddress(ErrorMessage = "Podaj poprawny e-mail."), MaxLength(254)] string? Email,
    [property: MaxLength(40)] string? Phone,
    [property: MaxLength(200)] string? Experience,
    [property: MaxLength(200)] string? EmergencyContact,
    [property: MaxLength(4000)] string? Notes,
    [property: MaxLength(60)] string? Source = null,
    string[]? Tags = null,
    bool MarketingConsent = false,
    CourseType[]? ManualQualifications = null);

/// <summary>Uprawnienie ze zdanego u nas egzaminu.</summary>
public record EarnedQualification(CourseType Type, Guid CourseId, string CourseName, DateOnly Date, string? CertificateNumber);

public record ParticipantListItem(
    Guid Id, string FullName, string? Email, string? Phone, string? Experience, string? Source, string[] Tags, bool MarketingConsent,
    ContactStage Stage, int Cruises, DateOnly? LastCruiseEnd, DateOnly? NextCruiseStart, decimal Paid, decimal Remaining,
    int OpenTasks, DateOnly? NextTaskDue, DateTimeOffset CreatedAt, bool Anonymized, CourseType[] Qualifications);

public record ParticipantPaymentItem(decimal Amount, DateOnly PaidOn, string Method);

/// <summary>Rejs albo szkolenie w historii klienta. Dokładnie jedno z CruiseId i CourseId jest ustawione.</summary>
public record ParticipantHistoryItem(
    Guid EnrollmentId, Guid? CruiseId, Guid? CourseId, string CruiseName, DateOnly StartDate, DateOnly EndDate, EnrollmentStatus Status,
    decimal Total, decimal Paid, PaymentState State, DateTimeOffset EnrolledAt, IReadOnlyList<ParticipantPaymentItem> Payments);

public record ActivityDto(Guid Id, ActivityType Type, string Text, DateTimeOffset OccurredAt, string? Author, bool IsSystem);
public record TaskDto(Guid Id, Guid ParticipantId, string ParticipantName, string Title, DateOnly DueDate, DateTimeOffset? DoneAt);

public record ParticipantDetail(
    Guid Id, string FullName, string? Email, string? Phone, string? Experience, string? EmergencyContact, string? Notes,
    string? Source, string[] Tags, bool MarketingConsent, DateTimeOffset? MarketingConsentAt, DateTimeOffset CreatedAt, DateTimeOffset? AnonymizedAt,
    CourseType[] ManualQualifications, IReadOnlyList<EarnedQualification> EarnedQualifications,
    ContactStage Stage, IReadOnlyList<ParticipantHistoryItem> History, IReadOnlyList<ActivityDto> Activities, IReadOnlyList<TaskDto> Tasks);

public record ActivityRequest(
    ActivityType Type,
    [property: Required(ErrorMessage = "Wpisz treść."), MaxLength(2000)] string Text,
    DateTimeOffset? OccurredAt);

public record TaskRequest(
    [property: Required(ErrorMessage = "Wpisz, co jest do zrobienia."), MaxLength(200)] string Title,
    DateOnly DueDate);

public record TaskDoneRequest(bool Done);

public static class CrmEndpoints
{
    private const int MaxTags = 10;
    private const int MaxTagLength = 30;
    private const string AnonymizedName = "Osoba zanonimizowana";

    public static ContactStage StageOf(int cruises, DateOnly? lastEnd, DateOnly? nextStart, DateOnly today) =>
        cruises == 0 ? ContactStage.Lead
        : nextStart is null && lastEnd < today.AddYears(-1) ? ContactStage.Lapsed
        : cruises >= 2 ? ContactStage.Regular
        : ContactStage.Customer;

    public static void MapParticipantEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/participants").RequireModule(Modules.Participants);

        group.MapGet("/", async (string? search, AppDbContext db, TimeProvider time, CancellationToken ct) =>
        {
            var today = time.Today();
            var query = db.Participants.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search.Trim().Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_")}%";
                query = query.Where(p => EF.Functions.ILike(p.FullName, pattern) || EF.Functions.ILike(p.Email ?? "", pattern) || EF.Functions.ILike(p.Phone ?? "", pattern));
            }
            var rows = await query.OrderBy(p => p.FullName).Take(1000).Select(p => new
            {
                p.Id, p.FullName, p.Email, p.Phone, p.Experience, p.Source, p.Tags, p.MarketingConsent, p.CreatedAt, p.AnonymizedAt,
                // Rejsy i szkolenia liczą się jednakowo: każde z nich to zakup u organizatora.
                Cruises = p.Enrollments.Count(e => e.Status == EnrollmentStatus.Confirmed && (e.Cruise != null ? e.Cruise.Status : e.Course!.Status) == CruiseStatus.Planned),
                LastEnd = p.Enrollments
                    .Where(e => e.Status == EnrollmentStatus.Confirmed && (e.Cruise != null ? e.Cruise.Status : e.Course!.Status) == CruiseStatus.Planned && (e.Cruise != null ? e.Cruise.EndDate : e.Course!.EndDate) < today)
                    .Max(e => (DateOnly?)(e.Cruise != null ? e.Cruise.EndDate : e.Course!.EndDate)),
                NextStart = p.Enrollments
                    .Where(e => e.Status == EnrollmentStatus.Confirmed && (e.Cruise != null ? e.Cruise.Status : e.Course!.Status) == CruiseStatus.Planned && (e.Cruise != null ? e.Cruise.EndDate : e.Course!.EndDate) >= today)
                    .Min(e => (DateOnly?)(e.Cruise != null ? e.Cruise.StartDate : e.Course!.StartDate)),
                Paid = p.Enrollments.SelectMany(e => e.Payments).Sum(x => (decimal?)x.Amount) ?? 0,
                Due = p.Enrollments.Where(e => e.Status == EnrollmentStatus.Confirmed).Sum(e => (decimal?)e.Total) ?? 0,
                PaidConfirmed = p.Enrollments.Where(e => e.Status == EnrollmentStatus.Confirmed).SelectMany(e => e.Payments).Sum(x => (decimal?)x.Amount) ?? 0,
                OpenTasks = p.Tasks.Count(t => t.DoneAt == null),
                NextTask = p.Tasks.Where(t => t.DoneAt == null).Min(t => (DateOnly?)t.DueDate),
                p.ManualQualifications,
                Earned = p.Enrollments.Where(e => e.CourseId != null && e.ExamResult == ExamResult.Passed).Select(e => e.Course!.Type).ToList(),
            }).ToListAsync(ct);

            return rows.Select(r => new ParticipantListItem(
                r.Id, r.FullName, r.Email, r.Phone, r.Experience, r.Source, r.Tags, r.MarketingConsent,
                StageOf(r.Cruises, r.LastEnd, r.NextStart, today), r.Cruises, r.LastEnd, r.NextStart,
                r.Paid, Math.Max(0, r.Due - r.PaidConfirmed), r.OpenTasks, r.NextTask, r.CreatedAt, r.AnonymizedAt is not null,
                Qualifications(r.ManualQualifications, r.Earned))).ToList();
        });

        group.MapGet("/{id:guid}", async (Guid id, AppDbContext db, TimeProvider time, CancellationToken ct) =>
        {
            var detail = await LoadDetail(db, id, time.Today(), ct);
            return detail is null ? Problems.NotFound("Nie znaleziono klienta.") : Results.Ok(detail);
        });

        // Eksport wszystkich danych osoby (prawo dostępu i przenoszenia danych, art. 15 i 20 RODO).
        group.MapGet("/{id:guid}/export", async (Guid id, AppDbContext db, TimeProvider time, CancellationToken ct) =>
        {
            var detail = await LoadDetail(db, id, time.Today(), ct);
            if (detail is null)
            {
                return Problems.NotFound("Nie znaleziono klienta.");
            }
            return Results.Json(new { exportedAt = DateTimeOffset.UtcNow, person = detail }, statusCode: 200);
        });

        group.MapPost("/", async (ParticipantRequest request, AppDbContext db, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var error = ValidateTags(request.Tags);
            if (error is not null)
            {
                return error;
            }
            var participant = new Participant { FullName = request.FullName.Trim() };
            Apply(participant, request);
            db.Participants.Add(participant);
            LogConsentChange(db, participant, before: false, user);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/participants/{participant.Id}", new { participant.Id });
        }).Validate<ParticipantRequest>();

        group.MapPut("/{id:guid}", async (Guid id, ParticipantRequest request, AppDbContext db, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var participant = await db.Participants.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (participant is null)
            {
                return Problems.NotFound("Nie znaleziono klienta.");
            }
            if (participant.AnonymizedAt is not null)
            {
                return Problems.Conflict("Dane tej osoby zostały zanonimizowane i nie można ich już edytować.");
            }
            var error = ValidateTags(request.Tags);
            if (error is not null)
            {
                return error;
            }
            var before = participant.MarketingConsent;
            Apply(participant, request);
            LogConsentChange(db, participant, before, user);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).Validate<ParticipantRequest>();

        group.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
        {
            var participant = await db.Participants.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (participant is null)
            {
                return Problems.NotFound("Nie znaleziono klienta.");
            }
            if (await db.Enrollments.AnyAsync(e => e.ParticipantId == id, ct))
            {
                return Problems.Conflict("Ta osoba ma historię rejsów i wpłat, więc nie można jej usunąć. Możesz ją zanonimizować.");
            }
            if (await db.Charters.AnyAsync(c => c.ParticipantId == id, ct))
            {
                return Problems.Conflict("Ta osoba ma rezerwacje czarterowe, więc nie można jej usunąć. Możesz ją zanonimizować.");
            }
            db.ContactActivities.RemoveRange(await db.ContactActivities.Where(a => a.ParticipantId == id).ToListAsync(ct));
            db.ContactTasks.RemoveRange(await db.ContactTasks.Where(t => t.ParticipantId == id).ToListAsync(ct));
            db.Participants.Remove(participant);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        // Usunięcie danych na żądanie osoby, gdy rozliczenia trzeba zachować. Zostają kwoty i daty wpłat, znika wszystko, co identyfikuje osobę.
        group.MapPost("/{id:guid}/anonymize", async (Guid id, AppDbContext db, CancellationToken ct) =>
        {
            var participant = await db.Participants.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (participant is null)
            {
                return Problems.NotFound("Nie znaleziono klienta.");
            }
            participant.FullName = AnonymizedName;
            participant.Email = null;
            participant.Phone = null;
            participant.Experience = null;
            participant.EmergencyContact = null;
            participant.Notes = null;
            participant.Source = null;
            participant.Tags = [];
            participant.ManualQualifications = [];
            participant.MarketingConsent = false;
            participant.MarketingConsentAt = null;
            participant.AnonymizedAt = DateTimeOffset.UtcNow;
            // Numer patentu identyfikuje osobę, więc też znika. Kwoty i daty wpłat zostają.
            foreach (var enrollment in await db.Enrollments.Where(e => e.ParticipantId == id && e.CertificateNumber != null).ToListAsync(ct))
            {
                enrollment.CertificateNumber = null;
            }
            db.ContactActivities.RemoveRange(await db.ContactActivities.Where(a => a.ParticipantId == id).ToListAsync(ct));
            db.ContactTasks.RemoveRange(await db.ContactTasks.Where(t => t.ParticipantId == id).ToListAsync(ct));
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        group.MapPost("/{id:guid}/activities", async (Guid id, ActivityRequest request, AppDbContext db, ClaimsPrincipal user, TimeProvider time, CancellationToken ct) =>
        {
            var participant = await db.Participants.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (participant is null)
            {
                return Problems.NotFound("Nie znaleziono klienta.");
            }
            if (participant.AnonymizedAt is not null)
            {
                return Problems.Conflict("Dane tej osoby zostały zanonimizowane.");
            }
            var now = time.GetUtcNow();
            if (request.OccurredAt > now.AddMinutes(5))
            {
                return Problems.Bad("Data kontaktu nie może być z przyszłości. Na przyszłość dodaj zadanie.");
            }
            var activity = new ContactActivity { ParticipantId = id, Type = request.Type, Text = request.Text.Trim(), OccurredAt = request.OccurredAt ?? now, CreatedByUserId = user.UserId() };
            db.ContactActivities.Add(activity);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/activities/{activity.Id}", new { activity.Id });
        }).Validate<ActivityRequest>();

        group.MapPost("/{id:guid}/tasks", async (Guid id, TaskRequest request, AppDbContext db, CancellationToken ct) =>
        {
            var participant = await db.Participants.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (participant is null)
            {
                return Problems.NotFound("Nie znaleziono klienta.");
            }
            if (participant.AnonymizedAt is not null)
            {
                return Problems.Conflict("Dane tej osoby zostały zanonimizowane.");
            }
            var task = new ContactTask { ParticipantId = id, Title = request.Title.Trim(), DueDate = request.DueDate };
            db.ContactTasks.Add(task);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/tasks/{task.Id}", new { task.Id });
        }).Validate<TaskRequest>();

        var activities = api.MapGroup("/activities").RequireModule(Modules.Participants);
        activities.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
        {
            var activity = await db.ContactActivities.FirstOrDefaultAsync(a => a.Id == id, ct);
            if (activity is null)
            {
                return Problems.NotFound("Nie znaleziono wpisu.");
            }
            if (activity.IsSystem)
            {
                return Problems.Conflict("Tego wpisu nie można usunąć: dokumentuje zgodę klienta.");
            }
            db.ContactActivities.Remove(activity);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        var tasks = api.MapGroup("/tasks").RequireModule(Modules.Participants);
        // Otwarte zadania całej organizacji: lista „do zrobienia” na górze CRM.
        tasks.MapGet("/", async (AppDbContext db, CancellationToken ct) =>
            await db.ContactTasks.AsNoTracking().Where(t => t.DoneAt == null).OrderBy(t => t.DueDate).Take(200)
                .Select(t => new TaskDto(t.Id, t.ParticipantId, t.Participant!.FullName, t.Title, t.DueDate, t.DoneAt)).ToListAsync(ct));
        tasks.MapPut("/{id:guid}/done", async (Guid id, TaskDoneRequest request, AppDbContext db, TimeProvider time, CancellationToken ct) =>
        {
            var task = await db.ContactTasks.FirstOrDefaultAsync(t => t.Id == id, ct);
            if (task is null)
            {
                return Problems.NotFound("Nie znaleziono zadania.");
            }
            task.DoneAt = request.Done ? time.GetUtcNow() : null;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
        tasks.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
        {
            var task = await db.ContactTasks.FirstOrDefaultAsync(t => t.Id == id, ct);
            if (task is null)
            {
                return Problems.NotFound("Nie znaleziono zadania.");
            }
            db.ContactTasks.Remove(task);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
    }

    private static async Task<ParticipantDetail?> LoadDetail(AppDbContext db, Guid id, DateOnly today, CancellationToken ct)
    {
        var p = await db.Participants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null)
        {
            return null;
        }
        var history = await db.Enrollments.AsNoTracking().Where(e => e.ParticipantId == id)
            .Select(e => new
            {
                e.Id, e.CruiseId, e.CourseId,
                Name = e.Cruise != null ? e.Cruise.Name : e.Course!.Name,
                StartDate = e.Cruise != null ? e.Cruise.StartDate : e.Course!.StartDate,
                EndDate = e.Cruise != null ? e.Cruise.EndDate : e.Course!.EndDate,
                BalanceDueDate = e.Cruise != null ? e.Cruise.BalanceDueDate : e.Course!.BalanceDueDate,
                CruiseStatus = e.Cruise != null ? e.Cruise.Status : e.Course!.Status,
                e.Status, e.Total, e.CreatedAt,
                Payments = e.Payments.OrderBy(x => x.PaidOn).Select(x => new ParticipantPaymentItem(x.Amount, x.PaidOn, x.Method)).ToList(),
            })
            .ToListAsync(ct);
        history = history.OrderByDescending(h => h.StartDate).ToList();
        var activities = await db.ContactActivities.AsNoTracking().Where(a => a.ParticipantId == id).OrderByDescending(a => a.OccurredAt)
            .Select(a => new { a.Id, a.Type, a.Text, a.OccurredAt, a.CreatedByUserId, a.IsSystem }).ToListAsync(ct);
        var authorIds = activities.Where(a => a.CreatedByUserId != null).Select(a => a.CreatedByUserId!.Value).Distinct().ToList();
        var authors = await db.Users.AsNoTracking().Where(u => authorIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.FullName, ct);
        var tasks = await db.ContactTasks.AsNoTracking().Where(t => t.ParticipantId == id).OrderBy(t => t.DoneAt != null).ThenBy(t => t.DueDate)
            .Select(t => new TaskDto(t.Id, t.ParticipantId, p.FullName, t.Title, t.DueDate, t.DoneAt)).ToListAsync(ct);

        var earned = await db.Enrollments.AsNoTracking().Where(e => e.ParticipantId == id && e.CourseId != null && e.ExamResult == ExamResult.Passed)
            .Select(e => new EarnedQualification(e.Course!.Type, e.Course.Id, e.Course.Name, e.Course.ExamDate ?? e.Course.EndDate, e.CertificateNumber))
            .ToListAsync(ct);

        var sailed = history.Where(h => h.Status == EnrollmentStatus.Confirmed && h.CruiseStatus == CruiseStatus.Planned).ToList();
        var stage = StageOf(
            sailed.Count,
            sailed.Where(h => h.EndDate < today).Select(h => (DateOnly?)h.EndDate).Max(),
            sailed.Where(h => h.EndDate >= today).Select(h => (DateOnly?)h.StartDate).Min(),
            today);

        return new ParticipantDetail(
            p.Id, p.FullName, p.Email, p.Phone, p.Experience, p.EmergencyContact, p.Notes,
            p.Source, p.Tags, p.MarketingConsent, p.MarketingConsentAt, p.CreatedAt, p.AnonymizedAt,
            ParseQualifications(p.ManualQualifications), earned, stage,
            history.Select(h =>
            {
                var paid = h.Payments.Sum(x => x.Amount);
                return new ParticipantHistoryItem(h.Id, h.CruiseId, h.CourseId, h.Name, h.StartDate, h.EndDate, h.Status, h.Total, paid, CruiseEndpoints.StateOf(h.Total, paid, h.BalanceDueDate, today), h.CreatedAt, h.Payments);
            }).ToList(),
            activities.Select(a => new ActivityDto(a.Id, a.Type, a.Text, a.OccurredAt, a.CreatedByUserId is { } uid ? authors.GetValueOrDefault(uid) : null, a.IsSystem)).ToList(),
            tasks);
    }

    private static IResult? ValidateTags(string[]? tags)
    {
        if (tags is null)
        {
            return null;
        }
        if (tags.Length > MaxTags)
        {
            return Problems.Bad($"Klient może mieć najwyżej {MaxTags} tagów.");
        }
        return tags.Any(t => string.IsNullOrWhiteSpace(t) || t.Trim().Length > MaxTagLength)
            ? Problems.Bad($"Tag nie może być pusty ani dłuższy niż {MaxTagLength} znaków.")
            : null;
    }

    private static void Apply(Participant participant, ParticipantRequest request)
    {
        participant.FullName = request.FullName.Trim();
        participant.Email = Clean(request.Email);
        participant.Phone = Clean(request.Phone);
        participant.Experience = Clean(request.Experience);
        participant.EmergencyContact = Clean(request.EmergencyContact);
        participant.Notes = Clean(request.Notes);
        participant.Source = Clean(request.Source);
        participant.Tags = (request.Tags ?? []).Select(t => t.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        participant.ManualQualifications = (request.ManualQualifications ?? []).Distinct().Select(q => q.ToString()).ToArray();
        // Data zgody zmienia się tylko przy jej udzieleniu; wycofanie ją kasuje.
        if (request.MarketingConsent && !participant.MarketingConsent)
        {
            participant.MarketingConsentAt = DateTimeOffset.UtcNow;
        }
        else if (!request.MarketingConsent)
        {
            participant.MarketingConsentAt = null;
        }
        participant.MarketingConsent = request.MarketingConsent;
    }

    /// <summary>Każda zmiana zgody trafia do historii kontaktu, żeby dało się wykazać, kiedy ją udzielono lub wycofano.</summary>
    private static void LogConsentChange(AppDbContext db, Participant participant, bool before, ClaimsPrincipal user)
    {
        if (before == participant.MarketingConsent)
        {
            return;
        }
        db.ContactActivities.Add(new ContactActivity
        {
            Participant = participant,
            Type = ActivityType.Note,
            IsSystem = true,
            Text = participant.MarketingConsent ? "Odnotowano zgodę na informacje marketingowe." : "Odnotowano wycofanie zgody na informacje marketingowe.",
            OccurredAt = DateTimeOffset.UtcNow,
            CreatedByUserId = user.UserId(),
        });
    }

    private static CourseType[] ParseQualifications(string[] stored) =>
        stored.Select(q => Enum.TryParse<CourseType>(q, out var type) ? (CourseType?)type : null).OfType<CourseType>().ToArray();

    /// <summary>Uprawnienia klienta: wpisane ręcznie oraz ze zdanych u nas egzaminów, bez powtórzeń.</summary>
    private static CourseType[] Qualifications(string[] manual, IEnumerable<CourseType> earned) =>
        ParseQualifications(manual).Concat(earned).Distinct().OrderBy(q => q).ToArray();

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

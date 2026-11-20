using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Nautivo.Api.Data;
using Nautivo.Api.Domain;
using Nautivo.Api.Infrastructure;

namespace Nautivo.Api.Features;

public record RoutePointDto(string Name, double? Lat, double? Lng);

public record CruiseRequest(
    [property: Required(ErrorMessage = "Podaj nazwę rejsu."), MaxLength(200)] string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    [property: Range(1, 500, ErrorMessage = "Podaj liczbę osób od 1 do 500.")] int Capacity,
    [property: Range(typeof(decimal), "0", "9999999", ErrorMessage = "Podaj poprawną cenę.")] decimal Price,
    [property: Required(ErrorMessage = "Podaj port startowy i docelowy.")] List<RoutePointDto> RoutePoints,
    [property: Range(typeof(decimal), "0", "9999999", ErrorMessage = "Podaj poprawną zaliczkę.")] decimal Deposit = 0,
    DateOnly? BalanceDueDate = null,
    Guid? VesselId = null,
    [property: MaxLength(100)] string? YachtName = null,
    [property: MaxLength(100)] string? YachtModel = null,
    [property: Range(typeof(decimal), "0", "999", ErrorMessage = "Podaj poprawną długość jachtu.")] decimal? YachtLengthMeters = null,
    CruiseStatus Status = CruiseStatus.Planned,
    [property: MaxLength(4000)] string? Description = null,
    bool? IsConfirmed = null);

public record NewParticipant(
    [property: Required(ErrorMessage = "Podaj imię i nazwisko."), MaxLength(200)] string FullName,
    [property: EmailAddress(ErrorMessage = "Podaj poprawny e-mail."), MaxLength(254)] string? Email,
    [property: MaxLength(40)] string? Phone,
    [property: MaxLength(200)] string? Experience);

public record EnrollRequest(Guid? ParticipantId, NewParticipant? Participant, decimal? Total);

public record CrewMemberRequest(
    [property: Required(ErrorMessage = "Podaj imię i nazwisko."), MaxLength(200)] string FullName,
    [property: EmailAddress(ErrorMessage = "Podaj poprawny e-mail."), MaxLength(254)] string? Email,
    [property: MaxLength(40)] string? Phone,
    [property: Range(typeof(decimal), "0", "9999999", ErrorMessage = "Podaj poprawną kwotę do zapłaty.")] decimal Total);

public record PaymentRequest(
    [property: Range(typeof(decimal), "0.01", "9999999", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true, ErrorMessage = "Podaj kwotę większą od zera.")] decimal Amount,
    DateOnly? PaidOn,
    [property: MaxLength(40)] string? Method,
    [property: MaxLength(300)] string? Note);

public record CostRequest(
    [property: Required(ErrorMessage = "Wybierz kategorię kosztu."), MaxLength(60)] string Category,
    [property: MaxLength(300)] string? Description,
    [property: Range(typeof(decimal), "0", "9999999", ErrorMessage = "Podaj poprawną kwotę kosztu.")] decimal Amount);

/// <summary>Stan rozliczenia uczestnika. Partial oznacza wpłaconą zaliczkę lub część kwoty przed terminem dopłaty.</summary>
public enum PaymentState { Paid, Partial, None, Overdue, Overpaid }

public record CruiseListItem(
    Guid Id, string Name, string DeparturePort, string ArrivalPort, DateOnly StartDate, DateOnly EndDate,
    Guid? VesselId, string? YachtName, string? YachtModel, decimal? YachtLengthMeters,
    decimal Price, decimal Deposit, DateOnly BalanceDueDate, int Capacity, CruiseStatus Status, bool IsConfirmed,
    int Confirmed, decimal Collected, decimal Expected, int Overdue, decimal Costs);

public record PaymentDto(Guid Id, decimal Amount, DateOnly PaidOn, string Method, string? Note);

public record EnrollmentDto(
    Guid Id, Guid ParticipantId, string FullName, string? Email, string? Phone, string? Experience,
    EnrollmentStatus Status, decimal Total, decimal Paid, decimal Remaining, PaymentState State,
    DateTimeOffset CreatedAt, IReadOnlyList<PaymentDto> Payments);

public record CostDto(Guid Id, string Category, string? Description, decimal Amount);

public record CruiseDetail(CruiseListItem Cruise, string? Description, IReadOnlyList<RoutePointDto> RoutePoints, IReadOnlyList<EnrollmentDto> Enrollments, IReadOnlyList<CostDto> Costs);

public static class CruiseEndpoints
{
    private const int MaxRoutePoints = 20;

    public static PaymentState StateOf(decimal total, decimal paid, DateOnly due, DateOnly today) =>
        paid > total ? PaymentState.Overpaid
        : paid == total ? PaymentState.Paid
        : due < today ? PaymentState.Overdue
        : paid > 0 ? PaymentState.Partial
        : PaymentState.None;

    public static void MapCruiseEndpoints(this RouteGroupBuilder api)
    {
        var cruises = api.MapGroup("/cruises").RequireModule(Modules.Cruises);

        cruises.MapGet("/", async (AppDbContext db, TimeProvider time, CancellationToken ct) =>
            await ListItems(db.Cruises, time.Today()).ToListAsync(ct));

        cruises.MapGet("/{id:guid}", async (Guid id, AppDbContext db, TimeProvider time, CancellationToken ct) =>
        {
            var today = time.Today();
            var cruise = await ListItems(db.Cruises.Where(c => c.Id == id), today).FirstOrDefaultAsync(ct);
            if (cruise is null)
            {
                return Problems.NotFound("Nie znaleziono rejsu.");
            }
            var extra = await db.Cruises.AsNoTracking().Where(c => c.Id == id).Select(c => new { c.Description, c.RoutePoints }).FirstAsync(ct);
            var enrollments = await db.Enrollments.AsNoTracking().Where(e => e.CruiseId == id && e.Status == EnrollmentStatus.Confirmed)
                .Include(e => e.Participant).Include(e => e.Payments)
                .OrderBy(e => e.CreatedAt).ToListAsync(ct);
            var costs = await db.CruiseCosts.AsNoTracking().Where(c => c.CruiseId == id).OrderBy(c => c.CreatedAt)
                .Select(c => new CostDto(c.Id, c.Category, c.Description, c.Amount)).ToListAsync(ct);
            return Results.Ok(new CruiseDetail(
                cruise, extra.Description,
                extra.RoutePoints.Select(p => new RoutePointDto(p.Name, p.Lat, p.Lng)).ToList(),
                enrollments.Select(e => ToDto(e, cruise.BalanceDueDate, today)).ToList(),
                costs));
        });

        cruises.MapPost("/", async (CruiseRequest request, AppDbContext db, CancellationToken ct) =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            var error = await ValidateCruise(db, request, null, ct);
            if (error is not null)
            {
                return error;
            }
            var cruise = new Cruise { Name = request.Name.Trim() };
            Apply(cruise, request);
            db.Cruises.Add(cruise);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return Results.Created($"/api/cruises/{cruise.Id}", new { cruise.Id });
        }).Validate<CruiseRequest>();

        cruises.MapPut("/{id:guid}", async (Guid id, CruiseRequest request, AppDbContext db, CancellationToken ct) =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            var cruise = await db.Cruises.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (cruise is null)
            {
                return Problems.NotFound("Nie znaleziono rejsu.");
            }
            await LockCruise(db, id, ct);
            var error = await ValidateCruise(db, request, id, ct);
            if (error is not null)
            {
                return error;
            }
            var crew = await db.Enrollments.Where(e => e.CruiseId == id && e.Status == EnrollmentStatus.Confirmed).ToListAsync(ct);
            if (request.Capacity < crew.Count)
            {
                return Problems.Conflict($"Na rejs zapisanych jest {crew.Count} osób. Nie można zmniejszyć limitu poniżej tej liczby.");
            }
            // Nowa cena obejmuje osoby, które płacą cenę standardową. Indywidualne kwoty (np. rabaty) zostają.
            foreach (var member in crew.Where(e => e.Total == cruise.Price))
            {
                member.Total = request.Price;
            }
            Apply(cruise, request);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return Results.NoContent();
        }).Validate<CruiseRequest>();

        // Usuwa rejs razem z zapisami, wpłatami i kosztami. Osoby zostają w bazie uczestników.
        cruises.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            var cruise = await db.Cruises.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (cruise is null)
            {
                return Problems.NotFound("Nie znaleziono rejsu.");
            }
            await LockCruise(db, id, ct);
            db.Payments.RemoveRange(await db.Payments.Where(p => p.Enrollment!.CruiseId == id).ToListAsync(ct));
            db.Enrollments.RemoveRange(await db.Enrollments.Where(e => e.CruiseId == id).ToListAsync(ct));
            db.CruiseCosts.RemoveRange(await db.CruiseCosts.Where(c => c.CruiseId == id).ToListAsync(ct));
            db.Cruises.Remove(cruise);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return Results.NoContent();
        });

        cruises.MapPost("/{id:guid}/enrollments", Enroll);
        cruises.MapPost("/{id:guid}/costs", AddCost).Validate<CostRequest>();

        var enrollments = api.MapGroup("/enrollments").RequireAnyModule(Modules.Cruises, Modules.Courses);
        enrollments.MapPut("/{id:guid}", UpdateCrewMember).Validate<CrewMemberRequest>();
        enrollments.MapDelete("/{id:guid}", RemoveCrewMember);
        enrollments.MapPost("/{id:guid}/payments", AddPayment).Validate<PaymentRequest>();

        api.MapGroup("/payments").RequireAnyModule(Modules.Cruises, Modules.Courses).MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
        {
            var payment = await db.Payments.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (payment is null)
            {
                return Problems.NotFound("Nie znaleziono wpłaty.");
            }
            db.Payments.Remove(payment);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        var costs = api.MapGroup("/costs").RequireAnyModule(Modules.Cruises, Modules.Courses);
        costs.MapPut("/{id:guid}", async (Guid id, CostRequest request, AppDbContext db, CancellationToken ct) =>
        {
            var cost = await db.CruiseCosts.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (cost is null)
            {
                return Problems.NotFound("Nie znaleziono kosztu.");
            }
            if (decimal.Round(request.Amount, 2) != request.Amount)
            {
                return Problems.Bad("Kwota może mieć najwyżej dwa miejsca po przecinku.");
            }
            cost.Category = request.Category.Trim();
            cost.Description = Clean(request.Description);
            cost.Amount = request.Amount;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).Validate<CostRequest>();
        costs.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
        {
            var cost = await db.CruiseCosts.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (cost is null)
            {
                return Problems.NotFound("Nie znaleziono kosztu.");
            }
            db.CruiseCosts.Remove(cost);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
    }

    private static async Task<IResult> Enroll(Guid id, EnrollRequest request, AppDbContext db, CancellationToken ct)
    {
        if (EnrollmentRules.ValidateRequest(request) is { } invalid)
        {
            return invalid;
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var cruise = await db.Cruises.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (cruise is null)
        {
            return Problems.NotFound("Nie znaleziono rejsu.");
        }
        if (cruise.Status == CruiseStatus.Cancelled)
        {
            return Problems.Conflict("Ten rejs jest odwołany.");
        }
        // Blokada rejsu: równoczesne zapisy nie przekroczą limitu miejsc.
        await LockCruise(db, id, ct);

        var confirmed = await db.Enrollments.CountAsync(e => e.CruiseId == id && e.Status == EnrollmentStatus.Confirmed, ct);
        if (confirmed >= cruise.Capacity)
        {
            return Problems.Conflict($"Na rejsie nie ma wolnych miejsc (limit: {cruise.Capacity}).");
        }

        var (participant, error) = await EnrollmentRules.ResolveParticipantAsync(db, request,
            participantId => db.Enrollments.AnyAsync(e => e.CruiseId == id && e.ParticipantId == participantId && e.Status != EnrollmentStatus.Cancelled, ct), ct);
        if (error is not null)
        {
            return error;
        }

        var enrollment = new Enrollment
        {
            CruiseId = id,
            Participant = participant,
            Total = request.Total ?? cruise.Price,
            Status = EnrollmentStatus.Confirmed,
        };
        db.Enrollments.Add(enrollment);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return Results.Created($"/api/enrollments/{enrollment.Id}", new { enrollment.Id, enrollment.Status });
    }

    /// <summary>Zmienia dane osoby i kwotę do zapłaty. Dane osoby są wspólne dla wszystkich jej rejsów.</summary>
    private static async Task<IResult> UpdateCrewMember(Guid id, CrewMemberRequest request, AppDbContext db, CancellationToken ct)
    {
        var enrollment = await db.Enrollments.Include(e => e.Participant).FirstOrDefaultAsync(e => e.Id == id, ct);
        if (enrollment is null)
        {
            return Problems.NotFound("Nie znaleziono zapisu.");
        }
        if (decimal.Round(request.Total, 2) != request.Total)
        {
            return Problems.Bad("Kwota może mieć najwyżej dwa miejsca po przecinku.");
        }
        var person = enrollment.Participant!;
        person.FullName = request.FullName.Trim();
        person.Email = Clean(request.Email);
        person.Phone = Clean(request.Phone);
        enrollment.Total = request.Total;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    /// <summary>Usuwa osobę z rejsu lub szkolenia razem z jej wpłatami (i obecnościami). Osoba zostaje w bazie klientów.</summary>
    private static async Task<IResult> RemoveCrewMember(Guid id, AppDbContext db, CancellationToken ct)
    {
        var enrollment = await db.Enrollments.Include(e => e.Payments).FirstOrDefaultAsync(e => e.Id == id, ct);
        if (enrollment is null)
        {
            return Problems.NotFound("Nie znaleziono zapisu.");
        }
        db.CourseAttendance.RemoveRange(await db.CourseAttendance.Where(a => a.EnrollmentId == id).ToListAsync(ct));
        db.CourseTestResults.RemoveRange(await db.CourseTestResults.Where(r => r.EnrollmentId == id).ToListAsync(ct));
        db.Payments.RemoveRange(enrollment.Payments);
        db.Enrollments.Remove(enrollment);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    /// <summary>Wpłata może przekroczyć należność; uczestnik dostaje wtedy stan „nadpłata” do zwrotu lub rozliczenia.</summary>
    private static async Task<IResult> AddPayment(Guid id, PaymentRequest request, AppDbContext db, ClaimsPrincipal user, TimeProvider time, CancellationToken ct)
    {
        var enrollment = await db.Enrollments.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (enrollment is null)
        {
            return Problems.NotFound("Nie znaleziono zapisu.");
        }
        if (enrollment.Status != EnrollmentStatus.Confirmed)
        {
            return Problems.Conflict("Wpłatę można zapisać tylko dla osoby zapisanej na rejs.");
        }
        if (decimal.Round(request.Amount, 2) != request.Amount)
        {
            return Problems.Bad("Kwota może mieć najwyżej dwa miejsca po przecinku.");
        }
        var today = time.Today();
        if (request.PaidOn > today)
        {
            return Problems.Bad("Data wpłaty nie może być z przyszłości.");
        }
        var payment = new Payment
        {
            EnrollmentId = id,
            Amount = request.Amount,
            PaidOn = request.PaidOn ?? today,
            Method = string.IsNullOrWhiteSpace(request.Method) ? "Przelew" : request.Method.Trim(),
            Note = Clean(request.Note),
            RecordedByUserId = user.UserId(),
        };
        db.Payments.Add(payment);
        await db.SaveChangesAsync(ct);
        return Results.Created($"/api/payments/{payment.Id}", new { payment.Id });
    }

    private static async Task<IResult> AddCost(Guid id, CostRequest request, AppDbContext db, CancellationToken ct)
    {
        if (!await db.Cruises.AnyAsync(c => c.Id == id, ct))
        {
            return Problems.NotFound("Nie znaleziono rejsu.");
        }
        if (decimal.Round(request.Amount, 2) != request.Amount)
        {
            return Problems.Bad("Kwota może mieć najwyżej dwa miejsca po przecinku.");
        }
        var cost = new CruiseCost { CruiseId = id, Category = request.Category.Trim(), Description = Clean(request.Description), Amount = request.Amount };
        db.CruiseCosts.Add(cost);
        await db.SaveChangesAsync(ct);
        return Results.Created($"/api/costs/{cost.Id}", new { cost.Id });
    }

    private static IQueryable<CruiseListItem> ListItems(IQueryable<Cruise> cruises, DateOnly today) =>
        cruises.AsNoTracking().OrderBy(c => c.StartDate).Select(c => new CruiseListItem(
            c.Id, c.Name, c.DeparturePort, c.ArrivalPort, c.StartDate, c.EndDate,
            c.VesselId, c.YachtName, c.YachtModel, c.YachtLengthMeters,
            c.Price, c.Deposit, c.BalanceDueDate, c.Capacity, c.Status, c.IsConfirmed,
            c.Enrollments.Count(e => e.Status == EnrollmentStatus.Confirmed),
            c.Enrollments.Where(e => e.Status == EnrollmentStatus.Confirmed).SelectMany(e => e.Payments).Sum(p => (decimal?)p.Amount) ?? 0,
            c.Enrollments.Where(e => e.Status == EnrollmentStatus.Confirmed).Sum(e => (decimal?)e.Total) ?? 0,
            c.BalanceDueDate < today
                ? c.Enrollments.Count(e => e.Status == EnrollmentStatus.Confirmed && (e.Payments.Sum(p => (decimal?)p.Amount) ?? 0) < e.Total)
                : 0,
            c.Costs.Sum(x => (decimal?)x.Amount) ?? 0));

    public static EnrollmentDto ToDto(Enrollment e, DateOnly due, DateOnly today)
    {
        var paid = e.Payments.Sum(p => p.Amount);
        return new EnrollmentDto(
            e.Id, e.ParticipantId, e.Participant!.FullName, e.Participant.Email, e.Participant.Phone, e.Participant.Experience,
            e.Status, e.Total, paid, Math.Max(0, e.Total - paid), StateOf(e.Total, paid, due, today), e.CreatedAt,
            e.Payments.OrderBy(p => p.PaidOn).Select(p => new PaymentDto(p.Id, p.Amount, p.PaidOn, p.Method, p.Note)).ToList());
    }

    private static Task LockCruise(AppDbContext db, Guid id, CancellationToken ct) =>
        db.Database.ExecuteSqlAsync($"""SELECT 1 FROM "Cruises" WHERE "Id" = {id} FOR UPDATE""", ct);

    private static async Task<IResult?> ValidateCruise(AppDbContext db, CruiseRequest request, Guid? cruiseId, CancellationToken ct)
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
            return Problems.Bad("Termin dopłaty nie może być późniejszy niż początek rejsu.");
        }
        if (request.RoutePoints.Count < 2 || string.IsNullOrWhiteSpace(request.RoutePoints[0].Name) || string.IsNullOrWhiteSpace(request.RoutePoints[^1].Name))
        {
            return Problems.Bad("Podaj port startowy i docelowy.");
        }
        if (request.RoutePoints.Count > MaxRoutePoints)
        {
            return Problems.Bad($"Trasa może mieć najwyżej {MaxRoutePoints} punktów.");
        }
        foreach (var point in request.RoutePoints)
        {
            if (string.IsNullOrWhiteSpace(point.Name) || point.Name.Length > 100)
            {
                return Problems.Bad("Każdy port na trasie musi mieć nazwę (do 100 znaków).");
            }
            if ((point.Lat is null) != (point.Lng is null) || point.Lat is < -90 or > 90 || point.Lng is < -180 or > 180)
            {
                return Problems.Bad($"Niepoprawne współrzędne punktu „{point.Name}”.");
            }
        }
        if (request.VesselId is not { } vesselId)
        {
            return null;
        }

        var vessel = await db.Vessels.FirstOrDefaultAsync(v => v.Id == vesselId, ct);
        if (vessel is null)
        {
            return Problems.Bad("Wybrana jednostka nie istnieje.");
        }
        if (request.Capacity > vessel.Berths)
        {
            return Problems.Bad($"{vessel.Name} ma {vessel.Berths} miejsc, a rejs {request.Capacity} osób.");
        }
        if (request.Status == CruiseStatus.Cancelled)
        {
            return null;
        }

        // Blokada jednostki: dwa równoczesne zapisy nie zarezerwują jej na ten sam termin.
        await db.Database.ExecuteSqlAsync($"""SELECT 1 FROM "Vessels" WHERE "Id" = {vesselId} FOR UPDATE""", ct);

        // Rejsy mogą się stykać dniem wymiany załogi.
        var clash = await db.Cruises
            .Where(c => c.VesselId == vesselId && c.Id != cruiseId && c.Status == CruiseStatus.Planned
                && ((c.StartDate < request.EndDate && c.EndDate > request.StartDate) || c.StartDate == request.StartDate))
            .Select(c => c.Name).FirstOrDefaultAsync(ct);
        if (clash is not null)
        {
            return Problems.Conflict($"{vessel.Name} płynie w tym terminie w rejsie „{clash}”.");
        }
        var block = await db.VesselBlocks
            .Where(b => b.VesselId == vesselId && b.StartDate <= request.EndDate && b.EndDate >= request.StartDate)
            .Select(b => b.Reason).FirstOrDefaultAsync(ct);
        if (block is not null)
        {
            return Problems.Conflict($"{vessel.Name} jest w tym terminie niedostępna: {block}.");
        }
        // Czarter i rejs mogą się stykać dniem przekazania jednostki.
        var charter = await db.Charters
            .Where(c => c.VesselId == vesselId && ((c.StartDate < request.EndDate && c.EndDate > request.StartDate) || c.StartDate == request.StartDate))
            .Select(c => c.Participant!.FullName).FirstOrDefaultAsync(ct);
        return charter is null ? null : Problems.Conflict($"{vessel.Name} jest w tym terminie wynajęta w czarterze ({charter}).");
    }

    private static void Apply(Cruise cruise, CruiseRequest request)
    {
        var points = request.RoutePoints.Select(p => new RoutePoint { Name = p.Name.Trim(), Lat = p.Lat, Lng = p.Lng }).ToList();
        cruise.Name = request.Name.Trim();
        cruise.RoutePoints = points;
        cruise.DeparturePort = points[0].Name;
        cruise.ArrivalPort = points[^1].Name;
        cruise.StartDate = request.StartDate;
        cruise.EndDate = request.EndDate;
        cruise.VesselId = request.VesselId;
        cruise.YachtName = Clean(request.YachtName);
        cruise.YachtModel = Clean(request.YachtModel);
        cruise.YachtLengthMeters = request.YachtLengthMeters is > 0 ? request.YachtLengthMeters : null;
        cruise.Price = request.Price;
        cruise.Deposit = request.Deposit;
        cruise.BalanceDueDate = request.BalanceDueDate ?? request.StartDate;
        cruise.Capacity = request.Capacity;
        cruise.Status = request.Status;
        cruise.Description = Clean(request.Description);
        if (request.IsConfirmed is { } isConfirmed)
        {
            cruise.IsConfirmed = isConfirmed;
        }
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

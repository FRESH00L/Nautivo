using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Nautivo.Api.Data;
using Nautivo.Api.Domain;
using Nautivo.Api.Infrastructure;

namespace Nautivo.Api.Features;

/// <summary>Rezerwacja czarteru. Klient z bazy (ParticipantId) albo nowy (Participant). Total pusty: cena za dzień razy liczba dni.</summary>
public record CharterRequest(
    Guid VesselId,
    DateOnly StartDate,
    DateOnly EndDate,
    Guid? ParticipantId,
    NewParticipant? Participant,
    decimal? Total,
    [property: MaxLength(1000, ErrorMessage = "Notatka może mieć najwyżej 1000 znaków.")] string? Notes = null);

public record CharterDto(
    Guid Id, Guid VesselId, string VesselName, VesselKind VesselKind, Guid ParticipantId, string ClientName, string? ClientEmail, string? ClientPhone,
    DateOnly StartDate, DateOnly EndDate, int Days, decimal DayRate, decimal Total, string? Notes);

public record CharterClient(Guid Id, string FullName, string? Email, string? Phone);

public record CharterMonth(int Month, int Bookings, int Days, decimal Revenue);

/// <summary>Rozliczenie jednostki w roku. ListValue to cena cennikowa (stawka razy dni), Discounts to różnica do uzgodnionych kwot.</summary>
public record CharterUnitSummary(
    Guid VesselId, string Name, VesselKind Kind, decimal? DayRate, VesselStatus Status,
    int Bookings, int Days, decimal ListValue, decimal Revenue, decimal Discounts, int UtilizationPercent, int ChecklistMissing, int ChecklistTotal);

public record CharterSummary(
    int Year, int Units, int UnitsAvailable, int Clients, int ClientsThisYear,
    decimal RevenueYear, decimal RevenueThisMonth, int BookingsYear, int DaysYear, int Upcoming, int ChecklistMissing,
    IReadOnlyList<CharterMonth> Months, IReadOnlyList<CharterUnitSummary> PerUnit);

public static class CharterRules
{
    /// <summary>
    /// Liczba płatnych dni. Dzień zwrotu jest dniem przekazania następnemu klientowi, więc się nie liczy
    /// (sobota–sobota to 7 dni). Wynajem w obrębie jednego dnia to 1 dzień.
    /// </summary>
    public static int Days(DateOnly start, DateOnly end) => Math.Max(1, end.DayNumber - start.DayNumber);

    /// <summary>
    /// Sprawdza, czy jednostka jest wolna. Nowy wynajem może zacząć się w ostatnim dniu poprzedniego (przekazanie łodzi),
    /// ale nie może się z nim nakładać. Ta sama zasada co przy rejsach z dniem wymiany załogi.
    /// Blokuje wiersz jednostki do końca transakcji, żeby dwie równoczesne rezerwacje nie zajęły tego samego terminu.
    /// </summary>
    public static async Task<IResult?> CheckAvailabilityAsync(AppDbContext db, Vessel vessel, DateOnly start, DateOnly end, Guid? charterId, CancellationToken ct)
    {
        await db.Database.ExecuteSqlAsync($"""SELECT 1 FROM "Vessels" WHERE "Id" = {vessel.Id} FOR UPDATE""", ct);

        var charter = await db.Charters.AsNoTracking()
            .Where(c => c.VesselId == vessel.Id && c.Id != charterId && ((c.StartDate < end && c.EndDate > start) || c.StartDate == start))
            .OrderBy(c => c.StartDate)
            .Select(c => new { c.Participant!.FullName, c.StartDate, c.EndDate })
            .FirstOrDefaultAsync(ct);
        if (charter is not null)
        {
            return Problems.Conflict($"{vessel.Name} jest już wynajęta od {charter.StartDate:dd.MM.yyyy} do {charter.EndDate:dd.MM.yyyy} ({charter.FullName}). Nowy wynajem może zacząć się najwcześniej w dniu zwrotu.");
        }
        var cruise = await db.Cruises.AsNoTracking()
            .Where(c => c.VesselId == vessel.Id && c.Status == CruiseStatus.Planned && ((c.StartDate < end && c.EndDate > start) || c.StartDate == start))
            .Select(c => c.Name).FirstOrDefaultAsync(ct);
        if (cruise is not null)
        {
            return Problems.Conflict($"{vessel.Name} płynie w tym terminie w rejsie „{cruise}”.");
        }
        var block = await db.VesselBlocks.AsNoTracking()
            .Where(b => b.VesselId == vessel.Id && b.StartDate <= end && b.EndDate >= start)
            .Select(b => b.Reason).FirstOrDefaultAsync(ct);
        return block is null ? null : Problems.Conflict($"{vessel.Name} jest w tym terminie niedostępna: {block}.");
    }
}

public static class CharterEndpoints
{
    public static void MapCharterEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/charters").RequireModule(Modules.Charters);

        group.MapGet("/", async (Guid? vesselId, AppDbContext db, CancellationToken ct) =>
        {
            var query = db.Charters.AsNoTracking();
            if (vesselId is { } id)
            {
                query = query.Where(c => c.VesselId == id);
            }
            var rows = await query.OrderBy(c => c.StartDate).Select(c => new
            {
                c.Id, c.VesselId, VesselName = c.Vessel!.Name, c.Vessel.Kind, c.ParticipantId, c.Participant!.FullName, c.Participant.Email, c.Participant.Phone,
                c.StartDate, c.EndDate, c.DayRate, c.Total, c.Notes,
            }).ToListAsync(ct);
            return rows.Select(c => new CharterDto(
                c.Id, c.VesselId, c.VesselName, c.Kind, c.ParticipantId, c.FullName, c.Email, c.Phone,
                c.StartDate, c.EndDate, CharterRules.Days(c.StartDate, c.EndDate), c.DayRate, c.Total, c.Notes)).ToList();
        });

        // Lista klientów do wyboru przy rezerwacji. Działa także bez modułu klientów, ale pokazuje tylko imię i kontakt.
        group.MapGet("/clients", async (AppDbContext db, CancellationToken ct) =>
            await db.Participants.AsNoTracking().Where(p => p.AnonymizedAt == null).OrderBy(p => p.FullName)
                .Select(p => new CharterClient(p.Id, p.FullName, p.Email, p.Phone)).ToListAsync(ct));

        group.MapPost("/", async (CharterRequest request, AppDbContext db, CancellationToken ct) =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            var charter = new Charter();
            var result = await SaveAsync(db, charter, request, isNew: true, ct);
            if (result is not null)
            {
                return result;
            }
            db.Charters.Add(charter);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return Results.Created($"/api/charters/{charter.Id}", new { charter.Id, charter.Total });
        }).Validate<CharterRequest>();

        group.MapPut("/{id:guid}", async (Guid id, CharterRequest request, AppDbContext db, CancellationToken ct) =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            var charter = await db.Charters.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (charter is null)
            {
                return Problems.NotFound("Nie znaleziono rezerwacji.");
            }
            var result = await SaveAsync(db, charter, request, isNew: false, ct);
            if (result is not null)
            {
                return result;
            }
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return Results.NoContent();
        }).Validate<CharterRequest>();

        group.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
        {
            var charter = await db.Charters.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (charter is null)
            {
                return Problems.NotFound("Nie znaleziono rezerwacji.");
            }
            db.Charters.Remove(charter);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        group.MapGet("/summary", async (int? year, AppDbContext db, TimeProvider time, CancellationToken ct) =>
        {
            var today = time.Today();
            var y = year ?? today.Year;
            if (y is < 2000 or > 2100)
            {
                return Problems.Bad("Podaj rok od 2000 do 2100.");
            }
            var yearStart = new DateOnly(y, 1, 1);
            var yearEnd = new DateOnly(y, 12, 31);

            var vessels = await db.Vessels.AsNoTracking().Where(v => v.CharterDayRate != null).OrderBy(v => v.Name).ToListAsync(ct);
            // Przychód i dni przypisujemy do miesiąca rozpoczęcia wynajmu.
            var charters = await db.Charters.AsNoTracking().Where(c => c.StartDate >= yearStart && c.StartDate <= yearEnd)
                .Select(c => new { c.VesselId, c.ParticipantId, c.StartDate, c.EndDate, c.DayRate, c.Total }).ToListAsync(ct);
            var rows = charters.Select(c => new { c.VesselId, c.ParticipantId, c.StartDate, c.DayRate, c.Total, Days = CharterRules.Days(c.StartDate, c.EndDate) }).ToList();
            var daysInYear = yearEnd.DayNumber - yearStart.DayNumber + 1;

            var months = Enumerable.Range(1, 12).Select(m =>
            {
                var inMonth = rows.Where(r => r.StartDate.Month == m).ToList();
                return new CharterMonth(m, inMonth.Count, inMonth.Sum(r => r.Days), inMonth.Sum(r => r.Total));
            }).ToList();

            // Jednostki, które wynajmowano w tym roku, ale później zdjęto im cenę, też trafiają do rozliczenia.
            var formerIds = rows.Select(r => r.VesselId).Distinct().Except(vessels.Select(v => v.Id)).ToList();
            if (formerIds.Count > 0)
            {
                vessels.AddRange(await db.Vessels.AsNoTracking().Where(v => formerIds.Contains(v.Id)).ToListAsync(ct));
            }

            var perUnit = vessels.Select(v =>
            {
                var own = rows.Where(r => r.VesselId == v.Id).ToList();
                var days = own.Sum(r => r.Days);
                var listValue = own.Sum(r => r.DayRate * r.Days);
                var revenue = own.Sum(r => r.Total);
                return new CharterUnitSummary(
                    v.Id, v.Name, v.Kind, v.CharterDayRate, v.Status, own.Count, days, listValue, revenue, listValue - revenue,
                    (int)Math.Round(100.0 * Math.Min(days, daysInYear) / daysInYear), v.Checklist.Count(i => !i.IsOk), v.Checklist.Count);
            }).ToList();

            var clientsAll = await db.Charters.AsNoTracking().Select(c => c.ParticipantId).Distinct().CountAsync(ct);
            var upcoming = await db.Charters.AsNoTracking().CountAsync(c => c.EndDate >= today, ct);
            var rentable = vessels.Where(v => v.CharterDayRate != null).ToList();

            return Results.Ok(new CharterSummary(
                y, rentable.Count, rentable.Count(v => v.Status == VesselStatus.InService), clientsAll, rows.Select(r => r.ParticipantId).Distinct().Count(),
                rows.Sum(r => r.Total), y == today.Year ? months[today.Month - 1].Revenue : 0, rows.Count, rows.Sum(r => r.Days), upcoming,
                rentable.Sum(v => v.Checklist.Count(i => !i.IsOk)), months, perUnit));
        });
    }

    /// <summary>Sprawdza dane i dostępność, a potem przepisuje je do rezerwacji. Zwraca błąd albo null.</summary>
    private static async Task<IResult?> SaveAsync(AppDbContext db, Charter charter, CharterRequest request, bool isNew, CancellationToken ct)
    {
        if (request.EndDate < request.StartDate)
        {
            return Problems.Bad("Data zwrotu nie może być wcześniejsza niż data odbioru.");
        }
        if (request.EndDate.DayNumber - request.StartDate.DayNumber > 366)
        {
            return Problems.Bad("Wynajem może trwać najwyżej rok.");
        }
        if (request.Total is { } total && (total < 0 || total > 9_999_999 || decimal.Round(total, 2) != total))
        {
            return Problems.Bad("Podaj poprawną kwotę wynajmu.");
        }
        var enroll = new EnrollRequest(request.ParticipantId, request.Participant, request.Total);
        if (EnrollmentRules.ValidateRequest(enroll) is { } invalid)
        {
            return invalid;
        }

        var vessel = await db.Vessels.FirstOrDefaultAsync(v => v.Id == request.VesselId, ct);
        if (vessel is null)
        {
            return Problems.Bad("Wybrana jednostka nie istnieje.");
        }
        var vesselChanged = isNew || vessel.Id != charter.VesselId;
        if (vesselChanged && vessel.CharterDayRate is null)
        {
            return Problems.Bad($"{vessel.Name} nie ma ceny czarteru. Ustaw cenę za dzień, aby ją wynajmować.");
        }
        if (vessel.Status == VesselStatus.OutOfService)
        {
            return Problems.Conflict($"{vessel.Name} jest oznaczona jako niedostępna.");
        }
        if (await CharterRules.CheckAvailabilityAsync(db, vessel, request.StartDate, request.EndDate, isNew ? null : charter.Id, ct) is { } clash)
        {
            return clash;
        }

        // Przy zmianie klienta na tej samej rezerwacji nie ma „podwójnego zapisu”, więc duplikat nie jest sprawdzany.
        if (isNew || request.ParticipantId != charter.ParticipantId)
        {
            var (participant, error) = await EnrollmentRules.ResolveParticipantAsync(db, enroll, _ => Task.FromResult(false), ct);
            if (error is not null)
            {
                return error;
            }
            charter.Participant = participant;
            charter.ParticipantId = participant!.Id;
        }

        var dateOrVesselChanged = vesselChanged || charter.StartDate != request.StartDate || charter.EndDate != request.EndDate;
        if (vesselChanged)
        {
            charter.DayRate = vessel.CharterDayRate!.Value;
        }
        charter.VesselId = vessel.Id;
        charter.StartDate = request.StartDate;
        charter.EndDate = request.EndDate;
        charter.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        // Bez podanej kwoty: stawka razy dni. Przy edycji bez kwoty i bez zmiany terminu zostaje uzgodniona wcześniej kwota.
        charter.Total = request.Total ?? (dateOrVesselChanged ? charter.DayRate * CharterRules.Days(request.StartDate, request.EndDate) : charter.Total);
        return null;
    }
}

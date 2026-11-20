using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Nautivo.Api.Data;
using Nautivo.Api.Domain;
using Nautivo.Api.Infrastructure;

namespace Nautivo.Api.Features;

public record VesselRequest(
    [property: Required(ErrorMessage = "Podaj nazwę jednostki."), MaxLength(100)] string Name,
    [property: MaxLength(100)] string? Model,
    [property: Range(0, 200, ErrorMessage = "Podaj długość od 0 do 200 m.")] decimal LengthMeters,
    [property: Range(0, 500, ErrorMessage = "Podaj liczbę koi od 0 do 500.")] int Berths,
    [property: MaxLength(50)] string? RegistrationNumber,
    DateOnly? InspectionDue,
    VesselStatus Status,
    [property: MaxLength(2000)] string? Notes,
    VesselKind Kind = VesselKind.Yacht,
    [property: Range(typeof(decimal), "0", "9999999", ErrorMessage = "Podaj poprawną cenę za dzień.")] decimal? CharterDayRate = null);

public record ChecklistItemDto(
    [property: Required(ErrorMessage = "Podaj nazwę pozycji."), MaxLength(100, ErrorMessage = "Nazwa pozycji może mieć najwyżej 100 znaków.")] string Name,
    bool IsOk,
    [property: MaxLength(200, ErrorMessage = "Uwaga może mieć najwyżej 200 znaków.")] string? Note);

public record ChecklistRequest([property: Required] List<ChecklistItemDto> Items);

public record VesselBlockRequest(
    DateOnly StartDate,
    DateOnly EndDate,
    [property: Required(ErrorMessage = "Podaj powód."), MaxLength(200)] string Reason);

public record VesselBlockDto(Guid Id, DateOnly StartDate, DateOnly EndDate, string Reason);

public record VesselDto(
    Guid Id, string Name, string Model, decimal LengthMeters, int Berths, string? RegistrationNumber,
    DateOnly? InspectionDue, VesselStatus Status, string? Notes, int UpcomingCruises, IReadOnlyList<VesselBlockDto> Blocks,
    VesselKind Kind, decimal? CharterDayRate, int UpcomingCharters, IReadOnlyList<ChecklistItemDto> Checklist);

public static class FleetEndpoints
{
    public static void MapFleetEndpoints(this RouteGroupBuilder api)
    {
        // Jednostki są wspólne dla floty i czarterów: ta sama łódź nie może płynąć w rejsie i być wynajęta jednocześnie.
        var group = api.MapGroup("/vessels").RequireAnyModule(Modules.Fleet, Modules.Charters);

        group.MapGet("/", async (AppDbContext db, TimeProvider time, CancellationToken ct) =>
        {
            var today = time.Today();
            var rows = await db.Vessels.AsNoTracking().OrderBy(v => v.Name).Select(v => new
            {
                Vessel = v,
                UpcomingCruises = db.Cruises.Count(c => c.VesselId == v.Id && c.Status == CruiseStatus.Planned && c.EndDate >= today),
                UpcomingCharters = db.Charters.Count(c => c.VesselId == v.Id && c.EndDate >= today),
                Blocks = v.Blocks.Where(b => b.EndDate >= today).OrderBy(b => b.StartDate).Select(b => new VesselBlockDto(b.Id, b.StartDate, b.EndDate, b.Reason)).ToList(),
            }).ToListAsync(ct);
            return rows.Select(r => new VesselDto(
                r.Vessel.Id, r.Vessel.Name, r.Vessel.Model, r.Vessel.LengthMeters, r.Vessel.Berths, r.Vessel.RegistrationNumber, r.Vessel.InspectionDue,
                r.Vessel.Status, r.Vessel.Notes, r.UpcomingCruises, r.Blocks, r.Vessel.Kind, r.Vessel.CharterDayRate, r.UpcomingCharters,
                r.Vessel.Checklist.Select(i => new ChecklistItemDto(i.Name, i.IsOk, i.Note)).ToList())).ToList();
        });

        group.MapPost("/", async (VesselRequest request, AppDbContext db, CancellationToken ct) =>
        {
            if (ValidateRate(request) is { } invalid)
            {
                return invalid;
            }
            var vessel = new Vessel { Name = request.Name.Trim() };
            Apply(vessel, request);
            db.Vessels.Add(vessel);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/vessels/{vessel.Id}", new { vessel.Id });
        }).Validate<VesselRequest>();

        group.MapPut("/{id:guid}", async (Guid id, VesselRequest request, AppDbContext db, CancellationToken ct) =>
        {
            var vessel = await db.Vessels.FirstOrDefaultAsync(v => v.Id == id, ct);
            if (vessel is null)
            {
                return Problems.NotFound("Nie znaleziono jednostki.");
            }
            var largestCruise = await db.Cruises.Where(c => c.VesselId == id && c.Status == CruiseStatus.Planned)
                .MaxAsync(c => (int?)c.Capacity, ct) ?? 0;
            if (request.Berths < largestCruise)
            {
                return Problems.Conflict($"Jeden z rejsów na tej jednostce ma {largestCruise} miejsc. Zmniejsz najpierw limit rejsu.");
            }
            if (ValidateRate(request) is { } invalid)
            {
                return invalid;
            }
            Apply(vessel, request);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).Validate<VesselRequest>();

        group.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
        {
            var vessel = await db.Vessels.Include(v => v.Blocks).FirstOrDefaultAsync(v => v.Id == id, ct);
            if (vessel is null)
            {
                return Problems.NotFound("Nie znaleziono jednostki.");
            }
            if (await db.Cruises.AnyAsync(c => c.VesselId == id, ct))
            {
                return Problems.Conflict("Ta jednostka ma przypisane rejsy. Zmień jednostkę w rejsach albo oznacz ją jako niedostępną.");
            }
            if (await db.Charters.AnyAsync(c => c.VesselId == id, ct))
            {
                return Problems.Conflict("Ta jednostka ma rezerwacje czarterowe. Usuń je najpierw albo oznacz jednostkę jako niedostępną.");
            }
            db.VesselBlocks.RemoveRange(vessel.Blocks);
            db.Vessels.Remove(vessel);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        group.MapPost("/{id:guid}/blocks", async (Guid id, VesselBlockRequest request, AppDbContext db, CancellationToken ct) =>
        {
            if (request.EndDate < request.StartDate)
            {
                return Problems.Bad("Data końca nie może być wcześniejsza niż początek.");
            }
            if (!await db.Vessels.AnyAsync(v => v.Id == id, ct))
            {
                return Problems.NotFound("Nie znaleziono jednostki.");
            }
            var clash = await db.Cruises.Where(c => c.VesselId == id && c.Status == CruiseStatus.Planned && c.StartDate <= request.EndDate && c.EndDate >= request.StartDate)
                .Select(c => c.Name).FirstOrDefaultAsync(ct);
            if (clash is not null)
            {
                return Problems.Conflict($"W tym okresie jednostka płynie w rejsie „{clash}”.");
            }
            var charter = await db.Charters.Where(c => c.VesselId == id && c.StartDate <= request.EndDate && c.EndDate >= request.StartDate)
                .Select(c => c.Participant!.FullName).FirstOrDefaultAsync(ct);
            if (charter is not null)
            {
                return Problems.Conflict($"W tym okresie jednostka jest wynajęta: {charter}.");
            }
            var block = new VesselBlock { VesselId = id, StartDate = request.StartDate, EndDate = request.EndDate, Reason = request.Reason.Trim() };
            db.VesselBlocks.Add(block);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/vessels/{id}/blocks/{block.Id}", new { block.Id });
        }).Validate<VesselBlockRequest>();

        // Lista wyposażenia zapisywana w całości: formularz wysyła wszystkie pozycje naraz.
        group.MapPut("/{id:guid}/checklist", async (Guid id, ChecklistRequest request, AppDbContext db, CancellationToken ct) =>
        {
            if (request.Items.Count > MaxChecklistItems)
            {
                return Problems.Bad($"Lista może mieć najwyżej {MaxChecklistItems} pozycji.");
            }
            foreach (var item in request.Items)
            {
                var results = new List<ValidationResult>();
                if (!Validator.TryValidateObject(item, new ValidationContext(item), results, true) || string.IsNullOrWhiteSpace(item.Name))
                {
                    return Problems.Bad(results.FirstOrDefault()?.ErrorMessage ?? "Każda pozycja musi mieć nazwę.");
                }
            }
            var vessel = await db.Vessels.FirstOrDefaultAsync(v => v.Id == id, ct);
            if (vessel is null)
            {
                return Problems.NotFound("Nie znaleziono jednostki.");
            }
            vessel.Checklist = request.Items
                .Select(i => new ChecklistItem { Name = i.Name.Trim(), IsOk = i.IsOk, Note = string.IsNullOrWhiteSpace(i.Note) ? null : i.Note.Trim() })
                .ToList();
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).Validate<ChecklistRequest>();

        group.MapDelete("/{id:guid}/blocks/{blockId:guid}", async (Guid id, Guid blockId, AppDbContext db, CancellationToken ct) =>
        {
            var block = await db.VesselBlocks.FirstOrDefaultAsync(b => b.Id == blockId && b.VesselId == id, ct);
            if (block is null)
            {
                return Problems.NotFound("Nie znaleziono blokady.");
            }
            db.VesselBlocks.Remove(block);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
    }

    private static void Apply(Vessel vessel, VesselRequest request)
    {
        vessel.Name = request.Name.Trim();
        vessel.Model = request.Model?.Trim() ?? "";
        vessel.LengthMeters = request.LengthMeters;
        vessel.Berths = request.Berths;
        vessel.RegistrationNumber = request.RegistrationNumber?.Trim();
        vessel.InspectionDue = request.InspectionDue;
        vessel.Status = request.Status;
        vessel.Notes = request.Notes?.Trim();
        vessel.Kind = request.Kind;
        vessel.CharterDayRate = request.CharterDayRate;
    }

    private const int MaxChecklistItems = 100;

    private static IResult? ValidateRate(VesselRequest request) =>
        request.CharterDayRate is { } rate && decimal.Round(rate, 2) != rate
            ? Problems.Bad("Cena za dzień może mieć najwyżej dwa miejsca po przecinku.")
            : null;
}

public static class TimeExtensions
{
    public static DateOnly Today(this TimeProvider time) => DateOnly.FromDateTime(time.GetLocalNow().DateTime);
}

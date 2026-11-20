using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Nautivo.Api.Data;
using Nautivo.Api.Domain;
using Nautivo.Api.Infrastructure;

namespace Nautivo.Api.Features;

public record DashboardCruise(Guid Id, string Name, DateOnly StartDate, DateOnly EndDate, string? VesselName, int Confirmed, int Capacity, decimal Remaining, string Location, bool IsConfirmed);
public record DashboardYourCruise(Guid Id, string Name, DateOnly StartDate, string Location, int Reservations);
public record DashboardPeriod(int Reservations, int ReservationsPrevious, decimal Income, decimal IncomePrevious, int NewClients, int NewClientsPrevious, int CruisesStarting);
public record TrendPoint(DateOnly Date, DateOnly End, int Reservations, decimal Income);
public record TrendResponse(int Days, string Bucket, IReadOnlyList<TrendPoint> Points);
public record OrganizationSettings(Guid Id, string Name, string? ContactEmail, string? Phone, string? City, string? TaxId, string? Website, bool CanEdit);
public record OrganizationSettingsRequest(
    [property: Required(ErrorMessage = "Podaj nazwę firmy."), MaxLength(200)] string Name,
    [property: EmailAddress(ErrorMessage = "Podaj poprawny e-mail."), MaxLength(254)] string? ContactEmail,
    [property: MaxLength(40)] string? Phone,
    [property: MaxLength(100)] string? City,
    [property: MaxLength(20)] string? TaxId,
    [property: MaxLength(200)] string? Website);
public record DashboardOverdue(Guid EnrollmentId, Guid? CruiseId, Guid? CourseId, string FullName, string CruiseName, DateOnly DueDate, decimal Remaining);
public record DashboardVessel(Guid Id, string Name, DateOnly? InspectionDue, VesselStatus Status, string? Notes);

public record DashboardResponse(
    int CruisesTotal, int CruisesUpcoming, int CruisesFinished, int CruisesCancelled,
    decimal EarnedTotal, decimal EarnedThisMonth, decimal EarnedThisYear, decimal ToCollect, decimal OverdueAmount, int OverdueCount,
    int ParticipantsTotal, int SeatsTaken, int SeatsTotal, int WaitlistCount,
    int VesselsTotal, int VesselsAvailable,
    IReadOnlyList<DashboardCruise> NextCruises, IReadOnlyList<DashboardOverdue> Overdue, IReadOnlyList<DashboardVessel> VesselAlerts,
    DashboardPeriod Last30Days, IReadOnlyList<DashboardYourCruise> YourCruises);

public record CalendarItem(string Type, Guid Id, string Title, DateOnly StartDate, DateOnly EndDate, int? Confirmed, int? Capacity);
public record CalendarVessel(Guid Id, string Name, string Model, VesselStatus Status, IReadOnlyList<CalendarItem> Items);
/// <summary>Type: cruise, block, charter (Title to klient) albo course. Szkolenia nie mają jednostki, więc są w Courses.</summary>
public record CalendarResponse(DateOnly From, DateOnly To, IReadOnlyList<CalendarVessel> Vessels, IReadOnlyList<CalendarItem> WithoutVessel, IReadOnlyList<CalendarItem> Courses);

public record OrganizationRequest([property: Required(ErrorMessage = "Podaj nazwę organizacji."), MaxLength(200)] string Name);

public static class OtherEndpoints
{
    public static void MapDashboardEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet("/dashboard", async (AppDbContext db, TimeProvider time, CancellationToken ct) =>
        {
            var today = time.Today();
            var monthStart = new DateOnly(today.Year, today.Month, 1);
            var yearStart = new DateOnly(today.Year, 1, 1);

            var cruises = await db.Cruises.AsNoTracking().Select(c => new
            {
                c.Id, c.Name, c.StartDate, c.EndDate, c.Status, c.Capacity, c.BalanceDueDate, Location = c.DeparturePort, c.IsConfirmed,
                VesselName = c.YachtName ?? (c.Vessel != null ? c.Vessel.Name : null),
                Confirmed = c.Enrollments.Count(e => e.Status == EnrollmentStatus.Confirmed),
                Waitlist = c.Enrollments.Count(e => e.Status == EnrollmentStatus.Waitlist),
            }).ToListAsync(ct);

            var planned = cruises.Where(c => c.Status == CruiseStatus.Planned).ToList();
            var upcoming = planned.Where(c => c.EndDate >= today).OrderBy(c => c.StartDate).ToList();

            var open = await db.Enrollments.AsNoTracking()
                // Należności z rejsów i ze szkoleń.
                .Where(e => e.Status == EnrollmentStatus.Confirmed && (e.Cruise != null ? e.Cruise.Status : e.Course!.Status) == CruiseStatus.Planned)
                .Select(e => new
                {
                    e.Id, e.CruiseId, e.CourseId, e.Participant!.FullName,
                    CruiseName = e.Cruise != null ? e.Cruise.Name : e.Course!.Name,
                    BalanceDueDate = e.Cruise != null ? e.Cruise.BalanceDueDate : e.Course!.BalanceDueDate,
                    Remaining = e.Total - (e.Payments.Sum(p => (decimal?)p.Amount) ?? 0),
                })
                .Where(e => e.Remaining > 0)
                .ToListAsync(ct);
            var overdue = open.Where(e => e.BalanceDueDate < today).OrderBy(e => e.BalanceDueDate).ToList();
            var remainingByCruise = open.Where(e => e.CruiseId != null).GroupBy(e => e.CruiseId!.Value).ToDictionary(g => g.Key, g => g.Sum(x => x.Remaining));

            var payments = db.Payments.AsNoTracking();
            var vessels = await db.Vessels.AsNoTracking().OrderBy(v => v.Name).Select(v => new { v.Id, v.Name, v.InspectionDue, v.Status, v.Notes }).ToListAsync(ct);
            var inspectionLimit = today.AddDays(30);

            // Okres 30 dni włącznie z dzisiaj oraz poprzednie 30 dni do porównania.
            var periodStart = today.AddDays(-29);
            var previousStart = today.AddDays(-59);
            var periodStartUtc = new DateTimeOffset(periodStart.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var previousStartUtc = new DateTimeOffset(previousStart.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var activeEnrollments = db.Enrollments.AsNoTracking().Where(e => e.Status != EnrollmentStatus.Cancelled);
            var last30 = new DashboardPeriod(
                await activeEnrollments.CountAsync(e => e.CreatedAt >= periodStartUtc, ct),
                await activeEnrollments.CountAsync(e => e.CreatedAt >= previousStartUtc && e.CreatedAt < periodStartUtc, ct),
                await payments.Where(p => p.PaidOn >= periodStart).SumAsync(p => (decimal?)p.Amount, ct) ?? 0,
                await payments.Where(p => p.PaidOn >= previousStart && p.PaidOn < periodStart).SumAsync(p => (decimal?)p.Amount, ct) ?? 0,
                await db.Participants.CountAsync(p => p.CreatedAt >= periodStartUtc, ct),
                await db.Participants.CountAsync(p => p.CreatedAt >= previousStartUtc && p.CreatedAt < periodStartUtc, ct),
                upcoming.Count(c => c.StartDate >= today && c.StartDate <= inspectionLimit));

            return new DashboardResponse(
                cruises.Count, upcoming.Count, planned.Count - upcoming.Count, cruises.Count - planned.Count,
                await payments.SumAsync(p => (decimal?)p.Amount, ct) ?? 0,
                await payments.Where(p => p.PaidOn >= monthStart).SumAsync(p => (decimal?)p.Amount, ct) ?? 0,
                await payments.Where(p => p.PaidOn >= yearStart).SumAsync(p => (decimal?)p.Amount, ct) ?? 0,
                open.Sum(e => e.Remaining), overdue.Sum(e => e.Remaining), overdue.Count,
                await db.Participants.CountAsync(ct),
                upcoming.Sum(c => c.Confirmed), upcoming.Sum(c => c.Capacity), upcoming.Sum(c => c.Waitlist),
                vessels.Count, vessels.Count(v => v.Status == VesselStatus.InService),
                upcoming.Take(5).Select(c => new DashboardCruise(c.Id, c.Name, c.StartDate, c.EndDate, c.VesselName, c.Confirmed, c.Capacity, remainingByCruise.GetValueOrDefault(c.Id), c.Location, c.IsConfirmed)).ToList(),
                overdue.Take(6).Select(e => new DashboardOverdue(e.Id, e.CruiseId, e.CourseId, e.FullName, e.CruiseName, e.BalanceDueDate, e.Remaining)).ToList(),
                vessels.Where(v => v.Status == VesselStatus.OutOfService || v.InspectionDue <= inspectionLimit)
                    .Select(v => new DashboardVessel(v.Id, v.Name, v.InspectionDue, v.Status, v.Notes)).ToList(),
                last30,
                upcoming.OrderByDescending(c => c.Confirmed).ThenBy(c => c.StartDate).Take(4)
                    .Select(c => new DashboardYourCruise(c.Id, c.Name, c.StartDate, c.Location, c.Confirmed)).ToList());
        });

        api.MapGet("/dashboard/trend", async (int? days, AppDbContext db, TimeProvider time, CancellationToken ct) =>
        {
            var range = days ?? 30;
            if (range is not (7 or 30 or 90 or 365))
            {
                return Problems.Bad("Dostępne zakresy to 7, 30, 90 i 365 dni.");
            }

            var today = time.Today();
            var first = today.AddDays(1 - range);
            var firstUtc = new DateTimeOffset(first.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

            var reservationDays = await db.Enrollments.AsNoTracking()
                .Where(e => e.Status != EnrollmentStatus.Cancelled && e.CreatedAt >= firstUtc)
                .Select(e => e.CreatedAt).ToListAsync(ct);
            var incomeRows = await db.Payments.AsNoTracking()
                .Where(p => p.PaidOn >= first)
                .Select(p => new { p.PaidOn, p.Amount }).ToListAsync(ct);

            // Pojedyncze dni dają przy małym ruchu same piki, dlatego dłuższe zakresy są grupowane.
            var bucketDays = range switch { 365 => 30, 90 => 7, 30 => 3, _ => 1 };
            var points = new List<TrendPoint>();
            // Kubełki liczone od dzisiaj wstecz, więc ostatni zawsze kończy się dziś i jest pełny.
            for (var end = today; end >= first; end = end.AddDays(-bucketDays))
            {
                var start = end.AddDays(1 - bucketDays);
                if (start < first) start = first;
                var from = start;
                var to = end;
                points.Add(new TrendPoint(
                    start, end,
                    reservationDays.Count(d => DateOnly.FromDateTime(d.UtcDateTime) >= from && DateOnly.FromDateTime(d.UtcDateTime) <= to),
                    incomeRows.Where(p => p.PaidOn >= from && p.PaidOn <= to).Sum(p => p.Amount)));
            }
            points.Reverse();
            return Results.Ok(new TrendResponse(range, bucketDays switch { 1 => "day", 3 => "3days", 7 => "week", _ => "month" }, points));
        });
    }

    public static void MapOrganizationEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet("/organization", async (AppDbContext db, TenantContext tenant, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var id = tenant.RequiredOrganizationId;
            var organization = await db.Organizations.AsNoTracking().FirstAsync(o => o.Id == id, ct);
            return new OrganizationSettings(id, organization.Name, organization.ContactEmail, organization.Phone, organization.City, organization.TaxId, organization.Website, await CanEdit(db, tenant, user, ct));
        });

        api.MapPut("/organization", async (OrganizationSettingsRequest request, AppDbContext db, TenantContext tenant, ClaimsPrincipal user, CancellationToken ct) =>
        {
            if (!await CanEdit(db, tenant, user, ct))
            {
                return Problems.Forbidden("Ustawienia firmy może zmieniać tylko właściciel.");
            }
            var id = tenant.RequiredOrganizationId;
            var organization = await db.Organizations.FirstAsync(o => o.Id == id, ct);
            organization.Name = request.Name.Trim();
            organization.ContactEmail = Clean(request.ContactEmail);
            organization.Phone = Clean(request.Phone);
            organization.City = Clean(request.City);
            organization.TaxId = Clean(request.TaxId);
            organization.Website = Clean(request.Website);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).Validate<OrganizationSettingsRequest>();
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static async Task<bool> CanEdit(AppDbContext db, TenantContext tenant, ClaimsPrincipal user, CancellationToken ct)
    {
        if (tenant.IsAdmin)
        {
            return true;
        }
        var userId = user.UserId();
        var id = tenant.RequiredOrganizationId;
        return await db.Memberships.AnyAsync(m => m.UserId == userId && m.OrganizationId == id && m.Role == OrganizationRole.Owner, ct);
    }

    public static void MapCalendarEndpoints(this RouteGroupBuilder api)
    {
        api.MapGroup("/calendar").RequireModule(Modules.Calendar).MapGet("/", async (DateOnly from, DateOnly to, AppDbContext db, TenantContext tenant, CancellationToken ct) =>
        {
            if (to < from || to.DayNumber - from.DayNumber > 370)
            {
                return Problems.Bad("Podaj zakres dat nie dłuższy niż rok.");
            }

            var cruises = await db.Cruises.AsNoTracking()
                .Where(c => c.Status == CruiseStatus.Planned && c.StartDate <= to && c.EndDate >= from)
                .Select(c => new { c.VesselId, Item = new CalendarItem("cruise", c.Id, c.Name, c.StartDate, c.EndDate, c.Enrollments.Count(e => e.Status == EnrollmentStatus.Confirmed), c.Capacity) })
                .ToListAsync(ct);
            var blocks = await db.VesselBlocks.AsNoTracking()
                .Where(b => b.StartDate <= to && b.EndDate >= from)
                .Select(b => new { b.VesselId, Item = new CalendarItem("block", b.Id, b.Reason, b.StartDate, b.EndDate, null, null) })
                .ToListAsync(ct);
            // Czartery i szkolenia pokazujemy tylko organizacjom z włączonym modułem.
            var charters = tenant.EnabledModules.Contains(Modules.Charters)
                ? await db.Charters.AsNoTracking()
                    .Where(c => c.StartDate <= to && c.EndDate >= from)
                    .Select(c => new { c.VesselId, Item = new CalendarItem("charter", c.Id, c.Participant!.FullName, c.StartDate, c.EndDate, null, null) })
                    .ToListAsync(ct)
                : [];
            var courses = tenant.EnabledModules.Contains(Modules.Courses)
                ? await db.Courses.AsNoTracking()
                    .Where(c => c.Status == CruiseStatus.Planned && c.StartDate <= to && c.EndDate >= from)
                    .OrderBy(c => c.StartDate)
                    .Select(c => new CalendarItem("course", c.Id, c.Name, c.StartDate, c.EndDate, c.Enrollments.Count(e => e.Status == EnrollmentStatus.Confirmed), c.Capacity))
                    .ToListAsync(ct)
                : [];
            var vessels = await db.Vessels.AsNoTracking().OrderBy(v => v.Name).ToListAsync(ct);

            return Results.Ok(new CalendarResponse(
                from, to,
                vessels.Select(v => new CalendarVessel(v.Id, v.Name, v.Model, v.Status,
                    cruises.Where(c => c.VesselId == v.Id).Select(c => c.Item)
                        .Concat(blocks.Where(b => b.VesselId == v.Id).Select(b => b.Item))
                        .Concat(charters.Where(c => c.VesselId == v.Id).Select(c => c.Item))
                        .OrderBy(i => i.StartDate).ToList())).ToList(),
                cruises.Where(c => c.VesselId is null).Select(c => c.Item).OrderBy(i => i.StartDate).ToList(),
                courses));
        });
    }

    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var admin = app.MapGroup("/api/admin").RequireAuthorization()
            .AddEndpointFilter(async (context, next) =>
            {
                var http = context.HttpContext;
                var db = http.RequestServices.GetRequiredService<AppDbContext>();
                var userId = http.User.UserId();
                var allowed = http.User.HasAdminSession() && await db.Users.AnyAsync(u => u.Id == userId && u.IsPlatformAdmin, http.RequestAborted);
                return allowed ? await next(context) : Problems.Forbidden("Ta funkcja jest dostępna tylko dla administratora.");
            });

        admin.MapPost("/organizations", async (OrganizationRequest request, AppDbContext db, CancellationToken ct) =>
        {
            var organization = new Organization { Name = request.Name.Trim() };
            db.Organizations.Add(organization);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new OrganizationInfo(organization.Id, organization.Name, Modules.All));
        }).Validate<OrganizationRequest>();
    }
}

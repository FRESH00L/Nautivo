using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Nautivo.Api.Data;
using Nautivo.Api.Domain;
using Npgsql;

namespace Nautivo.Api.Infrastructure;

/// <summary>Organizacja bieżącego żądania. Ustawia ją wyłącznie TenantFilter.</summary>
public sealed class TenantContext
{
    public Guid? OrganizationId { get; set; }
    public bool IsAdmin { get; set; }
    public string[] EnabledModules { get; set; } = [];
    public Guid RequiredOrganizationId => OrganizationId ?? throw new InvalidOperationException("Brak organizacji.");
}

public static class AuthClaims
{
    public const string AdminSession = "nautivo_admin";

    public static Guid UserId(this ClaimsPrincipal user) => Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    public static bool HasAdminSession(this ClaimsPrincipal user) => user.HasClaim(AdminSession, "1");
}

public sealed class TenantFilter(AppDbContext db, TenantContext tenant) : IEndpointFilter
{
    public const string Header = "X-Organization-Id";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var http = context.HttpContext;
        var userId = http.User.UserId();
        var ct = http.RequestAborted;
        Guid? requested = Guid.TryParse(http.Request.Headers[Header], out var parsed) ? parsed : null;

        var isAdmin = http.User.HasAdminSession()
            && await db.Users.AnyAsync(u => u.Id == userId && u.IsPlatformAdmin, ct);

        var query = isAdmin
            ? db.Organizations.AsQueryable()
            : db.Memberships.Where(m => m.UserId == userId).Select(m => m.Organization!);

        var organization = await (requested is { } id ? query.Where(o => o.Id == id) : query.OrderBy(o => o.CreatedAt))
            .Select(o => new { o.Id, o.EnabledModules })
            .FirstOrDefaultAsync(ct);

        if (organization is null)
        {
            return Problems.Forbidden("Nie masz dostępu do tej organizacji.");
        }

        tenant.OrganizationId = organization.Id;
        tenant.IsAdmin = isAdmin;
        tenant.EnabledModules = isAdmin ? Modules.All : organization.EnabledModules;
        return await next(context);
    }
}

public static class EndpointExtensions
{
    /// <summary>Administrator widzi wszystkie moduły, pozostali tylko włączone w organizacji.</summary>
    public static RouteGroupBuilder RequireModule(this RouteGroupBuilder group, string module) =>
        group.AddEndpointFilter(async (context, next) =>
        {
            var tenant = context.HttpContext.RequestServices.GetRequiredService<TenantContext>();
            return tenant.EnabledModules.Contains(module)
                ? await next(context)
                : Problems.Forbidden("Ten moduł nie jest włączony w organizacji.");
        });

    /// <summary>Wystarczy jeden z modułów, np. wpłaty działają dla rejsów i dla szkoleń.</summary>
    public static RouteGroupBuilder RequireAnyModule(this RouteGroupBuilder group, params string[] modules) =>
        group.AddEndpointFilter(async (context, next) =>
        {
            var tenant = context.HttpContext.RequestServices.GetRequiredService<TenantContext>();
            return modules.Any(tenant.EnabledModules.Contains)
                ? await next(context)
                : Problems.Forbidden("Ten moduł nie jest włączony w organizacji.");
        });

    public static RouteHandlerBuilder Validate<T>(this RouteHandlerBuilder builder) where T : class =>
        builder.AddEndpointFilter(async (context, next) =>
        {
            var model = context.Arguments.OfType<T>().FirstOrDefault();
            if (model is null)
            {
                return Problems.Bad("Brak danych w żądaniu.");
            }
            var results = new List<ValidationResult>();
            if (Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true))
            {
                return await next(context);
            }
            return Results.ValidationProblem(
                results.SelectMany(r => r.MemberNames.DefaultIfEmpty("").Select(m => (m, r.ErrorMessage ?? "Nieprawidłowa wartość.")))
                    .GroupBy(x => x.Item1)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Item2).ToArray()),
                title: "Popraw dane w formularzu.");
        });
}

public static class Problems
{
    public static IResult Bad(string detail) => Results.Problem(detail: detail, statusCode: 400, title: "Nieprawidłowe dane");
    public static IResult Conflict(string detail) => Results.Problem(detail: detail, statusCode: 409, title: "Konflikt");
    public static IResult Forbidden(string detail) => Results.Problem(detail: detail, statusCode: 403, title: "Brak dostępu");
    public static IResult NotFound(string detail = "Nie znaleziono.") => Results.Problem(detail: detail, statusCode: 404, title: "Nie znaleziono");
}

/// <summary>Naruszenie unikalności w bazie to konflikt danych, np. dwa równoczesne zapisy na tę samą koję.</summary>
public sealed class ProblemExceptionHandler(ILogger<ProblemExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception exception, CancellationToken ct)
    {
        if (exception is DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } })
        {
            await Problems.Conflict("Ta wartość jest już zajęta. Odśwież dane i spróbuj ponownie.").ExecuteAsync(http);
            return true;
        }
        logger.LogError(exception, "Nieobsłużony błąd żądania {Path}", http.Request.Path);
        await Results.Problem(detail: "Wystąpił błąd serwera. Spróbuj ponownie.", statusCode: 500, title: "Błąd serwera").ExecuteAsync(http);
        return true;
    }
}

public static class Security
{
    /// <summary>
    /// Formularz z innej strony nie ustawi tego nagłówka, a fetch między domenami zablokuje CORS.
    /// To druga warstwa ochrony CSRF obok ciasteczka SameSite=Strict.
    /// </summary>
    public const string CsrfHeader = "X-Requested-With";
    public const string CsrfValue = "nautivo";

    public static IApplicationBuilder UseApiSecurity(this IApplicationBuilder app) =>
        app.Use(async (http, next) =>
        {
            http.Response.Headers.XContentTypeOptions = "nosniff";
            http.Response.Headers.XFrameOptions = "DENY";
            http.Response.Headers["Referrer-Policy"] = "no-referrer";

            var unsafeMethod = !(HttpMethods.IsGet(http.Request.Method) || HttpMethods.IsHead(http.Request.Method) || HttpMethods.IsOptions(http.Request.Method));
            if (unsafeMethod && http.Request.Path.StartsWithSegments("/api") && http.Request.Headers[CsrfHeader] != CsrfValue)
            {
                await Problems.Forbidden("Brak wymaganego nagłówka żądania.").ExecuteAsync(http);
                return;
            }
            await next();
        });
}

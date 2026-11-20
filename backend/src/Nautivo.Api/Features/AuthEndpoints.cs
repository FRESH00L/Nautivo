using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nautivo.Api.Data;
using Nautivo.Api.Domain;
using Nautivo.Api.Infrastructure;

namespace Nautivo.Api.Features;

/// <summary>
/// Jedno logowanie dla wszystkich. Pole Email przyjmuje też login administratora (np. „admin”).
/// SecretKey dołącza przeglądarka, jeśli ma go zapisanego; bez poprawnego klucza konto administratora się nie zaloguje.
/// </summary>
public record LoginRequest(
    [property: Required(ErrorMessage = "Podaj e-mail."), MaxLength(254)] string Email,
    [property: Required(ErrorMessage = "Podaj hasło."), MaxLength(128)] string Password,
    [property: MaxLength(256)] string? SecretKey = null);

public record RegisterRequest(
    [property: Required(ErrorMessage = "Podaj imię i nazwisko."), MaxLength(200)] string FullName,
    [property: Required(ErrorMessage = "Podaj e-mail."), EmailAddress(ErrorMessage = "Podaj poprawny e-mail."), MaxLength(254)] string Email,
    [property: Required, MinLength(12, ErrorMessage = "Hasło musi mieć co najmniej 12 znaków."), MaxLength(128)] string Password,
    [property: Required(ErrorMessage = "Podaj nazwę organizacji."), MaxLength(200)] string OrganizationName);

public record OrganizationInfo(Guid Id, string Name, string[] Modules);
public record MeResponse(Guid Id, string Email, string FullName, bool IsAdmin, IReadOnlyList<OrganizationInfo> Organizations);

public static class AuthEndpoints
{
    public const string RateLimitPolicy = "auth";
    private const string InvalidCredentials = "Nieprawidłowe dane logowania.";
    private const int MaxFailedLogins = 5;
    private static readonly TimeSpan LockoutTime = TimeSpan.FromMinutes(15);

    // Wyrównuje czas odpowiedzi, gdy konto nie istnieje, żeby nie zdradzać, które e-maile są w bazie.
    private static readonly string DummyHash = new PasswordHasher<User>().HashPassword(new User { Email = "", FullName = "" }, Guid.NewGuid().ToString());

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth");
        var limited = auth.MapGroup("").RequireRateLimiting(RateLimitPolicy);

        limited.MapPost("/register", Register).Validate<RegisterRequest>();
        limited.MapPost("/login", Login).Validate<LoginRequest>();

        auth.MapPost("/logout", async (HttpContext http) =>
        {
            await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.NoContent();
        }).RequireAuthorization();

        auth.MapGet("/me", Me).RequireAuthorization();
    }

    private static async Task<IResult> Register(RegisterRequest request, AppDbContext db, IPasswordHasher<User> hasher, IConfiguration config, HttpContext http, CancellationToken ct)
    {
        if (!config.GetValue("Auth:AllowRegistration", false))
        {
            return Problems.Forbidden("Rejestracja jest wyłączona.");
        }

        var email = Normalize(request.Email);
        if (await db.Users.AnyAsync(u => u.Email == email, ct))
        {
            return Problems.Conflict("Konto z tym e-mailem już istnieje.");
        }

        var organization = new Organization { Name = request.OrganizationName.Trim() };
        var user = new User { Email = email, FullName = request.FullName.Trim() };
        user.PasswordHash = hasher.HashPassword(user, request.Password);
        user.Memberships.Add(new Membership { Organization = organization, Role = OrganizationRole.Owner });
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        await SignIn(http, user, adminSession: false);
        return Results.Ok(await BuildMe(db, user.Id, adminSession: false, ct));
    }

    private static async Task<IResult> Login(LoginRequest request, AppDbContext db, IPasswordHasher<User> hasher, IConfiguration config, HttpContext http, CancellationToken ct)
    {
        var user = await VerifyPassword(db, hasher, request.Email, request.Password, ct);
        if (user is null)
        {
            return Unauthorized();
        }

        // Konto administratora loguje się wyłącznie z poprawnym secret key. Bez klucza ta sama odpowiedź co przy złym haśle.
        var adminSession = user.IsPlatformAdmin;
        if (adminSession && !SecretKeyMatches(config, request.SecretKey))
        {
            return Unauthorized();
        }

        await SignIn(http, user, adminSession);
        return Results.Ok(await BuildMe(db, user.Id, adminSession, ct));
    }

    /// <summary>
    /// Zakłada lub aktualizuje konto administratora z konfiguracji (Auth:AdminLogin, Auth:AdminPassword).
    /// Poza Development wymaga hasła co najmniej 12-znakowego, żeby „admin/admin” nie trafiło na serwer.
    /// </summary>
    public static async Task EnsureAdminAccountAsync(IServiceProvider services, IHostEnvironment env)
    {
        var config = services.GetRequiredService<IConfiguration>();
        var login = config["Auth:AdminLogin"];
        var password = config["Auth:AdminPassword"];
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrEmpty(password))
        {
            return;
        }
        if (!env.IsDevelopment() && password.Length < 12)
        {
            throw new InvalidOperationException("Auth:AdminPassword musi mieć co najmniej 12 znaków poza środowiskiem Development.");
        }

        var db = services.GetRequiredService<AppDbContext>();
        var hasher = services.GetRequiredService<IPasswordHasher<User>>();
        var normalized = Normalize(login);
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == normalized);
        if (user is null)
        {
            user = new User { Email = normalized, FullName = "Administrator" };
            db.Users.Add(user);
        }
        user.IsPlatformAdmin = true;
        if (user.PasswordHash == "" || hasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Failed)
        {
            user.PasswordHash = hasher.HashPassword(user, password);
        }
        await db.SaveChangesAsync();
    }

    private static async Task<IResult> Me(ClaimsPrincipal principal, AppDbContext db, CancellationToken ct)
    {
        var me = await BuildMe(db, principal.UserId(), principal.HasAdminSession(), ct);
        return me is null ? Results.Unauthorized() : Results.Ok(me);
    }

    private static async Task<MeResponse?> BuildMe(AppDbContext db, Guid userId, bool adminSession, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
        {
            return null;
        }

        var isAdmin = adminSession && user.IsPlatformAdmin;
        var organizations = isAdmin
            ? await db.Organizations.OrderBy(o => o.Name).Select(o => new OrganizationInfo(o.Id, o.Name, Modules.All)).ToListAsync(ct)
            : await db.Memberships.Where(m => m.UserId == userId).OrderBy(m => m.Organization!.Name)
                .Select(m => new OrganizationInfo(m.OrganizationId, m.Organization!.Name, m.Organization.EnabledModules)).ToListAsync(ct);

        return new MeResponse(user.Id, user.Email, user.FullName, isAdmin, organizations);
    }

    /// <summary>Zwraca użytkownika po poprawnym haśle. Liczy nieudane próby i blokuje konto na 15 minut.</summary>
    private static async Task<User?> VerifyPassword(AppDbContext db, IPasswordHasher<User> hasher, string emailInput, string password, CancellationToken ct)
    {
        var email = Normalize(emailInput);
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (user is null)
        {
            hasher.VerifyHashedPassword(new User { Email = "", FullName = "" }, DummyHash, password);
            return null;
        }

        var locked = user.LockedUntil > DateTimeOffset.UtcNow;
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (locked)
        {
            return null;
        }

        if (result == PasswordVerificationResult.Failed)
        {
            user.FailedLogins++;
            if (user.FailedLogins >= MaxFailedLogins)
            {
                user.LockedUntil = DateTimeOffset.UtcNow.Add(LockoutTime);
                user.FailedLogins = 0;
            }
            await db.SaveChangesAsync(ct);
            return null;
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = hasher.HashPassword(user, password);
        }
        user.FailedLogins = 0;
        user.LockedUntil = null;
        await db.SaveChangesAsync(ct);
        return user;
    }

    /// <summary>Porównanie w stałym czasie. Brak klucza lub klucz krótszy niż 32 znaki wyłącza panel administratora.</summary>
    private static bool SecretKeyMatches(IConfiguration config, string? provided)
    {
        var expected = config["Auth:AdminSecretKey"];
        if (string.IsNullOrEmpty(expected) || expected.Length < 32 || string.IsNullOrEmpty(provided))
        {
            return false;
        }
        return CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(expected)),
            SHA256.HashData(Encoding.UTF8.GetBytes(provided)));
    }

    private static Task SignIn(HttpContext http, User user, bool adminSession)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, user.Id.ToString()) };
        if (adminSession)
        {
            claims.Add(new Claim(AuthClaims.AdminSession, "1"));
        }
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }

    private static IResult Unauthorized() => Results.Problem(detail: InvalidCredentials, statusCode: 401, title: "Logowanie nieudane");

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();
}

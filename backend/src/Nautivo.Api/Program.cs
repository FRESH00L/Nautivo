using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nautivo.Api.Data;
using Nautivo.Api.Domain;
using Nautivo.Api.Features;
using Nautivo.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// W developmencie sekrety pochodzą z niecommitowanego pliku .env w katalogu głównym repozytorium.
if (builder.Environment.IsDevelopment())
{
    var envFile = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "..", ".env"));
    if (File.Exists(envFile))
    {
        var values = File.ReadAllLines(envFile)
            .Where(line => line.Contains('=') && !line.TrimStart().StartsWith('#'))
            .Select(line => line.Split('=', 2))
            .ToDictionary(parts => parts[0].Trim().Replace("__", ":"), parts => (string?)parts[1].Trim());
        builder.Configuration.AddInMemoryCollection(values);
    }
}

builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ProblemExceptionHandler>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<TenantContext>();
builder.Services.AddScoped<TenantFilter>();

builder.Services.AddDbContext<AppDbContext>((sp, options) =>
    options.UseNpgsql(sp.GetRequiredService<IConfiguration>().GetConnectionString("Default")
        ?? throw new InvalidOperationException("Brak ConnectionStrings:Default.")));

// PBKDF2-HMAC-SHA512 z solą. 210 000 iteracji to zalecenie OWASP dla tego algorytmu.
builder.Services.Configure<PasswordHasherOptions>(o => o.IterationCount = 210_000);
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.Cookie.Name = "nautivo.auth";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Strict;
        o.Cookie.SecurePolicy = builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing")
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
        o.SlidingExpiration = true;
        o.Events.OnRedirectToLogin = context => { context.Response.StatusCode = 401; return Task.CompletedTask; };
        o.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = 403; return Task.CompletedTask; };
    });
builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = 429;
    o.AddPolicy(AuthEndpoints.RateLimitPolicy, http => RateLimitPartition.GetFixedWindowLimiter(
        http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = http.RequestServices.GetRequiredService<IConfiguration>().GetValue("Auth:RateLimitPerMinute", 10),
            Window = TimeSpan.FromMinutes(1),
        }));
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseApiSecurity();
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
app.MapAuthEndpoints();
app.MapAdminEndpoints();

var api = app.MapGroup("/api").RequireAuthorization().AddEndpointFilter<TenantFilter>();
api.MapDashboardEndpoints();
api.MapCruiseEndpoints();
api.MapCourseEndpoints();
api.MapFleetEndpoints();
api.MapCharterEndpoints();
api.MapParticipantEndpoints();
api.MapCalendarEndpoints();
api.MapOrganizationEndpoints();

using (var scope = app.Services.CreateScope())
{
    if (app.Configuration.GetValue("Database:MigrateOnStartup", false))
    {
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    }
    await AuthEndpoints.EnsureAdminAccountAsync(scope.ServiceProvider, app.Environment);
    if (app.Configuration.GetValue("Seed:Demo", false))
    {
        await DemoSeeder.SeedAsync(scope.ServiceProvider);
    }
}

app.Run();

public partial class Program;

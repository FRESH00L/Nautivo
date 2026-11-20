using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Nautivo.Api.Infrastructure;
using Testcontainers.PostgreSql;

namespace Nautivo.Tests;

/// <summary>Prawdziwy PostgreSQL w kontenerze. Mock bazy nie sprawdziłby filtrów ani blokad.</summary>
public sealed class ApiFixture : IAsyncLifetime
{
    public const string AdminKey = "test-admin-secret-key-0123456789-abcdef";
    public const string AdminLogin = "admin";

    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder("postgres:17-alpine").Build();
    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _db.StartAsync();
        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(host =>
        {
            host.UseEnvironment("Testing");
            host.UseSetting("ConnectionStrings:Default", _db.GetConnectionString());
            host.UseSetting("Auth:AdminSecretKey", AdminKey);
            host.UseSetting("Auth:AdminLogin", AdminLogin);
            host.UseSetting("Auth:AdminPassword", TestData.Password);
            host.UseSetting("Auth:AllowRegistration", "true");
            host.UseSetting("Auth:RateLimitPerMinute", "10000");
            host.UseSetting("Database:MigrateOnStartup", "true");
        });
        _ = Factory.Server;
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _db.DisposeAsync();
    }

    /// <summary>Klient zalogowany na konto administratora z konfiguracji, z poprawnym secret key.</summary>
    public async Task<HttpClient> AdminClient(Guid? organizationId = null)
    {
        var client = Client(organizationId);
        (await client.PostAsJsonAsync("/api/auth/login", new { email = AdminLogin, password = TestData.Password, secretKey = AdminKey })).EnsureSuccessStatusCode();
        return client;
    }

    public HttpClient Client(Guid? organizationId = null)
    {
        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        client.DefaultRequestHeaders.Add(Security.CsrfHeader, Security.CsrfValue);
        if (organizationId is { } id)
        {
            client.DefaultRequestHeaders.Add(TenantFilter.Header, id.ToString());
        }
        return client;
    }

    /// <summary>Nowa organizacja z właścicielem. Zwraca zalogowanego klienta i identyfikator organizacji.</summary>
    public async Task<(HttpClient Client, Guid OrganizationId, string Email)> NewOrganization(string name = "Testowa")
    {
        var email = $"{Guid.NewGuid():N}@example.com";
        var client = Client();
        var response = await client.PostAsJsonAsync("/api/auth/register", new { fullName = "Test Owner", email, password = TestData.Password, organizationName = name });
        response.EnsureSuccessStatusCode();
        var me = await response.Content.ReadFromJsonAsync<MeDto>();
        return (client, me!.Organizations[0].Id, email);
    }
}

public record OrgDto(Guid Id, string Name, string[] Modules);
public record MeDto(Guid Id, string Email, bool IsAdmin, OrgDto[] Organizations);
public record IdDto(Guid Id, string? Status);

public static class TestData
{
    public const string Password = "bardzo-dlugie-haslo-123";

    public static object Vessel(string name = "Zefir", int berths = 8) =>
        new { name, model = "Delphia 40", lengthMeters = 12m, berths, status = "InService" };

    public static object Cruise(Guid? vesselId = null, string start = "2030-06-01", string end = "2030-06-08", int capacity = 2, decimal price = 1000m) =>
        new { name = "Rejs testowy", routePoints = Route("Gdynia", "Hel"), startDate = start, endDate = end, vesselId, price, deposit = 300m, balanceDueDate = start, capacity, status = "Planned" };

    public static object[] Route(params string[] ports) => ports.Select(p => (object)new { name = p, lat = (double?)null, lng = (double?)null }).ToArray();

    public static object NewPerson(string name) => new { participant = new { fullName = name, email = "osoba@example.com" } };

    public static async Task<Guid> CreateId(this HttpClient client, string url, object body)
    {
        var response = await client.PostAsJsonAsync(url, body);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IdDto>())!.Id;
    }
}

[CollectionDefinition("api")]
public class ApiCollection : ICollectionFixture<ApiFixture>;

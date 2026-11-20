using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nautivo.Api.Data;

namespace Nautivo.Tests;

[Collection("api")]
public class AuthTests(ApiFixture api)
{
    private static string NewEmail() => $"{Guid.NewGuid():N}@example.com";

    private Task<HttpResponseMessage> AdminLogin(HttpClient client, string? secretKey) =>
        client.PostAsJsonAsync("/api/auth/login", new { email = ApiFixture.AdminLogin, password = TestData.Password, secretKey });

    [Fact]
    public async Task Password_is_stored_only_as_a_salted_hash()
    {
        var first = await api.NewOrganization();
        var second = await api.NewOrganization();

        using var scope = api.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hashes = await db.Users.Where(u => u.Email == first.Email || u.Email == second.Email).Select(u => u.PasswordHash).ToListAsync();

        Assert.Equal(2, hashes.Count);
        Assert.DoesNotContain(hashes, h => h.Contains(TestData.Password));
        Assert.NotEqual(hashes[0], hashes[1]);
    }

    [Fact]
    public async Task Login_works_with_correct_password_and_rejects_wrong_one()
    {
        var (_, _, email) = await api.NewOrganization();

        var bad = await api.Client().PostAsJsonAsync("/api/auth/login", new { email, password = "zle-haslo-zle-haslo" });
        Assert.Equal(HttpStatusCode.Unauthorized, bad.StatusCode);

        var good = await api.Client().PostAsJsonAsync("/api/auth/login", new { email, password = TestData.Password });
        Assert.Equal(HttpStatusCode.OK, good.StatusCode);
    }

    [Fact]
    public async Task Auth_cookie_is_http_only_and_same_site_strict()
    {
        var response = await api.Client().PostAsJsonAsync("/api/auth/register", new { fullName = "A", email = NewEmail(), password = TestData.Password, organizationName = "X" });
        var cookie = response.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith("nautivo.auth"));
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Account_locks_after_five_failures_even_for_correct_password()
    {
        var (_, _, email) = await api.NewOrganization();
        for (var i = 0; i < 5; i++)
        {
            await api.Client().PostAsJsonAsync("/api/auth/login", new { email, password = "zle-haslo-zle-haslo" });
        }
        var response = await api.Client().PostAsJsonAsync("/api/auth/login", new { email, password = TestData.Password });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Admin_registration_endpoints_do_not_exist()
    {
        var body = new { fullName = "X", email = NewEmail(), password = TestData.Password, secretKey = ApiFixture.AdminKey };
        Assert.Equal(HttpStatusCode.NotFound, (await api.Client().PostAsJsonAsync("/api/auth/admin/register", body)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.Client().PostAsJsonAsync("/api/auth/admin/login", body)).StatusCode);
    }

    [Fact]
    public async Task Admin_login_requires_the_secret_key_even_with_correct_password()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await AdminLogin(api.Client(), secretKey: null)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await AdminLogin(api.Client(), secretKey: "zly-klucz")).StatusCode);

        var withKey = await AdminLogin(api.Client(), ApiFixture.AdminKey);
        var me = await withKey.Content.ReadFromJsonAsync<MeDto>();
        Assert.True(me!.IsAdmin);
    }

    [Fact]
    public async Task Rejected_admin_login_leaves_no_session()
    {
        var client = api.Client();
        await AdminLogin(client, secretKey: null);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/admin/organizations", new { name = "X" })).StatusCode);
    }

    [Fact]
    public async Task Normal_user_with_the_key_gets_no_admin_rights()
    {
        var (_, _, email) = await api.NewOrganization();
        var other = await api.NewOrganization("Cudza");
        var client = api.Client(other.OrganizationId);

        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password = TestData.Password, secretKey = ApiFixture.AdminKey });
        var me = await login.Content.ReadFromJsonAsync<MeDto>();

        Assert.False(me!.IsAdmin);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/dashboard")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/admin/organizations", new { name = "X" })).StatusCode);
    }

    [Fact]
    public async Task Admin_sees_all_modules_and_any_organization()
    {
        var other = await api.NewOrganization("Cudza");
        var client = api.Client(other.OrganizationId);
        var login = await AdminLogin(client, ApiFixture.AdminKey);
        var me = await login.Content.ReadFromJsonAsync<MeDto>();

        Assert.Contains(me!.Organizations, o => o.Id == other.OrganizationId);
        Assert.All(me.Organizations, o => Assert.Equal(["rejsy", "flota", "kalendarz", "uczestnicy", "szkolenia", "czartery"], o.Modules));
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/dashboard")).StatusCode);
    }

    [Fact]
    public async Task Requests_without_login_are_rejected()
    {
        var client = api.Client();
        foreach (var url in new[] { "/api/dashboard", "/api/cruises", "/api/vessels", "/api/participants", "/api/calendar?from=2030-01-01&to=2030-02-01", "/api/auth/me" })
        {
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(url)).StatusCode);
        }
    }

    [Fact]
    public async Task Writes_without_the_csrf_header_are_rejected()
    {
        var (client, _, _) = await api.NewOrganization();
        client.DefaultRequestHeaders.Remove("X-Requested-With");
        var response = await client.PostAsJsonAsync("/api/vessels", TestData.Vessel());
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Logout_ends_the_session()
    {
        var (client, _, _) = await api.NewOrganization();
        (await client.PostAsync("/api/auth/logout", null)).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
    }
}

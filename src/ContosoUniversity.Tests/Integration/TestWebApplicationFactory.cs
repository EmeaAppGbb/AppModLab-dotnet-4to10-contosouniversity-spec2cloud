using System.Security.Claims;
using System.Text.Encodings.Web;
using ContosoUniversity.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContosoUniversity.Tests.Integration
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName = "TestDb_" + Guid.NewGuid().ToString();
        private bool _seeded;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var dbName = _dbName;

            builder.ConfigureServices(services =>
            {
                // Remove ALL EF Core service registrations
                var efDescriptors = services.Where(
                    d => d.ServiceType.FullName != null && (
                        d.ServiceType.FullName.Contains("EntityFrameworkCore") ||
                        d.ServiceType.FullName.Contains("DbContextOptions") ||
                        d.ServiceType == typeof(SchoolContext)))
                    .ToList();
                foreach (var d in efDescriptors)
                    services.Remove(d);

                // Re-add SchoolContext with InMemory provider using fixed name
                services.AddDbContext<SchoolContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });

                // Re-register Identity stores since they depend on SchoolContext
                services.AddIdentityCore<Microsoft.AspNetCore.Identity.IdentityUser>()
                    .AddRoles<Microsoft.AspNetCore.Identity.IdentityRole>()
                    .AddEntityFrameworkStores<SchoolContext>();
            });

            builder.ConfigureTestServices(services =>
            {
                // Override authentication to always authenticate as test user
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder("Test")
                        .RequireAuthenticatedUser()
                        .Build();
                });
            });

            builder.UseEnvironment("Development");
        }

        public void EnsureSeeded()
        {
            if (_seeded) return;
            _seeded = true;

            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SchoolContext>();
            db.Database.EnsureCreated();
            DbInitializer.Initialize(db);
        }

        public HttpClient CreateSeededClient()
        {
            var client = CreateClient();
            EnsureSeeded();
            return client;
        }
    }

    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "testuser@contoso.edu"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}

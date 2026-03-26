using System;
using System.Net;
using System.Threading.Tasks;
using ContosoUniversity.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ContosoUniversity.Tests.Integration
{
    public class HomeControllerTests : IClassFixture<HomeControllerTests.CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public HomeControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/Home/Contact")]
        public async Task PublicPages_ReturnSuccess(string url)
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var response = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("/Students")]
        [InlineData("/Courses")]
        [InlineData("/Instructors")]
        [InlineData("/Departments")]
        [InlineData("/Home/About")]
        public async Task ProtectedPages_RedirectToLogin(string url)
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var response = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("Login", response.Headers.Location?.ToString() ?? "");
        }

        public class CustomWebApplicationFactory : WebApplicationFactory<Program>
        {
            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                builder.ConfigureServices(services =>
                {
                    // Remove ALL EF Core service registrations (SqlServer provider, DbContextOptions, DbContext itself)
                    var efDescriptors = services.Where(
                        d => d.ServiceType.FullName != null && (
                            d.ServiceType.FullName.Contains("EntityFrameworkCore") ||
                            d.ServiceType.FullName.Contains("DbContextOptions") ||
                            d.ServiceType == typeof(SchoolContext)))
                        .ToList();
                    foreach (var d in efDescriptors)
                        services.Remove(d);

                    // Re-add SchoolContext with InMemory provider
                    services.AddDbContext<SchoolContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString());
                    });

                    // Re-register Identity stores since they depend on SchoolContext
                    services.AddIdentityCore<Microsoft.AspNetCore.Identity.IdentityUser>()
                        .AddRoles<Microsoft.AspNetCore.Identity.IdentityRole>()
                        .AddEntityFrameworkStores<SchoolContext>();
                });

                builder.UseEnvironment("Development");
            }
        }
    }
}

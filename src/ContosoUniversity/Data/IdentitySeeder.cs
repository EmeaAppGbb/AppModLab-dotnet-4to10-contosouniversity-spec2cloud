using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ContosoUniversity.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            string[] roles = { "Admin", "Faculty", "ReadOnly" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminPassword = configuration["Identity:AdminPassword"]
                ?? Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
                ?? null;

            if (adminPassword == null)
            {
                var logger = serviceProvider.GetService<ILogger<IdentityUser>>();
                logger?.LogWarning("Admin password not configured. Skipping admin user seeding. Set Identity:AdminPassword in appsettings or ADMIN_PASSWORD environment variable.");
                return;
            }

            var adminEmail = "admin@contoso.edu";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(adminUser, adminPassword);
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}

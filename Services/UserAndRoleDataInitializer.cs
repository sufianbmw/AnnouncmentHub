using AnnouncmentHub.Models;
using Microsoft.AspNetCore.Identity;

namespace AnnouncmentHub.Services
{
    public class UserAndRoleDataInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UserAndRoleDataInitializer(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedDataAsync()
        {
            await SeedRolesAsync();
            await SeedUsersAsync();
        }

        private async Task SeedRolesAsync()
        {
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                var adminRole = new ApplicationRole
                {
                    Name = "Admin",
                    DisplayName = "مدير النظام"
                };
                await _roleManager.CreateAsync(adminRole);
            }

            if (!await _roleManager.RoleExistsAsync("NormalUser"))
            {
                var userRole = new ApplicationRole
                {
                    Name = "NormalUser",
                    DisplayName = "مستخدم عادي"
                };
                await _roleManager.CreateAsync(userRole);
            }
        }

        private async Task SeedUsersAsync()
        {
            // Check if admin user already exists
            var existingUser = await _userManager.FindByEmailAsync("sufianbmw@gmail.com");
            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = "wcpjordan",
                    Email = "sufianbmw@gmail.com",
                    FName = "Sufian",
                    LName = "Shwayat",
                    EmailConfirmed = true,
                    UserStatus = true
                };

                var result = await _userManager.CreateAsync(user, "Wcpadmin2023!");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                    Console.WriteLine("✅ Default Admin user created and assigned to Admin role.");
                }
                else
                {
                    Console.WriteLine("⚠️ Failed to create default Admin user:");
                    foreach (var error in result.Errors)
                        Console.WriteLine($" - {error.Description}");
                }
            }
            else
            {
                Console.WriteLine("ℹ️ Default Admin user already exists — skipping creation.");
            }
        }
    }
}
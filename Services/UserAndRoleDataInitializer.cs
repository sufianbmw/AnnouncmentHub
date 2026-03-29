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

        public async Task<List<string>> SeedDataAsync()
        {
            var messages = new List<string>();
            await SeedRolesAsync(messages);
            await SeedUsersAsync(messages);
            return messages;
        }

        private async Task SeedRolesAsync(List<string> messages)
        {
            var roles = new[]
            {
        new ApplicationRole { Name = "Admin", DisplayName = "مدير النظام" },
        new ApplicationRole { Name = "NormalUser", DisplayName = "مستخدم عادي" }
    };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role.Name))
                {
                    var result = await _roleManager.CreateAsync(role);
                    if (result.Succeeded)
                        messages.Add($"✅ تم إنشاء الدور '{role.DisplayName}' بنجاح.");
                    else
                        foreach (var error in result.Errors)
                            messages.Add($"❌ فشل إنشاء الدور '{role.DisplayName}': {error.Description}");
                }
                else
                {
                    messages.Add($"ℹ️ الدور '{role.DisplayName}' موجود مسبقاً — تم التخطي.");
                }
            }
        }

        private async Task SeedUsersAsync(List<string> messages)
        {
            var existingUser = await _userManager.FindByEmailAsync("sufianbmw@gmail.com");
            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = "prog.man.jo",
                    Email = "sufianbmw@gmail.com",
                    FName = "Sufian",
                    LName = "Shwayat",
                    EmailConfirmed = true,
                    UserStatus = true
                };

                var result = await _userManager.CreateAsync(user, "Admin@2026!");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                    messages.Add("✅ تم إنشاء المستخدم الافتراضي وتعيينه في دور المدير بنجاح.");
                }
                else
                {
                    foreach (var error in result.Errors)
                        messages.Add($"❌ فشل إنشاء المستخدم الافتراضي: {error.Description}");
                }
            }
            else
            {
                existingUser.UserName = "prog.man.jo";
                existingUser.NormalizedUserName = "PROG.MAN.JO";
                existingUser.Email = "sufianbmw@gmail.com";
                existingUser.NormalizedEmail = "SUFIANBMW@GMAIL.COM";
                existingUser.FName = "Sufian";
                existingUser.LName = "Shwayat";
                existingUser.EmailConfirmed = true;
                existingUser.UserStatus = true;

                var updateResult = await _userManager.UpdateAsync(existingUser);
                if (updateResult.Succeeded)
                    messages.Add("🔄 تم تحديث بيانات المستخدم الافتراضي بنجاح.");
                else
                    messages.Add("❌ فشل تحديث بيانات المستخدم الافتراضي.");

                await _userManager.RemovePasswordAsync(existingUser);
                var passResult = await _userManager.AddPasswordAsync(existingUser, "Admin@2026!");
                if (passResult.Succeeded)
                    messages.Add("🔑 تمت إعادة تعيين كلمة المرور إلى الافتراضية بنجاح.");
                else
                    messages.Add("❌ فشل إعادة تعيين كلمة المرور.");

                if (!await _userManager.IsInRoleAsync(existingUser, "Admin"))
                {
                    await _userManager.AddToRoleAsync(existingUser, "Admin");
                    messages.Add("🛡️ تمت إعادة تعيين دور المدير للمستخدم.");
                }
                else
                {
                    messages.Add("ℹ️ المستخدم لديه دور المدير مسبقاً.");
                }
            }
        }
    }
}
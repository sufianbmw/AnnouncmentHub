#nullable disable

using System.ComponentModel.DataAnnotations;
using AnnouncmentHub.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AnnouncmentHub.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "البريد الإلكتروني حقل أجباري.")]
            //[EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة.")]
            [Display(Name = "البريد الإلكتروني")]
            public string Email { get; set; }

            [Required(ErrorMessage = "كلمة المرور حقل أجباري.")]
            [DataType(DataType.Password)]
            [Display(Name = "كلمة المرور")]
            public string Password { get; set; }

            [Display(Name = "تذكرني")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
                ModelState.AddModelError(string.Empty, ErrorMessage);

            // If already logged in, redirect to admin dashboard
            if (User.Identity?.IsAuthenticated == true)
            {
                Response.Redirect("/Admin/Dashboard");
                return;
            }

            returnUrl ??= Url.Content("~/Admin/Dashboard");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/Admin/Dashboard");

            if (!ModelState.IsValid)
                return Page();

            // ✅ Step 1: Find user by email (not username)
            
            var user = await _userManager.FindByEmailAsync(Input.Email);

            // إذا ما لقى، ابحث بالـ username
            if (user == null)
                user = await _userManager.FindByNameAsync(Input.Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة المرور غير صحيحة.");
                return Page();
            }

            // ✅ Step 2: Check UserStatus — block inactive accounts
            if (!user.UserStatus)
            {
                ModelState.AddModelError(string.Empty, "حسابك غير مفعّل. تواصل مع المدير.");
                return Page();
            }

            // ✅ Step 3: Sign in using UserName (Identity requires username internally)
            var result = await _signInManager.PasswordSignInAsync(
                user.UserName,
                Input.Password,
                Input.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} logged in.", Input.Email);
                return LocalRedirect(returnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User {Email} account locked out.", Input.Email);
                ModelState.AddModelError(string.Empty, "الحساب مقفل مؤقتاً. حاول لاحقاً.");
                return Page();
            }

            ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة المرور غير صحيحة.");
            return Page();
        }
    }
}
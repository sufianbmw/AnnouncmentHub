using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AnnouncmentHub.Data;
using AnnouncmentHub.Models;
using AnnouncmentHub.ViewModels;

namespace AnnouncmentHub.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<ApplicationRole> _rolemanager;
        private readonly UserManager<ApplicationUser> _usermanager;
        public UsersController(UserManager<ApplicationUser> usermanager, RoleManager<ApplicationRole> rolemanager, ApplicationDbContext context)
        {
            _usermanager = usermanager;
            _rolemanager = rolemanager;
            _context = context;
        }
        //public async Task<IActionResult> Index()
        //{
        //    var users = await _usermanager.Users.Select(
        //        user => new UserViewModel
        //        {
        //            Id = user.Id,
        //            FName = user.FName,
        //            LName = user.LName,
        //            UserName = user.UserName,
        //            Email = user.Email,
        //            UserStatus = user.UserStatus
        //           // Roles = _usermanager.GetRolesAsync(user).Result
        //        }).ToListAsync();

        //    return View(users);

        //}
        public async Task<IActionResult> Index()
        {
            // Step 1: Load all users from DB
            var usersList = await _usermanager.Users.ToListAsync();

            // Step 2: Build ViewModel list with roles per user
            var viewModel = new List<UserViewModel>();

            foreach (var user in usersList)
            {
                var roles = await _usermanager.GetRolesAsync(user);

                viewModel.Add(new UserViewModel
                {
                    Id = user.Id,
                    FName = user.FName,
                    LName = user.LName,
                    UserName = user.UserName,
                    Email = user.Email,
                    UserStatus = user.UserStatus,
                    Roles = roles.ToList()
                });
            }

            return View(viewModel);
        }

        public async Task<IActionResult> AddUser()
        {

            var Role = await _rolemanager.Roles.Select(
            r => new RoleViewModel
            {
                RoleId = r.Id,
                RoleName = r.Name,
                DisplayName = r.DisplayName
            }
            ).ToListAsync();

            var ViewModel = new AddUserViewModel
            {
                Roles = Role,
            };

       
            return View(ViewModel);
       }
    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(AddUserViewModel model)
        {

            if (!ModelState.IsValid)
            {
                var allErrors = ModelState
                    .Where(ms => ms.Value!.Errors.Count > 0)
                    .SelectMany(kvp => kvp.Value!.Errors
                        .Select(e => $"{kvp.Key}: {e.ErrorMessage}"))
                    .ToList();

                System.Diagnostics.Debug.WriteLine(string.Join("\n", allErrors));

                // 🔹 Optionally, show a friendly message on the page
                TempData["Error"] = "⚠️ بعض الحقول تحتوي على أخطاء. يرجى مراجعة البيانات.";

                // 🔹 Refill any data needed for dropdowns or lists
                model.Roles = await _rolemanager.Roles
                    .Select(r => new RoleViewModel
                    {
                        RoleId = r.Id,
                        RoleName = r.Name!,
                        DisplayName = r.DisplayName
                    })
                    .ToListAsync();

                // 🔹 Return the same form view
                return View(model);
            }



            if (! model.Roles.Any(r=>r.IsSelected))
            {
                ModelState.AddModelError("Roles", "يرجى أختيار الصلاحيات للمستخدم! ...");
                return View(model);
            }
            if (await _usermanager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError("Email", "البريد الالكتروني موجود مسبقا! ...");
                return View(model);
            }

            if (await _usermanager.FindByNameAsync(model.UserName) != null)
            {
                ModelState.AddModelError("UserName", "اسم المستخدم موجود مسبقا !...");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FName = model.FName,
                LName = model.LName,
                EmailConfirmed = true,
                UserStatus = model.UserStatus

            };
            var result = await _usermanager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("Roles", error.Description);
                }
                return View(model);
            }
           
            await _usermanager.AddToRolesAsync(user, model.Roles.Where(r => r.IsSelected).Select(r => r.RoleName));
            return RedirectToAction(nameof(Index));
       }

        // ✅ GET: Edit
        public async Task<IActionResult> Edit(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("لم يتم تحديد المستخدم.");

            var user = await _usermanager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            // Get all roles from DB
            var allRoles = await _rolemanager.Roles
                .Select(r => new EditRoleViewModel
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    DisplayName = r.DisplayName
                })
                .ToListAsync();

            // Get current user roles
            var userRoles = await _usermanager.GetRolesAsync(user);

            // Mark selected roles
            foreach (var role in allRoles)
                role.IsSelected = userRoles.Contains(role.RoleName);

            var viewModel = new ProfileFormViewModel
            {
                Id = user.Id,
                FName = user.FName,
                LName = user.LName,
                UserName = user.UserName,
                Email = user.Email,
                UserStatus = user.UserStatus,
                Roles = allRoles
            };

            return View(viewModel);
        }

        // ✅ POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var allErrors = ModelState
           .Where(ms => ms.Value?.Errors.Count > 0)
           .SelectMany(kvp => kvp.Value!.Errors
               .Select(e => $"{kvp.Key}: {e.ErrorMessage}"))
           .ToList();

                System.Diagnostics.Debug.WriteLine("❌ ModelState Errors:");
                System.Diagnostics.Debug.WriteLine(string.Join("\n", allErrors));
                // Step 1: get roles from DB (materialize them first)
                var allRoles = await _rolemanager.Roles.ToListAsync();

                // Step 2: build model roles in memory
                model.Roles = allRoles.Select(r => new EditRoleViewModel
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    DisplayName = r.DisplayName,
                    IsSelected = model.Roles != null &&
                                 model.Roles.Any(x => x.RoleName == r.Name && x.IsSelected)
                }).ToList();

                return View(model);
            }


            var user = await _usermanager.FindByIdAsync(model.Id);
            if (user == null)
                return NotFound();

            // Check duplicates
            var userWithSameEmail = await _usermanager.FindByEmailAsync(model.Email);
            if (userWithSameEmail != null && userWithSameEmail.Id != model.Id)
            {
                ModelState.AddModelError(nameof(model.Email), "هذا البريد الإلكتروني مستخدم بالفعل.");
                return View(model);
            }

            var userWithSameUserName = await _usermanager.FindByNameAsync(model.UserName);
            if (userWithSameUserName != null && userWithSameUserName.Id != model.Id)
            {
                ModelState.AddModelError(nameof(model.UserName), "اسم المستخدم موجود مسبقًا.");
                return View(model);
            }
            if (model.Roles == null || !model.Roles.Any(r => r.IsSelected))
            {
                ModelState.AddModelError("Roles", "يجب اختيار صلاحية واحدة على الأقل للمستخدم!");
            }

            // Update basic info
            user.FName = model.FName;
            user.LName = model.LName;
            user.Email = model.Email;
            user.UserName = model.UserName;
            user.UserStatus = model.UserStatus;
            user.EmailConfirmed = true;

            var result = await _usermanager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            // ✅ Update user roles
            var userRoles = await _usermanager.GetRolesAsync(user);
            var selectedRoles = model.Roles.Where(r => r.IsSelected).Select(r => r.RoleName).ToList();

            var rolesToAdd = selectedRoles.Except(userRoles).ToList();
            var rolesToRemove = userRoles.Except(selectedRoles).ToList();

            if (rolesToAdd.Any())
                await _usermanager.AddToRolesAsync(user, rolesToAdd);

            if (rolesToRemove.Any())
                await _usermanager.RemoveFromRolesAsync(user, rolesToRemove);

            TempData["SuccessMessage"] = "✅ تم تعديل بيانات المستخدم والأدوار بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        //public async Task<IActionResult> Edit(string UserId)
        //{
        //    var user = await _usermanager.FindByIdAsync(UserId);
        //    if (user == null)
        //        return NotFound();
        //    var ViewModel = new ProfileFormViewModel
        //    {
        //        Id = user.Id,
        //        FName = user.FName,
        //        LName = user.LName,
        //        UserName = user.UserName,
        //        Email = user.Email,
        //        UserStatus = (bool)user.UserStatus
        //    };

        //    return View(ViewModel);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(ProfileFormViewModel model)
        //{
        //    //var branchslst = _context.Branches.ToList();
        //    //branchslst.Insert(0, new Branches
        //    //{
        //    //    Id = Guid.Empty,
        //    //    BranchName = "-- أختر --"
        //    //});
        //    //ViewBag.BrancheId = new SelectList(branchslst, "Id", "BranchName",model.BrancheId);
        //    if (!ModelState.IsValid)
        //        return View(model);


        //    var user = await _usermanager.FindByIdAsync(model.Id);
        //    if (user == null)
        //        return NotFound();

        //    var UserWithSameEmail = await _usermanager.FindByEmailAsync(model.Email);
        //    if (UserWithSameEmail != null && UserWithSameEmail.Id != model.Id)
        //    {
        //        ModelState.AddModelError("Email", "هذا البريد الالكتروني موجود مسبقا!...");
        //        return View(model);
        //    } 

        //    var UserWithSameUserName = await _usermanager.FindByNameAsync(model.UserName);
        //    if (UserWithSameUserName != null && UserWithSameUserName.Id != model.Id)
        //    {
        //        ModelState.AddModelError("UserName", "اسم المستخدم موجود مسبقا !...");
        //        return View(model);
        //    }
        //    user.FName = model.FName;
        //    user.LName = model.LName;
        //    user.Email = model.Email;
        //    user.UserName = model.UserName;
        //    user.UserStatus = model.UserStatus;
        //    user.EmailConfirmed = true;

        //    await _usermanager.UpdateAsync(user);

        //    return RedirectToAction(nameof(Index));
        //}


        public async Task<IActionResult> Delete(string UserId)
        {
            var user = await _usermanager.FindByIdAsync(UserId);
            //var branchslst = _context.Branches.ToList();
            //branchslst.Insert(0, new Branches
            //{
            //    Id = Guid.Empty,
            //    BranchName = "-- أختر --"
            //});
           // ViewBag.BrancheId = new SelectList(branchslst, "Id", "BranchName", user.BranchesId);

            if (user == null)
                return NotFound();
            var ViewModel = new ProfileFormViewModel
            {
                Id = user.Id,
                FName = user.FName,
                LName = user.LName,
                UserName = user.UserName,
                Email = user.Email,
                UserStatus = user.UserStatus
                //,BrancheId = (user.BranchesId != null ? (Guid)user.BranchesId : Guid.Empty)

            };
            return View(ViewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(ProfileFormViewModel model)
        {
            var user = await _usermanager.FindByIdAsync(model.Id);
            if (user == null)
                return NotFound();

            var UserName = User.Identity.Name;
            var CurrentUser = await _usermanager.FindByNameAsync(UserName);
            //var CurrentUser1 = _usermanager.GetUserId(User);
            if (CurrentUser.Id == user.Id)
            {
                TempData["ErorrMessage"] = " لا يمكن إزالة المستخدم الحالي الذي قام بتسجيل الدخول. عمليات غير صالحة ... ";
                var ViewModel = new ProfileFormViewModel
                {
                    Id = user.Id,
                    FName = user.FName,
                    LName = user.LName,
                    UserName = user.UserName,
                    Email = user.Email,
                    UserStatus = user.UserStatus
                   //,BrancheId = (user.BranchesId != null ? (Guid)user.BranchesId : Guid.Empty)
                };
                //var branchslst = _context.Branches.ToList();
                //branchslst.Insert(0, new Branches
                //{
                //    Id = Guid.Empty,
                //    BranchName = "-- أختر --"
                //});
                //ViewBag.BrancheId = new SelectList(branchslst, "Id", "BranchName", model.BrancheId);
                return View(ViewModel);
            }
            var result = await _usermanager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new Exception();
            }
            return RedirectToAction(nameof(Index));

        }


        public async Task<IActionResult> ManageRoles(string UserId)
        {
            var user = await _usermanager.FindByIdAsync(UserId);
            if (user == null)
                return NotFound();
            var Role = await _rolemanager.Roles.ToListAsync();
            var ViewModel = new UserRolesViewModel 
            {
                UserId = user.Id,
                UserName = user.UserName,
                Roles = Role.Select(role => new RoleViewModel {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    DisplayName=role.DisplayName,
                    IsSelected = _usermanager.IsInRoleAsync(user, role.Name).Result
                }).ToList()

             };
        return View(ViewModel);
       }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRoles(UserRolesViewModel model)
        {
            var user = await _usermanager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound();

            var UserRoles = await _usermanager.GetRolesAsync(user);

            foreach (var role in model.Roles)
            {
                if (UserRoles.Any(r=>r==role.RoleName)&& !role.IsSelected)
                {
                    await _usermanager.RemoveFromRoleAsync(user, role.RoleName);
                }
                if (!UserRoles.Any(r => r == role.RoleName) && role.IsSelected)
                {
                    await _usermanager.AddToRoleAsync(user, role.RoleName);
                }
               
            }
            return RedirectToAction(nameof(Index));
        }


    }
}

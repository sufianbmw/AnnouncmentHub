using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnnouncmentHub.Data;
using AnnouncmentHub.Models;
using AnnouncmentHub.ViewModels;

namespace WestrenPolutary.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly RoleManager<ApplicationRole> _rolemanager;
        private readonly ApplicationDbContext _context;
        public RolesController(RoleManager<ApplicationRole> rolemanager, ApplicationDbContext context)
        {
            _rolemanager = rolemanager;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var _roles = await _rolemanager.Roles.ToListAsync();

            return View(_roles);
        }
        public IActionResult GetRole()
        {
            // var _roles = await _rolemanager.Roles.ToListAsync();

            var obj = from B in _context.Roles
                      select new RoleFormViewModel
                      {
                          DisplayName = B.DisplayName,
                          Name = B.Name
                      };
            return PartialView("_GetRoleForm", obj.ToList());
       
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(RoleFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Index", await _rolemanager.Roles.ToListAsync());
            var RoleExist = await _rolemanager.RoleExistsAsync(model.Name);
            if (RoleExist)
            {
                ModelState.AddModelError("Name", "الدور موجود مسبقا!...");
                return View("Index", await _rolemanager.Roles.ToListAsync());
            }

            await _rolemanager.CreateAsync(new ApplicationRole(model.Name.Trim(), model.DisplayName.Trim()));
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(string RoleId)
        {
            var role = await _rolemanager.FindByIdAsync(RoleId);
            if (role == null)
                return NotFound();
            var ViewModel = new RoleViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name,
                DisplayName = role.DisplayName
            };
            return View(ViewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoleViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var role = await _rolemanager.FindByIdAsync(model.RoleId);
            if (role == null)
                return NotFound();

            role.Name = model.RoleName;
            role.DisplayName = model.DisplayName;

            await _rolemanager.UpdateAsync(role);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(string RoleId)
        {
            var role = await _rolemanager.FindByIdAsync(RoleId);
            if (role == null)
                return NotFound();                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          
            var ViewModel = new RoleViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name,                                                                                                                                                                                                              
                DisplayName = role.DisplayName
            };
            return View(ViewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(RoleViewModel model)
        {
            var role = await _rolemanager.FindByIdAsync(model.RoleId);
            if (role == null)
                return NotFound();

            var result = await _rolemanager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                throw new Exception();
            }
            return RedirectToAction(nameof(Index));

        }
    }
}

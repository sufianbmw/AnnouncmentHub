using AnnouncmentHub.Data;
using AnnouncmentHub.Helpers;
using AnnouncmentHub.Models;
using AnnouncmentHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AnnouncmentHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,NormalUser")]
    public class PagesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
        [HttpGet]
        public async Task<IActionResult> Seeding()
        {
           // await UserAndRoleDataInitializer.SeedDataAsync(_userManager, _roleManager);
            await DataSeeder.SeedCategoriesAsync(_context);

            return new JsonResult(new { message = "✅ Seeding completed successfully" });
        }
        [HttpGet]
        public async Task<IActionResult> Index(int? CategoriesId)
        {
            var pages = _context.Pages
                .Include(p => p.pagecategorie)
                .Select(p => new PageViewModel
                {
                    Id = p.Id,
                    PageTitle = p.PageTitle,
                    Ordring = p.Ordring,
                    Active = p.Active,
                    PageCategoriesId = p.PageCategorieId,
                    pagecategories = p.pagecategorie
                });

            if (CategoriesId is > 0)
                pages = pages.Where(p => p.PageCategoriesId == CategoriesId);

            ViewData["CategoriesId"] = new SelectList(GetCategoriesFltr(), "Id", "CatName");
            return View(await pages.ToListAsync());
        }

        // ✅ Category Filter List
        private List<PageCategorie> GetCategoriesFltr()
        {
            var cate = _context.PageCategories.ToList();
            cate.Insert(0, new PageCategorie
            {
                Id = -1,
                CatName = "-- اختر التصنيف --"
            });
            return cate;
        }


        // ✅ Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var page = await _context.Pages
                .Include(p => p.pagecategorie)
                .Select(p => new PageViewModel
                {
                    Id = p.Id,
                    PageTitle = p.PageTitle,
                    PageDetails = p.PageDetails,
                    Ordring = p.Ordring,
                    Active = p.Active,
                    PageCategoriesId = p.PageCategorieId,
                    pagecategories = p.pagecategorie
                })
                .FirstOrDefaultAsync(p => p.Id == id);

            return page == null ? NotFound() : View(page);
        }

        // ✅ Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["CategoriesId"] = new SelectList(_context.PageCategories, "Id", "CatName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["CategoriesId"] = new SelectList(_context.PageCategories, "Id", "CatName", model.PageCategoriesId);
                return View(model);
            }

            var page = new Page
            {
                PageTitle = model.PageTitle,
                PageDetails = model.PageDetails,
                Ordring = model.Ordring,
                Active = model.Active,
                PageCategorieId = model.PageCategoriesId
            };

            _context.Add(page);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ✅ Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var page = await _context.Pages
                .Where(p => p.Id == id)
                .Select(p => new PageViewModel
                {
                    Id = p.Id,
                    PageTitle = p.PageTitle,
                    PageDetails = p.PageDetails,
                    Ordring = p.Ordring,
                    Active = p.Active,
                    PageCategoriesId = p.PageCategorieId
                })
                .FirstOrDefaultAsync();

            if (page == null)
                return NotFound();

            ViewData["CategoriesId"] = new SelectList(_context.PageCategories, "Id", "CatName", page.PageCategoriesId);
            return View(page);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PageViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["CategoriesId"] = new SelectList(_context.PageCategories, "Id", "CatName", model.PageCategoriesId);
                return View(model);
            }

            var page = await _context.Pages.FindAsync(id);
            if (page == null)
                return NotFound();

            page.PageTitle = model.PageTitle;
            page.PageDetails = model.PageDetails;
            page.Ordring = model.Ordring;
            page.Active = model.Active;
            page.PageCategorieId = model.PageCategoriesId;

            _context.Update(page);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ✅ Delete
        //[HttpGet]
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //        return NotFound();

        //    var page = await _context.Pages
        //        .Include(p => p.pagecategorie)
        //        .Select(p => new PageViewModel
        //        {
        //            Id = p.Id,
        //            PageTitle = p.PageTitle,
        //            PageDetails = p.PageDetails,
        //            Ordring = p.Ordring,
        //            Active = p.Active,
        //            PageCategoriesId = p.PageCategorieId,
        //            pagecategories = p.pagecategorie
        //        })
        //        .FirstOrDefaultAsync(p => p.Id == id);

        //    return page == null ? NotFound() : View(page);
        //}

        //[HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        // POST: Pages/Delete/5
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var page = await _context.Pages.FindAsync(id);
            if (page == null)
                return NotFound();

            _context.Pages.Remove(page);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
            // Return success as JSON
        }

        private bool PageExists(int id) => _context.Pages.Any(e => e.Id == id);
        //[HttpDelete]
        //public IActionResult DeleteFile(string fileName)
        //{
        //    if (string.IsNullOrWhiteSpace(fileName))
        //        return Json(new { success = false, message = "❌ اسم الملف غير صالح." });

        //    try
        //    {
        //        // Full file path
        //        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", fileName);

        //        // Check if file exists
        //        if (!System.IO.File.Exists(filePath))
        //            return Json(new { success = false, message = "⚠️ الملف غير موجود." });

        //        // Delete the file
        //        System.IO.File.Delete(filePath);
        //        return Json(new { success = true, message = "✅ تم حذف الملف بنجاح." });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = $"⚠️ خطأ أثناء حذف الملف: {ex.Message}" });
        //    }
        //}

    }
}

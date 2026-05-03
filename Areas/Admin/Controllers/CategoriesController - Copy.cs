using AnnouncmentHub.Data;
using AnnouncmentHub.Models;
using AnnouncmentHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions; // ✅ Add this namespace

namespace AnnouncmentHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,NormalUser")]
    public class CategoriesController1 : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController1(ApplicationDbContext context)
        {
            _context = context;
        }

     public IActionResult Index1(string? categoryName = null, int? parentId = null, bool onlyParents = false, int page = 1)
        {
            // ✅ Load parent categories for the filter dropdown
            ViewBag.ParentCategories = GetParentCategoriesBasic1();

            const int pageSize = 40;

            // ✅ Base query with relationships
            var query = _context.Categories
                .Include(c => c.ParentMappings)
                    .ThenInclude(pm => pm.ParentCategory)
                .AsNoTracking()
                .AsQueryable();

            // 🔍 Search by category name (case-insensitive)
            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                var term = categoryName.Trim();
                query = query.Where(c => c.CatName.Contains(term));
            }

            // 📂 Filter by parent category
            if (parentId.HasValue && parentId.Value != -1)
            {
                query = query.Where(c => c.ParentMappings.Any(pm => pm.ParentCategoryId == parentId.Value));
            }

            // 👑 Show only main (parent) categories
            if (onlyParents)
            {
                query = query.Where(c => c.IsParent);
            }

            // 🔢 Pagination (sorted: parents first, then alphabetically)
            var pagedCategories = query
                .OrderByDescending(c => c.IsParent).ThenByDescending(c => c.Id)
                .ThenBy(c => c.CatName)
                .ToPagedList(page, pageSize);

            return View(pagedCategories);
        }


        // GET: Categories/Create
        public IActionResult Create1()
        {
            var model = new CategoryViewModel
            {
                ParentCategories = GetParentCategoriesBasic1()
            };

            return View(model);
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create1(CategoryViewModel model)
        {
            // 🔹 Validate: subcategory must have a parent
            if (!model.IsParent && (model.ParentCategoryIds == null || !model.ParentCategoryIds.Any()))
            {
                ModelState.AddModelError("ParentCategoryIds", "يجب ربط هذا التصنيف بتصنيف رئيسي واحد على الأقل.");
            }

            if (!ModelState.IsValid)
            {
                // Repopulate parent categories before returning view
                model.ParentCategories = GetParentCategoriesBasic1();
                return View(model);
            }

            // 🔹 Default icon path
            string iconPath = "/uploads/icons/no-icon.png";

            // 🔹 Handle file upload if present
            if (model.IconFile != null && model.IconFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "icons");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(model.IconFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.IconFile.CopyToAsync(stream);
                }

                iconPath = $"/uploads/icons/{uniqueName}";
            }

            // 🔹 Create category
            var category = new Category
            {
                CatName = model.CatName?.Trim(),
                IsParent = model.IsParent,
                IsActive = model.IsActive,
                IsVIP = model.IsVIP,
                IconUrl = iconPath
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // 🔹 Add parent mappings if not parent
            if (!model.IsParent && model.ParentCategoryIds.Any())
            {
                foreach (var parentId in model.ParentCategoryIds)
                {
                    _context.CategoryParentMappings.Add(new CategoryParentMapping
                    {
                        ParentCategoryId = parentId,
                        SubCategoryId = category.Id
                    });
                }
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "✅ تم إنشاء التصنيف بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        private List<SelectListItem> GetParentCategoriesBasic1()
        {
            var parents = _context.Categories
                .Where(c => c.IsParent && c.IsActive)
                .AsNoTracking()
                .OrderBy(c => c.CatName)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.CatName
                }).ToList();

            return parents;
        }


        public async Task<IActionResult> Edit1(int? id)
        {
            if (id == null)
                return NotFound();

            var category = await _context.Categories
                .Include(c => c.ParentMappings)
                .Where(c => c.Id == id)
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    CatName = c.CatName,
                    IsParent = c.IsParent,
                    IsActive = c.IsActive,
                    IsVIP = c.IsVIP,
                    IconName=c.IconUrl,
                    ParentCategoryIds = c.ParentMappings.Select(pm => pm.ParentCategoryId).ToList(),
                    ParentCategories = _context.Categories
                        .Where(pc => pc.IsParent && pc.IsActive)
                        .Select(pc => new SelectListItem
                        {
                            Value = pc.Id.ToString(),
                            Text = pc.CatName
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound();

            return View(category);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit1(int id, CategoryViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            // Validation: child categories must have at least one parent
            if (!model.IsParent && (model.ParentCategoryIds == null || !model.ParentCategoryIds.Any()))
            {
                ModelState.AddModelError("ParentCategoryIds", "يجب ربط هذا التصنيف بتصنيف رئيسي واحد على الأقل.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var category = await _context.Categories
                        .Include(c => c.ParentMappings)
                        .FirstOrDefaultAsync(c => c.Id == id);

                    if (category == null)
                        return NotFound();

                    // ✅ Update basic fields
                    category.CatName = model.CatName?.Trim();
                    category.IsParent = model.IsParent;
                    category.IsActive = model.IsActive;
                    category.IsVIP = model.IsVIP;

                    // ✅ Handle icon file upload
                    if (model.IconFile != null && model.IconFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "icons");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        // ✅ Delete old icon if it exists — but skip the default one
                        if (!string.IsNullOrEmpty(category.IconUrl))
                        {
                            // Normalize the URL and check if it’s the default placeholder
                            var iconPath = category.IconUrl.Replace('\\', '/');

                            // Skip deletion for the default icon
                            if (!iconPath.Equals("/uploads/icons/no-icon.png", StringComparison.OrdinalIgnoreCase))
                            {
                                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", category.IconUrl.TrimStart('/'));
                                if (System.IO.File.Exists(oldPath))
                                {
                                    System.IO.File.Delete(oldPath);
                                }
                            }
                        }


                        // Save new icon
                        var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(model.IconFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.IconFile.CopyToAsync(stream);
                        }

                        category.IconUrl = $"/uploads/icons/{uniqueName}";
                    }

                    // ✅ Update parent relationships
                    if (model.IsParent)
                    {
                        // Remove all parent mappings if now a top-level parent
                        _context.CategoryParentMappings.RemoveRange(category.ParentMappings);
                    }
                    else
                    {
                        // Remove old mappings not in the new selection
                        var toRemove = category.ParentMappings
                            .Where(pm => !model.ParentCategoryIds.Contains(pm.ParentCategoryId))
                            .ToList();
                        _context.CategoryParentMappings.RemoveRange(toRemove);

                        // Add new mappings
                        var existingParentIds = category.ParentMappings.Select(pm => pm.ParentCategoryId).ToList();
                        var toAdd = model.ParentCategoryIds
                            .Where(pid => !existingParentIds.Contains(pid))
                            .Select(pid => new CategoryParentMapping
                            {
                                ParentCategoryId = pid,
                                SubCategoryId = id
                            });

                        await _context.CategoryParentMappings.AddRangeAsync(toAdd);
                    }

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "✅ تم تحديث التصنيف بنجاح!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categories.Any(c => c.Id == id))
                        return NotFound();
                    else
                        throw;
                }
            }

            // ❌ If validation fails, reload parent list and redisplay
            model.ParentCategories = _context.Categories
                .Where(pc => pc.IsParent && pc.IsActive)
                .Select(pc => new SelectListItem
                {
                    Value = pc.Id.ToString(),
                    Text = pc.CatName
                }).ToList();

            return View(model);
        }


        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete1(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentMappings)
                    .ThenInclude(pm => pm.ParentCategory)
                .Include(c => c.SubCategoryMappings)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            var model = new CategoryViewModel
            {
                Id = category.Id,
                CatName = category.CatName,
                IsParent = category.IsParent,
                IsActive = category.IsActive,
                ParentCategoryNames = category.ParentMappings != null && category.ParentMappings.Any()
                    ? string.Join(", ", category.ParentMappings.Select(pm => pm.ParentCategory.CatName))
                    : "لا يوجد"
            };

            return View(model);
        }

        // POST: Categories/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var category = await _context.Categories
        //        .Include(c => c.ParentMappings)
        //        .Include(c => c.SubCategoryMappings)
        //        .FirstOrDefaultAsync(c => c.Id == id);

        //    if (category == null)
        //    {
        //        return NotFound();
        //    }
        //    var isUsedInAnnouncements = await _context.AnnouncementCategories
        //      .AnyAsync(ac => ac.CategoryId == id);

        //    if (isUsedInAnnouncements)
        //    {
        //        TempData["Error"] = "❌ لا يمكن حذف هذا التصنيف لأنه مرتبط بإعلانات.";
        //        return RedirectToAction(nameof(Index));
        //    }

        //    if (category.SubCategoryMappings != null && category.SubCategoryMappings.Any())
        //    {
        //        TempData["Error"] = "❌ لا يمكن حذف هذا التصنيف لأنه يحتوي على تصنيفات فرعية.";
        //        return RedirectToAction(nameof(Index));
        //    }

        //    if (category.ParentMappings != null && category.ParentMappings.Any())
        //    {
        //        _context.CategoryParentMappings.RemoveRange(category.ParentMappings);
        //    }

        //    _context.Categories.Remove(category);
        //    await _context.SaveChangesAsync();

        //    TempData["Success"] = "✅ تم حذف التصنيف بنجاح!";
        //    return RedirectToAction(nameof(Index));
        //}

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed1(int id)
        {
            var category = await _context.Categories
                .Include(c => c.ParentMappings)
                .Include(c => c.SubCategoryMappings)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            // 🔹 1. Check if category is used in announcements
            var isUsedInAnnouncements = await _context.AnnouncementCategories
                .AnyAsync(ac => ac.CategoryId == id);

            if (isUsedInAnnouncements)
            {
                TempData["Error"] = "❌ لا يمكن حذف هذا التصنيف لأنه مرتبط بإعلانات.";
                return RedirectToAction(nameof(Index));
            }

            // 🔹 2. Prevent deleting parent categories that have subcategories
            if (category.SubCategoryMappings != null && category.SubCategoryMappings.Any())
            {
                TempData["Error"] = "❌ لا يمكن حذف هذا التصنيف لأنه يحتوي على تصنيفات فرعية.";
                return RedirectToAction(nameof(Index));
            }

            // 🔹 3. Remove related parent mappings
            if (category.ParentMappings != null && category.ParentMappings.Any())
            {
                _context.CategoryParentMappings.RemoveRange(category.ParentMappings);
            }

            // 🔹 4. Delete category icon safely (skip default)
            if (!string.IsNullOrEmpty(category.IconUrl))
            {
                var iconPath = category.IconUrl.Replace('\\', '/');
                if (!iconPath.Equals("/uploads/icons/no-icon.png", StringComparison.OrdinalIgnoreCase))
                {
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", iconPath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }
            }

            // 🔹 5. Remove category itself
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ تم حذف التصنيف بنجاح!";
            return RedirectToAction(nameof(Index));
        }

    }
}

using AnnouncmentHub.Data;
using AnnouncmentHub.Models;
using AnnouncmentHub.Service;
using AnnouncmentHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using X.PagedList;
using X.PagedList.Extensions;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace AnnouncmentHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,NormalUser")]
    public class AnnouncementsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementsController(ApplicationDbContext context)
        {
            _context = context;
        }

          public async Task<IActionResult> Index(string categoryName, int? parentCategoryId, int? subCategoryId, int page = 1)
        {
            // ✅ إعداد القوائم المنسدلة للتصنيفات
            var parentCategories = GetParentCategoriesBasic(); // List<SelectListItem>
            var subCategories = GetSubCategoriesFltr(parentCategoryId ?? -1); // List<SelectListItem>

            ViewBag.ParentCategories = parentCategories;
            ViewBag.SubCategories = subCategories;

            // ✅ تمرير القيم المختارة لتثبيتها في الـ View
            ViewBag.ParentCategoryIdFilter = parentCategoryId;
            ViewBag.SubCategoryIdFilter = subCategoryId;
            ViewBag.CategoryNameFilter = categoryName;

            var pageSize = 50;

            // ✅ إعداد الاستعلام الأساسي مع العلاقات المطلوبة
            var announcementsQuery = _context.Announcements
                //.Where(a => a.IsActive)
                .Include(a => a.AnnouncementCategories)
                    .ThenInclude(ac => ac.Category)
                        .ThenInclude(c => c.ParentMappings)
                            .ThenInclude(pm => pm.ParentCategory)
                .AsQueryable();

            // ✅ فلترة حسب التصنيف الرئيسي
            if (parentCategoryId.HasValue && parentCategoryId.Value != -1)
            {
                var subCategoryIds = _context.CategoryParentMappings
                    .Where(cpm => cpm.ParentCategoryId == parentCategoryId.Value)
                    .Select(cpm => cpm.SubCategoryId)
                    .Distinct();

                announcementsQuery = announcementsQuery.Where(a =>
                    a.AnnouncementCategories.Any(ac =>
                        subCategoryIds.Contains(ac.CategoryId)
                    )
                );
            }

            // ✅ فلترة حسب العنوان
            if (!string.IsNullOrEmpty(categoryName))
            {
                announcementsQuery = announcementsQuery.Where(a =>
                    a.Title.Contains(categoryName)
                );
            }

            // ✅ فلترة حسب التصنيف الفرعي
            if (subCategoryId.HasValue && subCategoryId.Value != -1)
            {
                announcementsQuery = announcementsQuery.Where(a =>
                    a.AnnouncementCategories.Any(ac => ac.CategoryId == subCategoryId)
                );
            }

            // ✅ إحضار النتائج مع التصفح
            int totalCount = await announcementsQuery.CountAsync();

            var pagedAnnouncements = await announcementsQuery
                .OrderByDescending(a => a.AddedDate)
                .Select(a => new AnnouncementViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    FilePath = a.FilePath,
                    AddedDate = a.AddedDate,
                    IsActive = a.IsActive,
                    SubCategoryNames = a.AnnouncementCategories
                        .Select(ac => ac.Category.CatName)
                        .Distinct()
                        .ToList(),
                    ParentCategoryNames = a.AnnouncementCategories
                        .SelectMany(ac => ac.Category.ParentMappings.Select(pm => pm.ParentCategory.CatName))
                        .Distinct()
                        .ToList(),
                    SelectedSubCategories = a.AnnouncementCategories
                        .Select(ac => ac.CategoryId)
                        .Distinct()
                        .ToList()
                })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pagedList = new StaticPagedList<AnnouncementViewModel>(pagedAnnouncements, page, pageSize, totalCount);

            return View(pagedList);
        }

        private IEnumerable<Category> GetSubCategoriesFltr(int parentId)
        {
            if (parentId == -1) return new List<Category>();

            var subcategories = _context.CategoryParentMappings
                .Where(cpm => cpm.ParentCategoryId == parentId)
                .Select(cpm => cpm.SubCategory)
                .OrderBy(c => c.CatName)
                .ToList();

            subcategories.Insert(0, new Category { Id = -1, CatName = "اختر الفئة الفرعية" });

            return subcategories;
        }




        [HttpGet]
        public async Task<IActionResult> GetSubcategories(int parentId)
        {
            if (parentId == -1)
            {
                return Json(new List<object>()); // Return empty list if no parent selected
            }

            // Get all subcategories that are linked to the selected parent category
            var subcategories = await _context.CategoryParentMappings
                .Where(cpm => cpm.ParentCategoryId == parentId)  // Filter by the selected parent category
                .Select(cpm => new { id = cpm.SubCategory.Id, catName = cpm.SubCategory.CatName })
                .OrderBy(sc => sc.catName)
                .ToListAsync();

            return Json(subcategories);
        }

        private List<SelectListItem> GetParentCategoriesBasic()
        {
            var parentCategories = _context.Categories
                .Where(c => c.IsParent && c.IsActive)
                .OrderBy(c => c.CatName)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.CatName
                }).ToList();

            parentCategories.Insert(0, new SelectListItem { Value = "-1", Text = "-- حسب التصنيف الرئيسي --" });

            return parentCategories;
        }



 
        public IActionResult Create()
        {
            // Fetch all clients (to be selected in the form)
            var clients = _context.Clients.ToList();

            // Fetch all subcategories and their associated parent categories
            var subcategoriesWithParents = _context.Categories
                .Where(c => !c.IsParent) // Only subcategories (exclude parents)
                .Select(subcategory => new
                {
                    SubCategoryId = subcategory.Id,
                    SubCategoryName = subcategory.CatName,
                    ParentCategories = _context.CategoryParentMappings
                        .Where(cpm => cpm.SubCategoryId == subcategory.Id)
                        .Select(cpm => cpm.ParentCategory.CatName) // Get the names of the associated parent categories
                        .ToList()
                })
                .ToList();

            // Pass the subcategories with their parent categories
            ViewBag.SubCategoriesWithParents = subcategoriesWithParents;

            // Pass the number of subcategories to the view
            ViewBag.SubCategoriesCount = subcategoriesWithParents.Count();

            // Pass the list of clients to the view
            ViewBag.Clients = new SelectList(clients, "Id", "ClientName");

            return View(new AnnouncementViewModel());
        }
       [HttpPost]
       [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AnnouncementViewModel model, IFormFile file)
        {
            // Check if the model state is valid
            if (!ModelState.IsValid)
            {
                // Fetch parent categories and subcategories for the view
                var parentCategories = _context.Categories.Where(c => c.IsParent).ToList();
                ViewBag.ParentCategoriesWithSubCategories = parentCategories;

                // Fetch subcategories and their associated parent categories
                var subcategoriesWithParents = _context.Categories
                    .Where(c => !c.IsParent) // Only subcategories (exclude parents)
                    .Select(subcategory => new
                    {
                        SubCategoryId = subcategory.Id,
                        SubCategoryName = subcategory.CatName,
                        ParentCategories = _context.CategoryParentMappings
                            .Where(cpm => cpm.SubCategoryId == subcategory.Id)
                            .Select(cpm => cpm.ParentCategory.CatName) // Get the names of the associated parent categories
                            .ToList()
                    })
                    .ToList();

                // Pass the subcategories with their parent categories
                ViewBag.SubCategoriesWithParents = subcategoriesWithParents;

                // Fetch clients for the dropdown
                var clients = _context.Clients.ToList();
                ViewBag.Clients = clients.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.ClientName
                });
               
                return View(model);  // Return the view with validation errors and pre-populated data
            }

            // Validate the file upload
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("File", "يرجى رفع ملف.");

                // Fetch parent categories and subcategories for the view again
                var parentCategories = _context.Categories.Where(c => c.IsParent).ToList();
                ViewBag.ParentCategoriesWithSubCategories = parentCategories;

                // Fetch subcategories with their associated parent categories again
                var subcategoriesWithParents = _context.Categories
                    .Where(c => !c.IsParent) // Only subcategories
                    .Select(subcategory => new
                    {
                        SubCategoryId = subcategory.Id,
                        SubCategoryName = subcategory.CatName,
                        ParentCategories = _context.CategoryParentMappings
                            .Where(cpm => cpm.SubCategoryId == subcategory.Id)
                            .Select(cpm => cpm.ParentCategory.CatName)
                            .ToList()
                    })
                    .ToList();

                // Pass subcategories and their parent categories to ViewBag
                ViewBag.SubCategoriesWithParents = subcategoriesWithParents;

                // Fetch clients for the dropdown again
                var clients = _context.Clients.ToList();
                ViewBag.Clients = clients.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.ClientName
                });

                return View(model); // Return view with existing data and error messages
            }

            // Save the file and get its relative file path
            model.FilePath = await SaveFileAsync("Create", file, null);

            // Create the announcement object
            var announcement = new Announcement
            {
                Title = model.Title,
                Description = model.Description,
                FilePath = model.FilePath,  // Store the file path here
                AddedDate = DateTime.Now,
                IsActive = model.IsActive,
                DateFrom = model.DateFrom, // Nullable DateTime, can be null
                DateTo = model.DateTo,     // Nullable DateTime, can be null
                ClientId = model.ClientId, // Associate client if selected
            };

            // Link the selected subcategories to the announcement
            if (model.SelectedSubCategories != null)
            {
                foreach (var subcategoryId in model.SelectedSubCategories)
                {
                    var announcementCategory = new AnnouncementCategory
                    {
                        CategoryId = subcategoryId,
                        Announcement = announcement
                    };

                    // Add the subcategory to the announcement
                    announcement.AnnouncementCategories.Add(announcementCategory);
                }
            }

            // Save the announcement to the database
            _context.Announcements.Add(announcement);
            // ✅ Save multiple images using the same helper
            if (model.ImageFiles != null && model.ImageFiles.Any())
            {
                foreach (var image in model.ImageFiles)
                {
                    if (image == null || image.Length == 0)
                        continue;

                    var imagePath = await SaveFileAsync("Create", image, null);

                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        announcement.AnnouncementImages.Add(new AnnouncementImage
                        {
                            ImageUrl = imagePath
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            // Set success message and redirect to the Index page
            TempData["Success"] = "✅ تم إضافة الإعلان بنجاح!";
            return RedirectToAction(nameof(Index));
        }



        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var announcement = await _context.Announcements
                .Include(a => a.AnnouncementCategories)
                .Include(a => a.AnnouncementImages)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (announcement == null) return NotFound();

            // Fetch all clients (to be selected in the form)
            var clients = _context.Clients.ToList();

            // Load the view model and populate fields including selected subcategories
            var model = new AnnouncementViewModel
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Description = announcement.Description,
                FilePath = announcement.FilePath,
                AddedDate = announcement.AddedDate,
                IsActive = announcement.IsActive,
                DateFrom = announcement.DateFrom, // Nullable DateTime, can be null
                DateTo = announcement.DateTo,     // Nullable DateTime, can be null
                ClientId = announcement.ClientId, // Associate client if selected
                ExistingImages=announcement.AnnouncementImages.ToList(),
                SelectedSubCategories = announcement.AnnouncementCategories.Select(ac => ac.CategoryId).ToList()
            };

            // Fetch subcategories with their parent categories as in Create GET
            var subcategoriesWithParents = _context.Categories
                .Where(c => !c.IsParent)
                .Select(subcategory => new
                {
                    SubCategoryId = subcategory.Id,
                    SubCategoryName = subcategory.CatName,
                    ParentCategories = _context.CategoryParentMappings
                        .Where(cpm => cpm.SubCategoryId == subcategory.Id)
                        .Select(cpm => cpm.ParentCategory.CatName)
                        .ToList()
                })
                .ToList();

            ViewBag.SubCategoriesWithParents = subcategoriesWithParents;
            // Pass the list of clients to the view
            ViewBag.Clients = new SelectList(clients, "Id", "ClientName", announcement.ClientId);

            return View(model);
        }

        // POST: Announcements/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AnnouncementViewModel model, IFormFile? file)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                // Reload subcategories with parents for the view if validation fails
                var subcategoriesWithParents = _context.Categories
                    .Where(c => !c.IsParent)
                    .Select(subcategory => new
                    {
                        SubCategoryId = subcategory.Id,
                        SubCategoryName = subcategory.CatName,
                        ParentCategories = _context.CategoryParentMappings
                            .Where(cpm => cpm.SubCategoryId == subcategory.Id)
                            .Select(cpm => cpm.ParentCategory.CatName)
                            .ToList()
                    })
                    .ToList();

                ViewBag.SubCategoriesWithParents = subcategoriesWithParents;
                return View(model);
            }

            var announcement = await _context.Announcements
                .Include(a => a.AnnouncementCategories)
                .Include(a => a.AnnouncementImages)
               
                .FirstOrDefaultAsync(a => a.Id == id);

            if (announcement == null) return NotFound();

            // Update file if a new file is uploaded
            if (file != null && file.Length > 0)
            {
                // Save new file and delete old if exists
                model.FilePath = await SaveFileAsync("Edit", file, announcement.FilePath);
                announcement.FilePath = model.FilePath;
            }

            // Update other fields
            announcement.Title = model.Title;
            announcement.Description = model.Description;
            announcement.IsActive = model.IsActive;
            announcement.DateFrom = model.DateFrom; // Nullable DateTime, can be null
            announcement.DateTo = model.DateTo;    // Nullable DateTime, can be null
            announcement.ClientId = model.ClientId; // Associate client if selected
            // Optionally update AddedDate or keep original date


            // Remove old categories related to the announcement
            _context.AnnouncementCategories.RemoveRange(announcement.AnnouncementCategories);


            // Add newly selected subcategories
            if (model.SelectedSubCategories != null)
            {
                foreach (var subcategoryId in model.SelectedSubCategories)
                {
                    announcement.AnnouncementCategories.Add(new AnnouncementCategory
                    {
                        CategoryId = subcategoryId,
                        AnnouncementId = announcement.Id
                    });
                }
            }

            // Add new additional images (do NOT delete old ones)
            if (model.ImageFiles != null && model.ImageFiles.Any())
            {
                foreach (var image in model.ImageFiles)
                {
                    if (image.Length > 0)
                    {
                        var imagePath = await SaveFileAsync("Create", image,null);

                        announcement.AnnouncementImages.Add(new AnnouncementImage
                        {
                            ImageUrl = imagePath,
                            AnnouncementId = announcement.Id
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ تم تعديل الإعلان بنجاح!";
            return RedirectToAction(nameof(Index));
        }


        private async Task<string> SaveFileAsync(string operation, IFormFile file, string oldPathInEdit)
        {
            try
            {
                // ✅ Define the uploads folder inside "wwwroot/uploads/YYYY/MM" (organized by year/month)
                var currentYear = DateTime.Now.Year.ToString();
                var currentMonth = DateTime.Now.Month.ToString("D2"); // Numeric month (01, 02, ..., 12)
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", currentYear, currentMonth);

                // ✅ Ensure the directory exists
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);  // Create directory if it doesn't exist
                }

                // ✅ Generate a unique file name using GUID
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // ✅ Save the file securely to the file system
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);  // Copy the uploaded file to the file stream
                }

                // ✅ Delete old file when editing (if applicable)
                if (!string.IsNullOrEmpty(oldPathInEdit) && operation == "Edit")
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldPathInEdit.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);  // Delete old file from server
                    }
                }

                // ✅ Return the relative file path to store in the database (no leading '/' for easier storage)
                return $"/uploads/{currentYear}/{currentMonth}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                // ✅ Log the error (you can integrate with a logging framework like Serilog or NLog)
                Console.WriteLine($"Error saving file: {ex.Message}");
                return string.Empty;  // Return empty string if file save fails
            }
        }

        [HttpPost]
        [Route("Admin/Announcements/DeleteImage/{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var image = await _context.AnnouncementImage.FindAsync(id);
            if (image == null) return NotFound();

            // حذف الملف من السيرفر
            var filePath = Path.Combine(
                Directory.GetCurrentDirectory(),"wwwroot",image.ImageUrl.TrimStart('/')
            );

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _context.AnnouncementImage.Remove(image);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }

            // ✅ Delete associated file if exists
            if (!string.IsNullOrEmpty(announcement.FilePath))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", announcement.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ تم حذف الإعلان بنجاح!";
            return RedirectToAction(nameof(Index));
        }


        private bool AnnouncementExists(int id)
        {
            return _context.Announcements.Any(e => e.Id == id);
        }
    }
}

using AnnouncmentHub.Data;
using AnnouncmentHub.Models;
using AnnouncmentHub.Service;
using AnnouncmentHub.Services;
using AnnouncmentHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;

namespace AnnouncmentHub.Controllers
{
    public class HubController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AnnouncementRepository _repository;
        private readonly BreadcrumbService _breadcrumbService;

        public HubController(ApplicationDbContext context,  AnnouncementRepository repository, BreadcrumbService breadcrumbService)
        {
            _context = context;
            _repository = repository;
            _breadcrumbService = breadcrumbService;
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> SeedData(
    [FromServices] UserAndRoleDataInitializer seeder)
        {
            var messages = await seeder.SeedDataAsync();

            var result = new
            {
                success = true,
                عدد_العمليات = messages.Count,
                النتائج = messages
            };

            var options = new System.Text.Json.JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };

            var json = System.Text.Json.JsonSerializer.Serialize(result, options);
            return Content(json, "application/json; charset=utf-8");
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.AnnouncementCount = await _context.Announcements
                .CountAsync(a => a.IsActive);

            ViewBag.ClientCount = await _context.Clients
                .CountAsync(c => c.IsActive);

            // Parent categories for the hero search dropdown
            ViewBag.ParentCategories = await _context.Categories
                .Where(c => c.IsParent)
                .AsNoTracking()
                .ToListAsync();

            return View();
        }
        private string RenderPartial(string partialName, object model)
        {
            return RazorViewToStringRenderer.RenderViewToString(HttpContext, partialName, model);
        }

        // GET: LandingPage (Parent categories, announcements by default)
        public async Task<IActionResult> LoadingHub()
        {
            // Fetch all parent categories
            var parentCategories = await _context.Categories
                .Where(c => c.IsParent)
                .ToListAsync();

            // Fetch all announcements (no filter applied yet)
            var announcements = await _context.Announcements
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.AddedDate)
                .ToListAsync();

            // Map announcements to view model with ThumbnailPath
            var announcementsViewModel = announcements.Select(a => new AnnouncementViewModel
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                FilePath = a.FilePath,
                AddedDate = a.AddedDate,
                IsActive = a.IsActive,
                ThumbnailPath = GetThumbnailPath(a.FilePath)  // Ensure ThumbnailPath is set correctly
            }).ToList();

            // Pass data to view
            ViewBag.ParentCategories = parentCategories;
            ViewBag.Announcements = announcementsViewModel;

            // 🟢 Breadcrumb: just Home
            ViewBag.Breadcrumb = new List<BreadcrumbItem>
                {
                 new BreadcrumbItem { Id = 0, Name = "الرئيسية" }
                };

            return View();
        }

        // Deducing the thumbnail path based on file type
        private string GetThumbnailPath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return "/images/file-icons/no-image.jpg"; // Default fallback image

            var extension = Path.GetExtension(filePath)?.ToLower();

            if (extension == ".pdf")
                return "/images/file-icons/pdf-icon.png"; // PDF icon

            // List of image extensions
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };

            if (imageExtensions.Contains(extension))
                return filePath; // Use the image itself as thumbnail

            return "/images/file-icons/no-image.jpg"; // Default file icon for other types
        }


        // GET: GetSubcategories (For AJAX request to load subcategories)
        public JsonResult GetSubcategories(int parentId)
        {
            var subcategories = _context.CategoryParentMappings
                .Where(cpm => cpm.ParentCategoryId == parentId)
                .Select(cpm => new { id = cpm.SubCategory.Id, catName = cpm.SubCategory.CatName })
                .AsNoTracking().ToList();

            return Json(subcategories);
        }

        // GET: GetAnnouncementsBySubcategory (For AJAX request to load announcements by subcategory)
        public JsonResult GetAnnouncementsBySubcategory(int subCategoryId)
        {
            // 1. Get data from DB first (materialize it)
            var announcementData = _context.Announcements
                .Where(a => a.AnnouncementCategories.Any(ac => ac.CategoryId == subCategoryId))
                .ToList(); // <- Force query execution here

            // 2. Then project using C# methods
            var announcements = announcementData.Select(a => new
            {
                a.Id,
                a.Title,
                a.Description,
                a.FilePath,
                ThumbnailPath = GetThumbnailPath(a.FilePath)
            }).ToList();

            return Json(announcements);
        }

        //public async Task<IActionResult> RandomAnnouncementsTop30()
        //{
        //    var result = await _repository.GetAnnouncementsDynamic(
        //    title: null,
        //    categoryIds: new List<int>(), // فارغ
        //    clientId: null,
        //    dateFrom: null,
        //    dateTo: null,
        //    pageNumber: 1,
        //    isRandom: true,
        //    pageSize: 30);



        //    // خلط النتائج عشوائياً
        //    var random = new Random();
        //  //  var randomAnnouncements = result.Announcements.OrderBy(a => random.Next()).ToList();
        //  var randomAnnouncements = result.Announcements.OrderBy(x => Guid.NewGuid()).ToList();
        //    // إنشاء ViewModel
        //    var vm = new CategoryAnnouncementsViewModel
        //    {
        //        Announcements = randomAnnouncements,
        //        TotalCount = result.TotalCount,
        //        PageNumber = 1,
        //        PageSize = 30
        //    };

        //    return PartialView("_RandomAnnouncementsTop30", vm);
        //}



        //public async Task<IActionResult> RandomAnnouncementsTop30()
        //{
        //    var result = await _repository.GetAnnouncementsDynamic(
        //        title: null,
        //        categoryIds: null, // أفضل من new List<int>()
        //        clientId: null,
        //        dateFrom: null,
        //        dateTo: null,
        //        pageNumber: 1,
        //        pageSize: 30,
        //        isRandom: true
        //    );

        //    var vm = new CategoryAnnouncementsViewModel
        //    {
        //        Announcements = result.Announcements, // ✅ بدون إعادة ترتيب
        //        TotalCount = result.TotalCount,
        //        PageNumber = 1,
        //        PageSize = 30
        //    };

        //    return PartialView("_RandomAnnouncementsTop30", vm);
        //}
        public async Task<IActionResult> RandomAnnouncementsTop30()
        {
            var result = await _repository.GetAnnouncementsDynamic(
                title: null,
                categoryIds: null,
                clientId: null,
                dateFrom: null,
                dateTo: null,
                pageNumber: 1,
                pageSize: 30,
                isRandom: true
            );

            // ✅ استخدام AnnouncementSearchResult مباشرة بدل CategoryAnnouncementsViewModel
            return PartialView("_RandomAnnouncementsTop30", result);
        }
        public async Task<IActionResult> Top30RandomClients()
        {

            var clients = await _context.Clients
       .Where(c => c.IsActive)
       .OrderBy(c => Guid.NewGuid())
       .Take(30)
       .Select(c => new ClientViewModel
       {
           Id = c.Id,
           ClientName = c.ClientName,
           LogoUrl = c.LogoUrl,
           FacebookLink = c.FacebookLink,
           WhatsUp = c.WhatsUp,
           MobileNumber = c.MobileNumber,
           SiteUrl = c.SiteUrl
       })
       .ToListAsync();

            return PartialView("_Top30RandomClients", clients);

           
        }


    //    public async Task<IActionResult> Search(
    //string title,
    //List<int>? categoryIds,
    //int? clientId,
    //DateTime? dateFrom,
    //DateTime? dateTo,
    //int page = 1,
    //int pageSize = 10)
    //    {
    //        var result = await _repository.GetAnnouncementsDynamic(
    //            title,
    //            categoryIds,
    //            clientId,
    //            dateFrom,
    //            dateTo,
    //            page,
    //            pageSize
    //        );

    //        ViewBag.TotalCount = result.TotalCount;
    //        ViewBag.AllCategories = await GetAllCategories();
    //        ViewBag.SelectedCategories = categoryIds ?? new List<int>();

    //        return View(result);
    //    }
        public async Task<IActionResult> Search(
            string? title,
            int? mainCategoryId,
            List<int>? subCategoryIds,
            int? clientId,
            DateTime? dateFrom,
            DateTime? dateTo,
            int page = 1,
            int pageSize = 12)
        {
            // 1️⃣ Load Main Categories
            var mainCategories = await _context.Categories
                .Where(c => c.IsParent)
                .AsNoTracking()
                .ToListAsync();

            // 2️⃣ If main category selected → load its subcategories
            List<Category> subCategories = new();

            if (mainCategoryId.HasValue)
            {
                subCategories = await _context.CategoryParentMappings
                    .Where(x => x.ParentCategoryId == mainCategoryId)
                    .Select(x => x.SubCategory)
                    .AsNoTracking()
                    .ToListAsync();
            }

            // 3️⃣ If user selected main category but did NOT select subcategories → auto-select ALL subcategories
            if (mainCategoryId.HasValue && (subCategoryIds == null || !subCategoryIds.Any()))
            {
                subCategoryIds = subCategories.Select(s => s.Id).ToList();
            }

            // 4️⃣ Call repository (Dapper SP + TVP)
            var result = await _repository.GetAnnouncementsDynamic(
                title,
                subCategoryIds,
                clientId,
                dateFrom,
                dateTo,
                page,
                pageSize
            );

            // 5️⃣ Prepare ViewModel
            var vm = new Search2ViewModel
            {
                Title = title,
                MainCategoryId = mainCategoryId,
                SubCategoryIds = subCategoryIds,
                ClientId = clientId,
                DateFrom = dateFrom,
                DateTo = dateTo,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = result.TotalCount,

                MainCategories = mainCategories,
                SubCategories = subCategories,
                Announcements = result.Announcements
            };

            return View(vm);
        }


        //[HttpPost]
        //public async Task<IActionResult> Search2Ajax([FromBody] Search2AjaxRequest req)
        //{
        //    // حوّل التاريخ من string إلى DateTime?
        //    DateTime? from = string.IsNullOrEmpty(req.DateFrom) ? null : DateTime.Parse(req.DateFrom);
        //    DateTime? to = string.IsNullOrEmpty(req.DateTo) ? null : DateTime.Parse(req.DateTo);

        //    // نفس منطق تحديد الفئات الفعلية اللي بنيناه قبل:
        //    List<int>? realCategoryIds = null;

        //    if (req.SubCategoryIds != null && req.SubCategoryIds.Any())
        //    {
        //        realCategoryIds = req.SubCategoryIds;
        //    }
        //    else if (req.MainCategoryId.HasValue)
        //    {
        //        realCategoryIds = await _context.CategoryParentMappings
        //            .Where(m => m.ParentCategoryId == req.MainCategoryId.Value)
        //            .Select(m => m.SubCategoryId)
        //            .ToListAsync();
        //    }

        //    var result = await _repository.GetAnnouncementsDynamic(
        //        req.Title,
        //        realCategoryIds,
        //        null,
        //        from,
        //        to,
        //        req.Page,
        //        req.PageSize
        //    );

        //    // نحتاج SubCategories عشان نرجعها لنفس الصفحة (عادة مش ضروري للجزء الجزئي)
        //    var subCats = new List<Category>();
        //    if (req.MainCategoryId.HasValue)
        //    {
        //        subCats = await _context.CategoryParentMappings
        //            .Where(m => m.ParentCategoryId == req.MainCategoryId.Value)
        //            .Select(m => m.SubCategory)
        //            .ToListAsync();
        //    }

        //    var vm = new Search2ViewModel
        //    {
        //        MainCategoryId = req.MainCategoryId,
        //        SubCategoryIds = req.SubCategoryIds ?? new List<int>(),
        //        Title = req.Title,
        //        DateFrom = from,
        //        DateTo = to,
        //        PageNumber = req.Page,
        //        PageSize = req.PageSize,
        //        TotalCount = result.TotalCount,
        //        Announcements = result.Announcements,
        //        SubCategories = subCats
        //    };


        //    var announcementsHtml = RenderPartial("_Search2AnnouncementsPartial", vm);
        //    var paginationHtml = RenderPartial("_Search2PaginationPartial", vm);
        //    return Json(new
        //    {
        //        announcements = announcementsHtml,
        //        pagination = paginationHtml
        //    });
        //}
        [HttpPost]
        public async Task<IActionResult> Search2Ajax([FromBody] Search2AjaxRequest req)
        {
            DateTime? from = string.IsNullOrEmpty(req.DateFrom) ? null : DateTime.Parse(req.DateFrom);
            DateTime? to = string.IsNullOrEmpty(req.DateTo) ? null : DateTime.Parse(req.DateTo);

            List<int>? realCategoryIds = null;

            if (req.SubCategoryIds != null && req.SubCategoryIds.Any())
            {
                realCategoryIds = req.SubCategoryIds;
            }
            else if (req.MainCategoryId.HasValue)
            {
                realCategoryIds = await _context.CategoryParentMappings
                    .Where(m => m.ParentCategoryId == req.MainCategoryId.Value)
                    .Select(m => m.SubCategoryId)
                    .ToListAsync();
            }

            var result = await _repository.GetAnnouncementsDynamic(
                req.Title,
                realCategoryIds,
                null,
                from,
                to,
                req.Page,
                req.PageSize
            );

            // ✅ مباشرة AnnouncementSearchResult بدل Search2ViewModel
            return Json(new
            {
                announcements = RenderPartial("_AnnouncementsPartial", result),
                pagination = RenderPartial("_PaginationPartial", result)
            });
        }

        public async Task<List<CategoryJsonLink>> GetAllCategories()
        {
            return await _context.Categories.Where(c => c.IsParent == true)
                .Select(c => new CategoryJsonLink
                {
                    CategoryId = c.Id,
                    CategoryName = c.CatName
                })
                .AsNoTracking().ToListAsync();
      
        }
        public async Task<IActionResult> Clients(int page = 1, int pageSize = 10)
        {
            // Calculate the total count of clients
            var totalClients = await _context.Clients.Where(c => c.IsActive == true).CountAsync();

            // Get the clients for the current page
            var clients = await _context.Clients
                .Where(c => c.IsActive == true) // Only active clients
                .OrderBy(c => c.ClientName) // You can order by any column here
                .Skip((page - 1) * pageSize) // Skip based on the current page
                .Take(pageSize) // Take the number of items per page
                .ToListAsync();

            // If no clients are found
            if (clients == null || !clients.Any()) return NotFound();

            // 🟢 Breadcrumb: Home → Client
            ViewBag.Breadcrumb = await _breadcrumbService.GetClientBreadcrumbAsync();

            // ViewModel for pagination
            var viewModel = new ClientViewModel
            {
                Clients = clients,
                PageNumber = page,
                PageSize = pageSize,
                TotalClients = totalClients
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ClientProfile(int id, int page = 1, int pageSize = 10)
        {
            var client = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (client == null) return NotFound();

            var result = await _repository.GetAnnouncementsDynamic(
                title: null,
                categoryIds: null,
                clientId: id,
                dateFrom: null,
                dateTo: null,
                pageNumber: page,
                pageSize: pageSize
            );
            // 🟢 Breadcrumb: Home → Client
            var breadcrumb = new List<BreadcrumbItem>();

            breadcrumb.Add(new BreadcrumbItem
            {
                Id = client.Id,
                Name = client.ClientName ?? "الملف الشخصي للعميل" // "Client Profile" in Arabic
            });

            ViewBag.Breadcrumb = await _breadcrumbService.GetClientBreadcrumbAsync(id);

            var vm = new ClientAnnouncementsViewModel
            {
                Client = client,
                Announcements = result.Announcements,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };

            return View(vm);
        }

        //public async Task<IActionResult> CategoryAnnouncements(int id, int page = 1, int pageSize = 10, List<int> selectedSubcategories = null)
        // {
        //     var category = await _context.Categories
        //         .Include(c => c.SubCategoryMappings)
        //         .ThenInclude(m => m.SubCategory)
        //         .FirstOrDefaultAsync(c => c.Id == id);

        //     if (category == null)
        //         return NotFound();

        //     var subcategories = category.SubCategoryMappings
        //                                 .Select(m => m.SubCategory)
        //                                 .ToList();
        //     // ⭐ Breadcrumb MUST be here!
        //     ViewBag.Breadcrumb = await _breadcrumbService.GetCategoryBreadcrumbAsync(id);

        //     // ⭐ Default: ALL selected
        //     bool isAllSelected =
        //         selectedSubcategories == null ||
        //         selectedSubcategories.Count == 0 ||
        //         selectedSubcategories.Contains(0);

        //     List<int> filterIds = isAllSelected
        //         ? subcategories.Select(s => s.Id).ToList()
        //         : selectedSubcategories;

        //     // Query announcements
        //     var query = _context.AnnouncementCategories
        //         .Where(ac => filterIds.Contains(ac.CategoryId))
        //         .Select(ac => ac.Announcement)
        //         .Distinct();

        //     int totalCount = query.Count();

        //     var announcements = query
        //         .OrderByDescending(a => a.AddedDate)
        //         .Skip((page - 1) * pageSize)
        //         .Take(pageSize)
        //         .Select(a => new AnnouncementDto
        //         {
        //             Id = a.Id,
        //             Title = a.Title,
        //             Description = a.Description,
        //             FilePath = a.FilePath,
        //             AddedDate = a.AddedDate,
        //             ClientId = a.ClientId,
        //             Client = a.Client
        //         })
        //         .ToList();

        //     // Breadcrumb
        //     var breadcrumb = await _breadcrumbService.GetCategoryBreadcrumbAsync(id);

        //     // Build ViewModel
        //     var vm = new CategoryAnnouncementsViewModel
        //     {
        //         IsSubcategoryPage = false,
        //         CategoryId = id,
        //         CategoryName = category.CatName,
        //         IconUrl = category.IconUrl,
        //         Announcements = announcements,
        //         PageNumber = page,
        //         PageSize = pageSize,
        //         TotalCount = totalCount,
        //         Subcategories = subcategories,
        //         SelectedSubcategories = selectedSubcategories ?? new List<int> { 0 },
        //         Breadcrumb = breadcrumb
        //     };

        //     return View(vm);
        // }

        // [HttpPost]
        // public IActionResult CategoryAnnouncementsAjax([FromBody] FilterRequestViewModel req)
        // {
        //     // Load subcategories of the main category
        //     var subcategories = _context.CategoryParentMappings
        //         .Where(cpm => cpm.ParentCategoryId == req.CategoryId)
        //         .Select(cpm => cpm.SubCategory)
        //         .Distinct()
        //         .ToList();

        //     // Default ALL behavior
        //     bool isAllSelected =
        //         req.Subcategories == null ||
        //         req.Subcategories.Count == 0 ||
        //         req.Subcategories.Contains(0);

        //     var filterIds = isAllSelected
        //         ? subcategories.Select(s => s.Id).ToList()
        //         : req.Subcategories;

        //     // Query announcements
        //     var query = _context.AnnouncementCategories
        //         .Where(ac => filterIds.Contains(ac.CategoryId))
        //         .Select(ac => ac.Announcement)
        //         .Distinct()
        //         .AsQueryable();

        //     int totalCount = query.Count();

        //     var announcements = query
        //         .OrderByDescending(a => a.AddedDate)
        //         .Skip((req.Page - 1) * req.PageSize)
        //         .Take(req.PageSize)
        //         .Select(a => new AnnouncementDto
        //         {
        //             Id = a.Id,
        //             Title = a.Title,
        //             Description = a.Description,
        //             FilePath = a.FilePath,
        //             AddedDate = a.AddedDate,
        //             ClientId = a.ClientId,
        //             Client = a.Client
        //         })
        //         .ToList();

        //     // Build VM for partial views
        //     var vm = new CategoryAnnouncementsViewModel
        //     {
        //         IsSubcategoryPage = false,

        //         CategoryId = req.CategoryId,
        //         Announcements = announcements,
        //         Subcategories = subcategories,
        //         SelectedSubcategories = req.Subcategories ?? new List<int> { 0 },
        //         PageNumber = req.Page,
        //         PageSize = req.PageSize,
        //         TotalCount = totalCount
        //     };

        //     // Return the partial HTML blocks
        //     return Json(new
        //     {
        //         announcements = RenderPartial("_AnnouncementsPartial", vm),
        //         pagination = RenderPartial("_PaginationPartial", vm)
        //     });
        // }
        public async Task<IActionResult> CategoryAnnouncements(int id, int page = 1, int pageSize = 10, List<int> selectedSubcategories = null)
        {
            var category = await _context.Categories
                .Include(c => c.SubCategoryMappings)
                .ThenInclude(m => m.SubCategory)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            var subcategories = category.SubCategoryMappings
                                        .Select(m => m.SubCategory)
                                        .ToList();

            ViewBag.Breadcrumb = await _breadcrumbService.GetCategoryBreadcrumbAsync(id);

            bool isAllSelected = selectedSubcategories == null ||
                                 selectedSubcategories.Count == 0 ||
                                 selectedSubcategories.Contains(0);

            List<int>? filterIds = isAllSelected
                ? subcategories.Select(s => s.Id).ToList()
                : selectedSubcategories;

            // ✅ Repository بدل EF مباشر
            var result = await _repository.GetAnnouncementsDynamic(
                title: null,
                categoryIds: filterIds,
                clientId: null,
                dateFrom: null,
                dateTo: null,
                pageNumber: page,
                pageSize: pageSize
            );

            var breadcrumb = await _breadcrumbService.GetCategoryBreadcrumbAsync(id);

            var vm = new CategoryAnnouncementsViewModel
            {
                IsSubcategoryPage = false,
                CategoryId = id,
                CategoryName = category.CatName,
                IconUrl = category.IconUrl,
                Announcements = result.Announcements,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                Subcategories = subcategories,
                SelectedSubcategories = selectedSubcategories ?? new List<int> { 0 },
                Breadcrumb = breadcrumb
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> CategoryAnnouncementsAjax([FromBody] FilterRequestViewModel req)
        {
            bool isAllSelected = req.Subcategories == null ||
                                 req.Subcategories.Count == 0 ||
                                 req.Subcategories.Contains(0);

            // ✅ إذا "عرض الكل" → جيب كل الـ subcategories من الـ DB
            List<int>? filterIds = isAllSelected
                ? await _context.CategoryParentMappings
                    .Where(cpm => cpm.ParentCategoryId == req.CategoryId)
                    .Select(cpm => cpm.SubCategoryId)
                    .ToListAsync()
                : req.Subcategories;

            // ✅ Repository بدل EF مباشر
            var result = await _repository.GetAnnouncementsDynamic(
                title: null,
                categoryIds: filterIds,
                clientId: null,
                dateFrom: null,
                dateTo: null,
                pageNumber: req.Page,
                pageSize: req.PageSize
            );

            return Json(new
            {
                announcements = RenderPartial("_AnnouncementsPartial", result),
                pagination = RenderPartial("_PaginationPartial", result)
            });
        }


        //public async Task<IActionResult> SubCategoryAnnouncements(int id, int page = 1, int pageSize = 10)
        //{
        //    // Load subcategory itself
        //    var subcategory = await _context.Categories
        //        .Include(c => c.ParentMappings)
        //        .FirstOrDefaultAsync(c => c.Id == id);

        //    if (subcategory == null)
        //        return NotFound();

        //    // Get parent category
        //    var parentId = subcategory.ParentMappings.FirstOrDefault()?.ParentCategoryId;

        //    if (parentId == null)
        //        return NotFound("This subcategory has no parent category.");

        //    // Load ALL Subcategories under same parent
        //    var siblingSubcategories = _context.CategoryParentMappings
        //        .Where(x => x.ParentCategoryId == parentId)
        //        .Select(x => x.SubCategory)
        //        .ToList();

        //    // Prepare default filter
        //    List<int> filterIds = new List<int> { id };

        //    // Query announcements
        //    var query = _context.AnnouncementCategories
        //        .Where(ac => filterIds.Contains(ac.CategoryId))
        //        .Select(ac => ac.Announcement)
        //        .Distinct();

        //    int totalCount = query.Count();

        //    var announcements = query
        //        .OrderByDescending(a => a.AddedDate)
        //        .Skip((page - 1) * pageSize)
        //        .Take(pageSize)
        //        .Select(a => new AnnouncementDto
        //        {
        //            Id = a.Id,
        //            Title = a.Title,
        //            Description = a.Description,
        //            FilePath = a.FilePath,
        //            AddedDate = a.AddedDate,
        //            ClientId = a.ClientId,
        //            Client = a.Client
        //        })
        //        .ToList();

        //    // Breadcrumb
        //    var breadcrumb = await _breadcrumbService.GetCategoryBreadcrumbAsync(id);
        //    ViewBag.Breadcrumb = await _breadcrumbService.GetCategoryBreadcrumbAsync(id);


        //    // Build VM
        //    var vm = new CategoryAnnouncementsViewModel
        //    {
        //        IsSubcategoryPage = true,
        //        ParentCategoryId = parentId,
        //        CategoryId = id,
        //        CategoryName = subcategory.CatName,
        //        IconUrl = subcategory.IconUrl,
        //        Breadcrumb = breadcrumb,
        //        Subcategories = siblingSubcategories,
        //        SelectedSubcategories = new List<int> { id },
        //        Announcements = announcements,
        //        PageNumber = page,
        //        PageSize = pageSize,
        //        TotalCount = totalCount
        //    };

        //    return View("CategoryAnnouncements", vm);
        //}
        //[HttpPost]
        //public IActionResult SubCategoryFilterAjax([FromBody] FilterRequestViewModel req)
        //{
        //    // Load siblings
        //    var subcategories = _context.CategoryParentMappings
        //        .Where(cpm => cpm.ParentCategoryId == req.CategoryId) // Parent
        //        .Select(cpm => cpm.SubCategory)
        //        .ToList();

        //    // Filter logic
        //    bool all = req.Subcategories == null || req.Subcategories.Count == 0;

        //    var filterIds = all
        //        ? subcategories.Select(s => s.Id).ToList()
        //        : req.Subcategories;

        //    var query = _context.AnnouncementCategories
        //        .Where(ac => filterIds.Contains(ac.CategoryId))
        //        .Select(ac => ac.Announcement)
        //        .Distinct();

        //    int totalCount = query.Count();

        //    var announcements = query
        //        .OrderByDescending(a => a.AddedDate)
        //        .Skip((req.Page - 1) * req.PageSize)
        //        .Take(req.PageSize)
        //        .Select(a => new AnnouncementDto
        //        {
        //            Id = a.Id,
        //            Title = a.Title,
        //            Description = a.Description,
        //            FilePath = a.FilePath,
        //            AddedDate = a.AddedDate
        //        })
        //        .ToList();

        //    var vm = new CategoryAnnouncementsViewModel
        //    {
        //        CategoryId = req.CategoryId,
        //        Announcements = announcements,
        //        PageNumber = req.Page,
        //        PageSize = req.PageSize,
        //        TotalCount = totalCount,
        //        Subcategories = subcategories,
        //        SelectedSubcategories = req.Subcategories
        //    };

        //    return Json(new
        //    {
        //        announcements = RenderPartial("_AnnouncementsPartial", vm),
        //        pagination = RenderPartial("_PaginationPartial", vm)
        //    });
        //}
        public async Task<IActionResult> SubCategoryAnnouncements(int id, int page = 1, int pageSize = 10)
        {
            var subcategory = await _context.Categories
                .Include(c => c.ParentMappings)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (subcategory == null)
                return NotFound();

            var parentId = subcategory.ParentMappings.FirstOrDefault()?.ParentCategoryId;
            if (parentId == null)
                return NotFound("This subcategory has no parent category.");

            // Load sibling subcategories
            var siblingSubcategories = await _context.CategoryParentMappings
                .Where(x => x.ParentCategoryId == parentId)
                .Select(x => x.SubCategory)
                .ToListAsync();

            // ✅ Repository بدل EF مباشر
            var result = await _repository.GetAnnouncementsDynamic(
                title: null,
                categoryIds: new List<int> { id },
                clientId: null,
                dateFrom: null,
                dateTo: null,
                pageNumber: page,
                pageSize: pageSize
            );

            var breadcrumb = await _breadcrumbService.GetCategoryBreadcrumbAsync(id);
            ViewBag.Breadcrumb = breadcrumb;

            var vm = new CategoryAnnouncementsViewModel
            {
                IsSubcategoryPage = true,
                ParentCategoryId = parentId,
                CategoryId = id,
                CategoryName = subcategory.CatName,
                IconUrl = subcategory.IconUrl,
                Breadcrumb = breadcrumb,
                Subcategories = siblingSubcategories,
                SelectedSubcategories = new List<int> { id },
                Announcements = result.Announcements,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount
            };

            return View("CategoryAnnouncements", vm);
        }

        [HttpPost]
        public async Task<IActionResult> SubCategoryFilterAjax([FromBody] FilterRequestViewModel req)
        {
            // Load siblings
            var subcategories = await _context.CategoryParentMappings
                .Where(cpm => cpm.ParentCategoryId == req.CategoryId)
                .Select(cpm => cpm.SubCategory)
                .ToListAsync();

            bool all = req.Subcategories == null || req.Subcategories.Count == 0;

            // ✅ إذا "عرض الكل" → كل الـ siblings
            List<int> filterIds = all
                ? subcategories.Select(s => s.Id).ToList()
                : req.Subcategories;

            // ✅ Repository بدل EF مباشر
            var result = await _repository.GetAnnouncementsDynamic(
                title: null,
                categoryIds: filterIds,
                clientId: null,
                dateFrom: null,
                dateTo: null,
                pageNumber: req.Page,
                pageSize: req.PageSize
            );

            return Json(new
            {
                announcements = RenderPartial("_AnnouncementsPartial", result),
                pagination = RenderPartial("_PaginationPartial", result)
            });
        }
        public async Task<IActionResult> Details(int id, int? clientId = null)
        {
            var announcement = await _context.Announcements
                .Include(a => a.Client)
                .Include(a => a.AnnouncementImages) // 🔥 مهم
                .Include(a => a.AnnouncementCategories)
                    .ThenInclude(ac => ac.Category)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (announcement == null)
                return NotFound();

            var breadcrumb = new List<BreadcrumbItem>();
            

            if (clientId.HasValue && announcement.Client != null)
            {
                // Add client
                breadcrumb.Add(new BreadcrumbItem { Id = announcement.Client.Id, Name = announcement.Client.ClientName });

                // If the announcement belongs to a category
                var firstCategory = announcement.AnnouncementCategories.FirstOrDefault()?.CategoryId;
                if (firstCategory.HasValue)
                {
                    var catTrail = await _breadcrumbService.GetCategoryBreadcrumbAsync(firstCategory.Value);
                    breadcrumb.AddRange(catTrail.Skip(1)); // skip "الرئيسية"
                }
            }
            else
            {
                // Pure category path
                var firstCategory = announcement.AnnouncementCategories.FirstOrDefault()?.CategoryId;
                if (firstCategory.HasValue)
                {
                    var catTrail = await _breadcrumbService.GetCategoryBreadcrumbAsync(firstCategory.Value);
                    breadcrumb.AddRange(catTrail);
                }
            }

            // Always add announcement title at the end
            breadcrumb.Add(new BreadcrumbItem { Id = announcement.Id, Name = announcement.Title });
            ViewBag.Breadcrumb = breadcrumb;

            var vm = new AnnouncementViewModel
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Description = announcement.Description,
                FilePath = announcement.FilePath,
                AddedDate = announcement.AddedDate,
                ClientId = announcement.ClientId,
                Client = announcement.Client,
                ImageUrls = announcement.AnnouncementImages?
                            .Select(i => i.ImageUrl)
                            .ToList() ?? new List<string>()
            };

            return View(vm);
        }

        [Route("Hub/Page/{id}")]
        public async Task<IActionResult> Page(int id)
        {
            var page = await _context.Pages
                .Include(p => p.pageCategory)
                .FirstOrDefaultAsync(p => p.Id == id && p.Active);

            if (page == null) return NotFound();

            // صفحات من نفس التصنيف
            var relatedPages = await _context.Pages
                .Where(p => p.PageCategoryId == page.PageCategoryId
                         && p.Active
                         && p.Id != id)
                .OrderBy(p => p.Ordering)
                .ToListAsync();

            ViewBag.RelatedPages = relatedPages;

            return View(page);
        }


    }
}

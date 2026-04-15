using AnnouncmentHub.Data;
using AnnouncmentHub.Models;
using AnnouncmentHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnnouncmentHub.Areas.Admin.Controllers
{
    [Area("Admin")]
   // [Authorize(Roles = "Admin,NormalUser")]
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<string> SaveFileAsync(string operation, IFormFile file, string oldPathInEdit)
        {
            try
            {
                // ✅ Define the uploads folder inside "wwwroot/uploads/YYYY/MM" (organized by year/month)
                var currentYear = DateTime.Now.Year.ToString();
                var currentMonth = DateTime.Now.Month.ToString("D2"); // Numeric month (01, 02, ..., 12)
                //var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", currentYear, currentMonth);
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads","clientslogo");

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
                return $"/uploads/clientslogo/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                // ✅ Log the error (you can integrate with a logging framework like Serilog or NLog)
                //Console.WriteLine($"Error saving file: {ex.Message}");
                return string.Empty;  // Return empty string if file save fails
            }
        }

        // GET: Clients
        //public async Task<IActionResult> Index()
        //{
        //    var clients = await _context.Clients.Select(c=>new ClientViewModel
        //    {
        //        Id=c.Id,
        //        ClientName=c.ClientName,
        //        LogoUrl=c.LogoUrl,
        //        IsActive=c.IsActive
        //    }).ToListAsync();
        //    return View(clients);
        //}
        public async Task<IActionResult> Index(
                                                string? searchName,
                                                bool? isActive,
                                                bool? isVIP,
                                                bool? isClosed,
                                                int pageNumber = 1,
                                                int pageSize = 10)
        {
            var query = _context.Clients.AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
                query = query.Where(c => c.ClientName.Contains(searchName));

            if (isActive.HasValue)
                query = query.Where(c => c.IsActive == isActive.Value);

            if (isVIP.HasValue)
                query = query.Where(c => c.IsVIP == isVIP.Value);

            if (isClosed.HasValue)
                query = query.Where(c => c.IsClosed == isClosed.Value);

            int totalClients = await query.CountAsync();

            var clients = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new ClientViewModel
            {
                Clients = clients,
                TotalClients = totalClients,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return View(viewModel);
        }
        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var client = await _context.Clients.Select(c => new ClientViewModel
            {
                Id = c.Id,
                ClientName = c.ClientName,
                CoverImageUrl = c.CoverImageUrl,
                LogoUrl = c.LogoUrl,
                IsActive = c.IsActive,
                IsVIP = c.IsVIP,
                IsClosed = c.IsClosed,
                WhatsUp = c.WhatsUp,
                FacebookLink = c.FacebookLink,
                SiteUrl = c.SiteUrl,
                MobileNumber = c.MobileNumber,
                OpenFrom = c.OpenFrom,
                OpenTo = c.OpenTo,
                Latitude = c.Latitude,
                Longitude = c.Longitude
            }).FirstOrDefaultAsync(m => m.Id == id);

            if (client == null) return NotFound();

            return View(client);
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            return View(new ClientViewModel());
        }

           // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // لمعرفة سبب الخطأ
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }

                return View(model);
            }
            if (ModelState.IsValid)
            {

                string? logoUrl = null;
                string? coverUrl = null;

                // ✅ Handle logo upload
                if (model.LogoFile != null && model.LogoFile.Length > 0)
                {
                    logoUrl = await SaveFileAsync("Create", model.LogoFile, null);
                }
                else if (!string.IsNullOrEmpty(model.LogoUrl))
                {
                    logoUrl = model.LogoUrl;
                }

                // ✅ Handle cover upload
                if (model.CoverImageFile != null && model.CoverImageFile.Length > 0)
                {
                    // here you might want a separate folder e.g. "clientscovers"
                    coverUrl = await SaveFileAsync("Create", model.CoverImageFile, null);
                }
                else if (!string.IsNullOrEmpty(model.CoverImageUrl))
                {
                    coverUrl = model.CoverImageUrl;
                }

                // ✅ Create a new client
                var client = new Client
                {
                    ClientName = model.ClientName,
                    WhatsUp = model.WhatsUp,
                    FacebookLink = model.FacebookLink,
                    SiteUrl = model.SiteUrl,
                    MobileNumber = model.MobileNumber,
                    LogoUrl = logoUrl,
                    CoverImageUrl = coverUrl,
                    IsActive = model.IsActive,
                    IsVIP = model.IsVIP,        // ✅ added
                    IsClosed = model.IsClosed,     // ✅ added
                    OpenFrom = model.OpenFrom,     // ✅ added
                    OpenTo = model.OpenTo,       // ✅ added
                    Latitude = model.Latitude,     // ✅ added
                    Longitude = model.Longitude     // ✅ added
                };

                await _context.Clients.AddAsync(client);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم إضافة العميل بنجاح ✅";
                return RedirectToAction(nameof(Index));

         
            }

            // Return the view with validation errors
            return View(model);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();

            var model = new ClientViewModel
            {
                Id = client.Id,
                ClientName = client.ClientName,
                WhatsUp = client.WhatsUp,
                SiteUrl = client.SiteUrl,
                MobileNumber = client.MobileNumber,
                FacebookLink = client.FacebookLink,
                IsActive = client.IsActive,
                IsVIP = client.IsVIP,
                IsClosed = client.IsClosed,
                OpenFrom = client.OpenFrom,
                OpenTo = client.OpenTo,
                Latitude = client.Latitude,
                Longitude = client.Longitude,
                LogoUrl = client.LogoUrl,
                CoverImageUrl = client.CoverImageUrl
            };

            return View(model);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClientViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var client = await _context.Clients.FindAsync(id);
                if (client == null) return NotFound();

                // ✅ Handle logo
                string logoUrl = client.LogoUrl;
                if (model.LogoFile != null && model.LogoFile.Length > 0)
                    logoUrl = await SaveFileAsync("Edit", model.LogoFile, client.LogoUrl);
                else if (!string.IsNullOrEmpty(model.LogoUrl))
                    logoUrl = model.LogoUrl;

                // ✅ Handle cover
                string coverUrl = client.CoverImageUrl;
                if (model.CoverImageFile != null && model.CoverImageFile.Length > 0)
                    coverUrl = await SaveFileAsync("Edit", model.CoverImageFile, client.CoverImageUrl);
                else if (!string.IsNullOrEmpty(model.CoverImageUrl))
                    coverUrl = model.CoverImageUrl;

                // ✅ Update all fields
                client.ClientName = model.ClientName;
                client.WhatsUp = model.WhatsUp;
                client.FacebookLink = model.FacebookLink;
                client.SiteUrl = model.SiteUrl;
                client.MobileNumber = model.MobileNumber;
                client.IsActive = model.IsActive;
                client.IsVIP = model.IsVIP;
                client.IsClosed = model.IsClosed;
                client.OpenFrom = model.OpenFrom;
                client.OpenTo = model.OpenTo;
                client.Latitude = model.Latitude;
                client.Longitude = model.Longitude;
                client.LogoUrl = logoUrl;
                client.CoverImageUrl = coverUrl;

                try
                {
                    _context.Update(client);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "تم تعديل بيانات العميل بنجاح ✅";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Announcements)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
            {
                return NotFound();
            }

            // 🔍 Check if client has announcements
            if (client.Announcements.Any())
            {
                TempData["Error"] = "عذرًا، لا يمكن حذف هذا العميل لأنه مرتبط بإعلانات موجودة.";
                return RedirectToAction(nameof(Index));
            }

            // If logo exists, delete file
            if (!string.IsNullOrEmpty(client.LogoUrl))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", client.LogoUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }




        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }
        // Method to check if the client exists (for concurrency)
      
    }
}


using AnnouncmentHub.Data;
using Microsoft.AspNetCore.Mvc;

namespace AnnouncmentHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var AdsCounter = _context.Announcements.Count();
            var ClientCounter = _context.Clients.Count();
            var CategoryCounter = _context.Categories.Where(c => c.IsParent == true).Count();
            var SubCategoryCounter = _context.Categories.Where(c => c.IsParent == false).Count();
            var PagesCounter = _context.Pages.Count();
            ViewBag.AdsCount = AdsCounter;
            ViewBag.ClientCount = ClientCounter;
            ViewBag.CategoryCount = CategoryCounter;
            ViewBag.SubCategoryCount = SubCategoryCounter;
            ViewBag.pagesCount = PagesCounter;
            // 🚧 Skip authentication for now
            ViewData["Title"] = "لوحة التحكم";
            return View();
        }
    }
}

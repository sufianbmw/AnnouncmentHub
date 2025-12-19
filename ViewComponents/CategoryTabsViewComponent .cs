using AnnouncmentHub.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnnouncmentHub.ViewComponents
{
    public class CategoryTabsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public CategoryTabsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Fetch main categories along with their subcategories
            var categories = await _context.Categories
                .Include(c => c.SubCategoryMappings)
                .ThenInclude(m => m.SubCategory)
                .Where(c => c.IsParent)
                .AsNoTracking()
                .ToListAsync();

            // Return the partial view with the categories data
            return View("~/Views/Shared/_CategoryTabsPartial.cshtml", categories);
        }
    }
}

//using AnnouncmentHub.Data;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace AnnouncmentHub.ViewComponents
//{
//    public class CategoryTabsViewComponent : ViewComponent
//    {
//        private readonly ApplicationDbContext _context;

//        public CategoryTabsViewComponent(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<IViewComponentResult> InvokeAsync()
//        {
//            Console.WriteLine("🚀 CategoryTabsViewComponent Invoked!");

//            // ✅ نحضر الفئات الرئيسية فقط
//            var categories = await _context.Categories
//                .Include(c => c.SubCategoryMappings)
//                .ThenInclude(m => m.SubCategory)
//                .Where(c => c.IsParent)
//                .AsNoTracking()
//                .ToListAsync();

//            return View("~/Views/Shared/_CategoryTabsPartial.cshtml", categories);
//        }
//    }
//}

using AnnouncmentHub.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnnouncmentHub.ViewComponents
{
    public class NavCategoryStripViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public NavCategoryStripViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var vipCategories = await _context.Categories
                .Where(c => !c.IsParent && c.IsVIP)
                .OrderBy(c => c.CatName)
                .AsNoTracking()
                .ToListAsync();

            return View(vipCategories);
        }
    }
}

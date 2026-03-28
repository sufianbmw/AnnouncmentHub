using AnnouncmentHub.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnnouncmentHub.ViewComponents
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public FooterViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var pages = await _context.Pages
                .Where(p => p.Active)
                .OrderBy(p => p.Ordering)
                .ToListAsync();

            return View(pages);
        }
    }
}
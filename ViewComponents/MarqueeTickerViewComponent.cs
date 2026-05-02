using AnnouncmentHub.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnnouncmentHub.ViewComponents
{
    public class MarqueeTickerViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public MarqueeTickerViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var vipAnnouncements = await _context.Announcements
                .Where(a => a.IsActive && a.IsVIP)
                .OrderByDescending(a => a.AddedDate)
                .Take(20)
                .Select(a => new { a.Id, a.Title })
                .AsNoTracking()
                .ToListAsync();

            return View(vipAnnouncements.Select(a => new AnnouncmentHub.ViewModels.CategoryDto
            {
                Id = a.Id,
                Name = a.Title
            }).ToList());
        }
    }
}

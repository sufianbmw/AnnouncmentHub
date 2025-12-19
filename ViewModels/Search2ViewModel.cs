using AnnouncmentHub.Models;

namespace AnnouncmentHub.ViewModels
{
    public class Search2ViewModel
    {
        // Filters
        public string? Title { get; set; }
        public int? MainCategoryId { get; set; }
        public List<int>? SubCategoryIds { get; set; }
        public int? ClientId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        // Pagination
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Data
        public List<Category>? MainCategories { get; set; }
        public List<Category>? SubCategories { get; set; }
        public List<AnnouncementDto>? Announcements { get; set; }
    }
}

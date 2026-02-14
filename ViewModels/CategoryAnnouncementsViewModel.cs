using AnnouncmentHub.Models;

namespace AnnouncmentHub.ViewModels
{
    public class CategoryAnnouncementsViewModel
    {
        // 🔹 Is this page a SubCategory Page?
        public bool IsSubcategoryPage { get; set; } = false;

        // 🔹 Parent Category (only for subcategory pages)
        public int? ParentCategoryId { get; set; }
        // 🔹 معلومات التصنيف
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string? IconUrl { get; set; }


        // 🔹 Breadcrumb (مسار التنقل)
        public List<BreadcrumbItem> Breadcrumb { get; set; } = new();

        // 🔹 قائمة الإعلانات
        public List<AnnouncementDto> Announcements { get; set; } = new();

        // 🔹 Subcategories (to filter announcements)
        public List<Category> Subcategories { get; set; } = new();

        // 🔹 Selected Subcategories (IDs for filtering)
        public List<int> SelectedSubcategories { get; set; } = new();
        // 🔹 Pagination

        public List<string>? ImageUrls { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages =>
            (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}

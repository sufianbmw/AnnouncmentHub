using AnnouncmentHub.Models;
using System.ComponentModel.DataAnnotations;

namespace AnnouncmentHub.ViewModels
{
    public class AnnouncementViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "العنوان حقل أجباري.")]
        [MaxLength(250)]
        [Display(Name = "العنوان")]
        public string Title { get; set; }

        [Display(Name = "الوصف")]
        public string Description { get; set; }

        [Display(Name = "مفعل")]
        public bool IsActive { get; set; } = true; // Default to active


        // Add this list to capture selected subcategories
        [Display(Name = "التصنيفات الفرعية")]
        public List<int> SelectedSubCategories { get; set; } = new List<int>(); // List to hold selected subcategory IDs

        [Display(Name = "رفع ملف")]
        //[Required(ErrorMessage = "الملف  أجباري لادخال الاعلان.")]
        public IFormFile? File { get; set; } // File upload

        [Display(Name = "المرفق")]
        public string? FilePath { get; set; } // Path to PDF or image
        public string? ThumbnailPath { get; set; }


        [Display(Name = "التاريخ")]
        public DateTime AddedDate { get; set; } = DateTime.UtcNow; // Set to current date/time by default
                                                                   // Add ClientId for reference
        [Display(Name = "العميل")]
        public int? ClientId { get; set; }  // Reference to Client ID (optional)
        public Client? Client { get; set; }

        //[Display(Name = "اسم العميل")]
        //public string? ClientName { get; set; }  // Optional: To display the name of the client if needed

        // Add DateFrom and DateTo for validity period
        [Display(Name = "تاريخ البداية")]
        public DateTime? DateFrom { get; set; }  // Nullable DateTime

        [Display(Name = "تاريخ الانتهاء")]
        public DateTime? DateTo { get; set; }  // Nullable DateTime

        [Display(Name = "التصنيفات الفرعية")]
        public List<string> SubCategoryNames { get; set; } = new List<string>();

        [Display(Name = "الأصناف الرئيسية")]
        public List<string> ParentCategoryNames { get; set; } = new List<string>();


        // Add the parent categories for each subcategory
        public Dictionary<int, List<string>> SubCategoryParents { get; set; } = new Dictionary<int, List<string>>();
        // ✅ بدال ما نخزن JSON string بس
        // نخلي كمان قائمة جاهزة للتعامل معها في الـ View
        public string? CategoriesJson { get; set; }

        [Display(Name = "التصنيفات")]
        public List<CategoryJsonLink> Categories { get; set; } = new();

    }
}




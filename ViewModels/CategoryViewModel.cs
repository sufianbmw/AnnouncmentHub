using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AnnouncmentHub.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "التصنيف حقل أجباري.")]
        [MaxLength(250)]
        [Display(Name = "التصنيف")]
        public string? CatName { get; set; }
        public string? ParentCategoryNames { get; set; }

        [Display(Name = "يتبع للتصنيف")]
        public List<int> ParentCategoryIds { get; set; } = new List<int>(); // Changed from single int? ParentId to list
       
        [Display(Name = "ايقونة التصنيف")]
        public string? IconUrl { get; set; }

        [Display(Name = "رفع الأيقونة")]
        public IFormFile? IconFile { get; set; }  // 👈 new

        [Display(Name = "تصنيف رئيسي")]
        public bool IsParent { get; set; }

        [Display(Name = "مفعل")]
        public bool IsActive { get; set; }

        [Display(Name = "التصنيف VIP")]
        public bool IsVIP { get; set; }
        // Remove ParentCategoryName property, or keep it if you want to show something in edit view (optional)

        // For dropdown list of parent categories
        //public IEnumerable<Category> ParentCategories { get; set; } = new List<Category>();
        public IEnumerable<SelectListItem> ParentCategories { get; set; } = new List<SelectListItem>();

    }

}

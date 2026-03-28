using AnnouncmentHub.Models;
using System.ComponentModel.DataAnnotations;
namespace AnnouncmentHub.ViewModels
{
    public class PageViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان الصفحة حقل أجباري."), MaxLength(500)]
        [Display(Name = "عنوان الصفحة")]
        public string PageTitle { get; set; }
        [Required(ErrorMessage = "التفاصيل حقل أجباري.")]
        [Display(Name = "التفاصيل")]
        public string PageDetails { get; set; }
        [Display(Name = "مفعل ؟")]
        public bool Active { get; set; } = false;
        [Display(Name = "الترتيب")]
        public int? Ordering { get; set; } = -1;

        [Display(Name = "التصنيف")]
        public int PageCategoryId { get; set; }
        [Display(Name = "ضمن التصنيف")]
        public virtual PageCategory? PageCategory { get; set; }
    }
}

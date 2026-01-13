using AnnouncmentHub.Models;
using System.ComponentModel.DataAnnotations;

namespace AnnouncmentHub.ViewModels
{
    public class ClientViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم العميل حقل أجباري.")]
        [MaxLength(250)]
        [Display(Name = "اسم العميل")]
        public string ClientName { get; set; }

        [Display(Name = "رابط الفيسبوك")]
       public string? FacebookLink { get; set; }

        [Display(Name = "رابط الواتس اب")]
        public string? WhatsUp { get; set; }

        [Display(Name = "رقم الهاتف")]
        public string? MobileNumber { get; set; }

        [Display(Name = "رابط الموقع الالكتروني")]
        public string? SiteUrl { get; set; }
        [Display(Name = "مفعل")]
        public bool IsActive { get; set; } = true; // Default to active

        [Display(Name = "صورة الشعار")]
        public IFormFile? LogoFile { get; set; }  // This will handle file upload

        [Display(Name = "صورة الشعار")]
        //[Url(ErrorMessage = "الرجاء إدخال رابط صحيح لصورة الشعار.")]
        public string? LogoUrl { get; set; }  // For the user to provide a URL

        [Display(Name = "صورة الغلاف")]
        public IFormFile? CoverImageFile { get; set; }

        [Display(Name = "رابط صورة الغلاف")]
        public string? CoverImageUrl { get; set; }

        // Optional: If you want to include related Announcements in the ViewModel, you can keep this
        public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalClients { get; set; }
        public List<Client>? Clients { get; set; }
    }
}

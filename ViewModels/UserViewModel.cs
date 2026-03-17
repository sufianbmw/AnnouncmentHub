using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AnnouncmentHub.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "الاسم الأول حقل إجباري.")]
        [Display(Name = "الاسم الأول")]
        public string FName { get; set; }

        [Required(ErrorMessage = "الاسم الأخير حقل إجباري.")]
        [Display(Name = "الاسم الأخير")]
        public string LName { get; set; }

        [Required(ErrorMessage = "اسم المستخدم حقل إجباري.")]
        [Display(Name = "اسم المستخدم")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني حقل إجباري.")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة.")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [Display(Name = "حالة الحساب")]
        public bool UserStatus { get; set; }

        [Display(Name = "الأدوار")]
        public List<string>? Roles { get; set; } = new();
    }
}

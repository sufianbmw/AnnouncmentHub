using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AnnouncmentHub.ViewModels
{
    public class ProfileFormViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "الاسم الاول حقل أجباري.")]
        [StringLength(100, ErrorMessage = "يجب أن يكون {0} بين {2} و {1} أحرف.", MinimumLength = 3)]
        [Display(Name = "الاسم الاول")]
        public string FName { get; set; } = null!;

        [Required(ErrorMessage = "الاسم الاخير حقل أجباري.")]
        [StringLength(100, ErrorMessage = "يجب أن يكون {0} بين {2} و {1} أحرف.", MinimumLength = 3)]
        [Display(Name = "الاسم الاخير")]
        public string LName { get; set; } = null!;

        [Required(ErrorMessage = "البريد الالكتروني حقل أجباري.")]
        [EmailAddress(ErrorMessage = "صيغة البريد الالكتروني غير صحيحة")]
        [Display(Name = "البريد الالكتروني")]
        public string Email { get; set; }

        [Required(ErrorMessage = "اسم المستخدم حقل أجباري.")]
        [StringLength(100, ErrorMessage = "يجب أن يكون {0} بين {2} و {1} أحرف.", MinimumLength = 3)]
        [Display(Name = "اسم المستخدم")]
        public string UserName { get; set; } = null!;

        [Display(Name = "حالة الحساب / مفعل ؟")]
        public bool UserStatus { get; set; } = false;

        public List<EditRoleViewModel> Roles { get; set; } = new();

        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور الجديدة")]
        [StringLength(100, ErrorMessage = "كلمة المرور يجب أن تكون على الأقل 6 خانات.", MinimumLength = 6)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة المرور")]
        [Compare("NewPassword", ErrorMessage = "تأكيد كلمة المرور غير متطابق.")]
        public string? ConfirmPassword { get; set; }
    }
}

//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Linq;
//using System.Threading.Tasks;

//namespace AnnouncmentHub.ViewModels
//{
//    public class ProfileFormViewModel
//    {
//        public string Id  { get; set; }
//        [Required(ErrorMessage = "الاسم الاول حقل أجباري.")]

//        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
//        [Display(Name = "الاسم الاول")]
//        public string FName { get; set; } = null!;

//        [Required(ErrorMessage = "الاسم الاخير حقل أجباري.")]

//        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
//        [Display(Name = "الاسم الاخير")]
//        public string LName { get; set; } = null!;

//        [Required(ErrorMessage = " البريد الالكتروني حقل أجباري.")]
//        [EmailAddress(ErrorMessage = "صيعة البريد الالكتروني غير صحيحة")]
//        [Display(Name = "البريد الالكتروني")]
//        public string Email { get; set; }


//        [Required(ErrorMessage = " اسم المستخدم حقل أجباري.")]
//        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
//        [Display(Name = "اسم المستخدم")]
//        public string UserName { get; set; } = null!;

//        [Display(Name = "حالة الحساب / مفعل ؟")]
//        public bool UserStatus { get; set; } = false;
//        public List<EditRoleViewModel> Roles { get; set; } = new();


//    }
//}

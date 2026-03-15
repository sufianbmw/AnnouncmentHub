using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AnnouncmentHub.ViewModels
{
    public class AddUserViewModel
    {
        [Required(ErrorMessage = "الاسم الاول حقل أجباري.")]

        [StringLength(100, ErrorMessage = " {0} يجب على الاقل ان يكون {2} وعلى الاكثر {1} الحروف.", MinimumLength = 3)]
        [Display(Name = "الاسم الاول")]
        public string FName { get; set; }

        [Required(ErrorMessage = "الاسم الاخير حقل أجباري.")]

        [StringLength(100, ErrorMessage = " {0} يجب على الاقل ان يكون {2} وعلى الاكثر {1} الحروف.", MinimumLength = 3)]
        [Display(Name = "الاسم الاخير")]
        public string LName { get; set; }

        [Required(ErrorMessage = " البريد الالكتروني حقل أجباري.")]
        [EmailAddress(ErrorMessage = "صيعة البريد الالكتروني غير صحيحة")]
        [Display(Name = "البريد الالكتروني")]
        public string Email { get; set; }


        [Required(ErrorMessage = " اسم المستخدم حقل أجباري.")]
        [StringLength(100, ErrorMessage = " {0} يجب على الاقل ان يكون {2} وعلى الاكثر {1} الحروف.", MinimumLength = 3)]
   
        [Display(Name = "اسم المستخدم")]
        public string UserName { get; set; }

        [Display(Name = "حالة الحساب")]
        public bool UserStatus { get; set; } 
        
        [Required(ErrorMessage = "كلمة المرور حقل أجباري.")]
        [StringLength(100, ErrorMessage = " {0} يجب على الاقل ان يكون {2} وعلى الاكثر {1} الحروف.", MinimumLength = 3)]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تاكيد كلمة المرور")]
        [Compare("Password", ErrorMessage = "حقل كلمة المرور وحقل تاكيد كلمة المرور غير متطابق!.")]
        public string? ConfirmPassword { get; set; }
        public List<RoleViewModel> Roles { get; set; }



    }
}

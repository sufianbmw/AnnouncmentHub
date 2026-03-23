using System.ComponentModel.DataAnnotations;

namespace AnnouncmentHub.ViewModels
{
 public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "كلمة المرور الحالية مطلوبة")]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور الحالية")]
    public string CurrentPassword { get; set; }

    [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور يجب أن تكون 6 خانات على الأقل")]
    [Display(Name = "كلمة المرور الجديدة")]
    public string NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "تأكيد كلمة المرور غير متطابق")]
    [Display(Name = "تأكيد كلمة المرور")]
    public string ConfirmPassword { get; set; }
}
}



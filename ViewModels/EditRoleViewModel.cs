using System.ComponentModel.DataAnnotations;

namespace AnnouncmentHub.ViewModels
{
    public class EditRoleViewModel
    {
        public string RoleId { get; set; }
        [Display(Name = "اسم الدور/الصلاحية بالانجليزيه")]
        public string RoleName { get; set; }
        [Display(Name = "الاسم المعروض")]
        public string DisplayName { get; set; }
        public bool IsSelected { get; set; }
    }
}

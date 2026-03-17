using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AnnouncmentHub.ViewModels
{
    public class RoleFormViewModel
    {
     

        [Required(ErrorMessage ="اسم الدور/الصلاحية حقل أجباري"),MaxLength(256)]
        [Display(Name ="اسم الدور/الصلاحية بالانجليزيه")]
        public string Name { get; set; }

        [Required(ErrorMessage ="الاسم المعروض للدور حقل أجباري"), MaxLength(256)]
        [Display(Name="الاسم المعروض")]
        public string DisplayName { get; set; }
    }
}

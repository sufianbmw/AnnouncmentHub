using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AnnouncmentHub.Models
{
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string name, string description) : base(name)
        {
            this.DisplayName = description;
        }

        [Required(ErrorMessage = "وصف الصلاحية حقل أجباري!"), MaxLength(100)]
        [Display(Name = "وصف الصلاحية")]
        public virtual string DisplayName { get; set; }

    }
}

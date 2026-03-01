using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AnnouncmentHub.Models
{
    public class ApplicationUser : IdentityUser
    {

        [Display(Name = "First Name")]
        public string FName { get; set; }
        [Display(Name = "Last Name")]
        public string LName { get; set; }
        public bool UserStatus { get; set; }
        [Display(Name = "Profile Picture")]
        public byte[]? ProfilePic { get; set; }


    }
}

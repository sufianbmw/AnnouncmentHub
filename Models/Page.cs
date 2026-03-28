using System.ComponentModel.DataAnnotations;

namespace AnnouncmentHub.Models
{
    public partial class Page : EntityBase
    {
        [Required, MaxLength(500)]
        public string PageTitle { get; set; }
        [Required]
        public string PageDetails { get; set; }
        public bool Active { get; set; } = false;
        public int? Ordering { get; set; } = -1;
        public int PageCategoryId { get; set; }
        public virtual PageCategory pageCategory { get; set; }
    }
}

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
        public int? Ordring { get; set; } = -1;
        public int PageCategorieId { get; set; }
        public virtual PageCategorie pagecategorie { get; set; }
    }
}

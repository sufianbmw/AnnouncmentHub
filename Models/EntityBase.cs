using System.ComponentModel.DataAnnotations.Schema;

namespace AnnouncmentHub.Models
{
    public class EntityBase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public bool IsActive { get; set; } = true; // Default to active

    }


}

using AnnouncmentHub.Models;

namespace AnnouncmentHub.ViewModels
{
    public class AnnouncementDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public DateTime? AddedDate { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        // Optional relationship to Client
        public int? ClientId { get; set; }
        public Client? Client { get; set; }

        // JSON string coming from SP
        public string? CategoriesJson { get; set; }

        // ✅ Deserialized objects (ready to use in Views)
        public List<CategoryJsonLink> Categories { get; set; } = new();

        // ✅ Optional: just IDs extracted from Categories list
        public List<int> CategoryIds => Categories?.Select(c => c.CategoryId).ToList() ?? new();

    }

}

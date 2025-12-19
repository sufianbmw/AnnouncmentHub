namespace AnnouncmentHub.Models
{
        public class Announcement : EntityBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public DateTime AddedDate { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        // Optional relationship to Client
        public int? ClientId { get; set; }
        public Client? Client { get; set; }
        public ICollection<AnnouncementCategory> AnnouncementCategories { get; set; } = new List<AnnouncementCategory>();
    }

}


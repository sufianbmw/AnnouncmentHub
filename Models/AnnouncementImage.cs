namespace AnnouncmentHub.Models
{
    public class AnnouncementImage : EntityBase
    {
        public string ImageUrl { get; set; }
        public bool IsCovered { get; set; } = false;
        // FK
        public int AnnouncementId { get; set; }
        public Announcement Announcement { get; set; }
    }
}

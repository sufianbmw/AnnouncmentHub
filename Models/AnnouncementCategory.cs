namespace AnnouncmentHub.Models
{


    public class AnnouncementCategory 
    {
        public int AnnouncementId { get; set; }  // Foreign Key for the Announcement
        public Announcement Announcement { get; set; }  // Navigation property to Announcement

        public int CategoryId { get; set; }  // Foreign Key for the Category
        public Category Category { get; set; }  // Navigation property to Category (subcategories)
    }

}

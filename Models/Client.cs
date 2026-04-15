using System.ComponentModel.DataAnnotations.Schema;

namespace AnnouncmentHub.Models
{
    public class Client:EntityBase
    {
        public string ClientName { get; set; }
        public string? LogoUrl { get; set; }
        public string?  FacebookLink { get; set; }
        public string?  WhatsUp { get; set; }
        public string?  MobileNumber { get; set; }
        public string?  SiteUrl { get; set; }

        [NotMapped]
        public IFormFile? LogoFile { get; set; }
        public string? CoverImageUrl { get; set; }   // Stored in DB

        [NotMapped]
        public IFormFile? CoverImageFile { get; set; }  // Used only for uploads

        public bool IsVIP { get; set; } = false;

        public TimeSpan? OpenFrom { get; set; }
        public TimeSpan? OpenTo { get; set; }
        public bool IsClosed { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        [NotMapped]
        public string? GoogleMapsUrl =>
         Latitude is not null && Longitude is not null
        ? $"https://www.google.com/maps?q={Latitude},{Longitude}"
        : null;
        // Navigation property - one client can have many announcements
        public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();

    }
}

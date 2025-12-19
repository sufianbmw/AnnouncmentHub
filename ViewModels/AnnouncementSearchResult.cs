namespace AnnouncmentHub.ViewModels
{
    public class AnnouncementSearchResult
    {
        public List<AnnouncementDto> Announcements { get; set; } = new();
        public int TotalCount { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

}

namespace AnnouncmentHub.ViewModels
{
    public class BreadcrumbItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Url { get; set; } // This will store the link (if any)
    }

}

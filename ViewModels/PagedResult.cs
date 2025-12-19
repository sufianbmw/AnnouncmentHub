namespace AnnouncmentHub.ViewModels
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();   // Current page items
        public int TotalCount { get; set; }           // Total items matching filters
        public int PageNumber { get; set; } = 1;      // Current page number
        public int PageSize { get; set; } = 10;       // Items per page

        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}

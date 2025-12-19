namespace AnnouncmentHub.ViewModels
{
    public class Search2AjaxRequest
    {
        public int? MainCategoryId { get; set; }
        public List<int>? SubCategoryIds { get; set; }
        public string? Title { get; set; }
        public string? DateFrom { get; set; }
        public string? DateTo { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}

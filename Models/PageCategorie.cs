namespace AnnouncmentHub.Models
{
  
    public class PageCategory : EntityBase
    {
        public PageCategory()
        {
            PageCategore = new HashSet<Page>();
        }
        public string CatName { get; set; }
        public bool Active { get; set; } = false;
        public int? Ordering { get; set; } = -1;
        public virtual ICollection<Page> PageCategore { get; set; }

    }
}

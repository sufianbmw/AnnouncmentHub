namespace AnnouncmentHub.Models
{
  
    public class PageCategorie : EntityBase
    {
        public PageCategorie()
        {
            PageCategore = new HashSet<Page>();
        }
        public string CatName { get; set; }
        public bool Active { get; set; } = false;
        public int? Ordring { get; set; } = -1;
        public virtual ICollection<Page> PageCategore { get; set; }

    }
}

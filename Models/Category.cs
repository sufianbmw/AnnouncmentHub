namespace AnnouncmentHub.Models
{

    public class Category : EntityBase
    {
        public string? CatName { get; set; }
        public bool IsParent { get; set; } = false;
        public string? IconUrl { get; set; }

        public ICollection<CategoryParentMapping> ParentMappings { get; set; } = new List<CategoryParentMapping>();
        public ICollection<CategoryParentMapping> SubCategoryMappings { get; set; } = new List<CategoryParentMapping>();

        public ICollection<AnnouncementCategory> AnnouncementCategories { get; set; } = new List<AnnouncementCategory>();
    }


}
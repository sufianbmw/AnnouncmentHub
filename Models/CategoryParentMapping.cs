namespace AnnouncmentHub.Models
{

    public class CategoryParentMapping
    {
        public int ParentCategoryId { get; set; }
        public Category ParentCategory { get; set; }

        public int SubCategoryId { get; set; }
        public Category SubCategory { get; set; }
    }

}

using AnnouncmentHub.Data;
using AnnouncmentHub.Models;

namespace AnnouncmentHub.Helpers
{
    public static class DataSeeder
    {
        public static async Task SeedCategoriesAsync(ApplicationDbContext context)
        {
            // Ensure database exists
            await context.Database.EnsureCreatedAsync();

            // ✅ Only seed if the table is empty
            if (!context.PageCategories.Any())
            {
                var categories = new List<PageCategorie>
        {
            //new PageCategorie { CatName = "الكلمة الترحيبية", Active = true, Ordring = 1 },
            ///new PageCategorie { CatName = "الخدمات والمنتجات", Active = true, Ordring = 2 },
            new PageCategorie { CatName = "معلومات الاتصال", Active = true, Ordring = 3 },
            //new PageCategorie { CatName = "الأبحاث والدراسات", Active = true, Ordring = 4 },
            new PageCategorie { CatName = "مجلس الإدارة", Active = true, Ordring = 5 }
        };

                await context.PageCategories.AddRangeAsync(categories);
                await context.SaveChangesAsync();

                //Console.WriteLine("✅ Categories seeded successfully!");
            }
            else
            {
                //Console.WriteLine("ℹ️ Categories already exist, skipping seeding...");
            }
        }

    }

}

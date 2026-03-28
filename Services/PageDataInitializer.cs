using AnnouncmentHub.Data;
using AnnouncmentHub.Models;

namespace AnnouncmentHub.Services
{
    public class PageDataInitializer
    {
        private readonly ApplicationDbContext _context;

        public PageDataInitializer(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedDataAsync()
        {
            await SeedCategoriesAsync();
            await SeedPagesAsync();
        }

        private async Task SeedCategoriesAsync()
        {
            if (_context.PageCategory.Any()) return;

            var categories = new List<PageCategory>
            {
                new PageCategory { CatName = "معلومات عامة", Active = true, Ordering = 1 },
                new PageCategory { CatName = "قانوني",        Active = true, Ordering = 2 },
            };

           await _context.PageCategory.AddRangeAsync(categories);

            await _context.SaveChangesAsync();
            Console.WriteLine("✅ تم إنشاء التصنيفات بنجاح.");
        }

        private async Task SeedPagesAsync()
        {
            if (_context.Pages.Any()) return;

            var generalCat = _context.PageCategory.FirstOrDefault(c => c.CatName == "معلومات عامة");
            var legalCat = _context.PageCategory.FirstOrDefault(c => c.CatName == "قانوني");

            if (generalCat == null || legalCat == null)
            {
                Console.WriteLine("⚠️ لم يتم العثور على التصنيفات، تعذّر إنشاء الصفحات.");
                return;
            }

            var pages = new List<Page>
            {
                // ── معلومات عامة ──
                new Page
                {
                    PageTitle      = "من نحن",
                    PageDetails    = "نحن متجر إلكتروني نسعى لتقديم أفضل المنتجات بأعلى جودة وأسعار منافسة. نؤمن بأن تجربة التسوق يجب أن تكون سهلة وممتعة لكل عميل.",
                    Active         = true,
                    Ordering       = 1,
                    PageCategoryId = generalCat.Id
                },
                new Page
                {
                    PageTitle      = "تواصل معنا",
                    PageDetails    = "نحن هنا لمساعدتك! يمكنك التواصل معنا عبر البريد الإلكتروني أو الهاتف وسنرد عليك في أقرب وقت ممكن.",
                    Active         = true,
                    Ordering       = 2,
                    PageCategoryId = generalCat.Id
                },

                // ── قانوني ──
                new Page
                {
                    PageTitle      = "سياسة الخصوصية",
                    PageDetails    = "نحن نحترم خصوصيتك ونلتزم بحماية بياناتك الشخصية. لا نشارك معلوماتك مع أي طرف ثالث دون موافقتك، ونستخدم أحدث تقنيات التشفير لحماية بياناتك.",
                    Active         = true,
                    Ordering       = 1,
                    PageCategoryId = legalCat.Id
                },
                new Page
                {
                    PageTitle      = "الشروط والأحكام",
                    PageDetails    = "باستخدامك لمتجرنا الإلكتروني فإنك توافق على الشروط والأحكام المذكورة. نحتفظ بحق تعديل هذه الشروط في أي وقت مع إشعار المستخدمين بذلك.",
                    Active         = true,
                    Ordering       = 2,
                    PageCategoryId = legalCat.Id
                },
            };

            await _context.Pages.AddRangeAsync(pages);
            await _context.SaveChangesAsync();
            Console.WriteLine("✅ تم إنشاء الصفحات بنجاح.");
        }
    }
}
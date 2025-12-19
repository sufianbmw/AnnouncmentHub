//using AnnouncmentHub.Data;
//using AnnouncmentHub.ViewModels;
//using Microsoft.EntityFrameworkCore;

//public class BreadcrumbService
//{
//    private readonly ApplicationDbContext _context;

//    public BreadcrumbService(ApplicationDbContext context)
//    {
//        _context = context;
//    }

//    // 🟢 Breadcrumb for categories (parent → sub → current)
//    public async Task<List<BreadcrumbItem>> GetCategoryBreadcrumbAsync(int categoryId)
//    {
//        var breadcrumb = new List<BreadcrumbItem>();
//        await BuildCategoryChain(categoryId, breadcrumb);
//        breadcrumb.Reverse();

//        // Always start with Home
//        return breadcrumb;
//    }

//    private async Task BuildCategoryChain(int categoryId, List<BreadcrumbItem> breadcrumb)
//    {
//        var category = await _context.Categories
//            .Include(c => c.ParentMappings)
//            .ThenInclude(pm => pm.ParentCategory)
//            .FirstOrDefaultAsync(c => c.Id == categoryId);

//        if (category != null)
//        {
//            breadcrumb.Add(new BreadcrumbItem
//            {
//                Id = category.Id,
//                Name = category.CatName ?? "تصنيف"
//            });

//            var parent = category.ParentMappings.FirstOrDefault();
//            if (parent != null)
//                await BuildCategoryChain(parent.ParentCategoryId, breadcrumb);
//        }
//    }

//    // 🟢 Breadcrumb for clients (Home → Clients → [optional category chain] or Client Profile)
//    public async Task<List<BreadcrumbItem>> GetClientBreadcrumbAsync(int? clientId = null, int? categoryId = null)
//    {
//        var breadcrumb = new List<BreadcrumbItem>
//        {
//            new BreadcrumbItem
//            {
//                Id = 0,
//                Name = "الرئيسية", // "Home" in Arabic
//                Url = "/Home" // URL for Home
//            },
//            new BreadcrumbItem
//            {
//                Id = 0,
//                Name = "العملاء", // "Clients" in Arabic
//                Url = "/Clients" // URL for Clients
//            }
//        };

//        if (clientId.HasValue)
//        {
//            // Client Profile Page: Fetch client details and add breadcrumb
//            var client = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == clientId);
//            if (client != null)
//            {
//                breadcrumb.Add(new BreadcrumbItem
//                {
//                    Id = clientId.Value,
//                    Name = client.ClientName ?? "الملف الشخصي للعميل", // "Client Profile" in Arabic
//                    Url = "" // No URL, as it's the current page
//                });
//            }
//        }

//        // Add category breadcrumb if categoryId is provided
//        if (categoryId.HasValue)
//        {
//            var catTrail = await GetCategoryBreadcrumbAsync(categoryId.Value);
//            breadcrumb.AddRange(catTrail.Skip(1)); // Skip "الرئيسية" from the category path
//        }

//        return breadcrumb;
//    }
//}


using AnnouncmentHub.Data;
using AnnouncmentHub.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AnnouncmentHub.Service
{
    public class BreadcrumbService
    {
        private readonly ApplicationDbContext _context;

        public BreadcrumbService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🟢 Breadcrumb for categories (parent → sub → current)
        public async Task<List<BreadcrumbItem>> GetCategoryBreadcrumbAsync(int categoryId)
        {
            var breadcrumb = new List<BreadcrumbItem>();
            await BuildCategoryChain(categoryId, breadcrumb);
            breadcrumb.Reverse();

            // Always start with Home
            return breadcrumb;
        }

        private async Task BuildCategoryChain(int categoryId, List<BreadcrumbItem> breadcrumb)
        {
            var category = await _context.Categories
                .Include(c => c.ParentMappings)
                .ThenInclude(pm => pm.ParentCategory)
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category != null)
            {
                // 🔍 Determine URL based on whether it has a parent or not
                bool isChild = category.ParentMappings.Any();  // Child if it has a parent

                string url = isChild
                    ? "Hub/SubCategoryAnnouncements/"
                    : "Hub/CategoryAnnouncements/";

                breadcrumb.Add(new BreadcrumbItem
                {
                    Id = category.Id,
                    Name = category.CatName ?? "تصنيف",
                    Url= url
                });

                var parent = category.ParentMappings.FirstOrDefault();
                if (parent != null)
                    await BuildCategoryChain(parent.ParentCategoryId, breadcrumb);
            }
        }
        // 🟢 Breadcrumb for client list (Home → Clients → [optional category chain])
        public async Task<List<BreadcrumbItem>> GetClientBreadcrumbAsync(int? categoryId = null)
        {
            var breadcrumb = new List<BreadcrumbItem>
    {

        new BreadcrumbItem
        {
            Id = 0, // Id for "Clients" is not necessary, but can be assigned if needed
            Name = "العملاء" ,// "Clients" in Arabic
            Url="Hub/Clients/"
        }
    };

            if (categoryId.HasValue)
            {
                // If a categoryId is provided, include the category breadcrumb
                var catTrail = await GetCategoryBreadcrumbAsync(categoryId.Value);
                breadcrumb.AddRange(catTrail.Skip(1)); // Skip "الرئيسية" from the category path
            }

            return breadcrumb;
        }

        public async Task<List<BreadcrumbItem>> GetClientBreadcrumbAsync(int clientId, int? categoryId = null)
        {
            var client = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == clientId);
            var breadcrumb = new List<BreadcrumbItem>
             {

           new BreadcrumbItem
        {
            Id = 0, // Id for "Clients" is not necessary, but can be assigned if needed
            Name = "العملاء", // "Clients" in Arabic
            Url="Hub/Clients/"
        },
          new BreadcrumbItem
                  {
                      Id = clientId,
                      Name = await _context.Clients
                          .Where(c => c.Id == clientId)
                          .Select(c => c.ClientName)
                          .FirstOrDefaultAsync() ?? "عميل"
                  }
             };

            if (categoryId.HasValue)
            {
                var catTrail = await GetCategoryBreadcrumbAsync(categoryId.Value);
                breadcrumb.AddRange(catTrail.Skip(1)); // Skip "الرئيسية" from category path
            }

            return breadcrumb;
        }

    }
}

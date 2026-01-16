using AnnouncmentHub.Models;
using AnnouncmentHub.ViewModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AnnouncmentHub.Data
{

    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<CategoryParentMapping> CategoryParentMappings { get; set; }

        // Add this:
        public DbSet<AnnouncementCategory> AnnouncementCategories { get; set; }
        public DbSet<AnnouncementImage> AnnouncementImage { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<PageCategorie> PageCategories { get; set; }

        //public DbSet<AnnouncementDto> AnnouncementDtos { get; set; }  // Not mapped to a table

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CategoryParentMapping>().HasKey(cpm => new { cpm.ParentCategoryId, cpm.SubCategoryId });

            modelBuilder.Entity<CategoryParentMapping>()
                .HasOne(cpm => cpm.ParentCategory)
                .WithMany(c => c.SubCategoryMappings)
                .HasForeignKey(cpm => cpm.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CategoryParentMapping>()
                .HasOne(cpm => cpm.SubCategory)
                .WithMany(c => c.ParentMappings)
                .HasForeignKey(cpm => cpm.SubCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AnnouncementCategory>().HasKey(ac => new { ac.AnnouncementId, ac.CategoryId });

            modelBuilder.Entity<AnnouncementCategory>()
                .HasOne(ac => ac.Announcement)
                .WithMany(a => a.AnnouncementCategories)
                .HasForeignKey(ac => ac.AnnouncementId);

            modelBuilder.Entity<AnnouncementCategory>()
                .HasOne(ac => ac.Category)
                .WithMany(c => c.AnnouncementCategories)
                .HasForeignKey(ac => ac.CategoryId);

            //modelBuilder.Entity<AnnouncementDto>().HasNoKey(); // Important: No primary key

        }
    }

}

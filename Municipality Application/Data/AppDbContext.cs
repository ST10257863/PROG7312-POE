using Microsoft.EntityFrameworkCore;
using Municipality_Application.Models;

namespace Municipality_Application.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Relationships
            builder.Entity<Report>()
                .HasOne(i => i.Category)
                .WithMany(c => c.Issues)
                .HasForeignKey(i => i.CategoryId);

            builder.Entity<Report>()
                .HasMany(i => i.Attachments)
                .WithOne(a => a.Report)
                .HasForeignKey(a => a.ReportId);

            builder.Entity<Report>()
                .HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .IsRequired(false);
        }
    }
}
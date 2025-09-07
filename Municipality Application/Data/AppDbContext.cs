using Microsoft.EntityFrameworkCore;
using Municipality_Application.Models;

namespace Municipality_Application.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Issue> Issues { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Relationships
            builder.Entity<Issue>()
                .HasOne(i => i.Category)
                .WithMany(c => c.Issues)
                .HasForeignKey(i => i.CategoryId);

            builder.Entity<Issue>()
                .HasMany(i => i.Attachments)
                .WithOne(a => a.Issue)
                .HasForeignKey(a => a.IssueId);

            builder.Entity<Issue>()
                .HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .IsRequired(false);
        }
    }
}
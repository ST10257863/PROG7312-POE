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
        public DbSet<EventSearchFrequency> EventSearchFrequencies { get; set; }
        public DbSet<Address> Addresses { get; set; }

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
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Report>()
                .HasOne(r => r.Address)
                .WithMany()
                .HasForeignKey(r => r.AddressId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Report>()
                .HasIndex(r => r.Id);

            builder.Entity<Report>()
                .HasIndex(r => r.ReportedAt);

            builder.Entity<Report>()
                .HasIndex(r => r.CategoryId);

            builder.Entity<Report>()
                .HasIndex(r => r.Status);

            builder.Entity<Address>()
                .HasIndex(a => a.City);

            builder.Entity<Address>()
                .HasIndex(a => a.Province);

            builder.Entity<Address>()
                .HasIndex(a => a.Street);

            builder.Entity<Address>()
                .HasIndex(a => a.FormattedAddress);
        }
    }
}
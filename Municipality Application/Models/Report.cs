using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Municipality_Application.Models
{
    public class Report
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public DateTime ReportedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public required string Description { get; set; }

        public ICollection<Attachment>? Attachments { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public required string Address { get; set; }

        public int? UserId { get; set; }

        [Required]
        public IssueStatus Status { get; set; } = IssueStatus.Reported;

        [Phone]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [ForeignKey("CategoryId")]
        public /*required*/ Category? Category { get; set; } //Will be changed to required and not nullable when DB is implemented

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }

    public enum IssueStatus
    {
        Reported,
        InProgress,
        Resolved,
        Closed
    }
}

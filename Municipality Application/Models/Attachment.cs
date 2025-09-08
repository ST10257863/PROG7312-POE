using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Municipality_Application.Models
{
    public class Attachment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid IssueId { get; set; }

        [Required]
        public required string FileType { get; set; }

        public long? FileSize { get; set; }

        [Required]
        public required string FilePath { get; set; }

        // Navigation property
        [ForeignKey("IssueId")]
        public Report Issue { get; set; } = null!;
    }
}

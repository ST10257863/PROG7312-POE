using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Municipality_Application.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }

        public int? DepartmentId { get; set; }

        // Navigation properties
        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        public ICollection<Issue>? Issues { get; set; }
    }
}
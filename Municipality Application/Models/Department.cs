using System.ComponentModel.DataAnnotations;

namespace Municipality_Application.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }
    }
}
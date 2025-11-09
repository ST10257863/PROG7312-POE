using System.ComponentModel.DataAnnotations;

namespace Municipality_Application.ViewModels
{
    public class ReportCreateViewModel
    {
        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public AddressViewModel Address { get; set; } = new();

        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public List<IFormFile>? Files { get; set; }
    }
}
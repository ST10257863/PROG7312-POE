using System.ComponentModel.DataAnnotations;

namespace Municipality_Application.Models
{
    public class Address
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Street { get; set; } = string.Empty;

        public string? City { get; set; }
        public string? Province { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // For Google Maps display
        public string? FormattedAddress { get; set; }
    }
}
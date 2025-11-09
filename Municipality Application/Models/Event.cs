using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Municipality_Application.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        // Use Address model instead of string address/location/lat/long
        [Required]
        public int AddressId { get; set; }

        [ForeignKey("AddressId")]
        public Address Address { get; set; } = null!;

        public int Priority { get; set; } // For priority queue
    }
}

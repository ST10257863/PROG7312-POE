using System.ComponentModel.DataAnnotations;

namespace Municipality_Application.ViewModels
{
    public class AddressViewModel
    {
        public string Street { get; set; } = string.Empty;
        public string? Suburb { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? FormattedAddress { get; set; }

        public string DisplayAddress =>
            string.IsNullOrWhiteSpace(Street)
                ? (Suburb ?? string.Empty)
                : (string.IsNullOrWhiteSpace(Suburb) ? Street : $"{Street}, {Suburb}");
    }
}

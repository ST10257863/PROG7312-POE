using System;
using System.Collections.Generic;

namespace Municipality_Application.ViewModels
{
    public class EventIndexViewModel
    {
        // Search/filter properties
        public string? Search { get; set; }
        public string? Category { get; set; }
        public DateTime? Date { get; set; }
        public string? Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Event lists
        public IEnumerable<EventViewModel> Events { get; set; } = new List<EventViewModel>();
        public IEnumerable<EventViewModel> Recommendations { get; set; } = new List<EventViewModel>();

        // Categories for filter dropdown
        public HashSet<string>? Categories { get; set; }
    }
}

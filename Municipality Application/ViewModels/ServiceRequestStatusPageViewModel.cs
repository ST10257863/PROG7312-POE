using Microsoft.AspNetCore.Mvc.Rendering;
using Municipality_Application.Models;

namespace Municipality_Application.ViewModels
{
    public class ServiceRequestStatusPageViewModel
    {
        // Search/filter properties
        public string? SearchTitle { get; set; }
        public string? SearchArea { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int? CategoryId { get; set; }
        public string? Status { get; set; }

        public IEnumerable<Category>? Categories { get; set; }
        public IEnumerable<SelectListItem>? Statuses { get; set; }

        // Results
        public List<ServiceRequestStatusViewModel> Results { get; set; } = new();
    }
}

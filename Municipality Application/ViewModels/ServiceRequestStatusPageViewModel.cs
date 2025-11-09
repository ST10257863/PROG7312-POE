namespace Municipality_Application.ViewModels
{
    public class ServiceRequestStatusPageViewModel
    {
        // Search/filter properties
        public string? SearchTitle { get; set; }
        public string? SearchArea { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Results
        public List<ServiceRequestStatusViewModel> Results { get; set; } = new();
    }
}

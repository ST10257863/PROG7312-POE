namespace Municipality_Application.ViewModels
{
    public class ServiceRequestStatusViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime ReportedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Category { get; set; }
    }
}

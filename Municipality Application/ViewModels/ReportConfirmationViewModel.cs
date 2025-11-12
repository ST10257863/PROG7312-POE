namespace Municipality_Application.ViewModels
{
    public class ReportConfirmationViewModel
    {
        public Guid Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ReportedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public AddressViewModel? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public List<AttachmentViewModel>? Attachments { get; set; }
    }

    public class AttachmentViewModel
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
    }
}

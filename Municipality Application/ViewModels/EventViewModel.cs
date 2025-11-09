namespace Municipality_Application.ViewModels
{
    public class EventViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int Priority { get; set; }
        public AddressViewModel? Address { get; set; }
    }
}

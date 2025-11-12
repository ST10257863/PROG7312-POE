using Municipality_Application.Models;
using Municipality_Application.ViewModels;

namespace Municipality_Application.Mappers
{
    public static class EventMapper
    {
        public static EventViewModel ToViewModel(Event ev)
        {
            return new EventViewModel
            {
                Id = ev.Id,
                Title = ev.Title,
                Description = ev.Description,
                Category = ev.Category,
                Date = ev.Date,
                Priority = ev.Priority,
                Address = ev.Address != null ? new AddressViewModel
                {
                    Street = ev.Address.Street,
                    Suburb = ev.Address.Suburb,
                    City = ev.Address.City,
                    Province = ev.Address.Province,
                    PostalCode = ev.Address.PostalCode,
                    Country = ev.Address.Country,
                    Latitude = ev.Address.Latitude,
                    Longitude = ev.Address.Longitude,
                    FormattedAddress = ev.Address.FormattedAddress
                } : null
            };
        }

        public static List<EventViewModel> ToViewModelList(IEnumerable<Event> events)
        {
            return events.Select(ToViewModel).ToList();
        }

        public static EventIndexViewModel ToIndexViewModel(
            IEnumerable<Event> events,
            IEnumerable<Event> recommendations,
            HashSet<string> categories,
            string? search,
            string? category,
            DateTime? date,
            string? address,
            double? latitude,
            double? longitude)
        {
            return new EventIndexViewModel
            {
                Search = search,
                Category = category,
                Date = date,
                Address = address,
                Latitude = latitude,
                Longitude = longitude,
                Events = ToViewModelList(events),
                Recommendations = ToViewModelList(recommendations),
                Categories = categories
            };
        }
    }
}

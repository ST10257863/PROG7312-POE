using Microsoft.AspNetCore.Mvc;
using Municipality_Application.Interfaces.Service;
using Municipality_Application.Mappers;
using Municipality_Application.ViewModels;

namespace Municipality_Application.Controllers
{
    public class EventsController : Controller
    {
        private readonly IEventService _eventService;
        private readonly IConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventsController"/> class.
        /// </summary>
        /// <param name="eventService">The event service instance.</param>
        /// <param name="config">The configuration instance.</param>
        public EventsController(IEventService eventService, IConfiguration config)
        {
            _eventService = eventService;
            _config = config;
        }

        /// <summary>
        /// Displays a list of events, optionally filtered by search, category, date, or location.
        /// Also provides event recommendations and available categories.
        /// </summary>
        /// <param name="Search">Optional search keyword.</param>
        /// <param name="Category">Optional event category.</param>
        /// <param name="Date">Optional event date.</param>
        /// <param name="Address">Optional address string (for display/filtering).</param>
        /// <param name="Latitude">Optional latitude for location-based filtering.</param>
        /// <param name="Longitude">Optional longitude for location-based filtering.</param>
        /// <returns>The events view with filtered events and recommendations.</returns>
        public async Task<IActionResult> Index(
            string? Search,
            string? Category,
            DateTime? Date,
            string? Address,
            double? Latitude,
            double? Longitude)
        {
            var googleMapsKey = _config["ApiKeys:GoogleMaps"];
            ViewBag.GoogleMapsApiKey = string.IsNullOrWhiteSpace(googleMapsKey) ? null : googleMapsKey;

            var events = await _eventService.GetEventsAsync(Search, Category, Date, Latitude, Longitude);
            var recommendations = await _eventService.GetRecommendationsAsync(Search, Category, Date);
            var categories = await _eventService.GetCategoriesAsync();

            var viewModel = EventMapper.ToIndexViewModel(
                events, 
                recommendations, 
                categories, 
                Search, 
                Category, 
                Date, 
                Address, 
                Latitude, 
                Longitude);

            return View(viewModel);
        }
    }
}
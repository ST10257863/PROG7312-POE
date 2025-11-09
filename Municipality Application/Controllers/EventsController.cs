using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Municipality_Application.Interfaces.Service;

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
        /// <param name="search">Optional search keyword.</param>
        /// <param name="category">Optional event category.</param>
        /// <param name="date">Optional event date.</param>
        /// <param name="latitude">Optional latitude for location-based filtering.</param>
        /// <param name="longitude">Optional longitude for location-based filtering.</param>
        /// <returns>The events view with filtered events and recommendations.</returns>
        public async Task<IActionResult> Index(string search, string category, DateTime? date, double? latitude, double? longitude)
        {
            var googleMapsKey = _config["ApiKeys:GoogleMaps"];
            ViewBag.GoogleMapsApiKey = string.IsNullOrWhiteSpace(googleMapsKey) ? null : googleMapsKey;

            var events = await _eventService.GetEventsAsync(search, category, date, latitude, longitude);
            var recommendations = await _eventService.GetRecommendationsAsync(search, category, date);
            ViewBag.Categories = await _eventService.GetCategoriesAsync();
            ViewBag.Recommendations = recommendations;
            return View(events);
        }
    }
}
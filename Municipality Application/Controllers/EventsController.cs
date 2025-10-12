using Microsoft.AspNetCore.Mvc;
using Municipality_Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Municipality_Application.Controllers
{
    public class EventsController : Controller
    {
        private readonly IEventService _eventService;
        private readonly IConfiguration _config;

        public EventsController(IEventService eventService, IConfiguration config)
        {
            _eventService = eventService;
            _config = config;
        }

        public IActionResult Index(string search, string category, DateTime? date, double? latitude, double? longitude)
        {
            var googleMapsKey = _config["ApiKeys:GoogleMaps"];
            ViewBag.GoogleMapsApiKey = string.IsNullOrWhiteSpace(googleMapsKey) ? null : googleMapsKey;

            var events = _eventService.GetEvents(search, category, date, latitude, longitude);
            var recommendations = _eventService.GetRecommendations(search, category, date);
            ViewBag.Categories = _eventService.GetCategories();
            ViewBag.Recommendations = recommendations;
            return View(events);
        }
    }
}
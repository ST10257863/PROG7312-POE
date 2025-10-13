using Microsoft.AspNetCore.Mvc;
using Municipality_Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

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
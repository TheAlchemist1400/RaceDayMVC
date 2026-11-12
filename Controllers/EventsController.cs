
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers
{
    public class EventsController : Controller
    {
        private readonly RaceDayApiClient _apiClient;

        public EventsController(RaceDayApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // GET: /Events
        // Retrieves all events from the API.
        public async Task<IActionResult> Index()
        {
            // Send a GET request to the RaceDay API.
            using var response = await _apiClient.SendAsync(
                HttpMethod.Get,
                "api/events"
            );

            // Check whether the API request succeeded.
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.ErrorMessage = "Unable to load events.";

                return View(new List<EventViewModel>());
            }

            // Convert the API JSON response into C# objects(Deserialize)
            var events = await _apiClient.ReadAsync<List<EventViewModel>>(
                response
            );

            // Send the events to the Razor view.
            return View(events ?? new List<EventViewModel>());
        }
    }
}


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


        // GET: /Events/MyEvents
        // Displays only the events belonging to the logged-in Organizer.
        public async Task<IActionResult> MyEvents()
        {
            // STEP 1: Check whether someone is logged in.
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // STEP 2: Make sure the logged-in user is an Organizer.
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Organizer")
            {
                return StatusCode(403);
            }

            // STEP 3: Ask the API for this Organizer's events.
            // RaceDayApiClient automatically forwards the API session cookie.
            using var response = await _apiClient.SendAsync(
                HttpMethod.Get,
                "api/events/mine"
            );

            // STEP 4: Handle possible API errors.
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ViewBag.ErrorMessage =
                    "Your API session has expired. Please log in again.";

                return View(new List<EventViewModel>());
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return StatusCode(403);
            }

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.ErrorMessage =
                    "Unable to retrieve your events. Please try again.";

                return View(new List<EventViewModel>());
            }

            // STEP 5: Convert the API's JSON response into C# objects.
            var events = await _apiClient.ReadAsync<List<EventViewModel>>(response);

            // STEP 6: Send the events to the View.
            return View(events ?? new List<EventViewModel>());
        }


        // Retrieves available event locations from the API.
        private async Task<List<EventLocationViewModel>> GetLocationsAsync()
        {
            using var response = await _apiClient.SendAsync(
                HttpMethod.Get,
                "api/locations"
            );

            if (!response.IsSuccessStatusCode)
            {
                return new List<EventLocationViewModel>();
            }

            var locations = await _apiClient.ReadAsync<List<EventLocationViewModel>>(
                response
            );

            return locations ?? new List<EventLocationViewModel>();
        }


        // GET: /Events/Create
        // Displays the form for creating a new event.
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Only logged-in Organizers can create events.
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            string? role = HttpContext.Session.GetString("Role");

            if (role != "Organizer")
            {
                return StatusCode(403);
            }

            // Retrieve locations from the API.
            var locations = await GetLocationsAsync();

            // If the API returned no locations, inform the Organizer.
            if (!locations.Any())
            {
                ViewBag.ErrorMessage =
                    "No event locations are available. Please add a location first.";
            }

            // Create an empty form model and attach the locations.
            var model = new CreateEventViewModel
            {
                EventDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                Locations = locations
            };

            return View(model);
        }
    }
}

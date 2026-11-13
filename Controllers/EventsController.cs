
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


        // POST: /Events/Create
        // Receives the form data and sends it to the API.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEventViewModel model)
        {
            // STEP 1: Check that the user is logged in.
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // STEP 2: Check that the user is an Organizer.
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Organizer")
            {
                return StatusCode(403);
            }

            // STEP 3: Make sure the event date is not in the past.
            if (model.EventDate < DateOnly.FromDateTime(DateTime.Today))
            {
                ModelState.AddModelError(
                    nameof(model.EventDate),
                    "The event date cannot be in the past."
                );
            }

            // STEP 4: If validation fails, reload locations and show the form.
            if (!ModelState.IsValid)
            {
                model.Locations = await GetLocationsAsync();
                return View(model);
            }

            // STEP 5: Prepare the information expected by the API.
            var request = new
            {
                model.EventsName,
                model.EventDate,
                model.MaxParticipants,
                model.LocationId
            };

            // STEP 6: Send the POST request to the API.
            using var response = await _apiClient.SendAsync(
                HttpMethod.Post,
                "api/events",
                request
            );

            // STEP 7: Handle API errors.
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Your API session has expired. Please log in again."
                );
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return StatusCode(403);
            }
            else if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    $"Unable to create event. {error}"
                );
            }
            else
            {
                // STEP 8: Event successfully created!
                TempData["Success"] = "Your event was published successfully.";

                return RedirectToAction(nameof(MyEvents));
            }

            // If the API returned an error, reload locations before displaying the form again so the dropdown does not become empty.
            model.Locations = await GetLocationsAsync();

            return View(model);
        }


        // GET: /Events/Edit/5
        // Loads an existing event into the editing form.
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // STEP 1: Check login.
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            // STEP 2: Check Organizer role.
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Organizer")
                return StatusCode(403);

            // STEP 3: Retrieve only this Organizer's events.
            using var response = await _apiClient.SendAsync(
                HttpMethod.Get,
                "api/events/mine"
            );

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode);
            }

            var events = await _apiClient.ReadAsync<List<EventViewModel>>(response);

            // STEP 4: Find the requested event in the Organizer's own events.
            var eventItem = events?.FirstOrDefault(e => e.EventId == id);

            if (eventItem == null)
                return NotFound("Event not found.");

            // STEP 5: Load available locations.
            var locations = await GetLocationsAsync();

            // STEP 6: Populate the editing form.
            var model = new EditEventViewModel
            {
                EventId = eventItem.EventId,
                EventsName = eventItem.EventsName,
                EventDate = eventItem.EventDate,
                MaxParticipants = eventItem.MaxParticipants,
                LocationId = eventItem.LocationId,
                Locations = locations
            };

            return View(model);
        }


        // POST: /Events/Edit/{id}
        // Updates an existing event through the API.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditEventViewModel model)
        {
            // STEP 1: Check whether the user is logged in.
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // STEP 2: Confirm that the user is an Organizer.
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Organizer")
            {
                return StatusCode(403);
            }

            // STEP 3: Make sure the URL ID matches the submitted EventId.
            if (id != model.EventId)
            {
                return BadRequest("Event ID mismatch.");
            }

            // STEP 4: Validate the event date.
            if (model.EventDate < DateOnly.FromDateTime(DateTime.Today))
            {
                ModelState.AddModelError(
                    nameof(model.EventDate),
                    "The event date cannot be in the past."
                );
            }

            // STEP 5: If validation fails, reload locations and return the form.
            if (!ModelState.IsValid)
            {
                model.Locations = await GetLocationsAsync();
                return View(model);
            }

            // STEP 6: Prepare the request expected by the API.
            var request = new
            {
                model.EventsName,
                model.EventDate,
                model.MaxParticipants,
                model.LocationId
            };

            // STEP 7: Send PUT request to the API.
            using var response = await _apiClient.SendAsync(
                HttpMethod.Put,
                $"api/events/{id}",
                request
            );

            // STEP 8: Handle API responses.
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Your API session has expired. Please log in again."
                );
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return StatusCode(403);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound("Event not found.");
            }
            else if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    $"Unable to update event. {error}"
                );
            }
            else
            {
                // STEP 9: Update successful.
                TempData["Success"] = "Your event was updated successfully.";

                return RedirectToAction(nameof(MyEvents));
            }

            // Reload locations if we need to display the form again.
            model.Locations = await GetLocationsAsync();

            return View(model);
        }


        // GET: /Events/Delete/5
        // Displays a confirmation page before deleting an event.
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            // STEP 1: Check login.
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            // STEP 2: Only Organizers can delete events.
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Organizer")
                return StatusCode(403);

            // STEP 3: Retrieve this Organizer's events.
            using var response = await _apiClient.SendAsync(
                HttpMethod.Get,
                "api/events/mine"
            );

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode);
            }

            var events = await _apiClient.ReadAsync<List<EventViewModel>>(response);

            // STEP 4: Find the event within this Organizer's events.
            var eventItem = events?.FirstOrDefault(e => e.EventId == id);

            if (eventItem == null)
                return NotFound("Event not found.");

            // STEP 5: Display the event details for confirmation.
            return View(eventItem);
        }


        // POST: /Events/Delete/5
        // Sends the DELETE request to the API.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteCornfirmation(int id)
        {
            // STEP 1: Check login.
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            // STEP 2: Only Organizers can delete events.
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Organizer")
                return StatusCode(403);

            // STEP 3: Send DELETE request to the API.
            using var response = await _apiClient.SendAsync(
                HttpMethod.Delete,
                $"api/events/{id}"
            );

            // STEP 4: Handle API responses.
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                TempData["Error"] =
                    "Your API session has expired. Please log in again.";

                return RedirectToAction("Login", "Auth");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return StatusCode(403);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound("Event not found.");
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to delete this event.";
                return RedirectToAction(nameof(MyEvents));
            }

            // STEP 5: Confirm successful deletion.
            TempData["Success"] = "Your event was deleted successfully.";

            return RedirectToAction(nameof(MyEvents));
        }

        // GET: /Events/Details/5
        // Displays an event and its available activities.
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // STEP 1: Require a logged-in user.
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // STEP 2: Retrieve the selected event.
            using var eventResponse = await _apiClient.SendAsync(
                HttpMethod.Get,
                $"api/events/{id}"
            );

            if (eventResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound("Event not found.");
            }

            if (!eventResponse.IsSuccessStatusCode)
            {
                return StatusCode((int)eventResponse.StatusCode);
            }

            var eventItem = await _apiClient.ReadAsync<EventViewModel>(
                eventResponse
            );

            if (eventItem == null)
            {
                return NotFound("Event not found.");
            }

            // STEP 3: Retrieve activities belonging to this event.
            using var activityResponse = await _apiClient.SendAsync(
                HttpMethod.Get,
                $"api/events/{id}/categories"
            );

            if (!activityResponse.IsSuccessStatusCode)
            {
                return StatusCode((int)activityResponse.StatusCode);
            }

            var activities =
                await _apiClient.ReadAsync<List<ActivityViewModel>>(
                    activityResponse
                );

            // STEP 4: Combine event and activity information.
            var model = new EventDetailsViewModel
            {
                Event = eventItem,
                Activities = activities ?? new List<ActivityViewModel>()
            };

            // STEP 5: Check whether the Participant is already enrolled.
            string? role = HttpContext.Session.GetString("Role");

            if (role == "Participant")
            {
                using var enrolmentResponse = await _apiClient.SendAsync(
                    HttpMethod.Get,
                    "api/enrolments/mine"
                );

                // Do not silently treat an API failure as "not enrolled".
                if (!enrolmentResponse.IsSuccessStatusCode)
                {
                    return StatusCode((int)enrolmentResponse.StatusCode);
                }

                var enrolments =
                    await _apiClient.ReadAsync<List<MyEnrolmentViewModel>>(
                        enrolmentResponse
                    );

                // Find any registration belonging to this event.
                var existingEnrolment = enrolments?
                    .FirstOrDefault(e => e.EventId == id);

                if (existingEnrolment != null)
                {
                    model.IsEnrolled = true;
                    model.EnrolmentStatus =
                        existingEnrolment.RegistrationStatus;
                }
            }

            // STEP 6: Display the event details.
            return View(model);
        }
    }
}

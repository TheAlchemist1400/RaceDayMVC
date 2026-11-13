
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers
{
    public class ActivitiesController : Controller
    {
        private readonly RaceDayApiClient _apiClient;

        public ActivitiesController(RaceDayApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // GET: /Activities/Create?eventId=5
        // Displays the form for adding an activity to an event.
        [HttpGet]
        public async Task<IActionResult> Create(int eventId)
        {
            // STEP 1: Check whether the user is logged in.
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // STEP 2: Only Organizers can create activities.
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Organizer")
            {
                return StatusCode(403);
            }

            // STEP 3: Make sure the event ID is valid.
            if (eventId <= 0)
            {
                return BadRequest("Invalid event ID.");
            }

            // STEP 4: Retrieve this Organizer's events.
            // This prevents them from opening the creation form
            // for an event belonging to another Organizer.
            using var response = await _apiClient.SendAsync(
                HttpMethod.Get,
                "api/events/mine"
            );

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode);
            }

            var events = await _apiClient.ReadAsync<List<EventViewModel>>(
                response
            );

            // STEP 5: Confirm that this event belongs to the Organizer.
            bool ownsEvent = events?.Any(e => e.EventId == eventId) == true;

            if (!ownsEvent)
            {
                return NotFound("Event not found.");
            }

            // STEP 6: Prepare the form.
            var model = new CreateActivityViewModel
            {
                EventId = eventId,
                StartTime = new TimeOnly(7, 0)
            };

            return View(model);
        }


        // POST: /Activities/Create
        // Receives the activity form and sends it to our API.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateActivityViewModel model)
        {
            // STEP 1: Check whether the user is logged in.
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // STEP 2: Only Organizers can create activities.
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Organizer")
            {
                return StatusCode(403);
            }

            // STEP 3: Validate the submitted form.
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // STEP 4: Prepare the request body.
            // EventId belongs in the URL, not the JSON body.
            var request = new
            {
                model.ActivityName,
                model.DistanceKm,
                model.EntryFee,
                model.StartTime,
                model.MaximumEntrants
            };

            // STEP 5: Send the activity to the API.
            // RaceDayApiClient forwards the API session cookie.
            using var response = await _apiClient.SendAsync(
                HttpMethod.Post,
                $"api/events/{model.EventId}/categories",
                request
            );

            // STEP 6: Handle an expired API session.
            if (response.StatusCode ==
                System.Net.HttpStatusCode.Unauthorized)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Your API session has expired. Please log in again."
                );

                return View(model);
            }

            // STEP 7: Handle permission errors.
            if (response.StatusCode ==
                System.Net.HttpStatusCode.Forbidden)
            {
                return StatusCode(403);
            }

            // STEP 8: Handle a missing event.
            if (response.StatusCode ==
                System.Net.HttpStatusCode.NotFound)
            {
                return NotFound("Event not found.");
            }

            // STEP 9: Handle duplicate activities.
            if (response.StatusCode ==
                System.Net.HttpStatusCode.Conflict)
            {
                ModelState.AddModelError(
                    nameof(model.ActivityName),
                    "An activity with this name already exists for this event."
                );

                return View(model);
            }

            // STEP 10: Handle other API errors.
            if (!response.IsSuccessStatusCode)
            {
                string error =
                    await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    $"Unable to create activity. {error}"
                );

                return View(model);
            }

            // STEP 11: Activity created successfully!
            TempData["Success"] =
                "Activity created successfully.";

            // Return to the Organizer's events page.
            return RedirectToAction("MyEvents", "Events");
        }


        // GET: /Activities/Edit/3
        // Displays an edit form populated with existing activity details.
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // STEP 1: Check login and role.
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Role") != "Organizer")
                return StatusCode(403);

            if (id <= 0)
                return BadRequest("Invalid activity ID.");

            // STEP 2: Retrieve events belonging to this Organizer.
            using var eventsResponse = await _apiClient.SendAsync(
                HttpMethod.Get,
                "api/events/mine"
            );

            if (!eventsResponse.IsSuccessStatusCode)
                return StatusCode((int)eventsResponse.StatusCode);

            var myEvents =
                await _apiClient.ReadAsync<List<EventViewModel>>(
                    eventsResponse
                ) ?? new List<EventViewModel>();

            // STEP 3: Search the Organizer's events for this activity.
            // Our API lists activities by EventId, so we check each event.
            foreach (var eventItem in myEvents)
            {
                using var activitiesResponse = await _apiClient.SendAsync(
                    HttpMethod.Get,
                    $"api/events/{eventItem.EventId}/categories"
                );

                if (!activitiesResponse.IsSuccessStatusCode)
                    return StatusCode((int)activitiesResponse.StatusCode);

                var activities =
                    await _apiClient.ReadAsync<List<ActivityViewModel>>(
                        activitiesResponse
                    ) ?? new List<ActivityViewModel>();

                var activity = activities.FirstOrDefault(
                    a => a.ActivityId == id
                );

                if (activity == null)
                    continue;

                // STEP 4: Populate the edit form.
                var model = new EditActivityViewModel
                {
                    ActivityId = activity.ActivityId,
                    EventId = eventItem.EventId,
                    ActivityName = activity.ActivityName,
                    DistanceKm = activity.DistanceKm,
                    EntryFee = activity.EntryFee,
                    StartTime = activity.StartTime,
                    MaximumEntrants = activity.MaximumEntrants
                };

                return View(model);
            }

            return NotFound("Activity not found.");
        }


        // POST: /Activities/Edit/3
        // Receives the edited activity and updates it through the API.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            EditActivityViewModel model)
        {
            // STEP 1: Check whether the Organizer is logged in.
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // STEP 2: Only Organizers can edit activities.
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Organizer")
            {
                return StatusCode(403);
            }

            // STEP 3: Make sure the activity IDs match.
            // The ID in the URL must match the submitted form.
            if (id <= 0 || id != model.ActivityId)
            {
                return BadRequest("Invalid activity ID.");
            }

            // STEP 4: Validate the form fields.
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // STEP 5: Prepare the JSON request body.
            // Only send fields the API allows us to update.
            var request = new
            {
                model.ActivityName,
                model.DistanceKm,
                model.EntryFee,
                model.StartTime,
                model.MaximumEntrants
            };

            // STEP 6: Send the PUT request to our API.
            using var response = await _apiClient.SendAsync(
                HttpMethod.Put,
                $"api/categories/{id}",
                request
            );

            // STEP 7: Handle an expired API session.
            if (response.StatusCode ==
                System.Net.HttpStatusCode.Unauthorized)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Your session has expired. Please log in again."
                );

                return View(model);
            }

            // STEP 8: Handle authorization errors.
            if (response.StatusCode ==
                System.Net.HttpStatusCode.Forbidden)
            {
                return StatusCode(403);
            }

            // STEP 9: Handle missing activities.
            if (response.StatusCode ==
                System.Net.HttpStatusCode.NotFound)
            {
                return NotFound("Activity not found.");
            }

            // STEP 10: Handle duplicate activity names.
            if (response.StatusCode ==
                System.Net.HttpStatusCode.Conflict)
            {
                ModelState.AddModelError(
                    nameof(model.ActivityName),
                    "An activity with this name already exists for this event."
                );

                return View(model);
            }

            // STEP 11: Handle other API errors.
            if (!response.IsSuccessStatusCode)
            {
                string error =
                    await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    $"Unable to update activity. {error}"
                );

                return View(model);
            }

            // STEP 12: Activity updated successfully.
            TempData["Success"] =
                "Activity updated successfully!";

            // Return to the event's Details page.
            return RedirectToAction(
                "Details",
                "Events",
                new { id = model.EventId }
            );
        }
    }
}

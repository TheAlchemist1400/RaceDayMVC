
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;
using System.Net;

namespace RaceDay.Mvc.Controllers
{
    public class EnrolmentsController : Controller
    {
        private readonly RaceDayApiClient _apiClient;

        public EnrolmentsController(RaceDayApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // POST: /Enrolments/Create
        // Registers the logged-in Participant for an activity.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateEnrolmentViewModel model)
        {
            // STEP 1: Check login.
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            // STEP 2: Only Participants can enrol.
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Participant")
                return StatusCode(403);

            // STEP 3: Validate submitted IDs.
            if (!ModelState.IsValid)
                return BadRequest("Invalid event or activity.");

            // STEP 4: Send the registration request to the API.
            var request = new
            {
                model.EventId,
                model.ActivityId
            };

            using var response = await _apiClient.SendAsync(
                HttpMethod.Post,
                "api/enrolments",
                request
            );

            // STEP 5: Handle possible errors.
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                TempData["Error"] =
                    "Your session has expired. Please log in again.";

                return RedirectToAction("Login", "Auth");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
                return StatusCode(403);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return NotFound("Event not found.");

            if (response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.Conflict)
            {
                // API explains why registration was rejected.
                string error = await response.Content.ReadAsStringAsync();

                TempData["Error"] = error;

                return RedirectToAction(
                    "Details",
                    "Events",
                    new { id = model.EventId }
                );
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] =
                    "Unable to complete your enrolment. Please try again.";

                return RedirectToAction(
                    "Details",
                    "Events",
                    new { id = model.EventId }
                );
            }

            // STEP 6: Registration successful.
            TempData["Success"] =
                "You have successfully enrolled in this event!";

            return RedirectToAction(
                "Details",
                "Events",
                new { id = model.EventId }
            );
        }


        // GET: /Enrolments/MyEnrolments
        // Displays registrations belonging to the logged-in Participant.
        [HttpGet]
        public async Task<IActionResult> MyEnrolments()
        {
            // STEP 1: Check login.
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // STEP 2: Only Participants can access this page.
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Participant")
            {
                return StatusCode(403);
            }

            // STEP 3: Request this Participant's enrolments.
            // The API identifies the Participant using the session cookie.
            using var response = await _apiClient.SendAsync(
                HttpMethod.Get,
                "api/enrolments/mine"
            );

            // STEP 4: Handle an expired API session.
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Auth");
            }

            // STEP 5: Handle other API errors.
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode);
            }

            // STEP 6: Convert the API JSON into our ViewModel list.
            var enrolments =
                await _apiClient.ReadAsync<List<MyEnrolmentViewModel>>(
                    response
                );

            // STEP 7: Send the registrations to the Razor view.
            return View(enrolments ?? new List<MyEnrolmentViewModel>());
        }

    }
}

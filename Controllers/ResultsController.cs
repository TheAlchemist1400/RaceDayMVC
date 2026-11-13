
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;
using System.Net;

namespace RaceDay.Mvc.Controllers
{
    public class ResultsController : Controller
    {
        private readonly RaceDayApiClient _apiClient;

        public ResultsController(RaceDayApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // GET: /Results/Capture/5
        // The ID identifies an existing registration.
        [HttpGet]
        public async Task<IActionResult> Capture(int id, int eventId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Role") != "Organizer")
                return StatusCode(403);

            if (id <= 0 || eventId <= 0)
                return BadRequest("Invalid registration or event ID.");

            // Retrieve registrations through the API.
            // The API verifies event ownership.
            using var response = await _apiClient.SendAsync(
                HttpMethod.Get,
                $"api/enrolments/event/{eventId}"
            );

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Auth");

            if (response.StatusCode == HttpStatusCode.Forbidden)
                return StatusCode(403);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode);

            var registrations =
                await _apiClient.ReadAsync<List<EventRegistrationViewModel>>(
                    response
                ) ?? new List<EventRegistrationViewModel>();

            var registration = registrations.FirstOrDefault(
                r => r.RegistrationId == id
            );

            if (registration == null)
                return NotFound("Registration not found in this event.");

            if (registration.RegistrationStatus != "Confirmed")
            {
                TempData["Error"] =
                    "Only confirmed registrations can receive results.";

                return RedirectToAction(
                    "EventRegistrations",
                    "Enrolments",
                    new { id = eventId }
                );
            }

            var model = new CaptureResultViewModel
            {
                RegistrationId = registration.RegistrationId,
                EventId = eventId,
                ResultStatus = "Finished"
            };

            ViewBag.ParticipantName = registration.ParticipantName;
            ViewBag.ActivityName = registration.ActivityName;
            ViewBag.BibNumber = registration.BibNumber;

            return View(model);
        }

        // POST: /Results/Capture
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Capture(
            CaptureResultViewModel model)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Role") != "Organizer")
                return StatusCode(403);

            if (!ModelState.IsValid)
                return View(model);

            // Send only the fields expected by the API.
            var request = new
            {
                model.RegistrationId,
                model.FinishTime,
                model.Position,
                model.ResultStatus
            };

            using var response = await _apiClient.SendAsync(
                HttpMethod.Post,
                "api/results",
                request
            );

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Auth");

            if (response.StatusCode == HttpStatusCode.Forbidden)
                return StatusCode(403);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return NotFound("Registration not found.");

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                TempData["Error"] =
                    "A result already exists for this registration.";

                return RedirectToAction(
                    "EventRegistrations",
                    "Enrolments",
                    new { id = model.EventId }
                );
            }

            if (!response.IsSuccessStatusCode)
            {
                string error =
                    await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    $"Unable to capture result. {error}"
                );

                return View(model);
            }

            TempData["Success"] =
                "Participant result captured successfully!";

            return RedirectToAction(
                "EventRegistrations",
                "Enrolments",
                new { id = model.EventId }
            );
        }


        // GET: /Results/MyResults
        // Allows a Participant to view their own race results.
        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            // STEP 1: Check whether the user is logged in.
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

            // STEP 3: Request the Participant's results from API.
            using var response = await _apiClient.SendAsync(
                HttpMethod.Get,
                "api/results/mine"
            );

            // STEP 4: Handle expired API sessions.
            if (response.StatusCode ==
                System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Auth");
            }

            // STEP 5: Handle forbidden access.
            if (response.StatusCode ==
                System.Net.HttpStatusCode.Forbidden)
            {
                return StatusCode(403);
            }

            // STEP 6: Handle unexpected API errors.
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] =
                    "Unable to load your race results.";

                return View(new List<MyResultViewModel>());
            }

            // STEP 7: Deserialize the API response.
            var results =
                await _apiClient.ReadAsync<List<MyResultViewModel>>(
                    response
                ) ?? new List<MyResultViewModel>();

            // STEP 8: Display results.
            return View(results);
        }

    }
}

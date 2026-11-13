
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;
using System.Net;

namespace RaceDay.Mvc.Controllers
{
    public class ProfileController : Controller
    {
        private readonly RaceDayApiClient _apiClient;

        public ProfileController(RaceDayApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // GET: /Profile/Index
        // Display the logged-in Participant's profile.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Role") != "Participant")
                return StatusCode(403);

            using var response = await _apiClient.SendAsync(
                HttpMethod.Get,
                "api/profile/me"
            );

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Auth");

            if (response.StatusCode == HttpStatusCode.Forbidden)
                return StatusCode(403);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode);

            var profile =
                await _apiClient.ReadAsync<ParticipantProfileViewModel>(
                    response
                );

            if (profile == null)
                return NotFound("Profile not found.");

            return View(profile);
        }

        // POST: /Profile/Index
        // Update the logged-in Participant's profile.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            ParticipantProfileViewModel model)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Role") != "Participant")
                return StatusCode(403);

            if (!ModelState.IsValid)
                return View(model);

            // Send the exact fields expected by the API.
            var request = new
            {
                model.Name,
                model.Lastname,
                model.Email,
                model.PhoneNumber
            };

            using var response = await _apiClient.SendAsync(
                HttpMethod.Put,
                "api/profile/me",
                request
            );

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Auth");

            if (response.StatusCode == HttpStatusCode.Forbidden)
                return StatusCode(403);

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "This email address is already in use."
                );

                return View(model);
            }

            if (!response.IsSuccessStatusCode)
            {
                string error =
                    await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    $"Unable to update profile. {error}"
                );

                return View(model);
            }

            // Keep MVC session information synchronized.
            HttpContext.Session.SetString("DisplayName", model.Name);
            HttpContext.Session.SetString("Email", model.Email);

            TempData["Success"] = "Profile updated successfully!";

            return RedirectToAction(nameof(Index));
        }
    }
}


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


        // POST: /Profile/UploadImage
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(5 * 1024 * 1024 + 65536)]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            // STEP 1: Verify MVC session.
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            // STEP 2: Only Participants can upload profile pictures.
            if (HttpContext.Session.GetString("Role") != "Participant")
                return StatusCode(403);

            // STEP 3: Check that a file was selected.
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select an image.";
                return RedirectToAction(nameof(Index));
            }

            // STEP 4: Validate file size.
            const long maxFileSize = 5 * 1024 * 1024;

            if (file.Length > maxFileSize)
            {
                TempData["Error"] = "Image must not exceed 5 MB.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // STEP 5: Send the image to RaceDay API.
                using var response = await _apiClient.UploadImageAsync(
                    "api/profile/upload-image",
                    file
                );

                // STEP 6: Handle expired API sessions.
                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Auth");
                }

                // STEP 7: Handle forbidden access.
                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Forbidden)
                {
                    return StatusCode(403);
                }

                // STEP 8: Handle API validation errors.
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] =
                        "Unable to upload profile picture. " +
                        "Please check the image format and try again.";

                    return RedirectToAction(nameof(Index));
                }

                // STEP 9: Upload successful.
                TempData["Success"] =
                    "Profile picture uploaded successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "Could not connect to the RaceDay API.";

                return RedirectToAction(nameof(Index));
            }
        }

    }
}


using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers
{
    public class AuthController : Controller
    {
        private readonly RaceDayApiClient _apiClient;

        public AuthController(RaceDayApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // ====================================================================================
        // REGISTER

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Validate fields according to selected role
            if (model.Role == "Participant")
            {
                if (string.IsNullOrWhiteSpace(model.Name))
                    ModelState.AddModelError(nameof(model.Name),
                        "First name is required.");

                if (string.IsNullOrWhiteSpace(model.Lastname))
                    ModelState.AddModelError(nameof(model.Lastname),
                        "Last name is required.");
            }
            else if (model.Role == "Organizer")
            {
                if (string.IsNullOrWhiteSpace(model.OrganizationName))
                    ModelState.AddModelError(nameof(model.OrganizationName),
                        "Organization name is required.");
            }
            else
            {
                ModelState.AddModelError(nameof(model.Role),
                    "Please select a valid account type.");
            }

            if (!ModelState.IsValid)
                return View(model);

            // Convert website form data into API request
            var request = new RegisterRequest
            {
                Email = model.Email,
                Password = model.Password,
                Role = model.Role,
                Name = model.Name,
                Lastname = model.Lastname,
                OrganizationName = model.OrganizationName,
                PhoneNumber = model.PhoneNumber
            };

            using var response = await _apiClient.SendAsync(
                HttpMethod.Post,
                "api/auth/register",
                request
            );

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(string.Empty, error);

                return View(model);
            }

            TempData["Success"] =
                "Registration successful. You can now log in.";

            return RedirectToAction(nameof(Login));
        }

        // ====================================================================================
        // LOGIN

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var request = new LoginRequest
            {
                Email = model.Email,
                Password = model.Password
            };

            using var response = await _apiClient.SendAsync(
                HttpMethod.Post,
                "api/auth/login",
                request
            );

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password."
                );

                return View(model);
            }

            var authResponse =
                await _apiClient.ReadAsync<AuthResponse>(response);

            if (authResponse == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to process the login response."
                );

                return View(model);
            }

            // Extract the API session cookie
            string? apiCookie =
                RaceDayApiClient.ExtractSessionCookie(response);

            if (string.IsNullOrWhiteSpace(apiCookie))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The API did not return a session cookie."
                );

                return View(model);
            }

            // Save authenticated user information in MVC session
            HttpContext.Session.SetInt32(
                "UserId",
                authResponse.UserId
            );

            HttpContext.Session.SetString(
                "Role",
                authResponse.Role
            );

            HttpContext.Session.SetString(
                "DisplayName",
                authResponse.DisplayName
            );

            HttpContext.Session.SetString(
                "Email",
                authResponse.Email
            );

            HttpContext.Session.SetString(
                "ApiSessionCookie",
                apiCookie
            );

            return RedirectToAction("Index", "Home");
        }

        // ====================================================================================
        // LOGOUT

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                using var response = await _apiClient.SendAsync(
                    HttpMethod.Post,
                    "api/auth/logout"
                );
            }
            finally
            {
                // Always clear the MVC session
                HttpContext.Session.Clear();
            }

            return RedirectToAction(nameof(Login));
        }
    }
}

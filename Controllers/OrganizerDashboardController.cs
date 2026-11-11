
using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Mvc.Controllers
{
    public class OrganizerDashboardController : Controller
    {
        public IActionResult Index()
        {
            // Step 1: Check whether someone is logged in.
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Step 2: Check the user's role.
            string? role = HttpContext.Session.GetString("Role");

            // Only Organizers may access this dashboard.
            if (role != "Organizer")
            {
                return Forbid();
            }

            // Step 3: Retrieve the Organizer's information.
            ViewBag.DisplayName = HttpContext.Session.GetString("DisplayName");
            ViewBag.Email = HttpContext.Session.GetString("Email");

            // Step 4: Display the dashboard.
            return View();
        }
    }
}

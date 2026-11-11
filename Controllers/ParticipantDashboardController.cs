
using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Mvc.Controllers
{
    public class ParticipantDashboardController : Controller
    {
        public IActionResult Index()
        {
            // Check whether someone is logged in.
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Check whether the logged-in user is a participant.
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Participant")
            {
                return Forbid();
            }

            return View();
        }
    }
}

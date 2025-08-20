using Microsoft.AspNetCore.Mvc;

namespace HealthFit.Controllers
{
    public class ManagerRoleController : Controller
    {
        [HttpGet]
        public IActionResult SystemAdmin()
        {
            // Chuyển hướng vào Area SystemAdmin
            return RedirectToAction("SystemAdmin", "ManagerRole", new { area = "SystemAdmin" });
        }
    }
} 
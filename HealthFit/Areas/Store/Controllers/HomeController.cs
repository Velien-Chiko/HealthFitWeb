using Microsoft.AspNetCore.Mvc;

namespace HealthFit.Areas.Store.Controllers
{
    [Area("Store")]

    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

    }
}

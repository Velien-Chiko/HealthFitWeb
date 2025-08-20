using System.Diagnostics;
using HealthFit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthFit.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;


        
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        
        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }


        [Authorize(Roles = "SystemAdmin")]
        public IActionResult SystemAdmin()
        {
            return View();
        }

        [Authorize(Roles = "Customer")]
        public IActionResult Customer()
        {
            return View();
        }

        [Authorize(Roles = "Nutri")]
        public IActionResult Nutri()
        {
            return View();
        }
        [Authorize(Roles = "Seller")]
        public IActionResult Seller()
        {
            return View();
        }
        [Authorize(Roles = "Manager")]
        public IActionResult Manager()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult BMICalculator()
        {
            return RedirectToAction("Index", "BMICalculator");
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult BMICalculator(string HeightUnit, double Height, double Weight, string Gender)
        {
            double heightInMeters = HeightUnit switch
            {
                "CM" => Height / 100.0,
                "Inches" => Height * 0.0254,
                "Feet" => Height * 0.3048,
                _ => 0
            };

            double bmi = 0;
            string status = "";
            double idealWeight = 0;

            if (heightInMeters > 0)
            {
                bmi = Weight / (heightInMeters * heightInMeters);
                if (bmi < 18.5) status = "Underweight";
                else if (bmi < 23) status = "Normal weight";
                else if (bmi < 25) status = "Overweight";
                else status = "Obese";

                // Devine formula for ideal weight
                double heightInInches = HeightUnit == "CM" ? Height / 2.54 : (HeightUnit == "Feet" ? Height * 12 : Height);
                if (Gender == "MALE")
                    idealWeight = 50 + 2.3 * (heightInInches - 60);
                else
                    idealWeight = 45.5 + 2.3 * (heightInInches - 60);
                idealWeight = Math.Round(idealWeight * 0.453592, 1); // to kg
            }

            ViewBag.BMI = heightInMeters > 0 ? Math.Round(bmi, 2).ToString() : "";
            ViewBag.Status = status;
            ViewBag.IdealWeight = idealWeight > 0 ? idealWeight.ToString() : "";

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

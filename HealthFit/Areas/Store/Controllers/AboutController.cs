using HealthFit.Models.ViewModels;
using HealthFit.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HealthFit.Areas.Store.Controllers
{
    [Area("Store")]
    public class AboutController : Controller
    {
        private readonly IProductService _productService;
        private readonly IMealPlanService _mealPlanService;
        private readonly IOrderService _orderService;

        public AboutController(IProductService productService, IMealPlanService mealPlanService, IOrderService orderService)
        {
            _productService = productService;
            _mealPlanService = mealPlanService;
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            var mealPlans = await _mealPlanService.GetAllMealPlanDetailsAsync();
            var orders = await _orderService.GetAll();

            var model = new AboutViewModel
            {
                ProductCount = products.Count(),
                MealPlanCount = mealPlans.Count(),
                OrderCount = orders.Count()
            };

            return View(model);
        }
    }
}

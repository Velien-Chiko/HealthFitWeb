using HealthFit.Models.Constants;
using HealthFit.Models.ViewModels;
using HealthFit.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthFit.Areas.Manager.Controllers
{
    [Area("Manager")]
    [Authorize(Roles = "Manager")]

    public class DashboardController : Controller
    {
        private readonly IProductService _productService;
        private readonly IMealPlanService _mealPlanService;
        private readonly IOrderService _orderService;

        public DashboardController(IProductService productService, IMealPlanService mealPlanService, IOrderService orderService)
        {
            _productService = productService;
            _mealPlanService = mealPlanService;
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var year = now.Year;
            var month = now.Month;

            var orders = await _orderService.GetAll();
            var currentMonthRevenue = orders
        .Where(o => o.OrderDate.HasValue &&
                    o.OrderDate.Value.Year == year &&
                    o.OrderDate.Value.Month == month &&
                    o.Status == "Completed")
                    .Sum(o => o.TotalAmount);
            var products = await _productService.GetAllProductsAsync();
            int pendingProducts = products.Count(p => p.IsActive == ProductStatusConstants.Pending);

            var mealPlans = await _mealPlanService.GetAllMealPlanDetailsAsync();
            int pendingMealPlans = mealPlans.Count(m => m.IsApproved == ProductStatusConstants.Pending);

            var vm = new DashboardViewModel
            {
                CurrentMonthRevenue = currentMonthRevenue,
                PendingProducts = pendingProducts,
                PendingMealPlans = pendingMealPlans
            };

            return View(vm);


        }


    }
}

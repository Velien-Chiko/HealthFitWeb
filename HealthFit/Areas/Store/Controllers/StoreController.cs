using HealthFit.Models.Constants;
using HealthFit.Models.ViewModels;
using HealthFit.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HealthFit.Areas.Store.Controllers
{
    [Area("Store")]
    public class StoreController : Controller
    {
        private readonly IProductService _productService;
        private readonly IMealPlanService _mealPlanService;

        public StoreController(IProductService productService, IMealPlanService mealPlanService)
        {
            _productService = productService;
            _mealPlanService = mealPlanService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            var mealPlans = await _mealPlanService.GetAllMealPlanDetailsAsync();

            // Lọc chỉ các sản phẩm và meal plan đã "Approved"
            var approvedProducts = products.Where(p => p.IsActive == ProductStatusConstants.Approved).ToList();
            var approvedMealPlans = mealPlans.Where(m => m.IsApproved == ProductStatusConstants.Approved).ToList();

            var viewModel = new ProductAndMealPlanViewModel
            {
                Products = approvedProducts,
                MealPlans = approvedMealPlans
            };

            return View(viewModel);
        }

    }
}

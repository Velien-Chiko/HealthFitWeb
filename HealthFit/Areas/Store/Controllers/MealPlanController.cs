using HealthFit.Models.ViewModels;
using HealthFit.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using X.PagedList.Extensions;

namespace HealthFit.Areas.Store.Controllers
{
    [Area("Store")]
    public class MealPlanController : Controller
    {
        private readonly IMealPlanService _mealPlanService;

        public MealPlanController(IMealPlanService mealPlanService)
        {
            _mealPlanService = mealPlanService;
        }

        public async Task<IActionResult> Index(string searchString, string? bmi, int? page, string? priceRange, string? approvalStatus = "Approved")
        {
            try
            {
                var model = new MealPlanSearchViewModel
                {
                    SearchString = searchString ?? string.Empty,
                    BMI = bmi,
                    ApprovalStatus = approvalStatus,
                    PriceRange = priceRange
                };

                var mealPlans = await _mealPlanService.SearchMealPlanDetailsAsync(searchString, bmi, approvalStatus, priceRange);

                int pageNumber = page ?? 1;
                int pageSize = 8;

                model.MealPlans = mealPlans.ToPagedList(pageNumber, pageSize);

                return View("Index", model);
            }
            catch (ArgumentException ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }



        public async Task<IActionResult> MealPlanSingle(int id)
        {
            var mealPlan = await _mealPlanService.GetMealPlanWithProductsAsync(id);
            if (mealPlan == null)
                return NotFound();

            // Tính số tiền tiết kiệm
            var saving = await _mealPlanService.CalculateSavingsForMealPlanAsync(id);
            mealPlan.SavingsAmount = saving;

            return View(mealPlan);
        }


    }
}

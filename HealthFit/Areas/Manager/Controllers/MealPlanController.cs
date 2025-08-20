using HealthFit.Models.Constants;
using HealthFit.Models.ViewModels;
using HealthFit.Services.Interfaces;
using HealthFit.Services.Interfaces.Export;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList.Extensions;


namespace HealthFit.Areas.Manager.Controllers
{
    [Area("Manager")]
    [Authorize(Roles = "Manager")]
    public class MealPlanController : Controller
    {
        private readonly IMealPlanService _mealPlanService;
        private readonly IMealPlanExportService _mealPlanExportService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MealPlanController(IMealPlanService mealPlanService, IMealPlanExportService mealPlanExportService, IWebHostEnvironment webHostEnvironment)
        {
            this._mealPlanService = mealPlanService;
            this._mealPlanExportService = mealPlanExportService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string searchString, string? bmi, string? approvalStatus, int? page, string? priceRange)
        {
            try
            {
                var model = new MealPlanSearchViewModel
                {
                    SearchString = searchString ?? string.Empty,
                    BMI = bmi,
                    ApprovalStatus = approvalStatus
                };

                var mealPlans = await _mealPlanService.SearchMealPlanDetailsAsync(searchString, bmi, approvalStatus, priceRange);

                if (string.IsNullOrEmpty(approvalStatus))
                {
                    mealPlans = mealPlans
                        .Where(m => m.IsApproved == ProductStatusConstants.Approved || m.IsApproved == ProductStatusConstants.Pending)
                        .OrderByDescending(m => m.IsApproved == ProductStatusConstants.Pending)
                        .ThenByDescending(m => m.MealPlanDetailId);
                }
                else
                {
                    mealPlans = mealPlans
                        .OrderByDescending(m => m.MealPlanDetailId);
                }

                int pageNumber = page ?? 1;
                int pageSize = 5;

                model.MealPlans = mealPlans.ToPagedList(pageNumber, pageSize);

                if (!string.IsNullOrWhiteSpace(searchString) || !string.IsNullOrWhiteSpace(bmi) || !string.IsNullOrWhiteSpace(approvalStatus))
                {
                    TempData["success"] = "Tìm kiếm kế hoạch bữa ăn thành công";
                }

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["error"] = "Có lỗi xảy ra: " + ex.Message;
                return View(new MealPlanSearchViewModel());
            }
        }


        public async Task<IActionResult> ViewProducts(int id)
        {
            try
            {
                var product = await _mealPlanService.GetMealPlanWithProductsAsync(id);
                if (product == null)
                {
                    return NotFound();
                }
                return View(product);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi ở đây nếu cần
                TempData["Error"] = "Không thể lấy chi tiết thực đơn";
                return RedirectToAction(nameof(Index));
            }
        }


        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var mealPlan = await _mealPlanService.GetMealPlanEditByIdAsync(id);
                if (mealPlan == null)
                {
                    TempData["error"] = "Không tìm thấy kế hoạch bữa ăn!";
                    return RedirectToAction(nameof(Index));
                }

                return View(mealPlan); // Trả về View Edit.cshtml
            }
            catch (Exception ex)
            {
                TempData["error"] = "Có lỗi xảy ra!";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MealPlanEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _mealPlanService.UpdateMealPlanInfoAsync(model);
                TempData["success"] = "Cập nhật thành công!";
                return RedirectToAction("Index");
            }
            catch (ArgumentException argEx)
            {
                // Nếu lỗi liên quan đến ảnh, hiển thị ngay dưới trường NewImage
                if (argEx.Message.Contains("Ảnh"))
                {
                    ModelState.AddModelError("NewImage", argEx.Message);
                }
                else
                {
                    ModelState.AddModelError("", argEx.Message);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi không xác định: " + ex.Message);
                return View(model);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string newStatus)
        {
            try
            {
                await _mealPlanService.UpdateStatusMealPlanAsync(id, newStatus);
                TempData["success"] = "✔️ Cập nhật trạng thái thành công!";
            }
            catch (ArgumentException argEx)
            {
                TempData["error"] = "⚠️ " + argEx.Message;
            }
            catch (Exception ex)
            {
                TempData["error"] = "❌ Có lỗi xảy ra khi cập nhật trạng thái.";
                // Nếu bạn muốn ghi log ở đây thì thêm _logger.LogError(ex, ...) nhé
            }

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> ExportToExcel(string? searchString, string? bmi, string? approvalStatus, string? priceRange)
        {

            var mealPlans = await _mealPlanService.SearchMealPlanDetailsAsync(searchString, bmi, approvalStatus, priceRange);
            if (mealPlans == null)
            {
                TempData["error"] = "Không có kế hoạch bữa ăn nào phù hợp để xuất Excel.";
                return RedirectToAction("Index");
            }

            var stream = _mealPlanExportService.ExportMealPlansToExcel(mealPlans);

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "DanhSachKeHoachBuaAn.xlsx");
        }


    }
}

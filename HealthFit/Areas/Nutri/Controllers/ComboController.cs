using System.Text.RegularExpressions;
using HealthFit.DataAccess.Data;
using HealthFit.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace HealthFit.Areas.Nutri.Controllers
{
    [Area("Nutri")]
    public class ComboController : Controller
    {
        private readonly HealthyShopContext _context;

        public ComboController(HealthyShopContext context)
        {
            _context = context;
        }

        // ✅ INDEX - HIỂN THỊ DANH SÁCH COMBO CÓ PHÂN TRANG
        public IActionResult Index(string? searchTerm, int? page)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var combos = _context.MealPlanDetails
                .Include(m => m.MealPlanProductDetails)
                .ThenInclude(mp => mp.Product)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                combos = combos.Where(c => c.PlanDescription.Contains(searchTerm));
            }

            var pagedCombos = combos
                .OrderByDescending(c => c.MealPlanDetailId)
                .ToPagedList(pageNumber, pageSize);

            ViewBag.SearchTerm = searchTerm;
            return View(pagedCombos);
        }


        // ✅ HIỂN THỊ FORM TẠO COMBO
        public IActionResult CreateCombo()
        {
            ViewBag.Products = _context.Products.ToList();
            return View();
        }

        // ✅ XỬ LÝ TẠO COMBO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCombo(
            string bmirange,
            string planDescription,
            IFormFile? imageFile,
            List<int> productIds,
            List<int> quantities)
        {
            var combo = new MealPlanDetail
            {
                Bmirange = bmirange,
                PlanDescription = planDescription
            };

            if (!ValidateComboInput(combo, productIds, ModelState))
            {
                ViewBag.Products = _context.Products.ToList();
                return View();
            }

            combo.ImageUrl = await SaveImageAsync(imageFile, "combo");
            combo.Price = 0;
            combo.IsApproved = "Pending";

            _context.MealPlanDetails.Add(combo);
            await _context.SaveChangesAsync();

            combo.MealPlanProductDetails = new List<MealPlanProductDetail>();

            for (int i = 0; i < productIds.Count; i++)
            {
                combo.MealPlanProductDetails.Add(new MealPlanProductDetail
                {
                    ProductId = productIds[i],
                    Quantity = quantities[i]
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // ✅ HIỂN THỊ FORM EDIT COMBO
        public IActionResult Edit(int id)
        {
            var combo = _context.MealPlanDetails
                .Include(m => m.MealPlanProductDetails)
                .FirstOrDefault(m => m.MealPlanDetailId == id);

            if (combo == null) return NotFound();

            ViewBag.Products = _context.Products.ToList();
            ViewBag.SelectedProductIds = combo.MealPlanProductDetails.Select(mp => mp.ProductId).ToList();

            return View(combo);
        }

        // ✅ XỬ LÝ EDIT COMBO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string isapproved,
            string bmirange,
            string planDescription,
            IFormFile? imageFile,
            List<int> productIds,
            List<int> quantities)
        {
            var combo = await _context.MealPlanDetails
                .Include(m => m.MealPlanProductDetails)
                .FirstOrDefaultAsync(m => m.MealPlanDetailId == id);

            if (combo == null) return NotFound();

            combo.Bmirange = bmirange;
            combo.PlanDescription = planDescription;
            combo.IsApproved = "Pending";

            if (!ValidateComboInput(combo, productIds, ModelState))
            {
                ViewBag.Products = _context.Products.ToList();
                ViewBag.SelectedProductIds = productIds;
                return View(combo);
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                combo.ImageUrl = await SaveImageAsync(imageFile, "combo");
            }

            // Cập nhật danh sách sản phẩm
            _context.MealPlanProductDetails.RemoveRange(combo.MealPlanProductDetails);

            

            combo.MealPlanProductDetails = new List<MealPlanProductDetail>();

            for (int i = 0; i < productIds.Count; i++)
            {
                combo.MealPlanProductDetails.Add(new MealPlanProductDetail
                {
                    ProductId = productIds[i],
                    Quantity = quantities[i]
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // ✅ FORM XOÁ
        public async Task<IActionResult> Delete(int id)
        {
            var combo = await _context.MealPlanDetails
                .Include(m => m.MealPlanProductDetails)
                .FirstOrDefaultAsync(m => m.MealPlanDetailId == id);

            if (combo == null) return NotFound();

            return View(combo);
        }

        // ✅ XỬ LÝ XOÁ
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var combo = await _context.MealPlanDetails
                .Include(m => m.MealPlanProductDetails)
                .FirstOrDefaultAsync(m => m.MealPlanDetailId == id);

            if (combo == null) return NotFound();

            _context.MealPlanProductDetails.RemoveRange(combo.MealPlanProductDetails);
            _context.MealPlanDetails.Remove(combo);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ✅ HÀM CHUNG: VALIDATE DỮ LIỆU
        private bool ValidateComboInput(MealPlanDetail combo, List<int> productIds, ModelStateDictionary modelState)
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(combo.PlanDescription))
            {
                modelState.AddModelError("PlanDescription", "Mô tả không được để trống.");
                isValid = false;
            }
            else
            {
                var regex = new Regex(@"^[\p{L}0-9\s\.\-\(\)]+$");
                if (!regex.IsMatch(combo.PlanDescription))
                {
                    modelState.AddModelError("PlanDescription", "Mô tả không được chứa ký tự đặc biệt.");
                    isValid = false;
                }
            }

            if (string.IsNullOrWhiteSpace(combo.Bmirange))
            {
                modelState.AddModelError("Bmirange", "Vui lòng chọn mức BMI.");
                isValid = false;
            }

            if (productIds == null || !productIds.Any())
            {
                modelState.AddModelError("productIds", "Vui lòng chọn ít nhất 1 sản phẩm.");
                isValid = false;
            }

            return isValid;
        }

        // ✅ HÀM CHUNG: LƯU ẢNH
        private async Task<string?> SaveImageAsync(IFormFile? imageFile, string folder = "combo")
        {
            if (imageFile == null || imageFile.Length == 0) return null;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/images/{folder}");
            Directory.CreateDirectory(uploadsFolder);
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/images/{folder}/{fileName}";
        }
    }
}

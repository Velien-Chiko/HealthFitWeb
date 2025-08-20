using HealthFit.Models;
using HealthFit.Models.ViewModels;
using HealthFit.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthFit.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _productService.SearchProducts("");
                return View(products);
            }
            catch (Exception ex)
            {
                TempData["error"] = "Lỗi khi tải danh sách sản phẩm: " + ex.Message;
                return View(new List<Product>());
            }
        }

        public async Task<IActionResult> Search(string searchString)
        {
            try
            {
                var products = await _productService.SearchProducts(searchString);
                return View("Index", products);
            }
            catch (Exception ex)
            {
                TempData["error"] = "Lỗi khi tìm kiếm sản phẩm: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productService.GetProductById(id.Value);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _productService.UpdateProduct(product, imageFile);
                    if (result)
                    {
                        TempData["success"] = "Cập nhật sản phẩm thành công";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["error"] = "Cập nhật sản phẩm thất bại";
                    }
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Lỗi khi cập nhật sản phẩm: " + ex.Message;
                }
            }
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _productService.DeleteProduct(id);
                if (result)
                {
                    TempData["success"] = "Xóa sản phẩm thành công";
                }
                else
                {
                    TempData["error"] = "Không tìm thấy sản phẩm để xóa";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "Lỗi khi xóa sản phẩm: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var result = await _productService.ToggleProductStatus(id);
                if (result)
                {
                    TempData["success"] = "Cập nhật trạng thái sản phẩm thành công";
                }
                else
                {
                    TempData["error"] = "Không tìm thấy sản phẩm để cập nhật trạng thái";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "Lỗi khi cập nhật trạng thái sản phẩm: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
} 
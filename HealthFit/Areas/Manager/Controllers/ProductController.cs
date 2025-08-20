using HealthFit.Models;
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
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IProductExportService _productExportService;

        public ProductController(IProductService productService, IProductExportService productExportService)
        {
            _productService = productService;
            _productExportService = productExportService;
        }

        public async Task<IActionResult> Index(
                   string? SearchString,
                   string? showHidden,
                   string? caloRange,
                   int? page,
                   string? priceRange,
                   int? categoryId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(showHidden))
                {
                    showHidden = null;
                }

                var model = new ProductSearchViewModel
                {
                    SearchString = SearchString,
                    ShowHidden = showHidden,
                    CaloRange = caloRange,
                    PriceRange = priceRange,
                    CategoryId = categoryId
                };


                var productsRaw = await _productService.SearchProductsAsync(SearchString ?? "", showHidden, caloRange, priceRange, categoryId);


                List<Product> products;

                if (string.IsNullOrEmpty(showHidden))
                {
                    products = productsRaw
                        .Where(p => p.IsActive == ProductStatusConstants.Approved || p.IsActive == ProductStatusConstants.Pending)
                        .OrderByDescending(p => p.IsActive == ProductStatusConstants.Pending)
                        .ThenByDescending(p => p.ProductId)
                        .ToList();
                }
                else
                {
                    products = productsRaw
                      .OrderByDescending(p => p.ProductId)
                      .ToList();
                }


                int pageNumber = page ?? 1;
                int pageSize = 5;
                model.ProductList = products.ToPagedList(pageNumber, pageSize);


                model.TotalCount = productsRaw.Count();
                model.PendingCount = productsRaw.Count(p => p.IsActive == ProductStatusConstants.Pending);
                model.ApprovedCount = productsRaw.Count(p => p.IsActive == ProductStatusConstants.Approved);
                model.RejectedCount = productsRaw.Count(p => p.IsActive == ProductStatusConstants.Rejected);


                model.Categories = await _productService.GetProductCategories();

                if (!string.IsNullOrWhiteSpace(SearchString) || !string.IsNullOrWhiteSpace(showHidden) || !string.IsNullOrWhiteSpace(caloRange))
                {
                    TempData["success"] = "Tìm kiếm sản phẩm thành công";
                }

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Có lỗi xảy ra khi tải danh sách sản phẩm: {ex.Message}";
                return View(new ProductSearchViewModel());
            }
        }



        public IActionResult Create()
        {
            return View(new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? file)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ModelState.AddModelError("", "Thông tin không hợp lệ. Vui lòng kiểm tra lại.");
                    return View(product);
                }

                await _productService.CreateProductAsync(product, file);
                TempData["success"] = "Thêm sản phẩm thành công";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Hiển thị lỗi từ service vào ModelState để hiển thị trong form
                ModelState.AddModelError("", ex.Message);
                return View(product);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var vm = await _productService.GetProductEditViewModelAsync(id);
                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    foreach (var entry in ModelState)
                    {
                        foreach (var error in entry.Value.Errors)
                        {
                            Console.WriteLine($"❌ Field: {entry.Key} - Error: {error.ErrorMessage}");
                        }
                    }
                    return View(model);
                }

                await _productService.UpdateProductBasicInfoAsync(model);

                TempData["success"] = "Cập nhật sản phẩm thành công";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(nameof(model.NewImage), ex.Message);
                return View(model);
            }
        }



        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    TempData["error"] = "ID sản phẩm không hợp lệ.";
                    return RedirectToAction("Index");
                }

                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    TempData["error"] = "Không tìm thấy sản phẩm.";
                    return RedirectToAction("Index");
                }

                await _productService.DeleteProductAsync(id);
                TempData["success"] = "Xóa sản phẩm thành công";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Có lỗi xảy ra khi xóa sản phẩm: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ChangeStatus(int id, string newStatus)
        {
            try
            {
                await _productService.UpdateStatusAsync(id, newStatus);
                TempData["success"] = $"Đã chuyển trạng thái sản phẩm sang \"{newStatus}\" thành công.";
            }
            catch (ArgumentException argEx)
            {
                TempData["error"] = argEx.Message;
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Có lỗi xảy ra khi thay đổi trạng thái sản phẩm: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ExportToExcel(
              string? searchString, string? showHidden, string? caloRange, string? priceRange, int? categoryId)
        {
            var products = await _productService.SearchProductsAsync(searchString, showHidden, caloRange, priceRange, categoryId);
            if (products == null)
            {
                TempData["error"] = "Không có sản phẩm nào phù hợp để xuất Excel.";
                return RedirectToAction("Index");
            }
            var stream = _productExportService.ExportProductsToExcel(products);

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "DanhSachSanPham.xlsx");
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                if (id < 0)
                {
                    TempData["error"] = "ID sản phẩm không hợp lệ.";
                    return RedirectToAction("Index");
                }
                var product = await _productService.GetProductDetailAsync(id);
                if (product == null)
                {
                    TempData["error"] = "Không tìm thấy sản phẩm.";
                    return RedirectToAction("Index");
                }
                return View(product);
            }
            catch (Exception ex)
            {

                TempData["error"] = $"Có lỗi xảy ra xen trạng thái sản phầm : {ex.Message}";
                return RedirectToAction("Index");
            }
        }


    }
}



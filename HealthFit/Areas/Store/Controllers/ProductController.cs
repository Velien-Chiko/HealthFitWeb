using HealthFit.Models.ViewModels;
using HealthFit.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using X.PagedList.Extensions;

namespace HealthFit.Areas.Store.Controllers
{
    [Area("Store")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;

        }
        public async Task<IActionResult> Index(
                  string SearchString, string? showHidden, string? caloRange, int? page, string? priceRange, int? categoryId)
        {
            try
            {
                var model = new ProductSearchViewModel
                {
                    SearchString = SearchString,
                    ShowHidden = "Approved",
                    CaloRange = caloRange,
                    PriceRange = priceRange,
                    CategoryId = categoryId
                };

                // Tìm kiếm sản phẩm
                var products = await _productService.SearchProductsAsync(SearchString, model.ShowHidden, caloRange, priceRange, categoryId);

                // Phân trang
                var pageNumber = page ?? 1;
                var pageSize = 8;
                model.ProductList = products.ToPagedList(pageNumber, pageSize);
                model.Categories = await _productService.GetProductCategories();
                model.TotalCount = products.Count();


                return View(model);
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Có lỗi xảy ra khi tải danh sách sản phẩm: {ex.Message}";
                return View(new ProductSearchViewModel());
            }
        }

        public async Task<IActionResult> ProductSingle(int id)
        {
            var product = await _productService.GetProductDetailAsync(id);
            if (product == null)
                return NotFound();

            return View(product);
        }
    }
}

using System.Security.Claims;
using System.Text.RegularExpressions;
using HealthFit.DataAccess.Data;
using HealthFit.Models;
using HealthFit.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace HealthFit.Areas.Nutri.Controllers
{
    [Area("Nutri")]
    public class CreateDishController : Controller
    {
        private readonly HealthyShopContext _db;

        public CreateDishController(HealthyShopContext db)
        {
            _db = db;
        }

        // ✅ HÀM KIỂM TRA KÝ TỰ ĐẶC BIỆT
        private bool ContainsSpecialChars(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Chỉ cho phép chữ cái, số, tiếng Việt, khoảng trắng và một số dấu hợp lệ
            return !Regex.IsMatch(input, @"^[a-zA-Z0-9À-ỹà-ỹ\s\.,'-]+$");
        }

        public IActionResult Index(string searchString, string status, int? categoryId, int? page)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = status;
            ViewData["CurrentCategory"] = categoryId;

            var dishes = _db.Products
                .Include(p => p.CreatedByNavigation)
                .Include(p => p.ProductCategoryMappings)
                    .ThenInclude(m => m.Category) // Lấy CategoryName qua Mapping
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                dishes = dishes.Where(d => d.IsActive == status);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                dishes = dishes.Where(d => d.Name.Contains(searchString));
            }

            if (categoryId != null)
            {
                dishes = dishes.Where(d => d.ProductCategoryMappings.Any(m => m.CategoryId == categoryId));
            }

            int pageSize = 5;
            int pageNumber = page ?? 1;

            var pagedDishes = dishes.OrderByDescending(d => d.ProductId)
                                    .ToPagedList(pageNumber, pageSize);

            if (!pagedDishes.Any() && !string.IsNullOrEmpty(searchString))
            {
                TempData["error"] = "Không tìm thấy sản phẩm phù hợp.";
            }

            return View(pagedDishes);
        }




        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_db.ProductCategories
                .Where(c => c.CategoryName == "Món mặn" || c.CategoryName == "Tráng miệng" || c.CategoryName == "Đồ uống")
                .ToList(), "CategoryId", "CategoryName");

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? file, int CategoryId)
        {
            if (ModelState.IsValid)
            {
                // ✅ Kiểm tra đặc biệt và calo
                if (ContainsSpecialChars(product.Name))
                {
                    ModelState.AddModelError("Name", "Tên sản phẩm không được chứa ký tự đặc biệt.");
                    return View(product);
                }

                if (product.Calo < 0 || product.Calo >= 1000)
                {
                    ModelState.AddModelError("Calories", "Lượng calo phải từ 0 đến dưới 1000.");
                    return View(product);
                }

                // ✅ Người tạo
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userIdClaim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    TempData["error"] = "Bạn phải đăng nhập để tạo sản phẩm.";
                    return RedirectToAction(nameof(Index));
                }

                product.CreatedBy = int.Parse(userIdClaim.Value);
                product.IsActive = "Pending";
                product.Price = 0;

                // ✅ Lưu ảnh nếu có
                if (file != null)
                {
                    string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, "images", "products");

                    if (!Directory.Exists(productPath))
                    {
                        Directory.CreateDirectory(productPath);
                    }

                    using var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create);
                    await file.CopyToAsync(fileStream);

                    product.ImageUrl = "/images/products/" + fileName;
                }

                // ✅ Lưu sản phẩm
                _db.Products.Add(product);
                await _db.SaveChangesAsync();

                // ✅ Tạo mapping ProductCategory
                var mapping = new ProductCategoryMapping
                {
                    ProductId = product.ProductId,
                    CategoryId = CategoryId
                };
                _db.ProductCategoryMappings.Add(mapping);
                await _db.SaveChangesAsync();

                TempData["success"] = "Sản phẩm đã tạo và gắn danh mục thành công.";
                return RedirectToAction(nameof(Index));
            }

            // Nếu lỗi, gửi lại categories
            ViewBag.Categories = new SelectList(_db.ProductCategories
                .Where(c => c.CategoryName == "Món mặn" || c.CategoryName == "Tráng miệng" || c.CategoryName == "Đồ uống")
                .ToList(), "CategoryId", "CategoryName");

            return View(product);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null)
            {
                TempData["error"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            TempData["success"] = "Product deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _db.Products
                .Include(p => p.ProductCategoryMappings)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                TempData["error"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction(nameof(Index));
            }

            // Lấy CategoryId đang gán cho sản phẩm (nếu có)
            var selectedCategoryId = product.ProductCategoryMappings.FirstOrDefault()?.CategoryId;

            // Lấy danh sách 3 danh mục chỉ định
            var categories = await _db.ProductCategories
                .Where(c => c.CategoryName == "Món mặn" || c.CategoryName == "Tráng miệng" || c.CategoryName == "Đồ uống")
                .ToListAsync();

            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", selectedCategoryId);
            ViewBag.SelectedCategoryId = selectedCategoryId;

            return View(product);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile? file, int CategoryId)
        {
            if (ModelState.IsValid)
            {
                if (ContainsSpecialChars(product.Name))
                {
                    ModelState.AddModelError("Name", "Tên sản phẩm không được chứa ký tự đặc biệt.");
                    return View(product);
                }

                if (product.Calo < 0 || product.Calo >= 1000)
                {
                    ModelState.AddModelError("Calories", "Lượng calo phải từ 0 đến dưới 1000.");
                    return View(product);
                }

                var existingProduct = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == product.ProductId);
                if (existingProduct == null)
                {
                    TempData["error"] = "Không tìm thấy sản phẩm.";
                    return RedirectToAction(nameof(Index));
                }

                product.Price = existingProduct.Price;
                product.IsActive = existingProduct.IsActive;

                if (file != null)
                {
                    if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                    {
                        string oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingProduct.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, "images", "products");

                    if (!Directory.Exists(productPath))
                    {
                        Directory.CreateDirectory(productPath);
                    }

                    using var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create);
                    await file.CopyToAsync(fileStream);

                    product.ImageUrl = "/images/products/" + fileName;
                }
                else
                {
                    product.ImageUrl = existingProduct.ImageUrl;
                }

                _db.Products.Update(product);
                await _db.SaveChangesAsync();

                // ✅ Cập nhật danh mục sản phẩm
                var oldMapping = await _db.ProductCategoryMappings
                    .FirstOrDefaultAsync(m => m.ProductId == product.ProductId);
                if (oldMapping != null)
                {
                    _db.ProductCategoryMappings.Remove(oldMapping);
                }

                _db.ProductCategoryMappings.Add(new ProductCategoryMapping
                {
                    ProductId = product.ProductId,
                    CategoryId = CategoryId
                });
                await _db.SaveChangesAsync();

                TempData["success"] = "Cập nhật sản phẩm thành công.";
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

    }
}

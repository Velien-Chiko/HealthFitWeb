using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HealthFit.Models;
using System.Threading.Tasks;
using HealthFit.DataAccess.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.IO;
using IOF = System.IO;
using System.Linq;
using HealthFit.Models.Constants;

namespace HealthFit.Areas.Seller.Controllers
{
    /// <summary>
    /// Controller Seller quản lý toàn bộ chức năng dành cho người bán hàng (Seller)
    /// Bao gồm: Quản lý sản phẩm (thêm, sửa, xóa), quản lý đơn hàng (xem, xác nhận, giao hàng), quản lý đánh giá sản phẩm, cập nhật thông tin cá nhân, dashboard tổng quan hoạt động bán hàng.
    /// </summary>
    [Area("Seller")] // Đánh dấu controller thuộc area Seller
    [Authorize(Roles = "Seller")] // Chỉ user có role Seller mới truy cập được
    public class SellerController : Controller
    {
        // Quản lý xác thực người dùng (UserManager) và truy cập database (HealthyShopContext)
        private readonly UserManager<User> _userManager; // Quản lý user
        private readonly HealthyShopContext _db; // DbContext

        /// <summary>
        /// Constructor nhận vào UserManager và DbContext để thao tác dữ liệu và xác thực
        /// </summary>
        public SellerController(UserManager<User> userManager, HealthyShopContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        // =============================
        // DASHBOARD: Thống kê tổng quan cho Seller
        // =============================
        /// <summary>
        /// Hiển thị dashboard tổng quan cho người bán
        /// - Tính tổng doanh thu, tổng số sản phẩm, tổng số đơn hàng
        /// - Trả về view Dashboard.cshtml
        /// </summary>
        public IActionResult Dashboard()
        {
            var totalRevenue = _db.Orders
                .Where(o => o.Status != "Đã hủy" && o.Status != "Pending")
                .Sum(o => o.TotalAmount);
            ViewBag.TotalRevenue = totalRevenue;

            ViewBag.TotalProducts = _db.Products.Count();
            ViewBag.TotalOrders = _db.Orders.Count();

            return View("~/Areas/Seller/Views/Seller/Dashboard.cshtml");
        }

        // =============================
        // QUẢN LÝ THÔNG TIN CÁ NHÂN (PROFILE)
        // =============================
        /// <summary>
        /// Hiển thị form chỉnh sửa thông tin cá nhân của người bán
        /// - Lấy user hiện tại từ UserManager
        /// - Trả về view EditProfile.cshtml
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User); // Lấy user hiện tại
            if (user == null) return NotFound();
            return View("~/Areas/Seller/Views/Seller/EditProfile.cshtml", user);
        }

        /// <summary>
        /// Xử lý cập nhật thông tin cá nhân và đổi mật khẩu
        /// - Cập nhật các trường thông tin cá nhân
        /// - Nếu có nhập mật khẩu mới thì đổi mật khẩu
        /// - Trả về view EditProfile.cshtml với thông báo thành công/lỗi
        /// </summary>
        /// <param name="model">Thông tin user cần cập nhật</param>
        /// <param name="newPassword">Mật khẩu mới (nếu có)</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(User model, string? newPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Validate họ tên chỉ chứa chữ cái, khoảng trắng, dấu gạch ngang hoặc nháy đơn
            if (string.IsNullOrWhiteSpace(model.FullName) || !System.Text.RegularExpressions.Regex.IsMatch(model.FullName, @"^[\p{L} \-']+$"))
            {
                ViewBag.Error = "Họ và tên chỉ được chứa chữ cái, khoảng trắng, dấu gạch ngang hoặc dấu nháy đơn.";
                return View("~/Areas/Seller/Views/Seller/EditProfile.cshtml", model);
            }
            // Validate email hợp lệ
            if (string.IsNullOrWhiteSpace(model.Email) || !System.Text.RegularExpressions.Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ViewBag.Error = "Email không hợp lệ.";
                return View("~/Areas/Seller/Views/Seller/EditProfile.cshtml", model);
            }
            // Validate số điện thoại Việt Nam chuẩn
            if (!string.IsNullOrEmpty(model.PhoneNumber) && !System.Text.RegularExpressions.Regex.IsMatch(model.PhoneNumber, @"^(0[3|5|7|8|9])[0-9]{8}$"))
            {
                ViewBag.Error = "Số điện thoại phải là số di động Việt Nam hợp lệ (bắt đầu bằng 03, 05, 07, 08, 09 và đủ 10 số).";
                return View("~/Areas/Seller/Views/Seller/EditProfile.cshtml", model);
            }
            // Validate độ dài các trường địa chỉ, thành phố, quốc gia
            if (!string.IsNullOrEmpty(model.Address) && model.Address.Length > 200)
            {
                ViewBag.Error = "Địa chỉ không được vượt quá 200 ký tự.";
                return View("~/Areas/Seller/Views/Seller/EditProfile.cshtml", model);
            }
            if (!string.IsNullOrEmpty(model.City) && model.City.Length > 100)
            {
                ViewBag.Error = "Thành phố không được vượt quá 100 ký tự.";
                return View("~/Areas/Seller/Views/Seller/EditProfile.cshtml", model);
            }
            if (!string.IsNullOrEmpty(model.Country) && model.Country.Length > 100)
            {
                ViewBag.Error = "Quốc gia không được vượt quá 100 ký tự.";
                return View("~/Areas/Seller/Views/Seller/EditProfile.cshtml", model);
            }
            // Validate mật khẩu mới nếu có nhập
            if (!string.IsNullOrEmpty(newPassword))
            {
                if (newPassword.Length < 6 || !System.Text.RegularExpressions.Regex.IsMatch(newPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$"))
                {
                    ViewBag.Error = "Mật khẩu phải có ít nhất 6 ký tự, gồm chữ hoa, chữ thường và số.";
                    return View("~/Areas/Seller/Views/Seller/EditProfile.cshtml", user);
                }
            }

            // Cập nhật các trường thông tin cá nhân
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.City = model.City;
            user.District = model.District;
            user.Country = model.Country;
            user.Gender = model.Gender;

            // Đổi mật khẩu nếu có nhập mật khẩu mới
            if (!string.IsNullOrEmpty(newPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
                if (!passwordResult.Succeeded)
                {
                    ViewBag.Error = string.Join("; ", passwordResult.Errors.Select(e => e.Description));
                    return View("~/Areas/Seller/Views/Seller/EditProfile.cshtml", user);
                }
            }

            // Lưu thay đổi thông tin user
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                ViewBag.Success = "Cập nhật thành công!";
            }
            else
            {
                ViewBag.Error = "Có lỗi xảy ra khi cập nhật!";
            }
            return View("~/Areas/Seller/Views/Seller/EditProfile.cshtml", user);
        }

        // =============================
        // QUẢN LÝ SẢN PHẨM (Xem, tìm kiếm, lọc, sửa, tạo, xóa)
        // =============================
        /// <summary>
        /// Hiển thị danh sách sản phẩm của người bán (tìm kiếm, lọc, phân trang)
        /// - Hỗ trợ tìm kiếm theo tên, lọc trạng thái, sắp xếp theo giá/số lượng
        /// - Phân trang kết quả
        /// - Trả về view ProductList.cshtml
        /// </summary>
        public async Task<IActionResult> ProductList(string? search, string? filter, string? sort, int? pg)
        {
            // Lấy danh sách sản phẩm theo điều kiện tìm kiếm, lọc, sắp xếp
            var productsQuery = _db.Products.AsQueryable(); // Bắt đầu với toàn bộ sản phẩm dưới dạng truy vấn IQueryable

            // Nếu có từ khóa tìm kiếm, lọc theo tên sản phẩm chứa từ khóa (không phân biệt hoa thường)
            if (!string.IsNullOrEmpty(search))
            {
                productsQuery = productsQuery.Where(p => p.Name.ToLower().Contains(search.ToLower()));
            }

            // Nếu có filter trạng thái (Pending/Approved/Rejected), lọc theo trạng thái sản phẩm
            if (!string.IsNullOrEmpty(filter))
            {
                productsQuery = productsQuery.Where(p => p.IsActive == filter);
            }

            // Sắp xếp theo giá hoặc số lượng tùy theo lựa chọn sort
            if (sort == "price_asc")
            {
                productsQuery = productsQuery.OrderBy(p => p.Price); // Giá tăng dần
            }
            else if (sort == "price_desc")
            {
                productsQuery = productsQuery.OrderByDescending(p => p.Price); // Giá giảm dần
            }
            else if (sort == "quantity_asc")
            {
                productsQuery = productsQuery.OrderBy(p => p.Quantity); // Số lượng tăng dần
            }
            else if (sort == "quantity_desc")
            {
                productsQuery = productsQuery.OrderByDescending(p => p.Quantity); // Số lượng giảm dần
            }

            // Thực thi truy vấn và lấy toàn bộ kết quả sau khi đã tìm kiếm, lọc, sắp xếp
            var allProducts = await productsQuery.ToListAsync();

            // Phân trang kết quả
            const int pageSize = 6; // Số sản phẩm mỗi trang
            int pageNumber = pg ?? 1; // Trang hiện tại, mặc định là 1 nếu không truyền vào
            if (pageNumber < 1) pageNumber = 1;
            int recsCount = allProducts.Count; // Tổng số sản phẩm sau khi lọc
            var pager = new HealthFit.Models.Models.Pager(recsCount, pageNumber, pageSize); // Tạo đối tượng phân trang
            int recsSkip = (pageNumber - 1) * pageSize; // Số sản phẩm cần bỏ qua
            var pageProducts = allProducts.Skip(recsSkip).Take(pageSize).ToList(); // Lấy danh sách sản phẩm cho trang hiện tại

            // Truyền thông tin phân trang và filter/search/sort xuống view để giữ trạng thái giao diện
            ViewBag.Pager = pager;
            ViewBag.Search = search;
            ViewBag.Filter = filter;
            ViewBag.Sort = sort;

            // Trả về view danh sách sản phẩm với dữ liệu đã phân trang
            return View("~/Areas/Seller/Views/Seller/ProductList.cshtml", pageProducts);
        }

        /// <summary>
        /// Hiển thị form chỉnh sửa sản phẩm
        /// - Lấy sản phẩm theo id
        /// - Trả về view EditProduct.cshtml
        /// </summary>
        /// <param name="id">ID của sản phẩm cần chỉnh sửa</param>
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _db.Products.FindAsync(id); // Lấy sản phẩm theo id
            if (product == null)
            {
                return NotFound();
            }
            // Lấy danh sách category
            ViewBag.Categories = _db.ProductCategories.ToList();
            // Lấy categoryId hiện tại của sản phẩm (nếu có)
            var currentCategoryId = _db.ProductCategoryMappings
                .Where(m => m.ProductId == id)
                .Select(m => m.CategoryId)
                .FirstOrDefault();
            ViewBag.CurrentCategoryId = currentCategoryId;
            return View("~/Areas/Seller/Views/Seller/EditProduct.cshtml", product);
        }

        /// <summary>
        /// Xử lý cập nhật thông tin sản phẩm
        /// - Chỉ cho phép sửa giá/số lượng khi trạng thái Approved
        /// - Xử lý upload/xóa ảnh sản phẩm
        /// - Lưu thay đổi vào database
        /// </summary>
        /// <param name="model">Thông tin sản phẩm cần cập nhật</param>
        /// <param name="imageFile">File ảnh mới (nếu có)</param>
        /// <param name="removeImage">Có xóa ảnh hiện tại không</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(Product model, int CategoryId, IFormFile? imageFile, bool removeImage = false)
        {
            if (!ModelState.IsValid)
            {
                // Truyền lại danh sách category nếu có lỗi
                ViewBag.Categories = _db.ProductCategories.ToList();
                ViewBag.CurrentCategoryId = CategoryId;
                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var product = await _db.Products.FindAsync(model.ProductId);
            if (product == null)
            {
                return NotFound();
            }

            // Chỉ cho phép sửa sản phẩm đã duyệt
            if (product.IsActive != ProductStatusConstants.Approved)
            {
                TempData["Error"] = "Chỉ có thể chỉnh sửa sản phẩm đã được duyệt!";
                return RedirectToAction(nameof(ProductList));
            }

            // Validate tên sản phẩm (chỉ cho phép là chữ cái, chữ số và khoảng trắng, không ký tự đặc biệt)
            if (string.IsNullOrWhiteSpace(model.Name) || model.Name.Length > 100 ||
                !System.Text.RegularExpressions.Regex.IsMatch(model.Name, @"^[\p{L}0-9 ]+$"))
            {
                ModelState.AddModelError("Name", "Tên sản phẩm chỉ được phép là chữ cái, chữ số và tối đa 100 ký tự, không chứa ký tự đặc biệt.");
                ViewBag.Categories = _db.ProductCategories.ToList();
                ViewBag.CurrentCategoryId = CategoryId;
                // Giữ trạng thái duyệt nếu sản phẩm gốc là Approved
                if (product.IsActive == ProductStatusConstants.Approved)
                {
                    model.IsActive = ProductStatusConstants.Approved;
                }
                return View(model);
            }
            // Validate mô tả
            if (!string.IsNullOrEmpty(model.Description) && model.Description.Length > 1000)
            {
                ModelState.AddModelError("Description", "Mô tả sản phẩm tối đa 1000 ký tự.");
                ViewBag.Categories = _db.ProductCategories.ToList();
                ViewBag.CurrentCategoryId = CategoryId;
                if (product.IsActive == ProductStatusConstants.Approved)
                {
                    model.IsActive = ProductStatusConstants.Approved;
                }
                return View(model);
            }
            // Validate giá
            if (model.Price <= 0)
            {
                ModelState.AddModelError("Price", "Giá sản phẩm phải lớn hơn 0.");
                ViewBag.Categories = _db.ProductCategories.ToList();
                ViewBag.CurrentCategoryId = CategoryId;
                if (product.IsActive == ProductStatusConstants.Approved)
                {
                    model.IsActive = ProductStatusConstants.Approved;
                }
                return View(model);
            }
            // Validate số lượng
            if (model.Quantity < 0)
            {
                ModelState.AddModelError("Quantity", "Số lượng sản phẩm không được âm.");
                ViewBag.Categories = _db.ProductCategories.ToList();
                ViewBag.CurrentCategoryId = CategoryId;
                if (product.IsActive == ProductStatusConstants.Approved)
                {
                    model.IsActive = ProductStatusConstants.Approved;
                }
                return View(model);
            }

            // Xử lý xóa ảnh nếu người dùng chọn xóa
            if (removeImage)
            {
                if (!string.IsNullOrEmpty(product.ImageUrl) && product.ImageUrl.StartsWith("/images/products/"))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
                    if (IOF.File.Exists(oldImagePath))
                    {
                        IOF.File.Delete(oldImagePath);
                    }
                }
                product.ImageUrl = null; // Đảm bảo xóa ảnh trong DB
                model.ImageUrl = null;   // Đảm bảo không hiển thị lại khi trả về view
            }
            // Xử lý upload file ảnh mới nếu có
            else if (imageFile != null && imageFile.Length > 0)
            {
                // Kiểm tra kích thước file (tối đa 5MB)
                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "File ảnh quá lớn. Kích thước tối đa là 5MB.");
                    return View(model);
                }

                // Kiểm tra loại file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("", "Chỉ chấp nhận file ảnh: JPG, PNG, GIF.");
                    return View(model);
                }

                try
                {
                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(product.ImageUrl) && product.ImageUrl.StartsWith("/images/products/"))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
                        if (IOF.File.Exists(oldImagePath))
                        {
                            IOF.File.Delete(oldImagePath);
                        }
                    }

                    // Tạo tên file unique
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                    
                    // Tạo thư mục nếu chưa tồn tại
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    var filePath = Path.Combine(uploadPath, fileName);
                    
                    // Lưu file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Cập nhật ImageUrl
                    model.ImageUrl = $"/images/products/{fileName}";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi upload file: {ex.Message}");
                    return View(model);
                }
            }

            // Cập nhật thông tin sản phẩm
            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.Quantity = model.Quantity;
            product.ImageUrl = model.ImageUrl;
            // Luôn giữ trạng thái đã duyệt
            product.IsActive = ProductStatusConstants.Approved;
            // Không cập nhật lại mapping danh mục khi chỉnh sửa sản phẩm (vì đã khóa ở view)
            // Nếu cần cập nhật mapping, phải xóa mapping cũ và tạo mapping mới (chỉ nên làm khi cho phép đổi danh mục)
            // var mapping = _db.ProductCategoryMappings.FirstOrDefault(m => m.ProductId == product.ProductId);
            // if (mapping != null)
            // {
            //     mapping.CategoryId = CategoryId;
            // }
            // else
            // {
            //     _db.ProductCategoryMappings.Add(new HealthFit.Models.Models.ProductCategoryMapping
            //     {
            //         ProductId = product.ProductId,
            //         CategoryId = CategoryId
            //     });
            // }
            await _db.SaveChangesAsync();
            TempData["Success"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction(nameof(ProductList));
        }

        /// <summary>
        /// Hiển thị form tạo sản phẩm mới
        /// - Trả về view CreateProduct.cshtml
        ///// </summary>
        //[HttpGet]
        //public IActionResult CreateProduct()
        //{
        //    return View();
        //}

        /// <summary>
        /// Xử lý tạo sản phẩm mới
        /// - Xử lý upload ảnh nếu có
        /// - Lưu sản phẩm mới vào database
        /// </summary>
        /// <param name="model">Thông tin sản phẩm</param>
        /// <param name="imageFile">File ảnh sản phẩm</param>
        /// <param name="imageUrl">URL ảnh (nếu không upload file)</param>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> CreateProduct(Product model, IFormFile? imageFile, string? imageUrl)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    // Xử lý upload file ảnh nếu có
        //    if (imageFile != null && imageFile.Length > 0)
        //    {
        //        // Kiểm tra kích thước file (tối đa 5MB)
        //        if (imageFile.Length > 5 * 1024 * 1024)
        //        {
        //            ModelState.AddModelError("", "File ảnh quá lớn. Kích thước tối đa là 5MB.");
        //            return View(model);
        //        }

        //        // Kiểm tra loại file
        //        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        //        var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        //        if (!allowedExtensions.Contains(fileExtension))
        //        {
        //            ModelState.AddModelError("", "Chỉ chấp nhận file ảnh: JPG, PNG, GIF.");
        //            return View(model);
        //        }

        //        try
        //        {
        //            // Tạo tên file unique
        //            var fileName = $"{Guid.NewGuid()}{fileExtension}";
        //            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                    
        //            // Tạo thư mục nếu chưa tồn tại
        //            if (!Directory.Exists(uploadPath))
        //            {
        //                Directory.CreateDirectory(uploadPath);
        //            }

        //            var filePath = Path.Combine(uploadPath, fileName);
                    
        //            // Lưu file
        //            using (var stream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await imageFile.CopyToAsync(stream);
        //            }

        //            // Cập nhật ImageUrl
        //            model.ImageUrl = $"/images/products/{fileName}";
        //        }
        //        catch (Exception ex)
        //        {
        //            ModelState.AddModelError("", $"Lỗi khi upload file: {ex.Message}");
        //            return View(model);
        //        }
        //    }
        //    // Nếu không có file upload nhưng có URL ảnh
        //    else if (!string.IsNullOrEmpty(imageUrl))
        //    {
        //        model.ImageUrl = imageUrl;
        //    }

        //    // Lấy thông tin seller hiện tại
        //    var currentUser = await _userManager.GetUserAsync(User);
        //    model.CreatedBy = currentUser.Id;

        //    // Thêm sản phẩm mới vào database
        //    _db.Products.Add(model);
        //    await _db.SaveChangesAsync();

        //    TempData["Success"] = "Thêm sản phẩm thành công!";
        //    return RedirectToAction(nameof(ProductList));
        //}

        /// <summary>
        /// Xử lý xóa sản phẩm
        /// - Xóa ảnh vật lý nếu có
        /// - Xóa sản phẩm khỏi database
        /// </summary>
        ///// <param name="id">ID của sản phẩm cần xóa</param>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteProduct(int id)
        //{
        //    var currentUser = await _userManager.GetUserAsync(User);
        //    var product = await _db.Products.FindAsync(id);
        //    if (product == null || product.CreatedBy != currentUser.Id)
        //    {
        //        return NotFound();
        //    }

        //    // Xóa ảnh vật lý nếu có
        //    if (!string.IsNullOrEmpty(product.ImageUrl) && product.ImageUrl.StartsWith("/images/products/"))
        //    {
        //        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
        //        if (IOF.File.Exists(imagePath))
        //        {
        //            IOF.File.Delete(imagePath);
        //        }
        //    }

        //    // Xóa sản phẩm khỏi database
        //    _db.Products.Remove(product);
        //    await _db.SaveChangesAsync();

        //    TempData["Success"] = "Xóa sản phẩm thành công!";
        //    return RedirectToAction(nameof(ProductList));
        //}

        //// =============================
        //// QUẢN LÝ ĐƠN HÀNG
        //// =============================
        ///// <summary>
        ///// Hiển thị danh sách đơn hàng (OrderList) cho Seller (tìm kiếm, lọc, phân trang)
        ///// - Cho phép tìm kiếm, lọc, sắp xếp, phân trang đơn hàng
        ///// - KHÔNG lọc theo seller, hiển thị toàn bộ đơn hàng hệ thống
        ///// - Sửa lỗi ép kiểu CS0266 bằng cách tách truy vấn filter/sort sang biến IQueryable<Order>
        ///// </summary>
        [HttpGet]
        public async Task<IActionResult> OrderList(string? search, string? filter, string? sort, int? pg, string? fromDate, string? toDate)
        {
            // 1. Lấy toàn bộ đơn hàng, bao gồm chi tiết sản phẩm, combo và thông tin user
            var ordersQuery = _db.Orders
                .Include(o => o.OrderProductDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.OrderMealPlanDetails)
                    .ThenInclude(omp => omp.MealPlanDetail)
                .Include(o => o.User);

            // 2. Để tránh lỗi ép kiểu CS0266 khi dùng Where/OrderBy, tách sang biến IQueryable<Order>
            IQueryable<Order> filteredOrdersQuery = ordersQuery;  //Để dễ dàng thêm các điều kiện lọc, sắp xếp

            // 3. Tìm kiếm theo mã đơn hoặc tên khách hàng (nếu có nhập search)
            if (!string.IsNullOrEmpty(search))
            {
                // Kiểm tra null tránh lỗi runtime
                filteredOrdersQuery = filteredOrdersQuery.Where(o =>
                    o.OrderId.ToString().Contains(search) ||
                    (!string.IsNullOrEmpty(o.FullName) && o.FullName.ToLower().Contains(search.ToLower())) ||
                    (!string.IsNullOrEmpty(o.PhoneNumber) && o.PhoneNumber.ToLower().Contains(search.ToLower()))
                );
            }

            // 4. Lọc theo trạng thái đơn hàng (nếu có chọn filter)
            if (!string.IsNullOrEmpty(filter))
            {
                // Hỗ trợ cả giá trị tiếng Anh và tiếng Việt
                filteredOrdersQuery = filteredOrdersQuery.Where(o => 
                    o.Status == filter || 
                    (filter == "Đã Thanh Toán" && o.Status == "Approved") ||
                    (filter == "Đang giao" && o.Status == "Shipped") ||
                    (filter == "Hoàn thành" && o.Status == "Completed") ||
                    (filter == "Đã hủy" && o.Status == "Cancelled")
                );
            }

            // 5. Sắp xếp theo ngày hoặc tổng tiền (nếu có chọn sort)
            if (sort == "date_desc")
            {
                filteredOrdersQuery = filteredOrdersQuery.OrderByDescending(o => o.OrderDate);
            }
            else if (sort == "date_asc")
            {
                filteredOrdersQuery = filteredOrdersQuery.OrderBy(o => o.OrderDate);
            }
            else if (sort == "total_asc")
            {
                filteredOrdersQuery = filteredOrdersQuery.OrderBy(o => o.TotalAmount);
            }
            else if (sort == "total_desc")
            {
                filteredOrdersQuery = filteredOrdersQuery.OrderByDescending(o => o.TotalAmount);
            }

            // Lọc theo ngày nếu có nhập fromDate hoặc toDate
            DateTime from, to;
            if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out from))
            {
                filteredOrdersQuery = filteredOrdersQuery.Where(o => o.OrderDate >= from);
                ViewBag.FromDate = fromDate;
            }
            if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out to))
            {
                // Để bao gồm cả ngày toDate, cộng thêm 1 ngày và so sánh nhỏ hơn
                filteredOrdersQuery = filteredOrdersQuery.Where(o => o.OrderDate < to.AddDays(1));
                ViewBag.ToDate = toDate;
            }

            // 6. Lấy toàn bộ kết quả sau khi filter/sort
            var allOrders = await filteredOrdersQuery.ToListAsync();

            // 7. Phân trang: mỗi trang 10 đơn hàng
            const int pageSize = 10;
            int pageNumber = pg ?? 1;
            int recsCount = allOrders.Count;
            var pager = new HealthFit.Models.Models.Pager(recsCount, pageNumber, pageSize);
            int recsSkip = (pageNumber - 1) * pageSize;
            var pageOrders = allOrders.Skip(recsSkip).Take(pageSize).ToList();

            // 8. Truyền thông tin filter/search/sort/pager xuống view để giữ trạng thái giao diện
            ViewBag.Pager = pager;
            ViewBag.Search = search;
            ViewBag.Filter = filter;
            ViewBag.Sort = sort;

            // 9. Trả về view danh sách đơn hàng
            return View("~/Areas/Seller/Views/Seller/OrderList.cshtml", pageOrders);
        }

        /// <summary>
        /// Cập nhật trạng thái đơn hàng (xác nhận, giao hàng, hủy, ...)
        /// - Cập nhật trạng thái đơn hàng (xác nhận, giao hàng, hủy, ...)
        /// - Trả về view chi tiết đơn hàng với thông báo
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            var order = await _db.Orders.FindAsync(orderId); // Lấy đơn hàng theo id
            if (order == null)
            {
                return NotFound();
            }

            // Nếu đã hoàn thành hoặc đã hủy thì không cho phép cập nhật nữa
            if (order.Status == "Completed" || order.Status == "Cancelled")
            {
                TempData["Error"] = "Không thể cập nhật trạng thái từ trạng thái hiện tại.";
                return RedirectToAction(nameof(OrderDetails), new { id = orderId });
            }

            // Chỉ cho phép cập nhật trạng thái theo luồng hợp lệ (dùng giá trị EN)
            if (order.Status == "Approved")
            {
                if (status != "Shipped" && status != "Completed" && status != "Cancelled")
                {
                    TempData["Error"] = "Chỉ được chuyển sang Đang giao, Hoàn thành hoặc Đã hủy từ Đã thanh toán.";
                    return RedirectToAction(nameof(OrderDetails), new { id = orderId });
                }
            }
            else if (order.Status == "Shipped")
            {
                if (status != "Completed" && status != "Cancelled")
                {
                    TempData["Error"] = "Chỉ được chuyển sang Hoàn thành hoặc Đã hủy từ Đang giao.";
                    return RedirectToAction(nameof(OrderDetails), new { id = orderId });
                }
            }
            else
            {
                TempData["Error"] = "Không thể cập nhật trạng thái từ trạng thái hiện tại.";
                return RedirectToAction(nameof(OrderDetails), new { id = orderId });
            }

            // Nếu chuyển sang Completed từ Shipped hoặc Approved thì hiểu là hoàn thành
            if ((order.Status == "Shipped" || order.Status == "Approved") && status == "Completed")
            {
                order.Status = "Completed";
                await _db.SaveChangesAsync();
                TempData["Success"] = "Đã xác nhận hoàn thành đơn hàng!";
                return RedirectToAction(nameof(OrderDetails), new { id = orderId });
            }

            // Cập nhật trạng thái đơn hàng
            order.Status = status;
            await _db.SaveChangesAsync();

            string statusMessage = status switch
            {
                "Shipped" => "Đã cập nhật trạng thái giao hàng!",
                "Completed" => "Đã xác nhận hoàn thành đơn hàng!",
                "Cancelled" => "Đã hủy đơn hàng!",
                _ => "Đã cập nhật trạng thái đơn hàng!"
            };

            TempData["Success"] = statusMessage;
            return RedirectToAction(nameof(OrderDetails), new { id = orderId });
        }

        /// <summary>
        /// Xác nhận đơn hàng (chuyển trạng thái thành "Đã xác nhận")
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmOrder(int orderId)
        {
            return await UpdateOrderStatus(orderId, OrderStatusConstants.Confirmed);
        }

        /// <summary>
        /// Đánh dấu đơn hàng là đang giao (chuyển trạng thái thành "Đang giao")
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShipOrder(int orderId)
        {
            return await UpdateOrderStatus(orderId, OrderStatusConstants.Shipped);
        }

        /// <summary>
        /// Đánh dấu đơn hàng là đã giao (chuyển trạng thái thành "Đã giao")
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeliverOrder(int orderId)
        {
            return await UpdateOrderStatus(orderId, OrderStatusConstants.Delivered);
        }

        /// <summary>
        /// Huỷ đơn hàng (chuyển trạng thái thành "Đã hủy")
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            return await UpdateOrderStatus(orderId, OrderStatusConstants.Cancelled);
        }

        /// <summary>
        /// Xem chi tiết đơn hàng
        /// - Lấy thông tin đơn hàng, sản phẩm, user
        /// - Trả về view OrderDetails.cshtml
        /// </summary>
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _db.Orders
                .Include(o => o.OrderProductDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.User)
                .Include(o => o.OrderMealPlanDetails)
                    .ThenInclude(omp => omp.MealPlanDetail)
                        .ThenInclude(mp => mp.MealPlanProductDetails)
                            .ThenInclude(mpp => mpp.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id); // Lấy đơn hàng theo id, bao gồm chi tiết sản phẩm, combo, sản phẩm con và user

            if (order == null)
            {
                return NotFound();
            }

            return View("~/Areas/Seller/Views/OrderDetails/OrderDetails.cshtml", order);
        }
    }
} 
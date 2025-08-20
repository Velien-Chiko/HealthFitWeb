using HealthFit.DataAccess.Data;
using HealthFit.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using HealthFit.Models.Models;
using HealthFit.Services;
using Microsoft.Extensions.Configuration;

namespace HealthFit.Areas.SystemAdmin.Controllers
{
    /// <summary>
    /// Controller quản lý vai trò và tài khoản quản trị hệ thống (SystemAdmin)
    /// Bao gồm: Quản lý user, phân quyền, tạo/xóa/cập nhật tài khoản, tìm kiếm, lọc, phân trang, thống kê user theo vai trò
    /// </summary>
    [Area("SystemAdmin")]
    [Authorize(Roles = "SystemAdmin")]
    public class ManagerRoleController : Controller
    {
        // DbContext để truy cập dữ liệu
        private readonly HealthyShopContext _db;
        // Quản lý tài khoản, vai trò, mật khẩu
        private readonly UserManager<User> _userManager;
        // Configuration để gửi email
        private readonly IConfiguration _configuration;
        // Định nghĩa thứ tự ưu tiên các vai trò (dùng cho sắp xếp)
        private readonly Dictionary<string, int> _rolePriority = new()
        {
            
            { "Manager", 1 },
            { "Nutri", 2 },
            { "Seller", 3 },
            { "Customer", 4 },
            { "Admin", 5}
        };

        
        public ManagerRoleController(HealthyShopContext db, UserManager<User> userManager, IConfiguration configuration)
        {
            _db = db;
            _userManager = userManager;
            _configuration = configuration;
        }

        /// <summary>
        /// Hiển thị trang quản lý hệ thống chính cho SystemAdmin
        /// - Xem danh sách user
        /// - Tìm kiếm theo email hoặc tên
        /// - Lọc theo vai trò
        /// - Sắp xếp theo vai trò
        /// - Phân trang danh sách user
        /// - Thống kê số lượng user theo vai trò
        /// </summary>
        /// <param name="pg">Số trang hiện tại</param>
        /// <param name="searchString">Từ khóa tìm kiếm (email hoặc tên)</param>
        /// <param name="roleFilter">Lọc theo vai trò</param>
        /// <param name="sortOrder">Thứ tự sắp xếp</param>
        [HttpGet]
        public async Task<IActionResult> SystemAdmin(int pg = 1, string searchString = null, string roleFilter = null, string sortOrder = null)
        { 
            try 
            {
                // Lấy danh sách user từ database, không tracking để tối ưu hiệu năng 
                IQueryable<User> query = _db.Users.AsNoTracking();
                ViewData["SearchString"] = searchString;
                ViewData["RoleFilter"] = roleFilter;
                ViewData["CurrentSort"] = sortOrder;
                // Nếu có từ khóa tìm kiếm, lọc theo email hoặc tên
                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    searchString = searchString.Trim().ToLower();
                    query = query.Where(u => u.Email.ToLower().Contains(searchString) || 
                                          (u.FullName != null && u.FullName.ToLower().Contains(searchString)));
                }
                // Lấy toàn bộ user sau khi lọc tìm kiếm(nếu có)
                List<User> allUsers = await query.ToListAsync();
                var filteredUsers = new List<User>();
                // Nếu có lọc theo vai trò, chỉ lấy user có vai trò đó
                if (!string.IsNullOrWhiteSpace(roleFilter))
                {
                    foreach (var user in allUsers)
                    {   // lấy ra danh sách role của user
                        var userRoleList = await _userManager.GetRolesAsync(user);
                        // Kiểm tra xem có ít nhất 1 phần tử thỏa mãn điều kiện không
                        if (userRoleList.Any(r => r.Equals(roleFilter, StringComparison.OrdinalIgnoreCase)))
                        {

                            // Nếu user có role "Admin" và roleFilter = "Admin" thì thêm vào danh sách 
                            filteredUsers.Add(user);
                        }
                    }
                    allUsers = filteredUsers;  // thay thế danh sách user ban đầu bằng danh sách user đã lọc theo vai trò và hiênr thị 
                }
                // Nếu có sắp xếp theo vai trò
                if (!string.IsNullOrWhiteSpace(sortOrder))
                {
                    var usersWithRoles = new List<(User User, string Role)>();
                    foreach (var user in allUsers)
                    {
                        var roles = await _userManager.GetRolesAsync(user);  // lấy ra danh sách role của user
                        
                        var role = roles.FirstOrDefault() ?? string.Empty;  // lấy ra role đầu tiên của user nếu user đấy có nhiều role 
                        // nếu roles.FirstOrDefault() là null thì trả về chuỗi rỗng, đảm bảo role luôn là string không bị null
                        
                        
                        
                        // sau vòng lặp usersWithRoles sẽ chứa user và vai trò chính của role đấy 
                        usersWithRoles.Add((user, role));  
                    }
                    switch (sortOrder.ToLower())
                    {
                        case "role_asc":
                            allUsers = usersWithRoles.OrderBy(x => x.Role).Select(x => x.User).ToList();
                            break;
                        case "role_desc":
                            allUsers = usersWithRoles.OrderByDescending(x => x.Role).Select(x => x.User).ToList();
                            break;
                    }
                }
                // Phân trang danh sách user
                const int pageSize = 6;  // có 6 user trên 1 trang
                if (pg < 1) pg = 1;  // nếu sô trang hiện tại < 1 (-1 ; 0)  thì chuyển về 1 để tránh lỗi 
                int recsCount = allUsers.Count;  
                var pager = new Pager(recsCount, pg, pageSize);  
                int recsSkip = (pg - 1) * pageSize;  // tính số lượng user bỏ qua để lấy user trên trang hiện tại
                
               //lấy ra danh sách user cho trang hiện tại  
                var pageUsers = allUsers.Skip(recsSkip).Take(pageSize).ToList(); 
                ViewBag.Pager = pager;
                // Tạo view model cho user (chỉ lấy thông tin cần thiết)
                var userViewModel = new List<object>();
                foreach (var u in pageUsers)// lặp qua các user của trang hiện tại (đã phân trang)
                {
                    var userRoles = await _userManager.GetRolesAsync(u) ?? new List<string>();
                    userViewModel.Add(new
                    {
                        u.Id,
                        u.Email,
                        FullName = u.FullName ?? "(Chưa cập nhật)",
                        Roles = userRoles.ToList()
                    });
                }
                // Thống kê số lượng user theo từng vai trò
                var roleCounts = new Dictionary<string, int>();  // lưu số lượng user theo từng vai trò
                var availableRoles = new HashSet<string>();  //lưu tất cả các vai trò hiện có(không trùng lặp).
                foreach (var u in allUsers)
                {
                    if (u != null)
                    {
                        var userRoleSet = await _userManager.GetRolesAsync(u) ?? new List<string>();
                        foreach (var role in userRoleSet)
                        {
                            if (!string.IsNullOrWhiteSpace(role))
                            {
                                var trimmedRole = role.Trim();
                                availableRoles.Add(trimmedRole);
                                if (!roleCounts.ContainsKey(trimmedRole))
                                    roleCounts[trimmedRole] = 1;   // nếu vai trò đã tồn tại trong từ điển thì sẽ là 1 
                                else
                                    roleCounts[trimmedRole]++;   // nếu vai trò đã có thì tăng dần 
                            }
                        }
                    }
                }
                ViewBag.RoleCounts = roleCounts ?? new Dictionary<string, int>();   // truyền xuống view để thống kê vai trò 
                ViewBag.AvailableRoles = availableRoles.OrderBy(r => r).ToList();  //hiển thị danh sách vai trò đã thống kêkê
                return View(userViewModel);
            }
            catch (Exception ex)
            {
                // Nếu có lỗi, trả về view rỗng và thống kê rỗng
                ViewBag.RoleCounts = new Dictionary<string, int>();
                ViewBag.AvailableRoles = new List<string>();
                return View(new List<object>());
            }
        }

        /// <summary>
        /// Hiển thị form tạo mới user
        /// - Nhập email, mật khẩu, họ tên, chọn vai trò
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            // Truyền danh sách vai trò cho dropdown
            ViewBag.Roles = new List<string> { "Manager", "Nutri", "Seller" };
            return View();
        }

        /// <summary>
        /// Xử lý tạo mới user (POST)
        /// - Kiểm tra dữ liệu đầu vào
        /// - Tạo user mới, gán role
        /// - Thông báo thành công/lỗi
        /// </summary>
        /// <param name="email">Email của user</param>
        /// <param name="password">Mật khẩu</param>
        /// <param name="fullName">Họ tên</param>
        /// <param name="gender">Giới tính</param>
        /// <param name="role">Vai trò</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string email, string password, string fullName, string gender, string role)
        {
            ViewBag.Roles = new List<string> { "Manager", "Nutri", "Seller" };
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || 
                string.IsNullOrWhiteSpace(role) || string.IsNullOrWhiteSpace(gender))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin bắt buộc (Email, Mật khẩu, Giới tính, Vai trò).");
                return View();
            }

            // Kiểm tra email đã tồn tại chưa
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                ModelState.AddModelError("email", "Email này đã được sử dụng. Vui lòng chọn email khác.");
                return View();
            }

            // Validate họ tên chỉ chứa chữ cái và khoảng trắng
            if (!string.IsNullOrWhiteSpace(fullName) && !System.Text.RegularExpressions.Regex.IsMatch(fullName, @"^[\p{L} \-']+$"))
            {
                ModelState.AddModelError("fullName", "Họ và tên chỉ được chứa chữ cái, khoảng trắng, dấu gạch ngang hoặc dấu nháy đơn.");
                return View();
            }
            // Tạo user mới
            var user = new User { 
                UserName = email, 
                Email = email, 
                EmailConfirmed = true,
                FullName = fullName?.Trim(),
                Gender = gender
            };

            //Thêm user vào hệ thống
            var result = await _userManager.CreateAsync(user, password);// tạo user mới và gán mật khẩu để lưu vào database
            if (result.Succeeded)
            {
                // Gán vai trò cho user
                await _userManager.AddToRoleAsync(user, role);

                // Gửi email thông báo tài khoản mới
                var mailService = new SendMail(_configuration);
                string subject = "Thông báo tài khoản HealthFit";
                string body = $@"
                    Xin chào {fullName},<br>
                    Tài khoản của bạn đã được tạo trên hệ thống HealthFit.<br>
                    <b>Email đăng nhập:</b> {email}<br>
                    <b>Mật khẩu:</b> {password}<br>
                    <b>Giới tính:</b> {gender}<br>
                    <b>Vai trò:</b> {role}<br>
                    Vui lòng đăng nhập và đổi mật khẩu sau khi sử dụng lần đầu.<br>
                    Trân trọng!";
                mailService.Send(email, subject, body);

                TempData["success"] = "Tạo tài khoản thành công và đã gửi email cho người dùng!";
                return RedirectToAction("SystemAdmin");
            }
            // Nếu có lỗi, hiển thị lỗi
            foreach (var error in result.Errors)
            {
                var vietnameseError = TranslateIdentityError(error.Description);
                ModelState.AddModelError("", vietnameseError);
            }
            return View();
        }

        // Phương thức dịch lỗi Identity sang tiếng Việt
        private string TranslateIdentityError(string errorDescription)
        {
            return errorDescription switch
            {
                var s when s.Contains("Email") && s.Contains("already taken") => "Email này đã được sử dụng. Vui lòng chọn email khác.",
                var s when s.Contains("Password") && s.Contains("too short") => "Mật khẩu quá ngắn. Vui lòng nhập mật khẩu dài hơn.",
                var s when s.Contains("Password") && s.Contains("requires") => "Mật khẩu không đủ mạnh. Vui lòng thêm chữ hoa, chữ thường, số và ký tự đặc biệt.",
                var s when s.Contains("UserName") && s.Contains("already taken") => "Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.",
                _ => errorDescription // Giữ nguyên nếu không có bản dịch
            };
        }

        /// <summary>
        /// Hiển thị xác nhận xóa user
        /// - Hiển thị thông tin user và form xác nhận trước khi xóa
        /// </summary>
        /// <param name="id">ID của user cần xóa</param>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("SystemAdmin"))
            {
                TempData["error"] = "Không thể xóa tài khoản SystemAdmin!";
                return RedirectToAction("SystemAdmin");
            }
            return View(new { user.Id, user.Email, user.FullName, user.PasswordHash });
        }

        // Xử lý xóa user (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteConfirmed(int id, string? confirm = null)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("SystemAdmin"))
            {
                TempData["error"] = "Không thể xóa tài khoản SystemAdmin!";
                return RedirectToAction("SystemAdmin");
            }

            try
            {
                // Xóa các CartItems liên quan trước
                var cartItems = _db.CartItems.Where(c => c.UserId == id).ToList();
                if (cartItems.Any())
                {
                    _db.CartItems.RemoveRange(cartItems);
                    await _db.SaveChangesAsync();
                }

                // Xóa các BlogComments liên quan
                var blogComments = _db.BlogComments.Where(bc => bc.UserId == id).ToList();
                if (blogComments.Any())
                {
                    _db.BlogComments.RemoveRange(blogComments);
                    await _db.SaveChangesAsync();
                }

                // Xóa các CustomerProfiles liên quan
                var customerProfiles = _db.CustomerProfiles.Where(cp => cp.UserId == id).ToList();
                if (customerProfiles.Any())
                {
                    _db.CustomerProfiles.RemoveRange(customerProfiles);
                    await _db.SaveChangesAsync();
                }

                // Xóa các ProductReviews liên quan
                var productReviews = _db.ProductReviews.Where(pr => pr.UserId == id).ToList();
                if (productReviews.Any())
                {
                    _db.ProductReviews.RemoveRange(productReviews);
                    await _db.SaveChangesAsync();
                }

                // Xóa các EmailLogs liên quan
                var emailLogs = _db.EmailLogs.Where(el => el.UserId == id).ToList();
                if (emailLogs.Any())
                {
                    _db.EmailLogs.RemoveRange(emailLogs);
                    await _db.SaveChangesAsync();
                }

                // Xóa các Aiinteractions liên quan
                var aiInteractions = _db.Aiinteractions.Where(ai => ai.UserId == id).ToList();
                if (aiInteractions.Any())
                {
                    _db.Aiinteractions.RemoveRange(aiInteractions);
                    await _db.SaveChangesAsync();
                }

                // Cuối cùng xóa user
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    TempData["success"] = "Xóa tài khoản thành công!";
                    return RedirectToAction("SystemAdmin");
                }
                TempData["error"] = string.Join("; ", result.Errors.Select(e => e.Description));
                return RedirectToAction("SystemAdmin");
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Lỗi khi xóa tài khoản: {ex.Message}";
                return RedirectToAction("SystemAdmin");
            }
        }

        /// <summary>
        /// Hiển thị form cập nhật user
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id == 0) return NotFound();// nếu id là null hoặc 0 thì trả về là notfound
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();// nếu không tìm thấy user thì trả về là notfound
            var roles = await _userManager.GetRolesAsync(user);  // lấy ra danh sách role của user này 
            ViewBag.Roles = new List<string> { "Manager", "Nutri", "Seller", "Admin" , }; // truyền xuống view để hiển thị danh sách role cho user chọn 
            return View(new {
                user.Id,
                user.Email,
                user.FullName,
                user.Address,
                user.City,
                user.Country,
                user.Gender,
                Role = roles.FirstOrDefault() ?? ""
            });
        }

        // Xử lý cập nhật user (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Update(int Id, string email, string password, string fullName, string role, string address, string city, string country, string gender)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == Id);
            if (user == null) return NotFound();
            // Validate họ tên chỉ chứa chữ cái và khoảng trắng
            if (!string.IsNullOrWhiteSpace(fullName) && !System.Text.RegularExpressions.Regex.IsMatch(fullName, @"^[\p{L} \-']+$"))
            {
                ModelState.AddModelError("fullName", "Họ và tên chỉ được chứa chữ cái, khoảng trắng, dấu gạch ngang hoặc dấu nháy đơn.");
                ViewBag.Roles = new List<string> { "Manager", "Nutri", "Seller" };
                return View(new {
                    user.Id,
                    user.Email,
                    user.FullName,
                    user.Address,
                    user.City,
                    user.Country,
                    user.Gender,
                    Role = role
                });
            }

            // Cập nhật thông tin cá nhân
            user.FullName = fullName?.Trim();
            user.Address = address;
            user.City = city;
            user.Country = country;
            user.Gender = gender;
            await _userManager.UpdateAsync(user);

            // Nếu có nhập mật khẩu mới thì cập nhật mật khẩu
            if (!string.IsNullOrEmpty(password))
            {

               // ASP.NET Identity yêu cầu một mã xác thực đặc biệt để xác thực việc đổi mật khẩu, đảm bảo an toàn.
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, password);// dùng token để đổi mk mới 
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)  // lặp để lấy và hiển thị các lỗi 
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    ViewBag.Roles = new List<string> { "Manager", "Nutri", "Seller", "Admin" };
                    return View(new {
                        user.Id,
                        user.Email,
                        user.FullName,
                        user.Address,
                        user.City,
                        user.Country,
                        user.Gender,
                        Role = role
                    });
                }
            }

            // Cập nhật vai trò
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())  // NẾU danh sách role của user có tồn tại 1 phần tử nào không 
                await _userManager.RemoveFromRolesAsync(user, currentRoles);   // xóa tất cả các role cũ của user trước khi gán role mới 
            if (!string.IsNullOrEmpty(role))  // nếu chọn được role mới thì gàn cho user luôn 
                await _userManager.AddToRoleAsync(user, role);

            TempData["success"] = "Cập nhật thành công!";
            return RedirectToAction("SystemAdmin");
        }

        /// <summary>
        /// Action test đơn giản để kiểm tra controller
        /// </summary>
        

        /// <summary>
        /// Action mặc định chuyển hướng sang SystemAdmin
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            // Chuyển hướng sang action SystemAdmin
            return RedirectToAction("SystemAdmin");
        }
    }
}

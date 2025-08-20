using HealthFit.Models;
using HealthFit.Models.ViewModels;
using HealthFit.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthFit.Areas.Store.Controllers
{
    [Area("Store")]
    public class AccountController : Controller
    {
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        private readonly OTPService otpService;
        private readonly SendMail sendMail;

        public AccountController(SignInManager<User> signInManager, UserManager<User> userManager, OTPService otpService, SendMail sendMail)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.otpService = otpService;
            this.sendMail = sendMail;
        }

        // Helper method để chuyển hướng dựa trên vai trò
        private async Task<IActionResult> RedirectBasedOnRole(User user)
        {
            var roles = await userManager.GetRolesAsync(user);
            System.IO.File.AppendAllText("redirect_log.txt", $"User: {user.Email}, Roles: {string.Join(",", roles)}, Time: {DateTime.Now}\n");
            // Các role quản lý - đi vào area quản lý tương ứng
            if (roles.Contains("SystemAdmin"))
                return RedirectToAction("SystemAdmin", "ManagerRole", new { area = "SystemAdmin" });
            else if (roles.Contains("Admin"))
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            else if (roles.Contains("Seller"))
                return RedirectToAction("Dashboard", "Seller", new { area = "Seller" });
            else if (roles.Contains("Manager"))
                return RedirectToAction("Index", "Dashboard", new { area = "Manager" });
            else if (roles.Contains("Nutri"))
                return RedirectToAction("Index", "Blog", new { area = "Nutri" });
            else if (roles.Contains("Customer"))
                return RedirectToAction("Index", "Home", new { area = "Store" });
            else
                // Guest hoặc không có role - đi vào Store area
                return RedirectToAction("Index", "Home", new { area = "Store" });
        }

        public IActionResult Login()
        {
            return View();
        }

        [Route("google-login")]
        public IActionResult LoginGoogle()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }


        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded || result.Principal == null)
            {
                TempData["error"] = "Đăng nhập Google thất bại!";
                return RedirectToAction("Login", "Account");
            }

            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
            var emailClaim = claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            var nameClaim = claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(emailClaim))
            {
                TempData["error"] = "Đăng nhập Google thất bại!";
                return RedirectToAction("Login", "Account");
            }

            var user = await userManager.FindByEmailAsync(emailClaim);
            if (user == null)
            {
                // Tạo người dùng mới
                user = new User
                {
                    UserName = emailClaim,
                    Email = emailClaim,
                    FullName = nameClaim ?? emailClaim,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    TempData["error"] = "Không thể tạo tài khoản mới!";
                    return RedirectToAction("Login", "Account");
                }

                // Gán vai trò Customer mặc định cho user mới
                await userManager.AddToRoleAsync(user, "Customer");
            }


            await signInManager.SignInAsync(user, isPersistent: false);
            TempData["success"] = "Đăng nhập Google thành công!";

            // Lấy vai trò của user và chuyển hướng phù hợp
            return await RedirectBasedOnRole(user);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user != null && !user.EmailConfirmed)
                {
                    ModelState.AddModelError("", "Bạn cần xác nhận email trước khi đăng nhập. Vui lòng kiểm tra email!");
                    return View(model);
                }
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    // Đổi tên biến user lồng nhau thành loggedInUser để tránh trùng tên
                    var loggedInUser = user;
                    var roles = await userManager.GetRolesAsync(loggedInUser);
                    TempData["success"] = "Đăng nhập thành công!";
                    // Chuyển hướng dựa trên vai trò
                    return await RedirectBasedOnRole(loggedInUser);
                }
                else
                {
                    ModelState.AddModelError("", "Email hoặc mật khẩu không đúng, vui lòng nhập lại.");
                    return View(model);
                }
            }
            return View(model);
        }


        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra email đã tồn tại chưa
                var existingUser = await userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng. Vui lòng chọn email khác hoặc đăng nhập nếu đã có tài khoản.");
                    return View(model);
                }

                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.Name,
                    PhoneNumber = model.PhoneNumber,
                    Gender = model.Gender,
                    Address = model.Address,
                    City = model.City,
                    Country = "Việt Nam",
                };
                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Gán role Customer mặc định cho user mới
                    await userManager.AddToRoleAsync(user, "Customer");

                    // gui ma OTP
                    var otp = otpService.GenerateOTP(user.Email);
                    sendMail.Send(user.Email, "Xác nhận đăng ký HealthFit", $"<b>Mã OTP của bạn là:</b> {otp}<br>Mã này sẽ hết hạn sau 2 phút.");

                    return RedirectToAction("VerifyOTP", new { email = user.Email });
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        // Dịch các lỗi Identity sang tiếng Việt
                        var vietnameseError = TranslateIdentityError(error.Description);
                        ModelState.AddModelError("", vietnameseError);
                    }
                    return View(model);
                }
            }
            return View(model);
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

        //xac thuc Email
        [HttpGet]
        public IActionResult VerifyEmail()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Email không tồn tại.");
                    return View(model);
                }
                // Gui OTP
                var otp = otpService.GenerateOTP(user.Email);
                sendMail.Send(user.Email, "Mã OTP đổi mật khẩu HealthFit", $"<b>Mã OTP của bạn là:</b> {otp}<br>Mã này sẽ hết hạn sau 2 phút.");
                // Chuyen sang luong nhap OTP
                return RedirectToAction("VerifyOTPResetPassword", new { email = user.Email });
            }
            return View(model);
        }


        //gui ma otp
        [HttpGet]
        public IActionResult VerifyOTP(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }

            var model = new OTPVerificationViewModel { Email = email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOTP(OTPVerificationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    TempData["error"] = "Tài khoản không tồn tại.";
                    return RedirectToAction("Login");
                }

                if (otpService.VerifyOTP(model.Email, model.OTP))
                {
                    user.EmailConfirmed = true;
                    await userManager.UpdateAsync(user);

                    // Đảm bảo user có role Customer
                    var roles = await userManager.GetRolesAsync(user);
                    if (!roles.Contains("Customer"))
                    {
                        await userManager.AddToRoleAsync(user, "Customer");
                    }

                    TempData["success"] = "Xác nhận email thành công! Bạn có thể đăng nhập.";
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError("", "Mã OTP không đúng hoặc đã hết hạn.");
                    return View(model);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyOTPResetPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }
            var model = new OTPVerificationViewModel { Email = email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOTPResetPassword(OTPVerificationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    TempData["error"] = "Tài khoản không tồn tại.";
                    return RedirectToAction("Login");
                }
                if (otpService.VerifyOTP(model.Email, model.OTP))
                {
                    // Luu email vao tempdata
                    TempData["ResetEmail"] = model.Email;
                    return RedirectToAction("ChangePassword");
                }
                else
                {
                    ModelState.AddModelError("", "Mã OTP không đúng hoặc đã hết hạn.");
                    return View(model);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var email = TempData["ResetEmail"] as string;
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }
            var model = new ChangePasswordViewModel { Email = email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Email không tồn tại.");
                    return View(model);
                }
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var result = await userManager.ResetPasswordAsync(user, token, model.NewPassword);
                if (result.Succeeded)
                {
                    TempData["success"] = "Đổi mật khẩu thành công! Bạn có thể đăng nhập.";
                    return RedirectToAction("Login");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
            }
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> UserProfile()
        {
            var user = await userManager.GetUserAsync(User) as User;
            if (user == null) return RedirectToAction("Login");

            // Lấy context
            var context = HttpContext.RequestServices.GetService(typeof(HealthFit.DataAccess.Data.HealthyShopContext)) as HealthFit.DataAccess.Data.HealthyShopContext;
            HealthFit.Models.CustomerProfile? profile = null;
            if (context != null)
            {
                profile = context.CustomerProfiles.FirstOrDefault(p => p.UserId == user.Id);
            }

            var vm = new UserProfileViewModel
            {
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Address = user.Address,
                City = user.City,
                Country = user.Country,
                Gender = profile?.Gender,
                Age = profile?.Age,
                Height = profile?.Height,
                Weight = profile?.Weight,
                Bmi = profile?.Bmi
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserProfile(UserProfileViewModel vm)
        {
            var user = await userManager.GetUserAsync(User) as User;
            if (user == null) return RedirectToAction("Login");

            if (!ModelState.IsValid)
                return View(vm);

            user.FullName = vm.FullName;
            user.PhoneNumber = vm.PhoneNumber;
            user.Address = vm.Address;
            user.City = vm.City;
            user.Country = vm.Country;
            await userManager.UpdateAsync(user);

            var context = HttpContext.RequestServices.GetService(typeof(HealthFit.DataAccess.Data.HealthyShopContext)) as HealthFit.DataAccess.Data.HealthyShopContext;
            HealthFit.Models.CustomerProfile? profile = null;
            if (context != null)
            {
                profile = context.CustomerProfiles.FirstOrDefault(p => p.UserId == user.Id);
                if (profile == null)
                {
                    profile = new HealthFit.Models.CustomerProfile { UserId = user.Id };
                    context.CustomerProfiles.Add(profile);
                }
                profile.Gender = vm.Gender;
                profile.Age = vm.Age;
                profile.Height = vm.Height;
                profile.Weight = vm.Weight;
                if (profile.Height.HasValue && profile.Weight.HasValue && profile.Height.Value > 0)
                {
                    var heightM = profile.Height.Value / 100;
                    profile.Bmi = (int)(profile.Weight.Value / (heightM * heightM));
                }
                else
                {
                    profile.Bmi = null;
                }
                await context.SaveChangesAsync();
            }
            TempData["success"] = "Cập nhật thành công!";
            return RedirectToAction("UserProfile");
        }

        [HttpGet]
        public IActionResult ChangeAccountPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangeAccountPassword(ChangeAccountPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);
                if (user == null)
                {
                    ModelState.AddModelError("", "Tài khoản không tồn tại hoặc chưa đăng nhập.");
                    return View(model);
                }
                var isCurrentPasswordValid = await userManager.CheckPasswordAsync(user, model.CurrentPassword);
                if (!isCurrentPasswordValid)
                {
                    ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng.");
                    return View(model);
                }
                if (model.NewPassword == model.CurrentPassword)
                {
                    ModelState.AddModelError("NewPassword", "Mật khẩu mới phải khác mật khẩu hiện tại.");
                    return View(model);
                }
                var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    TempData["success"] = "Đổi mật khẩu thành công!";
                    return RedirectToAction("Login");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
            }
            return View(model);
        }



        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home", new { area = "Store" });
        }
    }
}
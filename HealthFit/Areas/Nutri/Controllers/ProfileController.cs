using HealthFit.Models;
using HealthFit.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HealthFit.DataAccess.Data;

namespace HealthFit.Areas.Nutri.Controllers
{
    [Area("Nutri")]
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly HealthyShopContext _context;

        public ProfileController(UserManager<User> userManager, HealthyShopContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "Store" });

            var profile = _context.CustomerProfiles.FirstOrDefault(p => p.UserId == user.Id);
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
        public async Task<IActionResult> Index(UserProfileViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "Store" });

            if (!ModelState.IsValid)
                return View(vm);

            user.FullName = vm.FullName;
            user.PhoneNumber = vm.PhoneNumber;
            user.Address = vm.Address;
            user.City = vm.City;
            user.Country = vm.Country;
            await _userManager.UpdateAsync(user);
            var profile = _context.CustomerProfiles.FirstOrDefault(p => p.UserId == user.Id);
            if (profile == null)
            {
                profile = new CustomerProfile { UserId = user.Id };
                _context.CustomerProfiles.Add(profile);
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
            await _context.SaveChangesAsync();
            TempData["success"] = "Cập nhật thành công!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "Store" });

            var profile = _context.CustomerProfiles.FirstOrDefault(p => p.UserId == user.Id);
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
        public async Task<IActionResult> Edit(UserProfileViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "Store" });
            if (!ModelState.IsValid)
                return View(vm);
            user.FullName = vm.FullName;
            user.PhoneNumber = vm.PhoneNumber;
            user.Address = vm.Address;
            user.City = vm.City;
            user.Country = vm.Country;
            await _userManager.UpdateAsync(user);
            var profile = _context.CustomerProfiles.FirstOrDefault(p => p.UserId == user.Id);
            if (profile == null)
            {
                profile = new CustomerProfile { UserId = user.Id };
                _context.CustomerProfiles.Add(profile);
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
            await _context.SaveChangesAsync();
            TempData["success"] = "Cập nhật thành công!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangeAccountPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ModelState.AddModelError("", "Tài khoản không tồn tại hoặc chưa đăng nhập.");
                return View(model);
            }
            var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (!isCurrentPasswordValid)
            {
                ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng.");
                return View(model);
            }
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                TempData["success"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("Index");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }
    }
} 
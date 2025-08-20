using System.Security.Claims;
using HealthFit.Models;
using HealthFit.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthFit.Areas.Profile.Controllers
{
    [Area("Profile")]
    [Authorize] // Ensure only logged-in users can access
    public class ProfileController : Controller
    {
        private readonly ICustomerProfileService _profileService;

        public ProfileController(ICustomerProfileService profileService)
        {
            _profileService = profileService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Details");
        }

        public async Task<IActionResult> Details()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var profile = await _profileService.GetOrCreateProfileAsync(userId.Value);
            return View(profile);
        }

        public async Task<IActionResult> Edit()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var profile = await _profileService.GetProfileByUserIdWithUserAsync(userId.Value);

            if (profile == null)
            {
                return RedirectToAction(nameof(Details));
            }

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerProfile model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = GetCurrentUserId();
                    if (userId == null)
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    await _profileService.UpdateProfileAsync(model, userId.Value);
                    return RedirectToAction(nameof(Details));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(model);
        }

        private int? GetCurrentUserId()
        {
            // Get the current user's claims principal
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}

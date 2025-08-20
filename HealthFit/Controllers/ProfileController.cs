using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HealthFit.Models;
using Microsoft.EntityFrameworkCore;
using HealthFit.DataAccess.Data;
using System.Security.Claims;

namespace HealthFit.Controllers
{
    [Authorize] // Ensure only logged-in users can access
    public class ProfileController : Controller
    {
        private readonly HealthyShopContext _context;

        public ProfileController(HealthyShopContext context)
        {
            _context = context;
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

            var profile = await _context.CustomerProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                // Create new profile if it doesn't exist
                profile = new CustomerProfile
                {
                    UserId = userId.Value
                };
                _context.CustomerProfiles.Add(profile);
                await _context.SaveChangesAsync();

                // Reload profile with user information
                profile = await _context.CustomerProfiles
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.UserId == userId);
            }

            return View(profile);
        }

        public async Task<IActionResult> Edit()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var profile = await _context.CustomerProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

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

                    var profile = await _context.CustomerProfiles
                        .FirstOrDefaultAsync(p => p.UserId == userId);

                    if (profile == null)
                    {
                        return NotFound();
                    }

                    // Verify that the user owns this profile
                    if (profile.UserId != userId)
                    {
                        return Forbid();
                    }

                    // Update only the fields that can be modified
                    profile.Height = model.Height;
                    profile.Weight = model.Weight;
                    profile.Gender = model.Gender;
                    profile.Age = model.Age;
                    
                    // Calculate BMI if height and weight are provided
                    if (profile.Height.HasValue && profile.Weight.HasValue)
                    {
                        // BMI = weight (kg) / (height (m))²
                        var heightInMeters = profile.Height.Value / 100; // Convert cm to m
                        profile.Bmi = (int)(profile.Weight.Value / (heightInMeters * heightInMeters));
                    }

                    _context.Update(profile);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Details));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerProfileExists(model.ProfileId))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }
            return View(model);
        }

        private bool CustomerProfileExists(int id)
        {
            return _context.CustomerProfiles.Any(e => e.ProfileId == id);
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

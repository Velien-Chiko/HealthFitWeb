using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HealthFit.Models;
using System.Threading.Tasks;
using HealthFit.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace HealthFit.Controllers
{
    [Authorize(Roles = "Seller")]
    public class ProductReviewController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly HealthyShopContext _db;

        public ProductReviewController(UserManager<User> userManager, HealthyShopContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        // Hiển thị danh sách đánh giá sản phẩm của seller
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var sellerProductIds = await _db.Products
                .Where(p => p.CreatedBy == currentUser.Id)
                .Select(p => p.ProductId)
                .ToListAsync();

            var reviews = await _db.ProductReviews
                .Where(r => r.ProductId.HasValue && sellerProductIds.Contains(r.ProductId.Value))
                .Include(r => r.Product)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            return View(reviews);
        }

        // Phản hồi đánh giá
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int reviewId, string reply)
        {
            var review = await _db.ProductReviews.FindAsync(reviewId);
            if (review == null) return NotFound();

            review.SellerReply = reply;
            review.ReplyAt = DateTime.Now;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Phản hồi thành công!";
            return RedirectToAction("Index");
        }
    }
} 
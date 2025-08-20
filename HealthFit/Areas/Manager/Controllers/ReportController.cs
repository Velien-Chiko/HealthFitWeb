
using HealthFit.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthFit.Areas.Manager.Controllers
{
    [Area("Manager")]
    [Authorize(Roles = "Manager")]
    public class ReportController : Controller
    {
        private readonly IOrderService _orderService;

        public ReportController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // Trang chính của báo cáo
        public async Task<IActionResult> Index(int? year)
        {
            int selectedYear = year ?? DateTime.Now.Year;
            var monthlyData = await _orderService.GetMonthlyRevenueReportAsync(selectedYear);
            var quarterlyData = await _orderService.GetQuarterlyRevenueReportAsync(selectedYear);

            ViewBag.SelectedYear = selectedYear;
            ViewBag.MonthlyData = monthlyData;
            ViewBag.QuarterlyData = quarterlyData;

            return View();
        }


        [HttpGet]
        public async Task<IActionResult> SearchOrders(string username)
        {
            try
            {
                var orders = await _orderService.SearchOrdersByUsernameAsync(username);
                return View("SearchOrder", orders);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi ở đây nếu cần
                TempData["Error"] = "Không thể tìm kiếm đơn hàng: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // Trang chi tiết đơn hàng
        [HttpGet]
        public async Task<IActionResult> OrderDetails(int orderId)
        {
            try
            {
                var orderDetails = await _orderService.GetOrderDetailsByIdAsync(orderId);
                if (orderDetails == null)
                {
                    return NotFound();
                }
                return View(orderDetails);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi ở đây nếu cần
                TempData["Error"] = "Không thể lấy chi tiết đơn hàng";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

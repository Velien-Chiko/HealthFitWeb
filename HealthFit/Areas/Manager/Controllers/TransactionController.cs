using HealthFit.Services.Interfaces;
using HealthFit.Services.Interfaces.Export;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthFit.Areas.Manager.Controllers
{
    [Area("Manager")]
    [Authorize(Roles = "Manager")]
    public class TransactionController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IOrderExportService _orderExportService;

        public TransactionController(IOrderService orderService, IOrderExportService orderExportService)
        {
            _orderService = orderService;
            _orderExportService = orderExportService;
        }
        public async Task<IActionResult> IndexAsync()
        {
            var latestOrders = await _orderService.GetLatestOrdersAsync();
            return View(latestOrders);
        }

        [HttpGet]
        public async Task<IActionResult> SearchOrders(string username)
        {
            try
            {
                var orders = await _orderService.SearchOrdersByUsernameAsync(username);
                ViewBag.Username = username;
                return View("SearchOrder", orders);
            }
            catch (Exception ex)
            {

                TempData["Error"] = "Không thể tìm kiếm đơn hàng: " + ex.Message;
                return RedirectToAction(nameof(IndexAsync));
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
                return RedirectToAction(nameof(IndexAsync));
            }
        }

        public async Task<IActionResult> ExportSearchOrderToExcel(string username)
        {
            var orders = await _orderService.SearchOrdersByUsernameAsync(username);
            if (orders == null)
            {
                TempData["error"] = "Không tìm thấy đơn hàng nào phù hợp để xuất Excel.";
                return RedirectToAction("Index");
            }

            var stream = _orderExportService.ExportOrdersToExcel(orders);

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "KetQuaTimKiemDonHang.xlsx");
        }


    }
}

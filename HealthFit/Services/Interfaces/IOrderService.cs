using HealthFit.Models;
using HealthFit.Models.DTO.Order;
using HealthFit.Models.DTO.Report;

namespace HealthFit.Services.Interfaces
{
    public interface IOrderService
    {
        /// <summary>
        /// Lấy chi tiết đơn hàng (bao gồm sản phẩm và meal plan) theo mã đơn hàng.
        /// </summary>
        Task<OrderDto> GetOrderDetailsByIdAsync(int orderId);

        /// <summary>
        /// Báo cáo doanh thu theo tháng của năm chỉ định.
        /// </summary>
        Task<List<MonthlyRevenueReportDto>> GetMonthlyRevenueReportAsync(int year);

        /// <summary>
        /// Báo cáo doanh thu theo quý của năm chỉ định.
        /// </summary>
        Task<List<QuarterlyRevenueReportDto>> GetQuarterlyRevenueReportAsync(int year);

        /// <summary>
        /// Tìm kiếm đơn hàng theo tên người dùng.
        /// </summary>
        Task<List<OrderSearchResultDto>> SearchOrdersByUsernameAsync(string username);

        /// <summary>
        /// Lấy toàn bộ đơn hàng
        /// </summary>
        Task<List<Order>> GetAll();

        /// <summary>
        /// Lấy 10 đơn hàng gần nhất, sắp xếp theo ngày đặt hàng.
        /// </summary>
        /// <returns>Danh sách đơn hàng ở dạng OrderSearchResultDto</returns>
        Task<List<OrderSearchResultDto>> GetLatestOrdersAsync();

        /// <summary>
        /// Lấy tổng doanh thu hôm nay từ đơn hàng đã hoàn tất.
        /// </summary>
        Task<decimal> GetTodayRevenueAsync();

        /// <summary>
        /// Lấy số lượng đơn hàng đã tạo hôm nay.
        /// </summary>
        Task<int> GetTodayOrderCountAsync();


    }
}

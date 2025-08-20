using HealthFit.Models;
using HealthFit.Models.DTO.Order;
using HealthFit.Models.DTO.Report;

namespace HealthFit.DataAccess.Repository.IRepository
{
    public interface IOrderRepository : IRepository<Order>
    {
        /// <summary>
        /// Lấy thông tin chi tiết đơn hàng (bao gồm sản phẩm và meal plan) theo OrderId.
        /// </summary>
        /// <param name="orderId">Mã đơn hàng cần lấy</param>
        /// <returns>Thông tin đơn hàng ở dạng OrderDto</returns>
        Task<OrderDto> GetOrderDetailsByIdAsync(int orderId);

        /// <summary>
        /// Sinh báo cáo doanh thu theo từng tháng trong một năm cụ thể.
        /// </summary>
        /// <param name="year">Năm cần thống kê</param>
        /// <returns>Danh sách doanh thu từng tháng</returns>
        Task<List<MonthlyRevenueReportDto>> GetMonthlyRevenueReport(int year);

        /// <summary>
        /// Sinh báo cáo doanh thu theo từng quý trong một năm cụ thể.
        /// </summary>
        /// <param name="year">Năm cần thống kê</param>
        /// <returns>Danh sách doanh thu từng quý</returns>
        Task<List<QuarterlyRevenueReportDto>> GetQuarterlyRevenueReport(int year);

        /// <summary>
        /// Tìm kiếm đơn hàng theo tên người dùng.
        /// </summary>
        /// <param name="username">Tên người dùng (username hoặc họ tên)</param>
        /// <returns>Danh sách kết quả đơn hàng phù hợp</returns>
        Task<List<OrderSearchResultDto>> SearchOrdersByUsername(string username);
        void Update(Order obj);
        void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);
        void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId);
        /// <summary>
        /// Lấy danh sách 10 đơn hàng mới nhất, sắp xếp theo ngày đặt hàng giảm dần.
        /// </summary>
        /// <returns>Danh sách 10 đơn hàng mới nhất ở dạng OrderSearchResultDto</returns>
        Task<List<OrderSearchResultDto>> GetLatestOrdersAsync();

        /// <summary>
        /// Tính tổng doanh thu của tất cả đơn hàng đã hoàn tất trong ngày hôm nay.
        /// </summary>
        /// <returns>Tổng doanh thu hôm nay (đơn vị: decimal).</returns>
        Task<decimal> GetTodayRevenueAsync();

        /// <summary>
        /// Đếm tổng số đơn hàng được tạo trong ngày hôm nay (bất kể trạng thái).
        /// </summary>
        /// <returns>Số lượng đơn hàng trong ngày hôm nay.</returns>
        Task<int> GetTodayOrderCountAsync();

    }
}

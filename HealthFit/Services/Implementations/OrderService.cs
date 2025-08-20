using HealthFit.DataAccess.Repository.IRepository;
using HealthFit.Models;
using HealthFit.Models.DTO.Order;
using HealthFit.Models.DTO.Report;
using HealthFit.Services.Interfaces;

namespace HealthFit.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, ILogger<OrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public Task<List<Order>> GetAll()
        {
            _logger.LogInformation("Lấy toàn bộ đơn hàng");

            var result = _unitOfWork.Order.GetAll();
            return Task.FromResult(result);
        }

        public async Task<List<MonthlyRevenueReportDto>> GetMonthlyRevenueReportAsync(int year)
        {
            _logger.LogInformation("Lấy báo cáo doanh thu theo tháng cho năm: {Year}", year);
            return await _unitOfWork.Order.GetMonthlyRevenueReport(year);
        }

        public async Task<List<QuarterlyRevenueReportDto>> GetQuarterlyRevenueReportAsync(int year)
        {
            _logger.LogInformation("Lấy báo cáo doanh thu theo quý cho năm: {Year}", year);
            return await _unitOfWork.Order.GetQuarterlyRevenueReport(year);
        }

        public async Task<OrderDto> GetOrderDetailsByIdAsync(int orderId)
        {
            _logger.LogInformation("Lấy lịch sử giao dịch cho đơn hàng ID: {OrderId}", orderId);
            return await _unitOfWork.Order.GetOrderDetailsByIdAsync(orderId);
        }

        public async Task<List<OrderSearchResultDto>> SearchOrdersByUsernameAsync(string username)
        {
            _logger.LogInformation("Tìm kiếm đơn hàng theo username: {Username}", username);
            return await _unitOfWork.Order.SearchOrdersByUsername(username);
        }

        public async Task<List<OrderSearchResultDto>> GetLatestOrdersAsync()
        {
            _logger.LogInformation("Lấy 10 đơn hàng gần nhất");
            return await _unitOfWork.Order.GetLatestOrdersAsync();
        }

        public async Task<decimal> GetTodayRevenueAsync()
        {
            return await _unitOfWork.Order.GetTodayRevenueAsync();
        }

        public async Task<int> GetTodayOrderCountAsync()
        {
            return await _unitOfWork.Order.GetTodayOrderCountAsync();
        }
    }
}

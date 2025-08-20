using HealthFit.DataAccess.Data;
using HealthFit.Models;
using HealthFit.Models.DTO.Order;
using HealthFit.Models.DTO.Report;
using Microsoft.EntityFrameworkCore;

namespace HealthFit.DataAccess.Repository
{

    public class OrderRepository : Repository<Models.Order>, IRepository.IOrderRepository
    {
        private readonly HealthyShopContext _dbContext;
        public OrderRepository(HealthyShopContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<OrderSearchResultDto>> GetLatestOrdersAsync()
        {
            return await _dbContext.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new OrderSearchResultDto
                {
                    OrderId = o.OrderId,
                    // Nếu đơn của user đã đăng ký thì lấy FullName từ bảng User, còn không thì lấy từ Order.FullName (guest)
                    UserName = o.User != null ? o.User.FullName : o.FullName,
                    OrderDate = o.OrderDate ?? DateTime.MinValue,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status ?? "Chưa rõ"
                })
                .ToListAsync();
        }



        public async Task<List<MonthlyRevenueReportDto>> GetMonthlyRevenueReport(int year)
        {
            return await _dbContext.Orders
                          .Where(o => o.OrderDate.HasValue
                                      && o.OrderDate.Value.Year == year
                                      && o.Status == "Completed")
                          .GroupBy(o => new
                          {
                              Year = o.OrderDate.Value.Year,
                              Month = o.OrderDate.Value.Month
                          })

                          .Select(g => new MonthlyRevenueReportDto
                          {
                              Year = g.Key.Year,
                              Month = g.Key.Month,
                              // od đại diện cho mỗi phần tử trong nhóm g, tức là mỗi đối tượng Order.
                              TotalRevenue = g.Sum(od => od.TotalAmount),
                              OrderCount = g.Count()
                          })
                          .OrderBy(r => r.Year)
                          .ThenBy(r => r.Month)
                          .ToListAsync();

        }

        public async Task<List<QuarterlyRevenueReportDto>> GetQuarterlyRevenueReport(int year)
        {
            return await _dbContext.Orders

                .Where(o => o.OrderDate.HasValue
                            && o.OrderDate.Value.Year == year
                            && o.Status == "Completed")
                .GroupBy(o => new
                {
                    Year = o.OrderDate.Value.Year,
                    Quarter = ((o.OrderDate.Value.Month - 1) / 3) + 1
                })
                .Select(g => new QuarterlyRevenueReportDto
                {
                    Year = g.Key.Year,
                    Quarter = g.Key.Quarter,
                    TotalRevenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(r => r.Quarter)
                .ToListAsync();
        }

        public async Task<int> GetTodayOrderCountAsync()
        {
            var today = DateTime.Today;
            return await _dbContext.Orders
                .CountAsync(o => o.OrderDate.HasValue &&
                                 o.OrderDate.Value.Date == today);
        }

        public async Task<decimal> GetTodayRevenueAsync()
        {
            var today = DateTime.Today;
            return await _dbContext.Orders
                .Where(o => o.OrderDate.HasValue &&
                            o.OrderDate.Value.Date == today &&
                            o.Status == "Completed")
                .SumAsync(o => o.TotalAmount);
        }


        public async Task<OrderDto> GetOrderDetailsByIdAsync(int orderId)
        {
            // Đánh dấu await ngay trước FirstOrDefaultAsync(), và method phải có async
            return await _dbContext.Orders
                .Where(o => o.OrderId == orderId)
                .Select(o => new OrderDto
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate ?? DateTime.MinValue,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status ?? "Unknown",
                    SellerName = o.Seller != null ? o.Seller.FullName : "Unknown",
                    Products = o.OrderProductDetails
                        .Select(op => new OrderProductDto
                        {
                            //FK
                            ProductId = op.ProductId,
                            ProductName = op.Product.Name,
                            Quantity = op.Quantity,
                            UnitPrice = op.UnitPrice,
                            Subtotal = op.Subtotal,
                        })
                        .ToList(),
                    MealPlans = o.OrderMealPlanDetails
                        .Select(omp => new OrderMealPlanDto
                        {
                            MealPlanDetailId = omp.MealPlanDetailId,
                            PlanDescription = omp.MealPlanDetail.PlanDescription,
                            Quantity = omp.Quantity,
                            UnitPrice = omp.UnitPrice,
                            Subtotal = omp.Quantity * omp.UnitPrice
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<OrderSearchResultDto>> SearchOrdersByUsername(string username)
        {
            return await _dbContext.Orders
                .Include(o => o.User)
                .Where(o =>
                  // Đơn hàng của user
                  (o.User != null &&
                     (EF.Functions.Like(o.User.UserName, $"%{username}%") ||
                      EF.Functions.Like(o.User.FullName, $"%{username}%")))
                      || EF.Functions.Like(o.FullName, $"%{username}%")
                )
                .Select(o => new OrderSearchResultDto
                {
                    OrderId = o.OrderId,
                    // Nếu có User thì lấy FullName của User, không thì lấy FullName của Order (guest)
                    UserName = o.User != null ? o.User.FullName : o.FullName,
                    OrderDate = o.OrderDate ?? System.DateTime.MinValue,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status ?? "Unknown"
                })
                .OrderByDescending(o => o.OrderDate)
                .Take(50)
                .ToListAsync();
        }

        public void Update(Order obj)
        {
            _dbContext.Orders.Update(obj);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus)
        {
            var orderFromDb = _dbContext.Orders.FirstOrDefault(o => o.OrderId == id);
            if (orderFromDb != null)
            {
                orderFromDb.Status = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
            else
            {
                throw new Exception("Order not found");
            }
        }

        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
        {
            var orderFromDb = _dbContext.Orders.FirstOrDefault(o => o.OrderId == id);
            if (orderFromDb != null)
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    orderFromDb.SessionId = sessionId;
                }
                if (!string.IsNullOrEmpty(paymentIntentId))
                {
                    orderFromDb.PaymentIntentId = paymentIntentId;
                    orderFromDb.PaymentDate = DateTime.Now;
                }
            }
            else
            {
                throw new Exception("Order not found");
            }
        }

    }
}


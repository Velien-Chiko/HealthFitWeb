namespace HealthFit.Models.Constants
{
    /// <summary>
    /// Chứa các hằng số trạng thái đơn hàng dùng toàn hệ thống
    /// </summary>
    public class OrderStatusConstants
    {
        
        public const string Confirmed = "Đã Thanh Toán";
        public const string Shipped = "Đang giao";
        public const string Delivered = "Completed";
        public const string Cancelled = "Đã hủy";

        public static readonly List<string> All = new()
        {
            
            Confirmed,
            Shipped,
            Delivered,
            Cancelled
        };
    }
} 
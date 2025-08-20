namespace HealthFit.Models.DTO.NewFolder
{
    public class DashBoardDTO
    {
        public int TotalUsers { get; set; }
        public int OrdersToday { get; set; }
        public decimal MonthlyRevenue { get; set; }

        public int ProcessingOrders { get; set; }
    }
}

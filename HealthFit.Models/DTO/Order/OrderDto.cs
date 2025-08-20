namespace HealthFit.Models.DTO.Order
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string SellerName { get; set; }
        public List<OrderProductDto> Products { get; set; }
        public List<OrderMealPlanDto> MealPlans { get; set; }
    }
}

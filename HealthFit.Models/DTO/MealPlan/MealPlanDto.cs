namespace HealthFit.Models.DTO.MealPlan
{
    public class MealPlanDto
    {
        public int MealPlanId { get; set; }
        public string? MealPlanName { get; set; }
        public List<MealPlanProductDto> Products { get; set; } = new();
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public int QuantitySold { get; set; }
        public int QuantityInStock { get; set; }
        public decimal Price { get; set; }
        public string Bmirange { get; set; }
        public decimal SavingsAmount { get; set; }

    }
}

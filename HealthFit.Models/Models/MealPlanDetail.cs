using HealthFit.Models.Models;

namespace HealthFit.Models;

public partial class MealPlanDetail
{
    public int MealPlanDetailId { get; set; }

    public string Bmirange { get; set; } = null!;

    public string PlanDescription { get; set; } = null!;

    public string IsApproved { get; set; }

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public virtual ICollection<MealPlanProductDetail> MealPlanProductDetails { get; set; } = new List<MealPlanProductDetail>();

    public ICollection<OrderMealPlanDetail> OrderMealPlanDetails { get; set; }
}

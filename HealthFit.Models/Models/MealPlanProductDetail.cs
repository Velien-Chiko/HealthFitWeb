namespace HealthFit.Models;

public partial class MealPlanProductDetail
{
    public int MealPlanProductDetailId { get; set; }

    public int? MealPlanDetailId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public virtual MealPlanDetail? MealPlanDetail { get; set; }

    public virtual Product? Product { get; set; }
}

using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using HealthFit.Models;

public class CartItem
{
    public int CartItemId { get; set; }

    public int? UserId { get; set; }

    // Either ProductId or MealPlanDetailId (but not both)
    public int? ProductId { get; set; }

    public int? MealPlanDetailId { get; set; }

    public int Quantity { get; set; }

    public DateTime? AddedDate { get; set; }
    [JsonIgnore]
    public virtual Product? Product { get; set; }
    [JsonIgnore]
    public virtual MealPlanDetail? MealPlanDetail { get; set; }

    public virtual User? User { get; set; }

    [NotMapped]
    public decimal Price { get; set; }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HealthFit.Models.Models;

namespace HealthFit.Models;

public partial class Product
{
    [Key]
    public int ProductId { get; set; }
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Ingredients { get; set; }
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    [StringLength(20)]
    public string? IsActive { get; set; }


    public int? CreatedBy { get; set; }

    public int Quantity { get; set; }

    public int Calo { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<MealPlanProductDetail> MealPlanProductDetails { get; set; } = new List<MealPlanProductDetail>();

    public virtual ICollection<OrderProductDetail> OrderProductDetails { get; set; } = new List<OrderProductDetail>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    public virtual ICollection<ProductCategoryMapping> ProductCategoryMappings { get; set; } = new List<ProductCategoryMapping>();

}

using System.ComponentModel.DataAnnotations;
using HealthFit.Models.Models;


namespace HealthFit.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? UserId { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string? Status { get; set; }

    public int? SellerId { get; set; }
    public DateTime? LastUpdated { get; set; }

    // Payment properties
    public string? PaymentStatus { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateOnly PaymentDueDate { get; set; }
    public string? SessionId { get; set; }
    public string? PaymentIntentId { get; set; }

    // Shipping properties
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public virtual ICollection<OrderProductDetail> OrderProductDetails { get; set; } = new List<OrderProductDetail>();

    public virtual ICollection<OrderMealPlanDetail> OrderMealPlanDetails { get; set; } = new List<OrderMealPlanDetail>();

    // Navigation properties
    public virtual User? Seller { get; set; }

    public virtual User? User { get; set; }

    public virtual Payment? Payment { get; set; }
}


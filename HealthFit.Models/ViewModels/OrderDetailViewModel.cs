using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthFit.Models.ViewModels
{
    public class OrderDetailViewModel
    {
        public int OrderId { get; set; }
        public DateTime? OrderDate { get; set; }

        public string FullName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Address { get; set; } = "";
        public string City { get; set; } = "";
        public string Country { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PaymentStatus { get; set; }
        public string? Status { get; set; }
        public decimal TotalAmount { get; set; }

        public List<OrderProductItem> OrderProducts { get; set; } = new();
        public List<OrderMealPlanItem> OrderMealPlans { get; set; } = new();
    }

    public class OrderProductItem
    {
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
    }

    public class OrderMealPlanItem
    {
        public string MealPlanName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthFit.Models.Models
{
    public class OrderMealPlanDetail
    {
        public int OrderMealPlanDetailId { get; set; }   // PK

        public int OrderId { get; set; }                 // FK → Order
        public Order Order { get; set; }                 // Nav-prop

        public int MealPlanDetailId { get; set; }        // FK → MealPlanDetail (combo)
        public MealPlanDetail MealPlanDetail { get; set; } // Nav-prop

        public int Quantity { get; set; }                // Số gói mua
        public decimal UnitPrice { get; set; }           // Giá 1 gói lúc mua
    }

}

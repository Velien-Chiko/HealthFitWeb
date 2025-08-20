using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthFit.Models.DTO.Cart
{
    public class PendingOrderDTO
    {
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public List<PendingOrderItemDTO> Items { get; set; }
    }

    public class PendingOrderItemDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

}

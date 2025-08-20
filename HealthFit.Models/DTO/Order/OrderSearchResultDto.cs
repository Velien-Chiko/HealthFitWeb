using System;

namespace HealthFit.Models.DTO.Order
{
    public class OrderSearchResultDto
    {
        public int OrderId { get; set; }
        public string UserName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
    }
} 
namespace HealthFit.Models.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; } // FK đến Order
        public string Method { get; set; } // VD: "COD", "CreditCard", "Momo", "ZaloPay"
        public string Status { get; set; } // VD: "Pending", "Completed", "Failed"
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }

        public Order Order { get; set; }
    }
}

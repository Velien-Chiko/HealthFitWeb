using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthFit.Models.Models.VNPay
{
    public class PaymentResponseModel
    {
        public bool Success { get; set; }
        public string PaymentMethod { get; set; }
        public string OrderDescription { get; set; }
        public string OrderId { get; set; }
        public string PaymentId { get; set; }
        public string TransactionId { get; set; }
        public string Token { get; set; }
        public string VnPayResponseCode { get; set; }
        public decimal Amount { get; set; } // Thêm Amount để lưu số tiền thanh toán

}
    public class VnPayRequestModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Amount { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthFit.Models.ViewModels
{
    public class CartItemVM
    {
        public IEnumerable<CartItemDisplayDTO> CartItemList { get; set; } = new List<CartItemDisplayDTO>();
        public OrderInputDTO Order { get; set; } = new OrderInputDTO();
    }

    public class CartItemDisplayDTO
    {
        public int CartItemId { get; set; }
        public int Quantity { get; set; }

        public int? ProductId { get; set; }
        public int? MealPlanDetailId { get; set; }

        public string Name { get; set; } = "";
        public string? Description { get; set; } // 👈 Thêm trường này
        public string? ImageUrl { get; set; }

        public decimal UnitPrice { get; set; }
        public decimal SubTotal => UnitPrice * Quantity;

        public bool IsMealPlan => MealPlanDetailId.HasValue;
        public int MaxQuantity { get; set; }
    }

    public class OrderInputDTO
    {
        [Required(ErrorMessage = "Hãy điền thông tin.")]
        [StringLength(11, MinimumLength = 9, ErrorMessage = "Số điện thoại bắt buộc từ 9 đến 11 chữ số.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Số điện thoại chỉ được chứa các chữ số.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Hãy điền thông tin.")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Hãy điền thông tin.")]
        public string? City { get; set; }

        [Required(ErrorMessage = "Hãy điền thông tin.")]
        public string? Country { get; set; }

        [Required(ErrorMessage = "Hãy điền thông tin.")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Hãy điền thông tin.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
        public string? Email { get; set; }

        public User? User { get; set; }

        public decimal TotalAmount { get; set; }
    }

}
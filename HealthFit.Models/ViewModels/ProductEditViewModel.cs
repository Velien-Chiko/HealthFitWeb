using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HealthFit.Models.ViewModels
{
    public class ProductEditViewModel
    {
        public int ProductId { get; set; }

        public string? Name { get; set; }

        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm")]
        public int Quantity { get; set; }

        public string? CurrentImageUrl { get; set; }
        public string? IsActive { get; set; }

        public IFormFile? NewImage { get; set; }
    }
}

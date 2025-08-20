using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace HealthFit.Models.ViewModels
{
    public class UserProfileViewModel
    {
        // User fields
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        [Display(Name = "Họ và tên")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Display(Name = "Số Điện Thoại")]
        [StringLength(11, MinimumLength = 10, ErrorMessage = "Số điện thoại phải có từ 10 đến 11 ký tự")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Email")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Display(Name = "Thành phố")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string? City { get; set; }

        [Display(Name = "Quốc gia")]
        public string? Country { get; set; }

        // CustomerProfile fields
        [Display(Name = "Giới tính")]
        public string? Gender { get; set; }

        [Display(Name = "Tuổi")]
        public int? Age { get; set; }

        [Display(Name = "Chiều cao (cm)")]
        public decimal? Height { get; set; }

        [Display(Name = "Cân nặng (kg)")]
        public decimal? Weight { get; set; }

        [Display(Name = "BMI")]
        public decimal? Bmi { get; set; }
    }
}

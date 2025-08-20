using System.ComponentModel.DataAnnotations;

namespace HealthFit.Models.ViewModels
{
    public class OTPVerificationViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có 6 ký tự")]
        [Display(Name = "Mã OTP")]
        public string OTP { get; set; }

        public string Email { get; set; }
    }
} 
using System.ComponentModel.DataAnnotations;

namespace HealthFit.Models.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Cần phải xác nhận mật khẩu")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "Mật khẩu phải dài ít nhất 8 ký tự.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        [Compare("ConfirmNewPassword", ErrorMessage = "Mật khẩu không trùng nhau.")]

        public string NewPassword { get; set; }
        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]

        public string ConfirmNewPassword { get; set; }
    }
}

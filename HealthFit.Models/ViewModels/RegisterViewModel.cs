using System.ComponentModel.DataAnnotations;

namespace HealthFit.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        [Display(Name = "Họ và Tên")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "Mật khẩu phải dài ít nhất {2} ký tự và kí tự đặc biệt.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật Khẩu")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Xác nhận mật khẩu không khớp.")]
        [Display(Name = "Xác Nhận Mật Khẩu")]
        public string ConfirmPassword { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Display(Name = "Số Điện Thoại")]
        [StringLength(11, MinimumLength = 10, ErrorMessage = "Số điện thoại phải có từ 10 đến 11 ký tự")]
        public string PhoneNumber { get; set; }
        //city
        [Required(ErrorMessage = "Vui lòng chọn thành phố")]
        [Display(Name = "Thành Phố")]
        public string City { get; set; }
        //dia chi
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [Display(Name = "Địa Chỉ")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        [Display(Name = "Giới Tính")]
        public string Gender { get; set; }

    }
}

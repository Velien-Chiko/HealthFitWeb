using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthFit.Models.ViewModels
{
    public class ChangeAccountPasswordViewModel
    {
      
            // [Required(ErrorMessage = "Vui lòng nhập Email")]
            // [EmailAddress]
            // public string Email { get; set; }
            [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu hiện tại")]
            public string CurrentPassword { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
            [StringLength(40, MinimumLength = 8, ErrorMessage = "Mật khẩu phải dài ít nhất 8 ký tự.")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu mới")]
            public string NewPassword { get; set; }

            [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
            [DataType(DataType.Password)]
            [Display(Name = "Xác nhận mật khẩu mới")]
            [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
            public string ConfirmNewPassword { get; set; }
        
    }
}

using Microsoft.AspNetCore.Identity;

public class VietnameseIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError PasswordTooShort(int length)
        => new IdentityError { Code = nameof(PasswordTooShort), Description = $"Mật khẩu phải có ít nhất {length} ký tự." };

    public override IdentityError PasswordRequiresNonAlphanumeric()
        => new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Mật khẩu phải chứa ít nhất một ký tự đặc biệt." };

    public override IdentityError PasswordRequiresDigit()
        => new IdentityError { Code = nameof(PasswordRequiresDigit), Description = "Mật khẩu phải chứa ít nhất một chữ số." };

    public override IdentityError PasswordRequiresLower()
        => new IdentityError { Code = nameof(PasswordRequiresLower), Description = "Mật khẩu phải chứa ít nhất một chữ thường." };

    public override IdentityError PasswordRequiresUpper()
        => new IdentityError { Code = nameof(PasswordRequiresUpper), Description = "Mật khẩu phải chứa ít nhất một chữ hoa." };

    // Bạn có thể override thêm các phương thức khác nếu muốn dịch toàn bộ thông báo
} 
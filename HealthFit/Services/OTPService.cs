using System.Security.Cryptography;
using System.Text;

namespace HealthFit.Services
{
    public class OTPService
    {
        private static readonly Dictionary<string, (string Code, DateTime ExpiryTime)> _otpStore = new();

        public string GenerateOTP(string email)
        {
            // tao ma OTP
            var otp = new StringBuilder();
            using (var rng = RandomNumberGenerator.Create())
            {
                var otpBytes = new byte[4];
                rng.GetBytes(otpBytes);
                var otpNumber = Math.Abs(BitConverter.ToInt32(otpBytes, 0)) % 1000000;
                otp.Append(otpNumber.ToString("D6"));
            }

            // nhap thoi gian ma OTP
            _otpStore[email] = (otp.ToString(), DateTime.UtcNow.AddMinutes(2));

            return otp.ToString();
        }

        public bool VerifyOTP(string email, string otp)
        {
            if (!_otpStore.TryGetValue(email, out var storedOtp))
            {
                return false;
            }

            if (DateTime.UtcNow > storedOtp.ExpiryTime)
            {
                _otpStore.Remove(email);
                return false;
            }

            if (storedOtp.Code != otp)
            {
                return false;
            }

            _otpStore.Remove(email);
            return true;
        }
    }
} 
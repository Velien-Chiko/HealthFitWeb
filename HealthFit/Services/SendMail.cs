using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace HealthFit.Services
{
    public class SendMail
    {
        private readonly IConfiguration _configuration;

        public SendMail(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool Send(string to, string subject, string body)
        {
            try
            {
                var emailConfig = _configuration.GetSection("EMAIL_CONFIGURATION");
                var fromAddress = new MailAddress(emailConfig["EMAIL"], "HealthFit");
                var toAddress = new MailAddress(to);
                string fromPassword = emailConfig["PASSWORD"];

                var smtp = new SmtpClient
                {
                    Host = emailConfig["HOST"],
                    Port = int.Parse(emailConfig["PORT"]),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
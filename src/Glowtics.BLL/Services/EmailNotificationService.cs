using System;
using System.IO;
using System.Threading.Tasks;
using Glowtics.BLL.Constants;
using Glowtics.BLL.Interfaces;

namespace Glowtics.BLL.Services
{
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly IEmailService _emailService;

        public EmailNotificationService(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task SendRegistrationEmailAsync(string toEmail, string otpCode)
        {
            var htmlBody = await GetPopulatedTemplateAsync(
                EmailConstants.Registration.Title,
                EmailConstants.Registration.Message,
                otpCode,
                EmailConstants.Registration.Footer);

            await _emailService.SendEmailAsync(toEmail, EmailConstants.Registration.Subject, htmlBody);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string otpCode)
        {
            var htmlBody = await GetPopulatedTemplateAsync(
                EmailConstants.PasswordReset.Title,
                EmailConstants.PasswordReset.Message,
                otpCode,
                EmailConstants.PasswordReset.Footer);

            await _emailService.SendEmailAsync(toEmail, EmailConstants.PasswordReset.Subject, htmlBody);
        }

        private async Task<string> GetPopulatedTemplateAsync(string title, string message, string otpCode, string footerContext)
        {
            string htmlBody;
            var assembly = typeof(EmailNotificationService).Assembly;
            var resourceName = "Glowtics.BLL.Resources.Email.OtpTemplate.html";
            
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream!))
            {
                htmlBody = await reader.ReadToEndAsync();
            }

            return htmlBody.Replace("{{Title}}", title)
                           .Replace("{{Message}}", message)
                           .Replace("{{OtpCode}}", otpCode)
                           .Replace("{{FooterContext}}", footerContext)
                           .Replace("{{Year}}", DateTime.UtcNow.Year.ToString());
        }
    }
}

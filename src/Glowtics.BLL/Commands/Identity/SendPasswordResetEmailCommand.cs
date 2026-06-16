using System;
using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.Interfaces;
using Glowtics.DAL.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Glowtics.BLL.Commands.Identity
{
    public record SendPasswordResetEmailCommand(string Email) : IRequest<bool>;

    public class SendPasswordResetEmailCommandHandler : IRequestHandler<SendPasswordResetEmailCommand, bool>
    {
        private readonly UserManager<GlowticsUser> _userManager;
        private readonly IEmailService _emailService;

        public SendPasswordResetEmailCommandHandler(UserManager<GlowticsUser> userManager, IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task<bool> Handle(SendPasswordResetEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return true;
            }

            // invalidate any previously generated OTPs 
            await _userManager.UpdateSecurityStampAsync(user);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            string htmlBody;
            var assembly = typeof(SendPasswordResetEmailCommandHandler).Assembly;
            var resourceName = "Glowtics.BLL.Resources.Email.OtpTemplate.html";
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new System.IO.StreamReader(stream!))
            {
                htmlBody = await reader.ReadToEndAsync();
            }

            htmlBody = htmlBody.Replace("{{Title}}", "Reset Your Password")
                               .Replace("{{Message}}", "We received a request to reset your password. Please use the following one-time password (OTP) to choose a new password.")
                               .Replace("{{OtpCode}}", token)
                               .Replace("{{FooterContext}}", "If you didn't request a password reset, you can safely ignore this email.")
                               .Replace("{{Year}}", DateTime.UtcNow.Year.ToString());

            await _emailService.SendEmailAsync(user.Email!, "Reset your Glowtics Password", htmlBody);

            return true;
        }
    }
}

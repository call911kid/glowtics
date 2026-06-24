using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Glowtics.BLL.Exceptions;
using Glowtics.BLL.Interfaces;
using Glowtics.BLL.Settings;
using Glowtics.DAL.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Glowtics.BLL.Constants;

namespace Glowtics.BLL.Commands.Identity
{
    public record SendConfirmationEmailCommand(Guid UserId) : IRequest<bool>;

    public class SendConfirmationEmailCommandHandler : IRequestHandler<SendConfirmationEmailCommand, bool>
    {
        private readonly UserManager<GlowticsUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly EmailSettings _settings;

        public SendConfirmationEmailCommandHandler(UserManager<GlowticsUser> userManager, IEmailService emailService, IOptions<EmailSettings> settings)
        {
            _userManager = userManager;
            _emailService = emailService;
            _settings = settings.Value;
        }

        public async Task<bool> Handle(SendConfirmationEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                throw new EntityNotFoundException(ErrorCodes.UserNotFound);
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            string htmlBody;
            var assembly = typeof(SendConfirmationEmailCommandHandler).Assembly;
            var resourceName = "Glowtics.BLL.Resources.Email.OtpTemplate.html";
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new System.IO.StreamReader(stream!))
            {
                htmlBody = await reader.ReadToEndAsync();
            }

            htmlBody = htmlBody.Replace("{{Title}}", "Confirm your email address")
                               .Replace("{{Message}}", "You're almost there. Please use the following one-time password (OTP) to complete your registration and secure your account.")
                               .Replace("{{OtpCode}}", token)
                               .Replace("{{FooterContext}}", "If you didn't create an account with Glowtics, you can safely ignore this email.")
                               .Replace("{{Year}}", DateTime.UtcNow.Year.ToString());

            await _emailService.SendEmailAsync(user.Email!, "Confirm your Glowtics Email", htmlBody);

            return true;
        }
    }
}


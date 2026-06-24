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
        private readonly IEmailNotificationService _notificationService;

        public SendPasswordResetEmailCommandHandler(UserManager<GlowticsUser> userManager, IEmailNotificationService notificationService)
        {
            _userManager = userManager;
            _notificationService = notificationService;
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

            await _notificationService.SendPasswordResetEmailAsync(user.Email!, token);

            return true;
        }
    }
}

using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.Interfaces;
using Glowtics.DAL.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Glowtics.BLL.Commands.Identity
{
    public record ResendConfirmationEmailCommand(string Email) : IRequest<bool>;

    public class ResendConfirmationEmailCommandHandler : IRequestHandler<ResendConfirmationEmailCommand, bool>
    {
        private readonly UserManager<GlowticsUser> _userManager;
        private readonly IEmailNotificationService _notificationService;

        public ResendConfirmationEmailCommandHandler(UserManager<GlowticsUser> userManager, IEmailNotificationService notificationService)
        {
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<bool> Handle(ResendConfirmationEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            
            // Prevent user enumeration attacks by exiting gracefully if not found
            if (user == null)
            {
                return true; 
            }

            var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            
            // If already confirmed, just exit gracefully
            if (isEmailConfirmed)
            {
                return true;
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            await _notificationService.SendRegistrationEmailAsync(user.Email, token);

            return true;
        }
    }
}

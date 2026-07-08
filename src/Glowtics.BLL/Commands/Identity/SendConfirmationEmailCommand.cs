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
        private readonly IEmailNotificationService _notificationService;

        public SendConfirmationEmailCommandHandler(UserManager<GlowticsUser> userManager, IEmailNotificationService notificationService)
        {
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<bool> Handle(SendConfirmationEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString()) 
                ?? throw new UserNotFoundException();

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            await _notificationService.SendRegistrationEmailAsync(user.Email, token);

            return true;
        }
    }
}


using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.Exceptions;
using Glowtics.DAL.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Glowtics.BLL.Constants;

namespace Glowtics.BLL.Commands.Identity
{
    public record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : IRequest<bool>;

    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
    {
        private readonly UserManager<GlowticsUser> _userManager;

        public ChangePasswordCommandHandler(UserManager<GlowticsUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                throw new EntityNotFoundException(ErrorCodes.UserNotFound);
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                throw new BusinessRuleViolationException(ErrorCodes.PasswordChangeFailed, errors);
            }

            return true;
        }
    }
}


using System;
using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.Exceptions;
using Glowtics.DAL.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Glowtics.BLL.Commands.Identity
{
    public record ResetPasswordCommand(string Email, string Otp, string NewPassword) : IRequest<bool>;

    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
    {
        private readonly UserManager<GlowticsUser> _userManager;

        public ResetPasswordCommandHandler(UserManager<GlowticsUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new BusinessRuleViolationException("Invalid or expired OTP.");
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Otp, request.NewPassword);
            if (!result.Succeeded)
            {
                throw new BusinessRuleViolationException("Invalid or expired OTP.");
            }

            return true;
        }
    }
}

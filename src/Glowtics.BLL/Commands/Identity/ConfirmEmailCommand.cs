using System;
using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.Exceptions;
using Glowtics.DAL.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Glowtics.BLL.Constants;

namespace Glowtics.BLL.Commands.Identity
{
    public record ConfirmEmailCommand(string Email, string Otp) : IRequest<bool>;

    public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, bool>
    {
        private readonly UserManager<GlowticsUser> _userManager;

        public ConfirmEmailCommandHandler(UserManager<GlowticsUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new InvalidOtpException();


            var result = await _userManager.ConfirmEmailAsync(user, request.Otp);
            if (!result.Succeeded)
            {
                throw new InvalidOtpException();
            }

            return true;
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glowtics.BLL.Exceptions;
using Glowtics.BLL.Interfaces;
using Glowtics.BLL.Responses;
using Glowtics.DAL.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Glowtics.BLL.Commands.Auth
{
    public record LoginCommand(string Email, string Password) : IRequest<GenerateTokenResponse>;

    public class LoginCommandHandler : IRequestHandler<LoginCommand, GenerateTokenResponse>
    {
        private readonly UserManager<GlowticsUser> _userManager;
        private readonly IJwtService _jwtService;

        public LoginCommandHandler(UserManager<GlowticsUser> userManager, IJwtService jwtService)
        {
            _userManager = userManager;
            _jwtService = jwtService;
        }

        public async Task<GenerateTokenResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new InvalidCredentialsException("Invalid Email or Password.");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                throw new InvalidCredentialsException("Invalid Email or Password");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var token = _jwtService.GenerateToken(user, roles);

            return token;
        }
    }
}

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

namespace Glowtics.BLL.Commands
{
    public record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;

    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
    {
        private readonly UserManager<GlowticsUser> _userManager;
        private readonly IJwtService _jwtService;

        public LoginCommandHandler(UserManager<GlowticsUser> userManager, IJwtService jwtService)
        {
            _userManager = userManager;
            _jwtService = jwtService;
        }

        public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new BadRequestException("Invalid email or password.");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                throw new BadRequestException("Invalid email or password.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var token = _jwtService.GenerateToken(user, roles);

            return token;
        }
    }
}

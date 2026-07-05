using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glowtics.BLL.Exceptions;
using Glowtics.BLL.Interfaces;
using Glowtics.BLL.Responses;
using Glowtics.DAL.Context;
using Glowtics.DAL.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace Glowtics.BLL.Commands.Auth
{
    public record LoginCommand(string Email, string Password) : IRequest<GenerateTokenResponse>;

    public class LoginCommandHandler : IRequestHandler<LoginCommand, GenerateTokenResponse>
    {
        private readonly UserManager<GlowticsUser> _userManager;
        private readonly IJwtService _jwtService;
        private readonly GlowticsDbContext _dbContext;

        public LoginCommandHandler(UserManager<GlowticsUser> userManager, IJwtService jwtService, GlowticsDbContext dbContext)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _dbContext = dbContext;
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
            var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            if (!isEmailConfirmed)
            {
                throw new AccountRestrictedException("Please confirm your email address before logging in.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            // A retailer's dashboard/catalog endpoints resolve their store from the "RetailerId" claim,
            // so it must be baked into the login token (null for non-retailer users -> no claim).
            var retailerId = await _dbContext.Retailers
                .Where(r => r.UserId == user.Id)
                .Select(r => (Guid?)r.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var token = _jwtService.GenerateToken(user, roles, retailerId);

            return token;
        }
    }
}

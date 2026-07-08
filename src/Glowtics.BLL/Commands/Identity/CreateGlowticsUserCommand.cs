using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glowtics.BLL.Exceptions;
using Glowtics.DAL.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Glowtics.BLL.Constants;

namespace Glowtics.BLL.Commands.Identity
{
    public record CreateGlowticsUserCommand(
    
        string Email,
        string Password,
        string Role
        
    ):IRequest<Guid>;

    public class CreateGlowticsUserCommandHandler:IRequestHandler<CreateGlowticsUserCommand, Guid>
    {
        private readonly UserManager<GlowticsUser> _userManager;

        public CreateGlowticsUserCommandHandler(UserManager<GlowticsUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Guid> Handle(CreateGlowticsUserCommand request, CancellationToken cancellationToken)
        {
            var user = new GlowticsUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            var result =await  _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                throw new UserCreationFailedException(string.Join(", ", string.Join(", ", result.Errors.Select(e => e.Description))));
            }

            var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
            if(!roleResult.Succeeded)
            {
                throw new RoleAssignmentFailedException(string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }

            return user.Id;

        }
    }
}


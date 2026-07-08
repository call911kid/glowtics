using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glowtics.BLL.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Glowtics.BLL.Constants;

namespace Glowtics.BLL.Commands.Identity
{
    public record CreateRoleCommand(
        string RoleName

    ):IRequest;

    public class CreateRoleCommandHandler:IRequestHandler<CreateRoleCommand>
    {
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        public CreateRoleCommandHandler(RoleManager<IdentityRole<Guid>> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            var roleExists = await _roleManager.RoleExistsAsync(request.RoleName);
            if (roleExists)
            {
                throw new RoleAlreadyExistsException();
            }
            var result = await _roleManager.CreateAsync(new IdentityRole<Guid>(request.RoleName));
            if (!result.Succeeded)
            {
                throw new RoleCreationFailedException(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            
        }
    }


}


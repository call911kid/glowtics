using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glowtics.BLL.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

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
                throw new BusinessRuleViolationException("Role already exists.");
            }
            var result = await _roleManager.CreateAsync(new IdentityRole<Guid>(request.RoleName));
            if (!result.Succeeded)
            {
                throw new BusinessRuleViolationException("Role creation failed.", result.Errors.Select(e => e.Description));
            }
            
        }
    }


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glowtics.BLL.Responses;
using Glowtics.DAL.Entities;

namespace Glowtics.BLL.Interfaces
{
    public interface IJwtService
    {
        // retailerId is added as a "RetailerId" claim (the retailer dashboard/catalog endpoints read it).
        // Null for users with no retailer (e.g. admins), so no claim is emitted.
        GenerateTokenResponse GenerateToken(GlowticsUser user, IList<string> roles, Guid? retailerId = null);
    }
}

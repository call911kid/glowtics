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
        GenerateTokenResponse GenerateToken(GlowticsUser user, IList<string> roles);
    }
}

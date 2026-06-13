using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glowtics.BLL.Exceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message ="An error occurred during the request.") : base(message) { }
    }
}

using System;
using System.Collections.Generic;

namespace Glowtics.BLL.Exceptions
{
    public class DatabaseProvisioningException : GlowticsException
    {
        public DatabaseProvisioningException(string message) : base(message) { }
        public DatabaseProvisioningException(string message, Exception innerException) : base(message, innerException) { }
        public DatabaseProvisioningException(string message, IEnumerable<string> errors) : base(message, errors) { }
    }
}

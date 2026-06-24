using System;
using System.Collections.Generic;
using Glowtics.BLL.Constants;

namespace Glowtics.BLL.Exceptions
{
    public class DatabaseProvisioningException : GlowticsException
    {
        // Parameterless
        public DatabaseProvisioningException() : base(ErrorCodes.DatabaseProvisioning) { }

        // Message-less
        public DatabaseProvisioningException(string errorCode) : base(errorCode) { }
        public DatabaseProvisioningException(string errorCode, Exception innerException) : base(errorCode, innerException) { }
        public DatabaseProvisioningException(string errorCode, IEnumerable<string> errors) : base(errorCode, errors) { }
        public DatabaseProvisioningException(string errorCode, Exception innerException, IEnumerable<string> errors) : base(errorCode, innerException, errors) { }

        // Explicit message
        public DatabaseProvisioningException(string errorCode, string message) : base(errorCode, message) { }
        public DatabaseProvisioningException(string errorCode, string message, Exception innerException) : base(errorCode, message, innerException) { }
        public DatabaseProvisioningException(string errorCode, string message, IEnumerable<string> errors) : base(errorCode, message, errors) { }
        public DatabaseProvisioningException(string errorCode, string message, Exception innerException, IEnumerable<string> errors) : base(errorCode, message, innerException, errors) { }
    }
}

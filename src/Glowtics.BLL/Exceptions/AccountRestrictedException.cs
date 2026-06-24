using System;
using System.Collections.Generic;
using Glowtics.BLL.Constants;

namespace Glowtics.BLL.Exceptions
{
    public class AccountRestrictedException : GlowticsException
    {
        // Parameterless
        public AccountRestrictedException() : base(ErrorCodes.AccountRestricted) { }

        // Message-less
        public AccountRestrictedException(string errorCode) : base(errorCode) { }
        public AccountRestrictedException(string errorCode, Exception innerException) : base(errorCode, innerException) { }
        public AccountRestrictedException(string errorCode, IEnumerable<string> errors) : base(errorCode, errors) { }
        public AccountRestrictedException(string errorCode, Exception innerException, IEnumerable<string> errors) : base(errorCode, innerException, errors) { }

        // Explicit message
        public AccountRestrictedException(string errorCode, string message) : base(errorCode, message) { }
        public AccountRestrictedException(string errorCode, string message, Exception innerException) : base(errorCode, message, innerException) { }
        public AccountRestrictedException(string errorCode, string message, IEnumerable<string> errors) : base(errorCode, message, errors) { }
        public AccountRestrictedException(string errorCode, string message, Exception innerException, IEnumerable<string> errors) : base(errorCode, message, innerException, errors) { }
    }
}

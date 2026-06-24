using System;
using System.Collections.Generic;
using Glowtics.BLL.Constants;

namespace Glowtics.BLL.Exceptions
{
    public class InvalidCredentialsException : GlowticsException
    {
        // Parameterless
        public InvalidCredentialsException() : base(ErrorCodes.InvalidCredentials) { }

        // Message-less
        public InvalidCredentialsException(string errorCode) : base(errorCode) { }
        public InvalidCredentialsException(string errorCode, Exception innerException) : base(errorCode, innerException) { }
        public InvalidCredentialsException(string errorCode, IEnumerable<string> errors) : base(errorCode, errors) { }
        public InvalidCredentialsException(string errorCode, Exception innerException, IEnumerable<string> errors) : base(errorCode, innerException, errors) { }

        // Explicit message
        public InvalidCredentialsException(string errorCode, string message) : base(errorCode, message) { }
        public InvalidCredentialsException(string errorCode, string message, Exception innerException) : base(errorCode, message, innerException) { }
        public InvalidCredentialsException(string errorCode, string message, IEnumerable<string> errors) : base(errorCode, message, errors) { }
        public InvalidCredentialsException(string errorCode, string message, Exception innerException, IEnumerable<string> errors) : base(errorCode, message, innerException, errors) { }
    }
}

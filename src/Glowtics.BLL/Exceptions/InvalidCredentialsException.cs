using System;
using System.Collections.Generic;

namespace Glowtics.BLL.Exceptions
{
    public class InvalidCredentialsException : GlowticsException
    {
        public InvalidCredentialsException() : base("Invalid credentials.") { }
        public InvalidCredentialsException(string message) : base(message) { }
        public InvalidCredentialsException(string message, Exception innerException) : base(message, innerException) { }
        public InvalidCredentialsException(string message, IEnumerable<string> errors) : base(message, errors) { }
    }
}

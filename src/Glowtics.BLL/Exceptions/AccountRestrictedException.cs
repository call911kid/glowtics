using System;
using System.Collections.Generic;

namespace Glowtics.BLL.Exceptions
{
    public class AccountRestrictedException : GlowticsException
    {
        public AccountRestrictedException() : base("Account is restricted.") { }
        public AccountRestrictedException(string message) : base(message) { }
        public AccountRestrictedException(string message, Exception innerException) : base(message, innerException) { }
        public AccountRestrictedException(string message, IEnumerable<string> errors) : base(message, errors) { }
    }
}

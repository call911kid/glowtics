using System;

namespace Glowtics.BLL.Exceptions
{
    public class PasswordChangeFailedException : BusinessRuleViolationException
    {
        public PasswordChangeFailedException() : base("Password change failed.") { }
        public PasswordChangeFailedException(string message) : base(message) { }
        public PasswordChangeFailedException(string message, Exception innerException) : base(message, innerException) { }
    }
}

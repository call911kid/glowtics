using System;

namespace Glowtics.BLL.Exceptions
{
    public class UserCreationFailedException : BusinessRuleViolationException
    {
        public UserCreationFailedException() : base("User creation failed.") { }
        public UserCreationFailedException(string message) : base(message) { }
        public UserCreationFailedException(string message, Exception innerException) : base(message, innerException) { }
    }
}

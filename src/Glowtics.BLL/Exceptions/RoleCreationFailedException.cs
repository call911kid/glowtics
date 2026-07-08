using System;

namespace Glowtics.BLL.Exceptions
{
    public class RoleCreationFailedException : BusinessRuleViolationException
    {
        public RoleCreationFailedException() : base("Role creation failed.") { }
        public RoleCreationFailedException(string message) : base(message) { }
        public RoleCreationFailedException(string message, Exception innerException) : base(message, innerException) { }
    }
}

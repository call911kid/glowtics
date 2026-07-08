using System;

namespace Glowtics.BLL.Exceptions
{
    public class RoleAssignmentFailedException : BusinessRuleViolationException
    {
        public RoleAssignmentFailedException() : base("Role assignment failed.") { }
        public RoleAssignmentFailedException(string message) : base(message) { }
        public RoleAssignmentFailedException(string message, Exception innerException) : base(message, innerException) { }
    }
}

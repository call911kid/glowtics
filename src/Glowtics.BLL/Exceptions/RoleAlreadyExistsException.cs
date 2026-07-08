using System;

namespace Glowtics.BLL.Exceptions
{
    public class RoleAlreadyExistsException : BusinessRuleViolationException
    {
        public RoleAlreadyExistsException() : base("Role already exists.") { }
        public RoleAlreadyExistsException(string message) : base(message) { }
        public RoleAlreadyExistsException(string message, Exception innerException) : base(message, innerException) { }
    }
}

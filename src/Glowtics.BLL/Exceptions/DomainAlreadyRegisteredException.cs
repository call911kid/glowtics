using System;

namespace Glowtics.BLL.Exceptions
{
    public class DomainAlreadyRegisteredException : BusinessRuleViolationException
    {
        public DomainAlreadyRegisteredException() : base("Domain already registered.") { }
        public DomainAlreadyRegisteredException(string message) : base(message) { }
        public DomainAlreadyRegisteredException(string message, Exception innerException) : base(message, innerException) { }
    }
}

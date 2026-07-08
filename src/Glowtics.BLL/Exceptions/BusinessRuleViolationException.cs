using System;
using System.Collections.Generic;

namespace Glowtics.BLL.Exceptions
{
    public class BusinessRuleViolationException : GlowticsException
    {
        public BusinessRuleViolationException() : base("The requested operation is not permitted under the current business rules.") { }
        public BusinessRuleViolationException(string message) : base(message) { }
        public BusinessRuleViolationException(string message, Exception innerException) : base(message, innerException) { }
        public BusinessRuleViolationException(string message, IEnumerable<string> errors) : base(message, errors) { }
    }
}

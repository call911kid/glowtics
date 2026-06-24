using System;
using System.Collections.Generic;
using Glowtics.BLL.Constants;

namespace Glowtics.BLL.Exceptions
{
    public class BusinessRuleViolationException : GlowticsException
    {
        // Parameterless
        public BusinessRuleViolationException() : base(ErrorCodes.BusinessRuleViolation) { }

        // Message-less
        public BusinessRuleViolationException(string errorCode) : base(errorCode) { }
        public BusinessRuleViolationException(string errorCode, Exception innerException) : base(errorCode, innerException) { }
        public BusinessRuleViolationException(string errorCode, IEnumerable<string> errors) : base(errorCode, errors) { }
        public BusinessRuleViolationException(string errorCode, Exception innerException, IEnumerable<string> errors) : base(errorCode, innerException, errors) { }

        // Explicit message
        public BusinessRuleViolationException(string errorCode, string message) : base(errorCode, message) { }
        public BusinessRuleViolationException(string errorCode, string message, Exception innerException) : base(errorCode, message, innerException) { }
        public BusinessRuleViolationException(string errorCode, string message, IEnumerable<string> errors) : base(errorCode, message, errors) { }
        public BusinessRuleViolationException(string errorCode, string message, Exception innerException, IEnumerable<string> errors) : base(errorCode, message, innerException, errors) { }
    }
}

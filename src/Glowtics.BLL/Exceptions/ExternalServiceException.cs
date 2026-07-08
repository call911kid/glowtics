using System;
using System.Collections.Generic;

namespace Glowtics.BLL.Exceptions
{
    public class ExternalServiceException : GlowticsException
    {
        public ExternalServiceException() : base("An error occurred while communicating with an external service.") { }
        public ExternalServiceException(string message) : base(message) { }
        public ExternalServiceException(string message, Exception innerException) : base(message, innerException) { }
        public ExternalServiceException(string message, IEnumerable<string> errors) : base(message, errors) { }
    }
}

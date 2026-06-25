using System;
using System.Collections.Generic;

namespace Glowtics.BLL.Exceptions
{
    public class ExternalServiceException : GlowticsException
    {
        public ExternalServiceException(string errorCode) 
            : base(errorCode, "An error occurred while communicating with an external service.")
        {
        }

        public ExternalServiceException(string errorCode, string message) 
            : base(errorCode, message)
        {
        }

        public ExternalServiceException(string errorCode, string message, IEnumerable<string> errors) 
            : base(errorCode, message, errors)
        {
        }

        public ExternalServiceException(string errorCode, string message, Exception innerException) 
            : base(errorCode, message, innerException)
        {
        }
    }
}

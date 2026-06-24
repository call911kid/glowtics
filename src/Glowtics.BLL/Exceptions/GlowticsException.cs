using System;
using System.Collections.Generic;
using System.Linq;

namespace Glowtics.BLL.Exceptions
{
    public abstract class GlowticsException : Exception
    {
        public string ErrorCode { get; }
        public IReadOnlyCollection<string> Errors { get; } = Array.Empty<string>();

        // Message-less constructors
        protected GlowticsException(string errorCode) 
            : base("An error occurred.") 
        {
            ErrorCode = errorCode;
        }

        protected GlowticsException(string errorCode, Exception innerException) 
            : base("An error occurred.", innerException) 
        {
            ErrorCode = errorCode;
        }

        protected GlowticsException(string errorCode, IEnumerable<string> errors)
            : base("An error occurred.")
        {
            ErrorCode = errorCode;
            if (errors != null) Errors = errors.ToList().AsReadOnly();
        }

        protected GlowticsException(string errorCode, Exception innerException, IEnumerable<string> errors)
            : base("An error occurred.", innerException)
        {
            ErrorCode = errorCode;
            if (errors != null) Errors = errors.ToList().AsReadOnly();
        }

        // Explicit message constructors
        protected GlowticsException(string errorCode, string message) 
            : base(message) 
        {
            ErrorCode = errorCode;
        }

        protected GlowticsException(string errorCode, string message, Exception innerException)
            : base(message, innerException) 
        {
            ErrorCode = errorCode;
        }

        protected GlowticsException(string errorCode, string message, IEnumerable<string> errors)
            : base(message)
        {
            ErrorCode = errorCode;
            if (errors != null) Errors = errors.ToList().AsReadOnly();
        }

        protected GlowticsException(string errorCode, string message, Exception innerException, IEnumerable<string> errors)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            if (errors != null) Errors = errors.ToList().AsReadOnly();
        }
    }
}

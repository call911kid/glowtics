using System;
using System.Collections.Generic;
using System.Linq;

namespace Glowtics.BLL.Exceptions
{
    public abstract class GlowticsException : Exception
    {
        public IReadOnlyCollection<string> Errors { get; } = Array.Empty<string>();

        // Message-less constructors
        protected GlowticsException() 
            : base("An error occurred.") 
        {
        }

        protected GlowticsException(Exception innerException) 
            : base("An error occurred.", innerException) 
        {
        }

        protected GlowticsException(IEnumerable<string> errors)
            : base("An error occurred.")
        {
            if (errors != null) Errors = errors.ToList().AsReadOnly();
        }

        protected GlowticsException(Exception innerException, IEnumerable<string> errors)
            : base("An error occurred.", innerException)
        {
            if (errors != null) Errors = errors.ToList().AsReadOnly();
        }

        // Explicit message constructors
        protected GlowticsException(string message) 
            : base(message) 
        {
        }

        protected GlowticsException(string message, Exception innerException)
            : base(message, innerException) 
        {
        }

        protected GlowticsException(string message, IEnumerable<string> errors)
            : base(message)
        {
            if (errors != null) Errors = errors.ToList().AsReadOnly();
        }

        protected GlowticsException(string message, Exception innerException, IEnumerable<string> errors)
            : base(message, innerException)
        {
            if (errors != null) Errors = errors.ToList().AsReadOnly();
        }
    }
}

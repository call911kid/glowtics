using System;
using System.Collections.Generic;

namespace Glowtics.BLL.Exceptions
{
    public abstract class GlowticsException : Exception
    {
        public IReadOnlyCollection<string> Errors { get; } = Array.Empty<string>();

        protected GlowticsException() 
            : base("An error occurred.") { }

        protected GlowticsException(string message) 
            : base(message) { }

        protected GlowticsException(string message, Exception innerException)
            : base(message, innerException) { }

        protected GlowticsException(string message, IEnumerable<string> errors)
            : base(message)
        {
            if (errors != null) Errors = new List<string>(errors).AsReadOnly();
        }

        protected GlowticsException(string message, Exception innerException, IEnumerable<string> errors)
            : base(message, innerException)
        {
            if (errors != null) Errors = new List<string>(errors).AsReadOnly();
        }
    }
}

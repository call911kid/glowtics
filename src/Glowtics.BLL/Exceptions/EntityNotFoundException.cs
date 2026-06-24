using System;
using System.Collections.Generic;

namespace Glowtics.BLL.Exceptions
{
    public class EntityNotFoundException : GlowticsException
    {
        // No parameterless constructor (An entity not found exception should always be specific)

        // Message-less
        public EntityNotFoundException(string errorCode) : base(errorCode) { }
        public EntityNotFoundException(string errorCode, Exception innerException) : base(errorCode, innerException) { }
        public EntityNotFoundException(string errorCode, IEnumerable<string> errors) : base(errorCode, errors) { }
        public EntityNotFoundException(string errorCode, Exception innerException, IEnumerable<string> errors) : base(errorCode, innerException, errors) { }

        // Explicit message
        public EntityNotFoundException(string errorCode, string message) : base(errorCode, message) { }
        public EntityNotFoundException(string errorCode, string message, Exception innerException) : base(errorCode, message, innerException) { }
        public EntityNotFoundException(string errorCode, string message, IEnumerable<string> errors) : base(errorCode, message, errors) { }
        public EntityNotFoundException(string errorCode, string message, Exception innerException, IEnumerable<string> errors) : base(errorCode, message, innerException, errors) { }
    }
}

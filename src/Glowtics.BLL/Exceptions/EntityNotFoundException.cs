using System;
using System.Collections.Generic;

namespace Glowtics.BLL.Exceptions
{
    public class EntityNotFoundException : GlowticsException
    {
        public EntityNotFoundException() : base("Entity not found.") { }
        public EntityNotFoundException(string message) : base(message) { }
        public EntityNotFoundException(string message, Exception innerException) : base(message, innerException) { }
        public EntityNotFoundException(string message, IEnumerable<string> errors) : base(message, errors) { }
    }
}

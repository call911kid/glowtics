using System;
using System.Collections.Generic;

namespace Glowtics.BLL.Exceptions
{
    public class EntityNotFoundException : GlowticsException
    {
        public EntityNotFoundException(string entityName, object key) : base($"Entity '{entityName}' ({key}) was not found.") { }
        public EntityNotFoundException(string message) : base(message) { }
        public EntityNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}

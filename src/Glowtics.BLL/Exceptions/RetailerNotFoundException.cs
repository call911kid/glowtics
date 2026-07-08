using System;

namespace Glowtics.BLL.Exceptions
{
    public class RetailerNotFoundException : EntityNotFoundException
    {
        public RetailerNotFoundException() : base("Retailer not found.") { }
        public RetailerNotFoundException(string message) : base(message) { }
        public RetailerNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}

using System;

namespace Glowtics.BLL.Exceptions
{
    public class ProductNotFoundException : EntityNotFoundException
    {
        public ProductNotFoundException() : base("Product not found.") { }
        public ProductNotFoundException(string message) : base(message) { }
        public ProductNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}

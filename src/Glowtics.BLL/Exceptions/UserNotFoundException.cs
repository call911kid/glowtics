using System;

namespace Glowtics.BLL.Exceptions
{
    public class UserNotFoundException : EntityNotFoundException
    {
        public UserNotFoundException() : base("User not found.") { }
        public UserNotFoundException(string message) : base(message) { }
        public UserNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}

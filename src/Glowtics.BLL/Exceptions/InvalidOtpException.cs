using System;

namespace Glowtics.BLL.Exceptions
{
    public class InvalidOtpException : BusinessRuleViolationException
    {
        public InvalidOtpException() : base("Invalid or expired OTP.") { }
        public InvalidOtpException(string message) : base(message) { }
        public InvalidOtpException(string message, Exception innerException) : base(message, innerException) { }
    }
}

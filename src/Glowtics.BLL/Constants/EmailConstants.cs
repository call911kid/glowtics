namespace Glowtics.BLL.Constants
{
    public static class EmailConstants
    {
        public static class Registration
        {
            public const string Subject = "Confirm your Glowtics Email";
            public const string Title = "Confirm your email address";
            public const string Message = "You're almost there. Please use the following one-time password (OTP) to complete your registration and secure your account.";
            public const string Footer = "If you didn't create an account with Glowtics, you can safely ignore this email.";
        }

        public static class PasswordReset
        {
            public const string Subject = "Reset your Glowtics Password";
            public const string Title = "Reset Your Password";
            public const string Message = "We received a request to reset your password. Please use the following one-time password (OTP) to choose a new password.";
            public const string Footer = "If you didn't request a password reset, you can safely ignore this email.";
        }
    }
}

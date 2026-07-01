namespace Glowtics.BLL.Constants
{
    internal static class ErrorCodes
    {
        public const string InvalidCredentials = "ERR_INVALID_CREDENTIALS";
        public const string AccountRestricted = "ERR_ACCOUNT_RESTRICTED";
        public const string DatabaseProvisioning = "ERR_DATABASE_PROVISIONING";
        public const string InternalServerError = "ERR_INTERNAL_SERVER_ERROR";
        public const string BusinessRuleViolation = "ERR_BUSINESS_RULE_VIOLATION";

        // Specific Entity Not Found Codes
        public const string RetailerNotFound = "ERR_RETAILER_NOT_FOUND";
        public const string ProductNotFound = "ERR_PRODUCT_NOT_FOUND";
        public const string UserNotFound = "ERR_USER_NOT_FOUND";
        

        // Specific Business Rule Violations
        public const string DomainAlreadyRegistered = "ERR_DOMAIN_ALREADY_REGISTERED";
        public const string InvalidOrExpiredOtp = "ERR_INVALID_OR_EXPIRED_OTP";
        public const string PasswordChangeFailed = "ERR_PASSWORD_CHANGE_FAILED";
        public const string UserCreationFailed = "ERR_USER_CREATION_FAILED";
        public const string RoleAlreadyExists = "ERR_ROLE_ALREADY_EXISTS";
        public const string ValidationFailed = "ERR_VALIDATION_FAILED";
        public const string DiagnosisFailed = "ERR_DIAGNOSIS_FAILED";
        public const string RoleCreationFailed = "ERR_ROLE_CREATION_FAILED";
        public const string RoleAssignmentFailed = "ERR_ROLE_ASSIGNMENT_FAILED";

        // Infrastructure & Integration Errors
        public const string EmbeddingGenerationFailed = "ERR_EMBEDDING_GENERATION_FAILED";
    }
}

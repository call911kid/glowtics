namespace Glowtics.BLL.DTOs
{
    /// <summary>Result of the standalone validation flow.</summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string? Reason { get; set; }
    }
}

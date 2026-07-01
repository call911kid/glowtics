namespace Glowtics.BLL.Settings
{
    public class LangfuseSettings
    {
        public const string SectionName = "Langfuse";

        public string Host { get; set; } = "https://us.cloud.langfuse.com";
        public string PublicKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;

        public bool Enabled => !string.IsNullOrWhiteSpace(PublicKey) && !string.IsNullOrWhiteSpace(SecretKey);
    }
}

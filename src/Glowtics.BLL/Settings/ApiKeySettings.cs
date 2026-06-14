namespace Glowtics.BLL.Settings
{
    public class ApiKeySettings
    {
        public const string SectionName = "ApiKeySettings";

        public string Prefix { get; set; } = "glx_";
        public int KeyLengthBytes { get; set; } = 32;
    }
}

namespace ProjectAssetAPI.Options
{
    public sealed class SecurityOptions
    {
        public const string SectionName = "Security";

        public string ApiKey { get; set; } = string.Empty;

        public int HeartbeatTimeoutSeconds { get; set; } = 120;
    }
}

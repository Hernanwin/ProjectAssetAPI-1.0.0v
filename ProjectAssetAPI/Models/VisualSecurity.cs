namespace ProjectAssetAPI.Models
{
    public class VisualSecurity
    {
        public int Id { get; set; }

        public int AssetId { get; set; }

        public required string EventType { get; set; }

        public required string Severity { get; set; }

        public required string Description { get; set; }

    }
}

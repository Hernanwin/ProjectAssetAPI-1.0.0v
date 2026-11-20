namespace ProjectAssetAgent.Models
{
    public class VisualLab
    {
        public int Id { get; set; }

        public required string Hostname { get; set; }

        public required string IPAddress { get; set; }

        public required string OperatingSystem { get; set; }

        public required string Status { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace ProjectAssetAPI.Models
{
    public class VisualLab
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public required string Hostname { get; set; }

        [Required]
        [MaxLength(64)]
        public required string IPAddress { get; set; }

        [Required]
        [MaxLength(255)]
        public required string OperatingSystem { get; set; }

        [Required]
        [MaxLength(32)]
        public required string Status { get; set; }

        public DateTime LastSeenUtc { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<VisualSecurity> SecurityEvents { get; set; } = new List<VisualSecurity>();
    }
}

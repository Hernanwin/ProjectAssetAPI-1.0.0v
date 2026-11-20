using System.ComponentModel.DataAnnotations;

namespace ProjectAssetAPI.Models
{
    public class VisualSecurity
    {
        public int Id { get; set; }

        public int AssetId { get; set; }

        [Required]
        [MaxLength(100)]
        public required string EventType { get; set; }

        [Required]
        [MaxLength(32)]
        public required string Severity { get; set; }

        [Required]
        [MaxLength(1000)]
        public required string Description { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public VisualLab? Asset { get; set; }
    }
}

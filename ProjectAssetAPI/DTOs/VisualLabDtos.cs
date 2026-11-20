using System.ComponentModel.DataAnnotations;
using ProjectAssetAPI.Models;

namespace ProjectAssetAPI.DTOs
{
    public sealed class RegisterAssetRequest
    {
        [Required]
        [StringLength(255, MinimumLength = 1)]
        public required string Hostname { get; set; }

        [Required]
        [StringLength(64, MinimumLength = 1)]
        public required string IPAddress { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 1)]
        public required string OperatingSystem { get; set; }

        [StringLength(32)]
        [RegularExpression("^(Online|Offline|Maintenance)$", ErrorMessage = "Status debe ser Online, Offline o Maintenance.")]
        public string? Status { get; set; }
    }

    public sealed class UpdateAssetRequest
    {
        [Required]
        [StringLength(255, MinimumLength = 1)]
        public required string Hostname { get; set; }

        [Required]
        [StringLength(64, MinimumLength = 1)]
        public required string IPAddress { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 1)]
        public required string OperatingSystem { get; set; }

        [Required]
        [StringLength(32)]
        [RegularExpression("^(Online|Offline|Maintenance)$", ErrorMessage = "Status debe ser Online, Offline o Maintenance.")]
        public required string Status { get; set; }
    }

    public sealed record VisualLabResponse(
        int Id,
        string Hostname,
        string IPAddress,
        string OperatingSystem,
        string Status,
        DateTime LastSeenUtc,
        DateTime CreatedAtUtc,
        int SecurityEventCount)
    {
        public static VisualLabResponse FromEntity(VisualLab entity, TimeSpan heartbeatTimeout, int securityEventCount)
        {
            var effectiveStatus = entity.Status;
            if (string.Equals(entity.Status, "Online", StringComparison.OrdinalIgnoreCase) &&
                DateTime.UtcNow - entity.LastSeenUtc > heartbeatTimeout)
            {
                effectiveStatus = "Offline";
            }

            return new VisualLabResponse(
                entity.Id,
                entity.Hostname,
                entity.IPAddress,
                entity.OperatingSystem,
                effectiveStatus,
                entity.LastSeenUtc,
                entity.CreatedAtUtc,
                securityEventCount);
        }
    }
}

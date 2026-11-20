using System.ComponentModel.DataAnnotations;
using ProjectAssetAPI.Models;

namespace ProjectAssetAPI.DTOs
{
    public sealed class CreateSecurityEventRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "AssetId debe ser un entero positivo.")]
        public int AssetId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public required string EventType { get; set; }

        [Required]
        [StringLength(32)]
        [RegularExpression("^(Low|Medium|High|Critical)$", ErrorMessage = "Severity debe ser Low, Medium, High o Critical.")]
        public required string Severity { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public required string Description { get; set; }
    }

    public sealed record VisualSecurityResponse(
        int Id,
        int AssetId,
        string? Hostname,
        string EventType,
        string Severity,
        string Description,
        DateTime CreatedAtUtc)
    {
        public static VisualSecurityResponse FromEntity(VisualSecurity entity) =>
            new(
                entity.Id,
                entity.AssetId,
                entity.Asset?.Hostname,
                entity.EventType,
                entity.Severity,
                entity.Description,
                entity.CreatedAtUtc);
    }
}

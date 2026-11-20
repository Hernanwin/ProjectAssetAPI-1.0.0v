using Microsoft.EntityFrameworkCore;
using ProjectAssetAPI.Models;

namespace ProjectAssetAPI.Context
{
    public class VisualDBContext : DbContext
    {
        public VisualDBContext(DbContextOptions<VisualDBContext> options) : base(options)
        {
        }

        public DbSet<VisualLab> VisualLabs { get; set; }
        public DbSet<VisualSecurity> VisualSecurities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<VisualLab>(entity =>
            {
                entity.ToTable("VisualLabs");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Hostname).IsRequired().HasMaxLength(255);
                entity.Property(e => e.IPAddress).IsRequired().HasMaxLength(64);
                entity.Property(e => e.OperatingSystem).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(32);
                entity.Property(e => e.LastSeenUtc).IsRequired();
                entity.Property(e => e.CreatedAtUtc).IsRequired();

                entity.HasIndex(e => e.Hostname).IsUnique();
            });

            modelBuilder.Entity<VisualSecurity>(entity =>
            {
                entity.ToTable("VisualSecurities");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Severity).IsRequired().HasMaxLength(32);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.CreatedAtUtc).IsRequired();

                entity.HasIndex(e => e.AssetId);
                entity.HasIndex(e => e.CreatedAtUtc);

                entity.HasOne(e => e.Asset)
                    .WithMany(a => a.SecurityEvents)
                    .HasForeignKey(e => e.AssetId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

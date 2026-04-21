using Microsoft.EntityFrameworkCore;

namespace ProjectAssetAPI.Context
{
    public class VisualDBContext:DbContext
    {
        public VisualDBContext(DbContextOptions<VisualDBContext> options) : base(options)
        {
        }

        public DbSet<Models.VisualLab> VisualLabs { get; set; }
        public DbSet<Models.VisualSecurity> VisualSecuritys { get; set; }

    }
}

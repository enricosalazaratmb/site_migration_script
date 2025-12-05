using Microsoft.EntityFrameworkCore;
using SiteLocationMigration.Models.Modern;

namespace SiteLocationMigration.Db
{
    public class ModernAtmbContext : DbContext
    {
        public ModernAtmbContext(DbContextOptions<ModernAtmbContext> options)
            : base(options)
        {
        }

        public DbSet<ENT_Location> Locations { get; set; }
        public DbSet<ENT_Geography> Geographies { get; set; }
        public DbSet<ENT_GeographyLocation> GeographyLocations { get; set; }
    }
}

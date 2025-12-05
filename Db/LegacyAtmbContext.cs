using Microsoft.EntityFrameworkCore;
using SiteLocationMigration.Models.Legacy;

namespace SiteLocationMigration.Db
{
    public class LegacyAtmbContext : DbContext
    {
        public LegacyAtmbContext(DbContextOptions<LegacyAtmbContext> options)
            : base(options)
        {
        }

        public DbSet<ENT_Site> Sites { get; set; }
        public DbSet<Web_SiteLocation> SiteLocations { get; set; }
        public DbSet<WEB_SiteLocationItem> SiteLocationItems { get; set; }

        
    }
}

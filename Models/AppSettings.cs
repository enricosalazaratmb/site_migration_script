using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteLocationMigration.Models
{
    public class AppSettings
    {
        public string LegacyAtmbString { get; set; } = string.Empty;
        public string ModernAtmbString { get; set; } = string.Empty;
    }
}

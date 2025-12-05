using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteLocationMigration.Models.Legacy
{
    [Table("Web_SiteLocation")]
    public class Web_SiteLocation
    {
        // Primary Key
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SitLocId { get; set; }

        // FIX: Changed to nullable int?
        public int? ParentSitLocId { get; set; }

        [Required][MaxLength(31)] public string WebKey { get; set; } = string.Empty;
        [Required][MaxLength(63)] public string DefaultText { get; set; } = string.Empty;

        // FIX: Changed to nullable double?
        public double? MapLat { get; set; }
        public double? MapLong { get; set; }

        // FIX: Changed to nullable int?
        public int? GoogleZoom { get; set; }

        [Required][StringLength(2)] public string CountryIso2 { get; set; } = string.Empty;

        [Required][MaxLength(50)][Column(TypeName = "varchar(50)")] public string GeoRegion { get; set; } = string.Empty;

        // FIX: Changed to nullable Guid?
        public Guid? LocationId { get; set; }
        public Guid? ParentLocationId { get; set; }
    }
}

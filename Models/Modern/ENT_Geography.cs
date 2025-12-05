using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteLocationMigration.Models.Modern
{
    [Table("ENT_Geography")]
    public class ENT_Geography
    {
        // Primary Key
        [Key]
        public Guid GeographyId { get; set; }

        // FIX: Changed to nullable Guid?
        public Guid? ParentId { get; set; }

        // FIX: Changed nullable string?
        [MaxLength(50)] public string? ExternalKey { get; set; }
        [MaxLength(80)] public string? GeoCity { get; set; }

        [Required][MaxLength(3)] public string GeoCountryISO { get; set; } = string.Empty;
        [Required][MaxLength(50)] public string GeoCountryName { get; set; } = string.Empty;

        // FIX: Changed to nullable double?
        public double? GeoLatitude { get; set; }
        public double? GeoLongitude { get; set; }

        // FIX: Changed nullable string?
        [MaxLength(10)] public string? GeoRegionISO { get; set; }
        [MaxLength(50)] public string? GeoRegionName { get; set; }

        // FIX: Changed to nullable int?
        public int? MapZoom { get; set; }

        [Required][MaxLength(100)] public string Name { get; set; } = string.Empty;

        // FIX: Changed nullable string?
        [MaxLength(50)] public string? SourceKey { get; set; }

        [Required][MaxLength(20)] public string Status { get; set; } = string.Empty;
        [Required][MaxLength(20)] public string Type { get; set; } = string.Empty;

        [Required] public DateTime CreatedAt { get; set; }
        [Required] public DateTime UpdatedAt { get; set; }
    }
}

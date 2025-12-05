using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SiteLocationMigration.Models.Modern
{
    [Table("ENT_GeographyLocation")]
    public class ENT_GeographyLocation
    {
        // Primary Key
        [Key]
        public Guid GeographyLocationId { get; set; }

        // Foreign Keys (NOT NULL)
        public Guid GeographyId { get; set; }
        public Guid LocationId { get; set; }

        // FIX: Changed to nullable DateTime?
        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }

        [Required] public int SortOrder { get; set; }

        [Required][MaxLength(20)] public string Status { get; set; } = string.Empty;
        [Required][MaxLength(20)] public string Type { get; set; } = string.Empty;

        [Required] public DateTime CreatedAt { get; set; }
        [Required] public DateTime UpdatedAt { get; set; }
    }
}

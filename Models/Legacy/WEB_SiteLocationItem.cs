using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteLocationMigration.Models.Legacy
{
    [Table("WEB_SiteLocationItem")]
    public class WEB_SiteLocationItem
    {
        // Primary Key
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SitLocItmId { get; set; }

        // Foreign Keys (NOT NULL)
        [Required] public int SitLocId { get; set; }
        [Required] public int SitId { get; set; }

        [Required][MaxLength(50)] public string WebKey { get; set; } = string.Empty;
        [Required][MaxLength(127)] public string LocationTitle { get; set; } = string.Empty;

        [Required] public int Rank { get; set; }

        // FIX: Changed to non-nullable bool
        [Required] public bool PremierLocation { get; set; }

        [Required] public DateTime CreationDate { get; set; }

        // FIX: Changed to non-nullable byte
        [Required] public byte Status { get; set; }

        // FIX: Changed to nullable DateTime?
        public DateTime? RemovalDate { get; set; }
    }
}

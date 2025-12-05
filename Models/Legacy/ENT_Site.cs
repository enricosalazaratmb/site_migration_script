using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteLocationMigration.Models.Legacy
{
    [Table("ENT_Site")] // FIX: Added Table attribute based on previous error context
    public class ENT_Site
    {
        // Primary Key
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SitId { get; set; }

        [Required][StringLength(10)] public string Guid { get; set; } = string.Empty;
        [Required][MaxLength(63)] public string SitTx { get; set; } = string.Empty;

        // FIX: Changed to nullable int?
        public int? SitAccTypId { get; set; }

        [Required][MaxLength(160)] public string Company { get; set; } = string.Empty;
        [Required][MaxLength(127)] public string AddressLine1 { get; set; } = string.Empty;
        [Required][MaxLength(127)] public string AddressLine2 { get; set; } = string.Empty;
        [Required][MaxLength(50)] public string ZipCode { get; set; } = string.Empty;
        [Required][MaxLength(63)] public string City { get; set; } = string.Empty;
        [Required][StringLength(5)] public string StateIso { get; set; } = string.Empty;
        [Required][MaxLength(50)] public string StateShortText { get; set; } = string.Empty;
        [Required][StringLength(3)] public string CountryIso3 { get; set; } = string.Empty;
        [Required][MaxLength(127)] public string CountryText { get; set; } = string.Empty;

        [Required][MaxLength(50)][Column(TypeName = "varchar(50)")] public string Phone { get; set; } = string.Empty;
        [Required][MaxLength(127)] public string Email { get; set; } = string.Empty;
        [Required][MaxLength(15)][Column(TypeName = "varchar(15)")] public string Culture { get; set; } = string.Empty;
        [Required][MaxLength(60)][Column(TypeName = "varchar(60)")] public string TimeZoneInfo { get; set; } = string.Empty;

        // FIX: Changed to nullable int?
        public int? TmzId { get; set; }

        [Required][StringLength(3)] public string DefaultCurrency { get; set; } = string.Empty;
        [Required][MaxLength(63)] public string DomainFolder { get; set; } = string.Empty;

        // FIX: Changed to nullable DateTime?
        public DateTime? CreationDate { get; set; }

        // FIX: Changed to nullable bool?
        public bool? LogoAvailable { get; set; }

        // FIX: Changed to nullable int?
        public int? LogoVersion { get; set; }

        // FIX: Changed to nullable int?
        public int? ResEntId { get; set; }

        [Required][MaxLength(2000)] public string RenterAddressPattern { get; set; } = string.Empty;
        [Required][MaxLength(512)] public string RenterSupportUrl { get; set; } = string.Empty;

        // FIX: Changed to nullable double?
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // FIX: Changed to nullable byte?
        public byte? Suspension { get; set; }

        // FIX: Changed to nullable bool?
        public bool? Active { get; set; }

        // FIX: Changed to nullable DateTime?
        public DateTime? DeletionDateUtc { get; set; }
        public DateTime? AnonymizationDateUtc { get; set; }

        // FIX: Changed to non-nullable bool
        [Required] public bool ShowBanner { get; set; }

        // Nullable string
        [MaxLength(60)][Column(TypeName = "varchar(60)")] public string? InitialTimezoneInfo { get; set; }

        // FIX: Changed to nullable DateTime?
        public DateTime? EffectiveSiteClosureDate { get; set; }

        // FIX: Changed to nullable int?
        public int? ClosureState { get; set; }
    }
}

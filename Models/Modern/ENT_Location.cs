using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteLocationMigration.Models.Modern
{
    [Table("ENT_Location")]
    public class ENT_Location
    {
        // Primary Key: LocationId (uniqueidentifier) - NULL NO
        // We use Guid for uniqueidentifier, as it's the standard C# mapping.
        [Key]
        public Guid LocationId { get; set; }

        // Foreign Key: OrganizationId (uniqueidentifier) - NULL NO
        public Guid OrganizationId { get; set; }

        // Foreign Key: ServiceProviderId (uniqueidentifier) - NULL NO
        public Guid ServiceProviderId { get; set; }

        // Column: Category (nvarchar(20)) - NULL NO
        [Required]
        [MaxLength(20)]
        public string Category { get; set; } = string.Empty;

        // Column: Class (nvarchar(20)) - NULL NO
        [Required]
        [MaxLength(20)]
        public string Class { get; set; } = string.Empty;

        // Column: ContactEmailAddress (nvarchar(150)) - NULL NO
        [Required]
        [MaxLength(150)]
        public string ContactEmailAddress { get; set; } = string.Empty;

        // Column: ContactPhone (nvarchar(50)) - NULL NO
        [Required]
        [MaxLength(50)]
        public string ContactPhone { get; set; } = string.Empty;

        // Column: DateBegin (datetime2) - NULL YES
        public DateTime? DateBegin { get; set; }

        // Column: DateClose (datetime2) - NULL YES
        public DateTime? DateClose { get; set; }

        // Column: DateEnd (datetime2) - NULL YES
        public DateTime? DateEnd { get; set; }

        // Column: DefaultCulture (nvarchar(5)) - NULL YES
        [MaxLength(5)]
        public string? DefaultCulture { get; set; }

        // Column: DefaultCurrency (nvarchar(3)) - NULL NO
        [Required]
        [MaxLength(3)]
        public string DefaultCurrency { get; set; } = string.Empty;

        // Column: ExternalKey (nvarchar(50)) - NULL YES
        [MaxLength(50)]
        public string? ExternalKey { get; set; }

        // Column: GeoAddressLine1 (nvarchar(150)) - NULL NO
        [Required]
        [MaxLength(150)]
        public string GeoAddressLine1 { get; set; } = string.Empty;

        // Column: GeoAddressLine2 (nvarchar(150)) - NULL NO
        [Required]
        [MaxLength(150)]
        public string GeoAddressLine2 { get; set; } = string.Empty;

        // Column: GeoCity (nvarchar(80)) - NULL NO
        [Required]
        [MaxLength(80)]
        public string GeoCity { get; set; } = string.Empty;

        // Column: GeoCountryISO (nvarchar(3)) - NULL NO
        [Required]
        [MaxLength(3)]
        public string GeoCountryISO { get; set; } = string.Empty;

        // Column: GeoCountryName (nvarchar(50)) - NULL NO
        [Required]
        [MaxLength(50)]
        public string GeoCountryName { get; set; } = string.Empty;

        // Column: GeoCulture (nvarchar(50)) - NULL NO
        [Required]
        [MaxLength(50)]
        public string GeoCulture { get; set; } = string.Empty;

        // Column: GeoLatitude (float) - NULL NO
        // Mapped to double as standard C# practice for SQL float/real
        public double GeoLatitude { get; set; }

        // Column: GeoLongitude (float) - NULL NO
        public double GeoLongitude { get; set; }

        // Column: GeoPostalCode (nvarchar(20)) - NULL NO
        [Required]
        [MaxLength(20)]
        public string GeoPostalCode { get; set; } = string.Empty;

        // Column: GeoRegionISO (nvarchar(10)) - NULL NO
        [Required]
        [MaxLength(10)]
        public string GeoRegionISO { get; set; } = string.Empty;

        // Column: GeoRegionName (nvarchar(50)) - NULL NO
        [Required]
        [MaxLength(50)]
        public string GeoRegionName { get; set; } = string.Empty;

        // Column: GeoTimezoneISO (nvarchar(10)) - NULL NO
        [Required]
        [MaxLength(10)]
        public string GeoTimezoneISO { get; set; } = string.Empty;

        // Column: GeoTimezoneName (nvarchar(50)) - NULL NO
        [Required]
        [MaxLength(50)]
        public string GeoTimezoneName { get; set; } = string.Empty;

        // Column: Name (nvarchar(100)) - NULL NO
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // Column: ProcessStatus (nvarchar(20)) - NULL YES
        [MaxLength(20)]
        public string? ProcessStatus { get; set; }

        // Column: ProcessUpdated (datetime2) - NULL YES
        public DateTime? ProcessUpdated { get; set; }

        // Column: ServiceProviderKey (nvarchar(50)) - NULL NO
        [Required]
        [MaxLength(50)]
        public string ServiceProviderKey { get; set; } = string.Empty;

        // Column: ShortCode (nvarchar(100)) - NULL NO
        [Required]
        [MaxLength(100)]
        public string ShortCode { get; set; } = string.Empty;

        // Column: Status (nvarchar(20)) - NULL NO
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty;

        // Column: Type (nvarchar(20)) - NULL NO
        [Required]
        [MaxLength(20)]
        public string Type { get; set; } = string.Empty;

        // Column: CreatedAt (datetime2) - NULL NO
        public DateTime CreatedAt { get; set; }

        // Column: UpdatedAt (datetime2) - NULL NO
        public DateTime UpdatedAt { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client;
using SiteLocationMigration.Db;
using SiteLocationMigration.Helpers;
using SiteLocationMigration.Models;
using SiteLocationMigration.Models.Modern;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteLocationMigration.Services
{
    public class MigrationService
    {
        private readonly AppSettings _appSettings;
        private readonly LegacyAtmbContext _legacyAtmbContext;
        private readonly ModernAtmbContext _modernAtmbContext;

        public MigrationService(AppSettings settings, LegacyAtmbContext legacyAtmbContext, ModernAtmbContext modernAtmbContext)
        {
            _appSettings = settings;
            _legacyAtmbContext = legacyAtmbContext;
            _modernAtmbContext = modernAtmbContext;
        }

        public async Task CheckConnection()
        {
            bool isLegacyContextConnected = await _legacyAtmbContext.Database.CanConnectAsync();
            if (!isLegacyContextConnected)
            {
                throw new Exception("Missing legacy context!");
            }

            bool isModernContextConnected = await _modernAtmbContext.Database.CanConnectAsync();
            if (!isModernContextConnected)
            {
                throw new Exception("Missing modern context!");
            }
        }

        public async Task RunMigration()
        {
            Console.WriteLine("\n--- Running Migration Worker ---");
            Console.WriteLine($"Modern Connection String (Injected): {_appSettings.ModernAtmbString}");
            Console.WriteLine($"Legacy Connection String (Injected): {_appSettings.LegacyAtmbString}");
            Console.WriteLine("--------------------------------");

            try
            {
                await CheckConnection();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not establish a connection! Throwing error and ending the program.");
                Console.WriteLine(ex);
                return;
            }

            try
            {
                await MigrateParentSiteLocationsToGeographies();
                await MigrateChildrenSiteLocationsToGeographies();

                // await MigrateUsaSiteLocationsToGeographies();
                await SetGeoCityToNull();
                await AssignGeographiesToLocations();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n\nSUCCESS: Done.\n\n");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"FAILURE: Error during migration.");
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Per request, all GeoCity fields in ENT_Geography should be set to null.
        /// </summary>
        /// <returns></returns>
        private async Task SetGeoCityToNull()
        {
            var geographiesToUpdate = await _modernAtmbContext.Geographies.Where(g => g.GeoCity != null).ToListAsync();
            foreach (var geography in geographiesToUpdate)
            {
                geography.GeoCity = null;
            }
            try
            {
                int recordsAffected = await _modernAtmbContext.SaveChangesAsync();
                Console.WriteLine($"======================== SetGeoCityToNull ========================");
                Console.WriteLine($"UPDATED ENT_Geography table. Rows affected: {recordsAffected}");
                Console.WriteLine($"================================================");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"FAILURE: Could not update geographies to set GeoCity to null.");
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        /// <summary>
        /// Gets parent sites first so that we have a reference
        /// </summary>
        /// <returns></returns>
        private async Task MigrateParentSiteLocationsToGeographies()
        {
            // Get parent locations first since they are root
            var parentSiteLocations = (await _legacyAtmbContext.SiteLocations.ToListAsync()).Where(sl => sl.DefaultText != string.Empty && sl.ParentSitLocId == 0).ToList();
            var existingExternalKeys = await _modernAtmbContext.Geographies.Select(g => g.ExternalKey).Where(k => k != null).Distinct().ToListAsync();
            var existingKeysSet = new HashSet<string>(existingExternalKeys, StringComparer.OrdinalIgnoreCase);
            var keysInCurrentBatch = new HashSet<string>();
            var geographies = new List<ENT_Geography>();

            var index = 1;
            foreach (var parentSiteLocation in parentSiteLocations)
            {
                Console.WriteLine($"Location {index}/{parentSiteLocations.Count} being migrated: ${parentSiteLocation.DefaultText}, SitLocId: {parentSiteLocation.SitLocId}, ParentSitLocId: {parentSiteLocation.ParentSitLocId}");
                var regionInfo = FindRegionByCountryName(parentSiteLocation.DefaultText);

                // No need to insert duplicates
                if (existingKeysSet.Contains(parentSiteLocation.WebKey))
                {
                    Console.WriteLine($"Location '{parentSiteLocation.DefaultText}' already exists in the DB as '{parentSiteLocation.WebKey}'");
                    index++;
                    continue;
                }

                if (keysInCurrentBatch.Contains(parentSiteLocation.WebKey))
                {
                    Console.WriteLine($"Location '{parentSiteLocation.DefaultText}' is a duplicate within the source data. Skipping.");
                    index++;
                    continue;
                }

                var geographyEquivalent = new ENT_Geography()
                {
                    ParentId = new Guid("00000000-0000-0000-0000-000000000001"), // for parent/root records
                    ExternalKey = parentSiteLocation.WebKey,
                    GeoCity = null, // Set to null per request
                    GeoCountryName = parentSiteLocation.DefaultText,
                    GeoCountryISO = regionInfo?.ThreeLetterISORegionName ?? "",
                    GeoLatitude = parentSiteLocation.MapLat,
                    GeoLongitude = parentSiteLocation.MapLong,
                    GeoRegionISO = regionInfo?.TwoLetterISORegionName ?? "",
                    GeoRegionName = null, // Parent's GeoRegionName is always NULL
                    MapZoom = parentSiteLocation.GoogleZoom,
                    Name = parentSiteLocation.DefaultText,
                    SourceKey = string.Empty, // Remains empty
                    Status = "ACTIVE",
                    Type = "PUBLIC",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                geographies.Add(geographyEquivalent);
                index++;
            }

            try
            {
                await _modernAtmbContext.Geographies.AddRangeAsync(geographies);
                int recordsAffected = await _modernAtmbContext.SaveChangesAsync();

                Console.WriteLine($"======================== MigrateParentSiteLocationsToGeographies ========================");
                Console.WriteLine($"INSERTED in ENT_Geography table. Rows affected: {recordsAffected}");
                Console.WriteLine($"================================================");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"FAILURE: Could not add new geography.");
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Gets parent sites and links them to child sites
        /// </summary>
        /// <returns></returns>
        private async Task MigrateChildrenSiteLocationsToGeographies()
        {
            // Get parent locations first since they are root
            var siteLocations = (await _legacyAtmbContext.SiteLocations.ToListAsync()).Where(sl => sl.DefaultText != string.Empty && sl.ParentSitLocId != 0).ToList();
            var existingExternalKeys = await _modernAtmbContext.Geographies.Select(g => g.ExternalKey).Where(k => k != null).Distinct().ToListAsync();
            var existingKeysSet = new HashSet<string>(existingExternalKeys, StringComparer.OrdinalIgnoreCase);
            var keysInCurrentBatch = new HashSet<string>();

            var geographies = new List<ENT_Geography>();

            var index = 1;
            foreach (var siteLocation in siteLocations)
            {
                var parentLocation = (await _legacyAtmbContext.SiteLocations.ToListAsync()).Where(sl => sl.SitLocId == siteLocation.ParentSitLocId).FirstOrDefault();
                
                if (parentLocation == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("==============================");
                    Console.WriteLine($"FAILURE: This site location {siteLocation.SitLocId} - {siteLocation.DefaultText} does not have an existing parent with SitLocId of '{siteLocation.ParentSitLocId}'.");
                    Console.WriteLine("==============================");
                    Console.ResetColor();
                    index++;
                    continue;
                }

                var parentGeography = (await _modernAtmbContext.Geographies.ToListAsync()).Where(g => g.ExternalKey == parentLocation.WebKey).FirstOrDefault();

                if (parentGeography == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("==============================");
                    Console.WriteLine($"FAILURE: This site location's parent geography does not exist in ENT_Geography for parent WebKey '{parentLocation.WebKey}'.");
                    Console.WriteLine("==============================");
                    Console.ResetColor();
                    index++;
                    continue;
                }

                Console.WriteLine($"Location {index}/{siteLocations.Count} being migrated: ${siteLocation.DefaultText}, SitLocId: {siteLocation.SitLocId}, ParentSitLocId: {siteLocation.ParentSitLocId}");
                var regionInfo = FindRegionByCountryName(siteLocation.DefaultText);

                // No need to insert duplicates
                if (existingKeysSet.Contains(siteLocation.WebKey))
                {
                    Console.WriteLine($"Location '{siteLocation.DefaultText}' already exists in the DB as '{siteLocation.WebKey}'. Updating instead");

                    var existingGeography = (await _modernAtmbContext.Geographies.ToListAsync()).Where(g => g.ExternalKey == siteLocation.WebKey).FirstOrDefault();
                    existingGeography.GeoCountryISO = regionInfo?.ThreeLetterISORegionName ?? "";
                    existingGeography.GeoRegionISO = regionInfo?.TwoLetterISORegionName ?? "";
                    existingGeography.Name = siteLocation.DefaultText;
                    existingGeography.GeoCountryName = parentLocation.DefaultText;

                    try
                    {
                        int recordsAffected = await _modernAtmbContext.SaveChangesAsync();

                        Console.WriteLine($"======================== MigrateChildrenSiteLocationsToGeographies ========================");
                        Console.WriteLine($"UPDATED in ENT_Geography table. Rows affected: {recordsAffected}");
                        Console.WriteLine($"================================================");
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"FAILURE: Could not update new geography.");
                        Console.WriteLine($"Error: {ex.Message}");
                        Console.ResetColor();
                    }

                    index++;
                    continue;
                }

                if (keysInCurrentBatch.Contains(siteLocation.WebKey))
                {
                    Console.WriteLine($"Location '{siteLocation.DefaultText}' is a duplicate within the source data. Skipping.");
                    index++;
                    continue;
                }

                var geographyEquivalent = new ENT_Geography()
                {
                    ParentId = parentGeography.GeographyId,
                    ExternalKey = siteLocation.WebKey,
                    GeoCity = null, // Set to null per request
                    GeoCountryName = parentLocation.DefaultText,
                    GeoCountryISO = regionInfo?.ThreeLetterISORegionName ?? "",
                    GeoLatitude = siteLocation.MapLat,
                    GeoLongitude = siteLocation.MapLong,
                    GeoRegionISO = regionInfo?.TwoLetterISORegionName ?? "",
                    GeoRegionName = siteLocation.DefaultText,
                    MapZoom = siteLocation.GoogleZoom,
                    Name = siteLocation.DefaultText,
                    SourceKey = string.Empty, // Remains empty
                    Status = "ACTIVE",
                    Type = "PUBLIC",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                geographies.Add(geographyEquivalent);
                keysInCurrentBatch.Add(siteLocation.WebKey);

                index++;
            }

            try
            {
                await _modernAtmbContext.Geographies.AddRangeAsync(geographies);
                int recordsAffected = await _modernAtmbContext.SaveChangesAsync();

                Console.WriteLine($"======================== MigrateChildrenSiteLocationsToGeographies ========================");
                Console.WriteLine($"INSERTED in ENT_Geography table. Rows affected: {recordsAffected}");
                Console.WriteLine($"================================================");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"FAILURE: Could not add new geography.");
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }
        }

        private async Task MigrateUsaSiteLocationsToGeographies()
        {
            const int USA_SIT_LOC_ID = 1;

            // Get locations where they are either USA or their parent is USA only
            var preexistingGeographies = (await _modernAtmbContext.Geographies.ToListAsync());
            var geographies = new List<ENT_Geography>();

            var parentUnitedStatesGeography = preexistingGeographies.Where(g => g.ExternalKey == "usa").FirstOrDefault();

            if (parentUnitedStatesGeography == null)
            {
                throw new Exception("No USA in ENT_Geography! Ending program");
            }

            var existingExternalKeys = await _modernAtmbContext.Geographies.Select(g => g.ExternalKey).Where(k => k != null).Distinct().ToListAsync();
            var existingKeysSet = new HashSet<string>(existingExternalKeys, StringComparer.OrdinalIgnoreCase);

            var usaSiteLocations = (await _legacyAtmbContext.SiteLocations.ToListAsync()).Where(sl => sl.DefaultText != string.Empty && (sl.SitLocId == USA_SIT_LOC_ID || sl.ParentSitLocId == USA_SIT_LOC_ID)).ToList();
            
            var index = 1;
            foreach (var usaSiteLocation in usaSiteLocations)
            {
                Console.WriteLine($"Location {index}/{usaSiteLocations.Count} being migrated: ${usaSiteLocation.DefaultText}, SitLocId: {usaSiteLocation.SitLocId}, ParentSitLocId: {usaSiteLocation.ParentSitLocId}");
                var stateIso = MapperHelper.GetStateCodeFromStateName(usaSiteLocation.DefaultText);

                // No need to insert duplicates
                if (existingKeysSet.Contains(usaSiteLocation.WebKey))
                {
                    Console.WriteLine($"Location '{usaSiteLocation.DefaultText}' already exists in the DB as '{usaSiteLocation.WebKey}'");
                    index++;
                    continue;
                }

                var geographyEquivalent = new ENT_Geography() 
                {
                    ParentId = parentUnitedStatesGeography.GeographyId,
                    ExternalKey = usaSiteLocation.WebKey,
                    GeoCity = null, // Set to null per request
                    GeoCountryName = "United States",
                    GeoCountryISO = "USA",
                    GeoLatitude = usaSiteLocation.MapLat,
                    GeoLongitude = usaSiteLocation.MapLong,
                    GeoRegionISO = stateIso,
                    GeoRegionName = usaSiteLocation.DefaultText,
                    MapZoom = usaSiteLocation.GoogleZoom,
                    Name = usaSiteLocation.DefaultText,
                    SourceKey = string.Empty, // Remains empty
                    Status = "ACTIVE",
                    Type = "PUBLIC",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                geographies.Add(geographyEquivalent);
                index++;
            }

            try
            {
                await _modernAtmbContext.Geographies.AddRangeAsync(geographies);
                int recordsAffected = await _modernAtmbContext.SaveChangesAsync();

                Console.WriteLine($"======================== MigrateUsaSiteLocationsToGeographies ========================");
                Console.WriteLine($"INSERTED in ENT_Geography table. Rows affected: {recordsAffected}");
                Console.WriteLine($"================================================");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"FAILURE: Could not add new geograph.");
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }
        }

        private RegionInfo FindRegionByCountryName(string countryName)
        {
            if (string.IsNullOrWhiteSpace(countryName))
                return null;

            // Normalize the search name
            string normalizedName = countryName.Trim();

            try
            {
                RegionInfo region = CultureInfo
                .GetCultures(CultureTypes.SpecificCultures)
                .Select(c =>
                {
                    try { return new RegionInfo(c.Name); }
                    catch { return null; }
                })
                .Where(r => r != null)
                .FirstOrDefault(r =>
                    r.EnglishName.Equals(countryName, StringComparison.OrdinalIgnoreCase));

                return region;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"FAILURE: Could not get list of regions.");
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }

            return null;
        }

        private async Task AssignGeographiesToLocations()
        {
            // Set to 2000 per request
            const int MAX_RECORDS_TO_ADD = 1000;
            const bool IS_INTERNATIONAL = false;

            var preexistingGeographies = (await _modernAtmbContext.Geographies.ToListAsync());
            var preexistingLocations = IS_INTERNATIONAL ? (await _modernAtmbContext.Locations.Where(l => l.GeoCountryISO != "USA").ToListAsync()) : (await _modernAtmbContext.Locations.ToListAsync());
            var preexistingGeographyLocations = (await _modernAtmbContext.GeographyLocations.ToListAsync());

            int locationInsertedCount = 0;

            for(int i = 0; locationInsertedCount < MAX_RECORDS_TO_ADD && i < preexistingLocations.Count; i++)
            {
                var location = preexistingLocations[i];
                Console.WriteLine($"GeographyLocation {i}/{preexistingLocations.Count} being migrated: ${location.Name}, LocationId: {location.LocationId}");

                var geography = preexistingGeographies.Where(g => g.GeoRegionName == location.GeoRegionName).FirstOrDefault();

                if (geography == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("==============================");
                    Console.WriteLine($"FAILURE: This location {location.LocationId} - {location.Name} does not have an existing geography with a name of '{location.GeoRegionName}'.");
                    Console.WriteLine("==============================");
                    Console.ResetColor();
                    continue;
                }

                var existingGeographyLocation = preexistingGeographyLocations.Where(gl => gl.LocationId == location.LocationId).FirstOrDefault();

                if (existingGeographyLocation != null)
                {
                    Console.WriteLine($"GeographyLocation link already exists for location ({location.Name} - {location.LocationId}). Delete first.");
                    continue;
                }

                var geographyLocation = preexistingGeographyLocations.Where(gl => gl.LocationId == location.LocationId && gl.GeographyId == geography.GeographyId).FirstOrDefault();

                if (geographyLocation != null)
                {
                    Console.WriteLine($"GeographyLocation link already exists between location ({location.Name} - {location.LocationId}) and geography ({geography.Name} - {geography.GeographyId}). No need to update values");
                    continue;
                }

                try
                {
                    var newGeographyLocation = new ENT_GeographyLocation()
                    {
                        GeographyId = geography.GeographyId,
                        LocationId = location.LocationId,
                        DateBegin = null,
                        DateEnd = null,
                        SortOrder = 0, // 0 by default,
                        Status = "ACTIVE",
                        Type = "SITE",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _modernAtmbContext.GeographyLocations.AddAsync(newGeographyLocation);
                    int recordsAffected = await _modernAtmbContext.SaveChangesAsync();
                    locationInsertedCount++;

                    Console.WriteLine($"======================== GiveLocationsGeographies ========================");
                    Console.WriteLine($"INSERTED in ENT_GeographyLocation table. Rows affected: {recordsAffected}");
                    Console.WriteLine($"Total ENT_GeographyLocation records inserted as of running: {locationInsertedCount}");
                    Console.WriteLine($"================================================");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"FAILURE: Could not add new ENT_GeographyLocation record.");
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.ResetColor();
                }
            }

        }
    }
}

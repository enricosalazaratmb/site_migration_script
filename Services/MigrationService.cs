using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SiteLocationMigration.Db;
using SiteLocationMigration.Helpers;
using SiteLocationMigration.Models;
using SiteLocationMigration.Models.Modern;
using System;
using System.Collections.Generic;
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
            bool isModernContextConnected = await _modernAtmbContext.Database.CanConnectAsync();

            if (!isLegacyContextConnected)
            {
                throw new Exception("Missing legacy context!");
            }

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
                var sites = await _legacyAtmbContext.Sites.ToListAsync();
               
                var siteLocationItems = await _legacyAtmbContext.SiteLocationItems.ToListAsync();

                var locations = await _modernAtmbContext.Locations.ToListAsync();

                var geographyLocations = await _modernAtmbContext.GeographyLocations.ToListAsync();

                await MigrateUsaSiteLocationsToGeographies();
                await AssignGeographiesToLocations();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"SUCCESS: Found {sites.Count} sites in the Legacy Database.");
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

        public async Task MigrateUsaSiteLocationsToGeographies()
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
                    GeoCity = usaSiteLocation.DefaultText,
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

        public async Task AssignGeographiesToLocations()
        {
            var preexistingGeographies = (await _modernAtmbContext.Geographies.ToListAsync());
            var preexistingLocations = (await _modernAtmbContext.Locations.ToListAsync());
            var preexistingGeographyLocations = (await _modernAtmbContext.GeographyLocations.ToListAsync());

            int locationIndex = 1;
            int locationInsertedCount = 0;

            for(int i = 0; i < preexistingLocations.Count; i++)
            {
                var location = preexistingLocations[i];
                Console.WriteLine($"Location {locationIndex}/{preexistingLocations.Count} being migrated: ${location.Name}, LocationId: {location.LocationId}");

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
                    Console.WriteLine($"================================================");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"FAILURE: Could not add new ENT_GeographyLocation record.");
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.ResetColor();
                }

                Console.WriteLine($"======================== GiveLocationsGeographies ========================");
                Console.WriteLine($"Total ENT_GeographyLocation records inserted: {locationInsertedCount}");
                Console.WriteLine($"================================================");
            }

        }
    }
}

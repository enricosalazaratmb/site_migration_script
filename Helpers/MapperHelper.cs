using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteLocationMigration.Helpers
{
    public class MapperHelper
    {
            // Dictionary for case-insensitive lookup (Name -> Code)
        private static readonly Dictionary<string, string> StateNameToCodeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // --- US States ---
            {"ALABAMA", "AL"},
            {"ALASKA", "AK"},
            {"ARIZONA", "AZ"},
            {"ARKANSAS", "AR"},
            {"CALIFORNIA", "CA"},
            {"COLORADO", "CO"},
            {"CONNECTICUT", "CT"},
            {"DELAWARE", "DE"},
            {"FLORIDA", "FL"},
            {"GEORGIA", "GA"},
            {"HAWAII", "HI"},
            {"IDAHO", "ID"},
            {"ILLINOIS", "IL"},
            {"INDIANA", "IN"},
            {"IOWA", "IA"},
            {"KANSAS", "KS"},
            {"KENTUCKY", "KY"},
            {"LOUISIANA", "LA"},
            {"MAINE", "ME"},
            {"MARYLAND", "MD"},
            {"MASSACHUSETTS", "MA"},
            {"MICHIGAN", "MI"},
            {"MINNESOTA", "MN"},
            {"MISSISSIPPI", "MS"},
            {"MISSOURI", "MO"},
            {"MONTANA", "MT"},
            {"NEBRASKA", "NE"},
            {"NEVADA", "NV"},
            {"NEW HAMPSHIRE", "NH"},
            {"NEW JERSEY", "NJ"},
            {"NEW MEXICO", "NM"},
            {"NEW YORK", "NY"},
            {"NORTH CAROLINA", "NC"},
            {"NORTH DAKOTA", "ND"},
            {"OHIO", "OH"},
            {"OKLAHOMA", "OK"},
            {"OREGON", "OR"},
            {"PENNSYLVANIA", "PA"},
            {"RHODE ISLAND", "RI"},
            {"SOUTH CAROLINA", "SC"},
            {"SOUTH DAKOTA", "SD"},
            {"TENNESSEE", "TN"},
            {"TEXAS", "TX"},
            {"UTAH", "UT"},
            {"VERMONT", "VT"},
            {"VIRGINIA", "VA"},
            {"WASHINGTON", "WA"},
            {"WEST VIRGINIA", "WV"},
            {"WISCONSIN", "WI"},
            {"WYOMING", "WY"},

            // --- US Territories (Optional, but often needed) ---
            {"DISTRICT OF COLUMBIA", "DC"},
            {"AMERICAN SAMOA", "AS"},
            {"GUAM", "GU"},
            {"NORTHERN MARIANA ISLANDS", "MP"},
            {"PUERTO RICO", "PR"},
            {"US VIRGIN ISLANDS", "VI"}
        };

        [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase")]
        public static string? GetStateCodeFromStateName(string? stateName)
        {
            if (string.IsNullOrWhiteSpace(stateName))
            {
                throw new Exception($"State name is empty!");
            }

            string cleanName = stateName.Trim().ToUpperInvariant();

            // Handle your special 'NULL' cases if they are literals in the database
            if (cleanName == "NULL")
            {
                return null;
            }

            if (StateNameToCodeMap.TryGetValue(cleanName, out string? code))
            {
                return code;
            }

            return null; // Return null if the state name is not recognized
        }
    }
}

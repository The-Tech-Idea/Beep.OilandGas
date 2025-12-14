using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Drawing.Styling
{
    /// <summary>
    /// Maps lithology names to USGS-FGDC pattern codes (sed601-sed686, igm701-igm733).
    /// Based on FGDC Digital Cartographic Standard for Geologic Map Symbolization.
    /// </summary>
    public static class UsgsFgdcPatternMapping
    {
        /// <summary>
        /// Maps lithology names to USGS-FGDC pattern codes.
        /// </summary>
        private static readonly Dictionary<string, string> LithologyToPatternCode = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Sedimentary Rocks - Common
            ["sandstone"] = "sed601",
            ["sand"] = "sed601",
            ["shale"] = "sed602",
            ["siltstone"] = "sed603",
            ["silt"] = "sed603",
            ["claystone"] = "sed604",
            ["clay"] = "sed604",
            ["mudstone"] = "sed605",
            ["mud"] = "sed605",
            ["conglomerate"] = "sed606",
            ["breccia"] = "sed607",
            
            // Sedimentary Rocks - Carbonates
            ["limestone"] = "sed608",
            ["dolomite"] = "sed609",
            ["dolostone"] = "sed609",
            ["chalk"] = "sed610",
            ["marl"] = "sed611",
            ["coquina"] = "sed612",
            
            // Sedimentary Rocks - Evaporites
            ["anhydrite"] = "sed613",
            ["gypsum"] = "sed614",
            ["halite"] = "sed615",
            ["salt"] = "sed615",
            ["rock salt"] = "sed615",
            
            // Sedimentary Rocks - Organic
            ["coal"] = "sed616",
            ["lignite"] = "sed617",
            ["peat"] = "sed618",
            
            // Sedimentary Rocks - Other
            ["chert"] = "sed619",
            ["flint"] = "sed620",
            ["ironstone"] = "sed621",
            ["phosphorite"] = "sed622",
            
            // Igneous Rocks - Intrusive
            ["granite"] = "igm701",
            ["granodiorite"] = "igm702",
            ["diorite"] = "igm703",
            ["gabbro"] = "igm704",
            ["peridotite"] = "igm705",
            ["syenite"] = "igm706",
            ["monzonite"] = "igm707",
            ["tonalite"] = "igm708",
            ["anorthosite"] = "igm709",
            
            // Igneous Rocks - Volcanic
            ["rhyolite"] = "igm710",
            ["dacite"] = "igm711",
            ["andesite"] = "igm712",
            ["basalt"] = "igm713",
            ["pumice"] = "igm714",
            ["obsidian"] = "igm715",
            ["tuff"] = "igm716",
            ["volcanic breccia"] = "igm717",
            ["volcanic"] = "igm713", // Default to basalt
            
            // Metamorphic Rocks
            ["gneiss"] = "igm718",
            ["schist"] = "igm719",
            ["phyllite"] = "igm720",
            ["slate"] = "igm721",
            ["quartzite"] = "igm722",
            ["marble"] = "igm723",
            ["amphibolite"] = "igm724",
            ["hornfels"] = "igm725",
            ["migmatite"] = "igm726",
            ["serpentinite"] = "igm727",
            ["eclogite"] = "igm728",
            ["blueschist"] = "igm729",
            ["greenschist"] = "igm730",
            ["mylonite"] = "igm731",
            ["cataclasite"] = "igm732",
            ["mylonite gneiss"] = "igm733"
        };

        /// <summary>
        /// Gets the USGS-FGDC pattern code for a lithology name.
        /// </summary>
        /// <param name="lithology">The lithology name.</param>
        /// <returns>The pattern code (e.g., "sed601", "igm701"), or null if not found.</returns>
        public static string GetPatternCode(string lithology)
        {
            if (string.IsNullOrEmpty(lithology))
                return null;

            string normalized = lithology.Trim();
            
            // Try exact match first
            if (LithologyToPatternCode.ContainsKey(normalized))
            {
                return LithologyToPatternCode[normalized];
            }

            // Try case-insensitive match
            foreach (var kvp in LithologyToPatternCode)
            {
                if (string.Equals(kvp.Key, normalized, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            // Try partial match (contains)
            foreach (var kvp in LithologyToPatternCode)
            {
                if (normalized.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase) ||
                    kvp.Key.Contains(normalized, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the pattern directory for a pattern code.
        /// Always returns SED or IGM (attributable colors) - fixed color versions are not needed.
        /// </summary>
        /// <param name="patternCode">The pattern code (e.g., "sed601", "igm701").</param>
        /// <returns>The directory name ("SED" or "IGM").</returns>
        public static string GetPatternDirectory(string patternCode)
        {
            if (string.IsNullOrEmpty(patternCode))
                return null;

            string prefix = patternCode.Substring(0, 3).ToLowerInvariant();
            
            if (prefix == "sed")
            {
                return "SED";  // Always use SED (attributable colors)
            }
            else if (prefix == "igm")
            {
                return "IGM";  // Always use IGM (attributable colors)
            }

            return null;
        }

        /// <summary>
        /// Gets the full pattern file path.
        /// Always uses SED/ or IGM/ directories (attributable colors).
        /// </summary>
        /// <param name="baseDirectory">Base directory containing the pattern folders.</param>
        /// <param name="patternCode">The pattern code.</param>
        /// <returns>The full file path to the SVG pattern file.</returns>
        public static string GetPatternFilePath(string baseDirectory, string patternCode)
        {
            if (string.IsNullOrEmpty(baseDirectory) || string.IsNullOrEmpty(patternCode))
                return null;

            string directory = GetPatternDirectory(patternCode);
            if (directory == null)
                return null;

            return System.IO.Path.Combine(baseDirectory, directory, $"{patternCode}.svg");
        }

        /// <summary>
        /// Checks if a pattern code is valid.
        /// </summary>
        /// <param name="patternCode">The pattern code to check.</param>
        /// <returns>True if valid, false otherwise.</returns>
        public static bool IsValidPatternCode(string patternCode)
        {
            if (string.IsNullOrEmpty(patternCode))
                return false;

            string prefix = patternCode.Substring(0, 3).ToLowerInvariant();
            if (prefix != "sed" && prefix != "igm")
                return false;

            if (prefix == "sed")
            {
                // sed601 to sed686
                if (int.TryParse(patternCode.Substring(3), out int num))
                {
                    return num >= 601 && num <= 686;
                }
            }
            else if (prefix == "igm")
            {
                // igm701 to igm733
                if (int.TryParse(patternCode.Substring(3), out int num))
                {
                    return num >= 701 && num <= 733;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets all available pattern codes.
        /// </summary>
        /// <returns>A list of all pattern codes.</returns>
        public static List<string> GetAllPatternCodes()
        {
            var codes = new List<string>();
            
            // Sedimentary patterns (sed601-sed686)
            for (int i = 601; i <= 686; i++)
            {
                codes.Add($"sed{i}");
            }
            
            // Igneous and metamorphic patterns (igm701-igm733)
            for (int i = 701; i <= 733; i++)
            {
                codes.Add($"igm{i}");
            }
            
            return codes;
        }
    }
}


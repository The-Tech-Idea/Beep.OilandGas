using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.Drawing.DataLoaders.PWLS
{
    /// <summary>
    /// Provides mapping between vendor-specific log curve mnemonics and PWLS (Practical Well Log Standard) property names.
    /// PWLS standardizes log curve identification across different service companies.
    /// </summary>
    public static class PwlsMnemonicMapper
    {
        // PWLS property mappings (vendor mnemonic -> PWLS property name)
        private static readonly Dictionary<string, string> GammaRayMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Schlumberger
            { "GR", "GammaRay" },
            { "GRC", "GammaRay" },
            { "GR_EDTC", "GammaRay" },
            
            // Halliburton
            { "GRAPI", "GammaRay" },
            { "GR_1", "GammaRay" },
            
            // Baker Hughes
            { "GR", "GammaRay" },
            { "GR_CORR", "GammaRay" },
            
            // Weatherford
            { "GR", "GammaRay" },
            
            // Generic
            { "GAMMA", "GammaRay" },
            { "GAMMA_RAY", "GammaRay" },
            { "GAMMARAY", "GammaRay" }
        };

        private static readonly Dictionary<string, string> ResistivityMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Deep Resistivity
            { "RT", "DeepResistivity" },
            { "RD", "DeepResistivity" },
            { "LLD", "DeepResistivity" },
            { "ILD", "DeepResistivity" },
            { "AT90", "DeepResistivity" },
            
            // Medium Resistivity
            { "RM", "MediumResistivity" },
            { "LLM", "MediumResistivity" },
            { "ILM", "MediumResistivity" },
            { "AT60", "MediumResistivity" },
            
            // Shallow Resistivity
            { "RS", "ShallowResistivity" },
            { "LLS", "ShallowResistivity" },
            { "ILS", "ShallowResistivity" },
            { "AT10", "ShallowResistivity" },
            
            // Micro Resistivity
            { "MSFL", "MicroResistivity" },
            { "MCFL", "MicroResistivity" },
            { "RMF", "MicroResistivity" }
        };

        private static readonly Dictionary<string, string> PorosityMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Neutron Porosity
            { "NPHI", "NeutronPorosity" },
            { "NPOR", "NeutronPorosity" },
            { "TNPH", "NeutronPorosity" },
            { "CN", "NeutronPorosity" },
            
            // Density Porosity
            { "DPHI", "DensityPorosity" },
            { "DPOR", "DensityPorosity" },
            { "RHOB", "BulkDensity" },
            { "RHOZ", "BulkDensity" },
            { "ROBB", "BulkDensity" }, // LWD
            
            // Sonic Porosity
            { "SPHI", "SonicPorosity" },
            { "SPOR", "SonicPorosity" },
            { "DT", "AcousticSlowness" },
            { "DTCO", "AcousticSlowness" }, // LWD
            { "DT4P", "AcousticSlowness" }, // Wireline
            { "DTC", "AcousticSlowness" }
        };

        private static readonly Dictionary<string, string> DensityMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "RHOB", "BulkDensity" },
            { "RHOZ", "BulkDensity" },
            { "ROBB", "BulkDensity" },
            { "DEN", "BulkDensity" },
            { "DENS", "BulkDensity" },
            { "DENSITY", "BulkDensity" }
        };

        private static readonly Dictionary<string, string> SonicMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "DT", "AcousticSlowness" },
            { "DTCO", "AcousticSlowness" },
            { "DT4P", "AcousticSlowness" },
            { "DTC", "AcousticSlowness" },
            { "DT_COMP", "AcousticSlowness" },
            { "SONIC", "AcousticSlowness" }
        };

        private static readonly Dictionary<string, string> CaliperMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "CALI", "Caliper" },
            { "CAL", "Caliper" },
            { "CALIPER", "Caliper" },
            { "CAL1", "Caliper" },
            { "CAL2", "Caliper" }
        };

        // Combined mapping dictionary
        private static readonly Dictionary<string, string> AllMappings;

        static PwlsMnemonicMapper()
        {
            AllMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            // Combine all mappings
            foreach (var mapping in GammaRayMappings)
                AllMappings[mapping.Key] = mapping.Value;
            foreach (var mapping in ResistivityMappings)
                AllMappings[mapping.Key] = mapping.Value;
            foreach (var mapping in PorosityMappings)
                AllMappings[mapping.Key] = mapping.Value;
            foreach (var mapping in DensityMappings)
                AllMappings[mapping.Key] = mapping.Value;
            foreach (var mapping in SonicMappings)
                AllMappings[mapping.Key] = mapping.Value;
            foreach (var mapping in CaliperMappings)
                AllMappings[mapping.Key] = mapping.Value;
        }

        /// <summary>
        /// Maps a vendor-specific mnemonic to its PWLS property name.
        /// </summary>
        /// <param name="mnemonic">The vendor-specific mnemonic (e.g., "GR", "RT", "NPHI").</param>
        /// <returns>The PWLS property name, or the original mnemonic if no mapping exists.</returns>
        public static string MapToPwlsProperty(string mnemonic)
        {
            if (string.IsNullOrWhiteSpace(mnemonic))
                return mnemonic;

            // Try exact match first
            if (AllMappings.TryGetValue(mnemonic, out var pwlsName))
                return pwlsName;

            // Try case-insensitive match
            var key = AllMappings.Keys.FirstOrDefault(k => 
                string.Equals(k, mnemonic, StringComparison.OrdinalIgnoreCase));
            
            if (key != null)
                return AllMappings[key];

            // Return original if no mapping found
            return mnemonic;
        }

        /// <summary>
        /// Maps multiple mnemonics to their PWLS property names.
        /// </summary>
        /// <param name="mnemonics">The list of vendor-specific mnemonics.</param>
        /// <returns>A dictionary mapping original mnemonics to PWLS property names.</returns>
        public static Dictionary<string, string> MapToPwlsProperties(IEnumerable<string> mnemonics)
        {
            var result = new Dictionary<string, string>();
            
            foreach (var mnemonic in mnemonics ?? Enumerable.Empty<string>())
            {
                result[mnemonic] = MapToPwlsProperty(mnemonic);
            }
            
            return result;
        }

        /// <summary>
        /// Gets all known mnemonics for a specific PWLS property.
        /// </summary>
        /// <param name="pwlsProperty">The PWLS property name (e.g., "GammaRay", "BulkDensity").</param>
        /// <returns>A list of vendor mnemonics that map to this property.</returns>
        public static List<string> GetMnemonicsForProperty(string pwlsProperty)
        {
            if (string.IsNullOrWhiteSpace(pwlsProperty))
                return new List<string>();

            return AllMappings
                .Where(kvp => string.Equals(kvp.Value, pwlsProperty, StringComparison.OrdinalIgnoreCase))
                .Select(kvp => kvp.Key)
                .ToList();
        }

        /// <summary>
        /// Checks if a mnemonic has a PWLS mapping.
        /// </summary>
        /// <param name="mnemonic">The vendor-specific mnemonic.</param>
        /// <returns>True if a mapping exists, false otherwise.</returns>
        public static bool HasMapping(string mnemonic)
        {
            if (string.IsNullOrWhiteSpace(mnemonic))
                return false;

            return AllMappings.ContainsKey(mnemonic) ||
                   AllMappings.Keys.Any(k => string.Equals(k, mnemonic, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets all available PWLS property names.
        /// </summary>
        /// <returns>A list of unique PWLS property names.</returns>
        public static List<string> GetAvailablePwlsProperties()
        {
            return AllMappings.Values
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(p => p)
                .ToList();
        }

        /// <summary>
        /// Adds a custom mnemonic mapping.
        /// </summary>
        /// <param name="mnemonic">The vendor-specific mnemonic.</param>
        /// <param name="pwlsProperty">The PWLS property name.</param>
        public static void AddCustomMapping(string mnemonic, string pwlsProperty)
        {
            if (string.IsNullOrWhiteSpace(mnemonic) || string.IsNullOrWhiteSpace(pwlsProperty))
                return;

            AllMappings[mnemonic] = pwlsProperty;
        }

        /// <summary>
        /// Removes a mnemonic mapping.
        /// </summary>
        /// <param name="mnemonic">The vendor-specific mnemonic to remove.</param>
        /// <returns>True if the mapping was removed, false if it didn't exist.</returns>
        public static bool RemoveMapping(string mnemonic)
        {
            if (string.IsNullOrWhiteSpace(mnemonic))
                return false;

            return AllMappings.Remove(mnemonic);
        }

        /// <summary>
        /// Gets the count of available mappings.
        /// </summary>
        /// <returns>The number of mnemonic mappings.</returns>
        public static int GetMappingCount()
        {
            return AllMappings.Count;
        }
    }
}


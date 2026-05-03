using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.FlashCalculations;

namespace Beep.OilandGas.FlashCalculations.Services
{
    /// <summary>
    /// Builds <see cref="FLASH_COMPONENT"/> feed rows from liquid mole fractions while preserving
    /// physical properties (Tc, Pc, ω, MW) from an original component catalog — required for Wilson K
    /// on multi-stage separator trains.
    /// </summary>
    public static class FlashFeedCatalogMerge
    {
        /// <summary>
        /// Maps liquid-phase mole fractions to feed components, copying critical data from <paramref name="catalog"/>
        /// when <see cref="FlashComponentFraction.ComponentName"/> matches <see cref="FLASH_COMPONENT.NAME"/>
        /// or <see cref="FLASH_COMPONENT.COMPONENT_NAME"/> (ordinal case-insensitive).
        /// </summary>
        public static List<FLASH_COMPONENT> FromLiquidComposition(
            IEnumerable<FlashComponentFraction>? liquid,
            IReadOnlyList<FLASH_COMPONENT>? catalog)
        {
            var liquidList = liquid?.Where(l => l != null).ToList() ?? new List<FlashComponentFraction>();
            var catalogList = catalog?.Where(c => c != null).ToList() ?? new List<FLASH_COMPONENT>();

            var lookup = new Dictionary<string, FLASH_COMPONENT>(StringComparer.OrdinalIgnoreCase);
            foreach (var c in catalogList)
            {
                AddKey(lookup, c.NAME, c);
                AddKey(lookup, c.COMPONENT_NAME, c);
            }

            var result = new List<FLASH_COMPONENT>();
            foreach (var row in liquidList)
            {
                var name = row.ComponentName?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(name))
                    continue;

                lookup.TryGetValue(name, out var template);

                result.Add(new FLASH_COMPONENT
                {
                    NAME = template?.NAME ?? name,
                    COMPONENT_NAME = template?.COMPONENT_NAME ?? name,
                    MOLE_FRACTION = row.Fraction,
                    CRITICAL_TEMPERATURE = template?.CRITICAL_TEMPERATURE ?? 0m,
                    CRITICAL_PRESSURE = template?.CRITICAL_PRESSURE ?? 0m,
                    ACENTRIC_FACTOR = template?.ACENTRIC_FACTOR ?? 0m,
                    MOLECULAR_WEIGHT = template?.MOLECULAR_WEIGHT ?? 0m
                });
            }

            return result;
        }

        private static void AddKey(Dictionary<string, FLASH_COMPONENT> lookup, string? key, FLASH_COMPONENT component)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;
            var k = key.Trim();
            if (!lookup.ContainsKey(k))
                lookup[k] = component;
        }
    }
}

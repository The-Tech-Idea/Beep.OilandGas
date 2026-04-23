using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Drawing.DataLoaders.Models;

namespace Beep.OilandGas.Drawing.DataLoaders.PWLS
{
    /// <summary>
    /// Applies shared ingestion-time normalization for log curves and metadata.
    /// </summary>
    public static class LogDataIngestionNormalizer
    {
        /// <summary>
        /// Normalizes log curve metadata and applies alias-aware curve filtering.
        /// </summary>
        public static void Normalize(LogData logData, LogLoadConfiguration configuration)
        {
            if (logData == null)
                throw new ArgumentNullException(nameof(logData));

            configuration ??= new LogLoadConfiguration();
            logData.Curves ??= new Dictionary<string, List<double>>(StringComparer.OrdinalIgnoreCase);
            logData.CurveMetadata ??= new Dictionary<string, LogCurveMetadata>(StringComparer.OrdinalIgnoreCase);

            foreach (var curveName in logData.Curves.Keys.ToList())
            {
                if (!logData.CurveMetadata.TryGetValue(curveName, out var metadata) || metadata == null)
                {
                    metadata = new LogCurveMetadata();
                }

                metadata.Mnemonic ??= curveName;
                metadata.DisplayName = ResolveDisplayName(curveName, metadata, configuration.UsePwlsMapping);
                metadata.Description ??= metadata.DisplayName ?? metadata.Mnemonic ?? curveName;
                logData.CurveMetadata[curveName] = metadata;
            }

            ApplyCurveFilter(logData, configuration.CurvesToLoad);
        }

        private static string ResolveDisplayName(string curveName, LogCurveMetadata metadata, bool usePwlsMapping)
        {
            var sourceName = metadata?.Mnemonic ?? curveName;
            if (!usePwlsMapping)
                return metadata?.DisplayName ?? sourceName;

            var mappedName = PwlsMnemonicMapper.MapToPwlsProperty(sourceName);
            return string.IsNullOrWhiteSpace(mappedName)
                ? sourceName
                : mappedName;
        }

        private static void ApplyCurveFilter(LogData logData, List<string> curvesToLoad)
        {
            if (curvesToLoad == null || curvesToLoad.Count == 0)
                return;

            var requested = new HashSet<string>(
                curvesToLoad.Where(name => !string.IsNullOrWhiteSpace(name)),
                StringComparer.OrdinalIgnoreCase);

            var curvesToRemove = logData.Curves.Keys
                .Where(curveName => !MatchesRequestedCurve(curveName, logData.CurveMetadata.TryGetValue(curveName, out var metadata) ? metadata : null, requested))
                .ToList();

            foreach (var curveName in curvesToRemove)
            {
                logData.Curves.Remove(curveName);
                logData.CurveMetadata.Remove(curveName);
            }
        }

        private static bool MatchesRequestedCurve(string curveName, LogCurveMetadata metadata, HashSet<string> requested)
        {
            foreach (var candidate in new[] { curveName, metadata?.Mnemonic, metadata?.DisplayName })
            {
                if (!string.IsNullOrWhiteSpace(candidate) && requested.Contains(candidate))
                    return true;
            }

            return false;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.DataLoaders.PWLS;
using SkiaSharp;

namespace Beep.OilandGas.Drawing.Rendering
{
    /// <summary>
    /// Builds standard petroleum log track layouts from available curve data.
    /// </summary>
    public static class LogTrackTemplates
    {
        private sealed record CurveScaleDefaults(double? MinValue, double? MaxValue, bool InvertScale = false, string ValueFormat = null);

        private static readonly HashSet<string> KnownPwlsProperties = new HashSet<string>(
            PwlsMnemonicMapper.GetAvailablePwlsProperties(),
            StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Creates a standard petrophysical track layout from the available curves.
        /// </summary>
        public static List<LogTrackDefinition> CreateStandardPetrophysicalTracks(LogData logData)
        {
            if (logData == null)
                return new List<LogTrackDefinition>();

            var tracks = new List<LogTrackDefinition>();

            AddIntervalTrackIfAny(tracks, "Zones", LogTrackKind.Zonation, logData.ZoneIntervals, 92f);
            AddIntervalTrackIfAny(tracks, "Lithology", LogTrackKind.Lithology, logData.LithologyIntervals, 68f);

            if (logData.Curves == null || logData.Curves.Count == 0)
                return tracks;

            var curveByProperty = BuildCurveLookup(logData);
            var usedCurves = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            AddTrackIfAny(tracks, usedCurves, logData, curveByProperty,
                "Gamma Ray",
                new[] { "GammaRay" },
                new Dictionary<string, SKColor>(StringComparer.OrdinalIgnoreCase)
                {
                    ["GammaRay"] = SKColors.ForestGreen
                },
                scaleDefaults: new Dictionary<string, CurveScaleDefaults>(StringComparer.OrdinalIgnoreCase)
                {
                    ["GammaRay"] = new CurveScaleDefaults(0.0, 150.0, ValueFormat: "F0")
                });

            AddTrackIfAny(tracks, usedCurves, logData, curveByProperty,
                "Resistivity",
                new[] { "DeepResistivity", "MediumResistivity", "ShallowResistivity", "MicroResistivity" },
                new Dictionary<string, SKColor>(StringComparer.OrdinalIgnoreCase)
                {
                    ["DeepResistivity"] = SKColors.Purple,
                    ["MediumResistivity"] = SKColors.DarkOrange,
                    ["ShallowResistivity"] = SKColors.SaddleBrown,
                    ["MicroResistivity"] = SKColors.SteelBlue
                },
                LogTrackScaleType.Logarithmic,
                new Dictionary<string, CurveScaleDefaults>(StringComparer.OrdinalIgnoreCase)
                {
                    ["DeepResistivity"] = new CurveScaleDefaults(0.2, 2000.0, ValueFormat: "0.###"),
                    ["MediumResistivity"] = new CurveScaleDefaults(0.2, 2000.0, ValueFormat: "0.###"),
                    ["ShallowResistivity"] = new CurveScaleDefaults(0.2, 2000.0, ValueFormat: "0.###"),
                    ["MicroResistivity"] = new CurveScaleDefaults(0.2, 2000.0, ValueFormat: "0.###")
                });

            AddTrackIfAny(tracks, usedCurves, logData, curveByProperty,
                "Density-Neutron",
                new[] { "BulkDensity", "NeutronPorosity", "DensityPorosity" },
                new Dictionary<string, SKColor>(StringComparer.OrdinalIgnoreCase)
                {
                    ["BulkDensity"] = SKColors.Red,
                    ["NeutronPorosity"] = SKColors.Blue,
                    ["DensityPorosity"] = SKColors.IndianRed
                },
                scaleDefaults: new Dictionary<string, CurveScaleDefaults>(StringComparer.OrdinalIgnoreCase)
                {
                    ["BulkDensity"] = new CurveScaleDefaults(1.95, 2.95, ValueFormat: "0.00"),
                    ["NeutronPorosity"] = new CurveScaleDefaults(-0.15, 0.45, true, "0.00"),
                    ["DensityPorosity"] = new CurveScaleDefaults(-0.15, 0.45, true, "0.00")
                });

            AddTrackIfAny(tracks, usedCurves, logData, curveByProperty,
                "Sonic",
                new[] { "AcousticSlowness" },
                new Dictionary<string, SKColor>(StringComparer.OrdinalIgnoreCase)
                {
                    ["AcousticSlowness"] = SKColors.Orange
                },
                scaleDefaults: new Dictionary<string, CurveScaleDefaults>(StringComparer.OrdinalIgnoreCase)
                {
                    ["AcousticSlowness"] = new CurveScaleDefaults(40.0, 140.0, ValueFormat: "F0")
                });

            AddTrackIfAny(tracks, usedCurves, logData, curveByProperty,
                "Caliper",
                new[] { "Caliper" },
                new Dictionary<string, SKColor>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Caliper"] = SKColors.DarkCyan
                },
                scaleDefaults: new Dictionary<string, CurveScaleDefaults>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Caliper"] = new CurveScaleDefaults(6.0, 16.0, ValueFormat: "0.0")
                });

            foreach (var curveName in logData.Curves.Keys)
            {
                if (usedCurves.Contains(curveName))
                    continue;

                tracks.Add(new LogTrackDefinition
                {
                    Name = GetCurveLabel(logData, curveName),
                    Curves = new List<LogTrackCurveDefinition>
                    {
                        new LogTrackCurveDefinition
                        {
                            CurveName = curveName,
                            DisplayName = GetCurveLabel(logData, curveName)
                        }
                    }
                });
            }

            return tracks;
        }

        private static void AddIntervalTrackIfAny(List<LogTrackDefinition> tracks, string name, LogTrackKind kind, List<LogIntervalData> intervals, float width)
        {
            var availableIntervals = (intervals ?? new List<LogIntervalData>())
                .Where(interval => interval != null && interval.BottomDepth > interval.TopDepth)
                .OrderBy(interval => interval.TopDepth)
                .ToList();

            if (availableIntervals.Count == 0)
                return;

            tracks.Add(new LogTrackDefinition
            {
                Kind = kind,
                Name = name,
                Width = width,
                Intervals = availableIntervals
            });
        }

        private static Dictionary<string, List<string>> BuildCurveLookup(LogData logData)
        {
            var lookup = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var curveName in logData.Curves.Keys)
            {
                string property = ResolvePwlsProperty(logData, curveName);
                if (!lookup.TryGetValue(property, out var curves))
                {
                    curves = new List<string>();
                    lookup[property] = curves;
                }

                curves.Add(curveName);
            }

            return lookup;
        }

        private static void AddTrackIfAny(
            List<LogTrackDefinition> tracks,
            HashSet<string> usedCurves,
            LogData logData,
            Dictionary<string, List<string>> curveByProperty,
            string trackName,
            IEnumerable<string> properties,
            Dictionary<string, SKColor> colors,
            LogTrackScaleType? scaleType = null,
            Dictionary<string, CurveScaleDefaults> scaleDefaults = null)
        {
            var trackCurves = new List<LogTrackCurveDefinition>();

            foreach (var property in properties)
            {
                if (!curveByProperty.TryGetValue(property, out var curves))
                    continue;

                foreach (var curveName in curves)
                {
                    if (!usedCurves.Add(curveName))
                        continue;

                    trackCurves.Add(new LogTrackCurveDefinition
                    {
                        CurveName = curveName,
                        DisplayName = GetCurveLabel(logData, curveName),
                        Color = colors.TryGetValue(property, out var color) ? color : null,
                        ScaleType = scaleType,
                        MinValue = scaleDefaults != null && scaleDefaults.TryGetValue(property, out var defaults) ? defaults.MinValue : null,
                        MaxValue = scaleDefaults != null && scaleDefaults.TryGetValue(property, out defaults) ? defaults.MaxValue : null,
                        InvertScale = scaleDefaults != null && scaleDefaults.TryGetValue(property, out defaults) && defaults.InvertScale,
                        ValueFormat = scaleDefaults != null && scaleDefaults.TryGetValue(property, out defaults) ? defaults.ValueFormat : null
                    });
                }
            }

            if (trackCurves.Count == 0)
                return;

            tracks.Add(new LogTrackDefinition
            {
                Name = trackName,
                Curves = trackCurves
            });
        }

        private static string ResolvePwlsProperty(LogData logData, string curveName)
        {
            if (logData.CurveMetadata.TryGetValue(curveName, out var metadata))
            {
                foreach (var candidate in new[] { metadata.DisplayName, metadata.Mnemonic, curveName })
                {
                    var resolved = ResolvePwlsProperty(candidate);
                    if (!string.Equals(resolved, candidate, StringComparison.OrdinalIgnoreCase) || KnownPwlsProperties.Contains(resolved))
                        return resolved;
                }
            }

            return ResolvePwlsProperty(curveName);
        }

        private static string ResolvePwlsProperty(string candidate)
        {
            if (string.IsNullOrWhiteSpace(candidate))
                return string.Empty;

            if (KnownPwlsProperties.Contains(candidate))
                return candidate;

            return PwlsMnemonicMapper.MapToPwlsProperty(candidate);
        }

        private static string GetCurveLabel(LogData logData, string curveName)
        {
            if (logData.CurveMetadata.TryGetValue(curveName, out var metadata))
            {
                return metadata.DisplayName ?? metadata.Mnemonic ?? curveName;
            }

            return curveName;
        }
    }
}
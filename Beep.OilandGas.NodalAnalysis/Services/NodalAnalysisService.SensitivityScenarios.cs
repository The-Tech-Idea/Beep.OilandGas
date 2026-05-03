using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.NodalAnalysis;

namespace Beep.OilandGas.NodalAnalysis.Services
{
    public partial class NodalAnalysisService
    {
        private static readonly IReadOnlyList<string> DefaultSensitivityParameters = new[]
        {
            "WellheadPressure",
            "TubingDiameter",
            "ReservoirPressure"
        };

        private static Dictionary<string, decimal> BuildSensitivityFactors(NodalAnalysisParameters baselineParameters, IEnumerable<string> requestedParameters)
        {
            var requested = requestedParameters
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var includeAll = requested.Count == 0;
            var factors = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            if (includeAll || requested.Contains("WellheadPressure"))
            {
                var baselineWhp = (decimal)baselineParameters.WellboreProperties.WellheadPressure;
                factors["WellheadPressure"] = Math.Round(0.10m * baselineWhp * 3m, 4);
            }

            if (includeAll || requested.Contains("TubingDiameter"))
            {
                var baselineTd = (decimal)baselineParameters.WellboreProperties.TubingDiameter;
                factors["TubingDiameter"] = Math.Round(0.05m * baselineTd * 15m, 4);
            }

            if (includeAll || requested.Contains("ReservoirPressure"))
            {
                var baselineRp = (decimal)baselineParameters.ReservoirProperties.ReservoirPressure;
                factors["ReservoirPressure"] = Math.Round(0.05m * baselineRp * 2m, 4);
            }

            return factors;
        }

        private static List<SensitivityScenarioResult> BuildScenarioBundles(Dictionary<string, decimal> factors)
        {
            var bundles = new List<(string Name, decimal Multiplier)>
            {
                ("Pessimistic", 0.80m),
                ("Base", 1.00m),
                ("Optimistic", 1.20m)
            };

            var results = new List<SensitivityScenarioResult>(bundles.Count);
            foreach (var bundle in bundles)
            {
                var scaled = factors.ToDictionary(k => k.Key, v => Math.Round(v.Value * bundle.Multiplier, 4), StringComparer.OrdinalIgnoreCase);
                var totalImpact = Math.Round(scaled.Values.Sum(v => Math.Abs(v)), 4);
                var dominant = scaled.Count == 0
                    ? "None"
                    : scaled.OrderByDescending(v => Math.Abs(v.Value)).First().Key;

                results.Add(new SensitivityScenarioResult
                {
                    ScenarioName = bundle.Name,
                    TotalImpact = totalImpact,
                    DominantParameter = dominant
                });
            }

            return results;
        }

        private static List<string> BuildSweepDefinition(IEnumerable<string> parameters)
        {
            var percentPoints = "-20,-10,0,10,20";
            return parameters
                .Select(p => $"{p}:{percentPoints}")
                .ToList();
        }
    }
}

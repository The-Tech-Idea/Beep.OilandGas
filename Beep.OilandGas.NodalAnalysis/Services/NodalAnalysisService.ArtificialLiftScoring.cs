using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.NodalAnalysis.Constants;

namespace Beep.OilandGas.NodalAnalysis.Services
{
    public partial class NodalAnalysisService
    {
        private static List<(string Name, decimal Score)> RankLiftCandidates(decimal targetProduction, decimal wellDepth, decimal waterCut)
        {
            var candidates = new List<(string Name, decimal Score)>
            {
                ("ESP - Electric Submersible Pump", ScoreEsp(targetProduction, wellDepth, waterCut)),
                ("Gas Lift - High Volume Production", ScoreGasLift(targetProduction, wellDepth, waterCut)),
                ("SuckerRod - Cost Effective for High Water Cut", ScoreSuckerRod(targetProduction, wellDepth, waterCut)),
                ("Plunger Lift - Intermittent Flow Wells", ScorePlungerLift(targetProduction, wellDepth, waterCut)),
                ("Hydraulic Jet Pump", ScoreHydraulicJet(targetProduction, wellDepth, waterCut)),
                ("Progressive Cavity Pump", ScorePcp(targetProduction, wellDepth, waterCut))
            };

            return candidates
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Name, StringComparer.Ordinal)
                .ToList();
        }

        private static Dictionary<string, decimal> BuildCandidateScoreMap(List<(string Name, decimal Score)> ranked)
        {
            return ranked.ToDictionary(x => x.Name, x => Math.Round(x.Score, 2));
        }

        private static List<string> BuildLiftScoreBreakdown(decimal targetProduction, decimal wellDepth, decimal waterCut)
        {
            return new List<string>
            {
                $"target_production_bpd={targetProduction:F2}",
                $"well_depth_ft={wellDepth:F2}",
                $"water_cut_fraction={waterCut:F3}",
                $"policy.deep_well_threshold_ft={NodalArtificialLiftPolicyConstants.DeepWellThresholdFt:F0}",
                $"policy.high_rate_threshold_bpd={NodalArtificialLiftPolicyConstants.HighRateThresholdBpd:F0}",
                $"policy.high_water_cut_threshold={NodalArtificialLiftPolicyConstants.HighWaterCutThreshold:F2}"
            };
        }

        private static decimal ScoreEsp(decimal targetProduction, decimal wellDepth, decimal waterCut)
        {
            var score = 45m;
            if (targetProduction >= NodalArtificialLiftPolicyConstants.EspPreferredRateBpd) score += 22m;
            if (wellDepth >= NodalArtificialLiftPolicyConstants.DeepWellThresholdFt) score += 18m;
            score -= Math.Min(18m, Math.Max(0m, waterCut - 0.40m) * 40m);
            return score;
        }

        private static decimal ScoreGasLift(decimal targetProduction, decimal wellDepth, decimal waterCut)
        {
            var score = 42m;
            if (targetProduction >= NodalArtificialLiftPolicyConstants.HighRateThresholdBpd) score += 24m;
            if (wellDepth >= 7000m) score += 10m;
            score -= Math.Min(10m, waterCut * 8m);
            return score;
        }

        private static decimal ScoreSuckerRod(decimal targetProduction, decimal wellDepth, decimal waterCut)
        {
            var score = 40m;
            if (targetProduction <= 300m) score += 16m;
            if (waterCut >= NodalArtificialLiftPolicyConstants.HighWaterCutThreshold) score += 15m;
            score -= Math.Min(22m, Math.Max(0m, wellDepth - 8500m) / 250m);
            return score;
        }

        private static decimal ScorePlungerLift(decimal targetProduction, decimal wellDepth, decimal waterCut)
        {
            var score = 36m;
            if (targetProduction < 450m) score += 14m;
            if (waterCut < 0.35m) score += 8m;
            score -= Math.Min(12m, Math.Max(0m, wellDepth - 9000m) / 500m);
            return score;
        }

        private static decimal ScoreHydraulicJet(decimal targetProduction, decimal wellDepth, decimal waterCut)
        {
            var score = 34m;
            if (targetProduction > 700m) score += 10m;
            if (wellDepth > 6000m) score += 8m;
            score += Math.Min(6m, waterCut * 5m);
            return score;
        }

        private static decimal ScorePcp(decimal targetProduction, decimal wellDepth, decimal waterCut)
        {
            var score = 32m;
            if (wellDepth <= NodalArtificialLiftPolicyConstants.MaxDepthForPcpFt) score += 14m;
            if (waterCut >= NodalArtificialLiftPolicyConstants.HighWaterCutThreshold) score += 10m;
            score -= Math.Min(10m, Math.Max(0m, targetProduction - 500m) / 100m);
            return score;
        }
    }
}

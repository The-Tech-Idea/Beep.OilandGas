using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.PumpPerformance.Calculations;
using Beep.OilandGas.PumpPerformance.Exceptions;
using Beep.OilandGas.PumpPerformance.Validation;

namespace Beep.OilandGas.PumpPerformance.SystemAnalysis
{
    /// <summary>
    /// Represents a pump candidate for selection.
    /// </summary>
    public class PumpCandidate
    {
        /// <summary>
        /// Gets or sets the pump identifier.
        /// </summary>
        public string PumpId { get; set; }

        /// <summary>
        /// Gets or sets the H-Q curve.
        /// </summary>
        public List<HeadQuantityPoint> Curve { get; set; }

        /// <summary>
        /// Gets or sets the pump cost.
        /// </summary>
        public double Cost { get; set; }

        /// <summary>
        /// Gets or sets the pump efficiency at BEP.
        /// </summary>
        public double BestEfficiency { get; set; }

        public PumpCandidate()
        {
            Curve = new List<HeadQuantityPoint>();
        }
    }

    /// <summary>
        /// Represents pump selection result.
        /// </summary>
    public class PumpSelectionResult
    {
        /// <summary>
        /// Gets or sets the selected pump candidate.
        /// </summary>
        public PumpCandidate SelectedPump { get; set; }

        /// <summary>
        /// Gets or sets the operating point.
        /// </summary>
        public (double flowRate, double head)? OperatingPoint { get; set; }

        /// <summary>
        /// Gets or sets the efficiency at operating point.
        /// </summary>
        public double OperatingEfficiency { get; set; }

        /// <summary>
        /// Gets or sets the selection score (higher is better).
        /// </summary>
        public double SelectionScore { get; set; }

        /// <summary>
        /// Gets or sets the reason for selection.
        /// </summary>
        public string SelectionReason { get; set; }
    }

    /// <summary>
    /// Provides pump selection and optimization algorithms.
    /// </summary>
    public static class PumpSelection
    {
        /// <summary>
        /// Selects the best pump from candidates based on operating point and efficiency.
        /// </summary>
        /// <param name="candidates">List of pump candidates.</param>
        /// <param name="systemCurve">System resistance curve.</param>
        /// <param name="preferEfficiency">If true, prioritize efficiency over cost (default: true).</param>
        /// <returns>Pump selection result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when input is null.</exception>
        /// <exception cref="InvalidInputException">Thrown when input is invalid.</exception>
        public static PumpSelectionResult SelectBestPump(
            List<PumpCandidate> candidates,
            List<SystemCurvePoint> systemCurve,
            bool preferEfficiency = true)
        {
            if (candidates == null || candidates.Count == 0)
                throw new ArgumentNullException(nameof(candidates), 
                    "Candidates list cannot be null or empty.");

            if (systemCurve == null || systemCurve.Count == 0)
                throw new ArgumentNullException(nameof(systemCurve), 
                    "System curve cannot be null or empty.");

            PumpSelectionResult bestResult = null;
            double bestScore = double.MinValue;

            foreach (var candidate in candidates)
            {
                if (candidate.Curve == null || candidate.Curve.Count == 0)
                    continue;

                var operatingPoint = SystemCurveCalculations.FindOperatingPoint(
                    candidate.Curve, systemCurve);

                if (!operatingPoint.HasValue)
                    continue;

                // Find efficiency at operating point
                double efficiency = 0;
                var curvePoint = candidate.Curve.FirstOrDefault(p =>
                    Math.Abs(p.FlowRate - operatingPoint.Value.flowRate) < 0.1);
                if (curvePoint != null)
                {
                    efficiency = curvePoint.Efficiency;
                }
                else
                {
                    // Use BEP efficiency as approximation
                    efficiency = candidate.BestEfficiency;
                }

                // Calculate selection score
                double score = CalculateSelectionScore(
                    candidate, operatingPoint.Value, efficiency, preferEfficiency);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestResult = new PumpSelectionResult
                    {
                        SelectedPump = candidate,
                        OperatingPoint = operatingPoint,
                        OperatingEfficiency = efficiency,
                        SelectionScore = score,
                        SelectionReason = preferEfficiency 
                            ? "Best efficiency at operating point" 
                            : "Best cost-effectiveness"
                    };
                }
            }

            if (bestResult == null)
                throw new InvalidInputException("Selection", 
                    "No suitable pump found for the given system curve.");

            return bestResult;
        }

        /// <summary>
        /// Calculates selection score for a pump candidate.
        /// </summary>
        private static double CalculateSelectionScore(
            PumpCandidate candidate,
            (double flowRate, double head) operatingPoint,
            double efficiency,
            bool preferEfficiency)
        {
            // Normalize factors (0 to 1 scale)
            double efficiencyScore = efficiency; // Already 0-1
            double costScore = candidate.Cost > 0 ? 1.0 / (1.0 + candidate.Cost / 10000.0) : 1.0;

            if (preferEfficiency)
            {
                // Weight: 70% efficiency, 30% cost
                return 0.7 * efficiencyScore + 0.3 * costScore;
            }
            else
            {
                // Weight: 40% efficiency, 60% cost
                return 0.4 * efficiencyScore + 0.6 * costScore;
            }
        }

        /// <summary>
        /// Finds pumps that can meet the required flow rate and head.
        /// </summary>
        /// <param name="candidates">List of pump candidates.</param>
        /// <param name="requiredFlowRate">Required flow rate in GPM.</param>
        /// <param name="requiredHead">Required head in feet.</param>
        /// <param name="tolerance">Tolerance for matching (default: 0.1 = 10%).</param>
        /// <returns>List of suitable pumps.</returns>
        public static List<PumpCandidate> FindSuitablePumps(
            List<PumpCandidate> candidates,
            double requiredFlowRate,
            double requiredHead,
            double tolerance = 0.1)
        {
            PumpDataValidator.ValidateFlowRate(requiredFlowRate, nameof(requiredFlowRate));
            PumpDataValidator.ValidateHead(requiredHead, nameof(requiredHead));

            if (candidates == null)
                return new List<PumpCandidate>();

            var suitablePumps = new List<PumpCandidate>();

            foreach (var candidate in candidates)
            {
                if (candidate.Curve == null || candidate.Curve.Count == 0)
                    continue;

                // Check if pump can provide required head at required flow
                double headAtFlow = HeadQuantityCalculations.InterpolateHead(
                    candidate.Curve, requiredFlowRate);

                double headRatio = headAtFlow / requiredHead;

                // Check if head is sufficient (within tolerance)
                if (headRatio >= (1.0 - tolerance) && headRatio <= (1.0 + tolerance * 2))
                {
                    suitablePumps.Add(candidate);
                }
            }

            return suitablePumps;
        }

        /// <summary>
        /// Calculates cost-effectiveness ratio (efficiency per unit cost).
        /// </summary>
        /// <param name="candidate">Pump candidate.</param>
        /// <param name="operatingEfficiency">Efficiency at operating point.</param>
        /// <returns>Cost-effectiveness ratio (higher is better).</returns>
        public static double CalculateCostEffectiveness(
            PumpCandidate candidate,
            double operatingEfficiency)
        {
            if (candidate.Cost <= 0)
                return operatingEfficiency; // If cost is zero, return efficiency

            return operatingEfficiency / candidate.Cost;
        }

        /// <summary>
        /// Ranks pumps by efficiency at operating point.
        /// </summary>
        /// <param name="candidates">List of pump candidates.</param>
        /// <param name="systemCurve">System resistance curve.</param>
        /// <returns>List of candidates sorted by efficiency (descending).</returns>
        public static List<(PumpCandidate candidate, double efficiency)> RankByEfficiency(
            List<PumpCandidate> candidates,
            List<SystemCurvePoint> systemCurve)
        {
            var rankings = new List<(PumpCandidate candidate, double efficiency)>();

            foreach (var candidate in candidates)
            {
                if (candidate.Curve == null || candidate.Curve.Count == 0)
                    continue;

                var operatingPoint = SystemCurveCalculations.FindOperatingPoint(
                    candidate.Curve, systemCurve);

                if (!operatingPoint.HasValue)
                    continue;

                double efficiency = candidate.BestEfficiency;
                var curvePoint = candidate.Curve.FirstOrDefault(p =>
                    Math.Abs(p.FlowRate - operatingPoint.Value.flowRate) < 0.1);
                if (curvePoint != null)
                {
                    efficiency = curvePoint.Efficiency;
                }

                rankings.Add((candidate, efficiency));
            }

            return rankings.OrderByDescending(r => r.efficiency).ToList();
        }
    }
}


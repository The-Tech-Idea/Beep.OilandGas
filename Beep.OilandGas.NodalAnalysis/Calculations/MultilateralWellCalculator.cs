using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.NodalAnalysis;

namespace Beep.OilandGas.NodalAnalysis.Calculations
{
    /// <summary>
    /// Provides multilateral well deliverability calculations.
    /// </summary>
    public static class MultilateralWellCalculator
    {
        /// <summary>
        /// Calculates multilateral oil well deliverability.
        /// </summary>
        /// <param name="wellProperties">Multilateral well properties.</param>
        /// <param name="bottomholePressure">Bottomhole pressure at junction in psi.</param>
        /// <returns>Multilateral deliverability results.</returns>
        public static MultilateralDeliverabilityResult CalculateOilWellDeliverability(
            MultilateralWellProperties wellProperties,
            double bottomholePressure)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            if (wellProperties.LateralBranches == null || wellProperties.LateralBranches.Count == 0)
                throw new ArgumentException("Multilateral well must have at least one lateral branch.", nameof(wellProperties));

            var result = new MultilateralDeliverabilityResult
            {
                JunctionBottomholePressure = bottomholePressure
            };

            // Calculate production from each lateral branch
            double totalProduction = 0.0;

            foreach (var branch in wellProperties.LateralBranches)
            {
                // Calculate individual branch production using Vogel IPR
                double branchProduction = CalculateBranchOilProduction(
                    branch, bottomholePressure);

                result.BranchProductionRates[branch.Name ?? $"Branch_{wellProperties.LateralBranches.IndexOf(branch)}"] = branchProduction;
                totalProduction += branchProduction;
            }

            result.TotalProductionRate = totalProduction;

            return result;
        }

        /// <summary>
        /// Calculates multilateral gas well deliverability using C-n IPR method.
        /// </summary>
        /// <param name="wellProperties">Multilateral well properties.</param>
        /// <param name="bottomholePressure">Bottomhole pressure at junction in psi.</param>
        /// <param name="cValue">C value for C-n IPR equation.</param>
        /// <param name="nValue">n value (deliverability exponent) for C-n IPR equation.</param>
        /// <returns>Multilateral deliverability results.</returns>
        public static MultilateralDeliverabilityResult CalculateGasWellDeliverabilityCN(
            MultilateralWellProperties wellProperties,
            double bottomholePressure,
            double cValue,
            double nValue)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            if (wellProperties.LateralBranches == null || wellProperties.LateralBranches.Count == 0)
                throw new ArgumentException("Multilateral well must have at least one lateral branch.", nameof(wellProperties));

            var result = new MultilateralDeliverabilityResult
            {
                JunctionBottomholePressure = bottomholePressure
            };

            // Calculate production from each lateral branch using C-n IPR
            double totalProduction = 0.0;

            foreach (var branch in wellProperties.LateralBranches)
            {
                // C-n IPR: q = C * (Pr² - Pwf²)^n
                double pressureDifference = branch.ReservoirPressure * branch.ReservoirPressure - 
                                          bottomholePressure * bottomholePressure;

                if (pressureDifference < 0)
                    pressureDifference = 0;

                double branchProduction = cValue * Math.Pow(pressureDifference, nValue);

                // Adjust for branch productivity
                double branchC = CalculateBranchCValue(branch);
                branchProduction = branchC * Math.Pow(pressureDifference, nValue);

                result.BranchProductionRates[branch.Name ?? $"Branch_{wellProperties.LateralBranches.IndexOf(branch)}"] = branchProduction;
                totalProduction += branchProduction;
            }

            result.TotalProductionRate = totalProduction;

            return result;
        }

        /// <summary>
        /// Calculates multilateral gas well deliverability using radial-flow IPR method.
        /// </summary>
        /// <param name="wellProperties">Multilateral well properties.</param>
        /// <param name="bottomholePressure">Bottomhole pressure at junction in psi.</param>
        /// <param name="gasViscosity">Gas viscosity in cp.</param>
        /// <param name="gasZFactor">Gas Z-factor.</param>
        /// <param name="temperature">Reservoir temperature in Rankine.</param>
        /// <returns>Multilateral deliverability results.</returns>
        public static MultilateralDeliverabilityResult CalculateGasWellDeliverabilityRadialFlow(
            MultilateralWellProperties wellProperties,
            double bottomholePressure,
            double gasViscosity = 0.02,
            double gasZFactor = 0.9,
            double temperature = 580.0)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            if (wellProperties.LateralBranches == null || wellProperties.LateralBranches.Count == 0)
                throw new ArgumentException("Multilateral well must have at least one lateral branch.", nameof(wellProperties));

            var result = new MultilateralDeliverabilityResult
            {
                JunctionBottomholePressure = bottomholePressure
            };

            // Calculate production from each lateral branch using radial-flow IPR
            double totalProduction = 0.0;

            foreach (var branch in wellProperties.LateralBranches)
            {
                // Radial-flow IPR for gas: q = (kh / (1422 * μ * Z * T)) * (Pr² - Pwf²) / (ln(re/rw) + S)
                double kh = branch.Permeability * branch.FormationThickness; // md-ft
                double re_rw = branch.DrainageRadius / branch.WellboreRadius;
                double denominator = Math.Log(re_rw) + branch.SkinFactor;

                if (denominator <= 0)
                    denominator = 1.0;

                double pressureDifference = branch.ReservoirPressure * branch.ReservoirPressure - 
                                          bottomholePressure * bottomholePressure;

                if (pressureDifference < 0)
                    pressureDifference = 0;

                double branchProduction = (kh / (1422.0 * gasViscosity * gasZFactor * temperature)) * 
                                        (pressureDifference / denominator);

                result.BranchProductionRates[branch.Name ?? $"Branch_{wellProperties.LateralBranches.IndexOf(branch)}"] = branchProduction;
                totalProduction += branchProduction;
            }

            result.TotalProductionRate = totalProduction;

            return result;
        }

        /// <summary>
        /// Generates IPR curve for multilateral oil well.
        /// </summary>
        public static List<IPRPoint> GenerateMultilateralOilIPR(
            MultilateralWellProperties wellProperties,
            int points = 50,
            double maxFlowRate = 10000)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            var iprCurve = new List<IPRPoint>();

            double minPressure = 0.0;
            double maxPressure = wellProperties.LateralBranches.Max(b => b.ReservoirPressure);

            for (int i = 0; i <= points; i++)
            {
                double bottomholePressure = minPressure + (maxPressure - minPressure) * i / points;

                var deliverability = CalculateOilWellDeliverability(wellProperties, bottomholePressure);

                iprCurve.Add(new IPRPoint(deliverability.TotalProductionRate, bottomholePressure));
            }

            return iprCurve;
        }

        /// <summary>
        /// Generates IPR curve for multilateral gas well (C-n method).
        /// </summary>
        public static List<IPRPoint> GenerateMultilateralGasIPRCN(
            MultilateralWellProperties wellProperties,
            double cValue,
            double nValue,
            int points = 50)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            var iprCurve = new List<IPRPoint>();

            double minPressure = 0.0;
            double maxPressure = wellProperties.LateralBranches.Max(b => b.ReservoirPressure);

            for (int i = 0; i <= points; i++)
            {
                double bottomholePressure = minPressure + (maxPressure - minPressure) * i / points;

                var deliverability = CalculateGasWellDeliverabilityCN(wellProperties, bottomholePressure, cValue, nValue);

                iprCurve.Add(new IPRPoint(deliverability.TotalProductionRate, bottomholePressure));
            }

            return iprCurve;
        }

        /// <summary>
        /// Generates IPR curve for multilateral gas well (radial-flow method).
        /// </summary>
        public static List<IPRPoint> GenerateMultilateralGasIPRRadialFlow(
            MultilateralWellProperties wellProperties,
            double gasViscosity = 0.02,
            double gasZFactor = 0.9,
            double temperature = 580.0,
            int points = 50)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            var iprCurve = new List<IPRPoint>();

            double minPressure = 0.0;
            double maxPressure = wellProperties.LateralBranches.Max(b => b.ReservoirPressure);

            for (int i = 0; i <= points; i++)
            {
                double bottomholePressure = minPressure + (maxPressure - minPressure) * i / points;

                var deliverability = CalculateGasWellDeliverabilityRadialFlow(
                    wellProperties, bottomholePressure, gasViscosity, gasZFactor, temperature);

                iprCurve.Add(new IPRPoint(deliverability.TotalProductionRate, bottomholePressure));
            }

            return iprCurve;
        }

        // Helper methods

        /// <summary>
        /// Calculates oil production from a single branch using Vogel IPR.
        /// </summary>
        private static double CalculateBranchOilProduction(LateralBranch branch, double bottomholePressure)
        {
            if (branch.ReservoirPressure <= 0)
                return 0.0;

            // Vogel equation: q = q_max * [1 - 0.2 * (Pwf/Pr) - 0.8 * (Pwf/Pr)²]
            double qMax = branch.ProductivityIndex * branch.ReservoirPressure / 1.8;

            double pwfRatio = bottomholePressure / branch.ReservoirPressure;
            if (pwfRatio >= 1.0)
                return 0.0;

            double production = qMax * (1.0 - 0.2 * pwfRatio - 0.8 * pwfRatio * pwfRatio);

            return Math.Max(0.0, production);
        }

        /// <summary>
        /// Calculates C value for a branch based on its properties.
        /// </summary>
        private static double CalculateBranchCValue(LateralBranch branch)
        {
            // C value calculation based on branch properties
            // Simplified: C = PI * Pr / (Pr²)^n
            // For n = 0.5 (typical), C = PI * Pr / Pr = PI

            double nValue = 0.5; // Typical value
            double cValue = branch.ProductivityIndex * branch.ReservoirPressure / 
                          Math.Pow(branch.ReservoirPressure * branch.ReservoirPressure, nValue);

            return cValue;
        }
    }
}


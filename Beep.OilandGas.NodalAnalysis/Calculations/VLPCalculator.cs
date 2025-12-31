using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.NodalAnalysis;

namespace Beep.OilandGas.NodalAnalysis.Calculations
{
    /// <summary>
    /// Provides VLP (Vertical Lift Performance) calculation methods.
    /// </summary>
    public static class VLPCalculator
    {
        /// <summary>
        /// Generates VLP curve using Hagedorn-Brown correlation.
        /// </summary>
        public static List<VLPPoint> GenerateVLP(WellboreProperties wellbore, double[] flowRates)
        {
            if (wellbore == null)
                throw new ArgumentNullException(nameof(wellbore));

            if (flowRates == null || flowRates.Length == 0)
                throw new ArgumentException("Flow rates array cannot be null or empty.", nameof(flowRates));

            var vlp = new List<VLPPoint>();

            foreach (var flowRate in flowRates)
            {
                double requiredBHP = CalculateHagedornBrown(wellbore, flowRate);
                vlp.Add(new VLPPoint(flowRate, requiredBHP));
            }

            return vlp;
        }

        /// <summary>
        /// Calculates required bottomhole pressure using Hagedorn-Brown method.
        /// </summary>
        private static double CalculateHagedornBrown(WellboreProperties wellbore, double flowRate)
        {
            // Simplified Hagedorn-Brown calculation
            // Full implementation would include:
            // - Multiphase flow regime determination
            // - Holdup calculations
            // - Friction factor calculations
            // - Temperature profile

            double tubingArea = Math.PI * Math.Pow(wellbore.TubingDiameter / 12.0 / 2.0, 2); // ft²
            double velocity = (flowRate * 5.615) / (86400.0 * tubingArea); // ft/s (assuming oil)

            // Hydrostatic pressure
            double oilDensity = 141.5 / (131.5 + wellbore.OilGravity); // specific gravity
            double hydrostatic = oilDensity * 0.433 * wellbore.TubingLength; // psi

            // Friction pressure (simplified)
            double reynolds = CalculateReynoldsNumber(wellbore, flowRate, velocity);
            double frictionFactor = CalculateFrictionFactor(reynolds, wellbore.TubingRoughness, wellbore.TubingDiameter);
            double friction = frictionFactor * (wellbore.TubingLength / (wellbore.TubingDiameter / 12.0)) *
                (oilDensity * 62.4) * (velocity * velocity) / (2 * 32.174) / 144.0; // psi

            // Acceleration pressure (usually small, simplified)
            double acceleration = 0.0;

            double requiredBHP = wellbore.WellheadPressure + hydrostatic + friction + acceleration;

            return requiredBHP;
        }

        /// <summary>
        /// Calculates Reynolds number.
        /// </summary>
        private static double CalculateReynoldsNumber(WellboreProperties wellbore, double flowRate, double velocity)
        {
            double oilDensity = 141.5 / (131.5 + wellbore.OilGravity) * 62.4; // lb/ft³
            double oilViscosity = 1.5 * 0.000672; // lb-s/ft² (converted from cp)
            double diameter = wellbore.TubingDiameter / 12.0; // feet

            return (oilDensity * velocity * diameter) / oilViscosity;
        }

        /// <summary>
        /// Calculates friction factor using Colebrook equation (simplified).
        /// </summary>
        private static double CalculateFrictionFactor(double reynolds, double roughness, double diameter)
        {
            if (reynolds < 2100)
            {
                // Laminar flow
                return 64.0 / reynolds;
            }
            else
            {
                // Turbulent flow - simplified Swamee-Jain
                double relativeRoughness = roughness / (diameter / 12.0);
                double term1 = relativeRoughness / 3.7;
                double term2 = 5.74 / Math.Pow(reynolds, 0.9);
                double term3 = Math.Log10(term1 + term2);
                return 0.25 / (term3 * term3);
            }
        }

        /// <summary>
        /// Generates VLP curve using Beggs-Brill correlation.
        /// </summary>
        public static List<VLPPoint> GenerateVLPBeggsBrill(WellboreProperties wellbore, double[] flowRates)
        {
            if (wellbore == null)
                throw new ArgumentNullException(nameof(wellbore));

            if (flowRates == null || flowRates.Length == 0)
                throw new ArgumentException("Flow rates array cannot be null or empty.", nameof(flowRates));

            var vlp = new List<VLPPoint>();

            foreach (var flowRate in flowRates)
            {
                // Simplified Beggs-Brill (full implementation would be more complex)
                double requiredBHP = CalculateHagedornBrown(wellbore, flowRate); // Using H-B as approximation
                vlp.Add(new VLPPoint(flowRate, requiredBHP));
            }

            return vlp;
        }

        /// <summary>
        /// Correlation types for VLP calculations.
        /// </summary>
        public enum CorrelationType
        {
            HagedornBrown,
            BeggsBrill,
            Gray,
            DunsRos
        }

        /// <summary>
        /// Generates VLP curve with specified parameters (wrapper for DataFlowService).
        /// </summary>
        public static List<VLPPoint> GenerateVLPCurve(
            decimal tubingDiameter,
            decimal tubingLength,
            decimal wellheadPressure,
            decimal waterCut,
            decimal gasOilRatio,
            decimal oilGravity,
            decimal gasSpecificGravity,
            decimal wellheadTemperature,
            decimal bottomholeTemperature,
            decimal tubingRoughness,
            int points,
            CorrelationType correlationType = CorrelationType.HagedornBrown)
        {
            // Create wellbore properties from parameters
            var wellbore = new WellboreProperties
            {
                TubingDiameter = (double)tubingDiameter,
                TubingLength = (double)tubingLength,
                WellheadPressure = (double)wellheadPressure,
                WaterCut = (double)waterCut,
                GasOilRatio = (double)gasOilRatio,
                OilGravity = (double)oilGravity,
                GasSpecificGravity = (double)gasSpecificGravity,
                WellheadTemperature = (double)wellheadTemperature,
                BottomholeTemperature = (double)bottomholeTemperature,
                TubingRoughness = (double)tubingRoughness
            };

            // Generate flow rates
            var flowRates = new double[points + 1];
            double maxFlowRate = 5000.0; // bbl/day
            for (int i = 0; i <= points; i++)
            {
                flowRates[i] = maxFlowRate * i / points;
            }

            // Use specified correlation
            return correlationType switch
            {
                CorrelationType.HagedornBrown => GenerateVLP(wellbore, flowRates),
                CorrelationType.BeggsBrill => GenerateVLPBeggsBrill(wellbore, flowRates),
                _ => GenerateVLP(wellbore, flowRates) // Default to Hagedorn-Brown
            };
        }
    }
}


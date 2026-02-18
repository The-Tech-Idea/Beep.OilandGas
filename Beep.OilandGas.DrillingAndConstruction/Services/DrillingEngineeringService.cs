using System;
using System.Collections.Generic;
using Beep.OilandGas.DrillingAndConstruction.Calculations;

namespace Beep.OilandGas.DrillingAndConstruction.Services
{
    public class DrillingEngineeringService
    {
        public double CalculateStandpipePressure(
            double flowRateGpm, 
            double mudDensityPpg, 
            List<PipeSection> stringSections, 
            double bitNozzleAreaSqIn)
        {
            double totalLoss = 0;

            // Surface lines (simplified 100 psi or calc)
            totalLoss += 50; 

            foreach (var sec in stringSections)
            {
                totalLoss += HydraulicsCalculator.CalculatePressureLoss(
                    sec.Length, flowRateGpm, sec.ID, 0, mudDensityPpg, sec.PV, sec.YP);
            }

            // Annulus (simplified one section for demo, typically multi-section)
            // Assuming 8.5" hole for 5" DP
            totalLoss += HydraulicsCalculator.CalculatePressureLoss(
                stringSections[0].Length * stringSections.Count, flowRateGpm, 8.5, 5.0, mudDensityPpg, 15, 12);

            // Bit
            totalLoss += HydraulicsCalculator.CalculateBitPressureDrop(flowRateGpm, mudDensityPpg, bitNozzleAreaSqIn);

            return totalLoss;
        }

        public TORQUE_DRAG_RESULT CalculateTorqueAndDrag(
            List<SURVEY_POINT> survey,
            DRILL_STRING_COMPONENT comp,
            double frictionFactor,
            double mudDensity,
            string operation)
        {
            return TorqueDragCalculator.CalculateSurfaceLoads(survey, comp, frictionFactor, mudDensity, operation);
        }
    }

    public class PipeSection
    {
        public double Length { get; set; }
        public double ID { get; set; }
        public double OD { get; set; }
        public double PV { get; set; }
        public double YP { get; set; }
    }
}

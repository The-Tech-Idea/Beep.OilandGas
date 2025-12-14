using System;
using Beep.OilandGas.PumpPerformance.Constants;
using Beep.OilandGas.PumpPerformance.Exceptions;
using Beep.OilandGas.PumpPerformance.Validation;
using static Beep.OilandGas.PumpPerformance.Constants.PumpConstants;

namespace Beep.OilandGas.PumpPerformance.PumpTypes
{
    /// <summary>
    /// Provides specialized calculations for jet pumps.
    /// </summary>
    public static class JetPump
    {
        /// <summary>
        /// Calculates jet pump flow rate.
        /// Formula: Q_production = Q_power * (H_power / H_lift) * η_jet
        /// Where Q_power = power fluid flow rate, H_power = power fluid head, H_lift = total lift
        /// </summary>
        /// <param name="powerFluidFlowRate">Power fluid flow rate in GPM.</param>
        /// <param name="powerFluidHead">Power fluid head in feet.</param>
        /// <param name="totalLift">Total lift (suction + discharge) in feet.</param>
        /// <param name="jetEfficiency">Jet pump efficiency (default: 0.25 for typical jet pumps).</param>
        /// <returns>Production flow rate in GPM.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateProductionFlowRate(
            double powerFluidFlowRate,
            double powerFluidHead,
            double totalLift,
            double jetEfficiency = 0.25)
        {
            PumpDataValidator.ValidateFlowRate(powerFluidFlowRate, nameof(powerFluidFlowRate));
            PumpDataValidator.ValidateHead(powerFluidHead, nameof(powerFluidHead));
            PumpDataValidator.ValidateHead(totalLift, nameof(totalLift));
            PumpDataValidator.ValidateEfficiency(jetEfficiency, nameof(jetEfficiency));

            if (Math.Abs(totalLift) < Epsilon)
                throw new InvalidInputException(nameof(totalLift), 
                    "Total lift cannot be zero for production flow rate calculation.");

            if (Math.Abs(powerFluidHead) < Epsilon)
                throw new InvalidInputException(nameof(powerFluidHead), 
                    "Power fluid head cannot be zero.");

            return powerFluidFlowRate * (powerFluidHead / totalLift) * jetEfficiency;
        }

        /// <summary>
        /// Calculates required power fluid flow rate.
        /// Formula: Q_power = Q_production * (H_lift / H_power) / η_jet
        /// </summary>
        /// <param name="productionFlowRate">Required production flow rate in GPM.</param>
        /// <param name="totalLift">Total lift in feet.</param>
        /// <param name="powerFluidHead">Power fluid head in feet.</param>
        /// <param name="jetEfficiency">Jet pump efficiency (default: 0.25).</param>
        /// <returns>Required power fluid flow rate in GPM.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateRequiredPowerFluidFlowRate(
            double productionFlowRate,
            double totalLift,
            double powerFluidHead,
            double jetEfficiency = 0.25)
        {
            PumpDataValidator.ValidateFlowRate(productionFlowRate, nameof(productionFlowRate));
            PumpDataValidator.ValidateHead(totalLift, nameof(totalLift));
            PumpDataValidator.ValidateHead(powerFluidHead, nameof(powerFluidHead));
            PumpDataValidator.ValidateEfficiency(jetEfficiency, nameof(jetEfficiency));

            if (Math.Abs(powerFluidHead) < Epsilon)
                throw new InvalidInputException(nameof(powerFluidHead), 
                    "Power fluid head cannot be zero.");

            if (Math.Abs(jetEfficiency) < Epsilon)
                throw new InvalidInputException(nameof(jetEfficiency), 
                    "Jet efficiency cannot be zero.");

            return productionFlowRate * (totalLift / powerFluidHead) / jetEfficiency;
        }

        /// <summary>
        /// Calculates nozzle area for jet pump.
        /// Formula: A_nozzle = Q_power / (C_d * √(2 * g * H_power))
        /// Where C_d = discharge coefficient (default: 0.95)
        /// </summary>
        /// <param name="powerFluidFlowRate">Power fluid flow rate in GPM.</param>
        /// <param name="powerFluidHead">Power fluid head in feet.</param>
        /// <param name="dischargeCoefficient">Discharge coefficient (default: 0.95).</param>
        /// <returns>Nozzle area in square inches.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateNozzleArea(
            double powerFluidFlowRate,
            double powerFluidHead,
            double dischargeCoefficient = 0.95)
        {
            PumpDataValidator.ValidateFlowRate(powerFluidFlowRate, nameof(powerFluidFlowRate));
            PumpDataValidator.ValidateHead(powerFluidHead, nameof(powerFluidHead));

            if (dischargeCoefficient <= 0 || dischargeCoefficient > 1)
                throw new InvalidInputException(nameof(dischargeCoefficient), 
                    "Discharge coefficient must be between 0 and 1.");

            if (Math.Abs(powerFluidHead) < Epsilon)
                throw new InvalidInputException(nameof(powerFluidHead), 
                    "Power fluid head cannot be zero for nozzle area calculation.");

            // Convert GPM to ft³/s: 1 GPM = 0.002228 ft³/s
            double flowRateFt3PerSec = powerFluidFlowRate * 0.002228;
            double velocity = dischargeCoefficient * Math.Sqrt(2.0 * GravityFeetPerSecondSquared * powerFluidHead);
            
            if (Math.Abs(velocity) < Epsilon)
                throw new InvalidInputException(nameof(powerFluidHead), 
                    "Calculated velocity is zero. Check power fluid head.");

            double areaFt2 = flowRateFt3PerSec / velocity;
            return areaFt2 * 144.0; // Convert to square inches
        }

        /// <summary>
        /// Calculates nozzle diameter from area.
        /// Formula: D = √(4 * A / π)
        /// </summary>
        /// <param name="nozzleArea">Nozzle area in square inches.</param>
        /// <returns>Nozzle diameter in inches.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateNozzleDiameter(double nozzleArea)
        {
            if (nozzleArea <= 0)
                throw new InvalidInputException(nameof(nozzleArea), 
                    "Nozzle area must be positive.");

            return Math.Sqrt(4.0 * nozzleArea / Math.PI);
        }

        /// <summary>
        /// Calculates throat area for jet pump (typically 2-4 times nozzle area).
        /// </summary>
        /// <param name="nozzleArea">Nozzle area in square inches.</param>
        /// <param name="areaRatio">Throat to nozzle area ratio (default: 3.0).</param>
        /// <returns>Throat area in square inches.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateThroatArea(
            double nozzleArea,
            double areaRatio = 3.0)
        {
            if (nozzleArea <= 0)
                throw new InvalidInputException(nameof(nozzleArea), 
                    "Nozzle area must be positive.");

            if (areaRatio <= 0)
                throw new InvalidInputException(nameof(areaRatio), 
                    "Area ratio must be positive.");

            return nozzleArea * areaRatio;
        }

        /// <summary>
        /// Calculates throat diameter from area.
        /// </summary>
        /// <param name="throatArea">Throat area in square inches.</param>
        /// <returns>Throat diameter in inches.</returns>
        public static double CalculateThroatDiameter(double throatArea)
        {
            return CalculateNozzleDiameter(throatArea);
        }

        /// <summary>
        /// Calculates power fluid power requirement.
        /// Formula: P = (Q_power * H_power * SG) / (3960 * η_power_pump)
        /// </summary>
        /// <param name="powerFluidFlowRate">Power fluid flow rate in GPM.</param>
        /// <param name="powerFluidHead">Power fluid head in feet.</param>
        /// <param name="specificGravity">Specific gravity (default: 1.0).</param>
        /// <param name="powerPumpEfficiency">Power pump efficiency (default: 0.75).</param>
        /// <returns>Power requirement in horsepower.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculatePowerFluidPowerRequirement(
            double powerFluidFlowRate,
            double powerFluidHead,
            double specificGravity = WaterSpecificGravity,
            double powerPumpEfficiency = 0.75)
        {
            PumpDataValidator.ValidateFlowRate(powerFluidFlowRate, nameof(powerFluidFlowRate));
            PumpDataValidator.ValidateHead(powerFluidHead, nameof(powerFluidHead));
            PumpDataValidator.ValidateSpecificGravity(specificGravity, nameof(specificGravity));
            PumpDataValidator.ValidateEfficiency(powerPumpEfficiency, nameof(powerPumpEfficiency));

            return Calculations.PowerCalculations.CalculateBrakeHorsepower(
                powerFluidFlowRate, powerFluidHead, specificGravity, powerPumpEfficiency);
        }

        /// <summary>
        /// Calculates jet pump efficiency from measured data.
        /// Formula: η_jet = (Q_production * H_lift) / (Q_power * H_power)
        /// </summary>
        /// <param name="productionFlowRate">Production flow rate in GPM.</param>
        /// <param name="totalLift">Total lift in feet.</param>
        /// <param name="powerFluidFlowRate">Power fluid flow rate in GPM.</param>
        /// <param name="powerFluidHead">Power fluid head in feet.</param>
        /// <returns>Jet pump efficiency (0 to 1).</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateJetEfficiency(
            double productionFlowRate,
            double totalLift,
            double powerFluidFlowRate,
            double powerFluidHead)
        {
            PumpDataValidator.ValidateFlowRate(productionFlowRate, nameof(productionFlowRate));
            PumpDataValidator.ValidateFlowRate(powerFluidFlowRate, nameof(powerFluidFlowRate));
            PumpDataValidator.ValidateHead(totalLift, nameof(totalLift));
            PumpDataValidator.ValidateHead(powerFluidHead, nameof(powerFluidHead));

            if (Math.Abs(powerFluidFlowRate) < Epsilon || Math.Abs(powerFluidHead) < Epsilon)
                throw new InvalidInputException("Power fluid", 
                    "Power fluid flow rate and head cannot be zero for efficiency calculation.");

            double productionPower = productionFlowRate * totalLift;
            double powerFluidPower = powerFluidFlowRate * powerFluidHead;

            return Math.Min(1.0, productionPower / powerFluidPower);
        }
    }
}


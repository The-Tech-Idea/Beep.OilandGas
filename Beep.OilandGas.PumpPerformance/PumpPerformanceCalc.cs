using System;
using System.Collections.Generic;
using Beep.OilandGas.PumpPerformance.Calculations;
using Beep.OilandGas.PumpPerformance.Constants;
using Beep.OilandGas.PumpPerformance.Exceptions;
using Beep.OilandGas.PumpPerformance.Validation;
using static Beep.OilandGas.PumpPerformance.Constants.PumpConstants;

namespace Beep.OilandGas.PumpPerformance
{
    /// <summary>
    /// Provides pump performance calculations including H-Q (Head-Quantity) curves and efficiency calculations.
    /// </summary>
    public class PumpPerformanceCalc
    {
        /// <summary>
        /// Calculates pump efficiency from flow rate, head, and power data.
        /// Formula: η = (Q * H * SG) / (3960 * BHP)
        /// Note: This method assumes flow rates are in GPM. For BPD, convert first.
        /// </summary>
        /// <param name="flowRate">Array of flow rates in gallons per minute (GPM).</param>
        /// <param name="head">Array of heads in feet.</param>
        /// <param name="power">Array of brake horsepower (BHP) values.</param>
        /// <param name="specificGravity">Specific gravity of the fluid (default: 1.0 for water).</param>
        /// <returns>Array of efficiency values (0 to 1).</returns>
        /// <exception cref="ArgumentNullException">Thrown when input arrays are null.</exception>
        /// <exception cref="InvalidInputException">Thrown when input data is invalid.</exception>
        public static double[] HQCalc(
            double[] flowRate, 
            double[] head, 
            double[] power,
            double specificGravity = WaterSpecificGravity)
        {
            // Validate inputs
            PumpDataValidator.ValidateFlowRates(flowRate, nameof(flowRate));
            PumpDataValidator.ValidateHeads(head, nameof(head));
            PumpDataValidator.ValidatePowers(power, nameof(power));
            PumpDataValidator.ValidateMatchingLengths(flowRate, head, nameof(flowRate), nameof(head));
            PumpDataValidator.ValidateMatchingLengths(flowRate, power, nameof(flowRate), nameof(power));
            PumpDataValidator.ValidateSpecificGravity(specificGravity, nameof(specificGravity));

            // Use the comprehensive efficiency calculation
            return EfficiencyCalculations.CalculateOverallEfficiency(
                flowRate, head, power, specificGravity);
        }
        /// <summary>
        /// Calculates C-Factor and generates pump performance data based on motor input power.
        /// C-Factor is used to predict pump performance at different flow rates.
        /// Formula: C = MotorInputPower / Q₀³, then Power = C * Q³, Head = H₀ * (Q/Q₀)²
        /// </summary>
        /// <param name="motorInputPower">Motor input power in horsepower.</param>
        /// <param name="flowRate">Array of flow rates in GPM (first value is reference).</param>
        /// <param name="head">Array of heads in feet (first value is reference).</param>
        /// <returns>List of CfactorOutput objects containing calculated head and power values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when input arrays are null.</exception>
        /// <exception cref="InvalidInputException">Thrown when input data is invalid.</exception>
        public static List<CfactorOutput> CFactorCalc(
            double motorInputPower, 
            double[] flowRate, 
            double[] head)
        {
            // Validate inputs
            PumpDataValidator.ValidatePower(motorInputPower, nameof(motorInputPower));
            PumpDataValidator.ValidateFlowRates(flowRate, nameof(flowRate));
            PumpDataValidator.ValidateHeads(head, nameof(head));
            PumpDataValidator.ValidateMatchingLengths(flowRate, head, nameof(flowRate), nameof(head));

            if (flowRate.Length == 0)
                throw new InvalidInputException(nameof(flowRate), 
                    "Flow rate array cannot be empty for C-Factor calculation.");

            if (Math.Abs(flowRate[0]) < Epsilon)
                throw new InvalidInputException(nameof(flowRate), 
                    "Reference flow rate (first value) cannot be zero for C-Factor calculation.");

            // Calculate the C-Factor based on the motor input power and reference flow rate
            double cFactor = motorInputPower / Math.Pow(flowRate[0], 3);
            
            List<CfactorOutput> retval = new List<CfactorOutput>();
            double referenceHead = head[0];
            double referenceFlowRate = flowRate[0];

            // Calculate the pump head and power based on the C-Factor for each flow rate
            for (int i = 0; i < flowRate.Length; i++)
            {
                double pumpHead = referenceHead * Math.Pow(flowRate[i] / referenceFlowRate, 2);
                double pumpPower = cFactor * Math.Pow(flowRate[i], 3);
                retval.Add(new CfactorOutput(pumpHead, pumpPower));
            }

            return retval;
        }

        /// <summary>
        /// Represents the output of a C-Factor calculation, containing calculated pump head and power.
        /// </summary>
        public class CfactorOutput
        {
            /// <summary>
            /// Gets or sets the calculated pump head in feet.
            /// </summary>
            public double PumpHead { get; set; }

            /// <summary>
            /// Gets or sets the calculated pump power in horsepower.
            /// </summary>
            public double PumpPower { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="CfactorOutput"/> class.
            /// </summary>
            public CfactorOutput()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CfactorOutput"/> class.
            /// </summary>
            /// <param name="pumpHead">The calculated pump head in feet.</param>
            /// <param name="pumpPower">The calculated pump power in horsepower.</param>
            public CfactorOutput(double pumpHead, double pumpPower)
            {
                PumpHead = pumpHead;
                PumpPower = pumpPower;
            }
        }

    }
}

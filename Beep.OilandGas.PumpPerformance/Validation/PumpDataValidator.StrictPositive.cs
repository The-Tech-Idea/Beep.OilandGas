using Beep.OilandGas.PumpPerformance.Exceptions;
using static Beep.OilandGas.PumpPerformance.Constants.PumpConstants;

namespace Beep.OilandGas.PumpPerformance.Validation
{
    /// <summary>
    /// Strictly-positive checks for division and power-law contexts (C-factor, operating points).
    /// </summary>
    public static partial class PumpDataValidator
    {
        /// <summary>
        /// Ensures flow rate is finite and greater than zero (GPM).
        /// </summary>
        public static void ValidateStrictlyPositiveFlowRate(double flowRate, string paramName)
        {
            ValidateFlowRate(flowRate, paramName);
            if (flowRate <= Epsilon)
                throw new InvalidInputException(paramName, $"Flow rate must be positive. Value: {flowRate}.");
        }

        /// <summary>
        /// Ensures head is finite and greater than zero (feet).
        /// </summary>
        public static void ValidateStrictlyPositiveHead(double head, string paramName)
        {
            ValidateHead(head, paramName);
            if (head <= Epsilon)
                throw new InvalidInputException(paramName, $"Head must be positive. Value: {head}.");
        }

        /// <summary>
        /// Ensures power is finite and greater than zero (HP).
        /// </summary>
        public static void ValidateStrictlyPositivePower(double power, string paramName)
        {
            ValidatePower(power, paramName);
            if (power <= Epsilon)
                throw new InvalidInputException(paramName, $"Power must be positive. Value: {power}.");
        }
    }
}

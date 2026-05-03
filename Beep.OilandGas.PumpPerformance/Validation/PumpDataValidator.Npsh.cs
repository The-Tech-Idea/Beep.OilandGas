using Beep.OilandGas.PumpPerformance.Exceptions;

namespace Beep.OilandGas.PumpPerformance.Validation
{
    /// <summary>
    /// NPSH-related validation (available vs required, margin).
    /// </summary>
    public static partial class PumpDataValidator
    {
        /// <summary>
        /// Validates that NPSHa exceeds NPSHr by at least <paramref name="minimumMarginFeet"/> (ft of fluid).
        /// </summary>
        /// <param name="npshAvailable">NPSH available (ft).</param>
        /// <param name="npshRequired">NPSH required from vendor curve or correlation (ft).</param>
        /// <param name="minimumMarginFeet">Minimum acceptable margin (ft).</param>
        /// <exception cref="InvalidInputException">When margin is insufficient or inputs are non-finite.</exception>
        public static void ValidateNpshMargin(
            double npshAvailable,
            double npshRequired,
            double minimumMarginFeet)
        {
            if (double.IsNaN(npshAvailable) || double.IsInfinity(npshAvailable))
                throw new InvalidInputException(nameof(npshAvailable), "NPSHa must be a finite number.");
            if (double.IsNaN(npshRequired) || double.IsInfinity(npshRequired))
                throw new InvalidInputException(nameof(npshRequired), "NPSHr must be a finite number.");
            if (double.IsNaN(minimumMarginFeet) || double.IsInfinity(minimumMarginFeet) || minimumMarginFeet < 0)
                throw new InvalidInputException(nameof(minimumMarginFeet), "Minimum margin must be a non-negative finite number.");

            double margin = npshAvailable - npshRequired;
            if (margin < minimumMarginFeet)
                throw new InvalidInputException(
                    "NPSH",
                    $"Insufficient NPSH margin: NPSHa ({npshAvailable:F2} ft) - NPSHr ({npshRequired:F2} ft) = {margin:F2} ft; required margin ≥ {minimumMarginFeet:F2} ft.");
        }
    }
}

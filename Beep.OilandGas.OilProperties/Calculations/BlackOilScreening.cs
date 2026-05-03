using System;

namespace Beep.OilandGas.OilProperties.Calculations
{
    /// <summary>
    /// Shared black-oil screening flow: bubble point, solution GOR at pressure, and Rs at P (Standing).
    /// </summary>
    public static class BlackOilScreening
    {
        /// <summary>
        /// Resolves bubble pressure and solution GOR at the given pressure for Standing screening.
        /// </summary>
        /// <param name="pressurePsia">Current pressure (psia).</param>
        /// <param name="tempFahrenheit">Temperature (°F).</param>
        /// <param name="apiGravity">API gravity.</param>
        /// <param name="gasSpecificGravity">Gas specific gravity (air = 1).</param>
        /// <param name="bubblePointPressure">Optional lab Pb (psia); must be &gt; 0 to be used.</param>
        /// <param name="solutionGasOilRatio">Optional reported Rs (scf/STB); must be &gt; 0 to derive Pb.</param>
        /// <returns>Tuple (Pb, Rs at P).</returns>
        public static (decimal Pb, decimal RsAtP) GetPbAndRsAtPressure(
            decimal pressurePsia,
            decimal tempFahrenheit,
            decimal apiGravity,
            decimal gasSpecificGravity,
            decimal? bubblePointPressure,
            decimal? solutionGasOilRatio)
        {
            if (pressurePsia <= 0m)
                throw new ArgumentOutOfRangeException(nameof(pressurePsia), "Pressure must be greater than zero.");

            decimal rs = 0m;
            decimal pb;

            if (bubblePointPressure is decimal pbp && pbp > 0m)
            {
                pb = pbp;
            }
            else if (solutionGasOilRatio is decimal sgor && sgor > 0m)
            {
                rs = sgor;
                pb = OilPropertyCalculator.CalculateBubblePointPressure_Standing(rs, gasSpecificGravity, apiGravity, tempFahrenheit);
            }
            else
            {
                pb = pressurePsia;
                rs = OilPropertyCalculator.CalculateSolutionGOR_Standing(pressurePsia, gasSpecificGravity, apiGravity, tempFahrenheit);
            }

            decimal rsAtP = pressurePsia > pb
                ? OilPropertyCalculator.CalculateSolutionGOR_Standing(pb, gasSpecificGravity, apiGravity, tempFahrenheit)
                : OilPropertyCalculator.CalculateSolutionGOR_Standing(pressurePsia, gasSpecificGravity, apiGravity, tempFahrenheit);

            return (pb, rsAtP);
        }
    }
}

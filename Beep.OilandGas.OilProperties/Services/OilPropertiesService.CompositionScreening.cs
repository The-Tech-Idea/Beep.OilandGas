using Beep.OilandGas.Models.Data;
using Beep.OilandGas.OilProperties.Calculations;
using Beep.OilandGas.OilProperties.Constants;

namespace Beep.OilandGas.OilProperties.Services
{
    /// <summary>
    /// Composition-scoped Standing / Beggs screening (same default gas Sg as CalculateOilPropertiesAsync).
    /// Per-point validation is minimal for sweep loops; callers must validate the P–T window first.
    /// </summary>
    public partial class OilPropertiesService
    {
        private static bool IsScreeningPointValid(decimal pressurePsia, decimal temperatureRankine) =>
            pressurePsia > 0m && temperatureRankine >= OilPropertyConstants.MinimumTemperatureRankine;

        /// <summary>Standing Bo using <see cref="BlackOilScreening"/> Rs at P (default gas Sg).</summary>
        private decimal CompositionFormationVolumeFactor(decimal pressurePsia, decimal temperatureRankine, OilComposition composition)
        {
            if (!IsScreeningPointValid(pressurePsia, temperatureRankine))
                return OilPropertyConstants.MinimumFormationVolumeFactor;

            decimal tempF = OilPropertyUnits.RankineToFahrenheit(temperatureRankine);
            decimal gasSg = OilPropertyConstants.DefaultGasSpecificGravity;
            decimal? bubble = composition.BubblePointPressure > 0m ? composition.BubblePointPressure : null;
            decimal? gor = composition.GasOilRatio > 0m ? composition.GasOilRatio : null;
            var (_, rsAtP) = BlackOilScreening.GetPbAndRsAtPressure(pressurePsia, tempF, composition.OilGravity, gasSg, bubble, gor);
            return OilPropertyCalculator.CalculateOilFVF_Standing(rsAtP, gasSg, composition.OilGravity, tempF);
        }

        /// <summary>Beggs–Robinson saturated oil viscosity at (P, T) for the composition.</summary>
        private decimal CompositionSaturatedOilViscosity(decimal pressurePsia, decimal temperatureRankine, OilComposition composition)
        {
            if (!IsScreeningPointValid(pressurePsia, temperatureRankine))
                return OilPropertyConstants.MinimumViscosity;

            decimal tempF = OilPropertyUnits.RankineToFahrenheit(temperatureRankine);
            decimal gasSg = OilPropertyConstants.DefaultGasSpecificGravity;
            decimal? bubble = composition.BubblePointPressure > 0m ? composition.BubblePointPressure : null;
            decimal? gor = composition.GasOilRatio > 0m ? composition.GasOilRatio : null;
            var (_, rsAtP) = BlackOilScreening.GetPbAndRsAtPressure(pressurePsia, tempF, composition.OilGravity, gasSg, bubble, gor);
            decimal dead = OilPropertyCalculator.CalculateDeadOilViscosity_BeggsRobinson(composition.OilGravity, tempF);
            return OilPropertyCalculator.CalculateSaturatedViscosity_BeggsRobinson(dead, rsAtP);
        }
    }
}

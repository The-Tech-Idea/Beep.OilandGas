namespace Beep.OilandGas.OilProperties.Constants
{
    /// <summary>
    /// Unit conversions for oil PVT correlations (Standing, Beggs–Robinson use °F).
    /// Service and <see cref="Beep.OilandGas.Models.Core.Interfaces.IOilPropertiesService"/> use Rankine for temperature inputs.
    /// </summary>
    public static class OilPropertyUnits
    {
        /// <summary>Rankine offset: T(°R) = T(°F) + 459.67 (0 °F = 459.67 °R).</summary>
        public const decimal RankineOffsetFromFahrenheit = 459.67m;

        /// <summary>Converts Rankine to Fahrenheit for correlation inputs.</summary>
        public static decimal RankineToFahrenheit(decimal rankine) => rankine - RankineOffsetFromFahrenheit;
    }
}

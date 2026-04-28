using System;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.ProductionAccounting.Constants
{
    /// <summary>BSW and net-oil arithmetic when deriving <c>NET_VOLUME</c> from ticket gross and water cut.</summary>
    public static class MeasurementVolumeRules
    {
        /// <summary>Ticket <c>BSW_PERCENTAGE</c> is 0–100 on this scale.</summary>
        public const decimal BswPercentScale = 100m;

        /// <summary>Whole fluid fraction before subtracting water (1 − BSW).</summary>
        public const decimal NetOilWholeFraction = 1m;
    }

    /// <summary><c>MEASUREMENT_RECORD.MEASUREMENT_STANDARD</c> string aligned with <see cref="MeasurementStandard"/> enum seed.</summary>
    public static class MeasurementStandardCodes
    {
        public const string Api = nameof(MeasurementStandard.API);
    }

    /// <summary>Recognized <c>MEASUREMENT_METHOD</c> values: legacy <see cref="LegacyMeasurementMethodCodes"/> plus <see cref="MeasurementMethod"/> enum names.</summary>
    public static class MeasurementMethodValidation
    {
        public static bool IsSeededOrLegacy(string? method)
        {
            if (string.IsNullOrWhiteSpace(method))
                return false;
            if (string.Equals(method, LegacyMeasurementMethodCodes.Automated, StringComparison.OrdinalIgnoreCase))
                return true;
            return Enum.TryParse<MeasurementMethod>(method, ignoreCase: true, out _);
        }
    }
}

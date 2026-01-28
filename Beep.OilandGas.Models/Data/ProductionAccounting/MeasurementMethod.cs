
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public enum MeasurementMethod
    {
        /// <summary>
        /// Manual measurement (tank gauging, manual sampling).
        /// </summary>
        Manual,

        /// <summary>
        /// Automatic metering (flow meters).
        /// </summary>
        Automatic,

        /// <summary>
        /// Automatic Custody Transfer.
        /// </summary>
        ACT,

        /// <summary>
        /// Lease Automatic Custody Transfer.
        /// </summary>
        LACT
    }
}

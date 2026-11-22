using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionForecasting
{
    /// <summary>
    /// The volume columns of a forecast point (RES-07). The rate columns are the generated ones; these hold the closed-form
    /// volume each month and the running total, so a reloaded forecast shows the same table it was saved with and the
    /// cumulative column is data rather than a recomputation.
    /// </summary>
    public partial class PRODUCTION_FORECAST_POINT
    {
        /// <summary>The volume produced in this month, in the stream's own unit.</summary>
        public decimal MONTHLY_VOLUME
        {
            get => MonthlyVolumeValue;
            set => SetProperty(ref MonthlyVolumeValue, value);
        }

        private decimal MonthlyVolumeValue;

        /// <summary>The running total from the first month to this one.</summary>
        public decimal CUMULATIVE_VOLUME
        {
            get => CumulativeVolumeValue;
            set => SetProperty(ref CumulativeVolumeValue, value);
        }

        private decimal CumulativeVolumeValue;

        /// <summary>The unit <see cref="MONTHLY_VOLUME"/> and <see cref="CUMULATIVE_VOLUME"/> are in.</summary>
        [MaxLength(16)]
        public string? VOLUME_OUOM
        {
            get => VolumeOuomValue;
            set => SetProperty(ref VolumeOuomValue, value);
        }

        private string? VolumeOuomValue;
    }
}

#nullable enable

namespace Beep.OilandGas.Models.Data.Measurement
{
    /// <summary>
    /// Represents measurement accuracy standards.
    /// </summary>
    public class MeasurementAccuracy : ModelEntityBase
    {
        private string? AccuracyIdValue;

        public string? AccuracyId

        {

            get { return this.AccuracyIdValue; }

            set { SetProperty(ref AccuracyIdValue, value); }

        }
        private string? AccuracyLevelValue;

        public string? AccuracyLevel

        {

            get { return this.AccuracyLevelValue; }

            set { SetProperty(ref AccuracyLevelValue, value); }

        }
        private decimal? TolerancePercentageValue;

        public decimal? TolerancePercentage

        {

            get { return this.TolerancePercentageValue; }

            set { SetProperty(ref TolerancePercentageValue, value); }

        }
        private string? MeasurementTypeValue;

        public string? MeasurementType

        {

            get { return this.MeasurementTypeValue; }

            set { SetProperty(ref MeasurementTypeValue, value); }

        }
        private string? StandardValue;

        public string? Standard

        {

            get { return this.StandardValue; }

            set { SetProperty(ref StandardValue, value); }

        }
    }
}


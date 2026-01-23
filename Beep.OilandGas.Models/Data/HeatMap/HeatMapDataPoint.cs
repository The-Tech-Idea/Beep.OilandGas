namespace Beep.OilandGas.Models.Data.HeatMap
{
    /// <summary>
    /// Data point for heat map visualization
    /// DTO for calculations - Entity class: HEAT_MAP_DATA_POINT
    /// </summary>
    public class HeatMapDataPoint : ModelEntityBase
    {
        /// <summary>
        /// X coordinate (normalized or screen coordinates)
        /// </summary>
        private double XValue;

        public double X

        {

            get { return this.XValue; }

            set { SetProperty(ref XValue, value); }

        }

        /// <summary>
        /// Y coordinate (normalized or screen coordinates)
        /// </summary>
        private double YValue;

        public double Y

        {

            get { return this.YValue; }

            set { SetProperty(ref YValue, value); }

        }

        /// <summary>
        /// Original X coordinate (data space)
        /// </summary>
        private double OriginalXValue;

        public double OriginalX

        {

            get { return this.OriginalXValue; }

            set { SetProperty(ref OriginalXValue, value); }

        }

        /// <summary>
        /// Original Y coordinate (data space)
        /// </summary>
        private double OriginalYValue;

        public double OriginalY

        {

            get { return this.OriginalYValue; }

            set { SetProperty(ref OriginalYValue, value); }

        }

        /// <summary>
        /// Value at this point
        /// </summary>
        private double ValueValue;

        public double Value

        {

            get { return this.ValueValue; }

            set { SetProperty(ref ValueValue, value); }

        }

        /// <summary>
        /// Optional label for this point
        /// </summary>
        private string? LabelValue;

        public string? Label

        {

            get { return this.LabelValue; }

            set { SetProperty(ref LabelValue, value); }

        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public HeatMapDataPoint()
        {
        }

        /// <summary>
        /// Constructor with coordinates and value
        /// </summary>
        public HeatMapDataPoint(double originalX, double originalY, double value, string? label = null)
        {
            OriginalX = originalX;
            OriginalY = originalY;
            X = originalX;
            Y = originalY;
            Value = value;
            Label = label;
        }
    }
}





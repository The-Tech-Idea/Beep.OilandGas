using SkiaSharp;

namespace Beep.OilandGas.Drawing.Visualizations.Reservoir
{
    /// <summary>
    /// Controls contour generation, styling, and label density for reservoir map surfaces.
    /// </summary>
    public sealed class ReservoirContourConfiguration
    {
        /// <summary>
        /// Gets or sets the contour interval. Use zero or a negative value to auto-compute a reasonable interval.
        /// </summary>
        public double ContourInterval { get; set; } = 0;

        /// <summary>
        /// Gets or sets the optional minimum contour value.
        /// </summary>
        public double? MinimumContourValue { get; set; }

        /// <summary>
        /// Gets or sets the optional maximum contour value.
        /// </summary>
        public double? MaximumContourValue { get; set; }

        /// <summary>
        /// Gets or sets the number of contour intervals between major contours.
        /// </summary>
        public int MajorContourEvery { get; set; } = 5;

        /// <summary>
        /// Gets or sets the color for minor contours.
        /// </summary>
        public SKColor MinorContourColor { get; set; } = new SKColor(105, 105, 105);

        /// <summary>
        /// Gets or sets the color for major contours.
        /// </summary>
        public SKColor MajorContourColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the stroke width for minor contours.
        /// </summary>
        public float MinorContourLineWidth { get; set; } = 1.0f;

        /// <summary>
        /// Gets or sets the stroke width for major contours.
        /// </summary>
        public float MajorContourLineWidth { get; set; } = 1.7f;

        /// <summary>
        /// Gets or sets whether contour labels should be rendered.
        /// </summary>
        public bool ShowContourLabels { get; set; } = true;

        /// <summary>
        /// Gets or sets whether only major contours should be labeled.
        /// </summary>
        public bool LabelMajorContoursOnly { get; set; } = true;

        /// <summary>
        /// Gets or sets the contour label font size.
        /// </summary>
        public float LabelFontSize { get; set; } = 10f;

        /// <summary>
        /// Gets or sets the contour label color.
        /// </summary>
        public SKColor LabelColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the numeric format for contour labels.
        /// </summary>
        public string LabelFormat { get; set; } = "0.##";

        /// <summary>
        /// Gets or sets the minimum segment length required before a label can be placed.
        /// </summary>
        public float MinimumLabeledSegmentLength { get; set; } = 75f;

        /// <summary>
        /// Gets or sets the interpolation sample count in the X direction.
        /// </summary>
        public int SampleColumns { get; set; } = 80;

        /// <summary>
        /// Gets or sets the interpolation sample count in the Y direction.
        /// </summary>
        public int SampleRows { get; set; } = 80;

        /// <summary>
        /// Gets or sets the number of nearby source points used for inverse-distance interpolation.
        /// </summary>
        public int MaxInfluencePoints { get; set; } = 8;

        /// <summary>
        /// Gets or sets the inverse-distance interpolation power.
        /// </summary>
        public double InterpolationPower { get; set; } = 2.0;
    }
}
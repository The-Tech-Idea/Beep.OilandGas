using Beep.OilandGas.Drawing.Styling;
using System;

namespace Beep.OilandGas.Drawing.Visualizations.WellSchematic.Configuration
{
    /// <summary>
    /// Configuration for well schematic rendering.
    /// </summary>
    public class WellSchematicConfiguration
    {
        /// <summary>
        /// Gets or sets the theme.
        /// </summary>
        public Theme Theme { get; set; } = Theme.Standard;

        /// <summary>
        /// Gets or sets the wellbore stroke width.
        /// </summary>
        public float WellboreStrokeWidth { get; set; } = 2.0f;

        /// <summary>
        /// Gets or sets the casing stroke width.
        /// </summary>
        public float CasingStrokeWidth { get; set; } = 1.5f;

        /// <summary>
        /// Gets or sets the tubing stroke width.
        /// </summary>
        public float TubingStrokeWidth { get; set; } = 1.0f;

        /// <summary>
        /// Gets or sets the spacing between casing segments.
        /// </summary>
        public float CasingSpacing { get; set; } = 10.0f;

        /// <summary>
        /// Gets or sets the padding between tubes.
        /// </summary>
        public float PaddingBetweenTubes { get; set; } = 5.0f;

        /// <summary>
        /// Gets or sets the padding to the side of wellbore.
        /// </summary>
        public float PaddingToSide { get; set; } = 5.0f;

        /// <summary>
        /// Gets or sets the perforation mark width.
        /// </summary>
        public float PerforationMarkWidth { get; set; } = 5.0f;

        /// <summary>
        /// Gets or sets the depth scale width.
        /// </summary>
        public float DepthScaleWidth { get; set; } = 60.0f;

        /// <summary>
        /// Gets or sets the diameter scale factor.
        /// </summary>
        public float DiameterScale { get; set; } = 1.0f;

        /// <summary>
        /// Gets or sets the depth scale factor.
        /// </summary>
        public float DepthScale { get; set; } = 1.0f;

        /// <summary>
        /// Gets or sets whether to use Bezier curves for deviated wellbores.
        /// </summary>
        public bool UseBezierCurves { get; set; } = true;

        /// <summary>
        /// Gets or sets the number of points for Bezier curve calculation.
        /// </summary>
        public int BezierCurvePoints { get; set; } = 100;

        /// <summary>
        /// Gets or sets the SVG resources path.
        /// </summary>
        public string SvgResourcesPath { get; set; } = "";

        /// <summary>
        /// Gets or sets whether SVG resources are embedded.
        /// </summary>
        public bool UseEmbeddedSvg { get; set; } = true;

        /// <summary>
        /// Gets the default configuration.
        /// </summary>
        public static WellSchematicConfiguration Default => new WellSchematicConfiguration();
    }
}


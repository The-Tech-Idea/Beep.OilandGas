using Beep.OilandGas.Drawing.Styling;
using System;

namespace Beep.OilandGas.Drawing.Visualizations.WellSchematic.Configuration
{
    /// <summary>
    /// Controls the annotation density and layout style for well schematics.
    /// </summary>
    public enum WellSchematicAnnotationProfile
    {
        Compact,
        Detailed,
        Print
    }

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
        /// Gets or sets the annotation profile.
        /// </summary>
        public WellSchematicAnnotationProfile AnnotationProfile { get; set; } = WellSchematicAnnotationProfile.Detailed;

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
        /// Gets or sets the lateral stretch factor used for survey-driven wellbore paths.
        /// </summary>
        public float HorizontalStretchFactor { get; set; } = 10.0f;

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
        /// Gets or sets whether to render survey event markers such as kickoff and landing points.
        /// </summary>
        public bool ShowSurveyMarkers { get; set; } = true;

        /// <summary>
        /// Gets or sets the deviation angle threshold used to identify the kickoff point.
        /// </summary>
        public float KickoffDeviationThresholdDegrees { get; set; } = 5.0f;

        /// <summary>
        /// Gets or sets the deviation angle threshold used to identify the landing point.
        /// </summary>
        public float LandingDeviationThresholdDegrees { get; set; } = 80.0f;

        /// <summary>
        /// Gets or sets whether to render equipment callouts.
        /// </summary>
        public bool ShowEquipmentCallouts { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to render perforation callouts.
        /// </summary>
        public bool ShowPerforationCallouts { get; set; } = true;

        /// <summary>
        /// Gets or sets the reserved width for annotation callouts.
        /// </summary>
        public float AnnotationCalloutWidth { get; set; } = 180.0f;

        /// <summary>
        /// Gets or sets the minimum vertical spacing between adjacent callout labels.
        /// </summary>
        public float AnnotationMinimumSpacing { get; set; } = 18.0f;

        /// <summary>
        /// Gets the default configuration.
        /// </summary>
        public static WellSchematicConfiguration Default => new WellSchematicConfiguration();

        /// <summary>
        /// Gets the effective annotation callout width for the selected profile.
        /// </summary>
        public float GetEffectiveCalloutWidth()
        {
            return AnnotationProfile switch
            {
                WellSchematicAnnotationProfile.Compact => Math.Min(AnnotationCalloutWidth, 120.0f),
                WellSchematicAnnotationProfile.Print => Math.Max(AnnotationCalloutWidth, 220.0f),
                _ => AnnotationCalloutWidth
            };
        }

        /// <summary>
        /// Gets the effective annotation spacing for the selected profile.
        /// </summary>
        public float GetEffectiveAnnotationSpacing()
        {
            return AnnotationProfile switch
            {
                WellSchematicAnnotationProfile.Compact => Math.Min(AnnotationMinimumSpacing, 14.0f),
                WellSchematicAnnotationProfile.Print => Math.Max(AnnotationMinimumSpacing, 24.0f),
                _ => AnnotationMinimumSpacing
            };
        }

        /// <summary>
        /// Gets the effective callout label text size for the selected profile.
        /// </summary>
        public float GetEffectiveCalloutTextSize()
        {
            return AnnotationProfile switch
            {
                WellSchematicAnnotationProfile.Compact => 10.0f,
                WellSchematicAnnotationProfile.Print => 12.0f,
                _ => 11.0f
            };
        }

        /// <summary>
        /// Gets the effective survey marker text size for the selected profile.
        /// </summary>
        public float GetEffectiveMarkerTextSize()
        {
            return AnnotationProfile switch
            {
                WellSchematicAnnotationProfile.Compact => 11.0f,
                WellSchematicAnnotationProfile.Print => 13.0f,
                _ => 12.0f
            };
        }

        /// <summary>
        /// Gets a value indicating whether equipment callouts should be rendered.
        /// </summary>
        public bool ShouldRenderEquipmentCallouts() => ShowEquipmentCallouts && AnnotationProfile != WellSchematicAnnotationProfile.Compact;

        /// <summary>
        /// Gets a value indicating whether perforation callouts should be rendered.
        /// </summary>
        public bool ShouldRenderPerforationCallouts() => ShowPerforationCallouts && AnnotationProfile != WellSchematicAnnotationProfile.Compact;

        /// <summary>
        /// Gets a value indicating whether survey markers should be rendered.
        /// </summary>
        public bool ShouldRenderSurveyMarkers() => ShowSurveyMarkers;
    }
}


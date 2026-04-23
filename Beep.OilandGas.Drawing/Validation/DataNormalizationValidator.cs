using System;
using System.Linq;
using Beep.OilandGas.Drawing.CoordinateSystems;
using Beep.OilandGas.Drawing.DataLoaders.Models;

namespace Beep.OilandGas.Drawing.Validation
{
    /// <summary>
    /// Validates normalized loader models before they are converted into render scenes.
    /// </summary>
    public static class DataNormalizationValidator
    {
        public static DrawingValidationReport ValidateReservoirData(ReservoirData reservoirData)
        {
            if (reservoirData == null)
                throw new ArgumentNullException(nameof(reservoirData));

            var report = new DrawingValidationReport();

            if (reservoirData.CoordinateReferenceSystem == null)
            {
                report.AddError(
                    "RES001",
                    "Reservoir data is missing a coordinate reference system.",
                    nameof(reservoirData.CoordinateReferenceSystem),
                    "Populate a projected, geographic, or local reservoir CRS before map or contour rendering.");
            }
            else
            {
                AddCoordinateReferenceSystemWarnings(reservoirData.CoordinateReferenceSystem, report, nameof(reservoirData.CoordinateReferenceSystem), "RESCRS");
            }

            if (reservoirData.Layers == null || reservoirData.Layers.Count == 0)
            {
                report.AddError("RES002", "Reservoir data must contain at least one layer.", nameof(reservoirData.Layers));
                return report;
            }

            for (int index = 0; index < reservoirData.Layers.Count; index++)
            {
                var layer = reservoirData.Layers[index];
                var source = $"{nameof(reservoirData.Layers)}[{index}]";
                if (layer == null)
                {
                    report.AddError("RES003", "Reservoir layer cannot be null.", source);
                    continue;
                }

                if (layer.BottomDepth <= layer.TopDepth)
                {
                    report.AddError(
                        "RES004",
                        "Reservoir layer bottom depth must be greater than top depth.",
                        source,
                        "Normalize interval tops and bottoms before rendering.");
                }

                if (layer.Geometry == null || layer.Geometry.Count == 0)
                {
                    report.AddWarning(
                        "RES005",
                        "Reservoir layer has no geometry points.",
                        source,
                        "Provide polygon or surface points for map and cross-section workflows.");
                }
                else if (layer.Geometry.Any(point => point == null || !double.IsFinite(point.X) || !double.IsFinite(point.Y) || !double.IsFinite(point.Z)))
                {
                    report.AddError("RES006", "Reservoir layer geometry contains non-finite coordinates.", source);
                }
            }

            if (reservoirData.BoundingBox != null)
            {
                if (!double.IsFinite(reservoirData.BoundingBox.MinX) ||
                    !double.IsFinite(reservoirData.BoundingBox.MaxX) ||
                    !double.IsFinite(reservoirData.BoundingBox.MinY) ||
                    !double.IsFinite(reservoirData.BoundingBox.MaxY) ||
                    !double.IsFinite(reservoirData.BoundingBox.MinZ) ||
                    !double.IsFinite(reservoirData.BoundingBox.MaxZ))
                {
                    report.AddError("RES007", "Reservoir bounding box contains non-finite values.", nameof(reservoirData.BoundingBox));
                }
                else if (reservoirData.BoundingBox.MaxX < reservoirData.BoundingBox.MinX ||
                         reservoirData.BoundingBox.MaxY < reservoirData.BoundingBox.MinY ||
                         reservoirData.BoundingBox.MaxZ < reservoirData.BoundingBox.MinZ)
                {
                    report.AddError("RES008", "Reservoir bounding box has inverted extents.", nameof(reservoirData.BoundingBox));
                }
            }

            if ((reservoirData.Grids == null || reservoirData.Grids.Count == 0) &&
                (reservoirData.Surfaces == null || reservoirData.Surfaces.Count == 0))
            {
                report.AddWarning(
                    "RES009",
                    "Reservoir data has no typed grid or surface models.",
                    nameof(reservoirData.Grids),
                    "Populate grid or surface representations before contour generation and map workflows.");
            }

            if (reservoirData.Grids != null)
            {
                for (int index = 0; index < reservoirData.Grids.Count; index++)
                {
                    var grid = reservoirData.Grids[index];
                    var source = $"{nameof(reservoirData.Grids)}[{index}]";
                    if (grid == null)
                    {
                        report.AddError("RES010", "Reservoir grid cannot be null.", source);
                        continue;
                    }

                    if ((grid.ColumnCount.HasValue && grid.ColumnCount.Value <= 0) ||
                        (grid.RowCount.HasValue && grid.RowCount.Value <= 0) ||
                        (grid.LayerCount.HasValue && grid.LayerCount.Value <= 0))
                    {
                        report.AddError("RES011", "Reservoir grid dimensions must be positive when provided.", source);
                    }

                    if (grid.Nodes != null && grid.Nodes.Any(node => node?.Position == null ||
                        !double.IsFinite(node.Position.X) ||
                        !double.IsFinite(node.Position.Y) ||
                        !double.IsFinite(node.Position.Z)))
                    {
                        report.AddError("RES012", "Reservoir grid contains non-finite node coordinates.", source);
                    }
                }
            }

            if (reservoirData.Surfaces != null)
            {
                for (int index = 0; index < reservoirData.Surfaces.Count; index++)
                {
                    var surface = reservoirData.Surfaces[index];
                    var source = $"{nameof(reservoirData.Surfaces)}[{index}]";
                    if (surface == null)
                    {
                        report.AddError("RES013", "Reservoir surface cannot be null.", source);
                        continue;
                    }

                    if (surface.Points == null || surface.Points.Count < 3)
                    {
                        report.AddWarning(
                            "RES014",
                            "Reservoir surface has fewer than three sampled points.",
                            source,
                            "Provide at least three points per surface before contour generation.");
                    }
                    else if (surface.Points.Any(point => point == null ||
                        !double.IsFinite(point.X) ||
                        !double.IsFinite(point.Y) ||
                        !double.IsFinite(point.Z)))
                    {
                        report.AddError("RES015", "Reservoir surface contains non-finite coordinates.", source);
                    }
                }
            }

            return report;
        }

        public static DrawingValidationReport ValidateLogData(LogData logData)
        {
            if (logData == null)
                throw new ArgumentNullException(nameof(logData));

            var report = new DrawingValidationReport();

            if (string.IsNullOrWhiteSpace(logData.DepthUnit))
            {
                report.AddError(
                    "LOG001",
                    "Log data is missing a depth unit.",
                    nameof(logData.DepthUnit),
                    "Normalize LAS, DLIS, or WITSML depth units before rendering log tracks.");
            }

            if (logData.EndDepth <= logData.StartDepth)
            {
                report.AddError("LOG002", "Log end depth must be greater than start depth.", nameof(logData.EndDepth));
            }

            if (logData.Depths == null || logData.Depths.Count == 0)
            {
                report.AddError("LOG003", "Log data must contain depth samples.", nameof(logData.Depths));
            }
            else
            {
                for (int index = 0; index < logData.Depths.Count; index++)
                {
                    if (!double.IsFinite(logData.Depths[index]))
                    {
                        report.AddError("LOG004", "Log depths contain non-finite values.", $"{nameof(logData.Depths)}[{index}]");
                        break;
                    }

                    if (index > 0 && logData.Depths[index] <= logData.Depths[index - 1])
                    {
                        report.AddError(
                            "LOG005",
                            "Log depths must be strictly increasing.",
                            $"{nameof(logData.Depths)}[{index}]",
                            "Sort and de-duplicate depth samples during ingestion.");
                        break;
                    }
                }
            }

            if (logData.Curves == null || logData.Curves.Count == 0)
            {
                report.AddError("LOG006", "Log data must contain at least one curve.", nameof(logData.Curves));
            }
            else if (logData.Depths != null)
            {
                foreach (var curve in logData.Curves)
                {
                    if (curve.Value == null)
                    {
                        report.AddError("LOG007", $"Curve '{curve.Key}' does not have values.", nameof(logData.Curves));
                        continue;
                    }

                    if (curve.Value.Count != logData.Depths.Count)
                    {
                        report.AddError(
                            "LOG008",
                            $"Curve '{curve.Key}' sample count does not match the depth sample count.",
                            nameof(logData.Curves),
                            "Each curve should have one value per depth sample.");
                    }

                    if (!logData.CurveMetadata.TryGetValue(curve.Key, out var metadata) || string.IsNullOrWhiteSpace(metadata?.Unit))
                    {
                        report.AddWarning(
                            "LOG009",
                            $"Curve '{curve.Key}' is missing a declared unit.",
                            nameof(logData.CurveMetadata),
                            "Populate curve units so axis labels and legends can be explicit.");
                    }
                }
            }

            if (logData.DepthStep <= 0)
            {
                report.AddWarning(
                    "LOG010",
                    "Log data does not declare a positive depth step.",
                    nameof(logData.DepthStep),
                    "Set the depth step during normalization when the source format provides one.");
            }

            return report;
        }

        public static DrawingValidationReport ValidateDeviationSurvey(DeviationSurvey survey)
        {
            if (survey == null)
                throw new ArgumentNullException(nameof(survey));

            var report = new DrawingValidationReport();

            if (survey.SurveyPoints == null || survey.SurveyPoints.Count == 0)
            {
                report.AddError("SUR001", "Deviation survey must contain at least one survey point.", nameof(survey.SurveyPoints));
                return report;
            }

            if (string.IsNullOrWhiteSpace(survey.WellIdentifier))
            {
                report.AddWarning(
                    "SUR002",
                    "Deviation survey is missing a well identifier.",
                    nameof(survey.WellIdentifier),
                    "Populate the well identifier so scene provenance and overlays can be traced back to the source well.");
            }

            for (int index = 0; index < survey.SurveyPoints.Count; index++)
            {
                var point = survey.SurveyPoints[index];
                var source = $"{nameof(survey.SurveyPoints)}[{index}]";

                if (point == null)
                {
                    report.AddError("SUR003", "Deviation survey point cannot be null.", source);
                    continue;
                }

                if (!double.IsFinite(point.MD) || point.MD < 0)
                {
                    report.AddError("SUR004", "Measured depth must be finite and non-negative.", $"{source}.MD");
                }

                if (!double.IsFinite(point.DEVIATION_ANGLE) || point.DEVIATION_ANGLE < 0 || point.DEVIATION_ANGLE > 180)
                {
                    report.AddError("SUR005", "Deviation angle must be within the range 0 to 180 degrees.", $"{source}.DEVIATION_ANGLE");
                }

                if (!double.IsFinite(point.AZIMUTH))
                {
                    report.AddError("SUR006", "Azimuth must be finite.", $"{source}.AZIMUTH");
                }
                else if (point.AZIMUTH < 0 || point.AZIMUTH > 360)
                {
                    report.AddWarning(
                        "SUR007",
                        "Azimuth falls outside the conventional 0 to 360 degree range.",
                        $"{source}.AZIMUTH",
                        "Normalize azimuth values during survey ingestion if the source format permits wrap-around values.");
                }

                if (index > 0)
                {
                    var previousPoint = survey.SurveyPoints[index - 1];
                    if (previousPoint != null && point.MD <= previousPoint.MD)
                    {
                        report.AddError(
                            "SUR008",
                            "Survey points must be ordered by strictly increasing measured depth.",
                            source,
                            "Sort survey points by MD and remove duplicate stations before rendering directional paths.");
                    }
                }
            }

            return report;
        }

        private static void AddCoordinateReferenceSystemWarnings(
            CoordinateReferenceSystem coordinateReferenceSystem,
            DrawingValidationReport report,
            string source,
            string codePrefix)
        {
            for (int index = 0; index < coordinateReferenceSystem.Axes.Count; index++)
            {
                var axis = coordinateReferenceSystem.Axes[index];
                if (axis.Unit.Dimension == MeasurementDimension.Unknown)
                {
                    report.AddWarning(
                        $"{codePrefix}001",
                        $"Axis '{axis.Name}' is missing a recognized measurement unit.",
                        $"{source}.Axes[{index}]",
                        "Normalize axis units so scale bars, legends, and labels can be rendered consistently.");
                }
            }
        }
    }
}
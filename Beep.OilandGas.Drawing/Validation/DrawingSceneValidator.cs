using System;
using System.Linq;
using Beep.OilandGas.Drawing.CoordinateSystems;
using Beep.OilandGas.Drawing.Exceptions;
using Beep.OilandGas.Drawing.Scenes;

namespace Beep.OilandGas.Drawing.Validation
{
    /// <summary>
    /// Validates typed drawing scenes and emits diagnostics for Phase 01 scene and CRS requirements.
    /// </summary>
    public static class DrawingSceneValidator
    {
        /// <summary>
        /// Validates a scene and returns all diagnostics.
        /// </summary>
        public static DrawingValidationReport Validate(DrawingScene scene)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            var report = new DrawingValidationReport();

            if (string.IsNullOrWhiteSpace(scene.Name))
            {
                report.AddError("SCN001", "Scene name is required.", nameof(scene.Name));
            }

            if (scene.CoordinateReferenceSystem == null)
            {
                report.AddError(
                    "SCN002",
                    "A scene must declare a coordinate reference system.",
                    nameof(scene.CoordinateReferenceSystem),
                    "Assign a projected, geographic, depth, section, or time CRS before rendering.");

                return report;
            }

            AddCoordinateReferenceSystemDiagnostics(scene.CoordinateReferenceSystem, report, nameof(scene.CoordinateReferenceSystem), "SCNCRS");

            switch (scene.Kind)
            {
                case DrawingSceneKind.Map:
                    if (!scene.CoordinateReferenceSystem.SupportsPlanView)
                    {
                        report.AddError(
                            "SCN003",
                            "Map scenes require a projected or geographic coordinate reference system.",
                            nameof(scene.CoordinateReferenceSystem),
                            "Use CoordinateReferenceSystem.CreateGeographicCrs84 or a projected CRS definition.");
                    }
                    break;

                case DrawingSceneKind.Depth:
                    if (!scene.CoordinateReferenceSystem.Axes.Any(axis => axis.Kind == CoordinateAxisKind.Depth || axis.Kind == CoordinateAxisKind.MeasuredDepth))
                    {
                        report.AddError(
                            "SCN004",
                            "Depth scenes require a depth-aware axis.",
                            nameof(scene.CoordinateReferenceSystem),
                            "Add a Depth or MeasuredDepth axis to the scene CRS.");
                    }
                    break;

                case DrawingSceneKind.Section:
                    if (!scene.CoordinateReferenceSystem.Axes.Any(axis => axis.Kind == CoordinateAxisKind.SectionDistance))
                    {
                        report.AddError(
                            "SCN005",
                            "Section scenes require a section-distance axis.",
                            nameof(scene.CoordinateReferenceSystem),
                            "Create the scene with CoordinateReferenceSystem.CreateSection or add a SectionDistance axis.");
                    }

                    if (!scene.CoordinateReferenceSystem.Axes.Any(axis => axis.Kind == CoordinateAxisKind.Depth || axis.Kind == CoordinateAxisKind.MeasuredDepth))
                    {
                        report.AddError(
                            "SCN006",
                            "Section scenes require a depth-aware axis.",
                            nameof(scene.CoordinateReferenceSystem),
                            "Add a Depth or MeasuredDepth axis to the scene CRS.");
                    }
                    break;

                case DrawingSceneKind.Time:
                    if (!scene.CoordinateReferenceSystem.Axes.Any(axis => axis.Kind == CoordinateAxisKind.Time))
                    {
                        report.AddError(
                            "SCN007",
                            "Time scenes require a time axis.",
                            nameof(scene.CoordinateReferenceSystem),
                            "Use CoordinateReferenceSystem.CreateTime or add a Time axis.");
                    }
                    break;
            }

            if (scene.WorldBounds.HasValue)
            {
                var bounds = scene.WorldBounds.Value;
                if (!float.IsFinite(bounds.Left) || !float.IsFinite(bounds.Top) || !float.IsFinite(bounds.Right) || !float.IsFinite(bounds.Bottom))
                {
                    report.AddError("SCN008", "World bounds contain non-finite values.", nameof(scene.WorldBounds));
                }
                else if (bounds.Right < bounds.Left || bounds.Bottom < bounds.Top)
                {
                    report.AddError("SCN009", "World bounds have an invalid extent ordering.", nameof(scene.WorldBounds));
                }
            }
            else
            {
                report.AddInfo(
                    "SCN010",
                    "Scene does not declare world bounds.",
                    nameof(scene.WorldBounds),
                    "Provide world bounds if the engine should auto-fit the scene on load.");
            }

            if (scene.ViewportState.Zoom <= 0)
            {
                report.AddError("SCN011", "Viewport zoom must be greater than zero.", nameof(scene.ViewportState.Zoom));
            }

            return report;
        }

        /// <summary>
        /// Validates a scene and throws when error diagnostics are present.
        /// </summary>
        public static void EnsureValid(DrawingScene scene)
        {
            var report = Validate(scene);
            if (!report.HasErrors)
                return;

            throw new DrawingValidationException($"Scene validation failed: {report.BuildSummary()}", report);
        }

        private static void AddCoordinateReferenceSystemDiagnostics(
            CoordinateReferenceSystem coordinateReferenceSystem,
            DrawingValidationReport report,
            string source,
            string codePrefix)
        {
            if (coordinateReferenceSystem.Axes.Count == 0)
            {
                report.AddError($"{codePrefix}001", "Coordinate reference system must declare at least one axis.", source);
                return;
            }

            var duplicateAxisKinds = coordinateReferenceSystem.Axes
                .GroupBy(axis => axis.Kind)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            foreach (var axisKind in duplicateAxisKinds)
            {
                report.AddWarning(
                    $"{codePrefix}002",
                    $"Coordinate reference system declares duplicate axis kind '{axisKind}'.",
                    source,
                    "Use one axis per semantic role unless the scene intentionally duplicates the dimension.");
            }

            for (int index = 0; index < coordinateReferenceSystem.Axes.Count; index++)
            {
                var axis = coordinateReferenceSystem.Axes[index];
                if (axis.Unit.Dimension == MeasurementDimension.Unknown)
                {
                    report.AddWarning(
                        $"{codePrefix}003",
                        $"Axis '{axis.Name}' does not declare a recognized measurement unit.",
                        $"{source}.Axes[{index}]",
                        "Normalize the axis unit code so legends and labels can be rendered explicitly.");
                }
            }
        }
    }
}
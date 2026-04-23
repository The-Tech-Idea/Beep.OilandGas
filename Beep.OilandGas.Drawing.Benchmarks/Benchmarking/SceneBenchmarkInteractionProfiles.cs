using System;
using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.Measurements;
using Beep.OilandGas.Drawing.Samples;
using SkiaSharp;

namespace Beep.OilandGas.Drawing.Benchmarks.Benchmarking
{
    internal static class SceneBenchmarkInteractionProfiles
    {
        public static void PreparePersistedAnnotations(DrawingSampleScene scene, DrawingEngine engine)
        {
            ArgumentNullException.ThrowIfNull(scene);
            ArgumentNullException.ThrowIfNull(engine);

            if (engine.ActiveScene == null)
                return;

            engine.ActiveScene.InteractionState.ClearSelections();
            engine.ActiveScene.InteractionState.ClearMeasurements();

            var measurementVertices = ResolveMeasurementVertices(scene, engine);
            if (measurementVertices.Length >= 2)
            {
                SceneMeasurementResult measurement = engine.MeasureDistance(measurementVertices[0], measurementVertices[1]);
                engine.RecordMeasurement(measurement, measurementVertices, $"Benchmark {scene.Name} distance");
            }

            var hit = engine.HitTest(ResolveHitTestPoint(scene, engine), screenTolerance: 8f);
            if (hit != null)
            {
                engine.RecordSelection(hit, replaceExisting: true);
            }
        }

        public static SKPoint ResolveHitTestPoint(DrawingSampleScene scene, DrawingEngine engine)
        {
            ArgumentNullException.ThrowIfNull(scene);
            ArgumentNullException.ThrowIfNull(engine);

            return scene.Name switch
            {
                "FieldMap_AssetNetwork" => engine.WorldToScreen(new SKPoint(2050f, 980f)),
                "WellLog_Petrophysical" => ResolveWellLogProbe(engine),
                "ReservoirContour_StructureMap" => ResolveScreenFraction(engine, 0.52f, 0.48f),
                "ReservoirCrossSection_Midline" => ResolveScreenFraction(engine, 0.50f, 0.50f),
                "WellSchematic_EnhancedVertical" => ResolveScreenFraction(engine, 0.60f, 0.48f),
                _ => ResolveScreenFraction(engine, 0.50f, 0.50f)
            };
        }

        private static SKPoint[] ResolveMeasurementVertices(DrawingSampleScene scene, DrawingEngine engine)
        {
            return scene.Name switch
            {
                "WellLog_Petrophysical" =>
                [
                    ResolveScreenFraction(engine, 0.58f, 0.28f),
                    ResolveScreenFraction(engine, 0.58f, 0.72f)
                ],
                "WellSchematic_EnhancedVertical" =>
                [
                    ResolveScreenFraction(engine, 0.52f, 0.22f),
                    ResolveScreenFraction(engine, 0.52f, 0.76f)
                ],
                _ =>
                [
                    ResolveScreenFraction(engine, 0.24f, 0.30f),
                    ResolveScreenFraction(engine, 0.76f, 0.68f)
                ]
            };
        }

        private static SKPoint ResolveWellLogProbe(DrawingEngine engine)
        {
            SKRect bounds = engine.GetBounds();
            float worldX = bounds.Left + (bounds.Width * 0.58f);
            float worldY = bounds.Top + (bounds.Height * 0.46f);
            return engine.WorldToScreen(new SKPoint(worldX, worldY));
        }

        private static SKPoint ResolveScreenFraction(DrawingEngine engine, float xRatio, float yRatio)
        {
            return new SKPoint(engine.Width * xRatio, engine.Height * yRatio);
        }
    }
}
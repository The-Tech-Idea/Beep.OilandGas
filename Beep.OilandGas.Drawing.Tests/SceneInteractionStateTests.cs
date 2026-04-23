using System.Collections.Generic;
using Beep.OilandGas.Drawing.CoordinateSystems;
using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Interaction;
using Beep.OilandGas.Drawing.Layers;
using Beep.OilandGas.Drawing.Measurements;
using Beep.OilandGas.Drawing.Scenes;
using Beep.OilandGas.Drawing.Visualizations.FieldMap;
using SkiaSharp;
using Xunit;

namespace Beep.OilandGas.Drawing.Tests
{
    public class SceneInteractionStateTests
    {
        [Fact]
        public void RecordSelection_PersistsFeatureHitIntoSceneState()
        {
            var scene = DrawingScene.CreateMapScene(
                "Field Map",
                CoordinateReferenceSystem.CreateProjected("EPSG:26915", "NAD83 / UTM zone 15N", "m"));
            scene.WorldBounds = new SKRect(0, 0, 100, 100);

            var data = new FieldMapData();
            data.PointAssets.Add(new FieldMapPointAsset
            {
                AssetId = "well-1",
                AssetName = "A-01",
                AssetKind = FieldMapAssetKind.Well,
                Location = new Point3D { X = 40, Y = 60, Z = 0 },
                Metadata = new Dictionary<string, string> { ["Operator"] = "Beep" }
            });

            using var engine = new DrawingEngine(1000, 1000);
            engine.UseScene(scene);
            engine.AddLayer(new FieldMapLayer(data, new FieldMapConfiguration { ShowLegend = false }));

            var hit = engine.HitTest(engine.WorldToScreen(new SKPoint(40, 60)));
            var selection = engine.RecordSelection(hit, replaceExisting: true);

            Assert.Single(scene.InteractionState.Selections);
            Assert.Equal(selection.AnnotationId, scene.InteractionState.Selections[0].AnnotationId);
            Assert.Equal("well-1", selection.FeatureId);
            Assert.Equal("Beep", selection.Metadata["Operator"]);
        }

        [Fact]
        public void RecordMeasurement_PersistsMeasurementAnnotationIntoSceneState()
        {
            var scene = DrawingScene.CreateMapScene(
                "Field Map",
                CoordinateReferenceSystem.CreateProjected("EPSG:26915", "NAD83 / UTM zone 15N", "m"));
            scene.WorldBounds = new SKRect(0, 0, 100, 100);

            using var engine = new DrawingEngine(1000, 1000);
            engine.UseScene(scene);

            var screenVertices = new[]
            {
                engine.WorldToScreen(new SKPoint(0, 0)),
                engine.WorldToScreen(new SKPoint(3, 4))
            };

            var measurement = engine.MeasureDistance(screenVertices[0], screenVertices[1]);
            var annotation = engine.RecordMeasurement(measurement, screenVertices, label: "Tie Line");

            Assert.Single(scene.InteractionState.Measurements);
            Assert.Equal(annotation.AnnotationId, scene.InteractionState.Measurements[0].AnnotationId);
            Assert.Equal("Tie Line", annotation.Label);
            Assert.Equal(SceneMeasurementKind.Distance, annotation.Kind);
            Assert.Equal(2, annotation.Vertices.Count);
            Assert.Equal(5d, annotation.Value, 3);
        }

        [Fact]
        public void RemoveSelection_RemovesPersistedSelectionByAnnotationId()
        {
            var scene = DrawingScene.CreateMapScene(
                "Field Map",
                CoordinateReferenceSystem.CreateProjected("EPSG:26915", "NAD83 / UTM zone 15N", "m"));
            scene.WorldBounds = new SKRect(0, 0, 100, 100);

            var data = new FieldMapData();
            data.PointAssets.Add(new FieldMapPointAsset
            {
                AssetId = "well-1",
                AssetName = "A-01",
                AssetKind = FieldMapAssetKind.Well,
                Location = new Point3D { X = 40, Y = 60, Z = 0 }
            });

            using var engine = new DrawingEngine(1000, 1000);
            engine.UseScene(scene);
            engine.AddLayer(new FieldMapLayer(data, new FieldMapConfiguration { ShowLegend = false }));

            var hit = engine.HitTest(engine.WorldToScreen(new SKPoint(40, 60)));
            var selection = engine.RecordSelection(hit, replaceExisting: true);

            var removed = scene.InteractionState.RemoveSelection(selection.AnnotationId);

            Assert.True(removed);
            Assert.Empty(scene.InteractionState.Selections);
        }

        [Fact]
        public void RemoveMeasurement_RemovesPersistedMeasurementByAnnotationId()
        {
            var scene = DrawingScene.CreateMapScene(
                "Field Map",
                CoordinateReferenceSystem.CreateProjected("EPSG:26915", "NAD83 / UTM zone 15N", "m"));
            scene.WorldBounds = new SKRect(0, 0, 100, 100);

            using var engine = new DrawingEngine(1000, 1000);
            engine.UseScene(scene);

            var screenVertices = new[]
            {
                engine.WorldToScreen(new SKPoint(0, 0)),
                engine.WorldToScreen(new SKPoint(3, 4))
            };

            var measurement = engine.MeasureDistance(screenVertices[0], screenVertices[1]);
            var annotation = engine.RecordMeasurement(measurement, screenVertices, label: "Tie Line");

            var removed = scene.InteractionState.RemoveMeasurement(annotation.AnnotationId);

            Assert.True(removed);
            Assert.Empty(scene.InteractionState.Measurements);
        }

        [Fact]
        public void RenderToImage_PersistedSelection_DrawsSelectionMarkerThroughCorePipeline()
        {
            var scene = DrawingScene.CreateMapScene(
                "Selection Render",
                CoordinateReferenceSystem.CreateProjected("EPSG:26915", "NAD83 / UTM zone 15N", "m"));
            scene.WorldBounds = new SKRect(0, 0, 100, 100);

            using var engine = new DrawingEngine(400, 400);
            engine.UseScene(scene);
            engine.AddLayer(new TestInteractiveLayer(new SKPoint(40, 60)));

            var hit = engine.HitTest(engine.WorldToScreen(new SKPoint(40, 60)));
            engine.RecordSelection(hit, replaceExisting: true);

            using var image = engine.RenderToImage();
            using var bitmap = SKBitmap.FromImage(image);
            var anchor = engine.WorldToScreen(new SKPoint(40, 60));
            var color = bitmap.GetPixel((int)anchor.X, (int)anchor.Y);

            Assert.NotEqual(SKColors.White, color);
        }

        [Fact]
        public void RenderToImage_PersistedMeasurement_DrawsMeasurementThroughCorePipeline()
        {
            var scene = DrawingScene.CreateMapScene(
                "Measurement Render",
                CoordinateReferenceSystem.CreateProjected("EPSG:26915", "NAD83 / UTM zone 15N", "m"));
            scene.WorldBounds = new SKRect(0, 0, 100, 100);

            using var engine = new DrawingEngine(400, 400);
            engine.UseScene(scene);

            var screenVertices = new[]
            {
                engine.WorldToScreen(new SKPoint(20, 20)),
                engine.WorldToScreen(new SKPoint(80, 20))
            };

            var measurement = engine.MeasureDistance(screenVertices[0], screenVertices[1]);
            engine.RecordMeasurement(measurement, screenVertices, label: "Flowline");

            using var image = engine.RenderToImage();
            using var bitmap = SKBitmap.FromImage(image);
            var midpoint = engine.WorldToScreen(new SKPoint(50, 20));
            var color = bitmap.GetPixel((int)midpoint.X, (int)midpoint.Y);

            Assert.NotEqual(SKColors.White, color);
        }

        [Fact]
        public void RenderToImage_PersistedSelection_UsesConfiguredStyleColor()
        {
            var scene = DrawingScene.CreateMapScene(
                "Styled Selection Render",
                CoordinateReferenceSystem.CreateProjected("EPSG:26915", "NAD83 / UTM zone 15N", "m"));
            scene.WorldBounds = new SKRect(0, 0, 100, 100);
            scene.InteractionState.RenderStyle.SelectionColor = SKColors.LimeGreen;

            using var engine = new DrawingEngine(400, 400);
            engine.UseScene(scene);
            engine.AddLayer(new TestInteractiveLayer(new SKPoint(40, 60)));

            var hit = engine.HitTest(engine.WorldToScreen(new SKPoint(40, 60)));
            engine.RecordSelection(hit, replaceExisting: true);

            using var image = engine.RenderToImage();
            using var bitmap = SKBitmap.FromImage(image);
            var anchor = engine.WorldToScreen(new SKPoint(40, 60));
            var color = bitmap.GetPixel((int)anchor.X, (int)anchor.Y);

            Assert.Equal(SKColors.LimeGreen, color);
        }

        [Fact]
        public void RenderToImage_PersistedSelection_CanBeHiddenByStyle()
        {
            var scene = DrawingScene.CreateMapScene(
                "Hidden Selection Render",
                CoordinateReferenceSystem.CreateProjected("EPSG:26915", "NAD83 / UTM zone 15N", "m"));
            scene.WorldBounds = new SKRect(0, 0, 100, 100);
            scene.InteractionState.RenderStyle.IsVisible = false;

            using var engine = new DrawingEngine(400, 400);
            engine.UseScene(scene);
            engine.AddLayer(new TestInteractiveLayer(new SKPoint(40, 60)));

            var hit = engine.HitTest(engine.WorldToScreen(new SKPoint(40, 60)));
            engine.RecordSelection(hit, replaceExisting: true);

            using var image = engine.RenderToImage();
            using var bitmap = SKBitmap.FromImage(image);
            var anchor = engine.WorldToScreen(new SKPoint(40, 60));
            var color = bitmap.GetPixel((int)anchor.X, (int)anchor.Y);

            Assert.Equal(SKColors.White, color);
        }

        private sealed class TestInteractiveLayer : LayerBase, IInteractiveLayer
        {
            private readonly SKPoint _anchor;

            public TestInteractiveLayer(SKPoint anchor)
                : base("Test Interaction")
            {
                _anchor = anchor;
            }

            protected override void RenderContent(SKCanvas canvas, Viewport viewport)
            {
            }

            public override SKRect GetBounds()
            {
                return new SKRect(_anchor.X - 1f, _anchor.Y - 1f, _anchor.X + 1f, _anchor.Y + 1f);
            }

            public LayerHitResult HitTest(SKPoint worldPoint, float worldTolerance)
            {
                if (SKPoint.Distance(worldPoint, _anchor) <= worldTolerance)
                {
                    return new LayerHitResult(Name, "feature-1", "Selection Target", "Target", _anchor, 0f, new Dictionary<string, string>());
                }

                return null!;
            }
        }
    }
}
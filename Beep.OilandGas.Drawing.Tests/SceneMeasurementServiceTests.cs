using System;
using Beep.OilandGas.Drawing.CoordinateSystems;
using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.Measurements;
using Beep.OilandGas.Drawing.Scenes;
using SkiaSharp;
using Xunit;

namespace Beep.OilandGas.Drawing.Tests
{
    public class SceneMeasurementServiceTests
    {
        [Fact]
        public void MeasureDistance_ProjectedMap_ReturnsPlanarDistanceInSceneUnits()
        {
            var scene = DrawingScene.CreateMapScene(
                "Projected Map",
                CoordinateReferenceSystem.CreateProjected("EPSG:26915", "NAD83 / UTM zone 15N", "m"));

            var result = SceneMeasurementService.MeasureDistance(scene, new[]
            {
                new SKPoint(0, 0),
                new SKPoint(3, 4)
            });

            Assert.Equal(SceneMeasurementKind.Distance, result.Kind);
            Assert.Equal(SceneMeasurementGeometryKind.Segment, result.GeometryKind);
            Assert.Equal(5d, result.Value, 6);
            Assert.Equal("m", result.UnitCode);
            Assert.False(result.IsApproximate);
        }

        [Fact]
        public void MeasureArea_ProjectedMap_ReturnsPlanarAreaInSquareUnits()
        {
            var scene = DrawingScene.CreateMapScene(
                "Projected Map",
                CoordinateReferenceSystem.CreateProjected("EPSG:32613", "WGS 84 / UTM zone 13N", "m"));

            var result = SceneMeasurementService.MeasureArea(scene, new[]
            {
                new SKPoint(0, 0),
                new SKPoint(10, 0),
                new SKPoint(10, 20),
                new SKPoint(0, 20)
            });

            Assert.Equal(SceneMeasurementKind.Area, result.Kind);
            Assert.Equal(200d, result.Value, 6);
            Assert.Equal("m2", result.UnitCode);
            Assert.Equal("m^2", result.UnitLabel);
            Assert.False(result.IsApproximate);
        }

        [Fact]
        public void MeasureDistance_GeographicMap_UsesGeodesicMeters()
        {
            var scene = DrawingScene.CreateMapScene("Geographic Map", CoordinateReferenceSystem.CreateGeographicCrs84());

            var result = SceneMeasurementService.MeasureDistance(scene, new[]
            {
                new SKPoint(0, 0),
                new SKPoint(1, 0)
            });

            Assert.Equal("m", result.UnitCode);
            Assert.True(result.IsApproximate);
            Assert.InRange(result.Value, 111000d, 112000d);
        }

        [Fact]
        public void MeasureArea_DepthScene_Throws()
        {
            var scene = DrawingScene.CreateDepthScene("Depth Profile", "ft");

            var exception = Assert.Throws<InvalidOperationException>(() =>
                SceneMeasurementService.MeasureArea(scene, new[]
                {
                    new SKPoint(0, 0),
                    new SKPoint(0, 100),
                    new SKPoint(0, 200)
                }));

            Assert.Contains("Area measurements are not supported", exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void DrawingEngine_MeasureDistance_UsesScreenSpaceWorkflow()
        {
            var scene = DrawingScene.CreateMapScene(
                "Interaction Map",
                CoordinateReferenceSystem.CreateProjected("EPSG:26915", "NAD83 / UTM zone 15N", "m"));
            scene.WorldBounds = new SKRect(0, 0, 100, 100);

            using var engine = new DrawingEngine(1000, 1000);
            engine.UseScene(scene);

            var startScreen = engine.WorldToScreen(new SKPoint(10, 20));
            var endScreen = engine.WorldToScreen(new SKPoint(40, 60));
            var result = engine.MeasureDistance(startScreen, endScreen);

            Assert.Equal(50d, result.Value, 3);
            Assert.Equal("m", result.UnitCode);
        }
    }
}
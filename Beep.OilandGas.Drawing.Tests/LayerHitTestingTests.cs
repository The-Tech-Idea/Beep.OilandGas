using System.Collections.Generic;
using System.Globalization;
using Beep.OilandGas.Drawing.Builders;
using Beep.OilandGas.Drawing.CoordinateSystems;
using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Rendering;
using Beep.OilandGas.Drawing.Scenes;
using Beep.OilandGas.Drawing.Visualizations.FieldMap;
using Beep.OilandGas.Drawing.Visualizations.Reservoir;
using Beep.OilandGas.Models;
using SkiaSharp;
using Xunit;

namespace Beep.OilandGas.Drawing.Tests
{
    public class LayerHitTestingTests
    {
        [Fact]
        public void HitTest_FieldMapPoint_ReturnsPointAsset()
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

            Assert.NotNull(hit);
            Assert.Equal("well-1", hit.FeatureId);
            Assert.Equal("A-01", hit.FeatureLabel);
            Assert.Equal("Well", hit.FeatureKind);
            Assert.Equal("Beep", hit.Metadata["Operator"]);
        }

        [Fact]
        public void HitTest_FieldMapArea_ReturnsPolygonAsset()
        {
            var scene = DrawingScene.CreateMapScene(
                "Field Map",
                CoordinateReferenceSystem.CreateProjected("EPSG:26915", "NAD83 / UTM zone 15N", "m"));
            scene.WorldBounds = new SKRect(0, 0, 100, 100);

            var data = new FieldMapData();
            data.AreaAssets.Add(new FieldMapAreaAsset
            {
                AssetId = "lease-1",
                AssetName = "Lease A",
                AssetKind = FieldMapAssetKind.Lease,
                BoundaryPoints =
                {
                    new Point3D { X = 10, Y = 10, Z = 0 },
                    new Point3D { X = 80, Y = 10, Z = 0 },
                    new Point3D { X = 80, Y = 70, Z = 0 },
                    new Point3D { X = 10, Y = 70, Z = 0 }
                }
            });

            using var engine = new DrawingEngine(1000, 1000);
            engine.UseScene(scene);
            engine.AddLayer(new FieldMapLayer(data, new FieldMapConfiguration { ShowLegend = false }));

            var hit = engine.HitTest(engine.WorldToScreen(new SKPoint(30, 30)));

            Assert.NotNull(hit);
            Assert.Equal("lease-1", hit.FeatureId);
            Assert.Equal("Lease", hit.FeatureKind);
        }

        [Fact]
        public void HitTest_ReservoirOverlayWell_ReturnsWellHit()
        {
            var scene = DrawingScene.CreateMapScene(
                "Reservoir Map",
                CoordinateReferenceSystem.CreateProjected("EPSG:26915", "NAD83 / UTM zone 15N", "m"));
            scene.WorldBounds = new SKRect(0, 0, 100, 100);

            var well = new ReservoirWellMapPoint
            {
                WellId = "well-a01",
                WellName = "A01",
                Uwi = "100011223344W500",
                SurfaceLocation = new Point3D { X = 55, Y = 45, Z = 0 },
                Metadata = new Dictionary<string, string> { ["Status"] = "Producer" }
            };

            using var engine = new DrawingEngine(1000, 1000);
            engine.UseScene(scene);
            engine.AddLayer(new ReservoirMapOverlayLayer(wells: new[] { well }));

            var hit = engine.HitTest(engine.WorldToScreen(new SKPoint(55, 45)));

            Assert.NotNull(hit);
            Assert.Equal("well-a01", hit.FeatureId);
            Assert.Equal("A01", hit.FeatureLabel);
            Assert.Equal("Well", hit.FeatureKind);
            Assert.Equal("Producer", hit.Metadata["Status"]);
        }

        [Fact]
        public void HitTest_ReservoirContour_ReturnsContourHit()
        {
            var scene = DrawingScene.CreateMapScene(
                "Reservoir Contour",
                CoordinateReferenceSystem.CreateProjected("EPSG:26915", "NAD83 / UTM zone 15N", "m"));
            scene.WorldBounds = new SKRect(0, 0, 100, 100);

            var surface = new ReservoirSurfaceData
            {
                SurfaceId = "structure-top",
                SurfaceName = "Top Reservoir"
            };

            surface.Points.AddRange(new[]
            {
                new Point3D { X = 0, Y = 0, Z = 0 },
                new Point3D { X = 100, Y = 0, Z = 100 },
                new Point3D { X = 0, Y = 100, Z = 100 },
                new Point3D { X = 100, Y = 100, Z = 200 }
            });

            using var engine = new DrawingEngine(1000, 1000);
            engine.UseScene(scene);
            engine.AddLayer(new ReservoirContourLayer(surface, new ReservoirContourConfiguration
            {
                ContourInterval = 50,
                SampleColumns = 20,
                SampleRows = 20,
                ShowContourLabels = false
            }));

            var hit = engine.HitTest(engine.WorldToScreen(new SKPoint(50, 50)));

            Assert.NotNull(hit);
            Assert.Equal("Contour", hit.FeatureKind);
            Assert.Equal("Reservoir Contours", hit.LayerName);
            Assert.Equal("Contour 100", hit.FeatureLabel);
            Assert.Equal("100", hit.Metadata["Level"]);
        }

        [Fact]
        public void HitTest_WellLogTrack_UsesDepthSceneCoordinates()
        {
            var logData = CreateWellLogTestData();
            var configuration = new LogRendererConfiguration
            {
                UseStandardTrackTemplates = false,
                ShowWellborePath = false,
                ShowDepthMarkers = false,
                Tracks = new List<LogTrackDefinition>
                {
                    new()
                    {
                        Kind = LogTrackKind.Depth,
                        Name = "Depth",
                        Width = 72f,
                        MajorInterval = 100,
                        MinorSubdivisionCount = 4,
                        LabelFormat = "F0"
                    },
                    new()
                    {
                        Name = "Gamma Ray",
                        Curves = new List<LogTrackCurveDefinition>
                        {
                            new()
                            {
                                CurveName = "GR",
                                DisplayName = "Gamma Ray",
                                MinValue = 0,
                                MaxValue = 150
                            }
                        }
                    }
                }
            };

            using var engine = WellLogBuilder.Create()
                .WithLogData(logData)
                .WithConfiguration(configuration)
                .WithSize(600, 1200)
                .Build();

            Assert.NotNull(engine.ActiveScene);
            Assert.Equal(DrawingSceneKind.Depth, engine.ActiveScene.Kind);

            var hit = engine.HitTest(engine.WorldToScreen(new SKPoint(176f, 9925f)), screenTolerance: 12f);
            var distance = engine.MeasureDistance(
                engine.WorldToScreen(new SKPoint(132f, 9900f)),
                engine.WorldToScreen(new SKPoint(132f, 10000f)));

            Assert.NotNull(hit);
            Assert.Equal("Log Track", hit.FeatureKind);
            Assert.Equal("Gamma Ray", hit.FeatureLabel);
            Assert.Equal("Curve", hit.Metadata["TrackKind"]);
            Assert.Equal("feet", hit.Metadata["DepthUnit"]);
            Assert.True(hit.Metadata.ContainsKey("Value:GR"));
            Assert.Equal("ft", distance.UnitCode);
            Assert.Equal(100d, distance.Value, 3);
        }

        [Fact]
        public void HitTest_WellLogCurve_ReturnsExactCurveHit()
        {
            using var engine = WellLogBuilder.Create()
                .WithLogData(CreateWellLogCurveInteractionData())
                .WithConfiguration(new LogRendererConfiguration
                {
                    UseStandardTrackTemplates = false,
                    ShowTrackHeaders = false,
                    ShowTrackScaleAnnotations = false,
                    ShowDepthScale = false,
                    RenderDepthScaleAsTrack = false,
                    ShowWellborePath = false,
                    ShowDepthMarkers = false,
                    ShowGrid = false,
                    Tracks = new List<LogTrackDefinition>
                    {
                        new()
                        {
                            Name = "Gamma Ray",
                            Width = 100f,
                            Curves = new List<LogTrackCurveDefinition>
                            {
                                new()
                                {
                                    CurveName = "GR",
                                    DisplayName = "Gamma Ray",
                                    MinValue = 0,
                                    MaxValue = 100
                                }
                            }
                        }
                    }
                })
                .WithSize(400, 400)
                .Build();

            var hit = engine.HitTest(engine.WorldToScreen(new SKPoint(50f, 150f)), screenTolerance: 10f);

            Assert.NotNull(hit);
            Assert.Equal("Log Curve", hit.FeatureKind);
            Assert.Equal("Gamma Ray", hit.FeatureLabel);
            Assert.Equal("GR", hit.Metadata["CurveName"]);
            Assert.Equal("50", hit.Metadata["Value"]);
        }

        [Fact]
        public void HitTest_WellLogLithologyInterval_ReturnsExactIntervalHit()
        {
            using var engine = WellLogBuilder.Create()
                .WithLogData(CreateWellLogIntervalInteractionData())
                .WithConfiguration(new LogRendererConfiguration
                {
                    UseStandardTrackTemplates = false,
                    ShowTrackHeaders = false,
                    ShowTrackScaleAnnotations = false,
                    ShowDepthScale = false,
                    RenderDepthScaleAsTrack = false,
                    ShowWellborePath = false,
                    ShowDepthMarkers = false,
                    ShowGrid = false,
                    Tracks = new List<LogTrackDefinition>
                    {
                        new()
                        {
                            Kind = LogTrackKind.Lithology,
                            Name = "Lithology",
                            Width = 60f,
                            Intervals = new List<LogIntervalData>
                            {
                                new()
                                {
                                    IntervalId = "L1",
                                    Label = "Upper Sand",
                                    Lithology = "Sandstone",
                                    TopDepth = 120,
                                    BottomDepth = 160,
                                    PatternType = "sandstone"
                                }
                            }
                        }
                    }
                })
                .WithSize(400, 400)
                .Build();

            var hit = engine.HitTest(engine.WorldToScreen(new SKPoint(30f, 160f)), screenTolerance: 10f);

            Assert.NotNull(hit);
            Assert.Equal("Lithology Interval", hit.FeatureKind);
            Assert.Equal("Sandstone", hit.FeatureLabel);
            Assert.Equal("L1", hit.Metadata["IntervalId"]);
            Assert.Equal("Sandstone", hit.Metadata["Lithology"]);
        }

        [Fact]
        public void HitTest_WellLogDensityNeutronCrossover_ReturnsCompositeHit()
        {
            using var engine = WellLogBuilder.Create()
                .WithLogData(CreateWellLogCrossoverInteractionData())
                .WithConfiguration(new LogRendererConfiguration
                {
                    UseStandardTrackTemplates = false,
                    ShowTrackHeaders = false,
                    ShowTrackScaleAnnotations = false,
                    ShowDepthScale = false,
                    RenderDepthScaleAsTrack = false,
                    ShowWellborePath = false,
                    ShowDepthMarkers = false,
                    ShowGrid = false,
                    ShowDensityNeutronCrossoverShading = true,
                    Tracks = new List<LogTrackDefinition>
                    {
                        new()
                        {
                            Name = "Density-Neutron",
                            Width = 100f,
                            Curves = new List<LogTrackCurveDefinition>
                            {
                                new()
                                {
                                    CurveName = "DPHI",
                                    DisplayName = "Density Porosity",
                                    MinValue = -0.15,
                                    MaxValue = 0.45
                                },
                                new()
                                {
                                    CurveName = "NPHI",
                                    DisplayName = "Neutron Porosity",
                                    MinValue = -0.15,
                                    MaxValue = 0.45
                                }
                            }
                        }
                    }
                })
                .WithSize(400, 400)
                .Build();

            var hit = engine.HitTest(engine.WorldToScreen(new SKPoint(60f, 150f)), screenTolerance: 6f);

            Assert.NotNull(hit);
            Assert.Equal("Density-Neutron Crossover", hit.FeatureKind);
            Assert.Equal("Density Porosity / Neutron Porosity", hit.FeatureLabel);
            Assert.Equal("DPHI", hit.Metadata["DensityCurveName"]);
            Assert.Equal("NPHI", hit.Metadata["NeutronCurveName"]);
            Assert.Equal("100", hit.Metadata["TopDepth"]);
            Assert.True(double.TryParse(hit.Metadata["BottomDepth"], NumberStyles.Float, CultureInfo.InvariantCulture, out var bottomDepth));
            Assert.True(bottomDepth >= 150d);
        }

        [Fact]
        public void HitTest_EnhancedWellSchematicEquipment_ReturnsEquipmentHit()
        {
            using var engine = WellSchematicBuilder.Create()
                .WithWellData(CreateWellSchematicTestData())
                .WithSize(900, 1600)
                .WithGrid(true)
                .WithAnnotations(true)
                .Build();

            Assert.NotNull(engine.ActiveScene);
            Assert.Equal(DrawingSceneKind.Depth, engine.ActiveScene.Kind);

            var bounds = engine.Layers[0].GetBounds();
            float anchorDepth = (6800f + 6940f) * 0.5f;
            float anchorY = bounds.Height * (1f - (anchorDepth / 8500f));
            var screenPoint = engine.WorldToScreen(new SKPoint(bounds.MidX, anchorY));

            var hit = engine.HitTest(screenPoint, screenTolerance: 240f);

            Assert.NotNull(hit);
            Assert.Equal("Equipment", hit.FeatureKind);
            Assert.Equal("Production Packer", hit.FeatureLabel);
            Assert.Equal("Packer", hit.Metadata["EquipmentType"]);
        }

        [Fact]
        public void HitTest_EnhancedWellSchematicEquipmentCallout_ReturnsEquipmentHit()
        {
            using var engine = WellSchematicBuilder.Create()
                .WithWellData(CreateWellSchematicTestData())
                .WithSize(900, 1600)
                .WithGrid(true)
                .WithAnnotations(true)
                .Build();

            var bounds = engine.Layers[0].GetBounds();
            float anchorDepth = (6800f + 6940f) * 0.5f;
            float anchorY = bounds.Height * (1f - (anchorDepth / 8500f));
            var screenPoint = engine.WorldToScreen(new SKPoint(bounds.Right - 200f, anchorY));

            var hit = engine.HitTest(screenPoint, screenTolerance: 10f);

            Assert.NotNull(hit);
            Assert.Equal("Equipment", hit.FeatureKind);
            Assert.Equal("Production Packer", hit.FeatureLabel);
        }

        [Fact]
        public void HitTest_EnhancedWellSchematicPerforationCallout_ReturnsPerforationHit()
        {
            using var engine = WellSchematicBuilder.Create()
                .WithWellData(CreateWellSchematicTestData())
                .WithSize(900, 1600)
                .WithGrid(true)
                .WithAnnotations(true)
                .Build();

            var bounds = engine.Layers[0].GetBounds();
            float anchorDepth = (7200f + 7425f) * 0.5f;
            float anchorY = bounds.Height * (1f - (anchorDepth / 8500f));
            var screenPoint = engine.WorldToScreen(new SKPoint(bounds.Right - 220f, anchorY));

            var hit = engine.HitTest(screenPoint, screenTolerance: 10f);

            Assert.NotNull(hit);
            Assert.Equal("Perforation", hit.FeatureKind);
            Assert.Equal("Gun", hit.Metadata["PerfType"]);
        }

        private static WellData CreateWellSchematicTestData()
        {
            var wellData = new WellData
            {
                UWI = "100012345678W500",
                UBHI = "A-12H"
            };

            var borehole = new WellData_Borehole
            {
                UWI = wellData.UWI,
                UBHI = wellData.UBHI,
                BoreHoleIndex = 0,
                TopDepth = 0,
                BottomDepth = 8500,
                Diameter = 8.5f,
                IsVertical = true,
                OuterDiameterOffset = 24f,
                Casing =
                {
                    new WellData_Casing
                    {
                        UWI = wellData.UWI,
                        UBHI = wellData.UBHI,
                        BoreHoleIndex = 0,
                        TopDepth = 0,
                        BottomDepth = 7900,
                        OUTER_DIAMETER = 7.0f,
                        INNER_DIAMETER = 6.2f,
                        CasingType = "Production"
                    }
                },
                Tubing =
                {
                    new WellData_Tubing
                    {
                        UWI = wellData.UWI,
                        UBHI = wellData.UBHI,
                        BoreHoleIndex = 0,
                        TubeIndex = 0,
                        TopDepth = 0,
                        BottomDepth = 7700,
                        Diameter = 3.5f
                    }
                },
                Equip =
                {
                    new WellData_Equip
                    {
                        UWI = wellData.UWI,
                        UBHI = wellData.UBHI,
                        BoreHoleIndex = 0,
                        TopDepth = 6800,
                        BottomDepth = 6940,
                        Diameter = 4.2f,
                        EquipmentType = "Packer",
                        EquipmentName = "Production Packer",
                        EquipmentDescription = "Regression packer"
                    }
                },
                Perforation =
                {
                    new WellData_Perf
                    {
                        UWI = wellData.UWI,
                        UBHI = wellData.UBHI,
                        BoreHoleIndex = 0,
                        TopDepth = 7200,
                        BottomDepth = 7425,
                        PerfType = "Gun"
                    }
                }
            };

            wellData.BoreHoles.Add(borehole);
            return wellData;
        }

        private static LogData CreateWellLogTestData()
        {
            var depths = new List<double> { 9800, 9850, 9900, 9950, 10000, 10050, 10100, 10150, 10200, 10250, 10300, 10350, 10400 };

            return new LogData
            {
                WellIdentifier = "A-12H",
                LogName = "Regression Gamma",
                LogType = "Wireline",
                DepthUnit = "feet",
                StartDepth = 9800,
                EndDepth = 10400,
                DepthStep = 50,
                Depths = depths,
                Curves =
                {
                    ["GR"] = new List<double> { 48, 52, 57, 63, 68, 72, 75, 73, 69, 66, 61, 56, 51 }
                },
                CurveMetadata =
                {
                    ["GR"] = new LogCurveMetadata
                    {
                        Mnemonic = "GR",
                        DisplayName = "Gamma Ray",
                        Unit = "gAPI",
                        MinValue = 0,
                        MaxValue = 150
                    }
                }
            };
        }

        private static LogData CreateWellLogCurveInteractionData()
        {
            return new LogData
            {
                WellIdentifier = "CURVE-1",
                LogName = "Curve Interaction",
                LogType = "Wireline",
                DepthUnit = "feet",
                StartDepth = 100,
                EndDepth = 200,
                DepthStep = 50,
                Depths = new List<double> { 100, 150, 200 },
                Curves =
                {
                    ["GR"] = new List<double> { 10, 50, 90 }
                },
                CurveMetadata =
                {
                    ["GR"] = new LogCurveMetadata
                    {
                        Mnemonic = "GR",
                        DisplayName = "Gamma Ray",
                        Unit = "gAPI",
                        MinValue = 0,
                        MaxValue = 100
                    }
                }
            };
        }

        private static LogData CreateWellLogIntervalInteractionData()
        {
            return new LogData
            {
                WellIdentifier = "INT-1",
                LogName = "Interval Interaction",
                LogType = "Interpretation",
                DepthUnit = "feet",
                StartDepth = 100,
                EndDepth = 200,
                DepthStep = 20,
                Depths = new List<double> { 100, 120, 140, 160, 180, 200 }
            };
        }

        private static LogData CreateWellLogCrossoverInteractionData()
        {
            return new LogData
            {
                WellIdentifier = "XOVER-1",
                LogName = "Crossover Interaction",
                LogType = "Petrophysical",
                DepthUnit = "feet",
                StartDepth = 100,
                EndDepth = 200,
                DepthStep = 50,
                Depths = new List<double> { 100, 150, 200 },
                Curves =
                {
                    ["DPHI"] = new List<double> { 0.05, 0.10, 0.15 },
                    ["NPHI"] = new List<double> { 0.35, 0.30, 0.25 }
                },
                CurveMetadata =
                {
                    ["DPHI"] = new LogCurveMetadata
                    {
                        Mnemonic = "DPHI",
                        DisplayName = "Density Porosity",
                        Unit = "v/v",
                        MinValue = -0.15,
                        MaxValue = 0.45
                    },
                    ["NPHI"] = new LogCurveMetadata
                    {
                        Mnemonic = "NPHI",
                        DisplayName = "Neutron Porosity",
                        Unit = "v/v",
                        MinValue = -0.15,
                        MaxValue = 0.45
                    }
                }
            };
        }
    }
}
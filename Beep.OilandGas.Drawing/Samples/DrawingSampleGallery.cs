using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using Beep.OilandGas.Drawing.Builders;
using Beep.OilandGas.Drawing.CoordinateSystems;
using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Export;
using Beep.OilandGas.Drawing.Primitives;
using Beep.OilandGas.Drawing.Rendering;
using Beep.OilandGas.Drawing.Visualizations.FieldMap;
using Beep.OilandGas.Drawing.Visualizations.Reservoir;
using Beep.OilandGas.Models;

namespace Beep.OilandGas.Drawing.Samples
{
    /// <summary>
    /// Provides a canonical set of in-memory sample scenes for docs, demos, exports, and regression tests.
    /// </summary>
    public static class DrawingSampleGallery
    {
        private static readonly IReadOnlyList<DrawingSampleScene> StandardScenes = new ReadOnlyCollection<DrawingSampleScene>(new[]
        {
            new DrawingSampleScene(
                "FieldMap_AssetNetwork",
                "Plan-view field map with leases, infrastructure, seismic, land-right, protected-area, and hazard overlays.",
                1600,
                1000,
                CreateFieldMapAssetNetworkScene,
                CreateFieldMapAssetNetworkExports()),
            new DrawingSampleScene(
                "WellLog_Petrophysical",
                "Standard petrophysical panel with gamma ray, resistivity, density, neutron, lithology, and zonation.",
                1500,
                1800,
                CreateWellLogPetrophysicalScene),
            new DrawingSampleScene(
                "ReservoirContour_StructureMap",
                "Interpolated reservoir structure map with contour labels and deterministic sampling.",
                1600,
                1000,
                CreateReservoirContourStructureMapScene,
                CreateReservoirContourStructureMapExports()),
            new DrawingSampleScene(
                "ReservoirCrossSection_Midline",
                "Reservoir cross-section along a fixed section line with projected well trajectory and contacts.",
                1600,
                900,
                CreateReservoirCrossSectionMidlineScene,
                CreateReservoirCrossSectionMidlineExports()),
            new DrawingSampleScene(
                "WellSchematic_EnhancedVertical",
                "Vertical completion schematic with casing, tubing, packer, and perforated interval.",
                900,
                1600,
                CreateWellSchematicEnhancedVerticalScene,
                CreateWellSchematicEnhancedVerticalExports())
        });

        /// <summary>
        /// Gets the canonical built-in sample scene descriptors.
        /// </summary>
        public static IReadOnlyList<DrawingSampleScene> GetStandardScenes()
        {
            return StandardScenes;
        }

        /// <summary>
        /// Gets a sample scene descriptor by its stable name.
        /// </summary>
        public static DrawingSampleScene GetStandardScene(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            var scene = StandardScenes.FirstOrDefault(candidate => string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase));
            if (scene == null)
                throw new KeyNotFoundException($"Unknown drawing sample scene '{name}'.");

            return scene;
        }

        /// <summary>
        /// Builds a standard sample scene by name.
        /// </summary>
        public static DrawingEngine CreateStandardScene(string name)
        {
            return GetStandardScene(name).CreateEngine();
        }

        /// <summary>
        /// Builds the standard field-map asset-network sample scene.
        /// </summary>
        public static DrawingEngine CreateFieldMapAssetNetworkScene()
        {
            var fieldMap = CreateFieldMapAssetNetworkData();

            return FieldMapBuilder.Create()
                .WithFieldMapData(fieldMap)
                .WithConfiguration(new FieldMapConfiguration
                {
                    LegendAnchor = OverlayAnchor.BottomRight,
                    ShowConnectionLabels = true,
                    ShowAreaLabels = true,
                    LabelFontSize = 11f
                })
                .WithSize(1600, 1000)
                .Build();
        }

        private static IReadOnlyList<DrawingSampleExportAction> CreateFieldMapAssetNetworkExports()
        {
            return new[]
            {
                new DrawingSampleExportAction(
                    id: "field-map-geojson",
                    label: "Field Map GeoJSON",
                    fileNameSuffix: "field-map.geojson",
                    contentType: "application/geo+json",
                    export: _ => Encoding.UTF8.GetBytes(GeoJsonExporter.ExportFieldMapToString(CreateFieldMapAssetNetworkData())),
                    description: "Feature collection export for the canonical field map assets and network."),
                new DrawingSampleExportAction(
                    id: "field-map-georeferenced-png-bundle",
                    label: "Geo-Referenced PNG",
                    fileNameSuffix: "georeferenced.zip",
                    contentType: "application/zip",
                    export: engine => CreateGeoReferencedPngBundle(engine, "field-map"),
                    description: "Zip bundle containing the PNG, world file, and CRS sidecar for the canonical field map.")
            };
        }

        private static byte[] CreateGeoReferencedPngBundle(DrawingEngine engine, string artifactStem)
        {
            if (engine == null)
                throw new ArgumentNullException(nameof(engine));

            var workingDirectory = Path.Combine(Path.GetTempPath(), $"beep-drawing-sample-export-{Guid.NewGuid():N}");
            var imagePath = Path.Combine(workingDirectory, $"{artifactStem}.png");

            try
            {
                Directory.CreateDirectory(workingDirectory);
                GeoReferencedImageExporter.ExportToPng(engine, imagePath);

                using var stream = new MemoryStream();
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
                {
                    archive.CreateEntryFromFile(imagePath, Path.GetFileName(imagePath));

                    var worldFilePath = GeoReferencedImageExporter.GetWorldFilePath(imagePath);
                    archive.CreateEntryFromFile(worldFilePath, Path.GetFileName(worldFilePath));

                    var crsMetadataPath = GeoReferencedImageExporter.GetCoordinateReferenceMetadataPath(imagePath);
                    archive.CreateEntryFromFile(crsMetadataPath, Path.GetFileName(crsMetadataPath));
                }

                return stream.ToArray();
            }
            finally
            {
                if (Directory.Exists(workingDirectory))
                {
                    Directory.Delete(workingDirectory, recursive: true);
                }
            }
        }

        private static FieldMapData CreateFieldMapAssetNetworkData()
        {
            return new FieldMapData
            {
                MapName = "North Dome Asset Network",
                CoordinateReferenceSystem = CoordinateReferenceSystem.CreateProjected("EPSG:26915", "NAD83 / UTM zone 15N", "m"),
                BoundingBox = new BoundingBox
                {
                    MinX = 0,
                    MaxX = 3200,
                    MinY = 0,
                    MaxY = 2200,
                    MinZ = 0,
                    MaxZ = 0
                }
            }
            .With(fieldMap =>
            {
                fieldMap.AreaAssets.Add(new FieldMapAreaAsset
                {
                    AssetId = "FIELD-01",
                    AssetName = "North Dome",
                    BoundaryPoints =
                    {
                        new Point3D { X = 0, Y = 0 },
                        new Point3D { X = 3200, Y = 0 },
                        new Point3D { X = 3200, Y = 2200 },
                        new Point3D { X = 0, Y = 2200 }
                    }
                });
                fieldMap.AreaAssets.Add(new FieldMapAreaAsset
                {
                    AssetId = "LEASE-A",
                    AssetName = "Lease A",
                    BoundaryPoints =
                    {
                        new Point3D { X = 250, Y = 250 },
                        new Point3D { X = 1600, Y = 250 },
                        new Point3D { X = 1600, Y = 1450 },
                        new Point3D { X = 250, Y = 1450 }
                    }
                });
                fieldMap.AreaAssets.Add(new FieldMapAreaAsset
                {
                    AssetId = "SEIS-3D-01",
                    AssetName = "Phase A 3D",
                    BoundaryPoints =
                    {
                        new Point3D { X = 900, Y = 700 },
                        new Point3D { X = 2900, Y = 700 },
                        new Point3D { X = 2900, Y = 1900 },
                        new Point3D { X = 900, Y = 1900 }
                    }
                });
                fieldMap.AreaAssets.Add(new FieldMapAreaAsset
                {
                    AssetId = "ROW-07",
                    AssetName = "Pipeline ROW",
                    BoundaryPoints =
                    {
                        new Point3D { X = 1700, Y = 300 },
                        new Point3D { X = 1850, Y = 300 },
                        new Point3D { X = 1850, Y = 2100 },
                        new Point3D { X = 1700, Y = 2100 }
                    }
                });
                fieldMap.AreaAssets.Add(new FieldMapAreaAsset
                {
                    AssetId = "PA-02",
                    AssetName = "Wetland Preserve",
                    BoundaryPoints =
                    {
                        new Point3D { X = 2200, Y = 200 },
                        new Point3D { X = 3000, Y = 200 },
                        new Point3D { X = 3000, Y = 900 },
                        new Point3D { X = 2200, Y = 900 }
                    }
                });
                fieldMap.AreaAssets.Add(new FieldMapAreaAsset
                {
                    AssetId = "HZ-12",
                    AssetName = "Sour Gas Buffer",
                    BoundaryPoints =
                    {
                        new Point3D { X = 2100, Y = 1200 },
                        new Point3D { X = 2600, Y = 1200 },
                        new Point3D { X = 2600, Y = 1700 },
                        new Point3D { X = 2100, Y = 1700 }
                    }
                });
                fieldMap.PointAssets.Add(new FieldMapPointAsset
                {
                    AssetId = "CPF-1",
                    AssetName = "Central Facility",
                    Location = new Point3D { X = 2050, Y = 980, Z = 0 }
                });
                fieldMap.PointAssets.Add(new FieldMapPointAsset
                {
                    AssetId = "WELL-A12",
                    AssetName = "A-12H",
                    Location = new Point3D { X = 840, Y = 860, Z = 0 }
                });
                fieldMap.PointAssets.Add(new FieldMapPointAsset
                {
                    AssetId = "MAN-03",
                    AssetName = "Test Manifold",
                    Location = new Point3D { X = 1380, Y = 930, Z = 0 }
                });
                fieldMap.ConnectionAssets.Add(new FieldMapConnectionAsset
                {
                    ConnectionId = "FL-001",
                    ConnectionName = "A Pad Flowline",
                    FromAssetId = "WELL-A12",
                    ToAssetId = "MAN-03",
                    Vertices =
                    {
                        new Point3D { X = 840, Y = 860, Z = 0 },
                        new Point3D { X = 1120, Y = 900, Z = 0 },
                        new Point3D { X = 1380, Y = 930, Z = 0 }
                    }
                });
                fieldMap.ConnectionAssets.Add(new FieldMapConnectionAsset
                {
                    ConnectionId = "GL-010",
                    ConnectionName = "Gathering Spur",
                    FromAssetId = "MAN-03",
                    ToAssetId = "CPF-1",
                    Vertices =
                    {
                        new Point3D { X = 1380, Y = 930, Z = 0 },
                        new Point3D { X = 1710, Y = 955, Z = 0 },
                        new Point3D { X = 2050, Y = 980, Z = 0 }
                    }
                });
            });
        }

        private static T With<T>(this T value, Action<T> configure)
        {
            configure(value);
            return value;
        }

        /// <summary>
        /// Builds the standard petrophysical well-log sample scene.
        /// </summary>
        public static DrawingEngine CreateWellLogPetrophysicalScene()
        {
            List<double> depths = Enumerable.Range(0, 25)
                .Select(index => 9800.0 + (index * 25.0))
                .ToList();

            var logData = new LogData
            {
                WellIdentifier = "A-12H",
                LogName = "Regression Petrophysical",
                LogType = "Wireline",
                StartDepth = depths.First(),
                EndDepth = depths.Last(),
                DepthStep = 25.0,
                Depths = depths,
                Curves =
                {
                    ["GR"] = depths.Select((_, index) => 55.0 + (18.0 * Math.Sin(index / 3.0)) + (index * 0.9)).ToList(),
                    ["RESD"] = depths.Select((_, index) => Math.Exp(0.65 + (index * 0.085))).ToList(),
                    ["RHOB"] = depths.Select((_, index) => 2.35 + (0.12 * Math.Cos(index / 4.0))).ToList(),
                    ["NPHI"] = depths.Select((_, index) => 0.28 - (0.09 * Math.Sin(index / 5.0))).ToList()
                },
                CurveMetadata =
                {
                    ["GR"] = new LogCurveMetadata { Mnemonic = "GR", DisplayName = "GammaRay", Unit = "gAPI", MinValue = 0, MaxValue = 150 },
                    ["RESD"] = new LogCurveMetadata { Mnemonic = "RESD", DisplayName = "DeepResistivity", Unit = "ohm.m", MinValue = 0.2, MaxValue = 2000 },
                    ["RHOB"] = new LogCurveMetadata { Mnemonic = "RHOB", DisplayName = "BulkDensity", Unit = "g/cc", MinValue = 1.95, MaxValue = 2.95 },
                    ["NPHI"] = new LogCurveMetadata { Mnemonic = "NPHI", DisplayName = "NeutronPorosity", Unit = "v/v", MinValue = -0.15, MaxValue = 0.45 }
                },
                LithologyIntervals =
                {
                    new LogIntervalData { IntervalId = "L1", Label = "Upper Sand", TopDepth = 9800, BottomDepth = 9925, Lithology = "Sandstone", PatternType = "sandstone" },
                    new LogIntervalData { IntervalId = "L2", Label = "Middle Shale", TopDepth = 9925, BottomDepth = 10075, Lithology = "Shale", PatternType = "shale" },
                    new LogIntervalData { IntervalId = "L3", Label = "Lower Carbonate", TopDepth = 10075, BottomDepth = 10400, Lithology = "Limestone", PatternType = "limestone" }
                },
                ZoneIntervals =
                {
                    new LogIntervalData { IntervalId = "Z1", Label = "A Sand", TopDepth = 9800, BottomDepth = 9950, Facies = "Channel" },
                    new LogIntervalData { IntervalId = "Z2", Label = "B Shale", TopDepth = 9950, BottomDepth = 10125, Facies = "Seal" },
                    new LogIntervalData { IntervalId = "Z3", Label = "C Carbonate", TopDepth = 10125, BottomDepth = 10400, Facies = "Platform" }
                }
            };

            return WellLogBuilder.Create()
                .WithLogData(logData)
                .WithConfiguration(new LogRendererConfiguration
                {
                    UseStandardTrackTemplates = true,
                    RenderDepthScaleAsTrack = true,
                    ShowDensityNeutronCrossoverShading = true,
                    ShowLogDecadeGridLines = true,
                    ShowTrackScaleAnnotations = true,
                    ShowTrackHeaders = true,
                    ShowCurveNames = true
                })
                .WithSize(1500, 1800)
                .Build();
        }

        /// <summary>
        /// Builds the standard reservoir contour structure-map sample scene.
        /// </summary>
        public static DrawingEngine CreateReservoirContourStructureMapScene()
        {
            ReservoirData reservoir = CreateReservoirRegressionData();

            return ReservoirContourBuilder.Create()
                .WithReservoirData(reservoir)
                .SelectSurface("Regression Top")
                .WithConfiguration(CreateReservoirContourStructureMapConfiguration())
                .WithSize(1600, 1000)
                .Build();
        }

        private static IReadOnlyList<DrawingSampleExportAction> CreateReservoirContourStructureMapExports()
        {
            return new[]
            {
                new DrawingSampleExportAction(
                    id: "reservoir-contour-geojson",
                    label: "Reservoir Contour GeoJSON",
                    fileNameSuffix: "reservoir-contour.geojson",
                    contentType: "application/geo+json",
                    export: _ =>
                    {
                        var reservoir = CreateReservoirRegressionData();
                        var contourSurface = reservoir.Surfaces.First(surface => string.Equals(surface.SurfaceName, "Regression Top", StringComparison.OrdinalIgnoreCase));
                        var geoJson = GeoJsonExporter.ExportReservoirMapToString(
                            contourSurface,
                            reservoir.CoordinateReferenceSystem,
                            contourConfiguration: CreateReservoirContourStructureMapConfiguration());
                        return Encoding.UTF8.GetBytes(geoJson);
                    },
                    description: "Contour-segment GeoJSON export for the canonical regression structure map."),
                new DrawingSampleExportAction(
                    id: "reservoir-contour-georeferenced-png-bundle",
                    label: "Geo-Referenced PNG",
                    fileNameSuffix: "georeferenced.zip",
                    contentType: "application/zip",
                    export: engine => CreateGeoReferencedPngBundle(engine, "reservoir-contour"),
                    description: "Zip bundle containing the PNG, world file, and CRS sidecar for the canonical regression structure map.")
            };
        }

        private static ReservoirContourConfiguration CreateReservoirContourStructureMapConfiguration()
        {
            return new ReservoirContourConfiguration
            {
                ContourInterval = 20,
                MajorContourEvery = 4,
                ShowContourLabels = true,
                SampleColumns = 40,
                SampleRows = 32
            };
        }

        /// <summary>
        /// Builds the standard reservoir cross-section sample scene.
        /// </summary>
        public static DrawingEngine CreateReservoirCrossSectionMidlineScene()
        {
            ReservoirData reservoir = CreateReservoirRegressionData();

            return ReservoirCrossSectionBuilder.Create()
                .WithReservoirData(reservoir)
                .WithSectionLine(CreateReservoirCrossSectionMidlineSectionLine())
                .WithWells(CreateReservoirCrossSectionMidlineWells())
                .WithConfiguration(CreateReservoirCrossSectionMidlineConfiguration())
                .WithSize(1600, 900)
                .Build();
        }

        private static IReadOnlyList<DrawingSampleExportAction> CreateReservoirCrossSectionMidlineExports()
        {
            return new[]
            {
                new DrawingSampleExportAction(
                    id: "reservoir-cross-section-json",
                    label: "Cross Section JSON",
                    fileNameSuffix: "reservoir-cross-section.json",
                    contentType: "application/json",
                    export: _ => JsonSerializer.SerializeToUtf8Bytes(CreateReservoirCrossSectionMidlineData()),
                    description: "Typed section profile export for the canonical midline reservoir cross-section.")
            };
        }

        private static ReservoirCrossSectionData CreateReservoirCrossSectionMidlineData()
        {
            var reservoir = CreateReservoirRegressionData();
            return ReservoirCrossSectionExtractor.Extract(
                reservoir,
                CreateReservoirCrossSectionMidlineSectionLine(),
                CreateReservoirCrossSectionMidlineConfiguration(),
                wells: CreateReservoirCrossSectionMidlineWells());
        }

        private static ReservoirSectionLine CreateReservoirCrossSectionMidlineSectionLine()
        {
            return new ReservoirSectionLine
            {
                SectionName = "A-A'",
                Start = new Point3D { X = 0, Y = 500, Z = 0 },
                End = new Point3D { X = 1000, Y = 500, Z = 0 }
            };
        }

        private static IReadOnlyList<ReservoirWellMapPoint> CreateReservoirCrossSectionMidlineWells()
        {
            return new[]
            {
                new ReservoirWellMapPoint
                {
                    WellId = "WELL-A12",
                    WellName = "A-12H",
                    Uwi = "100012345678W500",
                    SurfaceLocation = new Point3D { X = 420, Y = 520, Z = 0 },
                    TrajectoryPoints =
                    {
                        new Point3D { X = 420, Y = 520, Z = 0 },
                        new Point3D { X = 445, Y = 515, Z = 8820 }
                    }
                }
            };
        }

        private static ReservoirCrossSectionConfiguration CreateReservoirCrossSectionMidlineConfiguration()
        {
            return new ReservoirCrossSectionConfiguration
            {
                ShowFluidContacts = true,
                ShowSurfaceLabels = true,
                MaximumWellOffsetFromSection = 100
            };
        }

        /// <summary>
        /// Builds the standard enhanced vertical well-schematic sample scene.
        /// </summary>
        public static DrawingEngine CreateWellSchematicEnhancedVerticalScene()
        {
            var wellData = CreateWellSchematicEnhancedVerticalData();

            return WellSchematicBuilder.Create()
                .WithWellData(wellData)
                .WithSize(900, 1600)
                .WithGrid(true)
                .WithAnnotations(true)
                .Build();
        }

        private static IReadOnlyList<DrawingSampleExportAction> CreateWellSchematicEnhancedVerticalExports()
        {
            return new[]
            {
                new DrawingSampleExportAction(
                    id: "well-schematic-json",
                    label: "Well Schematic JSON",
                    fileNameSuffix: "well-schematic.json",
                    contentType: "application/json",
                    export: _ => JsonSerializer.SerializeToUtf8Bytes(CreateWellSchematicEnhancedVerticalData()),
                    description: "Typed well schematic payload for the canonical enhanced vertical completion sample.")
            };
        }

        private static WellData CreateWellSchematicEnhancedVerticalData()
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

        private static ReservoirData CreateReservoirRegressionData()
        {
            var topSurface = CreateSurface(
                "TOP-01",
                "Regression Top",
                ReservoirSurfaceKind.Structure,
                (x, y) => 8450 + (x * 0.08) - (y * 0.05) + (Math.Sin(x / 180.0) * 18) + (Math.Cos(y / 200.0) * 12));

            var baseSurface = CreateSurface(
                "BASE-01",
                "Regression Base",
                ReservoirSurfaceKind.Horizon,
                (x, y) => 8725 + (x * 0.06) - (y * 0.04) + (Math.Sin((x + y) / 210.0) * 15));

            return new ReservoirData
            {
                ReservoirId = "RES-01",
                ReservoirName = "Regression Reservoir",
                FormationName = "North Dome Sand",
                CoordinateReferenceSystem = CoordinateReferenceSystem.CreateProjected(
                    "LOCAL:REGRESSION",
                    "Regression Local Grid",
                    "m",
                    CoordinateAuthority.Custom),
                BoundingBox = new BoundingBox
                {
                    MinX = 0,
                    MaxX = 1000,
                    MinY = 0,
                    MaxY = 1000,
                    MinZ = Math.Min(topSurface.BoundingBox!.MinZ, baseSurface.BoundingBox!.MinZ),
                    MaxZ = Math.Max(topSurface.BoundingBox!.MaxZ, baseSurface.BoundingBox!.MaxZ)
                },
                FluidContacts = new FluidContacts
                {
                    OilWaterContact = 8600,
                    GasOilContact = 8525,
                    Source = "Regression"
                },
                Surfaces =
                {
                    topSurface,
                    baseSurface
                }
            };
        }

        private static ReservoirSurfaceData CreateSurface(
            string surfaceId,
            string surfaceName,
            ReservoirSurfaceKind kind,
            Func<double, double, double> valueFactory)
        {
            var points = new List<Point3D>();
            for (int row = 0; row <= 4; row++)
            {
                for (int column = 0; column <= 4; column++)
                {
                    double x = column * 250.0;
                    double y = row * 250.0;
                    points.Add(new Point3D
                    {
                        X = x,
                        Y = y,
                        Z = valueFactory(x, y)
                    });
                }
            }

            return new ReservoirSurfaceData
            {
                SurfaceId = surfaceId,
                SurfaceName = surfaceName,
                SurfaceKind = kind,
                ValueUnit = "ft",
                Points = points,
                BoundingBox = new BoundingBox
                {
                    MinX = points.Min(point => point.X),
                    MaxX = points.Max(point => point.X),
                    MinY = points.Min(point => point.Y),
                    MaxY = points.Max(point => point.Y),
                    MinZ = points.Min(point => point.Z),
                    MaxZ = points.Max(point => point.Z)
                }
            };
        }
    }
}
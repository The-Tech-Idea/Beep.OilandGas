using System.Linq;
using System.Text.Json;
using Beep.OilandGas.Drawing.CoordinateSystems;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Export;
using Beep.OilandGas.Drawing.Visualizations.Reservoir;
using Xunit;

namespace Beep.OilandGas.Drawing.Tests
{
    public class GeoJsonExporterTests
    {
        [Fact]
        public void ExportFieldMapToString_WritesFeatureCollectionWithProjectedCrsMetadata()
        {
            var fieldMap = new FieldMapData
            {
                MapName = "North Lease",
                CoordinateReferenceSystem = CoordinateReferenceSystem.CreateProjected("EPSG:26915", "NAD83 / UTM zone 15N")
            };

            fieldMap.AreaAssets.Add(new FieldMapAreaAsset
            {
                AssetId = "lease-1",
                AssetName = "Lease Block A",
                AssetKind = FieldMapAssetKind.Lease,
                BoundaryPoints =
                {
                    new Point3D { X = 0, Y = 0 },
                    new Point3D { X = 1000, Y = 0 },
                    new Point3D { X = 1000, Y = 800 },
                    new Point3D { X = 0, Y = 800 }
                }
            });

            fieldMap.PointAssets.Add(new FieldMapPointAsset
            {
                AssetId = "well-1",
                AssetName = "Well A-01",
                AssetKind = FieldMapAssetKind.Well,
                Location = new Point3D { X = 250, Y = 300 }
            });

            fieldMap.ConnectionAssets.Add(new FieldMapConnectionAsset
            {
                ConnectionId = "flowline-1",
                ConnectionName = "Main Flowline",
                ConnectionKind = FieldMapConnectionKind.Flowline,
                Vertices =
                {
                    new Point3D { X = 250, Y = 300 },
                    new Point3D { X = 850, Y = 500 }
                }
            });

            var json = GeoJsonExporter.ExportFieldMapToString(fieldMap);
            using var document = JsonDocument.Parse(json);

            Assert.Equal("FeatureCollection", document.RootElement.GetProperty("type").GetString());
            Assert.Equal("EPSG:26915", document.RootElement.GetProperty("beepCrs").GetProperty("identifier").GetString());
            Assert.Equal(3, document.RootElement.GetProperty("features").GetArrayLength());
            Assert.Contains(document.RootElement.GetProperty("features").EnumerateArray(), feature =>
                feature.GetProperty("geometry").GetProperty("type").GetString() == "Polygon");
        }

        [Fact]
        public void ExportReservoirMapToString_WritesContourFaultAndWellFeatures()
        {
            var contourSurface = new ReservoirSurfaceData
            {
                SurfaceId = "top-reservoir",
                SurfaceName = "Top Reservoir",
                SurfaceKind = ReservoirSurfaceKind.Structure,
                ValueUnit = "ft",
                Points =
                {
                    new Point3D { X = 0, Y = 0, Z = 10100 },
                    new Point3D { X = 1000, Y = 0, Z = 10050 },
                    new Point3D { X = 0, Y = 1000, Z = 10150 },
                    new Point3D { X = 1000, Y = 1000, Z = 10000 }
                }
            };

            var faultSurface = new ReservoirSurfaceData
            {
                SurfaceId = "fault-a",
                SurfaceName = "Fault A",
                SurfaceKind = ReservoirSurfaceKind.Fault,
                Points =
                {
                    new Point3D { X = 100, Y = 100, Z = 0 },
                    new Point3D { X = 500, Y = 550, Z = 0 },
                    new Point3D { X = 900, Y = 950, Z = 0 }
                }
            };

            var well = new ReservoirWellMapPoint
            {
                WellId = "well-a01",
                WellName = "A01",
                Uwi = "100011223344W500",
                SurfaceLocation = new Point3D { X = 350, Y = 400, Z = 0 }
            };

            var json = GeoJsonExporter.ExportReservoirMapToString(
                contourSurface,
                CoordinateReferenceSystem.CreateProjected("EPSG:26915", "NAD83 / UTM zone 15N"),
                new[] { faultSurface },
                new[] { well },
                new ReservoirContourConfiguration { ContourInterval = 25, SampleColumns = 8, SampleRows = 8 });

            using var document = JsonDocument.Parse(json);
            var featureTypes = document.RootElement
                .GetProperty("features")
                .EnumerateArray()
                .Select(feature => feature.GetProperty("properties").GetProperty("featureType").GetString())
                .ToList();

            Assert.Contains("contour-segment", featureTypes);
            Assert.Contains("fault-trace", featureTypes);
            Assert.Contains("well", featureTypes);
        }
    }
}
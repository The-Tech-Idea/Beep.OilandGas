using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using Beep.OilandGas.Drawing.Samples;
using Xunit;

namespace Beep.OilandGas.Drawing.Tests
{
    public class DrawingSampleGalleryTests
    {
        [Fact]
        public void FieldMapSampleScene_DeclaresGeoJsonSupplementalExport()
        {
            var scene = DrawingSampleGallery.GetStandardScene("FieldMap_AssetNetwork");

            var export = Assert.Single(scene.SupplementalExports, candidate => candidate.Id == "field-map-geojson");
            Assert.Equal("field-map-geojson", export.Id);
            Assert.Equal("application/geo+json", export.ContentType);
            Assert.Equal(DrawingSampleExportPresentationKind.MapExchange, export.PresentationKind);
        }

        [Fact]
        public void FieldMapSampleScene_GeoJsonSupplementalExport_ReturnsFeatureCollection()
        {
            var scene = DrawingSampleGallery.GetStandardScene("FieldMap_AssetNetwork");
            using var engine = scene.CreateEngine();

            var export = Assert.Single(scene.SupplementalExports, candidate => candidate.Id == "field-map-geojson");
            var content = Encoding.UTF8.GetString(export.Export(engine));

            Assert.Contains("\"type\": \"FeatureCollection\"", content);
            Assert.Contains("\"featureType\": \"point-asset\"", content);
        }

        [Fact]
        public void FieldMapSampleScene_DeclaresGeoreferencedBundleSupplementalExport()
        {
            var scene = DrawingSampleGallery.GetStandardScene("FieldMap_AssetNetwork");

            var export = Assert.Single(scene.SupplementalExports, candidate => candidate.Id == "field-map-georeferenced-png-bundle");
            Assert.Equal("application/zip", export.ContentType);
            Assert.Equal("georeferenced.zip", export.FileNameSuffix);
            Assert.Equal(DrawingSampleExportPresentationKind.RasterBundle, export.PresentationKind);
        }

        [Fact]
        public void FieldMapSampleScene_GeoreferencedBundleSupplementalExport_ReturnsZipWithExpectedEntries()
        {
            var scene = DrawingSampleGallery.GetStandardScene("FieldMap_AssetNetwork");
            using var engine = scene.CreateEngine();

            var export = Assert.Single(scene.SupplementalExports, candidate => candidate.Id == "field-map-georeferenced-png-bundle");
            var bytes = export.Export(engine);

            using var archive = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);
            Assert.Contains(archive.Entries, entry => entry.FullName.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase));
            Assert.Contains(archive.Entries, entry => entry.FullName.EndsWith(".pgw", System.StringComparison.OrdinalIgnoreCase));
            Assert.Contains(archive.Entries, entry => entry.FullName.EndsWith(".png.crs.json", System.StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void ReservoirContourSampleScene_DeclaresGeoJsonSupplementalExport()
        {
            var scene = DrawingSampleGallery.GetStandardScene("ReservoirContour_StructureMap");

            var export = Assert.Single(scene.SupplementalExports, candidate => candidate.Id == "reservoir-contour-geojson");
            Assert.Equal("reservoir-contour-geojson", export.Id);
            Assert.Equal("application/geo+json", export.ContentType);
        }

        [Fact]
        public void ReservoirContourSampleScene_GeoJsonSupplementalExport_ReturnsContourFeatureCollection()
        {
            var scene = DrawingSampleGallery.GetStandardScene("ReservoirContour_StructureMap");
            using var engine = scene.CreateEngine();

            var export = Assert.Single(scene.SupplementalExports, candidate => candidate.Id == "reservoir-contour-geojson");
            var content = Encoding.UTF8.GetString(export.Export(engine));

            Assert.Contains("\"type\": \"FeatureCollection\"", content);
            Assert.Contains("\"featureType\": \"contour-segment\"", content);
        }

        [Fact]
        public void ReservoirContourSampleScene_DeclaresGeoreferencedBundleSupplementalExport()
        {
            var scene = DrawingSampleGallery.GetStandardScene("ReservoirContour_StructureMap");

            var export = Assert.Single(scene.SupplementalExports, candidate => candidate.Id == "reservoir-contour-georeferenced-png-bundle");
            Assert.Equal("application/zip", export.ContentType);
            Assert.Equal("georeferenced.zip", export.FileNameSuffix);
        }

        [Fact]
        public void ReservoirContourSampleScene_GeoreferencedBundleSupplementalExport_ReturnsZipWithExpectedEntries()
        {
            var scene = DrawingSampleGallery.GetStandardScene("ReservoirContour_StructureMap");
            using var engine = scene.CreateEngine();

            var export = Assert.Single(scene.SupplementalExports, candidate => candidate.Id == "reservoir-contour-georeferenced-png-bundle");
            var bytes = export.Export(engine);

            using var archive = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);
            Assert.Contains(archive.Entries, entry => entry.FullName.EndsWith("reservoir-contour.png", System.StringComparison.OrdinalIgnoreCase));
            Assert.Contains(archive.Entries, entry => entry.FullName.EndsWith("reservoir-contour.pgw", System.StringComparison.OrdinalIgnoreCase));
            Assert.Contains(archive.Entries, entry => entry.FullName.EndsWith("reservoir-contour.png.crs.json", System.StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void ReservoirCrossSectionSampleScene_DeclaresJsonSupplementalExport()
        {
            var scene = DrawingSampleGallery.GetStandardScene("ReservoirCrossSection_Midline");

            var export = Assert.Single(scene.SupplementalExports, candidate => candidate.Id == "reservoir-cross-section-json");
            Assert.Equal("application/json", export.ContentType);
            Assert.Equal("reservoir-cross-section.json", export.FileNameSuffix);
            Assert.Equal(DrawingSampleExportPresentationKind.EngineeringData, export.PresentationKind);
        }

        [Fact]
        public void ReservoirCrossSectionSampleScene_JsonSupplementalExport_ReturnsSectionData()
        {
            var scene = DrawingSampleGallery.GetStandardScene("ReservoirCrossSection_Midline");
            using var engine = scene.CreateEngine();

            var export = Assert.Single(scene.SupplementalExports, candidate => candidate.Id == "reservoir-cross-section-json");
            var bytes = export.Export(engine);

            using var document = JsonDocument.Parse(bytes);
            var root = document.RootElement;
            Assert.Equal("A-A'", root.GetProperty("SectionLine").GetProperty("SectionName").GetString());
            Assert.True(root.GetProperty("Profiles").GetArrayLength() >= 2);
            Assert.Equal("A-12H", root.GetProperty("WellMarkers")[0].GetProperty("WellName").GetString());
        }

        [Fact]
        public void WellSchematicSampleScene_DeclaresJsonSupplementalExport()
        {
            var scene = DrawingSampleGallery.GetStandardScene("WellSchematic_EnhancedVertical");

            var export = Assert.Single(scene.SupplementalExports, candidate => candidate.Id == "well-schematic-json");
            Assert.Equal("application/json", export.ContentType);
            Assert.Equal("well-schematic.json", export.FileNameSuffix);
            Assert.Equal(DrawingSampleExportPresentationKind.EngineeringData, export.PresentationKind);
        }

        [Fact]
        public void WellSchematicSampleScene_JsonSupplementalExport_ReturnsWellData()
        {
            var scene = DrawingSampleGallery.GetStandardScene("WellSchematic_EnhancedVertical");
            using var engine = scene.CreateEngine();

            var export = Assert.Single(scene.SupplementalExports, candidate => candidate.Id == "well-schematic-json");
            var bytes = export.Export(engine);

            using var document = JsonDocument.Parse(bytes);
            var root = document.RootElement;
            Assert.Equal("100012345678W500", root.GetProperty("UWI").GetString());
            Assert.Equal("A-12H", root.GetProperty("UBHI").GetString());
            Assert.Equal("Production Packer", root.GetProperty("BoreHoles")[0].GetProperty("Equip")[0].GetProperty("EquipmentName").GetString());
        }

        [Fact]
        public void WellLogSampleScene_DeclaresJsonSupplementalExport()
        {
            var scene = DrawingSampleGallery.GetStandardScene("WellLog_Petrophysical");

            var export = Assert.Single(scene.SupplementalExports, candidate => candidate.Id == "well-log-json");
            Assert.Equal("application/json", export.ContentType);
            Assert.Equal("well-log.json", export.FileNameSuffix);
            Assert.Equal(DrawingSampleExportPresentationKind.EngineeringData, export.PresentationKind);
        }

        [Fact]
        public void WellLogSampleScene_JsonSupplementalExport_ReturnsLogData()
        {
            var scene = DrawingSampleGallery.GetStandardScene("WellLog_Petrophysical");
            using var engine = scene.CreateEngine();

            var export = Assert.Single(scene.SupplementalExports, candidate => candidate.Id == "well-log-json");
            var bytes = export.Export(engine);

            using var document = JsonDocument.Parse(bytes);
            var root = document.RootElement;
            Assert.Equal("A-12H", root.GetProperty("WellIdentifier").GetString());
            Assert.Equal("Regression Petrophysical", root.GetProperty("LogName").GetString());
            Assert.True(root.GetProperty("Curves").GetProperty("GR").GetArrayLength() > 0);
        }
    }
}
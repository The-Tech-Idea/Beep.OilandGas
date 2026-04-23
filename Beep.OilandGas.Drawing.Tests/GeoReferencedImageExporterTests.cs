using System.IO;
using System.Text.Json;
using Beep.OilandGas.Drawing.Export;
using Beep.OilandGas.Drawing.Samples;
using Xunit;

namespace Beep.OilandGas.Drawing.Tests
{
    public class GeoReferencedImageExporterTests
    {
        [Fact]
        public void ExportToPng_WritesImageAndGeoreferenceSidecars()
        {
            var outputDirectory = Path.Combine(Path.GetTempPath(), "beep-drawing-georef-tests");
            Directory.CreateDirectory(outputDirectory);

            var imagePath = Path.Combine(outputDirectory, "field-map.png");
            var worldFilePath = GeoReferencedImageExporter.GetWorldFilePath(imagePath);
            var coordinateMetadataPath = GeoReferencedImageExporter.GetCoordinateReferenceMetadataPath(imagePath);

            if (File.Exists(imagePath)) File.Delete(imagePath);
            if (File.Exists(worldFilePath)) File.Delete(worldFilePath);
            if (File.Exists(coordinateMetadataPath)) File.Delete(coordinateMetadataPath);

            var engine = DrawingSampleGallery.CreateFieldMapAssetNetworkScene();
            GeoReferencedImageExporter.ExportToPng(engine, imagePath);

            Assert.True(File.Exists(imagePath));
            Assert.True(File.Exists(worldFilePath));
            Assert.True(File.Exists(coordinateMetadataPath));

            var worldFileLines = File.ReadAllLines(worldFilePath);
            Assert.Equal(6, worldFileLines.Length);

            using var document = JsonDocument.Parse(File.ReadAllText(coordinateMetadataPath));
            Assert.Equal("Projected", document.RootElement.GetProperty("kind").GetString());
            Assert.True(document.RootElement.GetProperty("axes").GetArrayLength() >= 2);
        }
    }
}
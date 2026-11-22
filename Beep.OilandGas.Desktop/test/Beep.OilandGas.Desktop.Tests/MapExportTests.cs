using System.Text.Json;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Export;
using TheTechIdea.OilGas.Wellbore;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// VSB-09 AC-6: a map with coordinates exports as a PNG plus a world file and coordinate metadata, and the
/// transform in the file maps pixels back to the wells' positions.
/// </summary>
public sealed class MapExportTests
{
    [Fact]
    public void A_world_file_transform_round_trips_a_coordinate()
    {
        var transform = new OgGeoTransform(A: 2.5, B: 0, C: 1000, D: 0, E: -2.5, F: 5000);

        var (x, y) = transform.ToWorld(120, 40);
        var (column, row) = transform.ToPixel(x, y);

        Assert.Equal(120, column, 9);
        Assert.Equal(40, row, 9);
        Assert.Equal(2.5, transform.PixelSize, 9);
    }

    [Fact]
    public void The_world_file_text_round_trips_the_six_numbers()
    {
        var transform = new OgGeoTransform(1.25, 0, 500_000, 0, -1.25, 3_200_000);

        var read = OgGeoTransform.FromWorldFile(transform.ToWorldFile());

        Assert.Equal(transform, read);
        Assert.Throws<FormatException>(() => OgGeoTransform.FromWorldFile("1\n2\n3"));
    }

    [Fact]
    public void Exporting_a_map_writes_the_image_the_world_file_and_the_metadata()
    {
        var directory = Path.Combine(OgPaths.DataRoot, "maps-" + Guid.NewGuid().ToString("n")[..8]);
        var path = Path.Combine(directory, "map.png");

        var exported = OgMapExport.Export(OgMapExport.DemoMap(), path, width: 800, height: 600);

        Assert.True(exported.Success, exported.Message);
        var result = exported.Value!;

        Assert.True(File.Exists(result.ImagePath));
        Assert.Equal(Path.ChangeExtension(path, ".pgw"), result.WorldFilePath);
        Assert.Equal(Path.ChangeExtension(path, ".geo.json"), result.MetadataPath);
        Assert.True(File.Exists(result.WorldFilePath));
        Assert.True(File.Exists(result.MetadataPath));

        // A real PNG, not an empty file with the right name.
        var header = File.ReadAllBytes(result.ImagePath).Take(4).ToArray();
        Assert.Equal([0x89, (byte)'P', (byte)'N', (byte)'G'], header);

        Assert.Equal("EPSG:32631", result.Crs);
        Assert.Contains("EPSG:32631", result.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void The_world_file_maps_every_well_inside_the_image()
    {
        var directory = Path.Combine(OgPaths.DataRoot, "maps-" + Guid.NewGuid().ToString("n")[..8]);
        var path = Path.Combine(directory, "map.png");
        var map = OgMapExport.DemoMap();

        var exported = OgMapExport.Export(map, path, width: 800, height: 600);
        Assert.True(exported.Success, exported.Message);

        var transform = OgGeoTransform.FromWorldFile(File.ReadAllText(exported.Value!.WorldFilePath!));

        foreach (var well in map.Wells)
        {
            var (column, row) = transform.ToPixel(well.X, well.Y);

            // The image covers the wells, so each one is inside it...
            Assert.InRange(column, 0, 800);
            Assert.InRange(row, 0, 600);

            // ...and reading that pixel back through the file returns the well's own position.
            var (x, y) = transform.ToWorld(column, row);
            Assert.Equal(well.X, x, 6);
            Assert.Equal(well.Y, y, 6);
        }

        // The file's extent brackets the map's own bounds, with the view's margin.
        var (minX, minY, maxX, maxY) = MapLayout.Bounds(map);
        var (left, top) = transform.ToWorld(0, 0);
        var (right, bottom) = transform.ToWorld(800, 600);

        Assert.True(left <= minX && right >= maxX, $"x extent {left}..{right} does not cover {minX}..{maxX}.");
        Assert.True(top >= maxY && bottom <= minY, $"y extent {bottom}..{top} does not cover {minY}..{maxY}.");
    }

    [Fact]
    public void The_metadata_carries_the_crs_the_transform_and_the_wells()
    {
        var directory = Path.Combine(OgPaths.DataRoot, "maps-" + Guid.NewGuid().ToString("n")[..8]);
        var path = Path.Combine(directory, "map.png");

        var exported = OgMapExport.Export(OgMapExport.DemoMap(), path, width: 640, height: 480);
        Assert.True(exported.Success, exported.Message);

        using var document = JsonDocument.Parse(File.ReadAllText(exported.Value!.MetadataPath!));
        var root = document.RootElement;

        Assert.Equal("EPSG:32631", root.GetProperty("crs").GetString());
        Assert.Equal(640, root.GetProperty("width").GetInt32());
        Assert.Equal(480, root.GetProperty("height").GetInt32());
        Assert.Equal(3, root.GetProperty("wells").GetArrayLength());
        Assert.True(root.GetProperty("pixelSize").GetDouble() > 0);
        Assert.Equal("Meters", root.GetProperty("units").GetString());
    }

    [Fact]
    public void A_well_without_a_coordinate_system_is_refused_rather_than_georeferenced_wrongly()
    {
        var map = OgMapExport.DemoMap();
        map.Wells[1].Crs = null;
        map.Wells[1].Document.Crs = null;

        var exported = OgMapExport.Export(map, Path.Combine(OgPaths.DataRoot, "maps-refused.png"));

        Assert.False(exported.Success);
        Assert.Equal(OgMapExport.CodeNoCrs, exported.Reference);
        Assert.Contains(map.Wells[1].Title, exported.Message!, StringComparison.Ordinal);
    }

    [Fact]
    public void Exporting_without_georeferencing_writes_only_the_image()
    {
        var directory = Path.Combine(OgPaths.DataRoot, "maps-" + Guid.NewGuid().ToString("n")[..8]);
        var path = Path.Combine(directory, "plain.png");

        var exported = OgMapExport.Export(OgMapExport.DemoMap(), path, 400, 300, georeferenced: false);

        Assert.True(exported.Success, exported.Message);
        Assert.True(File.Exists(exported.Value!.ImagePath));
        Assert.Null(exported.Value.WorldFilePath);
        Assert.Null(exported.Value.MetadataPath);
        Assert.False(File.Exists(Path.ChangeExtension(path, ".pgw")));
    }

    [Fact]
    public void An_existing_image_is_refused_unless_overwrite_is_asked_for()
    {
        var directory = Path.Combine(OgPaths.DataRoot, "maps-" + Guid.NewGuid().ToString("n")[..8]);
        var path = Path.Combine(directory, "exists.png");

        Assert.True(OgMapExport.Export(OgMapExport.DemoMap(), path, 320, 240).Success);

        var second = OgMapExport.Export(OgMapExport.DemoMap(), path, 320, 240);
        Assert.False(second.Success);
        Assert.Equal(OgMapExport.CodeDestinationExists, second.Reference);

        Assert.True(OgMapExport.Export(OgMapExport.DemoMap(), path, 320, 240, overwrite: true).Success);
    }
}

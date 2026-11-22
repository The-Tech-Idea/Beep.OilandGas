using System.Globalization;
using System.IO;
using System.Text.Json;
using SkiaSharp;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Export;

/// <summary>
/// The affine transform between image pixels and map coordinates, in the world-file convention:
/// <c>x = A·column + B·row + C</c>, <c>y = D·column + E·row + F</c>.
/// </summary>
/// <remarks>
/// A north-up map has <c>B = D = 0</c>, a positive <c>A</c> and a negative <c>E</c> (rows run south). The
/// transform is taken from the <see cref="MapView"/> that drew the image, so the file describes the picture
/// that was actually written rather than a second calculation that could disagree with it.
/// </remarks>
public sealed record OgGeoTransform(double A, double B, double C, double D, double E, double F)
{
    /// <summary>World coordinates at a pixel position (pixel centres are at column + 0.5).</summary>
    public (double X, double Y) ToWorld(double column, double row) =>
        ((A * column) + (B * row) + C, (D * column) + (E * row) + F);

    /// <summary>
    /// Pixel position of a world coordinate. Exact for a north-up map (B = D = 0), which is what the map view
    /// produces; a rotated map returns NaN rather than a wrong answer.
    /// </summary>
    public (double Column, double Row) ToPixel(double x, double y) =>
        Math.Abs(A) < 1e-12 || Math.Abs(E) < 1e-12
            ? (double.NaN, double.NaN)
            : ((x - C) / A, (y - F) / E);

    /// <summary>The six lines of an ESRI world file (invariant, one number per line).</summary>
    public string ToWorldFile() =>
        string.Join(
            "\n",
            new[] { A, B, C, D, E, F }.Select(value => value.ToString("R", CultureInfo.InvariantCulture)));

    /// <summary>Reads a world file back; a file without six numbers throws.</summary>
    public static OgGeoTransform FromWorldFile(string text)
    {
        var values = text
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(value => double.Parse(value, CultureInfo.InvariantCulture))
            .ToArray();

        if (values.Length < 6)
        {
            throw new FormatException($"A world file needs six numbers; this one has {values.Length}.");
        }

        return new OgGeoTransform(values[0], values[1], values[2], values[3], values[4], values[5]);
    }

    /// <summary>Pixel size on the ground, for the summary line.</summary>
    public double PixelSize => Math.Sqrt(Math.Abs(A * E - B * D));
}

/// <summary>What a georeferenced map export wrote.</summary>
public sealed record OgMapExportResult(
    string ImagePath,
    string? WorldFilePath,
    string? MetadataPath,
    OgGeoTransform Transform,
    string Crs,
    double MinX,
    double MinY,
    double MaxX,
    double MaxY,
    int Width,
    int Height,
    string Unit)
{
    public string Describe() =>
        $"{Width}×{Height} px · {Crs} · {Transform.PixelSize:0.###} {Unit}/px · " +
        $"extent {MinX:0.#},{MinY:0.#} to {MaxX:0.#},{MaxY:0.#}";
}

/// <summary>
/// VSB-09 AC-6: a well map exported as a PNG, with a world file and coordinate metadata when the wells carry
/// positions and a coordinate reference system.
/// </summary>
/// <remarks>
/// <para>
/// Georeferencing is only offered when it is true: every well must have a coordinate reference system. A map
/// with one well lacking a CRS is <b>refused</b> rather than written with a georeference that is wrong for part
/// of the picture — a world file is a claim about every pixel in the image.
/// </para>
/// <para>
/// The PNG is written to <c>.partial</c> and moved into place only after it is encoded, so a failure never
/// leaves a half-written image where a GIS would open it (the same rule as every other export).
/// </para>
/// </remarks>
public static class OgMapExport
{
    public const string CodeNoCoordinates = "map-no-coordinates";
    public const string CodeNoCrs = "map-no-crs";
    public const string CodeWriteFailed = "map-export-failed";
    public const string CodeDestinationExists = "map-export-exists";

    /// <summary>A deterministic map: the three demo wells at the positions VSB-06 uses, in UTM 31N.</summary>
    public static WellMap DemoMap()
    {
        var map = new WellMap { Name = "Well map", Unit = DepthUnit.Meters };

        var positions = new (string Name, double X, double Y)[]
        {
            ("DEMO-1", 0, 0),
            ("DEMO-2", 1_500, 220),
            ("DEMO-3", 3_200, 540),
        };

        foreach (var (name, x, y) in positions)
        {
            var document = TheTechIdea.OilGas.WellLogs.WellLogTemplates.CreateTripleComboDemo();
            document.Name = name;
            document.WellName = name;

            map.Wells.Add(new CrossSectionWell
            {
                Document = document,
                Label = name,
                X = x,
                Y = y,
                Crs = "EPSG:32631",
            });
        }

        return map;
    }

    /// <summary>
    /// Exports the map. <paramref name="georeferenced"/> writes the world file and the metadata beside the PNG;
    /// when false only the image is written, and the result says so.
    /// </summary>
    public static OgResult<OgMapExportResult> Export(
        WellMap map,
        string destination,
        int width = 1_000,
        int height = 800,
        bool georeferenced = true,
        bool overwrite = false)
    {
        ArgumentNullException.ThrowIfNull(map);

        if (string.IsNullOrWhiteSpace(destination))
        {
            return OgResult<OgMapExportResult>.Failed("An export needs a destination path.", CodeWriteFailed);
        }

        if (width <= 0 || height <= 0)
        {
            return OgResult<OgMapExportResult>.Failed("The image needs a positive size.", CodeWriteFailed);
        }

        if (map.Wells.Count == 0)
        {
            return OgResult<OgMapExportResult>.Failed(
                "The map has no wells, so there is nothing to place.",
                CodeNoCoordinates);
        }


        var withoutCrs = map.Wells
            .Where(well => string.IsNullOrWhiteSpace(well.EffectiveCrs))
            .Select(well => well.Title)
            .ToList();

        if (georeferenced && withoutCrs.Count > 0)
        {
            return OgResult<OgMapExportResult>.Failed(
                $"No coordinate reference system for {string.Join(", ", withoutCrs)}; " +
                "a world file would be wrong for those wells. Name a CRS or export without georeferencing.",
                CodeNoCrs);
        }

        var path = Path.GetFullPath(destination);

        if (File.Exists(path) && !overwrite)
        {
            return OgResult<OgMapExportResult>.Failed(
                $"'{path}' already exists. Tick overwrite to replace it.",
                CodeDestinationExists);
        }

        var directory = Path.GetDirectoryName(path);

        if (string.IsNullOrWhiteSpace(directory))
        {
            return OgResult<OgMapExportResult>.Failed(
                $"'{destination}' has no folder to write into.",
                CodeWriteFailed);
        }

        Directory.CreateDirectory(directory);

        var view = new MapView { Map = map, Width = width, Height = height };
        view.ZoomToFit();

        using var renderer = new ViewSurfaceRenderer { Surface = view };

        if (!renderer.Render(width, height) || renderer.Frame is null)
        {
            return OgResult<OgMapExportResult>.Failed("The map could not be drawn.", CodeWriteFailed);
        }

        var transform = TransformOf(view);
        var partial = path + ".partial";
        var moved = false;

        try
        {
            using (var image = SKImage.FromPixelCopy(
                       new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul),
                       renderer.Frame))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            using (var stream = File.Create(partial))
            {
                data.SaveTo(stream);
            }

            File.Move(partial, path, overwrite: true);
            moved = true;
        }
        finally
        {
            // A half-written PNG next to a .pgw would be a georeferencing lie. The exception still reaches the
            // caller's reporter; this only guarantees the partial file is not left behind.
            if (!moved && File.Exists(partial))
            {
                File.Delete(partial);
            }
        }

        var crs = map.Wells[0].EffectiveCrs ?? string.Empty;
        var bounds = MapLayout.Bounds(map);

        var worldPath = georeferenced ? Path.ChangeExtension(path, ".pgw") : null;
        var metadataPath = georeferenced ? Path.ChangeExtension(path, ".geo.json") : null;

        if (georeferenced)
        {
            File.WriteAllText(worldPath!, transform.ToWorldFile());

            var metadata = new
            {
                schema = 1,
                crs,
                image = Path.GetFileName(path),
                width,
                height,
                pixelSize = transform.PixelSize,
                worldFile = Path.GetFileName(worldPath),
                transform = new { a = transform.A, b = transform.B, c = transform.C, d = transform.D, e = transform.E, f = transform.F },
                units = map.Unit.ToString(),
                extent = new { minX = bounds.minX, minY = bounds.minY, maxX = bounds.maxX, maxY = bounds.maxY },
                wells = map.Wells.Select(well => new { name = well.Title, x = well.X, y = well.Y, crs = well.EffectiveCrs }),
            };

            File.WriteAllText(
                metadataPath!,
                JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }));
        }

        return OgResult<OgMapExportResult>.Ok(new OgMapExportResult(
            path,
            worldPath,
            metadataPath,
            transform,
            crs,
            bounds.minX,
            bounds.minY,
            bounds.maxX,
            bounds.maxY,
            width,
            height,
            map.Unit.ToString()));
    }

    /// <summary>
    /// The affine transform of the rendered view, sampled at the centres of three pixels. Sampling the same
    /// object that drew the image is what makes the world file describe this picture rather than an intention.
    /// </summary>
    public static OgGeoTransform TransformOf(MapView view)
    {
        ArgumentNullException.ThrowIfNull(view);

        var (x0, y0) = view.ToWorld(new SKPoint(0.5f, 0.5f));
        var (x1, _) = view.ToWorld(new SKPoint(1.5f, 0.5f));
        var (_, y2) = view.ToWorld(new SKPoint(0.5f, 1.5f));

        return new OgGeoTransform(
            A: x1 - x0,
            B: 0,
            C: x0,
            D: 0,
            E: y2 - y0,
            F: y0);
    }
}

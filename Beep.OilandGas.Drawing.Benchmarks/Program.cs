using System.Text.Json;
using Beep.OilandGas.Drawing.Benchmarks.Benchmarking;
using Beep.OilandGas.Drawing.Samples;

try
{
    var options = ParseArguments(args);
    var runner = new SceneBenchmarkRunner(DrawingSampleGallery.GetStandardScenes());

    if (options.ShowHelp)
    {
        WriteHelp();
        return 0;
    }

    if (options.ListOnly)
    {
        WriteAvailableScenes(runner);
        return 0;
    }

    IReadOnlyList<SceneBenchmarkResult> results = runner.Run(options);
    string outputPath = ResolveOutputPath(options.OutputPath);
    WriteJsonReport(results, outputPath);
    WriteConsoleReport(results, outputPath);
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    Console.Error.WriteLine();
    WriteHelp();
    return 1;
}

static SceneBenchmarkOptions ParseArguments(string[] args)
{
    var options = new SceneBenchmarkOptions();

    for (int index = 0; index < args.Length; index++)
    {
        string token = args[index];
        switch (token)
        {
            case "--help":
            case "-h":
            case "/?":
                options.ShowHelp = true;
                break;

            case "--list":
                options.ListOnly = true;
                break;

            case "--warmup":
                options.WarmupIterations = ParsePositiveInteger(ReadValue(args, ref index, token), allowZero: true, token);
                break;

            case "--iterations":
                options.MeasurementIterations = ParsePositiveInteger(ReadValue(args, ref index, token), allowZero: false, token);
                break;

            case "--scene":
                AddSceneNames(ReadValue(args, ref index, token), options);
                break;

            case "--operation":
                AddOperations(ReadValue(args, ref index, token), options);
                break;

            case "--output":
                options.OutputPath = ReadValue(args, ref index, token);
                break;

            default:
                throw new ArgumentException($"Unknown argument '{token}'.");
        }
    }

    return options;
}

static string ReadValue(string[] args, ref int index, string token)
{
    if (index + 1 >= args.Length)
        throw new ArgumentException($"Missing value for argument '{token}'.");

    index++;
    return args[index];
}

static int ParsePositiveInteger(string value, bool allowZero, string token)
{
    if (!int.TryParse(value, out int parsedValue))
        throw new ArgumentException($"Argument '{token}' expects an integer value.");

    if (allowZero ? parsedValue < 0 : parsedValue <= 0)
        throw new ArgumentException($"Argument '{token}' must be {(allowZero ? "zero or greater" : "greater than zero")}.");

    return parsedValue;
}

static void AddSceneNames(string value, SceneBenchmarkOptions options)
{
    foreach (string sceneName in SplitValues(value))
    {
        options.SceneNames.Add(sceneName);
    }
}

static void AddOperations(string value, SceneBenchmarkOptions options)
{
    foreach (string operationName in SplitValues(value))
    {
        switch (operationName.ToLowerInvariant())
        {
            case "build":
            case "build-scene":
            case "buildscene":
                options.Operations.Add(SceneBenchmarkOperation.BuildScene);
                break;

            case "render":
            case "render-png":
            case "renderpng":
            case "png":
                options.Operations.Add(SceneBenchmarkOperation.RenderPng);
                break;

            case "render-svg":
            case "rendersvg":
            case "svg":
                options.Operations.Add(SceneBenchmarkOperation.RenderSvg);
                break;

            case "render-pdf":
            case "renderpdf":
            case "pdf":
                options.Operations.Add(SceneBenchmarkOperation.RenderPdf);
                break;

            case "render-svg-annotated":
            case "rendersvgannotated":
            case "annotated-svg":
                options.Operations.Add(SceneBenchmarkOperation.RenderSvgWithAnnotations);
                break;

            case "render-pdf-annotated":
            case "renderpdfannotated":
            case "annotated-pdf":
                options.Operations.Add(SceneBenchmarkOperation.RenderPdfWithAnnotations);
                break;

            case "render-png-annotated":
            case "renderpngannotated":
            case "annotated-render":
            case "annotated":
                options.Operations.Add(SceneBenchmarkOperation.RenderPngWithAnnotations);
                break;

            case "hit-test":
            case "hittest":
            case "hit":
                options.Operations.Add(SceneBenchmarkOperation.HitTest);
                break;

            default:
                throw new ArgumentException($"Unknown benchmark operation '{operationName}'. Supported operations are build-scene, render-png, render-svg, render-pdf, render-png-annotated, render-svg-annotated, render-pdf-annotated, and hit-test.");
        }
    }
}

static IEnumerable<string> SplitValues(string value)
{
    return value
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Where(entry => !string.IsNullOrWhiteSpace(entry));
}

static string ResolveOutputPath(string? outputPath)
{
    if (!string.IsNullOrWhiteSpace(outputPath))
        return outputPath;

    return Path.Combine(
        AppContext.BaseDirectory,
        "BenchmarkResults",
        $"drawing-scene-benchmarks-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json");
}

static void WriteJsonReport(IReadOnlyList<SceneBenchmarkResult> results, string outputPath)
{
    string? directory = Path.GetDirectoryName(outputPath);
    if (!string.IsNullOrWhiteSpace(directory))
    {
        Directory.CreateDirectory(directory);
    }

    string json = JsonSerializer.Serialize(results, new JsonSerializerOptions
    {
        WriteIndented = true
    });

    File.WriteAllText(outputPath, json);
}

static void WriteConsoleReport(IReadOnlyList<SceneBenchmarkResult> results, string outputPath)
{
    Console.WriteLine("Beep.OilandGas.Drawing benchmark suite");
    Console.WriteLine();
    Console.WriteLine("Operation    Scene                              Mean ms   Median   Max ms   Mean alloc   Payload");
    Console.WriteLine("-----------  --------------------------------  --------  -------  -------  -----------  -------");

    foreach (SceneBenchmarkResult result in results.OrderBy(entry => entry.Operation).ThenBy(entry => entry.SceneName, StringComparer.OrdinalIgnoreCase))
    {
        Console.WriteLine(
            $"{result.Operation,-11}  {Truncate(result.SceneName, 32),-32}  {result.MeanDurationMilliseconds,8:F2}  {result.MedianDurationMilliseconds,7:F2}  {result.MaximumDurationMilliseconds,7:F2}  {FormatBytes(result.MeanAllocatedBytes),11}  {FormatBytes(result.PayloadBytes),7}");
    }

    Console.WriteLine();
    Console.WriteLine($"Results written to {outputPath}");
}

static void WriteAvailableScenes(SceneBenchmarkRunner runner)
{
    Console.WriteLine("Available sample scenes:");
    foreach (DrawingSampleScene scene in runner.GetAvailableScenes())
    {
        Console.WriteLine($"- {scene.Name} ({scene.Width}x{scene.Height}) : {scene.Description}");
    }

    Console.WriteLine();
    Console.WriteLine("Available operations:");
    Console.WriteLine($"- {SceneBenchmarkRunner.GetOperationName(SceneBenchmarkOperation.BuildScene)}");
    Console.WriteLine($"- {SceneBenchmarkRunner.GetOperationName(SceneBenchmarkOperation.RenderPng)}");
    Console.WriteLine($"- {SceneBenchmarkRunner.GetOperationName(SceneBenchmarkOperation.RenderSvg)}");
    Console.WriteLine($"- {SceneBenchmarkRunner.GetOperationName(SceneBenchmarkOperation.RenderPdf)}");
    Console.WriteLine($"- {SceneBenchmarkRunner.GetOperationName(SceneBenchmarkOperation.RenderPngWithAnnotations)}");
    Console.WriteLine($"- {SceneBenchmarkRunner.GetOperationName(SceneBenchmarkOperation.RenderSvgWithAnnotations)}");
    Console.WriteLine($"- {SceneBenchmarkRunner.GetOperationName(SceneBenchmarkOperation.RenderPdfWithAnnotations)}");
    Console.WriteLine($"- {SceneBenchmarkRunner.GetOperationName(SceneBenchmarkOperation.HitTest)}");
}

static void WriteHelp()
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project Beep.OilandGas.Drawing.Benchmarks -- [options]");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --list                 List available sample scenes and operations.");
    Console.WriteLine("  --scene <names>        Comma-separated scene names to benchmark.");
    Console.WriteLine("  --operation <names>    Comma-separated operations: build-scene, render-png, render-svg, render-pdf, render-png-annotated, render-svg-annotated, render-pdf-annotated, hit-test.");
    Console.WriteLine("  --warmup <count>       Warmup iteration count. Default: 1.");
    Console.WriteLine("  --iterations <count>   Measurement iteration count. Default: 5.");
    Console.WriteLine("  --output <path>        JSON output path. Default: BenchmarkResults under the build output.");
    Console.WriteLine("  --help                 Show this help text.");
}

static string Truncate(string value, int length)
{
    if (value.Length <= length)
        return value;

    return value.Substring(0, length - 3) + "...";
}

static string FormatBytes(long value)
{
    const double oneKilobyte = 1024;
    const double oneMegabyte = 1024 * 1024;

    if (value >= oneMegabyte)
        return $"{value / oneMegabyte:F1} MB";

    if (value >= oneKilobyte)
        return $"{value / oneKilobyte:F1} KB";

    return value + " B";
}
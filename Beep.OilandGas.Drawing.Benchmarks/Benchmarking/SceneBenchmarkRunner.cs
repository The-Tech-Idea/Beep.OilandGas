using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.Export;
using Beep.OilandGas.Drawing.Samples;
using SkiaSharp;

namespace Beep.OilandGas.Drawing.Benchmarks.Benchmarking
{
    public sealed class SceneBenchmarkRunner
    {
        private readonly IReadOnlyDictionary<string, DrawingSampleScene> scenesByName;

        public SceneBenchmarkRunner(IReadOnlyList<DrawingSampleScene> scenes)
        {
            ArgumentNullException.ThrowIfNull(scenes);

            scenesByName = scenes.ToDictionary(scene => scene.Name, StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyList<DrawingSampleScene> GetAvailableScenes()
        {
            return scenesByName.Values.OrderBy(scene => scene.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }

        public IReadOnlyList<SceneBenchmarkResult> Run(SceneBenchmarkOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            ValidateOptions(options);

            List<DrawingSampleScene> selectedScenes = ResolveSelectedScenes(options);
            List<SceneBenchmarkOperation> selectedOperations = ResolveSelectedOperations(options);
            var results = new List<SceneBenchmarkResult>(selectedScenes.Count * selectedOperations.Count);

            foreach (DrawingSampleScene scene in selectedScenes)
            {
                foreach (SceneBenchmarkOperation operation in selectedOperations)
                {
                    results.Add(Run(scene, operation, options));
                }
            }

            return results;
        }

        private SceneBenchmarkResult Run(
            DrawingSampleScene scene,
            SceneBenchmarkOperation operation,
            SceneBenchmarkOptions options)
        {
            return operation switch
            {
                SceneBenchmarkOperation.BuildScene => Measure(
                    scene,
                    operation,
                    options,
                    execute: () =>
                    {
                        using DrawingEngine engine = scene.CreateEngine();
                        return checked(engine.Width * engine.Height);
                    }),
                SceneBenchmarkOperation.RenderPng => MeasureRender(scene, options),
                SceneBenchmarkOperation.RenderSvg => MeasureSvgExport(scene, options),
                SceneBenchmarkOperation.RenderPdf => MeasurePdfExport(scene, options),
                SceneBenchmarkOperation.RenderPngWithAnnotations => MeasureRenderWithAnnotations(scene, options),
                SceneBenchmarkOperation.RenderSvgWithAnnotations => MeasureSvgExportWithAnnotations(scene, options),
                SceneBenchmarkOperation.RenderPdfWithAnnotations => MeasurePdfExportWithAnnotations(scene, options),
                SceneBenchmarkOperation.HitTest => MeasureHitTest(scene, options),
                _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "Unsupported benchmark operation.")
            };
        }

        private static SceneBenchmarkResult MeasureRender(DrawingSampleScene scene, SceneBenchmarkOptions options)
        {
            DrawingEngine? engine = null;

            return Measure(
                scene,
                SceneBenchmarkOperation.RenderPng,
                options,
                setup: () => engine = scene.CreateEngine(),
                cleanup: () => engine?.Dispose(),
                execute: () =>
                {
                    using SKImage image = engine!.RenderToImage();
                    using SKData encoded = image.Encode(SKEncodedImageFormat.Png, 100);
                    return checked((int)encoded.Size);
                });
        }

        private static SceneBenchmarkResult MeasureSvgExport(DrawingSampleScene scene, SceneBenchmarkOptions options)
        {
            DrawingEngine? engine = null;

            return Measure(
                scene,
                SceneBenchmarkOperation.RenderSvg,
                options,
                setup: () => engine = scene.CreateEngine(),
                cleanup: () => engine?.Dispose(),
                execute: () =>
                {
                    string svg = SvgExporter.ExportToString(engine!);
                    return checked(System.Text.Encoding.UTF8.GetByteCount(svg));
                });
        }

        private static SceneBenchmarkResult MeasurePdfExport(DrawingSampleScene scene, SceneBenchmarkOptions options)
        {
            DrawingEngine? engine = null;
            string? filePath = null;

            return Measure(
                scene,
                SceneBenchmarkOperation.RenderPdf,
                options,
                setup: () =>
                {
                    engine = scene.CreateEngine();
                    filePath = Path.Combine(Path.GetTempPath(), $"beep-drawing-benchmark-{Guid.NewGuid():N}.pdf");
                },
                cleanup: () =>
                {
                    engine?.Dispose();
                    if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                },
                execute: () =>
                {
                    PdfExporter.Export(engine!, filePath!);
                    return checked((int)new FileInfo(filePath!).Length);
                });
        }

        private static SceneBenchmarkResult MeasureRenderWithAnnotations(DrawingSampleScene scene, SceneBenchmarkOptions options)
        {
            DrawingEngine? engine = null;

            return Measure(
                scene,
                SceneBenchmarkOperation.RenderPngWithAnnotations,
                options,
                setup: () =>
                {
                    engine = scene.CreateEngine();
                    SceneBenchmarkInteractionProfiles.PreparePersistedAnnotations(scene, engine);
                },
                cleanup: () => engine?.Dispose(),
                execute: () =>
                {
                    using SKImage image = engine!.RenderToImage();
                    using SKData encoded = image.Encode(SKEncodedImageFormat.Png, 100);
                    return checked((int)encoded.Size);
                });
        }

        private static SceneBenchmarkResult MeasureSvgExportWithAnnotations(DrawingSampleScene scene, SceneBenchmarkOptions options)
        {
            DrawingEngine? engine = null;

            return Measure(
                scene,
                SceneBenchmarkOperation.RenderSvgWithAnnotations,
                options,
                setup: () =>
                {
                    engine = scene.CreateEngine();
                    SceneBenchmarkInteractionProfiles.PreparePersistedAnnotations(scene, engine);
                },
                cleanup: () => engine?.Dispose(),
                execute: () =>
                {
                    string svg = SvgExporter.ExportToString(engine!);
                    return checked(System.Text.Encoding.UTF8.GetByteCount(svg));
                });
        }

        private static SceneBenchmarkResult MeasurePdfExportWithAnnotations(DrawingSampleScene scene, SceneBenchmarkOptions options)
        {
            DrawingEngine? engine = null;
            string? filePath = null;

            return Measure(
                scene,
                SceneBenchmarkOperation.RenderPdfWithAnnotations,
                options,
                setup: () =>
                {
                    engine = scene.CreateEngine();
                    SceneBenchmarkInteractionProfiles.PreparePersistedAnnotations(scene, engine);
                    filePath = Path.Combine(Path.GetTempPath(), $"beep-drawing-benchmark-annotated-{Guid.NewGuid():N}.pdf");
                },
                cleanup: () =>
                {
                    engine?.Dispose();
                    if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                },
                execute: () =>
                {
                    PdfExporter.Export(engine!, filePath!);
                    return checked((int)new FileInfo(filePath!).Length);
                });
        }

        private static SceneBenchmarkResult MeasureHitTest(DrawingSampleScene scene, SceneBenchmarkOptions options)
        {
            DrawingEngine? engine = null;
            SKPoint hitTestPoint = SKPoint.Empty;

            return Measure(
                scene,
                SceneBenchmarkOperation.HitTest,
                options,
                setup: () =>
                {
                    engine = scene.CreateEngine();
                    hitTestPoint = SceneBenchmarkInteractionProfiles.ResolveHitTestPoint(scene, engine);
                },
                cleanup: () => engine?.Dispose(),
                execute: () => engine!.HitTest(hitTestPoint, screenTolerance: 8f) != null ? 1 : 0);
        }

        private static SceneBenchmarkResult Measure(
            DrawingSampleScene scene,
            SceneBenchmarkOperation operation,
            SceneBenchmarkOptions options,
            Func<int> execute,
            Action? setup = null,
            Action? cleanup = null)
        {
            setup?.Invoke();

            try
            {
                for (int iteration = 0; iteration < options.WarmupIterations; iteration++)
                {
                    execute();
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var durationMeasurements = new List<double>(options.MeasurementIterations);
                var allocationMeasurements = new List<long>(options.MeasurementIterations);
                int payloadBytes = 0;

                for (int iteration = 0; iteration < options.MeasurementIterations; iteration++)
                {
                    long allocatedBefore = GC.GetTotalAllocatedBytes(true);
                    var stopwatch = Stopwatch.StartNew();
                    payloadBytes = execute();
                    stopwatch.Stop();
                    long allocatedAfter = GC.GetTotalAllocatedBytes(true);

                    durationMeasurements.Add(stopwatch.Elapsed.TotalMilliseconds);
                    allocationMeasurements.Add(allocatedAfter - allocatedBefore);
                }

                return new SceneBenchmarkResult
                {
                    SceneName = scene.Name,
                    SceneDescription = scene.Description,
                    Operation = GetOperationName(operation),
                    Width = scene.Width,
                    Height = scene.Height,
                    WarmupIterations = options.WarmupIterations,
                    MeasurementIterations = options.MeasurementIterations,
                    MeanDurationMilliseconds = durationMeasurements.Average(),
                    MedianDurationMilliseconds = CalculateMedian(durationMeasurements),
                    MinimumDurationMilliseconds = durationMeasurements.Min(),
                    MaximumDurationMilliseconds = durationMeasurements.Max(),
                    StandardDeviationMilliseconds = CalculateStandardDeviation(durationMeasurements),
                    MeanAllocatedBytes = Convert.ToInt64(Math.Round(allocationMeasurements.Average(value => (double)value))),
                    PayloadBytes = payloadBytes,
                    MeasuredAtUtc = DateTime.UtcNow
                };
            }
            finally
            {
                cleanup?.Invoke();
            }
        }

        private List<DrawingSampleScene> ResolveSelectedScenes(SceneBenchmarkOptions options)
        {
            if (options.SceneNames.Count == 0)
            {
                return GetAvailableScenes().ToList();
            }

            var selectedScenes = new List<DrawingSampleScene>(options.SceneNames.Count);
            foreach (string sceneName in options.SceneNames)
            {
                if (!scenesByName.TryGetValue(sceneName, out DrawingSampleScene? scene))
                {
                    throw new ArgumentException($"Unknown scene '{sceneName}'. Use --list to inspect the available sample scenes.");
                }

                selectedScenes.Add(scene);
            }

            return selectedScenes;
        }

        private static List<SceneBenchmarkOperation> ResolveSelectedOperations(SceneBenchmarkOptions options)
        {
            if (options.Operations.Count == 0)
            {
                return new List<SceneBenchmarkOperation>
                {
                    SceneBenchmarkOperation.BuildScene,
                    SceneBenchmarkOperation.RenderPng
                };
            }

            return options.Operations.OrderBy(operation => operation).ToList();
        }

        private static void ValidateOptions(SceneBenchmarkOptions options)
        {
            if (options.WarmupIterations < 0)
                throw new ArgumentOutOfRangeException(nameof(options.WarmupIterations), "Warmup iterations must be zero or greater.");

            if (options.MeasurementIterations <= 0)
                throw new ArgumentOutOfRangeException(nameof(options.MeasurementIterations), "Measurement iterations must be greater than zero.");
        }

        private static double CalculateMedian(IReadOnlyList<double> values)
        {
            var ordered = values.OrderBy(value => value).ToArray();
            int middleIndex = ordered.Length / 2;

            if (ordered.Length % 2 == 0)
            {
                return (ordered[middleIndex - 1] + ordered[middleIndex]) / 2.0;
            }

            return ordered[middleIndex];
        }

        private static double CalculateStandardDeviation(IReadOnlyList<double> values)
        {
            if (values.Count == 1)
                return 0;

            double mean = values.Average();
            double variance = values.Sum(value => Math.Pow(value - mean, 2)) / values.Count;
            return Math.Sqrt(variance);
        }

        public static string GetOperationName(SceneBenchmarkOperation operation)
        {
            return operation switch
            {
                SceneBenchmarkOperation.BuildScene => "build-scene",
                SceneBenchmarkOperation.RenderPng => "render-png",
                SceneBenchmarkOperation.RenderSvg => "render-svg",
                SceneBenchmarkOperation.RenderPdf => "render-pdf",
                SceneBenchmarkOperation.RenderPngWithAnnotations => "render-png-annotated",
                SceneBenchmarkOperation.RenderSvgWithAnnotations => "render-svg-annotated",
                SceneBenchmarkOperation.RenderPdfWithAnnotations => "render-pdf-annotated",
                SceneBenchmarkOperation.HitTest => "hit-test",
                _ => operation.ToString()
            };
        }
    }
}
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Drawing.Benchmarks.Benchmarking
{
    public enum SceneBenchmarkOperation
    {
        BuildScene,
        RenderPng,
        RenderSvg,
        RenderPdf,
        RenderPngWithAnnotations,
        RenderSvgWithAnnotations,
        RenderPdfWithAnnotations,
        HitTest
    }

    public sealed class SceneBenchmarkOptions
    {
        public bool ShowHelp { get; set; }

        public bool ListOnly { get; set; }

        public int WarmupIterations { get; set; } = 1;

        public int MeasurementIterations { get; set; } = 5;

        public HashSet<string> SceneNames { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public HashSet<SceneBenchmarkOperation> Operations { get; } = new HashSet<SceneBenchmarkOperation>();

        public string? OutputPath { get; set; }
    }

    public sealed class SceneBenchmarkResult
    {
        public string SceneName { get; init; } = string.Empty;

        public string SceneDescription { get; init; } = string.Empty;

        public string Operation { get; init; } = string.Empty;

        public int Width { get; init; }

        public int Height { get; init; }

        public int WarmupIterations { get; init; }

        public int MeasurementIterations { get; init; }

        public double MeanDurationMilliseconds { get; init; }

        public double MedianDurationMilliseconds { get; init; }

        public double MinimumDurationMilliseconds { get; init; }

        public double MaximumDurationMilliseconds { get; init; }

        public double StandardDeviationMilliseconds { get; init; }

        public long MeanAllocatedBytes { get; init; }

        public long PayloadBytes { get; init; }

        public DateTime MeasuredAtUtc { get; init; }
    }
}
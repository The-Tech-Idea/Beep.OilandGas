using BenchmarkDotNet.Attributes;
using Beep.OilandGas.GasProperties.Calculations;

namespace Beep.OilandGas.FlashCalculations.Benchmarks;

/// <summary>
/// Benchmarks for Z-factor calculations — the most frequently called PVT routine.
///
/// Run with: dotnet run -c Release --filter *
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
public class ZFactorBenchmarks
{
    private const decimal Pressure = 3000m;   // psia
    private const decimal Temperature = 600m; // °R (140 °F)
    private const decimal SpecificGravity = 0.65m;

    [Benchmark(Baseline = true)]
    public decimal HallYarborough()
    {
        return ZFactorCalculator.CalculateHallYarborough(Pressure, Temperature, SpecificGravity);
    }

    [Benchmark]
    public decimal BrillBeggs()
    {
        return ZFactorCalculator.CalculateBrillBeggs(Pressure, Temperature, SpecificGravity);
    }

    [Benchmark]
    public decimal StandingKatz()
    {
        return ZFactorCalculator.CalculateStandingKatz(Pressure, Temperature, SpecificGravity);
    }

    [Benchmark]
    public decimal PseudoCriticalProperties()
    {
        return ZFactorCalculator.CalculatePseudoCriticalProperties(SpecificGravity).Temperature;
    }
}

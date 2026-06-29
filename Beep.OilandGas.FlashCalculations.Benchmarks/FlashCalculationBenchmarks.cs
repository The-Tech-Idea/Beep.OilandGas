using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Beep.OilandGas.FlashCalculations.Calculations;
using Beep.OilandGas.Models.Data.FlashCalculations;

namespace Beep.OilandGas.FlashCalculations.Benchmarks;

/// <summary>
/// Benchmarks for flash calculation core routines — Wilson K-values,
/// Rachford-Rice solver, and isothermal flash.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
public class FlashCalculationBenchmarks
{
    private List<FLASH_COMPONENT> _feed = null!;

    [GlobalSetup]
    public void Setup()
    {
        _feed = new List<FLASH_COMPONENT>
        {
            new() { COMPONENT_NAME = "Methane",  MOLE_FRACTION = 0.70m, CRITICAL_TEMPERATURE = 343.0m, CRITICAL_PRESSURE = 667.8m, ACENTRIC_FACTOR = 0.0115m, MOLECULAR_WEIGHT = 16.043m },
            new() { COMPONENT_NAME = "Ethane",   MOLE_FRACTION = 0.15m, CRITICAL_TEMPERATURE = 549.8m, CRITICAL_PRESSURE = 707.8m, ACENTRIC_FACTOR = 0.0995m, MOLECULAR_WEIGHT = 30.070m },
            new() { COMPONENT_NAME = "Propane",  MOLE_FRACTION = 0.10m, CRITICAL_TEMPERATURE = 665.7m, CRITICAL_PRESSURE = 616.3m, ACENTRIC_FACTOR = 0.1523m, MOLECULAR_WEIGHT = 44.097m },
            new() { COMPONENT_NAME = "n-Butane", MOLE_FRACTION = 0.03m, CRITICAL_TEMPERATURE = 765.3m, CRITICAL_PRESSURE = 550.7m, ACENTRIC_FACTOR = 0.2002m, MOLECULAR_WEIGHT = 58.123m },
            new() { COMPONENT_NAME = "n-Pentane",MOLE_FRACTION = 0.02m, CRITICAL_TEMPERATURE = 845.4m, CRITICAL_PRESSURE = 488.8m, ACENTRIC_FACTOR = 0.2515m, MOLECULAR_WEIGHT = 72.150m },
        };
    }

    [Benchmark]
    public decimal WilsonKValues()
    {
        decimal sum = 0;
        foreach (var c in _feed)
            sum += FlashCalculator.CalculateWilsonKValue(500m, 560m, c);
        return sum;
    }

    [Benchmark]
    public decimal RachfordRiceSolver()
    {
        return FlashCalculator.SolveRachfordRice(_feed, 500m, 560m, out _, out _);
    }

    [Benchmark]
    public FlashResult IsothermalFlash()
    {
        var conditions = new FLASH_CONDITIONS
        {
            PRESSURE = 500m,
            TEMPERATURE = 560m,
            FEED_COMPOSITION = _feed
        };
        return FlashCalculator.PerformIsothermalFlash(conditions);
    }
}

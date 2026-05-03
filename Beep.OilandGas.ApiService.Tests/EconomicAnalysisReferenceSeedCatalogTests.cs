using System.Linq;
using Beep.OilandGas.EconomicAnalysis.Constants;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class EconomicAnalysisReferenceSeedCatalogTests
{
    [Fact]
    public void MetricRows_AreUniqueByMetric()
    {
        var duplicates = EconomicAnalysisReferenceCodeSeed.GetMetricRows()
            .GroupBy(r => r.Metric)
            .Where(g => g.Count() > 1)
            .ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void ScenarioRows_AreUniqueByScenario()
    {
        var duplicates = EconomicAnalysisReferenceCodeSeed.GetScenarioRows()
            .GroupBy(r => r.Scenario)
            .Where(g => g.Count() > 1)
            .ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void ScheduleRows_AreUniqueBySchedule()
    {
        var duplicates = EconomicAnalysisReferenceCodeSeed.GetScheduleRows()
            .GroupBy(r => r.Schedule)
            .Where(g => g.Count() > 1)
            .ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void SeedCatalog_CoversCriticalFamilies()
    {
        var metrics = EconomicAnalysisReferenceCodeSeed.GetMetricRows().Select(r => r.Metric).ToHashSet();
        var scenarios = EconomicAnalysisReferenceCodeSeed.GetScenarioRows().Select(r => r.Scenario).ToHashSet();
        var schedules = EconomicAnalysisReferenceCodeSeed.GetScheduleRows().Select(r => r.Schedule).ToHashSet();

        Assert.Contains(EconomicMetricCodes.Npv, metrics);
        Assert.Contains(EconomicMetricCodes.Irr, metrics);
        Assert.Contains(EconomicScenarioCodes.Base, scenarios);
        Assert.Contains(EconomicScenarioCodes.Best, scenarios);
        Assert.Contains(EconomicScheduleCodes.Monthly, schedules);
        Assert.Contains(EconomicScheduleCodes.Annual, schedules);
    }
}

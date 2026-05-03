using System.Collections.Generic;

namespace Beep.OilandGas.EconomicAnalysis.Constants;

public static class EconomicAnalysisReferenceCodeSeed
{
    public readonly record struct MetricSeedRow(string Metric, string Abbreviation, string LongName, string ShortName, string ActiveInd = "Y");
    public readonly record struct ScenarioSeedRow(string Scenario, string Abbreviation, string LongName, string ShortName, string ActiveInd = "Y");
    public readonly record struct ScheduleSeedRow(string Schedule, string Abbreviation, string LongName, string ShortName, string ActiveInd = "Y");

    public static IEnumerable<MetricSeedRow> GetMetricRows()
    {
        yield return new(EconomicMetricCodes.Npv, "NPV", "Net Present Value", "NPV");
        yield return new(EconomicMetricCodes.Irr, "IRR", "Internal Rate Of Return", "IRR");
        yield return new(EconomicMetricCodes.PaybackPeriod, "PBP", "Payback Period", "PBP");
        yield return new(EconomicMetricCodes.ProfitabilityIndex, "PI", "Profitability Index", "PI");
        yield return new(EconomicMetricCodes.Roi, "ROI", "Return On Investment", "ROI");
        yield return new(EconomicMetricCodes.Mirr, "MIRR", "Modified Internal Rate Of Return", "MIRR");
    }

    public static IEnumerable<ScenarioSeedRow> GetScenarioRows()
    {
        yield return new(EconomicScenarioCodes.Base, "BASE", "Base Case", "BASE");
        yield return new(EconomicScenarioCodes.Best, "BEST", "Best Case", "BEST");
        yield return new(EconomicScenarioCodes.Worst, "WORST", "Worst Case", "WORST");
        yield return new(EconomicScenarioCodes.Stress, "STRESS", "Stress Case", "STRESS");
    }

    public static IEnumerable<ScheduleSeedRow> GetScheduleRows()
    {
        yield return new(EconomicScheduleCodes.Monthly, "M", "Monthly", "MONTH");
        yield return new(EconomicScheduleCodes.Quarterly, "Q", "Quarterly", "QTR");
        yield return new(EconomicScheduleCodes.Annual, "Y", "Annual", "YEAR");
        yield return new(EconomicScheduleCodes.LifeOfField, "LOF", "Life Of Field", "LOF");
    }
}

using TheTechIdea.Beep.OilandGas.Desktop.Data.WellTest;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WTA-02: the register rules — the one-well link, the reference codes, the duplicate, pending versus failed, the units and
/// the delete guard. The database behaviour (create, reload, duplicate refusal against real rows, delete with a linked
/// analysis) is proven in the gateway test.
/// </summary>
public sealed class WellTestRegisterTests
{
    private const string PageFileName = "WellTestRegisterPageView.cs";

    private static readonly IReadOnlyList<string> Types = ["BUILDUP", "DRAWDOWN", "PBU"];
    private static readonly IReadOnlyList<string> Results = ["OK", "FAILED", "INCONCLUSIVE"];

    [Fact]
    public void A_test_links_to_exactly_one_well()
    {
        // AC-1 and SG-08-4: the natural key starts with the well, so one test cannot belong to two wells.
        Assert.Equal(
            "WELL-1|BUILDUP|1|1",
            WellTestRegisterRules.TestKey("WELL-1", "BUILDUP", 1, 1));

        // The same well, type, run and test number is the same key whatever the casing or spacing of the well.
        Assert.Equal(
            WellTestRegisterRules.TestKey("WELL-1", "BUILDUP", 1, 1),
            WellTestRegisterRules.TestKey(" WELL-1 ", "buildup", 1, 1));

        // A different well is a different test, and so is a different type.
        Assert.NotEqual(
            WellTestRegisterRules.TestKey("WELL-1", "BUILDUP", 1, 1),
            WellTestRegisterRules.TestKey("WELL-2", "BUILDUP", 1, 1));

        Assert.NotEqual(
            WellTestRegisterRules.TestKey("WELL-1", "BUILDUP", 1, 1),
            WellTestRegisterRules.TestKey("WELL-1", "DRAWDOWN", 1, 1));

        // A test with no well is refused before anything else is considered.
        var noWell = WellTestRegisterRules.Validate(Entry(well: " "), Types, Results);

        Assert.Equal(WellTestRegisterRules.CodeWellRequired, noWell!.Value.Code);
        Assert.Contains("exactly one well", noWell.Value.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void The_type_and_result_codes_come_from_the_reference_tables()
    {
        // AC-2 and the feature's open question: the lists are read, never assumed.
        Assert.Null(WellTestRegisterRules.Validate(Entry(type: "BUILDUP", result: "OK"), Types, Results));

        var unknownType = WellTestRegisterRules.Validate(Entry(type: "ISochronal"), Types, Results);

        Assert.Equal(WellTestRegisterRules.CodeTypeUnknown, unknownType!.Value.Code);
        Assert.Contains("R_WELL_TEST_TYPE", unknownType.Value.Message, StringComparison.Ordinal);
        Assert.Contains("DRAWDOWN", unknownType.Value.Message, StringComparison.Ordinal);

        var unknownResult = WellTestRegisterRules.Validate(Entry(result: "MAGIC"), Types, Results);

        Assert.Equal(WellTestRegisterRules.CodeResultUnknown, unknownResult!.Value.Code);
        Assert.Contains("R_TEST_RESULT", unknownResult.Value.Message, StringComparison.Ordinal);

        // An empty reference table is a state, and it says to seed it rather than accepting any code.
        var empty = WellTestRegisterRules.Validate(Entry(), [], []);

        Assert.Equal(WellTestRegisterRules.CodeTypeUnknown, empty!.Value.Code);
        Assert.Contains("seed the reference table first", empty.Value.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void A_pending_result_is_distinct_from_a_failed_one()
    {
        // AC-4: an absent code reads as pending, and a failure reads as the code the reference table names.
        var pending = Summary(result: null, resultName: null);

        Assert.True(pending.PendingResult);
        Assert.Equal("pending result", pending.StatusText);
        Assert.DoesNotContain("fail", pending.StatusText, StringComparison.OrdinalIgnoreCase);

        var failed = Summary(result: "FAILED", resultName: "Test failed");

        Assert.False(failed.PendingResult);
        Assert.Contains("FAILED", failed.StatusText, StringComparison.Ordinal);
        Assert.Contains("Test failed", failed.StatusText, StringComparison.Ordinal);

        // A blank code is pending too, not a failure.
        Assert.True(Summary(result: "  ", resultName: null).PendingResult);
    }

    [Fact]
    public void Rates_and_pressures_carry_units()
    {
        // AC-5.
        var test = Summary(oil: 1200m, oilUnit: "bbl/d", gas: 900m, gasUnit: "Mscf/d",
            pressure: 2400m, pressureUnit: "psia");

        Assert.Contains("1,200 bbl/d", test.RatesText, StringComparison.Ordinal);
        Assert.Contains("900 Mscf/d", test.RatesText, StringComparison.Ordinal);
        Assert.Contains("flowing 2,400 psia", test.RatesText, StringComparison.Ordinal);

        // A value with no unit is refused rather than printed as a bare number.
        var noUnit = WellTestRegisterRules.Validate(
            Entry(oil: 1200m, oilUnit: null), Types, Results);

        Assert.Equal(WellTestRegisterRules.CodeUnitMissing, noUnit!.Value.Code);
        Assert.Contains("oil rate", noUnit.Value.Message, StringComparison.Ordinal);

        // The same for a pressure, and for a duration.
        Assert.Equal(
            WellTestRegisterRules.CodeUnitMissing,
            WellTestRegisterRules.Validate(Entry(pressure: 2400m, pressureUnit: null), Types, Results)!.Value.Code);

        Assert.Equal(
            WellTestRegisterRules.CodeUnitMissing,
            WellTestRegisterRules.Validate(Entry(duration: 24m, durationUnit: null), Types, Results)!.Value.Code);

        Assert.Equal(
            WellTestRegisterRules.CodeUnitMissing,
            WellTestRegisterRules.Validate(Entry(duration: 24m, durationUnit: "weeks"), Types, Results)!.Value.Code);

        // A recorded test prints its duration with its unit, and one without says so.
        Assert.Contains("24 h", Summary(duration: 24m, durationUnit: "h").DurationText, StringComparison.Ordinal);
        Assert.Contains("no duration", Summary().DurationText, StringComparison.Ordinal);

        Assert.Contains("h", WellTestRegisterRules.DurationUnits);
        Assert.Contains("min", WellTestRegisterRules.DurationUnits);
        Assert.Contains("d", WellTestRegisterRules.DurationUnits);
    }

    [Fact]
    public void A_test_without_an_analysis_is_flagged_with_the_link()
    {
        // The state table's "test without analysis - flag with a link to WTA-01".
        var unanalysed = Summary(analyses: 0);

        Assert.False(unanalysed.HasAnalysis);
        Assert.Contains("not analysed", unanalysed.AnalysisText, StringComparison.Ordinal);
        Assert.Contains("well test analysis screen", unanalysed.AnalysisText, StringComparison.Ordinal);

        Assert.True(Summary(analyses: 1).HasAnalysis);
        Assert.Equal("1 analysis", Summary(analyses: 1).AnalysisText);
        Assert.Equal("3 analyses", Summary(analyses: 3).AnalysisText);

        // A test may carry several analyses, which is why the link lives on the analysis side.
        Assert.Contains("analyses", Summary(analyses: 3).Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void The_register_binds_the_reference_lists_and_names_no_code_itself()
    {
        // The page must take its lists from the workspace, and it must not carry a code literal.
        var page = File.ReadAllText(SourceRoot.AllSources().Single(file => Path.GetFileName(file) == PageFileName));

        Assert.Contains("workspace.TestTypes", page, StringComparison.Ordinal);
        Assert.Contains("workspace.ResultCodes", page, StringComparison.Ordinal);
        Assert.Contains("workspace.DurationUnits", page, StringComparison.Ordinal);

        Assert.DoesNotContain("\"BUILDUP\"", page, StringComparison.Ordinal);
        Assert.DoesNotContain("\"FAILED\"", page, StringComparison.Ordinal);
    }

    [Fact]
    public void The_error_codes_are_stated_once_and_are_stable()
    {
        // A refusal code is part of the contract: these are the values the page and the tests refer to.
        Assert.Equal("welltest-register-duplicate", WellTestRegisterRules.CodeDuplicate);
        Assert.Equal("welltest-register-analysis-linked", WellTestRegisterRules.CodeAnalysisLinked);
        Assert.Equal("welltest-register-type-unknown", WellTestRegisterRules.CodeTypeUnknown);
        Assert.Equal("welltest-register-result-unknown", WellTestRegisterRules.CodeResultUnknown);
        Assert.Equal("welltest-register-unit-missing", WellTestRegisterRules.CodeUnitMissing);
        Assert.Equal("welltest-register-unknown-test", WellTestRegisterRules.CodeUnknownTest);
    }

    // ------------------------------------------------------------------ helpers

    private static OgWellTestEntry Entry(
        string? well = "WELL-1",
        string? type = "BUILDUP",
        string? result = null,
        decimal? duration = null,
        string? durationUnit = null,
        decimal? oil = null,
        string? oilUnit = null,
        decimal? pressure = null,
        string? pressureUnit = null) =>
        new(
            null,
            well,
            new DateTime(2026, 3, 14),
            type,
            null,
            null,
            duration,
            durationUnit,
            result,
            null,
            null,
            oil,
            oilUnit,
            null,
            null,
            pressure,
            pressureUnit);

    private static OgWellTestSummary Summary(
        string? result = null,
        string? resultName = null,
        decimal? duration = null,
        string? durationUnit = null,
        decimal? oil = null,
        string? oilUnit = null,
        decimal? gas = null,
        string? gasUnit = null,
        decimal? pressure = null,
        string? pressureUnit = null,
        int analyses = 0) =>
        new(
            "REG-1",
            "WELL-1|BUILDUP|1|1",
            "WELL-1",
            new DateTime(2026, 3, 14),
            "BUILDUP",
            "Pressure build-up",
            duration,
            durationUnit,
            result,
            resultName,
            null,
            null,
            oil,
            oilUnit,
            gas,
            gasUnit,
            pressure,
            pressureUnit,
            analyses,
            "sara",
            new DateTime(2026, 3, 15));
}

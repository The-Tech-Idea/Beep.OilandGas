using TheTechIdea.Beep.OilandGas.Desktop.Data.Production;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// PRD-04: the factor arithmetic, the residual and tolerance rule, the override-reason rule, the input hash and the
/// row/run formatting - the parts that do not need a database. The measured reads, the commit, the idempotence and the
/// dry run are proven in the gateway test.
/// </summary>
public sealed class AllocationTests
{
    private static readonly DateTime Day = new(2026, 10, 1);

    [Fact]
    public void The_factor_is_the_wells_basis_over_the_total_basis()
    {
        Assert.Equal(0.25m, AllocationRules.Factor(25m, 100m));
        Assert.Equal(0.5m, AllocationRules.Factor(1m, 2m));

        // No basis, or no total basis, is no factor - the row is unallocated, never a zero share.
        Assert.Null(AllocationRules.Factor(null, 100m));
        Assert.Null(AllocationRules.Factor(10m, 0m));
        Assert.Null(AllocationRules.Factor(10m, null));
    }

    [Fact]
    public void A_row_with_no_method_reads_unallocated_and_never_zero()
    {
        var row = Row(measured: 100m, allocated: 0m, factor: null, method: AllocationRules.NoMethod,
            status: OgAllocationRow.UnallocatedStatus);

        Assert.True(row.IsUnallocated);
        Assert.Equal("unallocated", row.AllocatedText);
        Assert.Equal("no residual", row.ResidualText);
        Assert.Equal("no factor", row.FactorText);
        Assert.Contains("(unallocated)", row.Describe(), StringComparison.Ordinal);

        var allocated = Row(measured: 100m, allocated: 40m, factor: 0.4m);

        Assert.False(allocated.IsUnallocated);
        Assert.Equal("40 bbl", allocated.AllocatedText);
        Assert.Equal("40%", allocated.FactorText);
        Assert.Equal("-60 bbl", allocated.ResidualText);

        var gain = Row(measured: 100m, allocated: 120m, factor: 0.6m);

        Assert.Equal("+20 bbl", gain.ResidualText);
    }

    [Fact]
    public void Every_row_carries_its_method_and_residual()
    {
        // AC-1: nothing on a row is left blank - the method, the status and the residual are always stated.
        foreach (var method in AllocationRules.Methods)
        {
            var row = Row(measured: 50m, allocated: 50m, factor: 0.5m, method: method);

            Assert.Equal(method, row.Method);
            Assert.Equal("0 bbl", row.ResidualText);
            Assert.False(string.IsNullOrWhiteSpace(row.Describe()));
        }

        Assert.Equal(["MEASURED_SHARE", "PRIOR_PERIOD", "EQUAL"], AllocationRules.Methods);
    }

    [Fact]
    public void The_tolerance_is_a_percentage_of_the_measured_total()
    {
        Assert.Equal(5m, AllocationRules.ToleranceValue(1000m, 0.5m));
        Assert.Equal(0m, AllocationRules.ToleranceValue(0m, 0.5m));
        Assert.Null(AllocationRules.ToleranceValue(null, 0.5m));

        Assert.False(AllocationRules.IsBeyondTolerance(4m, 1000m, 0.5m));
        Assert.False(AllocationRules.IsBeyondTolerance(-4.9m, 1000m, 0.5m));
        Assert.True(AllocationRules.IsBeyondTolerance(5.5m, 1000m, 0.5m));
        Assert.True(AllocationRules.IsBeyondTolerance(-60m, 1000m, 0.5m));
        Assert.False(AllocationRules.IsBeyondTolerance(null, 1000m, 0.5m));
    }

    [Fact]
    public void A_manual_override_must_record_its_reason()
    {
        var inputs = Inputs(overrides: [new OgAllocationOverride("WELL-1", 50m, null)]);

        Assert.Equal(AllocationRules.CodeOverrideReasonRequired, AllocationRules.Validate(inputs)!.Value.Code);

        Assert.Equal(
            AllocationRules.CodeOverrideReasonRequired,
            AllocationRules.Validate(Inputs(overrides: [new OgAllocationOverride("WELL-1", 50m, "  ")]))!.Value.Code);

        Assert.Null(AllocationRules.Validate(
            Inputs(overrides: [new OgAllocationOverride("WELL-1", 50m, "the test was suspect")])));

        // No override at all is valid.
        Assert.Null(AllocationRules.Validate(Inputs()));
    }

    [Fact]
    public void The_run_inputs_are_validated()
    {
        Assert.Equal(
            AllocationRules.CodePeriodRequired,
            AllocationRules.Validate(Inputs() with { Period = default })!.Value.Code);

        Assert.Equal(
            AllocationRules.CodeMethodInvalid,
            AllocationRules.Validate(Inputs(method: "GUESS"))!.Value.Code);

        Assert.Equal(
            AllocationRules.CodeToleranceInvalid,
            AllocationRules.Validate(Inputs(tolerance: -1m))!.Value.Code);

        Assert.Equal(
            AllocationRules.CodeToleranceInvalid,
            AllocationRules.Validate(Inputs(tolerance: 101m))!.Value.Code);

        Assert.Null(AllocationRules.Validate(Inputs(tolerance: 0m)));
    }

    [Fact]
    public void The_hash_is_stable_and_changes_with_the_inputs()
    {
        var rows = new[]
        {
            Row("WELL-1", measured: 100m, allocated: 60m, factor: 0.6m),
            Row("WELL-2", measured: 50m, allocated: 40m, factor: 0.4m),
        };

        var first = AllocationRules.Hash(Inputs(), rows);
        var again = AllocationRules.Hash(Inputs(), rows);

        Assert.Equal(first, again);
        Assert.Equal(64, first.Length);

        // The row order does not change the hash; the values do.
        var reordered = AllocationRules.Hash(Inputs(), rows.Reverse().ToList());

        Assert.Equal(first, reordered);
        Assert.NotEqual(first, AllocationRules.Hash(Inputs(period: Day.AddDays(1)), rows));
        Assert.NotEqual(first, AllocationRules.Hash(Inputs(method: AllocationRules.Equal), rows));
        Assert.NotEqual(first, AllocationRules.Hash(Inputs(), [rows[0]]));

        var overridden = new[]
        {
            Row("WELL-1", measured: 100m, allocated: 60m, factor: 0.6m) with { OverrideReason = "test suspect" },
            rows[1],
        };

        Assert.NotEqual(first, AllocationRules.Hash(Inputs(), overridden));
    }

    [Fact]
    public void A_run_beyond_tolerance_is_flagged_and_says_why()
    {
        var within = Run(residual: 2m, measured: 1000m);
        var beyond = Run(residual: -60m, measured: 1000m);
        var acknowledged = beyond with { Acknowledged = true };

        Assert.False(within.IsBeyondTolerance);
        Assert.Contains("no flag", within.FlagsText, StringComparison.Ordinal);

        Assert.True(beyond.IsBeyondTolerance);
        Assert.Contains("beyond the tolerance", beyond.FlagsText, StringComparison.Ordinal);
        Assert.Contains("has not been acknowledged", beyond.FlagsText, StringComparison.Ordinal);

        Assert.Contains("acknowledged before the commit", acknowledged.FlagsText, StringComparison.Ordinal);

        Assert.Contains("measured 1,000 bbl", within.Summary, StringComparison.Ordinal);
        Assert.Contains("tolerance 5 bbl (0.5% of measured)", within.Summary, StringComparison.Ordinal);
        Assert.Contains("draft", within.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void A_run_counts_its_unallocated_and_overridden_rows()
    {
        var run = Run(residual: 0m, measured: 150m) with
        {
            Rows =
            [
                Row("WELL-1", measured: 100m, allocated: 100m, factor: 1m),
                Row("WELL-2", measured: 50m, allocated: 50m, factor: 1m) with
                {
                    Status = OgAllocationRow.OverriddenStatus,
                    OverrideReason = "the test was suspect",
                    OverrideBy = "the operator",
                },
                Row("WELL-3", measured: null, allocated: null, factor: null, method: AllocationRules.NoMethod,
                    status: OgAllocationRow.UnallocatedStatus),
            ],
        };

        Assert.Equal(3, run.RowCount);
        Assert.Equal(1, run.UnallocatedCount);
        Assert.Equal(1, run.OverriddenCount);
        Assert.Contains("1 row(s) are unallocated", run.FlagsText, StringComparison.Ordinal);
        Assert.Contains("1 row(s) were manually overridden", run.FlagsText, StringComparison.Ordinal);
        Assert.Contains("the test was suspect", run.BasisText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_explanation_names_the_basis_of_every_well()
    {
        var run = Run(residual: 0m, measured: 150m) with
        {
            Rows =
            [
                Row("WELL-1", measured: 100m, allocated: 100m, factor: 1m) with
                {
                    Basis = "prior period: the mean of the last seven recorded days (100 bbl)",
                },
            ],
        };

        Assert.Contains("WELL-1: measured_share - prior period", run.BasisText, StringComparison.Ordinal);

        var committed = run with { State = OgAllocationRun.CommittedState, CommittedBy = "sara" };

        Assert.True(committed.IsCommitted);
        Assert.Equal("committed", committed.StateText);
        Assert.False(run.IsCommitted);
    }

    [Fact]
    public async Task No_connection_is_a_failure_and_nothing_is_pending()
    {
        var service = new OgAllocationWorkbenchService(() => null, new Quiet());

        var loaded = await service.LoadAsync("F-1", "Alpha");

        Assert.False(loaded.Success);
        Assert.Equal(AllocationRules.CodeNoConnection, loaded.Reference);
        Assert.Null(loaded.Value);
        Assert.Null(service.Pending);

        var retried = await service.RetryCommitAsync(false);

        Assert.False(retried.Success);
        Assert.Equal("allocation-nothing-pending", retried.Reference);
    }

    [Fact]
    public async Task A_commit_needs_an_actor()
    {
        var service = new OgAllocationWorkbenchService(() => null, new Quiet());

        var committed = await service.CommitAsync(Inputs(), acknowledgeResidual: false, actor: " ");

        Assert.False(committed.Success);
        Assert.Equal(AllocationRules.CodeActorRequired, committed.Reference);
    }

    private static OgAllocationInputs Inputs(
        DateTime? period = null,
        string method = AllocationRules.MeasuredShare,
        decimal tolerance = AllocationRules.DefaultTolerancePercent,
        IReadOnlyList<OgAllocationOverride>? overrides = null) =>
        new("F-1", null, period ?? Day, method, tolerance, overrides ?? []);

    private static OgAllocationRow Row(
        string well = "WELL-1",
        decimal? measured = null,
        decimal? allocated = null,
        decimal? factor = null,
        string method = AllocationRules.MeasuredShare,
        string status = OgAllocationRow.AllocatedStatus) =>
        new(
            well,
            well,
            measured,
            allocated,
            "bbl",
            factor,
            AllocationRules.Residual(allocated, measured),
            method,
            status,
            "measured share",
            null,
            null);

    private static OgAllocationRun Run(decimal residual, decimal measured) =>
        new(
            string.Empty,
            "F-1",
            null,
            Day,
            AllocationRules.MeasuredShare,
            [Row()],
            measured,
            measured + residual,
            residual,
            "bbl",
            "hash",
            OgAllocationRun.DraftState,
            AllocationRules.DefaultTolerancePercent,
            false,
            null,
            null,
            "the field's own well measurements");

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}

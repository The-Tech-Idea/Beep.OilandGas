using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Decommissioning;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DEC-03 against a real SQLite database: the total is computed in decimal from the recorded inputs (AC-1); a revision appends
/// and leaves the version it replaces untouched (AC-2, SG-12-3); the restoration record has its own state and only gates the
/// facility's completion (AC-3, SG-12-4); the approval records actor, date and conditions with the author excluded (AC-6); and
/// the provision preview names the accounting period (AC-4). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class FacilityCostGatewayTests
{
    private const string ConnectionName = "dec03-facilitycost";

    private static readonly OgHseScope Scope = new("SMOKE", "F-1");

    [IntegrationFact]
    public async Task A_revision_appends_and_the_restoration_gates_completion()
    {        var root = Path.Combine(OgPaths.DataRoot, "dec03-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasDec03"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "facility.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "facility.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Execute(database, """
            CREATE TABLE DECOMMISSIONING_STATUS (
                PLANNED_DATE TEXT,
                PLANNED_BY TEXT,
                DECOMMISSIONING_METHOD TEXT,
                DECOMMISSIONING_STATUS_ID TEXT,
                FACILITY_DECOMMISSIONING_ID TEXT,
                STATUS TEXT,
                STATUS_DATE TEXT,
                STATUS_CHANGED_BY TEXT,
                NOTES TEXT,
                FIELD_ID TEXT,
                FACILITY_ID TEXT,
                DECOM_PHASE TEXT,
                REGULATORY_AUTHORITY TEXT,
                REGULATORY_APPROVAL_NUMBER TEXT,
                ESTIMATED_COST REAL,
                ACTUAL_COST REAL,
                COST_CURRENCY TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (DECOMMISSIONING_STATUS_ID));

            CREATE TABLE DECOMMISSIONING_COST_REVISION (
                DECOMMISSIONING_COST_REVISION_ID TEXT,
                FACILITY_DECOMMISSIONING_ID TEXT,
                FACILITY_ID TEXT,
                FIELD_ID TEXT,
                REVISION_NO REAL,
                PREVIOUS_REVISION_ID TEXT,
                REASON TEXT,
                LINES_JSON TEXT,
                BASE_COST REAL,
                ESCALATION_RATE REAL,
                ESCALATION_YEARS REAL,
                CONTINGENCY_PCT REAL,
                TOTAL_COST REAL,
                CURRENCY TEXT,
                STATUS TEXT,
                AUTHOR TEXT,
                APPROVED_BY TEXT,
                APPROVED_DATE TEXT,
                APPROVAL_CONDITIONS TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (DECOMMISSIONING_COST_REVISION_ID));

            CREATE TABLE ENVIRONMENTAL_RESTORATION (
                RESTORATION_ID TEXT,
                SITE_ID TEXT,
                SITE_TYPE TEXT,
                RESTORATION_TYPE TEXT,
                START_DATE TEXT,
                COMPLETION_DATE TEXT,
                STATUS TEXT,
                CERTIFICATION_STATUS TEXT,
                CERTIFICATION_DATE TEXT,
                CERTIFYING_AUTHORITY TEXT,
                CONTRACTOR_ID TEXT,
                ENVIRONMENTAL_IMPACT TEXT,
                REMEDIATION_METHOD TEXT,
                REMARK TEXT,
                SOURCE TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (RESTORATION_ID));
            """);

        var service = new OgFacilityCostService(() => source, reporter);
        var planned = new DateTime(2027, 3, 15);

        // ---- AC-1 and AC-5: the first estimate, computed step by step in its own currency ----
        var first = await service.SaveEstimateAsync(
            Scope,
            "FAC-1",
            planned,
            "REMOVAL",
            "USD",
            [new OgCostLine("ENGINEERING", 400_000m), new OgCostLine("REMOVAL", 600_000m)],
            0.03m,
            2m,
            10m,
            null,
            "sara");

        Assert.True(first.Success, first.Message + " :: " + string.Join(" | ", reporter.Failures));

        var revision1 = first.Value!.Current!;

        Assert.Equal(1, revision1.RevisionNo);
        Assert.Equal(1_000_000m, revision1.Computation.Base);
        Assert.Equal(1_060_900m, revision1.Computation.Escalated);
        Assert.Equal(106_090m, revision1.Computation.ContingencyAmount);
        Assert.Equal(1_166_990m, revision1.Computation.Total);
        Assert.Equal("USD", revision1.Computation.Currency);
        Assert.Contains("base 1000", revision1.Computation.StepsText.Replace(",", string.Empty), StringComparison.Ordinal);

        // The facility's own record carries the plan and the current figure, which is what DEC-01 reads.
        Assert.Equal(1, Count(database,
            "DECOMMISSIONING_STATUS WHERE FACILITY_DECOMMISSIONING_ID = 'FD-FAC-1' AND STATUS = 'PLANNED' AND PLANNED_DATE LIKE '2027-03-15%' AND ESTIMATED_COST = 1166990"));
        Assert.Equal("2027-03", first.Value.ProvisionPeriod);

        // ---- AC-2 / SG-12-3: a revision appends and never rewrites ----
        var second = await service.SaveEstimateAsync(
            Scope,
            "FAC-1",
            planned,
            "REMOVAL",
            "USD",
            [new OgCostLine("ENGINEERING", 500_000m), new OgCostLine("REMOVAL", 700_000m)],
            0.03m,
            2m,
            10m,
            "Scope grew after the site survey.",
            "sara");

        Assert.True(second.Success, second.Message + " :: " + string.Join(" | ", reporter.Failures));

        var revision2 = second.Value!.Current!;

        Assert.Equal(2, revision2.RevisionNo);
        Assert.Equal(revision1.Id, revision2.PreviousId);
        Assert.Equal("Scope grew after the site survey.", revision2.Reason);

        // The first revision's own row is unchanged: same base, same total, same author.
        Assert.Equal(1, Count(database,
            "DECOMMISSIONING_COST_REVISION WHERE DECOMMISSIONING_COST_REVISION_ID = '" + revision1.Id +
            "' AND REVISION_NO = 1 AND BASE_COST = 1000000 AND TOTAL_COST = 1166990 AND AUTHOR = 'sara' AND STATUS = 'DRAFT'"));
        Assert.Equal(2, Count(database, "DECOMMISSIONING_COST_REVISION WHERE FACILITY_DECOMMISSIONING_ID = 'FD-FAC-1'"));

        // A revision needs its reason; the first estimate may explain itself in the record.
        var noReason = await service.SaveEstimateAsync(
            Scope, "FAC-1", planned, "REMOVAL", "USD",
            [new OgCostLine("REMOVAL", 1m)], 0m, 0m, 0m, "  ", "sara");

        Assert.False(noReason.Success);
        Assert.Equal(FacilityCostRules.CodeReasonRequired, noReason.Reference);
        Assert.Equal(2, Count(database, "DECOMMISSIONING_COST_REVISION WHERE FACILITY_DECOMMISSIONING_ID = 'FD-FAC-1'"));

        // ---- AC-5: nothing is defaulted ----
        var noCurrency = await service.SaveEstimateAsync(
            Scope, "FAC-1", planned, "REMOVAL", " ", [new OgCostLine("REMOVAL", 1m)], 0m, 0m, 0m, "why", "sara");

        Assert.False(noCurrency.Success);
        Assert.Equal(FacilityCostRules.CodeCurrencyRequired, noCurrency.Reference);

        // ---- AC-6: the approval records actor, date and conditions, and not by the author ----
        var self = await service.ApproveAsync(Scope, "FAC-1", revision1.Id, "no conditions", "sara");

        Assert.False(self.Success);
        Assert.Equal(FacilityCostRules.CodeSelfApproval, self.Reference);

        var noConditions = await service.ApproveAsync(Scope, "FAC-1", revision1.Id, "  ", "nadia");

        Assert.False(noConditions.Success);
        Assert.Equal(FacilityCostRules.CodeConditionsRequired, noConditions.Reference);

        var approved = await service.ApproveAsync(
            Scope, "FAC-1", revision1.Id, "Not to be exceeded without a new revision.", "nadia");

        Assert.True(approved.Success, approved.Message);

        Assert.Equal(1, Count(database,
            "DECOMMISSIONING_COST_REVISION WHERE DECOMMISSIONING_COST_REVISION_ID = '" + revision1.Id +
            "' AND STATUS = 'APPROVED' AND APPROVED_BY = 'nadia' AND APPROVED_DATE IS NOT NULL AND APPROVAL_CONDITIONS LIKE 'Not to be exceeded%'"));
        Assert.Contains("provision preview 1166990.00 USD", approved.Value.ProvisionText, StringComparison.Ordinal);
        Assert.Contains("ACC-02 owns the posting", approved.Value.ProvisionText, StringComparison.Ordinal);

        // ---- AC-3 / SG-12-4: the restoration is its own record and only gates completion ----
        var noRestoration = await service.CompleteFacilityAsync(Scope, "FAC-1", "All done.", "nadia");

        Assert.False(noRestoration.Success);
        Assert.Contains("No environmental restoration", noRestoration.Message!, StringComparison.Ordinal);

        var plannedRestoration = await service.PlanRestorationAsync(Scope, "FAC-1", "SOIL", "Excavate and replace", "nadia");

        Assert.True(plannedRestoration.Success, plannedRestoration.Message);

        var restoration = Assert.Single(plannedRestoration.Value!.Restoration);

        Assert.Equal(FacilityCostRules.Planned, restoration.Status);

        var stillBlocked = await service.CompleteFacilityAsync(Scope, "FAC-1", "All done.", "nadia");

        Assert.False(stillBlocked.Success);
        Assert.Contains("not complete", stillBlocked.Message!, StringComparison.Ordinal);

        var restored = await service.CompleteRestorationAsync(
            Scope, "FAC-1", restoration.Id, "Site inspection report 42", "nadia");

        Assert.True(restored.Success, restored.Message);
        Assert.True(Assert.Single(restored.Value!.Restoration).IsCertified);

        // The restoration being complete did not move the facility: its own state is still PLANNED.
        Assert.Equal("PLANNED", restored.Value.Facility!.State);
        // Two PLANNED rows: each estimate appended its own planning state, which is why the latest is the current one.
        Assert.Equal(2, Count(database,
            "DECOMMISSIONING_STATUS WHERE FACILITY_DECOMMISSIONING_ID = 'FD-FAC-1' AND STATUS = 'PLANNED'"));

        // ---- and now the facility completes, recording actor and state ----
        var completed = await service.CompleteFacilityAsync(
            Scope, "FAC-1", "Restoration certified and the facility removed.", "nadia");

        Assert.True(completed.Success, completed.Message);
        Assert.True(completed.Value!.IsComplete);
        Assert.Equal(FacilityCostRules.Completed, completed.Value.Facility!.State);
        Assert.Equal(1, Count(database,
            "DECOMMISSIONING_STATUS WHERE FACILITY_DECOMMISSIONING_ID = 'FD-FAC-1' AND STATUS = 'COMPLETED'"));
        Assert.Equal(3, Count(database, "DECOMMISSIONING_STATUS WHERE FACILITY_DECOMMISSIONING_ID = 'FD-FAC-1'"));

        // The revisions are untouched by the completion.
        Assert.Equal(2, Count(database, "DECOMMISSIONING_COST_REVISION WHERE FACILITY_DECOMMISSIONING_ID = 'FD-FAC-1'"));
        Assert.Equal(1, Count(database,
            "DECOMMISSIONING_COST_REVISION WHERE DECOMMISSIONING_COST_REVISION_ID = '" + revision1.Id +
            "' AND TOTAL_COST = 1166990"));

        // Completing twice is refused.
        var twice = await service.CompleteFacilityAsync(Scope, "FAC-1", "Again.", "nadia");

        Assert.False(twice.Success);
        Assert.Contains("already completed", twice.Message!, StringComparison.Ordinal);

        // ---- the failure path: no connection is not an empty estimate ----
        var offline = new OgFacilityCostService(() => null, reporter);
        var noConnection = await offline.LoadAsync(Scope, "FAC-1");

        Assert.False(noConnection.Success);
        Assert.Null(noConnection.Value);
    }

    // ------------------------------------------------------------------ the schema

    private static void Execute(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static long Count(string database, string fromAndWhere)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM " + fromAndWhere + ";";

        return Convert.ToInt64(command.ExecuteScalar());
    }

    private sealed class Recording : IFailureReporter
    {
        public List<string> Failures { get; } = [];

        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error)
        {
            Failures.Add($"{operation} | {consequence}");
            return FailureReferenceFormat.New();
        }
    }
}

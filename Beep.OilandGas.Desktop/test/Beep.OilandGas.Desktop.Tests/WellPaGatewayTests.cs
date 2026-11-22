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
/// DEC-02 against a real SQLite database: planning records the planned date DEC-01 needs and the barriers with their depths and
/// units (AC-4); a barrier is verified only by its evidence (AC-1); the required set is the owner's template, so all three of
/// its envelopes must be verified before closure (AC-2); an open leak path blocks the closure until it is addressed (AC-3);
/// and the closure records the actor, the date and the verification, with the planner kept out of it (AC-6).
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class WellPaGatewayTests
{
    private const string ConnectionName = "dec02-wellpa";

    private static readonly OgHseScope Scope = new("SMOKE", "F-1");

    [IntegrationFact]
    public async Task A_programme_is_planned_evidenced_and_closed_only_when_every_barrier_counts()
    {        var root = Path.Combine(OgPaths.DataRoot, "dec02-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasDec02"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "wellpa.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "wellpa.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var wellPa = new OgWellPaService(() => source, reporter);

        // ---- plan the programme: the planned date and one barrier per required envelope ----
        var planned = await wellPa.PlanAsync(
            Scope,
            "WELL-1",
            new DateTime(2027, 3, 15),
            [
                new OgBarrierInput("Primary", "CEMENT_PLUG", "Deep plug", 2500m, 2560m, "m", "Cement bond log"),
                new OgBarrierInput("Secondary", "CEMENT_PLUG", "Intermediate plug", 1500m, 1560m, "m", "Pressure test"),
                new OgBarrierInput("Environmental", "CEMENT_PLUG", "Surface plug", 300m, 360m, "m", "Plug tag"),
            ],
            "sara");

        Assert.True(planned.Success, planned.Message + " :: " + string.Join(" | ", reporter.Failures));

        var programme = planned.Value!;

        Assert.Equal("WELL-1", programme.WellId);
        Assert.Equal(WellPaRules.Planned, programme.Status);
        Assert.Equal(new DateTime(2027, 3, 15), programme.PlannedDate);
        Assert.Equal("sara", programme.PlannedBy);
        Assert.Equal(3, programme.Barriers.Count);
        Assert.All(programme.Barriers, barrier => Assert.False(barrier.IsVerifiedWithEvidence));

        // AC-4: the depth is stored and shown with its unit.
        var primary = programme.Barriers.Single(barrier => barrier.Envelope == "PRIMARY");
        Assert.Equal(2500m, primary.TopDepth);
        Assert.Equal("m", primary.DepthUom);
        Assert.Equal("2500 - 2560 m", primary.DepthText);

        // The planned date is on the abandonment record, which is what DEC-01 reads for its planned year.
        Assert.Equal(1, Count(database,
            "ABANDONMENT_STATUS WHERE WELL_ABANDONMENT_ID = 'WA-WELL-1' AND STATUS = 'PLANNED' AND PLANNED_DATE LIKE '2027-03-15%' AND PLANNED_BY = 'sara'"));

        // ---- AC-4's refusal: a depth without its unit is not a depth ----
        var noUnit = await wellPa.PlanAsync(
            Scope, "WELL-1", new DateTime(2027, 3, 15),
            [new OgBarrierInput("Primary", "CEMENT_PLUG", null, 2500m, 2560m, " ", "Cement bond log")],
            "sara");

        Assert.False(noUnit.Success);
        Assert.Equal(WellPaRules.CodeDepthUnitRequired, noUnit.Reference);

        // ---- AC-1: a barrier is verified by its evidence, never by assertion ----
        var noEvidence = await wellPa.VerifyBarrierAsync(Scope, "WELL-1", primary.Id, "Cement bond log", "  ", "omar");

        Assert.False(noEvidence.Success);
        Assert.Equal(WellPaRules.CodeEvidenceRequired, noEvidence.Reference);
        Assert.Equal(0, Count(database, "ABANDONMENT_BARRIER WHERE VERIFIED_IND = 'Y'"));

        var verified = await wellPa.VerifyBarrierAsync(Scope, "WELL-1", primary.Id, "Cement bond log", "CBL-2026-11", "omar");

        Assert.True(verified.Success, verified.Message);

        var verifiedPrimary = verified.Value!.Barriers.Single(barrier => barrier.Id == primary.Id);

        Assert.True(verifiedPrimary.IsVerifiedWithEvidence);
        Assert.Equal("CBL-2026-11", verifiedPrimary.EvidenceRef);
        Assert.Equal("omar", verifiedPrimary.VerifiedBy);
        Assert.Equal(1, Count(database,
            "ABANDONMENT_BARRIER WHERE ABANDONMENT_BARRIER_ID = '" + primary.Id + "' AND VERIFIED_IND = 'Y' AND EVIDENCE_REF = 'CBL-2026-11'"));

        // ---- AC-2: the owner's template requires all three envelopes, so one verified barrier is not completion ----
        var blocked = await wellPa.CloseAsync(Scope, "WELL-1", "All barriers verified.", "nadia");

        Assert.False(blocked.Success);
        Assert.Contains("Secondary envelope", blocked.Message!, StringComparison.Ordinal);
        Assert.Contains("Environmental envelope", blocked.Message!, StringComparison.Ordinal);
        Assert.DoesNotContain("Primary envelope", blocked.Message!, StringComparison.Ordinal);
        Assert.Equal(0, Count(database, "ABANDONMENT_STATUS WHERE WELL_ABANDONMENT_ID = 'WA-WELL-1' AND STATUS = 'COMPLETED'"));

        // Verify the other two envelopes.
        foreach (var envelope in new[] { "Secondary", "Environmental" })
        {
            var barrier = verified.Value!.Barriers.Single(candidate => candidate.Envelope == envelope.ToUpperInvariant());

            Assert.True((await wellPa.VerifyBarrierAsync(
                Scope, "WELL-1", barrier.Id, "Pressure test", "EV-" + envelope, "omar")).Success);
        }

        // ---- AC-3: an open leak path blocks the closure until it is addressed ----
        var withPath = await wellPa.AddLeakPathAsync(Scope, "WELL-1", "Annulus pressure at the casing shoe", "nadia");

        Assert.True(withPath.Success, withPath.Message);

        var path = Assert.Single(withPath.Value!.LeakPaths);

        Assert.True(path.IsOpen);
        Assert.Contains("still open", WellPaRules.CompletionBlocks(withPath.Value).Single(), StringComparison.Ordinal);

        var blockedByPath = await wellPa.CloseAsync(Scope, "WELL-1", "All barriers verified.", "nadia");

        Assert.False(blockedByPath.Success);
        Assert.Contains("Annulus pressure at the casing shoe", blockedByPath.Message!, StringComparison.Ordinal);

        var addressed = await wellPa.AddressLeakPathAsync(
            Scope, "WELL-1", path.Id, "Cement squeeze and a pressure test", "nadia");

        Assert.True(addressed.Success, addressed.Message);
        Assert.False(Assert.Single(addressed.Value!.LeakPaths).IsOpen);

        // ---- AC-6's segregation of duties: the planner cannot close ----
        var self = await wellPa.CloseAsync(Scope, "WELL-1", "All barriers verified.", "sara");

        Assert.False(self.Success);
        Assert.Equal(WellPaRules.CodeSelfClosure, self.Reference);
        Assert.Contains("planned this abandonment", self.Message!, StringComparison.Ordinal);

        var byExecutor = await wellPa.CloseAsync(Scope, "WELL-1", "All barriers verified.", "omar");

        Assert.False(byExecutor.Success);
        Assert.Equal(WellPaRules.CodeSelfClosure, byExecutor.Reference);

        // ---- AC-6: the closure records the actor, the date and the verification ----
        var closed = await wellPa.CloseAsync(
            Scope, "WELL-1", "All three envelopes verified and the annulus leak addressed.", "nadia");

        Assert.True(closed.Success, closed.Message + " :: " + string.Join(" | ", reporter.Failures));

        var complete = closed.Value!;

        Assert.True(complete.IsComplete);
        Assert.Equal(WellPaRules.Completed, complete.Status);
        Assert.Contains("nadia", complete.VerificationText, StringComparison.Ordinal);
        Assert.Contains("3 barrier(s) verified", complete.VerificationText, StringComparison.Ordinal);

        Assert.Equal(3, complete.VerifiedBarriers);

        // A closed programme reads as complete rather than as one with a defect left in it.
        Assert.Contains("The abandonment is complete", complete.ChecklistText, StringComparison.Ordinal);
        Assert.Contains("already completed", WellPaRules.CompletionBlocks(complete).Single(), StringComparison.Ordinal);
        Assert.Equal(1, Count(database,
            "ABANDONMENT_STATUS WHERE WELL_ABANDONMENT_ID = 'WA-WELL-1' AND STATUS = 'COMPLETED' AND VERIFIED_BY = 'nadia' AND VERIFICATION_DATE IS NOT NULL"));

        // The PLANNED row is still there: the programme's history is appended, not replaced.
        Assert.Equal(1, Count(database,
            "ABANDONMENT_STATUS WHERE WELL_ABANDONMENT_ID = 'WA-WELL-1' AND STATUS = 'PLANNED'"));
        Assert.Equal(2, Count(database, "ABANDONMENT_STATUS WHERE WELL_ABANDONMENT_ID = 'WA-WELL-1'"));

        // Closing again is refused.
        var twice = await wellPa.CloseAsync(Scope, "WELL-1", "Again.", "nadia");

        Assert.False(twice.Success);
        Assert.Contains("already completed", twice.Message!, StringComparison.Ordinal);

        // ---- the failure path: no connection is not an empty programme ----
        var offline = new OgWellPaService(() => null, reporter);
        var noConnection = await offline.LoadAsync(Scope, "WELL-1");

        Assert.False(noConnection.Success);
        Assert.Null(noConnection.Value);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) =>
        Execute(database, """
            CREATE TABLE ABANDONMENT_STATUS (
                PLANNED_DATE TEXT,
                PLANNED_BY TEXT,
                ABANDONMENT_STATUS_ID TEXT,
                WELL_ABANDONMENT_ID TEXT,
                STATUS TEXT,
                STATUS_DATE TEXT,
                STATUS_CHANGED_BY TEXT,
                NOTES TEXT,
                UWI TEXT,
                WELLBORE_ID TEXT,
                ABANDONMENT_TYPE TEXT,
                REGULATORY_PERMIT_NUMBER TEXT,
                PLUG_BACK_DEPTH_MD REAL,
                PLUG_BACK_DEPTH_OUOM TEXT,
                VERIFIED_BY TEXT,
                VERIFICATION_DATE TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (ABANDONMENT_STATUS_ID));

            CREATE TABLE ABANDONMENT_BARRIER (
                ABANDONMENT_BARRIER_ID TEXT,
                WELL_ABANDONMENT_ID TEXT,
                UWI TEXT,
                ENVELOPE TEXT,
                BARRIER_TYPE TEXT,
                DESCRIPTION TEXT,
                TOP_DEPTH REAL,
                BASE_DEPTH REAL,
                DEPTH_UOM TEXT,
                VERIFICATION_METHOD TEXT,
                EVIDENCE_REF TEXT,
                VERIFIED_BY TEXT,
                VERIFIED_DATE TEXT,
                VERIFIED_IND TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (ABANDONMENT_BARRIER_ID));

            CREATE TABLE ABANDONMENT_LEAK_PATH (
                ABANDONMENT_LEAK_PATH_ID TEXT,
                WELL_ABANDONMENT_ID TEXT,
                UWI TEXT,
                DESCRIPTION TEXT,
                STATUS TEXT,
                ADDRESSED_HOW TEXT,
                ADDRESSED_BY TEXT,
                ADDRESSED_DATE TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (ABANDONMENT_LEAK_PATH_ID));
            """);

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

using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// HSE-07 against a real SQLite database: only the owner's legal transitions are applied (AC-1), a blank jurisdiction and a
/// foreign asset are refused before anything is written (AC-2, AC-3), every decision records actor, date, rationale and
/// conditions with the applicant excluded (AC-4), the document is the record's own content while the PDF export is disabled
/// with its reason (AC-5), and the status history only ever grows (AC-6).
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class HsePermitGatewayTests
{
    private const string ConnectionName = "hse07-permits";

    private static readonly OgHseScope Scope = new("SMOKE", "F-1");

    [IntegrationFact]
    public async Task An_application_is_filed_decided_and_its_history_only_grows()
    {        var root = Path.Combine(OgPaths.DataRoot, "hse07-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasHse07"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "permits.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "permits.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var permits = new OgPermits(() => source, reporter);

        // ---- the inbox: this field's applications, with their recorded histories ----
        var loaded = await permits.LoadAsync(Scope);

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));

        var inbox = loaded.Value!;

        Assert.Single(inbox.Applications);
        Assert.Equal("REF-1", inbox.Applications[0].Number);

        var application = inbox.Applications[0];

        Assert.Equal("PERMIT-1", application.Id);
        Assert.Equal("Drilling", application.Type);
        Assert.Equal("Canada / Alberta", application.Jurisdiction);
        Assert.Equal("WELL-1", application.AssetId);
        Assert.Equal("F-1", application.FieldId);
        Assert.Equal("sara", application.Applicant);
        Assert.Equal(2, application.History.Count);

        // The state is the latest history entry, not a column that could disagree with it.
        Assert.Equal("SUBMITTED", application.State);
        Assert.Equal(new DateTime(2026, 11, 1), application.SubmittedAt);

        // AC-1: the legal next states come from the owner's rules.
        Assert.Contains("UNDER_REVIEW", application.LegalNextStates);
        Assert.Contains("APPROVED", application.LegalNextStates);
        Assert.DoesNotContain("DRAFT", application.LegalNextStates);

        // AC-5: the document is the record's own content, and the export says why it is disabled.
        Assert.Contains("Reference: REF-1", application.Document(), StringComparison.Ordinal);
        Assert.Contains("Scope: Drill the 12-1 well", application.Document(), StringComparison.Ordinal);
        Assert.Contains("PDF export is disabled", OgPermitWorkspace.RendererNotice, StringComparison.Ordinal);
        Assert.False(permits.RenderPdf("PERMIT-1").Success);

        // ---- AC-4: a decision needs its rationale, its conditions, and someone other than the applicant ----
        var noRationale = await permits.DecideAsync("PERMIT-1", "APPROVED", "  ", "Monitor pressure", Scope, "nadia");

        Assert.False(noRationale.Success);
        Assert.Equal(PermitRules.CodeRationaleRequired, noRationale.Reference);

        var noConditions = await permits.DecideAsync("PERMIT-1", "APPROVED", "Approved by the regulator.", null, Scope, "nadia");

        Assert.False(noConditions.Success);
        Assert.Equal(PermitRules.CodeConditionsRequired, noConditions.Reference);

        var self = await permits.DecideAsync("PERMIT-1", "APPROVED", "Approved.", "Monitor pressure", Scope, "sara");

        Assert.False(self.Success);
        Assert.Equal(PermitRules.CodeSelfDecision, self.Reference);
        Assert.Contains("sara", self.Message!, StringComparison.Ordinal);
        Assert.Equal(2, Count(database, "PERMIT_STATUS_HISTORY WHERE PERMIT_APPLICATION_ID = 'PERMIT-1'"));

        // AC-1: a transition the owner's rules do not allow is refused and nothing is written.
        var illegal = await permits.DecideAsync("PERMIT-1", "DRAFT", "Back to draft.", null, Scope, "nadia");

        Assert.False(illegal.Success);
        Assert.Equal(PermitRules.CodeTransitionIllegal, illegal.Reference);
        Assert.Contains("UNDER_REVIEW", illegal.Message!, StringComparison.Ordinal);
        Assert.Equal(2, Count(database, "PERMIT_STATUS_HISTORY WHERE PERMIT_APPLICATION_ID = 'PERMIT-1'"));

        // ---- AC-4 and AC-6: the decision is recorded in the history and the header follows ----
        var approved = await permits.DecideAsync("PERMIT-1", "APPROVED", "Approved by the regulator.",
            "Monitor pressure monthly", Scope, "nadia");

        Assert.True(approved.Success, approved.Message + " :: " + string.Join(" | ", reporter.Failures));

        var decided = approved.Value!.Applications.Single();

        Assert.Equal("APPROVED", decided.State);
        Assert.Equal(3, decided.History.Count);
        Assert.Equal("nadia", decided.History[^1].By);
        Assert.Contains("Approved by the regulator.", decided.History[^1].Remarks!, StringComparison.Ordinal);
        Assert.Contains("conditions: Monitor pressure monthly", decided.History[^1].Remarks!, StringComparison.Ordinal);

        // AC-6: the earlier entries are untouched - the record is append-only.
        Assert.Equal("DRAFT", decided.History[0].State);
        Assert.Equal("Application filed.", decided.History[0].Remarks);
        Assert.Equal("SUBMITTED", decided.History[1].State);
        Assert.Equal(1, Count(database, "PERMIT_STATUS_HISTORY WHERE PERMIT_APPLICATION_ID = 'PERMIT-1' AND STATUS = 'DRAFT' AND STATUS_REMARKS = 'Application filed.'"));

        // The header carries the decision too, so a reader of the table sees it.
        Assert.Equal(1, Count(database,
            "PERMIT_APPLICATION WHERE PERMIT_APPLICATION_ID = 'PERMIT-1' AND STATUS = 'Approved' AND DECISION LIKE 'Approved by the regulator.%' AND DECISION_DATE IS NOT NULL"));

        // ---- filing a new application: the intent key makes a retry adopt its own record ----
        var filed = await permits.CreateAsync(
            Inputs("permit-new"), Scope, "sara");

        Assert.True(filed.Success, filed.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.Equal(2, filed.Value!.Applications.Count);
        Assert.Equal(1, Count(database, "PERMIT_APPLICATION WHERE PERMIT_APPLICATION_ID = 'PERMIT-permit-new' AND STATUS = 'Draft' AND FIELD_ID = 'F-1'"));
        Assert.Equal(1, Count(database, "PERMIT_STATUS_HISTORY WHERE PERMIT_APPLICATION_ID = 'PERMIT-permit-new' AND STATUS = 'DRAFT'"));

        var retried = await permits.CreateAsync(Inputs("permit-new"), Scope, "omar");

        Assert.True(retried.Success, retried.Message);
        Assert.Equal(1, Count(database, "PERMIT_APPLICATION WHERE PERMIT_APPLICATION_ID = 'PERMIT-permit-new'"));
        Assert.Equal(1, Count(database, "PERMIT_STATUS_HISTORY WHERE PERMIT_APPLICATION_ID = 'PERMIT-permit-new'"));

        var conflict = await permits.CreateAsync(
            Inputs("permit-new") with { Scope = "Something else entirely." }, Scope, "omar");

        Assert.False(conflict.Success);
        Assert.Equal(PermitRules.CodeIntentConflict, conflict.Reference);

        // ---- AC-2 and AC-3: the refusals, each before anything is written ----
        var noCountry = await permits.CreateAsync(Inputs("permit-nocountry") with { Country = "  " }, Scope, "sara");

        Assert.False(noCountry.Success);
        Assert.Equal(PermitRules.CodeJurisdictionRequired, noCountry.Reference);
        Assert.Equal(0, Count(database, "PERMIT_APPLICATION WHERE PERMIT_APPLICATION_ID = 'PERMIT-permit-nocountry'"));

        var foreign = await permits.CreateAsync(Inputs("permit-foreign") with { FieldId = "F-2" }, Scope, "sara");

        Assert.False(foreign.Success);
        Assert.Equal(PermitRules.CodeForeignAsset, foreign.Reference);
        Assert.Equal(0, Count(database, "PERMIT_APPLICATION WHERE PERMIT_APPLICATION_ID = 'PERMIT-permit-foreign'"));

        // ---- the header enum problem: the inbox refuses rather than showing another field's application ----
        // ADDITIONAL_INFO_REQUIRED is a storage key whose enum member is AdditionalInformationRequired, so the header
        // cannot be materialised from it. The inbox refuses instead of listing applications whose field cannot be verified.
        Execute(database, "UPDATE PERMIT_APPLICATION SET STATUS = 'ADDITIONAL_INFO_REQUIRED' WHERE PERMIT_APPLICATION_ID = 'PERMIT-1';");

        Assert.Equal(1, Count(database,
            "PERMIT_APPLICATION WHERE PERMIT_APPLICATION_ID = 'PERMIT-1' AND STATUS = 'ADDITIONAL_INFO_REQUIRED'"));

        // The stored key is not the enum member's name, and the register does not depend on it: the state is read from the
        // history, so the inbox still shows the decision it recorded.
        var afterKey = await permits.LoadAsync(Scope);

        Assert.True(afterKey.Success, afterKey.Message + " :: " + string.Join(" | ", reporter.Failures));

        var fromHistory = afterKey.Value!.Applications.Single(candidate => candidate.Id == "PERMIT-1");

        Assert.Equal("APPROVED", fromHistory.State);
        Assert.Equal("Drilling", fromHistory.Type);
        Assert.Equal(3, fromHistory.History.Count);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) =>
        Execute(database, """
            CREATE TABLE PERMIT_APPLICATION (
                PERMIT_APPLICATION_ID TEXT,
                OWNER_TENANT_ID TEXT,
                OWNER_COMPANY_ID TEXT,
                APPLICANT_ID TEXT,
                APPLICATION_TYPE TEXT,
                STATUS TEXT,
                COUNTRY TEXT,
                STATE_PROVINCE TEXT,
                REGULATORY_AUTHORITY TEXT,
                CREATED_DATE TEXT,
                SUBMITTED_BY_USER_ID TEXT,
                SUBMITTED_DATE TEXT,
                RECEIVED_DATE TEXT,
                DECISION_DATE TEXT,
                DECISION TEXT,
                REFERENCE_NUMBER TEXT,
                FEES_DESCRIPTION TEXT,
                FEES_PAID INTEGER,
                REMARKS TEXT,
                SUBMISSION_COMPLETE INTEGER,
                SUBMISSION_DESCRIPTION TEXT,
                WELL_ID TEXT,
                FACILITY_ID TEXT,
                FACILITY_TYPE TEXT,
                FIELD_ID TEXT,
                APPLICATION_ID TEXT,
                OPERATOR_ID TEXT,
                RELATED_WELL_UWI TEXT,
                RELATED_FACILITY_ID TEXT,
                FEES_PAID_IND TEXT,
                SUBMISSION_COMPLETE_IND TEXT,
                WASTE_TYPE TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (PERMIT_APPLICATION_ID));

            CREATE TABLE PERMIT_STATUS_HISTORY (
                PERMIT_STATUS_HISTORY_ID TEXT,
                PERMIT_APPLICATION_ID TEXT,
                STATUS TEXT,
                STATUS_DATE TEXT,
                STATUS_REMARKS TEXT,
                UPDATED_BY TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (PERMIT_STATUS_HISTORY_ID));

            INSERT INTO PERMIT_APPLICATION (PERMIT_APPLICATION_ID, OWNER_TENANT_ID, OWNER_COMPANY_ID, APPLICANT_ID,
                APPLICATION_TYPE, STATUS, COUNTRY, STATE_PROVINCE, CREATED_DATE, SUBMITTED_BY_USER_ID, SUBMITTED_DATE,
                FIELD_ID, WELL_ID, SUBMISSION_DESCRIPTION, REFERENCE_NUMBER, ACTIVE_IND) VALUES
                ('PERMIT-1', 'SMOKE', 'SMOKE', 'sara', 'Drilling', 'Submitted', 'Canada', 'Alberta',
                 '2026-10-30', 'sara', '2026-11-01', 'F-1', 'WELL-1', 'Drill the 12-1 well', 'REF-1', 'Y'),
                ('PERMIT-2', 'SMOKE', 'SMOKE', 'omar', 'Environmental', 'Submitted', 'Canada', 'Alberta',
                 '2026-10-30', 'omar', '2026-11-01', 'F-2', 'WELL-9', 'Another field application', 'REF-2', 'Y');

            INSERT INTO PERMIT_STATUS_HISTORY (PERMIT_STATUS_HISTORY_ID, PERMIT_APPLICATION_ID, STATUS, STATUS_DATE,
                STATUS_REMARKS, UPDATED_BY) VALUES
                ('H-1', 'PERMIT-1', 'DRAFT', '2026-10-30', 'Application filed.', 'sara'),
                ('H-2', 'PERMIT-1', 'SUBMITTED', '2026-11-01', 'Filed with the regulator.', 'sara'),
                ('H-9', 'PERMIT-2', 'DRAFT', '2026-10-30', 'Application filed.', 'omar');
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

    private static OgPermitInputs Inputs(string intent) =>
        new(
            intent,
            "Drilling",
            "Canada",
            "Alberta",
            "WELL",
            "WELL-7",
            "F-1",
            "Drill the 12-7 well");

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

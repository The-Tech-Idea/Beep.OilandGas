using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Work;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WRK-04 against a real SQLite database: the register reads each contractor's licence and its expiry (AC-2); a request records
/// its requester and its window and a retry with the same intent key lands on the same permit (AC-6); the requester cannot issue
/// (AC-3); an incomplete readiness blocks the issue (AC-1) and, once every prerequisite is recorded, the permit is issued and
/// the start is allowed only inside its window (AC-4, AC-5). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class PermitToWorkGatewayTests
{
    private const string ConnectionName = "wrk04-permits";

    private static readonly DateTime AsOf = DateTime.UtcNow;

    [IntegrationFact]
    public async Task A_permit_is_requested_issued_and_gates_the_start()
    {        var root = Path.Combine(OgPaths.DataRoot, "wrk04-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasWrk04"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "permits.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "permits.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgPermitsToWorkService(() => source, reporter);

        // ---- the register: a current contractor and one whose licence has expired (AC-2) ----
        var loaded = await service.LoadAsync(null, AsOf);

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));

        var register = loaded.Value!.Contractors;

        Assert.Equal(2, register.Count);

        var current = register.Single(contractor => contractor.Id == "BA-1");

        Assert.True(current.IsCurrent);
        Assert.Contains("current", current.Describe(), StringComparison.Ordinal);

        var expired = register.Single(contractor => contractor.Id == "BA-2");

        Assert.False(expired.IsCurrent);
        Assert.Contains("expired on 2025-06-30", expired.Describe(), StringComparison.Ordinal);

        // ---- a request records its requester and window, and a retry writes nothing new (AC-6) ----
        var requested = await service.RequestAsync(
            "WO-1", "Replace the wellhead seal", "Pressure", "ISO-1", "PER-1", "MAT-1",
            AsOf.AddHours(-1), AsOf.AddHours(8), "permit-abc", "sara", AsOf);

        Assert.True(requested.Success, requested.Message + " :: " + string.Join(" | ", reporter.Failures));

        var permit = Assert.Single(requested.Value!.Permits);

        Assert.Equal("PTW-permit-abc", permit.Id);
        Assert.Equal(PermitToWorkRules.Requested, permit.State);
        Assert.Equal("sara", permit.Requester);
        Assert.Contains("requested by sara", permit.Describe(), StringComparison.Ordinal);

        var retried = await service.RequestAsync(
            "WO-1", "Replace the wellhead seal", "Pressure", "ISO-1", "PER-1", "MAT-1",
            AsOf.AddHours(-1), AsOf.AddHours(8), "permit-abc", "sara", AsOf);

        Assert.True(retried.Success, retried.Message);
        Assert.Equal(1, Count(database, "WORK_ORDER_EXECUTION_PERMIT"));

        // The same key with a different scope is refused rather than silently reusing the permit.
        var conflict = await service.RequestAsync(
            "WO-1", "Something else entirely", null, null, null, null,
            AsOf.AddHours(-1), AsOf.AddHours(8), "permit-abc", "sara", AsOf);

        Assert.False(conflict.Success);
        Assert.Equal(PermitToWorkRules.CodeAlreadyIssued, conflict.Reference);

        // ---- AC-4: a window needs both ends ----
        var noWindow = await service.RequestAsync(
            "WO-1", "Scope", null, null, null, null, AsOf, AsOf, "permit-nowindow", "sara", AsOf);

        Assert.False(noWindow.Success);
        Assert.Equal(PermitToWorkRules.CodeWindowInvalid, noWindow.Reference);

        // ---- AC-1: the readiness is incomplete, so the issue is refused and names the item ----
        var blocked = await service.IssueAsync("PTW-permit-abc", "No hot work", "nadia", AsOf);

        Assert.False(blocked.Success);
        Assert.Equal(PermitToWorkRules.CodeReadinessIncomplete, blocked.Reference);
        Assert.Contains("MATERIAL_READINESS", blocked.Message!, StringComparison.Ordinal);
        Assert.Equal(0, Count(database, "WORK_ORDER_EXECUTION_PERMIT WHERE STATUS = 'ISSUED'"));

        // ---- AC-3: the requester cannot issue it ----
        Execute(database, """
            UPDATE WORK_ORDER_INSPECTION SET RESULT_STATUS = 'PASS', RESULT_NOTES = 'Materials staged',
                MATERIAL_ACCEPTANCE_REVISION = 'ACC-1' WHERE INSPECTION_ID = 'I-MAT';
            """);

        var self = await service.IssueAsync("PTW-permit-abc", "No hot work", "sara", AsOf);

        Assert.False(self.Success);
        Assert.Equal(PermitToWorkRules.CodeSelfIssue, self.Reference);
        Assert.Contains("cannot issue it", self.Message!, StringComparison.Ordinal);

        // ---- and once every prerequisite is recorded, another person issues it ----
        var issued = await service.IssueAsync("PTW-permit-abc", "No hot work; gas test every shift", "nadia", AsOf);

        Assert.True(issued.Success, issued.Message + " :: " + string.Join(" | ", reporter.Failures));

        var ready = Assert.Single(issued.Value!.Permits);

        Assert.Equal(PermitToWorkRules.Issued, ready.State);
        Assert.Equal("nadia", ready.Issuer);
        Assert.Contains("No hot work", ready.Conditions!, StringComparison.Ordinal);
        Assert.True(ready.StartAllowed, ready.BlocksText);
        Assert.Contains("may start", ready.StartText, StringComparison.Ordinal);
        Assert.Equal(1, Count(database,
            "WORK_ORDER_EXECUTION_PERMIT WHERE PERMIT_ID = 'PTW-permit-abc' AND STATUS = 'ISSUED' AND ISSUED_BY = 'nadia'"));

        // AC-5: every required item carries its evidence reference.
        Assert.All(ready.Readiness.Where(item => item.Satisfied),
            item => Assert.False(string.IsNullOrWhiteSpace(item.EvidenceRef)));

        // ---- AC-6: a second issued permit covering the same moment is refused ----
        var second = await service.RequestAsync(
            "WO-1", "Replace the wellhead seal", null, null, null, null,
            AsOf.AddHours(-1), AsOf.AddHours(8), "permit-abc-2", "sara", AsOf);

        Assert.True(second.Success, second.Message);

        var twice = await service.IssueAsync("PTW-permit-abc-2", "again", "nadia", AsOf);

        Assert.False(twice.Success);
        Assert.Equal(PermitToWorkRules.CodeAlreadyIssued, twice.Reference);
        Assert.Contains("The original is retained", twice.Message!, StringComparison.Ordinal);
        Assert.Equal(1, Count(database, "WORK_ORDER_EXECUTION_PERMIT WHERE STATUS = 'ISSUED'"));

        // ---- AC-2: an expired permit leaves the start refused ----
        Execute(database,
            "UPDATE WORK_ORDER_EXECUTION_PERMIT SET VALID_FROM_UTC = '2026-01-01', VALID_TO_UTC = '2026-01-02' WHERE PERMIT_ID = 'PTW-permit-abc';");

        var expiredWindow = await service.LoadAsync("WO-1", AsOf);

        Assert.True(expiredWindow.Success, expiredWindow.Message);

        var stale = expiredWindow.Value!.Permits.Single(candidate => candidate.Id == "PTW-permit-abc");

        Assert.False(stale.IsWithinWindow);
        Assert.False(stale.StartAllowed);
        Assert.Contains("does not cover this moment", stale.StartText, StringComparison.Ordinal);

        // ---- an expired competency leaves the personnel item unsatisfied ----
        // The expired contractor is assigned to this order, which is exactly what AC-2 is about.
        Execute(database, "UPDATE WORK_ORDER_COMPONENT SET WORK_ORDER_ID = 'WO-1' WHERE COMPONENT_ID = 'C-2';");

        var stalePerson = await service.LoadAsync("WO-1", AsOf);

        Assert.True(stalePerson.Success, stalePerson.Message);

        var personnel = stalePerson.Value!.Permits
            .Single(candidate => candidate.Id == "PTW-permit-abc")
            .Readiness.Single(item => item.Kind == "PERSONNEL_COMPETENCY");

        Assert.False(personnel.Satisfied);
        Assert.Contains("expired on 2025-06-30", personnel.Why, StringComparison.Ordinal);

        // ---- the failure path: no connection is not an empty register ----
        var offline = new OgPermitsToWorkService(() => null, reporter);
        var noConnection = await offline.LoadAsync(null, AsOf);

        Assert.False(noConnection.Success);
        Assert.Null(noConnection.Value);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) =>
        Execute(database, """
            CREATE TABLE WORK_ORDER_EXECUTION_PERMIT (
                REQUESTED_BY TEXT,
                REQUESTED_AT_UTC TEXT,
                CONDITIONS TEXT,
                REQUEST_INTENT_ID TEXT,
                PERMIT_ID TEXT,
                TENANT_ID TEXT,
                BUSINESS_ASSOCIATE_ID TEXT,
                FIELD_ID TEXT,
                WORK_ORDER_ID TEXT,
                PROCESS_INSTANCE_ID TEXT,
                ISSUE_INTENT_ID TEXT,
                ISSUED_BY TEXT,
                STATUS TEXT,
                INSPECTION_REVISION TEXT,
                PERMIT_TYPE TEXT,
                JURISDICTION TEXT,
                SCOPE_DESCRIPTION TEXT,
                ISOLATION_REFERENCE TEXT,
                RISK_REFERENCE TEXT,
                PERSONNEL_REFERENCE TEXT,
                MATERIAL_REFERENCE TEXT,
                VALID_FROM_UTC TEXT,
                VALID_TO_UTC TEXT,
                ISSUE_DATA_JSON TEXT,
                INSPECTION_DATA_JSON TEXT,
                DECISION_INTENT_ID TEXT,
                DECIDED_BY TEXT,
                DECIDED_AT_UTC TEXT,
                DECISION_DATA_JSON TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (PERMIT_ID));

            CREATE TABLE WORK_ORDER (
                WORK_ORDER_ID TEXT,
                ACTIVE_IND TEXT,
                COMPLETE_DATE TEXT,
                DUE_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                FINAL_BILLING_DATE TEXT,
                INSTRUCTIONS TEXT,
                PPDM_GUID TEXT,
                REMARK TEXT,
                REQUEST_DATE TEXT,
                RUSH_IND TEXT,
                SOURCE TEXT,
                WORK_ORDER_NUMBER TEXT,
                WORK_ORDER_TYPE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PRIMARY KEY (WORK_ORDER_ID));

            CREATE TABLE WORK_ORDER_COMPONENT (
                WORK_ORDER_ID TEXT,
                COMPONENT_ID TEXT,
                ACTIVE_IND TEXT,
                ACTIVITY_OBS_NO REAL,
                ANALYSIS_ID TEXT,
                APPLICATION_ID TEXT,
                AREA_ID TEXT,
                AREA_TYPE TEXT,
                AUTHORITY_ID TEXT,
                BA_ORGANIZATION_ID TEXT,
                BA_ORGANIZATION_SEQ_NO REAL,
                BUSINESS_ASSOCIATE_ID TEXT,
                CATALOGUE_ADDITIVE_ID TEXT,
                CATALOGUE_EQUIP_ID TEXT,
                CIRC_ID TEXT,
                CLASSIFICATION_SYSTEM_ID TEXT,
                CLASS_LEVEL_ID TEXT,
                COMPONENT_TYPE TEXT,
                CONSENT_ID TEXT,
                CONSULT_ID TEXT,
                CONTEST_ID TEXT,
                CONTRACT_ID TEXT,
                ECOZONE_ID TEXT,
                EFFECTIVE_DATE TEXT,
                EMPLOYEE_BA_ID TEXT,
                EMPLOYEE_OBS_NO REAL,
                EMPLOYER_BA_ID TEXT,
                ENTITLEMENT_ID TEXT,
                EQUIPMENT_ID TEXT,
                EQUIP_MAINT_ID TEXT,
                EXPIRY_DATE TEXT,
                FACILITY_ID TEXT,
                FACILITY_MAINTAIN_ID TEXT,
                FACILITY_TYPE TEXT,
                FIELD_ID TEXT,
                FIELD_STATION_ID TEXT,
                FINANCE_ID TEXT,
                FOSSIL_ID TEXT,
                INCIDENT_ID TEXT,
                INCIDENT_SET_ID TEXT,
                INCIDENT_TYPE TEXT,
                INFORMATION_ITEM_ID TEXT,
                INFO_ITEM_SUBTYPE TEXT,
                INSPECTION_ID TEXT,
                INSTRUMENT_ID TEXT,
                INTEREST_SET_ID TEXT,
                INTEREST_SET_SEQ_NO REAL,
                JURISDICTION TEXT,
                LAND_RIGHT_ID TEXT,
                LAND_RIGHT_SUBTYPE TEXT,
                LAND_SALE_NUMBER TEXT,
                LANGUAGE TEXT,
                LITHOLOGY_LOG_ID TEXT,
                LITH_LOG_SOURCE TEXT,
                NOTIFICATION_ID TEXT,
                OBLIGATION_ID TEXT,
                OBLIGATION_SEQ_NO REAL,
                ORIGIN_SEQ_NO REAL,
                PALEO_SUMMARY_ID TEXT,
                PDEN_ID TEXT,
                PDEN_SOURCE TEXT,
                PDEN_SUBTYPE TEXT,
                PHYSICAL_ITEM_ID TEXT,
                POOL_ID TEXT,
                PPDM_GUID TEXT,
                PPDM_SYSTEM_ID TEXT,
                PPDM_TABLE_NAME TEXT,
                PRODUCT_TYPE TEXT,
                PROD_STRING_ID TEXT,
                PROD_STRING_SOURCE TEXT,
                PROJECT_ID TEXT,
                RATE_SCHEDULE_ID TEXT,
                REFERENCED_GUID TEXT,
                REFERENCED_SYSTEM_ID TEXT,
                REFERENCED_TABLE_NAME TEXT,
                REMARK TEXT,
                RESENT_ID TEXT,
                RESERVE_CLASS_ID TEXT,
                SAMPLE_ANAL_SOURCE TEXT,
                SEIS_SET_ID TEXT,
                SEIS_SET_SUBTYPE TEXT,
                SEIS_TRANSACTION_ID TEXT,
                SF_SUBTYPE TEXT,
                SOURCE TEXT,
                SPATIAL_DESCRIPTION_ID TEXT,
                SPATIAL_OBS_NO REAL,
                STORE_ID TEXT,
                STORE_STRUCTURE_OBS_NO REAL,
                STRAT_NAME_SET_ID TEXT,
                STRAT_UNIT_ID TEXT,
                SUPPORT_FACILITY_ID TEXT,
                SW_APPLICATION_ID TEXT,
                THESAURUS_ID TEXT,
                THESAURUS_WORD_ID TEXT,
                TRANSACTION_TYPE TEXT,
                UWI TEXT,
                WELL_ACTIVITY_SET_ID TEXT,
                WELL_ACTIVITY_SOURCE TEXT,
                WELL_ACTIVITY_TYPE_ID TEXT,
                WELL_SET_ID TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PRIMARY KEY (WORK_ORDER_ID, COMPONENT_ID));

            CREATE TABLE WORK_ORDER_INSPECTION (
                APPLICABLE_STAGE TEXT,
                DOCUMENT_ACCEPTANCE_DATA_JSON TEXT,
                DOCUMENT_ACCEPTANCE_REVISION TEXT,
                PERSONNEL_ACCEPTANCE_DATA_JSON TEXT,
                PERSONNEL_ACCEPTANCE_REVISION TEXT,
                INSPECTION_ID TEXT,
                WORK_ORDER_ID TEXT,
                FIELD_ID TEXT,
                COND_SEQ INTEGER,
                COND_TYPE TEXT,
                REQUIRED_TEXT TEXT,
                JURISDICTION TEXT,
                REGULATORY_REF TEXT,
                PERMIT_PREREQUISITE_KIND TEXT,
                REQUIREMENT_ACTOR TEXT,
                RESULT_STATUS TEXT,
                RESULT_NOTES TEXT,
                INSPECTOR TEXT,
                INSPECTED_AT_UTC TEXT,
                MATERIAL_ACCEPTANCE_DATA_JSON TEXT,
                MATERIAL_ACCEPTANCE_REVISION TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (INSPECTION_ID));

            CREATE TABLE BUSINESS_ASSOCIATE (
                BUSINESS_ASSOCIATE_ID TEXT,
                ACTIVE_IND TEXT,
                BA_ABBREVIATION TEXT,
                BA_CATEGORY TEXT,
                BA_CODE TEXT,
                BA_LONG_NAME TEXT,
                BA_SHORT_NAME TEXT,
                BA_TYPE TEXT,
                CREDIT_CHECK_DATE TEXT,
                CREDIT_CHECK_IND TEXT,
                CREDIT_CHECK_SOURCE TEXT,
                CREDIT_RATING TEXT,
                CREDIT_RATING_SOURCE TEXT,
                CURRENT_STATUS TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                FIRST_NAME TEXT,
                LAST_NAME TEXT,
                MIDDLE_INITIAL TEXT,
                PPDM_GUID TEXT,
                REMARK TEXT,
                SERVICE_PERIOD TEXT,
                SOURCE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PRIMARY KEY (BUSINESS_ASSOCIATE_ID));

            CREATE TABLE BA_LICENSE (
                LICENSEE_BA_ID TEXT,
                LICENSE_ID TEXT,
                ACTIVE_IND TEXT,
                APPLICATION_ID TEXT,
                DESCRIPTION TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                EXTENDED_DATE TEXT,
                FEES_PAID_IND TEXT,
                GRANTED_BY_BA_ID TEXT,
                GRANTED_BY_CONTACT_ID TEXT,
                GRANTED_DATE TEXT,
                GRANTED_TO_BA_ID TEXT,
                GRANTED_TO_CONTACT_ID TEXT,
                LICENSE_NUM TEXT,
                LICENSE_TERM REAL,
                LICENSE_TERM_UOM TEXT,
                LICENSE_TYPE TEXT,
                PPDM_GUID TEXT,
                RATE_SCHEDULE_ID TEXT,
                REL_LICENSEE_BA_ID TEXT,
                REL_LICENSE_ID TEXT,
                REMARK TEXT,
                SECONDARY_TERM REAL,
                SECONDARY_TERM_UOM TEXT,
                SOURCE TEXT,
                VIOLATION_IND TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PRIMARY KEY (LICENSEE_BA_ID, LICENSE_ID));

            CREATE TABLE BA_LICENSE_STATUS (
                LICENSEE_BA_ID TEXT,
                LICENSE_ID TEXT,
                STATUS_ID TEXT,
                ACTIVE_IND TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                LICENSE_STATUS TEXT,
                LICENSE_STATUS_TYPE TEXT,
                PPDM_GUID TEXT,
                REMARK TEXT,
                SOURCE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PRIMARY KEY (LICENSEE_BA_ID, LICENSE_ID, STATUS_ID));

            INSERT INTO BUSINESS_ASSOCIATE (BUSINESS_ASSOCIATE_ID, BA_LONG_NAME, BA_TYPE, ACTIVE_IND) VALUES
                ('BA-1', 'North Rig Services', 'CONTRACTOR', 'Y'),
                ('BA-2', 'Old Seal Co', 'CONTRACTOR', 'Y');

            INSERT INTO BA_LICENSE (LICENSEE_BA_ID, LICENSE_ID, LICENSE_TYPE, LICENSE_NUM, EXPIRY_DATE, ACTIVE_IND) VALUES
                ('BA-1', 'LIC-1', 'PRESSURE_EQUIPMENT', 'PE-1001', '2027-06-30', 'Y'),
                ('BA-2', 'LIC-2', 'PRESSURE_EQUIPMENT', 'PE-1002', '2025-06-30', 'Y');

            INSERT INTO BA_LICENSE_STATUS (LICENSEE_BA_ID, LICENSE_ID, STATUS_ID, LICENSE_STATUS, EFFECTIVE_DATE,
                ACTIVE_IND) VALUES
                ('BA-1', 'LIC-1', 'S-1', 'GRANTED', '2026-01-01', 'Y'),
                ('BA-2', 'LIC-2', 'S-2', 'GRANTED', '2024-01-01', 'Y');

            INSERT INTO WORK_ORDER (WORK_ORDER_ID, WORK_ORDER_NUMBER, WORK_ORDER_TYPE, DUE_DATE, ACTIVE_IND) VALUES
                ('WO-1', 'WO-0001', 'WO-CORRECTIVE', '2026-12-01', 'Y');

            INSERT INTO WORK_ORDER_COMPONENT (WORK_ORDER_ID, COMPONENT_ID, FIELD_ID, UWI, EMPLOYEE_BA_ID,
                COMPONENT_TYPE, ACTIVE_IND) VALUES
                ('WO-1', 'C-1', 'F-1', 'UWI-1', 'BA-1', 'WELL', 'Y'),
                ('WO-2', 'C-2', 'F-1', 'UWI-2', 'BA-2', 'WELL', 'Y');

            INSERT INTO WORK_ORDER_INSPECTION (INSPECTION_ID, WORK_ORDER_ID, FIELD_ID, COND_SEQ, COND_TYPE,
                PERMIT_PREREQUISITE_KIND, REQUIRED_TEXT, JURISDICTION, REQUIREMENT_ACTOR, RESULT_STATUS, RESULT_NOTES,
                PERSONNEL_ACCEPTANCE_REVISION, ACTIVE_IND) VALUES
                ('I-PER', 'WO-1', 'F-1', 1, 'PRE_START', 'PERSONNEL_READINESS', 'Crew competency', 'TX', 'sara',
                 'PASS', 'Crew checked', 'PER-ACC-1', 'Y'),
                ('I-MAT', 'WO-1', 'F-1', 2, 'PRE_START', 'MATERIAL_READINESS', 'Materials staged', 'TX', 'sara',
                 'PENDING', '', NULL, 'Y');
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

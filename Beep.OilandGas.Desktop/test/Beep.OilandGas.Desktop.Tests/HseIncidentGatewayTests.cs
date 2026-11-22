using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.Models.Data.HSE;
using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// HSE-02 against a real SQLite database: the register reads the owner's header, detail, component and status rows back into
/// one incident per record; a report writes those four rows and is idempotent on its intent key; a blank jurisdiction, a
/// timestamp without an offset and a field that moved are refused before anything is written; a company/field change during a
/// read discards the rows; and a state change is refused because the workflow engine is not wired.
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class HseIncidentGatewayTests
{
    private const string ConnectionName = "hse02-incidents";

    private static readonly OgHseScope Scope = new("SMOKE", "F-1");

    [IntegrationFact]
    public async Task A_report_writes_four_rows_and_the_register_reads_them_back()
    {        var root = Path.Combine(OgPaths.DataRoot, "hse02-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasHse02"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "incidents.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "incidents.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var incidents = new OgIncidents(() => source, reporter);
        var stable = () => Scope;

        // ---- the register ----
        var loaded = await incidents.LoadAsync(incidents.BeginLoad(), Scope, stable, OgIncidentFilter.None);

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));

        var register = loaded.Value!;

        Assert.Equal(2, register.Incidents.Count);
        Assert.Equal(IncidentRules.States, register.States);
        Assert.Equal(IncidentRules.Tiers, register.Tiers);
        Assert.Contains(HseReferenceCodes.IncidentType.Spill, register.Types);

        var first = register.Incidents.Single(incident => incident.Number == "INC-1");

        Assert.Equal(HseReferenceCodes.IncidentType.Spill, first.Type);
        Assert.Equal(HseReferenceCodes.Severity.Tier2, first.Tier);
        Assert.Equal("Serious / recordable", first.Severity);
        Assert.Equal("F-1", first.FieldId);
        Assert.Equal("FAC-1", first.FacilityId);
        Assert.Equal("TX", first.Jurisdiction);
        Assert.Equal(IncidentState.Reported, first.State);
        Assert.Equal("sara", first.Investigator);
        Assert.Equal(1, first.OpenActions);
        Assert.True(first.IsOpen);
        Assert.Contains("UTC", first.OccurredText, StringComparison.Ordinal);
        Assert.Contains("open action(s)", first.Describe(), StringComparison.Ordinal);

        var closed = register.Incidents.Single(incident => incident.Number == "INC-2");

        Assert.Equal(IncidentState.Closed, closed.State);
        Assert.False(closed.IsOpen);
        Assert.Equal(0, closed.OpenActions);
        Assert.Equal("ND", closed.Jurisdiction);

        // ---- filters (AC-6) ----
        var byState = await incidents.LoadAsync(
            incidents.BeginLoad(), Scope, stable, new OgIncidentFilter(State: IncidentState.Closed));

        Assert.True(byState.Success, byState.Message);
        Assert.Single(byState.Value!.Incidents);
        Assert.Equal("INC-2", byState.Value.Incidents[0].Number);

        var byTier = await incidents.LoadAsync(
            incidents.BeginLoad(), Scope, stable, new OgIncidentFilter(Tier: HseReferenceCodes.Severity.Tier2));

        Assert.Equal("INC-1", Assert.Single(byTier.Value!.Incidents).Number);

        var byType = await incidents.LoadAsync(
            incidents.BeginLoad(), Scope, stable,
            new OgIncidentFilter(Type: HseReferenceCodes.IncidentType.NearMiss));

        Assert.Equal("INC-2", Assert.Single(byType.Value!.Incidents).Number);

        var byDate = await incidents.LoadAsync(
            incidents.BeginLoad(), Scope, stable, new OgIncidentFilter(From: new DateTime(2026, 10, 1)));

        Assert.Equal("INC-1", Assert.Single(byDate.Value!.Incidents).Number);

        // An unknown state is refused and its sentence lists the recorded ones.
        var badFilter = await incidents.LoadAsync(
            incidents.BeginLoad(), Scope, stable, new OgIncidentFilter(State: "PENDING_TYPO"));

        Assert.False(badFilter.Success);
        Assert.Equal(IncidentRules.CodeFilterInvalid, badFilter.Reference);
        Assert.Contains(IncidentState.PendingRCA, badFilter.Message!, StringComparison.Ordinal);

        // ---- AC-3: a field change during the read discards the rows ----
        var calls = 0;

        OgHseScope Changing()
        {
            calls++;
            return calls == 1 ? new OgHseScope("SMOKE", "F-1") : new OgHseScope("SMOKE", "F-2");
        }

        var changed = await incidents.LoadAsync(incidents.BeginLoad(), Scope, Changing, OgIncidentFilter.None);

        Assert.False(changed.Success);
        Assert.Null(changed.Value);
        Assert.Equal(IncidentRules.CodeScopeChanged, changed.Reference);
        Assert.Contains("discarded", changed.Message!, StringComparison.Ordinal);

        // A superseded load is discarded the same way.
        var staleVersion = incidents.BeginLoad();
        incidents.BeginLoad();

        var stale = await incidents.LoadAsync(staleVersion, Scope, stable, OgIncidentFilter.None);

        Assert.False(stale.Success);
        Assert.Equal(IncidentRules.CodeScopeChanged, stale.Reference);

        // ---- a complete report writes the header, detail, component and status ----
        var report = Complete();

        var reported = await incidents.ReportAsync(report, Scope, "sara");

        Assert.True(reported.Success, reported.Message + " :: " + string.Join(" | ", reporter.Failures));

        var written = reported.Value!.Incidents.Single(incident => incident.Number == "INC-intent-abc");

        Assert.Equal(HseReferenceCodes.IncidentType.Spill, written.Type);
        Assert.Equal(HseReferenceCodes.Severity.Tier1, written.Tier);
        Assert.Equal("Major / fatal", written.Severity);
        Assert.Equal("ND", written.Jurisdiction);
        Assert.Equal("F-1", written.FieldId);
        Assert.Equal("FAC-9", written.FacilityId);
        Assert.Equal("Rig 7", written.Location);
        Assert.Equal(IncidentState.Reported, written.State);

        Assert.Equal(1, Count(database, "HSE_INCIDENT WHERE INCIDENT_ID = 'INC-intent-abc' AND REMARK = 'A small spill was contained at the separator.'"));
        Assert.Equal(1, Count(database, "HSE_INCIDENT_DETAIL WHERE INCIDENT_ID = 'INC-intent-abc' AND DETAIL_OBS_NO = 1 AND INCIDENT_TYPE_ID = 'SPILL' AND INCIDENT_SEVERITY_ID = 'TIER_1'"));
        Assert.Equal(1, Count(database, "HSE_INCIDENT_DETAIL WHERE INCIDENT_ID = 'INC-intent-abc' AND DETAIL_OBS_NO = 2 AND DETAIL_TYPE = 'ACTION'"));
        Assert.Equal(1, Count(database, "HSE_INCIDENT_DETAIL WHERE INCIDENT_ID = 'INC-intent-abc' AND DETAIL_OBS_NO = 3 AND DETAIL_TYPE = 'PEOPLE'"));
        Assert.Equal(1, Count(database, "HSE_INCIDENT_COMPONENT WHERE INCIDENT_ID = 'INC-intent-abc' AND JURISDICTION = 'ND' AND FIELD_ID = 'F-1' AND FACILITY_ID = 'FAC-9'"));
        Assert.Equal(1, Count(database, "HSE_INCIDENT_RESPONSE WHERE INCIDENT_ID = 'INC-intent-abc' AND RESPONSE_RESULT = 'REPORTED'"));

        // The instant is stored in UTC from the offset it was given: the register reads the row back at 10:00 UTC, and the
        // stored value is on the reported date.
        Assert.Equal(new DateTime(2026, 11, 8, 10, 0, 0), written.OccurredUtc);
        Assert.Equal(1, Count(database,
            "HSE_INCIDENT WHERE INCIDENT_ID = 'INC-intent-abc' AND INCIDENT_DATE LIKE '2026-11-08%'"));

        // ---- retained-original: the same intent does not write a second incident ----
        var retried = await incidents.ReportAsync(report, Scope, "omar");

        Assert.True(retried.Success, retried.Message);
        Assert.Equal(1, Count(database, "HSE_INCIDENT WHERE INCIDENT_ID = 'INC-intent-abc'"));
        Assert.Equal(1, Count(database, "HSE_INCIDENT_RESPONSE WHERE INCIDENT_ID = 'INC-intent-abc'"));

        // The same intent with different content is a conflict, not an overwrite.
        var conflict = await incidents.ReportAsync(report with { Description = "Something else entirely." }, Scope, "omar");

        Assert.False(conflict.Success);
        Assert.Equal(IncidentRules.CodeIntentConflict, conflict.Reference);
        Assert.Equal(1, Count(database, "HSE_INCIDENT WHERE INCIDENT_ID = 'INC-intent-abc'"));

        // A partial write is completed by the retry rather than duplicated.
        Execute(database, "DELETE FROM HSE_INCIDENT_RESPONSE WHERE INCIDENT_ID = 'INC-intent-abc';");

        Assert.True((await incidents.ReportAsync(report, Scope, "omar")).Success);
        Assert.Equal(1, Count(database, "HSE_INCIDENT_RESPONSE WHERE INCIDENT_ID = 'INC-intent-abc'"));
        Assert.Equal(1, Count(database, "HSE_INCIDENT WHERE INCIDENT_ID = 'INC-intent-abc'"));

        // ---- AC-1 and AC-2: the refusals, each before anything is written ----
        var noJurisdiction = await incidents.ReportAsync(
            Complete() with { IntentId = "intent-nojur", Jurisdiction = "   " }, Scope, "sara");

        Assert.False(noJurisdiction.Success);
        Assert.Equal(IncidentRules.CodeJurisdictionRequired, noJurisdiction.Reference);
        Assert.Equal(0, Count(database, "HSE_INCIDENT WHERE INCIDENT_ID = 'INC-intent-nojur'"));

        var noOffset = await incidents.ReportAsync(
            Complete() with { IntentId = "intent-local", OccurredAtIso = "2026-11-08T10:00:00" }, Scope, "sara");

        Assert.False(noOffset.Success);
        Assert.Equal(IncidentRules.CodeTimestampNeedsOffset, noOffset.Reference);
        Assert.Equal(0, Count(database, "HSE_INCIDENT WHERE INCIDENT_ID = 'INC-intent-local'"));

        var movedField = await incidents.ReportAsync(
            Complete() with { IntentId = "intent-f2", FieldId = "F-2" }, Scope, "sara");

        Assert.False(movedField.Success);
        Assert.Equal(IncidentRules.CodeFieldChanged, movedField.Reference);
        Assert.Equal(0, Count(database, "HSE_INCIDENT WHERE INCIDENT_ID = 'INC-intent-f2'"));

        // ---- AC-5: a state change is refused and nothing is written ----
        var responsesBefore = Count(database, "HSE_INCIDENT_RESPONSE");

        var transition = await incidents.TransitionAsync("INC-1", "investigate", "started", "sara");

        Assert.False(transition.Success);
        Assert.Equal(IncidentRules.CodeWorkflowNotWired, transition.Reference);
        Assert.Equal(responsesBefore, Count(database, "HSE_INCIDENT_RESPONSE"));

        // ---- AC-4: a failed read is not an empty register ----
        Execute(database, "DROP TABLE HSE_INCIDENT_DETAIL;");

        var broken = await incidents.LoadAsync(incidents.BeginLoad(), Scope, stable, OgIncidentFilter.None);

        Assert.False(broken.Success);
        Assert.Null(broken.Value);
        Assert.False(string.IsNullOrWhiteSpace(broken.Message));
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) =>
        Execute(database,
            """
            CREATE TABLE HSE_INCIDENT (
                INCIDENT_ID TEXT,
                ACTIVE_IND TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                INCIDENT_CLASS_ID TEXT,
                INCIDENT_DATE TEXT,
                INCIDENT_DURATION REAL,
                INCIDENT_DURATION_OUOM TEXT,
                INCIDENT_DURATION_UOM TEXT,
                LOST_TIME_IND TEXT,
                PPDM_GUID TEXT,
                RECORDED_TIME TEXT,
                RECORDED_TIMEZONE TEXT,
                REMARK TEXT,
                REPORTED_BY_BA_ID TEXT,
                REPORTED_BY_NAME TEXT,
                REPORTED_IND TEXT,
                SOURCE TEXT,
                WORK_RELATED_IND TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT);

            CREATE TABLE HSE_INCIDENT_DETAIL (
                INCIDENT_ID TEXT,
                DETAIL_OBS_NO REAL,
                ACTIVE_IND TEXT,
                DETAIL_TYPE TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                INCIDENT_DATE TEXT,
                INCIDENT_DURATION REAL,
                INCIDENT_DURATION_OUOM TEXT,
                INCIDENT_DURATION_UOM TEXT,
                INCIDENT_SET_ID TEXT,
                INCIDENT_SEVERITY_ID TEXT,
                INCIDENT_TYPE_ID TEXT,
                PPDM_GUID TEXT,
                REMARK TEXT,
                SOURCE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT);

            CREATE TABLE HSE_INCIDENT_COMPONENT (
                INCIDENT_ID TEXT,
                COMPONENT_OBS_NO REAL,
                ACTIVE_IND TEXT,
                ACTIVITY_OBS_NO REAL,
                ADDRESS_OBS_NO REAL,
                ADDRESS_SOURCE TEXT,
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
                CLASSIFICATION_SYSTEM_ID TEXT,
                CLASS_LEVEL_ID TEXT,
                COMPONENT_ROLE TEXT,
                COMPONENT_TYPE TEXT,
                CONSENT_ID TEXT,
                CONSULT_ID TEXT,
                CONTEST_ID TEXT,
                CONTRACT_ID TEXT,
                CREW_COMPANY_BA_ID TEXT,
                CREW_ID TEXT,
                CREW_MEMBER_BA_ID TEXT,
                CREW_MEMBER_OBS_NO REAL,
                ECOZONE_ID TEXT,
                EFFECTIVE_DATE TEXT,
                EMPLOYEE_BA_ID TEXT,
                EMPLOYEE_OBS_NO REAL,
                EMPLOYER_BA_ID TEXT,
                ENTITLEMENT_ID TEXT,
                EQUIPMENT_ID TEXT,
                EXPIRY_DATE TEXT,
                FACILITY_ID TEXT,
                FACILITY_TYPE TEXT,
                FIELD_ID TEXT,
                FIELD_STATION_ID TEXT,
                FINANCE_ID TEXT,
                FOSSIL_ID TEXT,
                INCIDENT_SET_ID TEXT,
                INCIDENT_TYPE TEXT,
                INFORMATION_ITEM_ID TEXT,
                INFO_ITEM_SUBTYPE TEXT,
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
                PALEO_SUMMARY_ID TEXT,
                PDEN_ACTIVITY_TYPE TEXT,
                PDEN_AMENDMENT_SEQ_NO REAL,
                PDEN_ID TEXT,
                PDEN_PERIOD_ID TEXT,
                PDEN_PERIOD_TYPE TEXT,
                PDEN_SOURCE TEXT,
                PDEN_SUBTYPE TEXT,
                PDEN_VOLUME_DATE TEXT,
                PDEN_VOLUME_DATE_DESC TEXT,
                PDEN_VOLUME_METHOD TEXT,
                PHYSICAL_ITEM_ID TEXT,
                POOL_ID TEXT,
                PPDM_GUID TEXT,
                PPDM_SYSTEM_ID TEXT,
                PPDM_TABLE_NAME TEXT,
                PRODUCT_TYPE TEXT,
                PROD_STRING_ID TEXT,
                PROD_STRING_SOURCE TEXT,
                PROJECT_ID TEXT,
                PROJECT_PLAN_ID TEXT,
                PR_STR_FORM_OBS_NO REAL,
                RATE_SCHEDULE_ID TEXT,
                REFERENCED_GUID TEXT,
                REFERENCED_SYSTEM_ID TEXT,
                REFERENCED_TABLE_NAME TEXT,
                REMARK TEXT,
                RESENT_ID TEXT,
                RESERVE_CLASS_ID TEXT,
                RESTRICTION_ID TEXT,
                RESTRICTION_VERSION REAL,
                SAMPLE_ANAL_SOURCE TEXT,
                SEIS_SET_ID TEXT,
                SEIS_SET_SUBTYPE TEXT,
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
                TOTAL_INCIDENT_COUNT REAL,
                TOTAL_INCIDENT_PERIOD REAL,
                TOTAL_INCIDENT_PERIOD_UOM TEXT,
                UWI TEXT,
                WELL_ACTIVITY_SET_ID TEXT,
                WELL_ACTIVITY_SOURCE TEXT,
                WELL_ACTIVITY_TYPE_ID TEXT,
                WELL_DRILL_PERIOD_OBS_NO REAL,
                WELL_SET_ID TEXT,
                WORK_ORDER_ID TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT);

            CREATE TABLE HSE_INCIDENT_RESPONSE (
                INCIDENT_ID TEXT,
                RESPONSE_OBS_NO REAL,
                ACTIVE_IND TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                PPDM_GUID TEXT,
                PROJECT_ID TEXT,
                REMARK TEXT,
                RESPONSE_RESULT TEXT,
                RESPONSE_TYPE TEXT,
                SOURCE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT);

            CREATE TABLE PROJECT (
                PROJECT_ID TEXT PRIMARY KEY,
                ACTIVE_IND TEXT,
                COMPLETE_DATE TEXT,
                CONFIDENTIAL_IND TEXT,
                CONFIDENTIAL_RELEASE_DATE TEXT,
                DESCRIPTION TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                FIELD_STATION_IND TEXT,
                LAND_RIGHT_IND TEXT,
                PDEN_IND TEXT,
                PPDM_GUID TEXT,
                PROJECT_NAME TEXT,
                PROJECT_NUM TEXT,
                PROJECT_TYPE TEXT,
                REMARK TEXT,
                SEIS_SET_IND TEXT,
                SOURCE TEXT,
                START_DATE TEXT,
                STRAT_COLUMN_IND TEXT,
                STRAT_INTERPRETATION_IND TEXT,
                WELL_IND TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT);

            CREATE TABLE PROJECT_STEP (
                PROJECT_ID TEXT,
                STEP_ID TEXT,
                ACTIVE_IND TEXT,
                ACTUAL_END_DATE TEXT,
                ACTUAL_START_DATE TEXT,
                ACTUAL_TIME_ELAPSED REAL,
                ACTUAL_TIME_ELAPSED_OUOM TEXT,
                ACTUAL_TIME_ELAPSED_UOM TEXT,
                CRITICAL_DATE TEXT,
                DESCRIPTION TEXT,
                DUE_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                ESTIMATED_TIME_ELAPSED REAL,
                ESTIMATED_TIME_ELAPSED_OUOM TEXT,
                ESTIMATED_TIME_ELAPSED_UOM TEXT,
                EXPIRY_DATE TEXT,
                PLAN_END_DATE TEXT,
                PLAN_START_DATE TEXT,
                PLAN_STEP_ID TEXT,
                PPDM_GUID TEXT,
                PROJECT_PLAN_ID TEXT,
                REMARK TEXT,
                SOURCE TEXT,
                STEP_NAME TEXT,
                STEP_SEQ_NO REAL,
                STEP_TYPE TEXT,
                WHERE_COMPLETED TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT);

            CREATE TABLE PROJECT_STATUS (
                PROJECT_ID TEXT,
                STATUS_ID TEXT,
                ACTIVE_IND TEXT,
                DEFINED_BY_BA_ID TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                PPDM_GUID TEXT,
                REMARK TEXT,
                SOURCE TEXT,
                STATUS TEXT,
                STATUS_TYPE TEXT,
                STEP_ID TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT);

            INSERT INTO HSE_INCIDENT (INCIDENT_ID, INCIDENT_DATE, REMARK, ACTIVE_IND, SOURCE) VALUES
                ('INC-1', '2026-10-01 08:00:00', 'Spill at separator', 'Y', 'seed'),
                ('INC-2', '2026-09-01 08:00:00', 'Closed near miss', 'Y', 'seed');

            INSERT INTO HSE_INCIDENT_DETAIL (INCIDENT_ID, DETAIL_OBS_NO, DETAIL_TYPE, INCIDENT_TYPE_ID,
                INCIDENT_SEVERITY_ID, REMARK, INCIDENT_DATE, ACTIVE_IND) VALUES
                ('INC-1', 1, 'REPORT', 'SPILL', 'TIER_2', 'Spill at separator', '2026-10-01', 'Y'),
                ('INC-2', 1, 'REPORT', 'NEAR_MISS', 'TIER_3', 'Closed near miss', '2026-09-01', 'Y');

            INSERT INTO HSE_INCIDENT_COMPONENT (INCIDENT_ID, COMPONENT_OBS_NO, FIELD_ID, FACILITY_ID,
                COMPONENT_ROLE, COMPONENT_TYPE, JURISDICTION, REMARK, PROJECT_ID, BUSINESS_ASSOCIATE_ID,
                ACTIVE_IND) VALUES
                ('INC-1', 1, 'F-1', 'FAC-1', 'PRIMARY_FIELD_CONTEXT', 'INCIDENT_LOCATION', 'TX', 'Rig 7',
                 'P-CA1', NULL, 'Y'),
                ('INC-1', 2, NULL, NULL, 'INVESTIGATOR', NULL, NULL, NULL, NULL, 'sara', 'Y'),
                ('INC-2', 1, 'F-1', 'FAC-2', 'PRIMARY_FIELD_CONTEXT', 'INCIDENT_LOCATION', 'ND', 'Yard',
                 NULL, NULL, 'Y');

            INSERT INTO HSE_INCIDENT_RESPONSE (INCIDENT_ID, RESPONSE_OBS_NO, RESPONSE_TYPE, RESPONSE_RESULT,
                EFFECTIVE_DATE, ACTIVE_IND) VALUES
                ('INC-1', 1, 'STATUS', 'REPORTED', '2026-10-01', 'Y'),
                ('INC-2', 1, 'STATUS', 'REPORTED', '2026-09-01', 'Y'),
                ('INC-2', 2, 'STATUS', 'CLOSED', '2026-09-20', 'Y');

            INSERT INTO PROJECT (PROJECT_ID, PROJECT_TYPE, PROJECT_NAME, ACTIVE_IND) VALUES
                ('P-CA1', 'CORRECTIVE_ACTION', 'Incident INC-1 actions', 'Y');

            INSERT INTO PROJECT_STEP (PROJECT_ID, STEP_ID, STEP_NAME, STEP_SEQ_NO, DUE_DATE, ACTIVE_IND) VALUES
                ('P-CA1', 'S-1', 'Fix the seal', 1, '2026-10-15', 'Y');

            INSERT INTO PROJECT_STATUS (PROJECT_ID, STATUS_ID, STEP_ID, STATUS_TYPE, STATUS, EFFECTIVE_DATE,
                ACTIVE_IND) VALUES
                ('P-CA1', 'CS-1', 'S-1', 'CA_STEP', 'OPEN', '2026-10-01', 'Y');
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

    private static OgIncidentReport Complete() =>
        new(
            "intent-abc",
            HseReferenceCodes.IncidentType.Spill,
            HseReferenceCodes.Severity.Tier1,
            "ND",
            "2026-11-08T10:00:00Z",
            "F-1",
            "FAC-9",
            "A small spill was contained at the separator.",
            "Rig 7",
            "Area isolated and the crew mustered.",
            "sara, omar");

    private sealed class Recording : IFailureReporter
    {
        public List<string> Failures { get; } = [];

        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error)
        {
            Failures.Add($"{operation} | {consequence} | {exception.Message}");
            return FailureReferenceFormat.New();
        }
    }
}

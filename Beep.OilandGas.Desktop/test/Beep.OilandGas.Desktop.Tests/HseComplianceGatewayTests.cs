using Beep.OilandGas.Models.Data.Compliance;
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
/// HSE-05 against a real SQLite database: the register derives each state from the recorded dates and flags (AC-1), a missing
/// due date is unscheduled and never overdue (AC-2), the jurisdiction is explicit (AC-3), an obligation without components
/// cannot be fulfilled (AC-4), the report process says no regulator delivery is established (AC-5), and the calendar and the
/// list count the same rows (AC-6). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class HseComplianceGatewayTests
{
    private const string ConnectionName = "hse05-compliance";

    [IntegrationFact]
    public async Task Obligations_show_their_recorded_dates_and_discharge_with_their_components()
    {        var root = Path.Combine(OgPaths.DataRoot, "hse05-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasHse05"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "compliance.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "compliance.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var compliance = new OgCompliance(() => source, reporter);
        var today = DateTime.Today;
        var month = new DateTime(today.Year, today.Month, 1);

        // ---- the register ----
        var loaded = await compliance.LoadAsync("F-1", month);

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));

        var workspace = loaded.Value!;

        Assert.Equal(5, workspace.Total);
        Assert.Equal(1, workspace.OverdueCount);
        Assert.Equal(1, workspace.PendingCount);
        Assert.Equal(1, workspace.UnscheduledCount);
        Assert.Equal(1, workspace.FulfilledCount);
        Assert.Equal(1, workspace.WaivedCount);

        var overdue = workspace.Obligations.Single(row => row.Id == "OBL-1");

        Assert.Equal(ObligationStatus.Overdue, overdue.State);
        Assert.True(overdue.IsOverdue);
        Assert.Equal(today.AddDays(-1), overdue.DueDate);
        Assert.Equal("TX", overdue.JurisdictionText);
        Assert.Equal(2, overdue.Components.Count);
        Assert.All(overdue.Components, component => Assert.False(component.Fulfilled));

        // AC-2: no recorded due date is unscheduled, not overdue.
        var undated = workspace.Obligations.Single(row => row.Id == "OBL-5");

        Assert.Equal(ComplianceRules.Unscheduled, undated.State);
        Assert.True(undated.IsUnscheduled);
        Assert.False(undated.IsOverdue);
        Assert.Null(undated.DueDate);
        Assert.Equal("unscheduled", undated.DueText);

        // AC-3: an absent jurisdiction reads "not specified" - it is never a country.
        Assert.Equal("not specified", undated.JurisdictionText);

        Assert.Equal(ObligationStatus.Waived, workspace.Obligations.Single(row => row.Id == "OBL-4").State);
        Assert.Equal(ObligationStatus.Submitted, workspace.Obligations.Single(row => row.Id == "OBL-3").State);

        // ---- AC-6: the calendar and the list count the same rows ----
        var dueDates = new[] { today.AddDays(-1), today.AddDays(10) }
            .Where(date => date.Year == month.Year && date.Month == month.Month)
            .ToList();

        Assert.Equal(dueDates.Count, workspace.CalendarDue);
        Assert.Equal(dueDates.Count(date => date.Date < today.Date), workspace.CalendarOverdue);
        Assert.Equal(dueDates.Count, workspace.Calendar.Sum(day => day.Due));
        Assert.Equal(dueDates.Count(date => date.Date < today.Date),
            workspace.Calendar.Sum(day => day.Overdue));

        // ---- the field filter ----
        var otherField = await compliance.LoadAsync("F-2", month);

        Assert.True(otherField.Success, otherField.Message);
        Assert.Equal("OBL-6", Assert.Single(otherField.Value!.Obligations).Id);

        // ---- create, with its explicit jurisdiction and components ----
        var created = await compliance.CreateAsync(
            new OgObligationInputs(
                ObligationType.BseeSemsAudit, "WY", "F-1", today.AddDays(20), "Quarterly audit",
                ["Signed form", "Fee paid"]),
            "sara");

        Assert.True(created.Success, created.Message + " :: " + string.Join(" | ", reporter.Failures));

        var newId = created.Value!.Obligations
            .Single(row => row.Description == "Quarterly audit").Id;

        Assert.StartsWith("OBL-F-1-", newId, StringComparison.Ordinal);
        Assert.Equal("WY", created.Value.Obligations.Single(row => row.Id == newId).JurisdictionText);
        Assert.Equal(ObligationStatus.Pending, created.Value.Obligations.Single(row => row.Id == newId).State);
        Assert.Equal(1, Count(database, $"OBLIGATION WHERE OBLIGATION_ID = '{newId}' AND JURISDICTION = 'WY' AND FIELD_ID = 'F-1'"));
        Assert.Equal(2, Count(database, $"OBLIGATION_COMPONENT WHERE OBLIGATION_ID = '{newId}' AND REQUIRED_IND = 'Y'"));

        // AC-3: a blank jurisdiction is refused and no row is written.
        var noJurisdiction = await compliance.CreateAsync(
            new OgObligationInputs(ObligationType.NrcReport, "  ", "F-1", today.AddDays(5), "No jurisdiction",
                ["Signed form"]),
            "sara");

        Assert.False(noJurisdiction.Success);
        Assert.Equal(ComplianceRules.CodeJurisdictionRequired, noJurisdiction.Reference);
        Assert.Equal(0, Count(database, "OBLIGATION WHERE DESCRIPTION = 'No jurisdiction'"));

        var noComponents = await compliance.CreateAsync(
            new OgObligationInputs(ObligationType.NrcReport, "TX", "F-1", today.AddDays(5), "No components", []),
            "sara");

        Assert.False(noComponents.Success);
        Assert.Equal(ComplianceRules.CodeNoComponents, noComponents.Reference);
        Assert.Equal(0, Count(database, "OBLIGATION WHERE DESCRIPTION = 'No components'"));

        // ---- AC-4: fulfilment needs components, and every required one ----
        var noComponentsObligation = await compliance.FulfilAsync("OBL-5", 1m, today, "sara");

        Assert.False(noComponentsObligation.Success);
        Assert.Equal(ComplianceRules.CodeNoComponents, noComponentsObligation.Reference);

        var outstanding = await compliance.FulfilAsync("OBL-1", 1m, today, "sara");

        Assert.False(outstanding.Success);
        Assert.Equal(ComplianceRules.CodeComponentsOutstanding, outstanding.Reference);
        Assert.Contains("Fee paid", outstanding.Message!, StringComparison.Ordinal);
        Assert.Equal(0, Count(database, "OBLIGATION WHERE OBLIGATION_ID = 'OBL-1' AND FULFILLED_IND = 'Y'"));

        // Each component is discharged with its evidence, then the obligation.
        Assert.True((await compliance.FulfilComponentAsync("OBL-1", 1m, 1m, today, "form reference 42", "sara")).Success);

        var stillOutstanding = await compliance.FulfilAsync("OBL-1", 1m, today, "sara");

        Assert.False(stillOutstanding.Success);
        Assert.Equal(ComplianceRules.CodeComponentsOutstanding, stillOutstanding.Reference);

        Assert.True((await compliance.FulfilComponentAsync("OBL-1", 1m, 2m, today, "receipt 7", "sara")).Success);

        var fulfilled = await compliance.FulfilAsync("OBL-1", 1m, today, "sara");

        Assert.True(fulfilled.Success, fulfilled.Message);
        Assert.Equal(ObligationStatus.Submitted,
            fulfilled.Value!.Obligations.Single(row => row.Id == "OBL-1").State);
        Assert.Equal(1, Count(database,
            $"OBLIGATION WHERE OBLIGATION_ID = 'OBL-1' AND FULFILLED_IND = 'Y' AND FULFILLED_DATE LIKE '{today:yyyy-MM-dd}%'"));
        Assert.Equal(2, Count(database, "OBLIGATION_COMPONENT WHERE OBLIGATION_ID = 'OBL-1' AND FULFILLED_IND = 'Y'"));

        // ---- waiving keeps the reason as its own record ----
        var noReason = await compliance.WaiveAsync("OBL-2", 1m, "  ", "sara");

        Assert.False(noReason.Success);
        Assert.Equal(ComplianceRules.CodeReasonRequired, noReason.Reference);

        var waived = await compliance.WaiveAsync("OBL-2", 1m, "Regulator confirmed it is not required this year", "sara");

        Assert.True(waived.Success, waived.Message);
        Assert.Equal(ObligationStatus.Waived, waived.Value!.Obligations.Single(row => row.Id == "OBL-2").State);
        Assert.Equal(1, Count(database,
            "OBLIGATION WHERE OBLIGATION_ID = 'OBL-2' AND REMARK LIKE 'WAIVED: Regulator confirmed%'"));

        // ---- AC-5: the report process states the truth ----
        var report = await compliance.RequestReportAsync("OBL-1", 1m, "sara");

        Assert.False(report.Success);
        Assert.Equal(ComplianceRules.CodeReportNotWired, report.Reference);
        Assert.Contains("No regulator delivery is established", report.Message!, StringComparison.Ordinal);

        // ---- a failed read is not an empty register ----
        Execute(database, "DROP TABLE OBLIGATION_COMPONENT;");

        var broken = await compliance.LoadAsync("F-1", month);

        Assert.False(broken.Success);
        Assert.Null(broken.Value);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database)
    {
        var today = DateTime.Today;
        var overdue = today.AddDays(-1).ToString("yyyy-MM-dd");
        var future = today.AddDays(10).ToString("yyyy-MM-dd");

        Execute(database, """
            CREATE TABLE OBLIGATION (
                FIELD_ID TEXT,
                JURISDICTION TEXT,
                OBLIGATION_ID TEXT,
                OBLIGATION_SEQ_NO REAL,
                ACTIVE_IND TEXT,
                CALCULATION_METHOD TEXT,
                CONVERTIBLE_IND TEXT,
                CRITICAL_DATE TEXT,
                CURRENCY_CONVERSION REAL,
                CURRENCY_OUOM TEXT,
                DESCRIPTION TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                FULFILLED_DATE TEXT,
                FULFILLED_IND TEXT,
                GROSS_OBLIGATION_COST REAL,
                INSTRUMENT_ID TEXT,
                LIABILITY_RELEASE_DATE TEXT,
                NET_OBLIGATION_COST REAL,
                NOTICE_PERIOD_LENGTH REAL,
                NOTICE_PERIOD_OUOM TEXT,
                OBLIGATION_CATEGORY TEXT,
                OBLIGATION_DURATION REAL,
                OBLIGATION_DURATION_OUOM TEXT,
                OBLIGATION_FREQUENCY TEXT,
                OBLIGATION_TYPE TEXT,
                PAYMENT_IND TEXT,
                PAYMENT_RESPONSIBILITY TEXT,
                PERCENTAGE REAL,
                POTENTIAL_OBLIGATION_DESC TEXT,
                POTENTIAL_OBLIGATION_IND TEXT,
                PPDM_GUID TEXT,
                PREPAID_IND TEXT,
                REMARK TEXT,
                RESP_PARTY_BA_ID TEXT,
                REVIEW_FREQUENCY TEXT,
                ROYALTY_OWNER_BA_ID TEXT,
                ROYALTY_PAYOR_BA_ID TEXT,
                ROYALTY_TYPE TEXT,
                SOURCE TEXT,
                START_DATE TEXT,
                SUBSTANCE_ID TEXT,
                TRIGGER_METHOD TEXT,
                WORK_OBLIGATION_DESC TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PRIMARY KEY (OBLIGATION_ID, OBLIGATION_SEQ_NO));

            CREATE TABLE OBLIGATION_COMPONENT (
                REQUIRED_IND TEXT,
                OBLIGATION_ID TEXT,
                OBLIGATION_SEQ_NO REAL,
                COMPONENT_OBS_NO REAL,
                ACTIVE_IND TEXT,
                ACTIVITY_OBS_NO REAL,
                ANALYSIS_ID TEXT,
                APPLICATION_ID TEXT,
                AREA_ID TEXT,
                AREA_TYPE TEXT,
                AUTHORITY_ID TEXT,
                BA_LICENSEE_BA_ID TEXT,
                BA_LICENSE_ID TEXT,
                BA_ORGANIZATION_ID TEXT,
                BA_ORGANIZATION_SEQ_NO REAL,
                BA_SERVICE_BA_ID TEXT,
                BA_SERVICE_PROVIDER TEXT,
                BA_SERVICE_SEQ_NO REAL,
                BUSINESS_ASSOCIATE_ID TEXT,
                CALCULATION_METHOD TEXT,
                CATALOGUE_ADDITIVE_ID TEXT,
                CATALOGUE_EQUIP_ID TEXT,
                CLASSIFICATION_SYSTEM_ID TEXT,
                CLASS_LEVEL_ID TEXT,
                COMPONENT_REASON TEXT,
                CONSENT_ID TEXT,
                CONSULT_ID TEXT,
                CONTEST_ID TEXT,
                CONTRACT_EXTENSION_ID TEXT,
                CONTRACT_ID TEXT,
                CONTRACT_PROVISION_ID TEXT,
                CONT_SERVICE_BA_ID TEXT,
                CONT_SERVICE_SEQ_NO REAL,
                CRITICAL_DATE TEXT,
                DESCRIPTION TEXT,
                ECOZONE_ID TEXT,
                EFFECTIVE_DATE TEXT,
                EMPLOYEE_BA_ID TEXT,
                EMPLOYEE_OBS_NO REAL,
                EMPLOYER_BA_ID TEXT,
                ENTITLEMENT_ID TEXT,
                EQUIPMENT_ID TEXT,
                EXPIRY_DATE TEXT,
                FACILITY_ID TEXT,
                FACILITY_LICENSE_ID TEXT,
                FACILITY_SERVICE_BA_ID TEXT,
                FACILITY_SERVICE_SEQ_NO REAL,
                FACILITY_TYPE TEXT,
                FIELD_ID TEXT,
                FIELD_STATION_ID TEXT,
                FINANCE_ID TEXT,
                FOSSIL_ID TEXT,
                FULFILLED_DATE TEXT,
                FULFILLED_IND TEXT,
                FULFILLED_REMARK TEXT,
                HSE_INCIDENT_ID TEXT,
                INCIDENT_SET_ID TEXT,
                INCIDENT_TYPE TEXT,
                INFORMATION_ITEM_ID TEXT,
                INFO_ITEM_SUBTYPE TEXT,
                INSTRUMENT_ID TEXT,
                INTEREST_SET_ID TEXT,
                INTEREST_SET_SEQ_NO REAL,
                LAND_COST_ID TEXT,
                LAND_OFFERING_BID_ID TEXT,
                LAND_REQUEST_ID TEXT,
                LAND_RIGHT_ID TEXT,
                LAND_RIGHT_SUBTYPE TEXT,
                LAND_SALE_BID_SET_ID TEXT,
                LAND_SALE_JURISDICTION TEXT,
                LAND_SALE_NUMBER TEXT,
                LAND_SALE_OFFERING_ID TEXT,
                LAND_SERVICE_PROVIDED_BY TEXT,
                LAND_SERVICE_SEQ_NO REAL,
                LANGUAGE TEXT,
                LITHOLOGY_LOG_ID TEXT,
                LITH_LOG_SOURCE TEXT,
                LR_TERMINATION_ID TEXT,
                LR_TERMINATION_SEQ_NO REAL,
                NOTIFICATION_ID TEXT,
                OBLIGATION_COMPONENT_TYPE TEXT,
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
                SEIS_LICENSE_ID TEXT,
                SEIS_POINT_FLOW_ID TEXT,
                SEIS_POINT_ID TEXT,
                SEIS_SERVICE_BA_ID TEXT,
                SEIS_SERVICE_SEQ_NO REAL,
                SEIS_SET_ID TEXT,
                SEIS_SET_SUBTYPE TEXT,
                SEIS_TRANSACTION_ID TEXT,
                SEIS_TRANSACTION_TYPE TEXT,
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
                UWI TEXT,
                WELL_ACTIVITY_SET_ID TEXT,
                WELL_ACTIVITY_SOURCE TEXT,
                WELL_ACTIVITY_TYPE_ID TEXT,
                WELL_LICENSE_ID TEXT,
                WELL_LICENSE_SOURCE TEXT,
                WELL_SERVICE_BA_ID TEXT,
                WELL_SERVICE_SEQ_NO REAL,
                WELL_SET_ID TEXT,
                WORK_ORDER_DELIVERY_ID TEXT,
                WORK_ORDER_ID TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PRIMARY KEY (OBLIGATION_ID, OBLIGATION_SEQ_NO, COMPONENT_OBS_NO));
""");

        Execute(database, $"""
            INSERT INTO OBLIGATION (OBLIGATION_ID, OBLIGATION_SEQ_NO, OBLIGATION_TYPE, FIELD_ID, JURISDICTION,
                DESCRIPTION, CRITICAL_DATE, FULFILLED_IND, REMARK, ACTIVE_IND, SOURCE) VALUES
                ('OBL-1', 1, 'ONRR_PRODUCTION_REPORT', 'F-1', 'TX', 'Monthly production report', '{overdue}', 'N', NULL, 'Y', 'seed'),
                ('OBL-2', 1, 'BSEE_SEMS_AUDIT', 'F-1', 'ND', 'Annual SEMS audit', '{future}', 'N', NULL, 'Y', 'seed'),
                ('OBL-3', 1, 'EPA_GHG_REPORT_FLARE', 'F-1', 'TX', 'GHG report', '{overdue}', 'Y', NULL, 'Y', 'seed'),
                ('OBL-4', 1, 'AER_ST39_REPORT', 'F-1', 'CA', 'Waived report', '{overdue}', 'N', 'WAIVED: regulator closed the requirement', 'Y', 'seed'),
                ('OBL-5', 1, 'NRC_REPORT', 'F-1', NULL, 'Unscheduled report', NULL, 'N', NULL, 'Y', 'seed'),
                ('OBL-6', 1, 'OSPAR_REPORT', 'F-2', 'NO', 'Other field report', '{future}', 'N', NULL, 'Y', 'seed');

            INSERT INTO OBLIGATION_COMPONENT (OBLIGATION_ID, OBLIGATION_SEQ_NO, COMPONENT_OBS_NO, DESCRIPTION,
                REQUIRED_IND, FULFILLED_IND, FULFILLED_DATE, ACTIVE_IND, SOURCE) VALUES
                ('OBL-1', 1, 1, 'Signed form', 'Y', 'N', NULL, 'Y', 'seed'),
                ('OBL-1', 1, 2, 'Fee paid', 'Y', 'N', NULL, 'Y', 'seed');
            """);
    }

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
            Failures.Add($"{operation} | {consequence} | {exception.Message}");
            return FailureReferenceFormat.New();
        }
    }
}

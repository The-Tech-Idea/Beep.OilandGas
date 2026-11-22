using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Lease;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// LSE-01 against a real SQLite database: the four lease tables merge by id, a lease that exists only in a typed
/// table is on the register, the state is derived from the dates, the field scope follows the acquisition row, and
/// linking a field records the actor on that row. Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class LeaseRegisterGatewayTests
{
    private const string ConnectionName = "lse01-register";

    [IntegrationFact]
    public async Task The_register_merges_four_tables_derives_state_and_records_a_field_link()
    {        var root = Path.Combine(OgPaths.DataRoot, "lse01-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasLse01"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "leases.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "leases.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var register = new OgLeaseRegisterService(() => source, reporter);
        var asOf = new DateTime(2026, 6, 1);

        // AC-1: all four tables are on the register, including the typed-only leases.
        var loaded = await register.LoadAsync(null, null, null, asOf);

        Assert.True(loaded.Success, loaded.Message);
        var all = loaded.Value!;

        Assert.Equal(4, all.Count);
        Assert.Contains(all.Leases, lease => lease.Id == "LA-1" && lease.Kind == "LEASE");
        Assert.Contains(all.Leases, lease => lease.Id == "FM-1" && lease.Kind == OgLeaseSummary.KindFeeMineral);
        Assert.Contains(all.Leases, lease => lease.Id == "GV-1" && lease.Kind == OgLeaseSummary.KindGovernment);
        Assert.Contains(all.Leases, lease => lease.Id == "NP-1" && lease.Kind == OgLeaseSummary.KindNetProfit);
        Assert.Equal(["FEE_MINERAL", "GOVERNMENT", "LEASE", "NET_PROFIT"], all.Kinds);

        // AC-3: the state comes from the dates, not from the stored status.
        Assert.Equal(LeaseStatus.Expiring, all.Leases.Single(lease => lease.Id == "LA-1").Status);
        Assert.Equal(LeaseStatus.Expired, all.Leases.Single(lease => lease.Id == "FM-1").Status);
        Assert.Equal(LeaseStatus.Active, all.Leases.Single(lease => lease.Id == "GV-1").Status);

        // AC-2: the acreage unit travels with the number.
        var acquisition = all.Leases.Single(lease => lease.Id == "LA-1");
        Assert.True(acquisition.HasAcreageUnit);
        Assert.Equal("acres", acquisition.AcreageUnit);
        Assert.Equal("100 acres", acquisition.AcreageText);

        // AC-4: a field scope follows the acquisition row; typed-only leases carry no field and are not shown.
        var scoped = await register.LoadAsync("F-1", "Alpha field", null, asOf);

        Assert.True(scoped.Success, scoped.Message);
        Assert.Equal("LA-1", Assert.Single(scoped.Value!.Leases).Id);

        // AC-4: linking records the field and the actor on the acquisition row.
        var linked = await register.LinkFieldAsync("LA-1", "F-2", "tester");

        Assert.True(linked.Success, linked.Message);

        using (var connection = new SqliteConnection($"Data Source={database}"))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText =
                "SELECT FIELD_ID, ROW_CHANGED_BY FROM LEASE_ACQUISITION WHERE LEASE_ACQUISITION_ID = 'LA-1';";
            using var reader = command.ExecuteReader();

            Assert.True(reader.Read());
            Assert.Equal("F-2", reader.GetString(0));
            Assert.Equal("tester", reader.GetString(1));
        }

        // A typed-only lease has nowhere to record the field, and the refusal says so.
        var typedOnly = await register.LinkFieldAsync("FM-1", "F-2", "tester");

        Assert.False(typedOnly.Success);
        Assert.Equal(OgLeaseRegisterService.CodeTypedOnly, typedOnly.Reference);
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE LEASE_ACQUISITION (
                LEASE_ACQUISITION_ID TEXT NOT NULL PRIMARY KEY,
                LEASE_NAME TEXT NULL,
                LEASE_TYPE TEXT NULL,
                GROSS_ACREAGE REAL NOT NULL,
                ACREAGE_OUOM TEXT NULL,
                STATE_ID TEXT NULL,
                COUNTY_ID TEXT NULL,
                OPERATOR TEXT NULL,
                ACQUISITION_DATE TEXT NULL,
                LEASE_EXPIRATION TEXT NULL,
                ROYALTY_RATE REAL NOT NULL,
                NET_REVENUE_INTEREST REAL NOT NULL,
                FIELD_ID TEXT NULL,
                ACQUISITION_STATUS TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE FEE_MINERAL_LEASE (
                LEASE_ID TEXT NOT NULL PRIMARY KEY,
                LEASE_NUMBER TEXT NULL,
                LEASE_NAME TEXT NULL,
                GROSS_ACREAGE REAL NOT NULL,
                ACREAGE_OUOM TEXT NULL,
                STATE_ID TEXT NULL,
                COUNTY_ID TEXT NULL,
                ACQUISITION_DATE TEXT NULL,
                EXPIRATION_DATE TEXT NULL,
                ROYALTY_RATE REAL NULL,
                NET_REVENUE_INTEREST REAL NULL,
                ACQUISITION_STATUS TEXT NULL);

            CREATE TABLE GOVERNMENT_LEASE (
                LEASE_ID TEXT NOT NULL PRIMARY KEY,
                LEASE_NUMBER TEXT NULL,
                LEASE_NAME TEXT NULL,
                GROSS_ACREAGE REAL NOT NULL,
                ACREAGE_OUOM TEXT NULL,
                STATE_ID TEXT NULL,
                COUNTY_ID TEXT NULL,
                EXPIRATION_DATE TEXT NULL,
                ROYALTY_RATE REAL NULL,
                NET_REVENUE_INTEREST REAL NULL);

            CREATE TABLE NET_PROFIT_LEASE (
                LEASE_ID TEXT NOT NULL PRIMARY KEY,
                LEASE_NUMBER TEXT NULL,
                LEASE_NAME TEXT NULL,
                EXPIRATION_DATE TEXT NULL,
                ROYALTY_RATE REAL NOT NULL,
                NET_REVENUE_INTEREST REAL NOT NULL,
                AREA_ID TEXT NULL,
                BUSINESS_ASSOCIATE_ID TEXT NULL);

            INSERT INTO LEASE_ACQUISITION
                (LEASE_ACQUISITION_ID, LEASE_NAME, LEASE_TYPE, GROSS_ACREAGE, ACREAGE_OUOM, STATE_ID,
                 OPERATOR, ACQUISITION_DATE, LEASE_EXPIRATION, ROYALTY_RATE, NET_REVENUE_INTEREST, FIELD_ID,
                 ACQUISITION_STATUS, ROW_CHANGED_BY, ROW_CHANGED_DATE)
            VALUES
                ('LA-1', 'Alpha lease', 'LEASE', 100, 'acres', 'NORTH', 'OP-A', '2025-01-01',
                 '2026-07-01', 0.125, 0.8, 'F-1', 'ACTIVE', 'seed', '2026-01-01');

            INSERT INTO FEE_MINERAL_LEASE
                (LEASE_ID, LEASE_NUMBER, LEASE_NAME, GROSS_ACREAGE, ACREAGE_OUOM, STATE_ID, EXPIRATION_DATE,
                 ROYALTY_RATE, NET_REVENUE_INTEREST, ACQUISITION_STATUS)
            VALUES ('FM-1', 'LN-FM-1', 'Fee mineral', 50, 'acres', 'NORTH', '2026-01-01', 0.2, 0.75, 'ACTIVE');

            INSERT INTO GOVERNMENT_LEASE
                (LEASE_ID, LEASE_NUMBER, LEASE_NAME, GROSS_ACREAGE, ACREAGE_OUOM, STATE_ID, EXPIRATION_DATE,
                 ROYALTY_RATE, NET_REVENUE_INTEREST)
            VALUES ('GV-1', 'LN-GV-1', 'Government lease', 80, 'acres', 'SOUTH', '2027-01-01', 0.18, 0.82);

            INSERT INTO NET_PROFIT_LEASE
                (LEASE_ID, LEASE_NUMBER, LEASE_NAME, EXPIRATION_DATE, ROYALTY_RATE, NET_REVENUE_INTEREST,
                 AREA_ID, BUSINESS_ASSOCIATE_ID)
            VALUES ('NP-1', 'LN-NP-1', 'Net profit lease', '2027-06-01', 0.15, 0.7, 'AREA-9', 'OP-NP');
            """;
        command.ExecuteNonQuery();
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

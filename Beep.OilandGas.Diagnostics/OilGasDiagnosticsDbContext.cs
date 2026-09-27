using Microsoft.EntityFrameworkCore;
using TheTechIdeaWeb.Diagnostics.Records;

namespace Beep.OilandGas.Diagnostics;

/// <summary>
/// Where OilGas keeps its failures: the shared failure store's one table (<see cref="FailureRecordConfiguration"/>), read
/// by the administrator's list on the Web and written by both the Web and the API.
/// </summary>
/// <remarks>
/// <para>
/// A context of its own rather than the repository's, because the Web holds no connection to the repository — it asks the
/// API about accounts — and a failure store the Web could not write into would lose exactly the failures of the host a
/// person is looking at. It may still live in the repository's database: its migrations keep their own history table
/// (<see cref="MigrationsHistoryTable"/>), so the two contexts never read each other's.
/// </para>
/// <para>
/// One subclass per engine, as the repository has, because each engine has its own migrations.
/// </para>
/// </remarks>
public abstract class OilGasDiagnosticsDbContext(DbContextOptions options) : DbContext(options)
{
    /// <summary>The table this context's migrations are recorded in, apart from the repository's.</summary>
    public const string MigrationsHistoryTable = "__OilGasDiagnosticsMigrations";

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // The repository's limit, so an index name is shortened the same way on every engine.
        builder.HasAnnotation("Relational:MaxIdentifierLength", 30);
        builder.ApplyConfiguration(new FailureRecordConfiguration());
    }
}

/// <summary>The failure store on SQL Server.</summary>
public sealed class SqlServerOilGasDiagnosticsDbContext(DbContextOptions<SqlServerOilGasDiagnosticsDbContext> options)
    : OilGasDiagnosticsDbContext(options);

/// <summary>The failure store on PostgreSQL.</summary>
public sealed class PostgreSqlOilGasDiagnosticsDbContext(DbContextOptions<PostgreSqlOilGasDiagnosticsDbContext> options)
    : OilGasDiagnosticsDbContext(options);

/// <summary>The failure store on Oracle.</summary>
public sealed class OracleOilGasDiagnosticsDbContext(DbContextOptions<OracleOilGasDiagnosticsDbContext> options)
    : OilGasDiagnosticsDbContext(options);

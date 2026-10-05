using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Oracle.ManagedDataAccess.Client;

namespace Beep.OilandGas.ApiService.Data;

/// <summary>Whether a refused save was a unique index refusing a duplicate, on each provider the repository runs on.</summary>
/// <remarks>
/// <para>
/// A <see cref="DbUpdateException"/> is any refused save — a broken connection, a foreign key, a value too long — and the
/// identity endpoints answered every one of them "changed, reload" (OILGAS-CATCH-01). Only a unique-key refusal is a row
/// somebody else wrote first; anything else reaches the API's handler as the failure it is.
/// </para>
/// <para>
/// The codes are each provider's own, for the providers <c>RepositoryServiceRegistration</c> configures: SQL Server 2601
/// (a duplicate key in a unique index) and 2627 (a unique constraint); PostgreSQL's SQLSTATE 23505 (unique_violation);
/// Oracle's ORA-00001. The shape is the Events application's (<c>Beep.EventsRegistration.Data.UniqueKeyViolation</c>).
/// </para>
/// </remarks>
public static class UniqueKeyViolation
{
    public static bool Is(DbUpdateException refused) => refused.InnerException switch
    {
        SqlException sqlServer => sqlServer.Number is 2601 or 2627,
        PostgresException postgres => postgres.SqlState == PostgresErrorCodes.UniqueViolation,
        OracleException oracle => oracle.Number == 1,
        _ => false,
    };
}

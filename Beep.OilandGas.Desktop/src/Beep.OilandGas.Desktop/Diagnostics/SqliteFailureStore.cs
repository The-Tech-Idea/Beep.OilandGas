using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;

/// <summary>
/// The local, durable failure store: one SQLite file at <see cref="OgPaths.FailureStorePath"/>.
/// </summary>
/// <remarks>
/// <para>
/// Written directly rather than through EF migrations, because this store is append-only, disposable and
/// must exist before any business database is reachable. Schema changes are handled by adding a nullable
/// column with a default, or by recreating the file while keeping the retention window.
/// </para>
/// <para>
/// It deliberately lives outside the business database so a broken business connection cannot also
/// destroy the record of why it broke (ADR-013).
/// </para>
/// </remarks>
public sealed class SqliteFailureStore : IOgFailureStore
{
    private const string CreateSql =
        """
        CREATE TABLE IF NOT EXISTS FAILURE_RECORD (
            REFERENCE      TEXT PRIMARY KEY NOT NULL,
            OCCURRED_AT_UTC TEXT NOT NULL,
            SEVERITY       INTEGER NOT NULL,
            OPERATION      TEXT NOT NULL,
            CONSEQUENCE    TEXT NOT NULL,
            EXCEPTION_TYPE TEXT NOT NULL,
            MESSAGE        TEXT NOT NULL,
            STACK          TEXT NOT NULL,
            USER_ID        TEXT NULL,
            COMPANY_ID     TEXT NULL,
            FORWARDED      INTEGER NOT NULL DEFAULT 0
        );
        CREATE INDEX IF NOT EXISTS IX_FAILURE_RECORD_OCCURRED ON FAILURE_RECORD (OCCURRED_AT_UTC);
        """;

    private readonly string _path;
    private readonly object _gate = new();

    public SqliteFailureStore(string? path = null)
    {
        _path = path ?? OgPaths.FailureStorePath;

        if (!OgPaths.IsWritableLocation(_path))
        {
            throw new InvalidOperationException(
                $"The failure store must live under {OgPaths.DataRoot}, not '{_path}'.");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        EnsureSchema();
    }

    public string StorePath => _path;

    public void Write(OgFailureRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        lock (_gate)
        {
            using var connection = Open();
            using var command = connection.CreateCommand();
            command.CommandText =
                """
                INSERT INTO FAILURE_RECORD
                    (REFERENCE, OCCURRED_AT_UTC, SEVERITY, OPERATION, CONSEQUENCE,
                     EXCEPTION_TYPE, MESSAGE, STACK, USER_ID, COMPANY_ID, FORWARDED)
                VALUES
                    ($reference, $occurred, $severity, $operation, $consequence,
                     $exceptionType, $message, $stack, $userId, $companyId, $forwarded);
                """;
            command.Parameters.AddWithValue("$reference", record.Reference);
            command.Parameters.AddWithValue("$occurred", record.OccurredAtUtc.ToString("O"));
            command.Parameters.AddWithValue("$severity", (int)record.Severity);
            command.Parameters.AddWithValue("$operation", record.Operation);
            command.Parameters.AddWithValue("$consequence", record.Consequence);
            command.Parameters.AddWithValue("$exceptionType", record.ExceptionType);
            command.Parameters.AddWithValue("$message", record.Message);
            command.Parameters.AddWithValue("$stack", record.Stack);
            command.Parameters.AddWithValue("$userId", (object?)record.UserId ?? DBNull.Value);
            command.Parameters.AddWithValue("$companyId", (object?)record.CompanyId ?? DBNull.Value);
            command.Parameters.AddWithValue("$forwarded", record.Forwarded ? 1 : 0);
            command.ExecuteNonQuery();
        }
    }

    public OgFailureRecord? Find(string reference)
    {
        lock (_gate)
        {
            using var connection = Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM FAILURE_RECORD WHERE REFERENCE = $reference LIMIT 1;";
            command.Parameters.AddWithValue("$reference", reference);

            using var reader = command.ExecuteReader();
            return reader.Read() ? Read(reader) : null;
        }
    }

    public IReadOnlyList<OgFailureRecord> Recent(int take)
    {
        lock (_gate)
        {
            using var connection = Open();
            using var command = connection.CreateCommand();
            command.CommandText =
                "SELECT * FROM FAILURE_RECORD ORDER BY OCCURRED_AT_UTC DESC LIMIT $take;";
            command.Parameters.AddWithValue("$take", take);

            using var reader = command.ExecuteReader();
            var results = new List<OgFailureRecord>();
            while (reader.Read())
            {
                results.Add(Read(reader));
            }

            return results;
        }
    }

    public int Count()
    {
        lock (_gate)
        {
            using var connection = Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM FAILURE_RECORD;";
            return Convert.ToInt32(command.ExecuteScalar());
        }
    }

    public int Purge(DateTimeOffset olderThanUtc)
    {
        lock (_gate)
        {
            using var connection = Open();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM FAILURE_RECORD WHERE OCCURRED_AT_UTC < $cutoff;";
            command.Parameters.AddWithValue("$cutoff", olderThanUtc.ToString("O"));
            return command.ExecuteNonQuery();
        }
    }

    public int MarkForwarded(IEnumerable<string> references)
    {
        ArgumentNullException.ThrowIfNull(references);

        var marked = 0;

        lock (_gate)
        {
            using var connection = Open();

            foreach (var reference in references.Where(FailureReferenceFormat.IsWellFormed))
            {
                using var command = connection.CreateCommand();
                command.CommandText =
                    "UPDATE FAILURE_RECORD SET FORWARDED = 1 WHERE REFERENCE = $reference AND FORWARDED = 0;";
                command.Parameters.AddWithValue("$reference", reference);
                marked += command.ExecuteNonQuery();
            }
        }

        return marked;
    }

    private void EnsureSchema()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = CreateSql;
        command.ExecuteNonQuery();
    }

    private SqliteConnection Open()
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = _path,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared,
        };

        var connection = new SqliteConnection(builder.ToString());
        connection.Open();
        return connection;
    }

    private static OgFailureRecord Read(SqliteDataReader reader) =>
        new(
            reader.GetString(reader.GetOrdinal("REFERENCE")),
            DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal("OCCURRED_AT_UTC"))),
            (FailureSeverity)reader.GetInt32(reader.GetOrdinal("SEVERITY")),
            reader.GetString(reader.GetOrdinal("OPERATION")),
            reader.GetString(reader.GetOrdinal("CONSEQUENCE")),
            reader.GetString(reader.GetOrdinal("EXCEPTION_TYPE")),
            reader.GetString(reader.GetOrdinal("MESSAGE")),
            reader.GetString(reader.GetOrdinal("STACK")),
            reader.IsDBNull(reader.GetOrdinal("USER_ID")) ? null : reader.GetString(reader.GetOrdinal("USER_ID")),
            reader.IsDBNull(reader.GetOrdinal("COMPANY_ID")) ? null : reader.GetString(reader.GetOrdinal("COMPANY_ID")),
            reader.GetInt32(reader.GetOrdinal("FORWARDED")) == 1);
}

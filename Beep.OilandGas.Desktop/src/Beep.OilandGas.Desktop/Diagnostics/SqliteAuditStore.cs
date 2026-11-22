using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;

namespace TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;

/// <summary>One audit row: who did what, to what, when, with what outcome.</summary>
public sealed record OgAuditRecord(
    long Id,
    DateTimeOffset AtUtc,
    string Verb,
    string SubjectKind,
    string SubjectId,
    string Outcome,
    string? Detail,
    string? UserId,
    string? CompanyId,
    string? FieldId);

/// <summary>Durable business audit. Every write command and every refusal lands here.</summary>
public interface IOgAuditStore
{
    void Write(
        string verb,
        string subjectKind,
        string subjectId,
        string outcome,
        string? detail,
        string? userId,
        string? companyId,
        string? fieldId);

    IReadOnlyList<OgAuditRecord> Recent(int take);

    int Count();
}

/// <summary>The audit table, in the same local file as the failure store and the same rules.</summary>
public sealed class SqliteAuditStore : IOgAuditStore
{
    private const string CreateSql =
        """
        CREATE TABLE IF NOT EXISTS AUDIT_RECORD (
            ID           INTEGER PRIMARY KEY AUTOINCREMENT,
            AT_UTC       TEXT NOT NULL,
            VERB         TEXT NOT NULL,
            SUBJECT_KIND TEXT NOT NULL,
            SUBJECT_ID   TEXT NOT NULL,
            OUTCOME      TEXT NOT NULL,
            DETAIL       TEXT NULL,
            USER_ID      TEXT NULL,
            COMPANY_ID   TEXT NULL,
            FIELD_ID     TEXT NULL
        );
        CREATE INDEX IF NOT EXISTS IX_AUDIT_RECORD_AT ON AUDIT_RECORD (AT_UTC);
        """;

    private readonly string _path;
    private readonly object _gate = new();

    public SqliteAuditStore(string? path = null)
    {
        _path = path ?? OgPaths.FailureStorePath;

        if (!OgPaths.IsWritableLocation(_path))
        {
            throw new InvalidOperationException(
                $"The audit store must live under {OgPaths.DataRoot}, not '{_path}'.");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        EnsureSchema();
    }

    public void Write(
        string verb,
        string subjectKind,
        string subjectId,
        string outcome,
        string? detail,
        string? userId,
        string? companyId,
        string? fieldId)
    {
        lock (_gate)
        {
            using var connection = Open();
            using var command = connection.CreateCommand();
            command.CommandText =
                """
                INSERT INTO AUDIT_RECORD
                    (AT_UTC, VERB, SUBJECT_KIND, SUBJECT_ID, OUTCOME, DETAIL, USER_ID, COMPANY_ID, FIELD_ID)
                VALUES
                    ($at, $verb, $kind, $id, $outcome, $detail, $user, $company, $field);
                """;
            command.Parameters.AddWithValue("$at", DateTimeOffset.UtcNow.ToString("O"));
            command.Parameters.AddWithValue("$verb", verb);
            command.Parameters.AddWithValue("$kind", subjectKind);
            command.Parameters.AddWithValue("$id", subjectId);
            command.Parameters.AddWithValue("$outcome", outcome);
            command.Parameters.AddWithValue("$detail", (object?)detail ?? DBNull.Value);
            command.Parameters.AddWithValue("$user", (object?)userId ?? DBNull.Value);
            command.Parameters.AddWithValue("$company", (object?)companyId ?? DBNull.Value);
            command.Parameters.AddWithValue("$field", (object?)fieldId ?? DBNull.Value);
            command.ExecuteNonQuery();
        }
    }

    public IReadOnlyList<OgAuditRecord> Recent(int take)
    {
        lock (_gate)
        {
            using var connection = Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM AUDIT_RECORD ORDER BY ID DESC LIMIT $take;";
            command.Parameters.AddWithValue("$take", take);

            using var reader = command.ExecuteReader();
            var results = new List<OgAuditRecord>();
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
            command.CommandText = "SELECT COUNT(*) FROM AUDIT_RECORD;";
            return Convert.ToInt32(command.ExecuteScalar());
        }
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

    private static OgAuditRecord Read(SqliteDataReader reader) =>
        new(
            reader.GetInt64(reader.GetOrdinal("ID")),
            DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal("AT_UTC"))),
            reader.GetString(reader.GetOrdinal("VERB")),
            reader.GetString(reader.GetOrdinal("SUBJECT_KIND")),
            reader.GetString(reader.GetOrdinal("SUBJECT_ID")),
            reader.GetString(reader.GetOrdinal("OUTCOME")),
            reader.IsDBNull(reader.GetOrdinal("DETAIL")) ? null : reader.GetString(reader.GetOrdinal("DETAIL")),
            reader.IsDBNull(reader.GetOrdinal("USER_ID")) ? null : reader.GetString(reader.GetOrdinal("USER_ID")),
            reader.IsDBNull(reader.GetOrdinal("COMPANY_ID")) ? null : reader.GetString(reader.GetOrdinal("COMPANY_ID")),
            reader.IsDBNull(reader.GetOrdinal("FIELD_ID")) ? null : reader.GetString(reader.GetOrdinal("FIELD_ID")));
}

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Ppdm;

namespace TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;

/// <summary>
/// DAT-08's durable half: row versions, the read audit and the audit chain, in the local SQLite file the failure
/// and audit stores already use.
/// </summary>
/// <remarks>
/// The in-memory implementations are still used by tests and by any caller that does not want a file; this one
/// keeps the trail after the process ends. Row values are stored as JSON, so the exact CLR shape (a decimal stays
/// a decimal, a date stays a date) survives the round trip. Nothing updates or deletes an existing row here: every
/// operation is an insert, which is what makes the version history and the chain append-only.
/// </remarks>
public sealed class SqliteGovernanceStore : IOgVersionStore, IOgAccessAudit, IOgAuditChain, IOgQualityJournal
{
    private const string Genesis = "genesis";

    private const string CreateSql =
        """
        CREATE TABLE IF NOT EXISTS ROW_VERSION (
            ID            TEXT PRIMARY KEY,
            TABLE_NAME    TEXT NOT NULL,
            ROW_KEY       TEXT NOT NULL,
            VERSION       INTEGER NOT NULL,
            ACTOR         TEXT NOT NULL,
            AT_UTC        TEXT NOT NULL,
            VALUES_JSON   TEXT NOT NULL,
            RESTORED_FROM TEXT NULL
        );
        CREATE UNIQUE INDEX IF NOT EXISTS IX_ROW_VERSION_KEY ON ROW_VERSION (TABLE_NAME, ROW_KEY, VERSION);

        CREATE TABLE IF NOT EXISTS ACCESS_AUDIT (
            ID         INTEGER PRIMARY KEY AUTOINCREMENT,
            AT_UTC     TEXT NOT NULL,
            READER     TEXT NOT NULL,
            TABLE_NAME TEXT NOT NULL,
            FILTER     TEXT NOT NULL,
            ROW_COUNT  INTEGER NOT NULL
        );
        CREATE INDEX IF NOT EXISTS IX_ACCESS_AUDIT_AT ON ACCESS_AUDIT (AT_UTC);

        CREATE TABLE IF NOT EXISTS QUALITY_RUN (
            TABLE_NAME        TEXT PRIMARY KEY,
            LAST_SUCCESS_UTC  TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS AUDIT_CHAIN (
            SEQUENCE      INTEGER PRIMARY KEY,
            KIND          TEXT NOT NULL,
            DETAIL        TEXT NOT NULL,
            ACTOR         TEXT NOT NULL,
            AT_UTC        TEXT NOT NULL,
            PREVIOUS_HASH TEXT NOT NULL,
            HASH          TEXT NOT NULL
        );
        """;

    private static readonly JsonSerializerOptions Json = new() { WriteIndented = false };

    private readonly string _path;
    private readonly object _gate = new();

    public SqliteGovernanceStore(string? path = null)
    {
        _path = path ?? OgPaths.FailureStorePath;

        if (!OgPaths.IsWritableLocation(_path))
        {
            throw new InvalidOperationException(
                $"The governance store must live under {OgPaths.DataRoot}, not '{_path}'.");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        EnsureSchema();
    }

    // ---------------------------------------------------------------- versions

    public OgRowVersion Record(
        string table,
        string key,
        IReadOnlyDictionary<string, object?> values,
        string actor,
        DateTimeOffset? at = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(table);
        ArgumentNullException.ThrowIfNull(values);

        lock (_gate)
        {
            using var connection = Open();
            var version = NextVersion(connection, table, key);

            return InsertVersion(
                connection,
                table,
                key,
                version,
                actor,
                values,
                at ?? DateTimeOffset.UtcNow,
                restoredFrom: null);
        }
    }

    public IReadOnlyList<OgRowVersion> History(string table, string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(table);

        lock (_gate)
        {
            using var connection = Open();
            using var command = connection.CreateCommand();
            command.CommandText =
                """
                SELECT ID, TABLE_NAME, ROW_KEY, VERSION, ACTOR, AT_UTC, VALUES_JSON, RESTORED_FROM
                FROM ROW_VERSION
                WHERE TABLE_NAME = $table AND ROW_KEY = $key
                ORDER BY VERSION;
                """;
            command.Parameters.AddWithValue("$table", table);
            command.Parameters.AddWithValue("$key", key ?? string.Empty);

            using var reader = command.ExecuteReader();
            var versions = new List<OgRowVersion>();

            while (reader.Read())
            {
                versions.Add(ReadVersion(reader));
            }

            return versions;
        }
    }

    public OgResult<OgRowVersion> Restore(
        string table,
        string key,
        string versionId,
        string actor,
        DateTimeOffset? at = null)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgRowVersion>.Failed(
                "A restore must record who performed it.", "version-actor-required");
        }

        lock (_gate)
        {
            using var connection = Open();

            var source = History(connection, table, key)
                .FirstOrDefault(version => string.Equals(version.Id, versionId, StringComparison.Ordinal));

            if (source is null)
            {
                return OgResult<OgRowVersion>.Failed(
                    $"Version '{versionId}' is not in the history of {table} / {key}.",
                    "version-unknown");
            }

            var restored = InsertVersion(
                connection,
                table,
                key,
                NextVersion(connection, table, key),
                actor,
                source.Values,
                at ?? DateTimeOffset.UtcNow,
                source.Id);

            return OgResult<OgRowVersion>.Ok(restored);
        }
    }

    // ------------------------------------------------------------ access audit

    public void Record(string reader, string table, string filter, int rowCount, DateTimeOffset? at = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reader);
        ArgumentException.ThrowIfNullOrWhiteSpace(table);

        lock (_gate)
        {
            using var connection = Open();
            using var command = connection.CreateCommand();
            command.CommandText =
                """
                INSERT INTO ACCESS_AUDIT (AT_UTC, READER, TABLE_NAME, FILTER, ROW_COUNT)
                VALUES ($at, $reader, $table, $filter, $rows);
                """;
            command.Parameters.AddWithValue("$at", (at ?? DateTimeOffset.UtcNow).ToString("O"));
            command.Parameters.AddWithValue("$reader", reader.Trim());
            command.Parameters.AddWithValue("$table", table.Trim());
            command.Parameters.AddWithValue("$filter", filter?.Trim() ?? string.Empty);
            command.Parameters.AddWithValue("$rows", rowCount);
            command.ExecuteNonQuery();
        }
    }

    public IReadOnlyList<OgAccessAuditEntry> Query(OgAccessAuditQuery? query = null)
    {
        query ??= new OgAccessAuditQuery();

        lock (_gate)
        {
            using var connection = Open();
            using var command = connection.CreateCommand();

            var where = new List<string>();

            if (query.Reader is not null)
            {
                where.Add("READER = $reader COLLATE NOCASE");
                command.Parameters.AddWithValue("$reader", query.Reader);
            }

            if (query.Table is not null)
            {
                where.Add("TABLE_NAME = $table COLLATE NOCASE");
                command.Parameters.AddWithValue("$table", query.Table);
            }

            if (query.From is { } from)
            {
                where.Add("AT_UTC >= $from");
                command.Parameters.AddWithValue("$from", from.ToString("O"));
            }

            if (query.To is { } to)
            {
                where.Add("AT_UTC <= $to");
                command.Parameters.AddWithValue("$to", to.ToString("O"));
            }

            command.CommandText =
                "SELECT AT_UTC, READER, TABLE_NAME, FILTER, ROW_COUNT FROM ACCESS_AUDIT" +
                (where.Count == 0 ? string.Empty : " WHERE " + string.Join(" AND ", where)) +
                " ORDER BY AT_UTC DESC, ID DESC;";

            using var reader = command.ExecuteReader();
            var entries = new List<OgAccessAuditEntry>();

            while (reader.Read())
            {
                entries.Add(new OgAccessAuditEntry(
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetInt32(4),
                    DateTimeOffset.Parse(reader.GetString(0), CultureInfo.InvariantCulture)));
            }

            return entries;
        }
    }

    public string ToCsv(OgAccessAuditQuery? query = null)
    {
        var builder = new StringBuilder("utc,reader,table,filter,rowCount\n");

        foreach (var entry in Query(query))
        {
            builder.Append(entry.AtUtc.ToString("O", CultureInfo.InvariantCulture)).Append(',')
                .Append(Quote(entry.Reader)).Append(',')
                .Append(Quote(entry.Table)).Append(',')
                .Append(Quote(entry.Filter)).Append(',')
                .Append(entry.RowCount)
                .Append('\n');
        }

        return builder.ToString();
    }

    // ----------------------------------------------------------- quality runs

    /// <summary>DAT-07 AC-3: only a successful run advances this table's timestamp.</summary>
    public void Record(OgQualityRun run)
    {
        ArgumentNullException.ThrowIfNull(run);

        if (!run.Succeeded)
        {
            return;
        }

        lock (_gate)
        {
            using var connection = Open();
            using var command = connection.CreateCommand();
            command.CommandText =
                """
                INSERT INTO QUALITY_RUN (TABLE_NAME, LAST_SUCCESS_UTC)
                VALUES ($table, $at)
                ON CONFLICT (TABLE_NAME) DO UPDATE SET LAST_SUCCESS_UTC = excluded.LAST_SUCCESS_UTC;
                """;
            command.Parameters.AddWithValue("$table", run.Table);
            command.Parameters.AddWithValue("$at", run.CompletedAtUtc.ToString("O"));
            command.ExecuteNonQuery();
        }
    }

    public DateTimeOffset? LastSuccess(string table)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(table);

        lock (_gate)
        {
            using var connection = Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT LAST_SUCCESS_UTC FROM QUALITY_RUN WHERE TABLE_NAME = $table;";
            command.Parameters.AddWithValue("$table", table);

            var value = command.ExecuteScalar();

            return value is null or DBNull
                ? null
                : DateTimeOffset.Parse((string)value, CultureInfo.InvariantCulture);
        }
    }

    // ------------------------------------------------------------- audit chain

    public IReadOnlyList<OgAuditChainEntry> Entries
    {
        get
        {
            lock (_gate)
            {
                using var connection = Open();
                return Chain(connection);
            }
        }
    }

    public OgAuditChainEntry Append(string kind, string detail, string actor, DateTimeOffset? at = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);

        lock (_gate)
        {
            using var connection = Open();
            var entries = Chain(connection);

            var sequence = entries.Count + 1;
            var previous = entries.Count == 0 ? Genesis : entries[^1].Hash;
            var when = at ?? DateTimeOffset.UtcNow;
            var normalisedActor = string.IsNullOrWhiteSpace(actor) ? "(unknown)" : actor.Trim();

            var entry = new OgAuditChainEntry(
                sequence,
                kind,
                detail ?? string.Empty,
                normalisedActor,
                when,
                previous,
                HashOf(sequence, kind, detail ?? string.Empty, normalisedActor, when, previous));

            using var command = connection.CreateCommand();
            command.CommandText =
                """
                INSERT INTO AUDIT_CHAIN (SEQUENCE, KIND, DETAIL, ACTOR, AT_UTC, PREVIOUS_HASH, HASH)
                VALUES ($sequence, $kind, $detail, $actor, $at, $previous, $hash);
                """;
            command.Parameters.AddWithValue("$sequence", entry.Sequence);
            command.Parameters.AddWithValue("$kind", entry.Kind);
            command.Parameters.AddWithValue("$detail", entry.Detail);
            command.Parameters.AddWithValue("$actor", entry.Actor);
            command.Parameters.AddWithValue("$at", entry.AtUtc.ToString("O"));
            command.Parameters.AddWithValue("$previous", entry.PreviousHash);
            command.Parameters.AddWithValue("$hash", entry.Hash);
            command.ExecuteNonQuery();

            return entry;
        }
    }

    public OgChainCheck Verify()
    {
        lock (_gate)
        {
            using var connection = Open();
            var entries = Chain(connection);
            var previous = Genesis;

            foreach (var entry in entries)
            {
                var expected = HashOf(
                    entry.Sequence, entry.Kind, entry.Detail, entry.Actor, entry.AtUtc, previous);

                if (!string.Equals(entry.PreviousHash, previous, StringComparison.Ordinal)
                    || !string.Equals(entry.Hash, expected, StringComparison.Ordinal))
                {
                    return new OgChainCheck(false, entry.Sequence, entries.Count);
                }

                previous = entry.Hash;
            }

            return new OgChainCheck(true, null, entries.Count);
        }
    }

    /// <summary>The rule is the same one the in-memory chain applies; it is stated once, here.</summary>
    public bool WritesAllowed(OgChainCheck check, OgChainAcknowledgement? acknowledgement)
    {
        ArgumentNullException.ThrowIfNull(check);

        if (check.IsIntact)
        {
            return true;
        }

        return acknowledgement is not null
               && acknowledgement.BrokenAtSequence == check.BrokenAtSequence
               && !string.IsNullOrWhiteSpace(acknowledgement.Actor);
    }

    /// <summary>Rewrites one entry's detail without re-hashing, so a drill can prove <see cref="Verify"/> notices.</summary>
    public void Tamper(int sequence, string detail)
    {
        lock (_gate)
        {
            using var connection = Open();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE AUDIT_CHAIN SET DETAIL = $detail WHERE SEQUENCE = $sequence;";
            command.Parameters.AddWithValue("$detail", detail);
            command.Parameters.AddWithValue("$sequence", sequence);

            if (command.ExecuteNonQuery() == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sequence), sequence, "No such chain entry.");
            }
        }
    }

    /// <summary>The hash an entry must have, given its sequence, content and the hash before it.</summary>
    /// <remarks>
    /// The payload format is part of the stored data's meaning: an existing database was written with it, and a
    /// verification that used a different format would report every entry as tampered.
    /// </remarks>
    public static string HashOf(
        int sequence,
        string kind,
        string detail,
        string actor,
        DateTimeOffset at,
        string previous) =>
        Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes($"{sequence}|{kind}|{detail}|{actor}|{at:O}|{previous}")));

    // ---------------------------------------------------------------- helpers

    private OgRowVersion InsertVersion(
        SqliteConnection connection,
        string table,
        string key,
        int version,
        string actor,
        IReadOnlyDictionary<string, object?> values,
        DateTimeOffset at,
        string? restoredFrom)
    {
        var stored = new OgRowVersion(
            Guid.NewGuid().ToString("n")[..12],
            table,
            key ?? string.Empty,
            version,
            string.IsNullOrWhiteSpace(actor) ? "(unknown)" : actor.Trim(),
            at,
            new Dictionary<string, object?>(values, StringComparer.OrdinalIgnoreCase),
            restoredFrom);

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO ROW_VERSION
                (ID, TABLE_NAME, ROW_KEY, VERSION, ACTOR, AT_UTC, VALUES_JSON, RESTORED_FROM)
            VALUES
                ($id, $table, $key, $version, $actor, $at, $values, $restoredFrom);
            """;
        command.Parameters.AddWithValue("$id", stored.Id);
        command.Parameters.AddWithValue("$table", stored.Table);
        command.Parameters.AddWithValue("$key", stored.Key);
        command.Parameters.AddWithValue("$version", stored.Version);
        command.Parameters.AddWithValue("$actor", stored.Actor);
        command.Parameters.AddWithValue("$at", stored.AtUtc.ToString("O"));
        command.Parameters.AddWithValue(
            "$values",
            JsonSerializer.Serialize(
                stored.Values.ToDictionary(pair => pair.Key, pair => Envelope(pair.Value), StringComparer.OrdinalIgnoreCase),
                Json));
        command.Parameters.AddWithValue("$restoredFrom", (object?)restoredFrom ?? DBNull.Value);
        command.ExecuteNonQuery();

        return stored;
    }

    private static int NextVersion(SqliteConnection connection, string table, string key)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT COALESCE(MAX(VERSION), 0) + 1 FROM ROW_VERSION WHERE TABLE_NAME = $table AND ROW_KEY = $key;";
        command.Parameters.AddWithValue("$table", table);
        command.Parameters.AddWithValue("$key", key ?? string.Empty);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static List<OgRowVersion> History(SqliteConnection connection, string table, string key)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT ID, TABLE_NAME, ROW_KEY, VERSION, ACTOR, AT_UTC, VALUES_JSON, RESTORED_FROM
            FROM ROW_VERSION
            WHERE TABLE_NAME = $table AND ROW_KEY = $key
            ORDER BY VERSION;
            """;
        command.Parameters.AddWithValue("$table", table);
        command.Parameters.AddWithValue("$key", key ?? string.Empty);

        using var reader = command.ExecuteReader();
        var versions = new List<OgRowVersion>();

        while (reader.Read())
        {
            versions.Add(ReadVersion(reader));
        }

        return versions;
    }

    private static OgRowVersion ReadVersion(SqliteDataReader reader)
    {
        var values = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                         reader.GetString(reader.GetOrdinal("VALUES_JSON")), Json)
                     ?? [];

        var restored = reader.IsDBNull(reader.GetOrdinal("RESTORED_FROM"))
            ? null
            : reader.GetString(reader.GetOrdinal("RESTORED_FROM"));

        return new OgRowVersion(
            reader.GetString(reader.GetOrdinal("ID")),
            reader.GetString(reader.GetOrdinal("TABLE_NAME")),
            reader.GetString(reader.GetOrdinal("ROW_KEY")),
            reader.GetInt32(reader.GetOrdinal("VERSION")),
            reader.GetString(reader.GetOrdinal("ACTOR")),
            DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal("AT_UTC")), CultureInfo.InvariantCulture),
            values.ToDictionary(
                pair => pair.Key,
                pair => ToValue(pair.Value),
                StringComparer.OrdinalIgnoreCase),
            restored);
    }

    /// <summary>JSON back to the CLR shape the caller stored, so a version can be compared with a live row.</summary>
    private static object? ToValue(JsonElement element)
    {
        if (element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        // Values are stored as { k: kind, v: text } so a decimal stays a decimal and a date stays a date; a bare
        // JSON value would turn a date into a string on the way back.
        if (element.ValueKind != JsonValueKind.Object)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Number when element.TryGetInt64(out var integer) => integer,
                JsonValueKind.Number => element.GetDecimal(),
                _ => element.GetRawText(),
            };
        }

        var kind = element.TryGetProperty("k", out var kindElement) ? kindElement.GetString() : null;
        var text = element.TryGetProperty("v", out var valueElement) ? valueElement.GetString() : null;

        return kind switch
        {
            "string" => text,
            "bool" => bool.Parse(text!),
            "int" => long.Parse(text!, CultureInfo.InvariantCulture),
            "decimal" => decimal.Parse(text!, CultureInfo.InvariantCulture),
            "double" => double.Parse(text!, CultureInfo.InvariantCulture),
            "date" => DateTime.Parse(text!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            "offset" => DateTimeOffset.Parse(text!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            "guid" => Guid.Parse(text!),
            _ => text,
        };
    }

    /// <summary>The stored envelope for one value: its kind and its invariant text.</summary>
    private static object? Envelope(object? value) => value switch
    {
        null => null,
        string text => new { k = "string", v = text },
        bool flag => new { k = "bool", v = flag ? "true" : "false" },
        int or long or short or byte => new { k = "int", v = Convert.ToString(value, CultureInfo.InvariantCulture) },
        decimal number => new { k = "decimal", v = number.ToString(CultureInfo.InvariantCulture) },
        double number => new { k = "double", v = number.ToString("R", CultureInfo.InvariantCulture) },
        float number => new { k = "double", v = ((double)number).ToString("R", CultureInfo.InvariantCulture) },
        DateTime moment => new { k = "date", v = moment.ToString("O", CultureInfo.InvariantCulture) },
        DateTimeOffset moment => new { k = "offset", v = moment.ToString("O", CultureInfo.InvariantCulture) },
        Guid id => new { k = "guid", v = id.ToString() },
        _ => new { k = "string", v = Convert.ToString(value, CultureInfo.InvariantCulture) },
    };

    private static List<OgAuditChainEntry> Chain(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT SEQUENCE, KIND, DETAIL, ACTOR, AT_UTC, PREVIOUS_HASH, HASH FROM AUDIT_CHAIN ORDER BY SEQUENCE;";

        using var reader = command.ExecuteReader();
        var entries = new List<OgAuditChainEntry>();

        while (reader.Read())
        {
            entries.Add(new OgAuditChainEntry(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                DateTimeOffset.Parse(reader.GetString(4), CultureInfo.InvariantCulture),
                reader.GetString(5),
                reader.GetString(6)));
        }

        return entries;
    }

    private static string Quote(string value) => '"' + value.Replace("\"", "\"\"") + '"';

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
}

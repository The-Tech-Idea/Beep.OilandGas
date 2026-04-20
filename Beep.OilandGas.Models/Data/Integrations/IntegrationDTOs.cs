namespace Beep.OilandGas.Models.Data.Integrations;

// ── Shared ────────────────────────────────────────────────────────────────────

public enum CircuitBreakerState { Closed, Open, HalfOpen }

public record DateRange(DateTime Start, DateTime End);

// ── WITSML ────────────────────────────────────────────────────────────────────

public record WitsmlSyncResult(
    bool    Success,
    string  TargetTableName,
    string  TargetId,
    int     RecordsWritten,
    string? ErrorMessage);

public record WitsmlWellSummary(
    string   Uid,
    string   Name,
    string   Country,
    string   Operator,
    DateTime? SpudDate);

// ── PRODML ────────────────────────────────────────────────────────────────────

public record ProdmlSyncResult(
    bool      Success,
    int       VolumeRowsWritten,
    int       DispositionRowsWritten,
    DateRange PeriodCovered,
    string?   ErrorMessage);

public record ProdmlWellSummary(string Uid, string WellName, string Status);

// ── OPC-UA / SCADA ────────────────────────────────────────────────────────────

public record OpcTagValue(
    string   NodeId,
    string   TagName,
    object?  Value,
    string   DataType,
    DateTime SourceTimestamp,
    bool     IsGood);

// ── Document Management ───────────────────────────────────────────────────────

public record DocumentSyncResult(
    bool    Success,
    int     RecordsWritten,
    int     RecordsUpdated,
    string? ErrorMessage);

public record DocumentSummary(
    string   DocumentId,
    string   Name,
    string   ContentType,
    long     SizeBytes,
    DateTime LastModified,
    string   Author,
    string   Folder);

// ── SAP ERP ───────────────────────────────────────────────────────────────────

public record SapSyncResult(
    bool    Success,
    string? ProjectId,
    int     CostRowsWritten,
    string? ErrorMessage);

public record SapWorkOrderSummary(
    string   SapWoNumber,
    string   Description,
    string   WoType,
    string   Status,
    DateTime? BasicFinishDate,
    decimal? TotalCost);

// ── OSDU ──────────────────────────────────────────────────────────────────────

public record OsduSyncResult(
    bool    Success,
    string  EntityType,
    string  TargetId,
    string  SyncMode,      // 'INSERT' or 'UPDATE'
    string? ErrorMessage);

public record OsduEntitySummary(
    string OsduId,
    string Name,
    string Kind,
    string Status);

// ── Health Monitor ────────────────────────────────────────────────────────────

public record AdapterHealthStatus(
    string              AdapterName,
    CircuitBreakerState State,
    int                 ConsecutiveFailures,
    DateTime?           LastSuccessAt,
    DateTime?           LastFailureAt,
    string?             LastErrorMessage,
    TimeSpan?           HalfOpenCooldown);

/// <summary>Lightweight record describing the last result of one adapter sync run.</summary>
public record IntegrationSyncHistoryEntry(
    string   AdapterName,
    DateTime RunAt,
    bool     Success,
    int      RecordsWritten,
    string?  ErrorMessage);

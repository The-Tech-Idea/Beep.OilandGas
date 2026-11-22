using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;

/// <summary>What the pipeline publishes after it has made a failure or an audit row durable.</summary>
public sealed record OgDiagnosticEvent(string? Reference, FailureSeverity Severity, string Operation);

/// <summary>
/// The single fan-in: implements <see cref="IFailureReporter"/> for every <c>catch</c> and
/// <see cref="IOgAuditService"/> for every write and refusal, writes the store, writes the log line,
/// and publishes to the notification hub.
/// </summary>
/// <remarks>
/// Reporting never throws back into the catch that called it. When the store is unavailable the
/// implementation degrades to the log line (and <see cref="LastResortLog"/>) and marks
/// <see cref="IsDegraded"/> so the shell can say so; the reference is still returned.
/// </remarks>
public sealed class OgDiagnosticsPipeline : IFailureReporter, IOgAuditService
{
    private const int MaxMessageLength = 4_000;
    private const int MaxStackLength = 16_000;
    private const int MaxDetailLength = 8_000;

    private readonly IFailureStoreAccessor _failures;
    private readonly IOgAuditStore _audit;
    private readonly OgRuntimeContext _context;
    private readonly ILogger<OgDiagnosticsPipeline> _logger;

    public OgDiagnosticsPipeline(
        IFailureStoreAccessor failures,
        IOgAuditStore audit,
        OgRuntimeContext context,
        ILogger<OgDiagnosticsPipeline> logger)
    {
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>True once a store write failed; the shell shows a persistent banner while it is set.</summary>
    public bool IsDegraded { get; private set; }

    public string? DegradedReason { get; private set; }

    /// <summary>Raised after a failure or audit row is durable (or degraded).</summary>
    public event EventHandler<OgDiagnosticEvent>? Published;

    public string ReportHandled(
        Exception exception,
        string operation,
        string consequence,
        FailureSeverity severity = FailureSeverity.Error)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        ArgumentException.ThrowIfNullOrWhiteSpace(consequence);

        var reference = FailureReferenceFormat.New();
        var record = new OgFailureRecord(
            reference,
            DateTimeOffset.UtcNow,
            severity,
            operation,
            consequence,
            exception.GetType().FullName ?? exception.GetType().Name,
            Redact(exception.Message, MaxMessageLength),
            Redact(exception.StackTrace ?? string.Empty, MaxStackLength),
            _context.UserId,
            _context.CompanyId,
            Forwarded: false);

        WriteFailure(record);

        Published?.Invoke(this, new OgDiagnosticEvent(reference, severity, operation));
        return reference;
    }

    public void Add(
        string verb,
        string subjectKind,
        string subjectId,
        string outcome,
        string? detail = null,
        string? fieldId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(verb);
        ArgumentException.ThrowIfNullOrWhiteSpace(subjectKind);
        ArgumentException.ThrowIfNullOrWhiteSpace(subjectId);
        ArgumentException.ThrowIfNullOrWhiteSpace(outcome);

        WriteAudit(
            verb,
            subjectKind,
            subjectId,
            outcome,
            detail is null ? null : Redact(detail, MaxDetailLength),
            fieldId);
    }

    private void WriteFailure(OgFailureRecord record)
    {
        try
        {
            _failures.Store.Write(record);
            _logger.LogWarning(
                "Failure {Reference} in {Operation} ({Severity}).",
                record.Reference,
                record.Operation,
                record.Severity);
            return;
        }
#pragma warning disable BEEP0003 // the reporter's own store failed; there is nothing further to report to
        catch (Exception storeFailure)
#pragma warning restore BEEP0003
        {
            // Reporting must not throw into the catch that called it. The line is the only record left.
            Degrade("the failure store could not be written");
            LastResortLog.Write(
                () => _logger.LogError(storeFailure, "Failure store write failed for {Reference}.", record.Reference),
                "a failure record");
        }
    }

    private void WriteAudit(
        string verb,
        string subjectKind,
        string subjectId,
        string outcome,
        string? detail,
        string? fieldId)
    {
        try
        {
            _audit.Write(
                verb,
                subjectKind,
                subjectId,
                outcome,
                detail,
                _context.UserId,
                _context.CompanyId,
                fieldId ?? _context.FieldId);
        }
#pragma warning disable BEEP0003 // the audit store failed; recorded through the log line below
        catch (Exception storeFailure)
#pragma warning restore BEEP0003
        {
            Degrade("the audit store could not be written");
            LastResortLog.Write(
                () => _logger.LogError(storeFailure, "Audit write failed for {Verb} {Subject}.", verb, subjectId),
                "an audit record");
        }

        Published?.Invoke(this, new OgDiagnosticEvent(null, FailureSeverity.Degraded, $"{verb} {subjectKind}"));
    }

    private void Degrade(string reason)
    {
        IsDegraded = true;
        DegradedReason = reason;
    }

    /// <summary>
    /// Removes the shapes that must never be stored: an authorization header, a password, a token or a
    /// connection-string secret. Bounded so a runaway message cannot bloat the store.
    /// </summary>
    private static string Redact(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var redacted = System.Text.RegularExpressions.Regex.Replace(
            value,
            @"(?i)(password|pwd|token|secret|api[_-]?key|authorization)\s*[=:]\s*\S+",
            "$1=***");

        return redacted.Length <= maxLength ? redacted : redacted[..maxLength];
    }
}

/// <summary>
/// Indirection so a test can substitute the store without the pipeline depending on a file, and so the
/// store itself can be fail-fast when the directory is unusable.
/// </summary>
public interface IFailureStoreAccessor
{
    IOgFailureStore Store { get; }
}

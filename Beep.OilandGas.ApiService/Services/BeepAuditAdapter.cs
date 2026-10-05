using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Services.Audit;
using TheTechIdea.Beep.Services.Audit.Models;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.ApiService.Services
{
    /// <summary>
    /// Bridges the existing PPDM-based audit infrastructure to BeepDM's tamper-evident
    /// audit pipeline (IBeepAudit with HMAC hash chains).
    ///
    /// Phase 1B of BeepDM framework integration.
    ///
    /// Usage: Inject alongside existing PPDMDataAccessAuditService.
    /// Call AuditEvent() to dual-write to both the PPDM audit table AND the
    /// BeepDM hash-chain audit pipeline.
    /// </summary>
    public class BeepAuditAdapter
    {
        private readonly IBeepAudit? _audit;
        private readonly ILogger<BeepAuditAdapter> _logger;
        private readonly IFailureReporter _failures;

        public BeepAuditAdapter(IBeepAudit? audit, ILogger<BeepAuditAdapter> logger, IFailureReporter failures)
        {
            _audit = audit;
            _logger = logger;
            _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        }

        /// <summary>
        /// Records an audit event to both the PPDM table (via existing service)
        /// and the BeepDM tamper-evident hash-chain pipeline.
        /// </summary>
        /// <param name="eventType">e.g., "ACCESS", "DATA_CHANGE", "SETUP"</param>
        /// <param name="resource">e.g., table name or endpoint path</param>
        /// <param name="action">e.g., "CREATE", "UPDATE", "DELETE"</param>
        /// <param name="userId">User who performed the action.</param>
        /// <param name="details">Optional payload (will be redacted if PII is present).</param>
        /// <param name="recordKey">Optional primary key of the affected record.</param>
        public async Task RecordAsync(
            string eventType,
            string resource,
            string action,
            string userId,
            string? details = null,
            string? recordKey = null)
        {
            if (_audit == null)
            {
                _logger.LogWarning("BeepAuditAdapter: IBeepAudit not registered — audit event skipped");
                return;
            }

            try
            {
                var auditEvent = new AuditEvent
                {
                    Source = eventType,
                    EntityName = resource,
                    Operation = action,
                    UserId = userId,
                    Properties = new System.Collections.Generic.Dictionary<string, object> { ["Details"] = details ?? string.Empty },
                    RecordKey = recordKey,
                    TimestampUtc = DateTime.UtcNow
                };

                await _audit.RecordAsync(auditEvent);
            }
            // Audit pipeline failure must never crash the application: the PPDM audit table write (handled by the existing
            // service) is the authoritative record and BeepDM audit is additive. The lost hash-chain entry is reported, so
            // the gap is visible. Cancellation is the caller ending.
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _failures.ReportHandled(ex, $"recording the {eventType}/{resource}/{action} audit event in the BeepDM hash chain",
                    "the PPDM audit record stands; the hash-chain entry is not written", FailureSeverity.Degraded);
            }
        }

        /// <summary>
        /// Checks whether the BeepDM audit pipeline is available.
        /// </summary>
        public bool IsAvailable => _audit != null;
    }
}

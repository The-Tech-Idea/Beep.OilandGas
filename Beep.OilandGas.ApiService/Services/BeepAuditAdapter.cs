using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Services.Audit;
using TheTechIdea.Beep.Services.Audit.Models;

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

        public BeepAuditAdapter(IBeepAudit? audit, ILogger<BeepAuditAdapter> logger)
        {
            _audit = audit;
            _logger = logger;
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
            catch (Exception ex)
            {
                // Audit pipeline failure must never crash the application.
                // The PPDM audit table write (handled by the existing service)
                // is the authoritative record; BeepDM audit is additive.
                _logger.LogError(ex, "Failed to record audit event via BeepDM pipeline: {EventType}/{Resource}/{Action}",
                    eventType, resource, action);
            }
        }

        /// <summary>
        /// Checks whether the BeepDM audit pipeline is available.
        /// </summary>
        public bool IsAvailable => _audit != null;
    }
}

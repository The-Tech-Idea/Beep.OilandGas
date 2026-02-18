using System;

namespace Beep.OilandGas.Accounting.Models
{
    public class AuditLogEntry
    {
        public string EntryId { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; }
        public string UserId { get; set; }
        public string Action { get; set; } // Created, Modified, Deleted, Anomaly
        public string EntityName { get; set; }
        public string RecordId { get; set; }
        public string Details { get; set; }
        public bool IsAnomaly { get; set; }
        public double AnomalyScore { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class AuditRecord
    {
        public string AuditId { get; set; } = Guid.NewGuid().ToString();
        public string Action { get; set; } = string.Empty; // Create, Read, Update, Delete
        public string Resource { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Details { get; set; } = new Dictionary<string, object>();
    }
}

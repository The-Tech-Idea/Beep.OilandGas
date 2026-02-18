using System;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class AccessHistory
    {
        public string UserId { get; set; } = string.Empty;
        public string ResourceId { get; set; } = string.Empty;
        public string AccessType { get; set; } = string.Empty;
        public DateTime AccessTime { get; set; }
        public bool Success { get; set; }
        public string FailureReason { get; set; } = string.Empty;
    }
}

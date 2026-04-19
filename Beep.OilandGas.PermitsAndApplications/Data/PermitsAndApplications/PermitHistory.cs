using System;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public class PermitHistory
    {
        public string HistoryId { get; set; } = string.Empty;
        public string PermitId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // Submitted, Approved, Rejected, etc.
        public DateTime ActionDate { get; set; }
        public string PerformedBy { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
    }
}

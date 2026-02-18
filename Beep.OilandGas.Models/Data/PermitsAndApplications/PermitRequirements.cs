using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public class PermitRequirements
    {
        public string PermitType { get; set; } = string.Empty;
        public string Jurisdiction { get; set; } = string.Empty;
        public List<string> RequiredDocuments { get; set; } = new(); // List of document names/types
        public decimal ApplicationFee { get; set; }
        public string ProcessingTime { get; set; } = string.Empty; // e.g., "30 days"
        public string Guidelines { get; set; } = string.Empty;
    }
}

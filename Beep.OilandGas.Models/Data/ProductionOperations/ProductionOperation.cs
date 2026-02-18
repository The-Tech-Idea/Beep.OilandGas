using System;

namespace Beep.OilandGas.Models.Data.ProductionOperations
{
    public class ProductionOperation : ModelEntityBase
    {
        public string OperationId { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty; // Maintenance, Inspection, etc.
        public DateTime ScheduledDate { get; set; }
        public string Status { get; set; } = "Planned";
        public string AssignedTo { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
    }
}

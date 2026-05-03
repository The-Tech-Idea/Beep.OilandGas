using System;

namespace Beep.OilandGas.Models.Data;

public class CreateWellMaintenancePlan : ModelEntityBase
{
    public string PlanId { get; set; } = string.Empty;
    public string WellUwi { get; set; } = string.Empty;
    public string MaintenanceType { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string TriggerBasis { get; set; } = string.Empty;
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public string ServiceBusinessAssociateId { get; set; } = string.Empty;
}

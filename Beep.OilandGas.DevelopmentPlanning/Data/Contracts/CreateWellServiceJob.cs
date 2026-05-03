using System;

namespace Beep.OilandGas.Models.Data;

public class CreateWellServiceJob : ModelEntityBase
{
    public string PlanId { get; set; } = string.Empty;
    public string WellUwi { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public string ServiceBusinessAssociateId { get; set; } = string.Empty;
    public string BusinessAssociateServiceType { get; set; } = string.Empty;
}

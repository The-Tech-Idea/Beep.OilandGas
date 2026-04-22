using System;

namespace Beep.OilandGas.Models.Data.ProductionAccounting;

public class AfeSummaryModel : ModelEntityBase
{
    public string AfeId { get; set; } = string.Empty;
    public string? AfeNumber { get; set; }
    public string? AfeName { get; set; }
    public string? PropertyId { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string? Status { get; set; }
    public DateTime? EffectiveDate { get; set; }
}

public class CreateAfeResponse : ModelEntityBase
{
    public string? AfeId { get; set; }
    public string? AfeNumber { get; set; }
}
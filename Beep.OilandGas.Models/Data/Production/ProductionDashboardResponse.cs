namespace Beep.OilandGas.Models.Data.Production;

/// <summary>One field-bound response; repository reads are not a transactional database snapshot.</summary>
public sealed class ProductionDashboardResponse
{
    public ProductionDashboardSummary Summary { get; set; } = new();
    public List<ProductionWellStatusDto> Wells { get; set; } = new();
}

/// <summary>Summary KPIs returned by GET /production/dashboard/summary</summary>
public class ProductionDashboardSummary
{
    public string FieldId        { get; set; } = string.Empty;
    public int    TotalWells     { get; set; }
    public int    OpenWorkOrders { get; set; }
}

/// <summary>Per-well status row returned by GET /production/dashboard/wells</summary>
public class ProductionWellStatusDto
{
    public string    WellId       { get; set; } = string.Empty;
    public string    WellName     { get; set; } = string.Empty;
    public string    Status       { get; set; } = string.Empty;
    public double    OilRate      { get; set; }
    public double    GasRate      { get; set; }
    public double    WaterCut     { get; set; }
    public DateTime? LastTestDate { get; set; }
}


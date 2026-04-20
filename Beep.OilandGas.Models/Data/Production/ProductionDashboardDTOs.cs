using System;

namespace Beep.OilandGas.Models.Data.Production;

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

/// <summary>Summary KPIs returned by GET /development/dashboard/summary</summary>
public class DevelopmentDashboardSummary
{
    public string FieldId         { get; set; } = string.Empty;
    public int    TotalWells      { get; set; }
    public int    DrillingWells   { get; set; }
    public int    CompletedWells  { get; set; }
    public int    PlannedWells    { get; set; }
}

/// <summary>Per-well status row returned by GET /development/dashboard/wells</summary>
public class DevelopmentWellStatusDto
{
    public string WellId          { get; set; } = string.Empty;
    public string WellName        { get; set; } = string.Empty;
    public string WellType        { get; set; } = string.Empty;
    public string DrillingStatus  { get; set; } = string.Empty;
    public double CurrentDepthM   { get; set; }
    public double TargetDepthM    { get; set; }
}

/// <summary>Summary KPIs returned by GET /reservoir/dashboard/summary</summary>
public class ReservoirDashboardSummary
{
    public string FieldId                  { get; set; } = string.Empty;
    public double OoipMmbbl                { get; set; }
    public double RecoveryFactorPct        { get; set; }
    public double Reserves1PMmbbl          { get; set; }
    public double Reserves2PMmbbl          { get; set; }
    public double Reserves3PMmbbl          { get; set; }
    public double ContingentResourcesMmbbl { get; set; }
    public int    ActivePoolCount          { get; set; }
}

/// <summary>Pool row returned by GET /reservoir/dashboard/pools</summary>
public class ReservoirPoolDto
{
    public string PoolId     { get; set; } = string.Empty;
    public string PoolName   { get; set; } = string.Empty;
    public string Status     { get; set; } = string.Empty;
    public double AreaAcres  { get; set; }
    public string Formation  { get; set; } = string.Empty;
}

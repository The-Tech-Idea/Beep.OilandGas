using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.HSE;

// ── Incident Types ─────────────────────────────────────────────────────────────
public static class IncidentType
{
    public const string Spill         = "SPILL";
    public const string NearMiss      = "NEAR_MISS";
    public const string Injury        = "INJURY";
    public const string PseTier1      = "PSE_TIER1";
    public const string PseTier2      = "PSE_TIER2";
    public const string Fire          = "FIRE";
    public const string Explosion     = "EXPLOSION";
    public const string PropertyDmg   = "PROPERTY_DAMAGE";
}

public static class IncidentState
{
    public const string Reported                = "REPORTED";
    public const string UnderInvestigation      = "UNDER_INVESTIGATION";
    public const string PendingRCA              = "PENDING_RCA";
    public const string PendingCorrectiveAction = "PENDING_CORRECTIVE_ACTION";
    public const string PendingCloseOut         = "PENDING_CLOSE_OUT";
    public const string Closed                  = "CLOSED";
    public const string Cancelled               = "CANCELLED";
}

public static class CauseType
{
    public const string Immediate    = "IMMEDIATE";
    public const string Contributing = "CONTRIBUTING";
    public const string Root         = "ROOT";
}

public static class CauseCategory
{
    public const string Human       = "HUMAN";
    public const string Equipment   = "EQUIPMENT";
    public const string Process     = "PROCESS";
    public const string Environment = "ENVIRONMENT";
}

public static class BarrierSide
{
    public const string Left  = "LEFT";   // Preventive (threat → top event)
    public const string Right = "RIGHT";  // Mitigative (top event → consequence)
}

public static class BarrierStatus
{
    public const string Effective     = "EFFECTIVE";
    public const string Degraded      = "DEGRADED";
    public const string Failed        = "FAILED";
    public const string NotApplicable = "NOT_APPLICABLE";
}

// ── Date Range ─────────────────────────────────────────────────────────────────
public record DateRangeFilter(DateTime Start, DateTime End);

// ── Incident DTOs ──────────────────────────────────────────────────────────────
public class HSEIncidentRecord : ModelEntityBase
{
    public string   IncidentId      { get; set; } = string.Empty;
    public string   FieldId         { get; set; } = string.Empty;
    public string   IncidentType    { get; set; } = string.Empty;
    public int      Tier            { get; set; } = 4;
    public DateTime IncidentDate    { get; set; } = DateTime.UtcNow;
    public string   Location        { get; set; } = string.Empty;
    public string   Description     { get; set; } = string.Empty;
    public string   Jurisdiction    { get; set; } = "USA";
    public string   CurrentState    { get; set; } = IncidentState.Reported;
    public string?  InvestigatorId  { get; set; }
}

public record ReportIncidentRequest(
    string   FieldId,
    string   IncidentType,
    int      Tier,
    DateTime IncidentDate,
    string   Location,
    string   Description,
    string   Jurisdiction);

public class TransitionIncidentRequest
{
    public string  Trigger { get; set; } = string.Empty;
    public string? Reason  { get; set; }
}

// ── RCA DTOs ───────────────────────────────────────────────────────────────────
public record CauseFinding(
    int    Seq,
    string CauseType,
    string Category,
    string Description);

public record AddCauseRequest(
    string CauseType,
    string CauseDesc,
    string CauseCategory,
    int    Seq);

public record UpdateCauseRequest(
    string CauseDesc,
    string CauseCategory);

// ── Barrier DTOs ───────────────────────────────────────────────────────────────
public record BarrierRecord(
    string  EquipId,
    string  BarrierName,
    string  BarrierType,
    string  BarrierSide,
    string  Status,
    string? FailureDesc);

public record BarrierSummary(
    int TotalBarriers,
    int FailedBarriers,
    int DegradedBarriers,
    int EffectiveBarriers,
    int API754TierInfluence);

public record AddBarrierRequest(
    string  EquipId,
    string  BarrierSide,
    string  BarrierType,
    string? FailureDesc);

// ── Corrective Action DTOs ─────────────────────────────────────────────────────
public record AddCARequest(
    string   CADescription,
    string   CAType,
    DateTime DueDate,
    string   ResponsibleBaId);

public record CAStatus(
    int       StepSeq,
    string    Description,
    string    CAType,
    string    Status,
    DateTime  DueDate,
    bool      IsOverdue,
    string?   ResponsiblePerson,
    DateTime? CompletedDate);

// ── HAZOP DTOs ────────────────────────────────────────────────────────────────
public record HAZOPSummary(
    string StudyId,
    string StudyName,
    int    TotalNodes,
    int    TotalDeviations,
    int    OpenDeviations,
    int    ActionRequired,
    int    Closed);

public class HAZOPNode
{
    public int    NodeSeq   { get; set; }
    public string NodeName  { get; set; } = string.Empty;
    public string Safeguard { get; set; } = string.Empty;
    public List<HAZOPDeviation> Deviations { get; set; } = new();
}

public class HAZOPDeviation
{
    public int    CondSeq     { get; set; }
    public string GuideWord   { get; set; } = string.Empty;
    public string Deviation   { get; set; } = string.Empty;
    public string Consequence { get; set; } = string.Empty;
    public string Status      { get; set; } = "OPEN";
}

public class CreateHAZOPStudyRequest
{
    public string FieldId    { get; set; } = string.Empty;
    public string StudyName  { get; set; } = string.Empty;
    public string Scope      { get; set; } = string.Empty;
}

public record AddNodeRequest(
    string NodeName,
    string Safeguard);

public record AddDeviationRequest(
    string GuideWord,
    string Deviation,
    string Consequence);

// ── KPI DTOs ──────────────────────────────────────────────────────────────────
public record HSEKPISet(
    double Tier1PSERate,
    double Tier2PSERate,
    double TRIR,
    double LTIF,
    double FatalityRate,
    double CAOnTimeRate,
    double BarrierDegradationRate,
    double HAZOPClosureRate,
    double ExposureHours,
    DateRangeFilter Period);

public record TierRateTrend(
    string Month,
    double Tier1Rate,
    double Tier2Rate);

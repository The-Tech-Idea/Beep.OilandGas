using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Compliance;

// ── Obligation Status / Type Constants ────────────────────────────────────────

public static class ObligationStatus
{
    public const string Pending          = "PENDING";
    public const string Submitted        = "SUBMITTED";
    public const string Overdue          = "OVERDUE";
    public const string Waived           = "WAIVED";
    public const string VarianceFlagged  = "VARIANCE_FLAGGED";
}

public static class ObligationType
{
    // USA
    public const string OnrrProductionReport = "ONRR_PRODUCTION_REPORT";
    public const string OnrrRoyaltyPayment   = "ONRR_ROYALTY_PAYMENT";
    public const string BseeSemsAudit        = "BSEE_SEMS_AUDIT";
    public const string BseeIncidentReport   = "BSEE_REPORT";
    public const string EpaGhgReportFlare    = "EPA_GHG_REPORT_FLARE";
    public const string EpaGhgReportVent     = "EPA_GHG_REPORT_VENT";

    // Canada
    public const string AerSt39Report        = "AER_ST39_REPORT";
    public const string AerIncidentReport    = "AER_INCIDENT_REPORT";
    public const string AlbertaCrownRoyalty  = "ALBERTA_CROWN_ROYALTY";
    public const string EcccNirReport        = "ECCC_NIR_REPORT";

    // International
    public const string NrcReport            = "NRC_REPORT";
    public const string OsparReport          = "OSPAR_REPORT";
    public const string EuEtsReport          = "EU_ETS_REPORT";
    public const string IogpGhgReport        = "IOGP_GHG_REPORT";
}

public static class PaymentType
{
    public const string Royalty        = "ROYALTY";
    public const string Fine           = "FINE";
    public const string EuEtsAllowance = "EU_ETS_ALLOWANCE";
    public const string Filing         = "FILING";
}

// ── DTOs ──────────────────────────────────────────────────────────────────────

public record CreateObligationRequest(
    string    FieldId,
    string    ObligType,
    DateTime  DueDate,
    string    Jurisdiction,
    string    Description,
    DateTime? ReportingPeriodStart = null,
    DateTime? ReportingPeriodEnd   = null,
    string?   CreatedByProcess     = null);

public record ObligationSummary(
    string  ObligationId,
    string  ObligType,
    string  Status,
    DateTime DueDate,
    bool    IsOverdue,
    int     DaysUntilDue,
    string  Jurisdiction,
    string? FulfillDate);

public class ObligationDetailModel : ModelEntityBase
{
    public string   ObligationId          { get; set; } = string.Empty;
    public string   FieldId               { get; set; } = string.Empty;
    public string   ObligType             { get; set; } = string.Empty;
    public string   ObligStatus           { get; set; } = ObligationStatus.Pending;
    public DateTime DueDate               { get; set; }
    public DateTime? FulfillDate          { get; set; }
    public string   JurisdictionCode      { get; set; } = string.Empty;
    public string   Description           { get; set; } = string.Empty;
    public DateTime? ReportingPeriodStart { get; set; }
    public DateTime? ReportingPeriodEnd   { get; set; }
    public string?  RegulatorBaId         { get; set; }
    public string?  CreatedByProcess      { get; set; }

    public List<ObligationPayment> Payments { get; set; } = new();
}

public record ObligationPayment(
    string   PaymentId,
    string   ObligationId,
    string   PaymentType,
    DateTime PaymentDate,
    string   PaymentCurrency,
    decimal  GrossAmt,
    decimal  ActualAmt,
    decimal  VarianceAmt,
    string?  PaymentNotes);

public record RecordPaymentRequest(
    string   PaymentType,
    DateTime PaymentDate,
    string   PaymentCurrency,
    decimal  GrossAmt,
    decimal  ActualAmt,
    string?  PaymentNotes = null);

public record ComplianceScoreCard(
    int    Year,
    int    TotalObligations,
    int    SubmittedOnTime,
    int    SubmittedLate,
    int    Overdue,
    int    Waived,
    double OnTimeRate);

// ── Royalty DTOs ──────────────────────────────────────────────────────────────

public record RoyaltySummary(
    string  FieldId,
    string  Jurisdiction,
    string  Period,
    double  ProductionVolume,
    string  ProductType,
    string  VolumeUnit,
    double  RoyaltyRate,
    double  GrossRevenue,
    decimal DueRoyalty,
    decimal? PaidRoyalty,
    decimal? VarianceAmount);

public record RoyaltyVariance(
    string  Period,
    string  ObligationId,
    decimal DueRoyalty,
    decimal PaidRoyalty,
    decimal Variance,
    bool    IsUnder);

// ── GHG DTOs ─────────────────────────────────────────────────────────────────

public record GHGEmissionReport(
    string                  FieldId,
    int                     Year,
    string                  Jurisdiction,
    double                  TotalCO2e,
    string                  Units,
    List<EmissionSourceLine> Sources,
    DateTime                GeneratedAt);

public record EmissionSourceLine(
    string SourceCategory,
    string Regulation,
    double ActivityVolume,
    string VolumeUnit,
    double EmissionFactor,
    string FactorUnit,
    double EmissionsCO2e);

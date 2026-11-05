using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Models.Data.WorkOrder;
/// <summary>Recorded activity evidence. Nullable metrics are absent, never inferred zero.</summary>
public sealed class ProductionInterventionCandidateDto
{
    public CompanyScope? Company { get; set; }
    public FieldSelectionReview? SelectionContext { get; set; }
    public Guid? LatestDecisionId { get; set; }
    public string Actor { get; set; } = string.Empty;
    public string FieldId { get; set; } = string.Empty;
    public string WellId { get; set; } = string.Empty;
    public string WellName { get; set; } = string.Empty;
    public string? SourcePreparer { get; set; }
    public string SourceRevision { get; set; } = string.Empty;
    public decimal? SourceObservationNumber { get; set; }
    public decimal? LatestObservationNumber { get; set; }
    public DateTime? SourceStartedAt { get; set; }
    public DateTimeOffset RetrievedAtUtc { get; set; }
    public string InterventionType { get; set; } = string.Empty;
    public string Problem { get; set; } = string.Empty;
    public double? DeferredBopd { get; set; }
    public int? EstDaysSinceOnset { get; set; }
    public string? Priority { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? LatestActivityType {get;set;}
    public string? LatestActivityRemark {get;set;}
    public string? LatestRecordedBy {get;set;}
    public double? EstCostUsd { get; set; }
    public string? WorkOrderId { get; set; }
    public string? AfeId { get; set; }
    public string? AfeNumber { get; set; }
    public string? AbandonmentId { get; set; }
    public string? ProcessInstanceId { get; set; }
}
public static class ProductionInterventionCandidateChecks
{
    public static void Require(IReadOnlyList<ProductionInterventionCandidateDto>? rows,CompanyScope company,string actor,string field)
    {
        if(string.IsNullOrWhiteSpace(actor) || string.IsNullOrWhiteSpace(field) || rows is null)
            throw new InvalidOperationException("Intervention source scope is unavailable.");
        var wells=new HashSet<string>(StringComparer.Ordinal);
        foreach(var row in rows)
        {
            if(row is null || row.Company!=company || row.Actor!=actor || row.FieldId!=field || string.IsNullOrWhiteSpace(row.WellId) || !wells.Add(row.WellId) ||
                !ProductionInterventionDecisionChecks.IsRevision(row.SourceRevision) || row.SourceObservationNumber is null || row.LatestObservationNumber is null || row.RetrievedAtUtc==default || row.RetrievedAtUtc.Offset!=TimeSpan.Zero)
                throw new InvalidOperationException("Intervention evidence does not match the current company, account, FIELD and recorded source.");
            if(row.SelectionContext is {} context){FieldSelectionReviewValidator.RequireReview(context,company,actor);if(context.FieldId!=field)throw new InvalidOperationException("The intervention FIELD review differs from its source.");}
            if(row.DeferredBopd is {} rate && (!double.IsFinite(rate) || rate<0) || row.EstDaysSinceOnset<0 ||
                row.EstCostUsd is {} cost && (!double.IsFinite(cost) || cost<0))
                throw new InvalidOperationException("Recorded intervention metrics are invalid.");
        }
    }
}

namespace TheTechIdea.Data.OilGas;

public sealed class ProductionInterventionDecisionRequest
{
    public Guid IntentId { get; set; }
    public FieldSelectionReview? Context { get; set; }
    public string FieldId { get; set; } = string.Empty;
    public string ExpectedSourceRevision { get; set; } = string.Empty;
    public string Jurisdiction { get; set; } = string.Empty;
    public string? Decision { get; set; }
    public string? Note { get; set; }
    public string? InterventionType { get; set; }
    public string? Problem { get; set; }
    public double? EstimatedCostUsd { get; set; }
    public bool CreateWorkOrder { get; set; }
    public bool LinkAfe { get; set; }
    public string? WorkOrderNumber { get; set; }
    public string? WorkOrderId { get; set; }
    public string? AfeId { get; set; }
}
public sealed class ProductionInterventionDecisionResult
{
    public Guid IntentId { get; set; }
    public string NativeReceiptRevision { get; set; } = string.Empty;
    public string DecisionSource { get; set; } = string.Empty;
    public decimal? DecisionObservationNumber { get; set; }
    public string ReviewedSourceRevision { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string FieldId { get; set; } = string.Empty;
    public string WellId { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty;
    public string? WorkOrderId { get; set; }
    public string? AfeId { get; set; }
    public string? AfeNumber { get; set; }
    public string? Message { get; set; }
}
public static class ProductionInterventionDecisionChecks
{
    public static string RecordedRemark(string? note,string? order,string? afe,string? number)
    {
        ProductionInterventionMarkers.RequireRationale(note);
        ProductionInterventionMarkers.RequireReference(order);ProductionInterventionMarkers.RequireReference(afe);ProductionInterventionMarkers.RequireReference(number);
        var parts=new List<string>();if(!string.IsNullOrWhiteSpace(note))parts.Add(note.Trim());
        if(!string.IsNullOrWhiteSpace(order))parts.Add($"WORK_ORDER_ID:{order}");
        if(!string.IsNullOrWhiteSpace(afe))parts.Add($"AFE_ID:{afe}");
        if(!string.IsNullOrWhiteSpace(number))parts.Add($"AFE_NUMBER:{number}");return string.Join(" ",parts);
    }
    public static void RequireRecordedOutcome(ProductionInterventionDecisionRequest request,ProductionInterventionDecisionResult? result,
        ProductionInterventionCandidateDto? recorded,CompanyScope company,string actor,string well)
    {
        RequireAcknowledgement(request,result,well);
        if(recorded is null)throw new InvalidOperationException("The recorded intervention outcome is unavailable.");
        ProductionInterventionCandidateChecks.Require([recorded],company,actor,request.FieldId);
        if(recorded.WellId!=well || recorded.SourceRevision==request.ExpectedSourceRevision || recorded.Status!=request.Decision ||
            recorded.LatestActivityType!=$"WORKOVER_{request.Decision}" || recorded.LatestRecordedBy!=actor ||
            recorded.LatestActivityRemark!=RecordedRemark(request.Note,result!.WorkOrderId,result.AfeId,result.AfeNumber) ||
            recorded.WorkOrderId!=result.WorkOrderId || recorded.AfeId!=result.AfeId || recorded.AfeNumber!=result.AfeNumber ||
            recorded.SourceObservationNumber is not {} original || recorded.LatestObservationNumber is not {} latest || latest<=original)
            throw new InvalidOperationException("Recorded activity does not confirm the original actor, rationale, decision and handoff. Review the outcome before retrying.");
    }
    public static void RequireAcknowledgement(ProductionInterventionDecisionRequest request,ProductionInterventionDecisionResult? result,string well)
    {
        if(result is null || !result.Success || result.ReviewedSourceRevision!=request.ExpectedSourceRevision || result.FieldId!=request.FieldId || result.WellId!=well || result.Decision!=request.Decision ||
            (request.IntentId!=Guid.Empty && (result.IntentId!=request.IntentId || !IsRevision(result.NativeReceiptRevision) ||
                !LeaseSummaryValidator.Identifier(result.DecisionSource) || result.DecisionObservationNumber is not {} nativeObservation || nativeObservation<=0 || decimal.Truncate(nativeObservation)!=nativeObservation)) ||
            (request.CreateWorkOrder && string.IsNullOrWhiteSpace(result.WorkOrderId)) ||
            (!string.IsNullOrWhiteSpace(request.WorkOrderId) && result.WorkOrderId!=request.WorkOrderId) ||
            (!request.CreateWorkOrder && string.IsNullOrWhiteSpace(request.WorkOrderId) && !string.IsNullOrWhiteSpace(result.WorkOrderId)) ||
            (!request.LinkAfe && (!string.IsNullOrWhiteSpace(result.AfeId) || !string.IsNullOrWhiteSpace(result.AfeNumber))) ||
            (request.LinkAfe && (string.IsNullOrWhiteSpace(result.AfeId) || !string.IsNullOrWhiteSpace(request.AfeId) && result.AfeId!=request.AfeId)))
            throw new InvalidOperationException("The returned result does not confirm the original intervention decision and requested handoff.");
    }
    public static bool IsIndependentApprover(string? preparer,string? actor)=>!string.IsNullOrWhiteSpace(preparer) && !string.IsNullOrWhiteSpace(actor) && !string.Equals(preparer,actor,StringComparison.Ordinal);
    public static void RequireIndependentApprover(string? preparer,string actor)
    {
        if(!IsIndependentApprover(preparer,actor))throw new UnauthorizedAccessException("Approval requires an identified source preparer and a different current actor.");
    }
    public static bool IsRevision(string? revision)=>revision is { Length:64 } && revision.All(c=>char.IsAsciiHexDigit(c));
    public static void RequireRevision(string expected,string current)
    {
        if(!IsRevision(expected) || !IsRevision(current) || !string.Equals(expected,current,StringComparison.Ordinal))
            throw new InvalidOperationException("Recorded intervention evidence changed. Reload and review it before deciding.");
    }
    public static string Require(ProductionInterventionDecisionRequest request,string field)
    {
        ArgumentNullException.ThrowIfNull(request);
        if(request.CreateWorkOrder && string.IsNullOrWhiteSpace(request.WorkOrderId) && string.IsNullOrWhiteSpace(request.WorkOrderNumber))
            throw new ArgumentException("An explicit recorded order number is required before creating an intervention work order.");
        if(string.IsNullOrWhiteSpace(field) || request.FieldId!=field)
            throw new InvalidOperationException("The decision FIELD differs from the original reviewed selection.");
        if(!IsRevision(request.ExpectedSourceRevision)) throw new ArgumentException("Original recorded source review is required.");
        var decision=request.Decision?.Trim().ToUpperInvariant();
        if(decision is not ("APPROVED" or "DEFERRED" or "REJECTED"))
            throw new ArgumentException("Choose an explicit approval, deferral or rejection.");
        if(string.IsNullOrWhiteSpace(request.Note)) throw new ArgumentException("A decision rationale is required.");
        ProductionInterventionMarkers.RequireRationale(request.Note);
        ProductionInterventionMarkers.RequireReference(request.WorkOrderId);ProductionInterventionMarkers.RequireReference(request.AfeId);
        if(request.EstimatedCostUsd is {} cost && (!double.IsFinite(cost) || cost<0))
            throw new ArgumentException("An estimate must be finite and nonnegative, with its declared USD unit.");
        if(decision!="APPROVED" && (request.CreateWorkOrder || request.LinkAfe || !string.IsNullOrWhiteSpace(request.WorkOrderId) || !string.IsNullOrWhiteSpace(request.AfeId)))
            throw new ArgumentException("Only an approved intervention can request a downstream handoff.");
        if(decision=="APPROVED" && request.CreateWorkOrder && string.IsNullOrWhiteSpace(request.WorkOrderId) && string.IsNullOrWhiteSpace(request.Jurisdiction))
            throw new ArgumentException("The applicable jurisdiction is required before creating an order.");
        return decision;
    }
}

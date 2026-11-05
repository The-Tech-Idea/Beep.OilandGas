namespace TheTechIdea.Data.OilGas;

public sealed record CompanyAccountingStepCommand(CompanyAccountingStepReview Review,string IntentId,string Notes,
    [property:System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] string? Action=null)
{
    public void Require()
    {
        ArgumentNullException.ThrowIfNull(Review);Review.Require(Review.Company,Review.ActorUserId,Review.InstanceId);
        var validAction=Action is null?Review.Eligible:CompanyAccountingSourceTypes.IsNativeDecision(Review.EntityType) && Action==CompanyAccountingSourceTypes.RejectionRoute(Review.EntityType)
            ? !Review.Eligible && Review.Eligibility==CompanyAccountingSourceTypes.RejectionEligibility(Review.EntityType) && Review.RequiresIndependentActor && Review.AccountingEvidenceRevision is not null
            : Action=="RETURN_CORRECTION_WORK" && !Review.Eligible && Review.Eligibility=="CORRECTION_WORK_EVIDENCE_REQUIRED" && Review.CorrectionEvidence?.Action==CompanyAccountingStepEvidence.InvoiceCorrectionReturn;
        if(!validAction || !Guid.TryParseExact(IntentId,"N",out _) || IntentId!=IntentId.ToLowerInvariant() ||
            string.IsNullOrWhiteSpace(Notes) || Notes.Length>2000 || Notes.Any(value=>char.IsControl(value) && value is not ('\n' or '\r' or '\t')))
            throw new ArgumentException("One eligible reviewed step, retained intent and bounded completion reason are required.");
    }
}
public sealed record CompanyAccountingStepReceipt(CompanyScope Company,string ActorUserId,string IntentId,string InstanceId,string StepInstanceId,
    string? NextStepInstanceId,string HistoryId,IReadOnlyList<string> CompletedTaskIds,IReadOnlyList<string> NextTaskIds,string RecordedRevision)
{
    public void Require(CompanyAccountingStepCommand command)
    {
        command.Require();var review=command.Review;
        static bool InvalidId(string id)=>string.IsNullOrWhiteSpace(id) || id.Length>128 || id.Any(char.IsControl);
        static bool Invalid(IReadOnlyList<string>? ids)=>ids is null || ids.Count>128 || ids.Any(id=>string.IsNullOrWhiteSpace(id) || id.Length>128 || id.Any(char.IsControl)) || ids.Distinct(StringComparer.Ordinal).Count()!=ids.Count;
        if(Company!=review.Company || ActorUserId!=review.ActorUserId || IntentId!=command.IntentId || InstanceId!=review.InstanceId || StepInstanceId!=review.StepInstanceId ||
            InvalidId(HistoryId) || NextStepInstanceId is not null && InvalidId(NextStepInstanceId) || Invalid(CompletedTaskIds) || CompletedTaskIds.Count==0 || Invalid(NextTaskIds) ||
            (NextStepInstanceId is null)!=(NextTaskIds.Count==0) || !ProcessDefinitionRevision.IsValid(RecordedRevision))
            throw new InvalidOperationException("The recorded step receipt does not confirm the original retained command.");
    }
}

using System.Text.Json;
using System.Text.Json.Serialization;
namespace TheTechIdea.Data.OilGas;

/// <summary>One immutable original preparation or independent decision, saved before its business write.</summary>
public sealed record RevenueDeliveryAcceptanceIntent(RevenueDeliveryAcceptancePreparation? Preparation,RevenueDeliveryAcceptanceSubmission? Submission)
{
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public CompanyDeliveryCorrectionSubmission? Correction{get;init;}
    [JsonIgnore] public string Kind=>Correction is not null?"CORRECTION":Preparation is null?"DECISION":"PREPARATION";
    [JsonIgnore] public RevenueDeliveryAcceptancePreparation Original=>Correction?.Correction.Preparation ?? Preparation ?? Submission!.Preparation.Preparation;
    [JsonIgnore] public CompanyScope Company=>Original.Company;
    [JsonIgnore] public string ActorUserId=>Submission?.Command.ActorUserId ??Original.ActorUserId;
    [JsonIgnore] public string IntentId=>Submission?.Command.IntentId ??Original.IntentId;
    [JsonIgnore] public string SourceRevision=>Correction?.Correction.Previous.Revision ??Submission?.Preparation.Revision ??Original.Source.Revision;
    [JsonIgnore] public string ScopeKey=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company,ActorUserId,Kind,Ticket=Original.Source.TicketId,SourceRevision}));
    public void Require()
    {
        if(new[]{Preparation is not null,Submission is not null,Correction is not null}.Count(value=>value)!=1)throw new InvalidOperationException("One complete original delivery command is required.");
        if(Correction is {} correction){correction.Require();return;}
        if(Preparation is {} preparation)preparation.Require();
        else
        {
            if(Submission?.Preparation is null || Submission.Command is null)throw new InvalidOperationException("The complete original delivery decision is required.");
            Submission.Command.Require(Submission.Preparation);
        }
    }
    public void RequireCurrent(RevenueDeliveryAcceptanceReadReview current)
    {
        Require();current.Require();if(current.IsHistorical)throw new InvalidOperationException("Historical receiving evidence is read-only and cannot confirm a current command.");Original.RequireCurrent(current.Source);
        if(Correction is {} correction)
        {
            var proof=RevenueDeliveryAcceptanceProof.RequireEnvelope(Company,current.Source.Ticket);
            if(JsonSerializer.Serialize(proof.Record)==JsonSerializer.Serialize(correction.Correction.Previous) && proof.Revision==correction.Correction.PreviousNativeRevision)return;
            if(JsonSerializer.Serialize(proof.Record.Preparation)!=JsonSerializer.Serialize(Original) || proof.Correction!=new RevenueDeliveryAcceptanceCorrectionOrigin(correction.Correction.Previous.Revision,correction.Correction.PreviousNativeRevision,correction.Correction.Reason) ||
                proof.PriorVersions is not {Count:>0} || JsonSerializer.Serialize(proof.PriorVersions[^1].Record)!=JsonSerializer.Serialize(correction.Correction.Previous))throw new InvalidOperationException("The original receiving correction predecessor or recorded outcome changed.");
            return;
        }
        if(Preparation is {} preparation)
        {
            if(current.Recorded is {} recorded && JsonSerializer.Serialize(recorded.Preparation)!=JsonSerializer.Serialize(preparation))throw new InvalidOperationException("A different delivery preparation is recorded.");
            return;
        }
        var existing=current.Recorded ??throw new InvalidOperationException("The original delivery preparation is unavailable.");
        var undecided=RevenueDeliveryAcceptanceRecord.Create(existing.Preparation,existing.PreparedAtUtc);
        if(JsonSerializer.Serialize(undecided)!=JsonSerializer.Serialize(Submission!.Preparation) || existing.Decision is {} decision && decision!=Submission.Command)
            throw new InvalidOperationException("The original undecided preparation or decision changed.");
    }
    public void RequireExecution()
    {
        Require();if(Correction is not null)return;if(Preparation is {} preparation)preparation.Facts.RequireQuantity(preparation.Source);
        else Submission!.Command.RequireExecution(Submission.Preparation);
    }
}

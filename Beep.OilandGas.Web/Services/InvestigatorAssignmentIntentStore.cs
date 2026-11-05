using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

public sealed record InvestigatorAssignmentIntent(InvestigatorAssignmentReview Review,RecordedIncidentParticipant Participant)
{
    public void Require()
    {
        if(Review?.Context is not {} context || Participant is null)throw new InvalidOperationException("The original investigator review and participant are required.");
        InvestigatorAssignmentReviewValidator.Require(Review,context.Company,context.ActorUserId,Review.IncidentId,context.FieldId!);
        _=InvestigatorAssignmentReviewValidator.Select(Review,Participant);
    }
    public string Revision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this));}
}

public interface IInvestigatorAssignmentIntentStore
{
    Task<InvestigatorAssignmentIntent?> ReadAsync(FieldSelectionReview context,string incident);
    Task RetainAsync(InvestigatorAssignmentIntent original);
    Task ClearAsync(InvestigatorAssignmentIntent original);
}

/// <summary>Protected tab-session recovery input; it is neither authorization nor proof of a business write.</summary>
public sealed class InvestigatorAssignmentIntentStore(ProtectedSessionStorage storage):IInvestigatorAssignmentIntentStore
{
    private readonly SemaphoreSlim gate=new(1,1);
    private static string Key(FieldSelectionReview context,string incident)=>"oilgas.hse.investigator."+Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new{context.Company,context.ActorUserId,context.Database,context.FieldId,incident})));
    public async Task<InvestigatorAssignmentIntent?> ReadAsync(FieldSelectionReview context,string incident)
    {
        FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);
        if(!LeaseSummaryValidator.Identifier(context.FieldId) || !LeaseSummaryValidator.Identifier(incident))throw new InvalidOperationException("An exact FIELD and incident are required.");
        var result=await storage.GetAsync<InvestigatorAssignmentIntent>(Key(context,incident));if(!result.Success)return null;
        var original=result.Value??throw new InvalidOperationException("The retained investigator submission is unavailable.");original.Require();var prior=original.Review.Context;
        if(prior.Company!=context.Company || prior.ActorUserId!=context.ActorUserId || prior.Database!=context.Database || prior.FieldId!=context.FieldId || original.Review.IncidentId!=incident)throw new InvalidOperationException("The retained investigator submission belongs to another source.");
        return original;
    }
    public async Task RetainAsync(InvestigatorAssignmentIntent original)
    {
        original.Require();await gate.WaitAsync();try
        {
            var context=original.Review.Context;var old=await ReadAsync(context,original.Review.IncidentId);
            if(old is not null && old.Revision()!=original.Revision())throw new InvalidOperationException("Review the original investigator submission before another change.");
            await storage.SetAsync(Key(context,original.Review.IncidentId),original);
            if((await ReadAsync(context,original.Review.IncidentId))?.Revision()!=original.Revision())throw new InvalidOperationException("The original investigator submission was not confirmed retained.");
        }finally{gate.Release();}
    }
    public async Task ClearAsync(InvestigatorAssignmentIntent original)
    {
        original.Require();await gate.WaitAsync();try
        {
            var context=original.Review.Context;var old=await ReadAsync(context,original.Review.IncidentId);
            if(old is not null && old.Revision()!=original.Revision())throw new InvalidOperationException("The retained investigator submission changed and was preserved.");
            await storage.DeleteAsync(Key(context,original.Review.IncidentId));
        }finally{gate.Release();}
    }
}

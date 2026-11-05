using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

public sealed record CorrectivePlanOriginal(CAPlanCreationReview Review,CreateCAPlanSubmission Submission)
{
    public void Require()
    {
        if(Review?.Context is not {} context || Submission?.Intent is not {} intent || Submission.Context!=context)throw new InvalidOperationException("The original plan review and submission are required.");
        InvestigatorAssignmentReviewValidator.Require(new(context,Review.IncidentId,Review.Revision,Review.Participants,null),context.Company,context.ActorUserId,Review.IncidentId,context.FieldId!);
        if(intent.PlanId==Guid.Empty || !LeaseSummaryValidator.Identifier(intent.PlanName) || !LeaseSummaryValidator.Identifier(intent.PlanNumber) || intent.ExpectedRevision!=Review.Revision || Review.ResponseTypes is null || Review.ResponseTypes.Count==0 || Review.ResponseTypes.Any(x=>!LeaseSummaryValidator.Identifier(x) || x=="STATUS") || Review.ResponseTypes.Distinct(StringComparer.Ordinal).Count()!=Review.ResponseTypes.Count || !Review.ResponseTypes.Contains(intent.ResponseType,StringComparer.Ordinal) || Review.Participants.Count(p=>p.PartyObservation==intent.PartyObservation && p.RoleObservation==intent.RoleObservation && p.BusinessAssociateId==intent.ResponsibleBaId)!=1)
            throw new InvalidOperationException("The plan must use the exact reviewed participant, response type and revision.");
    }
    public string Revision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this));}
}
public interface ICorrectivePlanIntentStore
{
    Task<CorrectivePlanOriginal?> ReadAsync(FieldSelectionReview context,string incident);
    Task RetainAsync(CorrectivePlanOriginal original);
    Task ClearAsync(CorrectivePlanOriginal original);
}
public sealed class CorrectivePlanIntentStore(ProtectedSessionStorage storage):ICorrectivePlanIntentStore
{
    private readonly SemaphoreSlim gate=new(1,1);
    private static string Key(FieldSelectionReview context,string incident)=>"oilgas.hse.corrective-plan."+Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new{context.Company,context.ActorUserId,context.Database,context.FieldId,incident})));
    public async Task<CorrectivePlanOriginal?> ReadAsync(FieldSelectionReview context,string incident)
    {
        FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);if(!LeaseSummaryValidator.Identifier(context.FieldId) || !LeaseSummaryValidator.Identifier(incident))throw new InvalidOperationException("An exact FIELD and incident are required.");
        var saved=await storage.GetAsync<CorrectivePlanOriginal>(Key(context,incident));if(!saved.Success)return null;var original=saved.Value??throw new InvalidOperationException("The retained plan submission is unavailable.");original.Require();var prior=original.Review.Context;
        if(prior.Company!=context.Company || prior.ActorUserId!=context.ActorUserId || prior.Database!=context.Database || prior.FieldId!=context.FieldId || original.Review.IncidentId!=incident)throw new InvalidOperationException("The retained plan belongs to another source.");return original;
    }
    public async Task RetainAsync(CorrectivePlanOriginal original)
    {
        original.Require();await gate.WaitAsync();try{var context=original.Review.Context;var prior=await ReadAsync(context,original.Review.IncidentId);if(prior is not null && prior.Revision()!=original.Revision())throw new InvalidOperationException("Recover the original plan before another submission.");await storage.SetAsync(Key(context,original.Review.IncidentId),original);if((await ReadAsync(context,original.Review.IncidentId))?.Revision()!=original.Revision())throw new InvalidOperationException("The original plan was not confirmed retained.");}finally{gate.Release();}
    }
    public async Task ClearAsync(CorrectivePlanOriginal original)
    {
        original.Require();await gate.WaitAsync();try{var context=original.Review.Context;var prior=await ReadAsync(context,original.Review.IncidentId);if(prior is not null && prior.Revision()!=original.Revision())throw new InvalidOperationException("The retained plan changed and was preserved.");await storage.DeleteAsync(Key(context,original.Review.IncidentId));}finally{gate.Release();}
    }
}

using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
namespace Beep.OilandGas.Web.Services;

public interface IPermitApplicationIntentStore
{
    Task<PermitApplicationCommand?> ReadAsync(FieldSelectionReview context,string applicationId);
    Task<PermitApplicationCommand?> ReadCreationAsync(FieldSelectionReview context);
    Task RetainAsync(PermitApplicationCommand original);
    Task ClearAsync(PermitApplicationCommand original);
    Task<PermitDocumentStageOriginal?> ReadDocumentAsync(FieldSelectionReview context,string applicationId);
    Task RetainDocumentAsync(PermitDocumentStageOriginal original);
    Task ClearDocumentAsync(PermitDocumentStageOriginal original);
    Task<PermitDocumentStageReceipt?> ReadStagedDocumentAsync(FieldSelectionReview context,string applicationId);
    Task RememberStagedDocumentAsync(FieldSelectionReview context,PermitDocumentStageReceipt receipt);
    Task ClearStagedDocumentAsync(FieldSelectionReview context,PermitDocumentStageReceipt receipt);
}

/// <summary>Protected tab input only. A changed binding/revision must not hide or replace an unresolved original.</summary>
public sealed class PermitApplicationIntentStore(ProtectedSessionStorage storage):IPermitApplicationIntentStore
{
    private readonly SemaphoreSlim gate=new(1,1);
    private static string Key(FieldSelectionReview context,string id)
    {
        PermitApplicationSourceReview.RequireTarget(id,context);
        return "oilgas.permits.original."+Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new{context.Company,context.ActorUserId,context.FieldId,ApplicationId=id})));
    }
    private static string CreationKey(FieldSelectionReview context)
    {
        PermitApplicationSourceReview.RequireTarget("new-permit",context);
        return "oilgas.permits.creation."+Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new{context.Company,context.ActorUserId,context.FieldId})));
    }
    private async Task<PermitApplicationCommand?> ReadAt(FieldSelectionReview context,string key,string? applicationId,bool creation)
    {
        var saved=await storage.GetAsync<PermitApplicationCommand>(key);if(!saved.Success)return null;
        var original=saved.Value??throw new InvalidOperationException("The protected permit original is unavailable.");original.Require();var before=original.Review.Context.Field;
        if(before.Company!=context.Company || before.ActorUserId!=context.ActorUserId || before.FieldId!=context.FieldId ||
            applicationId is not null && original.Review.ReadApplication().PERMIT_APPLICATION_ID!=applicationId || creation && original.Action!="CREATE_DRAFT")
            throw new InvalidOperationException("The protected permit original belongs to another company/account/FIELD/application or workflow.");
        return original;
    }
    public async Task<PermitApplicationCommand?> ReadAsync(FieldSelectionReview context,string applicationId)
    {
        var stored=await ReadAt(context,Key(context,applicationId),applicationId,false);if(stored is not null)return stored;
        var creation=await ReadCreationAsync(context);
        return creation?.Review.ReadApplication().PERMIT_APPLICATION_ID==applicationId?creation:null;
    }
    public Task<PermitApplicationCommand?> ReadCreationAsync(FieldSelectionReview context)=>ReadAt(context,CreationKey(context),null,true);
    private Task<PermitApplicationCommand?> ReadOriginal(PermitApplicationCommand original)=>original.Action=="CREATE_DRAFT"?
        ReadCreationAsync(original.Review.Context.Field):ReadAsync(original.Review.Context.Field,original.Review.ReadApplication().PERMIT_APPLICATION_ID);
    private static string OriginalKey(PermitApplicationCommand original)=>original.Action=="CREATE_DRAFT"?CreationKey(original.Review.Context.Field):
        Key(original.Review.Context.Field,original.Review.ReadApplication().PERMIT_APPLICATION_ID);
    public async Task RetainAsync(PermitApplicationCommand original)
    {
        original.Require();await gate.WaitAsync();try
        {
            if(await ReadDocumentAsync(original.Review.Context.Field,original.Review.ReadApplication().PERMIT_APPLICATION_ID)is not null)
                throw new InvalidOperationException("Recover the retained document upload before sending a lifecycle command.");
            var prior=await ReadOriginal(original);if(prior is not null && prior!=original)throw new InvalidOperationException("Recover the retained permit original before preparing another.");
            await storage.SetAsync(OriginalKey(original),original);
            if(await ReadOriginal(original)!=original)throw new InvalidOperationException("The original permit command was not confirmed saved in this tab.");
        }finally{gate.Release();}
    }
    public async Task ClearAsync(PermitApplicationCommand original)
    {
        original.Require();await gate.WaitAsync();try
        {
            var prior=await ReadOriginal(original);if(prior is not null && prior!=original)throw new InvalidOperationException("Another retained permit original was preserved.");
            await storage.DeleteAsync(OriginalKey(original));
            if(await ReadOriginal(original)is not null)throw new InvalidOperationException("The confirmed permit input could not be cleared.");
        }finally{gate.Release();}
    }
    private static string DocumentKey(FieldSelectionReview context,string id)=>Key(context,id)+".document";
    private static string CandidateKey(FieldSelectionReview context,string id)=>Key(context,id)+".staged";
    public async Task<PermitDocumentStageOriginal?> ReadDocumentAsync(FieldSelectionReview context,string id)
    {
        var saved=await storage.GetAsync<PermitDocumentStageOriginal>(DocumentKey(context,id));if(!saved.Success)return null;
        var original=saved.Value??throw new InvalidOperationException("The protected upload original is unconfirmed.");original.Require();
        var before=original.Review.Context.Field;
        if(before.Company!=context.Company || before.ActorUserId!=context.ActorUserId || before.FieldId!=context.FieldId || original.Review.ReadApplication().PERMIT_APPLICATION_ID!=id)
            throw new InvalidOperationException("The protected upload belongs to another account/company/FIELD/application.");return original;
    }
    public async Task RetainDocumentAsync(PermitDocumentStageOriginal original)
    {
        original.Require();var field=original.Review.Context.Field;var id=original.Review.ReadApplication().PERMIT_APPLICATION_ID;
        await gate.WaitAsync();try
        {
            if(await ReadAsync(field,id)is not null)throw new InvalidOperationException("Recover the retained lifecycle original first.");
            var prior=await ReadDocumentAsync(field,id);if(prior is not null && prior!=original)throw new InvalidOperationException("Recover the exact retained upload before choosing another.");
            var candidate=await ReadStagedDocumentAsync(field,id);if(candidate is not null && candidate.StageIntentId!=original.IntentId)throw new InvalidOperationException("Link the staged candidate before choosing another upload.");
            await storage.SetAsync(DocumentKey(field,id),original);
            if(await ReadDocumentAsync(field,id)!=original)throw new InvalidOperationException("The upload original was not confirmed saved.");
        }finally{gate.Release();}
    }
    public async Task ClearDocumentAsync(PermitDocumentStageOriginal original)
    {
        original.Require();var field=original.Review.Context.Field;var id=original.Review.ReadApplication().PERMIT_APPLICATION_ID;
        await gate.WaitAsync();try
        {
            var prior=await ReadDocumentAsync(field,id);if(prior is not null && prior!=original)throw new InvalidOperationException("Another upload original is preserved.");
            await storage.DeleteAsync(DocumentKey(field,id));if(await ReadDocumentAsync(field,id)is not null)throw new InvalidOperationException("The upload cleanup is unconfirmed.");
        }finally{gate.Release();}
    }
    public async Task<PermitDocumentStageReceipt?> ReadStagedDocumentAsync(FieldSelectionReview field,string id)
    {
        var saved=await storage.GetAsync<PermitDocumentStageReceipt>(CandidateKey(field,id));if(!saved.Success)return null;
        var receipt=saved.Value??throw new InvalidOperationException("The staged candidate is unconfirmed.");receipt.Require();
        if(receipt.Company!=field.Company || receipt.FieldId!=field.FieldId || receipt.ApplicationId!=id)throw new InvalidOperationException("The staged candidate belongs to another company/FIELD/application.");return receipt;
    }
    public async Task RememberStagedDocumentAsync(FieldSelectionReview field,PermitDocumentStageReceipt receipt)
    {
        receipt.Require();
        if(receipt.Company!=field.Company || receipt.FieldId!=field.FieldId || receipt.Core!=field.Database || receipt.UploadedBy!=field.ActorUserId)throw new InvalidOperationException("The staged candidate belongs to another upload context.");
        await gate.WaitAsync();try
        {
            var prior=await ReadStagedDocumentAsync(field,receipt.ApplicationId);if(prior is not null && prior!=receipt)throw new InvalidOperationException("Link the existing staged candidate before staging another.");
            await storage.SetAsync(CandidateKey(field,receipt.ApplicationId),receipt);
            if(await ReadStagedDocumentAsync(field,receipt.ApplicationId)!=receipt)throw new InvalidOperationException("The staged candidate was not confirmed saved.");
        }finally{gate.Release();}
    }
    public async Task ClearStagedDocumentAsync(FieldSelectionReview field,PermitDocumentStageReceipt receipt)
    {
        receipt.Require();await gate.WaitAsync();try
        {
            var prior=await ReadStagedDocumentAsync(field,receipt.ApplicationId);if(prior is not null && prior!=receipt)throw new InvalidOperationException("Another staged candidate is preserved.");
            await storage.DeleteAsync(CandidateKey(field,receipt.ApplicationId));if(await ReadStagedDocumentAsync(field,receipt.ApplicationId)is not null)throw new InvalidOperationException("The staged candidate cleanup is unconfirmed.");
        }finally{gate.Release();}
    }
}

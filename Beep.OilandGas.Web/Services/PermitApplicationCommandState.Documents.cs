using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Web.Services;

public sealed partial class PermitApplicationCommandState
{
    private PermitDocumentStageOriginal? pendingDocument;
    private readonly RetainedOriginalFeedback documentFeedback=new();
    public PermitDocumentStageReceipt? StagedDocument {get;private set;}
    public PermitDocumentStageResponse? DocumentOutcome {get;private set;}
    public bool DocumentAbsenceObserved {get;private set;}
    public PermitDocumentStageOriginal? VisibleDocumentOriginal=>pendingDocument is {} original && readable && original.Review.Context.Field==currentField?original:null;
    public bool HasPendingDocument=>pendingDocument is not null;
    public Guid? PendingDocumentReference=>pendingDocument?.IntentId;
    public string DocumentExplanation=>documentFeedback.Explanation;
    public bool CanRecoverDocument=>!disposed && Ready && readable && !Busy && VisibleDocumentOriginal is not null;
    public bool CanStageDocument=>CanPrepare("ATTACH_DOCUMENT") && StagedDocument is null;
    public bool CanResendDocument=>CanRecoverDocument && DocumentAbsenceObserved && Permission("ATTACH_DOCUMENT");
    public bool CanLinkDocument
    {
        get
        {
            if(!CanPrepare("ATTACH_DOCUMENT") || StagedDocument is not {} staged || Review is null)return false;
            try{new PermitAttachmentProposal(staged).Require(Review);return true;}catch(InvalidOperationException){return false;}
        }
    }
    public async Task StageDocumentAsync(PermitDocumentUploadProposal proposal,byte[] content,string reason,bool confirmed)
    {
        if(!CanStageDocument || Review is not {} review || !confirmed || string.IsNullOrWhiteSpace(reason) || reason.Trim().Length is <20 or >2000)throw new InvalidOperationException("Review the parent and explicit document metadata, confirm and give a20-2000 character reason.");
        proposal.RequireContent(content);using var op=Begin();if(await op.Authority("ATTACH_DOCUMENT")!=review.Context.Field)throw new InvalidOperationException("The reviewed upload context changed.");
        var original=new PermitDocumentStageOriginal(Guid.NewGuid(),review,proposal,reason.Trim());original.Require();
        pendingDocument=original;Review=null;Outcome=null;DocumentOutcome=null;DocumentAbsenceObserved=false;
        await SendDocumentAsync(op,original,content);
    }
    private async Task SendDocumentAsync(Operation op,PermitDocumentStageOriginal original,byte[] content)
    {
        original.Document.RequireContent(content);documentFeedback.BeginSaving();Changed?.Invoke();
        try
        {
            await originals.RetainDocumentAsync(original);op.Check();
            if(await op.Authority("ATTACH_DOCUMENT")!=original.Review.Context.Field)throw new InvalidOperationException("Return to the original upload context before sending.");
            documentFeedback.ConfirmSaved();documentFeedback.AttemptSubmission();Changed?.Invoke();
            var result=await client.StageDocumentAsync(new(original,content),op.Token);await AcceptDocumentAsync(op,original,result);
        }
        catch{if(op.IsCurrent)documentFeedback.Failed();throw;}
    }
    private async Task AcceptDocumentAsync(Operation op,PermitDocumentStageOriginal original,PermitDocumentStageResponse result)
    {
        result.Require(original);if(result.Receipt is not {} receipt)throw new InvalidOperationException("Document staging is unconfirmed; retain the original.");
        op.Check();if(await op.Authority()!=original.Review.Context.Field)throw new InvalidOperationException("Upload recovery context changed.");
        DocumentOutcome=result;documentFeedback.ConfirmReceiptAndBeginClear(true);Changed?.Invoke();
        await originals.RememberStagedDocumentAsync(original.Review.Context.Field,receipt);op.Check();StagedDocument=receipt;
        await originals.ClearDocumentAsync(original);op.Check();pendingDocument=null;Review=null;DocumentAbsenceObserved=false;
    }
    public async Task RecoverDocumentAsync()
    {
        if(!CanRecoverDocument)throw new InvalidOperationException("Read the retained upload in its original context first.");
        var original=pendingDocument!;using var op=Begin();if(await op.Authority()!=original.Review.Context.Field)throw new InvalidOperationException("Upload context changed.");
        var result=await client.RecoverDocumentStageAsync(original,op.Token);result.Require(original);op.Check();
        if(await op.Authority()!=original.Review.Context.Field)throw new InvalidOperationException("Upload recovery authority changed.");
        if(result.Receipt is null){if(DocumentOutcome?.Receipt is not null)throw new InvalidOperationException("Previously confirmed staging is now absent; retain the original for investigation.");DocumentAbsenceObserved=true;return;}
        await AcceptDocumentAsync(op,original,result);
    }
    public async Task ResendDocumentAsync(byte[] content)
    {
        if(!CanResendDocument)throw new InvalidOperationException("Recover first, then reselect the exact original file under current author authority.");
        var original=pendingDocument!;original.Document.RequireContent(content);using var op=Begin();DocumentAbsenceObserved=false;await SendDocumentAsync(op,original,content);
    }
    public Task LinkDocumentAsync(string reason,bool confirmed)
    {
        if(!CanLinkDocument)throw new InvalidOperationException("Review current source and the exact staged candidate before linking.");
        return RecordAsync("ATTACH_DOCUMENT",reason,confirmed,attachment:new(StagedDocument!));
    }
}

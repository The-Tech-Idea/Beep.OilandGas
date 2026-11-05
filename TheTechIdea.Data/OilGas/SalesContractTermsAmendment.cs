namespace TheTechIdea.Data.OilGas;

public sealed record SalesContractTermsAmendment(SalesContractTermsRecord Previous,string PreviousNativeRevision,
    SalesContractTermsPreparation Preparation,string Reason,
    [property:System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] SalesContractTermsAmendmentProcessOrigin? Process=null)
{
    public void Require()
    {
        Previous.Require();Preparation.Require();SalesContractTermsChecks.Text(Reason,4096);
        Process?.Require();
        var before=Previous.Preparation;var next=Preparation;
        if(Previous.Decision?.Decision!=SalesContractTermsDecision.Reject || !ProcessDefinitionRevision.IsValid(PreviousNativeRevision) ||
            next.Company!=before.Company || next.ActorUserId!=before.ActorUserId || next.IntentId==before.IntentId || next.Terms.Version==before.Terms.Version ||
            next.Source.Contract.TERMS_WORKFLOW_JSON is not null || next.Source.Contract.SALES_CONTRACT_ID!=before.Source.Contract.SALES_CONTRACT_ID ||
            next.Source.Source.FieldId!=before.Source.Source.FieldId || next.Source.TransactionId!=before.Source.TransactionId)
            throw new InvalidOperationException("A rejected original version, its native revision and the original preparer's distinct amendment are required.");
    }
    public static RevenueSalesContractReview NormalizeSource(RevenueSalesContractReview current,SalesContractTermsRecord previous)
    {
        previous.Require();var original=SalesContractTermsNativeProof.RestoreOriginalReview(current,previous);
        return RevenueSalesContractReview.Create(current.Source,current.TransactionId,original.Contract);
    }
}

public sealed record SalesContractTermsAmendmentOrigin(string PreviousRecordRevision,string PreviousNativeRevision,string Reason,
    [property:System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] SalesContractTermsAmendmentProcessOrigin? Process=null)
{
    public void Require()
    {
        SalesContractTermsChecks.Text(Reason,4096);
        Process?.Require();
        if(!ProcessDefinitionRevision.IsValid(PreviousRecordRevision) || !ProcessDefinitionRevision.IsValid(PreviousNativeRevision))
            throw new InvalidOperationException("The recorded amendment predecessor and reason are required.");
    }
}

public sealed record SalesContractTermsAmendmentProcessOrigin(string InstanceId,string ProcessRevision)
{
    public void Require()
    {
        SalesContractTermsChecks.Text(InstanceId,128);
        if(!ProcessDefinitionRevision.IsValid(ProcessRevision))throw new InvalidOperationException("The closed rejection-review process revision is required.");
    }
}

/// <summary>Flat preserved native evidence, excluding recursive history duplication.</summary>
public sealed record SalesContractTermsPriorVersion(SalesContractTermsRecord Record,string NativeRowJson,string Revision,
    [property:System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] SalesContractTermsAmendmentOrigin? Amendment=null);

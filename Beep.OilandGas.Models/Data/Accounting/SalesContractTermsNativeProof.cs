using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.Accounting;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.Accounting;

/// <summary>Canonical native evidence envelope; approval does not overwrite the recorded source terms.</summary>
public sealed record SalesContractTermsNativeProof(SalesContractTermsRecord Record,string NativeRowJson,string Revision,
    [property:System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<SalesContractTermsPriorVersion>? PriorVersions=null,
    [property:System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] SalesContractTermsAmendmentOrigin? Amendment=null)
{
    public const int MaximumLength=524288;
    public static string Create(SalesContractTermsRecord record,SALES_CONTRACT row,IReadOnlyList<SalesContractTermsPriorVersion>? priorVersions=null,SalesContractTermsAmendmentOrigin? amendment=null)
    {
        record.Require();var snapshot=Snapshot(row);
        var history=priorVersions is null?null:CompanyInventorySourceReview.Copy(priorVersions.ToArray());
        var proof=new SalesContractTermsNativeProof(CompanyInventorySourceReview.Copy(record),snapshot,Digest(record,snapshot,history,amendment),history,CompanyInventorySourceReview.Copy(amendment));
        proof.RequireHistory(record.Preparation.Company);
        var json=JsonSerializer.Serialize(proof);
        if(json.Length>MaximumLength)throw new InvalidOperationException("Commercial evidence exceeds the reviewed native column capacity.");
        return json;
    }
    public static SalesContractTermsNativeProof Require(CompanyScope company,SALES_CONTRACT row)
    {
        var json=row.TERMS_WORKFLOW_JSON;
        if(string.IsNullOrWhiteSpace(json) || json.Length>MaximumLength)throw new InvalidOperationException("Recorded commercial evidence is unavailable.");
        var proof=JsonSerializer.Deserialize<SalesContractTermsNativeProof>(json) ??throw new InvalidOperationException("Recorded commercial evidence is malformed.");
        proof.Record.Require();proof.RequireHistory(company);var original=proof.Record.Preparation.Source.Contract;
        var actor=proof.Record.Decision?.ActorUserId ??proof.Record.Preparation.ActorUserId;
        if(JsonSerializer.Serialize(proof)!=json || proof.Record.Preparation.Company!=company || row.SELLER_BA_ID!=company.BusinessAssociateId ||
            row.SALES_CONTRACT_ID!=original.SALES_CONTRACT_ID || row.ACTIVE_IND!="Y" || row.ROW_CHANGED_BY!=actor || row.ROW_CHANGED_DATE is null || row.ROW_CHANGED_DATE<(proof.Record.DecidedAtUtc ??proof.Record.PreparedAtUtc) ||
            Snapshot(row)!=proof.NativeRowJson || proof.Revision!=Digest(proof.Record,proof.NativeRowJson,proof.PriorVersions,proof.Amendment) || Business(row)!=Business(original))
            throw new InvalidOperationException("Native commercial evidence, original business facts or audit read-back changed.");
        return proof;
    }
    public static RevenueSalesContractReview RestoreOriginalReview(RevenueSalesContractReview current,SalesContractTermsRecord expected)
    {
        var proof=Require(expected.Preparation.Company,current.Contract);
        if(JsonSerializer.Serialize(proof.Record.Preparation)!=JsonSerializer.Serialize(expected.Preparation) || Business(current.Contract)!=Business(expected.Preparation.Source.Contract))
            throw new InvalidOperationException("The native contract belongs to a different frozen preparation.");
        return RevenueSalesContractReview.Create(current.Source,current.TransactionId,expected.Preparation.Source.Contract);
    }
    /// <summary>Reconstructs one preserved rejected version from the validated current native envelope.</summary>
    public static SalesContractTermsReadReview HistoricalReview(CompanyScope company,string actor,SALES_CONTRACT current,string preparationRevision)
    {
        var proof=Require(company,current);
        var entries=proof.PriorVersions?.ToArray() ?? [];
        var index=Array.FindIndex(entries,entry=>CompanyAccountingSourceTypes.CommercialPreparationRevision(entry.Record)==preparationRevision);
        SalesContractTermsRecord record;SALES_CONTRACT row;
        if(index<0)
        {
            if(CompanyAccountingSourceTypes.CommercialPreparationRevision(proof.Record)!=preparationRevision || proof.Record.Decision?.Decision!=SalesContractTermsDecision.Reject)
                throw new InvalidOperationException("The original rejected commercial version is not preserved in the current native evidence.");
            record=proof.Record;row=CompanyInventorySourceReview.Copy(current);
        }
        else
        {
            var entry=entries[index];record=entry.Record;
            row=JsonSerializer.Deserialize<SALES_CONTRACT>(entry.NativeRowJson)!;
            row.TERMS_WORKFLOW_JSON=Create(entry.Record,row,index==0?null:entries.Take(index).ToArray(),entry.Amendment);
        }
        var original=record.Preparation.Source;
        var captured=original.Source;
        var source=FieldRevenueSourceReview.Create(company,actor,captured.FieldId,captured.From,captured.Through,captured.BindingRevision,captured.Rows);
        var result=new SalesContractTermsReadReview(RevenueSalesContractReview.Create(source,original.TransactionId,row),CompanyInventorySourceReview.Copy(record));
        result.Require(company,actor,captured.FieldId,captured.From,captured.Through,original.TransactionId);
        return result;
    }
    private static string Snapshot(SALES_CONTRACT row)
    {row=CompanyInventorySourceReview.Copy(row);row.TERMS_WORKFLOW_JSON=null;return JsonSerializer.Serialize(row);}
    private static string Business(SALES_CONTRACT row)
    {row=CompanyInventorySourceReview.Copy(row);row.TERMS_WORKFLOW_JSON=null;row.ROW_CHANGED_BY=string.Empty;row.ROW_CHANGED_DATE=null;return JsonSerializer.Serialize(row);}
    private void RequireHistory(CompanyScope company)
    {
        if(PriorVersions is null){if(Amendment is not null)throw new InvalidOperationException("An amendment requires its preserved predecessor history.");return;}
        if(PriorVersions.Count is <1 or >16 || Amendment is null || PriorVersions.Any(row=>row is null))throw new InvalidOperationException("The bounded native commercial version history is incomplete.");
        var entries=PriorVersions.ToArray();
        for(var index=0;index<entries.Length;index++)
        {
            var entry=entries[index];entry.Record.Require();
            var row=JsonSerializer.Deserialize<SALES_CONTRACT>(entry.NativeRowJson) ??throw new InvalidOperationException("Historical native source is missing.");
            var actor=entry.Record.Decision?.ActorUserId ??entry.Record.Preparation.ActorUserId;
            if(entry.Record.Decision?.Decision!=SalesContractTermsDecision.Reject || entry.Record.Preparation.Company!=company ||
                Snapshot(row)!=entry.NativeRowJson || row.TERMS_WORKFLOW_JSON is not null || row.ACTIVE_IND!="Y" || row.SELLER_BA_ID!=company.BusinessAssociateId ||
                Business(row)!=Business(entry.Record.Preparation.Source.Contract) || row.ROW_CHANGED_BY!=actor || row.ROW_CHANGED_DATE is null || row.ROW_CHANGED_DATE<entry.Record.DecidedAtUtc ||
                entry.Revision!=Digest(entry.Record,entry.NativeRowJson,index==0?null:entries.Take(index).ToArray(),entry.Amendment))
                throw new InvalidOperationException("Historical commercial native evidence changed.");
            if(index==0){if(entry.Amendment is not null)throw new InvalidOperationException("The first commercial preparation cannot claim an amendment predecessor.");}
            else RequireSuccessor(entries[index-1],entry.Record,entry.Amendment);
        }
        RequireSuccessor(entries[^1],Record,Amendment);
        var records=entries.Select(row=>row.Record).Append(Record).ToArray();
        if(records.Select(row=>row.Preparation.IntentId).Distinct(StringComparer.Ordinal).Count()!=records.Length ||
            records.Select(row=>row.Preparation.Terms.Version).Distinct(StringComparer.Ordinal).Count()!=records.Length)
            throw new InvalidOperationException("Every commercial amendment requires a distinct version and intent.");
    }
    private static void RequireSuccessor(SalesContractTermsPriorVersion previous,SalesContractTermsRecord next,SalesContractTermsAmendmentOrigin? origin)
    {
        if(origin is null)throw new InvalidOperationException("The original amendment link is missing.");origin.Require();
        if(origin.PreviousRecordRevision!=previous.Record.Revision || origin.PreviousNativeRevision!=previous.Revision || next.PreparedAtUtc<previous.Record.DecidedAtUtc)
            throw new InvalidOperationException("The amendment predecessor or chronology changed.");
        new SalesContractTermsAmendment(previous.Record,previous.Revision,next.Preparation,origin.Reason,origin.Process).Require();
    }
    private static string Digest(SalesContractTermsRecord record,string snapshot,IReadOnlyList<SalesContractTermsPriorVersion>? priorVersions=null,SalesContractTermsAmendmentOrigin? amendment=null)=>
        ProcessDefinitionRevision.FromJson(priorVersions is null && amendment is null?JsonSerializer.Serialize(new{record,snapshot}):JsonSerializer.Serialize(new{record,snapshot,priorVersions,amendment}));
}

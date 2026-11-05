using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace TheTechIdea.Data.OilGas;

public sealed record RevenueDeliveryAcceptanceFacts(DateTime AcceptedAtUtc,string ReceivingBusinessAssociateId,string EvidenceReference,string Notes)
{
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public decimal? AcceptedQuantity{get;init;}
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? AcceptedQuantityUnit{get;init;}
    [JsonIgnore] public bool HasExplicitQuantity=>AcceptedQuantity is not null && AcceptedQuantityUnit is not null;
    public void Require(RevenueDeliverySourceReview source)
    {
        if(AcceptedAtUtc==default || AcceptedAtUtc.Kind!=DateTimeKind.Utc || string.IsNullOrWhiteSpace(source.Source.Contract.BUYER_BA_ID) || ReceivingBusinessAssociateId!=source.Source.Contract.BUYER_BA_ID)
            throw new InvalidOperationException("An explicit UTC acceptance timestamp and original receiving buyer are required.");
        SalesContractTermsChecks.Text(ReceivingBusinessAssociateId,128);SalesContractTermsChecks.Text(EvidenceReference,512);SalesContractTermsChecks.Text(Notes,4096);
        if((AcceptedQuantity is null)!=(AcceptedQuantityUnit is null))throw new InvalidOperationException("The accepted quantity and its exact certified unit must be recorded together.");
        if(HasExplicitQuantity)
        {
            SalesContractTermsChecks.Text(AcceptedQuantityUnit!,128);
            if(!source.HasCertifiedMeasuredQuantity || AcceptedQuantity<=0 || AcceptedQuantity>source.NetVolume || AcceptedQuantityUnit!=source.VolumeUnit)
                throw new InvalidOperationException("The accepted positive quantity must not exceed its certified measurement and must use the exact recorded unit.");
        }
    }
    public void RequireQuantity(RevenueDeliverySourceReview source){Require(source);if(!HasExplicitQuantity)throw new InvalidOperationException("Explicit accepted quantity and certified unit are required; historic unknown quantity cannot be inferred.");}
}
public sealed record RevenueDeliveryAcceptancePreparation(string IntentId,CompanyScope Company,string ActorUserId,RevenueDeliverySourceReview Source,RevenueDeliveryAcceptanceFacts Facts)
{
    public void Require()
    {
        PurchaseOrderOwnership.RequireScope(Company);SalesContractTermsChecks.Text(ActorUserId,128);RevenueDeliveryAcceptanceRecord.Intent(IntentId);
        if(Source is null || Facts is null || Source.Source.Source.Company!=Company || Source.Source.Source.ActorUserId!=ActorUserId || Source.Ticket.DELIVERY_WORKFLOW_JSON is not null)
            throw new InvalidOperationException("The original delivery preparation source is required.");
        RequireSource(Source);Facts.Require(Source);
    }
    public static void RequireSource(RevenueDeliverySourceReview source)
    {
        source.Require(source.Source);
        if(!source.HasCertifiedMeasuredQuantity || source.TicketDate is null)throw new InvalidOperationException("A recorded certified positive net quantity, explicit unit and ticket timestamp are required.");
        var terms=SalesContractTermsNativeProof.Require(source.Source.Source.Company,source.Source.Contract).Record;
        if(new SalesContractTermsReadReview(source.Source,terms).ReviewApplicability().Status!=SalesContractTermApplicability.ApprovedStoredDateAndCurrency)
            throw new InvalidOperationException("Independently approved applicable commercial terms are required.");
    }
    public void RequireCurrent(RevenueDeliverySourceReview current)
    {
        Require();current.Require(current.Source);
        if(Snapshot(Source)!=Snapshot(current))throw new InvalidOperationException("The original measured delivery or approved commercial source changed.");
    }
    private static string Snapshot(RevenueDeliverySourceReview value)
    {
        var source=value.Source.Source;var ticket=CompanyInventorySourceReview.Copy(value.Ticket);
        ticket.DELIVERY_WORKFLOW_JSON=null;ticket.ROW_CHANGED_BY=string.Empty;ticket.ROW_CHANGED_DATE=null;
        return JsonSerializer.Serialize(new{source.Company,source.FieldId,source.From,source.Through,source.BindingRevision,source.Rows,value.Source.TransactionId,value.Source.Contract,value.Allocation,Ticket=ticket});
    }
}
public enum RevenueDeliveryAcceptanceDecision { Approve,Reject }
public sealed record RevenueDeliveryAcceptanceDecisionCommand(string IntentId,CompanyScope Company,string ActorUserId,string PreparationRevision,RevenueDeliveryAcceptanceDecision Decision,string Reason)
{
    public void RequireExecution(RevenueDeliveryAcceptanceRecord record){Require(record);if(Decision==RevenueDeliveryAcceptanceDecision.Approve)record.Preparation.Facts.RequireQuantity(record.Preparation.Source);}
    public void Require(RevenueDeliveryAcceptanceRecord record)
    {
        record.Require();RevenueDeliveryAcceptanceRecord.Intent(IntentId);SalesContractTermsChecks.Text(ActorUserId,128);SalesContractTermsChecks.Text(Reason,4096);
        if(record.Decision is not null || Company!=record.Preparation.Company || PreparationRevision!=record.Revision || !Enum.IsDefined(Decision))throw new InvalidOperationException("An exact undecided delivery preparation is required.");
        RequireReviewer(record,ActorUserId);
    }
    public static void RequireReviewer(RevenueDeliveryAcceptanceRecord record,string actor)
    {
        record.Require();SalesContractTermsChecks.Text(actor,128);
        var source=record.Preparation.Source;
        var makers=new[]{record.Preparation.ActorUserId,source.Ticket.ROW_CREATED_BY,source.Allocation.ROW_CREATED_BY,source.Source.Source.Rows.Single(row=>row.REVENUE_TRANSACTION_ID==source.Source.TransactionId).ROW_CREATED_BY};
        if(makers.Any(string.IsNullOrWhiteSpace) || makers.Contains(actor,StringComparer.Ordinal))throw new InvalidOperationException("Complete maker identities and an independent delivery reviewer are required.");
    }
}
public sealed record RevenueDeliveryAcceptanceRecord(RevenueDeliveryAcceptancePreparation Preparation,DateTime PreparedAtUtc,RevenueDeliveryAcceptanceDecisionCommand? Decision,DateTime? DecidedAtUtc,string Revision)
{
    internal static void Intent(string intent)
    {if(!Guid.TryParseExact(intent,"N",out var id) || id==Guid.Empty || intent!=id.ToString("N"))throw new InvalidOperationException("An original delivery intent identity is required.");}
    public void Require()
    {
        Preparation.Require();
        if(PreparedAtUtc.Kind!=DateTimeKind.Utc || PreparedAtUtc==default || Preparation.Facts.AcceptedAtUtc>PreparedAtUtc || (Decision is null)!=(DecidedAtUtc is null) ||
            DecidedAtUtc is {} at && (at.Kind!=DateTimeKind.Utc || at<PreparedAtUtc) || Revision!=Digest(Preparation,PreparedAtUtc,Decision,DecidedAtUtc))throw new InvalidOperationException("Recorded delivery audit or revision is unconfirmed.");
        if(Decision is {} decision)decision.Require(Create(Preparation,PreparedAtUtc));
    }
    public static RevenueDeliveryAcceptanceRecord Create(RevenueDeliveryAcceptancePreparation preparation,DateTime at)
    {preparation=CompanyInventorySourceReview.Copy(preparation);var result=new RevenueDeliveryAcceptanceRecord(preparation,at,null,null,Digest(preparation,at,null,null));result.Require();return result;}
    public RevenueDeliveryAcceptanceRecord Decide(RevenueDeliveryAcceptanceDecisionCommand command,RevenueDeliverySourceReview current,DateTime at)
    {
        command.Require(this);Preparation.RequireCurrent(current);command=CompanyInventorySourceReview.Copy(command);
        var result=this with{Decision=command,DecidedAtUtc=at,Revision=Digest(Preparation,PreparedAtUtc,command,at)};result.Require();return result;
    }
    private static string Digest(RevenueDeliveryAcceptancePreparation preparation,DateTime at,RevenueDeliveryAcceptanceDecisionCommand? decision,DateTime? decided)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{preparation,at,decision,decided}));
}
public sealed record RevenueDeliveryAcceptanceSubmission(RevenueDeliveryAcceptanceRecord Preparation,RevenueDeliveryAcceptanceDecisionCommand Command);
public sealed record RevenueDeliveryAcceptanceReadReview(RevenueDeliverySourceReview Source,RevenueDeliveryAcceptanceRecord? Recorded)
{
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public RevenueDeliverySourceReview? CurrentSource{get;init;}
    [JsonIgnore] public bool IsHistorical=>CurrentSource is not null;
    public RevenueDeliveryBillingBasisReview ReviewBillingBasis()=>RevenueDeliveryBillingBasisReview.Create(this);
    [JsonIgnore] public decimal? ApprovedAcceptedQuantity{get{Require();return Recorded?.Decision?.Decision==RevenueDeliveryAcceptanceDecision.Approve?Recorded.Preparation.Facts.AcceptedQuantity:null;}}
    [JsonIgnore] public string? ApprovedQuantityUnit=>ApprovedAcceptedQuantity is not null?Recorded!.Preparation.Facts.AcceptedQuantityUnit:null;
    [JsonIgnore] public decimal? MeasuredQuantityOutsideApprovedAcceptance=>ApprovedAcceptedQuantity is {} quantity?Source.NetVolume-quantity:null;
    [JsonIgnore] public bool IsApprovedPartialAcceptance=>ApprovedAcceptedQuantity is {} quantity && quantity<Source.NetVolume;
    public void Require()
    {
        Source.Require(Source.Source);
        if(Source.Ticket.DELIVERY_WORKFLOW_JSON is null){if(Recorded is not null || CurrentSource is not null)throw new InvalidOperationException("Native delivery evidence is absent.");return;}
        var native=RevenueDeliveryAcceptanceProof.Require(Source.Source.Source.Company,Source.Ticket);
        if(JsonSerializer.Serialize(native)!=JsonSerializer.Serialize(Recorded))throw new InvalidOperationException("Native delivery evidence differs from its recorded result.");
        native.Preparation.RequireCurrent(Source);
        if(CurrentSource is {} current)
        {
            var currentRecord=RevenueDeliveryAcceptanceProof.Require(Source.Source.Source.Company,current.Ticket);
            if(currentRecord.Preparation.IntentId==native.Preparation.IntentId || current.Source.Source.ActorUserId!=Source.Source.Source.ActorUserId ||
                JsonSerializer.Serialize(new RevenueDeliveryAcceptanceReadReview(Source,Recorded))!=JsonSerializer.Serialize(RevenueDeliveryAcceptanceProof.HistoricalReview(Source.Source.Source.Company,Source.Source.Source.ActorUserId,current,CompanyAccountingSourceTypes.DeliveryPreparationRevision(native))))
                throw new InvalidOperationException("Historical receiving evidence must match its preserved native version and current source exactly.");
        }
    }
}
public sealed record RevenueDeliveryAcceptanceProof(RevenueDeliveryAcceptanceRecord Record,string NativeRowJson,string Revision)
{
    public const int MaximumLength=4194304;
    public const int MaximumPriorVersions=16;
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public IReadOnlyList<RevenueDeliveryAcceptancePriorVersion>? PriorVersions{get;init;}
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public RevenueDeliveryAcceptanceCorrectionOrigin? Correction{get;init;}
    private static string Native(RUN_TICKET row){row=CompanyInventorySourceReview.Copy(row);row.DELIVERY_WORKFLOW_JSON=null;return JsonSerializer.Serialize(row);}
    public static string Create(RevenueDeliveryAcceptanceRecord record,RUN_TICKET row)
        =>Create(record,row,null,null);
    public static string Create(RevenueDeliveryAcceptanceRecord record,RUN_TICKET row,IReadOnlyList<RevenueDeliveryAcceptancePriorVersion>? history,RevenueDeliveryAcceptanceCorrectionOrigin? correction)
    {
        record=CompanyInventorySourceReview.Copy(record);history=CompanyInventorySourceReview.Copy(history);correction=CompanyInventorySourceReview.Copy(correction);
        record.Require();var native=Native(row);var proof=new RevenueDeliveryAcceptanceProof(record,native,Digest(record,native,history,correction)){PriorVersions=history,Correction=correction};proof.RequireHistory(record.Preparation.Company);var json=JsonSerializer.Serialize(proof);
        if(json.Length>MaximumLength)throw new InvalidOperationException("Delivery evidence exceeds the native envelope bound.");return json;
    }
    public static RevenueDeliveryAcceptanceRecord Require(CompanyScope company,RUN_TICKET row)
        =>CompanyInventorySourceReview.Copy(RequireEnvelope(company,row).Record);
    public static RevenueDeliveryAcceptanceProof RequireEnvelope(CompanyScope company,RUN_TICKET row)
    {
        if(row.DELIVERY_WORKFLOW_JSON is not {} json || json.Length>MaximumLength)throw new InvalidOperationException("Complete recorded native delivery evidence is required.");
        var proof=JsonSerializer.Deserialize<RevenueDeliveryAcceptanceProof>(json) ??throw new InvalidOperationException("Native delivery evidence is malformed.");proof.Record.Require();proof.RequireHistory(company);
        var actor=proof.Record.Decision?.ActorUserId ??proof.Record.Preparation.ActorUserId;var at=proof.Record.DecidedAtUtc ??proof.Record.PreparedAtUtc;
        if(proof.Record.Preparation.Company!=company || proof.Record.Preparation.Source.TicketId!=row.RUN_TICKET_ID || row.ROW_CHANGED_BY!=actor || row.ROW_CHANGED_DATE is not {} changed || changed.Kind!=DateTimeKind.Utc || changed<at ||
            proof.NativeRowJson!=Native(row) || proof.Revision!=Digest(proof.Record,proof.NativeRowJson,proof.PriorVersions,proof.Correction))throw new InvalidOperationException("Native delivery row, company or audit changed.");
        var original=CompanyInventorySourceReview.Copy(proof.Record.Preparation.Source.Ticket);var current=CompanyInventorySourceReview.Copy(row);
        original.DELIVERY_WORKFLOW_JSON=current.DELIVERY_WORKFLOW_JSON=null;original.ROW_CHANGED_BY=current.ROW_CHANGED_BY=string.Empty;original.ROW_CHANGED_DATE=current.ROW_CHANGED_DATE=null;
        if(JsonSerializer.Serialize(original)!=JsonSerializer.Serialize(current))throw new InvalidOperationException("Native measured delivery changed.");return CompanyInventorySourceReview.Copy(proof);
    }
    private void RequireHistory(CompanyScope company)
    {
        if(PriorVersions is null){if(Correction is not null)throw new InvalidOperationException("A receiving correction requires preserved predecessor history.");return;}
        if(PriorVersions.Count is <1 or >MaximumPriorVersions || Correction is null || PriorVersions.Any(entry=>entry is null))throw new InvalidOperationException("Complete bounded receiving history is required.");
        var entries=PriorVersions.ToArray();
        for(var index=0;index<entries.Length;++index)
        {
            var entry=entries[index];entry.Record.Require();var row=JsonSerializer.Deserialize<RUN_TICKET>(entry.NativeRowJson) ??throw new InvalidOperationException("The historical native ticket is unavailable.");
            if(entry.Record.Decision?.Decision!=RevenueDeliveryAcceptanceDecision.Reject || entry.Record.Preparation.Company!=company || row.DELIVERY_WORKFLOW_JSON is not null || Native(row)!=entry.NativeRowJson ||
                row.ROW_CHANGED_BY!=entry.Record.Decision.ActorUserId || row.ROW_CHANGED_DATE is not {} changed || changed.Kind!=DateTimeKind.Utc || changed<entry.Record.DecidedAtUtc ||
                entry.Revision!=Digest(entry.Record,entry.NativeRowJson,index==0?null:entries.Take(index).ToArray(),entry.Correction))throw new InvalidOperationException("Historical receiving source, audit or revision changed.");
            var original=CompanyInventorySourceReview.Copy(entry.Record.Preparation.Source.Ticket);original.ROW_CHANGED_BY=row.ROW_CHANGED_BY=string.Empty;original.ROW_CHANGED_DATE=row.ROW_CHANGED_DATE=null;
            if(JsonSerializer.Serialize(original)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("Historical receiving measurement changed.");
            if(index==0){if(entry.Correction is not null)throw new InvalidOperationException("The first receiving record cannot claim a correction predecessor.");}
            else RequireSuccessor(entries[index-1],entry.Record,entry.Correction);
        }
        RequireSuccessor(entries[^1],Record,Correction);
        var records=entries.Select(entry=>entry.Record).Append(Record).ToArray();
        var intents=records.SelectMany(record=>record.Decision is {} decision?new[]{record.Preparation.IntentId,decision.IntentId}:new[]{record.Preparation.IntentId}).ToArray();
        if(intents.Distinct(StringComparer.Ordinal).Count()!=intents.Length)throw new InvalidOperationException("Every receiving preparation and decision in correction history requires a distinct original identity.");
    }
    private static void RequireSuccessor(RevenueDeliveryAcceptancePriorVersion previous,RevenueDeliveryAcceptanceRecord next,RevenueDeliveryAcceptanceCorrectionOrigin? origin)
    {
        if(origin is null)throw new InvalidOperationException("The original receiving correction link is missing.");origin.Require();
        if(origin.PreviousRecordRevision!=previous.Record.Revision || origin.PreviousNativeRevision!=previous.Revision || next.PreparedAtUtc<previous.Record.DecidedAtUtc)throw new InvalidOperationException("The receiving correction predecessor or chronology changed.");
        new RevenueDeliveryAcceptanceCorrection(previous.Record,previous.Revision,next.Preparation,origin.Reason).Require();
    }
    public static RevenueDeliveryAcceptanceReadReview HistoricalReview(CompanyScope company,string actor,RevenueDeliverySourceReview current,string preparationRevision)
    {
        current.Require(current.Source);var proof=RequireEnvelope(company,current.Ticket);proof.Record.Preparation.RequireCurrent(current);
        var entries=proof.PriorVersions?.ToArray() ??[];var index=Array.FindIndex(entries,entry=>CompanyAccountingSourceTypes.DeliveryPreparationRevision(entry.Record)==preparationRevision);
        RevenueDeliveryAcceptanceRecord record;RUN_TICKET row;
        if(index<0){if(CompanyAccountingSourceTypes.DeliveryPreparationRevision(proof.Record)!=preparationRevision)throw new InvalidOperationException("The exact receiving preparation is not preserved.");record=proof.Record;row=CompanyInventorySourceReview.Copy(current.Ticket);}
        else{var entry=entries[index];record=entry.Record;row=JsonSerializer.Deserialize<RUN_TICKET>(entry.NativeRowJson)!;row.DELIVERY_WORKFLOW_JSON=Create(record,row,index==0?null:entries.Take(index).ToArray(),entry.Correction);}
        var original=record.Preparation.Source;var captured=original.Source.Source;
        var field=FieldRevenueSourceReview.Create(company,actor,captured.FieldId,captured.From,captured.Through,captured.BindingRevision,captured.Rows);
        var review=new RevenueDeliveryAcceptanceReadReview(RevenueDeliverySourceReview.Create(RevenueSalesContractReview.Create(field,original.Source.TransactionId,original.Source.Contract),original.Allocation,row),CompanyInventorySourceReview.Copy(record));review.Require();return review;
    }
    private static string Digest(RevenueDeliveryAcceptanceRecord record,string native,IReadOnlyList<RevenueDeliveryAcceptancePriorVersion>? history=null,RevenueDeliveryAcceptanceCorrectionOrigin? correction=null)=>
        ProcessDefinitionRevision.FromJson(history is null && correction is null?JsonSerializer.Serialize(new{record,native}):JsonSerializer.Serialize(new{record,native,history,correction}));
}

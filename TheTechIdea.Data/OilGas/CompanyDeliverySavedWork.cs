using System.Text.Json;
using System.Text.Json.Serialization;
namespace TheTechIdea.Data.OilGas;

/// <summary>Owned original command plus independently reviewed current source; discovery grants no execution authority.</summary>
public sealed record CompanyDeliverySavedWorkReview(CompanyScope Company,string ActorUserId,string BindingRevision,DELIVERY_ACCEPTANCE_INTENT Entry,RevenueDeliveryAcceptanceReadReview? Current,string Revision)
{
    [JsonIgnore] public string Status
    {
        get
        {
            var command=Entry.Restore(Company,ActorUserId,Entry.SCOPE_KEY);
            if(Current is null)return "RECONCILIATION_REQUIRED";
            Current.Require();
            try{command.RequireCurrent(Current);}catch(InvalidOperationException){return "RECONCILIATION_REQUIRED";}
            if(command.Preparation is not null && Current.Recorded is null && !command.Original.Facts.HasExplicitQuantity ||
                command.Submission?.Command.Decision==RevenueDeliveryAcceptanceDecision.Approve && Current.Recorded?.Decision is null && !command.Original.Facts.HasExplicitQuantity)return "RECONCILIATION_REQUIRED";
            if(command.Correction is not null)return JsonSerializer.Serialize(Current.Recorded!.Preparation)==JsonSerializer.Serialize(command.Original)?"RECORDED":"SAVED_PENDING";
            return command.Preparation is not null?Current.Recorded is not null?"RECORDED":"SAVED_PENDING":Current.Recorded?.Decision is not null?"RECORDED":"SAVED_PENDING";
        }
    }
    public void Require(CompanyScope company,string actor,string intent)
    {
        APInvoiceOwnership.RequireScope(company);RequireIntentId(intent);
        if(Company!=company || ActorUserId!=actor || string.IsNullOrWhiteSpace(actor) || actor.Length>128 || actor.Any(char.IsControl) ||
            string.IsNullOrWhiteSpace(BindingRevision) || BindingRevision.Length>512 || BindingRevision.Any(char.IsControl) || Entry is null || Entry.DELIVERY_ACCEPTANCE_INTENT_ID!=intent)
            throw new InvalidOperationException("The original company/account saved delivery review is required.");
        var command=Entry.Restore(company,actor,Entry.SCOPE_KEY);var source=command.Original.Source.Source.Source;
        if(Current is not null)
        {
            Current.Require();Current.Source.Source.Require(company,actor,source.FieldId,source.From,source.Through,command.Original.Source.Source.TransactionId,Current.Source.Source.Source.Revision);
            if(Current.Source.Source.Source.BindingRevision!=BindingRevision)throw new InvalidOperationException("The reviewed delivery source binding changed.");
        }
        _=Status;if(Revision!=Digest(Company,ActorUserId,BindingRevision,Entry,Current))throw new InvalidOperationException("The complete saved delivery review changed.");
    }
    public static void RequireIntentId(string intent)
    {if(string.IsNullOrWhiteSpace(intent) || !Guid.TryParseExact(intent,"N",out _))throw new ArgumentException("The exact original delivery intent UUID is required.");}
    public static CompanyDeliverySavedWorkReview Create(CompanyScope company,string actor,string binding,DELIVERY_ACCEPTANCE_INTENT entry,RevenueDeliveryAcceptanceReadReview? current)
    {
        var result=new CompanyDeliverySavedWorkReview(company,actor,binding,CompanyInventorySourceReview.Copy(entry),CompanyInventorySourceReview.Copy(current),Digest(company,actor,binding,entry,current));
        result.Require(company,actor,entry.DELIVERY_ACCEPTANCE_INTENT_ID);return result;
    }
    private static string Digest(CompanyScope company,string actor,string binding,DELIVERY_ACCEPTANCE_INTENT entry,RevenueDeliveryAcceptanceReadReview? current)
        =>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=company,ActorUserId=actor,BindingRevision=binding,Entry=entry,Current=current}));
}
/// <summary>Totals are owned saved entries; outcome counts are limited to this complete independently reviewed page.</summary>
public sealed record CompanyDeliverySavedWorkPage(CompanyScope Company,string ActorUserId,string BindingRevision,IReadOnlyList<CompanyDeliverySavedWorkReview> Rows,int PageNumber,int PageSize,int TotalRecords,string Revision)
{
    public int TotalPages=>TotalRecords==0?0:(int)(((long)TotalRecords+PageSize-1)/PageSize);
    public bool HasNextPage=>(long)PageNumber*PageSize<TotalRecords;
    public bool HasPreviousPage=>PageNumber>1;
    public void Require(CompanyScope company,string actor,int page,int size)
    {
        DELIVERY_ACCEPTANCE_INTENT.RequirePage(page,size);APInvoiceOwnership.RequireScope(company);
        if(Company!=company || ActorUserId!=actor || string.IsNullOrWhiteSpace(actor) || actor.Length>128 || actor.Any(char.IsControl) || PageNumber!=page || PageSize!=size || TotalRecords<0 || page>Math.Max(1,TotalPages) || Rows is null ||
            string.IsNullOrWhiteSpace(BindingRevision) || BindingRevision.Length>512 || BindingRevision.Any(char.IsControl))throw new InvalidOperationException("The complete original saved delivery page is required.");
        var expected=(int)Math.Min(size,Math.Max(0L,TotalRecords-(long)(page-1)*size));
        if(Rows.Count!=expected || Rows.Any(row=>row?.Entry is null) || Rows.Sum(row=>(long)(row.Entry.COMMAND_JSON?.Length ??0))>DELIVERY_ACCEPTANCE_INTENT.MaximumSetCharacters ||
            Rows.Select(row=>row.Entry.SCOPE_KEY).Distinct(StringComparer.Ordinal).Count()!=Rows.Count || Rows.Select(row=>row.Entry.DELIVERY_ACCEPTANCE_INTENT_ID).Distinct(StringComparer.Ordinal).Count()!=Rows.Count ||
            !Rows.Select(row=>row.Entry.SCOPE_KEY).SequenceEqual(Rows.Select(row=>row.Entry.SCOPE_KEY).Order(StringComparer.Ordinal)))throw new InvalidOperationException("The complete bounded saved delivery page membership changed.");
        foreach(var row in Rows){row.Require(company,actor,row.Entry.DELIVERY_ACCEPTANCE_INTENT_ID);if(row.BindingRevision!=BindingRevision)throw new InvalidOperationException("The delivery page binding changed.");}
        if(Revision!=Digest(Company,ActorUserId,BindingRevision,Rows,page,size,TotalRecords))throw new InvalidOperationException("The complete delivery page evidence changed.");
    }
    public static CompanyDeliverySavedWorkPage Create(CompanyScope company,string actor,string binding,IReadOnlyList<CompanyDeliverySavedWorkReview> rows,int page,int size,int total)
    {
        var result=new CompanyDeliverySavedWorkPage(company,actor,binding,CompanyInventorySourceReview.Copy(rows),page,size,total,Digest(company,actor,binding,rows,page,size,total));result.Require(company,actor,page,size);return result;
    }
    private static string Digest(CompanyScope company,string actor,string binding,IReadOnlyList<CompanyDeliverySavedWorkReview> rows,int page,int size,int total)
        =>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=company,ActorUserId=actor,BindingRevision=binding,Rows=rows,PageNumber=page,PageSize=size,TotalRecords=total}));
}

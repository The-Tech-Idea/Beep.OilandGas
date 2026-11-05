using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace TheTechIdea.Data.OilGas;
public sealed record PurchaseOrderInventoryLinkHistoryReview(CompanyScope Company,string ActorUserId,string OrderId,string LineId,IReadOnlyList<PO_INVENTORY_LINK_HISTORY> Entries,string Revision,DateTimeOffset RetrievedAtUtc)
{
    public const int MaximumEntries=256;
    public static string EntryRevision(PO_INVENTORY_LINK_HISTORY entry)
    {var copy=CompanyInventorySourceReview.Copy(entry);copy.RECORDED_REVISION="";return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static PO_LINE_ITEM RequireEntry(CompanyScope company,string order,string line,PO_INVENTORY_LINK_HISTORY entry)
    {
        static bool GuidId(string? value)=>Guid.TryParseExact(value,"N",out var id) && id!=Guid.Empty && id.ToString("N")==value;
        static bool Text(string? value,int bound)=>!string.IsNullOrWhiteSpace(value) && value.Length<=bound && !value.Any(char.IsControl);
        if(entry is null || entry.OWNER_TENANT_ID!=company.TenantId || entry.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || entry.PURCHASE_ORDER_ID!=order || entry.PO_LINE_ITEM_ID!=line ||
            !GuidId(entry.PO_INVENTORY_LINK_HISTORY_ID) || !GuidId(entry.CYCLE_INTENT_ID) || entry.SEQUENCE_NUMBER<1 || !ProcessDefinitionRevision.IsValid(entry.SOURCE_REVISION) ||
            !Text(entry.BINDING_REVISION,512) || !Text(entry.REASON,512) || !Text(entry.ROW_CREATED_BY,128) || entry.ROW_CREATED_BY=="SYSTEM" || entry.ACTIVE_IND!="Y" ||
            entry.ROW_CREATED_DATE is not {} created || created.Kind!=DateTimeKind.Utc || created==default || entry.ROW_CHANGED_DATE is not {} changed || changed.Kind!=DateTimeKind.Utc || changed<created || entry.ROW_CHANGED_BY!=entry.ROW_CREATED_BY ||
            entry.ARCHIVED_LINE_JSON is not {Length:>0} || entry.ARCHIVED_LINE_JSON.Length>4194304 || entry.RECORDED_REVISION!=EntryRevision(entry))throw new InvalidOperationException("Retained cycle ownership, intent, bound or actor/UTC audit is unconfirmed.");
        PO_LINE_ITEM? archived;try{archived=JsonSerializer.Deserialize<PO_LINE_ITEM>(entry.ARCHIVED_LINE_JSON);}catch(JsonException){throw new InvalidOperationException("Archived cycle line is malformed.");}
        if(archived is null || archived.PURCHASE_ORDER_ID!=order || archived.PO_LINE_ITEM_ID!=line || JsonSerializer.Serialize(archived)!=entry.ARCHIVED_LINE_JSON || archived.INVENTORY_LINK_HISTORY_REVISION!=entry.PREVIOUS_REVISION ||
            archived.ROW_CHANGED_DATE is not {} prior || created<prior)throw new InvalidOperationException("The archived original line or predecessor audit changed.");
        _=PurchaseOrderInventoryLinkProposalProof.Require(company,archived);return archived;
    }
    public void Require(CompanyScope company,string actor,string order,string line,string? currentHead)
    {
        PurchaseOrderOwnership.RequireScope(company);
        if(Company!=company || ActorUserId!=actor || string.IsNullOrWhiteSpace(actor) || OrderId!=order || LineId!=line || Entries is null || Entries.Count>MaximumEntries || RetrievedAtUtc==default || RetrievedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("Complete original company cycle history is required.");
        string? predecessor=null,previousId=null;DateTime? previousAt=null;var ids=new HashSet<string>(StringComparer.Ordinal);var intents=new HashSet<string>(StringComparer.Ordinal);
        for(var index=0;index<Entries.Count;index++)
        {
            var entry=Entries[index];RequireEntry(company,order,line,entry);
            if(entry.SEQUENCE_NUMBER!=index+1 || entry.PREVIOUS_REVISION!=predecessor || entry.PREVIOUS_ENTRY_ID!=previousId || !ids.Add(entry.PO_INVENTORY_LINK_HISTORY_ID) || !intents.Add(entry.CYCLE_INTENT_ID) || previousAt is {} utc && entry.ROW_CREATED_DATE<utc)
                throw new InvalidOperationException("History has a missing, repeated, reordered or changed predecessor.");
            predecessor=entry.RECORDED_REVISION;previousId=entry.PO_INVENTORY_LINK_HISTORY_ID;previousAt=entry.ROW_CHANGED_DATE;
        }
        if(currentHead!=predecessor || Revision!=Digest(company,order,line,Entries))throw new InvalidOperationException("The active line cursor or complete history revision changed.");
    }
    public static PurchaseOrderInventoryLinkHistoryReview Create(CompanyScope company,string actor,string order,string line,IReadOnlyList<PO_INVENTORY_LINK_HISTORY> entries,string? head)
    {
        var frozen=Array.AsReadOnly(entries.OrderBy(row=>row.SEQUENCE_NUMBER).Select(CompanyInventorySourceReview.Copy).ToArray());
        var result=new PurchaseOrderInventoryLinkHistoryReview(company,actor,order,line,frozen,Digest(company,order,line,frozen),DateTimeOffset.UtcNow);result.Require(company,actor,order,line,head);return result;
    }
    private static string Digest(CompanyScope company,string order,string line,IReadOnlyList<PO_INVENTORY_LINK_HISTORY> entries)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{company,order,line,entries}));
}

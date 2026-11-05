using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.Accounting;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.Accounting;

/// <summary>Original stock preimages and prior movement hashes, stored separately from movement history.</summary>
public sealed class INVENTORY_STOCK_INTENT:ModelEntityBase
{
    [Key,Required,MaxLength(32)] public string INVENTORY_STOCK_INTENT_ID {get;set;}="";
    [Required,MaxLength(64)] public string OWNER_TENANT_ID {get;set;}="";
    [Required,MaxLength(128)] public string OWNER_COMPANY_BA_ID {get;set;}="";
    [Required,MaxLength(128)] public string PURCHASE_ORDER_ID {get;set;}="";
    [Required,MaxLength(128)] public string PO_RECEIPT_ID {get;set;}="";
    [Required,MaxLength(128)] public string INVENTORY_ITEM_ID {get;set;}="";
    [Required,MaxLength(16384)] public string COMMAND_JSON {get;set;}="";
    [Required,MaxLength(4194304)] public string ORIGINAL_PURCHASE_JSON {get;set;}="";
    [Required,MaxLength(16384)] public string ORIGINAL_ITEM_JSON {get;set;}="";
    [Required,MaxLength(1048576)] public string PRIOR_MOVEMENT_MANIFEST_JSON {get;set;}="";
    [Required,MaxLength(64)] public string MOVEMENT_REVISION {get;set;}="";
    [Required,MaxLength(64)] public string RECORDED_REVISION {get;set;}="";

    public sealed record MovementReference(string Id,string Revision);
    public static string MovementRevision(INVENTORY_TRANSACTION row)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(row));
    public static string EntryRevision(INVENTORY_STOCK_INTENT entry)
    {var copy=CompanyInventorySourceReview.Copy(entry);copy.RECORDED_REVISION="";return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static INVENTORY_STOCK_INTENT Create(PurchaseReceiptStockSubmission submission,INVENTORY_TRANSACTION movement)
    {
        submission.Require();var command=submission.Command;var proof=PurchaseReceiptStockMovement.Read(command.Company,movement);
        if(proof!=command.Movement(submission.Review,movement) || submission.Review.Source.Inventory.Transactions.Any(row=>row.INVENTORY_TRANSACTION_ID==command.IntentId))
            throw new InvalidOperationException("Only an original stock movement can retain its preimages.");
        var entry=new INVENTORY_STOCK_INTENT{INVENTORY_STOCK_INTENT_ID=command.IntentId,OWNER_TENANT_ID=command.Company.TenantId,
            OWNER_COMPANY_BA_ID=command.Company.BusinessAssociateId,PURCHASE_ORDER_ID=command.OrderId,PO_RECEIPT_ID=command.ReceiptId,INVENTORY_ITEM_ID=proof.ItemId,
            COMMAND_JSON=JsonSerializer.Serialize(command),ORIGINAL_PURCHASE_JSON=JsonSerializer.Serialize(submission.Review.Source.Purchase),ORIGINAL_ITEM_JSON=JsonSerializer.Serialize(submission.Review.Source.Inventory.Item),
            PRIOR_MOVEMENT_MANIFEST_JSON=JsonSerializer.Serialize(submission.Review.Source.Inventory.Transactions.OrderBy(row=>row.INVENTORY_TRANSACTION_ID,StringComparer.Ordinal).Select(row=>new MovementReference(row.INVENTORY_TRANSACTION_ID,MovementRevision(row)))),
            MOVEMENT_REVISION=MovementRevision(movement),PPDM_GUID=command.IntentId,ACTIVE_IND="Y",ROW_CREATED_BY=command.ActorUserId,ROW_CHANGED_BY=command.ActorUserId,ROW_CREATED_DATE=movement.ROW_CREATED_DATE,ROW_CHANGED_DATE=movement.ROW_CREATED_DATE};
        entry.RECORDED_REVISION=EntryRevision(entry);_=entry.Restore(command.Company,command.ActorUserId,submission.Review.Source.Inventory.Transactions.Concat([movement]).ToArray());return entry;
    }
    public PurchaseReceiptStockSubmission Restore(CompanyScope company,string actor,IReadOnlyList<INVENTORY_TRANSACTION> completeHistory)
    {
        RequireHeader(company,actor);
        var command=Read<PurchaseReceiptStockCommand>(COMMAND_JSON,16384);var purchase=Read<PurchaseOrderSourceReview>(ORIGINAL_PURCHASE_JSON,4194304);
        var item=Read<INVENTORY_ITEM>(ORIGINAL_ITEM_JSON,16384);var manifest=Read<MovementReference[]>(PRIOR_MOVEMENT_MANIFEST_JSON,1048576);
        if(command.Company!=company || command.ActorUserId!=actor || command.IntentId!=INVENTORY_STOCK_INTENT_ID || command.OrderId!=PURCHASE_ORDER_ID ||
            command.ReceiptId!=PO_RECEIPT_ID || item.INVENTORY_ITEM_ID!=INVENTORY_ITEM_ID || completeHistory is null || completeHistory.Any(row=>row is null || row.INVENTORY_ITEM_ID!=INVENTORY_ITEM_ID) ||
            completeHistory.Select(row=>row.INVENTORY_TRANSACTION_ID).Distinct(StringComparer.Ordinal).Count()!=completeHistory.Count ||
            manifest.Any(value=>value is null || string.IsNullOrWhiteSpace(value.Id) || value.Id.Length>128 || value.Id.Any(char.IsControl) || value.Id==command.IntentId || !ProcessDefinitionRevision.IsValid(value.Revision)) ||
            manifest.Select(value=>value.Id).Distinct(StringComparer.Ordinal).Count()!=manifest.Length || !manifest.SequenceEqual(manifest.OrderBy(value=>value.Id,StringComparer.Ordinal)))
            throw new InvalidOperationException("Original stock command, source or prior movement manifest is malformed.");
        var prior=manifest.Select(reference=>
        {
            var row=completeHistory.SingleOrDefault(value=>value.INVENTORY_TRANSACTION_ID==reference.Id);
            if(row is null || MovementRevision(row)!=reference.Revision)throw new InvalidOperationException("Original prior stock movement changed or is missing.");
            return CompanyInventorySourceReview.Copy(row);
        }).ToArray();
        var review=PurchaseReceiptStockReview.Create(PurchaseReceiptInventoryReview.Create(purchase,CompanyInventorySourceReview.Create(company,actor,item,prior),command.ReceiptId,command.BindingRevision));
        var submission=new PurchaseReceiptStockSubmission(command,review);submission.Require();
        var movement=completeHistory.SingleOrDefault(row=>row.INVENTORY_TRANSACTION_ID==command.IntentId) ??throw new InvalidOperationException("Original recorded stock movement is missing.");
        var proof=PurchaseReceiptStockMovement.Read(company,movement);
        if(MOVEMENT_REVISION!=MovementRevision(movement) || proof!=command.Movement(review,movement) || ROW_CREATED_DATE!=movement.ROW_CREATED_DATE ||
            movement.UNIT_COST is not null || movement.TOTAL_COST is not null || movement.REFERENCE_NUMBER!=command.ReceiptId || movement.NOTES!=command.Reason)
            throw new InvalidOperationException("Original stock recovery record and physical movement disagree.");
        return CompanyInventorySourceReview.Copy(submission);
    }
    public IReadOnlyList<string> RequiredMovementIds(CompanyScope company,string actor)
    {
        RequireHeader(company,actor);var manifest=Read<MovementReference[]>(PRIOR_MOVEMENT_MANIFEST_JSON,1048576);
        var ids=manifest.Select(value=>value?.Id).Append(INVENTORY_STOCK_INTENT_ID).ToArray();
        if(ids.Any(id=>string.IsNullOrWhiteSpace(id) || id.Length>128 || id.Any(char.IsControl)) || ids.Distinct(StringComparer.Ordinal).Count()!=ids.Length)
            throw new InvalidOperationException("Original historical movement identities are malformed.");return ids!;
    }
    private void RequireHeader(CompanyScope company,string actor)
    {
        PurchaseOrderOwnership.RequireScope(company);
        if(OWNER_TENANT_ID!=company.TenantId || OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || ACTIVE_IND!="Y" || ROW_CREATED_BY!=actor ||
            ROW_CHANGED_BY!=actor || ROW_CREATED_DATE is null || ROW_CREATED_DATE.Value.Kind!=DateTimeKind.Utc || ROW_CHANGED_DATE!=ROW_CREATED_DATE ||
            !ProcessDefinitionRevision.IsValid(RECORDED_REVISION) || RECORDED_REVISION!=EntryRevision(this))
            throw new InvalidOperationException("Original stock recovery ownership, actor, recording audit or revision is unconfirmed.");
    }
    private static T Read<T>(string json,int max)
    {
        if(string.IsNullOrWhiteSpace(json) || json.Length>max)throw new InvalidOperationException("Bounded original stock recovery evidence is unavailable.");
        T? result;try{result=JsonSerializer.Deserialize<T>(json);}catch(JsonException error){throw new InvalidOperationException("Original stock recovery evidence is malformed.",error);}
        if(result is null || JsonSerializer.Serialize(result)!=json)throw new InvalidOperationException("Original stock recovery evidence is not canonical.");return result;
    }
}

/// <summary>Recorded original effect alongside current owned source; later source changes do not imply replay or current effect confirmation.</summary>
public sealed record PurchaseReceiptStockRecovery(INVENTORY_STOCK_INTENT Entry,PurchaseOrderSourceReview? CurrentPurchase,
    CompanyInventorySourceReview? CurrentInventory,string Revision,IReadOnlyList<INVENTORY_TRANSACTION>? NativeHistory=null)
{
    public bool CurrentPurchaseAvailable=>CurrentPurchase is not null;
    public bool CurrentInventoryAvailable=>CurrentInventory is not null;
    public PurchaseReceiptStockSubmission OriginalSubmission()=>Entry.Restore(new(Entry.OWNER_TENANT_ID,Entry.OWNER_COMPANY_BA_ID),Entry.ROW_CREATED_BY,
        CurrentInventory?.Transactions ?? NativeHistory ??throw new InvalidOperationException("Original native movement evidence is unavailable."));
    public void Require(CompanyScope company,string actor,string intentId)
    {
        if(Entry is null || Entry.INVENTORY_STOCK_INTENT_ID!=intentId || CurrentInventory is not null && NativeHistory is not null)
            throw new InvalidOperationException("Complete original stock recovery evidence is unavailable.");
        CurrentPurchase?.Require(company,actor,Entry.PURCHASE_ORDER_ID);CurrentInventory?.Require(company,actor,Entry.INVENTORY_ITEM_ID);
        _=Entry.Restore(company,actor,CurrentInventory?.Transactions ?? NativeHistory ??throw new InvalidOperationException("Original native history is unavailable."));
        if(Revision!=Digest(Entry,CurrentPurchase,CurrentInventory,NativeHistory))throw new InvalidOperationException("Current stock recovery evidence changed.");
    }
    public bool CurrentMatchesOriginalEffect()
    {
        Require(new(Entry.OWNER_TENANT_ID,Entry.OWNER_COMPANY_BA_ID),Entry.ROW_CREATED_BY,Entry.INVENTORY_STOCK_INTENT_ID);
        if(CurrentPurchase is null || CurrentInventory is null)return false;var submission=OriginalSubmission();
        return submission.MatchesResult(PurchaseReceiptInventoryReview.Create(CurrentPurchase,CurrentInventory,Entry.PO_RECEIPT_ID,submission.Command.BindingRevision));
    }
    public static PurchaseReceiptStockRecovery Create(INVENTORY_STOCK_INTENT entry,PurchaseOrderSourceReview? purchase,CompanyInventorySourceReview? inventory,IReadOnlyList<INVENTORY_TRANSACTION>? history=null)
    {
        entry=CompanyInventorySourceReview.Copy(entry);purchase=purchase is null?null:CompanyInventorySourceReview.Copy(purchase);inventory=inventory is null?null:CompanyInventorySourceReview.Copy(inventory);history=history is null?null:CompanyInventorySourceReview.Copy(history);
        var result=new PurchaseReceiptStockRecovery(entry,purchase,inventory,Digest(entry,purchase,inventory,history),history);result.Require(new(entry.OWNER_TENANT_ID,entry.OWNER_COMPANY_BA_ID),entry.ROW_CREATED_BY,entry.INVENTORY_STOCK_INTENT_ID);return result;
    }
    private static string Digest(INVENTORY_STOCK_INTENT entry,PurchaseOrderSourceReview? purchase,CompanyInventorySourceReview? inventory,IReadOnlyList<INVENTORY_TRANSACTION>? history)=>
        ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{entry.RECORDED_REVISION,PurchaseRevision=purchase?.Revision,InventoryRevision=inventory?.Revision,NativeHistory=history,entry.OWNER_TENANT_ID,entry.OWNER_COMPANY_BA_ID,entry.ROW_CREATED_BY}));
}

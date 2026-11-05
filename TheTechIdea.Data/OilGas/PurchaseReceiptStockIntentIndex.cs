using System.Text.Json;
namespace TheTechIdea.Data.OilGas;

public sealed record PurchaseReceiptStockIntentSummary(PurchaseReceiptStockCommand Command,string ItemId,DateTime RecordedAtUtc,
    string EntryRevision,string CurrentRecoveryRevision,bool CurrentMatchesOriginalEffect,bool CurrentPurchaseAvailable,bool CurrentInventoryAvailable);

public sealed record PurchaseReceiptStockIntentIndex(CompanyScope Company,string ActorUserId,IReadOnlyList<PurchaseReceiptStockIntentSummary> Entries,string Revision)
{
    public const int MaximumEntries=200;
    public void Require(CompanyScope company,string actor)
    {
        PurchaseOrderOwnership.RequireScope(company);
        if(Company!=company || string.IsNullOrWhiteSpace(actor) || ActorUserId!=actor || Entries is null || Entries.Count>MaximumEntries ||
            Entries.Any(row=>row is null || row.Command is null) || Entries.Select(row=>row.Command.IntentId).Distinct(StringComparer.Ordinal).Count()!=Entries.Count ||
            !Entries.Select(row=>row.Command.IntentId).SequenceEqual(Entries.Select(row=>row.Command.IntentId).Order(StringComparer.Ordinal)))
            throw new InvalidOperationException("Complete original-account stock intent index is unavailable or ambiguous.");
        foreach(var row in Entries)
        {
            var command=row.Command;
            if(row.CurrentMatchesOriginalEffect && (!row.CurrentPurchaseAvailable || !row.CurrentInventoryAvailable) || command.Company!=company || command.ActorUserId!=actor || !Guid.TryParseExact(command.IntentId,"N",out var intent) || intent==Guid.Empty || intent.ToString("N")!=command.IntentId ||
                !Enum.IsDefined(command.Kind) || command.Quantity<=0 || row.RecordedAtUtc.Kind!=DateTimeKind.Utc ||
                !ProcessDefinitionRevision.IsValid(row.EntryRevision) || !ProcessDefinitionRevision.IsValid(row.CurrentRecoveryRevision) || !ProcessDefinitionRevision.IsValid(command.ExpectedReviewRevision) ||
                new[]{command.OrderId,command.ReceiptId,row.ItemId,command.QuantityUnit}.Any(value=>string.IsNullOrWhiteSpace(value) || value.Length>128 || value.Any(char.IsControl)) ||
                new[]{command.BindingRevision,command.Reason}.Any(value=>string.IsNullOrWhiteSpace(value) || value.Length>512 || value.Any(char.IsControl)) ||
                command.Kind==PurchaseReceiptStockMovementKind.Receive && command.OriginalReceiptTransactionId is not null ||
                command.Kind==PurchaseReceiptStockMovementKind.Return && (string.IsNullOrWhiteSpace(command.OriginalReceiptTransactionId) || command.OriginalReceiptTransactionId.Length>128 || command.OriginalReceiptTransactionId.Any(char.IsControl)))
                throw new InvalidOperationException("Saved stock intent index entry is malformed.");
        }
        if(Revision!=Digest(company,actor,Entries))throw new InvalidOperationException("Saved stock intent index changed.");
    }
    public static PurchaseReceiptStockIntentIndex Create(CompanyScope company,string actor,IEnumerable<PurchaseReceiptStockRecovery> recoveries)
    {
        var entries=recoveries.Select(recovery=>{
            recovery.Require(company,actor,recovery.Entry.INVENTORY_STOCK_INTENT_ID);
            return new PurchaseReceiptStockIntentSummary(recovery.OriginalSubmission().Command,recovery.Entry.INVENTORY_ITEM_ID,recovery.Entry.ROW_CREATED_DATE!.Value,
                recovery.Entry.RECORDED_REVISION,recovery.Revision,recovery.CurrentMatchesOriginalEffect(),recovery.CurrentPurchaseAvailable,recovery.CurrentInventoryAvailable);
        }).OrderBy(row=>row.Command.IntentId,StringComparer.Ordinal).ToArray();
        var result=new PurchaseReceiptStockIntentIndex(company,actor,entries,Digest(company,actor,entries));result.Require(company,actor);return CompanyInventorySourceReview.Copy(result);
    }
    private static string Digest(CompanyScope company,string actor,IReadOnlyList<PurchaseReceiptStockIntentSummary> entries)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=company,ActorUserId=actor,Entries=entries}));
}

using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class AccountingServiceClient
{
    public async Task<CompanyMaterialReservationQueue> GetCompanyMaterialReservationsAsync(string fieldId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldId);var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the original warehouse company/account.");if(!selected.Permissions.Contains("Inventory.View",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current Inventory.View is required.");var result=await _apiClient.GetAsync<CompanyMaterialReservationQueue>("api/accounting/traditional/inventory/company/work-order-reservations?fieldId="+Uri.EscapeDataString(fieldId))??throw new InvalidOperationException("Complete current FIELD reservation queue is unavailable.");if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original warehouse company/account changed.");result.Require(selected.Scope,selected.UserId,fieldId);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<PurchaseReceiptStockIntentPage> GetCompanyStockRecoveryPageAsync(int pageNumber,int pageSize)
    {
        PurchaseReceiptStockIntentPage.RequireRequest(pageNumber,pageSize);var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original stock company and account.");
        var result=await _apiClient.GetAsync<PurchaseReceiptStockIntentPage>($"api/accounting/traditional/inventory/company/stock-intents/pages/{pageNumber}/{pageSize}") ??throw new InvalidOperationException("Complete original stock page is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during stock page discovery.");
        result.Require(original.Selection.Scope,original.Selection.UserId,pageNumber,pageSize);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<PurchaseReceiptStockIntentIndex> GetCompanyStockRecoveryIndexAsync()
    {
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original stock company and account.");
        var result=await _apiClient.GetAsync<PurchaseReceiptStockIntentIndex>("api/accounting/traditional/inventory/company/stock-intents") ??throw new InvalidOperationException("Complete saved stock discovery is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during saved stock discovery.");
        result.Require(original.Selection.Scope,original.Selection.UserId);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<PurchaseReceiptStockRecovery> GetPurchaseReceiptStockRecoveryAsync(string intentId)
    {
        if(!Guid.TryParseExact(intentId,"N",out var intent) || intent==Guid.Empty || intent.ToString("N")!=intentId)
            throw new InvalidOperationException("A canonical original stock intent is required.");
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original stock company and account.");
        var result=await _apiClient.GetAsync<PurchaseReceiptStockRecovery>("api/accounting/traditional/inventory/company/stock-intents/"+Uri.EscapeDataString(intentId))
            ??throw new InvalidOperationException("Complete original stock recovery evidence is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during original stock recovery.");
        result.Require(original.Selection.Scope,original.Selection.UserId,intentId);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<PurchaseReceiptInventoryReview> RecordPurchaseReceiptStockAsync(PurchaseReceiptStockSubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);submission=CompanyInventorySourceReview.Copy(submission);submission.Require();
        var original=_apiClient.CompanySnapshot;if(original.Selection is null || original.Selection.Scope!=submission.Command.Company || original.Selection.UserId!=submission.Command.ActorUserId)
            throw new InvalidOperationException("Select the original company and account for stock movement reconciliation.");
        var result=await _apiClient.PostAsync<PurchaseReceiptStockSubmission,PurchaseReceiptInventoryReview>("api/accounting/traditional/inventory/company/purchase-orders/"+Uri.EscapeDataString(submission.Command.OrderId)+"/receipts/"+Uri.EscapeDataString(submission.Command.ReceiptId)+"/stock-movement",submission)
            ??throw new InvalidOperationException("Complete stock movement confirmation is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during stock recording.");
        submission.RequireResult(result);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<PurchaseReceiptStockReview> GetPurchaseReceiptStockReviewAsync(string orderId,string receiptId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderId);ArgumentException.ThrowIfNullOrWhiteSpace(receiptId);
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original company and account for receipt stock review.");
        var result=await _apiClient.GetAsync<PurchaseReceiptStockReview>("api/accounting/traditional/inventory/company/purchase-orders/"+Uri.EscapeDataString(orderId)+"/receipts/"+Uri.EscapeDataString(receiptId)+"/stock-review")
            ??throw new InvalidOperationException("Complete receipt stock review is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during receipt stock review.");
        result.Require(original.Selection.Scope,original.Selection.UserId,orderId,receiptId);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<PurchaseOrderInventoryLinkCycleResult> GetInventoryLinkHistoryAsync(string orderId,string lineId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderId);ArgumentException.ThrowIfNullOrWhiteSpace(lineId);
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original company and account for cycle history.");
        var result=await _apiClient.GetAsync<PurchaseOrderInventoryLinkCycleResult>("api/accounting/traditional/inventory/company/purchase-orders/"+Uri.EscapeDataString(orderId)+"/lines/"+Uri.EscapeDataString(lineId)+"/inventory-link-history")
            ??throw new InvalidOperationException("Complete owned cycle history is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during history review.");
        result.Require(original.Selection.Scope,original.Selection.UserId,orderId,lineId);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<PurchaseOrderInventoryLinkCycleReview> GetInventoryLinkCycleReviewAsync(string orderId,string lineId,PurchaseOrderInventoryLinkCycleAction action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderId);ArgumentException.ThrowIfNullOrWhiteSpace(lineId);
        if(!Enum.IsDefined(action))throw new InvalidOperationException("Choose an explicit cycle action.");
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original cycle company and account.");
        var result=await _apiClient.GetAsync<PurchaseOrderInventoryLinkCycleReview>("api/accounting/traditional/inventory/company/purchase-orders/"+Uri.EscapeDataString(orderId)+"/lines/"+Uri.EscapeDataString(lineId)+"/inventory-link-cycle-review/"+action)
            ??throw new InvalidOperationException("Original cycle review is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during cycle review.");
        result.Require(original.Selection.Scope,original.Selection.UserId,orderId,lineId);if(result.Action!=action)throw new InvalidOperationException("The reviewed cycle action changed.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<PurchaseOrderInventoryLinkCycleResult> ReopenInventoryLinkAsync(PurchaseOrderInventoryLinkCycleSubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);ArgumentNullException.ThrowIfNull(submission.Command);ArgumentNullException.ThrowIfNull(submission.Review);submission=CompanyInventorySourceReview.Copy(submission);
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original cycle company and account.");
        submission.Require(original.Selection.Scope,original.Selection.UserId,submission.Command.OrderId,submission.Command.LineId);
        var result=await _apiClient.PostAsync<PurchaseOrderInventoryLinkCycleSubmission,PurchaseOrderInventoryLinkCycleResult>("api/accounting/traditional/inventory/company/purchase-orders/"+Uri.EscapeDataString(submission.Command.OrderId)+"/lines/"+Uri.EscapeDataString(submission.Command.LineId)+"/inventory-link-cycle",submission)
            ??throw new InvalidOperationException("Original cycle outcome is unconfirmed. Retain the original intent.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during cycle recording.");
        submission.RequireResult(result);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<PurchaseOrderInventoryLinkResolution> GetInventoryLinkResolutionAsync(PurchaseOrderInventoryLinkProposalSubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);ArgumentNullException.ThrowIfNull(submission.Command);ArgumentNullException.ThrowIfNull(submission.Review);submission=CompanyInventorySourceReview.Copy(submission);
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original preparer company and account.");
        submission.Require(original.Selection.Scope,original.Selection.UserId,submission.Command.OrderId,submission.Command.LineId);
        var route="api/accounting/traditional/inventory/company/purchase-orders/"+Uri.EscapeDataString(submission.Command.OrderId)+"/lines/"+Uri.EscapeDataString(submission.Command.LineId)+"/inventory-link-resolution";
        var result=await _apiClient.PostAsync<PurchaseOrderInventoryLinkProposalSubmission,PurchaseOrderInventoryLinkResolution>(route,submission) ??throw new InvalidOperationException("Original preparation resolution is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during resolution review.");
        result.Require(submission);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<PurchaseOrderInventoryLinkDecisionReview> GetInventoryLinkDecisionReviewAsync(string orderId,string lineId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderId);ArgumentException.ThrowIfNullOrWhiteSpace(lineId);
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original reviewer company and account.");
        var route="api/accounting/traditional/inventory/company/purchase-orders/"+Uri.EscapeDataString(orderId)+"/lines/"+Uri.EscapeDataString(lineId)+"/inventory-link-decision-review";
        var review=await _apiClient.GetAsync<PurchaseOrderInventoryLinkDecisionReview>(route) ??throw new InvalidOperationException("Independent inventory-link decision evidence is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during decision review.");
        review.Require(original.Selection.Scope,original.Selection.UserId,orderId,lineId,review.Target?.Item?.INVENTORY_ITEM_ID ?? "");return CompanyInventorySourceReview.Copy(review);
    }
    public async Task<PO_LINE_ITEM> RecordInventoryLinkDecisionAsync(PurchaseOrderInventoryLinkDecisionSubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);ArgumentNullException.ThrowIfNull(submission.Command);ArgumentNullException.ThrowIfNull(submission.Review);submission=CompanyInventorySourceReview.Copy(submission);
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original reviewer company and account.");
        submission.Require(original.Selection.Scope,original.Selection.UserId,submission.Command.OrderId,submission.Command.LineId);
        var route="api/accounting/traditional/inventory/company/purchase-orders/"+Uri.EscapeDataString(submission.Command.OrderId)+"/lines/"+Uri.EscapeDataString(submission.Command.LineId)+"/inventory-link-decision";
        var result=await _apiClient.PostAsync<PurchaseOrderInventoryLinkDecisionSubmission,PO_LINE_ITEM>(route,submission) ??throw new InvalidOperationException("Original independent decision outcome is unconfirmed. Retain the original intent.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during decision recording.");
        submission.RequireResult(result);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<PurchaseOrderInventoryLinkProposalReview> GetInventoryLinkPreparationAsync(string orderId,string lineId,string itemId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderId);ArgumentException.ThrowIfNullOrWhiteSpace(lineId);ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original procurement company and account.");
        var route="api/accounting/traditional/inventory/company/purchase-orders/"+Uri.EscapeDataString(orderId)+"/lines/"+Uri.EscapeDataString(lineId)+"/inventory-link-preparation/targets/"+Uri.EscapeDataString(itemId);
        var review=await _apiClient.GetAsync<PurchaseOrderInventoryLinkProposalReview>(route) ??throw new InvalidOperationException("Inventory-link preparation evidence is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during preparation review.");
        review.Require(original.Selection.Scope,original.Selection.UserId,orderId,lineId,itemId);return CompanyInventorySourceReview.Copy(review);
    }
    public async Task<PO_LINE_ITEM> PrepareInventoryLinkAsync(PurchaseOrderInventoryLinkProposalSubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);ArgumentNullException.ThrowIfNull(submission.Command);ArgumentNullException.ThrowIfNull(submission.Review);submission=CompanyInventorySourceReview.Copy(submission);
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original preparation company and account.");
        submission.Require(original.Selection.Scope,original.Selection.UserId,submission.Command.OrderId,submission.Command.LineId);
        var route="api/accounting/traditional/inventory/company/purchase-orders/"+Uri.EscapeDataString(submission.Command.OrderId)+"/lines/"+Uri.EscapeDataString(submission.Command.LineId)+"/inventory-link-preparation";
        var result=await _apiClient.PostAsync<PurchaseOrderInventoryLinkProposalSubmission,PO_LINE_ITEM>(route,submission) ??throw new InvalidOperationException("Original inventory-link preparation is unconfirmed.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during preparation recording.");
        submission.RequireResult(result);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<PurchaseReceiptInventoryReview> GetPurchaseReceiptInventoryReviewAsync(string orderId,string receiptId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderId);ArgumentException.ThrowIfNullOrWhiteSpace(receiptId);
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original receiving and warehouse company.");
        var review=await _apiClient.GetAsync<PurchaseReceiptInventoryReview>("api/accounting/traditional/inventory/company/purchase-orders/"+Uri.EscapeDataString(orderId)+"/receipts/"+Uri.EscapeDataString(receiptId))
            ??throw new InvalidOperationException("Receipt inventory source is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during receipt inventory review.");
        review.Require(original.Selection.Scope,original.Selection.UserId,orderId,receiptId);return CompanyInventorySourceReview.Copy(review);
    }
    public async Task<List<INVENTORY_ITEM>> GetCompanyInventoryItemsAsync()
    {
        var original=_apiClient.CompanySnapshot;
        if(original.Selection is null)throw new InvalidOperationException("Select the original warehouse company and account.");
        var rows=await _apiClient.GetAsync<List<INVENTORY_ITEM>>("api/accounting/traditional/inventory/company/items")
            ??throw new InvalidOperationException("Company inventory evidence is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during inventory review.");
        CompanyInventorySourceReview.RequireRows(original.Selection.Scope,rows);return rows.Select(CompanyInventorySourceReview.Copy).ToList();
    }
    public async Task<CompanyInventorySourceReview?> GetCompanyInventorySourceAsync(string itemId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);var original=_apiClient.CompanySnapshot;
        if(original.Selection is null)throw new InvalidOperationException("Select the original warehouse company and account.");
        var review=await _apiClient.GetAsync<CompanyInventorySourceReview>("api/accounting/traditional/inventory/company/items/"+Uri.EscapeDataString(itemId));
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during inventory source review.");
        review?.Require(original.Selection.Scope,original.Selection.UserId,itemId);return review is null?null:CompanyInventorySourceReview.Copy(review);
    }
}

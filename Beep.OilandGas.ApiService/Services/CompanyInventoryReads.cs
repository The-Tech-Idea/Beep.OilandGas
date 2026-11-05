using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;

/// <summary>Current app roles and complete repeat captures for the existing inventory source owner.</summary>
public sealed partial class CompanyInventoryReads
{
    public async Task<PurchaseReceiptStockIntentPage> GetStockRecoveryPageAsync(ClaimsPrincipal user,PurchaseOrderService orders,int page,int size)
    {
        PurchaseReceiptStockIntentPage.RequireRequest(page,size);var original=await Capture(user,true);
        async Task Authority(){if(await Capture(user,true)!=original)throw new UnauthorizedAccessException("Original stock page authority changed.");}
        var result=await inventory.GetStockRecoveryPageAsync(original.Company,original.Actor,page,size,orders,Authority);await Authority();result.Require(original.Company,original.Actor,page,size);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<PurchaseReceiptStockIntentIndex> GetStockRecoveryIndexAsync(ClaimsPrincipal user,PurchaseOrderService orders)
    {
        var original=await Capture(user,true);
        async Task Authority(){if(await Capture(user,true)!=original)throw new UnauthorizedAccessException("Original stock discovery company, account, roles, grants or binding changed.");}
        var result=await inventory.GetStockRecoveryIndexAsync(original.Company,original.Actor,orders,Authority);await Authority();result.Require(original.Company,original.Actor);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<PurchaseReceiptStockRecovery> GetStockRecoveryAsync(ClaimsPrincipal user,PurchaseOrderService orders,string intentId)
    {
        var original=await Capture(user,true);
        async Task Authority(){if(await Capture(user,true)!=original)throw new UnauthorizedAccessException("Original stock recovery company, account, roles, grants or binding changed.");}
        var result=await inventory.GetStockRecoveryAsync(original.Company,intentId,original.Actor,orders,Authority);
        await Authority();result.Require(original.Company,original.Actor,intentId);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<PurchaseReceiptInventoryReview> RecordStockAsync(ClaimsPrincipal user,PurchaseOrderService orders,string orderId,string receiptId,PurchaseReceiptStockSubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);submission=CompanyInventorySourceReview.Copy(submission);
        if(submission.Command is null || !Enum.IsDefined(submission.Command.Kind))throw new InvalidOperationException("Choose an explicit physical stock movement.");
        var original=await Capture(user,true,stockKind:submission.Command.Kind);submission.Require();
        if(submission.Command.Company!=original.Company || submission.Command.ActorUserId!=original.Actor || submission.Command.OrderId!=orderId ||
            submission.Command.ReceiptId!=receiptId || submission.Command.BindingRevision!=original.Binding)
            throw new InvalidOperationException("The original stock company, account, receipt route or binding changed.");
        async Task Authority(){if(await Capture(user,true,stockKind:submission.Command.Kind)!=original)throw new UnauthorizedAccessException("Original stock account, company, roles, grants or binding changed.");}
        var result=await inventory.RecordPurchaseReceiptStockAsync(submission,orders,Authority);await Authority();submission.RequireResult(result);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<PurchaseOrderInventoryLinkCycleResult> GetLinkHistoryAsync(ClaimsPrincipal user,PurchaseOrderService orders,string orderId,string lineId)
    {
        var original=await Capture(user,true);
        async Task Authority(){if(await Capture(user,true)!=original)throw new UnauthorizedAccessException("Original history company, account, roles, grants or binding changed.");}
        async Task<PurchaseOrderInventoryLinkCycleResult> Read()
        {
            var history=await orders.GetInventoryLinkHistoryAsync(original.Company,orderId,lineId,original.Actor,Authority);
            var purchase=await orders.GetSourceReviewAsync(original.Company,orderId,original.Actor) ?? throw new InvalidOperationException("Original history order is unavailable.");
            var result=new PurchaseOrderInventoryLinkCycleResult(purchase,history);result.Require(original.Company,original.Actor,orderId,lineId);await Authority();return result;
        }
        var first=await Read();var closing=await Read();await Authority();
        if(first.Purchase.Revision!=closing.Purchase.Revision || first.History.Revision!=closing.History.Revision)throw new InvalidOperationException("Complete original order or history changed.");
        return closing;
    }
    public async Task<PurchaseOrderInventoryLinkCycleReview> GetLinkCycleReviewAsync(ClaimsPrincipal user,PurchaseOrderService orders,string orderId,string lineId,PurchaseOrderInventoryLinkCycleAction action)
    {
        var original=await Capture(user,true,cycling:true);
        var result=await GetLinkHistoryAsync(user,orders,orderId,lineId);
        if(await Capture(user,true,cycling:true)!=original)throw new UnauthorizedAccessException("Original cycle review authority or binding changed.");
        return PurchaseOrderInventoryLinkCycleReview.Create(result.Purchase,result.History,lineId,action,original.Binding);
    }
    public async Task<PurchaseOrderInventoryLinkCycleResult> ReopenLinkAsync(ClaimsPrincipal user,PurchaseOrderService orders,string orderId,string lineId,PurchaseOrderInventoryLinkCycleSubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);submission=CompanyInventorySourceReview.Copy(submission);
        var original=await Capture(user,true,cycling:true);submission.Require(original.Company,original.Actor,orderId,lineId);
        if(submission.Command.BindingRevision!=original.Binding)throw new InvalidOperationException("Original cycle binding changed.");
        async Task Authority(){if(await Capture(user,true,cycling:true)!=original)throw new UnauthorizedAccessException("Original cycle company, account, roles, grants or binding changed.");}
        var result=await orders.ReopenInventoryLinkAsync(submission.Command,submission.Review,Authority);await Authority();submission.RequireResult(result);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<PurchaseOrderInventoryLinkDecisionReview> GetLinkDecisionAsync(ClaimsPrincipal user,PurchaseOrderService orders,string orderId,string lineId)
    {
        var original=await Capture(user,true,deciding:true);
        async Task<PurchaseOrderInventoryLinkDecisionReview> Read()
        {
            var purchase=await orders.GetSourceReviewAsync(original.Company,orderId,original.Actor) ?? throw new InvalidOperationException("The original owned order is unavailable.");
            var line=purchase.Lines.SingleOrDefault(row=>row.PO_LINE_ITEM_ID==lineId) ?? throw new InvalidOperationException("The line is outside this owned order.");
            var proposal=PurchaseOrderInventoryLinkProposalProof.Require(original.Company,line);
            var target=await inventory.GetCompanyInventorySourceAsync(original.Company,proposal.Command.TargetItemId,original.Actor) ?? throw new InvalidOperationException("The original proposed target is unavailable.");
            return PurchaseOrderInventoryLinkDecisionReview.Create(purchase,target,lineId,original.Binding);
        }
        var first=await Read();var closing=await Read();
        if(await Capture(user,true,deciding:true)!=original || first.Revision!=closing.Revision)throw new InvalidOperationException("Independent decision source, target or company authority changed.");
        return closing;
    }
    public async Task<PurchaseOrderInventoryLinkResolution> GetLinkResolutionAsync(ClaimsPrincipal user,PurchaseOrderService orders,string orderId,string lineId,PurchaseOrderInventoryLinkProposalSubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);submission=CompanyInventorySourceReview.Copy(submission);var original=await Capture(user,true);
        submission.Require(original.Company,original.Actor,orderId,lineId);
        if(submission.Command.BindingRevision!=original.Binding)throw new InvalidOperationException("Original preparation resolution binding changed.");
        async Task<PurchaseOrderInventoryLinkResolution> Read()
        {
            var source=await orders.GetSourceReviewAsync(original.Company,orderId,original.Actor) ?? throw new InvalidOperationException("Original resolved order is unavailable.");
            var result=new PurchaseOrderInventoryLinkResolution(source,original.Binding);result.Require(submission);return result;
        }
        var first=await Read();var closing=await Read();
        if(await Capture(user,true)!=original || first.Purchase.Revision!=closing.Purchase.Revision)throw new InvalidOperationException("Original resolution source or company authority changed.");
        return closing;
    }
    public async Task<PO_LINE_ITEM> RecordLinkDecisionAsync(ClaimsPrincipal user,PurchaseOrderService orders,string orderId,string lineId,PurchaseOrderInventoryLinkDecisionSubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);submission=CompanyInventorySourceReview.Copy(submission);
        var original=await Capture(user,true,deciding:true);submission.Require(original.Company,original.Actor,orderId,lineId);
        if(submission.Command.BindingRevision!=original.Binding)throw new InvalidOperationException("Original decision binding changed.");
        async Task Authority()
        {
            if(await Capture(user,true,deciding:true)!=original)throw new UnauthorizedAccessException("Original reviewer company, account, roles, grants or binding changed.");
        }
        async Task<CompanyInventorySourceReview> Target()=>await inventory.GetCompanyInventorySourceAsync(original.Company,submission.Command.TargetItemId,original.Actor)
            ??throw new InvalidOperationException("Original decision target is unavailable.");
        var result=await orders.RecordInventoryLinkDecisionAsync(submission.Command,submission.Review,Target,Authority);
        await Authority();submission.RequireResult(result);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<PurchaseOrderInventoryLinkProposalReview> GetLinkPreparationAsync(ClaimsPrincipal user,PurchaseOrderService orders,string orderId,string lineId,string itemId)
    {
        var original=await Capture(user,true,true);
        async Task<PurchaseOrderInventoryLinkProposalReview> Read()
        {
            var purchase=await orders.GetSourceReviewAsync(original.Company,orderId,original.Actor) ?? throw new InvalidOperationException("The owned purchase order is unavailable.");
            var target=await inventory.GetCompanyInventorySourceAsync(original.Company,itemId,original.Actor) ?? throw new InvalidOperationException("The owned target inventory item is unavailable.");
            return PurchaseOrderInventoryLinkProposalReview.Create(purchase,target,lineId,original.Binding);
        }
        var first=await Read();var closing=await Read();
        if(await Capture(user,true,true)!=original || first.Revision!=closing.Revision)throw new InvalidOperationException("Preparation evidence or company authority changed.");
        return closing;
    }
    public async Task<PO_LINE_ITEM> PrepareLinkAsync(ClaimsPrincipal user,PurchaseOrderService orders,string orderId,string lineId,PurchaseOrderInventoryLinkProposalSubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);
        submission=CompanyInventorySourceReview.Copy(submission);
        var original=await Capture(user,true,true);
        submission.Require(original.Company,original.Actor,orderId,lineId);
        if(submission.Command.BindingRevision!=original.Binding)throw new InvalidOperationException("The original preparation binding changed.");
        async Task Authority()
        {
            if(await Capture(user,true,true)!=original)throw new UnauthorizedAccessException("Original preparation roles, grants, company, account or binding changed.");
        }
        async Task<CompanyInventorySourceReview> Target()=>await inventory.GetCompanyInventorySourceAsync(original.Company,submission.Command.TargetItemId,original.Actor)
            ??throw new InvalidOperationException("The original target inventory source is unavailable.");
        var result=await orders.PrepareInventoryLinkAsync(submission.Command,submission.Review,Target,Authority);
        await Authority();submission.RequireResult(result);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<PurchaseReceiptStockReview> GetReceiptStockReviewAsync(ClaimsPrincipal user,PurchaseOrderService orders,string orderId,string receiptId)
        =>PurchaseReceiptStockReview.Create(await GetReceiptLinkAsync(user,orders,orderId,receiptId,true));
    public async Task<PurchaseReceiptInventoryReview> GetReceiptLinkAsync(ClaimsPrincipal user,PurchaseOrderService orders,string orderId,string receiptId,bool stockEvidence=false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderId);ArgumentException.ThrowIfNullOrWhiteSpace(receiptId);
        var original=await Capture(user,true);
        async Task<PurchaseReceiptInventoryReview> Read()
        {
            var purchase=await orders.GetSourceReviewAsync(original.Company,orderId,original.Actor) ?? throw new InvalidOperationException("The original purchase order is unavailable.");
            var receipt=purchase.Receipts.SingleOrDefault(row=>row.PO_RECEIPT_ID==receiptId) ?? throw new InvalidOperationException("The receipt is outside this owned order.");
            var itemId=purchase.Lines.Single(row=>row.PO_LINE_ITEM_ID==receipt.PO_LINE_ITEM_ID).INVENTORY_ITEM_ID;
            if(string.IsNullOrWhiteSpace(itemId))throw new InvalidOperationException("The recorded PO line has no inventory-item linkage.");
            var source=stockEvidence?await inventory.GetReceiptStockInventoryAsync(purchase,receiptId):
                await inventory.GetCompanyInventorySourceAsync(original.Company,itemId,original.Actor) ?? throw new InvalidOperationException("The recorded inventory source is unavailable.");
            if(stockEvidence)
            {
                var finalPurchase=await orders.GetSourceReviewAsync(original.Company,orderId,original.Actor);
                if(finalPurchase?.Revision!=purchase.Revision)throw new InvalidOperationException("Complete purchase source changed while reading physical stock history.");
            }
            return PurchaseReceiptInventoryReview.Create(purchase,source,receiptId,original.Binding);
        }
        var initial=await Read();var closing=await Read();
        if(await Capture(user,true)!=original)throw new InvalidOperationException("Receipt inventory company authority or binding changed.");
        if(initial.Revision!=closing.Revision)throw new InvalidOperationException("Receipt inventory source changed.");
        return closing;
    }
}

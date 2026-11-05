using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class APInvoiceWorkspaceState : IDisposable
{
    private readonly CompanyClientContext company;
    private bool disposed;
    private int purchaseVersion,allocationVersion,proposalVersion;
    private int financialVersion;
    private int policyVersion;
    private CompanyMatchingPolicyReview? policies;
    public CompanyMatchingPolicyReview? MatchingPolicies=>policies is null?null:PurchaseReceiptDecisionProof.Copy(policies);
    public void ClearMatchingPolicies(){ClearCorrectionDecisionReview();++policyVersion;policies=null;InvalidatePolicyCommandDisplay();}
    public async Task LoadMatchingPoliciesAsync(Func<Task<CompanyMatchingPolicyReview>> read)
    {
        if(PolicyCommandBusy)throw new InvalidOperationException("Wait for the current policy outcome.");
        ClearMatchingPolicies();var generation=policyVersion;var version=Version;var original=company.Snapshot;
        if(disposed || original.Selection is null)return;
        var review=await read();
        if(disposed || generation!=policyVersion || version!=Version || original!=company.Snapshot)return;
        var frozen=PurchaseReceiptDecisionProof.Copy(review);frozen.Require(original.Selection.Scope,original.Selection.UserId);policies=frozen;
    }
    private APInvoiceFinancialReview? financial;
    public APInvoiceFinancialReview? FinancialReview=>financial is null?null:System.Text.Json.JsonSerializer.Deserialize<APInvoiceFinancialReview>(System.Text.Json.JsonSerializer.Serialize(financial));
    public void ClearFinancialReview(){ClearCorrectionDecisionReview();ClearCorrection();ClearVarianceDecisionReview();ClearVarianceProposal();++financialVersion;financial=null;}
    public async Task LoadFinancialReviewAsync(Func<string,string,Task<APInvoiceFinancialReview>> read)
    {
        ClearFinancialReview();var generation=financialVersion;var version=Version;var original=company.Snapshot;var id=Detail?.Id;var revision=SourceRevision;
        if(disposed || original.Selection is null || id is null || revision is null)return;
        var review=await read(id,revision);
        if(disposed || generation!=financialVersion || version!=Version || original!=company.Snapshot)return;
        var frozen=System.Text.Json.JsonSerializer.Deserialize<APInvoiceFinancialReview>(System.Text.Json.JsonSerializer.Serialize(review))!;
        frozen.Require(original.Selection.Scope,original.Selection.UserId,id,revision);financial=frozen;
    }
    public string? ProposalReceiptId {get;private set;}
    public decimal? ProposalQuantity {get;private set;}
    public ProposalItem? ReviewedProposal {get;private set;}
    public IReadOnlyList<PurchaseOrderWorkspaceState.ReceiptItem> ProposalReceipts=>Allocations is null?[]:LinkedOrder.Receipts?.Where(receipt=>receipt.LineId==Lines?.SingleOrDefault(line=>line.Id==SelectedPurchaseLineId)?.OrderLineId && receipt.ActiveIndicator=="Y" && receipt.Quantity is >0 && !string.IsNullOrWhiteSpace(receipt.QuantityUnit)).ToArray() ?? [];
    public string? ProposalUnit=>ProposalReceipts.SingleOrDefault(receipt=>receipt.Id==ProposalReceiptId)?.QuantityUnit;
    private void ClearProposal(){++proposalVersion;ProposalReceiptId=null;ProposalQuantity=null;ReviewedProposal=null;reviewedAllocationEvidence=null;}
    public void SetProposalReceipt(string id){if(!ProposalReceipts.Any(receipt=>receipt.Id==id))throw new InvalidOperationException("Select a current recorded receipt.");++proposalVersion;ProposalReceiptId=id;ProposalQuantity=null;ReviewedProposal=null;}
    public void SetProposalQuantity(decimal? quantity){++proposalVersion;ProposalQuantity=quantity;ReviewedProposal=null;}
    public IReadOnlyList<AllocationItem>? Allocations {get;private set;}
    public string? AllocationRevision {get;private set;}
    public DateTimeOffset? AllocationsRetrievedAtUtc {get;private set;}
    public void ClearReceiptAllocations(){ClearCorrectionDecisionReview();InvalidateAllocationDisplay();ClearProposal();++allocationVersion;Allocations=null;AllocationRevision=null;AllocationsRetrievedAtUtc=null;}
    public PurchaseOrderWorkspaceState LinkedOrder {get;}
    public string? SelectedPurchaseLineId {get;private set;}
    public int Version {get;private set;}
    public IReadOnlyList<Item>? Rows {get;private set;}
    public string? SupplierFilter {get;private set;}
    public string? StatusFilter {get;private set;}
    public IReadOnlyList<string> RecordedStatuses => Rows?.Where(row=>!string.IsNullOrWhiteSpace(row.Status)).Select(row=>row.Status!).Distinct(StringComparer.Ordinal).OrderBy(value=>value,StringComparer.Ordinal).ToArray() ?? [];
    public bool UnavailableStatusOnly {get;private set;}
    public IReadOnlyList<StatusBucket> StatusCounts=>Rows?.GroupBy(row=>string.IsNullOrWhiteSpace(row.Status)?null:row.Status,StringComparer.Ordinal)
        .OrderBy(group=>group.Key is null).ThenBy(group=>group.Key,StringComparer.Ordinal)
        .Select(group=>new StatusBucket(group.Key,group.Key is null,group.Count())).ToArray() ?? [];
    public IReadOnlyList<Item>? VisibleRows => Rows is null?null:Array.AsReadOnly(Rows.Where(row=>UnavailableStatusOnly?
        string.IsNullOrWhiteSpace(row.Status):StatusFilter is null || row.Status==StatusFilter).ToArray());
    public void SetStatusFilter(string? status,bool unavailableOnly=false)
    {
        if(unavailableOnly && (status is not null || !StatusCounts.Any(bucket=>bucket.Unavailable)) ||
            !unavailableOnly && status is not null && !RecordedStatuses.Contains(status,StringComparer.Ordinal))
            throw new InvalidOperationException("This status is not in the current recorded invoice queue.");
        StatusFilter=status;UnavailableStatusOnly=unavailableOnly;
    }
    public Item? Detail {get;private set;}
    public IReadOnlyList<LineItem>? Lines {get;private set;}
    public string? SourceRevision {get;private set;}
    public DateTimeOffset? RetrievedAtUtc {get;private set;}
    public APInvoiceWorkspaceState(CompanyClientContext company){this.company=company;LinkedOrder=new(company);company.Changed+=Clear;}
    public void ClearPurchaseSource(){ClearMatchingPolicies();ClearFinancialReview();++purchaseVersion;LinkedOrder.Clear();SelectedPurchaseLineId=null;ClearReceiptAllocations();}
    public void Clear(){InvalidateAllocationDisplay();++Version;ClearPurchaseSource();recordedCorrectionHistory=null;recordedVariance=null;HasRecordedVarianceSource=false;recordedVarianceDecision=null;HasRecordedVarianceDecisionSource=false;Rows=null;SupplierFilter=null;StatusFilter=null;UnavailableStatusOnly=false;Detail=null;Lines=null;SourceRevision=null;RetrievedAtUtc=null;}
    public async Task LoadPurchaseSourceAsync(string lineId,Func<string,string,string,Task<APLinePurchaseSourceReview>> read)
    {
        ClearPurchaseSource();var generation=purchaseVersion;
        var version=Version;var original=company.Snapshot;var invoice=Detail;var revision=SourceRevision;
        if(disposed || original.Selection is null || invoice is null || !ProcessDefinitionRevision.IsValid(revision))return;
        var line=Lines?.SingleOrDefault(row=>row.Id==lineId);
        if(line is null || string.IsNullOrWhiteSpace(line.OrderId) || string.IsNullOrWhiteSpace(line.OrderLineId))throw new InvalidOperationException("This invoice line has no complete recorded PO reference.");
        var review=await read(invoice.Id,lineId,revision!);
        if(disposed || generation!=purchaseVersion || version!=Version || original!=company.Snapshot)return;
        if(review is null)throw new InvalidOperationException("The AP purchase source was unavailable.");
        var order=review.RequireOrderId(original.Selection.Scope,original.Selection.UserId,invoice.Id,lineId,revision!);
        if(order!=line.OrderId || !review.Order.Lines.Any(row=>row.PO_LINE_ITEM_ID==line.OrderLineId))throw new InvalidOperationException("The returned purchase reference does not match the displayed AP line.");
        await LinkedOrder.LoadDetailAsync(order,_=>Task.FromResult<PurchaseOrderSourceReview?>(review.Order));
        if(!disposed && generation==purchaseVersion && version==Version && original==company.Snapshot)SelectedPurchaseLineId=lineId;
    }
    public async Task LoadReceiptAllocationsAsync(Func<string,string,string,string,Task<APReceiptAllocationReview>> read)
    {
        ClearReceiptAllocations();var generation=allocationVersion;var version=Version;var purchase=purchaseVersion;var original=company.Snapshot;
        var invoice=Detail;var line=SelectedPurchaseLineId;var revision=SourceRevision;var orderRevision=LinkedOrder.SourceRevision;
        if(disposed || original.Selection is null || invoice is null || string.IsNullOrWhiteSpace(line) ||
            !ProcessDefinitionRevision.IsValid(revision) || !ProcessDefinitionRevision.IsValid(orderRevision))return;
        var review=await read(invoice.Id,line,revision!,orderRevision!);
        if(disposed || generation!=allocationVersion || version!=Version || purchase!=purchaseVersion || original!=company.Snapshot)return;
        if(review is null)throw new InvalidOperationException("Recorded allocations were unavailable.");
        review.Require(original.Selection.Scope,original.Selection.UserId,invoice.Id,line,revision!);
        if(review.Source.Order.Revision!=orderRevision)throw new InvalidOperationException("The displayed order changed.");
        Allocations=Array.AsReadOnly(review.Allocations.Select(row=>new AllocationItem(row.AP_RECEIPT_ALLOCATION_ID,row.PO_RECEIPT_ID,row.PO_LINE_ITEM_ID,
            row.ALLOCATED_QUANTITY,row.QUANTITY_UOM,row.ACTIVE_IND,row.ROW_CREATED_BY,row.ROW_CREATED_DATE,row.ROW_CHANGED_BY,row.ROW_CHANGED_DATE)).ToArray());
        AllocationRevision=review.Revision;AllocationsRetrievedAtUtc=review.RetrievedAtUtc;
    }
    public async Task ReviewAllocationProposalAsync(Func<string,string,APReceiptAllocationProposal,Task<APReceiptAllocationProposalReview>> read)
    {
        if(CorrectionCommandUnsettled || AllocationCommandBusy || HasUnconfirmedAllocationIntent)throw new InvalidOperationException("Reconcile the original allocation before reviewing another proposal.");
        reviewedAllocationEvidence=null;
        ReviewedProposal=null;var generation=++proposalVersion;var version=Version;var purchase=purchaseVersion;var allocation=allocationVersion;var original=company.Snapshot;
        if(disposed || original.Selection is null || Detail is null || SelectedPurchaseLineId is null)return;
        var invoice=Detail.Id;var line=SelectedPurchaseLineId;
        if(string.IsNullOrWhiteSpace(ProposalReceiptId) || ProposalQuantity is not >0 || string.IsNullOrWhiteSpace(ProposalUnit) ||
            !ProcessDefinitionRevision.IsValid(SourceRevision) || !ProcessDefinitionRevision.IsValid(LinkedOrder.SourceRevision) || !ProcessDefinitionRevision.IsValid(AllocationRevision))
            throw new InvalidOperationException("Review sources and enter an explicit receipt quantity before reviewing a proposal.");
        var proposal=new APReceiptAllocationProposal(ProposalReceiptId,ProposalQuantity.Value,ProposalUnit,SourceRevision!,LinkedOrder.SourceRevision!,AllocationRevision!);
        var review=await read(invoice,line,proposal);
        if(disposed || generation!=proposalVersion || version!=Version || purchase!=purchaseVersion || allocation!=allocationVersion || original!=company.Snapshot)return;
        if(review is null)throw new InvalidOperationException("The proposal source review was unavailable.");
        review=CopyAllocation(review);review.Require(original.Selection.Scope,original.Selection.UserId,invoice,line,proposal);
        reviewedAllocationEvidence=review;
        var consumption=review.ReceiptConsumption;
        ReviewedProposal=new(proposal.ReceiptId,proposal.Quantity,proposal.Unit,review.Revision,review.RetrievedAtUtc,
            consumption is null?null:Array.AsReadOnly(consumption.Contributors.SelectMany(contributor=>contributor.Allocations.Select(row=>
                new ReceiptContributorItem(row.AP_INVOICE_ID,row.AP_INVOICE_LINE_ID,row.AP_RECEIPT_ALLOCATION_ID,row.ALLOCATED_QUANTITY,
                    row.QUANTITY_UOM,row.ACTIVE_IND))).ToArray()),consumption?.RecordedActiveQuantity,review.Comparisons,
            consumption?.VerifiedAcceptedQuantity,consumption?.VerifiedRejectedQuantity,consumption?.VerifiedReceiverDecision?.ActorUserId);
    }
    public async Task LoadVendorAsync(string vendor,Func<string,Task<List<AP_INVOICE>>> read)
    {
        if(string.IsNullOrWhiteSpace(vendor)){Clear();return;}
        await LoadCompanyAsync(value=>read(value!),vendor);
    }
    public async Task LoadCompanyAsync(Func<string?,Task<List<AP_INVOICE>>> read,string? vendor=null)
    {
        Clear();var version=Version;var original=company.Snapshot;
        if(disposed || original.Selection is null)return;
        if(vendor is not null && string.IsNullOrWhiteSpace(vendor))throw new InvalidOperationException("A supplied supplier restriction cannot be empty.");
        var rows=await read(vendor);if(disposed || version!=Version || original!=company.Snapshot)return;
        APInvoiceOwnership.RequireRows(original.Selection.Scope,rows);
        if(vendor is not null)APInvoiceOwnership.RequireVendorRows(original.Selection.Scope,vendor,rows);
        Rows=Array.AsReadOnly(rows.Select(Project).OrderBy(row=>row.Id,StringComparer.Ordinal).ToArray());
        SupplierFilter=vendor;RetrievedAtUtc=DateTimeOffset.UtcNow;
    }
    public async Task LoadDetailAsync(string id,Func<string,Task<APInvoiceSourceReview?>> read)
    {
        Clear();var version=Version;var original=company.Snapshot;
        if(disposed || original.Selection is null || string.IsNullOrWhiteSpace(id))return;
        var review=await read(id);if(disposed || version!=Version || original!=company.Snapshot)return;
        if(review is null)throw new InvalidOperationException("The AP source review was unavailable.");
        review.Require(original.Selection.Scope,original.Selection.UserId,id);
        ApplyDetail(review);
    }
    private void ApplyDetail(APInvoiceSourceReview review)
    {
        ReadRecordedVariance(review);
        recordedCorrectionHistory=review.Invoice.CORRECTION_HISTORY_JSON is null?null:APInvoiceCorrectionHistory.Require(review.Company,review.Invoice,review.Lines);
        Detail=Project(review.Invoice);
        Lines=Array.AsReadOnly(review.Lines.Select(line=>new LineItem(line.AP_INVOICE_LINE_ID,line.LINE_NUMBER,line.ITEM_DESCRIPTION,line.QUANTITY,
            line.QUANTITY_UOM,line.UNIT_PRICE,line.LINE_TOTAL,line.CURRENCY_CODE,line.PURCHASE_ORDER_ID,line.PO_LINE_ITEM_ID,line.ACTIVE_IND,
            line.ROW_CREATED_BY,line.ROW_CREATED_DATE,line.ROW_CHANGED_BY,line.ROW_CHANGED_DATE)).ToArray());
        SourceRevision=review.Revision;RetrievedAtUtc=review.RetrievedAtUtc;
    }
    private static Item Project(AP_INVOICE row)=>new(row.AP_INVOICE_ID,row.INVOICE_NUMBER,row.VENDOR_BA_ID,row.INVOICE_DATE,row.DUE_DATE,
        row.TOTAL_AMOUNT,row.PAID_AMOUNT,row.BALANCE_DUE,row.CURRENCY_CODE,row.STATUS,row.ACTIVE_IND,row.REMARK,row.ROW_CREATED_BY,row.ROW_CREATED_DATE,row.ROW_CHANGED_BY,row.ROW_CHANGED_DATE);
    public void Dispose(){disposed=true;company.Changed-=Clear;Clear();LinkedOrder.Dispose();}
    public sealed record ProposalItem(string ReceiptId,decimal Quantity,string Unit,string Revision,DateTimeOffset RetrievedAtUtc,
        IReadOnlyList<ReceiptContributorItem>? Contributors=null,decimal? RecordedActiveQuantity=null,IReadOnlyList<APSourceComparison>? Comparisons=null,
        decimal? AcceptedQuantity=null,decimal? RejectedQuantity=null,string? DecisionActor=null);
    public sealed record ReceiptContributorItem(string InvoiceId,string InvoiceLineId,string AllocationId,decimal? Quantity,string? Unit,string? Activity)
    {public string Route=>"/accounting/payables/invoices/"+Uri.EscapeDataString(InvoiceId);}
    public sealed record StatusBucket(string? Status,bool Unavailable,int Count);
    public sealed record AllocationItem(string Id,string ReceiptId,string OrderLineId,decimal? Quantity,string? Unit,string? ActiveIndicator,
        string? RecordedBy,DateTime? RecordedAt,string? ChangedBy,DateTime? ChangedAt);
    public sealed record LineItem(string Id,int? Number,string? Description,decimal? Quantity,string? Unit,decimal? UnitPrice,decimal? Amount,
        string? Currency,string? OrderId,string? OrderLineId,string? ActiveIndicator,string? RecordedBy,DateTime? RecordedAt,string? ChangedBy,DateTime? ChangedAt);
    public sealed record Item(string Id,string? Number,string? Vendor,DateTime? InvoiceDate,DateTime? DueDate,decimal? Amount,decimal? PaidAmount,
        decimal? BalanceDue,string? Currency,string? Status,string? ActiveIndicator,string? Remarks,string? RecordedBy,DateTime? RecordedAt,string? ChangedBy,DateTime? ChangedAt)
    {public string Route=>"/accounting/payables/invoices/"+Uri.EscapeDataString(Id);}
}

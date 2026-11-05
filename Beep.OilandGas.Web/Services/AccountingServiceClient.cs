using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Accounting.Cost;
using Beep.OilandGas.Models.Data.Accounting.Royalty;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Reporting;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services;

public sealed partial class AccountingServiceClient : IAccountingServiceClient
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<AccountingServiceClient> _logger;

    public AccountingServiceClient(ApiClient apiClient, ILogger<AccountingServiceClient> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Beep.OilandGas.Accounting.Models.PurchaseReceiptDecisionReview> GetPurchaseReceiptDecisionReviewAsync(string orderId,string receiptId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderId);ArgumentException.ThrowIfNullOrWhiteSpace(receiptId);
        var scope=_apiClient.CompanySnapshot;if(scope.Selection is null)throw new InvalidOperationException("Select the original receiving company and account.");
        var review=await _apiClient.GetAsync<Beep.OilandGas.Accounting.Models.PurchaseReceiptDecisionReview>("api/accounting/traditional/purchase-order/"+Uri.EscapeDataString(orderId)+"/receipts/"+Uri.EscapeDataString(receiptId)+"/decision-review")
            ?? throw new InvalidOperationException("The bound receiver review was unavailable.");
        if(scope!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company changed during receiver review.");
        review.Require(scope.Selection.Scope,scope.Selection.UserId,orderId,receiptId);return review;
    }
    public async Task<Beep.OilandGas.Accounting.Models.PurchaseReceiptDecisionResult> RecordPurchaseReceiptDecisionAsync(Beep.OilandGas.Accounting.Models.PurchaseReceiptDecisionSubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);ArgumentNullException.ThrowIfNull(submission.Command);ArgumentNullException.ThrowIfNull(submission.Review);
        submission=Beep.OilandGas.Models.Data.Accounting.PurchaseReceiptDecisionProof.Copy(submission);submission.Review.Require(submission.Command);
        var command=submission.Command;var scope=_apiClient.CompanySnapshot;
        if(scope.Selection is null || scope.Selection.Scope!=command.Company || scope.Selection.UserId!=command.ActorUserId)
            throw new InvalidOperationException("Return to the original receiver company and account to submit or reconcile.");
        var route="api/accounting/traditional/purchase-order/"+Uri.EscapeDataString(command.OrderId)+"/receipts/"+Uri.EscapeDataString(command.ReceiptId)+"/decision";
        var result=await _apiClient.PostAsync<Beep.OilandGas.Accounting.Models.PurchaseReceiptDecisionSubmission,Beep.OilandGas.Accounting.Models.PurchaseReceiptDecisionResult>(route,submission)
            ?? throw new InvalidOperationException("Receiver outcome is unavailable. Reconcile the original intent.");
        if(scope!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company changed during receiving. Reconcile the original intent.");
        result.Require(submission);return result;
    }
    public async Task<Beep.OilandGas.Models.Data.Accounting.APReceiptConsumptionReview> GetAPReceiptConsumptionAsync(string invoiceId,string lineId,string receiptId,string invoiceRevision,string orderRevision)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invoiceId);ArgumentException.ThrowIfNullOrWhiteSpace(lineId);ArgumentException.ThrowIfNullOrWhiteSpace(receiptId);
        if(!Beep.OilandGas.Models.Data.Process.ProcessDefinitionRevision.IsValid(invoiceRevision) || !Beep.OilandGas.Models.Data.Process.ProcessDefinitionRevision.IsValid(orderRevision))throw new InvalidOperationException("Review original invoice and purchase revisions first.");
        var scope=_apiClient.CompanySnapshot;if(scope.Selection is null)throw new InvalidOperationException("Select the original company and account first.");
        var route="api/accounting/traditional/ap/"+Uri.EscapeDataString(invoiceId)+"/lines/"+Uri.EscapeDataString(lineId)+"/receipt-consumption?receiptId="+Uri.EscapeDataString(receiptId)+"&expectedInvoiceRevision="+Uri.EscapeDataString(invoiceRevision)+"&expectedOrderRevision="+Uri.EscapeDataString(orderRevision);
        var review=await _apiClient.GetAsync<Beep.OilandGas.Models.Data.Accounting.APReceiptConsumptionReview>(route) ?? throw new InvalidOperationException("Receipt consumption evidence is unavailable.");
        if(scope!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company changed during receipt consumption review.");
        review.Source.RequireOrderId(scope.Selection.Scope,scope.Selection.UserId,invoiceId,lineId,invoiceRevision);
        if(review.Source.Order.Revision!=orderRevision)throw new InvalidOperationException("The reviewed purchase source changed.");
        review.Require(scope.Selection.Scope,scope.Selection.UserId,review.Source,receiptId);return review;
    }
    public async Task<Beep.OilandGas.Accounting.Models.APReceiptAllocationReceipt> RecordAPReceiptAllocationAsync(Beep.OilandGas.Accounting.Models.APReceiptAllocationSubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);ArgumentNullException.ThrowIfNull(submission.Command);ArgumentNullException.ThrowIfNull(submission.Review);
        submission=System.Text.Json.JsonSerializer.Deserialize<Beep.OilandGas.Accounting.Models.APReceiptAllocationSubmission>(System.Text.Json.JsonSerializer.Serialize(submission))!;
        var command=submission.Command;command.Require(submission.Review);
        var scope=_apiClient.CompanySnapshot;
        if(scope.Selection is null || scope.Selection.Scope!=command.Company || scope.Selection.UserId!=command.ActorUserId)
            throw new InvalidOperationException("Select the original allocation company and account before submission or reconciliation.");
        var route="api/accounting/traditional/ap/"+Uri.EscapeDataString(command.InvoiceId)+"/lines/"+Uri.EscapeDataString(command.LineId)+"/receipt-allocations";
        var receipt=await _apiClient.PostAsync<Beep.OilandGas.Accounting.Models.APReceiptAllocationSubmission,Beep.OilandGas.Accounting.Models.APReceiptAllocationReceipt>(route,submission)
            ?? throw new InvalidOperationException("The original allocation outcome was not returned; reconcile before another allocation.");
        if(scope!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during allocation submission; its outcome is unconfirmed.");
        receipt.Require(command,submission.Review);return receipt;
    }
    public async Task<Beep.OilandGas.Models.Data.Accounting.CompanyMatchingPolicyReview> GetCompanyMatchingPoliciesAsync()
    {
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original company and account first.");
        var review=await _apiClient.GetAsync<Beep.OilandGas.Models.Data.Accounting.CompanyMatchingPolicyReview>("api/accounting/traditional/ap/matching-policies")
            ?? throw new InvalidOperationException("Company matching-policy evidence was unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during matching-policy review.");
        review.Require(original.Selection.Scope,original.Selection.UserId);return review;
    }
    public async Task<Beep.OilandGas.Models.Data.Accounting.CompanyMatchingPolicyCommandReview> GetCompanyMatchingPolicyCommandReviewAsync(string action)
    {
        if(action is not ("DRAFT" or "APPROVE"))throw new InvalidOperationException("Choose an explicit policy action.");
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original policy company and account.");
        var review=await _apiClient.GetAsync<Beep.OilandGas.Models.Data.Accounting.CompanyMatchingPolicyCommandReview>("api/accounting/traditional/ap/matching-policies/command-review?action="+Uri.EscapeDataString(action))
            ?? throw new InvalidOperationException("The original policy command review is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during policy command review.");
        review.Require(original.Selection.Scope,original.Selection.UserId,action);return review;
    }
    public async Task<Beep.OilandGas.Models.Data.Accounting.CompanyMatchingPolicyResult> RecordCompanyMatchingPolicyAsync(Beep.OilandGas.Models.Data.Accounting.CompanyMatchingPolicySubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);ArgumentNullException.ThrowIfNull(submission.Command);ArgumentNullException.ThrowIfNull(submission.Review);
        submission=Beep.OilandGas.Models.Data.Accounting.PurchaseReceiptDecisionProof.Copy(submission);submission.Review.Require(submission.Command);
        var original=_apiClient.CompanySnapshot;var command=submission.Command;
        if(original.Selection is null || original.Selection.Scope!=command.Company || original.Selection.UserId!=command.ActorUserId)
            throw new InvalidOperationException("Return to the original policy company and account to submit or reconcile.");
        var result=await _apiClient.PostAsync<Beep.OilandGas.Models.Data.Accounting.CompanyMatchingPolicySubmission,Beep.OilandGas.Models.Data.Accounting.CompanyMatchingPolicyResult>("api/accounting/traditional/ap/matching-policies/commands",submission)
            ?? throw new InvalidOperationException("Policy outcome is unconfirmed. Reconcile the original intent.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during policy submission. Retain the original intent.");
        result.Require(submission);return result;
    }
    public async Task<Beep.OilandGas.Models.Data.Accounting.APInvoiceFinancialReview> GetAPInvoiceFinancialReviewAsync(string invoiceId,string expectedInvoiceRevision)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invoiceId);
        if(!Beep.OilandGas.Models.Data.Process.ProcessDefinitionRevision.IsValid(expectedInvoiceRevision))throw new InvalidOperationException("Reload the invoice before reviewing financial evidence.");
        var scope=_apiClient.CompanySnapshot;if(scope.Selection is null)throw new InvalidOperationException("Select the original company first.");
        var review=await _apiClient.GetAsync<Beep.OilandGas.Models.Data.Accounting.APInvoiceFinancialReview>("api/accounting/traditional/ap/"+Uri.EscapeDataString(invoiceId)+"/financial-review?expectedInvoiceRevision="+Uri.EscapeDataString(expectedInvoiceRevision))
            ?? throw new InvalidOperationException("The invoice financial review was unavailable.");
        if(scope!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company changed during invoice financial review.");
        review.Require(scope.Selection.Scope,scope.Selection.UserId,invoiceId,expectedInvoiceRevision);return review;
    }
    public async Task<Beep.OilandGas.Models.Data.Accounting.APReceiptAllocationProposalReview> ReviewAPReceiptAllocationAsync(string invoiceId,string lineId,Beep.OilandGas.Models.Data.Accounting.APReceiptAllocationProposal proposal)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invoiceId);ArgumentException.ThrowIfNullOrWhiteSpace(lineId);ArgumentNullException.ThrowIfNull(proposal);
        var scope=_apiClient.CompanySnapshot;if(scope.Selection is null)throw new InvalidOperationException("Select a company before reviewing a proposal.");
        var route="/api/accounting/traditional/ap/"+Uri.EscapeDataString(invoiceId)+"/lines/"+Uri.EscapeDataString(lineId)+"/receipt-allocation-review";
        var review=await _apiClient.PostAsync<Beep.OilandGas.Models.Data.Accounting.APReceiptAllocationProposal,Beep.OilandGas.Models.Data.Accounting.APReceiptAllocationProposalReview>(route,proposal)
            ?? throw new InvalidOperationException("Allocation preparation review was not returned.");
        if(scope!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company changed during allocation preparation review.");
        if(review.ReceiptConsumption is null)throw new InvalidOperationException("Receipt contributors were not reviewed.");
        review.Require(scope.Selection.Scope,scope.Selection.UserId,invoiceId,lineId,proposal);return review;
    }
    public async Task<Beep.OilandGas.Models.Data.Accounting.APReceiptAllocationReview> GetAPReceiptAllocationsAsync(string invoiceId,string lineId,string invoiceRevision,string orderRevision)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invoiceId);ArgumentException.ThrowIfNullOrWhiteSpace(lineId);
        if(!Beep.OilandGas.Models.Data.Process.ProcessDefinitionRevision.IsValid(invoiceRevision) || !Beep.OilandGas.Models.Data.Process.ProcessDefinitionRevision.IsValid(orderRevision))throw new InvalidOperationException("Review invoice and order before allocations.");
        var scope=_apiClient.CompanySnapshot;if(scope.Selection is null)throw new InvalidOperationException("Select a company before reviewing allocations.");
        var route="/api/accounting/traditional/ap/"+Uri.EscapeDataString(invoiceId)+"/lines/"+Uri.EscapeDataString(lineId)+"/receipt-allocations?expectedInvoiceRevision="+Uri.EscapeDataString(invoiceRevision)+"&expectedOrderRevision="+Uri.EscapeDataString(orderRevision);
        var review=await _apiClient.GetAsync<Beep.OilandGas.Models.Data.Accounting.APReceiptAllocationReview>(route) ?? throw new InvalidOperationException("Recorded allocations were not returned.");
        if(scope!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company changed during allocation review.");
        review.Require(scope.Selection.Scope,scope.Selection.UserId,invoiceId,lineId,invoiceRevision);
        if(review.Source.Order.Revision!=orderRevision)throw new InvalidOperationException("The reviewed order changed.");return review;
    }
    public async Task<Beep.OilandGas.Models.Data.Accounting.APLinePurchaseSourceReview> GetAPLinePurchaseSourceAsync(string invoiceId,string lineId,string expectedInvoiceRevision)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invoiceId);ArgumentException.ThrowIfNullOrWhiteSpace(lineId);
        if(!Beep.OilandGas.Models.Data.Process.ProcessDefinitionRevision.IsValid(expectedInvoiceRevision))throw new InvalidOperationException("Reload the AP source before reviewing its order.");
        var scope=_apiClient.CompanySnapshot;if(scope.Selection is null)throw new InvalidOperationException("Select a company before reviewing AP purchase sources.");
        var route="/api/accounting/traditional/ap/"+Uri.EscapeDataString(invoiceId)+"/lines/"+Uri.EscapeDataString(lineId)+"/purchase-source?expectedInvoiceRevision="+Uri.EscapeDataString(expectedInvoiceRevision);
        var review=await _apiClient.GetAsync<Beep.OilandGas.Models.Data.Accounting.APLinePurchaseSourceReview>(route) ?? throw new InvalidOperationException("The AP purchase source review was not returned.");
        if(scope!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company changed during AP purchase source review.");
        review.RequireOrderId(scope.Selection.Scope,scope.Selection.UserId,invoiceId,lineId,expectedInvoiceRevision);return review;
    }
    public async Task<List<AP_INVOICE>> GetCompanyAPInvoicesAsync(string? vendorId=null)
    {
        if(vendorId is not null && string.IsNullOrWhiteSpace(vendorId))throw new ArgumentException("A supplied supplier restriction cannot be empty.",nameof(vendorId));
        var scope=_apiClient.CompanySnapshot;
        if(scope.Selection is null)throw new InvalidOperationException("Select a company before reading AP sources.");
        var route="/api/accounting/traditional/ap"+(vendorId is null?"":"?vendorId="+Uri.EscapeDataString(vendorId));
        var rows=await _apiClient.GetAsync<List<AP_INVOICE>>(route) ?? throw new InvalidOperationException("The AP company source list was not returned.");
        if(scope!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company changed during AP read.");
        Beep.OilandGas.Models.Data.Accounting.APInvoiceOwnership.RequireRows(scope.Selection.Scope,rows);
        if(vendorId is not null)Beep.OilandGas.Models.Data.Accounting.APInvoiceOwnership.RequireVendorRows(scope.Selection.Scope,vendorId,rows);
        return rows;
    }
    public async Task<List<AP_INVOICE>> GetVendorAPInvoicesAsync(string vendorId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vendorId);var scope=_apiClient.CompanySnapshot;
        if(scope.Selection is null)throw new InvalidOperationException("Select a company before reading AP sources.");
        var rows=await _apiClient.GetAsync<List<AP_INVOICE>>("/api/accounting/traditional/ap/vendor/"+Uri.EscapeDataString(vendorId)+"/invoices")
            ?? throw new InvalidOperationException("The AP invoice source list was not returned.");
        if(scope!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company changed during AP read.");
        Beep.OilandGas.Models.Data.Accounting.APInvoiceOwnership.RequireVendorRows(scope.Selection.Scope,vendorId,rows);return rows;
    }
    public async Task<Beep.OilandGas.Models.Data.Accounting.APInvoiceSourceReview?> GetAPInvoiceAsync(string invoiceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invoiceId);var scope=_apiClient.CompanySnapshot;
        if(scope.Selection is null)throw new InvalidOperationException("Select a company before reading AP sources.");
        var review=await _apiClient.GetAsync<Beep.OilandGas.Models.Data.Accounting.APInvoiceSourceReview>("/api/accounting/traditional/ap/"+Uri.EscapeDataString(invoiceId))
            ?? throw new InvalidOperationException("The AP invoice source review was not returned.");
        if(scope!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company changed during AP read.");
        review.Require(scope.Selection.Scope,scope.Selection.UserId,invoiceId);return review;
    }

    public async Task<List<PURCHASE_ORDER>> GetCompanyPurchaseOrdersAsync(string? vendorId=null)
    {
        if(vendorId is not null && string.IsNullOrWhiteSpace(vendorId))throw new ArgumentException("A supplied supplier restriction cannot be empty.",nameof(vendorId));
        var scope=_apiClient.CompanySnapshot;
        if(scope.Selection is null)throw new InvalidOperationException("Select a company before reading purchase orders.");
        var route="/api/accounting/traditional/purchase-order"+(vendorId is null?"":"?vendorId="+Uri.EscapeDataString(vendorId));
        var rows=await _apiClient.GetAsync<List<PURCHASE_ORDER>>(route) ?? throw new InvalidOperationException("The company purchase-order source list was not returned.");
        if(scope!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company changed during order read.");
        Beep.OilandGas.Models.Data.Accounting.PurchaseOrderOwnership.RequireRows(scope.Selection.Scope,rows);
        if(vendorId is not null)Beep.OilandGas.Models.Data.Accounting.PurchaseOrderOwnership.RequireVendorRows(scope.Selection.Scope,vendorId,rows);
        return rows;
    }
    public async Task<List<PURCHASE_ORDER>> GetVendorPurchaseOrdersAsync(string vendorId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vendorId);var scope=_apiClient.CompanySnapshot;
        if(scope.Selection is null)throw new InvalidOperationException("Select a company before reading purchase orders.");
        var rows=await _apiClient.GetAsync<List<PURCHASE_ORDER>>("/api/accounting/traditional/purchase-order/vendor/"+Uri.EscapeDataString(vendorId)+"/orders")
            ?? throw new InvalidOperationException("The purchase-order source list was not returned.");
        if(scope!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company changed during purchase-order read.");
        Beep.OilandGas.Models.Data.Accounting.PurchaseOrderOwnership.RequireVendorRows(scope.Selection.Scope,vendorId,rows);
        return rows;
    }
    public async Task<Beep.OilandGas.Models.Data.Accounting.PurchaseOrderSourceReview?> GetPurchaseOrderAsync(string orderId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderId);var scope=_apiClient.CompanySnapshot;
        if(scope.Selection is null)throw new InvalidOperationException("Select a company before reading purchase orders.");
        var row=await _apiClient.GetAsync<Beep.OilandGas.Models.Data.Accounting.PurchaseOrderSourceReview>("/api/accounting/traditional/purchase-order/"+Uri.EscapeDataString(orderId));
        if(scope!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company changed during purchase-order read.");
        if(row is null)throw new InvalidOperationException("The recorded purchase-order source review was empty.");
        row.Require(scope.Selection.Scope,scope.Selection.UserId,orderId);return row;
    }

    public async Task<List<AccountingActivityDto>> GetRecentActivitiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<AccountingActivityDto>>(
                "/api/field/current/accounting/recent-activities",
                cancellationToken);
            return result ?? new List<AccountingActivityDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent accounting activities.");
            throw;
        }
    }

    public async Task<ProductionAccountingSummaryDto?> GetProductionSummaryAsync(DateTime? periodEnd = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = "/api/field/current/accounting/production-summary";
            if (periodEnd.HasValue)
                endpoint += $"?periodEnd={Uri.EscapeDataString(periodEnd.Value.ToString("yyyy-MM-dd"))}";

            return await _apiClient.GetAsync<ProductionAccountingSummaryDto>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting production accounting summary for period end {PeriodEnd}.", periodEnd);
            throw;
        }
    }

    public async Task<List<RevenueLine>> GetRevenueLinesAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new List<string>();
            if (startDate.HasValue)
                query.Add($"startDate={Uri.EscapeDataString(startDate.Value.ToString("yyyy-MM-dd"))}");
            if (endDate.HasValue)
                query.Add($"endDate={Uri.EscapeDataString(endDate.Value.ToString("yyyy-MM-dd"))}");

            var endpoint = "/api/field/current/accounting/revenue-lines";
            if (query.Count > 0)
                endpoint += "?" + string.Join("&", query);

            var result = await _apiClient.GetAsync<List<RevenueLine>>(endpoint, cancellationToken);
            return result ?? new List<RevenueLine>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue lines from {StartDate} to {EndDate}.", startDate, endDate);
            throw;
        }
    }

    public async Task<VolumeReconciliationResult?> ReconcileVolumesAsync(VolumeReconciliationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PostAsync<VolumeReconciliationRequest, VolumeReconciliationResult>(
                "/api/accounting/allocation/reconcile",
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reconciling accounting volumes for field {FieldId}.", request.FieldId);
            throw;
        }
    }

    public async Task<CostAllocationComputationResult?> AllocateCostsAsync(CostAllocationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PostAsync<CostAllocationRequest, CostAllocationComputationResult>(
                "/api/accounting/cost/allocations/allocate",
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error allocating accounting costs for field {FieldId}.", request.FieldId);
            throw;
        }
    }

    public async Task<List<ROYALTY_CALCULATION>> GetRoyaltyCalculationsAsync(string fieldId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldId);
        try
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(fieldId))
                query.Add($"fieldId={Uri.EscapeDataString(fieldId)}");
            if (startDate.HasValue)
                query.Add($"startDate={Uri.EscapeDataString(startDate.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture))}");
            if (endDate.HasValue)
                query.Add($"endDate={Uri.EscapeDataString(endDate.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture))}");

            var endpoint = "/api/accounting/royalty/calculations";
            if (query.Count > 0)
                endpoint += "?" + string.Join("&", query);

            var result = await _apiClient.GetAsync<List<ROYALTY_CALCULATION>>(endpoint, cancellationToken);
            return result ?? throw new InvalidOperationException("The royalty report response was empty.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting royalty calculations for field {FieldId} from {StartDate} to {EndDate}.", fieldId, startDate, endDate);
            throw;
        }
    }

    public Task<ROYALTY_CALCULATION> GetRoyaltyAsync(string calculationId, CancellationToken cancellationToken = default)
        => GetRequiredRoyaltyResourceAsync<ROYALTY_CALCULATION>(calculationId, "service/calculations", "", cancellationToken);

    public Task<List<ROYALTY_PAYMENT>> GetRoyaltyPaymentsAsync(string calculationId, CancellationToken cancellationToken = default)
        => GetRequiredRoyaltyResourceAsync<List<ROYALTY_PAYMENT>>(calculationId, "calculations", "/payments", cancellationToken);

    public Task<List<RoyaltyPostingReview>> GetRoyaltyPostingReviewAsync(string calculationId, CancellationToken cancellationToken = default)
        => GetRequiredRoyaltyResourceAsync<List<RoyaltyPostingReview>>(calculationId, "calculations", "/posting-review", cancellationToken);

    private async Task<T> GetRequiredRoyaltyResourceAsync<T>(string id, string resource, string suffix, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return await _apiClient.GetAsync<T>($"/api/accounting/royalty/{resource}/{Uri.EscapeDataString(id)}{suffix}", cancellationToken)
            ?? throw new InvalidOperationException("The royalty response was empty.");
    }

    public async Task<List<ROYALTY_CALCULATION>> PreviewRoyaltiesAsync(PreviewRoyaltiesRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PostAsync<PreviewRoyaltiesRequest, List<ROYALTY_CALCULATION>>(
                "/api/accounting/royalty/preview",
                request,
                cancellationToken) ?? throw new InvalidOperationException("The royalty preview response was empty.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating royalties for field {FieldId}.", request.FieldId);
            throw;
        }
    }

    public async Task<Beep.OilandGas.Accounting.Models.AccountingPeriodCheckReview> GetPeriodChecksAsync(DateTime periodEnd,string bookId,CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bookId);
        if(periodEnd==default || periodEnd.TimeOfDay!=TimeSpan.Zero)throw new ArgumentException("Choose the period end date.");
        return await _apiClient.GetAsync<Beep.OilandGas.Accounting.Models.AccountingPeriodCheckReview>(
            $"/api/field/current/accounting/period-checks?periodEnd={periodEnd.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture)}&bookId={Uri.EscapeDataString(bookId)}",cancellationToken)
            ?? throw new InvalidOperationException("Executed period checks were not returned.");
    }

    public async Task ClosePeriodAsync(CloseAccountingPeriodRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            await _apiClient.PostAsync(
                "/api/field/current/accounting/close-period",
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing accounting period ending {PeriodEnd}.", request.PeriodEnd);
            throw;
        }
    }

    public async Task<PeriodCloseReadinessResult> GetCloseReadinessAsync(DateTime periodEnd, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = "/api/field/current/accounting/close-readiness?periodEnd="
                + Uri.EscapeDataString(periodEnd.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
            return await _apiClient.GetAsync<PeriodCloseReadinessResult>(endpoint, cancellationToken)
                ?? throw new InvalidOperationException("The period close readiness response was empty.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking whether the accounting period ending {PeriodEnd} may be closed.", periodEnd);
            throw;
        }
    }

    public async Task<JOINT_INTEREST_STATEMENT> GenerateJibStatementAsync(GenerateJIBStatementRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var report = await _apiClient.PostAsync<GenerateJIBStatementRequest, JibStatementReport>(
                "/api/accounting/reporting/jib-statement",
                request,
                cancellationToken);
            return report?.ReportData ?? throw new InvalidOperationException("The JIB statement response carried no statement.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating the JIB statement for lease {LeaseId}.", request.LeaseId);
            throw;
        }
    }

    /// <summary>
    /// The reporting endpoint's <c>ReportResult</c>, read with the statement it carries as a statement: its
    /// <c>ReportData</c> is declared <see cref="object"/>, which would arrive as raw JSON.
    /// </summary>
    private sealed class JibStatementReport
    {
        public JOINT_INTEREST_STATEMENT? ReportData { get; set; }
    }
}

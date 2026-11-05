using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Security.Claims;
using Beep.OilandGas.Models.Data.Security;
namespace Beep.OilandGas.ApiService.Services;
public static class APInvoiceReadChecks
{
    public static async Task<APInvoiceFinancialReview> ReadFinancialReviewAsync(ClaimsPrincipal user,string actor,string id,string revision,
        Func<Task<(RepositoryCompanyAccess Access,string Binding)>> current,
        Func<CompanyScope,string,string,Task<APInvoiceSourceReview?>> readInvoice,
        Func<CompanyScope,string,string,Task<PurchaseOrderSourceReview?>> readOrder,
        Func<CompanyScope,string,string,Task<List<Beep.OilandGas.Models.Data.ProductionAccounting.AP_RECEIPT_ALLOCATION>>> readAllocations,
        Func<CompanyScope,string,Task<CompanyMatchingPolicyReview>> readPolicies,
        Func<CompanyScope,string,APLinePurchaseSourceReview,string,Func<Task>,Task<APReceiptConsumptionReview>> readConsumption)
    {
        if(!ProcessDefinitionRevision.IsValid(revision))throw new InvalidOperationException("A displayed invoice revision is required.");
        var original=await current();
        if(original.Access is null || !CanRead(user,original.Access.Scope,original.Access) || string.IsNullOrWhiteSpace(original.Binding))throw new UnauthorizedAccessException("Current company invoice read authority is required.");
        var scope=original.Access.Scope;
        var roles=original.Access.Roles.Order(StringComparer.Ordinal).ToArray();var permissions=original.Access.Permissions.Order(StringComparer.Ordinal).ToArray();
        async Task<(RepositoryCompanyAccess Access,string Binding)> Guard()
        {
            var value=await current();
            if(!CanRead(user,scope,value.Access) || value.Binding!=original.Binding || string.IsNullOrWhiteSpace(value.Binding) ||
                !value.Access.Roles.Order(StringComparer.Ordinal).SequenceEqual(roles) || !value.Access.Permissions.Order(StringComparer.Ordinal).SequenceEqual(permissions))
                throw new UnauthorizedAccessException("The original company invoice financial read authority changed.");
            return value;
        }
        async Task<APInvoiceFinancialReview> Capture()
        {
            await Guard();var invoice=await readInvoice(scope,id,actor) ?? throw new InvalidOperationException("The recorded invoice is unavailable.");
            invoice.Require(scope,actor,id);
            if(invoice.Revision!=revision || invoice.Lines.Count>128)throw new InvalidOperationException("The displayed invoice changed or exceeds the bounded financial review.");
            var purchases=new List<APReceiptAllocationReview>();
            var consumption=new List<APReceiptConsumptionReview>();
            foreach(var line in invoice.Lines.Where(line=>!string.IsNullOrWhiteSpace(line.PURCHASE_ORDER_ID)).OrderBy(line=>line.AP_INVOICE_LINE_ID,StringComparer.Ordinal))
            {
                var source=await ReadPurchaseSourceAsync(user,actor,id,line.AP_INVOICE_LINE_ID,revision,Guard,readInvoice,readOrder);
                var allocation=await ReadReceiptAllocationsAsync(user,actor,id,line.AP_INVOICE_LINE_ID,revision,source.Order.Revision,Guard,readInvoice,readOrder,readAllocations);
                purchases.Add(allocation);
                foreach(var receiptId in allocation.Allocations.Select(row=>row.PO_RECEIPT_ID).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
                {
                    await Guard();
                    var receipt=await readConsumption(scope,actor,allocation.Source,receiptId,async()=>{await Guard();});
                    receipt.Require(scope,actor,allocation.Source,receiptId);consumption.Add(receipt);await Guard();
                }
            }
            await Guard();var policies=await readPolicies(scope,actor);policies.Require(scope,actor);await Guard();return APInvoiceFinancialReview.Create(invoice,purchases,policies,consumption);
        }
        var initial=await Capture();var closing=await Capture();await Guard();
        if(initial.Revision!=closing.Revision)throw new InvalidOperationException("The whole invoice financial source changed during review.");
        return closing;
    }
    public static bool CanRead(ClaimsPrincipal user,CompanyScope expected,RepositoryCompanyAccess? current)=>
        user.Identity?.IsAuthenticated==true && current is not null && current.Scope==expected &&
        current.Roles is not null && current.Roles.Any(user.IsInRole) &&
        current.Permissions?.Contains(PermissionConstants.Accounting.View,StringComparer.Ordinal)==true;
    /// <summary>Review one recorded AP-to-PO reference under fresh authority at every read boundary.</summary>
    public static async Task<APLinePurchaseSourceReview> ReadPurchaseSourceAsync(
        ClaimsPrincipal user, string actor, string invoiceId, string lineId, string expectedInvoiceRevision,
        Func<Task<(RepositoryCompanyAccess Access, string Binding)>> current,
        Func<CompanyScope,string,string,Task<APInvoiceSourceReview?>> readInvoice,
        Func<CompanyScope,string,string,Task<PurchaseOrderSourceReview?>> readOrder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actor);
        ArgumentException.ThrowIfNullOrWhiteSpace(invoiceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(lineId);
        if (!ProcessDefinitionRevision.IsValid(expectedInvoiceRevision))
            throw new InvalidOperationException("Reload the invoice source before reviewing its order.");
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(readInvoice);
        ArgumentNullException.ThrowIfNull(readOrder);
        var original = await current();
        if (original.Access is null || string.IsNullOrWhiteSpace(original.Binding))
            throw new UnauthorizedAccessException("Current company and production binding are required.");
        var scope = original.Access.Scope;
        void RequireAuthority((RepositoryCompanyAccess Access,string Binding) evidence)
        {
            if (!CanRead(user,scope,evidence.Access) ||
                !PurchaseOrderReadChecks.CanRead(user,scope,evidence.Access) ||
                !string.Equals(evidence.Binding,original.Binding,StringComparison.Ordinal))
                throw new UnauthorizedAccessException("Company AP/purchase authority or binding changed.");
        }
        RequireAuthority(original);
        async Task<APLinePurchaseSourceReview> Capture()
        {
            RequireAuthority(await current());
            var invoice = await readInvoice(scope,invoiceId,actor) ??
                throw new InvalidOperationException("The owned invoice is unavailable.");
            invoice.Require(scope,actor,invoiceId);
            if (invoice.Revision != expectedInvoiceRevision)
                throw new InvalidOperationException("The invoice changed.");
            var line = invoice.Lines.SingleOrDefault(row => row.AP_INVOICE_LINE_ID == lineId);
            if (line is null || string.IsNullOrWhiteSpace(line.PURCHASE_ORDER_ID) ||
                string.IsNullOrWhiteSpace(line.PO_LINE_ITEM_ID))
                throw new InvalidOperationException("The line reference is unavailable.");
            RequireAuthority(await current());
            var order = await readOrder(scope,line.PURCHASE_ORDER_ID,actor) ??
                throw new InvalidOperationException("The owned order is unavailable.");
            PurchaseOrderReadChecks.RequireInventoryEvidence(order,original.Access);
            var review = new APLinePurchaseSourceReview(invoice,lineId,order);
            review.RequireOrderId(scope,actor,invoiceId,lineId,expectedInvoiceRevision);
            return review;
        }
        var initial = await Capture();
        var closing = await Capture();
        var closingAuthority=await current();RequireAuthority(closingAuthority);PurchaseOrderReadChecks.RequireInventoryEvidence(closing.Order,closingAuthority.Access);
        if (initial.Order.Revision != closing.Order.Revision)
            throw new InvalidOperationException("The order changed during source review.");
        return closing;
    }
    /// <summary>Allocation history reviewed against both exact displayed source revisions and fresh authority.</summary>
    public static async Task<APReceiptAllocationReview> ReadReceiptAllocationsAsync(
        ClaimsPrincipal user,string actor,string invoiceId,string lineId,string invoiceRevision,string orderRevision,
        Func<Task<(RepositoryCompanyAccess Access,string Binding)>> current,
        Func<CompanyScope,string,string,Task<APInvoiceSourceReview?>> readInvoice,
        Func<CompanyScope,string,string,Task<PurchaseOrderSourceReview?>> readOrder,
        Func<CompanyScope,string,string,Task<List<Beep.OilandGas.Models.Data.ProductionAccounting.AP_RECEIPT_ALLOCATION>>> readAllocations)
    {
        if(!ProcessDefinitionRevision.IsValid(invoiceRevision) || !ProcessDefinitionRevision.IsValid(orderRevision))
            throw new InvalidOperationException("Review the invoice and owned order before reviewing allocations.");
        ArgumentNullException.ThrowIfNull(current);ArgumentNullException.ThrowIfNull(readAllocations);
        var original=await current();
        if(original.Access is null || string.IsNullOrWhiteSpace(original.Binding))throw new UnauthorizedAccessException("Original company binding is required.");
        var scope=original.Access.Scope;
        async Task<(RepositoryCompanyAccess Access,string Binding)> GuardedCurrent()
        {
            var evidence=await current();
            if(!CanRead(user,scope,evidence.Access) || !PurchaseOrderReadChecks.CanRead(user,scope,evidence.Access) || evidence.Binding!=original.Binding)
                throw new UnauthorizedAccessException("Company allocation authority or binding changed.");
            return evidence;
        }
        if(!CanRead(user,scope,original.Access) || !PurchaseOrderReadChecks.CanRead(user,scope,original.Access))throw new UnauthorizedAccessException("Accounting and procurement read access are required.");
        async Task<APReceiptAllocationReview> Capture()
        {
            var source=await ReadPurchaseSourceAsync(user,actor,invoiceId,lineId,invoiceRevision,GuardedCurrent,readInvoice,readOrder);
            if(source.Order.Revision!=orderRevision)throw new InvalidOperationException("The reviewed order changed.");
            await GuardedCurrent();
            var rows=await readAllocations(scope,invoiceId,lineId);
            await GuardedCurrent();
            return APReceiptAllocationReview.Create(scope,actor,invoiceId,lineId,invoiceRevision,source,rows);
        }
        var initial=await Capture();var closing=await Capture();
        if(initial.Revision!=closing.Revision)throw new InvalidOperationException("Recorded allocation evidence changed during review.");
        return closing;
    }
}

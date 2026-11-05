using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using System.Security.Claims;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Processes;
namespace Beep.OilandGas.ApiService.Services;

/// <summary>One company process/native source read boundary for detail and assigned work.</summary>
public sealed partial class CompanyAccountingProcessReads(ICompanyAccountingProcessReadContext context, IAccessControlService resources,
    APInvoiceService invoices, PurchaseOrderService orders, ICompanyAccountingReadContext production, AccountingPolicyService policies,CompanyFieldRevenueReads? commercial=null,CompanySalesInvoiceReads? salesInvoices=null,CompanySalesReceivableReads? salesReceivables=null,JournalEntryService? journals=null)
{
    public Task<ProcessInstance?> ReadAsync(ClaimsPrincipal user, string id, CompanyScope? expectedCompany = null, string? expectedActor = null)
        =>ReadCoreAsync(user,id,expectedCompany,expectedActor,null);
    private async Task<(ProcessInstance? Process,CompanySalesReceivableReviewSource? Receivable,CompanySalesJournalWorkflowSource? Journal,CompanySalesPaymentWorkflowSource? Payment)> ReadWithNativeSourcesAsync(ClaimsPrincipal user,string id,CompanyScope company,string actor)
    {
        CompanySalesReceivableReviewSource? captured=null;CompanySalesJournalWorkflowSource? journal=null;
        CompanySalesPaymentWorkflowSource? payment=null;var process=await ReadCoreAsync(user,id,company,actor,source=>captured=source,source=>journal=source,source=>payment=source);
        // Publication occurs only after the existing process/source initial and closing checks pass.
        return(process,process is null?null:captured,process is null?null:journal,process is null?null:payment);
    }
    private async Task<ProcessInstance?> ReadCoreAsync(ClaimsPrincipal user,string id,CompanyScope? expectedCompany,string? expectedActor,Action<CompanySalesReceivableReviewSource>? captureReceivable,Action<CompanySalesJournalWorkflowSource>? captureJournal=null,Action<CompanySalesPaymentWorkflowSource>? capturePayment=null)
    {
        using var invoiceValidation=CompanySalesInvoiceValidation.Enter();
        var original = await context.CaptureProcessAsync(user);
        var inventorySourceRequired=false;
        if (expectedCompany is not null && original.Access.Scope != expectedCompany || expectedActor is not null && original.Actor != expectedActor)
            throw new UnauthorizedAccessException("The assigned work company or actor changed.");
        async Task<(RepositoryCompanyAccess Access, string Binding, string Actor)> Current()
        {
            var current = await context.CaptureProcessAsync(user);
            if(inventorySourceRequired && current.Access.Permissions?.Contains("Inventory.View",StringComparer.Ordinal)!=true)throw new UnauthorizedAccessException("Current purchase workflow inventory read authority is required.");
            return (current.Access, current.Binding, current.Actor);
        }
        async Task<string> SourceRevision(CompanyScope company, string actor, string type, string entity)
        {
            if(type=="AR_PAYMENT")return (await ReadSalesPaymentSourceAsync(user,company,actor,entity)).SourceRevision;
            if(type=="JOURNAL_ENTRY")return (await ReadSalesJournalSourceAsync(user,company,actor,entity)).SourceRevision;
            if(type=="AR_INVOICE")return CompanyAccountingSourceTypes.ReceivablePreparationRevision((await ReadSalesReceivableSourceAsync(user,company,actor,entity)).Original);
            if(type=="INVOICE")return CompanyAccountingSourceTypes.InvoicePreparationRevision((await ReadSalesInvoiceSourceAsync(user,company,actor,entity)).Original);
            if(type=="RUN_TICKET")return CompanyAccountingSourceTypes.DeliveryPreparationRevision(await ReadDeliverySourceAsync(user,company,actor,entity));
            if(type=="SALES_CONTRACT")return CompanyAccountingSourceTypes.CommercialPreparationRevision(await ReadCommercialSourceAsync(user,company,actor,entity));
            if (type == "AP_INVOICE")
            {
                var source = await invoices.GetSourceReviewAsync(company, entity, actor) ?? throw new InvalidOperationException("The owned invoice source is unavailable.");
                source.Require(company, actor, entity); return source.ForCompanyWorkflow().Revision;
            }
            if (type == "PURCHASE_ORDER")
            {
                var source = await orders.GetSourceReviewAsync(company, entity, actor) ?? throw new InvalidOperationException("The owned order source is unavailable.");
                source.Require(company, actor, entity);inventorySourceRequired=source.Lines.Any(row=>row.INVENTORY_LINK_REVIEW_JSON is not null || row.INVENTORY_LINK_DECISION_JSON is not null || row.INVENTORY_LINK_HISTORY_REVISION is not null);
                _=await ReadPurchaseMakerLinesAsync(user,source);return source.Revision;
            }
            throw new InvalidOperationException("The company accounting process source type is unsupported.");
        }
        async Task<string> BoundSourceRevision(ProcessInstance process)
        {
            if(process.EntityType=="AR_PAYMENT")
            {
                var paymentStart=NativeDecisionStart(process,"AR_PAYMENT");var payment=await ReadSalesPaymentSourceAsync(user,original.Access.Scope,original.Actor,process.EntityId,paymentStart.SourceRevision);if(paymentStart.Company!=original.Access.Scope || payment.Payment.CreatedBy!=paymentStart.SourcePreparerUserId)throw new UnauthorizedAccessException("The original payment workflow maker changed.");capturePayment?.Invoke(payment);return payment.Revision;
            }
            if(process.EntityType=="JOURNAL_ENTRY")
            {
                var journalStart=NativeDecisionStart(process,"JOURNAL_ENTRY");var journal=await ReadSalesJournalSourceAsync(user,original.Access.Scope,original.Actor,process.EntityId,journalStart.SourceRevision);
                if(journalStart.Company!=original.Access.Scope || journal.Journal.Header.ROW_CREATED_BY!=journalStart.SourcePreparerUserId)throw new UnauthorizedAccessException("The original journal workflow maker changed.");captureJournal?.Invoke(journal);return journal.Revision;
            }
            if(process.EntityType=="AR_INVOICE")
            {
                var receivableStart=NativeDecisionStart(process,"AR_INVOICE");var receivableSource=await ReadSalesReceivableSourceAsync(user,original.Access.Scope,original.Actor,process.EntityId,receivableStart.SourceRevision);
                if(receivableStart.Company!=original.Access.Scope || receivableSource.Original.Intent.ROW_CREATED_BY!=receivableStart.SourcePreparerUserId)throw new UnauthorizedAccessException("The original receivable workflow maker changed.");
                captureReceivable?.Invoke(receivableSource);
                // Require() already verifies this digest over the complete current review, including the native disposition.
                return receivableSource.Revision;
            }
            if(process.EntityType=="INVOICE")
            {
                var invoiceStart=NativeDecisionStart(process,"INVOICE");var invoice=await ReadSalesInvoiceSourceAsync(user,original.Access.Scope,original.Actor,process.EntityId,invoiceStart.SourceRevision);
                if(invoiceStart.Company!=original.Access.Scope || invoice.Original.Intent.ROW_CREATED_BY!=invoiceStart.SourcePreparerUserId)throw new UnauthorizedAccessException("The original invoice workflow maker changed.");
                return ProcessDefinitionRevision.FromJson(System.Text.Json.JsonSerializer.Serialize(invoice));
            }
            if(process.EntityType=="RUN_TICKET")
            {
                if(commercial is null)throw new InvalidOperationException("The canonical delivery source reader is required.");
                var deliveryStart=NativeDecisionStart(process,"RUN_TICKET");
                var deliverySource=await commercial.ReadProcessDeliveryAsync(user,process.EntityId,deliveryStart.SourceRevision);
                if(deliveryStart.Company!=original.Access.Scope || deliverySource.Source.Source.Source.Company!=original.Access.Scope || deliverySource.Source.Source.Source.ActorUserId!=original.Actor || deliverySource.Recorded?.Preparation.ActorUserId!=deliveryStart.SourcePreparerUserId)
                    throw new UnauthorizedAccessException("The original delivery process company, actor or preparer changed.");
                return ProcessDefinitionRevision.FromJson(System.Text.Json.JsonSerializer.Serialize(deliverySource));
            }
            if(process.EntityType!="SALES_CONTRACT")return await SourceRevision(original.Access.Scope,original.Actor,process.EntityType,process.EntityId);
            if(commercial is null)throw new InvalidOperationException("The canonical commercial source reader is required.");
            var start=CommercialStart(process);
            if(start.Company!=original.Access.Scope)throw new UnauthorizedAccessException("The original commercial handoff company changed.");
            var source=await commercial.ReadProcessTermsAsync(user,process.EntityId,start.SourceRevision,process.Status==ProcessStatus.COMPLETED && process.CurrentState=="REJECTION_REVIEWED");
            if(source.Terms.Source.Source.Company!=original.Access.Scope || source.Terms.Source.Source.ActorUserId!=original.Actor || source.Terms.Recorded?.Preparation.ActorUserId!=start.SourcePreparerUserId)
                throw new UnauthorizedAccessException("The original commercial process company or actor changed.");
            return ProcessDefinitionRevision.FromJson(System.Text.Json.JsonSerializer.Serialize(new{source.Terms,source.CurrentNativeContract}));
        }
        return await new CompanyProcessReadService(resources).ReadAccountingAsync(original.Process, user, original.Actor, original.Access.Scope,
            original.Binding, id, Current, SourceRevision,BoundSourceRevision);
    }
    private async Task<IReadOnlyList<PO_LINE_ITEM>> ReadPurchaseMakerLinesAsync(ClaimsPrincipal user,PurchaseOrderSourceReview source)
    {
        var histories=new List<PurchaseOrderInventoryLinkHistoryReview>();
        if(source.Lines.Any(row=>row.INVENTORY_LINK_REVIEW_JSON is not null || row.INVENTORY_LINK_DECISION_JSON is not null || row.INVENTORY_LINK_HISTORY_REVISION is not null))
        {
            var original=await production.CaptureAsync(user);
            if(!PurchaseOrderReadChecks.CanRead(user,source.Company,original.Access) || original.Actor!=source.ActorUserId || string.IsNullOrWhiteSpace(original.Binding) || original.Access.Permissions is null)
                throw new UnauthorizedAccessException("Current owned purchase workflow authority is required.");
            PurchaseOrderReadChecks.RequireInventoryEvidence(source,original.Access);
            async Task Authority()
            {
                var current=await production.CaptureAsync(user);
                if(!PurchaseOrderReadChecks.CanRead(user,source.Company,current.Access) || current.Actor!=source.ActorUserId || current.Binding!=original.Binding ||
                    !System.Text.Json.JsonSerializer.Serialize(current.Access.Roles.Order(StringComparer.Ordinal)).Equals(System.Text.Json.JsonSerializer.Serialize(original.Access.Roles.Order(StringComparer.Ordinal)),StringComparison.Ordinal) ||
                    !System.Text.Json.JsonSerializer.Serialize(current.Access.Permissions.Order(StringComparer.Ordinal)).Equals(System.Text.Json.JsonSerializer.Serialize(original.Access.Permissions.Order(StringComparer.Ordinal)),StringComparison.Ordinal))
                    throw new UnauthorizedAccessException("Original purchase workflow inventory authority changed.");
                PurchaseOrderReadChecks.RequireInventoryEvidence(source,current.Access);
            }
            await Authority();
            foreach(var line in source.Lines.Where(row=>row.INVENTORY_LINK_HISTORY_REVISION is not null))
                histories.Add(await orders.GetInventoryLinkHistoryAsync(source.Company,source.Header.PURCHASE_ORDER_ID,line.PO_LINE_ITEM_ID,source.ActorUserId,Authority));
            await Authority();
        }
        return source.WorkflowMakerLines(histories);
    }
}

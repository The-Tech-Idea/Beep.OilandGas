using System.Security.Claims;
using System.Text.Json;
using TheTechIdea.Data.OilGas;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.ApiService.Services;

public sealed partial class CompanyAccountingProcessReads
{
    public async Task<CompanyAccountingProcessStartReview> ReviewStartAsync(ClaimsPrincipal user,CompanyAccountingProcessStartQuery query)
    {
        using var invoiceValidation=CompanySalesInvoiceValidation.Enter();
        ArgumentNullException.ThrowIfNull(query);query.Require();
        var original=await context.CaptureProcessAsync(user);
        void RequireAuthority(RepositoryCompanyAccess access,string binding,string actor)
        {
            if(user.Identity?.IsAuthenticated!=true || access.Scope!=original.Access.Scope || actor!=original.Actor || binding!=original.Binding ||
                string.IsNullOrWhiteSpace(actor) || !ProcessDefinitionRevision.IsValid(binding) || access.Roles is null || !access.Roles.Any(user.IsInRole) ||
                access.Permissions?.Contains("Workflow.Start",StringComparer.Ordinal)!=true ||
                access.Permissions.Contains(CompanyAccountingSourceTypes.ReadPermission(query.EntityType),StringComparer.Ordinal)!=true)
                throw new UnauthorizedAccessException("Current company source and workflow start authority are required.");
        }
        async Task<CompanyAccountingProcessStartReview> Capture()
        {
            var current=await context.CaptureProcessAsync(user);RequireAuthority(current.Access,current.Binding,current.Actor);
            var definition=await original.Process.GetProcessDefinitionAsync(query.ProcessId) ?? throw new InvalidOperationException("The recorded workflow definition is unavailable.");
            if(definition.ProcessId!=query.ProcessId || !definition.IsActive || definition.EntityType!=query.EntityType || definition.Steps is null || definition.Steps.Count==0 || definition.Steps.Any(step=>step is null))
                throw new InvalidOperationException("The recorded workflow does not match this company source.");
            RequireNativeDecisionDefinition(definition);
            var first=definition.Steps.OrderBy(step=>step.SequenceNumber).First();
            if(first.RequiredRoles is null || first.RequiredRoles.Count!=1)throw new InvalidOperationException("A recorded unambiguous initial reviewer role is required.");
            var (revision,preparer)=await ReadStartSourceAsync(user,original.Access.Scope,original.Actor,query.EntityType,query.EntityId,query.ExpectedSourceRevision,query.EntityType=="AR_PAYMENT");
            if(query.EntityType=="SALES_CONTRACT" && (await ReadCommercialSourceAsync(user,original.Access.Scope,original.Actor,query.EntityId)).Decision is not null)
                throw new InvalidOperationException("A decided commercial preparation cannot start a new approval handoff.");
            if(query.EntityType=="RUN_TICKET" && (await ReadDeliverySourceAsync(user,original.Access.Scope,original.Actor,query.EntityId,revision)).Decision is not null)
                throw new InvalidOperationException("A decided delivery preparation cannot start a new approval handoff.");
            if(query.EntityType=="INVOICE" && (await ReadSalesInvoiceSourceAsync(user,original.Access.Scope,original.Actor,query.EntityId,revision)).Original.Invoice.STATUS!="DRAFT")
                throw new InvalidOperationException("A decided invoice cannot start a new approval handoff.");
            if(query.EntityType=="JOURNAL_ENTRY" && (await ReadSalesJournalSourceAsync(user,original.Access.Scope,original.Actor,query.EntityId,revision)).Journal.Header.STATUS!="DRAFT")throw new InvalidOperationException("A decided journal cannot start a new review/posting handoff.");
            if(query.EntityType=="AR_INVOICE" && (await ReadSalesReceivableSourceAsync(user,original.Access.Scope,original.Actor,query.EntityId,revision)).Original.Receivable.STATUS!="DRAFT")
                throw new InvalidOperationException("A decided receivable cannot start a new approval handoff.");
            var review=new CompanyAccountingProcessStartReview(original.Access.Scope,original.Actor,query.ProcessId,query.EntityType,query.EntityId,revision,
                ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition)),original.Binding,preparer,definition.ProcessName,definition.Version,first.RequiredRoles[0]);
            review.Require();
            var closing=await context.CaptureProcessAsync(user);RequireAuthority(closing.Access,closing.Binding,closing.Actor);return review;
        }
        RequireAuthority(original.Access,original.Binding,original.Actor);
        var initial=await Capture();var closing=await Capture();
        if(initial!=closing)throw new InvalidOperationException("The source, preparer or workflow changed during company handoff review.");
        return closing;
    }

    public async Task<CompanyAccountingProcessStartReceipt> StartAsync(ClaimsPrincipal user,CompanyAccountingProcessStart submission)
    {
        ArgumentNullException.ThrowIfNull(submission);submission.Require();
        var intent=JsonSerializer.Deserialize<CompanyAccountingProcessStart>(JsonSerializer.Serialize(submission))!;
        var original=await context.CaptureProcessAsync(user);
        if(original.Access.Scope!=intent.Company || original.Actor!=intent.ActorUserId || original.Binding!=intent.BindingRevision)
            throw new UnauthorizedAccessException("The original submission company, actor or database binding changed.");
        var query=new CompanyAccountingProcessStartQuery(intent.ProcessId,intent.EntityType,intent.EntityId,intent.SourceRevision);
        var review=await ReviewStartAsync(user,query);review.RequireIntent(intent);
        if(original.Process is not ICompanyAccountingProcessCommands commands)throw new InvalidOperationException("A bound native company process command owner is required.");
        async Task Guard()
        {
            // No PRODUCTION source read may join the native LIFECYCLE transaction.
            var current=await context.CaptureProcessAsync(user);
            if(user.Identity?.IsAuthenticated!=true || current.Access.Scope!=intent.Company || current.Actor!=intent.ActorUserId || current.Binding!=intent.BindingRevision ||
                current.Access.Roles is null || !current.Access.Roles.Any(user.IsInRole) || current.Access.Permissions?.Contains("Workflow.Start",StringComparer.Ordinal)!=true ||
                original.Access.Roles is null || original.Access.Permissions is null || !current.Access.Roles.Order(StringComparer.Ordinal).SequenceEqual(original.Access.Roles.Order(StringComparer.Ordinal)) ||
                !current.Access.Permissions.Order(StringComparer.Ordinal).SequenceEqual(original.Access.Permissions.Order(StringComparer.Ordinal)) ||
                current.Access.Permissions.Contains(CompanyAccountingSourceTypes.ReadPermission(intent.EntityType),StringComparer.Ordinal)!=true)
                throw new UnauthorizedAccessException("The original company workflow write authority changed.");
        }
        var receipt=await commands.StartCompanyAccountingAsync(intent,Guard);
        receipt.Require(intent);
        var closing=await ReviewStartAsync(user,query);closing.RequireIntent(intent);
        await Guard();return receipt;
    }
    private async Task<(string Revision,string Preparer)> ReadStartSourceAsync(ClaimsPrincipal user,CompanyScope company,string actor,string type,string id,string? expectedRevision,bool requireUndecidedPayment=false)
    {
        string revision,preparer;DateTime? created;string active;
        static void RequireUnmodifiedMakerAudit(ModelEntityBase header,IEnumerable<ModelEntityBase> lines)
        {
            if(string.IsNullOrWhiteSpace(header.ROW_CREATED_BY) || string.Equals(header.ROW_CREATED_BY,"SYSTEM",StringComparison.OrdinalIgnoreCase) || header.ROW_CREATED_DATE is null || header.ROW_CREATED_DATE==default(DateTime) ||
                !string.IsNullOrEmpty(header.ROW_CHANGED_BY) || header.ROW_CHANGED_DATE is not null ||
                lines.Any(line=>line.ROW_CREATED_BY!=header.ROW_CREATED_BY || line.ROW_CREATED_DATE is null || line.ROW_CREATED_DATE==default(DateTime) ||
                    !string.IsNullOrEmpty(line.ROW_CHANGED_BY) || line.ROW_CHANGED_DATE is not null))
                throw new InvalidOperationException("An unmodified source with complete consistent maker audit is required. Edited or ambiguous preparer evidence needs reconciliation.");
        }
        if(type=="AR_PAYMENT")
        {
            var source=await ReadSalesPaymentSourceAsync(user,company,actor,id,expectedRevision);if(requireUndecidedPayment && source.Payment.Status!="DRAFT")throw new InvalidOperationException("A decided payment cannot start a new review handoff.");revision=source.SourceRevision;preparer=source.Payment.CreatedBy;created=source.Payment.CreatedAtUtc;active="Y";
        }
        else if(type=="JOURNAL_ENTRY")
        {
            var source=await ReadSalesJournalSourceAsync(user,company,actor,id,expectedRevision);revision=source.SourceRevision;preparer=source.Journal.Header.ROW_CREATED_BY;created=source.Journal.Header.ROW_CREATED_DATE;active=source.Journal.Header.ACTIVE_IND;
        }
        else if(type=="AR_INVOICE")
        {
            var source=await ReadSalesReceivableSourceAsync(user,company,actor,id,expectedRevision);if(!source.HasCurrentOriginalSource)throw new InvalidOperationException("The original receivable source changed before handoff review.");
            revision=CompanyAccountingSourceTypes.ReceivablePreparationRevision(source.Original);preparer=source.Original.Intent.ROW_CREATED_BY;created=source.Original.Receivable.ROW_CREATED_DATE;active=source.Original.Receivable.ACTIVE_IND;
        }
        else if(type=="INVOICE")
        {
            var source=await ReadSalesInvoiceSourceAsync(user,company,actor,id,expectedRevision);revision=CompanyAccountingSourceTypes.InvoicePreparationRevision(source.Original);
            preparer=source.Original.Intent.ROW_CREATED_BY;created=source.Original.Invoice.ROW_CREATED_DATE;active=source.Original.Invoice.ACTIVE_IND;
        }
        else if(type=="AP_INVOICE")
        {
            var source=await invoices.GetSourceReviewAsync(company,id,actor) ?? throw new InvalidOperationException("The owned invoice source is unavailable.");
            source.Require(company,actor,id);
            source=source.ForCompanyWorkflow();
            var maker=source.Invoice.VARIANCE_PROPOSAL_JSON is null?source.Invoice:APInvoiceVarianceProof.Require(company,source.Invoice).OriginalInvoice;
            RequireUnmodifiedMakerAudit(maker,source.Lines);
            revision=source.Revision;preparer=maker.ROW_CREATED_BY;created=maker.ROW_CREATED_DATE;active=source.Invoice.ACTIVE_IND;
        }
        else if(type=="RUN_TICKET")
        {
            var source=await ReadDeliverySourceAsync(user,company,actor,id,expectedRevision);
            revision=CompanyAccountingSourceTypes.DeliveryPreparationRevision(source);preparer=source.Preparation.ActorUserId;created=source.PreparedAtUtc;active="Y";
        }
        else if(type=="SALES_CONTRACT")
        {
            var source=await ReadCommercialSourceAsync(user,company,actor,id);
            revision=CompanyAccountingSourceTypes.CommercialPreparationRevision(source);preparer=source.Preparation.ActorUserId;created=source.PreparedAtUtc;active="Y";
        }
        else
        {
            var source=await orders.GetSourceReviewAsync(company,id,actor) ?? throw new InvalidOperationException("The owned purchase source is unavailable.");
            source.Require(company,actor,id);
            RequireUnmodifiedMakerAudit(source.Header,await ReadPurchaseMakerLinesAsync(user,source));
            revision=source.Revision;preparer=source.Header.ROW_CREATED_BY;created=source.Header.ROW_CREATED_DATE;active=source.Header.ACTIVE_IND;
        }
        if(active!="Y" || created is null || created==default(DateTime))throw new InvalidOperationException("An active source with recorded preparer audit is required.");
        if(expectedRevision is not null && expectedRevision!=revision)throw new InvalidOperationException("The displayed company source changed before handoff review.");
        return(revision,preparer);
    }
}

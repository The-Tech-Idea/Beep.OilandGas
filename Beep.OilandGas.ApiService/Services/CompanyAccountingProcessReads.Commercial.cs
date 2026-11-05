using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Models.Processes;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyAccountingProcessReads
{
    public async Task<CompanyCommercialAmendmentQueue> ReadCommercialAmendmentQueueAsync(ClaimsPrincipal user)
        =>(await ReadCommercialAmendmentQueueCoreAsync(user,null,null)).Queue;
    public async Task<CompanyCommercialAmendmentPage> ReadCommercialAmendmentPageAsync(ClaimsPrincipal user,int page,int size)
    {
        CompanyCommercialAmendmentPage.RequireRequest(page,size);var result=await ReadCommercialAmendmentQueueCoreAsync(user,page,size);
        return CompanyCommercialAmendmentPage.Create(result.Queue,page,size,result.Total);
    }
    private async Task<(CompanyCommercialAmendmentQueue Queue,int Total)> ReadCommercialAmendmentQueueCoreAsync(ClaimsPrincipal user,int? page,int? size)
    {
        if(commercial is null)throw new InvalidOperationException("The canonical commercial owner is required.");
        var original=await context.CaptureProcessAsync(user);
        var roles=JsonSerializer.Serialize(original.Access.Roles?.Order(StringComparer.Ordinal));var grants=JsonSerializer.Serialize(original.Access.Permissions?.Order(StringComparer.Ordinal));
        async Task Guard()
        {
            var current=await context.CaptureProcessAsync(user);
            if(user.Identity?.IsAuthenticated!=true || current.Actor!=original.Actor || current.Access.Scope!=original.Access.Scope || current.Binding!=original.Binding || !ReferenceEquals(current.Process,original.Process) ||
                current.Access.Roles?.Any(user.IsInRole)!=true || current.Access.Permissions?.Contains("Accounting.View",StringComparer.Ordinal)!=true || !current.Access.Permissions.Contains("ProductionAccounting.Adjust",StringComparer.Ordinal) ||
                JsonSerializer.Serialize(current.Access.Roles.Order(StringComparer.Ordinal))!=roles || JsonSerializer.Serialize(current.Access.Permissions.Order(StringComparer.Ordinal))!=grants)throw new UnauthorizedAccessException("Original saved proposal queue authority changed.");
        }
        async Task<(CompanyCommercialAmendmentQueue Queue,int Total)> Capture()
        {
            await Guard();IReadOnlyList<COMMERCIAL_TERMS_INTENT> entries;int total;
            if(page is {} number && size is {} count){var selected=await commercial.ReadAmendmentIntentPageAsync(user,number,count,Guard);entries=selected.Entries;total=selected.Total;}
            else{entries=await commercial.ReadAmendmentIntentsAsync(user,Guard);total=entries.Count;}
            var rows=new List<CompanyCommercialAmendmentQueueRow>();
            foreach(var entry in entries)
            {
                var review=await ReadCommercialAmendmentIntentAsync(user,entry.PROCESS_INSTANCE_ID);
                if(JsonSerializer.Serialize(review.Entry)!=JsonSerializer.Serialize(entry))throw new InvalidOperationException("The discovered original proposal changed.");
                var source=await ReadCommercialProcessSourceAsync(user,entry.PROCESS_INSTANCE_ID) ??throw new InvalidOperationException("The original proposal process source is unavailable.");
                rows.Add(new(CompanyInventorySourceReview.Copy(entry),source));
            }
            await Guard();var result=new CompanyCommercialAmendmentQueue(original.Access.Scope,original.Actor,original.Binding,rows,
                ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=original.Access.Scope,ActorUserId=original.Actor,BindingRevision=original.Binding,Rows=rows})));
            result.Require(original.Access.Scope,original.Actor);return(result,total);
        }
        var first=await Capture();var last=await Capture();
        if(first.Total!=last.Total || JsonSerializer.Serialize(first.Queue)!=JsonSerializer.Serialize(last.Queue))throw new InvalidOperationException("The complete saved proposal queue/page or native history changed during review.");
        return(CompanyInventorySourceReview.Copy(last.Queue),last.Total);
    }
    private async Task<Func<Task>> AmendmentIntentAuthority(ClaimsPrincipal user,CompanyCommercialAmendmentReview review)
    {
        var original=await context.CaptureProcessAsync(user);
        var roles=original.Access.Roles.Order(StringComparer.Ordinal).ToArray();var grants=original.Access.Permissions.Order(StringComparer.Ordinal).ToArray();
        async Task Guard()
        {
            var current=await context.CaptureProcessAsync(user);
            if(user.Identity?.IsAuthenticated!=true || current.Actor!=review.ActorUserId || current.Access.Scope!=review.Company || current.Binding!=review.BindingRevision ||
                !ReferenceEquals(current.Process,original.Process) || current.Access.Roles?.Any(user.IsInRole)!=true || current.Access.Permissions is null ||
                !current.Access.Roles.Order(StringComparer.Ordinal).SequenceEqual(roles) || !current.Access.Permissions.Order(StringComparer.Ordinal).SequenceEqual(grants) ||
                !current.Access.Permissions.Contains("Accounting.View",StringComparer.Ordinal) || !current.Access.Permissions.Contains("ProductionAccounting.Adjust",StringComparer.Ordinal))throw new UnauthorizedAccessException("Original commercial intent authority changed.");
        }
        await Guard();return Guard;
    }
    public async Task<COMMERCIAL_TERMS_INTENT> SaveCommercialAmendmentIntentAsync(ClaimsPrincipal user,string id,CompanyCommercialAmendmentSubmission command)
    {
        command=CompanyInventorySourceReview.Copy(command);command.Require();var review=command.Review;
        if(id!=review.InstanceId)throw new ArgumentException("The intent route differs from its original process.");
        if(commercial is null)throw new InvalidOperationException("The canonical commercial owner is required.");
        var authority=await AmendmentIntentAuthority(user,review);
        await ClosedCommercialProcess(user,id,review.Company,review.ActorUserId,review.BindingRevision,review.ProcessRevision,review.Previous);
        var entry=await commercial.SaveAmendmentIntentAsync(user,command,authority);
        await ClosedCommercialProcess(user,id,review.Company,review.ActorUserId,review.BindingRevision,review.ProcessRevision,review.Previous);await authority();
        if(JsonSerializer.Serialize(entry.Restore(review.Company,review.ActorUserId,id))!=JsonSerializer.Serialize(command))throw new InvalidOperationException("The saved intent differs from the original reviewed command.");
        return CompanyInventorySourceReview.Copy(entry);
    }
    public async Task<CompanyCommercialAmendmentIntentReview> ReadCommercialAmendmentIntentAsync(ClaimsPrincipal user,string id)
    {
        if(commercial is null)throw new InvalidOperationException("The canonical commercial owner is required.");
        var original=await context.CaptureProcessAsync(user);
        var process=await ClosedCommercialProcess(user,id,original.Access.Scope,original.Actor,original.Binding);
        var source=await commercial.ReadProcessTermsAsync(user,process.EntityId,CommercialStart(process).SourceRevision,true);
        var previous=source.Terms.Recorded ??throw new InvalidOperationException("The original rejected commercial version is required.");
        var revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(process));
        var authorityReview=new CompanyCommercialAmendmentReview(original.Access.Scope,original.Actor,id,revision,original.Binding,source.Terms.Source.Contract,previous,SalesContractTermsAmendment.NormalizeSource(source.Terms.Source,previous));
        var authority=await AmendmentIntentAuthority(user,authorityReview);
        var entry=await commercial.ReadAmendmentIntentAsync(user,original.Access.Scope,original.Actor,id,previous.Preparation.Source.Source.FieldId,authority);
        if(entry is not null)
        {
            var command=entry.Restore(original.Access.Scope,original.Actor,id);
            if(command.Review.ProcessRevision!=revision || command.Review.BindingRevision!=original.Binding || JsonSerializer.Serialize(command.Review.Previous)!=JsonSerializer.Serialize(previous) || entry.SALES_CONTRACT_ID!=process.EntityId)
                throw new InvalidOperationException("The saved amendment belongs to a different original process/source.");
        }
        await ClosedCommercialProcess(user,id,original.Access.Scope,original.Actor,original.Binding,revision,previous);await authority();
        var result=new CompanyCommercialAmendmentIntentReview(original.Access.Scope,original.Actor,id,revision,original.Binding,entry);
        result.Require(original.Access.Scope,original.Actor,id);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyCommercialHandoffReview> DiscoverCommercialHandoffAsync(ClaimsPrincipal user,CompanyAccountingProcessStartCatalogQuery query)
    {
        if(query.EntityType!="SALES_CONTRACT")throw new ArgumentException("An exact commercial preparation is required.");
        var native=await DiscoverNativeDecisionHandoffAsync(user,query);
        var result=new CompanyCommercialHandoffReview(native.Company,native.ActorUserId,native.EntityId,native.PreparationRevision,native.BindingRevision,native.CandidateSetRevision,native.Start,native.ProcessRevision,native.Status);
        result.Require(native.Company,native.ActorUserId,query);return result;
    }
    public async Task<CompanyNativeDecisionHandoffReview> DiscoverNativeDecisionHandoffAsync(ClaimsPrincipal user,CompanyAccountingProcessStartCatalogQuery query)
    {
        using var invoiceValidation=CompanySalesInvoiceValidation.Enter();
        query=CompanyInventorySourceReview.Copy(query);query.Require();
        if(!CompanyAccountingSourceTypes.IsNativeDecision(query.EntityType) || query.ExpectedSourceRevision is null || (query.EntityType=="JOURNAL_ENTRY"?journals is null:query.EntityType is "AR_INVOICE" or "AR_PAYMENT"?salesReceivables is null:query.EntityType=="INVOICE"?salesInvoices is null:commercial is null))throw new ArgumentException("An exact recorded native preparation is required.");
        var original=await context.CaptureProcessAsync(user);
        string Authority(RepositoryCompanyAccess access,string actor,string binding)=>JsonSerializer.Serialize(new{access.Scope,actor,binding,Roles=access.Roles?.Order(StringComparer.Ordinal),Grants=access.Permissions?.Order(StringComparer.Ordinal)});
        var authority=Authority(original.Access,original.Actor,original.Binding);
        async Task Guard()
        {
            var current=await context.CaptureProcessAsync(user);
            if(user.Identity?.IsAuthenticated!=true || current.Access.Roles?.Any(user.IsInRole)!=true || current.Access.Permissions?.Contains("Accounting.View",StringComparer.Ordinal)!=true ||
                Authority(current.Access,current.Actor,current.Binding)!=authority || !ReferenceEquals(current.Process,original.Process))throw new UnauthorizedAccessException("Original native handoff read authority changed.");
        }
        async Task<(string Source,string Maker)> Source()
        {
            if(query.EntityType=="AR_PAYMENT")
            {
                var source=await ReadSalesPaymentSourceAsync(user,original.Access.Scope,original.Actor,query.EntityId,query.ExpectedSourceRevision);return(JsonSerializer.Serialize(source),source.Payment.CreatedBy);
            }
            if(query.EntityType=="JOURNAL_ENTRY")
            {
                var source=await ReadSalesJournalSourceAsync(user,original.Access.Scope,original.Actor,query.EntityId,query.ExpectedSourceRevision);return(JsonSerializer.Serialize(source),source.Journal.Header.ROW_CREATED_BY);
            }
            else if(query.EntityType=="AR_INVOICE")
            {
                var receivable=await ReadSalesReceivableSourceAsync(user,original.Access.Scope,original.Actor,query.EntityId,query.ExpectedSourceRevision);
                return(JsonSerializer.Serialize(receivable),receivable.Original.Intent.ROW_CREATED_BY);
            }
            if(query.EntityType=="INVOICE")
            {
                var invoice=await ReadSalesInvoiceSourceAsync(user,original.Access.Scope,original.Actor,query.EntityId,query.ExpectedSourceRevision);
                return(JsonSerializer.Serialize(invoice),invoice.Original.Intent.ROW_CREATED_BY);
            }
            if(query.EntityType=="SALES_CONTRACT")
            {
                var terms=await commercial.ReadProcessTermsAsync(user,query.EntityId,query.ExpectedSourceRevision);
                if(terms.Terms.Source.Source.Company!=original.Access.Scope || terms.Terms.Source.Source.ActorUserId!=original.Actor)throw new UnauthorizedAccessException();
                return(JsonSerializer.Serialize(new{terms.Terms,terms.CurrentNativeContract}),terms.Terms.Recorded?.Preparation.ActorUserId ??throw new InvalidOperationException("The original terms preparation is required."));
            }
            var delivery=await commercial.ReadProcessDeliveryAsync(user,query.EntityId,query.ExpectedSourceRevision);
            if(delivery.IsHistorical)throw new InvalidOperationException("Historical receiving evidence cannot admit a current handoff.");
            if(delivery.Source.Source.Source.Company!=original.Access.Scope || delivery.Source.Source.Source.ActorUserId!=original.Actor)throw new UnauthorizedAccessException();
            return(JsonSerializer.Serialize(delivery),delivery.Recorded?.Preparation.ActorUserId ??throw new InvalidOperationException("The original delivery preparation is required."));
        }
        async Task<(CompanyNativeDecisionHandoffReview Review,string Source)> Capture()
        {
            await Guard();var source=await Source();
            var candidates=await original.Process.GetProcessInstancesForEntityAsync(query.EntityId,query.EntityType);
            if(candidates is null || candidates.Any(row=>row is null || row.EntityType!=query.EntityType || row.EntityId!=query.EntityId) || candidates.Select(row=>row.InstanceId).Distinct(StringComparer.Ordinal).Count()!=candidates.Count)
                throw new InvalidOperationException("The complete native handoff candidate set is required.");
            var owned=new List<ProcessInstance>();
            foreach(var candidate in candidates)
            {
                if(!string.IsNullOrEmpty(candidate.FieldId))continue;
                var binding=CompanyAccountingProcessBinding.FromPayloadJson(JsonSerializer.Serialize(candidate.ProcessData),candidate.InstanceId,candidate.EntityType,candidate.EntityId);
                if(binding.Company!=original.Access.Scope)continue;
                var start=NativeDecisionStart(candidate,query.EntityType);
                if(start.Company!=binding.Company || start.ActorUserId!=candidate.StartedBy || start.BindingRevision!=original.Binding)throw new InvalidOperationException("The owned candidate handoff identity/binding changed.");
                if(start.SourceRevision!=query.ExpectedSourceRevision)continue;
                var process=await ReadAsync(user,candidate.InstanceId,original.Access.Scope,original.Actor) ?? throw new InvalidOperationException("The discovered handoff is unavailable.");
                if(JsonSerializer.Serialize(process)!=JsonSerializer.Serialize(candidate) || start.SourcePreparerUserId!=source.Maker)throw new InvalidOperationException("The discovered process/preparer changed.");
                owned.Add(process);
            }
            if(owned.Count>1)throw new InvalidOperationException("Multiple handoffs for the same native preparation require reconciliation.");
            var selected=owned.SingleOrDefault();
            var result=new CompanyNativeDecisionHandoffReview(original.Access.Scope,original.Actor,query.EntityType,query.EntityId,query.ExpectedSourceRevision,original.Binding,
                ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(candidates.OrderBy(row=>row.InstanceId,StringComparer.Ordinal))),selected is null?null:NativeDecisionStart(selected,query.EntityType),
                selected is null?null:ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(selected)),selected?.Status.ToString());
            result.Require(original.Access.Scope,original.Actor,query);await Guard();
            return(result,source.Source);
        }
        var first=await Capture();var last=await Capture();
        if(first.Source!=last.Source || JsonSerializer.Serialize(first.Review)!=JsonSerializer.Serialize(last.Review))throw new InvalidOperationException("The native handoff set or source changed during discovery.");
        return CompanyInventorySourceReview.Copy(last.Review);
    }
    private Task<ProcessInstance> ClosedCommercialProcess(ClaimsPrincipal user,string id,CompanyScope company,string actor,string binding,string? expectedRevision=null,SalesContractTermsRecord? previous=null)
    {
        if(previous is not null){previous.Require();if(previous.Preparation.Company!=company || previous.Preparation.ActorUserId!=actor || previous.Decision?.Decision!=SalesContractTermsDecision.Reject)throw new InvalidOperationException("The original rejected terms are required.");}
        return ClosedNativeRejectionProcess(user,id,company,actor,binding,"SALES_CONTRACT",expectedRevision,previous is null?null:CompanyAccountingSourceTypes.CommercialPreparationRevision(previous),previous?.Decision?.ActorUserId,previous?.Revision);
    }
    private Task<ProcessInstance> ClosedDeliveryProcess(ClaimsPrincipal user,string id,CompanyScope company,string actor,string binding,string? expectedRevision=null,RevenueDeliveryAcceptanceRecord? previous=null)
    {
        if(previous is not null){previous.Require();if(previous.Preparation.Company!=company || previous.Preparation.ActorUserId!=actor || previous.Decision?.Decision!=RevenueDeliveryAcceptanceDecision.Reject)throw new InvalidOperationException("The original rejected receiving evidence is required.");}
        return ClosedNativeRejectionProcess(user,id,company,actor,binding,"RUN_TICKET",expectedRevision,previous is null?null:CompanyAccountingSourceTypes.DeliveryPreparationRevision(previous),previous?.Decision?.ActorUserId,previous?.Revision);
    }
    private async Task<ProcessInstance> ClosedNativeRejectionProcess(ClaimsPrincipal user,string id,CompanyScope company,string actor,string binding,string entityType,string? expectedRevision,string? sourceRevision,string? reviewer,string? nativeRevision)
    {
        if(string.IsNullOrWhiteSpace(id) || id.Length>128 || id.Any(char.IsControl))throw new ArgumentException("The original bounded rejection-review process is required.");
        var authority=await context.CaptureProcessAsync(user);var access=authority.Access;
        if(user.Identity?.IsAuthenticated!=true || access.Scope!=company || authority.Actor!=actor || authority.Binding!=binding ||
            access.Roles?.Any(user.IsInRole)!=true || access.Permissions?.Contains("Accounting.View",StringComparer.Ordinal)!=true || !access.Permissions.Contains("ProductionAccounting.Adjust",StringComparer.Ordinal))
            throw new UnauthorizedAccessException("Current original-preparer amendment authority is required.");
        var process=CompanyInventorySourceReview.Copy(await authority.Process.GetProcessInstanceAsync(id)) ?? throw new InvalidOperationException("The rejection-review process is unavailable.");
        if(process.EntityType!=entityType || process.Status!=ProcessStatus.COMPLETED || process.CurrentState!="REJECTION_REVIEWED" || !string.IsNullOrEmpty(process.FieldId) || process.InstanceId!=id || process.ProcessData?.Data is null || process.StepInstances is null || process.History is null)
            throw new InvalidOperationException("Close the original rejection-review process before proposing an amendment.");
        if(!process.ProcessData.Data.TryGetValue(CompanyAccountingProcessBinding.PayloadKey,out var owner))throw new InvalidOperationException("Explicit company process ownership is required.");
        CompanyAccountingProcessBinding.FromJson(JsonSerializer.Serialize(owner),company,id,process.EntityType,process.EntityId);
        var start=NativeDecisionStart(process,entityType);
        var definition=await authority.Process.GetProcessDefinitionAsync(process.ProcessId) ?? throw new InvalidOperationException("The original process definition is unavailable.");
        RequireNativeDecisionDefinition(definition);
        if(!definition.IsActive || start.Company!=company || start.ActorUserId!=process.StartedBy || start.SourcePreparerUserId!=actor || process.CompletedBy!=actor || start.BindingRevision!=binding ||
            ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition))!=start.DefinitionRevision || expectedRevision is not null && ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(process))!=expectedRevision ||
            sourceRevision is not null && start.SourceRevision!=sourceRevision)
            throw new InvalidOperationException("The closed original rejection-review evidence changed.");
        var steps=process.StepInstances.OrderBy(row=>row.SequenceNumber).ToArray();var configured=definition.Steps.OrderBy(row=>row.SequenceNumber).ToArray();
        if(steps.Length!=2 || configured.Length!=2 || steps[1].StepId!=process.CurrentStepId || steps[0].Outcome!="REJECTED" || steps[1].Outcome!="REJECTION_REVIEWED" ||
            steps[0].CompletedBy==actor || steps[0].CompletedBy==start.ActorUserId || reviewer is not null && steps[0].CompletedBy!=reviewer || steps[1].CompletedBy!=actor || process.CompletionDate!=steps[1].CompletionDate ||
            process.StartDate==default || process.StartDate.Kind!=DateTimeKind.Utc || process.CompletionDate is not {} end || end.Kind!=DateTimeKind.Utc || end<process.StartDate ||
            steps.Select(row=>row.SequenceNumber).Distinct().Count()!=2 ||
            steps.Where((row,index)=>row.InstanceId!=id || row.StepId!=configured[index].StepId || row.StepName!=configured[index].StepName || row.ApprovalRequired!=configured[index].RequiresApproval || configured[index].RequiredRoles is not {Count:1} || row.RequiredRole!=configured[index].RequiredRoles[0] ||
                row.Status!=StepStatus.COMPLETED || row.CompletionDate is not {} date || date.Kind!=DateTimeKind.Utc || date<process.StartDate || date>end || string.IsNullOrWhiteSpace(row.CompletedBy) || string.IsNullOrWhiteSpace(row.Notes)).Any())
            throw new InvalidOperationException("The original rejection route and completed preparer review are required.");
        if(!(access.Roles.Contains(steps[1].RequiredRole,StringComparer.Ordinal) && user.IsInRole(steps[1].RequiredRole) || access.Roles.Contains("Administrator",StringComparer.Ordinal) && user.IsInRole("Administrator")))throw new UnauthorizedAccessException("The configured original-preparer role is required.");
        for(var index=0;index<steps.Length;index++)
        {
            var step=steps[index];var action=index==0?CompanyAccountingSourceTypes.RejectionHistory(entityType):CompanyAccountingSourceTypes.RejectionReviewHistory(entityType);
            var history=process.History.Where(row=>row.InstanceId==id && row.StepInstanceId==step.StepInstanceId && row.Action==action).ToArray();
            if(history.Length!=1 || history[0].PerformedBy!=step.CompletedBy || history[0].Timestamp!=step.CompletionDate || history[0].Notes!=step.Notes || index==1 && history[0].NewState!="REJECTION_REVIEWED")throw new InvalidOperationException("The original rejection-review audit changed.");
        }
        if(authority.Process is not ICompanyAccountingProcessCommands commands)throw new InvalidOperationException("The native company lifecycle command owner is required.");
        var completion=await commands.GetRecordedCompanyAccountingStepCommandAsync(id) ?? throw new InvalidOperationException("The original rejection-review command is unavailable.");completion.Require();
        if(completion.Action is not null || completion.Review.Company!=company || completion.Review.BindingRevision!=binding || completion.Review.DefinitionRevision!=start.DefinitionRevision || completion.Review.RequiresIndependentActor || completion.Review.InstanceId!=id || completion.Review.ActorUserId!=actor || completion.Review.SourceMakerUserId!=actor ||
            completion.Review.ProcessId!=process.ProcessId || completion.Review.EntityType!=entityType || completion.Review.EntityId!=process.EntityId || completion.Review.StepId!=steps[1].StepId || completion.Review.AssignedRole!=steps[1].RequiredRole || completion.Review.WorkflowStarterUserId!=process.StartedBy || completion.Review.RequiredPermission!="Workflow.Execute" ||
            completion.Review.StepInstanceId!=steps[1].StepInstanceId || completion.Notes!=steps[1].Notes ||
            completion.Review.SourceRevision!=start.SourceRevision || nativeRevision is not null && completion.Review.AccountingEvidenceRevision!=nativeRevision)
            throw new InvalidOperationException("The retained original rejection-review command changed.");
        return process;
    }
    public async Task<CompanyCommercialAmendmentReview> ReviewCommercialAmendmentAsync(ClaimsPrincipal user,string id)
    {
        if(commercial is null)throw new InvalidOperationException("The canonical commercial owner is required.");
        var original=await context.CaptureProcessAsync(user);
        var process=await ClosedCommercialProcess(user,id,original.Access.Scope,original.Actor,original.Binding);
        var revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(process));
        var predecessor=await commercial.ReadAmendmentPredecessorAsync(user,process.EntityId);
        var source=await commercial.ReadAmendmentSourceAsync(user,predecessor.Record);
        var review=new CompanyCommercialAmendmentReview(original.Access.Scope,original.Actor,id,revision,original.Binding,predecessor.Native,predecessor.Record,source);review.Require(original.Access.Scope,original.Actor,id);
        await ClosedCommercialProcess(user,id,review.Company,review.ActorUserId,review.BindingRevision,revision,review.Previous);
        var closing=await commercial.ReadAmendmentPredecessorAsync(user,process.EntityId);
        var closingSource=await commercial.ReadAmendmentSourceAsync(user,closing.Record);
        if(JsonSerializer.Serialize(closing.Native)!=JsonSerializer.Serialize(predecessor.Native) || JsonSerializer.Serialize(closingSource)!=JsonSerializer.Serialize(source))throw new InvalidOperationException("The amendment review source changed.");
        return review;
    }
    public async Task<SalesContractTermsRecord> SubmitCommercialAmendmentAsync(ClaimsPrincipal user,string id,CompanyCommercialAmendmentSubmission submission)
    {
        submission=CompanyInventorySourceReview.Copy(submission);submission.Require();var review=submission.Review;
        if(review.InstanceId!=id)throw new ArgumentException("The amendment route differs from its original process.");
        if(commercial is null)throw new InvalidOperationException("The canonical commercial owner is required.");
        async Task Guard(){await ClosedCommercialProcess(user,id,review.Company,review.ActorUserId,review.BindingRevision,review.ProcessRevision,review.Previous);}
        await Guard();
        var original=await context.CaptureProcessAsync(user);
        var roles=original.Access.Roles.Order(StringComparer.Ordinal).ToArray();var grants=original.Access.Permissions.Order(StringComparer.Ordinal).ToArray();
        async Task Authority()
        {
            var current=await context.CaptureProcessAsync(user);
            if(user.Identity?.IsAuthenticated!=true || current.Actor!=review.ActorUserId || current.Access.Scope!=review.Company || current.Binding!=review.BindingRevision ||
                !ReferenceEquals(current.Process,original.Process) || current.Access.Roles is null || current.Access.Permissions is null || !current.Access.Roles.Any(user.IsInRole) ||
                !current.Access.Roles.Order(StringComparer.Ordinal).SequenceEqual(roles) || !current.Access.Permissions.Order(StringComparer.Ordinal).SequenceEqual(grants) ||
                !current.Access.Permissions.Contains("Accounting.View",StringComparer.Ordinal) || !current.Access.Permissions.Contains("ProductionAccounting.Adjust",StringComparer.Ordinal))
                throw new UnauthorizedAccessException("Original amendment lifecycle authority changed.");
        }
        // Full LIFECYCLE evidence is checked outside the owned PRODUCTION transaction.
        await Authority();await Guard();
        var saved=await commercial.SaveAmendmentIntentAsync(user,submission,Authority);
        if(JsonSerializer.Serialize(saved.Restore(review.Company,review.ActorUserId,id))!=JsonSerializer.Serialize(submission))throw new InvalidOperationException("The original amendment proposal is not durably saved.");
        await Guard();await Authority();
        var result=await commercial.AmendTermsAsync(user,submission.Amendment,Authority);
        await Guard();
        await Authority();
        if(JsonSerializer.Serialize(result.Preparation)!=JsonSerializer.Serialize(submission.Amendment.Preparation))throw new InvalidOperationException("The recorded amendment does not match its retained proposal.");
        result.Require();return CompanyInventorySourceReview.Copy(result);
    }
    private static CompanyAccountingProcessStart CommercialStart(ProcessInstance process)=>NativeDecisionStart(process,"SALES_CONTRACT");
    private static CompanyAccountingProcessStart NativeDecisionStart(ProcessInstance process,string entityType)
    {
        if(process.ProcessData?.Data is null || !process.ProcessData.Data.TryGetValue(CompanyAccountingProcessStart.PayloadKey,out var payload))
            throw new InvalidOperationException("The original commercial handoff is unavailable.");
        var start=JsonSerializer.Deserialize<CompanyAccountingProcessStart>(JsonSerializer.Serialize(payload))
            ?? throw new InvalidOperationException("The original commercial handoff is unavailable.");
        start.Require();
        if(start.EntityType!=entityType || process.EntityType!=entityType || start.EntityId!=process.EntityId || start.ProcessId!=process.ProcessId || start.InstanceId()!=process.InstanceId)
            throw new InvalidOperationException("The original commercial handoff source changed.");
        return start;
    }
    public async Task<CompanyCommercialProcessSourceReview?> ReadCommercialProcessSourceAsync(ClaimsPrincipal user,string id)
    {
        var original=await context.CaptureProcessAsync(user);
        var originalRoles=original.Access.Roles?.Order(StringComparer.Ordinal).ToArray() ?? throw new UnauthorizedAccessException();
        var originalPermissions=original.Access.Permissions?.Order(StringComparer.Ordinal).ToArray() ?? throw new UnauthorizedAccessException();
        var process=await ReadAsync(user,id,original.Access.Scope,original.Actor);if(process is null)return null;
        if(process.EntityType!="SALES_CONTRACT" || commercial is null)throw new InvalidOperationException("A recorded commercial process is required.");
        var start=CommercialStart(process);
        var captured=process.Status==ProcessStatus.COMPLETED && process.CurrentState=="REJECTION_REVIEWED";
        var source=await commercial.ReadProcessTermsAsync(user,process.EntityId,start.SourceRevision,captured);
        var terms=source.Terms;
        var revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(process));
        var review=new CompanyCommercialProcessSourceReview(original.Access.Scope,original.Actor,id,process.EntityId,start.SourceRevision,revision,terms,source.CurrentNativeContract);
        review.Require(original.Access.Scope,original.Actor,id);
        var closing=await ReadAsync(user,id,original.Access.Scope,original.Actor);
        var closingSource=await commercial.ReadProcessTermsAsync(user,process.EntityId,start.SourceRevision,captured);
        var authority=await context.CaptureProcessAsync(user);
        if(closing is null || ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(closing))!=revision ||
            JsonSerializer.Serialize(new{closingSource.Terms,closingSource.CurrentNativeContract})!=JsonSerializer.Serialize(new{source.Terms,source.CurrentNativeContract}) ||
            authority.Binding!=original.Binding || authority.Actor!=original.Actor || authority.Access.Scope!=original.Access.Scope ||
            authority.Access.Roles is null || authority.Access.Permissions is null ||
            !authority.Access.Roles.Order(StringComparer.Ordinal).SequenceEqual(originalRoles) ||
            !authority.Access.Permissions.Order(StringComparer.Ordinal).SequenceEqual(originalPermissions))
            throw new UnauthorizedAccessException("The original commercial process or authority changed.");
        return review;
    }
    private async Task<SalesContractTermsRecord> ReadCommercialSourceAsync(ClaimsPrincipal user,CompanyScope company,string actor,string id)
    {
        if(commercial is null)throw new InvalidOperationException("The canonical commercial source reader is required.");
        var source=await commercial.ReadTermsByContractAsync(user,id);
        if(source.Source.Source.Company!=company || source.Source.Source.ActorUserId!=actor || source.Recorded is null || source.Source.Contract.SALES_CONTRACT_ID!=id)throw new InvalidOperationException("The owned commercial process source is unavailable.");
        return source.Recorded;
    }
    private static void RequireNativeDecisionDefinition(ProcessDefinition definition)
    {
        if(!CompanyAccountingSourceTypes.IsNativeDecision(definition.EntityType))return;
        var steps=definition.Steps?.OrderBy(row=>row.SequenceNumber).ToArray();
        if(definition.EntityType=="AR_PAYMENT" && (definition.ProcessType!="FINANCIAL" || steps is not {Length:2} || !NativeDecisionStep(steps[0],"AR_PAYMENT",true) || !NativeDecisionStep(steps[1],"AR_PAYMENT",false)))throw new InvalidOperationException("Payment review requires explicit independent approval and original-maker rejection-review branches.");
        if(definition.EntityType=="JOURNAL_ENTRY"){if(definition.ProcessType!="FINANCIAL" || steps is not {Length:3} || !NativeDecisionStep(steps[0],"JOURNAL_ENTRY",true) || !JournalPostingStep(steps[1]) || !NativeDecisionStep(steps[2],"JOURNAL_ENTRY",false))throw new InvalidOperationException("Journal review/posting requires explicit approval, posting and rejection-review branches.");return;}
        if(definition.ProcessType!="FINANCIAL" || steps is null || steps.Length is <1 or >2 || !NativeDecisionStep(steps[0],definition.EntityType,true) || steps.Length==2 && !NativeDecisionStep(steps[1],definition.EntityType,false))
            throw new InvalidOperationException("A configured native approval and optional rejection-review step are required.");
    }
    private static bool NativeDecisionStep(ProcessStepDefinition step,string type,bool approval)
    {
        if(step.RequiresApproval!=approval || step.StepType!=(approval?"APPROVAL":"ACTION") || step.StepConfiguration is not {Count:1} || !step.StepConfiguration.TryGetValue(CompanyAccountingStepEvidence.ConfigurationKey,out var value))return false;
        return (value switch{string text=>text,JsonElement {ValueKind:JsonValueKind.String} json=>json.GetString(),_=>null})==
            (approval?CompanyAccountingSourceTypes.Approval(type):CompanyAccountingSourceTypes.RejectionReview(type));
    }
    private async Task<RevenueDeliveryAcceptanceRecord> ReadDeliverySourceAsync(ClaimsPrincipal user,CompanyScope company,string actor,string id,string? preparationRevision=null)
    {
        if(commercial is null)throw new InvalidOperationException("The canonical delivery source reader is required.");
        var review=preparationRevision is null?await commercial.ReadPreparedDeliveryAsync(user,id):await commercial.ReadProcessDeliveryAsync(user,id,preparationRevision);
        if(review.IsHistorical)throw new InvalidOperationException("Historical receiving evidence cannot authorize a current workflow step.");
        if(review.Source.Source.Source.Company!=company || review.Source.Source.Source.ActorUserId!=actor || review.Source.TicketId!=id || review.Recorded is null)
            throw new InvalidOperationException("The original owned delivery preparation is unavailable.");
        return review.Recorded;
    }
    public async Task<CompanyDeliveryProcessSourceReview?> ReadDeliveryProcessSourceAsync(ClaimsPrincipal user,string id)
    {
        if(commercial is null)throw new InvalidOperationException("The canonical delivery source reader is required.");
        var original=await context.CaptureProcessAsync(user);
        var process=await ReadAsync(user,id,original.Access.Scope,original.Actor);if(process is null)return null;
        if(process.EntityType!="RUN_TICKET")throw new InvalidOperationException("A recorded company delivery process is required.");
        var start=NativeDecisionStart(process,"RUN_TICKET");
        var delivery=await commercial.ReadProcessDeliveryAsync(user,process.EntityId,start.SourceRevision);
        var revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(process));
        var result=new CompanyDeliveryProcessSourceReview(original.Access.Scope,original.Actor,id,process.EntityId,start.SourceRevision,revision,delivery);
        result.Require(original.Access.Scope,original.Actor,id);
        var closing=await ReadAsync(user,id,original.Access.Scope,original.Actor);
        var closingDelivery=await commercial.ReadProcessDeliveryAsync(user,process.EntityId,start.SourceRevision);
        var authority=await context.CaptureProcessAsync(user);
        if(closing is null || ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(closing))!=revision || JsonSerializer.Serialize(delivery)!=JsonSerializer.Serialize(closingDelivery) ||
            authority.Actor!=original.Actor || authority.Access.Scope!=original.Access.Scope || authority.Binding!=original.Binding || !ReferenceEquals(authority.Process,original.Process) ||
            JsonSerializer.Serialize(authority.Access.Roles?.Order(StringComparer.Ordinal))!=JsonSerializer.Serialize(original.Access.Roles?.Order(StringComparer.Ordinal)) ||
            JsonSerializer.Serialize(authority.Access.Permissions?.Order(StringComparer.Ordinal))!=JsonSerializer.Serialize(original.Access.Permissions?.Order(StringComparer.Ordinal)))
            throw new InvalidOperationException("The original company process, delivery evidence or authority changed during review.");
        return CompanyInventorySourceReview.Copy(result);
    }
    private async Task<(string Eligibility,string? Revision,APInvoiceCorrectionStepEvidence? Correction)> DeliveryStepEvidence(ClaimsPrincipal user,CompanyScope company,string actor,string id,string sourceRevision,ProcessStepDefinition step)
    {
        var rejectionReview=NativeDecisionStep(step,"RUN_TICKET",false);
        if(!NativeDecisionStep(step,"RUN_TICKET",true) && !rejectionReview)throw new InvalidOperationException("A configured native delivery decision is required.");
        var record=await ReadDeliverySourceAsync(user,company,actor,id,sourceRevision);
        var current=await production.CaptureAsync(user);
        var eligibility=rejectionReview?record.Decision?.Decision!=RevenueDeliveryAcceptanceDecision.Reject?"DELIVERY_APPROVAL_REQUIRED":record.Preparation.ActorUserId!=actor?"DELIVERY_PREPARER_REQUIRED":
            current.Access.Permissions?.Contains("ProductionAccounting.Adjust",StringComparer.Ordinal)!=true?"DELIVERY_PREPARATION_PERMISSION_REQUIRED":"ELIGIBLE":
            record.Decision is null?"DELIVERY_APPROVAL_REQUIRED":record.Decision.ActorUserId!=actor?"DELIVERY_REVIEWER_REQUIRED":
            current.Access.Permissions?.Contains("ProductionAccounting.Approve",StringComparer.Ordinal)!=true?"DELIVERY_APPROVAL_PERMISSION_REQUIRED":"ELIGIBLE";
        if(!rejectionReview && eligibility=="ELIGIBLE" && record.Decision!.Decision==RevenueDeliveryAcceptanceDecision.Reject)eligibility=CompanyAccountingSourceTypes.RejectionEligibility("RUN_TICKET");
        return(eligibility,record.Revision,null);
    }
    private static bool CommercialApprovalStep(ProcessStepDefinition step)=>NativeDecisionStep(step,"SALES_CONTRACT",true);
    private static bool CommercialRejectionStep(ProcessStepDefinition step)=>NativeDecisionStep(step,"SALES_CONTRACT",false);
    private async Task<(string Eligibility,string? Revision,APInvoiceCorrectionStepEvidence? Correction)> CommercialStepEvidence(ClaimsPrincipal user,CompanyScope company,string actor,string id,string sourceRevision,ProcessStepDefinition step)
    {
        var rejectionReview=CommercialRejectionStep(step);
        if(!CommercialApprovalStep(step) && !rejectionReview)throw new InvalidOperationException("The commercial step must require its recorded native decision.");
        var record=await ReadCommercialSourceAsync(user,company,actor,id);
        if(CompanyAccountingSourceTypes.CommercialPreparationRevision(record)!=sourceRevision)throw new InvalidOperationException("The original commercial preparation changed.");
        var current=await production.CaptureAsync(user);
        var eligibility=rejectionReview?record.Decision?.Decision!=SalesContractTermsDecision.Reject?"COMMERCIAL_APPROVAL_REQUIRED":record.Preparation.ActorUserId!=actor?"COMMERCIAL_PREPARER_REQUIRED":
            current.Access.Permissions?.Contains("ProductionAccounting.Adjust",StringComparer.Ordinal)!=true?"COMMERCIAL_PREPARATION_PERMISSION_REQUIRED":"ELIGIBLE":
            record.Decision is null?"COMMERCIAL_APPROVAL_REQUIRED":record.Decision.ActorUserId!=actor?"COMMERCIAL_REVIEWER_REQUIRED":
            current.Access.Permissions?.Contains("ProductionAccounting.Approve",StringComparer.Ordinal)!=true?"COMMERCIAL_APPROVAL_PERMISSION_REQUIRED":"ELIGIBLE";
        if(!rejectionReview && eligibility=="ELIGIBLE" && record.Decision!.Decision==SalesContractTermsDecision.Reject)eligibility="COMMERCIAL_REJECTION_RECORDED";
        return(eligibility,record.Revision,null);
    }
}

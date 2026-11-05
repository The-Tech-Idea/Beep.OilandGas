using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.ProductionOperations.Models;

namespace Beep.OilandGas.Web.Services;

public sealed partial class BusinessProcessServiceClient
{
    public async Task<CompanyWorkOrderCompletionQuery> DiscoverWorkOrderSubmissionAsync(CompanyMaintenanceReceivingQuery task,CancellationToken cancellationToken=default)
    {
        task.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection;
        if(selected is null || selected.Roles.Count==0 || !selected.Permissions.Contains("Maintenance.VerifyCompletion",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current standard completion reviewer authority is required.");
        var result=await _apiClient.PostAsync<CompanyMaintenanceReceivingQuery,CompanyWorkOrderCompletionQuery>("/api/field/current/maintenance-handoffs/execution-permit/completion-report",task,cancellationToken)??throw new InvalidOperationException("Original recorded report is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original reviewer company/account changed.");result.Require();if(result.Task!=task)throw new InvalidOperationException("Discovered report belongs to another task.");return result;
    }
    private Beep.OilandGas.Client.App.CompanyClientSelection HandoffActor()
    {
        var selected=_apiClient.CompanySnapshot.Selection;if(selected is null || selected.Roles.Count==0 || !selected.Permissions.Contains("Maintenance.ExecuteWork",StringComparer.Ordinal) || !selected.Permissions.Contains("Workflow.Execute",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current standard executor roles, physical work and workflow handoff capabilities are required.");return selected;
    }
    public async Task<CompanyMaintenanceExecutionHandoffReview> ReviewMaintenanceExecutionHandoffAsync(CompanyWorkOrderCompletionQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=HandoffActor();var result=await _apiClient.PostAsync<CompanyWorkOrderCompletionQuery,CompanyMaintenanceExecutionHandoffReview>("/api/field/current/maintenance-handoffs/execution-permit/report-handoff/review",query,cancellationToken)??throw new InvalidOperationException("Original report and configured review handoff are unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original executor company/account changed.");result.Require();
        if(result.Report.Query!=query || result.Stage.Context.Company!=selected.Scope || result.Stage.Context.ActorUserId!=selected.UserId || !selected.Roles.Contains(result.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original executor/report/configured role differs.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyMaintenanceExecutionHandoffRecovery> RecordMaintenanceExecutionHandoffAsync(CompanyMaintenanceExecutionHandoffCommand original,CancellationToken cancellationToken=default)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var snapshot=_apiClient.CompanySnapshot;var selected=HandoffActor();
        if(command.Review.Stage.Context.Company!=selected.Scope || command.Review.Stage.Context.ActorUserId!=selected.UserId || !selected.Roles.Contains(command.Review.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original configured executor role/company/account is required.");
        var result=await _apiClient.PostAsync<CompanyMaintenanceExecutionHandoffCommand,CompanyMaintenanceExecutionHandoffRecovery>("/api/field/current/maintenance-handoffs/execution-permit/report-handoff",command,cancellationToken)??throw new InvalidOperationException("Original report handoff acknowledgement is unconfirmed.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original executor company/account changed.");result.Require(selected.Scope,selected.UserId,command.Reference());
        if(System.Text.Json.JsonSerializer.Serialize(result.Command)!=System.Text.Json.JsonSerializer.Serialize(command))throw new InvalidOperationException("Recorded handoff differs from its original retained body.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyMaintenanceExecutionHandoffRecovery> RecoverMaintenanceExecutionHandoffAsync(CompanyMaintenanceExecutionHandoffReference reference,CancellationToken cancellationToken=default)
    {
        reference.Require();var snapshot=_apiClient.CompanySnapshot;var selected=HandoffActor();var result=await _apiClient.PostAsync<CompanyMaintenanceExecutionHandoffReference,CompanyMaintenanceExecutionHandoffRecovery>("/api/field/current/maintenance-handoffs/execution-permit/report-handoff/recovery/reference",reference,cancellationToken)??throw new InvalidOperationException("Original report handoff remains unconfirmed; retain its original reference.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original executor company/account changed.");result.Require(selected.Scope,selected.UserId,reference);
        if(!selected.Roles.Contains(result.Command.Review.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original configured executor role is required.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderCompletionReview> ReviewWorkOrderCompletionAsync(CompanyWorkOrderCompletionQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection;
        if(selected is null || selected.Roles.Count==0 || !selected.Permissions.Contains("Maintenance.VerifyCompletion",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current standard completion reviewer company/account/capability is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderCompletionQuery,CompanyWorkOrderCompletionReview>("/api/field/current/maintenance-handoffs/execution-permit/completion-review",query,cancellationToken)
            ??throw new InvalidOperationException("Original report and current independent completion evidence are unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original completion reviewer company/account changed.");result.Require();
        if(result.Report.Query!=query || result.Context.Company!=selected.Scope || result.Context.ActorUserId!=selected.UserId || result.Report.ExecutorUserId==selected.UserId)throw new UnauthorizedAccessException("Original independent completion reviewer/company/task/report differs.");return CompanyInventorySourceReview.Copy(result);
    }

    public async Task<CompanyWorkOrderLaborUsageReview> ReviewWorkOrderLaborUsageAsync(CompanyWorkOrderExecutionStartReference reference,CancellationToken cancellationToken=default)
    {
        reference.Require();var snapshot=_apiClient.CompanySnapshot;var selected=SubmissionActor();
        var result=await _apiClient.PostAsync<CompanyWorkOrderExecutionStartReference,CompanyWorkOrderLaborUsageReview>("/api/field/current/maintenance-handoffs/execution-permit/labor-usage/review",reference,cancellationToken)
            ??throw new InvalidOperationException("Current original start/employee/duty/time review is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original labor company/account changed.");result.Require();var start=result.Execution.Start;
        if(start.Company!=selected.Scope || start.ExecutorUserId!=selected.UserId || start.Permit!=reference.Permit || start.IntentId!=reference.IntentId || !selected.Roles.Contains(start.AssignedRole,StringComparer.Ordinal))
            throw new UnauthorizedAccessException("Original labor executing company/account/start/configured role is required.");
        return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderLaborUsageRecovery> RecordWorkOrderLaborUsageAsync(CompanyWorkOrderLaborUsageCommand original,CancellationToken cancellationToken=default)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var snapshot=_apiClient.CompanySnapshot;var selected=SubmissionActor();var start=command.Review.Execution.Start;
        if(start.Company!=selected.Scope || start.ExecutorUserId!=selected.UserId || !selected.Roles.Contains(start.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original labor executor and configured role are required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderLaborUsageCommand,CompanyWorkOrderLaborUsageRecovery>("/api/field/current/maintenance-handoffs/execution-permit/labor-usage",command,cancellationToken)
            ??throw new InvalidOperationException("Original labor acknowledgement is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original labor context changed.");result.Require(selected.Scope,selected.UserId,command.Reference());
        if(System.Text.Json.JsonSerializer.Serialize(result.Command)!=System.Text.Json.JsonSerializer.Serialize(command))throw new InvalidOperationException("Recorded labor differs from its retained original body.");
        return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderLaborUsageRecovery> RecoverWorkOrderLaborUsageAsync(CompanyWorkOrderLaborUsageReference reference,CancellationToken cancellationToken=default)
    {
        reference.Require();var snapshot=_apiClient.CompanySnapshot;var selected=SubmissionActor();
        var result=await _apiClient.PostAsync<CompanyWorkOrderLaborUsageReference,CompanyWorkOrderLaborUsageRecovery>("/api/field/current/maintenance-handoffs/execution-permit/labor-usage/recovery/reference",reference,cancellationToken)
            ??throw new InvalidOperationException("Original labor remains unconfirmed; retain its reference.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original private labor recovery context changed.");result.Require(selected.Scope,selected.UserId,reference);
        if(!selected.Roles.Contains(result.Command.Review.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original configured execution role is required for labor recovery.");
        return CompanyInventorySourceReview.Copy(result);
    }
    private Beep.OilandGas.Client.App.CompanyClientSelection MaterialUsageActor()
    {
        var selected=SubmissionActor();if(!selected.Permissions.Contains("Inventory.View",StringComparer.Ordinal))
            throw new UnauthorizedAccessException("Current inventory evidence capability is required for physical material use.");return selected;
    }
    public async Task<CompanyWorkOrderMaterialUsageReview> ReviewWorkOrderMaterialUsageAsync(CompanyWorkOrderExecutionStartReference reference,CancellationToken cancellationToken=default)
    {
        reference.Require();var snapshot=_apiClient.CompanySnapshot;var selected=MaterialUsageActor();
        var result=await _apiClient.PostAsync<CompanyWorkOrderExecutionStartReference,CompanyWorkOrderMaterialUsageReview>("/api/field/current/maintenance-handoffs/execution-permit/material-usage/review",reference,cancellationToken)
            ??throw new InvalidOperationException("Current original reservation/start/stock usage review is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original material usage company/account changed.");result.Require();
        if(result.Start.Company!=selected.Scope || result.Start.ExecutorUserId!=selected.UserId || result.Start.Permit!=reference.Permit || result.Start.IntentId!=reference.IntentId ||
            !selected.Roles.Contains(result.Start.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original executing company/account/start/configured role is required.");
        return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderMaterialUsageRecovery> RecordWorkOrderMaterialUsageAsync(CompanyWorkOrderMaterialUsageCommand original,CancellationToken cancellationToken=default)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var snapshot=_apiClient.CompanySnapshot;var selected=MaterialUsageActor();var start=command.Review.Start;
        if(start.Company!=selected.Scope || start.ExecutorUserId!=selected.UserId || !selected.Roles.Contains(start.AssignedRole,StringComparer.Ordinal))
            throw new UnauthorizedAccessException("Original material use company/account/configured execution role is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderMaterialUsageCommand,CompanyWorkOrderMaterialUsageRecovery>("/api/field/current/maintenance-handoffs/execution-permit/material-usage",command,cancellationToken)
            ??throw new InvalidOperationException("Original material usage acknowledgement is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original physical material use context changed.");
        result.Require(selected.Scope,selected.UserId,new(new(start.Permit,start.IntentId),command.IntentId));
        if(System.Text.Json.JsonSerializer.Serialize(result.Command)!=System.Text.Json.JsonSerializer.Serialize(command))throw new InvalidOperationException("Recorded material use differs from the retained original command.");
        return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderMaterialUsageRecovery> RecoverWorkOrderMaterialUsageAsync(CompanyWorkOrderMaterialUsageReference reference,CancellationToken cancellationToken=default)
    {
        reference.Require();var snapshot=_apiClient.CompanySnapshot;var selected=MaterialUsageActor();
        var result=await _apiClient.PostAsync<CompanyWorkOrderMaterialUsageReference,CompanyWorkOrderMaterialUsageRecovery>("/api/field/current/maintenance-handoffs/execution-permit/material-usage/recovery/reference",reference,cancellationToken)
            ??throw new InvalidOperationException("Original private material usage recovery is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original material usage recovery context changed.");
        result.Require(selected.Scope,selected.UserId,reference);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderSubmissionReview> ReviewWorkOrderSubmissionAsync(CompanyWorkOrderExecutionStartReference reference,CancellationToken cancellationToken=default)
    {
        reference.Require();var snapshot=_apiClient.CompanySnapshot;var selected=SubmissionActor();
        var result=await _apiClient.PostAsync<CompanyWorkOrderExecutionStartReference,CompanyWorkOrderSubmissionReview>("/api/field/current/maintenance-handoffs/execution-permit/work-submission/review",reference,cancellationToken)??throw new InvalidOperationException("Complete original work-submission review is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original submission review company/account changed.");result.Require();result.Inspections.Require(selected.Scope,selected.UserId,reference);
        if(!selected.Roles.Contains(result.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original configured execution role is required.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderSubmissionReceipt> SubmitWorkOrderAsync(CompanyWorkOrderSubmissionCommand original,CancellationToken cancellationToken=default)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var snapshot=_apiClient.CompanySnapshot;var selected=SubmissionActor();var context=command.Review.Inspections.Current.Context;
        if(context.Company!=selected.Scope || context.ActorUserId!=selected.UserId || !selected.Roles.Contains(command.Review.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Return to the original execution company/account/role before work submission.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderSubmissionCommand,CompanyWorkOrderSubmissionReceipt>("/api/field/current/maintenance-handoffs/execution-permit/work-submission",command,cancellationToken)??throw new InvalidOperationException("Original work-submission acknowledgement is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original work-submission context changed.");result.Require(command);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderSubmissionRecovery> RecoverWorkOrderSubmissionAsync(CompanyWorkOrderSubmissionReference reference,CancellationToken cancellationToken=default)
    {
        reference.Require();var snapshot=_apiClient.CompanySnapshot;var selected=SubmissionActor();
        var result=await _apiClient.PostAsync<CompanyWorkOrderSubmissionReference,CompanyWorkOrderSubmissionRecovery>("/api/field/current/maintenance-handoffs/execution-permit/work-submission/recovery/reference",reference,cancellationToken)??throw new InvalidOperationException("Original work-submission recovery is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original work-submission recovery context changed.");result.Require(selected.Scope,selected.UserId,reference);return CompanyInventorySourceReview.Copy(result);
    }
    private Beep.OilandGas.Client.App.CompanyClientSelection SubmissionActor()
    {var selected=_apiClient.CompanySnapshot.Selection??throw new UnauthorizedAccessException("Select the original execution company/account.");if(!selected.Permissions.Contains("Maintenance.ExecuteWork",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current work-execution capability is required.");return selected;}
    public async Task<CompanyWorkOrderSubmissionInspectionReview> ReviewWorkOrderSubmissionInspectionsAsync(CompanyWorkOrderExecutionStartReference reference,CancellationToken cancellationToken=default)
    {
        reference.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the current execution company/account.");
        if(!selected.Permissions.Contains("Maintenance.ExecuteWork",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current work-execution capability is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderExecutionStartReference,CompanyWorkOrderSubmissionInspectionReview>("/api/field/current/maintenance-handoffs/execution-permit/work-submission/inspection-review",reference,cancellationToken)??throw new InvalidOperationException("Original start/current submission inspections are unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original execution inspection context changed.");result.Require(selected.Scope,selected.UserId,reference);
        if(!selected.Roles.Contains(result.Start.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("The original configured execution role is required.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderDocumentContributorReview> ReviewWorkOrderDocumentContributorsAsync(CompanyWorkOrderPermitDocumentQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the current document contributor reviewer company/account.");if(!selected.Permissions.Contains("HSE.ApprovePermit",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current independent HSE source authority is required.");var result=await _apiClient.PostAsync<CompanyWorkOrderPermitDocumentQuery,CompanyWorkOrderDocumentContributorReview>("/api/field/current/maintenance-handoffs/execution-permit/documents/contributors",query,cancellationToken)??throw new InvalidOperationException("Complete current contributor/reviewer identity proof is unavailable.");if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original contributor reviewer context changed.");result.Require();result.Document.Require(selected.Scope,selected.UserId,query);if(!selected.Roles.Contains(result.Document.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current configured independent document reviewer role is required.");return CompanyInventorySourceReview.Copy(result);
    }
    private Beep.OilandGas.Client.App.CompanyClientSelection IdentityAdministrator()
    {
        var selected=_apiClient.CompanySnapshot.Selection??throw new UnauthorizedAccessException("Select the original identity-link administrator company/account.");if(!selected.Roles.Contains("Administrator",StringComparer.Ordinal) || !selected.Permissions.Contains("HSE.ConfigurePersonnelIdentity",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current app Administrator and personnel identity permission are required.");return selected;
    }
    public async Task<CompanyPersonnelIdentityCatalog> ReviewCompanyPersonnelIdentitiesAsync(CancellationToken cancellationToken=default)
    {
        var snapshot=_apiClient.CompanySnapshot;var selected=IdentityAdministrator();var result=await _apiClient.GetAsync<CompanyPersonnelIdentityCatalog>("/api/company/current/personnel-identities",cancellationToken)??throw new InvalidOperationException("Complete current identity-link catalog is unavailable.");if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original identity administrator context changed.");result.Require();if(result.Context.Company!=selected.Scope || result.Context.ActorUserId!=selected.UserId)throw new UnauthorizedAccessException("Original identity catalog company/account differs.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyPersonnelIdentityReview> ReviewCompanyPersonnelIdentityAsync(CompanyPersonnelIdentityQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=IdentityAdministrator();var result=await _apiClient.PostAsync<CompanyPersonnelIdentityQuery,CompanyPersonnelIdentityReview>("/api/company/current/personnel-identities/review",query,cancellationToken)??throw new InvalidOperationException("Exact current account/person relationship is unavailable.");if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original identity administrator context changed.");result.Require();if(result.Context.Company!=selected.Scope || result.Context.ActorUserId!=selected.UserId || result.Query!=query)throw new UnauthorizedAccessException("Exact original identity company/account/person differs.");return CompanyInventorySourceReview.Copy(result);
    }
    public Task<COMPANY_PERSONNEL_IDENTITY> RecordCompanyPersonnelIdentityAsync(CompanyPersonnelIdentityCommand command,CancellationToken cancellationToken=default)=>IdentityAsync(command,false,cancellationToken);
    public Task<COMPANY_PERSONNEL_IDENTITY> RecoverCompanyPersonnelIdentityAsync(CompanyPersonnelIdentityCommand command,CancellationToken cancellationToken=default)=>IdentityAsync(command,true,cancellationToken);
    private async Task<COMPANY_PERSONNEL_IDENTITY> IdentityAsync(CompanyPersonnelIdentityCommand original,bool readOnly,CancellationToken token)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var snapshot=_apiClient.CompanySnapshot;var selected=IdentityAdministrator();if(command.Review.Context.Company!=selected.Scope || command.Review.Context.ActorUserId!=selected.UserId)throw new UnauthorizedAccessException("Original identity-link administrator company/account differs.");var result=await _apiClient.PostAsync<CompanyPersonnelIdentityCommand,COMPANY_PERSONNEL_IDENTITY>("/api/company/current/personnel-identities"+(readOnly?"/recovery":""),command,token)??throw new InvalidOperationException("Original identity-link outcome is unconfirmed. Recover its original intent.");if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original identity administrator context changed.");command.RequireRecorded(result);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyPersonnelIdentityRecovery> RecoverCompanyPersonnelIdentityReferenceAsync(CompanyPersonnelIdentityReference reference,CancellationToken cancellationToken=default)
    {
        reference.Require();var snapshot=_apiClient.CompanySnapshot;var selected=IdentityAdministrator();var result=await _apiClient.PostAsync<CompanyPersonnelIdentityReference,CompanyPersonnelIdentityRecovery>("/api/company/current/personnel-identities/recovery/reference",reference,cancellationToken)??throw new InvalidOperationException("Original private identity reference is unavailable.");if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original identity administrator context changed.");result.Require(selected.Scope,selected.UserId,reference);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderPermitDocumentContent> ReadWorkOrderPermitDocumentContentAsync(CompanyWorkOrderPermitDocumentReview original,CancellationToken cancellationToken=default)
    {
        var review=CompanyInventorySourceReview.Copy(original);if(review is null)throw new InvalidOperationException("Original document review is unavailable.");review.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the original document reviewer company/account.");review.Require(selected.Scope,selected.UserId,review.Source.Query);
        if(!selected.Permissions.Contains("HSE.ApprovePermit",StringComparer.Ordinal) || !selected.Roles.Contains(review.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current independent document reviewer role and permission are required.");
        if(review.Blockers.Count!=1 || review.Blockers[0]!=CompanyWorkOrderPermitDocumentReview.PendingAcceptanceBlocker)throw new InvalidOperationException("Confirmed unrestricted document evidence is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderPermitDocumentReview,CompanyWorkOrderPermitDocumentContent>("/api/field/current/maintenance-handoffs/execution-permit/documents/content",review,cancellationToken)??throw new InvalidOperationException("Original document content is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original document reviewer context changed.");result.Require(review);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderDocumentCatalog> ReviewWorkOrderPermitDocumentCatalogAsync(CompanyWorkOrderExecutionPermitReference query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the current document reviewer company/account.");if(!selected.Permissions.Contains("HSE.ApprovePermit",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current independent HSE source authority is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderExecutionPermitReference,CompanyWorkOrderDocumentCatalog>("/api/field/current/maintenance-handoffs/execution-permit/documents/catalog",query,cancellationToken)??throw new InvalidOperationException("Complete recorded document links are unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original document reviewer context changed.");result.Require(selected.Scope,selected.UserId,query);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderPermitDocumentReview> ReviewWorkOrderPermitDocumentSourceAsync(CompanyWorkOrderPermitDocumentQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the current document reviewer company/account.");if(!selected.Permissions.Contains("HSE.ApprovePermit",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current independent HSE source authority is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderPermitDocumentQuery,CompanyWorkOrderPermitDocumentReview>("/api/field/current/maintenance-handoffs/execution-permit/documents/source",query,cancellationToken)??throw new InvalidOperationException("Complete recorded permit document source is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original document reviewer context changed.");result.Require(selected.Scope,selected.UserId,query);if(!selected.Roles.Contains(result.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current configured independent document reviewer role is required.");return CompanyInventorySourceReview.Copy(result);
    }
    public Task<Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION> RecordWorkOrderDocumentAcceptanceAsync(CompanyWorkOrderDocumentAcceptanceCommand command,CancellationToken cancellationToken=default)=>DocumentAcceptanceAsync(command,false,cancellationToken);
    public Task<Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION> RecoverWorkOrderDocumentAcceptanceAsync(CompanyWorkOrderDocumentAcceptanceCommand command,CancellationToken cancellationToken=default)=>DocumentAcceptanceAsync(command,true,cancellationToken);
    private async Task<Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION> DocumentAcceptanceAsync(CompanyWorkOrderDocumentAcceptanceCommand command,bool readOnly,CancellationToken token)
    {
        command.Require();if(!readOnly)command.RequireCurrentAcceptance();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the original independent document reviewer company/account.");var context=command.Review.PermitReview.Context;
        if(selected.Scope!=context.Company || selected.UserId!=context.ActorUserId || !selected.Roles.Contains(command.Review.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal) || !selected.Permissions.Contains("HSE.ApprovePermit",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original independent HSE reviewer/company/configured role is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderDocumentAcceptanceCommand,Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION>("/api/field/current/maintenance-handoffs/execution-permit/document-acceptance"+(readOnly?"/recovery":""),command,token)??throw new InvalidOperationException("Original document acceptance is unconfirmed. Recover its retained original intent.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original independent document reviewer context changed.");command.RequireRecorded(result);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderDocumentAcceptanceRecovery> RecoverWorkOrderDocumentAcceptanceReferenceAsync(CompanyWorkOrderDocumentAcceptanceReference reference,CancellationToken cancellationToken=default)
    {
        reference.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the original independent document reviewer company/account.");if(!selected.Permissions.Contains("HSE.ApprovePermit",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current independent HSE reviewer authority is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderDocumentAcceptanceReference,CompanyWorkOrderDocumentAcceptanceRecovery>("/api/field/current/maintenance-handoffs/execution-permit/document-acceptance/recovery/reference",reference,cancellationToken)??throw new InvalidOperationException("Original private document acceptance reference is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original private document acceptance context changed.");result.Require(selected.Scope,selected.UserId,reference);if(!selected.Roles.Contains(result.Command.Review.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original configured document reviewer role is required.");return CompanyInventorySourceReview.Copy(result);
    }
    public Task<Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION> RecordWorkOrderPersonnelAcceptanceAsync(CompanyWorkOrderPersonnelAcceptanceCommand command,CancellationToken cancellationToken=default)=>PersonnelAcceptanceAsync(command,false,cancellationToken);
    public Task<Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION> RecoverWorkOrderPersonnelAcceptanceAsync(CompanyWorkOrderPersonnelAcceptanceCommand command,CancellationToken cancellationToken=default)=>PersonnelAcceptanceAsync(command,true,cancellationToken);
    private async Task<Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION> PersonnelAcceptanceAsync(CompanyWorkOrderPersonnelAcceptanceCommand command,bool readOnly,CancellationToken token)
    {
        command.Require();if(!readOnly && command.Review.EligibilityEvidenceVersion!=1)throw new InvalidOperationException("Current explicit employment eligibility review is required for new acceptance.");var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the original independent personnel reviewer company/account.");var context=command.Review.Assignment.CurrentPersonnel.PermitReview.Context;
        if(selected.Scope!=context.Company || selected.UserId!=context.ActorUserId || !selected.Roles.Contains(command.Review.Assignment.CurrentPersonnel.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal) || !selected.Permissions.Contains("HSE.ApprovePermit",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original independent HSE reviewer/company/configured role is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderPersonnelAcceptanceCommand,Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION>("/api/field/current/maintenance-handoffs/execution-permit/personnel-acceptance"+(readOnly?"/recovery":""),command,token)??throw new InvalidOperationException("Original personnel acceptance is unconfirmed. Recover its retained original intent.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original independent personnel reviewer context changed.");command.RequireRecorded(result);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderPersonnelAcceptanceRecovery> RecoverWorkOrderPersonnelAcceptanceReferenceAsync(CompanyWorkOrderPersonnelAcceptanceReference reference,CancellationToken cancellationToken=default)
    {
        reference.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the original independent personnel reviewer company/account.");if(!selected.Permissions.Contains("HSE.ApprovePermit",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current independent HSE reviewer authority is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderPersonnelAcceptanceReference,CompanyWorkOrderPersonnelAcceptanceRecovery>("/api/field/current/maintenance-handoffs/execution-permit/personnel-acceptance/recovery/reference",reference,cancellationToken)??throw new InvalidOperationException("Original private personnel acceptance reference is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original private personnel acceptance context changed.");result.Require(selected.Scope,selected.UserId,reference);if(!selected.Roles.Contains(result.Command.Review.Assignment.CurrentPersonnel.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original configured personnel reviewer role is required.");return CompanyInventorySourceReview.Copy(result);
    }
    private Beep.OilandGas.Client.App.CompanyClientSelection PersonnelPolicyAdministrator()
    {
        var selected=_apiClient.CompanySnapshot.Selection??throw new UnauthorizedAccessException("Select the original policy administrator company/account.");
        if(!selected.Roles.Contains("Administrator",StringComparer.Ordinal) || !selected.Permissions.Contains("HSE.ConfigurePersonnelPolicy",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current app Administrator and personnel policy permission are required.");return selected;
    }
    public async Task<CompanyPersonnelPolicyCatalog> ReviewCompanyPersonnelPoliciesAsync(CancellationToken cancellationToken=default)
    {
        var snapshot=_apiClient.CompanySnapshot;var selected=PersonnelPolicyAdministrator();var result=await _apiClient.GetAsync<CompanyPersonnelPolicyCatalog>("/api/company/personnel-policies/review",cancellationToken)??throw new InvalidOperationException("Complete company personnel policy catalog is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original policy administrator context changed.");result.Require();if(result.Context.Company!=selected.Scope || result.Context.ActorUserId!=selected.UserId)throw new UnauthorizedAccessException("Original personnel policy company/account differs.");return CompanyInventorySourceReview.Copy(result);
    }
    public Task<WORK_ORDER_PERSONNEL_POLICY> RecordCompanyPersonnelPolicyAsync(CompanyPersonnelPolicyCommand command,CancellationToken cancellationToken=default)=>PersonnelPolicyAsync(command,false,cancellationToken);
    public Task<WORK_ORDER_PERSONNEL_POLICY> RecoverCompanyPersonnelPolicyAsync(CompanyPersonnelPolicyCommand command,CancellationToken cancellationToken=default)=>PersonnelPolicyAsync(command,true,cancellationToken);
    private async Task<WORK_ORDER_PERSONNEL_POLICY> PersonnelPolicyAsync(CompanyPersonnelPolicyCommand command,bool readOnly,CancellationToken token)
    {
        command.Require();var snapshot=_apiClient.CompanySnapshot;var selected=PersonnelPolicyAdministrator();if(command.Context.Company!=selected.Scope || command.Context.ActorUserId!=selected.UserId)throw new UnauthorizedAccessException("Original policy maker/company differs.");
        var result=await _apiClient.PostAsync<CompanyPersonnelPolicyCommand,WORK_ORDER_PERSONNEL_POLICY>("/api/company/personnel-policies/"+(readOnly?"recovery":"record"),command,token)??throw new InvalidOperationException("Original personnel policy append is unconfirmed; recover its retained intent.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original policy maker context changed.");command.RequireRecorded(result);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyPersonnelPolicyRecovery> RecoverCompanyPersonnelPolicyReferenceAsync(CompanyPersonnelPolicyReference reference,CancellationToken cancellationToken=default)
    {
        reference.Require();var snapshot=_apiClient.CompanySnapshot;var selected=PersonnelPolicyAdministrator();var result=await _apiClient.PostAsync<CompanyPersonnelPolicyReference,CompanyPersonnelPolicyRecovery>("/api/company/personnel-policies/recovery/reference",reference,cancellationToken)??throw new InvalidOperationException("Original private personnel policy reference is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original policy reference context changed.");result.Require(selected.Scope,selected.UserId,reference);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyPersonnelPolicyEvidenceReview> ReviewWorkOrderPersonnelPolicyEvidenceAsync(CompanyWorkOrderAssignedPersonnelQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the original HSE company/account.");if(!selected.Permissions.Contains("HSE.ApprovePermit",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current HSE source authority is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderAssignedPersonnelQuery,CompanyPersonnelPolicyEvidenceReview>("/api/field/current/maintenance-handoffs/execution-permit/personnel-assignments/policy-evidence",query,cancellationToken)??throw new InvalidOperationException("Complete current personnel policy/source comparison is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original personnel policy evidence context changed.");result.Require(selected.Scope,selected.UserId,query);if(result.EligibilityEvidenceVersion!=1)throw new InvalidOperationException("Current employment eligibility review is unavailable; refresh the current API.");if(!selected.Roles.Contains(result.Assignment.CurrentPersonnel.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current configured HSE role is required.");return CompanyInventorySourceReview.Copy(result);
    }
    public Task<WORK_ORDER_PERSONNEL_RELEASE> RecordWorkOrderPersonnelReleaseAsync(CompanyWorkOrderPersonnelReleaseCommand command,CancellationToken cancellationToken=default)=>PersonnelReleaseAsync(command,false,cancellationToken);
    public Task<WORK_ORDER_PERSONNEL_RELEASE> RecoverWorkOrderPersonnelReleaseAsync(CompanyWorkOrderPersonnelReleaseCommand command,CancellationToken cancellationToken=default)=>PersonnelReleaseAsync(command,true,cancellationToken);
    private async Task<WORK_ORDER_PERSONNEL_RELEASE> PersonnelReleaseAsync(CompanyWorkOrderPersonnelReleaseCommand command,bool readOnly,CancellationToken token)
    {
        command.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the original personnel cancellation company/account.");var context=command.Original.Command.Review.Personnel.Context;
        if(selected.Scope!=context.Company || selected.UserId!=context.ActorUserId || !selected.Roles.Contains(command.Original.Command.Review.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal) || !selected.Permissions.Contains("HSE.ApprovePermit",StringComparer.Ordinal) || !selected.Permissions.Contains("Maintenance.ReleasePersonnel",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original personnel maker, configured standard role and cancellation authority are required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderPersonnelReleaseCommand,WORK_ORDER_PERSONNEL_RELEASE>("/api/field/current/maintenance-handoffs/execution-permit/personnel-releases"+(readOnly?"/recovery":""),command,token)??throw new InvalidOperationException("Original personnel cancellation is unconfirmed. Recover its retained intent.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original personnel cancellation company/account changed.");command.RequireRecorded(result);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderPersonnelReleaseRecovery> RecoverWorkOrderPersonnelReleaseReferenceAsync(CompanyWorkOrderPersonnelReleaseReference reference,CancellationToken cancellationToken=default)
    {
        reference.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the original cancellation maker company/account.");
        if(!selected.Permissions.Contains("HSE.ApprovePermit",StringComparer.Ordinal) || !selected.Permissions.Contains("Maintenance.ReleasePersonnel",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original personnel cancellation authority is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderPersonnelReleaseReference,CompanyWorkOrderPersonnelReleaseRecovery>("/api/field/current/maintenance-handoffs/execution-permit/personnel-releases/recovery/reference",reference,cancellationToken)??throw new InvalidOperationException("Original private personnel cancellation reference is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original personnel cancellation context changed.");result.Require(selected.Scope,selected.UserId,reference);
        if(!selected.Roles.Contains(result.Command.Original.Command.Review.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original configured personnel role is required.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderPersonnelAssignmentRosterReview> ReviewWorkOrderPersonnelAssignmentsAsync(CompanyWorkOrderExecutionPermitReference query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the original personnel reviewer company/account.");
        if(!selected.Permissions.Contains("HSE.ApprovePermit",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current HSE source-review authority is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderExecutionPermitReference,CompanyWorkOrderPersonnelAssignmentRosterReview>("/api/field/current/maintenance-handoffs/execution-permit/personnel-assignments/roster",query,cancellationToken)??throw new InvalidOperationException("Complete original permit/personnel roster is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original personnel reviewer company/account changed.");result.Require(selected.Scope,selected.UserId,query);
        if(!selected.Roles.Contains(result.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original configured company role is required.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderAssignedPersonnelReview> ReviewWorkOrderAssignedPersonnelAsync(CompanyWorkOrderAssignedPersonnelQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the original personnel reviewer company/account.");
        if(!selected.Permissions.Contains("HSE.ApprovePermit",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current HSE source-review authority is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderAssignedPersonnelQuery,CompanyWorkOrderAssignedPersonnelReview>("/api/field/current/maintenance-handoffs/execution-permit/personnel-assignments/current",query,cancellationToken)??throw new InvalidOperationException("Complete original assignment/current personnel comparison is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original assigned-personnel company/account changed.");result.Require(selected.Scope,selected.UserId,query);
        if(!selected.Roles.Contains(result.CurrentPersonnel.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original configured company role is required.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderPersonnelAssignmentRecovery> RecoverWorkOrderPersonnelAssignmentReferenceAsync(CompanyWorkOrderPersonnelAssignmentReference reference,CancellationToken cancellationToken=default)
    {
        reference.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the original assignment company/account.");
        if(!selected.Permissions.Contains("HSE.ApprovePermit",StringComparer.Ordinal) || !selected.Permissions.Contains("Maintenance.AssignPersonnel",StringComparer.Ordinal) && !selected.Permissions.Contains("Maintenance.ReleasePersonnel",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original assignment or cancellation permission is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderPersonnelAssignmentReference,CompanyWorkOrderPersonnelAssignmentRecovery>("/api/field/current/maintenance-handoffs/execution-permit/personnel-assignments/recovery/reference",reference,cancellationToken)??throw new InvalidOperationException("Original private personnel assignment reference is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original private personnel assignment context changed.");
        result.Require(selected.Scope,selected.UserId,reference);if(!selected.Roles.Contains(result.Command.Review.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original assigned company role is required.");
        return CompanyInventorySourceReview.Copy(result);
    }
    public Task<WORK_ORDER_PERSONNEL_ASSIGNMENT> RecordWorkOrderPersonnelAssignmentAsync(CompanyWorkOrderPersonnelAssignmentCommand command,CancellationToken cancellationToken=default)=>PersonnelAssignmentAsync(command,false,cancellationToken);
    public Task<WORK_ORDER_PERSONNEL_ASSIGNMENT> RecoverWorkOrderPersonnelAssignmentAsync(CompanyWorkOrderPersonnelAssignmentCommand command,CancellationToken cancellationToken=default)=>PersonnelAssignmentAsync(command,true,cancellationToken);
    private async Task<WORK_ORDER_PERSONNEL_ASSIGNMENT> PersonnelAssignmentAsync(CompanyWorkOrderPersonnelAssignmentCommand command,bool readOnly,CancellationToken token)
    {
        command.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the original personnel assignment company/account.");
        var context=command.Review.Personnel.Context;
        if(selected.Scope!=context.Company || selected.UserId!=context.ActorUserId || !selected.Roles.Contains(command.Review.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal) || !selected.Permissions.Contains("HSE.ApprovePermit",StringComparer.Ordinal) || !selected.Permissions.Contains("Maintenance.AssignPersonnel",StringComparer.Ordinal))throw new UnauthorizedAccessException("Original configured company role, HSE source and personnel assignment permissions are required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderPersonnelAssignmentCommand,WORK_ORDER_PERSONNEL_ASSIGNMENT>("/api/field/current/maintenance-handoffs/execution-permit/personnel-assignments"+(readOnly?"/recovery":""),command,token)??throw new InvalidOperationException("Original personnel assignment is unconfirmed. Recover its retained original command.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original personnel assignment company/account changed.");
        command.RequireRecorded(result);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderPersonnelSourceReview> ReviewWorkOrderPersonnelSourceAsync(CompanyWorkOrderPersonnelSourceQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the original personnel reviewer company/account.");if(!selected.Permissions.Contains("HSE.ApprovePermit",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current independent HSE reviewer permission is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderPersonnelSourceQuery,CompanyWorkOrderPersonnelSourceReview>("/api/field/current/maintenance-handoffs/execution-permit/personnel-source",query,cancellationToken)??throw new InvalidOperationException("Complete original company personnel source is unavailable.");if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original personnel company/account changed.");result.Require(selected.Scope,selected.UserId,query);if(!selected.Roles.Contains(result.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original assigned company role is required.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderMaterialAcceptanceRecovery> RecoverWorkOrderMaterialAcceptanceReferenceAsync(CompanyWorkOrderMaterialAcceptanceReference reference,CancellationToken cancellationToken=default)
    {
        reference.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the original material reviewer company/account.");if(!selected.Permissions.Contains("HSE.ApprovePermit",StringComparer.Ordinal) || !selected.Permissions.Contains("Inventory.View",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original material reviewer permissions are required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderMaterialAcceptanceReference,CompanyWorkOrderMaterialAcceptanceRecovery>("/api/field/current/maintenance-handoffs/execution-permit/material-acceptance/recovery/reference",reference,cancellationToken)??throw new InvalidOperationException("Original private material acceptance reference is unavailable.");if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original private reference company/account changed.");result.Require(selected.Scope,selected.UserId,reference);if(!selected.Roles.Contains(result.Command.Review.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original assigned company role is required.");return CompanyInventorySourceReview.Copy(result);
    }
    public Task<Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION> RecordWorkOrderMaterialAcceptanceAsync(CompanyWorkOrderMaterialAcceptanceCommand command,CancellationToken cancellationToken=default)=>MaterialAcceptanceAsync(command,false,cancellationToken);
    public Task<Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION> RecoverWorkOrderMaterialAcceptanceAsync(CompanyWorkOrderMaterialAcceptanceCommand command,CancellationToken cancellationToken=default)=>MaterialAcceptanceAsync(command,true,cancellationToken);
    private async Task<Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION> MaterialAcceptanceAsync(CompanyWorkOrderMaterialAcceptanceCommand original,bool readOnly,CancellationToken token)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the original material reviewer company/account.");var source=command.Review.PermitReview;
        if(selected.Scope!=source.Context.Company || selected.UserId!=source.Context.ActorUserId || !selected.Roles.Contains(source.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal) || !selected.Permissions.Contains("HSE.ApprovePermit",StringComparer.Ordinal) || !selected.Permissions.Contains("Inventory.View",StringComparer.Ordinal))throw new UnauthorizedAccessException("Original independent material reviewer/assigned app role and inventory permission are required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderMaterialAcceptanceCommand,Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION>("/api/field/current/maintenance-handoffs/execution-permit/material-acceptance"+(readOnly?"/recovery":""),command,token)??throw new InvalidOperationException("Original material acceptance is unconfirmed. Recover the retained original intent.");if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original material reviewer company/account changed.");command.RequireRecorded(result);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderMaterialCatalog> ReviewWorkOrderMaterialCatalogAsync(CompanyMaintenanceReceivingQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the original task company/account.");if(!selected.Permissions.Contains("Inventory.View",StringComparer.Ordinal) || !selected.Permissions.Contains("Inventory.ReserveWorkOrderMaterial",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current inventory view and reservation permissions are required.");
        var result=await _apiClient.PostAsync<CompanyMaintenanceReceivingQuery,CompanyWorkOrderMaterialCatalog>("/api/field/current/maintenance-handoffs/materials/catalog",query,cancellationToken)??throw new InvalidOperationException("The complete assigned material catalog is unavailable.");if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original task account/company changed during item discovery.");result.Require(selected.Scope,selected.UserId,query);if(!selected.Roles.Contains(result.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current configured assigned role is required.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderMaterialReadinessReview> ReviewWorkOrderPermitMaterialReadinessAsync(CompanyWorkOrderMaterialReadinessQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the original independent company reviewer.");if(!selected.Permissions.Contains("HSE.ApprovePermit",StringComparer.Ordinal) || !selected.Permissions.Contains("Inventory.View",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current independent permit and inventory view permissions are required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderMaterialReadinessQuery,CompanyWorkOrderMaterialReadinessReview>("/api/field/current/maintenance-handoffs/execution-permit/material-readiness",query,cancellationToken)??throw new InvalidOperationException("Complete current material readiness evidence is unavailable.");if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original material reviewer/company changed.");result.Require(selected.Scope,selected.UserId,query);if(!selected.Roles.Contains(result.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current assigned company role is required.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderMaterialReleaseRecovery> RecoverWorkOrderMaterialReleaseReferenceAsync(CompanyWorkOrderMaterialReleaseReferenceQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the original release account/company.");if(!selected.Permissions.Contains("Inventory.View",StringComparer.Ordinal) || !selected.Permissions.Contains("Inventory.ReleaseWorkOrderMaterial",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current inventory release/view permissions are required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderMaterialReleaseReferenceQuery,CompanyWorkOrderMaterialReleaseRecovery>("/api/field/current/maintenance-handoffs/materials/allocations/release/recovery/reference",query,cancellationToken)??throw new InvalidOperationException("Original release reference is unconfirmed.");if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original release company/account changed.");result.Require(selected.Scope,selected.UserId,query);if(!selected.Roles.Contains(result.Command.Original.Command.Review.Source.WorkOrder.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original assigned role is required.");return CompanyInventorySourceReview.Copy(result);
    }
    public Task<WORK_ORDER_MATERIAL_RELEASE> ReleaseWorkOrderMaterialAsync(CompanyWorkOrderMaterialReleaseCommand command,CancellationToken cancellationToken=default)=>ReadOrReleaseMaterial(command,false,cancellationToken);
    public Task<WORK_ORDER_MATERIAL_RELEASE> RecoverWorkOrderMaterialReleaseAsync(CompanyWorkOrderMaterialReleaseCommand command,CancellationToken cancellationToken=default)=>ReadOrReleaseMaterial(command,true,cancellationToken);
    private async Task<WORK_ORDER_MATERIAL_RELEASE> ReadOrReleaseMaterial(CompanyWorkOrderMaterialReleaseCommand original,bool readOnly,CancellationToken token)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the original release company/account.");var source=command.Original.Command.Review.Source;
        if(selected.Scope!=source.Context.Company || selected.UserId!=source.Context.ActorUserId || !selected.Roles.Contains(source.WorkOrder.Stage.AssignedRole,StringComparer.Ordinal) || !selected.Permissions.Contains("Inventory.View",StringComparer.Ordinal) || !selected.Permissions.Contains("Inventory.ReleaseWorkOrderMaterial",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original account/assigned role and inventory release permission are required.");
        var row=await _apiClient.PostAsync<CompanyWorkOrderMaterialReleaseCommand,WORK_ORDER_MATERIAL_RELEASE>("/api/field/current/maintenance-handoffs/materials/allocations/release"+(readOnly?"/recovery":""),command,token)??throw new InvalidOperationException("Original release result is unconfirmed.");if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original release company/account changed.");command.RequireRecorded(row);return CompanyInventorySourceReview.Copy(row);
    }
    public async Task<CompanyWorkOrderMaterialAllocationRecovery> RecoverWorkOrderMaterialAllocationReferenceAsync(CompanyWorkOrderMaterialAllocationReferenceQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the original reservation company/account.");
        if(!selected.Permissions.Contains("Inventory.ReserveWorkOrderMaterial",StringComparer.Ordinal) || !selected.Permissions.Contains("Inventory.View",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original inventory reservation permissions are required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderMaterialAllocationReferenceQuery,CompanyWorkOrderMaterialAllocationRecovery>("/api/field/current/maintenance-handoffs/materials/allocations/recovery/reference",query,cancellationToken)??throw new InvalidOperationException("The complete original reservation reference is unconfirmed.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original recovery company/account changed.");result.Require(selected.Scope,selected.UserId,query);if(!selected.Roles.Contains(result.Command.Review.Source.WorkOrder.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original assigned company role is required.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderMaterialAllocationReview> ReviewWorkOrderMaterialAllocationAsync(CompanyWorkOrderMaterialSourceQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the original allocation company/account.");
        if(!selected.Permissions.Contains("Inventory.ReserveWorkOrderMaterial",StringComparer.Ordinal) || !selected.Permissions.Contains("Inventory.View",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current inventory view and reservation permissions are required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderMaterialSourceQuery,CompanyWorkOrderMaterialAllocationReview>("/api/field/current/maintenance-handoffs/materials/allocation-review",query,cancellationToken)??throw new InvalidOperationException("Complete allocation review is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original allocation account changed.");result.Require();result.Source.Require(selected.Scope,selected.UserId,query);
        if(!selected.Roles.Contains(result.Source.WorkOrder.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current assigned maintenance role is required.");return CompanyInventorySourceReview.Copy(result);
    }
    public Task<WORK_ORDER_MATERIAL_ALLOCATION> RecordWorkOrderMaterialAllocationAsync(CompanyWorkOrderMaterialAllocationCommand command,CancellationToken cancellationToken=default)=>ReadOrRecordMaterialAllocation(command,false,cancellationToken);
    public Task<WORK_ORDER_MATERIAL_ALLOCATION> RecoverWorkOrderMaterialAllocationAsync(CompanyWorkOrderMaterialAllocationCommand command,CancellationToken cancellationToken=default)=>ReadOrRecordMaterialAllocation(command,true,cancellationToken);
    private async Task<WORK_ORDER_MATERIAL_ALLOCATION> ReadOrRecordMaterialAllocation(CompanyWorkOrderMaterialAllocationCommand original,bool readOnly,CancellationToken token)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new UnauthorizedAccessException("Select the original reservation company/account.");var source=command.Review.Source;
        if(selected.Scope!=source.Context.Company || selected.UserId!=source.Context.ActorUserId || !selected.Permissions.Contains("Inventory.ReserveWorkOrderMaterial",StringComparer.Ordinal) || !selected.Permissions.Contains("Inventory.View",StringComparer.Ordinal) || !selected.Roles.Contains(source.WorkOrder.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original reservation account, assigned role and inventory permissions are required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderMaterialAllocationCommand,WORK_ORDER_MATERIAL_ALLOCATION>("/api/field/current/maintenance-handoffs/materials/allocations"+(readOnly?"/recovery":""),command,token)??throw new InvalidOperationException("Original allocation result is unconfirmed.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original reservation account changed.");command.RequireRecorded(result);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderMaterialSourceReview> ReviewWorkOrderMaterialSourceAsync(CompanyWorkOrderMaterialSourceQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Select the original work-order company/account.");if(!selected.Permissions.Contains("Inventory.View",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current company Inventory.View is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderMaterialSourceQuery,CompanyWorkOrderMaterialSourceReview>("/api/field/current/maintenance-handoffs/materials/source-review",query,cancellationToken)??throw new InvalidOperationException("The complete original work-order material source was not returned.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("The original material company/account changed during review.");result.Require(selected.Scope,selected.UserId,query);if(!selected.Roles.Contains(result.WorkOrder.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("The current configured work-order role is required.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderExecutionPermitDecisionRecovery> RecoverWorkOrderExecutionPermitDecisionReferenceAsync(CompanyWorkOrderExecutionPermitDecisionReference query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Select the original decision company/reviewer.");if(!selected.Permissions.Contains("HSE.ApprovePermit",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current independent permit decision authority is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderExecutionPermitDecisionReference,CompanyWorkOrderExecutionPermitDecisionRecovery>("/api/field/current/maintenance-handoffs/execution-permit/decision/recovery/reference",query,cancellationToken)??throw new InvalidOperationException("The original independent decision reference is unconfirmed.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("The decision reviewer/company changed during recovery.");result.Require(selected.Scope,selected.UserId,query);return CompanyInventorySourceReview.Copy(result);
    }
    public Task<CompanyWorkOrderExecutionPermitDecisionReceipt> DecideWorkOrderExecutionPermitAsync(CompanyWorkOrderExecutionPermitDecision decision,CancellationToken cancellationToken=default)=>ReadOrDecidePermitAsync(decision,false,cancellationToken);
    public Task<CompanyWorkOrderExecutionPermitDecisionReceipt> RecoverWorkOrderExecutionPermitDecisionAsync(CompanyWorkOrderExecutionPermitDecision decision,CancellationToken cancellationToken=default)=>ReadOrDecidePermitAsync(decision,true,cancellationToken);
    private async Task<CompanyWorkOrderExecutionPermitDecisionReceipt> ReadOrDecidePermitAsync(CompanyWorkOrderExecutionPermitDecision decision,bool readOnly,CancellationToken token)
    {
        decision=CompanyInventorySourceReview.Copy(decision);decision.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Select the original decision company/reviewer.");
        if(selected.Scope!=decision.Review.Context.Company || selected.UserId!=decision.Review.Context.ActorUserId || !selected.Permissions.Contains("HSE.ApprovePermit",StringComparer.Ordinal))throw new UnauthorizedAccessException("The original independent permit reviewer/company is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderExecutionPermitDecision,CompanyWorkOrderExecutionPermitDecisionReceipt>(readOnly?"/api/field/current/maintenance-handoffs/execution-permit/decision/recovery":"/api/field/current/maintenance-handoffs/execution-permit/decision",decision,token)??throw new InvalidOperationException("The original independent decision is unconfirmed. Retain its identity and body.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("The decision reviewer/company changed before confirmation.");result.Require(decision);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderExecutionPermitApprovalReview> ReviewWorkOrderExecutionPermitApprovalAsync(CompanyWorkOrderExecutionPermitReference query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Select the original permit company/account.");
        if(!selected.Permissions.Contains("HSE.ApprovePermit",StringComparer.Ordinal))throw new UnauthorizedAccessException("Independent HSE.ApprovePermit is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderExecutionPermitReference,CompanyWorkOrderExecutionPermitApprovalReview>("/api/field/current/maintenance-handoffs/execution-permit/approval/review",query,cancellationToken)??throw new InvalidOperationException("The complete independent permit review was not returned.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("The permit reviewer/company changed during review.");result.Require(selected.Scope,selected.UserId,query);
        if(!selected.Roles.Contains(result.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("The current configured maintenance role is required.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderRecordedApproval> ReadRecordedWorkOrderPermitApprovalAsync(CompanyWorkOrderExecutionPermitReference query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Select the recorded permit's company.");
        if(!selected.Permissions.Contains("Maintenance.ViewApprovedPermit",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current Maintenance.ViewApprovedPermit is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderExecutionPermitReference,CompanyWorkOrderRecordedApproval>("/api/field/current/maintenance-handoffs/execution-permit/recorded-approval",query,cancellationToken)??throw new InvalidOperationException("Complete recorded approval facts were not returned.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Company/account changed during recorded approval review.");result.Require(selected.Scope,query);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderExecutionRequirements> ReadWorkOrderExecutionRequirementsAsync(CompanyWorkOrderExecutionPermitReference query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Select the recorded permit's company.");if(!selected.Permissions.Contains("Maintenance.ViewApprovedPermit",StringComparer.Ordinal))throw new UnauthorizedAccessException("Maintenance.ViewApprovedPermit is required for current requirement comparison.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderExecutionPermitReference,CompanyWorkOrderExecutionRequirements>("/api/field/current/maintenance-handoffs/execution-permit/execution-requirements",query,cancellationToken)??throw new InvalidOperationException("Complete current execution requirement evidence was not returned.");if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Execution-view company/actor changed during review.");result.Require(selected.Scope,selected.UserId,query);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderExecutionPrerequisites> ReadWorkOrderExecutionPrerequisitesAsync(CompanyWorkOrderExecutionPermitReference query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Select the original execution company.");
        if(!selected.Permissions.Contains("Maintenance.ExecuteWork",StringComparer.Ordinal) || !selected.Permissions.Contains("Inventory.View",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current execution prerequisite review requires Maintenance.ExecuteWork and Inventory.View.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderExecutionPermitReference,CompanyWorkOrderExecutionPrerequisites>("/api/field/current/maintenance-handoffs/execution-permit/execution-prerequisites",query,cancellationToken)??throw new InvalidOperationException("Complete current execution prerequisite evidence was not returned.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Execution company/account changed during prerequisite review.");result.Require(selected.Scope,selected.UserId,query);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderExecutorIdentity> ReviewWorkOrderExecutionStartAsync(CompanyWorkOrderExecutionPermitReference query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Select the original work-start company.");
        if(!selected.Permissions.Contains("Maintenance.ExecuteWork",StringComparer.Ordinal) || !selected.Permissions.Contains("Inventory.View",StringComparer.Ordinal))throw new UnauthorizedAccessException("Work-start review requires Maintenance.ExecuteWork and Inventory.View.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderExecutionPermitReference,CompanyWorkOrderExecutorIdentity>("/api/field/current/maintenance-handoffs/execution-permit/work-start/review",query,cancellationToken)??throw new InvalidOperationException("Complete current executor/crew review was not returned.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("The work-start company/account changed during review.");result.Require();result.Prerequisites.Require(selected.Scope,selected.UserId,query);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderExecutionStartReceipt> StartWorkOrderExecutionAsync(CompanyWorkOrderExecutionStartCommand command,CancellationToken cancellationToken=default)
    {
        command=CompanyInventorySourceReview.Copy(command);command.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Return to the original work-start company.");
        command.Review.Prerequisites.Require(selected.Scope,selected.UserId,command.Review.Prerequisites.Requirements.Approval.Permit);
        if(!selected.Permissions.Contains("Maintenance.ExecuteWork",StringComparer.Ordinal) || !selected.Permissions.Contains("Inventory.View",StringComparer.Ordinal))throw new UnauthorizedAccessException("Recording work start requires Maintenance.ExecuteWork and Inventory.View.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderExecutionStartCommand,CompanyWorkOrderExecutionStartReceipt>("/api/field/current/maintenance-handoffs/execution-permit/work-start",command,cancellationToken)??throw new InvalidOperationException("Original work-start acknowledgement is unconfirmed; recover its retained intent.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("The original work-start company/account changed during transport; recover its retained intent.");result.Require(command);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderExecutionStartRecovery> RecoverWorkOrderExecutionStartAsync(CompanyWorkOrderExecutionStartReference reference,CancellationToken cancellationToken=default)
    {
        reference.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Return to the original executing company/account.");
        if(!selected.Permissions.Contains("Maintenance.ExecuteWork",StringComparer.Ordinal))throw new UnauthorizedAccessException("Original execution authority is required for private recovery.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderExecutionStartReference,CompanyWorkOrderExecutionStartRecovery>("/api/field/current/maintenance-handoffs/execution-permit/work-start/recovery/reference",reference,cancellationToken)??throw new InvalidOperationException("Original work start remains unconfirmed. Retain its original reference.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Company/account changed during original work-start recovery.");result.Require(selected.Scope,selected.UserId,reference);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderRecordedStart?> ReadRecordedWorkOrderStartAsync(CompanyWorkOrderExecutionPermitReference query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Select the original work-start company.");
        if(!selected.Permissions.Contains("Maintenance.ViewApprovedPermit",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current recorded-work view capability is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderExecutionPermitReference,CompanyWorkOrderRecordedStart>("/api/field/current/maintenance-handoffs/execution-permit/work-start/recorded",query,cancellationToken);
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Company/account changed during recorded work-start history review.");result?.Require(selected.Scope,query);return result is null?null:CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderExecutionPermitRecovery> RecoverWorkOrderExecutionPermitReferenceAsync(CompanyWorkOrderExecutionPermitReference query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Return to the original work-permit company/account.");if(!selected.Permissions.Contains("HSE.IssuePermit",StringComparer.Ordinal))throw new UnauthorizedAccessException("Original permit issue authority is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderExecutionPermitReference,CompanyWorkOrderExecutionPermitRecovery>("/api/field/current/maintenance-handoffs/execution-permit/recovery/reference",query,cancellationToken)??throw new InvalidOperationException("The original pending permit reference is unconfirmed.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("The company/account changed during original pending permit recovery.");result.Require(selected.Scope,selected.UserId,query);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderExecutionPermitIssueReview> ReviewWorkOrderExecutionPermitIssueAsync(CompanyMaintenanceReceivingQuery task,CancellationToken cancellationToken=default)
    {
        task.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Select the original work-permit company/account.");
        if(!selected.Permissions.Contains("HSE.IssuePermit",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current company HSE.IssuePermit is required.");
        var result=await _apiClient.PostAsync<CompanyMaintenanceReceivingQuery,CompanyWorkOrderExecutionPermitIssueReview>("/api/field/current/maintenance-handoffs/execution-permit/review",task,cancellationToken)??throw new InvalidOperationException("The original work-permit issue review was not returned.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("The company/account changed during work-permit issue review.");
        result.Require(selected.Scope,selected.UserId,task);if(!selected.Roles.Contains(result.Source.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("The original assigned maintenance role is required.");return CompanyInventorySourceReview.Copy(result);
    }
    public Task<CompanyWorkOrderExecutionPermitReceipt> IssueWorkOrderExecutionPermitAsync(CompanyWorkOrderExecutionPermitIntent intent,CancellationToken cancellationToken=default)=>ReadOrIssueExecutionPermitAsync(intent,false,cancellationToken);
    public Task<CompanyWorkOrderExecutionPermitReceipt> RecoverWorkOrderExecutionPermitAsync(CompanyWorkOrderExecutionPermitIntent intent,CancellationToken cancellationToken=default)=>ReadOrIssueExecutionPermitAsync(intent,true,cancellationToken);
    private async Task<CompanyWorkOrderExecutionPermitReceipt> ReadOrIssueExecutionPermitAsync(CompanyWorkOrderExecutionPermitIntent original,bool readOnly,CancellationToken cancellationToken)
    {
        var intent=CompanyInventorySourceReview.Copy(original);intent.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Return to the original work-permit company/account.");
        if(selected.Scope!=intent.Context.Company || selected.UserId!=intent.Context.ActorUserId || !selected.Permissions.Contains("HSE.IssuePermit",StringComparer.Ordinal))throw new UnauthorizedAccessException("The original work-permit issuer/company authority is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderExecutionPermitIntent,CompanyWorkOrderExecutionPermitReceipt>(readOnly?"/api/field/current/maintenance-handoffs/execution-permit/recovery":"/api/field/current/maintenance-handoffs/execution-permit",intent,cancellationToken)??throw new InvalidOperationException("Work-permit issue is unconfirmed. Retain the original intent before any reconciliation.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("The company/account changed during original work-permit confirmation.");result.Require(intent);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyMaintenanceInspectionReview> ReviewMaintenanceAssignedInspectionsAsync(CompanyMaintenanceReceivingQuery task,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(task);task.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Select the original maintenance company/account.");
        var result=await _apiClient.PostAsync<CompanyMaintenanceReceivingQuery,CompanyMaintenanceInspectionReview>("/api/field/current/maintenance-handoffs/assigned-stage/inspections",task,cancellationToken)??throw new InvalidOperationException("The recorded assigned inspection review was not returned.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("The company/account changed during assigned inspection review.");
        result.Require(selected.Scope,selected.UserId,task);if(!selected.Roles.Contains(result.Stage.AssignedRole,StringComparer.Ordinal))throw new InvalidOperationException("The configured next maintenance role is required.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyMaintenanceAssignedStageReview> ReviewMaintenanceAssignedStageAsync(CompanyMaintenanceReceivingQuery task,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(task);task.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Select the original maintenance company/account.");
        var result=await _apiClient.PostAsync<CompanyMaintenanceReceivingQuery,CompanyMaintenanceAssignedStageReview>("/api/field/current/maintenance-handoffs/assigned-stage/review",task,cancellationToken)??throw new InvalidOperationException("The recorded next-role maintenance review was not returned.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("The company/account changed during next-role maintenance review.");
        result.Require(selected.Scope,selected.UserId,task);if(!selected.Roles.Contains(result.AssignedRole,StringComparer.Ordinal))throw new InvalidOperationException("The configured next maintenance role is required.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyMaintenanceAcceptanceReview> ReviewMaintenanceReceivingTaskAsync(CompanyMaintenanceReceivingQuery task,CancellationToken cancellationToken=default)
    {
        task.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Select the original receiving company/account.");
        var review=await _apiClient.PostAsync<CompanyMaintenanceReceivingQuery,CompanyMaintenanceAcceptanceReview>("/api/field/current/maintenance-handoffs/acceptance/task-review",task,cancellationToken)??throw new InvalidOperationException("The original receiving task review was not returned.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("The company/account changed during receiving task discovery.");review.Require();
        if(review.Context.Company!=selected.Scope || review.Context.ActorUserId!=selected.UserId || review.InstanceId!=task.InstanceId || review.Assignment.FieldId!=task.FieldId || review.Assignment.WorkOrderId!=task.WorkOrderId || !selected.Roles.Contains(review.AssignedRole,StringComparer.Ordinal) || !selected.Permissions.Contains("Workflow.Approve",StringComparer.Ordinal))throw new InvalidOperationException("The task review differs from the original receiving company/account/role/source.");
        return CompanyInventorySourceReview.Copy(review);
    }
    public async Task<CompanyMaintenanceAcceptanceRecovery> RecoverMaintenanceAcceptanceReferenceAsync(CompanyMaintenanceAcceptanceReferenceQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Select the original receiving company/account.");
        var result=await _apiClient.PostAsync<CompanyMaintenanceAcceptanceReferenceQuery,CompanyMaintenanceAcceptanceRecovery>("/api/field/current/maintenance-handoffs/acceptance/recovery/reference",query,cancellationToken)??throw new InvalidOperationException("The original acceptance reference is unconfirmed.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("The company/account changed during acceptance-reference recovery.");result.Require(selected.Scope,selected.UserId,query);
        if(!selected.Roles.Contains(result.Command.Review.AssignedRole,StringComparer.Ordinal) || !selected.Permissions.Contains("Workflow.Approve",StringComparer.Ordinal))throw new InvalidOperationException("Return to the original configured receiving role/permission.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyMaintenanceAcceptanceReview> ReviewMaintenanceAcceptanceAsync(CompanyMaintenanceRecoveryQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Select the original receiving company/account.");
        var review=await _apiClient.PostAsync<CompanyMaintenanceRecoveryQuery,CompanyMaintenanceAcceptanceReview>("/api/field/current/maintenance-handoffs/acceptance/review",query,cancellationToken)
            ??throw new InvalidOperationException("The original independent receiving review was not returned.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("The company/account changed during receiving review.");
        review.Require();
        if(review.Context.Company!=selected.Scope || review.Context.ActorUserId!=selected.UserId || review.Assignment!=query || !selected.Roles.Contains(review.AssignedRole,StringComparer.Ordinal) || !selected.Permissions.Contains("Workflow.Approve",StringComparer.Ordinal))
            throw new InvalidOperationException("The receiving review differs from the original company/account/configured role/permission.");
        return CompanyInventorySourceReview.Copy(review);
    }
    public Task<CompanyMaintenanceAcceptanceReceipt> AcceptMaintenanceAsync(CompanyMaintenanceAcceptanceCommand command,CancellationToken cancellationToken=default)
        =>ReadOrAcceptMaintenanceAsync(command,false,cancellationToken);
    public Task<CompanyMaintenanceAcceptanceReceipt> RecoverMaintenanceAcceptanceAsync(CompanyMaintenanceAcceptanceCommand command,CancellationToken cancellationToken=default)
        =>ReadOrAcceptMaintenanceAsync(command,true,cancellationToken);
    private async Task<CompanyMaintenanceAcceptanceReceipt> ReadOrAcceptMaintenanceAsync(CompanyMaintenanceAcceptanceCommand command,bool readOnly,CancellationToken cancellationToken)
    {
        command=CompanyInventorySourceReview.Copy(command);command.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Select the original receiving company/account.");
        var review=command.Review;
        if(review.Context.Company!=selected.Scope || review.Context.ActorUserId!=selected.UserId || !selected.Roles.Contains(review.AssignedRole,StringComparer.Ordinal) || !selected.Permissions.Contains("Workflow.Approve",StringComparer.Ordinal))
            throw new InvalidOperationException("Return to the original receiving company/account/configured role/permission.");
        var receipt=await _apiClient.PostAsync<CompanyMaintenanceAcceptanceCommand,CompanyMaintenanceAcceptanceReceipt>(readOnly?"/api/field/current/maintenance-handoffs/acceptance/recovery":"/api/field/current/maintenance-handoffs/acceptance",command,cancellationToken)
            ??throw new InvalidOperationException("Receiving acceptance is unconfirmed. Retain its original reference.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("The company/account changed before receiving acceptance confirmation.");
        receipt.Require(command);return CompanyInventorySourceReview.Copy(receipt);
    }
    public async Task<CompanyMaintenanceStartCatalog> GetMaintenanceHandoffChoicesAsync(CompanyMaintenanceSourceQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Select the original maintenance company/account.");
        var catalog=await _apiClient.PostAsync<CompanyMaintenanceSourceQuery,CompanyMaintenanceStartCatalog>("/api/field/current/maintenance-handoffs/choices",query,cancellationToken)
            ??throw new InvalidOperationException("The configured maintenance workflow choices were not returned.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("The company/account changed during maintenance workflow discovery.");
        catalog.Require(selected.Scope,selected.UserId,query);return CompanyInventorySourceReview.Copy(catalog);
    }
    public async Task<CompanyMaintenanceRecovery> RecoverMaintenanceHandoffReferenceAsync(CompanyMaintenanceRecoveryQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Select the original maintenance company/account.");
        var result=await _apiClient.PostAsync<CompanyMaintenanceRecoveryQuery,CompanyMaintenanceRecovery>("/api/field/current/maintenance-handoffs/recovery/reference",query,cancellationToken)
            ??throw new InvalidOperationException("The original maintenance assignment is unconfirmed. Retain its reference.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("The company/account changed during original maintenance recovery.");
        result.Require(selected.Scope,selected.UserId,query);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyMaintenanceProcessStartReview> ReviewMaintenanceHandoffAsync(CompanyMaintenanceProcessStartQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Select the original maintenance company/account.");
        var review=await _apiClient.PostAsync<CompanyMaintenanceProcessStartQuery,CompanyMaintenanceProcessStartReview>("/api/field/current/maintenance-handoffs/review",query,cancellationToken)
            ??throw new InvalidOperationException("The original maintenance review was not returned.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("The company/account changed during maintenance review.");
        if(review.Context.Company!=selected.Scope || review.Context.ActorUserId!=selected.UserId || review.ProcessId!=query.ProcessId)
            throw new InvalidOperationException("The maintenance review differs from the original company/account/workflow.");
        if(!ProcessDefinitionRevision.IsValid(review.DefinitionRevision) || !LeaseSummaryValidator.Identifier(review.ReceivingRole) || string.IsNullOrWhiteSpace(review.ProcessName))
            throw new InvalidOperationException("The recorded maintenance definition and receiving role are unavailable.");
        review.Source.Require(selected.Scope,selected.UserId,review.Context.FieldId!,query.WorkOrderId,query.WellId,query.DecisionId);
        return CompanyInventorySourceReview.Copy(review);
    }
    public async Task<CompanyMaintenanceProcessStartReceipt> StartMaintenanceHandoffAsync(CompanyMaintenanceProcessStart intent,CancellationToken cancellationToken=default)
        => await ReadOrStartMaintenanceHandoffAsync(intent,false,cancellationToken);
    public async Task<CompanyMaintenanceProcessStartReceipt> RecoverMaintenanceHandoffAsync(CompanyMaintenanceProcessStart intent,CancellationToken cancellationToken=default)
        => await ReadOrStartMaintenanceHandoffAsync(intent,true,cancellationToken);
    private async Task<CompanyMaintenanceProcessStartReceipt> ReadOrStartMaintenanceHandoffAsync(CompanyMaintenanceProcessStart intent,bool readOnly,CancellationToken cancellationToken)
    {
        intent=CompanyInventorySourceReview.Copy(intent);intent.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Select the original maintenance company/account.");
        if(intent.Context.Company!=selected.Scope || intent.Context.ActorUserId!=selected.UserId)throw new InvalidOperationException("Return to the original maintenance company/account before submitting its retained intent.");
        var receipt=await _apiClient.PostAsync<CompanyMaintenanceProcessStart,CompanyMaintenanceProcessStartReceipt>(readOnly ? "/api/field/current/maintenance-handoffs/recovery" : "/api/field/current/maintenance-handoffs",intent,cancellationToken)
            ??throw new InvalidOperationException("The maintenance assignment outcome is unconfirmed. Retain its original reference.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("The company/account changed before maintenance confirmation.");
        receipt.Require(intent);return CompanyInventorySourceReview.Copy(receipt);
    }
}

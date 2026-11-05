using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.LifeCycle.Models;
using Beep.OilandGas.HSE.Models;
using Beep.OilandGas.Accounting.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Analytics;
using Beep.OilandGas.Models.Data.Process;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services;

public sealed partial class BusinessProcessServiceClient : IBusinessProcessServiceClient
{
    public async Task<Beep.OilandGas.ProductionAccounting.Models.CompanyCommercialAmendmentPage> GetCommercialAmendmentPageAsync(int pageNumber,int pageSize,CancellationToken cancellationToken=default)
    {
        Beep.OilandGas.ProductionAccounting.Models.CompanyCommercialAmendmentPage.RequireRequest(pageNumber,pageSize);
        var original=_apiClient.CompanySnapshot;var selection=original.Selection ??throw new InvalidOperationException("Select the original company/account.");
        var result=await _apiClient.GetAsync<Beep.OilandGas.ProductionAccounting.Models.CompanyCommercialAmendmentPage>("/api/accounting/processes/commercial-amendment-queue/pages/"+pageNumber.ToString(CultureInfo.InvariantCulture)+"/"+pageSize.ToString(CultureInfo.InvariantCulture),cancellationToken) ??throw new InvalidOperationException("The complete saved proposal page is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company/account changed during proposal page discovery.");
        result.Require(selection.Scope,selection.UserId,pageNumber,pageSize);return Beep.OilandGas.Models.Data.Accounting.CompanyInventorySourceReview.Copy(result);
    }
    public async Task<Beep.OilandGas.ProductionAccounting.Models.CompanyCommercialAmendmentQueue> GetCommercialAmendmentQueueAsync(CancellationToken cancellationToken=default)
    {
        var original=_apiClient.CompanySnapshot;var selection=original.Selection ??throw new InvalidOperationException("Select the original company/account.");
        var result=await _apiClient.GetAsync<Beep.OilandGas.ProductionAccounting.Models.CompanyCommercialAmendmentQueue>("/api/accounting/processes/commercial-amendment-queue",cancellationToken) ??throw new InvalidOperationException("The complete saved proposal queue is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company/account changed during proposal discovery.");
        result.Require(selection.Scope,selection.UserId);return Beep.OilandGas.Models.Data.Accounting.CompanyInventorySourceReview.Copy(result);
    }
    public async Task<Beep.OilandGas.ProductionAccounting.Models.COMMERCIAL_TERMS_INTENT?> GetCommercialAmendmentIntentAsync(string instanceId,CancellationToken cancellationToken=default)
    {
        if(string.IsNullOrWhiteSpace(instanceId) || instanceId.Length>128 || instanceId.Any(char.IsControl))throw new ArgumentException("The original bounded commercial process is required.");
        var original=_apiClient.CompanySnapshot;var selection=original.Selection ??throw new InvalidOperationException("Select the original company/account.");
        var result=await _apiClient.GetAsync<Beep.OilandGas.ProductionAccounting.Models.CompanyCommercialAmendmentIntentReview>("/api/accounting/processes/"+Uri.EscapeDataString(instanceId)+"/commercial-amendment-intent",cancellationToken)
            ??throw new InvalidOperationException("The saved intent review is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company/account changed during intent recovery.");
        result.Require(selection.Scope,selection.UserId,instanceId);return result.Entry is null?null:Beep.OilandGas.Models.Data.Accounting.CompanyInventorySourceReview.Copy(result.Entry);
    }
    public async Task<Beep.OilandGas.ProductionAccounting.Models.COMMERCIAL_TERMS_INTENT> SaveCommercialAmendmentIntentAsync(Beep.OilandGas.ProductionAccounting.Models.CompanyCommercialAmendmentSubmission command,CancellationToken cancellationToken=default)
    {
        command=Beep.OilandGas.Models.Data.Accounting.CompanyInventorySourceReview.Copy(command);command.Require();var review=command.Review;
        var original=_apiClient.CompanySnapshot;var selection=original.Selection ??throw new InvalidOperationException("Select the original company/account.");
        if(selection.Scope!=review.Company || selection.UserId!=review.ActorUserId)throw new InvalidOperationException("The intent belongs to its original company/account.");
        var result=await _apiClient.PostAsync<Beep.OilandGas.ProductionAccounting.Models.CompanyCommercialAmendmentSubmission,Beep.OilandGas.ProductionAccounting.Models.COMMERCIAL_TERMS_INTENT>("/api/accounting/processes/"+Uri.EscapeDataString(review.InstanceId)+"/commercial-amendment-intent",command,cancellationToken)
            ??throw new InvalidOperationException("The saved commercial intent is unconfirmed.");
        if(original!=_apiClient.CompanySnapshot || System.Text.Json.JsonSerializer.Serialize(result.Restore(selection.Scope,selection.UserId,review.InstanceId))!=System.Text.Json.JsonSerializer.Serialize(command))throw new InvalidOperationException("The original saved intent or company/account changed.");
        return Beep.OilandGas.Models.Data.Accounting.CompanyInventorySourceReview.Copy(result);
    }
    public async Task<Beep.OilandGas.Models.Data.Company.CompanyNativeDecisionHandoffReview> DiscoverNativeDecisionHandoffAsync(Beep.OilandGas.Models.Data.Process.CompanyAccountingProcessStartCatalogQuery query,CancellationToken cancellationToken=default)
    {
        query=Beep.OilandGas.Models.Data.Accounting.CompanyInventorySourceReview.Copy(query);query.Require();var original=_apiClient.CompanySnapshot;
        var selected=original.Selection ??throw new InvalidOperationException("Select the original company and account.");
        var result=await _apiClient.PostAsync<Beep.OilandGas.Models.Data.Process.CompanyAccountingProcessStartCatalogQuery,Beep.OilandGas.Models.Data.Company.CompanyNativeDecisionHandoffReview>("/api/accounting/processes/native-handoff-review",query,cancellationToken)
            ??throw new InvalidOperationException("Native preparation handoff discovery is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company/account changed during handoff discovery.");
        result.Require(selected.Scope,selected.UserId,query);return Beep.OilandGas.Models.Data.Accounting.CompanyInventorySourceReview.Copy(result);
    }
    public async Task<Beep.OilandGas.ProductionAccounting.Models.CompanyCommercialHandoffReview> DiscoverCommercialHandoffAsync(Beep.OilandGas.Models.Data.Process.CompanyAccountingProcessStartCatalogQuery query,CancellationToken cancellationToken=default)
    {
        query=Beep.OilandGas.Models.Data.Accounting.CompanyInventorySourceReview.Copy(query);query.Require();var original=_apiClient.CompanySnapshot;
        var selected=original.Selection ?? throw new InvalidOperationException("Select the original company and account.");
        var result=await _apiClient.PostAsync<Beep.OilandGas.Models.Data.Process.CompanyAccountingProcessStartCatalogQuery,Beep.OilandGas.ProductionAccounting.Models.CompanyCommercialHandoffReview>("/api/accounting/processes/commercial-handoff-review",query,cancellationToken)
            ?? throw new InvalidOperationException("Commercial handoff discovery is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company/account changed during handoff discovery.");
        result.Require(selected.Scope,selected.UserId,query);return Beep.OilandGas.Models.Data.Accounting.CompanyInventorySourceReview.Copy(result);
    }
    public async Task<Beep.OilandGas.ProductionAccounting.Models.CompanyCommercialAmendmentReview> ReviewCommercialAmendmentAsync(string instanceId,CancellationToken cancellationToken=default)
    {
        if(string.IsNullOrWhiteSpace(instanceId) || instanceId.Length>128 || instanceId.Any(char.IsControl))throw new ArgumentException("A bounded original rejection-review process is required.");
        var original=_apiClient.CompanySnapshot;var selection=original.Selection ?? throw new InvalidOperationException("Select the original company and account.");
        var review=await _apiClient.GetAsync<Beep.OilandGas.ProductionAccounting.Models.CompanyCommercialAmendmentReview>("/api/accounting/processes/"+Uri.EscapeDataString(instanceId)+"/commercial-amendment-review",cancellationToken)
            ?? throw new InvalidOperationException("The original amendment review is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company/account changed during amendment review.");
        review.Require(selection.Scope,selection.UserId,instanceId);return Beep.OilandGas.Models.Data.Accounting.CompanyInventorySourceReview.Copy(review);
    }
    public async Task<Beep.OilandGas.Models.Data.Accounting.SalesContractTermsRecord> SubmitCommercialAmendmentAsync(Beep.OilandGas.ProductionAccounting.Models.CompanyCommercialAmendmentSubmission submission,CancellationToken cancellationToken=default)
    {
        submission=Beep.OilandGas.Models.Data.Accounting.CompanyInventorySourceReview.Copy(submission);submission.Require();
        var original=_apiClient.CompanySnapshot;var review=submission.Review;
        if(original.Selection is not {} selection || selection.Scope!=review.Company || selection.UserId!=review.ActorUserId)throw new InvalidOperationException("Return to the original amendment company and account.");
        var result=await _apiClient.PostAsync<Beep.OilandGas.ProductionAccounting.Models.CompanyCommercialAmendmentSubmission,Beep.OilandGas.Models.Data.Accounting.SalesContractTermsRecord>("/api/accounting/processes/"+Uri.EscapeDataString(review.InstanceId)+"/commercial-amendment",submission,cancellationToken)
            ?? throw new InvalidOperationException("The amendment outcome is unconfirmed. Retain the same intent.");
        result.Require();
        if(original!=_apiClient.CompanySnapshot || System.Text.Json.JsonSerializer.Serialize(result.Preparation)!=System.Text.Json.JsonSerializer.Serialize(submission.Amendment.Preparation))
            throw new InvalidOperationException("The original company or amendment outcome changed. Retain the same intent.");
        return Beep.OilandGas.Models.Data.Accounting.CompanyInventorySourceReview.Copy(result);
    }
    public async Task<Beep.OilandGas.ProductionAccounting.Models.CompanyDeliveryProcessSourceReview> GetCompanyDeliverySourceAsync(string instanceId,CancellationToken cancellationToken=default)
    {
        if(string.IsNullOrWhiteSpace(instanceId) || instanceId.Length>128 || instanceId.Any(char.IsControl))throw new ArgumentException("A bounded recorded process identity is required.");
        var original=_apiClient.CompanySnapshot;
        if(original.Selection is null)throw new InvalidOperationException("Select the original company and account.");
        var review=await _apiClient.GetAsync<Beep.OilandGas.ProductionAccounting.Models.CompanyDeliveryProcessSourceReview>("/api/accounting/processes/"+Uri.EscapeDataString(instanceId)+"/delivery-source",cancellationToken)
            ??throw new InvalidOperationException("The recorded delivery process source is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during delivery source review.");
        review.Require(original.Selection.Scope,original.Selection.UserId,instanceId);
        return Beep.OilandGas.Models.Data.Accounting.CompanyInventorySourceReview.Copy(review);
    }
    public async Task<Beep.OilandGas.Accounting.Models.CompanySalesInvoiceProcessSourceReview> GetCompanySalesInvoiceSourceAsync(string instanceId,CancellationToken cancellationToken=default)
    {
        if(string.IsNullOrWhiteSpace(instanceId) || instanceId.Length>128 || instanceId.Any(char.IsControl))throw new ArgumentException("A bounded recorded invoice process identity is required.");
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original invoice company and account.");
        var review=await _apiClient.GetAsync<Beep.OilandGas.Accounting.Models.CompanySalesInvoiceProcessSourceReview>("/api/accounting/processes/"+Uri.EscapeDataString(instanceId)+"/sales-invoice-source",cancellationToken)??throw new InvalidOperationException("The recorded invoice process source is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during invoice process review.");review.Require(original.Selection.Scope,original.Selection.UserId,instanceId);return Beep.OilandGas.Models.Data.Accounting.CompanyInventorySourceReview.Copy(review);
    }
    public async Task<Beep.OilandGas.Accounting.Models.CompanySalesPaymentProcessSourceReview> GetCompanySalesPaymentSourceAsync(string instanceId,CancellationToken cancellationToken=default)
    {
        if(string.IsNullOrWhiteSpace(instanceId) || instanceId.Length>128 || instanceId.Any(char.IsControl))throw new ArgumentException("A bounded recorded payment process identity is required.");
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original payment company/account.");var review=await _apiClient.GetAsync<Beep.OilandGas.Accounting.Models.CompanySalesPaymentProcessSourceReview>("/api/accounting/processes/"+Uri.EscapeDataString(instanceId)+"/sales-payment-source",cancellationToken)??throw new InvalidOperationException("The original payment process source is unavailable.");if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("The company/account changed during payment process review.");review.Require(original.Selection.Scope,original.Selection.UserId,instanceId);return Beep.OilandGas.Models.Data.Accounting.CompanyInventorySourceReview.Copy(review);
    }
    public async Task<Beep.OilandGas.Accounting.Models.CompanySalesJournalProcessSourceReview> GetCompanySalesJournalSourceAsync(string instanceId,CancellationToken cancellationToken=default)
    {
        if(string.IsNullOrWhiteSpace(instanceId) || instanceId.Length>128 || instanceId.Any(char.IsControl))throw new ArgumentException("A bounded recorded journal process identity is required.");
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original journal company and account.");
        var review=await _apiClient.GetAsync<Beep.OilandGas.Accounting.Models.CompanySalesJournalProcessSourceReview>("/api/accounting/processes/"+Uri.EscapeDataString(instanceId)+"/sales-journal-source",cancellationToken)??throw new InvalidOperationException("The recorded journal process source is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during journal process review.");review.Require(original.Selection.Scope,original.Selection.UserId,instanceId);return Beep.OilandGas.Models.Data.Accounting.CompanyInventorySourceReview.Copy(review);
    }
    public async Task<Beep.OilandGas.Accounting.Models.CompanySalesReceivableProcessSourceReview> GetCompanySalesReceivableSourceAsync(string instanceId,CancellationToken cancellationToken=default)
    {
        if(string.IsNullOrWhiteSpace(instanceId) || instanceId.Length>128 || instanceId.Any(char.IsControl))throw new ArgumentException("A bounded recorded receivable process identity is required.");
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original receivable company and account.");
        var review=await _apiClient.GetAsync<Beep.OilandGas.Accounting.Models.CompanySalesReceivableProcessSourceReview>("/api/accounting/processes/"+Uri.EscapeDataString(instanceId)+"/sales-receivable-source",cancellationToken)??throw new InvalidOperationException("The recorded receivable process source is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during receivable process review.");review.Require(original.Selection.Scope,original.Selection.UserId,instanceId);return Beep.OilandGas.Models.Data.Accounting.CompanyInventorySourceReview.Copy(review);
    }
    public async Task<Beep.OilandGas.ProductionAccounting.Models.CompanyCommercialProcessSourceReview> GetCompanyCommercialSourceAsync(string instanceId,CancellationToken cancellationToken=default)
    {
        if(string.IsNullOrWhiteSpace(instanceId) || instanceId.Length>128 || instanceId.Any(char.IsControl))throw new ArgumentException("A bounded recorded process identity is required.");
        var original=_apiClient.CompanySnapshot;
        if(original.Selection is null)throw new InvalidOperationException("Select the original company and account.");
        var review=await _apiClient.GetAsync<Beep.OilandGas.ProductionAccounting.Models.CompanyCommercialProcessSourceReview>("/api/accounting/processes/"+Uri.EscapeDataString(instanceId)+"/commercial-source",cancellationToken)
            ??throw new InvalidOperationException("The recorded commercial process source is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during commercial source review.");
        review.Require(original.Selection.Scope,original.Selection.UserId,instanceId);
        return Beep.OilandGas.Models.Data.Accounting.CompanyInventorySourceReview.Copy(review);
    }
    public async Task<Beep.OilandGas.Models.Data.Accounting.CompanyAccountingStepReceipt> CompleteCompanyAccountingStepAsync(Beep.OilandGas.Models.Data.Accounting.CompanyAccountingStepCommand command,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(command);command.Require();var original=_apiClient.CompanySnapshot;
        if(original.Selection is null || original.Selection.Scope!=command.Review.Company || original.Selection.UserId!=command.Review.ActorUserId)
            throw new InvalidOperationException("Return to the retained command's original company and account before submitting.");
        var frozen=System.Text.Json.JsonSerializer.Deserialize<Beep.OilandGas.Models.Data.Accounting.CompanyAccountingStepCommand>(System.Text.Json.JsonSerializer.Serialize(command))!;
        var receipt=await _apiClient.PostAsync<Beep.OilandGas.Models.Data.Accounting.CompanyAccountingStepCommand,Beep.OilandGas.Models.Data.Accounting.CompanyAccountingStepReceipt>("/api/accounting/processes/step-complete",frozen,cancellationToken)
            ?? throw new InvalidOperationException("The step outcome is unconfirmed. Retain its original intent.");
        receipt.Require(frozen);if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("The company changed while confirming the step. Retain its original intent.");
        return receipt;
    }
    public async Task<Beep.OilandGas.Models.Data.Accounting.CompanyAccountingStepReview?> ReviewCompanyAccountingStepAsync(string instanceId,CancellationToken cancellationToken=default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);var original=_apiClient.CompanySnapshot;
        if(original.Selection is null)throw new InvalidOperationException("Select a company before reviewing its assigned step.");
        var review=await _apiClient.GetAsync<Beep.OilandGas.Models.Data.Accounting.CompanyAccountingStepReview>("/api/accounting/processes/"+Uri.EscapeDataString(instanceId)+"/step-review",cancellationToken);
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("The company changed during assigned step review.");
        review?.Require(original.Selection.Scope,original.Selection.UserId,instanceId);return review;
    }
    private readonly ApiClient _apiClient;
    private readonly ILogger<BusinessProcessServiceClient> _logger;

    public BusinessProcessServiceClient(ApiClient apiClient, ILogger<BusinessProcessServiceClient> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Beep.OilandGas.Models.Data.Process.CompanyAccountingProcessStartCatalog> GetCompanyAccountingStartCatalogAsync(Beep.OilandGas.Models.Data.Process.CompanyAccountingProcessStartCatalogQuery query,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(query);query.Require();var original=_apiClient.CompanySnapshot;
        if(original.Selection is null)throw new InvalidOperationException("Select a company before loading accounting workflow choices.");
        var catalog=await _apiClient.PostAsync<Beep.OilandGas.Models.Data.Process.CompanyAccountingProcessStartCatalogQuery,Beep.OilandGas.Models.Data.Process.CompanyAccountingProcessStartCatalog>(
            "/api/accounting/processes/start-catalog",query,cancellationToken) ?? throw new InvalidOperationException("Recorded accounting workflow choices are unavailable.");
        catalog.Require(original.Selection.Scope,original.Selection.UserId,query);
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("The company changed while loading workflow choices.");
        return catalog;
    }

    public async Task<Beep.OilandGas.Models.Data.Accounting.CompanyAccountingProcessStartReview> ReviewCompanyAccountingStartAsync(Beep.OilandGas.Models.Data.Accounting.CompanyAccountingProcessStartQuery query,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(query);query.Require();var original=_apiClient.CompanySnapshot;
        if(original.Selection is null)throw new InvalidOperationException("Select a company before reviewing an accounting handoff.");
        var review=await _apiClient.PostAsync<Beep.OilandGas.Models.Data.Accounting.CompanyAccountingProcessStartQuery,Beep.OilandGas.Models.Data.Accounting.CompanyAccountingProcessStartReview>(
            "/api/accounting/processes/start-review",query,cancellationToken) ?? throw new InvalidOperationException("The company handoff review is unavailable.");
        review.Require();
        if(original!=_apiClient.CompanySnapshot || review.Company!=original.Selection.Scope || review.ActorUserId!=original.Selection.UserId ||
            review.ProcessId!=query.ProcessId || review.EntityType!=query.EntityType || review.EntityId!=query.EntityId ||
            query.ExpectedSourceRevision is not null && query.ExpectedSourceRevision!=review.SourceRevision)
            throw new InvalidOperationException("The original company accounting source review changed.");
        return review;
    }

    public async Task<Beep.OilandGas.Models.Data.Accounting.CompanyAccountingProcessStartReceipt> StartCompanyAccountingAsync(Beep.OilandGas.Models.Data.Accounting.CompanyAccountingProcessStart intent,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(intent);intent.Require();var original=_apiClient.CompanySnapshot;
        if(original.Selection is null || original.Selection.Scope!=intent.Company || original.Selection.UserId!=intent.ActorUserId)
            throw new InvalidOperationException("Retain the handoff intent and return to its original company before submitting.");
        var frozen=System.Text.Json.JsonSerializer.Deserialize<Beep.OilandGas.Models.Data.Accounting.CompanyAccountingProcessStart>(System.Text.Json.JsonSerializer.Serialize(intent))!;
        var receipt=await _apiClient.PostAsync<Beep.OilandGas.Models.Data.Accounting.CompanyAccountingProcessStart,Beep.OilandGas.Models.Data.Accounting.CompanyAccountingProcessStartReceipt>(
            "/api/accounting/processes/start",frozen,cancellationToken) ?? throw new InvalidOperationException("The handoff outcome is unconfirmed. Retain its original intent.");
        receipt.Require(frozen);
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("The company changed while confirming the handoff. Retain its original intent.");
        return receipt;
    }

    public async Task<ProcessInstance?> GetCompanyAccountingInstanceAsync(string instanceId,CancellationToken cancellationToken=default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);var original=_apiClient.CompanySnapshot;
        if(original.Selection is null)throw new InvalidOperationException("Select a company before reviewing an accounting process.");
        var source=await _apiClient.GetAsync<ProcessInstance>("/api/accounting/processes/"+Uri.EscapeDataString(instanceId),cancellationToken);
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company changed during accounting process review.");
        if(source is null)return null;
        if(source.InstanceId!=instanceId || !string.IsNullOrEmpty(source.FieldId) || source.ProcessData?.InstanceId!=instanceId ||
            source.ProcessData.Data is null || !source.ProcessData.Data.TryGetValue(Beep.OilandGas.Accounting.Models.CompanyAccountingProcessBinding.PayloadKey,out var owner) || owner is null)
            throw new InvalidOperationException("The accounting process scope was not confirmed.");
        Beep.OilandGas.Accounting.Models.CompanyAccountingProcessBinding.FromJson(System.Text.Json.JsonSerializer.Serialize(owner),original.Selection.Scope,
            instanceId,source.EntityType,source.EntityId);
        return source;
    }

    public async Task<Beep.OilandGas.LifeCycle.Models.ProcessTransitionReview> GetTransitionReviewAsync(string instanceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);
        var scope=_apiClient.CompanySnapshot;
        if(scope.Selection is null)throw new InvalidOperationException("Select a company before reviewing process actions.");
        var review=await _apiClient.GetAsync<Beep.OilandGas.LifeCycle.Models.ProcessTransitionReview>($"/api/field/current/process/instances/{Uri.EscapeDataString(instanceId)}/transitions")
            ?? throw new InvalidOperationException("The source action review was empty.");
        if(scope!=_apiClient.CompanySnapshot)throw new InvalidOperationException("The selected company changed during process action review.");
        review.Require(scope.Selection.Scope,scope.Selection.UserId,instanceId);
        return review;
    }

    public async Task<List<ProcessDefinition>> GetDefinitionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<ProcessDefinition>>(
                "/api/field/current/process/definitions",
                cancellationToken);

            return result ?? throw new InvalidOperationException("The configured process catalog was not returned.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process definitions.");
            throw;
        }
    }

    public async Task<ProcessDefinition?> GetDefinitionAsync(string processId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(processId))
            throw new ArgumentException("Process ID is required.", nameof(processId));

        try
        {
            return await _apiClient.GetAsync<ProcessDefinition>(
                $"/api/field/current/process/definitions/{Uri.EscapeDataString(processId)}",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process definition {ProcessId}.", processId);
            throw;
        }
    }

    public async Task<Beep.OilandGas.LifeCycle.Models.ProcessStartSourceReview> ReviewStartSourceAsync(ProcessInstanceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);var scope=_apiClient.CompanySnapshot;
        if(scope.Selection is null)throw new InvalidOperationException("Select a company before reviewing a start source.");
        var review=await _apiClient.PostAsync<ProcessInstanceRequest,Beep.OilandGas.LifeCycle.Models.ProcessStartSourceReview>(
            "/api/field/current/process/instances/start-review",request)
            ?? throw new InvalidOperationException("The recorded start source was empty.");
        if(scope!=_apiClient.CompanySnapshot)throw new InvalidOperationException("The selected company changed during start source review.");
        review.Require(scope.Selection.Scope,scope.Selection.UserId,request);return review;
    }

    public async Task<ProcessInstance?> StartInstanceAsync(ProcessInstanceRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PostAsync<ProcessInstanceRequest, ProcessInstance>(
                "/api/field/current/process/instances",
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting process instance for process {ProcessId} and entity {EntityId}.", request.ProcessId, request.EntityId);
            throw;
        }
    }

    public async Task<ProcessInstance?> SubmitComplianceReportAsync(ComplianceReportRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PostAsync<ComplianceReportRequest, ProcessInstance>(
                "/api/field/current/process/compliance/reports",
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error submitting compliance report for jurisdiction {Jurisdiction} and obligation {ObligationType}.",
                request.JurisdictionTag,
                request.ObligationType);
            throw;
        }
    }

    public async Task<Beep.OilandGas.Models.Data.HSE.IncidentReportOptions> GetHseReportOptionsAsync(CancellationToken cancellationToken = default) =>
        await _apiClient.GetAsync<Beep.OilandGas.Models.Data.HSE.IncidentReportOptions>("/api/field/current/hse/report-options", cancellationToken)
        ?? throw new InvalidOperationException("Recorded incident report options are unavailable.");

    public async Task<Beep.OilandGas.Models.Data.HSE.IncidentReportReceipt> RecordHseIncidentAsync(Beep.OilandGas.Models.Data.HSE.IncidentReportIntent original, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(original); original.Require();
        var snapshot = _apiClient.CompanySnapshot;
        var selected = snapshot.Selection ?? throw new InvalidOperationException("Select the original company and account.");
        if (selected.Scope != original.Context.Company || selected.UserId != original.Context.ActorUserId)
            throw new InvalidOperationException("The report belongs to another company or account.");
        var receipt = await _apiClient.PostAsync<Beep.OilandGas.Models.Data.HSE.IncidentReportIntent, Beep.OilandGas.Models.Data.HSE.IncidentReportReceipt>(
            "/api/field/current/hse/reports", original, cancellationToken)
            ?? throw new InvalidOperationException("The recorded incident report is unconfirmed.");
        if (snapshot != _apiClient.CompanySnapshot) throw new InvalidOperationException("The company or account changed during reporting.");
        receipt.Require(original);
        return receipt;
    }

    public async Task<Beep.OilandGas.HSE.Models.CompanyHseWorkflowReview> ReviewHseWorkflowAsync(string incidentId, CancellationToken cancellationToken = default)
    {
        if (!Beep.OilandGas.Models.Data.Lease.LeaseSummaryValidator.Identifier(incidentId)) throw new ArgumentException("An exact incident is required.", nameof(incidentId));
        var snapshot = _apiClient.CompanySnapshot;
        var selected = snapshot.Selection ?? throw new InvalidOperationException("Select the original company and account.");
        var review = await _apiClient.GetAsync<Beep.OilandGas.HSE.Models.CompanyHseWorkflowReview>(
            "/api/field/current/hse/workflow/review?incidentId=" + Uri.EscapeDataString(incidentId), cancellationToken)
            ?? throw new InvalidOperationException("The workflow review is unavailable.");
        if (snapshot != _apiClient.CompanySnapshot || review.Context is null || review.Context.Company != selected.Scope || review.Context.ActorUserId != selected.UserId || review.IncidentId != incidentId)
            throw new InvalidOperationException("The workflow review belongs to another company, account or incident.");
        _ = review.CreateIntent(Guid.NewGuid());
        return review;
    }

    public async Task<Beep.OilandGas.Models.Data.HSE.IncidentRcaSourceReview> ReviewHseIncidentRcaAsync(string incidentId, CancellationToken cancellationToken = default)
    {
        if (!Beep.OilandGas.Models.Data.Lease.LeaseSummaryValidator.Identifier(incidentId)) throw new ArgumentException("An exact incident is required.", nameof(incidentId));
        var snapshot = _apiClient.CompanySnapshot;
        var selected = snapshot.Selection ?? throw new InvalidOperationException("Select the original company and account.");
        var review = await _apiClient.GetAsync<Beep.OilandGas.Models.Data.HSE.IncidentRcaSourceReview>(
            "/api/field/current/hse/workflow/rca/review?incidentId=" + Uri.EscapeDataString(incidentId), cancellationToken)
            ?? throw new InvalidOperationException("The incident cause review is unavailable.");
        review.Require();
        if (snapshot != _apiClient.CompanySnapshot || review.Context.Company != selected.Scope ||
            review.Context.ActorUserId != selected.UserId || review.IncidentId != incidentId)
            throw new InvalidOperationException("The incident cause review belongs to another company, account or incident.");
        return review;
    }

    public async Task<IReadOnlyList<Beep.OilandGas.Models.Data.HSE.IncidentRcaIntent>> ReadRecordedHseCausesAsync(string incidentId, CancellationToken cancellationToken = default)
    {
        if (!Beep.OilandGas.Models.Data.Lease.LeaseSummaryValidator.Identifier(incidentId)) throw new ArgumentException("An exact incident is required.", nameof(incidentId));
        var snapshot = _apiClient.CompanySnapshot;
        var selected = snapshot.Selection ?? throw new InvalidOperationException("Select the original company and account.");
        var originals = await _apiClient.GetAsync<List<Beep.OilandGas.Models.Data.HSE.IncidentRcaIntent>>(
            "/api/field/current/hse/workflow/rca/recorded?incidentId=" + Uri.EscapeDataString(incidentId), cancellationToken)
            ?? throw new InvalidOperationException("Recorded incident causes are unavailable.");
        if (snapshot != _apiClient.CompanySnapshot || originals.Count > 256 || originals.Any(original => original is null))
            throw new InvalidOperationException("The company or recorded causes changed during recovery.");
        foreach (var original in originals)
        {
            original.Require();
            if (original.Source.Context.Company != selected.Scope || original.Source.Context.ActorUserId != selected.UserId || original.Source.IncidentId != incidentId)
                throw new InvalidOperationException("A recorded cause belongs to another company, account or incident.");
        }
        if (originals.Select(original => original.IntentId).Distinct().Count() != originals.Count) throw new InvalidOperationException("Recorded cause originals are ambiguous.");
        return originals.AsReadOnly();
    }

    public async Task<IReadOnlyList<Beep.OilandGas.Models.Data.HSE.IncidentRcaRecordedCause>> ReadSharedHseCausesAsync(string incidentId, CancellationToken cancellationToken = default)
    {
        if (!Beep.OilandGas.Models.Data.Lease.LeaseSummaryValidator.Identifier(incidentId)) throw new ArgumentException("An exact incident is required.", nameof(incidentId));
        var snapshot = _apiClient.CompanySnapshot; var selected = snapshot.Selection ?? throw new InvalidOperationException("Select a company and account.");
        var causes = await _apiClient.GetAsync<List<Beep.OilandGas.Models.Data.HSE.IncidentRcaRecordedCause>>(
            "/api/field/current/hse/workflow/rca/shared?incidentId=" + Uri.EscapeDataString(incidentId), cancellationToken)
            ?? throw new InvalidOperationException("Shared incident causes are unavailable.");
        if (snapshot != _apiClient.CompanySnapshot || causes.Count > 256 || causes.Any(cause => cause is null))
            throw new InvalidOperationException("The company or shared causes changed during review.");
        foreach (var cause in causes)
        {
            cause.Require();
            if (cause.Company != selected.Scope || cause.IncidentId != incidentId) throw new InvalidOperationException("A shared cause belongs to another company or incident.");
        }
        if (causes.Select(cause => cause.IntentId).Distinct().Count() != causes.Count || causes.Select(cause => cause.Observation).Distinct().Count() != causes.Count)
            throw new InvalidOperationException("Shared causes are ambiguous.");
        return causes.AsReadOnly();
    }

    public async Task<Beep.OilandGas.HSE.Models.CompanyHseRcaHandoffReview> ReviewHseRcaHandoffAsync(Beep.OilandGas.Models.Data.HSE.IncidentRcaRecordedCause original, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(original); original.Require();
        var snapshot = _apiClient.CompanySnapshot;
        var selected = snapshot.Selection ?? throw new InvalidOperationException("Select the original company and account.");
        if (selected.Scope != original.Company)
            throw new InvalidOperationException("The recorded cause belongs to another company.");
        var review = await _apiClient.GetAsync<Beep.OilandGas.HSE.Models.CompanyHseRcaHandoffReview>(
            "/api/field/current/hse/workflow/rca/handoff-review?incidentId=" + Uri.EscapeDataString(original.IncidentId) + "&intentId=" + original.IntentId.ToString("D"), cancellationToken)
            ?? throw new InvalidOperationException("The RCA handoff review is unavailable.");
        review.Require();
        if (snapshot != _apiClient.CompanySnapshot || review.Context.Company != selected.Scope || review.Context.ActorUserId != selected.UserId ||
            System.Text.Json.JsonSerializer.Serialize(review.Cause) != System.Text.Json.JsonSerializer.Serialize(original))
            throw new InvalidOperationException("The RCA handoff review differs from the recorded cause or current account.");
        return review;
    }

    public async Task<Beep.OilandGas.HSE.Models.CompanyHseRcaRecoveredHandoff?> RecoverHseRcaHandoffAsync(string incidentId, CancellationToken cancellationToken = default)
    {
        if (!Beep.OilandGas.Models.Data.Lease.LeaseSummaryValidator.Identifier(incidentId)) throw new ArgumentException("An exact incident is required.", nameof(incidentId));
        var snapshot = _apiClient.CompanySnapshot; var selected = snapshot.Selection ?? throw new InvalidOperationException("Select the original company and account.");
        Beep.OilandGas.HSE.Models.CompanyHseRcaRecoveredHandoff? result;
        try { result = await _apiClient.GetAsync<Beep.OilandGas.HSE.Models.CompanyHseRcaRecoveredHandoff>("/api/field/current/hse/workflow/rca/handoff-recovery?incidentId=" + Uri.EscapeDataString(incidentId), cancellationToken); }
        catch (OilGasApiException error) when (error.StatusCode == System.Net.HttpStatusCode.NotFound)
        { if (snapshot != _apiClient.CompanySnapshot) throw new InvalidOperationException("The company or account changed during recovery."); return null; }
        if (result is null) throw new InvalidOperationException("The RCA handoff recovery response is unavailable.");
        result.Require(); var original = result.Original;
        if (snapshot != _apiClient.CompanySnapshot || original.Review.Context.Company != selected.Scope || original.Review.Context.ActorUserId != selected.UserId || original.Review.Cause.IncidentId != incidentId)
            throw new InvalidOperationException("The recovered handoff belongs to another company, account or incident.");
        return result;
    }

    public async Task<Beep.OilandGas.HSE.Models.CompanyHseRcaHandoffReceipt> CompleteHseRcaHandoffAsync(Beep.OilandGas.HSE.Models.CompanyHseRcaHandoffCommand original, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(original); original.Require(); var snapshot = _apiClient.CompanySnapshot;
        var selected = snapshot.Selection ?? throw new InvalidOperationException("Select the original company and account.");
        if (selected.Scope != original.Review.Context.Company || selected.UserId != original.Review.Context.ActorUserId) throw new InvalidOperationException("The handoff belongs to another company or account.");
        var receipt = await _apiClient.PostAsync<Beep.OilandGas.HSE.Models.CompanyHseRcaHandoffCommand, Beep.OilandGas.HSE.Models.CompanyHseRcaHandoffReceipt>(
            "/api/field/current/hse/workflow/rca/handoff", original, cancellationToken) ?? throw new InvalidOperationException("The RCA handoff is unconfirmed.");
        if (snapshot != _apiClient.CompanySnapshot) throw new InvalidOperationException("The company or account changed during the RCA handoff.");
        receipt.Require(original); return receipt;
    }

    public async Task<Beep.OilandGas.Models.Data.HSE.IncidentRcaReceipt> RecordHseIncidentCauseAsync(Beep.OilandGas.Models.Data.HSE.IncidentRcaIntent original, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(original); original.Require();
        var snapshot = _apiClient.CompanySnapshot;
        var selected = snapshot.Selection ?? throw new InvalidOperationException("Select the original company and account.");
        if (selected.Scope != original.Source.Context.Company || selected.UserId != original.Source.Context.ActorUserId)
            throw new InvalidOperationException("The incident cause belongs to another company or account.");
        var receipt = await _apiClient.PostAsync<Beep.OilandGas.Models.Data.HSE.IncidentRcaIntent, Beep.OilandGas.Models.Data.HSE.IncidentRcaReceipt>(
            "/api/field/current/hse/workflow/rca/record-cause", original, cancellationToken)
            ?? throw new InvalidOperationException("The recorded incident cause is unconfirmed.");
        if (snapshot != _apiClient.CompanySnapshot) throw new InvalidOperationException("The company or account changed during incident cause recording.");
        receipt.Require(original);
        return receipt;
    }

    public async Task<Beep.OilandGas.HSE.Models.CompanyHseWorkflowReceipt> StartHseWorkflowAsync(Beep.OilandGas.HSE.Models.CompanyHseWorkflowStart original, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(original); original.Require();
        var snapshot = _apiClient.CompanySnapshot;
        var selected = snapshot.Selection ?? throw new InvalidOperationException("Select the original company and account.");
        if (selected.Scope != original.Context.Company || selected.UserId != original.Context.ActorUserId)
            throw new InvalidOperationException("The workflow request belongs to another company or account.");
        var receipt = await _apiClient.PostAsync<Beep.OilandGas.HSE.Models.CompanyHseWorkflowStart, Beep.OilandGas.HSE.Models.CompanyHseWorkflowReceipt>(
            "/api/field/current/hse/workflow/start", original, cancellationToken)
            ?? throw new InvalidOperationException("The workflow start is unconfirmed.");
        if (snapshot != _apiClient.CompanySnapshot) throw new InvalidOperationException("The company or account changed during workflow start.");
        receipt.Require(original);
        return receipt;
    }

    public async Task<List<ProcessInstanceSummary>> GetHseIncidentsAsync(string? status = null, string? severity = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = "/api/field/current/process/hse/incidents";
            var queryParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(status))
                queryParts.Add($"status={Uri.EscapeDataString(status)}");
            if (!string.IsNullOrWhiteSpace(severity))
                queryParts.Add($"severity={Uri.EscapeDataString(severity)}");
            if (queryParts.Count > 0)
                endpoint += $"?{string.Join("&", queryParts)}";

            var result = await _apiClient.GetAsync<List<ProcessInstanceSummary>>(endpoint, cancellationToken);
            return result ?? throw new InvalidOperationException("The HSE workflow population was not returned.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow HSE incidents with status {Status} and severity {Severity}.", status, severity);
            throw;
        }
    }

    public async Task<ProcessInstance?> GetHseIncidentAsync(string incidentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(incidentId))
            throw new ArgumentException("Incident ID is required.", nameof(incidentId));

        try
        {
            return await _apiClient.GetAsync<ProcessInstance>(
                $"/api/field/current/process/hse/incident?incidentId={Uri.EscapeDataString(incidentId)}",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow HSE incident {IncidentId}.", incidentId);
            throw;
        }
    }

    public async Task<List<ProcessInstanceSummary>> GetInstancesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<ProcessInstanceSummary>>(
                "/api/field/current/process/instances",
                cancellationToken);

            return result ?? new List<ProcessInstanceSummary>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process instances.");
            throw;
        }
    }

    public async Task<ProcessInstance?> GetInstanceAsync(string instanceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(instanceId))
            throw new ArgumentException("Instance ID is required.", nameof(instanceId));

        try
        {
            return await _apiClient.GetAsync<ProcessInstance>(
                $"/api/field/current/process/instances/{Uri.EscapeDataString(instanceId)}",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process instance {InstanceId}.", instanceId);
            throw;
        }
    }

    public async Task<List<ProcessHistoryEntry>> GetHistoryAsync(string instanceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(instanceId))
            throw new ArgumentException("Instance ID is required.", nameof(instanceId));

        try
        {
            var result = await _apiClient.GetAsync<List<ProcessHistoryEntry>>(
                $"/api/field/current/process/instances/{Uri.EscapeDataString(instanceId)}/history",
                cancellationToken);

            return result ?? new List<ProcessHistoryEntry>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting history for process instance {InstanceId}.", instanceId);
            throw;
        }
    }

    public async Task<ProcessInstance?> SubmitForGateReviewAsync(string gateId, GateReviewSubmitRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(gateId))
            throw new ArgumentException("Gate ID is required.", nameof(gateId));

        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PostAsync<GateReviewSubmitRequest, ProcessInstance>(
                $"/api/field/current/gates/submit/{Uri.EscapeDataString(gateId)}",
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting entity {EntityId} for gate review {GateId}.", request.EntityId, gateId);
            throw;
        }
    }

    public async Task<List<ProcessInstanceSummary>> GetGatesAsync(string? status = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = "/api/field/current/gates";
            if (!string.IsNullOrWhiteSpace(status))
                endpoint += $"?status={Uri.EscapeDataString(status)}";

            var result = await _apiClient.GetAsync<List<ProcessInstanceSummary>>(endpoint, cancellationToken);
            return result ?? new List<ProcessInstanceSummary>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gate reviews with status {Status}.", status);
            throw;
        }
    }

    public async Task<List<ProcessInstanceSummary>> GetPendingGatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<ProcessInstanceSummary>>(
                "/api/field/current/gates/pending",
                cancellationToken);

            return result ?? new List<ProcessInstanceSummary>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending gate reviews.");
            throw;
        }
    }

    public async Task<ProcessInstanceSummary?> GetGateAsync(string gateId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(gateId))
            throw new ArgumentException("Gate ID is required.", nameof(gateId));

        try
        {
            return await _apiClient.GetAsync<ProcessInstanceSummary>(
                $"/api/field/current/gates/{Uri.EscapeDataString(gateId)}",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gate review {GateId}.", gateId);
            throw;
        }
    }

    public async Task<GateChecklistResponse?> GetGateChecklistAsync(string gateId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(gateId))
            throw new ArgumentException("Gate ID is required.", nameof(gateId));

        try
        {
            return await _apiClient.GetAsync<GateChecklistResponse>(
                $"/api/field/current/gates/{Uri.EscapeDataString(gateId)}/checklist",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting checklist for gate review {GateId}.", gateId);
            throw;
        }
    }

    public async Task ApproveGateAsync(string gateId, GateReviewDecisionRequest request, CancellationToken cancellationToken = default)
    {
        await PostGateDecisionAsync(gateId, request, "approve", cancellationToken);
    }

    public async Task RejectGateAsync(string gateId, GateReviewDecisionRequest request, CancellationToken cancellationToken = default)
    {
        await PostGateDecisionAsync(gateId, request, "reject", cancellationToken);
    }

    public async Task DeferGateAsync(string gateId, GateReviewDecisionRequest request, CancellationToken cancellationToken = default)
    {
        await PostGateDecisionAsync(gateId, request, "defer", cancellationToken);
    }

    private async Task PostGateDecisionAsync(string gateId, GateReviewDecisionRequest request, string action, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(gateId))
            throw new ArgumentException("Gate ID is required.", nameof(gateId));

        ArgumentNullException.ThrowIfNull(request);

        try
        {
            await _apiClient.PostAsync(
                $"/api/field/current/gates/{Uri.EscapeDataString(gateId)}/{action}",
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing gate action {Action} for gate {GateId}.", action, gateId);
            throw;
        }
    }

    private static string BuildHseIncidentEndpoint(string incidentId, string? suffix = null)
    {
        var endpoint = "/api/field/current/process/hse/incident";
        if (!string.IsNullOrWhiteSpace(suffix)) endpoint += "/" + suffix;
        return $"{endpoint}?incidentId={Uri.EscapeDataString(incidentId)}";
    }
}

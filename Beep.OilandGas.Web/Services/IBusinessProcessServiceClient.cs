using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Analytics;
using Beep.OilandGas.Models.Data.Process;

namespace Beep.OilandGas.Web.Services;

public interface IBusinessProcessServiceClient : ICompanyMaintenanceHandoffClient, IRecordedProcessClient
{
    Task<TheTechIdea.Data.OilGas.CompanyCommercialAmendmentPage> GetCommercialAmendmentPageAsync(int pageNumber,int pageSize,CancellationToken cancellationToken=default);
    Task<TheTechIdea.Data.OilGas.CompanyCommercialAmendmentQueue> GetCommercialAmendmentQueueAsync(CancellationToken cancellationToken=default);
    Task<TheTechIdea.Data.OilGas.COMMERCIAL_TERMS_INTENT?> GetCommercialAmendmentIntentAsync(string instanceId,CancellationToken cancellationToken=default);
    Task<TheTechIdea.Data.OilGas.COMMERCIAL_TERMS_INTENT> SaveCommercialAmendmentIntentAsync(TheTechIdea.Data.OilGas.CompanyCommercialAmendmentSubmission command,CancellationToken cancellationToken=default);
    Task<TheTechIdea.Data.OilGas.CompanyCommercialHandoffReview> DiscoverCommercialHandoffAsync(TheTechIdea.Data.OilGas.CompanyAccountingProcessStartCatalogQuery query,CancellationToken cancellationToken=default);
    Task<TheTechIdea.Data.OilGas.CompanyNativeDecisionHandoffReview> DiscoverNativeDecisionHandoffAsync(TheTechIdea.Data.OilGas.CompanyAccountingProcessStartCatalogQuery query,CancellationToken cancellationToken=default);
    Task<TheTechIdea.Data.OilGas.CompanyCommercialAmendmentReview> ReviewCommercialAmendmentAsync(string instanceId,CancellationToken cancellationToken=default);
    Task<TheTechIdea.Data.OilGas.SalesContractTermsRecord> SubmitCommercialAmendmentAsync(TheTechIdea.Data.OilGas.CompanyCommercialAmendmentSubmission submission,CancellationToken cancellationToken=default);
    Task<TheTechIdea.Data.OilGas.CompanyCommercialProcessSourceReview> GetCompanyCommercialSourceAsync(string instanceId,CancellationToken cancellationToken=default);
    Task<TheTechIdea.Data.OilGas.CompanyDeliveryProcessSourceReview> GetCompanyDeliverySourceAsync(string instanceId,CancellationToken cancellationToken=default);
    Task<TheTechIdea.Data.OilGas.CompanySalesInvoiceProcessSourceReview> GetCompanySalesInvoiceSourceAsync(string instanceId,CancellationToken cancellationToken=default);
    Task<TheTechIdea.Data.OilGas.CompanySalesReceivableProcessSourceReview> GetCompanySalesReceivableSourceAsync(string instanceId,CancellationToken cancellationToken=default);
    Task<TheTechIdea.Data.OilGas.CompanySalesJournalProcessSourceReview> GetCompanySalesJournalSourceAsync(string instanceId,CancellationToken cancellationToken=default);
    Task<TheTechIdea.Data.OilGas.CompanySalesPaymentProcessSourceReview> GetCompanySalesPaymentSourceAsync(string instanceId,CancellationToken cancellationToken=default);
    Task<TheTechIdea.Data.OilGas.CompanyAccountingStepReceipt> CompleteCompanyAccountingStepAsync(TheTechIdea.Data.OilGas.CompanyAccountingStepCommand command,CancellationToken cancellationToken=default);
    Task<TheTechIdea.Data.OilGas.CompanyAccountingStepReview?> ReviewCompanyAccountingStepAsync(string instanceId,CancellationToken cancellationToken=default);
    Task<TheTechIdea.Data.OilGas.CompanyAccountingProcessStartCatalog> GetCompanyAccountingStartCatalogAsync(TheTechIdea.Data.OilGas.CompanyAccountingProcessStartCatalogQuery query,CancellationToken cancellationToken=default);
    Task<TheTechIdea.Data.OilGas.CompanyAccountingProcessStartReview> ReviewCompanyAccountingStartAsync(TheTechIdea.Data.OilGas.CompanyAccountingProcessStartQuery query,CancellationToken cancellationToken=default);
    Task<TheTechIdea.Data.OilGas.CompanyAccountingProcessStartReceipt> StartCompanyAccountingAsync(TheTechIdea.Data.OilGas.CompanyAccountingProcessStart intent,CancellationToken cancellationToken=default);
    Task<ProcessInstance?> GetCompanyAccountingInstanceAsync(string instanceId,CancellationToken cancellationToken=default);
    Task<List<ProcessDefinition>> GetDefinitionsAsync(CancellationToken cancellationToken = default);
    Task<ProcessDefinition?> GetDefinitionAsync(string processId, CancellationToken cancellationToken = default);
    Task<TheTechIdea.Data.OilGas.ProcessStartSourceReview> ReviewStartSourceAsync(ProcessInstanceRequest request);
    Task<ProcessInstance?> StartInstanceAsync(ProcessInstanceRequest request, CancellationToken cancellationToken = default);
    Task<ProcessInstance?> SubmitComplianceReportAsync(ComplianceReportRequest request, CancellationToken cancellationToken = default);
    Task<TheTechIdea.Data.OilGas.IncidentReportOptions> GetHseReportOptionsAsync(CancellationToken cancellationToken = default);
    Task<TheTechIdea.Data.OilGas.IncidentReportReceipt> RecordHseIncidentAsync(TheTechIdea.Data.OilGas.IncidentReportIntent original, CancellationToken cancellationToken = default);
    Task<TheTechIdea.Data.OilGas.CompanyHseWorkflowReview> ReviewHseWorkflowAsync(string incidentId, CancellationToken cancellationToken = default);
    Task<TheTechIdea.Data.OilGas.IncidentRcaSourceReview> ReviewHseIncidentRcaAsync(string incidentId, CancellationToken cancellationToken = default);
    Task<TheTechIdea.Data.OilGas.IncidentRcaReceipt> RecordHseIncidentCauseAsync(TheTechIdea.Data.OilGas.IncidentRcaIntent original, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TheTechIdea.Data.OilGas.IncidentRcaIntent>> ReadRecordedHseCausesAsync(string incidentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TheTechIdea.Data.OilGas.IncidentRcaRecordedCause>> ReadSharedHseCausesAsync(string incidentId, CancellationToken cancellationToken = default);
    Task<TheTechIdea.Data.OilGas.CompanyHseRcaHandoffReview> ReviewHseRcaHandoffAsync(TheTechIdea.Data.OilGas.IncidentRcaRecordedCause original, CancellationToken cancellationToken = default);
    Task<TheTechIdea.Data.OilGas.CompanyHseRcaHandoffReceipt> CompleteHseRcaHandoffAsync(TheTechIdea.Data.OilGas.CompanyHseRcaHandoffCommand original, CancellationToken cancellationToken = default);
    Task<TheTechIdea.Data.OilGas.CompanyHseRcaRecoveredHandoff?> RecoverHseRcaHandoffAsync(string incidentId, CancellationToken cancellationToken = default);
    Task<TheTechIdea.Data.OilGas.CompanyHseWorkflowReceipt> StartHseWorkflowAsync(TheTechIdea.Data.OilGas.CompanyHseWorkflowStart original, CancellationToken cancellationToken = default);
    Task<List<ProcessInstanceSummary>> GetHseIncidentsAsync(string? status = null, string? severity = null, CancellationToken cancellationToken = default);
    Task<ProcessInstance?> GetHseIncidentAsync(string incidentId, CancellationToken cancellationToken = default);
    Task<List<ProcessInstanceSummary>> GetInstancesAsync(CancellationToken cancellationToken = default);
    Task<List<ProcessInstanceSummary>> GetGatesAsync(string? status = null, CancellationToken cancellationToken = default);
    Task<List<ProcessInstanceSummary>> GetPendingGatesAsync(CancellationToken cancellationToken = default);
    Task<ProcessInstanceSummary?> GetGateAsync(string gateId, CancellationToken cancellationToken = default);
    Task<GateChecklistResponse?> GetGateChecklistAsync(string gateId, CancellationToken cancellationToken = default);
    Task ApproveGateAsync(string gateId, GateReviewDecisionRequest request, CancellationToken cancellationToken = default);
    Task RejectGateAsync(string gateId, GateReviewDecisionRequest request, CancellationToken cancellationToken = default);
    Task DeferGateAsync(string gateId, GateReviewDecisionRequest request, CancellationToken cancellationToken = default);
}

using TheTechIdea.Data.OilGas;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.HSE;

namespace Beep.OilandGas.Web.Services;

public interface IHSEServiceClient
{
    Task<CompanyHseAssignedActions> GetAssignedOverdueActionsAsync(FieldSelectionReview context, CancellationToken cancellationToken = default);
    Task<InvestigatorAssignmentReview> ReviewInvestigatorAssignmentAsync(string incidentId, string expectedFieldId, CancellationToken cancellationToken = default);
    Task AssignInvestigatorAsync(InvestigatorAssignmentReview review, RecordedIncidentParticipant participant, CancellationToken cancellationToken = default);
    Task<List<HSEIncidentRecord>> GetIncidentsAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<HSEIncidentRecord?> GetIncidentAsync(string incidentId, CancellationToken cancellationToken = default);
    Task<List<CauseFinding>> GetIncidentCausesAsync(string incidentId, CancellationToken cancellationToken = default);
    Task<List<CAStatus>> GetIncidentCorrectiveActionsAsync(string incidentId, CancellationToken cancellationToken = default);
    Task<string> AddCorrectiveActionAsync(FieldSelectionReview context, string incidentId, AddCARequest request, string planId, string optionsRevision, CancellationToken cancellationToken = default);
    Task<CorrectiveActionOptions> ReviewCorrectiveActionOptionsAsync(FieldSelectionReview context, string incidentId, CancellationToken cancellationToken = default);
    Task<CACompletionReview> ReviewCompletionAsync(FieldSelectionReview context, string incidentId, int stepSequence, CancellationToken cancellationToken = default);
    Task<CAVerificationReview> ReviewVerificationAsync(FieldSelectionReview context, string incidentId, CancellationToken cancellationToken = default);
    Task<CAVerificationDecision> RecordVerificationAsync(CAVerificationDecision original, CancellationToken cancellationToken = default);
    Task<IncidentClosureReview> ReviewClosureAsync(FieldSelectionReview context, string incidentId, CancellationToken cancellationToken = default);
    Task<CompanyHseClosureReview> ReviewCorrectionReturnAsync(FieldSelectionReview context, string incidentId, CancellationToken cancellationToken = default);
    Task<CompanyHseCorrectionReceipt> ReturnForCorrectionAsync(CompanyHseCorrectionCommand original, CancellationToken cancellationToken = default);
    Task<CompanyHseClosureReview> ReviewClosureStageAsync(FieldSelectionReview context, string incidentId, CancellationToken cancellationToken = default);
    Task<CompanyHseClosureOutcome> CompleteClosureStageAsync(CompanyHseClosureCommand original, CancellationToken cancellationToken = default);
    Task<CompanyHseClosureRecovery?> RecoverClosureStageAsync(FieldSelectionReview context, string incidentId, CancellationToken cancellationToken = default);
    Task<IncidentClosureResolutionReview> ReviewClosureResolutionAsync(FieldSelectionReview context, string incidentId, CancellationToken cancellationToken = default);
    Task<IncidentClosedReceipt> ResolveClosureAsync(IncidentClosureResolution original, CancellationToken cancellationToken = default);
    Task CompleteCorrectiveActionAsync(CACompletionReview review, string notes, CancellationToken cancellationToken = default);
    Task<BarrierSummary?> GetIncidentBarrierSummaryAsync(string incidentId, CancellationToken cancellationToken = default);
    Task<bool> IsIncidentRcaCompleteAsync(string incidentId, CancellationToken cancellationToken = default);
    Task<HAZOPSummary?> GetHazopStudyAsync(string studyId, CancellationToken cancellationToken = default);
    Task<List<HAZOPNode>> GetHazopNodesAsync(string studyId, CancellationToken cancellationToken = default);
    Task<string?> AddHazopNodeAsync(string studyId, AddNodeRequest request, CancellationToken cancellationToken = default);
    Task<string?> AddHazopDeviationAsync(string studyId, int nodeSeq, AddDeviationRequest request, CancellationToken cancellationToken = default);
    Task UpdateHazopDeviationStatusAsync(string studyId, int nodeSeq, int condSeq, string status, CancellationToken cancellationToken = default);
    Task<HSEKPISet?> GetKpisAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
}

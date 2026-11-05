namespace TheTechIdea.Data.OilGas;

// Typed commands for the existing corrective-action owner, independently enforced by ApiService.
public interface ICompanyCorrectiveActionCommands
{
    // Null means two current native reads confirm that no project link has ever been recorded.
    // Missing/retired/ambiguous linked projects are refusals, not absence. Commands still require a plan.
    Task<CAPlanResolution?> ResolveIncidentPlanAsync(FieldSelectionReview review, string incidentId, string actor);
    Task<CAPlanCreationReview> ReviewPlanCreationAsync(FieldSelectionReview review, string incidentId);
    Task<string> CreatePlanAsync(FieldSelectionReview review, string incidentId, CreateCAPlanRequest request, string actor);
    Task<CorrectiveActionOptions> ReviewActionOptionsAsync(FieldSelectionReview review, string incidentId, string projectId, string actor);
    Task<string?> FindRecordedActionAsync(FieldSelectionReview review, string incidentId, string projectId, AddCARequest request, string actor);
    Task<string> AddCorrectiveActionAsync(FieldSelectionReview review, string incidentId, string projectId, AddCARequest request, string actor);
    Task AssignResponsiblePersonAsync(FieldSelectionReview review, string incidentId, string projectId, int stepSequence, string businessAssociateId, string actor);
    Task SetDueDateAsync(FieldSelectionReview review, string incidentId, string projectId, int stepSequence, DateTime dueDate, string actor);
    Task RecordCompletionAsync(FieldSelectionReview review, string incidentId, string projectId, int stepSequence, string notes, string actor);
    Task<CACompletionReview> ReviewCompletionAsync(FieldSelectionReview review, string incidentId, string projectId, int stepSequence, string actor);
    Task<CAVerificationReview> ReviewVerificationAsync(FieldSelectionReview review, string incidentId, string projectId, string actor);
    Task<CAVerificationDecision> RecordVerificationAsync(CAVerificationDecision original);
    Task<IncidentClosureReview> ReviewClosureAsync(FieldSelectionReview context, string incidentId, string projectId, string actor);
    Task<string> ReviewNativeClosureRevisionAsync(FieldSelectionReview context, string incidentId, string projectId);
    Task<IncidentClosedReceipt> RecordIncidentClosedAsync(CompanyHseClosureCommand original, CompanyHseClosureReceipt lifecycle, Func<Task> confirmLifecycle);
    Task<IncidentClosureResolutionReview> ReviewClosureResolutionAsync(FieldSelectionReview context, CompanyHseClosureCommand original, CompanyHseClosureReceipt lifecycle);
    Task<IncidentClosedReceipt> ResolveIncidentClosureAsync(CompanyHseClosureCommand original, CompanyHseClosureReceipt lifecycle, IncidentClosureResolution resolution, Func<Task> confirmLifecycle);
    Task RecordReviewedCompletionAsync(CACompletionReview review, string notes, string actor);
}

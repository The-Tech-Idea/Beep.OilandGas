namespace TheTechIdea.Data.OilGas;

// Typed mutations for the existing incident owner; ApiService independently enforces app/case authority.
public interface ICompanyIncidentCommands
{
    Task<IncidentReportOptions> ReviewReportOptionsAsync(FieldSelectionReview review);
    Task<string> CreateReportAsync(IncidentReportIntent intent);
    Task<string> ReadReportedJurisdictionAsync(FieldSelectionReview review, string incidentId);
    Task<string> ReviewWorkflowSourceRevisionAsync(FieldSelectionReview review, string incidentId);
    Task<IncidentRcaSourceReview> ReviewRcaSourceAsync(FieldSelectionReview review, string incidentId);
    Task<IncidentRcaReceipt> RecordRcaAsync(IncidentRcaIntent original);
    Task<IReadOnlyList<IncidentRcaIntent>> ReadRecordedRcaAsync(FieldSelectionReview review, string incidentId);
    Task<IReadOnlyList<IncidentRcaRecordedCause>> ReadSharedRcaAsync(FieldSelectionReview review, string incidentId);
    Task<InvestigatorAssignmentReview> ReviewInvestigatorAssignmentAsync(FieldSelectionReview review, string incidentId);
    Task AssignInvestigatorAsync(FieldSelectionReview review, string incidentId, AssignIncidentInvestigatorRequest request, string actor);
}

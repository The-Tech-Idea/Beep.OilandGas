using System.Collections.Generic;
using TheTechIdea.Data.OilGas;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Drilling;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Service interface for drilling and construction workflow operations.
    /// </summary>
    public interface IDrillingServiceClient
    {
        Task<DrillingCompletionReviewStartReceipt> SubmitCompletionReviewAsync(DrillingCompletionReviewStart original,CancellationToken cancellationToken=default);
        Task<DrillingCompletionReviewStartReceipt> RecoverCompletionReviewAsync(DrillingCompletionReviewStart original,CancellationToken cancellationToken=default);
    Task<DrillingReportingClosureStatus> GetReportingResponsibilityAsync(string assignmentId,CancellationToken cancellationToken=default);
    Task<DrillingReportingReassignmentCatalog> GetReportingReassignmentCatalogAsync(string assignmentId,CancellationToken cancellationToken=default);
    Task<DrillingReportingReassignmentReview> ReviewReportingReassignmentAsync(DrillingReportingReassignmentSelection selection,CancellationToken cancellationToken=default);
    Task<DrillingReportingReassignmentReceipt> ReassignReportingAsync(DrillingReportingReassignment original,CancellationToken cancellationToken=default);
    Task<DrillingReportingReassignmentReceipt> RecoverReportingReassignmentAsync(DrillingReportingReassignment original,CancellationToken cancellationToken=default);
    Task<DrillingReportingClosureReview> ReviewReportingClosureAsync(string assignmentId,string completionId,CancellationToken cancellationToken=default);
    Task<DrillingReportingClosureReceipt> CloseReportingAsync(DrillingReportingClosure original,CancellationToken cancellationToken=default);
    Task<DrillingReportingClosureReceipt> RecoverReportingClosureAsync(DrillingReportingClosure original,CancellationToken cancellationToken=default);
        Task<DrillingCompletionReviewProgress> GetCompletionReviewProgressAsync(string processId,CancellationToken cancellationToken=default);
    Task<DrillingCompletionReviewDecisionReceipt> DecideCompletionReviewAsync(DrillingCompletionReviewDecision original,CancellationToken cancellationToken=default);
    Task<DrillingCompletionReviewDecisionReceipt> RecoverCompletionReviewDecisionAsync(DrillingCompletionReviewDecision original,CancellationToken cancellationToken=default);
    Task<DrillingCompletionAssignedReview> GetAssignedCompletionReviewAsync(string processId,CancellationToken cancellationToken=default);
    Task<DrillingCompletionTestInspection> InspectCompletionTestAsync(DrillingCompletionTestSelection selection,CancellationToken cancellationToken=default);
        Task<DrillingCompletionReviewReadiness> GetCompletionRoutingAsync(string reviewId,CancellationToken cancellationToken=default);
        Task<DrillingExecutionConflictReview> GetExecutionConflictAsync(DrillingExecutionStart original,CancellationToken cancellationToken=default);
        Task<DrillingExecutionConflictReview> ConfirmExecutionConflictAsync(DrillingExecutionConflictAcknowledgement original,CancellationToken cancellationToken=default);
        Task<DrillingExecutionAssignmentConflictReview> GetExecutionAssignmentConflictAsync(DrillingExecutionAssignment original,CancellationToken cancellationToken=default);
        Task<DrillingExecutionAssignmentConflictReview> ConfirmExecutionAssignmentConflictAsync(DrillingExecutionAssignmentConflictAcknowledgement original,CancellationToken cancellationToken=default);
        Task<DrillingExecutionAssignmentReview> GetExecutionAssignmentAsync(string reviewId,CancellationToken cancellationToken=default);
        Task<DrillingExecutionAssignmentReceipt> AssignExecutionAsync(DrillingExecutionAssignment original,CancellationToken cancellationToken=default);
        Task<DrillingExecutionAssignmentReceipt> RecoverExecutionAssignmentAsync(DrillingExecutionAssignment original,CancellationToken cancellationToken=default);
        Task<DrillingExecutionAssignedWork> GetAssignedExecutionAsync(string processId,CancellationToken cancellationToken=default);
        Task<DrillingDailyReportConflictReview> GetDailyReportConflictAsync(DrillingDailyReportInput original,CancellationToken cancellationToken=default);
        Task<DrillingDailyReportConflictReview> ConfirmDailyReportConflictAsync(DrillingDailyReportConflictAcknowledgement original,CancellationToken cancellationToken=default);
        Task<DrillingDailyReportPage> GetDailyReportPageAsync(string reviewId,int after=0,int size=DrillingDailyReportPage.DefaultPageSize,string? historyRevision=null,CancellationToken cancellationToken=default);
        Task<DrillingDailyReportBasis> GetDailyReportBasisAsync(string reviewId,CancellationToken cancellationToken=default);
        Task<DrillingDailyReportReceipt> RecordDailyReportAsync(DrillingDailyReportInput original,CancellationToken cancellationToken=default);
        Task<DrillingDailyReportReceipt> RecoverDailyReportAsync(DrillingDailyReportInput original,CancellationToken cancellationToken=default);
        Task<DrillingProgramReviewDecisionReceipt> DecideProgramReviewAsync(DrillingProgramReviewDecision original,CancellationToken cancellationToken=default);
        Task<DrillingProgramReviewDecisionReceipt> RecoverProgramReviewDecisionAsync(DrillingProgramReviewDecision original,CancellationToken cancellationToken=default);
        Task<DrillingExecutionBasis> GetExecutionBasisAsync(string reviewId,CancellationToken cancellationToken=default);
        Task<DrillingExecutionStatus> GetExecutionStatusAsync(string reviewId,CancellationToken cancellationToken=default);
        Task<DrillingExecutionStartReceipt> StartExecutionAsync(DrillingExecutionStart original,CancellationToken cancellationToken=default);
        Task<DrillingExecutionStartReceipt> RecoverExecutionStartAsync(DrillingExecutionStart original,CancellationToken cancellationToken=default);
        Task<DrillingProgramReviewProgress> GetProgramReviewProgressAsync(string processId,CancellationToken cancellationToken=default);
        Task<DrillingProgramAssignedReview> GetAssignedProgramReviewAsync(string processId,CancellationToken cancellationToken=default);
        Task<DrillingProgramReviewStartReceipt> SubmitProgramReviewAsync(DrillingProgramReviewStart original,CancellationToken cancellationToken=default);
        Task<DrillingProgramReviewStartReceipt> RecoverProgramReviewAsync(DrillingProgramReviewStart original,CancellationToken cancellationToken=default);
        Task<DrillingProgramReviewReadiness> ReviewProgramRoutingAsync(string processId,string afeId,string wellUwi,string programId,string programRevision,CancellationToken cancellationToken=default);
        Task<DrillingProgramDraftList> GetProgramDraftsAsync(string processId,string afeId,string wellUwi,CancellationToken cancellationToken=default);
        Task<DrillingProgramBasis> ReviewProgramBasisAsync(string processId,string afeId,string wellUwi,CancellationToken cancellationToken=default);
        Task<DrillingProgramDraftReceipt> RecordProgramDraftAsync(CreateDrillingProgramDraft original,CancellationToken cancellationToken=default);
        Task<DrillingProgramDraftReceipt> RecoverProgramDraftAsync(CreateDrillingProgramDraft original,CancellationToken cancellationToken=default);

    }
}

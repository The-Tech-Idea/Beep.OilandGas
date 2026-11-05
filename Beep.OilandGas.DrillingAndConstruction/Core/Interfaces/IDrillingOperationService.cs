using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.DrillingAndConstruction.Core.Interfaces;
public interface IDrillingOperationService
{
    Task<DrillingCompletionTestInspection> InspectCompletionTestAsync(DrillingCompletionTestSelection selection,Func<Task> authority,Func<Task> confirmSource);
    Task<DrillingAcceptedCompletionTests> ReadAcceptedCompletionTestsAsync(DrillingCompletionReviewProgress acceptance,Func<Task> authority,Func<Task> confirmSource);
    Task<DrillingExecutionConflictReview> ReadExecutionConflictAsync(DrillingExecutionStart original,Func<Task> authority,Func<Task<DrillingExecutionBasis>> readBasis);
    Task<DrillingDailyReportHistory> ReadDailyReportsAsync(DrillingExecutionStatus execution,Func<Task> authority);
    Task<DrillingDailyReportPage> ReadDailyReportPageAsync(DrillingExecutionStatus execution,int after,int size,string? expectedHistory,Func<Task> authority,Func<Task> confirmSource);
    Task<DrillingDailyReportPage> ReadDailyReportEntryAsync(DrillingExecutionStatus execution,DrillingDailyReportHead entry,Func<Task> authority,Func<Task> confirmSource);
    Task<DrillingDailyReportReceipt> RecordDailyReportAsync(DrillingDailyReportInput input,Func<Task> authority,Func<Task<DrillingExecutionStatus>> readExecution,bool recover=false);
    Task<DrillingExecutionStartReceipt> RecordExecutionStartAsync(DrillingExecutionStart input,Func<Task> authority,Func<Task<DrillingExecutionBasis>> readBasis,bool recover=false);
    Task<DrillingExecutionStatus> ReadExecutionStatusAsync(DrillingExecutionBasis basis,Func<Task> authority);
    Task<DrillingProgramWellFacts> ReadProgramWellAsync(AssetDatabaseScope core,string field,string uwi,bool activeRequired,Func<Task> authority);
    Task<DrillingProgramDraftReceipt> RecordProgramDraftAsync(CreateDrillingProgramDraft input,Func<Task> authority,Func<Task<DrillingProgramBasis>> readBasis,bool recover=false);
    Task<DrillingProgramDraftList> ReadProgramDraftsAsync(DrillingProgramBasis basis,Func<Task> authority);
}

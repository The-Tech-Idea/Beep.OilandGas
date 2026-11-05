using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Tests;

namespace Beep.OilandGas.Web.Auth.Tests;

/// <summary>Controlled native-approved data for the isolated browser transport harness.</summary>
public static class DrillingWorkflowBrowserFixture
{
    public static async Task<DrillingReportingReassignment> CreateReportingReassignmentAsync()
    {
        var fixture=new DrillingReportingReassignmentTests.Fixture();await fixture.Prepare();
        var result=new DrillingReportingReassignment(await fixture.Review(),Guid.NewGuid(),DateTimeOffset.UtcNow,"Controlled transfer to the configured replacement reporting team",true);result.Require();return CompanyInventorySourceReview.Copy(result);
    }
    public static async Task<DrillingReportingClosure> CreateReportingClosureAsync()
    {
        var fixture=new DrillingReportingClosureNativeTests.Fixture();await fixture.Prepare();return CompanyInventorySourceReview.Copy(fixture.Command);
    }
    public static DrillingCompletionReviewReadiness CompletionRouting(DrillingDailyReportHistory history,string mode)
        =>DrillingCompletionRoutingTests.Review(history,mode);
    public static IReadOnlyList<DRILLING_DAILY_REPORT> ReportHistory(DrillingDailyReportInput seed,int count)
        =>DrillingDailyReportPageNativeTests.Series(seed,count);
    public static async Task<DrillingDailyReportInput> CreateAsync()
    {
        using var fixture = new DrillingDailyReportNativeTests.Fixture();
        await fixture.Prepare();
        var original = CompanyInventorySourceReview.Copy(fixture.Command); original.Require(); return original;
    }
    public static async Task<DrillingExecutionAssignmentReview> CreateAssignmentAsync()
    {
        var fixture=new DrillingExecutionAssignmentNativeTests.Fixture();await fixture.Prepare();var original=fixture.Original;
        var result=new DrillingExecutionAssignmentReview(original.Source,[original.Workflow],[original.SenderRole],original.SenderPersonaCode,
            original.SenderAuthorityRevision,null,"",DateTimeOffset.UtcNow);
        result=result with{Revision=result.ContentRevision()};var scope=original.Scope();result.Require(scope.Company,scope.ActorUserId,original.Source.Basis.ReviewInstanceId);
        return CompanyInventorySourceReview.Copy(result);
    }
    public static DrillingExecutionAssignmentReceipt AssignmentReceipt(DrillingExecutionAssignment original,bool recorded)
        =>DrillingExecutionAssignmentStateTests.Receipt(original,recorded);
    public static DrillingExecutionAssignedWork Assigned(DrillingExecutionAssignment original)
        =>DrillingExecutionAssignedStateTests.Packet(CompanyInventorySourceReview.Copy(original));
    public static DrillingDailyReportPage AssignedProgress(DrillingExecutionAssignment original)
        =>DrillingAssignedCompletionStateTests.Progress(Assigned(original));
}
#endif

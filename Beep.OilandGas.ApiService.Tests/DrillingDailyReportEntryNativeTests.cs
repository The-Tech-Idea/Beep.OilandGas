#if WORKFLOW_INBOX_FOCUSED
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class DrillingDailyReportEntryNativeTests
{
    [Theory,InlineData("current"),InlineData("id"),InlineData("revision"),InlineData("period"),InlineData("number")]
    public async Task ExactImmutableEntryUsesCompleteCurrentHistoryAndNeverWeakensPublicCursorAdmission(string fault)
    {
        using var f=new DrillingDailyReportNativeTests.Fixture();var rows=await DrillingDailyReportCorrectionNativeTests.Seed(f);f.Command=DrillingCompletionReportNativeTests.Complete(f,rows[2]);await f.Save();
        var history=await f.Store.Service.ReadDailyReportsAsync(f.Execution,f.Guard);var entry=history.LatestObservedReport!;var writes=f.Store.Store.Native.Writes;
        entry=fault switch{"id"=>entry with{ReportId="foreign"},"revision"=>entry with{NativeRevision=new('F',64)},"period"=>entry with{LatestPeriodEndUtc=entry.LatestPeriodEndUtc.AddTicks(1)},"number"=>entry with{Number=0},_=>entry};
        var read=()=>f.Store.Service.ReadDailyReportEntryAsync(f.Execution,entry,f.Guard,()=>Task.CompletedTask);
        if(fault=="current"){var found=await read();Assert.Single(found.Reports);Assert.Equal(history.Revision,found.History.Revision);Assert.Equal(entry.ReportId,found.Reports[0].DDR_ID);}else await Assert.ThrowsAnyAsync<Exception>(read);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Store.Service.ReadDailyReportPageAsync(f.Execution,3,1,null,f.Guard,()=>Task.CompletedTask));Assert.Equal(writes,f.Store.Store.Native.Writes);
    }
}
#endif

#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Moq;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class DrillingExecutionConflictNativeTests
{
    [Theory,InlineData("key"),InlineData("intent"),InlineData("program")]
    public async Task ProviderFailedFlagCannotBecomeConfirmedAbsenceOrAQualifiedCompetingRecord(string query)
    {
        using var f=new DrillingExecutionStartNativeTests.Fixture();await f.Prepare();await f.Save();var original=f.Command with{IntentId=Guid.NewGuid()};
        var native=f.Store.Store.Native;var error=new TheTechIdea.Beep.ConfigUtil.ErrorsInfo{Flag=TheTechIdea.Beep.ConfigUtil.Errors.Ok};
        native.Source.SetupGet(s=>s.ErrorObject).Returns(error);
        native.Source.Setup(s=>s.GetEntityAsync("DRILLING_OPERATION",It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>
        {
            var selected=query=="key"?filters.Any(filter=>filter.FieldName=="DRILLING_OPERATION_ID"):
                query=="intent"?filters.Any(filter=>filter.FieldName=="PPDM_GUID"):filters.Any(filter=>filter.FieldName=="PROGRAM_ID");
            error.Flag=selected?TheTechIdea.Beep.ConfigUtil.Errors.Failed:TheTechIdea.Beep.ConfigUtil.Errors.Ok;
            return native.Rows.Where(row=>row.GetType().Name==table && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(row=>JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray();
        });
        var refused=await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Store.Service.ReadExecutionConflictAsync(original,f.Guard,f.Read));
        Assert.Equal("The Beep provider reported a failed repository operation.",refused.Message);Assert.Equal(1,native.Writes);
    }
    [Fact]public async Task DifferentRecordedStartAndExactOriginalAbsenceAreReadOnlyAndDefensivelyCopied()
    {
        using var f=new DrillingExecutionStartNativeTests.Fixture();await f.Prepare();await f.Save();
        var original=f.Command with{IntentId=Guid.NewGuid()};var before=JsonSerializer.Serialize(f.Store.Store.Native.Rows);
        var result=await f.Store.Service.ReadExecutionConflictAsync(original,f.Guard,f.Read);result.Require(original);
        Assert.True(result.OriginalConfirmedAbsent);Assert.Equal(f.Command.OperationId(),result.Competing.DRILLING_OPERATION_ID);
        Assert.Equal(original.OperationId(),result.OriginalOperationId);Assert.NotEqual(result.OriginalOperationId,result.Competing.DRILLING_OPERATION_ID);
        Assert.Equal(before,JsonSerializer.Serialize(f.Store.Store.Native.Rows));Assert.Equal(1,f.Store.Store.Native.Writes);
        result.Competing.STATUS="Consumer mutation";
        Assert.Equal("STARTED",(await f.Store.Service.ReadExecutionConflictAsync(original,f.Guard,f.Read)).Competing.STATUS);
    }
    [Fact]public async Task DifferentOriginalRecorderCanBeInspectedUnderCurrentOriginalSenderReadOnlySource()
    {
        using var f=new DrillingExecutionStartNativeTests.Fixture();await f.Prepare();await f.Save();
        var source=DrillingProgramAssignedReviewTests.AsReader(f.Current.Source,"other-original-sender");
        f.Current=f.Current with{Source=source};f.Current=f.Current with{Revision=f.Current.ContentRevision()};
        var original=f.Command with{Basis=f.Current,IntentId=Guid.NewGuid()};f.ReadOnly();
        var result=await f.Store.Service.ReadExecutionConflictAsync(original,f.Guard,f.Read);result.Require(original);
        Assert.Equal("execution-recorder",result.Competing.ROW_CREATED_BY);Assert.Equal("other-original-sender",original.Basis.Scope().ActorUserId);
        Assert.Equal(1,f.Store.Store.Native.Writes);
    }
    [Theory,InlineData(false),InlineData(true)]public async Task MissingCompetitorOrAlreadyRecordedOriginalCannotBecomeClearableConflict(bool recorded)
    {
        using var f=new DrillingExecutionStartNativeTests.Fixture();await f.Prepare();if(recorded)await f.Save();
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Store.Service.ReadExecutionConflictAsync(f.Command,f.Guard,f.Read));
        Assert.Equal(recorded?1:0,f.Store.Store.Native.Writes);
    }
    [Theory,InlineData("key"),InlineData("intent"),InlineData("duplicate"),InlineData("corrupt"),InlineData("unavailable")]
    public async Task AnyOriginalEvidenceOrAmbiguousOrUnavailablePopulationRefusesClearableProof(string fault)
    {
        using var f=new DrillingExecutionStartNativeTests.Fixture();await f.Prepare();await f.Save();var original=f.Command with{IntentId=Guid.NewGuid()};
        var row=f.Store.Store.Native.Rows.OfType<DRILLING_OPERATION>().Single();
        if(fault is "key" or "intent" or "duplicate")
        {
            var copy=CompanyInventorySourceReview.Copy(row);
            if(fault=="key"){copy.DRILLING_OPERATION_ID=original.OperationId();copy.PROGRAM_ID="another-program";}
            if(fault=="intent"){copy.DRILLING_OPERATION_ID="another-operation";copy.PROGRAM_ID="another-program";copy.PPDM_GUID=original.IntentId.ToString("D");}
            f.Store.Store.Native.Rows.Add(copy);
        }
        if(fault=="corrupt")row.STATUS="UNQUALIFIED";if(fault=="unavailable")f.NullRows=true;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Store.Service.ReadExecutionConflictAsync(original,f.Guard,f.Read));
        Assert.Equal(1,f.Store.Store.Native.Writes);
    }
    [Theory,InlineData("borrowed"),InlineData("isolation"),InlineData("schema"),InlineData("revoked")]
    public async Task UnsupportedTransactionSchemaOrRevokedAuthorityCannotConfirmOriginalAbsence(string fault)
    {
        using var f=new DrillingExecutionStartNativeTests.Fixture();await f.Prepare();await f.Save();var original=f.Command with{IntentId=Guid.NewGuid()};
        if(fault=="borrowed")f.Store.Store.Native.Borrowed=true;if(fault=="isolation")f.Store.Store.Native.Isolation=IsolationLevel.ReadCommitted;
        if(fault=="schema")f.Schema=s=>s.Fields.RemoveAll(field=>field.FieldName=="OWNER_TENANT_ID");if(fault=="revoked")f.Revoked=true;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Store.Service.ReadExecutionConflictAsync(original,f.Guard,f.Read));
        Assert.Equal(1,f.Store.Store.Native.Writes);
    }
    [Fact]public async Task ClosingSourceReadCannotHideChangedCompetingEvidence()
    {
        using var f=new DrillingExecutionStartNativeTests.Fixture();await f.Prepare();await f.Save();var original=f.Command with{IntentId=Guid.NewGuid()};var reads=0;
        Task<DrillingExecutionBasis> Read(){if(++reads==2)f.Store.Store.Native.Rows.OfType<DRILLING_OPERATION>().Single().RECORDER_ROLE="Unqualified";return f.Read();}
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Store.Service.ReadExecutionConflictAsync(original,f.Guard,Read));Assert.Equal(1,f.Store.Store.Native.Writes);
    }
}
#endif

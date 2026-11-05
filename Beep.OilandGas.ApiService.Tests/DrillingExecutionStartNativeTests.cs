using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.PPDM39.Core;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class DrillingExecutionStartNativeTests
{
    internal sealed class Fixture:IDisposable
    {
        internal DrillingProgramReviewDecisionNativeTests.Fixture Approval=new();internal DrillingExecutionBasis Current=null!;internal DrillingExecutionStart Command=null!;internal bool Revoked,FailInsert,NullRows,Corrupt;internal Action<EntityStructure>? Schema;internal Action? OnInsert;
        internal DrillingProgramDraftNativeTests.Fixture Store=>Approval.Start.Program;
        internal async Task Prepare()
        {
            await Approval.Prepare(2);await Approval.Decide();Approval.Command=await Approval.Create();await Approval.Decide();var graph=await Approval.Start.Native.Service.ReadRecordedProgramReviewAsync(Approval.Start.Original.Scope().Company,Approval.Start.Original.InstanceId(),Approval.Guard);var source=DrillingProgramAssignedReviewTests.AsReader(Approval.Current,"execution-recorder");var facts=PPDMProcessService.ProgramReviewProgress(graph.Original,graph.Process,graph.Tasks,source,graph.NativeRevision);Current=new(source,facts.InstanceId,facts.Original.Revision(),facts.NativeRevision,facts.Original.Scope().ActorUserId,facts.Stages,["ConfiguredExecutionRecorder"],"DRILLING_RECORDER",new('A',64),"",DateTimeOffset.UtcNow);Current=Current with{Revision=Current.ContentRevision()};Current.Require();Command=new(Current,Guid.NewGuid(),"ConfiguredExecutionRecorder",DateTimeOffset.UtcNow,"Actual observed start reported by the configured company recorder");Command.Require();
            var native=Store.Store.Native;native.Source.Setup(source=>source.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure requested,bool refresh)=>{var value=new ClassCreator(native.Editor.Object).ConvertToEntityStructure(typeof(DRILLING_OPERATION));Schema?.Invoke(value);return value;});native.Source.Setup(source=>source.GetEntityAsync(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>NullRows?null!:native.Rows.Where(row=>row.GetType().Name==table && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(row=>JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray());native.Source.Setup(source=>source.InsertEntity("DRILLING_OPERATION",It.IsAny<object>())).Returns((string table,object value)=>{native.Writes++;if(FailInsert)return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((DRILLING_OPERATION)value);if(Corrupt)row.STATUS="COMPLETED";native.Rows.Add(row);OnInsert?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
        }
        internal Task Guard(){if(Revoked)throw new UnauthorizedAccessException("Current original execution authority changed");return Task.CompletedTask;}
        internal Task<DrillingExecutionBasis> Read(){Assert.DoesNotContain("begin",Store.Store.Native.Transactions.TakeLast(1));return Task.FromResult(Current);}
        internal Task<DrillingExecutionStartReceipt> Save(bool recover=false)=>Store.Service.RecordExecutionStartAsync(Command,Guard,Read,recover);
        internal void ReadOnly(){Current=Current with{RecorderRoles=[],PersonaCode=null,RecorderAuthorityRevision=null};Current=Current with{Revision=Current.ContentRevision()};Current.Require();}
        public void Dispose()=>Approval.Dispose();
    }
    [Fact]public async Task ActualApprovedOriginalStartCommitsOnceAndReadOnlyRetryRecoveryNeverWritesAgain()
    {
        using var f=new Fixture();await f.Prepare();var approvalRows=JsonSerializer.Serialize(f.Approval.Start.Native.Native.Rows);Assert.Null((await f.Save(true)).Recorded);var receipt=await f.Save();receipt.Require(f.Command);var row=Assert.IsType<DRILLING_OPERATION>(receipt.Recorded);Assert.Equal("STARTED",row.STATUS);Assert.Equal(f.Command.SpudAtUtc.UtcDateTime,row.SPUD_DATE);Assert.Equal("execution-recorder",row.ROW_CREATED_BY);Assert.Equal(f.Current.Source.Program.PROGRAM_ID,row.PROGRAM_ID);Assert.Equal("ConfiguredExecutionRecorder",row.RECORDER_ROLE);Assert.Equal(1,f.Store.Store.Native.Writes);Assert.NotNull((await f.Save()).Recorded);f.ReadOnly();Assert.NotNull((await f.Save(true)).Recorded);Assert.Equal(1,f.Store.Store.Native.Writes);Assert.Equal(approvalRows,JsonSerializer.Serialize(f.Approval.Start.Native.Native.Rows));Assert.Equal(0,f.Approval.Start.Program.Investment.Handoff.Native.Native.Rows.OfType<Beep.OilandGas.LifeCycle.Data.Tables.CROSS_PERSONA_TASK>().Count(row=>row.ENTITY_TYPE=="DRILLING_OPERATION"));
        receipt.Recorded!.STATUS="Consumer mutation";Assert.Equal("STARTED",(await f.Save(true)).Recorded!.STATUS);
    }
    [Theory][InlineData("DRILLING_OPERATION")][InlineData("alias")]
    public async Task GenericCrudCannotBypassTheOriginalExecutionOwner(string table)
    {
        using var f=new Fixture();await f.Prepare();var repository=new PPDMGenericRepository(f.Store.Store.Native.Editor.Object,Moq.Mock.Of<ICommonColumnHandler>(),Moq.Mock.Of<Beep.OilandGas.PPDM39.Core.IPPDM39DefaultsRepository>(),Moq.Mock.Of<Beep.OilandGas.PPDM39.Core.Metadata.IPPDMMetadataRepository>(),typeof(DRILLING_OPERATION),f.Current.Source.Basis.ExecutionDatabase.ConnectionName,table);await Assert.ThrowsAnyAsync<Exception>(()=>repository.InsertAsync(f.Command.ToRecord(DateTime.UtcNow),"execution-recorder"));await Assert.ThrowsAnyAsync<Exception>(()=>repository.UpdateAsync(f.Command.ToRecord(DateTime.UtcNow),"execution-recorder"));Assert.Equal(0,f.Store.Store.Native.Writes);
    }
    [Fact]public async Task StatusUsesCurrentReaderAndOriginalRecorderWithoutWriteOrGuessedExecution()
    {
        using var f=new Fixture();await f.Prepare();Assert.Null((await f.Store.Service.ReadExecutionStatusAsync(f.Current,f.Guard)).Recorded);await f.Save();var source=DrillingProgramAssignedReviewTests.AsReader(f.Current.Source,"another-reader");f.Current=f.Current with{Source=source,RecorderRoles=[],PersonaCode=null,RecorderAuthorityRevision=null};f.Current=f.Current with{Revision=f.Current.ContentRevision()};var result=await f.Store.Service.ReadExecutionStatusAsync(f.Current,f.Guard);result.Require();Assert.Equal("another-reader",result.Basis.Scope().ActorUserId);Assert.Equal("execution-recorder",result.Recorded!.ROW_CREATED_BY);Assert.Equal(1,f.Store.Store.Native.Writes);
    }
    [Theory][InlineData("key")][InlineData("owner")][InlineData("proof-width")][InlineData("missing")][InlineData("isolation")][InlineData("borrowed")]
    public async Task UnqualifiedSchemaOrTransactionCannotWriteExecution(string fault)
    {
        using var f=new Fixture();await f.Prepare();f.Schema=s=>{if(fault=="key")s.Fields.ForEach(row=>row.IsKey=false);if(fault=="owner")s.Fields.Single(row=>row.FieldName=="OWNER_TENANT_ID").AllowDBNull=true;if(fault=="proof-width"){var field=s.Fields.Single(row=>row.FieldName=="EXECUTION_START_JSON");field.Size=1;field.Size1=1;}if(fault=="missing")s.Fields.RemoveAll(row=>row.FieldName=="PROGRAM_REVIEW_NATIVE_REVISION");};if(fault=="isolation")f.Store.Store.Native.Isolation=IsolationLevel.ReadCommitted;if(fault=="borrowed")f.Store.Store.Native.Borrowed=true;await Assert.ThrowsAnyAsync<Exception>(()=>f.Save());Assert.Empty(f.Store.Store.Native.Rows);Assert.Equal(0,f.Store.Store.Native.Writes);
    }
    [Theory][InlineData("insert")][InlineData("corrupt")][InlineData("revoked")]
    public async Task FailedNativeStartRollsBackRatherThanAcknowledgingExecution(string fault)
    {
        using var f=new Fixture();await f.Prepare();f.FailInsert=fault=="insert";f.Corrupt=fault=="corrupt";f.OnInsert=()=>{if(fault=="revoked")f.Revoked=true;};await Assert.ThrowsAnyAsync<Exception>(()=>f.Save());Assert.Empty(f.Store.Store.Native.Rows);Assert.Equal("rollback",f.Store.Store.Native.Transactions.Last());
    }
    [Fact]public async Task LostCommitRecoversExactOriginalWithoutAnotherWriteAndNewIntentCannotDuplicateTheProgram()
    {
        using var f=new Fixture();await f.Prepare();f.Store.Store.Native.FailCommit=true;await Assert.ThrowsAnyAsync<Exception>(()=>f.Save());f.Store.Store.Native.FailCommit=false;f.ReadOnly();Assert.NotNull((await f.Save(true)).Recorded);Assert.Equal(1,f.Store.Store.Native.Writes);f.Command=f.Command with{IntentId=Guid.NewGuid()};await Assert.ThrowsAnyAsync<Exception>(()=>f.Save(true));Assert.Equal(1,f.Store.Store.Native.Writes);
    }
    [Theory][InlineData("actual-offset")][InlineData("before-approval")][InlineData("role")][InlineData("rationale")][InlineData("future")]
    public async Task ExplicitActualUtcRoleAndRationaleCannotBeReplacedByPlannedOrGuessedInputs(string fault)
    {
        using var f=new Fixture();await f.Prepare();f.Command=fault switch{"actual-offset"=>f.Command with{SpudAtUtc=f.Command.SpudAtUtc.ToOffset(TimeSpan.FromHours(3))},"before-approval"=>f.Command with{SpudAtUtc=f.Current.Approvals[^1].AtUtc!.Value.AddSeconds(-1)},"role"=>f.Command with{RecorderRole="OtherRole"},"rationale"=>f.Command with{Rationale=" "},_=>f.Command with{SpudAtUtc=DateTimeOffset.UtcNow.AddDays(1)}};await Assert.ThrowsAnyAsync<Exception>(()=>f.Save());Assert.Empty(f.Store.Store.Native.Rows);Assert.Equal(0,f.Store.Store.Native.Writes);
    }
    [Theory][InlineData("pending")][InlineData("rejected")][InlineData("approval-audit")][InlineData("owner")][InlineData("unavailable")]
    public async Task MissingApprovalChangedSourceOrUnavailableRowsCannotBecomeExecution(string fault)
    {
        using var f=new Fixture();await f.Prepare();if(fault=="unavailable")f.NullRows=true;else if(fault=="owner"){var source=DrillingProgramAssignedReviewTests.AsReader(f.Current.Source,"other-recorder");f.Current=f.Current with{Source=source};}else{var approvals=f.Current.Approvals.ToArray();approvals[0]=fault=="approval-audit"?approvals[0] with{HistoryId="forged"}:approvals[0] with{State=fault=="pending"?"PENDING":"REJECTED"};f.Current=f.Current with{Approvals=approvals};}f.Current=f.Current with{Revision=f.Current.ContentRevision()};await Assert.ThrowsAnyAsync<Exception>(()=>f.Save());Assert.Equal(0,f.Store.Store.Native.Writes);
    }
}
#endif

using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class DrillingProgramAssignedReviewTests
{
    internal static DrillingProgramReviewSource AsReader(DrillingProgramReviewSource original,string actor)
    {
        var status=original.Basis.Funding;var budget=status.Source.Current;var progress=budget.Source.Review with{Scope=budget.Source.Review.Scope with{ActorUserId=actor}};progress=progress with{Revision=progress.ContentRevision()};budget=budget with{Source=budget.Source with{Review=progress},Books=budget.Books.AsReader(actor)};budget=budget with{Revision=budget.ContentRevision()};
        status=status with{Source=status.Source with{Current=budget}};status=status with{Revision=status.ContentRevision()};var basis=original.Basis with{Funding=status};basis=basis with{Revision=basis.ContentRevision()};var result=original with{Basis=basis,Program=CompanyInventorySourceReview.Copy(original.Program)};result=result with{Revision=result.ContentRevision()};result.Require();return result;
    }
    internal static async Task<DrillingProgramAssignedReview> Packet(DrillingProgramReviewStartNativeTests.Fixture f)
    {
        var graph=await f.Native.Service.ReadRecordedProgramReviewAsync(f.Original.Scope().Company,f.Original.InstanceId(),f.Guard);var task=Assert.Single(graph.Tasks);var source=AsReader(f.Original.Source,"program-reviewer");
        var result=new DrillingProgramAssignedReview(f.Original,source,task.PROCESS_STEP_INSTANCE_ID,task.ASSIGNED_ROLE,task.TARGET_PERSONA_CODE,task.CROSS_TASK_ID,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(graph.Process)),ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(task)),graph.NativeRevision,"",DateTimeOffset.UtcNow);result=result with{Revision=result.ContentRevision()};result.Require(source.Basis.Scope().Company,"program-reviewer",f.Original.InstanceId());return result;
    }
    [Fact]public async Task ActualOwnedNativeAssignmentReadsExactSourceAndTasksWithoutMutationOrLiveDefinition()
    {
        using var f=new DrillingProgramReviewStartNativeTests.Fixture();await f.Prepare(2);await f.Start();var before=JsonSerializer.Serialize(f.Native.Native.Rows);var writes=f.Native.Native.Writes;var packet=await Packet(f);Assert.Equal("DRILLING_REVIEW",packet.PersonaCode);Assert.Equal("DRAFT",packet.Source.Program.PROGRAM_STATUS);Assert.Equal("program-reviewer",packet.Source.Basis.Scope().ActorUserId);Assert.NotEqual(packet.Source.Basis.Scope().ActorUserId,packet.Original.Scope().ActorUserId);Assert.Equal(before,JsonSerializer.Serialize(f.Native.Native.Rows));Assert.Equal(writes,f.Native.Native.Writes);
        var graph=await f.Native.Service.ReadRecordedProgramReviewAsync(f.Original.Scope().Company,f.Original.InstanceId(),f.Guard);graph.Original.Source.Program.PROGRAM_NAME="Caller mutation";Assert.Equal(f.Original.Source.Program.PROGRAM_NAME,(await Packet(f)).Original.Source.Program.PROGRAM_NAME);f.Native.Native.Rows.RemoveAll(row=>row is PROCESS_DEFINITION);Assert.NotNull(await Packet(f));Assert.Equal(writes,f.Native.Native.Writes);
    }
    [Fact]public async Task CollidingFieldAndCompanyAuthorityCannotReadAnotherCompanyProgramAssignment()
    {
        using var f=new DrillingProgramReviewStartNativeTests.Fixture();await f.Prepare();await f.Start();var writes=f.Native.Native.Writes;await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Native.Service.ReadRecordedProgramReviewAsync(new("tenant","foreign"),f.Original.InstanceId(),f.Guard));Assert.Equal(writes,f.Native.Native.Writes);
    }
    [Theory][InlineData("task")][InlineData("route")][InlineData("source")][InlineData("field")]
    public async Task ChangedNativeSourceOrTaskCannotBecomeAnAssignedReview(string fault)
    {
        using var f=new DrillingProgramReviewStartNativeTests.Fixture();await f.Prepare();await f.Start();if(fault=="task")f.Native.Native.Rows.RemoveAll(row=>row is CROSS_PERSONA_TASK);if(fault=="route")Assert.Single(f.Native.Native.Rows.OfType<CROSS_PERSONA_TASK>()).ROUTE="/other";if(fault=="source")Assert.Single(f.Native.Native.Rows.OfType<PROCESS_INSTANCE>()).ENTITY_TYPE="WELL";if(fault=="field")Assert.Single(f.Native.Native.Rows.OfType<PROCESS_INSTANCE>()).FIELD_ID="foreign-field";await Assert.ThrowsAnyAsync<Exception>(()=>f.Native.Service.ReadRecordedProgramReviewAsync(f.Original.Scope().Company,f.Original.InstanceId(),f.Guard));Assert.Equal(4,f.Native.Native.Writes);
    }
    [Theory][InlineData("actor")][InlineData("company")][InlineData("persona")][InlineData("role")][InlineData("task")][InlineData("source")][InlineData("native-revision")]
    public async Task AssignedProjectionCannotSubstituteCompanyActorWorkspaceTaskOrUnrecordedProgram(string fault)
    {
        using var f=new DrillingProgramReviewStartNativeTests.Fixture();await f.Prepare();await f.Start();var packet=await Packet(f);var scope=packet.Source.Basis.Scope();if(fault=="persona")packet=packet with{PersonaCode="FOREIGN"};if(fault=="role")packet=packet with{AssignedRole="ForeignRole"};if(fault=="task")packet=packet with{TaskId="foreign-task"};if(fault=="source")packet.Source.Program.PROGRAM_NAME="Unrecorded source";if(fault=="native-revision")packet=packet with{NativeRevision=""};packet=packet with{Revision=packet.ContentRevision()};Assert.ThrowsAny<Exception>(()=>packet.Require(fault=="company"?new("tenant","foreign"):scope.Company,fault=="actor"?"other":scope.ActorUserId,packet.InstanceId));
    }
}
#endif

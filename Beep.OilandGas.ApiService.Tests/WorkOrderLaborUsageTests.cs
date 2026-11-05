#if WORKFLOW_INBOX_FOCUSED
extern alias actualOwners;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
using System.Text.Json;
using Moq;
using Xunit;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.PPDM39.Core.Metadata;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Utilities;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Permit=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_EXECUTION_PERMIT;
using Detail=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderDetailModel;
using Condition=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.InspectionCondition;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class WorkOrderLaborUsageTests
{
    private sealed class Fixture
    {
        internal readonly PersonaTaskRoutingTests.Fixture Native=new();
        internal readonly WorkOrderExecutionPermitService Owner;
        internal readonly CompanyWorkOrderExecutionStartCommand Start;
        internal readonly WORK_ORDER_EXECUTION_START Started;
        internal readonly CompanyWorkOrderExecutionRequirements Before;
        internal bool Denied;
        internal Fixture()
        {
            WORK_ORDER_PERSONNEL_ASSIGNMENT? assigned=null;var original=WorkOrderExecutionStartFixture.Original(true,captureAssignment:row=>assigned=row);
            Start=original.Command;Before=Start.Review.Prerequisites.Requirements;Started=Start.Prepare(DateTime.UtcNow);
            Native.Rows.Clear();Native.Rows.Add(original.Recorded);Native.Rows.Add(Started);Native.Rows.Add(assigned!);
            var types=new Dictionary<string,Type>{{"WORK_ORDER_EXECUTION_PERMIT",typeof(Permit)},{"WORK_ORDER_EXECUTION_START",typeof(WORK_ORDER_EXECUTION_START)},
                {"WORK_ORDER_EXECUTION_SUBMISSION",typeof(WORK_ORDER_EXECUTION_SUBMISSION)},{"WORK_ORDER_PERSONNEL_ASSIGNMENT",typeof(WORK_ORDER_PERSONNEL_ASSIGNMENT)},
                {"WORK_ORDER_PERSONNEL_RELEASE",typeof(WORK_ORDER_PERSONNEL_RELEASE)},{"WORK_ORDER_LABOR_USAGE",typeof(WORK_ORDER_LABOR_USAGE)}};
            var creator=new ClassCreator(Native.Editor.Object);Native.Editor.SetupGet(value=>value.classCreator).Returns(creator);
            Native.Source.Setup(value=>value.CheckEntityExist(It.IsAny<string>())).Returns((string table)=>types.ContainsKey(table));
            Native.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure shape,bool _)=>creator.ConvertToEntityStructure(types[shape.EntityName]));
            var metadata=new Mock<IPPDMMetadataRepository>();foreach(var (table,type) in types)
            {
                var key=type.GetProperties().Single(property=>property.IsDefined(typeof(System.ComponentModel.DataAnnotations.KeyAttribute),true)).Name;
                metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(table)).ReturnsAsync([key]);
            }
            Native.Source.Setup(value=>value.InsertEntity("WORK_ORDER_LABOR_USAGE",It.IsAny<object>())).Returns((string _,object row)=>{
                Native.Writes++;Native.Rows.Add(CompanyInventorySourceReview.Copy((WORK_ORDER_LABOR_USAGE)row));return new ErrorsInfo{Flag=Errors.Ok};});
            Owner=new(Native.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object);
        }
        internal Task Authority()=>Denied?Task.FromException(new UnauthorizedAccessException("Revoked execution access")):Task.CompletedTask;
        internal Task<(Detail Order,IReadOnlyList<Condition> Requirements)> Current(bool owned)=>Task.FromResult((Before.Order,Before.Requirements));
        internal Task<CompanyMaintenanceAssignedStageReview> Stage()=>Task.FromResult(Start.Review.Stage);
        internal Task<CompanyWorkOrderLaborUsageReview> Review()=>Owner.ReviewLaborAsync(Before.Context,new(Before.Approval.Permit,Start.IntentId),Authority,Current,Stage);
        internal CompanyWorkOrderLaborUsageCommand Command(CompanyWorkOrderLaborUsageReview review)=>new(review,Guid.NewGuid(),Started.STARTED_AT_UTC,review.ComparedAtUtc,
            (decimal)(review.ComparedAtUtc.Ticks-Started.STARTED_AT_UTC.Ticks)/TimeSpan.TicksPerSecond,WorkOrderLaborTimeUnit.Second,"Actual native labor", "Own duty report");
        internal Task<CompanyWorkOrderLaborUsageRecovery> Record(CompanyWorkOrderLaborUsageCommand command)=>Owner.RecordLaborAsync(command,Authority,Current,Stage);
    }
    [Fact] public async Task NativeLaborRecordsExactDutyOnceAndRecoversOriginalWithoutCurrentProofOrReplay()
    {
        var fixture=new Fixture();var review=await fixture.Review();var command=fixture.Command(review);var start=JsonSerializer.Serialize(fixture.Started);
        var recovery=await fixture.Record(command);recovery.Require(review.Execution.Start.Company,review.Execution.Start.ExecutorUserId,command.Reference());
        Assert.Equal(1,fixture.Native.Writes);Assert.Equal(new[]{"begin","commit"},fixture.Native.Transactions);
        Assert.Equal(command.Quantity,recovery.Recorded.QUANTITY);Assert.Equal(review.Assignment.AssignmentId,recovery.Recorded.ASSIGNMENT_ID);
        Assert.Equal(start,JsonSerializer.Serialize(fixture.Started));
        var next=await fixture.Review();Assert.Single(next.Occupancies);Assert.Throws<InvalidOperationException>(()=>(command with{Review=next,IntentId=Guid.NewGuid()}).Require());
        fixture.Native.Rows.Remove(fixture.Started); // Recovery remains historical and cannot manufacture a new start.
        Assert.Equal(JsonSerializer.Serialize(recovery),JsonSerializer.Serialize(await fixture.Record(command)));
        Assert.Equal(JsonSerializer.Serialize(recovery),JsonSerializer.Serialize(await fixture.Owner.RecoverLaborAsync(fixture.Before.Context,command.Reference(),fixture.Authority)));
        Assert.Equal(1,fixture.Native.Writes);fixture.Denied=true;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>fixture.Owner.RecoverLaborAsync(fixture.Before.Context,command.Reference(),fixture.Authority));
    }
    [Theory][InlineData("zero")][InlineData("unit")][InlineData("quantity")][InlineData("future")][InlineData("before")][InlineData("zone")][InlineData("overlap")]
    public async Task ExplicitActualTimeCannotBeInferredRoundedOrDuplicated(string fault)
    {
        var fixture=new Fixture();var review=await fixture.Review();var command=fixture.Command(review);
        command=fault switch{
            "zero"=>command with{Quantity=0},"unit"=>command with{Unit=WorkOrderLaborTimeUnit.Unspecified},"quantity"=>command with{Quantity=command.Quantity+1},
            "future"=>command with{ToUtc=review.ComparedAtUtc.AddSeconds(1)},"before"=>command with{FromUtc=fixture.Started.STARTED_AT_UTC.AddSeconds(-1)},
            "zone"=>command with{FromUtc=DateTime.SpecifyKind(command.FromUtc,DateTimeKind.Unspecified)},
            _=>command with{Review=review with{Occupancies=[new("original-labor",new string('A',64),command.FromUtc,command.ToUtc)]}}};
        await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Record(command));Assert.Equal(0,fixture.Native.Writes);Assert.Empty(fixture.Native.Transactions);
    }
    [Theory][InlineData("missing")][InlineData("foreign")][InlineData("changed")]
    public async Task NativeAssignmentIsRequiredBeforeAnyLaborMutation(string fault)
    {
        var fixture=new Fixture();var row=fixture.Native.Rows.OfType<WORK_ORDER_PERSONNEL_ASSIGNMENT>().Single();
        if(fault=="missing")fixture.Native.Rows.Remove(row);else if(fault=="foreign")row.OWNER_TENANT_ID="other";else row.DUTY_CODE="changed";
        await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Review());Assert.Equal(0,fixture.Native.Writes);
    }
}
#endif

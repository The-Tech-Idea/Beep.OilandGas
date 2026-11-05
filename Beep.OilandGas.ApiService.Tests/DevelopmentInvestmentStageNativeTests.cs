#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;
public sealed class DevelopmentInvestmentStageNativeTests
{
    internal sealed class Fixture:IDisposable
    {
        internal DevelopmentInvestmentHandoffNativeTests.Fixture Handoff=new();
        internal DevelopmentInvestmentStageDecision Command=null!;
        internal DevelopmentInvestmentReview Current=null!;
        internal int Mutations,FailAt;internal bool Revoked;
        internal async Task Prepare(int stages=1,string decision="APPROVE")
        {
            await Handoff.Prepare(stages);await Handoff.Start();
            var basis=Handoff.Current.Current;var progress=basis.Source.Review with{Scope=basis.Source.Review.Scope with{ActorUserId="independent-reviewer"}};
            progress=progress with{Revision=progress.ContentRevision()};basis=basis with{Source=basis.Source with{Review=progress},Books=basis.Books.AsReader("independent-reviewer")};basis=basis with{Revision=basis.ContentRevision()};
            Current=Handoff.Current with{Current=basis};Current.Require();
            var native=Handoff.Native.Native;
            ErrorsInfo Write(string table,object value,bool insert)
            {
                native.Writes++;Mutations++;if(Mutations==FailAt)return new(){Flag=Errors.Failed};var copy=JsonSerializer.Deserialize(JsonSerializer.Serialize(value),value.GetType())!;
                if(insert)native.Rows.Add(copy);
                else
                {
                    var key=table switch{"PROCESS_INSTANCE"=>"PROCESS_INSTANCE_ID","PROCESS_STEP_INSTANCE"=>"PROCESS_STEP_INSTANCE_ID","CROSS_PERSONA_TASK"=>"CROSS_TASK_ID",_=>throw new InvalidOperationException(table)};
                    var identity=value.GetType().GetProperty(key)!.GetValue(value);var index=native.Rows.FindIndex(row=>row.GetType()==value.GetType() && Equals(row.GetType().GetProperty(key)!.GetValue(row),identity));
                    if(index<0)throw new InvalidOperationException("The original update row is missing.");native.Rows[index]=copy;
                }
                return new(){Flag=Errors.Ok};
            }
            native.Source.Setup(source=>source.InsertEntity(It.IsAny<string>(),It.IsAny<object>())).Returns((string table,object value)=>Write(table,value,true));
            native.Source.Setup(source=>source.UpdateEntity(It.IsAny<string>(),It.IsAny<object>())).Returns((string table,object value)=>Write(table,value,false));
            Command=await Create(decision);
        }
        internal async Task<DevelopmentInvestmentStageDecision> Create(string decision="APPROVE")
        {
            var process=(await Handoff.Native.Service.GetProcessInstanceAsync(Handoff.Original.InstanceId()))!;var step=process.StepInstances.Single(item=>item.StepId==process.CurrentStepId);
            var task=Handoff.Native.Native.Rows.OfType<CROSS_PERSONA_TASK>().Single(item=>item.PROCESS_STEP_INSTANCE_ID==step.StepInstanceId && item.TASK_STATUS=="PENDING");var original=Handoff.Original;
            var review=new DevelopmentInvestmentAssignedReview(Current,process.InstanceId,original.Workflow.ProcessName,original.Workflow.Version,original.Scope().ActorUserId,
                step.StepId,step.StepInstanceId,step.StepName,step.RequiredRole!,"FINANCE",task.CROSS_TASK_ID,original.Workflow.DefinitionRevision,
                ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(process)),ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(task)),"",DateTimeOffset.UtcNow)
                {FundingRationale=Handoff.Source.Request.Rationale,StageNumber=original.Workflow.Steps.ToList().FindIndex(item=>item.StepId==step.StepId)+1,StageCount=original.Workflow.Steps.Count};
            review=review with{Revision=review.ContentRevision()};var result=new DevelopmentInvestmentStageDecision(review,Guid.NewGuid(),decision,"Recorded independent investment stage rationale");result.Require();return result;
        }
        internal Task Guard(){if(Revoked)throw new UnauthorizedAccessException("Current stage authority was revoked.");return Task.CompletedTask;}
        internal Task<DevelopmentInvestmentReview> ReadSource(){Assert.DoesNotContain("begin",Handoff.Native.Native.Transactions.TakeLast(1));return Task.FromResult(Current);}
        internal Task<DevelopmentInvestmentStageReceipt> Reserve()=>Handoff.Native.Service.ReserveInvestmentStageAsync(Command,Guard,ReadSource);
        internal Task<DevelopmentInvestmentStageReceipt> Complete()=>Handoff.Native.Service.CompleteInvestmentStageAsync(Command,Guard,ReadSource);
        internal Task<DevelopmentInvestmentStageReceipt> Recover()=>Handoff.Native.Service.RecoverInvestmentStageAsync(Command,Guard,ReadSource);
        internal void Financial(){var facts=DevelopmentInvestmentCommand.Facts(Command.FinancialCommand().ToRecord(DateTime.UtcNow));Current=Current with{Recorded=facts};}
        public void Dispose()=>Handoff.Dispose();
    }
    [Theory][InlineData("step-name")][InlineData("process-revision")][InlineData("task-revision")][InlineData("workflow-version")]
    public async Task NativeReservationAdmitsTheExactDisplayedStageWithoutASeparateReceivingRead(string fault)
    {
        using var f=new Fixture();await f.Prepare();var review=f.Command.Review;
        review=fault switch{"step-name"=>review with{StepName="Forged displayed stage"},"process-revision"=>review with{ProcessRevision=new('E',64)},"task-revision"=>review with{TaskRevision=new('E',64)},_=>review with{WorkflowVersion="different-version"}};
        review=review with{Revision=review.ContentRevision()};f.Command=f.Command with{Review=review};await Assert.ThrowsAnyAsync<Exception>(()=>f.Reserve());Assert.Equal(0,f.Mutations);Assert.Empty(f.Handoff.Native.Native.Rows.OfType<PROCESS_APPROVAL>());
    }
    [Fact]public async Task DurableReservationOwnsOneOriginalIntentAndRecoveryDoesNotWrite()
    {
        using var f=new Fixture();await f.Prepare();Assert.Equal("ABSENT",(await f.Recover()).Status);Assert.Equal(0,f.Mutations);
        var reserved=await f.Reserve();reserved.Require(f.Command);Assert.Equal("RESERVED",reserved.Status);Assert.Equal(1,f.Mutations);
        Assert.Equal("RESERVED",(await f.Reserve()).Status);Assert.Equal("RESERVED",(await f.Recover()).Status);Assert.Equal(1,f.Mutations);
        var original=f.Command;f.Command=f.Command with{IntentId=Guid.NewGuid()};await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Reserve());f.Command=original;
        Assert.Empty(f.Handoff.Native.Native.Rows.OfType<PROCESS_APPROVAL>());Assert.Equal("PENDING",f.Handoff.Native.Native.Rows.OfType<CROSS_PERSONA_TASK>().Single().TASK_STATUS);
    }
    [Fact]public async Task IntermediateApprovalUsesCanonicalAuditAndRoutesOnlyTheNextStageWithoutFinance()
    {
        using var f=new Fixture();await f.Prepare(2);await f.Reserve();var receipt=await f.Complete();Assert.Equal("RECORDED",receipt.Status);Assert.NotNull(receipt.NextStepInstanceId);Assert.Null(receipt.Financial);
        var process=(await f.Handoff.Native.Service.GetProcessInstanceAsync(f.Command.Review.InstanceId))!;Assert.Equal(ProcessStatus.IN_PROGRESS,process.Status);Assert.Equal("investment-2",process.CurrentStepId);
        Assert.Single(f.Handoff.Native.Native.Rows.OfType<PROCESS_APPROVAL>());Assert.Equal(2,f.Handoff.Native.Native.Rows.OfType<CROSS_PERSONA_TASK>().Count());
        Assert.Single(f.Handoff.Native.Native.Rows.OfType<CROSS_PERSONA_TASK>().Where(task=>task.TASK_STATUS=="PENDING"));Assert.Null(f.Current.Recorded);
        var read=await f.Handoff.Native.Service.ReadRecordedInvestmentHandoffAsync(f.Command.Scope().Company,f.Command.Scope().FieldId,f.Current.AfeId,f.Guard);Assert.Equal("investment-2",read!.Value.Process.CurrentStepId);
        var originalReceipt=await f.Handoff.Native.Service.RecoverInvestmentHandoffAsync(f.Handoff.Original,f.Guard,()=>Task.FromResult(f.Handoff.Current));Assert.True(originalReceipt.Recorded);Assert.Single(originalReceipt.TaskIds);
        Assert.Equal("RECORDED",(await f.Recover()).Status);f.Command=await f.Create();Assert.Equal(2,f.Command.Review.StageNumber);Assert.Equal("RESERVED",(await f.Reserve()).Status);
    }
    [Fact]public async Task FinalApprovalRequiresItsExactOriginalCanonicalFinancialRecordBeforeCompletion()
    {
        using var f=new Fixture();await f.Prepare();await f.Reserve();var writes=f.Mutations;await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Complete());Assert.Equal(writes,f.Mutations);
        var other=f.Command.FinancialCommand() with{IntentId=Guid.NewGuid()};f.Current=f.Current with{Recorded=DevelopmentInvestmentCommand.Facts(other.ToRecord(DateTime.UtcNow))};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Complete());Assert.Equal(writes,f.Mutations);f.Financial();Assert.Equal("FINANCIAL_RECORDED",(await f.Recover()).Status);
        var result=await f.Complete();Assert.Equal("RECORDED",result.Status);Assert.Equal(f.Current.Recorded,result.Financial);Assert.Equal("REVIEW_APPROVED",result.WorkflowState);
        Assert.Equal(ProcessStatus.COMPLETED,(await f.Handoff.Native.Service.GetProcessInstanceAsync(result.InstanceId))!.Status);Assert.Equal("DRAFT",f.Current.Afe().Status);
        writes=f.Mutations;Assert.Equal("RECORDED",(await f.Recover()).Status);Assert.Equal(writes,f.Mutations);
    }
    [Fact]public async Task RejectionStopsAllRemainingStagesAfterItsRecordedFinancialOutcome()
    {
        using var f=new Fixture();await f.Prepare(3,"REJECT");await f.Reserve();f.Financial();var result=await f.Complete();Assert.Equal("REVIEW_REJECTED",result.WorkflowState);Assert.Null(result.NextStepInstanceId);
        var process=(await f.Handoff.Native.Service.GetProcessInstanceAsync(result.InstanceId))!;Assert.Equal(ProcessStatus.FAILED,process.Status);Assert.Equal(2,process.StepInstances.Count(step=>step.Status==StepStatus.BLOCKED));
        Assert.All(f.Handoff.Native.Native.Rows.OfType<CROSS_PERSONA_TASK>(),task=>Assert.Equal("COMPLETED",task.TASK_STATUS));
    }
    [Theory][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)][InlineData(5)][InlineData(6)]
    public async Task EveryNativeCompletionMutationRollsBackToTheOriginalReservedFinancialState(int mutation)
    {
        using var f=new Fixture();await f.Prepare();await f.Reserve();f.Financial();f.Mutations=0;f.FailAt=mutation;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Complete());Assert.Empty(f.Handoff.Native.Native.Rows.OfType<PROCESS_APPROVAL>());
        Assert.Equal("PENDING",f.Handoff.Native.Native.Rows.OfType<CROSS_PERSONA_TASK>().Single().TASK_STATUS);f.FailAt=0;var writes=f.Mutations;
        Assert.Equal("FINANCIAL_RECORDED",(await f.Recover()).Status);Assert.Equal(writes,f.Mutations);Assert.Equal("RECORDED",(await f.Complete()).Status);
    }
    [Fact]public async Task UnknownCommitRecoversWithoutRepairingOrDuplicatingTheRecordedStage()
    {
        using var f=new Fixture();await f.Prepare();await f.Reserve();f.Financial();f.Handoff.Native.Native.FailCommit=true;await Assert.ThrowsAnyAsync<Exception>(()=>f.Complete());f.Handoff.Native.Native.FailCommit=false;
        var writes=f.Mutations;Assert.Equal("RECORDED",(await f.Recover()).Status);Assert.Equal(writes,f.Mutations);
        f.Handoff.Native.Native.Rows.RemoveAll(row=>row is CROSS_PERSONA_TASK);await Assert.ThrowsAnyAsync<Exception>(()=>f.Recover());Assert.Equal(writes,f.Mutations);
    }
    [Theory][InlineData("approval")][InlineData("history")][InlineData("step")][InlineData("task")]
    public async Task ChangedCanonicalEvidenceCannotBeAdoptedAsOriginalRecovery(string fault)
    {
        using var f=new Fixture();await f.Prepare();await f.Reserve();f.Financial();await f.Complete();var native=f.Handoff.Native.Native;
        if(fault=="approval")native.Rows.OfType<PROCESS_APPROVAL>().Single().APPROVED_BY="another";
        if(fault=="history")native.Rows.OfType<PROCESS_HISTORY>().Single(row=>row.EVENT_TYPE=="INVESTMENT_REVIEW_APPROVED").DETAILS="Changed rationale";
        if(fault=="step")native.Rows.OfType<PROCESS_STEP_INSTANCE>().Single().STATUS="PENDING";
        if(fault=="task")native.Rows.OfType<CROSS_PERSONA_TASK>().Single().TASK_STATUS="PENDING";
        var writes=f.Mutations;await Assert.ThrowsAnyAsync<Exception>(()=>f.Recover());Assert.Equal(writes,f.Mutations);
    }
    [Fact]public async Task CurrentRevocationAndAnUnexpectedApprovalCannotBecomeAbsenceOrReservation()
    {
        using var f=new Fixture();await f.Prepare();f.Revoked=true;await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Reserve());Assert.Equal(0,f.Mutations);f.Revoked=false;
        f.Handoff.Native.Native.Rows.Add(new PROCESS_APPROVAL{PROCESS_APPROVAL_ID="unqualified",PROCESS_INSTANCE_ID=f.Command.Review.InstanceId});
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Recover());await Assert.ThrowsAnyAsync<Exception>(()=>f.Reserve());Assert.Equal(0,f.Mutations);
    }
}
#endif

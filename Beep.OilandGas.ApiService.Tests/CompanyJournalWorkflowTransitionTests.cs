#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyJournalWorkflowTransitionTests
{
    private static ProcessStepDefinition Step(string id,int sequence,string action,bool approval)=>new(){StepId=id,StepName=id,SequenceNumber=sequence,RequiredRoles=["ConfiguredCompanyRole"],RequiresApproval=approval,StepType=approval?"APPROVAL":"ACTION",StepConfiguration=new(){[CompanyAccountingStepEvidence.ConfigurationKey]=action}};
    private static async Task<(CompanyProcessStartTests.Fixture Native,CompanyAccountingProcessStart Start,CompanyAccountingProcessStartReceipt Receipt)> Prepare()
    {
        var f=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");f.Definition.ENTITY_TYPE="JOURNAL_ENTRY";f.Definition.PROCESS_CONFIG_JSON=JsonSerializer.Serialize(new{Steps=new[]{Step("review",1,CompanyAccountingSourceTypes.Approval("JOURNAL_ENTRY"),true),Step("post",2,CompanyAccountingSourceTypes.JournalPosting,false),Step("reject",3,CompanyAccountingSourceTypes.RejectionReview("JOURNAL_ENTRY"),false)}});CompanyStepCommandTests.ConfigureUpdates(f);var definition=await f.Service.GetProcessDefinitionAsync("definition");var start=new CompanyAccountingProcessStart(new("tenant","company"),"maker",Guid.NewGuid().ToString("N"),"definition","JOURNAL_ENTRY",Guid.NewGuid().ToString("N"),new string('A',64),ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition)),new string('B',64),"maker");return(f,start,await f.Start(start));
    }
    private static async Task<CompanyAccountingStepCommand> Command(CompanyProcessStartTests.Fixture f,CompanyAccountingProcessStart start,string actor,bool rejected=false)
    {
        var process=(await f.Service.GetProcessInstanceAsync(start.InstanceId()))!;var step=process.StepInstances.Single(value=>value.StepId==process.CurrentStepId);var approval=step.StepId=="review";var review=new CompanyAccountingStepReview(start.Company,actor,start.InstanceId(),start.ProcessId,start.EntityType,start.EntityId,step.StepInstanceId,step.StepId,step.StepName,step.RequiredRole,"maker","maker",approval,approval?"Workflow.Approve":"Workflow.Execute",!rejected,rejected?CompanyAccountingSourceTypes.RejectionEligibility("JOURNAL_ENTRY"):"ELIGIBLE",start.SourceRevision,start.DefinitionRevision,start.BindingRevision,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(process)),new string('C',64),new string('D',64),AccountingEvidenceAction:approval?CompanyAccountingSourceTypes.Approval("JOURNAL_ENTRY"):step.StepId=="post"?CompanyAccountingSourceTypes.JournalPosting:CompanyAccountingSourceTypes.RejectionReview("JOURNAL_ENTRY"));return new(review,Guid.NewGuid().ToString("N"),"Exact recorded native journal evidence",rejected?CompanyAccountingSourceTypes.RejectionRoute("JOURNAL_ENTRY"):null);
    }
    [Theory][InlineData(false)][InlineData(true)] public async Task JournalBranchesRouteCompletePersonaTaskSetsAndExactRetryWithoutAnotherWrite(bool rejected)
    {
        var (f,start,receipt)=await Prepare();var review=await Command(f,start,"reviewer",rejected);var first=await f.Service.CompleteCompanyAccountingStepAsync(review,f.Guard);first.Require(review);var instance=(await f.Service.GetProcessInstanceAsync(start.InstanceId()))!;Assert.Equal(rejected?"reject":"post",instance.CurrentStepId);Assert.Equal(2,first.CompletedTaskIds.Count);Assert.Equal(2,first.NextTaskIds.Count);Assert.Equal(StepStatus.PENDING,instance.StepInstances.Single(value=>value.StepId==(rejected?"reject":"post")).Status);if(rejected)Assert.Equal(StepStatus.SKIPPED,instance.StepInstances.Single(value=>value.StepId=="post").Status);
        var writeCount=f.Native.Writes;var replay=await f.Service.CompleteCompanyAccountingStepAsync(review,f.Guard);Assert.Equal(JsonSerializer.Serialize(first),JsonSerializer.Serialize(replay));Assert.Equal(writeCount,f.Native.Writes);
        var next=await Command(f,start,rejected?"maker":"poster");var completed=await f.Service.CompleteCompanyAccountingStepAsync(next,f.Guard);completed.Require(next);instance=(await f.Service.GetProcessInstanceAsync(start.InstanceId()))!;Assert.Equal(ProcessStatus.COMPLETED,instance.Status);Assert.Equal(rejected?"REJECTION_REVIEWED":"POSTED_CONFIRMED",instance.CurrentState);Assert.Empty(completed.NextTaskIds);Assert.All(f.Native.Rows.OfType<CROSS_PERSONA_TASK>(),value=>Assert.Equal("COMPLETED",value.TASK_STATUS));if(!rejected)Assert.Equal(StepStatus.SKIPPED,instance.StepInstances.Single(value=>value.StepId=="reject").Status);
    }
    [Fact] public async Task InvalidJournalGraphCannotStartOrBypassRequiredNativePostingEvidence()
    {
        var f=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");f.Definition.ENTITY_TYPE="JOURNAL_ENTRY";var definition=await f.Service.GetProcessDefinitionAsync("definition");var start=new CompanyAccountingProcessStart(new("tenant","company"),"maker",Guid.NewGuid().ToString("N"),"definition","JOURNAL_ENTRY",Guid.NewGuid().ToString("N"),new string('A',64),ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition)),new string('B',64),"maker");await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Start(start));Assert.Equal(0,f.Native.Writes);
        var (native,valid,_)=await Prepare();var command=await Command(native,valid,"reviewer");var missing=command with{Review=command.Review with{AccountingEvidenceRevision=null}};Assert.Throws<InvalidOperationException>(()=>missing.Require());
    }
}
#endif

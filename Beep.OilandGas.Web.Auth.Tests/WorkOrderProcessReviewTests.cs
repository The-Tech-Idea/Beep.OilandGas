using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderProcessReviewTests
{
    private static ProcessInstance Process()=>new(){InstanceId="process/a?b",ProcessId="definition",EntityId="order",EntityType="WORK_ORDER",FieldId="field",CurrentState="PM_SCHEDULE",CurrentStepId="schedule",Status=ProcessStatus.IN_PROGRESS};
    private static ProcessDefinition Definition()=>new(){ProcessId="definition",ProcessName="Recorded process",ProcessType="WORK_ORDER",EntityType="WORK_ORDER"};
    private static WorkOrderProcessReview Review()=>WorkOrderProcessReviews.Project(new("tenant","company"),"actor","field","order",[Process()],[Definition()]);
    [Fact] public void ExactProcessLinkPreservesStateAndEncodesActualIdentity(){var r=Review();var link=Assert.Single(r.Links);Assert.Equal("PM_SCHEDULE",link.State);Assert.Equal("IN_PROGRESS",link.Status);Assert.Equal("/ppdm39/process/instances/process%2Fa%3Fb",link.Route);Assert.Equal("schedule",link.StepId);}
    [Theory][InlineData("entity")][InlineData("entitytype")][InlineData("identity")][InlineData("field")][InlineData("definition")][InlineData("duplicate")][InlineData("null")]
    public void WrongEntityOrMalformedSourceCannotProduceLinks(string fault){var row=Process();if(fault=="entity")row.EntityId="foreign";if(fault=="entitytype")row.EntityType="EQUIPMENT";if(fault=="identity")row.InstanceId="";if(fault=="field")row.FieldId="";if(fault=="definition")row.ProcessId="";IReadOnlyList<ProcessInstance>? rows=fault switch{"duplicate"=>[row,row],"null"=>null,_=>[row]};Assert.Throws<RefusalException>(()=>WorkOrderProcessReviews.Project(new("tenant","company"),"actor","field","order",rows,[Definition()]));}
    [Theory][InlineData("entity")][InlineData("type")][InlineData("name")][InlineData("missing")][InlineData("duplicate")]
    public void SeededEquipmentOrUnconfirmedDefinitionCannotBecomeOrderWorkflow(string fault){var def=Definition();if(fault=="entity")def.EntityType="EQUIPMENT";if(fault=="type")def.ProcessType="PRODUCTION";if(fault=="name")def.ProcessName="";IReadOnlyList<ProcessDefinition> definitions=fault switch{"missing"=>[],"duplicate"=>[def,def],_=>[def]};Assert.Throws<RefusalException>(()=>WorkOrderProcessReviews.Project(new("tenant","company"),"actor","field","order",[Process()],definitions));}
    [Fact] public void AnotherFieldProcessDoesNotPublishOrRequireItsDefinition(){var row=Process();row.FieldId="other-field";Assert.Empty(WorkOrderProcessReviews.Project(new("tenant","company"),"actor","field","order",[row],[]).Links);}
    private static async Task<(CompanyClientContext Company,WorkOrderDetailState State)> Loaded(){var c=new CompanyClientContext();Assert.True(c.TrySelect(c.Clear(),"actor",new(new("tenant","company"),[],[])));var state=new WorkOrderDetailState(c);await state.LoadAsync("order",()=>Task.FromResult<string?>("field"),_=>Task.FromResult<WorkOrderDetailModel?>(new(){InstanceId="order",FieldId="field",State="DRAFT"}));return(c,state);}
    [Theory][InlineData("company")][InlineData("actor")][InlineData("field")][InlineData("order")][InlineData("time")][InlineData("duplicate")][InlineData("null")]
    public async Task WrongResponseCannotPublishIntoHeader(string fault){var(c,s)=await Loaded();using(c)using(s){var r=Review();r=fault switch{"company"=>r with{Company=new("tenant","foreign")},"actor"=>r with{Actor="other"},"field"=>r with{FieldId="other"},"order"=>r with{WorkOrderId="other"},"time"=>r with{RetrievedAtUtc=default},"duplicate"=>r with{Links=[r.Links[0],r.Links[0]]},_=>null!};await Assert.ThrowsAsync<InvalidOperationException>(()=>s.LoadProcessesAsync(()=>Task.FromResult(r)));Assert.Null(s.Processes);Assert.NotNull(s.Current);}}
    [Fact] public async Task CompanySwitchRefusesLateProcessReview(){var(c,s)=await Loaded();using(c)using(s){var wait=new TaskCompletionSource<WorkOrderProcessReview>();var load=s.LoadProcessesAsync(()=>wait.Task);c.Clear();wait.SetResult(Review());await load;Assert.Null(s.Processes);Assert.Null(s.Current);}}
    [Fact] public async Task FailurePreservesHeaderButDoesNotBecomeEmptyProcesses(){var(c,s)=await Loaded();using(c)using(s){await s.LoadProcessesAsync(()=>Task.FromResult(Review()));Assert.NotNull(s.Processes);await Assert.ThrowsAsync<HttpRequestException>(()=>s.LoadProcessesAsync(()=>throw new HttpRequestException()));Assert.NotNull(s.Current);Assert.Null(s.Processes);}}
    [Fact] public async Task ActualEmptyReviewIsRecordedAbsence(){var(c,s)=await Loaded();using(c)using(s){await s.LoadProcessesAsync(()=>Task.FromResult(Review() with{Links=[]}));Assert.NotNull(s.Processes);Assert.Empty(s.Processes.Links);}}
}

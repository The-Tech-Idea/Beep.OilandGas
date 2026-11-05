using Beep.OilandGas.Accounting.Models;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Web.Services;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class CompanyAccountingProcessStateTests
{
    internal static CompanyClientContext Company()
    {var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View","Procurement.View"])));return company;}
    internal static ProcessInstance Source(string type="AP_INVOICE")=>new(){InstanceId="process",ProcessId="definition",EntityType=type,EntityId="source/α%2F",StartedBy="maker",CurrentStepId="review",
        ProcessData=new(){InstanceId="process",Data=new(){[CompanyAccountingProcessBinding.PayloadKey]=new CompanyAccountingProcessBinding(new("tenant","company"),"process",type,"source/α%2F")}},
        StepInstances=[new(){InstanceId="process",StepInstanceId="step-instance",StepId="review",StepName="Recorded review",RequiredRole="Accountant",AssignedTo="reviewer"}],
        History=[new(){HistoryId="history",InstanceId="process",Action="RECORDED",PerformedBy="maker",Notes="Recorded source history"}]};
    internal static ProcessInstanceDetailState State(CompanyClientContext company)=>new(company,_=>throw new Exception("No FIELD read"),_=>throw new Exception("No separate FIELD history"));
    [Theory][InlineData("valid")][InlineData("date")][InlineData("actor")][InlineData("status")][InlineData("history")]
    [InlineData("reason")][InlineData("history-time")][InlineData("before-start")][InlineData("terminal-state")][InlineData("ambiguous-history")][InlineData("steps")]
    public async Task TerminalCompanyProcessRequiresCompleteRecordedExecutionEvidence(string fault)
    {
        using var company=Company();using var state=State(company);var source=Source();var date=new DateTime(2026,1,2,0,0,0,DateTimeKind.Utc);
        source.StartDate=date.AddDays(-1);source.Status=ProcessStatus.COMPLETED;source.CurrentState="COMPLETED";source.CompletionDate=date;source.CompletedBy="reviewer";
        var step=source.StepInstances[0];step.SequenceNumber=1;step.Status=StepStatus.COMPLETED;step.CompletionDate=date;step.CompletedBy="reviewer";step.Notes="Recorded workflow decision";
        source.History=[new(){HistoryId="completion",InstanceId="process",StepInstanceId=step.StepInstanceId,Action="STEP_COMPLETED",PreviousState=step.StepId,NewState="COMPLETED",Timestamp=date,PerformedBy="reviewer",Notes=step.Notes}];
        switch(fault)
        {
            case "date":source.CompletionDate=null;break;case "actor":source.CompletedBy="another";break;case "status":step.Status=StepStatus.PENDING;break;
            case "history":source.History.Clear();break;case "reason":source.History[0].Notes="different";break;case "history-time":source.History[0].Timestamp=date.AddSeconds(1);break;
            case "before-start":source.StartDate=date.AddDays(1);break;case "terminal-state":source.History[0].NewState="post";break;
            case "ambiguous-history":var extra=JsonSerializer.Deserialize<ProcessHistoryEntry>(JsonSerializer.Serialize(source.History[0]))!;extra.HistoryId="different-history";extra.Notes="competing";source.History.Add(extra);break;
            case "steps":source.StepInstances.Clear();break;
        }
        if(fault=="valid"){await state.LoadAccountingAsync("process",_=>Task.FromResult<ProcessInstance?>(source));Assert.Equal("reviewer",state.Instance!.CompletedBy);Assert.NotNull(state.AccountingSourceRoute);}
        else{await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadAccountingAsync("process",_=>Task.FromResult<ProcessInstance?>(source)));Assert.Null(state.Instance);Assert.Empty(state.History);Assert.Null(state.AccountingSourceRoute);}
    }
    [Theory][InlineData("AP_INVOICE","/accounting/payables/invoices/")][InlineData("PURCHASE_ORDER","/procurement/orders/")]
    public async Task CompanySourceFreezesStepsHistoryAndExactOwnedSourceLink(string type,string prefix)
    {
        using var company=Company();using var state=State(company);var source=Source(type);
        await state.LoadAccountingAsync("process",_=>Task.FromResult<ProcessInstance?>(source));
        source.History[0].Notes="mutated";source.StepInstances[0].RequiredRole="mutated";source.EntityId="mutated";
        Assert.True(state.IsCompanyAccounting);Assert.Equal("Recorded source history",Assert.Single(state.History).Notes);
        Assert.Equal("Accountant",Assert.Single(state.Instance!.StepInstances).RequiredRole);Assert.Equal(prefix+"source%2F%CE%B1%252F",state.AccountingSourceRoute);
        company.Clear();Assert.Null(state.Instance);Assert.Null(state.AccountingSourceRoute);Assert.Empty(state.History);
    }
    [Theory][InlineData("company")][InlineData("field")][InlineData("missing")][InlineData("instance")]
    [InlineData("history")][InlineData("duplicate-history")][InlineData("step")][InlineData("duplicate-step")][InlineData("status")]
    public async Task UnconfirmedCompanyProcessOrHistoryCannotPublish(string fault)
    {
        using var company=Company();using var state=State(company);var source=Source();
        if(fault=="company")source.ProcessData.Data[CompanyAccountingProcessBinding.PayloadKey]=new CompanyAccountingProcessBinding(new("tenant","other"),"process","AP_INVOICE",source.EntityId);
        if(fault=="field")source.FieldId="other";if(fault=="missing")source.ProcessData.Data.Clear();if(fault=="instance")source.ProcessData.InstanceId="other";
        if(fault=="history")source.History[0].InstanceId="other";if(fault=="duplicate-history")source.History.Add(source.History[0]);
        if(fault=="step")source.StepInstances[0].InstanceId="other";if(fault=="duplicate-step")source.StepInstances.Add(source.StepInstances[0]);if(fault=="status")source.Status=(ProcessStatus)999;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadAccountingAsync("process",_=>Task.FromResult<ProcessInstance?>(source)));
        Assert.Null(state.Instance);Assert.Empty(state.History);Assert.Null(state.AccountingSourceRoute);
    }
    [Theory][InlineData(true)][InlineData(false)] public async Task ChangedCompanyOrInvalidationDiscardsLateCompanyProcess(bool changeCompany)
    {
        using var company=Company();using var state=State(company);var pending=new TaskCompletionSource<ProcessInstance?>();
        var load=state.LoadAccountingAsync("process",_=>pending.Task);if(changeCompany)company.Clear();else state.Clear();pending.SetResult(Source());await load;Assert.Null(state.Instance);
    }
    [Fact] public async Task FailedCompanyReloadClearsPriorSourceAndHistory()
    {
        using var company=Company();using var state=State(company);await state.LoadAccountingAsync("process",_=>Task.FromResult<ProcessInstance?>(Source()));
        await Assert.ThrowsAsync<HttpRequestException>(()=>state.LoadAccountingAsync("process",_=>throw new HttpRequestException()));Assert.Null(state.Instance);Assert.Empty(state.History);
    }
    [Fact] public async Task CompanySourceCannotInvokeFieldTransitionReviewOrSubmission()
    {
        using var company=Company();using var state=State(company);await state.LoadAccountingAsync("process",_=>Task.FromResult<ProcessInstance?>(Source()));
        var requests=0;await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadActionsAsync(_=>{requests++;throw new Exception("FIELD action");}));
        Assert.Equal(0,requests);Assert.Null(state.ActionReview);
        Assert.False(state.CanSubmit(new(new("tenant","company"),"actor","","process","","review",new string('A',64),[])));
    }
}

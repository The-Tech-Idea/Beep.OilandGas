using System.Security.Claims;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Processes;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class ProcessStepRoleChecksTests
{
    [Theory]
    [InlineData("Administrator","actor","APPROVAL","FINANCIAL",false)]
    [InlineData("Reviewer","actor","APPROVAL","FINANCIAL",false)]
    [InlineData("Reviewer","maker","APPROVAL","FINANCIAL",true)]
    [InlineData("Administrator","maker","APPROVAL","FINANCIAL",true)]
    [InlineData("Reviewer","","APPROVAL","FINANCIAL",false)]
    [InlineData("Reviewer","actor","REVIEW","FINANCIAL",false)]
    [InlineData("Reviewer","maker","REVIEW","FINANCIAL",true)]
    [InlineData("Reader","maker","REVIEW","FINANCIAL",false)]
    [InlineData("Reviewer","actor","DATA_ENTRY","FINANCIAL",true)]
    [InlineData("Reviewer","actor","REVIEW","OPERATIONAL",true)]
    public void RecordedMakerCannotCheckOwnApprovalEvenAsAdministrator(string role,string maker,string type,string category,bool expected)
    {
        var user=new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Role,role)],"test"));
        var instance=new ProcessInstance { ProcessId="definition", CurrentStepId="step", StartedBy=maker };
        var step=new ProcessStepInstance { StepId="step",RequiredRole="Reviewer" };
        var definition=new ProcessDefinition { ProcessId="definition",ProcessType=category };
        var template=new ProcessStepDefinition { StepId="step",StepType=type,RequiredRoles=["Reviewer"] };
        Assert.Equal(expected,ProcessStepRoleChecks.CanAct(user,"actor",instance,step,definition,template));
    }
    [Theory][InlineData("definition")][InlineData("step")][InlineData("current")][InlineData("anonymous")][InlineData("actor")]
    public void UnconfirmedStepOrAuthorityCannotAct(string fault)
    {
        var user=new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Role,"Administrator")],fault=="anonymous"?null:"test"));
        var instance=new ProcessInstance { ProcessId="definition",CurrentStepId=fault=="current"?"other":"step",StartedBy="maker" };
        var step=new ProcessStepInstance { StepId="step" };
        var definition=new ProcessDefinition { ProcessId=fault=="definition"?"other":"definition" };
        var template=new ProcessStepDefinition { StepId=fault=="step"?"other":"step",RequiresApproval=true };
        Assert.False(ProcessStepRoleChecks.CanAct(user,fault=="actor"?"":"actor",instance,step,definition,template));
    }
    [Fact]
    public void RecordedApprovalFlagStillRequiresIndependentActorWhenTemplateTypeIsNotApproval()
    {
        var user=new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Role,"Administrator")],"test"));
        Assert.False(ProcessStepRoleChecks.CanAct(user,"actor",new() { ProcessId="p",CurrentStepId="s",StartedBy="actor" },
            new() { StepId="s",ApprovalRequired=true },new() { ProcessId="p" },new() { StepId="s",StepType="ACTION" }));
    }
    [Theory]
    [InlineData("valid",true)][InlineData("stale",false)][InlineData("target",false)][InlineData("trigger",false)]
    [InlineData("role",false)][InlineData("maker",false)][InlineData("duplicate",false)]
    public void TransitionMustMatchReviewedStateTriggerRolesAndIndependentActor(string fault,bool allowed)
    {
        var user=new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Role,fault=="role"?"Reader":"Reviewer")],"test"));
        var instance=new ProcessInstance { ProcessId="p", CurrentState="source", StartedBy=fault=="maker"?"actor":"maker" };
        var transition=new ProcessTransition { TransitionId="different-id",FromStateId="source",ToStateId="target",Trigger="approve",RequiredRoles=["Reviewer"],RequiresApproval=true };
        var definition=new ProcessDefinition { ProcessId="p",Transitions=new() { ["different-id"]=transition } };
        if(fault=="duplicate")definition.Transitions.Add("duplicate",transition);
        Assert.Equal(allowed,ProcessStepRoleChecks.CanTransition(user,"actor",instance,definition,fault=="stale"?"old":"source",fault=="target"?"different-id":"target",fault=="trigger"?"other":"approve"));
    }
}

using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Processes;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class AccountingWorkflowDefinitionTests
{
    [Fact] public void NativeJournalDefinitionSeparatesIndependentReviewPostingAndMakerRejectionWithoutSystemPosting()
    {
        var definition=ProcessDefinitionInitializer.BuildCompanySalesJournalDefinition();var second=ProcessDefinitionInitializer.BuildCompanySalesJournalDefinition();
        Assert.Equal("ACCT_SALES_JOURNAL_REVIEW_POST",definition.ProcessId);Assert.Equal("JOURNAL_ENTRY",definition.EntityType);Assert.Equal("FINANCIAL",definition.ProcessType);
        Assert.Equal(new[]{1,2,3},definition.Steps.Select(step=>step.SequenceNumber));Assert.Equal(new[]{"APPROVAL","ACTION","ACTION"},definition.Steps.Select(step=>step.StepType));
        Assert.Equal(new[]{TheTechIdea.Data.OilGas.CompanyAccountingSourceTypes.Approval("JOURNAL_ENTRY"),TheTechIdea.Data.OilGas.CompanyAccountingSourceTypes.JournalPosting,TheTechIdea.Data.OilGas.CompanyAccountingSourceTypes.RejectionReview("JOURNAL_ENTRY")},definition.Steps.Select(step=>Assert.Single(step.StepConfiguration).Value));
        Assert.True(definition.Steps[0].RequiresApproval);Assert.All(definition.Steps.Skip(1),step=>Assert.False(step.RequiresApproval));Assert.All(definition.Steps,step=>Assert.Equal("Accountant",Assert.Single(step.RequiredRoles)));
        definition.Steps[0].RequiredRoles.Clear();Assert.Single(second.Steps[0].RequiredRoles);
    }
    [Fact] public void CanonicalMatchingStepIsAssignedIndependentReviewWithoutAssumedToleranceAcceptance()
    {
        var definition=ProcessDefinitionInitializer.BuildProcureToPayDefinition();
        Assert.Equal("ACCT_PROCURE_TO_PAY",definition.ProcessId);Assert.Equal("FINANCIAL",definition.ProcessType);Assert.Equal("PURCHASE_ORDER",definition.EntityType);
        var match=Assert.Single(definition.Steps,row=>row.StepId=="THREE_WAY_MATCH");
        Assert.Equal("REVIEW",match.StepType);Assert.True(match.RequiresApproval);Assert.True(match.IsRequired);
        Assert.Equal("Accountant",Assert.Single(match.RequiredRoles));Assert.Equal("INVOICE_APPROVE",match.NextStepId);
        Assert.Contains("reviewed company matching policy",match.Description);Assert.Contains("invoice preparer",match.Description);
        Assert.DoesNotContain("5%",JsonSerializer.Serialize(definition));Assert.DoesNotContain("2%",JsonSerializer.Serialize(definition));
        Assert.True(ProcessStepRoleChecks.RequiresIndependentActor(new(){StepId=match.StepId},definition,match));
    }
    [Theory]
    [InlineData("Accountant","initiator","initiator",false)]
    [InlineData("Administrator","initiator","initiator",false)]
    [InlineData("Accountant","reviewer","initiator",true)]
    [InlineData("Administrator","reviewer","initiator",true)]
    [InlineData("Manager","reviewer","initiator",false)]
    [InlineData("Accountant","reviewer","",false)]
    public void SeededFinancialReviewUsesActualStandardRoleAndRecordedInitiatorGate(string role,string actor,string initiator,bool allowed)
    {
        var definition=ProcessDefinitionInitializer.BuildProcureToPayDefinition();var template=definition.Steps.Single(row=>row.StepId=="THREE_WAY_MATCH");
        var user=new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Role,role)],"test"));
        var instance=new ProcessInstance{ProcessId=definition.ProcessId,CurrentStepId=template.StepId,StartedBy=initiator};
        var step=new ProcessStepInstance{StepId=template.StepId,RequiredRole=template.RequiredRoles[0],ApprovalRequired=template.RequiresApproval};
        Assert.Equal(allowed,ProcessStepRoleChecks.CanAct(user,actor,instance,step,definition,template));
    }
    [Fact] public void FactoryKeepsTheExistingPreparationApprovalPaymentChainAndReturnsIndependentMutableDefinitions()
    {
        var first=ProcessDefinitionInitializer.BuildProcureToPayDefinition();var second=ProcessDefinitionInitializer.BuildProcureToPayDefinition();
        Assert.Equal(Enumerable.Range(1,11),first.Steps.Select(row=>row.SequenceNumber));
        Assert.Equal(first.Steps.Count,first.Steps.Select(row=>row.StepId).Distinct().Count());
        for(var index=0;index<first.Steps.Count-1;index++)Assert.Equal(first.Steps[index+1].StepId,first.Steps[index].NextStepId);
        Assert.Equal("PAYMENT_EXECUTE",first.Steps[^1].StepId);Assert.Equal("Manager",Assert.Single(first.Steps.Single(row=>row.StepId=="INVOICE_APPROVE").RequiredRoles));
        first.Steps.Single(row=>row.StepId=="THREE_WAY_MATCH").RequiredRoles.Clear();first.Configuration.Clear();
        Assert.Equal("Accountant",Assert.Single(second.Steps.Single(row=>row.StepId=="THREE_WAY_MATCH").RequiredRoles));Assert.NotEmpty(second.Configuration);
    }
#if ACCOUNTING_WORKFLOW_SEED_FOCUSED
    [Fact] public async Task ActualOperationalInitializerUsesOneCanonicalDefinitionAndPreservesTheCallingActor()
    {
        // The actual seed file runs against an explicit test-only persistence boundary; no live definition is created.
        var initializer=new ProcessDefinitionInitializer();await initializer.InitializeOperationalAccountingWorkflowsAsync("recorded-admin");
        Assert.Equal(11,initializer.RecordedDefinitions.Count);
        var call=Assert.Single(initializer.RecordedDefinitions,row=>row.Definition.ProcessId=="ACCT_PROCURE_TO_PAY");
        Assert.Equal("recorded-admin",call.Actor);Assert.Equal("REVIEW",call.Definition.Steps.Single(row=>row.StepId=="THREE_WAY_MATCH").StepType);
        Assert.All(initializer.RecordedDefinitions,row=>Assert.Equal("recorded-admin",row.Actor));
        Assert.Single(initializer.RecordedDefinitions,row=>row.Definition.ProcessId=="ACCT_SALES_JOURNAL_REVIEW_POST");
    }
#endif
}

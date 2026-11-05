using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.LifeCycle.Models;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Processes;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class WorkflowTaskProcessAdmissionTests
{
    [Theory][InlineData("type")][InlineData("retag-prefix")][InlineData("retag-payload")]
    public void UnownedCompletionCannotBecomeGenericCompanyOrTaskAuthority(string marker)
    {
        var row=Row("FIELD",marker=="type"?"DRILLING_DAILY_REPORT":"WELL");row.Status="PENDING";
        if(marker=="retag-prefix"){row.WorkflowName=DrillingCompletionReviewStart.InstancePrefix+"unqualified";row.Route=WorkflowTaskRoute.For("FIELD",row.WorkflowName,row.EntityType);}
        var process=Process(row);
        if(marker=="retag-payload")process.ProcessData=new(){Data=new(){[DrillingCompletionReviewStart.PayloadKey]=new{unqualified=true}}};
        Assert.Throws<InvalidOperationException>(()=>WorkflowTaskProcessAdmission.CompanyMatches(process,new("tenant","company")));
        Assert.False(WorkflowTaskProcessAdmission.Matches(row,process,"actor"));
        if(marker!="retag-payload")Assert.Throws<InvalidOperationException>(()=>WorkflowInboxReview.RequireTask(row,["Accountant"]));
    }
    [Theory][InlineData("AFE")][InlineData("WELL")]
    public void InvestmentCompanyHeaderIsCheckedBeforePrivatePayloadProjection(string type)
    {
        var foreign=new CompanyScope("tenant","foreign");
        var process=new ProcessInstance{InstanceId=DevelopmentInvestmentHandoff.InstancePrefix+"private",EntityType=type,ProcessData=new(){Data=new(){
            [DevelopmentInvestmentHandoff.PayloadKey]=new{Review=new{Source=new{Current=new{Source=new{Review=new{Scope=new{Company=foreign}}}}}}}
        }}};
        Assert.False(WorkflowTaskProcessAdmission.CompanyMatches(process,new("tenant","company")));
    }
    [Fact]public void MissingInvestmentOwnershipCannotBecomeGenericFieldAuthority()
    {
        var process=new ProcessInstance{InstanceId=DevelopmentInvestmentHandoff.InstancePrefix+"missing",EntityType="AFE",ProcessData=new(){Data=new()}};
        Assert.Throws<InvalidOperationException>(()=>WorkflowTaskProcessAdmission.CompanyMatches(process,new("tenant","company")));
    }
    private static UnifiedTask Row(string scope = "COMPANY_ACCOUNTING", string type = "AP_INVOICE") => new()
    { TaskId = "task", WorkflowName = "process/Ù…ÙŠØ²Ø§Ù†ÙŠØ©%2F", StepName = "step", ScopeKind = scope, AssignedRole = "Accountant",
        EntityType = type, EntityId = "source", Route = WorkflowTaskRoute.For(scope, "process/Ù…ÙŠØ²Ø§Ù†ÙŠØ©%2F", type) };
    private static ProcessInstance Process(UnifiedTask row) => new()
    { InstanceId = row.WorkflowName, EntityId = row.EntityId, EntityType = row.EntityType, FieldId = row.ScopeKind == "FIELD" ? "field" : "",
        Status = ProcessStatus.IN_PROGRESS, CurrentStepId = "review", StepInstances = [new() { InstanceId = row.WorkflowName, StepInstanceId = "step", StepId = "review", RequiredRole = "Accountant" }] };
    [Theory][InlineData("COMPANY_ACCOUNTING", "AP_INVOICE")][InlineData("COMPANY_ACCOUNTING", "PURCHASE_ORDER")][InlineData("FIELD", "WELL")]
    public void ExactOwnedTaskStepCanBeLinked(string scope, string type)
    { var row = Row(scope, type); Assert.True(WorkflowTaskProcessAdmission.Matches(row, Process(row), "actor")); Assert.Contains("%2F", row.Route); }
    [Theory][InlineData("source")][InlineData("process")][InlineData("parent")][InlineData("duplicate")][InlineData("completed")]
    [InlineData("role")][InlineData("assigned")][InlineData("current")][InlineData("scope")][InlineData("route")][InlineData("cancelled")]
    public void ForeignStaleOrUnassignedCompanyWorkIsRefused(string fault)
    {
        var row = Row(); var process = Process(row); var step = process.StepInstances[0];
        switch (fault)
        {
            case "source": process.EntityId = "foreign"; break;
            case "process": process.InstanceId = "foreign"; break;
            case "parent": step.InstanceId = "foreign"; break;
            case "duplicate": process.StepInstances.Add(step); break;
            case "completed": step.Status = StepStatus.COMPLETED; break;
            case "role": step.RequiredRole = "Buyer"; break;
            case "assigned": step.AssignedTo = "other actor"; break;
            case "current": process.CurrentStepId = "later"; break;
            case "scope": process.FieldId = "field"; break;
            case "route": row.Route = "https://foreign.example"; break;
            case "cancelled": process.Status = ProcessStatus.CANCELLED; break;
        }
        Assert.False(WorkflowTaskProcessAdmission.Matches(row, process, "actor"));
    }
    [Theory][InlineData("actor")][InlineData("Accountant")]
    public void ExplicitActorOrRoleAssignmentIsPreserved(string assignee)
    { var row = Row(); var process = Process(row); process.StepInstances[0].AssignedTo = assignee; Assert.True(WorkflowTaskProcessAdmission.Matches(row, process, "actor")); }
    [Theory][InlineData("role")][InlineData("assigned")][InlineData("current")][InlineData("cancelled")]
    public void FieldTaskAlsoRequiresCurrentActiveProcessAndAssignedRole(string fault)
    {
        var row=Row("FIELD","WORK_ORDER");var process=Process(row);
        if(fault=="role")process.StepInstances[0].RequiredRole="foreign-role";
        if(fault=="assigned")process.StepInstances[0].AssignedTo="foreign-actor";
        if(fault=="current")process.CurrentStepId="another-step";
        if(fault=="cancelled")process.Status=ProcessStatus.CANCELLED;
        Assert.False(WorkflowTaskProcessAdmission.Matches(row,process,"actor"));
    }
}

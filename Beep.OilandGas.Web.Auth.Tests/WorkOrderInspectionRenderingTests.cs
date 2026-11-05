using Beep.OilandGas.ProductionOperations.Models;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderInspectionRenderingTests
{
    public class WriteAccessFixture:System.Reflection.DispatchProxy
    {
        public bool Allowed {get;set;}
        public bool Failure {get;set;}
        protected override object? Invoke(System.Reflection.MethodInfo? method,object?[]? args)
        {
            Assert.Equal("CheckAssetAccessAsync",method!.Name);Assert.Equal("technician",args![0]);Assert.Equal("WRITE",args[5]);
            Assert.Equal(args[2]!.ToString()=="FIELD"?"field":"order",args[1]);
            return Failure?Task.FromException<bool>(new InvalidOperationException("Unavailable")):Task.FromResult(Allowed);
        }
    }
    private sealed class NoBrowser:IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>throw new InvalidOperationException("No browser in this static rendering check.");
        public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>InvokeAsync<T>(id,args);
    }
    private sealed class ReadFixture(List<InspectionCondition> rows,bool failure):IWorkOrderServiceClient
    {
        public Task<Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderFinancialSources> ReadFinancialSourcesAsync(Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderFinancialQuery query,CancellationToken token=default)=>throw new NotSupportedException();
        public Task<Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderMaterialCostReview> ReviewMaterialCostAsync(Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderMaterialCostQuery query,CancellationToken token=default)=>throw new NotSupportedException();
        public Task<Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderMaterialCostRecovery> RecordMaterialCostAsync(Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderMaterialCostCommand command,CancellationToken token=default)=>throw new NotSupportedException();
        public Task<Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderMaterialCostRecovery> RecoverMaterialCostAsync(Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderMaterialCostReference reference,CancellationToken token=default)=>throw new NotSupportedException();
        public Task<Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderActualCostReview> ReadActualCostsAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionQuery query,CancellationToken token=default)=>throw new NotSupportedException("No actual-cost reads in the inspection fixture.");
        public Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderLaborCostReview> ReviewLaborCostAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderLaborCostQuery query,CancellationToken token=default)=>throw new NotSupportedException("No cost source in the inspection fixture.");
        public Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderLaborCostRecovery> RecordLaborCostAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderLaborCostCommand command,CancellationToken token=default)=>throw new NotSupportedException("No cost command in the inspection fixture.");
        public Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderLaborCostRecovery> RecoverLaborCostAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderLaborCostReference reference,CancellationToken token=default)=>throw new NotSupportedException("No cost recovery in the inspection fixture.");
        public Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionStatus> ReadBudgetDecisionStatusAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionQuery query,CancellationToken token=default)=>throw new NotSupportedException("No financial status in the inspection rendering fixture.");
        public Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionReview> ReviewBudgetDecisionAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionQuery query,CancellationToken token=default)=>throw new NotSupportedException("No budget decision in the inspection fixture.");
        public Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionRecovery> RecordBudgetDecisionAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionCommand command,CancellationToken token=default)=>throw new NotSupportedException("No budget decision in the inspection fixture.");
        public Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionRecovery> RecoverBudgetDecisionAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionReference reference,CancellationToken token=default)=>throw new NotSupportedException("No budget decision in the inspection fixture.");
        public Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetReview> ReviewBudgetAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetQuery query,CancellationToken token=default)=>throw new NotSupportedException("No budget source in the inspection rendering fixture.");
        public Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetRecovery> CreateBudgetAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetCommand command,CancellationToken token=default)=>throw new NotSupportedException("No budget command in the inspection rendering fixture.");
        public Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetRecovery> RecoverBudgetAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetReference reference,CancellationToken token=default)=>throw new NotSupportedException("No budget recovery in the inspection rendering fixture.");
        public Task<List<InspectionCondition>> GetChecklistAsync(string id)=>failure?Task.FromException<List<InspectionCondition>>(new InvalidOperationException("Read refused")):Task.FromResult(rows);
        public Task<InspectionCondition> AddInspectionRequirementAsync(string id,AddWorkOrderInspectionRequirementRequest request)=>throw new NotSupportedException("No HTTP command in static rendering.");
        public Task<InspectionCondition> RecordConditionAsync(string id,int seq,RecordInspectionResultRequest request)=>throw new NotSupportedException("No HTTP command in static rendering.");
        public Task<List<WorkOrderSummary>> GetWorkOrdersAsync(string? state=null,string? woSubType=null)=>throw new NotSupportedException();
        public Task<WorkOrderSummary?> CreateWorkOrderAsync(CreateWorkOrderRequest request)=>throw new NotSupportedException();
        public Task<WorkOrderDetailModel?> GetWorkOrderAsync(string id)=>throw new NotSupportedException();
        public Task<Beep.OilandGas.ProductionOperations.Models.WorkOrderProcessReview> GetProcessesAsync(string id)=>throw new NotSupportedException();
        public Task<List<string>> GetTransitionsAsync(string id)=>throw new NotSupportedException();
        public Task<WorkOrderSummary?> TransitionAsync(string id,TransitionWorkOrderRequest request)=>throw new NotSupportedException();
        public Task<List<ContractorAssignment>> GetContractorsAsync(string id)=>throw new NotSupportedException();
        public Task<List<CalendarSlot>> GetCalendarAsync(DateTime from,DateTime to)=>throw new NotSupportedException();
    }
    private static WORK_ORDER_INSPECTION Row()=>WorkOrderInspectionRules.PrepareRequirement(new(){FieldId="field",CondSeq=1,CondType=ConditionType.PreStart,RequiredText="Actual isolation requirement",Jurisdiction="Recorded authority"},"order","planner","inspection",DateTime.UtcNow);
    private static async Task<string> Render(List<InspectionCondition> rows,bool failure=false,bool write=true,bool accessFailure=false)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"technician",new(new("tenant","company"),[],[])));using var review=new WorkOrderInspectionState(company);
        var services=new ServiceCollection();services.AddLogging();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddMudServices();services.AddSingleton<IWorkOrderServiceClient>(new ReadFixture(rows,failure));
        var access=System.Reflection.DispatchProxy.Create<Beep.OilandGas.Client.App.Services.AccessControl.IAccessControlService,WriteAccessFixture>();
        ((WriteAccessFixture)(object)access).Allowed=write;((WriteAccessFixture)(object)access).Failure=accessFailure;
        services.AddSingleton(access);
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async()=>{var root=await renderer.RenderComponentAsync<WorkOrderInspectionRenderHost>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Source",new WorkOrderDetailModel{InstanceId="order",FieldId="field"}},{"Review",review}}));return root.ToHtmlString();});
    }
    [Fact] public async Task EmptyReadDoesNotReportPassing(){var html=await Render([]);Assert.Contains("No recorded inspection requirements",html);Assert.Contains("does not confirm",html);Assert.Contains("FIELD field",html);Assert.Contains("Order order",html);}
    [Fact] public async Task FailedReadDoesNotBecomeSuccessfulEmptyChecklist(){var html=await Render([],true);Assert.Contains("Inspection evidence is unavailable",html);Assert.DoesNotContain("No recorded inspection requirements",html);Assert.DoesNotContain("Save requirement",html);}
    [Fact] public async Task UnrecordedRequirementShowsActualMakerAndJurisdiction(){var html=await Render([WorkOrderInspectionRules.Project(Row())]);Assert.Contains("planner",html);Assert.Contains("Recorded authority",html);Assert.Contains("UNRECORDED",html);Assert.Contains("Record result",html);Assert.DoesNotContain("Recorded UTC:",html);}
    [Fact] public async Task ActualResultShowsInspectorRationaleAndNoOverwriteAction(){var row=Row();var result=WorkOrderInspectionRules.PrepareResult(row,new(){FieldId="field",InspectionId="inspection",ExpectedSourceRevision=WorkOrderInspectionRules.Revision(row),Result="FAIL",Notes="Actual failed isolation check"},"technician",DateTime.UtcNow);var html=await Render([WorkOrderInspectionRules.Project(result)]);Assert.Contains("Actual failed isolation check",html);Assert.Contains("technician",html);Assert.Contains("Recorded UTC:",html);Assert.DoesNotContain(">Record result<",html);}
    [Fact] public async Task RequirementTextIsHtmlEncoded(){var row=Row();row.REQUIRED_TEXT="<script>alert(1)</script>";var html=await Render([WorkOrderInspectionRules.Project(row)]);Assert.Contains("&lt;script&gt;",html);Assert.DoesNotContain("<script>",html);}
    [Fact] public async Task ProgressRemainsVisibleToReaderAndDoesNotGrantCompletion()
    {
        var row=Row();row.COND_TYPE=ConditionType.CloseOut;
        var result=WorkOrderInspectionRules.PrepareResult(row,new(){FieldId="field",InspectionId="inspection",ExpectedSourceRevision=WorkOrderInspectionRules.Revision(row),Result="NA",Notes="Recorded exclusion rationale"},"technician",DateTime.UtcNow);
        var html=await Render([WorkOrderInspectionRules.Project(result)],write:false);
        Assert.Contains("Inspection progress by recorded category",html);Assert.Contains("Not applicable",html);Assert.Contains("Recorded exclusion rationale",html);
        Assert.Contains("independent supervisor acceptance",html);Assert.Contains("do not establish work-order completion",html);
        Assert.DoesNotContain("Save requirement",html);Assert.Contains("scope=\"col\"",html);Assert.Contains("scope=\"row\"",html);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task DeniedOrFailedWriteReviewPreservesEvidenceWithoutMutationControls(bool failure)
    {
        var html=await Render([WorkOrderInspectionRules.Project(Row())],write:false,accessFailure:failure);
        Assert.Contains("Actual isolation requirement",html);Assert.Contains("UNRECORDED",html);
        Assert.DoesNotContain("Record result",html);Assert.DoesNotContain("Save requirement",html);Assert.DoesNotContain("Recorded UTC:",html);
        Assert.Contains(failure?"write access could not be confirmed":"Current WRITE access",html);
    }
}

using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
namespace HseBrowser;
public static class Host
{
    public static async Task Main(string[] args)
    {
        var builder=WebApplication.CreateBuilder(args); builder.WebHost.UseStaticWebAssets(); builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddRazorComponents().AddOilGasInteractiveServerComponents();builder.Services.AddMudServices();
        builder.Services.AddSingleton<Backend>();builder.Services.AddScoped<Scope>();
        builder.Services.AddScoped(p=>p.GetRequiredService<Scope>().Company);
        builder.Services.AddScoped(p=>p.GetRequiredService<Scope>().Fields.Object);
        builder.Services.AddScoped(p=>p.GetRequiredService<Scope>().Client.Object);
        builder.Services.AddScoped(p=>p.GetRequiredService<Scope>().Personas.Object);
        builder.Services.AddScoped(p=>p.GetRequiredService<Scope>().Inbox.Object);
        builder.Services.AddScoped(p=>p.GetRequiredService<Scope>().Recorded.Object);
        builder.Services.AddScoped(p=>p.GetRequiredService<Scope>().Processes.Object);
        builder.Services.AddScoped<FieldSelectionState>(p=>new(p.GetRequiredService<CompanyClientContext>(),p.GetRequiredService<IFieldSelectionContext>(),p.GetRequiredService<OilGasCallFailures>().Explain));
        builder.Services.AddScoped<IncidentReportIntentStore>();
        builder.Services.AddScoped<IIncidentReportIntentStore>(p=>p.GetRequiredService<IncidentReportIntentStore>());
        builder.Services.AddScoped<ICompanyHseWorkflowIntentStore>(p=>p.GetRequiredService<IncidentReportIntentStore>());
        builder.Services.AddScoped<IIncidentRcaIntentStore>(p=>p.GetRequiredService<IncidentReportIntentStore>());
        builder.Services.AddScoped<IIncidentRcaHandoffIntentStore>(p=>p.GetRequiredService<IncidentReportIntentStore>());
        builder.Services.AddScoped<AuthenticationStateProvider,Auth>();builder.Services.AddScoped<ProtectedSessionStorage>();
        builder.Services.AddScoped<ICorrectiveActionIntentStore,CorrectiveActionIntentStore>();
        builder.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        var app=builder.Build();app.UseAntiforgery();app.MapStaticAssets();app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
        app.MapGet("/fixture/metadata",(Backend b)=>new{b.State,b.Attempts,b.Writes,b.SourceReads,b.IncidentReads,b.GenericActionReads,Recorded=b.Outcome is not null});
        await app.RunAsync();
    }
}
public sealed class Auth:AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id","actor"),new Claim(ClaimTypes.Role,"Safety")],"controlled-browser"))));
}
public sealed class Backend
{
    public string State=IncidentState.PendingCloseOut;public int Attempts,Writes,SourceReads,IncidentReads,GenericActionReads;public CompanyHseClosureOutcome? Outcome;
    public CompanyHseClosureOutcome Complete(CompanyHseClosureCommand original)
    {
        Attempts++;
        if(Outcome is null)
        {
            var r=original.Review;var receipt=new CompanyHseClosureReceipt(original.IntentId,r.Context.Company,"actor",r.InstanceId,r.StepInstanceId,r.Stage,"history","approval",null,["task"],[],new string('D',64));
            Outcome=new(original,receipt,new(original,receipt,4));Writes++;State=IncidentState.Closed;
            throw new HttpRequestException("Controlled lost acknowledgement after one retained effect");
        }
        if(JsonSerializer.Serialize(Outcome.Original)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("A different original cannot adopt recorded closure.");
        return Outcome;
    }
}
public sealed class Scope:IDisposable
{
    public CompanyClientContext Company{get;}=new();
    public FieldSelectionReview Context{get;}=new(new("tenant","company"),new("core","binding"),"actor","revision","field");
    public Mock<IFieldSelectionContext> Fields{get;}=new();public Mock<IHSEServiceClient> Client{get;}=new();
    public Mock<IPersonaContextService> Personas{get;}=new();public Mock<IUnifiedTaskInboxService> Inbox{get;}=new();
    public string WorkflowId {get;}="hse-process-assigned-original";
    public string IncidentId {get;}="case/شركة?one#2";
    public Mock<IRecordedProcessClient> Recorded{get;}=new();
    public Mock<IBusinessProcessServiceClient> Processes{get;}=new();
    public Scope(Backend backend)
    {
        Restore();Fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(()=>new FieldResponse{FieldId="field",FieldName="North FIELD",SelectionAvailable=true,SelectionContext=Context});
        Personas.Setup(x=>x.EnsureLoadedAsync()).Returns(Task.CompletedTask);
        Fields.Setup(x=>x.GetCurrentFieldIdAsync()).ReturnsAsync("field");
        Fields.Setup(x=>x.GetAvailableFieldsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([new FieldListItem{FieldId="field",FieldName="North FIELD"}]);
        ProcessInstance Instance()=>new(){InstanceId=WorkflowId,EntityId=IncidentId,EntityType=CompanyHseWorkflowStart.EntityType,ProcessId=CompanyHseWorkflowStart.ProcessId,FieldId="field",Status=backend.State==IncidentState.Closed?ProcessStatus.COMPLETED:ProcessStatus.IN_PROGRESS,CurrentStepId="INC_CLOSURE",CurrentState=backend.State,StartedBy="originator",StartDate=DateTime.UtcNow.AddDays(-1),StepInstances=[new(){StepInstanceId="step-instance",StepId="INC_CLOSURE",StepName="Final incident closure",RequiredRole="Safety",AssignedTo="actor"}]};
        Recorded.Setup(x=>x.GetInstanceAsync(WorkflowId,It.IsAny<CancellationToken>())).ReturnsAsync(()=>{backend.SourceReads++;return Company.Selection?.Scope==Context.Company?Instance():null;});
        Recorded.Setup(x=>x.GetHistoryAsync(WorkflowId,It.IsAny<CancellationToken>())).ReturnsAsync([]);
        Recorded.Setup(x=>x.GetTransitionReviewAsync(It.IsAny<string>())).Returns((string _)=>{backend.GenericActionReads++;throw new InvalidOperationException("Generic HSE action transport must not run.");});
        Processes.Setup(x=>x.GetHseIncidentAsync(IncidentId,It.IsAny<CancellationToken>())).ReturnsAsync(()=>Instance());
        Client.Setup(x=>x.GetIncidentAsync(IncidentId,It.IsAny<CancellationToken>())).ReturnsAsync(()=>{backend.IncidentReads++;return new HSEIncidentRecord{IncidentId=IncidentId,FieldId="field",Description="Recorded assigned incident",IncidentType="INJURY",Location="North FIELD",Jurisdiction="KUWAIT",IncidentDate=DateTime.UtcNow.AddDays(-1),CurrentState=backend.State};});
        Client.Setup(x=>x.GetIncidentCausesAsync(IncidentId,It.IsAny<CancellationToken>())).ReturnsAsync([]);
        Client.Setup(x=>x.GetIncidentCorrectiveActionsAsync(IncidentId,It.IsAny<CancellationToken>())).ReturnsAsync([new CAStatus(1,"Inspect valve","CORRECTIVE","COMPLETED",DateTime.UtcNow.AddDays(1),false,"worker",DateTime.UtcNow.AddHours(-1))]);

        var rows=Enumerable.Range(1,17).Select(i=>new HSEIncidentRecord{IncidentId=$"open-{i:00}",FieldId="field",CurrentState=IncidentState.Reported}).ToList();
        rows.Add(new(){IncidentId="closed/one?شركة",FieldId="field",CurrentState=IncidentState.Closed});rows.Add(new(){IncidentId="cancelled",FieldId="field",CurrentState=IncidentState.Cancelled});rows.Add(new(){IncidentId="unknown",FieldId="field",CurrentState="UNRECOGNIZED"});rows.Add(new(){IncidentId="unrecorded",FieldId="field",CurrentState=""});
        Client.Setup(x=>x.GetIncidentsAsync(null,null,It.IsAny<CancellationToken>())).ReturnsAsync(rows);
        var actions=new CAVerificationReview(Context,IncidentId,"plan",new string('A',64),[new(1,"step","Inspect valve","COMPLETED","worker",DateTime.UtcNow.AddDays(1),DateTime.UtcNow.AddHours(-1),"other","Tested valve")]);
        var review=new CompanyHseClosureReview(new(actions,[new(Guid.NewGuid(),actions,"VERIFIED","Independent inspection confirmed the repair.")]),new("lifecycle","binding"),WorkflowId,"step-instance","INC_CLOSURE","Safety",new string('B',64),new string('C',64));
        Client.Setup(x=>x.ReviewClosureStageAsync(Context,IncidentId,It.IsAny<CancellationToken>())).ReturnsAsync(review);
        Client.Setup(x=>x.CompleteClosureStageAsync(It.IsAny<CompanyHseClosureCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyHseClosureCommand original,CancellationToken _)=>Task.FromResult(backend.Complete(original)));
        Client.Setup(x=>x.RecoverClosureStageAsync(Context,IncidentId,It.IsAny<CancellationToken>())).ReturnsAsync(()=>backend.Outcome is {} o?new(o.Original,o.Lifecycle):null);
    }
    public void Other()=>Company.TrySelect(Company.Clear(),"actor",new(new("tenant","other"),["Safety"],["HSE.View","HSE.ManageIncidents","Workflow.Approve"]));
    public void Restore()=>Company.TrySelect(Company.Clear(),"actor",new(Context.Company,["Safety"],["HSE.View","HSE.ManageIncidents","Workflow.Approve"]));
    public void Dispose()=>Company.Dispose();
}

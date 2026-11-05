using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Net;
using System.Net.Http.Json;
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
        builder.Services.AddAuthentication("controlled-browser").AddScheme<AuthenticationSchemeOptions,FixtureAuth>("controlled-browser",_=>{});
        builder.Services.AddAuthorization();
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
        app.MapPost("/fixture/release-queue-a", (Backend b) => {b.ReleaseQueueA.TrySetResult(true);return Results.Ok();});
        app.MapGet("/fixture/metadata", (Backend b) => b.Snapshot());
        app.MapPost("/fixture/release-company-a", (Backend b) => { b.ReleaseA.TrySetResult(true); return Results.Ok(); });
        await app.RunAsync();
    }
}
public sealed class Auth:AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id","actor"),new Claim(ClaimTypes.Role,"Safety")],"controlled-browser"))));
}
public sealed class CompanyCase(string code)
{
    public string Code { get; } = code;
    public string State = IncidentState.PendingCloseOut;
    public int ApprovalAttempts, Approvals, NativeAttempts, NativeWrites, IncidentReads, ReviewReads, QueueReads;
    public CompanyHseClosureRecovery? Approved;
    public CompanyHseClosureOutcome? Outcome;
    public object Snapshot() => new { Code, State, ApprovalAttempts, Approvals, NativeAttempts, NativeWrites, IncidentReads, ReviewReads, QueueReads, ApprovedIntent = Approved?.Original.IntentId };
}
public sealed class Backend
{
    public const string IncidentId = "case/شركة?one#2";
    public const string WorkflowId = "hse-process-assigned-original";
    public CompanyCase A { get; } = new("company");
    public CompanyCase B { get; } = new("other");
    public TaskCompletionSource<bool> ReleaseA { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly List<object> requests = [];
    public int GenericActionReads, CompletedCalls, QueueCompleted;
    public TaskCompletionSource<bool> ReleaseQueueA {get;}=new(TaskCreationOptions.RunContinuationsAsynchronously);
    public CompanyCase Case(string company) => company == "company" ? A : company == "other" ? B : throw new UnauthorizedAccessException("Unknown fixture company");
    public static FieldSelectionReview Context(string company) => new(new(company == "company" ? "tenant" : "tenant-other", company), new(company == "company" ? "core" : "core-other", company == "company" ? "binding" : "binding-other"), "actor", "revision", "field");
    public static string Description(string company) => company == "company" ? "PRIVATE COMPANY A INCIDENT" : "PRIVATE COMPANY B INCIDENT";
    public static CompanyHseClosureReview Review(string company)
    {
        var context = Context(company);
        var action = new CAVerificationAction(1,"step","Completed repair for " + company,"COMPLETED","worker",DateTime.UtcNow.AddDays(1),DateTime.UtcNow.AddHours(-1),"other","Recorded inspection");
        var actions = new CAVerificationReview(context, IncidentId,"plan",new string('A',64),[action]);
        return new(new(actions,[new(Guid.NewGuid(),actions,"VERIFIED","Independent inspection confirmed the repair for " + company)]),new(company == "company" ? "lifecycle" : "lifecycle-other", "life-binding-" + company),WorkflowId,"step-instance","INC_CLOSURE","Safety",new string('B',64),new string('C',64));
    }
    public object Snapshot() { lock(requests) return new { A=A.Snapshot(), B=B.Snapshot(), GenericActionReads, CompletedCalls, QueueCompleted, Requests=requests.ToArray() }; }
    public async Task<HttpResponseMessage> Send(HttpRequestMessage request)
    {
        var tenant = request.Headers.GetValues(CompanyRequestHeaders.Tenant).Single();
        var company = request.Headers.GetValues(CompanyRequestHeaders.Company).Single();
        if (tenant != (company == "company" ? "tenant" : "tenant-other")) throw new UnauthorizedAccessException("Tenant differs from captured company headers");
        var data = Case(company); var path = request.RequestUri!.AbsolutePath;
        lock(requests) requests.Add(new { Company=company, Path=path, Method=request.Method.Method });
        if(request.Method == HttpMethod.Get)
        {
            if(path == "/api/field/current/hse/assigned-overdue-actions")
            {
                data.QueueReads++; var context=Context(company);var count=company=="company"?11:1;var now=DateTime.UtcNow;
                var result=new CompanyHseAssignedActions(context,now,1,Enumerable.Range(1,count).Select(i=>new CompanyHseAssignedAction(IncidentId,i,"PRIVATE "+(company=="company"?"A":"B")+" action "+i,now.AddDays(-1).AddMinutes(i))).ToArray());result.Require();
                if(company=="company" && data.QueueReads==2)await ReleaseQueueA.Task;
                return new(HttpStatusCode.OK){Content=JsonContent.Create(result)};
            }
            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(request.RequestUri.Query);
            if(!query.TryGetValue("incidentId",out var id) || id.ToString()!=IncidentId) throw new InvalidOperationException("Exact fixture incident required");
            if(path == "/api/field/current/hse/incident")
            {
                data.IncidentReads++;
                return new(HttpStatusCode.OK){Content=JsonContent.Create(new HSEIncidentRecord{IncidentId=IncidentId,FieldId="field",Description=Description(company),IncidentType="INJURY",Location="FIELD " + company,Jurisdiction="Recorded " + company,IncidentDate=DateTime.UtcNow.AddDays(-1),CurrentState=data.State})};
            }
            if(path == "/api/field/current/hse/incident/causes") return new(HttpStatusCode.OK){Content=JsonContent.Create(new List<CauseFinding>())};
            if(path == "/api/field/current/hse/incident/cas") return new(HttpStatusCode.OK){Content=JsonContent.Create(Enumerable.Range(1,company=="company"?11:1).Select(i=>new CAStatus(i,"PRIVATE "+(company=="company"?"A":"B")+" action "+i,"CORRECTIVE","OPEN",DateTime.UtcNow.AddDays(-1).AddMinutes(i),true,"worker-"+company,null)).ToList())};
            if(path.EndsWith("/closure/review")){if(data.Approved is not null)throw new InvalidOperationException("Use the recorded original after final approval");data.ReviewReads++;return new(HttpStatusCode.OK){Content=JsonContent.Create(Review(company))};}
            if(path.EndsWith("/closure/recovery")) return data.Approved is null ? new(HttpStatusCode.NotFound) : new(HttpStatusCode.OK){Content=JsonContent.Create(data.Approved)};
            throw new InvalidOperationException("Unselected read " + path);
        }
        if(path.EndsWith("/closure/complete-stage"))
        {
            data.ApprovalAttempts++;
            var original=(await request.Content!.ReadFromJsonAsync<CompanyHseClosureCommand>())!; original.Require();
            if(original.Review.Context != Context(company)) throw new UnauthorizedAccessException("Original company/database differs from captured request headers");
            if(data.Approved is null)
            {
                var r=original.Review;var receipt=new CompanyHseClosureReceipt(original.IntentId,r.Context.Company,"actor",r.InstanceId,r.StepInstanceId,r.Stage,"history","approval",null,["task"],[],new string('D',64));
                data.Approved=new(original,receipt);data.Approved.Require();data.Approvals++;
                if(company=="company") await ReleaseA.Task; // Commit has happened; caller may switch companies before acknowledgement.
            }
            if(JsonSerializer.Serialize(data.Approved.Original)!=JsonSerializer.Serialize(original)) throw new InvalidOperationException("Another original cannot adopt approval");
            return new(HttpStatusCode.OK){Content=JsonContent.Create(data.Approved.Receipt)};
        }
        if(!path.EndsWith("/closure/record-native"))throw new InvalidOperationException("Unselected mutation " + path);
        data.NativeAttempts++;
        var approved=(await request.Content!.ReadFromJsonAsync<CompanyHseClosureRecovery>())!; approved.Require();
        if(approved.Original.Review.Context!=Context(company) || data.Approved is null || JsonSerializer.Serialize(data.Approved)!=JsonSerializer.Serialize(approved)) throw new UnauthorizedAccessException("Exact approval/header scope required");
        if(data.Outcome is null){data.Outcome=new(approved.Original,approved.Receipt,new(approved.Original,approved.Receipt,4));data.Outcome.Require();data.NativeWrites++;data.State=IncidentState.Closed;}
        return new(HttpStatusCode.OK){Content=JsonContent.Create(data.Outcome)};
    }
}
public sealed class Transport(Func<HttpRequestMessage,Task<HttpResponseMessage>> send):HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)=>send(request);
}
public sealed class Scope:IDisposable
{
    public CompanyClientContext Company {get;}=new();
    public FieldSelectionReview Context => Backend.Context(Company.Selection?.Scope.BusinessAssociateId ?? "company");
    public Mock<IFieldSelectionContext> Fields {get;}=new(); public Mock<IHSEServiceClient> Client {get;}=new();
    public Mock<IPersonaContextService> Personas {get;}=new();public Mock<IUnifiedTaskInboxService> Inbox {get;}=new();
    public string WorkflowId => Backend.WorkflowId; public string IncidentId => Backend.IncidentId;
    public Mock<IRecordedProcessClient> Recorded {get;}=new(); public Mock<IBusinessProcessServiceClient> Processes {get;}=new();
    public Scope(Backend backend)
    {
        Restore();Fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(()=>new FieldResponse{FieldId="field",FieldName="FIELD " + Context.Company.BusinessAssociateId,SelectionAvailable=true,SelectionContext=Context});
        Fields.Setup(x=>x.GetCurrentFieldIdAsync()).ReturnsAsync("field");Fields.Setup(x=>x.GetAvailableFieldsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([new FieldListItem{FieldId="field",FieldName="Recorded FIELD"}]);Personas.Setup(x=>x.EnsureLoadedAsync()).Returns(Task.CompletedTask);
        ProcessInstance Instance()=>new(){InstanceId=WorkflowId,EntityId=IncidentId,EntityType=CompanyHseWorkflowStart.EntityType,ProcessId=CompanyHseWorkflowStart.ProcessId,FieldId="field",Status=backend.Case(Context.Company.BusinessAssociateId).Approved is not null?ProcessStatus.COMPLETED:ProcessStatus.IN_PROGRESS,CurrentStepId="INC_CLOSURE",CurrentState=backend.Case(Context.Company.BusinessAssociateId).State,StartedBy="originator",StartDate=DateTime.UtcNow.AddDays(-1),StepInstances=[new(){StepInstanceId="step-instance",StepId="INC_CLOSURE",StepName="Final incident closure",RequiredRole="Safety",AssignedTo="actor"}]};
        Recorded.Setup(x=>x.GetInstanceAsync(WorkflowId,It.IsAny<CancellationToken>())).ReturnsAsync(()=>Instance());Recorded.Setup(x=>x.GetHistoryAsync(WorkflowId,It.IsAny<CancellationToken>())).ReturnsAsync([]);
        Recorded.Setup(x=>x.GetTransitionReviewAsync(It.IsAny<string>())).Returns((string _)=>{backend.GenericActionReads++;throw new InvalidOperationException("Generic HSE actions must not run");});
        Processes.Setup(x=>x.GetHseIncidentAsync(IncidentId,It.IsAny<CancellationToken>())).ReturnsAsync(()=>Instance());
        var actual = new HSEServiceClient(new ApiClient(new HttpClient(new Transport(backend.Send)){BaseAddress=new Uri("http://controlled-company-api")},Mock.Of<IFailureReporter>(),Company),Microsoft.Extensions.Logging.Abstractions.NullLogger<HSEServiceClient>.Instance,Mock.Of<Beep.OilandGas.Client.App.Services.Field.IFieldService>());
        Client.Setup(x=>x.GetAssignedOverdueActionsAsync(It.IsAny<FieldSelectionReview>(),It.IsAny<CancellationToken>())).Returns(async (FieldSelectionReview context,CancellationToken token)=>{try{return await actual.GetAssignedOverdueActionsAsync(context,token);}finally{Interlocked.Increment(ref backend.QueueCompleted);}});
        Client.Setup(x=>x.GetIncidentAsync(IncidentId,It.IsAny<CancellationToken>())).Returns((string id,CancellationToken token)=>actual.GetIncidentAsync(id,token));
        Client.Setup(x=>x.GetIncidentCausesAsync(IncidentId,It.IsAny<CancellationToken>())).Returns((string id,CancellationToken token)=>actual.GetIncidentCausesAsync(id,token));
        Client.Setup(x=>x.GetIncidentCorrectiveActionsAsync(IncidentId,It.IsAny<CancellationToken>())).Returns((string id,CancellationToken token)=>actual.GetIncidentCorrectiveActionsAsync(id,token));
        Client.Setup(x=>x.ReviewClosureStageAsync(It.IsAny<FieldSelectionReview>(),IncidentId,It.IsAny<CancellationToken>())).Returns((FieldSelectionReview context,string id,CancellationToken token)=>actual.ReviewClosureStageAsync(context,id,token));
        Client.Setup(x=>x.CompleteClosureStageAsync(It.IsAny<CompanyHseClosureCommand>(),It.IsAny<CancellationToken>())).Returns(async (CompanyHseClosureCommand original,CancellationToken token)=>{try{return await actual.CompleteClosureStageAsync(original,token);}finally{Interlocked.Increment(ref backend.CompletedCalls);}});
        Client.Setup(x=>x.RecoverClosureStageAsync(It.IsAny<FieldSelectionReview>(),IncidentId,It.IsAny<CancellationToken>())).Returns((FieldSelectionReview context,string id,CancellationToken token)=>actual.RecoverClosureStageAsync(context,id,token));
    }
    public void Other()=>Select("other");public void Restore()=>Select("company");
    private void Select(string code)=>Company.TrySelect(Company.Clear(),"actor",new(new(code == "company" ? "tenant" : "tenant-other",code),["Safety"],["HSE.View","HSE.ManageIncidents","Workflow.Approve"]));
    public void Dispose()=>Company.Dispose();
}

public sealed class FixtureAuth(IOptionsMonitor<AuthenticationSchemeOptions> options,ILoggerFactory logs,UrlEncoder encoder):AuthenticationHandler<AuthenticationSchemeOptions>(options,logs,encoder)
{
 protected override Task<AuthenticateResult> HandleAuthenticateAsync()=>Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id","actor"),new Claim(ClaimTypes.Role,"Safety")],"controlled-browser")),"controlled-browser")));
}

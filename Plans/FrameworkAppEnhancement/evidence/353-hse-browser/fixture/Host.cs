using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.HSE;
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
        builder.Services.AddScoped<AuthenticationStateProvider,Auth>();builder.Services.AddScoped<ProtectedSessionStorage>();
        builder.Services.AddScoped<ICorrectiveActionIntentStore,CorrectiveActionIntentStore>();
        builder.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        var app=builder.Build();app.UseAntiforgery();app.MapStaticAssets();app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
        app.MapGet("/fixture/metadata",(Backend b)=>new{b.State,b.Attempts,b.Writes,Recorded=b.Outcome is not null});
        await app.RunAsync();
    }
}
public sealed class Auth:AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id","actor"),new Claim(ClaimTypes.Role,"Safety")],"controlled-browser"))));
}
public sealed class Backend
{
    public string State=IncidentState.PendingCloseOut;public int Attempts,Writes;public CompanyHseClosureOutcome? Outcome;
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
    public Scope(Backend backend)
    {
        Restore();Fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(()=>new FieldResponse{FieldId="field",FieldName="North FIELD",SelectionAvailable=true,SelectionContext=Context});
        Personas.Setup(x=>x.EnsureLoadedAsync()).Returns(Task.CompletedTask);
        var rows=Enumerable.Range(1,17).Select(i=>new HSEIncidentRecord{IncidentId=$"open-{i:00}",FieldId="field",CurrentState=IncidentState.Reported}).ToList();
        rows.Add(new(){IncidentId="closed/one?شركة",FieldId="field",CurrentState=IncidentState.Closed});rows.Add(new(){IncidentId="cancelled",FieldId="field",CurrentState=IncidentState.Cancelled});rows.Add(new(){IncidentId="unknown",FieldId="field",CurrentState="UNRECOGNIZED"});rows.Add(new(){IncidentId="unrecorded",FieldId="field",CurrentState=""});
        Client.Setup(x=>x.GetIncidentsAsync(null,null,It.IsAny<CancellationToken>())).ReturnsAsync(rows);
        var actions=new CAVerificationReview(Context,"case","plan",new string('A',64),[new(1,"step","Inspect valve","COMPLETED","worker",DateTime.UtcNow.AddDays(1),DateTime.UtcNow.AddHours(-1),"other","Tested valve")]);
        var review=new CompanyHseClosureReview(new(actions,[new(Guid.NewGuid(),actions,"VERIFIED","Independent inspection confirmed the repair.")]),new("lifecycle","binding"),"instance","step-instance","INC_CLOSURE","Safety",new string('B',64),new string('C',64));
        Client.Setup(x=>x.ReviewClosureStageAsync(Context,"case",It.IsAny<CancellationToken>())).ReturnsAsync(review);
        Client.Setup(x=>x.CompleteClosureStageAsync(It.IsAny<CompanyHseClosureCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyHseClosureCommand original,CancellationToken _)=>Task.FromResult(backend.Complete(original)));
        Client.Setup(x=>x.RecoverClosureStageAsync(Context,"case",It.IsAny<CancellationToken>())).ReturnsAsync(()=>backend.Outcome is {} o?new(o.Original,o.Lifecycle):null);
    }
    public void Other()=>Company.TrySelect(Company.Clear(),"actor",new(new("tenant","other"),["Safety"],["HSE.View","HSE.ManageIncidents","Workflow.Approve"]));
    public void Restore()=>Company.TrySelect(Company.Clear(),"actor",new(Context.Company,["Safety"],["HSE.View","HSE.ManageIncidents","Workflow.Approve"]));
    public void Dispose()=>Company.Dispose();
}

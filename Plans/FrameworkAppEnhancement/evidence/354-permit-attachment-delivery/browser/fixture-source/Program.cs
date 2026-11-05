using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using PermitBrowser354;
using Beep.OilandGas.Models.Data.HSE;
using System.Security.Claims;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
var builder=WebApplication.CreateBuilder(args);builder.WebHost.UseStaticWebAssets();builder.WebHost.UseUrls("http://127.0.0.1:0");builder.Services.AddRazorComponents().AddInteractiveServerComponents();builder.Services.AddMudServices();builder.Services.AddAuthentication("ControlledFixture").AddScheme<AuthenticationSchemeOptions,BrowserHttpAuth>("ControlledFixture",_=>{});builder.Services.AddAuthorization();builder.Services.AddCascadingAuthenticationState();builder.Services.AddDataProtection();
builder.Services.AddSingleton<BrowserBackend>();builder.Services.AddScoped<BrowserScope>();builder.Services.AddScoped(p=>p.GetRequiredService<BrowserScope>().Company);builder.Services.AddScoped(p=>p.GetRequiredService<BrowserScope>().Fields.Object);builder.Services.AddScoped(p=>p.GetRequiredService<BrowserScope>().Permits.Object);builder.Services.AddScoped(p=>p.GetRequiredService<BrowserScope>().Incidents.Object);builder.Services.AddScoped(p=>p.GetRequiredService<BrowserScope>().Personas.Object);builder.Services.AddScoped(p=>p.GetRequiredService<BrowserScope>().Tasks.Object);builder.Services.AddScoped<AuthenticationStateProvider,BrowserAuth>();builder.Services.AddScoped<ProtectedSessionStorage>();builder.Services.AddScoped<IPermitApplicationIntentStore,PermitApplicationIntentStore>();builder.Services.AddSingleton(new OilGasCallFailures(Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>()));
var app=builder.Build();app.UseAuthentication();app.UseAuthorization();app.UseStaticFiles();app.UseAntiforgery();app.MapRazorComponents<App>().AddInteractiveServerRenderMode().AddAdditionalAssemblies(typeof(Beep.OilandGas.Web.Pages.PPDM39.Permits.PermitDashboard).Assembly);app.MapGet("/fixture/metadata",(BrowserBackend b)=>new{b.Attempts,b.Effects,b.DocumentAttempts,b.DocumentEffects,b.Role,b.Field,Rows=b.Rows.Count,Intents=b.Receipts.Keys.ToArray(),DocumentIntents=b.Documents.Keys.ToArray()});await app.RunAsync();
public sealed class BrowserAuth:AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id","actor"),new Claim(ClaimTypes.Role,"Safety")],"controlled-fixture"))));}
public sealed class BrowserBackend
{
 public string Role="author",Field="A";public bool LoseNext;public int Attempts,Effects;
 public Dictionary<string,PermitApplicationSupportingSnapshot> Supporting=[];public Dictionary<Guid,PermitDocumentStageResponse> Documents=[];public Dictionary<Guid,byte[]> Content=[];public int DocumentAttempts,DocumentEffects;
 public Dictionary<string,PERMIT_APPLICATION> Rows=[];public List<PERMIT_STATUS_HISTORY> History=[];public Dictionary<Guid,PermitApplicationCommandOutcome> Receipts=[];
 public BrowserBackend()
 {
  foreach(var pair in new[]{("draft",PermitApplicationStatus.Draft),("correction",PermitApplicationStatus.AdditionalInformationRequired),("review/شركة?1",PermitApplicationStatus.Submitted),("self",PermitApplicationStatus.Submitted)})
   Rows.Add(pair.Item1,new(){PERMIT_APPLICATION_ID=pair.Item1,OWNER_TENANT_ID="tenant",OWNER_COMPANY_ID="company",FIELD_ID="A",ACTIVE_IND="Y",COUNTRY=Country.Kuwait,STATE_PROVINCE=StateProvince.Other,REGULATORY_AUTHORITY=RegulatoryAuthority.Other,APPLICATION_TYPE=PermitApplicationType.Drilling,APPLICANT_ID="Recorded applicant",STATUS=pair.Item2,SUBMITTED_BY_USER_ID=pair.Item1=="self"?"actor":"issuer"});
 }
 public PermitApplicationCommandOutcome Record(PermitApplicationCommand original,PermitApplicationDataScope scope)
 {
  original.Require();if(original.Review.Context!=scope)throw new InvalidOperationException("Fixture company/FIELD/binding changed.");Attempts++;
  if(Receipts.TryGetValue(original.IntentId,out var prior)){if(prior.Original!=original)throw new InvalidOperationException("Different original.");return prior;}
  if(Role=="readonly" || PermitApplicationCommand.IsAuthorAction(original.Action) && Role!="author" || !PermitApplicationCommand.IsAuthorAction(original.Action) && Role!="reviewer")throw new InvalidOperationException("Fixture role refuses a new effect.");
  var row=original.Review.ReadApplication();if(original.Action!="CREATE_DRAFT")original.Review.RequireCurrent(PermitApplicationSourceReview.Capture(scope,Rows[row.PERMIT_APPLICATION_ID],History.Where(x=>x.PERMIT_APPLICATION_ID==row.PERMIT_APPLICATION_ID).ToArray(),Supporting.GetValueOrDefault(row.PERMIT_APPLICATION_ID)));
  var outcome=PermitApplicationCommandOutcome.Prepare(original,DateTimeOffset.UtcNow);Rows[row.PERMIT_APPLICATION_ID]=outcome.ReadApplication();Supporting[row.PERMIT_APPLICATION_ID]=outcome.Supporting;History.Add(outcome.ReadHistory());Receipts.Add(original.IntentId,outcome);Effects++;
  if(LoseNext){LoseNext=false;throw new HttpRequestException("Controlled lost acknowledgement after retained effect.");}return outcome;
 }
}
public sealed class BrowserScope:IDisposable
{
 public BrowserBackend Backend;public CompanyClientContext Company=new();public Mock<IFieldSelectionContext> Fields=new();public Mock<IPermitServiceClient> Permits=new();public Mock<IHSEServiceClient> Incidents=new();public Mock<IPersonaContextService> Personas=new();public Mock<IUnifiedTaskInboxService> Tasks=new();
 public FieldSelectionReview Field=>new(new("tenant","company"),new("fixture-core","fixture-core-binding"),"actor","fixture-selection-"+Backend.Field,Backend.Field);
 public PermitApplicationDataScope Context=>new(Field,new("fixture-permits","fixture-permits-binding"));
 public BrowserScope(BrowserBackend backend)
 {
  Backend=backend;ChangeRole(backend.Role);Fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(()=>new FieldResponse{FieldId=Backend.Field,FieldName="Fixture FIELD "+Backend.Field,SelectionAvailable=true,SelectionContext=Field});
  Incidents.Setup(x=>x.GetIncidentsAsync(null,null,It.IsAny<CancellationToken>())).ReturnsAsync(new List<HSEIncidentRecord>());
  var persona=new AppPersona{Code="HSE",Name="Safety",DefaultRoute="/ppdm39/hse"};Personas.SetupGet(x=>x.CurrentPersona).Returns(persona);Personas.SetupGet(x=>x.CurrentProfile).Returns(new AppUserPersona{UserId="actor",PersonaCode="HSE"});Personas.SetupGet(x=>x.Catalog).Returns([persona]);Personas.Setup(x=>x.EnsureLoadedAsync()).Returns(Task.CompletedTask);Tasks.Setup(x=>x.GetInboxAsync("HSE")).ReturnsAsync(()=>new UnifiedInbox{Company=Field.Company,ActorUserId="actor",PersonaCode="HSE"});
  Permits.Setup(x=>x.ReadInboxAsync(It.IsAny<FieldSelectionReview>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>PermitApplicationInboxReview.Capture(new(Context,Backend.Role=="author",Backend.Role=="reviewer",ProcessDefinitionRevision.FromJson("{}")),DateTimeOffset.UtcNow,Backend.Rows.Values.Where(row=>row.FIELD_ID==Backend.Field).Select(Summary).ToArray()));
  Permits.Setup(x=>x.GetByIdAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync((string id,CancellationToken _)=>Backend.Rows.TryGetValue(id,out var row)&&row.FIELD_ID==Backend.Field?new PermitApplicationDetail{Application=Summary(row),StatusHistory=[]}:null);
  Permits.Setup(x=>x.ReviewSourceAsync(It.IsAny<string>(),It.IsAny<FieldSelectionReview>(),It.IsAny<CancellationToken>())).ReturnsAsync((string id,FieldSelectionReview _,CancellationToken __)=>PermitApplicationSourceReview.Capture(Context,Backend.Rows[id],Backend.History.Where(x=>x.PERMIT_APPLICATION_ID==id).ToArray(),Backend.Supporting.GetValueOrDefault(id)));
  Permits.Setup(x=>x.ReviewCreationAsync(It.IsAny<PermitApplicationCreationProposal>(),It.IsAny<FieldSelectionReview>(),It.IsAny<CancellationToken>())).ReturnsAsync((PermitApplicationCreationProposal p,FieldSelectionReview _,CancellationToken __)=>PermitApplicationCreationReview.Capture(Context,p,"field-source",p.WellId is null?null:"well-source",p.FacilityId is null?null:"facility-source"));
  Permits.Setup(x=>x.RecordCommandAsync(It.IsAny<PermitApplicationCommand>(),It.IsAny<CancellationToken>())).ReturnsAsync((PermitApplicationCommand p,CancellationToken _)=>Backend.Record(p,Context));Permits.Setup(x=>x.RecoverCommandAsync(It.IsAny<PermitApplicationCommand>(),It.IsAny<CancellationToken>())).ReturnsAsync((PermitApplicationCommand p,CancellationToken _)=>new PermitApplicationCommandRecovery(p,Backend.Receipts.GetValueOrDefault(p.IntentId)));
  Permits.Setup(x=>x.StageDocumentAsync(It.IsAny<PermitDocumentStageRequest>(),It.IsAny<CancellationToken>())).Returns(StageDocument);
  Permits.Setup(x=>x.RecoverDocumentStageAsync(It.IsAny<PermitDocumentStageOriginal>(),It.IsAny<CancellationToken>())).Returns(RecoverDocument);
  Permits.Setup(x=>x.ReadAttachmentContentAsync(It.IsAny<PermitAttachmentContentQuery>(),It.IsAny<CancellationToken>())).Returns(ReadContent);
 }

 public Task<PermitDocumentStageResponse> StageDocument(PermitDocumentStageRequest request,CancellationToken token)
 {
  request.Require();if(request.Original.Review.Context!=Context || Backend.Role!="author")throw new InvalidOperationException("Current fixture upload authority changed.");
  Backend.DocumentAttempts++;
  if(Backend.Documents.TryGetValue(request.Original.IntentId,out var prior)){prior.Require(request.Original);return Task.FromResult(prior);}
  var o=request.Original;var revision=ProcessDefinitionRevision.FromJson("controlled-browser-native-revision");var receipt=new PermitDocumentStageReceipt(o.IntentId,Context.Field.Company,Context.Field.FieldId!,Context.Field.Database,Context.Database,o.Review.ReadApplication().PERMIT_APPLICATION_ID,Context.Field.ActorUserId,o.Document,revision,revision,revision,DateTimeOffset.UtcNow);
  var response=new PermitDocumentStageResponse(o,receipt);response.Require(o);Backend.Documents.Add(o.IntentId,response);Backend.Content.Add(o.IntentId,request.Content.ToArray());Backend.DocumentEffects++;
  if(Backend.LoseNext){Backend.LoseNext=false;throw new HttpRequestException("Controlled lost upload acknowledgement after one in-memory effect.");}return Task.FromResult(response);
 }
 public Task<PermitDocumentStageResponse> RecoverDocument(PermitDocumentStageOriginal original,CancellationToken token)
 {original.Require();if(original.Review.Context!=Context)throw new InvalidOperationException("Fixture upload recovery context changed.");return Task.FromResult(Backend.Documents.GetValueOrDefault(original.IntentId)??new(original,null));}
 public Task<PermitAttachmentContent> ReadContent(PermitAttachmentContentQuery query,CancellationToken token)
 {query.Require();if(query.Review.Context!=Context)throw new InvalidOperationException("Fixture content context changed.");var receipt=query.Review.Supporting.Documents.Single(x=>x.AttachmentId==query.AttachmentId).Receipt!;var result=new PermitAttachmentContent(query,receipt,Backend.Content[receipt.StageIntentId].ToArray());result.Require(query);return Task.FromResult(result);}
 public void ChangeRole(string role){Backend.Role=role;var permissions=role=="author"?new[]{"HSE.ViewPermits","HSE.IssuePermit"}:role=="reviewer"?new[]{"HSE.ViewPermits","HSE.ApprovePermit"}:new[]{"HSE.ViewPermits"};Company.TrySelect(Company.Clear(),"actor",new(new("tenant","company"),["Safety"],permissions));}
 public void ChangeField(string field){Backend.Field=field;Fields.Raise(x=>x.CurrentFieldChanged+=null!,field);}
 private static PermitApplicationSummary Summary(PERMIT_APPLICATION row)=>new(){PermitApplicationId=row.PERMIT_APPLICATION_ID,FieldId=row.FIELD_ID,Status=row.STATUS.ToString(),Country=row.COUNTRY.ToString(),Region=row.STATE_PROVINCE.ToString(),RegulatoryAuthority=row.REGULATORY_AUTHORITY.ToString(),ApplicationType=row.APPLICATION_TYPE.ToString(),ApplicantName=row.APPLICANT_ID,SubmittedByUserId=row.SUBMITTED_BY_USER_ID,Decision=row.DECISION};
 public void Dispose()=>Company.Dispose();
}

public sealed class BrowserHttpAuth(IOptionsMonitor<AuthenticationSchemeOptions> options,ILoggerFactory logger,UrlEncoder encoder):AuthenticationHandler<AuthenticationSchemeOptions>(options,logger,encoder)
{
 protected override Task<AuthenticateResult> HandleAuthenticateAsync()=>Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id","actor"),new Claim(ClaimTypes.Role,"Safety")],Scheme.Name)),Scheme.Name)));
}

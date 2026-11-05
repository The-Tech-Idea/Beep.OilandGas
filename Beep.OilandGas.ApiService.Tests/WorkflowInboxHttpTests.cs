using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.LifeCycle.Models;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Security.Claims;
using System.Reflection;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

/// <summary>Complete controller + actual SQLite app RBAC + native accounting HTTP fixture. External account/reporting seams are explicit.</summary>
public sealed class WorkflowInboxHttpTests
{
    [Theory]
    [InlineData("confirmed")]
    [InlineData("wrong-receipt")]
    [InlineData("changed-company")]
    [InlineData("wrong-company")]
    public async Task IncidentReportClientKeepsTheOriginalCompanyAndReceipt(string mode)
    {
        var scope = new CompanyScope("tenant", "company");
        var original = new IncidentReportIntent(Guid.NewGuid(), new(scope, new("core", "fingerprint"), "actor", "revision", "field"),
            new("SET", "SPILL", "TIER_2"), new string('A', 64), new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc), "Site", "Incident", "Kuwait");
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(mode == "wrong-company" ? new("tenant", "other") : scope, [], [])));
        using var handler = new IncidentReportHandler(request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method); Assert.Equal("/api/field/current/hse/reports", request.RequestUri!.AbsolutePath);
            if (mode == "changed-company") company.Clear();
            var receipt = new IncidentReportReceipt(scope, "actor", "field", original.IntentId, mode == "wrong-receipt" ? "wrong" : original.IncidentId());
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(receipt) };
        });
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.test") };
        var client = new BusinessProcessServiceClient(new ApiClient(http, Mock.Of<IFailureReporter>(), company), NullLogger<BusinessProcessServiceClient>.Instance);
        if (mode == "confirmed") (await client.RecordHseIncidentAsync(original)).Require(original);
        else await Assert.ThrowsAnyAsync<Exception>(() => client.RecordHseIncidentAsync(original));
        Assert.Equal(mode == "wrong-company" ? 0 : 1, handler.Calls);
    }
    private sealed class IncidentReportHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public int Calls { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        { Calls++; return Task.FromResult(respond(request)); }
    }

    [Fact]
    public async Task InboxUsesTheSelectedCompanyPersonaForTheSameAccount()
    {
        using var fixture = new Fixture();
        fixture.Repository.Set<AppPersona>().Add(new() { Code = "BUYER", Name = "Buyer", DefaultRoute = "/procurement" });
        fixture.Repository.Set<AppUserPersona>().Single(row => row.UserId == "actor" && row.BusinessAssociateId == "second").PersonaCode = "BUYER";
        fixture.Repository.RoleClaims.Add(new() { RoleId = "role", ClaimType = RepositoryPersonaService.WorkspaceClaimType, ClaimValue = "BUYER" });
        fixture.Repository.SaveChanges();
        var router = new Mock<ICrossPersonaTaskRouter>(MockBehavior.Strict);
        router.Setup(value => value.GetTasksForPersonaAsync(It.IsAny<string>())).ReturnsAsync(new List<CROSS_PERSONA_TASK>());
        await using var host = await AccountingSourceHttpTests.Start(configure: (services, context) =>
        {
            fixture.Configure(services, context);
            services.AddSingleton(router.Object);
        });

        using var first = await host.Client.GetAsync(Inbox);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var firstInbox = (await first.Content.ReadFromJsonAsync<UnifiedInbox>())!;
        Assert.Equal("company", firstInbox.Company.BusinessAssociateId);
        Assert.Equal("ACCOUNTANT", firstInbox.PersonaCode);
        using var wrong = new HttpRequestMessage(HttpMethod.Get, Inbox);
        wrong.Headers.Add("X-Test-Company", "second");
        using var refused = await host.Client.SendAsync(wrong);
        Assert.Equal(HttpStatusCode.Forbidden, refused.StatusCode);
        using var second = new HttpRequestMessage(HttpMethod.Get, "/api/workflow/tasks/inbox?personaCode=BUYER");
        second.Headers.Add("X-Test-Company", "second");
        using var accepted = await host.Client.SendAsync(second);
        Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);
        var secondInbox = (await accepted.Content.ReadFromJsonAsync<UnifiedInbox>())!;
        Assert.Equal("second", secondInbox.Company.BusinessAssociateId);
        Assert.Equal("BUYER", secondInbox.PersonaCode);
        router.Verify(value => value.GetTasksForPersonaAsync("ACCOUNTANT"), Times.AtLeastOnce);
        router.Verify(value => value.GetTasksForPersonaAsync("BUYER"), Times.AtLeastOnce);
    }

    [Theory][InlineData(false)][InlineData(true)]
    public async Task InvestmentAssignmentsCannotUseGenericFieldInboxAdmissionBeforeTheReceivingSourceOwner(bool retagged)
    {
        using var fixture=new Fixture();await using var host=await AccountingSourceHttpTests.Start(configure:(services,context)=>{
            fixture.Configure(services,context);fixture.TaskRow.ENTITY_TYPE=retagged?"WELL":"AFE";fixture.TaskRow.ENTITY_ID="private-afe";
            fixture.TaskRow.PROCESS_INSTANCE_ID=DevelopmentInvestmentHandoff.InstancePrefix+"owned";
            context.Processes.Setup(value=>value.GetProcessInstanceAsync(fixture.TaskRow.PROCESS_INSTANCE_ID)).ReturnsAsync(new Beep.OilandGas.Models.Processes.ProcessInstance{
                InstanceId=fixture.TaskRow.PROCESS_INSTANCE_ID,FieldId="field",EntityType="WELL",EntityId="private-afe"});
        });
        var result=await host.Client.GetFromJsonAsync<UnifiedInbox>(Inbox);Assert.NotNull(result);Assert.Empty(result.CriticalTasks.Concat(result.HighPriorityTasks).Concat(result.NormalTasks));Assert.Equal(0,result.Counts.TotalPending);
    }
    private sealed class Db(DbContextOptions<Db> options) : RepositoryDbContext(options);
    internal sealed class Fixture : IDisposable
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        public RepositoryDbContext Repository { get; }
        public CROSS_PERSONA_TASK TaskRow = new() { CROSS_TASK_ID="task",PROCESS_INSTANCE_ID="process",PROCESS_STEP_INSTANCE_ID="step",
            TARGET_PERSONA_CODE="ACCOUNTANT",ASSIGNED_ROLE="ConfiguredCompanyRole",ENTITY_TYPE="AP_INVOICE",ENTITY_ID="invoice",PRIORITY=1 };
        public int TaskReads;
        public Action<int>? OnTaskRead;
        private readonly string Actor;
        public Fixture(string persona="ACCOUNTANT",string assignedRole="ConfiguredCompanyRole",string actor="actor")
        {
            Actor=actor;TaskRow.TARGET_PERSONA_CODE=persona;TaskRow.ASSIGNED_ROLE=assignedRole;
            connection.Open();Repository=new Db(new DbContextOptionsBuilder<Db>().UseSqlite(connection).Options);Repository.Database.EnsureCreated();
            Repository.Users.Add(new OilGasUser{Id=Actor,IsActive=true});
            Repository.Roles.Add(new IdentityRole{Id="role",Name=assignedRole,NormalizedName=assignedRole.ToUpperInvariant()});
            Repository.UserRoles.Add(new IdentityUserRole<string>{UserId=Actor,RoleId="role"});
            Repository.Set<AppTenant>().Add(new(){TenantId="tenant",Name="Company tenant"});
            foreach(var company in new[]{"company","second"})
            {
                Repository.Set<AppCompany>().Add(new(){TenantId="tenant",BusinessAssociateId=company});
                Repository.Set<AppCompanyMembership>().Add(new(){TenantId="tenant",BusinessAssociateId=company,UserId=Actor,GrantedByUserId=Actor,EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
                Repository.Set<AppUserRoleExtension>().Add(new(){TenantId="tenant",BusinessAssociateId=company,UserId=Actor,RoleId="role",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
            }
            Repository.Set<AppPersona>().Add(new(){Code=persona,Name=persona,DefaultRoute=persona is "MAINTENANCE" or "SUPERVISOR"?"/ppdm39/workorder":persona=="BUYER"?"/procurement":"/accounting/dashboard"});
            foreach(var company in new[]{"company","second"}) Repository.Set<AppUserPersona>().Add(new(){UserId=Actor,TenantId="tenant",BusinessAssociateId=company,PersonaCode=persona});
            Repository.RoleClaims.Add(new(){RoleId="role",ClaimType=RepositoryPersonaService.WorkspaceClaimType,ClaimValue=persona});
            foreach(var permission in new[]{"Accounting.View","Procurement.View"})Repository.RoleClaims.Add(new(){RoleId="role",ClaimType="permission",ClaimValue=permission});
            Repository.SaveChanges();
        }
        public void Configure(IServiceCollection services,AccountingSourceHttpTests.ReadContext context)
        {
            context.IncludeTaskStep=true;
            context.Roles=[TaskRow.ASSIGNED_ROLE];
            services.AddSingleton(Repository);services.AddSingleton<IRepositoryAccessService>(new RepositoryAccessService(Repository));
            services.AddSingleton(new RepositoryPersonaService(Repository));
            services.AddScoped(provider=>{
                var request=provider.GetRequiredService<IHttpContextAccessor>().HttpContext!.Request;
                var header=request.Headers[CompanyRequestHeaders.Company].FirstOrDefault() ?? request.Headers["X-Test-Company"].FirstOrDefault() ?? "company";
                var tenant=new RepositoryTenantContext();tenant.SelectCompany(new RepositoryCompanyContext(request.Headers["X-Test-Actor"].FirstOrDefault() ?? Actor,new(new("tenant",header),[TaskRow.ASSIGNED_ROLE],["Accounting.View","Procurement.View"])));return tenant;});
            var router=new Mock<ICrossPersonaTaskRouter>(MockBehavior.Strict);
            router.Setup(value=>value.GetTasksForPersonaAsync(TaskRow.TARGET_PERSONA_CODE)).ReturnsAsync(()=>{
                OnTaskRead?.Invoke(++TaskReads);return new List<CROSS_PERSONA_TASK>{JsonSerializer.Deserialize<CROSS_PERSONA_TASK>(JsonSerializer.Serialize(TaskRow))!};});
            services.AddSingleton(router.Object);services.AddSingleton(context.Processes.Object);
            services.AddSingleton(Mock.Of<IFailureReporter>());
        }
        public void Dispose(){Repository.Dispose();connection.Dispose();}
    }
    private const string Inbox="/api/workflow/tasks/inbox?personaCode=ACCOUNTANT";
    [Fact] public async Task NewlyAppearingAssignedCandidateCannotPublishPartialInboxCounts()
    {
        using var fixture=new Fixture();await using var host=await AccountingSourceHttpTests.Start(configure:(services,context)=>
        {
            fixture.Configure(services,context);var reads=0;var router=new Mock<ICrossPersonaTaskRouter>();
            router.Setup(value=>value.GetTasksForPersonaAsync("ACCOUNTANT")).ReturnsAsync(()=>
            {
                var original=JsonSerializer.Deserialize<CROSS_PERSONA_TASK>(JsonSerializer.Serialize(fixture.TaskRow))!;
                if(++reads==1)return new List<CROSS_PERSONA_TASK>{original};
                var added=JsonSerializer.Deserialize<CROSS_PERSONA_TASK>(JsonSerializer.Serialize(original))!;added.CROSS_TASK_ID="newly-assigned";
                return new List<CROSS_PERSONA_TASK>{original,added};
            });services.AddSingleton(router.Object);
        });
        using var response=await host.Client.GetAsync(Inbox);Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);Assert.DoesNotContain("CriticalTasks",await response.Content.ReadAsStringAsync());
    }
    [Fact] public async Task ActualNativeMaintenanceAcceptanceReloadRemovesClosedWorkAndAdmitsOnlyNextCompanyRole()
    {
        var native=new CompanyMaintenanceAcceptanceTests.Fixture();await native.Ready();
        var foreign=native.Start with{Context=native.Start.Context with{Company=new("tenant","second")},IntentId=Guid.NewGuid(),Source=native.Start.Source with{Company=new("tenant","second")}};
        await native.Owner.Service.StartMaintenanceProcessAsync(foreign,native.Owner.Guard,()=>Task.FromResult(foreign.Source));
        using var receiver=new Fixture("MAINTENANCE","MaintenanceRole","receiver");
        await using var host=await AccountingSourceHttpTests.Start(configure:(services,context)=>
        {
            receiver.Configure(services,context);services.AddSingleton<Beep.OilandGas.Models.Core.Interfaces.IProcessService>(native.Owner.Service);services.AddSingleton<ICrossPersonaTaskRouter>(native.Owner.Native.Router);
            context.Resources.Setup(value=>value.CheckAssetAccessAsync("receiver","field","FIELD","READ",null)).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=true});
            context.Resources.Setup(value=>value.CheckAssetAccessAsync("receiver",native.Link.WorkOrderId,"WORK_ORDER","READ",null)).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=true});
        });
        host.Client.DefaultRequestHeaders.Add("X-Test-Actor","receiver");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","MaintenanceRole");
        const string route="/api/workflow/tasks/inbox?personaCode=MAINTENANCE";
        using var before=await host.Client.GetAsync(route);Assert.Equal(HttpStatusCode.OK,before.StatusCode);var original=(await before.Content.ReadFromJsonAsync<UnifiedInbox>())!;
        var task=Assert.Single(original.CriticalTasks.Concat(original.HighPriorityTasks).Concat(original.NormalTasks));Assert.Equal(native.Start.InstanceId(),task.WorkflowName);Assert.Equal(native.Link.WorkOrderId,task.EntityId);Assert.Equal(1,original.Counts.TotalPending);
        var receiverDashboard=await RenderMaintenanceSummary(host.Client,"receiver","MAINTENANCE","MaintenanceRole");
        Assert.Contains("Maintenance work-order assignments: 1",receiverDashboard);Assert.Contains("Manage Work Orders",receiverDashboard);Assert.Contains("Recorded maintenance orders",receiverDashboard);Assert.Contains("No recorded work orders were returned",receiverDashboard);
        Assert.Contains("/ppdm39/process/instances/"+native.Start.InstanceId(),receiverDashboard);
        var command=new CompanyMaintenanceAcceptanceCommand(await native.Review(),Guid.NewGuid(),"Original independent receiving accepted");await native.Accept(command);
        using var after=await host.Client.GetAsync(route);Assert.Equal(HttpStatusCode.OK,after.StatusCode);Assert.Equal(0,(await after.Content.ReadFromJsonAsync<UnifiedInbox>())!.Counts.TotalPending);
        var completedDashboard=await RenderMaintenanceSummary(host.Client,"receiver","MAINTENANCE","MaintenanceRole");
        Assert.Contains("Pending work: 0",completedDashboard);
        Assert.DoesNotContain("Maintenance work-order assignments:",completedDashboard);
        using var next=new Fixture("SUPERVISOR","SupervisorRole","supervisor");
        await using var nextHost=await AccountingSourceHttpTests.Start(configure:(services,context)=>
        {
            next.Configure(services,context);services.AddSingleton<Beep.OilandGas.Models.Core.Interfaces.IProcessService>(native.Owner.Service);services.AddSingleton<ICrossPersonaTaskRouter>(native.Owner.Native.Router);
            context.Resources.Setup(value=>value.CheckAssetAccessAsync("supervisor","field","FIELD","READ",null)).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=true});
            context.Resources.Setup(value=>value.CheckAssetAccessAsync("supervisor",native.Link.WorkOrderId,"WORK_ORDER","READ",null)).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=true});
        });
        nextHost.Client.DefaultRequestHeaders.Add("X-Test-Actor","supervisor");nextHost.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","SupervisorRole");
        using var nextResponse=await nextHost.Client.GetAsync("/api/workflow/tasks/inbox?personaCode=SUPERVISOR");Assert.Equal(HttpStatusCode.OK,nextResponse.StatusCode);var following=(await nextResponse.Content.ReadFromJsonAsync<UnifiedInbox>())!;
        var assigned=Assert.Single(following.CriticalTasks.Concat(following.HighPriorityTasks).Concat(following.NormalTasks));Assert.Equal(native.Start.InstanceId(),assigned.WorkflowName);Assert.Equal("SupervisorRole",assigned.AssignedRole);Assert.Equal("WORK_ORDER",assigned.EntityType);Assert.Equal(1,following.Counts.TotalPending);
        var nextSummary=await RenderMaintenanceSummary(nextHost.Client,"supervisor","SUPERVISOR","SupervisorRole");Assert.Contains("Maintenance work-order assignments: 1",nextSummary);Assert.Contains("SupervisorRole",nextSummary);
    }
    [Theory][InlineData("no-workspace")][InlineData("inactive-persona")][InlineData("external-route")][InlineData("recursive-route")]
    public async Task MaintenanceLandingCannotOpenUnassignedInactiveOrUnavailableWorkspace(string fault)
    {
        using var fixture=new Fixture("MAINTENANCE","MaintenanceRole","receiver");
        if(fault=="no-workspace")fixture.Repository.RoleClaims.RemoveRange(fixture.Repository.RoleClaims.Where(row=>row.ClaimType==RepositoryPersonaService.WorkspaceClaimType));
        else {var persona=fixture.Repository.Set<AppPersona>().Single();if(fault=="inactive-persona")persona.IsActive=false;else persona.DefaultRoute=fault=="external-route"?"//foreign.invalid/workspace":"/landing";}
        fixture.Repository.SaveChanges();
        await using var host=await AccountingSourceHttpTests.Start(configure:fixture.Configure);host.Client.DefaultRequestHeaders.Add("X-Test-Actor","receiver");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","MaintenanceRole");
        var html=await RenderMaintenanceSummary(host.Client,"receiver","MAINTENANCE","MaintenanceRole",false);
        Assert.DoesNotContain("Manage Work Orders",html);Assert.DoesNotContain("Maintenance work-order assignments:",html);
        Assert.Contains(fault is "no-workspace" or "inactive-persona"?"No workspace is assigned":fault=="external-route"?"saved workspace destination is unavailable":"points back to this page",html);Assert.Equal(0,fixture.TaskReads);
    }
    [Theory][InlineData("account")][InlineData("company")][InlineData("company-return")]
    public async Task MaintenanceLandingCannotNavigateFromAWorkspaceResponseAfterItsAccountOrCompanyChanges(string change)
    {
        using var fixture=new Fixture("MAINTENANCE","MaintenanceRole","receiver");await using var host=await AccountingSourceHttpTests.Start(configure:fixture.Configure);host.Client.DefaultRequestHeaders.Add("X-Test-Actor","receiver");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","MaintenanceRole");
        var changes=0;var html=await RenderMaintenanceSummary(host.Client,"receiver","MAINTENANCE","MaintenanceRole",false,(company,auth)=>
        {
            changes++;
            if(change=="account")auth.Switch("another-account");
            else
            {
                company.Clear();
                if(change=="company-return")
                {
                    fixture.Repository.RoleClaims.RemoveRange(fixture.Repository.RoleClaims.Where(row=>row.ClaimType==RepositoryPersonaService.WorkspaceClaimType));fixture.Repository.SaveChanges();
                    Assert.True(company.TrySelect(company.Version,"receiver",new(new("tenant","company"),["MaintenanceRole"],[])));
                }
            }
        });
        Assert.Equal(1,changes);Assert.DoesNotContain("Manage Work Orders",html);Assert.DoesNotContain("Maintenance work-order assignments:",html);Assert.Equal(0,fixture.TaskReads);
    }
    private sealed class WorkspaceResponseBoundary(HttpClient upstream,Action afterResponse) : HttpMessageHandler
    {
        private bool changed;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Assert.Equal(HttpMethod.Get,request.Method);
            using var forwarded=new HttpRequestMessage(request.Method,request.RequestUri);foreach(var header in request.Headers)forwarded.Headers.TryAddWithoutValidation(header.Key,header.Value);
            var response=await upstream.SendAsync(forwarded,token);
            if(!changed && request.RequestUri!.AbsolutePath=="/api/personas/workspaces/current"){Assert.True(response.IsSuccessStatusCode);changed=true;afterResponse();}
            return response;
        }
    }
    private static async Task<string> RenderMaintenanceSummary(HttpClient http,string actor,string persona,string role,bool opensDashboard=true,Action<CompanyClientContext,AuthenticationFixture>? afterWorkspace=null)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),actor,new(new("tenant","company"),[role],[])));
        var auth=new AuthenticationFixture(role,actor);
        using var responseBoundary=afterWorkspace is null?null:new HttpClient(new WorkspaceResponseBoundary(http,()=>afterWorkspace(company,auth))){BaseAddress=http.BaseAddress};
        var api=new ApiClient(responseBoundary ?? http,Mock.Of<IFailureReporter>(),company);using var personas=new PersonaContextService(new PersonaClient(api),auth,company,user=>user.FindFirst("party_id")?.Value);
        var navigation=new MaintenanceLandingNavigation();
        var services=new ServiceCollection();services.AddLogging();services.AddRazorComponents();services.AddMudServices();services.AddAuthorizationCore();services.AddCascadingAuthenticationState();services.AddSingleton<AuthenticationStateProvider>(auth);services.AddSingleton(company);services.AddSingleton<IPersonaContextService>(personas);services.AddSingleton<IUnifiedTaskInboxService>(new UnifiedTaskInboxService(api));services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<NavigationManager>(navigation);
        var environment=new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();environment.SetupGet(value=>value.EnvironmentName).Returns("Development");environment.SetupGet(value=>value.ApplicationName).Returns(typeof(WorkflowInboxHttpTests).Assembly.GetName().Name!);environment.SetupGet(value=>value.ContentRootFileProvider).Returns(new Microsoft.Extensions.FileProviders.NullFileProvider());environment.SetupGet(value=>value.WebRootFileProvider).Returns(new Microsoft.Extensions.FileProviders.NullFileProvider());services.AddSingleton(environment.Object);services.AddSingleton<Microsoft.Extensions.Hosting.IHostEnvironment>(environment.Object);services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
        // Header/selector reads are explicit boundaries; the real native acceptance/inbox remains above.
        var fields=new Mock<IFieldSelectionContext>(MockBehavior.Strict);fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync("field");fields.Setup(value=>value.GetAvailableFieldsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<FieldListItem>{new(){FieldId="field",FieldName="Original FIELD"}});fields.Setup(value=>value.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{SelectionAvailable=true,FieldId="field",SelectionContext=new(new("tenant","company"),new("core","fingerprint"),actor,"revision","field")});
        using var selection=new FieldSelectionState(company,fields.Object,(_,_,message)=>message);services.AddSingleton<IFieldSelectionContext>(fields.Object);services.AddSingleton(selection);
        var orders=new Mock<IWorkOrderServiceClient>(MockBehavior.Strict);orders.Setup(value=>value.GetWorkOrdersAsync(null,null)).ReturnsAsync(new List<WorkOrderSummary>());services.AddSingleton(orders.Object);
        services.AddScoped<WorkOrderFinancialWorkspaceState>();
        services.AddSingleton(Mock.Of<INotificationService>());
        var companies=new Mock<ICompanyAccessClient>(MockBehavior.Strict);companies.Setup(value=>value.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[]{new CompanyScope("tenant","company")});services.AddSingleton(new CompanySelectionService(companies.Object,auth,company,user=>user.FindFirst("party_id")?.Value));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async()=>{var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Auth.Tests.Fixtures.MaintenanceLandingRouteHost>();await root.QuiescenceTask;
            if(opensDashboard){Assert.Equal("/ppdm39/workorder",new Uri(navigation.Uri).AbsolutePath);Assert.Equal(new[]{"/ppdm39/workorder"},navigation.Destinations);Assert.Equal(persona,personas.CurrentPersona!.Code);Assert.Equal("/ppdm39/workorder",personas.CurrentPersona.DefaultRoute);Assert.Equal(actor,personas.CurrentProfile!.UserId);}
            else {Assert.Equal("/landing",new Uri(navigation.Uri).AbsolutePath);Assert.Empty(navigation.Destinations);}
            return root.ToHtmlString();});
    }
    private sealed class MaintenanceLandingNavigation : NavigationManager
    {
        public List<string> Destinations {get;}=[];
        public MaintenanceLandingNavigation()=>Initialize("https://render.invalid/","https://render.invalid/landing");
        protected override void NavigateToCore(string uri,bool forceLoad){Destinations.Add(uri);Uri=ToAbsoluteUri(uri).ToString();NotifyLocationChanged(false);}
        protected override void NavigateToCore(string uri,NavigationOptions options)=>NavigateToCore(uri,options.ForceLoad);
    }
    [Theory][InlineData("changed-name")][InlineData("invalid-name")][InlineData("invalid-creator")][InlineData("changed-creator")]
    public async Task ChangedOrInvalidRecordedStepAndCreatorCannotPublishLandingEvidence(string fault)
    {
        using var fixture=new Fixture();await using var host=await AccountingSourceHttpTests.Start(configure:fixture.Configure);
        if(fault=="changed-name")host.Context.OnProcessRead=read=>{if(read==4)host.Context.TaskStepName="Different step description";};
        if(fault=="invalid-name")host.Context.TaskStepName="review\u0001";
        if(fault=="invalid-creator")fixture.TaskRow.ROW_CREATED_BY="actor\u0001";
        if(fault=="changed-creator")fixture.OnTaskRead=read=>{if(read==2)fixture.TaskRow.ROW_CREATED_BY="another-account";};
        var response=await host.Client.GetAsync(Inbox);Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);
        var body=await response.Content.ReadAsStringAsync();Assert.DoesNotContain("TotalPending",body);Assert.DoesNotContain("StepDisplayName",body);Assert.DoesNotContain("CreatedByUserId",body);
    }
    [Theory][InlineData(true)][InlineData(false)]
    public async Task ChangedSkippedForeignOwnershipOrTaskCannotReturnASuccessfulEmptyQueue(bool owner)
    {
        using var fixture=new Fixture();await using var host=await AccountingSourceHttpTests.Start(configure:fixture.Configure);
        host.Context.ForeignProcess=true;
        if(owner)host.Context.OnProcessRead=read=>{if(read==2)host.Context.ForeignProcess=false;};
        else fixture.OnTaskRead=read=>{if(read==2)fixture.TaskRow.TASK_STATUS="COMPLETED";};
        var response=await host.Client.GetAsync(Inbox);Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);
        Assert.DoesNotContain("TotalPending",await response.Content.ReadAsStringAsync());Assert.Equal(0,host.Context.Reads);
    }
    [Fact]
    public async Task ActualCompanyProcessWriterFeedsSeparateCompanyInboxesThroughActualProcessReads()
    {
        var native=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");
        var intent=await native.Intent();
        var first=await native.Start(intent);
        var second=await native.Start(intent with {Company=new("tenant","second")});
        using var fixture=new Fixture();
        await using var host=await AccountingSourceHttpTests.Start(configure:(services,context)=>{
            fixture.Configure(services,context);
            context.FixedProcess=native.Service;
            services.AddSingleton<Beep.OilandGas.Models.Core.Interfaces.IProcessService>(native.Service);
            services.AddSingleton<ICrossPersonaTaskRouter>(native.Native.Router);
        });
        foreach(var selected in new[]{first,second})
        {
            host.Client.DefaultRequestHeaders.Remove("X-Test-Company");host.Client.DefaultRequestHeaders.Add("X-Test-Company",selected.Company.BusinessAssociateId);
            var response=await host.Client.GetAsync(Inbox);Assert.Equal(HttpStatusCode.OK,response.StatusCode);
            var queue=(await response.Content.ReadFromJsonAsync<UnifiedInbox>())!;
            Assert.Equal(selected.Company,queue.Company);Assert.Equal(1,queue.Counts.TotalPending);
            var task=Assert.Single(queue.NormalTasks);Assert.Contains(task.TaskId,selected.TaskIds);Assert.Equal(selected.InstanceId,task.WorkflowName);
            var foreign=selected==first?second:first;
            Assert.DoesNotContain(foreign.InstanceId,await response.Content.ReadAsStringAsync());
            var detail=await host.Client.GetFromJsonAsync<Beep.OilandGas.Models.Processes.ProcessInstance>("/api/accounting/processes/"+Uri.EscapeDataString(selected.InstanceId));
            Assert.Equal(selected.InstanceId,detail!.InstanceId);Assert.Single(detail.History);Assert.Equal(2,detail.StepInstances.Count);
        }
        Assert.Equal(12,native.Native.Writes);
    }
    [Theory][InlineData("company",200)][InlineData("second",200)]
    public async Task NativeRoutedTaskSetFeedsTheActualOwnedCompanyInbox(string company,int status)
    {
        var native=new PersonaTaskRoutingTests.Fixture();native.Step.REQUIRED_ROLE="ConfiguredCompanyRole";native.Destinations=["ACCOUNTANT"];
        native.Personas.Setup(value=>value.GetActivePersonasAsync("ConfiguredCompanyRole")).ReturnsAsync(native.Destinations.ToList());
        var recorded=Assert.Single(await native.Router.RouteTaskAsync(native.Process.PROCESS_INSTANCE_ID,"step","ConfiguredCompanyRole","AP_INVOICE","source","Recorded source","actor"));
        using var fixture=new Fixture();
        await using var host=await AccountingSourceHttpTests.Start(configure:(services,context)=>{
            fixture.Configure(services,context);services.AddSingleton<ICrossPersonaTaskRouter>(native.Router);
            context.ProcessId=native.Process.PROCESS_INSTANCE_ID;context.InvoiceId="source";
        });
        host.Context.Processes.Setup(value=>value.GetProcessInstanceAsync(native.Process.PROCESS_INSTANCE_ID)).ReturnsAsync(()=>new Beep.OilandGas.Models.Processes.ProcessInstance{
            InstanceId=native.Process.PROCESS_INSTANCE_ID,ProcessId=native.Process.PROCESS_DEFINITION_ID,EntityType=native.Process.ENTITY_TYPE,EntityId=native.Process.ENTITY_ID,
            FieldId="",Status=Beep.OilandGas.Models.Processes.ProcessStatus.IN_PROGRESS,CurrentStepId=native.Process.CURRENT_STEP_ID!,StartDate=native.Process.STARTED_DATE!.Value,
            ProcessData=JsonSerializer.Deserialize<Beep.OilandGas.Models.Data.Process.PROCESS_DATA>(native.Process.INSTANCE_DATA_JSON!)!,
            StepInstances=[new(){InstanceId=native.Process.PROCESS_INSTANCE_ID,StepInstanceId=native.Step.PROCESS_STEP_INSTANCE_ID,StepId=native.Step.STEP_ID,RequiredRole=native.Step.REQUIRED_ROLE}]
        });
        host.Client.DefaultRequestHeaders.Add("X-Test-Company",company);
        var response=await host.Client.GetAsync(Inbox);Assert.Equal(status,(int)response.StatusCode);
        if(company=="company"){var row=Assert.Single((await response.Content.ReadFromJsonAsync<UnifiedInbox>())!.NormalTasks);Assert.Equal(recorded.CROSS_TASK_ID,row.TaskId);Assert.Equal(recorded.ROUTE,row.Route);Assert.Equal("APPROVAL",row.TaskType);}
        else {var result=(await response.Content.ReadFromJsonAsync<UnifiedInbox>())!;Assert.Equal(0,result.Counts.TotalPending);Assert.DoesNotContain(recorded.CROSS_TASK_ID,await response.Content.ReadAsStringAsync());Assert.Equal(0,host.Context.Reads);}
        Assert.Equal(1,native.Writes);
    }
    private sealed class AuthenticationFixture(string role="ConfiguredCompanyRole",string actor="actor") : AuthenticationStateProvider
    {
        private string currentActor=actor;
        public void Switch(string next){currentActor=next;NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());}
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(
            new[]{new Claim(ClaimTypes.NameIdentifier,currentActor),new Claim("party_id",currentActor),new Claim(ClaimTypes.Role,role)},"test"))));
    }
    private sealed class ProcessPageComponents(IServiceProvider services) : IComponentActivator
    {
        public IComponent? Page;
        public IComponent CreateInstance(Type type)
        {
            var component=(IComponent)ActivatorUtilities.CreateInstance(services,type);
            if(type==typeof(Beep.OilandGas.Web.Pages.PPDM39.Accounting.CompanyAccountingProcessDetail))Page=component;
            return component;
        }
    }
    [Theory][InlineData("AP_INVOICE","complete")][InlineData("PURCHASE_ORDER","complete")]
    [InlineData("AP_INVOICE","reconcile")][InlineData("PURCHASE_ORDER","reconcile")]
    [InlineData("AP_INVOICE","permission")][InlineData("AP_INVOICE","role")]
    public async Task ControllerPageExecutesTheRecordedFinalStepAndRefreshesAssignedWork(string type,string mode)
    {
        var (native,firstCommand)=await CompanyStepCommandTests.Prepare(type:type);
        await using var initial=await CompanyProcessStartHttpTests.Host(native,permissions:["Accounting.View","Procurement.View","Workflow.Approve"]);
        (await initial.Client.PostAsJsonAsync("/api/accounting/processes/step-complete",firstCommand)).EnsureSuccessStatusCode();
        using var fixture=new Fixture("CONTROLLER","Controller");fixture.Repository.RoleClaims.Add(new(){RoleId="role",ClaimType="permission",ClaimValue="Workflow.Execute"});fixture.Repository.SaveChanges();
        await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Procurement.View","Workflow.Execute"],configure:(services,context)=>{
            fixture.Configure(services,context);context.FixedProcess=native.Service;context.StartBinding=new string('B',64);context.Preparer="invoice-preparer";context.PreparedAt=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc);
            services.AddSingleton<Beep.OilandGas.Models.Core.Interfaces.IProcessService>(native.Service);services.AddSingleton<ICrossPersonaTaskRouter>(native.Native.Router);
        });
        host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","Controller");
        using var company=new CompanyClientContext();var access=await new RepositoryAccessService(fixture.Repository).GetCompanyAccessAsync("actor",new("tenant","company"));company.TrySelect(company.Clear(),"actor",access!);
        var api=new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company);var auth=new AuthenticationFixture("Controller");var client=new BusinessProcessServiceClient(api,NullLogger<BusinessProcessServiceClient>.Instance);
        using var personas=new PersonaContextService(new PersonaClient(api),auth,company,user=>user.FindFirst("party_id")?.Value);await personas.EnsureLoadedAsync();
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton(company);
        services.AddScoped<CompanyAccountingStepState>();services.AddSingleton<IBusinessProcessServiceClient>(client);services.AddSingleton<IAccountingServiceClient>(new Mock<IAccountingServiceClient>(MockBehavior.Strict).Object);services.AddSingleton<AuthenticationStateProvider>(auth);
        services.AddSingleton<NavigationManager>(new NavigationFixture("/accounting/processes/"+Uri.EscapeDataString(firstCommand.Review.InstanceId)));
        services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,ProcessPageComponents>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());var completion=provider.GetRequiredService<CompanyAccountingStepState>();
        CompanyAccountingStepCommand? command=null;CompanyAccountingStepReceipt? receipt=null;
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.CompanyAccountingProcessDetail>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",firstCommand.Review.InstanceId}}));
            var page=((ProcessPageComponents)provider.GetRequiredService<IComponentActivator>()).Page!;
            async Task Invoke(string method){await (Task)page.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null)!;typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null);}
            await Invoke("ReviewStep");Assert.True(completion.Review!.Eligible);Assert.Equal("Workflow.Execute",completion.Review.RequiredPermission);Assert.Equal("Controller",completion.Review.AssignedRole);
            page.GetType().GetField("_notes",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(page,"Recorded controller workflow execution; financial posting remains separate.");
            if(mode=="permission")host.Context.Permissions=["Accounting.View","Procurement.View"];
            if(mode=="role")host.Client.DefaultRequestHeaders.Remove("X-Test-Role");
            if(mode=="reconcile")host.Context.OnCapture=_=>{if(native.Native.Writes>16)host.Context.SourceEdited=true;};
            await Invoke("CompleteStep");
            if(mode is "permission" or "role"){Assert.Null(completion.Receipt);Assert.True(completion.HasUnconfirmedIntent);Assert.Equal(16,native.Native.Writes);Assert.DoesNotContain("Recorded workflow is completed",root.ToHtmlString());return;}
            if(mode=="reconcile")
            {
                Assert.Null(completion.Receipt);Assert.True(completion.HasUnconfirmedIntent);var retained=completion.Intent;var writes=native.Native.Writes;
                await Invoke("Reload");Assert.Contains("Reconcile original completion",root.ToHtmlString());
                host.Context.OnCapture=null;host.Context.SourceEdited=false;page.GetType().GetField("_notes",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(page,"Replacement must not alter the original execution.");
                await Invoke("CompleteStep");Assert.Equal(retained,completion.Intent);Assert.Equal(writes,native.Native.Writes);
            }
            command=completion.Intent;receipt=completion.Receipt;Assert.NotNull(receipt);Assert.Null(receipt.NextStepInstanceId);Assert.Empty(receipt.NextTaskIds);
            var html=System.Net.WebUtility.HtmlDecode(root.ToHtmlString());Assert.Contains("Recorded workflow is completed",html);Assert.Contains("Completed by account: actor",html);Assert.Contains("Return to assigned work",html);Assert.DoesNotContain("Review current step authority",html);
            var count=native.Native.Writes;await Invoke("ReviewStep");await Invoke("CompleteStep");Assert.Equal(count,native.Native.Writes);Assert.Null(completion.Review);
        });
        if(receipt is null)return;
        var process=(await client.GetCompanyAccountingInstanceAsync(firstCommand.Review.InstanceId))!;
        Assert.Equal(Beep.OilandGas.Models.Processes.ProcessStatus.COMPLETED,process.Status);Assert.Equal("actor",process.CompletedBy);Assert.Equal(3,process.History.Count);
        Assert.All(process.StepInstances,step=>Assert.Equal(Beep.OilandGas.Models.Processes.StepStatus.COMPLETED,step.Status));Assert.Single(native.Native.Rows.OfType<PROCESS_APPROVAL>());
        var writesBeforeRetry=native.Native.Writes;var retry=await client.CompleteCompanyAccountingStepAsync(command!);Assert.Equal(JsonSerializer.Serialize(receipt),JsonSerializer.Serialize(retry));Assert.Equal(writesBeforeRetry,native.Native.Writes);
        using var queue=new PersonaTaskInboxState(new UnifiedTaskInboxService(api),company,personas);queue.Start();await queue.LoadAsync(["/accounting/dashboard"]);Assert.Empty(queue.VisibleRows);Assert.Equal(0,queue.SelectedCounts.TotalPending);
        var landing=await RenderJourneyAsync(api,company,personas,auth,process.InstanceId,false,accountingLanding:true);Assert.Contains("Pending work: 0",landing);Assert.DoesNotContain("Controller assignments: 1",landing);
        var reopened=await RenderJourneyAsync(api,company,personas,auth,process.InstanceId,true);Assert.Contains("Recorded workflow is completed",reopened);Assert.Contains(command!.Notes,reopened);Assert.DoesNotContain("Complete reviewed workflow step",reopened);
    }
    [Theory][InlineData("AP_INVOICE")][InlineData("PURCHASE_ORDER")]
    public async Task ActualCompletedReviewMovesPendingWorkIntoTheControllerWorkspace(string type)
    {
        var (native,command)=await CompanyStepCommandTests.Prepare(type:type);
        await using var commandHost=await CompanyProcessStartHttpTests.Host(native,permissions:["Accounting.View","Procurement.View","Workflow.Approve"]);
        var response=await commandHost.Client.PostAsJsonAsync("/api/accounting/processes/step-complete",command);response.EnsureSuccessStatusCode();
        var receipt=(await response.Content.ReadFromJsonAsync<CompanyAccountingStepReceipt>())!;receipt.Require(command);
        // A separate explicit app-role/workspace fixture proves routing after actual HTTP completion.
        using var fixture=new Fixture("CONTROLLER","Controller");
        await using var host=await AccountingSourceHttpTests.Start(configure:(services,context)=>{
            fixture.Configure(services,context);context.FixedProcess=native.Service;
            services.AddSingleton<Beep.OilandGas.Models.Core.Interfaces.IProcessService>(native.Service);services.AddSingleton<ICrossPersonaTaskRouter>(native.Native.Router);
        });
        host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","Controller");
        using var company=new CompanyClientContext();var access=await new RepositoryAccessService(fixture.Repository).GetCompanyAccessAsync("actor",new("tenant","company"));
        Assert.True(company.TrySelect(company.Clear(),"actor",access!));var api=new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company);var auth=new AuthenticationFixture("Controller");
        using var personas=new PersonaContextService(new PersonaClient(api),auth,company,user=>user.FindFirst("party_id")?.Value);
        using var state=new PersonaTaskInboxState(new UnifiedTaskInboxService(api),company,personas);state.Start();await state.LoadAsync(["/accounting/dashboard"]);
        Assert.Equal("CONTROLLER",personas.CurrentPersona!.Code);state.SelectSource(type);state.SelectRole("Controller");
        var row=Assert.Single(state.VisibleRows);Assert.Contains(row.TaskId,receipt.NextTaskIds);Assert.DoesNotContain(row.TaskId,receipt.CompletedTaskIds);
        Assert.Equal(receipt.NextStepInstanceId,row.StepName);Assert.Equal("actor",row.CreatedByUserId);Assert.False(string.IsNullOrWhiteSpace(row.StepDisplayName));
        Assert.Equal(1,state.SelectedCounts.TotalPending);Assert.Equal(1,state.RoleCount("Controller"));
        var html=System.Net.WebUtility.HtmlDecode(await RenderJourneyAsync(api,company,personas,auth,command.Review.InstanceId,false));
        Assert.Contains("Controller assignments: 1",html);Assert.Contains("Recorded step: "+row.StepDisplayName,html);Assert.Contains("Recorded task creator account: actor",html);Assert.Contains(row.Route,html);
        var landing=System.Net.WebUtility.HtmlDecode(await RenderJourneyAsync(api,company,personas,auth,command.Review.InstanceId,false,accountingLanding:true));
        Assert.Contains("Oil & Gas Accounting",landing);Assert.Contains("Controller assignments: 1",landing);Assert.Contains(row.Route,landing);Assert.Contains("Company supplier invoice source queue",landing);Assert.Contains("Work-order finance sources",landing);
        var detail=await RenderJourneyAsync(api,company,personas,auth,command.Review.InstanceId,true);Assert.Contains(receipt.HistoryId,JsonSerializer.Serialize(await new BusinessProcessServiceClient(api,NullLogger<BusinessProcessServiceClient>.Instance).GetCompanyAccountingInstanceAsync(command.Review.InstanceId)));
        Assert.Contains(command.Notes,detail);Assert.Contains(type=="AP_INVOICE"?"/accounting/payables/invoices/invoice":"/procurement/orders/order",detail);
        var second=await new RepositoryAccessService(fixture.Repository).GetCompanyAccessAsync("actor",new("tenant","second"));company.TrySelect(company.Clear(),"actor",second!);Assert.Null(state.Current);Assert.Null(state.AssignedRole);
        await state.LoadAsync(["/accounting/dashboard"]);Assert.Empty(state.VisibleRows);Assert.Equal(0,state.SelectedCounts.TotalPending);
        Assert.DoesNotContain(row.Route,await RenderJourneyAsync(api,company,personas,auth,command.Review.InstanceId,false));
        Assert.Equal(16,native.Native.Writes);
        using var originalFixture=new Fixture(type=="AP_INVOICE"?"ACCOUNTANT":"BUYER");
        await using var originalHost=await AccountingSourceHttpTests.Start(configure:(services,context)=>{
            originalFixture.Configure(services,context);context.FixedProcess=native.Service;services.AddSingleton<Beep.OilandGas.Models.Core.Interfaces.IProcessService>(native.Service);services.AddSingleton<ICrossPersonaTaskRouter>(native.Native.Router);
        });
        var former=await originalHost.Client.GetFromJsonAsync<UnifiedInbox>("/api/workflow/tasks/inbox?personaCode="+originalFixture.TaskRow.TARGET_PERSONA_CODE);Assert.Equal(0,former!.Counts.TotalPending);
    }
    private sealed class NavigationFixture : NavigationManager
    {
        public NavigationFixture(string path)=>Initialize("https://render.invalid/","https://render.invalid"+path);
        protected override void NavigateToCore(string uri,bool forceLoad)=>throw new InvalidOperationException("Static fixture does not navigate.");
    }
    private sealed class NoBrowser : IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>throw new InvalidOperationException("This renderer has no browser.");
        public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>InvokeAsync<T>(id,args);
    }
    private static async Task<string> RenderJourneyAsync(ApiClient api,CompanyClientContext company,PersonaContextService personas,
        AuthenticationStateProvider auth,string processId,bool process,bool procurementLanding=false,bool accountingLanding=false)
    {
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddAuthorizationCore();
        services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<NavigationManager>(new NavigationFixture(process?"/accounting/processes/"+Uri.EscapeDataString(processId):"/tasks/inbox"));
        services.AddSingleton(company);services.AddScoped<PurchaseOrderWorkspaceState>();services.AddScoped<CompanyAccountingStepState>();services.AddSingleton<IPersonaContextService>(personas);services.AddSingleton(auth);services.AddCascadingAuthenticationState();
        services.AddSingleton<IUnifiedTaskInboxService>(new UnifiedTaskInboxService(api));
        services.AddSingleton<IBusinessProcessServiceClient>(new BusinessProcessServiceClient(api,NullLogger<BusinessProcessServiceClient>.Instance));
        services.AddSingleton<IAccountingServiceClient>(new AccountingServiceClient(api,NullLogger<AccountingServiceClient>.Instance));
        services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        if(process || procurementLanding || accountingLanding)
        {
            // Explicit source-client boundary: the complete parent still calls the real HTTP PO owner.
            var accounting=new Mock<IAccountingServiceClient>(MockBehavior.Strict);
            accounting.Setup(value=>value.GetCompanyPurchaseOrdersAsync(null)).Returns(async()=>await api.GetAsync<List<PURCHASE_ORDER>>("api/accounting/traditional/purchase-order") ?? throw new InvalidOperationException("The source queue response is unavailable."));
            accounting.Setup(value=>value.GetCompanyAPInvoicesAsync()).Returns(async()=>await api.GetAsync<List<AP_INVOICE>>("api/accounting/traditional/ap") ?? throw new InvalidOperationException("The source queue response is unavailable."));
            services.AddSingleton(accounting.Object);
        }
        if(accountingLanding)
        {
            services.AddScoped<WorkOrderFinancialWorkspaceState>();services.AddSingleton(Mock.Of<IWorkOrderServiceClient>());
            // Explicit FIELD preference boundary: company workflow evidence does not certify FIELD transport.
            var fields=new Mock<IFieldSelectionContext>(MockBehavior.Strict);
            fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync((string?)null);
            fields.Setup(value=>value.GetAvailableFieldsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<FieldListItem>());
            fields.Setup(value=>value.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(()=>new FieldResponse{FieldId="",SelectionAvailable=false,
                SelectionContext=new(company.Selection!.Scope,new("production","fingerprint"),"actor","revision",null)});
            services.AddSingleton(fields.Object);services.AddSingleton(provider=>new FieldSelectionState(company,fields.Object,provider.GetRequiredService<OilGasCallFailures>().Explain));
        }
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Auth.Tests.Fixtures.PersonaJourneyRenderHost>(ParameterView.FromDictionary(
                new Dictionary<string,object?>{{"ShowProcess",process},{"ProcessId",processId},{"ShowProcurementLanding",procurementLanding},{"ShowAccountingLanding",accountingLanding}}));return root.ToHtmlString();});
    }
    [Theory][InlineData("AP_INVOICE","ACCOUNTANT","invoice")][InlineData("PURCHASE_ORDER","BUYER","order")]
    public async Task ActualNativeCompanyHandoffsFeedExactSourceSummariesAndCompanyDrilldowns(string type,string persona,string id)
    {
        var native=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");native.Definition.ENTITY_TYPE=type;native.Native.Destinations=[persona,"CONTROLLER"];
        var intent=(await native.Intent()) with {EntityType=type,EntityId=id};
        var first=await native.Start(intent);var second=await native.Start(intent with {Company=new("tenant","second")});
        using var fixture=new Fixture(persona);
        await using var host=await AccountingSourceHttpTests.Start(configure:(services,context)=>{
            fixture.Configure(services,context);context.FixedProcess=native.Service;
            services.AddSingleton<Beep.OilandGas.Models.Core.Interfaces.IProcessService>(native.Service);services.AddSingleton<ICrossPersonaTaskRouter>(native.Native.Router);
        });
        using var company=new CompanyClientContext();var api=new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company);var auth=new AuthenticationFixture();
        using var personas=new PersonaContextService(new PersonaClient(api),auth,company,user=>user.FindFirst("party_id")?.Value);
        using var state=new PersonaTaskInboxState(new UnifiedTaskInboxService(api),company,personas);state.Start();
        foreach(var receipt in new[]{first,second})
        {
            var access=await new RepositoryAccessService(fixture.Repository).GetCompanyAccessAsync("actor",receipt.Company);
            Assert.True(company.TrySelect(company.Clear(),"actor",access!));await personas.EnsureLoadedAsync();await state.LoadAsync([personas.CurrentPersona!.DefaultRoute!]);
            state.SelectSource(type);Assert.Equal(1,state.SourceCount(type));Assert.Equal(1,state.SelectedCounts.TotalPending);
            state.SelectSummary("APPROVAL");var row=Assert.Single(state.VisibleRows);Assert.Contains(row.TaskId,receipt.TaskIds);Assert.Equal(receipt.InstanceId,row.WorkflowName);
            Assert.Equal(WorkflowTaskRoute.For("COMPANY_ACCOUNTING",receipt.InstanceId,type),row.Route);
            var html=await RenderJourneyAsync(api,company,personas,auth,receipt.InstanceId,false,persona=="BUYER");
            Assert.Contains((type=="AP_INVOICE"?"Invoice":"Purchase-order")+" workflow assignments: 1",html);Assert.Contains(row.Route,html);
            var foreign=receipt==first?second:first;Assert.DoesNotContain(WorkflowTaskRoute.For("COMPANY_ACCOUNTING",foreign.InstanceId,type),html);
            if(persona=="BUYER")Assert.Contains("Company purchase-order sources",html);
        }
        Assert.Equal(12,native.Native.Writes);
    }
    [Theory][InlineData("ACCOUNTANT","AP_INVOICE")][InlineData("BUYER","PURCHASE_ORDER")]
    public async Task ActualTypedClientsAndParentScreensFollowReviewedCompanyPersonaWork(string persona,string type)
    {
        using var fixture=new Fixture(persona);fixture.TaskRow.ENTITY_TYPE=type;fixture.TaskRow.ENTITY_ID=type=="AP_INVOICE"?"invoice":"order";
        fixture.TaskRow.PROCESS_INSTANCE_ID="process/حساب%2F";
        await using var host=await AccountingSourceHttpTests.Start(configure:fixture.Configure);host.Context.ProcessType=type;host.Context.ProcessId=fixture.TaskRow.PROCESS_INSTANCE_ID;
        using var company=new CompanyClientContext();var access=await new RepositoryAccessService(fixture.Repository).GetCompanyAccessAsync("actor",new("tenant","company"));
        Assert.True(company.TrySelect(company.Clear(),"actor",access!));
        var api=new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company);var auth=new AuthenticationFixture();
        using var personas=new PersonaContextService(new PersonaClient(api),auth,company,user=>user.FindFirst("party_id")?.Value);
        await personas.EnsureLoadedAsync();Assert.Equal(persona,personas.CurrentPersona!.Code);
        var client=new UnifiedTaskInboxService(api);using var state=new PersonaTaskInboxState(client,company,personas);state.Start();
        await state.LoadAsync([personas.CurrentPersona.DefaultRoute!]);var row=Assert.Single(state.VisibleRows);
        var inboxHtml=await RenderJourneyAsync(api,company,personas,auth,row.WorkflowName,false);
        Assert.Contains(row.Route,inboxHtml);Assert.Contains("Assigned role: ConfiguredCompanyRole",inboxHtml);Assert.Contains("Pending work: 1",inboxHtml);
        // Each static render owns its own popover provider. It does not issue a browser navigation.
        await personas.EnsureLoadedAsync();
        var processHtml=await RenderJourneyAsync(api,company,personas,auth,row.WorkflowName,true);
        Assert.Contains(type=="AP_INVOICE"?"/accounting/payables/invoices/invoice":"/procurement/orders/order",processHtml);
        Assert.Contains("Company accounting",processHtml);Assert.Contains("ConfiguredCompanyRole",processHtml);
        company.Clear();Assert.Null(state.Current);Assert.Null(personas.CurrentPersona);
        Assert.True(host.Context.Reads>0);
    }
    [Fact] public async Task ActualInboxParentShowsFailedOwnershipReadWithoutSuccessfulCountsOrLinks()
    {
        using var fixture=new Fixture();await using var host=await AccountingSourceHttpTests.Start(configure:fixture.Configure);host.Context.ForeignNative=true;
        using var company=new CompanyClientContext();var access=await new RepositoryAccessService(fixture.Repository).GetCompanyAccessAsync("actor",new("tenant","company"));
        Assert.True(company.TrySelect(company.Clear(),"actor",access!));
        var api=new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company);var auth=new AuthenticationFixture();
        using var personas=new PersonaContextService(new PersonaClient(api),auth,company,user=>user.FindFirst("party_id")?.Value);
        await personas.EnsureLoadedAsync();
        var html=await RenderJourneyAsync(api,company,personas,auth,"process",false);
        Assert.Contains("Reload company access and the inbox",html);Assert.DoesNotContain("Pending work: 1",html);
        Assert.DoesNotContain("/accounting/processes/process",html);Assert.DoesNotContain("private provider",html);
    }
    [Theory][InlineData("AP_INVOICE")][InlineData("PURCHASE_ORDER")]
    public async Task RealCompanyRoleWorkspaceTaskRoutesToOwnedProcessAndSource(string type)
    {
        var persona=type=="AP_INVOICE"?"ACCOUNTANT":"BUYER";
        using var fixture=new Fixture(persona);fixture.TaskRow.ENTITY_TYPE=type;fixture.TaskRow.ENTITY_ID=type=="AP_INVOICE"?"invoice":"order";
        fixture.TaskRow.PROCESS_INSTANCE_ID="process/حساب%2F";
        await using var host=await AccountingSourceHttpTests.Start(configure:fixture.Configure);host.Context.ProcessType=type;host.Context.ProcessId=fixture.TaskRow.PROCESS_INSTANCE_ID;
        foreach(var company in new[]{"company","second"})
        {
            host.Client.DefaultRequestHeaders.Remove("X-Test-Company");host.Client.DefaultRequestHeaders.Add("X-Test-Company",company);
            var result=await host.Client.GetFromJsonAsync<UnifiedInbox>("/api/workflow/tasks/inbox?personaCode="+persona);var row=Assert.Single(result!.CriticalTasks);
            Assert.Equal(new("tenant",company),result.Company);Assert.Equal("actor",result.ActorUserId);Assert.Equal(persona,result.PersonaCode);
            Assert.Equal("COMPANY_ACCOUNTING",row.ScopeKind);Assert.Equal("ConfiguredCompanyRole",row.AssignedRole);Assert.Equal(1,result.Counts.Reviews);
            Assert.Equal(WorkflowTaskRoute.For(row.ScopeKind,row.WorkflowName,row.EntityType),row.Route);
            var detail=await host.Client.GetFromJsonAsync<Beep.OilandGas.Models.Processes.ProcessInstance>("/api/accounting/processes/"+Uri.EscapeDataString(row.WorkflowName));
            Assert.Equal(row.EntityId,detail!.EntityId);
        }
        host.Context.Resources.VerifyNoOtherCalls();
        Assert.DoesNotContain(host.Context.Source!.Invocations,call=>new[]{"Insert","Update","Delete","BeginTransaction","Commit"}.Any(prefix=>call.Method.Name.StartsWith(prefix,StringComparison.Ordinal)));
    }
    [Theory][InlineData("anonymous",401)][InlineData("persona",403)][InlineData("membership",403)][InlineData("scopedRole",403)][InlineData("standardRole",200)]
    public async Task CompanyAndPersonaAuthorityGatesTaskAndNativeReads(string fault,int status)
    {
        using var fixture=new Fixture();
        if(fault=="membership"){foreach(var row in fixture.Repository.Set<AppCompanyMembership>())row.EffectiveToUtc=DateTime.UtcNow.AddHours(-1);fixture.Repository.SaveChanges();}
        if(fault=="scopedRole"){fixture.Repository.Set<AppUserRoleExtension>().RemoveRange(fixture.Repository.Set<AppUserRoleExtension>());fixture.Repository.SaveChanges();}
        await using var host=await AccountingSourceHttpTests.Start(authenticate:fault!="anonymous",role:fault!="standardRole",configure:fixture.Configure);
        var response=await host.Client.GetAsync(fault=="persona"?"/api/workflow/tasks/inbox?personaCode=BUYER":Inbox);
        Assert.Equal(status,(int)response.StatusCode);Assert.Equal(0,host.Context.ProcessReads);Assert.Equal(0,host.Context.Reads);
        if(status==200)Assert.Equal(0,(await response.Content.ReadFromJsonAsync<UnifiedInbox>())!.Counts.TotalPending);
        else Assert.Equal(0,fixture.TaskReads);
    }
    [Theory][InlineData("missingOwner")][InlineData("foreignNative")][InlineData("changedTask")][InlineData("revokedRole")]
    public async Task ChangedOrForeignEvidenceIsFixedConflictWithoutPartialCounts(string fault)
    {
        using var fixture=new Fixture();
        await using var host=await AccountingSourceHttpTests.Start(configure:fixture.Configure);
        if(fault=="missingOwner")host.Context.Processes.Setup(value=>value.GetProcessInstanceAsync("process")).ReturnsAsync(new Beep.OilandGas.Models.Processes.ProcessInstance{InstanceId="process",EntityType="AP_INVOICE",EntityId="invoice"});
        if(fault=="foreignNative")host.Context.ForeignNative=true;
        if(fault=="changedTask")fixture.OnTaskRead=read=>{if(read==2)fixture.TaskRow.TASK_STATUS="COMPLETED";};
        if(fault=="revokedRole")host.Context.OnProcessRead=read=>{if(read==3){fixture.Repository.Set<AppUserRoleExtension>().RemoveRange(fixture.Repository.Set<AppUserRoleExtension>());fixture.Repository.SaveChanges();}};
        var response=await host.Client.GetAsync(Inbox);Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);
        var body=await response.Content.ReadAsStringAsync();Assert.Contains("Reload company access",body);Assert.DoesNotContain("CriticalTasks",body);Assert.DoesNotContain("private provider",body);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task FieldTaskRequiresInitialAndClosingFieldResourceRead(bool revoke)
    {
        using var fixture=new Fixture();await using var host=await AccountingSourceHttpTests.Start(configure:fixture.Configure);host.Context.FieldProcess=true;
        host.Context.Resources.SetupSequence(value=>value.CheckAssetAccessAsync("actor","unrelated","FIELD","READ",null))
            .ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=true})
            .ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=!revoke});
        host.Context.Resources.Setup(value=>value.CheckAssetAccessAsync("actor","invoice","AP_INVOICE","READ",null))
            .ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=true});
        var response=await host.Client.GetAsync(Inbox);Assert.Equal(revoke?HttpStatusCode.Forbidden:HttpStatusCode.OK,response.StatusCode);
        if(!revoke){var row=Assert.Single((await response.Content.ReadFromJsonAsync<UnifiedInbox>())!.CriticalTasks);Assert.Equal("FIELD",row.ScopeKind);Assert.StartsWith("/ppdm39/process/instances/",row.Route);}
        Assert.Equal(0,host.Context.Reads);
    }
    [Theory][InlineData("role")][InlineData("assignee")][InlineData("completed")]
    public async Task ClosingCompanyAssignmentChangeCannotPublishAnOldTask(string fault)
    {
        using var fixture=new Fixture();await using var host=await AccountingSourceHttpTests.Start(configure:fixture.Configure);
        host.Context.OnProcessRead=read=>{if(read==4){
            if(fault=="role")host.Context.TaskStepRole="OtherCompanyRole";
            if(fault=="assignee")host.Context.TaskStepAssignee="another actor";
            if(fault=="completed")host.Context.TaskStepStatus=Beep.OilandGas.Models.Processes.StepStatus.COMPLETED;
        }};
        var response=await host.Client.GetAsync(Inbox);Assert.Equal(HttpStatusCode.Forbidden,response.StatusCode);
        Assert.DoesNotContain("task",await response.Content.ReadAsStringAsync());
    }
    [Theory]
    [InlineData("unspecified", "2026-01-01T00%3A00%3A00", 400, 0)]
    [InlineData("unknown-date", "2026-01-01T00%3A00%3A00Z", 200, 0)]
    [InlineData("utc-date", "2026-01-01T00%3A00%3A00Z", 200, 1)]
    public async Task FilteredTasksRequireUtcCutoffAndExcludeUnconfirmedDates(string mode, string cutoff, int status, int count)
    {
        using var fixture = new Fixture();
        fixture.TaskRow.DUE_DATE = mode == "unknown-date" ? new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Unspecified) : new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await using var host = await AccountingSourceHttpTests.Start(configure: fixture.Configure);
        var response = await host.Client.GetAsync("/api/workflow/tasks?personaCode=ACCOUNTANT&dueBefore=" + cutoff + "&sortBy=dueDate");
        Assert.Equal(status, (int)response.StatusCode);
        if (status == 200) Assert.Equal(count, (await response.Content.ReadFromJsonAsync<List<UnifiedTask>>())!.Count);
    }

    [Theory]
    [InlineData("none", "UNSCHEDULED", 0)]
    [InlineData("ambiguous", "UNCONFIRMED", 0)]
    [InlineData("past", "BREACHED", 1)]
    [InlineData("future", "ON_TRACK", 0)]
    public async Task InboxClassifiesOnlyExplicitUtcDeadlines(string mode, string expected, int overdue)
    {
        using var fixture = new Fixture();
        fixture.TaskRow.DUE_DATE = mode switch
        {
            "ambiguous" => new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
            "past" => new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            "future" => new DateTime(2100, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            _ => null
        };
        await using var host = await AccountingSourceHttpTests.Start(configure: fixture.Configure);
        var response = await host.Client.GetAsync(Inbox); Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var inbox = (await response.Content.ReadFromJsonAsync<UnifiedInbox>())!; var task = Assert.Single(inbox.CriticalTasks);
        Assert.Equal(expected, task.SlaStatus); Assert.Equal(overdue, inbox.Counts.Overdue);
    }

    [Theory][InlineData("counts")][InlineData("list")]
    public async Task CountsAndFilteredListUseSameReviewedCompanyAssignments(string endpoint)
    {
        using var fixture=new Fixture();await using var host=await AccountingSourceHttpTests.Start(configure:fixture.Configure);
        var response=await host.Client.GetAsync(endpoint=="counts"?"/api/workflow/tasks/counts?personaCode=ACCOUNTANT":"/api/workflow/tasks?personaCode=ACCOUNTANT&taskType=REVIEW&pageSize=1");
        Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        if(endpoint=="counts")Assert.Equal(1,(await response.Content.ReadFromJsonAsync<InboxCounts>())!.TotalPending);
        else Assert.Equal("COMPANY_ACCOUNTING",Assert.Single((await response.Content.ReadFromJsonAsync<List<UnifiedTask>>())!).ScopeKind);
    }
}
#endif

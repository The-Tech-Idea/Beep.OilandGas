using Bunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class ConstructionEvidenceScreenTests
{
    [Fact]
    public void SeededFacilitiesAndDevelopmentPersonaRoutesResolveToEvidenceWorkspace()
    {
        var page=typeof(Beep.OilandGas.Web.Pages.PPDM39.Development.ConstructionProgress);
        var routes=page.GetCustomAttributes(typeof(Microsoft.AspNetCore.Components.RouteAttribute),true).Cast<Microsoft.AspNetCore.Components.RouteAttribute>().Select(route=>route.Template).ToArray();
        foreach(var route in new[]{"/development","/development/construction","/facilities/dashboard","/lifecycle/facilities/dashboard","/lifecycle/development/dashboard"})Assert.Contains(route,routes);
    }
    private const string Field="field/الكويت";
    private static FieldResponse Selection()=>new(){FieldId=Field,FieldName="Recorded field",SelectionAvailable=true,SelectionContext=new(new("tenant","company"),new("core","binding"),"engineer","revision",Field)};
    private static FACILITY Row(string field=Field)=>new(){FACILITY_ID="facility-1",FACILITY_TYPE="PROCESSING",PRIMARY_FIELD_ID=field,CURRENT_OPERATOR="partner-operator",FACILITY_LONG_NAME="Recorded <plant>",ACTIVE_IND="Y"};
    private static void Configure(TestContext ui,CompanyClientContext company,IFieldSelectionContext fields,IDevelopmentServiceClient api,IPersonaContextService? personas=null,IAfeServiceClient? afe=null,IUnifiedTaskInboxService? inbox=null)
    {
        ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields);ui.Services.AddSingleton(api);ui.Services.AddSingleton<IPersonaContextService>(personas??Mock.Of<IPersonaContextService>());ui.Services.AddSingleton<IAfeServiceClient>(afe??Mock.Of<IAfeServiceClient>());ui.Services.AddSingleton<IUnifiedTaskInboxService>(inbox??Mock.Of<IUnifiedTaskInboxService>());ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
    }
    [Theory][InlineData(true)][InlineData(false)]
    public void ConstructionWorkspaceLoadsOnlyItsOwnPersonaAssignments(bool matchesWorkspace)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"engineer",new(new("tenant","company"),["Engineer"],["DevelopmentPlanning.View"])));
        var personas=new Mock<IPersonaContextService>();personas.SetupGet(x=>x.CurrentPersona).Returns(new AppPersona{Code="FACILITIES_ENGINEER",Name="Facilities engineer",DefaultRoute=matchesWorkspace?"/facilities/dashboard":"/drilling/workspace"});personas.SetupGet(x=>x.CurrentProfile).Returns(new AppUserPersona{UserId="engineer",PersonaCode="FACILITIES_ENGINEER"});personas.Setup(x=>x.EnsureLoadedAsync()).Returns(Task.CompletedTask);
        var inbox=new Mock<IUnifiedTaskInboxService>();inbox.Setup(x=>x.GetInboxAsync("FACILITIES_ENGINEER")).ReturnsAsync(new UnifiedInbox{Company=new("tenant","company"),ActorUserId="engineer",PersonaCode="FACILITIES_ENGINEER",CriticalTasks=[new(){TaskId="construction-task",TaskType="REVIEW",Priority=1,WorkflowName="construction-process",StepName="review",ScopeKind="FIELD",AssignedRole="Engineer",EntityId="facility-1",EntityType="FACILITY",Route="/ppdm39/process/instances/construction-process"}],Counts=new(){TotalPending=1,Critical=1,Reviews=1}});
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());var api=new Mock<IDevelopmentServiceClient>();api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).ReturnsAsync([]);
        using var ui=new TestContext();Configure(ui,company,fields.Object,api.Object,personas.Object,inbox:inbox.Object);var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Development.ConstructionProgress>();
        if(matchesWorkspace){page.WaitForAssertion(()=>Assert.Contains("construction-process",page.Markup));Assert.Contains("Pending work: 1",page.Markup);Assert.Contains("cannot advance construction, commissioning or handover",page.Markup);company.Clear();page.WaitForAssertion(()=>Assert.DoesNotContain("construction-process",page.Markup));}
        else {page.WaitForAssertion(()=>Assert.Contains("mapped to another landing page",page.Markup));inbox.Verify(x=>x.GetInboxAsync(It.IsAny<string>()),Times.Never);}
    }
    [Fact]
    public void ConstructionEvidenceUsesAuthorizedSourcesWithoutInventingProgress()
    {
        Assert.NotEmpty(typeof(Beep.OilandGas.Web.Pages.PPDM39.Development.ConstructionProgress).GetCustomAttributes(typeof(AuthorizeAttribute), true));
        using var company=new CompanyClientContext();var fields=new Mock<IFieldSelectionContext>();var api=new Mock<IDevelopmentServiceClient>();using var ui=new TestContext();Configure(ui,company,fields.Object,api.Object);
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Development.ConstructionProgress>();
        Assert.Contains("not yet available",page.Markup);Assert.Contains("Choose a company",page.Markup);Assert.Contains("do not by themselves accept construction",page.Markup);
        var links=page.FindAll("a").Select(a=>a.GetAttribute("href")).ToArray();foreach(var expected in new[]{"/development/fdp","/development/well-design","/ppdm39/operations/drillingoperations","/ppdm39/development/facilities","/economics/afe","/ppdm39/hse/incidents","/tasks/inbox"})Assert.Contains(expected,links);
        foreach(var invented in new[]{"On Track","3% Over","Zero incidents","98% punch closed","FIRST OIL","142","TRIR"})Assert.DoesNotContain(invented,page.Markup);
        api.Verify(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),It.IsAny<string>()),Times.Never);
    }
    [Theory][InlineData("valid")][InlineData("empty")][InlineData("foreign")][InlineData("denied")][InlineData("permission")]
    public void LandingShowsOnlyCurrentAuthorizedFacilitySource(string mode)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"engineer",new(new("tenant","company"),["Engineer"],mode=="permission"?[]:["DevelopmentPlanning.View"])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());var api=new Mock<IDevelopmentServiceClient>();
        if(mode=="denied")api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).ThrowsAsync(new HttpRequestException("Unavailable"));else api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).ReturnsAsync(mode=="empty"?[]:[Row(mode=="foreign"?"other":Field)]);
        using var ui=new TestContext();Configure(ui,company,fields.Object,api.Object);var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Development.ConstructionProgress>();
        if(mode=="valid")
        {
            page.WaitForAssertion(()=>Assert.Contains("facility-1",page.Markup));Assert.Contains("partner-operator",page.Markup);Assert.Contains("Recorded &lt;plant&gt;",page.Markup);Assert.DoesNotContain("Recorded <plant>",page.Markup);Assert.Contains("does not establish construction progress",page.Markup);
            var href=page.Find("a[aria-label='Open facility facility-1, type PROCESSING']").GetAttribute("href");Assert.Equal("/ppdm39/development/facilities?tenantId=tenant&companyId=company&fieldId="+Uri.EscapeDataString(Field)+"&facilityId=facility-1&facilityType=PROCESSING",href);
            fields.Raise(x=>x.CurrentFieldChanged+=null!,"other");page.WaitForAssertion(()=>Assert.DoesNotContain("facility-1",page.Markup));company.Clear();
        }
        else if(mode=="empty")page.WaitForAssertion(()=>Assert.Contains("No facility rows were returned",page.Markup));
        else if(mode=="permission"){Assert.Contains("does not have permission",page.Markup);api.Verify(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),It.IsAny<string>()),Times.Never);}
        else {page.WaitForAssertion(()=>Assert.Contains("Development facility evidence could not be confirmed",page.Markup));Assert.DoesNotContain("facility-1",page.Markup);Assert.DoesNotContain("No facility rows were returned",page.Markup);}
    }
    [Theory][InlineData(true)][InlineData(false)]
    public void ConstructionLandingShowsOnlyAuthorizedCompanyInvestmentWork(bool authorized)
    {
        using var fixture=new DevelopmentInvestmentStateTests.Fixture();if(!authorized){var selected=fixture.Base.Company.Selection!;fixture.Base.Company.TrySelect(fixture.Base.Company.Clear(),selected.UserId,new(selected.Scope,selected.Roles.ToArray(),["DevelopmentPlanning.View"]));}
        var scope=fixture.Review.Current.Source.Review.Scope;var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{FieldId=scope.FieldId,FieldName="Recorded field",SelectionAvailable=true,SelectionContext=new(scope.Company,scope.CoreDatabase,scope.ActorUserId,"revision",scope.FieldId)});
        var development=new Mock<IDevelopmentServiceClient>();development.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),It.IsAny<string>())).ReturnsAsync([]);
        using var ui=new TestContext();Configure(ui,fixture.Base.Company,fields.Object,development.Object,fixture.Base.Baseline.Reader.Personas.Object,fixture.Base.Client.Object);
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Development.ConstructionProgress>();
        if(authorized)
        {
            page.WaitForAssertion(()=>Assert.Contains("AFE-independent-1",page.Markup));Assert.Contains("Investment evidence across accessible FIELDs",page.Markup);Assert.Contains("25.125 KWD",page.Markup);Assert.Contains("Awaiting independent investment decision",page.Markup);Assert.Contains("processId=development-process",page.Markup);
            var selected=fixture.Base.Company.Selection!;fixture.Base.Company.TrySelect(fixture.Base.Company.Clear(),selected.UserId,new(selected.Scope,selected.Roles.ToArray(),["DevelopmentPlanning.View"]));
            page.WaitForAssertion(()=>Assert.DoesNotContain("AFE-independent-1",page.Markup));Assert.DoesNotContain("Company development investment work",page.Markup);
        }
        else {Assert.DoesNotContain("Company development investment work",page.Markup);fixture.Base.Client.Verify(x=>x.QueueAsync(It.IsAny<CancellationToken>()),Times.Never);}
    }

    [Fact]
    public async Task LateFacilityResponseCannotAppearUnderAnotherField()
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"engineer",new(new("tenant","company"),["Engineer"],["DevelopmentPlanning.View"])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());var pending=new TaskCompletionSource<List<FACILITY>>(TaskCreationOptions.RunContinuationsAsynchronously);var api=new Mock<IDevelopmentServiceClient>();api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).Returns(pending.Task);
        using var ui=new TestContext();Configure(ui,company,fields.Object,api.Object);var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Development.ConstructionProgress>();page.WaitForAssertion(()=>Assert.NotEmpty(page.FindAll("[aria-label=\"Loading facility records\"]")));
        fields.Raise(x=>x.CurrentFieldChanged+=null!,"other");pending.SetResult([Row()]);await page.InvokeAsync(()=>Task.CompletedTask);page.WaitForAssertion(()=>Assert.DoesNotContain("facility-1",page.Markup));
    }
    [Fact]
    public void ConstructionEquipmentUsesExactFacilityAndClearsOnFieldChange()
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"engineer",new(new("tenant","company"),["Engineer"],["DevelopmentPlanning.View"])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());var api=new Mock<IDevelopmentServiceClient>();api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).ReturnsAsync([Row(),new FACILITY{FACILITY_ID="facility-1",FACILITY_TYPE="STORAGE",PRIMARY_FIELD_ID=Field}]);
        api.Setup(x=>x.GetFacilityEquipmentAsync("facility-1","PROCESSING",Field,It.IsAny<CancellationToken>())).ReturnsAsync([new DevelopmentFacilityEquipmentItem("pump-1",1,"recorded")]);
        api.Setup(x=>x.GetFacilityEquipmentAsync("facility-1","STORAGE",Field,It.IsAny<CancellationToken>())).ReturnsAsync([]);
        using var ui=new TestContext();Configure(ui,company,fields.Object,api.Object);var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Development.ConstructionProgress>();
        page.WaitForAssertion(()=>Assert.NotEmpty(page.FindAll("[aria-label='Inspect equipment for facility-1, type PROCESSING']")));
        page.Find("[aria-label='Inspect equipment for facility-1, type PROCESSING']").Click();page.WaitForAssertion(()=>Assert.Contains("pump-1",page.Markup));Assert.Contains("not proof of installation",page.Markup);
        page.Find("[aria-label='Inspect equipment for facility-1, type STORAGE']").Click();page.WaitForAssertion(()=>Assert.Contains("No active equipment rows were returned",page.Markup));Assert.DoesNotContain("pump-1",page.Markup);
        fields.Raise(x=>x.CurrentFieldChanged+=null!,"other");page.WaitForAssertion(()=>Assert.DoesNotContain("Equipment recorded for",page.Markup));
    }
    [Fact]
    public async Task LateOrFailedEquipmentReadCannotBecomeAnotherFacilitysEvidence()
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"engineer",new(new("tenant","company"),["Engineer"],["DevelopmentPlanning.View"])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());var api=new Mock<IDevelopmentServiceClient>();api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).ReturnsAsync([Row(),new FACILITY{FACILITY_ID="facility-2",FACILITY_TYPE="STORAGE",PRIMARY_FIELD_ID=Field}]);
        var pending=new TaskCompletionSource<List<DevelopmentFacilityEquipmentItem>>(TaskCreationOptions.RunContinuationsAsynchronously);
        api.Setup(x=>x.GetFacilityEquipmentAsync("facility-1","PROCESSING",Field,It.IsAny<CancellationToken>())).Returns(pending.Task);
        api.Setup(x=>x.GetFacilityEquipmentAsync("facility-2","STORAGE",Field,It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException("unavailable"));
        using var ui=new TestContext();Configure(ui,company,fields.Object,api.Object);var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Development.ConstructionProgress>();
        page.WaitForAssertion(()=>Assert.NotEmpty(page.FindAll("[aria-label='Inspect equipment for facility-1, type PROCESSING']")));page.Find("[aria-label='Inspect equipment for facility-1, type PROCESSING']").Click();
        page.WaitForAssertion(()=>Assert.NotEmpty(page.FindAll("[aria-label='Loading facility equipment']")));page.Find("[aria-label='Inspect equipment for facility-2, type STORAGE']").Click();
        page.WaitForAssertion(()=>Assert.Contains("Facility equipment evidence could not be confirmed",page.Markup));pending.SetResult([new("late-pump",1,"source")]);await page.InvokeAsync(()=>Task.CompletedTask);
        Assert.DoesNotContain("late-pump",page.Markup);Assert.DoesNotContain("No active equipment rows were returned",page.Markup);Assert.Contains("facility-2 / STORAGE",page.Markup);
    }
}

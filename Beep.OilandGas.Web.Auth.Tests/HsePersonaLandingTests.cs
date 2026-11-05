using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using Bunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Pages.PPDM39.HSE;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data.Compliance;
using Beep.OilandGas.Web.Pages.PPDM39.BusinessProcess;
using Beep.OilandGas.Web.Pages.PPDM39.Compliance;
using Beep.OilandGas.Web.Pages.PPDM39.Permits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class HsePersonaLandingTests
{
    [Fact]
    public void ComplianceWorklistSearchesRecordedIdentityAndDescription()
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],[])));
        var context=new FieldSelectionReview(new("tenant","company"),new("core","binding"),"actor","revision","field");var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{FieldId="field",SelectionAvailable=true,SelectionContext=context});
        var link=new RecordedObligationComponent(1,"FIELD",null,null,new DateTime(2026,3,2),"link");var first=new RecordedFieldObligation("case-one",1,"REPORT","الكويت",null,null,null,[link]);var second=new RecordedFieldObligation("case-two",2,"FILING","different",null,null,null,[link]);
        var api=new Mock<IComplianceServiceClient>();api.Setup(x=>x.GetRecordedFieldObligationsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldObligationSourceReview(context,[first,second]));
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddLogging();ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton(api.Object);
        var page=ui.RenderComponent<ComplianceObligations>();page.WaitForAssertion(()=>Assert.Contains("case-two",page.Markup));page.Find("input").Input("الكويت");page.WaitForAssertion(()=>Assert.DoesNotContain("case-two",page.Markup));Assert.Contains("case-one",page.Markup);
        page.Find("input").Input("not-present");page.WaitForAssertion(()=>Assert.Contains("No recorded obligations match the search",page.Markup));
    }
    [Theory][InlineData("valid")][InlineData("foreign-field")][InlineData("duplicate")][InlineData("missing-parent")][InlineData("wrong-sequence")]
    public void PpdmObligationProjectionPreservesOnlyExactRecordedFieldRelationships(string mode)
    {
        var context=new FieldSelectionReview(new("tenant","company"),new("core","binding"),"actor","revision","field");
        var parent=new Beep.OilandGas.PPDM39.Models.OBLIGATION{OBLIGATION_ID="case",OBLIGATION_SEQ_NO=4,ACTIVE_IND="Y",OBLIGATION_TYPE="RECORDED",DESCRIPTION="description",CRITICAL_DATE=new DateTime(2026,1,2),FULFILLED_IND="N"};
        var link=new Beep.OilandGas.PPDM39.Models.OBLIGATION_COMPONENT{OBLIGATION_ID="case",OBLIGATION_SEQ_NO=mode=="wrong-sequence"?5:4,COMPONENT_OBS_NO=3,ACTIVE_IND="Y",FIELD_ID=mode=="foreign-field"?"other":"field",OBLIGATION_COMPONENT_TYPE="FIELD",DESCRIPTION="link"};
        var links=mode=="duplicate"?new[]{link,link}:new[]{link};var parents=mode=="missing-parent"?Array.Empty<Beep.OilandGas.PPDM39.Models.OBLIGATION>():[parent];
        if(mode!="valid"){Assert.Throws<InvalidOperationException>(()=>Beep.OilandGas.PPDM39.DataManagement.Services.Compliance.ComplianceFieldObligationMapper.Project(context,links,parents));return;}
        var review=Beep.OilandGas.PPDM39.DataManagement.Services.Compliance.ComplianceFieldObligationMapper.Project(context,links,parents);var record=Assert.Single(review.Obligations);
        Assert.Equal(context,review.Context);Assert.Equal(4,record.ObligationSequence);Assert.Equal("RECORDED",record.ObligationType);Assert.Equal(new DateTime(2026,1,2),record.CriticalDate);Assert.Equal("N",record.FulfilledIndicator);Assert.Equal(3,Assert.Single(record.Components).Observation);
    }
    [Fact]
    public async Task ComplianceNativeModelMismatchRefusesBeforeDatasourceRead()
    {
        var editor=new Mock<TheTechIdea.Beep.Editor.IDMEEditor>();var metadata=new Mock<Beep.OilandGas.PPDM39.Core.Metadata.IPPDMMetadataRepository>();
        var service=new Beep.OilandGas.PPDM39.DataManagement.Services.Compliance.ComplianceService(editor.Object,Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>(),Mock.Of<Beep.OilandGas.PPDM39.Core.IPPDM39DefaultsRepository>(),metadata.Object,"bound",Microsoft.Extensions.Logging.Abstractions.NullLogger<Beep.OilandGas.PPDM39.DataManagement.Services.Compliance.ComplianceService>.Instance);
        await Assert.ThrowsAsync<Beep.OilandGas.PPDM39.Core.NativeSchemaCompatibilityException>(()=>service.GetByIdAsync("case","field"));
        await Assert.ThrowsAsync<Beep.OilandGas.PPDM39.Core.NativeSchemaCompatibilityException>(()=>service.GetAllObligationsAsync("field",2026));
        editor.Verify(x=>x.GetDataSource(It.IsAny<string>()),Times.Never);metadata.Verify(x=>x.GetTableMetadataAsync(It.IsAny<string>()),Times.Never);
    }
    [Theory][InlineData("valid")][InlineData("foreign")][InlineData("payment")][InlineData("missing")]
    public void ObligationDetailShowsOnlyMatchingFieldAndPayments(string mode)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],[])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{FieldId="field",SelectionAvailable=true,SelectionContext=new(new("tenant","company"),new("core","binding"),"actor","revision","field")});
        var detail=new ObligationDetailModel{ObligationId="case",FieldId=mode=="foreign"?"other":"field",Description="Recorded obligation",Payments=mode=="payment"?[new ObligationPayment("payment","other","ROYALTY",DateTime.UtcNow,"KWD",1,1,0,null)]:[]};
        var api=new Mock<IComplianceServiceClient>();api.Setup(x=>x.GetObligationAsync("case",It.IsAny<CancellationToken>())).ReturnsAsync(mode=="missing"?null:detail);
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddLogging();ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton(api.Object);
        var page=ui.RenderComponent<ObligationDetail>(parameters=>parameters.Add(x=>x.ObligationId,"case"));
        if(mode=="valid"){page.WaitForAssertion(()=>Assert.Contains("Recorded obligation",page.Markup));fields.Raise(x=>x.CurrentFieldChanged+=null!,"other");page.WaitForAssertion(()=>Assert.DoesNotContain("Recorded obligation",page.Markup));}
        else {page.WaitForAssertion(()=>Assert.Contains(mode=="missing"?"unavailable in the current FIELD":"could not be confirmed",page.Markup));Assert.DoesNotContain("Recorded obligation",page.Markup);}
    }
    [Theory][InlineData("valid")][InlineData("foreign")][InlineData("changed")][InlineData("missing")]
    public async Task ComplianceDetailApiRequiresTheSelectedField(string mode)
    {
        var field="field";var orchestrator=new Mock<Beep.OilandGas.Models.Core.Interfaces.IFieldOrchestrator>();orchestrator.SetupGet(x=>x.CurrentFieldId).Returns(()=>field);
        var service=new Mock<Beep.OilandGas.Models.Core.Interfaces.IComplianceService>();service.Setup(x=>x.GetByIdAsync("case","field")).ReturnsAsync(()=>{if(mode=="changed")field="other";return mode=="missing"?null:new ObligationDetailModel{ObligationId="case",FieldId=mode=="foreign"?"other":"field"};});
        var controller=new Beep.OilandGas.ApiService.Controllers.Compliance.ComplianceController(service.Object,Mock.Of<Beep.OilandGas.Models.Core.Interfaces.IRoyaltyCalculationService>(),Mock.Of<Beep.OilandGas.Models.Core.Interfaces.IGHGReportingService>(),orchestrator.Object);
        if(mode is "foreign" or "changed")await Assert.ThrowsAsync<InvalidOperationException>(()=>controller.GetByIdAsync("case"));
        else {var answer=await controller.GetByIdAsync("case");if(mode=="missing")Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundObjectResult>(answer.Result);else Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(answer.Result);}
        service.Verify(x=>x.GetByIdAsync("case","field"),Times.Once());
    }
    [Theory][InlineData("accepted")][InlineData("missing")][InlineData("foreign")][InlineData("field-changed")]
    public async Task ComplianceProcessDoesNotClaimRegulatorSubmission(string mode)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Compliance"],[])));
        var original=new FieldSelectionReview(new("tenant","company"),new("core","binding"),"actor","revision","field");var current=original;
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(()=>new FieldResponse{FieldId="field",SelectionAvailable=true,SelectionContext=current});
        var api=new Mock<IBusinessProcessServiceClient>();api.Setup(x=>x.SubmitComplianceReportAsync(It.IsAny<Beep.OilandGas.Models.Data.Process.ComplianceReportRequest>(),It.IsAny<CancellationToken>())).ReturnsAsync(mode=="missing"?null:new Beep.OilandGas.Models.Processes.ProcessInstance{InstanceId="process",EntityId="report",EntityType="COMPLIANCE_REPORT",FieldId=mode=="foreign"?"other":"field"});
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddLogging();ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton(api.Object);
        var page=ui.RenderComponent<ComplianceReport>();page.WaitForAssertion(()=>Assert.Contains("Selected FIELD",page.Markup));Assert.Contains("does not send a report to a regulator",page.Markup);
        var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;var type=typeof(ComplianceReport);type.GetField("_jurisdiction",flags)!.SetValue(page.Instance,"الكويت");type.GetField("_type",flags)!.SetValue(page.Instance,"PRODUCTION_REPORT");type.GetField("_start",flags)!.SetValue(page.Instance,new DateTime(2026,1,1));type.GetField("_end",flags)!.SetValue(page.Instance,new DateTime(2026,2,1));
        if(mode=="field-changed")current=original with{Revision="changed"};
        await page.InvokeAsync(()=>(Task)type.GetMethod("SubmitAsync",flags)!.Invoke(page.Instance,null)!);await page.InvokeAsync(()=>page.Render());
        Assert.Contains(mode=="accepted"?"No regulator submission is confirmed":mode=="field-changed"?"Company or FIELD changed":"process outcome is unconfirmed",page.Markup);
        api.Verify(x=>x.SubmitComplianceReportAsync(It.Is<Beep.OilandGas.Models.Data.Process.ComplianceReportRequest>(r=>r.JurisdictionTag=="الكويت"),It.IsAny<CancellationToken>()),mode=="field-changed"?Times.Never():Times.Once());
        if(mode is "missing" or "foreign"){await page.InvokeAsync(()=>(Task)type.GetMethod("SubmitAsync",flags)!.Invoke(page.Instance,null)!);api.Verify(x=>x.SubmitComplianceReportAsync(It.IsAny<Beep.OilandGas.Models.Data.Process.ComplianceReportRequest>(),It.IsAny<CancellationToken>()),Times.Once());}
    }
    [Fact]
    public async Task LateCompliancePopulationCannotAppearAfterCompanyChange()
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],[])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{FieldId="field",SelectionAvailable=true,SelectionContext=new(new("tenant","company"),new("core","binding"),"actor","revision","field")});
        var response=new TaskCompletionSource<FieldObligationSourceReview>(TaskCreationOptions.RunContinuationsAsynchronously);var api=new Mock<IComplianceServiceClient>();api.Setup(x=>x.GetRecordedFieldObligationsAsync(It.IsAny<CancellationToken>())).Returns(response.Task);
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddLogging();ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton(api.Object);
        var page=ui.RenderComponent<ComplianceObligations>();company.Clear();response.SetResult(new(new(new("tenant","company"),new("core","binding"),"actor","revision","field"),[new("case",1,"OLD COMPANY RECORD",null,null,null,null,[new(1,null,null,null,null,null)])]));await page.InvokeAsync(()=>Task.CompletedTask);
        page.WaitForAssertion(()=>Assert.Contains("Company or FIELD changed",page.Markup));Assert.DoesNotContain("OLD COMPANY RECORD",page.Markup);
    }
    [Theory][InlineData("valid")][InlineData("duplicate")][InlineData("foreign")]
    public void ComplianceListShowsRecordedFactsOnlyForCurrentField(string mode)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],[])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{FieldId="field",FieldName="North field",SelectionAvailable=true,SelectionContext=new(new("tenant","company"),new("core","binding"),"actor","revision","field")});
        var context=new FieldSelectionReview(new("tenant","company"),new("core","binding"),"actor","revision",mode=="foreign"?"other":"field");var row=new RecordedFieldObligation("case",1,"RECORDED TYPE","description",null,null,null,[new(1,"FIELD",null,null,null,null)]);
        var api=new Mock<IComplianceServiceClient>();api.Setup(x=>x.GetRecordedFieldObligationsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldObligationSourceReview(context,mode=="duplicate"?[row,row]:[row]));
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddLogging();ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton(api.Object);
        var page=ui.RenderComponent<ComplianceObligations>();
        if(mode=="valid"){page.WaitForAssertion(()=>Assert.Contains("RECORDED TYPE",page.Markup));Assert.Contains("not a determination of regulatory deadlines",page.Markup);fields.Raise(x=>x.CurrentFieldChanged+=null!,"other");page.WaitForAssertion(()=>Assert.DoesNotContain("RECORDED TYPE",page.Markup));}
        else {page.WaitForAssertion(()=>Assert.Contains("Obligations could not be confirmed",page.Markup));Assert.DoesNotContain("RECORDED TYPE",page.Markup);}
    }
    [Theory][InlineData("before")][InlineData("during")][InlineData("unconfirmed")]
    public async Task HazopChangesRespectReviewedFieldAndUnconfirmedResults(string mode)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],[])));
        var original=new FieldSelectionReview(new("tenant","company"),new("core","binding"),"actor","revision","field");var current=original;
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(()=>new FieldResponse{FieldId="field",SelectionAvailable=true,SelectionContext=current});
        var reply=new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);var sent=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var hse=new Mock<IHSEServiceClient>();hse.Setup(x=>x.GetHazopStudyAsync("study",It.IsAny<CancellationToken>())).ReturnsAsync(new HAZOPSummary("study","Recorded",1,0,0,0,0));hse.Setup(x=>x.GetHazopNodesAsync("study",It.IsAny<CancellationToken>())).ReturnsAsync([new HAZOPNode{NodeSeq=1,NodeName="Old node"}]);hse.Setup(x=>x.AddHazopNodeAsync("study",It.IsAny<AddNodeRequest>(),It.IsAny<CancellationToken>())).Callback(()=>sent.SetResult()).Returns(reply.Task);
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddLogging();ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton(hse.Object);
        var page=ui.RenderComponent<HAZOPStudy>(parameters=>parameters.Add(x=>x.StudyId,"study"));page.WaitForAssertion(()=>Assert.Contains("Old node",page.Markup));
        var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;typeof(HAZOPStudy).GetField("_newNodeName",flags)!.SetValue(page.Instance,"New node");
        if(mode=="before")current=original with{Revision="changed"};
        var running=page.InvokeAsync(()=>(Task)typeof(HAZOPStudy).GetMethod("SubmitNodeAsync",flags)!.Invoke(page.Instance,null)!);
        if(mode!="before"){await sent.Task.WaitAsync(TimeSpan.FromSeconds(5));if(mode=="during")company.Clear();reply.SetResult(mode=="unconfirmed"?null:"1");}
        await running;await page.InvokeAsync(()=>page.Render());page.WaitForAssertion(()=>Assert.Contains(mode=="unconfirmed"?"outcome is unconfirmed":"Company or FIELD changed",page.Markup));Assert.DoesNotContain("Old node",page.Markup);
        hse.Verify(x=>x.AddHazopNodeAsync("study",It.IsAny<AddNodeRequest>(),It.IsAny<CancellationToken>()),mode=="before"?Times.Never():Times.Once());
    }
    [Theory][InlineData("valid")][InlineData("wrong")][InlineData("duplicate")]
    public void HazopStudyRequiresRecordedStudyAndCurrentField(string mode)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],[])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{FieldId="field",SelectionAvailable=true,SelectionContext=new(new("tenant","company"),new("core","binding"),"actor","revision","field")});
        var hse=new Mock<IHSEServiceClient>();hse.Setup(x=>x.GetHazopStudyAsync("study",It.IsAny<CancellationToken>())).ReturnsAsync(new HAZOPSummary(mode=="wrong"?"other":"study","Recorded study",1,0,0,0,0));hse.Setup(x=>x.GetHazopNodesAsync("study",It.IsAny<CancellationToken>())).ReturnsAsync(mode=="duplicate"?[new HAZOPNode{NodeSeq=1,NodeName="Recorded node"},new HAZOPNode{NodeSeq=1}]:[new HAZOPNode{NodeSeq=1,NodeName="Recorded node"}]);
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddLogging();ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton(hse.Object);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        var page=ui.RenderComponent<HAZOPStudy>(parameters=>parameters.Add(x=>x.StudyId,"study"));
        if(mode=="valid"){page.WaitForAssertion(()=>Assert.Contains("Recorded node",page.Markup));fields.Raise(x=>x.CurrentFieldChanged+=null!,"other");page.WaitForAssertion(()=>Assert.DoesNotContain("Recorded node",page.Markup));Assert.Contains("Company or FIELD changed",page.Markup);}
        else {page.WaitForAssertion(()=>Assert.Contains("could not be confirmed",page.Markup));Assert.DoesNotContain("Recorded node",page.Markup);Assert.DoesNotContain("No nodes yet",page.Markup);}
    }
    private static (TestContext Ui, CompanyClientContext Company, Mock<IFieldSelectionContext> Fields, Mock<IHSEServiceClient> Incidents) IncidentLanding(List<HSEIncidentRecord> rows)
    {
        var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.View"])));
        var personas = new Mock<IPersonaContextService>(); personas.Setup(x => x.EnsureLoadedAsync()).Returns(Task.CompletedTask);
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", FieldName = "North FIELD", SelectionAvailable = true, SelectionContext = new(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field") });
        var incidents = new Mock<IHSEServiceClient>(); incidents.Setup(x => x.GetIncidentsAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(rows);
        var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging();ui.Services.AddSingleton<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>(new LandingAuth()); ui.Services.AddSingleton(company); ui.Services.AddSingleton(personas.Object); ui.Services.AddSingleton(Mock.Of<IUnifiedTaskInboxService>()); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton(incidents.Object); ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        return (ui, company, fields, incidents);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HseIncidentCountsOpenExactPagedRecordsAndClearOnContextChange(bool fieldChanged)
    {
        var rows = Enumerable.Range(1, 17).Select(i => new HSEIncidentRecord { IncidentId = $"open-{i:00}", FieldId = "field", CurrentState = IncidentState.Reported }).ToList();
        rows.Add(new() { IncidentId = "closed/one?شركة", FieldId = "field", CurrentState = IncidentState.Closed });
        rows.Add(new() { IncidentId = "cancelled", FieldId = "field", CurrentState = IncidentState.Cancelled });
        rows.Add(new() { IncidentId = "unknown", FieldId = "field", CurrentState = "UNRECOGNIZED" });
        rows.Add(new() { IncidentId = "unrecorded", FieldId = "field", CurrentState = "" });
        var fixture = IncidentLanding(rows); using var ui = fixture.Ui; using var company = fixture.Company;
        var page = ui.RenderComponent<HSEDashboard>();
        page.WaitForAssertion(() => Assert.Contains("Returned: 21", page.Markup));
        Assert.Contains("no reporting-date filter", page.Markup); Assert.Contains("Checked ", page.Markup);
        Assert.Contains("completeness is not established", page.Markup);
        string[] Links() => page.FindAll("a[href^='/ppdm39/hse/incident-detail?incidentId=']").Select(a => a.GetAttribute("href")!).ToArray();
        async Task Click(string label) => await page.FindAll("button").Single(b => b.TextContent.Trim() == label).ClickAsync(new());
        Assert.Equal(10, Links().Length); Assert.Contains("page 1 of 3", page.Markup);
        await Click("Open: 17"); Assert.Equal(10, Links().Length); Assert.All(Links(), link => Assert.Contains("incidentId=open-", link));
        Assert.Equal("true", page.FindAll("button").Single(b => b.TextContent.Trim() == "Open: 17").GetAttribute("aria-pressed"));
        await Click("Next incidents"); Assert.Equal(7, Links().Length); Assert.Contains("page 2 of 2", page.Markup); Assert.Contains(Links(), link => link.EndsWith("open-17"));
        await Click("Closed: 1"); Assert.Single(Links()); Assert.Contains("incidentId=" + Uri.EscapeDataString("closed/one?شركة"), Links()[0]); Assert.Contains("page 1 of 1", page.Markup);
        await Click("Cancelled: 1"); Assert.Single(Links()); Assert.EndsWith("incidentId=cancelled", Links()[0]);
        await Click("Other or unrecorded state: 2"); Assert.Equal(2, Links().Length); Assert.Contains("State not recorded", page.Markup); Assert.Contains("UNRECOGNIZED", page.Markup);
        await Click("Returned: 21"); await Click("Next incidents"); await Click("Next incidents"); Assert.Single(Links()); Assert.Contains("page 3 of 3", page.Markup);
        await Click("Previous incidents"); Assert.Equal(10, Links().Length); Assert.Contains("page 2 of 3", page.Markup);
        if (fieldChanged) fixture.Fields.Raise(x => x.CurrentFieldChanged += null!, "other"); else company.Clear();
        page.WaitForAssertion(() => Assert.Empty(Links())); Assert.DoesNotContain("Returned: 21", page.Markup); Assert.DoesNotContain("Checked ", page.Markup);
        if (!fieldChanged) Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.View"])));
        await Click("Refresh"); page.WaitForAssertion(() => Assert.Contains("All returned incidents", page.Markup)); Assert.Contains("page 1 of 3", page.Markup); Assert.Equal(10, Links().Length);
        fixture.Incidents.Verify(x => x.GetIncidentsAsync(null, null, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HseIncidentCountDrillDownDistinguishesEmptyGroupFromUnavailablePopulation(bool emptyPopulation)
    {
        var fixture = IncidentLanding(emptyPopulation ? [] : [new() { IncidentId = "open", FieldId = "field", CurrentState = IncidentState.Reported }]); using var ui = fixture.Ui; using var company = fixture.Company;
        var page = ui.RenderComponent<HSEDashboard>(); page.WaitForAssertion(() => Assert.Contains(emptyPopulation ? "Returned: 0" : "Returned: 1", page.Markup));
        await page.FindAll("button").Single(b => b.TextContent.Trim() == "Closed: 0").ClickAsync(new());
        Assert.Contains("Closed incidents", page.Markup); Assert.Contains("No returned incident records match this group", page.Markup);
        Assert.Empty(page.FindAll("a[href^='/ppdm39/hse/incident-detail?incidentId=']"));
        Assert.DoesNotContain("could not be confirmed", page.Markup); Assert.DoesNotContain("Next incidents", page.Markup);
        await page.FindAll("button").Single(b => b.TextContent.Trim() == "Refresh").ClickAsync(new());
        Assert.Contains("All returned incidents", page.Markup); Assert.Contains("page 1 of 1", page.Markup);
    }

    [Theory][InlineData("/hse/dashboard")][InlineData("/lifecycle/hse/dashboard")]
    public void HsePersonasSeeTheirAssignedWorkWithoutFabricatedSafetyFigures(string route)
    {
        var pageType=typeof(HSEDashboard);Assert.NotEmpty(pageType.GetCustomAttributes(typeof(AuthorizeAttribute),true));Assert.Contains(pageType.GetCustomAttributes(typeof(RouteAttribute),true).Cast<RouteAttribute>(),entry=>entry.Template==route);
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],["HSE.View"])));
        var personas=new Mock<IPersonaContextService>();personas.SetupGet(x=>x.CurrentPersona).Returns(new AppPersona{Code="HSE",Name="Safety",DefaultRoute=route});personas.SetupGet(x=>x.CurrentProfile).Returns(new AppUserPersona{UserId="actor",PersonaCode="HSE"});personas.Setup(x=>x.EnsureLoadedAsync()).Returns(Task.CompletedTask);
        var inbox=new Mock<IUnifiedTaskInboxService>();inbox.Setup(x=>x.GetInboxAsync("HSE")).ReturnsAsync(new UnifiedInbox{Company=new("tenant","company"),ActorUserId="actor",PersonaCode="HSE",CriticalTasks=[new(){TaskId="task",TaskType="REVIEW",Priority=1,WorkflowName="safety-process",StepName="review",ScopeKind="FIELD",AssignedRole="Safety",EntityId="incident",EntityType="HSE_INCIDENT",Route="/ppdm39/process/instances/safety-process"}],Counts=new(){TotalPending=1,Critical=1,Reviews=1}});
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{FieldId="field",FieldName="North field",SelectionAvailable=true,SelectionContext=new(new("tenant","company"),new("core","binding"),"actor","revision","field")});
        var incidents=new Mock<IHSEServiceClient>();incidents.Setup(x=>x.GetIncidentsAsync(null,null,It.IsAny<CancellationToken>())).ReturnsAsync([new HSEIncidentRecord{IncidentId="one",FieldId="field",CurrentState=IncidentState.Reported},new HSEIncidentRecord{IncidentId="two",FieldId="field",CurrentState=IncidentState.Cancelled}]);
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddLogging();ui.Services.AddSingleton<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>(new LandingAuth());ui.Services.AddSingleton(company);ui.Services.AddSingleton(personas.Object);ui.Services.AddSingleton(inbox.Object);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton(incidents.Object);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        var page=ui.RenderComponent<HSEDashboard>();page.WaitForAssertion(()=>Assert.Contains("safety-process",page.Markup));page.WaitForAssertion(()=>Assert.Contains("Returned: 2",page.Markup));Assert.Contains("completeness is not established",page.Markup);Assert.Contains("Open: 1",page.Markup);Assert.Contains("Cancelled: 1",page.Markup);Assert.Contains("totals are not available",page.Markup);Assert.Contains("/ppdm39/hse/incident-detail?incidentId=one",page.Markup);Assert.Contains("/ppdm39/hse/incident-detail?incidentId=two",page.Markup);Assert.Contains("10 per page, ordered by ID",page.Markup);Assert.DoesNotContain("3 HAZOP",page.Markup);Assert.DoesNotContain("5 permits",page.Markup);
        company.Clear();page.WaitForAssertion(()=>{Assert.DoesNotContain("safety-process",page.Markup);Assert.DoesNotContain("/ppdm39/hse/incident-detail?incidentId=one",page.Markup);});
    }
    [Fact]
    public async Task LateIncidentTotalsAreClearedAfterCompanyChange()
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],["HSE.View"])));
        var personas=new Mock<IPersonaContextService>();personas.SetupGet(x=>x.CurrentPersona).Returns(new AppPersona{Code="HSE",Name="Safety",DefaultRoute="/hse/dashboard"});personas.Setup(x=>x.EnsureLoadedAsync()).Returns(Task.CompletedTask);
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{FieldId="field",SelectionAvailable=true,SelectionContext=new(new("tenant","company"),new("core","binding"),"actor","revision","field")});
        var response=new TaskCompletionSource<List<HSEIncidentRecord>>(TaskCreationOptions.RunContinuationsAsynchronously);var incidents=new Mock<IHSEServiceClient>();incidents.Setup(x=>x.GetIncidentsAsync(null,null,It.IsAny<CancellationToken>())).Returns(response.Task);
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddLogging();ui.Services.AddSingleton<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>(new LandingAuth());ui.Services.AddSingleton(company);ui.Services.AddSingleton(personas.Object);ui.Services.AddSingleton(Mock.Of<IUnifiedTaskInboxService>());ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton(incidents.Object);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        var page=ui.RenderComponent<HSEDashboard>();company.Clear();response.SetResult([new HSEIncidentRecord{IncidentId="one",FieldId="field",CurrentState=IncidentState.Reported}]);await page.InvokeAsync(()=>Task.CompletedTask);page.WaitForAssertion(()=>Assert.Contains("Company or FIELD changed",page.Markup));Assert.DoesNotContain("Returned: 1",page.Markup);
    }
    [Theory][InlineData("foreign")][InlineData("duplicate")]
    public void InvalidIncidentPopulationCannotProduceDashboardTotals(string mode)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],["HSE.View"])));
        var personas=new Mock<IPersonaContextService>();personas.Setup(x=>x.EnsureLoadedAsync()).Returns(Task.CompletedTask);
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{FieldId="field",SelectionAvailable=true,SelectionContext=new(new("tenant","company"),new("core","binding"),"actor","revision","field")});
        var rows=mode=="foreign"?new List<HSEIncidentRecord>{new(){IncidentId="one",FieldId="other"}}:new List<HSEIncidentRecord>{new(){IncidentId="one",FieldId="field"},new(){IncidentId="one",FieldId="field"}};
        var incidents=new Mock<IHSEServiceClient>();incidents.Setup(x=>x.GetIncidentsAsync(null,null,It.IsAny<CancellationToken>())).ReturnsAsync(rows);
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddLogging();ui.Services.AddSingleton<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>(new LandingAuth());ui.Services.AddSingleton(company);ui.Services.AddSingleton(personas.Object);ui.Services.AddSingleton(Mock.Of<IUnifiedTaskInboxService>());ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton(incidents.Object);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        var page=ui.RenderComponent<HSEDashboard>();page.WaitForAssertion(()=>Assert.Contains("Incident totals could not be confirmed",page.Markup));Assert.DoesNotContain("Returned: 1",page.Markup);Assert.DoesNotContain("Returned: 2",page.Markup);
    }
    private sealed class LandingAuth : Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider
    {
        public override Task<Microsoft.AspNetCore.Components.Authorization.AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new Microsoft.AspNetCore.Components.Authorization.AuthenticationState(new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity([new("party_id", "actor"), new(System.Security.Claims.ClaimTypes.Role, "Safety")], "test"))));
    }

}
#endif

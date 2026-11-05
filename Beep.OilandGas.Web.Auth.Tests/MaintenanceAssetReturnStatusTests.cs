#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Components.BusinessProcess;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class MaintenanceAssetReturnStatusTests
{
    internal static CompanyMaintenanceAssetReturnStatus QualifiedStatus(string targets="E",bool recorded=true,bool ambiguous=false,bool unknownReference=false)
    {
        var original=Status();var context=original.Context;var order=CompanyInventorySourceReview.Copy(MaintenanceAssetReturnStateTests.Review().Assets.Order);
        order.InstanceId=original.Query.Task.WorkOrderId;order.FieldId=original.Query.Task.FieldId;
        order.Components=[order.Components[0] with{WorkOrderId=order.InstanceId,FieldId=order.FieldId,ComponentId="qualified-component",WellId=targets.Contains('W')?original.CurrentWells[0].WellId:"",EquipmentId=targets.Contains('E')?"pump":"",FacilityId=targets.Contains('F')?"plant":null,FacilityType=targets.Contains('F')?"gas":null,InformationItemId=null,InformationItemSubtype=null,NativeRevision=new string('A',64)}];
        var now=DateTime.UtcNow;var from=now.AddDays(-1);
        var identity=new CompanyMaintenanceQualifiedAssets(context,CompanyWorkOrderExecutionRequirements.OrderRevision(order),"UTC",
            [new("plant","gas",order.FieldId,context.Company.BusinessAssociateId,"READ",from,null,new string('B',64))],
            targets.Contains('E')?[new("pump",context.Company.BusinessAssociateId,"READ",from,null,null,null,new string('B',64),new("pump","plant","gas",7,from,from,null,null,"recorded-installation",new string('C',64),1,new string('D',64)))]:[],now);
        var statuses=new List<CompanyMaintenanceQualifiedStatus>{new("FACILITY","plant","gas","facility-after","NativeCondition","RECORDED_TARGET","facility-new-source","Y",from,from,null,null,"UTC",true,new string('E',64))};
        if(targets.Contains('E'))statuses.Add(new("EQUIPMENT","pump",null,"equipment-after","EquipmentFacet","RECORDED_TARGET","equipment-new-source","Y",from,from,null,null,"UTC",true,new string('F',64)));
        if(ambiguous)statuses.Add(statuses[^1] with{StatusId="second-current-status",Source="second-source"});
        var references=statuses.Select(row=>new CompanyMaintenanceQualifiedStatusReference(row.AssetType,row.StatusType,row.Value,"Y",null,null,true,true,new string('A',64),new string('B',64))).DistinctBy(row=>(row.AssetType,row.StatusType,row.Value)).Where(row=>!unknownReference || row.AssetType!="EQUIPMENT").ToArray();
        var compared=DateTime.UtcNow;var operating=new CompanyMaintenanceQualifiedOperating(context,identity.IdentityRevision(),statuses,references,new string('C',64),compared,CompanyMaintenanceQualifiedOperating.GetBlockers(identity,statuses,references,compared));
        var qualified=identity with{Operating=operating};qualified.Require(order);
        var facts=new List<CompanyMaintenanceQualifiedReturnFact>();
        if(targets.Contains('F'))facts.Add(new("FACILITY","plant","gas","NativeCondition","facility-before","INACTIVE","facility-old-source","facility-after","RECORDED_TARGET","facility-new-source",0m));
        if(targets.Contains('E'))facts.Add(new("EQUIPMENT","pump",null,"EquipmentFacet","equipment-before","IDLE","equipment-old-source","equipment-after","RECORDED_TARGET","equipment-new-source",72.5m));
        var saved=original.Recorded! with{Wells=targets.Contains('W')?original.Recorded.Wells:[],QualifiedAssets=facts};
        var result=original with{Recorded=recorded?saved:null,CurrentWells=targets.Contains('W')?original.CurrentWells:[],OriginalComponents=order.Components,CurrentQualifiedAssets=qualified};result.Require();return result;
    }
    [Fact]
    public void ExistingWellOnlyStatusKeepsItsOriginalSerializedBody()
    {
        var status=Status();var legacy=JsonSerializer.Serialize(new{status.Context,status.Query,status.Recorded,status.CurrentWells});
        Assert.Equal(legacy,JsonSerializer.Serialize(status));Assert.DoesNotContain("CurrentQualifiedAssets",legacy);Assert.DoesNotContain("OriginalComponents",legacy);
        JsonSerializer.Deserialize<CompanyMaintenanceAssetReturnStatus>(legacy)!.Require();
    }
    [Theory][InlineData("missing-components")][InlineData("missing-current")][InlineData("foreign-reader")][InlineData("wrong-facility-type")][InlineData("omitted-well")][InlineData("omitted-recorded-target")][InlineData("installation-parent-as-target")]
    public void QualifiedPublicStatusRefusesIncompleteForeignOrExpandedAffectedScope(string mode)
    {
        var status=QualifiedStatus(mode=="installation-parent-as-target"?"E":"WEF");
        if(mode=="missing-components")status=status with{OriginalComponents=null};
        if(mode=="missing-current")status=status with{CurrentQualifiedAssets=null};
        if(mode=="foreign-reader")status=status with{CurrentQualifiedAssets=status.CurrentQualifiedAssets! with{Context=status.Context with{ActorUserId="foreign-account"}}};
        if(mode=="wrong-facility-type")status=status with{CurrentQualifiedAssets=status.CurrentQualifiedAssets! with{Facilities=status.CurrentQualifiedAssets.Facilities.Select(row=>row with{FacilityType="oil"}).ToArray()}};
        if(mode=="omitted-well")status=status with{CurrentWells=[],Recorded=status.Recorded! with{Wells=[]}};
        if(mode=="omitted-recorded-target")status=status with{Recorded=status.Recorded! with{QualifiedAssets=[status.Recorded.QualifiedAssets![0]]}};
        if(mode=="installation-parent-as-target")status=status with{Recorded=status.Recorded! with{QualifiedAssets=[..status.Recorded.QualifiedAssets!,new("FACILITY","plant","gas","NativeCondition","before","INACTIVE","old-source","after","RECORDED_TARGET","new-source",0m)]}};
        Assert.ThrowsAny<Exception>(status.Require);
    }
    [Theory][InlineData("W")][InlineData("F")][InlineData("E")][InlineData("WEF")]
    public async Task SharedOutcomeShowsEveryRecordedTargetAndSourceWithoutInventingCurrentCondition(string targets)
    {
        var original=Status().Recorded!;
        var qualified=new List<CompanyMaintenanceQualifiedReturnFact>();
        if(targets.Contains('F'))qualified.Add(new("FACILITY","plant","gas","RecordedFacet","facility-before","INACTIVE","facility-old-source","facility-after","RECORDED_TARGET","facility-new-source",0m));
        if(targets.Contains('E'))qualified.Add(new("EQUIPMENT","pump",null,"EquipmentFacet","equipment-before","IDLE","equipment-old-source","equipment-after","RECORDED_TARGET","equipment-new-source",72.5m));
        var facts=original with{Wells=targets.Contains('W')?original.Wells:[],QualifiedAssets=qualified.Count>0?qualified:null};facts.Require();
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,NoBrowser>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<MaintenanceAssetReturnOutcome>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Facts",facts}}));
            var html=System.Net.WebUtility.HtmlDecode(root.ToHtmlString());
            Assert.Contains(facts.ReturnId,html);Assert.Contains(facts.ActorUserId,html);Assert.Contains(facts.Role,html);
            Assert.Contains("Read current native evidence separately",html);Assert.DoesNotContain("Current native well conditions",html);Assert.DoesNotContain("Retain reference",html);
            if(targets.Contains('W')){Assert.Contains("Recorded well return transitions",html);Assert.Contains("Shut In to Active",html);}else Assert.DoesNotContain("Recorded well return transitions",html);
            if(targets.Contains('F')){Assert.Contains("FACILITY plant gas",html);Assert.Contains("INACTIVE to RECORDED_TARGET",html);Assert.Contains("0%",html);Assert.Contains("facility-new-source / facility-after",html);}else Assert.DoesNotContain("FACILITY plant",html);
            if(targets.Contains('E')){Assert.Contains("EQUIPMENT pump",html);Assert.Contains("IDLE to RECORDED_TARGET",html);Assert.Contains("72.5%",html);Assert.Contains("equipment-old-source / equipment-before",html);}else Assert.DoesNotContain("EQUIPMENT pump",html);
        });
    }
    internal static CompanyMaintenanceAssetReturnStatus Status(bool recorded=true,bool ambiguous=false,bool unknownReference=false)
    {
        var review=MaintenanceAssetReturnStateTests.Review();var command=new CompanyMaintenanceAssetReturnCommand(review,Guid.NewGuid(),DateTime.UtcNow,MaintenanceAssetReturnStateTests.Observations(review),"Private retained original return rationale",true);var saved=MaintenanceAssetReturnStateTests.Saved(command);
        var context=review.Assets.Context with{ActorUserId="authorized-reader"};var now=DateTime.UtcNow;var prior=review.Assets.Wells.Single();var rows=prior.Facets.Select(row=>row.Facet=="Condition"?row with{ActiveIndicator="N",ExpiresAtUtc=command.ActivatedAtUtc,EndedAtUtc=command.ActivatedAtUtc}:row).ToList();var condition=prior.Facets.Single(row=>row.Facet=="Condition");rows.Add(condition with{StatusId=command.StatusId(prior.WellId),Value="Active",EffectiveAtUtc=command.ActivatedAtUtc,Revision=new string('F',64)});
        if(ambiguous)rows.Add(condition with{Source="second-source",StatusId="second-current-condition",Value="Active",EffectiveAtUtc=command.ActivatedAtUtc,Revision=new string('E',64)});
        var references=prior.References.Where(reference=>!unknownReference || reference.Facet!="Condition" || reference.Value!="Active").ToArray();var blockers=CompanyMaintenanceWellEvidence.GetBlockers(context,prior.WellId,prior.FieldId,prior.OperatorBaId,"READ",prior.WellRevision,prior.DateTimeZoneId,prior.TimeZoneReferencesRevision,rows,references,now);
        var well=prior with{Context=context,AccessLevel="READ",Facets=rows,References=references,ComparedAtUtc=now,Blockers=blockers};var result=new CompanyMaintenanceAssetReturnStatus(context,review.Assets.Report.Query,recorded?CompanyMaintenanceAssetReturnFacts.From(saved):null,[well]);result.Require();return result;
    }
    private static CompanyClientContext Company(CompanyMaintenanceAssetReturnStatus status,bool permitted=true)
    {var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),status.Context.ActorUserId,new(status.Context.Company,["ReaderRole"],permitted?["Maintenance.ReturnToService"]:[])));return company;}
    [Theory][InlineData("recorded")][InlineData("absent")][InlineData("permission")][InlineData("failed")][InlineData("foreign-account")][InlineData("late-company")][InlineData("late-source")]
    public async Task PublicStateNeedsOnlyReadCapabilityAndDoesNotTurnFailureOrForeignEvidenceIntoAbsence(string mode)
    {
        var status=Status(mode!="absent");using var company=Company(status,mode!="permission");using var state=new MaintenanceAssetEvidenceState(company);state.Attach(status.Query);var client=new Mock<ICompanyMaintenanceHandoffClient>();
        client.Setup(value=>value.ReadMaintenanceAssetReturnStatusAsync(status.Query,It.IsAny<CancellationToken>())).Returns(()=>{if(mode=="failed")throw new HttpRequestException("Read unavailable");if(mode=="late-company")company.Clear();if(mode=="late-source")state.Clear();return Task.FromResult(mode=="foreign-account"?status with{Context=status.Context with{ActorUserId="foreign"},CurrentWells=status.CurrentWells.Select(well=>well with{Context=well.Context with{ActorUserId="foreign"}}).ToArray()}:status);});
        var error=await Record.ExceptionAsync(()=>state.ReadReturnStatusAsync(client.Object));if(mode is "recorded" or "absent"){Assert.Null(error);Assert.NotNull(state.ReturnStatus);Assert.Equal(mode=="recorded",state.ReturnStatus.Recorded is not null);}else Assert.Null(state.ReturnStatus);
        client.Verify(value=>value.RecordMaintenanceAssetReturnAsync(It.IsAny<CompanyMaintenanceAssetReturnCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    private sealed class Reply(CompanyMaintenanceAssetReturnStatus status,Action changed):HttpMessageHandler
    {internal int Calls;protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token){Calls++;Assert.EndsWith("/asset-return/status",request.RequestUri!.AbsolutePath);changed();return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=JsonContent.Create(status)});}}
    [Theory][InlineData("current")][InlineData("permission")][InlineData("late-company")]
    public async Task ActualPublicClientAllowsAReadRoleWithoutExecutionOrWellWrite(string mode)
    {
        var status=Status();using var company=Company(status,mode!="permission");var reply=new Reply(status,()=>{if(mode=="late-company")company.Clear();});using var http=new HttpClient(reply){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Record.ExceptionAsync(()=>client.ReadMaintenanceAssetReturnStatusAsync(status.Query));if(mode=="current")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,reply.Calls);
    }
    private sealed class Auth(string actor):AuthenticationStateProvider
    {public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new(new ClaimsIdentity([new("party_id",actor),new(ClaimTypes.Role,"ReaderRole")],"fixture"))));}
    private sealed class Components:IComponentActivator
    {internal MaintenanceAssetEvidencePanel Panel=null!;public IComponent CreateInstance(Type type){var result=(IComponent)Activator.CreateInstance(type)!;if(result is MaintenanceAssetEvidencePanel panel)Panel=panel;return result;}}
    private sealed class NoBrowser:IJSRuntime
    {public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);}
    [Theory][InlineData("F","recorded")][InlineData("E","recorded")][InlineData("WEF","recorded")][InlineData("E","absent")][InlineData("E","ambiguous")][InlineData("E","unknown-reference")][InlineData("E","late-company")][InlineData("E","late-field")][InlineData("E","failed")][InlineData("E","field")]
    public async Task ActualReadOnlyScreenShowsCompleteQualifiedScopeAndClearsUnconfirmedEvidence(string targets,string mode)
    {
        var status=QualifiedStatus(targets,mode!="absent",mode=="ambiguous",mode=="unknown-reference");using var company=Company(status);
        Assert.DoesNotContain("Workflow.Execute",company.Selection!.Permissions);Assert.DoesNotContain("Facilities.Edit",company.Selection.Permissions);Assert.DoesNotContain("Facilities.ManageEquipment",company.Selection.Permissions);
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(mode=="field"?"foreign-field":status.Context.FieldId);
        var client=new Mock<ICompanyMaintenanceHandoffClient>();var reads=0;
        client.Setup(value=>value.ReadMaintenanceAssetReturnStatusAsync(status.Query,It.IsAny<CancellationToken>())).Returns(()=>{reads++;if(mode=="failed")throw new HttpRequestException("Source unavailable");if(mode=="late-company")company.Clear();if(mode=="late-field")fields.Raise(value=>value.CurrentFieldChanged+=null!,"foreign-field");return Task.FromResult(status);});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<MaintenanceAssetEvidenceState>();services.AddSingleton(client.Object);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth(status.Context.ActorUserId));services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<MaintenanceAssetEvidencePanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Query",status.Query}}));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;
            await (Task)typeof(MaintenanceAssetEvidencePanel).GetMethod("ReadStatusAsync",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null)!;await Task.Yield();var html=System.Net.WebUtility.HtmlDecode(root.ToHtmlString());
            if(mode is "field" or "failed" or "late-field" or "late-company"){Assert.DoesNotContain("Current native facility and equipment evidence",html);Assert.DoesNotContain("No recorded asset return was found",html);return;}
            Assert.Contains("Current native facility and equipment evidence",html);Assert.Contains("source date zone UTC",html);Assert.Contains("access READ",html);
            Assert.Contains(targets.Contains('F')?"Affected facility":"Installation parent (read only)",html);
            if(targets.Contains('E')){Assert.Contains("Equipment pump",html);Assert.Contains("observation 7",html);}
            if(!targets.Contains('W'))Assert.DoesNotContain("Current native well conditions",html);
            if(mode=="absent")Assert.Contains("No recorded asset return was found",html);else Assert.Contains("Recorded asset return",html);
            if(mode is "ambiguous" or "unknown-reference"){Assert.Contains("current assignment or native reference is unresolved",html);Assert.DoesNotContain("EquipmentFacet: RECORDED_TARGET",html);}
            else if(targets.Contains('E'))Assert.Contains("EquipmentFacet: RECORDED_TARGET",html);
            Assert.DoesNotContain("Private retained original return rationale",html);Assert.DoesNotContain("COMMAND_JSON",html);Assert.DoesNotContain("BEFORE_JSON",html);
        });
        if(mode=="field")Assert.Equal(0,reads);
        client.Verify(value=>value.RecordMaintenanceAssetReturnAsync(It.IsAny<CompanyMaintenanceAssetReturnCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Theory][InlineData("recorded")][InlineData("absent")][InlineData("ambiguous")][InlineData("unknown-reference")][InlineData("permission")][InlineData("field")][InlineData("failed")][InlineData("late-field")][InlineData("late-company")]
    public async Task ActualPublicScreenSeparatesRecordedTransitionFromCurrentFacetsAndCannotLeakPrivateBodies(string mode)
    {
        var status=Status(mode is not ("absent" or "unknown-reference"),mode=="ambiguous",mode=="unknown-reference");using var company=Company(status,mode!="permission");var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(mode=="field"?"other":status.Context.FieldId);var client=new Mock<ICompanyMaintenanceHandoffClient>();var reads=0;
        client.Setup(value=>value.ReadMaintenanceAssetReturnStatusAsync(status.Query,It.IsAny<CancellationToken>())).Returns(()=>{reads++;if(mode=="failed")throw new HttpRequestException("Unavailable source");if(mode=="late-field")fields.Raise(value=>value.CurrentFieldChanged+=null!,"other");if(mode=="late-company")company.Clear();return Task.FromResult(status);});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<MaintenanceAssetEvidenceState>();services.AddSingleton(client.Object);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth(status.Context.ActorUserId));services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<MaintenanceAssetEvidencePanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Query",status.Query}}));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;await (Task)typeof(MaintenanceAssetEvidencePanel).GetMethod("ReadStatusAsync",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null)!;await Task.Yield();var html=root.ToHtmlString();
            Assert.DoesNotContain("Private retained original return rationale",html);Assert.DoesNotContain("COMMAND_JSON",html);Assert.DoesNotContain("BEFORE_JSON",html);Assert.DoesNotContain("AFTER_JSON",html);
            if(mode is "recorded" or "ambiguous"){Assert.Contains("Recorded asset return",html);Assert.Contains("Current native well conditions",html);Assert.Contains("Shut In to Active",html);Assert.Contains("65.5",html);Assert.Contains(status.Recorded!.ActorUserId,html);Assert.DoesNotContain("No recorded asset return was found",html);}
            if(mode=="recorded")Assert.Contains("Condition: Active",html);if(mode=="ambiguous")Assert.Contains("Current Condition is unresolved",html);if(mode=="absent"){Assert.Contains("No recorded asset return was found",html);Assert.Contains("Condition: Active",html);}
            if(mode=="unknown-reference"){Assert.Contains("Current Condition reference is unresolved",html);Assert.DoesNotContain("Condition: Active",html);}
            if(mode is "permission" or "field")Assert.Equal(0,reads);if(mode is "permission" or "field" or "failed" or "late-field" or "late-company")Assert.DoesNotContain("Current native well conditions",html);
        });
        client.Verify(value=>value.RecordMaintenanceAssetReturnAsync(It.IsAny<CompanyMaintenanceAssetReturnCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
}
#endif

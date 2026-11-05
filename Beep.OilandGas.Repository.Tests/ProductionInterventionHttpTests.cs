using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.ApiService.Controllers.Field;
using Beep.OilandGas.LifeCycle.Services.Production;
using Beep.OilandGas.LifeCycle.Services.Decommissioning;
using Beep.OilandGas.LifeCycle.Services.Decommissioning.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Repository;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor.Services;
using System.Security.Claims;
using System.Reflection;
using Beep.OilandGas.Web.Pages.PPDM39.Production;
namespace Beep.OilandGas.Repository.Tests;
public sealed partial class CurrentFieldLifecycleGateTests
{
    [Theory][InlineData(false)][InlineData(true)]
    public async Task ActualNativeInterventionApiAndTypedClientRecordAndRecoverOpaqueOriginalIdentity(bool linkMaintenance)
    {
        using var data = await Ready(); var profile = Profile(data); await Save(profile);
        const string well = "well/الكويت%2F";
        await data.Grant("WELL", well, "WRITE"); CompanyProductionBoundaryTests.NativeDecisionSource(data); CompanyProductionBoundaryTests.NativeDecisionProvider(data);
        const string order="order/العالم%2F";
        if(linkMaintenance)
        {
            await data.Grant("WORK_ORDER",order);
            data.Rows.Add(new WORK_ORDER{WORK_ORDER_ID=order,WORK_ORDER_NUMBER="Recorded corrective order",WORK_ORDER_TYPE=Beep.OilandGas.Models.Data.WorkOrder.WorkOrderSubType.Corrective,INSTRUCTIONS="Recorded original scope",ACTIVE_IND="Y"});
            data.Rows.Add(new WORK_ORDER_COMPONENT{WORK_ORDER_ID=order,COMPONENT_ID="component",FIELD_ID="A",UWI=well,EQUIPMENT_ID="equipment",ACTIVE_IND="Y"});
        }
        using var boundary = new Boundary(data, profile);
        boundary.Access.Setup(value => value.CheckAssetAccessAsync("actor", well, "WELL", It.IsAny<string?>(), null)).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse { HasAccess = true });
        boundary.Access.Setup(value => value.CheckAssetAccessAsync("actor", order, "WORK_ORDER", It.IsAny<string?>(), null)).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse { HasAccess = true });
        var selectedCompany = data.Gate.Company;
        await using var host = await HttpHost.Start(data, profile, boundary, true, typeof(ProductionController), services =>
        {
            services.AddScoped(_ => { var tenant = new RepositoryTenantContext(); tenant.SelectCompany(selectedCompany); return tenant; });
            services.AddScoped(_ => new PPDMProductionService(data.Editor.Object, data.Columns.Object, data.Defaults.Object, data.Metadata.Object, new PPDMMappingService(), profile, data.Policy));
            services.AddSingleton(Mock.Of<IWorkOrderService>()); 
            services.AddSingleton(new DecommissioningProcessService(Mock.Of<IProcessService>(), new PPDMDecommissioningService(data.Editor.Object, data.Columns.Object, data.Defaults.Object, data.Metadata.Object, new PPDMMappingService(), profile, data.Policy)));
            services.AddSingleton(Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>());
        });
        data.Http.HttpContext = data.Request;
        var current = await profile.GetCurrentFieldSelectionAsync(); var catalog = await profile.GetAvailableFieldsAsync();
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var transport = new HttpClient { BaseAddress = new(host.Url), Timeout = TimeSpan.FromMinutes(5) };
        var client = new ProductionServiceClient(new ApiClient(transport, Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(), company), NullLogger<ProductionServiceClient>.Instance);
        var result = (await RenderNativeEngineer(company, client, current, catalog, well, recordNative:true,workOrder:linkMaintenance?order:null))!;
        Assert.NotEqual(Guid.Empty, result.IntentId); Assert.Equal(well, result.WellId); Assert.True(result.Success);
        var writes = data.Writes.Count;
        var recovered = await client.RecoverInterventionDecisionAsync(well, result.IntentId);
        Assert.NotNull(recovered.Outcome); Assert.Equal(result.NativeReceiptRevision, recovered.Outcome.Restore(recovered.Intent!).NativeReceiptRevision); Assert.Equal(writes, data.Writes.Count);
        Assert.Equal(3, writes); Assert.Single(data.Rows.OfType<WELL_INTERVENTION_INTENT>()); Assert.Single(data.Rows.OfType<WELL_INTERVENTION_RESULT>());
        if(linkMaintenance)
        {
            var source=await client.GetInterventionMaintenanceLinkAsync(well,result.IntentId,order);
            Assert.Equal(order,source.WorkOrderId);Assert.Equal(well,source.WellId);Assert.Equal("actor",source.Reviewer);Assert.Equal(writes,data.Writes.Count);
            await RenderNativeMaintenanceSource(company,client,well,order,result.IntentId);
            Assert.Equal(writes,data.Writes.Count);
            boundary.Access.Setup(value=>value.CheckAssetAccessAsync("actor",order,"WORK_ORDER",It.IsAny<string?>(),null)).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=false});
            using var denied=await host.Http.GetAsync(host.Url+$"/api/field/current/production/intervention-decisions/{result.IntentId:D}/maintenance-link?wellId={Uri.EscapeDataString(well)}&workOrderId={Uri.EscapeDataString(order)}");
            Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);Assert.Equal(writes,data.Writes.Count);
        }
        var privateReads = data.Source.Invocations.Count(call => call.Method.Name == "GetEntityAsync" && call.Arguments[0] as string == "WELL_INTERVENTION_INTENT");
        selectedCompany = (await Beep.OilandGas.ApiService.Tests.Infrastructure.TestCompanyModules.CreateAsync(data.Db, "actor", "other-company")).Company;
        using var foreign = await host.Http.GetAsync(host.Url + $"/api/field/current/production/intervention-decisions/{result.IntentId:D}?wellId={Uri.EscapeDataString(well)}");
        Assert.Equal(HttpStatusCode.Forbidden, foreign.StatusCode);
        Assert.Equal(privateReads, data.Source.Invocations.Count(call => call.Method.Name == "GetEntityAsync" && call.Arguments[0] as string == "WELL_INTERVENTION_INTENT")); Assert.Equal(writes, data.Writes.Count);
    }
    private sealed class EngineerAuthentication:AuthenticationStateProvider
    {public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,"actor"),new(ClaimTypes.Role,"Administrator")],"fixture"))));}
    private sealed class EngineerNoBrowser:IJSRuntime
    {public ValueTask<T> InvokeAsync<T>(string identifier,object?[]? args)=>throw new InvalidOperationException("The native API/page gate has no browser.");public ValueTask<T> InvokeAsync<T>(string identifier,CancellationToken token,object?[]? args)=>InvokeAsync<T>(identifier,args);}
    private sealed class EngineerNavigation:NavigationManager
    {public EngineerNavigation()=>Initialize("http://fixture/","http://fixture/production/intervention");protected override void NavigateToCore(string uri,bool forceLoad)=>Uri=ToAbsoluteUri(uri).ToString();protected override void NavigateToCore(string uri,NavigationOptions options)=>Uri=ToAbsoluteUri(uri).ToString();}
    private sealed class EngineerComponents:IComponentActivator
    {internal InterventionDecisions? Page;public IComponent CreateInstance(Type type){var component=(IComponent)Activator.CreateInstance(type)!;if(component is InterventionDecisions page)Page=page;return component;}}
    private static async Task<ProductionInterventionDecisionResult?> RenderNativeEngineer(CompanyClientContext company,ProductionServiceClient client,FieldResponse current,List<FieldListItem> catalog,string well,bool recordNative=false,string? workOrder=null)
    {
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(current.FieldId);fields.Setup(value=>value.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(current);fields.Setup(value=>value.GetAvailableFieldsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(catalog);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddSingleton<IProductionServiceClient>(client);services.AddSingleton<AuthenticationStateProvider,EngineerAuthentication>();services.AddSingleton<IJSRuntime,EngineerNoBrowser>();services.AddSingleton<NavigationManager,EngineerNavigation>();services.AddScoped<ProductionInterventionDecisionState>();services.AddScoped(_=>new FieldSelectionState(company,fields.Object,(_,_,message)=>message));services.AddSingleton(fields.Object);services.AddSingleton(new OilGasCallFailures(Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>()));services.AddSingleton<IComponentActivator,EngineerComponents>();await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>());
        ProductionInterventionDecisionResult? result=null;
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<InterventionDecisions>();var components=(EngineerComponents)provider.GetRequiredService<IComponentActivator>();var flags=BindingFlags.NonPublic|BindingFlags.Instance;var rows=(List<ProductionInterventionCandidateDto>)components.Page!.GetType().GetField("_candidates",flags)!.GetValue(components.Page)!;var source=Assert.Single(rows);Assert.Equal(well,source.WellId);await(Task)components.Page.GetType().GetMethod("SelectCandidateAsync",flags)!.Invoke(components.Page,[source])!;typeof(ComponentBase).GetMethod("StateHasChanged",flags)!.Invoke(components.Page,null);
            var html=root.ToHtmlString();Assert.Contains("Original recorded diagnosis",html);Assert.Contains("Recorded preparer: engineer",html);Assert.Contains($"Source observation: {source.SourceObservationNumber}",html);Assert.Contains("Estimated cost: Unavailable",html);Assert.NotNull(provider.GetRequiredService<FieldSelectionState>().SelectedId);
            if(recordNative)
            {
                components.Page.GetType().GetField("_decisionNote",flags)!.SetValue(components.Page,"Independent native screen approval");
                if(workOrder is not null)components.Page.GetType().GetField("_linkedWorkOrderId",flags)!.SetValue(components.Page,workOrder);
                await(Task)components.Page.GetType().GetMethod("ApproveIntervention",flags)!.Invoke(components.Page,null)!;
                Assert.Null(components.Page.GetType().GetField("_error",flags)!.GetValue(components.Page));Assert.NotNull(components.Page.DecisionId);
                Assert.False(provider.GetRequiredService<ProductionInterventionDecisionState>().IsBlocked(current.FieldId,well));
                var recovery=await client.RecoverInterventionDecisionAsync(well,components.Page.DecisionId!.Value);Assert.NotNull(recovery.Outcome);result=recovery.Outcome.Restore(recovery.Intent!);
                await(Task)components.Page.GetType().GetMethod("ReviewNativeDecisionAsync",flags)!.Invoke(components.Page,null)!;typeof(ComponentBase).GetMethod("StateHasChanged",flags)!.Invoke(components.Page,null);
                Assert.Contains("Original APPROVED decision confirmed",root.ToHtmlString());
            }
            else {company.Clear();Assert.Null(components.Page.GetType().GetField("_selectedIntervention",flags)!.GetValue(components.Page));}
        });
        return result;
    }
    private static async Task RenderNativeMaintenanceSource(CompanyClientContext company,ProductionServiceClient client,string well,string order,Guid decision)
    {
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddSingleton<IProductionServiceClient>(client);services.AddSingleton<AuthenticationStateProvider,EngineerAuthentication>();services.AddSingleton<IJSRuntime,EngineerNoBrowser>();services.AddSingleton(new OilGasCallFailures(Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>()));
        services.AddSingleton<NavigationManager,EngineerNavigation>();services.AddSingleton(Mock.Of<ICompanyMaintenanceHandoffClient>());services.AddScoped<CompanyMaintenanceHandoffState>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>());
        var summary=new Beep.OilandGas.Models.Data.WorkOrder.WorkOrderSummary{InstanceId=order,FieldId="A",Source="WORK_ORDER",WoSubType=Beep.OilandGas.Models.Data.WorkOrder.WorkOrderSubType.Corrective,Components=[new(order,"component","A",well,"equipment")]};
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.BusinessProcess.WorkOrderInterventionSource>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Order",summary},{"DecisionId",decision},{"WellId",well}}));
            var html=root.ToHtmlString();Assert.Contains("Independent native screen approval",html);Assert.Contains("Reviewer actor",html);Assert.Contains("does not establish task assignment",html);Assert.DoesNotContain("could not be confirmed",html);
        });
    }
    [Theory][InlineData("allowed",200)][InlineData("anonymous",401)][InlineData("missing-selection",400)][InlineData("denied-field",403)]
    [InlineData("ambiguous",409)][InlineData("changed-field",409)][InlineData("metadata",409)][InlineData("grant",403)]
    [InlineData("unowned-afe",409)]
    [InlineData("unowned-decision",403)]
    public async Task ActualProductionInterventionHttpRouteUsesNativeCompanySourceAndCurrentFieldGuard(string authority,int status)
    {
        using var data=await Ready();var profile=Profile(data);if(authority!="missing-selection")await Save(profile);var well=authority=="unowned-decision"?"well":"well/الكويت%2F";data.Rows.Add(new WELL{UWI=well,ASSIGNED_FIELD="A",OPERATOR="company",ACTIVE_IND="Y"});data.Rows.Add(new WELL_ACTIVITY{UWI=well,SOURCE="engineer-source",ACTIVITY_OBS_NO=1,ACTIVE_IND="Y",ACTIVITY_TYPE_ID="WORKOVER_CANDIDATE",ROW_CREATED_BY="engineer",REMARK="Original recorded diagnosis"});await data.Grant("WELL",well);using var boundary=new Boundary(data,profile,authority!="denied-field");
        if(authority=="unowned-decision")boundary.Access.Setup(value=>value.CheckAssetAccessAsync("actor",well,"WELL","WRITE",null)).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=true});
        if(authority=="ambiguous")data.Rows.Add(new WELL_ACTIVITY{UWI=well,SOURCE="another-source",ACTIVITY_OBS_NO=1,ACTIVE_IND="Y",ACTIVITY_TYPE_ID="WORKOVER_CANDIDATE",ROW_CREATED_BY="engineer",REMARK="Another native original"});
        if(authority=="metadata"){var definition=await data.Metadata.Object.GetTableMetadataAsync("WELL_ACTIVITY");var altered=System.Text.Json.JsonSerializer.Deserialize<Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata>(System.Text.Json.JsonSerializer.Serialize(definition))!;altered.PrimaryKeyColumn="UWI";data.Metadata.Setup(value=>value.GetTableMetadataAsync("WELL_ACTIVITY")).ReturnsAsync(altered);}
        var providerRan=false;
        if(authority is "changed-field" or "grant")data.Source.Setup(value=>value.GetEntityAsync("WELL_ACTIVITY",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).Returns(async()=>{
            providerRan=true;if(authority=="changed-field")boundary.Field="B";
            if(authority=="grant"){foreach(var grant in data.Db.Set<AppUserAssetAccess>().Where(value=>value.AssetType=="WELL"))grant.IsActive=false;await data.Db.SaveChangesAsync();}
            return(IEnumerable<object>)data.Rows.OfType<WELL_ACTIVITY>().ToArray();
        });
        await using var host=await HttpHost.Start(data,profile,boundary,authority!="anonymous",typeof(ProductionController),services=>{
            var tenant=new RepositoryTenantContext();tenant.SelectCompany(data.Gate.Company);services.AddSingleton(tenant);
            services.AddSingleton(new PPDMProductionService(data.Editor.Object,data.Columns.Object,data.Defaults.Object,data.Metadata.Object,new PPDMMappingService(),profile,data.Policy));
            services.AddSingleton(Mock.Of<IWorkOrderService>());
            services.AddSingleton(new DecommissioningProcessService(Mock.Of<IProcessService>(),new PPDMDecommissioningService(data.Editor.Object,data.Columns.Object,data.Defaults.Object,data.Metadata.Object,new PPDMMappingService(),profile,data.Policy)));
            services.AddSingleton(Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>());
        });
        ProductionInterventionDecisionRequest? originalDecision=null;
        if(authority=="unowned-decision")
        {
            using var originalResponse=await host.Http.GetAsync(host.Url+"/api/field/current/production/intervention-candidates");Assert.Equal(HttpStatusCode.OK,originalResponse.StatusCode);
            var original=Assert.Single((await originalResponse.Content.ReadFromJsonAsync<List<ProductionInterventionCandidateDto>>())!);
            originalDecision=new(){FieldId="A",ExpectedSourceRevision=original.SourceRevision,Decision="APPROVED",Note="Independently reviewed original source",InterventionType=original.InterventionType,Problem=original.Problem};
        }
        using var response=originalDecision is not null
            ? await host.Http.PostAsJsonAsync(host.Url+"/api/field/current/production/intervention-candidates/well/decision",originalDecision)
            : authority=="unowned-afe"
            ? await host.Http.PostAsJsonAsync(host.Url+"/api/field/current/production/intervention-candidates/well/decision",new ProductionInterventionDecisionRequest{FieldId="A",ExpectedSourceRevision=new string('A',64),Decision="APPROVED",Note="Original decision rationale",LinkAfe=true,WorkOrderId="recorded-order"})
            : await host.Http.GetAsync(host.Url+"/api/field/current/production/intervention-candidates");
        Assert.Equal(status,(int)response.StatusCode);Assert.Empty(data.Writes);
        if(authority=="unowned-afe"){Assert.Contains("company-scoped financial handoff",await response.Content.ReadAsStringAsync());Assert.DoesNotContain("PPDM39",data.Connections);}
        if(status==200){var row=Assert.Single((await response.Content.ReadFromJsonAsync<List<ProductionInterventionCandidateDto>>())!);Assert.Equal(well,row.WellId);Assert.Equal(new CompanyScope("tenant","company"),row.Company);Assert.Equal("actor",row.Actor);Assert.Equal("A",row.FieldId);Assert.Equal("engineer",row.SourcePreparer);Assert.Equal("Original recorded diagnosis",row.Problem);Assert.Equal(64,row.SourceRevision.Length);Assert.Null(row.EstCostUsd);Assert.Null(row.DeferredBopd);Assert.Null(row.EstDaysSinceOnset);
            using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Administrator"],[])));using var transport=new HttpClient{BaseAddress=new(host.Url)};var client=new ProductionServiceClient(new ApiClient(transport,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),company),NullLogger<ProductionServiceClient>.Instance);var observed=Assert.Single(await client.GetInterventionCandidatesAsync());Assert.Equal(row.SourceRevision,observed.SourceRevision);Assert.Equal(well,observed.WellId);
            Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","other-company"),["Administrator"],[])));await Assert.ThrowsAsync<InvalidOperationException>(()=>client.GetInterventionCandidatesAsync());
            Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Administrator"],[])));data.Http.HttpContext=data.Request;var current=await profile.GetCurrentFieldSelectionAsync();var catalog=await profile.GetAvailableFieldsAsync();await RenderNativeEngineer(company,client,current,catalog,well);Assert.Empty(data.Writes);
        }
        else if(authority is "anonymous" or "missing-selection" or "denied-field" or "metadata" or "unowned-afe")data.Source.Verify(value=>value.GetEntityAsync("WELL_ACTIVITY",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>()),Times.Never);
        if(authority is "changed-field" or "grant")Assert.True(providerRan);
    }
}

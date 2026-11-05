param([ValidateSet('Debug', 'Release')][string]$Configuration = 'Debug', [switch]$RenderFieldInterests, [switch]$RenderWorkOrders, [switch]$RenderEquipment)
. (Join-Path $PSScriptRoot 'RestoredOwnerFixtureReferences.ps1')
$ErrorActionPreference = 'Stop'

# Compile the exact edited components against real focused production-state binaries and the app's MudBlazor version.
# This does not start the Web host, replace missing shared projects or claim browser/DI validation.
$repositoryRoot = Split-Path $PSScriptRoot -Parent
$focusedOutput = Join-Path $repositoryRoot "Beep.OilandGas.Repository.Tests/bin/company-focused/$Configuration/net10.0"
$assemblies = @('Beep.OilandGas.Repository.Tests', 'Beep.OilandGas.Models', 'DataManagementModels', 'Beep.OilandGas.PPDM.Models')
foreach ($name in $assemblies) {
    if (-not (Test-Path -LiteralPath (Join-Path $focusedOutput "$name.dll"))) {
        throw "Run the focused company tests in $Configuration first; the component check requires their actual compiled production-state assemblies."
    }
}
$checkDirectory = Join-Path ([IO.Path]::GetTempPath()) "oilgas-hierarchy-razor-$([Guid]::NewGuid().ToString('N'))"
[void](New-Item -ItemType Directory -Path $checkDirectory)
$sources = @{
    'Components/AccessControl/AssetAccessTree.razor' = 'Beep.OilandGas.Web/Components/AccessControl/AssetAccessTree.razor'
    'Pages/Admin/AccessControl/HierarchyConfig.razor' = 'Beep.OilandGas.Web/Pages/Admin/AccessControl/HierarchyConfig.razor'
    'Pages/Admin/AccessControl/AssetAccess.razor' = 'Beep.OilandGas.Web/Pages/Admin/AccessControl/AssetAccess.razor'
    'Fixtures/HierarchyConfigurationRenderHost.razor' = 'Beep.OilandGas.Web.Auth.Tests/Fixtures/HierarchyConfigurationRenderHost.razor'
}
foreach ($name in $sources.Keys) {
    $destination = Join-Path $checkDirectory $name
    [void](New-Item -ItemType Directory -Path (Split-Path $destination -Parent) -Force)
    Copy-Item -LiteralPath (Join-Path $repositoryRoot $sources[$name]) -Destination $destination
}
@'
@using System
@using System.Linq
@using System.Threading.Tasks
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Authorization
@using MudBlazor
'@ | Set-Content -LiteralPath (Join-Path $checkDirectory '_Imports.razor') -Encoding utf8

[xml]$webProject = Get-Content -LiteralPath (Join-Path $repositoryRoot 'Beep.OilandGas.Web/Beep.OilandGas.Web.csproj') -Raw
$mudVersion = @($webProject.Project.ItemGroup.PackageReference | Where-Object Include -eq 'MudBlazor')[0].Version
[xml]$checkProject = '<Project Sdk="Microsoft.NET.Sdk.Razor"><PropertyGroup/><ItemGroup/></Project>'
$propertyGroup = $checkProject.SelectSingleNode('/Project/PropertyGroup')
$itemGroup = $checkProject.SelectSingleNode('/Project/ItemGroup')
$properties = @{ TargetFramework = 'net10.0'; ImplicitUsings = 'enable'; Nullable = 'enable'; RootNamespace = 'Beep.OilandGas.Web' }
foreach ($name in $properties.Keys) {
    $element = $checkProject.CreateElement($name); $element.InnerText = $properties[$name]
    [void]$propertyGroup.AppendChild($element)
}
$framework = $checkProject.CreateElement('FrameworkReference'); $framework.SetAttribute('Include', 'Microsoft.AspNetCore.App')
[void]$itemGroup.AppendChild($framework)
$package = $checkProject.CreateElement('PackageReference'); $package.SetAttribute('Include', 'MudBlazor'); $package.SetAttribute('Version', $mudVersion)
[void]$itemGroup.AppendChild($package)
foreach ($name in $assemblies) {
    $reference = $checkProject.CreateElement('Reference'); $reference.SetAttribute('Include', $name)
    $hint = $checkProject.CreateElement('HintPath'); $hint.InnerText = Join-Path $focusedOutput "$name.dll"
    [void]$reference.AppendChild($hint); [void]$itemGroup.AppendChild($reference)
}
$projectPath = Join-Path $checkDirectory 'HierarchyRazorCheck.csproj'; Add-OilGasRestoredOwnerReferences -Project $checkProject -RepositoryRoot (Split-Path $PSScriptRoot -Parent)
$checkProject.Save($projectPath)
& dotnet build $projectPath --configuration $Configuration --verbosity quiet --nologo
if ($LASTEXITCODE -ne 0) { throw "The focused Razor compilation failed; inspect $checkDirectory." }
foreach ($name in $sources.Keys) {
    $original = (Get-FileHash -LiteralPath (Join-Path $repositoryRoot $sources[$name])).Hash
    $compiled = (Get-FileHash -LiteralPath (Join-Path $checkDirectory $name)).Hash
    if ($original -ne $compiled) { throw 'The source changed during compilation. Repeat the component check.' }
}
Write-Output "$($sources.Count) access-control components compiled from exact source copies against MudBlazor $mudVersion. Evidence: $checkDirectory"

if ($RenderFieldInterests -or $RenderWorkOrders -or $RenderEquipment) {
    # Static HtmlRenderer output verifies this exact page's markup and numeric/date presentation.
    # It does not execute a browser, live HTTP/auth pipeline or JS interactivity.
    $renderDirectory = "$checkDirectory-render"
    [void](New-Item -ItemType Directory -Path $renderDirectory)
    [xml]$renderProject = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup><ItemGroup/></Project>'
    $renderItems = $renderProject.SelectSingleNode('/Project/ItemGroup')
    $componentReference = $renderProject.CreateElement('ProjectReference'); $componentReference.SetAttribute('Include', $projectPath)
    [void]$renderItems.AppendChild($componentReference)
    foreach ($name in @($assemblies) + @('Moq')) {
        $reference = $renderProject.CreateElement('Reference'); $reference.SetAttribute('Include', $name)
        $hint = $renderProject.CreateElement('HintPath'); $hint.InnerText = Join-Path $focusedOutput "$name.dll"
        [void]$reference.AppendChild($hint); [void]$renderItems.AppendChild($reference)
    }
    $framework = $renderProject.CreateElement('FrameworkReference'); $framework.SetAttribute('Include', 'Microsoft.AspNetCore.App')
    [void]$renderItems.AppendChild($framework)
    $renderProjectPath = Join-Path $renderDirectory 'InterestRenderCheck.csproj'; Add-OilGasRestoredOwnerReferences -Project $renderProject -RepositoryRoot (Split-Path $PSScriptRoot -Parent)
$renderProject.Save($renderProjectPath)
    @'
using System.Globalization;
using System.Net;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.Models;
using ReviewClient = Beep.OilandGas.Client.App.Services.AccessControl.IAccessControlService;

var now = new DateTime(2026, 10, 5, 12, 0, 0, DateTimeKind.Utc);
foreach (var mode in new[] { "full-en", "full-fr", "empty", "error", "loading" })
{
    CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(mode == "full-fr" ? "fr-FR" : "en-US");
    using var company = new CompanyClientContext(); var scope = new CompanyScope("tenant", "company");
    if (!company.TrySelect(company.Clear(), "actor", new(scope, ["Administrator"], []))) throw new Exception("Selection failed.");
    var result = new FieldInterestReview(scope, new string('A', 64), "FIELD-A", now, "UTC", [new(
        new INTEREST_SET { INTEREST_SET_ID = "interest-set", INTEREST_SET_SEQ_NO = 3, INTEREST_SET_TYPE = "test-type", ACTIVE_IND = "Y", EFFECTIVE_DATE = now },
        [new INT_SET_COMPONENT { INTEREST_SET_ID = "interest-set", INTEREST_SET_SEQ_NO = 3, COMPONENT_OBS_NO = 7, FIELD_ID = "FIELD-A", COMPONENT_TYPE = "test-component", ACTIVE_IND = "Y", EFFECTIVE_DATE = now }],
        [new INT_SET_PARTNER { INTEREST_SET_ID = "interest-set", INTEREST_SET_SEQ_NO = 3, PARTNER_BA_ID = "company", PARTNER_OBS_NO = 2, INTEREST_SET_ROLE = "test<recorded-role>", ACTIVE_IND = "Y", GROSS_PERCENT_INTEREST = 37.5m, NET_PERCENT_INTEREST = 34.125m, BREACH_IND = "Y", ESCROW_IND = "N", EFFECTIVE_DATE = now }])]);
    var pending = new TaskCompletionSource<FieldInterestReview>(TaskCreationOptions.RunContinuationsAsynchronously);
    var client = new Mock<ReviewClient>(MockBehavior.Strict);
    var setup = client.Setup(value => value.GetFieldInterestReviewAsync("FIELD-A", It.IsAny<CancellationToken>()));
    if (mode == "error") setup.ThrowsAsync(new InvalidOperationException("Unavailable."));
    else if (mode == "loading") setup.Returns(pending.Task);
    else { if (mode == "empty") result.Sets.Clear(); setup.ReturnsAsync(result); }
    using var state = new AssetGrantAdministrationState(company, (_, _) => throw new Exception("Unexpected grant read."),
        (_, _, _) => throw new Exception("Unexpected grant."), (_, _, _) => throw new Exception("Unexpected revoke."),
        (_, _, sentence) => sentence + ". Verify your FIELD access.", client.Object);
    state.SelectInterestField("FIELD-A"); var read = state.LoadInterestReviewAsync(); if (mode != "loading") await read;
    var services = new ServiceCollection(); services.AddLogging(); services.AddMudServices();
    services.AddSingleton(company); services.AddSingleton(state); services.AddSingleton<IJSRuntime, StaticJs>();
    services.AddSingleton<NavigationManager, StaticNavigation>();
    await using var provider = services.BuildServiceProvider();
    await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
    var html = await renderer.Dispatcher.InvokeAsync(async () =>
        (await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.Admin.AccessControl.AssetAccess>()).ToHtmlString());
    await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory, mode + ".html"), html);
    void Require(bool condition, string message) { if (!condition) throw new Exception(mode + ": " + message); }
    Require(html.Contains("Exact FIELD ID") && html.Contains("Review FIELD Interests"), "Missing labeled review controls.");
    if (mode.StartsWith("full"))
    {
        Require(html.Contains("interest-set") && html.Contains("sequence 3") && html.Contains("<caption>Recorded FIELD components</caption>"), "Missing complete set identity or caption.");
        Require(html.Contains("scope=\"col\"") && html.Contains("tabindex=\"0\"") && html.Contains("role=\"region\""), "Missing accessible table navigation.");
        Require(html.Contains("test&lt;recorded-role&gt;") && !html.Contains("test<recorded-role>"), "Recorded text was not encoded.");
        var gross = mode == "full-fr" ? "37,5" : "37.5"; var net = mode == "full-fr" ? "34,125" : "34.125";
        Require(WebUtility.HtmlDecode(html).Contains(gross) && WebUtility.HtmlDecode(html).Contains(net) && !html.Contains("3750"), "Recorded percentage was changed or formatted incorrectly.");
        Require(html.Contains("Date zone: UTC") && html.Contains("2026-10-05T12:00:00.0000000Z") && html.Contains("Not recorded"), "Date provenance/default absence is missing.");
        Require(html.Contains("Recorded Breach Indicator") && html.Contains("Recorded Escrow Indicator") && html.Contains("Recorded Right-to-Earn Indicator") && html.Contains("<td>Y</td>") && html.Contains("<td>N</td>"), "Recorded conditional indicators were hidden or interpreted.");
    }
    else if (mode == "empty") Require(html.Contains("No current company interest records were returned") && !html.Contains("<caption>"), "Empty evidence was replaced by rows.");
    else if (mode == "error") Require(html.Contains("FIELD interests could not be reviewed") && html.Contains("role=\"alert\"") && !html.Contains("<caption>"), "Error does not clear evidence/report failure.");
    else
    {
        Require(html.Contains("aria-busy=\"true\"") && html.Contains("Updating asset administration") && !html.Contains("<caption>"), "Loading is not explicit.");
        state.Dispose(); pending.SetCanceled(); await read;
    }
    Console.WriteLine(mode + " exact-page static rendering passed.");
}
public sealed class StaticJs : IJSRuntime
{
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => throw new NotSupportedException("Static rendering cannot execute JS.");
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => throw new NotSupportedException("Static rendering cannot execute JS.");
}
public sealed class StaticNavigation : NavigationManager
{
    public StaticNavigation() => Initialize("https://render.test/", "https://render.test/admin/access-control/asset-access");
    protected override void NavigateToCore(string uri, bool forceLoad) => throw new NotSupportedException("Static rendering cannot navigate.");
}
'@ | Set-Content -LiteralPath (Join-Path $renderDirectory 'Program.cs') -Encoding utf8
    if ($RenderWorkOrders) {
        $programPath = Join-Path $renderDirectory 'Program.cs'
        $orderChecks = @'
foreach(var valid in new[]{true,false})
{
    using var company=new CompanyClientContext();
    if(!company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),[],[])))throw new Exception("Selection failed.");
    var order=new Beep.OilandGas.Models.Data.AssetHierarchyNode{AssetId="order",AssetType="WORK_ORDER",AssetName="Recorded maintenance",UserHasAccess=true};
    if(valid)order.WorkOrderComponents=[new("order","a","<east>","",""),new("order","b","west","","")];
    using var state=new AssetHierarchyState(company,(_,_)=>Task.FromResult<Beep.OilandGas.Models.Data.AssetHierarchyNode?>(new(){AssetId="company",AssetType="ORGANIZATION",Children=[order]}),(_,_,sentence)=>sentence);
    var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddSingleton(state);services.AddSingleton<IJSRuntime,StaticJs>();
    await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
    var html=await renderer.Dispatcher.InvokeAsync(async()=>(await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.AccessControl.AssetAccessTree>()).ToHtmlString());
    await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory,valid?"recorded-order.html":"invalid-order.html"),html);
    if(valid)
    {
        if(!html.Contains("Recorded maintenance") || !html.Contains("Component a") || !html.Contains("Component b") || !html.Contains("&lt;east&gt;") || !html.Contains("FIELD west") || html.Contains("<east>") || !html.Contains("do not grant access"))throw new Exception("Recorded order relationships were missing or not encoded.");
        if(html.Split("Recorded maintenance").Length!=2)throw new Exception("The order was duplicated by FIELD parent.");
    }
    else if(!html.Contains("could not be confirmed") || html.Contains("Recorded maintenance") || html.Contains("Component a"))throw new Exception("An unconfirmed order was rendered as confirmed hierarchy evidence.");
    Console.WriteLine(valid?"Recorded multi-FIELD order static rendering passed.":"Unconfirmed order static rendering passed.");
}
foreach(var configured in new[]{false,true})
{
    using var company=new CompanyClientContext();
    if(!company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Administrator"],[])))throw new Exception("Selection failed.");
    var revision=Guid.NewGuid().ToString();
    var rows=new[]{"FIELD","WELL","POOL","FACILITY"}.Select(type=>new Beep.OilandGas.Models.Data.HierarchyConfig{OrganizationId="company",AssetType=type,HierarchyLevel=type=="FIELD"?0:1,
        ParentLevel=type=="FIELD"?null:0,LevelName=type,Active=false,Revision=revision}).ToList();
    if(configured)rows.Add(new(){OrganizationId="company",AssetType="WORK_ORDER",HierarchyLevel=0,ParentLevel=null,LevelName="أوامر العمل",Active=true,Revision=revision});
    using var state=new HierarchyConfigurationState(company,(_,_)=>Task.FromResult<List<Beep.OilandGas.Models.Data.HierarchyConfig>?>(rows),(_,_,sentence)=>sentence,(_,_,_)=>throw new Exception("No command during static rendering."));
    using var tree=new AssetHierarchyState(company,(_,_)=>Task.FromResult<Beep.OilandGas.Models.Data.AssetHierarchyNode?>(new(){AssetId="company",AssetType="ORGANIZATION"}),(_,_,sentence)=>sentence);
    var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddSingleton(state);services.AddSingleton(tree);services.AddSingleton<IJSRuntime,StaticJs>();services.AddSingleton<NavigationManager,StaticNavigation>();
    await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
    var html=await renderer.Dispatcher.InvokeAsync(async()=>(await renderer.RenderComponentAsync<Beep.OilandGas.Web.Auth.Tests.HierarchyConfigurationRenderHost>()).ToHtmlString());
    await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory,configured?"order-category.html":"add-order-category.html"),html);
    if(!state.CanSave || !html.Contains("independent category"))throw new Exception("Configuration scope or independent category explanation was missing.");
    if(configured){if(!WebUtility.HtmlDecode(html).Contains("أوامر العمل") || !html.Contains("Show WORK_ORDER") || html.Contains("Add work-order category"))throw new Exception("Recorded order configuration was not rendered.");}
    else if(!html.Contains("Add work-order category"))throw new Exception("Explicit order configuration action was missing.");
    Console.WriteLine(configured?"Recorded order category page rendering passed.":"Explicit add-order category page rendering passed.");
}

'@
        $program = Get-Content -LiteralPath $programPath -Raw
        $program.Replace('public sealed class StaticJs', $orderChecks + 'public sealed class StaticJs') | Set-Content -LiteralPath $programPath -Encoding utf8
    }
    if ($RenderEquipment) {
        $programPath = Join-Path $renderDirectory 'Program.cs'
        $equipmentChecks = @'
foreach(var fakeField in new[]{false,true})
{
    using var company=new CompanyClientContext();
    if(!company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),[],[])))throw new Exception("Selection failed.");
    var equipment=new Beep.OilandGas.Models.Data.AssetHierarchyNode{AssetId="pump",AssetType="EQUIPMENT",AssetName="Recorded <pump>",UserHasAccess=true,ParentAssetId=fakeField?"invented-field":null,ParentAssetType=fakeField?"FIELD":null};
    using var state=new AssetHierarchyState(company,(_,_)=>Task.FromResult<Beep.OilandGas.Models.Data.AssetHierarchyNode?>(new(){AssetId="company",AssetType="ORGANIZATION",Children=[equipment]}),(_,_,sentence)=>sentence);
    var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddSingleton(state);services.AddSingleton<IJSRuntime,StaticJs>();
    await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
    var html=await renderer.Dispatcher.InvokeAsync(async()=>(await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.AccessControl.AssetAccessTree>()).ToHtmlString());
    await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory,fakeField?"invalid-equipment.html":"direct-equipment.html"),html);
    if(fakeField){if(!html.Contains("could not be confirmed") || html.Contains("Recorded &lt;pump&gt;"))throw new Exception("Invented equipment FIELD was accepted.");}
    else if(!html.Contains("Recorded &lt;pump&gt;") || html.Contains("Recorded <pump>") || !html.Contains("Equipment location requires separate installation evidence.") || !state.CanSelect(equipment))throw new Exception("Direct equipment identity, encoding or installation boundary is missing.");
    Console.WriteLine(fakeField?"Invented equipment FIELD rendering refused.":"Direct equipment static rendering passed.");
}
foreach(var configured in new[]{false,true})
{
    using var company=new CompanyClientContext();
    if(!company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Administrator"],[])))throw new Exception("Selection failed.");
    var revision=Guid.NewGuid().ToString();
    var rows=new[]{"FIELD","WELL","POOL","FACILITY"}.Select(type=>new Beep.OilandGas.Models.Data.HierarchyConfig{OrganizationId="company",AssetType=type,HierarchyLevel=type=="FIELD"?0:1,ParentLevel=type=="FIELD"?null:0,LevelName=type,Active=false,Revision=revision}).ToList();
    if(configured)rows.Add(new(){OrganizationId="company",AssetType="EQUIPMENT",HierarchyLevel=0,ParentLevel=null,LevelName="معدات الشركة",Active=true,Revision=revision});
    using var state=new HierarchyConfigurationState(company,(_,_)=>Task.FromResult<List<Beep.OilandGas.Models.Data.HierarchyConfig>?>(rows),(_,_,sentence)=>sentence,(_,_,_)=>throw new Exception("No command during static rendering."));
    using var tree=new AssetHierarchyState(company,(_,_)=>Task.FromResult<Beep.OilandGas.Models.Data.AssetHierarchyNode?>(new(){AssetId="company",AssetType="ORGANIZATION"}),(_,_,sentence)=>sentence);
    var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddSingleton(state);services.AddSingleton(tree);services.AddSingleton<IJSRuntime,StaticJs>();services.AddSingleton<NavigationManager,StaticNavigation>();
    await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
    var html=await renderer.Dispatcher.InvokeAsync(async()=>(await renderer.RenderComponentAsync<Beep.OilandGas.Web.Auth.Tests.HierarchyConfigurationRenderHost>()).ToHtmlString());
    await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory,configured?"equipment-category.html":"add-equipment-category.html"),html);
    if(!state.CanSave || !html.Contains("independent category") || !html.Contains("Equipment location requires separate installation evidence."))throw new Exception("Equipment category explanation or company scope is missing.");
    if(configured){if(!WebUtility.HtmlDecode(html).Contains("معدات الشركة") || !html.Contains("Show EQUIPMENT") || html.Contains("Add equipment category"))throw new Exception("Recorded equipment category was not rendered.");}
    else if(!html.Contains("Add equipment category"))throw new Exception("Explicit equipment configuration action was missing.");
    Console.WriteLine(configured?"Recorded equipment category rendering passed.":"Explicit add-equipment category rendering passed.");
}

'@
        $program = Get-Content -LiteralPath $programPath -Raw
        $program.Replace('public sealed class StaticJs', $equipmentChecks + 'public sealed class StaticJs') | Set-Content -LiteralPath $programPath -Encoding utf8
    }
    & dotnet run --project $renderProjectPath --configuration $Configuration --verbosity quiet --nologo
    if ($LASTEXITCODE -ne 0) { throw "The exact-page static rendering failed; inspect $renderDirectory." }
    Write-Output "5 FIELD interest static page render checks passed. Evidence: $renderDirectory"
}

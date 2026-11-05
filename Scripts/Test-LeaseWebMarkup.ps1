param([ValidateSet('Debug', 'Release')][string]$Configuration = 'Debug', [switch]$RenderPortfolio, [string]$FocusedOutputDirectory)
. (Join-Path $PSScriptRoot 'RestoredOwnerFixtureReferences.ps1')
$ErrorActionPreference = 'Stop'

# Compile exact page/component sources and their actual focused state assembly.
# This check does not replace missing host projects or exercise live HTTP/auth/browser/provider behavior.
$repositoryRoot = Split-Path $PSScriptRoot -Parent
$focusedOutput = if ([string]::IsNullOrEmpty($FocusedOutputDirectory)) { Join-Path $repositoryRoot "Beep.OilandGas.Repository.Tests/bin/company-focused/$Configuration/net10.0" } else { [IO.Path]::GetFullPath($FocusedOutputDirectory) }
$assemblies = @('Beep.OilandGas.Models', 'DataManagementModels', 'Beep.OilandGas.PPDM.Models', 'Beep.OilandGas.Repository.Tests')
foreach ($name in $assemblies) {
    if (-not (Test-Path -LiteralPath (Join-Path $focusedOutput "$name.dll"))) { throw 'Run focused company tests before this page check.' }
}
$checkDirectory = Join-Path ([IO.Path]::GetTempPath()) "oilgas-lease-page-$([Guid]::NewGuid().ToString('N'))"
[void](New-Item -ItemType Directory -Path $checkDirectory)
$sources = @{
    'Pages/PPDM39/Operations/LeaseAcquisition.razor' = 'Beep.OilandGas.Web/Pages/PPDM39/Operations/LeaseAcquisition.razor'
    'Components/Shared/KpiCard.razor' = 'Beep.OilandGas.Web/Components/Shared/KpiCard.razor'
    'Components/Shared/StatusBadge.razor' = 'Beep.OilandGas.Web/Components/Shared/StatusBadge.razor'
    'Components/FieldSelector.razor' = 'Beep.OilandGas.Web/Components/FieldSelector.razor'
}
foreach ($name in $sources.Keys) {
    $destination = Join-Path $checkDirectory $name
    [void](New-Item -ItemType Directory -Path (Split-Path $destination -Parent) -Force)
    Copy-Item -LiteralPath (Join-Path $repositoryRoot $sources[$name]) -Destination $destination
}
@'
@using System.Globalization
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Authorization
@using MudBlazor
@using Beep.OilandGas.Models.Data.Lease
@using Beep.OilandGas.Web.Services
@using Beep.OilandGas.Web.Components.Shared
'@ | Set-Content -LiteralPath (Join-Path $checkDirectory '_Imports.razor') -Encoding utf8
'<CascadingAuthenticationState><Beep.OilandGas.Web.Pages.PPDM39.Operations.LeaseAcquisition /></CascadingAuthenticationState>' | Set-Content -LiteralPath (Join-Path $checkDirectory 'LeaseRenderHarness.razor') -Encoding utf8
[xml]$webProject = Get-Content -LiteralPath (Join-Path $repositoryRoot 'Beep.OilandGas.Web/Beep.OilandGas.Web.csproj') -Raw
$mudVersion = @($webProject.Project.ItemGroup.PackageReference | Where-Object Include -eq 'MudBlazor')[0].Version
[xml]$project = '<Project Sdk="Microsoft.NET.Sdk.Razor"><PropertyGroup><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><RootNamespace>Beep.OilandGas.Web</RootNamespace></PropertyGroup><ItemGroup><FrameworkReference Include="Microsoft.AspNetCore.App"/></ItemGroup></Project>'
$items = $project.SelectSingleNode('/Project/ItemGroup')
$package = $project.CreateElement('PackageReference'); $package.SetAttribute('Include', 'MudBlazor'); $package.SetAttribute('Version', $mudVersion)
[void]$items.AppendChild($package)
foreach ($name in $assemblies) {
    $reference = $project.CreateElement('Reference'); $reference.SetAttribute('Include', $name)
    $hint = $project.CreateElement('HintPath'); $hint.InnerText = Join-Path $focusedOutput "$name.dll"
    [void]$reference.AppendChild($hint); [void]$items.AppendChild($reference)
}
$projectPath = Join-Path $checkDirectory 'LeasePageCheck.csproj'; Add-OilGasRestoredOwnerReferences -Project $project -RepositoryRoot (Split-Path $PSScriptRoot -Parent)
$project.Save($projectPath)
& dotnet build $projectPath --configuration $Configuration --verbosity quiet --nologo
if ($LASTEXITCODE -ne 0) { throw "Lease page compilation failed; inspect $checkDirectory." }
foreach ($name in $sources.Keys) {
    if ((Get-FileHash -LiteralPath (Join-Path $repositoryRoot $sources[$name])).Hash -ne (Get-FileHash -LiteralPath (Join-Path $checkDirectory $name)).Hash) { throw 'The source changed during compilation; repeat this check.' }
}
Write-Output "Complete lease page, two shared components and the actual FIELD selector compile against MudBlazor $mudVersion. Evidence: $checkDirectory"

if ($RenderPortfolio) {
    $renderDirectory = "$checkDirectory-render"
    [void](New-Item -ItemType Directory -Path $renderDirectory)
    [xml]$renderProject = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup><ItemGroup><FrameworkReference Include="Microsoft.AspNetCore.App"/></ItemGroup></Project>'
    $renderItems = $renderProject.SelectSingleNode('/Project/ItemGroup')
    $componentReference = $renderProject.CreateElement('ProjectReference'); $componentReference.SetAttribute('Include', $projectPath)
    [void]$renderItems.AppendChild($componentReference)
    foreach ($name in @($assemblies) + @('Moq', 'Castle.Core')) {
        $reference = $renderProject.CreateElement('Reference'); $reference.SetAttribute('Include', $name)
        $hint = $renderProject.CreateElement('HintPath'); $hint.InnerText = Join-Path $focusedOutput "$name.dll"
        [void]$reference.AppendChild($hint); [void]$renderItems.AppendChild($reference)
    }
    $renderProjectPath = Join-Path $renderDirectory 'PortfolioRenderCheck.csproj'; Add-OilGasRestoredOwnerReferences -Project $renderProject -RepositoryRoot (Split-Path $PSScriptRoot -Parent)
$renderProject.Save($renderProjectPath)
    @'
using System.Globalization;
using System.Net;
using System.Security.Claims;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using Beep.OilandGas.Models.Data.Company;

foreach (var mode in new[] { "full-en", "full-fr", "empty", "error", "missing-field", "missing-company", "loading", "uncertain-status" })
{
    CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(mode == "full-fr" ? "fr-FR" : "en-US");
    using var company = new CompanyClientContext();
    if (mode != "missing-company" && !company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], []))) throw new Exception("Selection failed.");
    var row = new LeaseSummary { LeaseId = "recorded-id", LandRightSubtype = "recorded-subtype", FieldId = "FIELD-A", DateZoneId = "UTC", Status = "COMPANY_REVIEW", LeaseName = "Name<recorded>", CalculatedInterest = .25m };
    var api = new Mock<ILeaseServiceClient>(MockBehavior.Strict);
    var completion = new TaskCompletionSource<List<LeaseSummary>>(TaskCreationOptions.RunContinuationsAsynchronously);
    var setup = api.Setup(x => x.GetAvailableLeasesAsync(It.Is<Dictionary<string, string>>(filter => filter["FieldId"] == "FIELD-A"), It.IsAny<CancellationToken>()));
    if (mode == "loading") setup.Returns(completion.Task);
    else if (mode == "error") setup.ThrowsAsync(new UnauthorizedAccessException());
    else setup.ReturnsAsync(mode == "empty" ? [] : [row]);
    using var state = new LeasePortfolioState(company, api.Object, (_, _, message) => message);
    using var creation = new LeaseCreationState(company, api.Object);
    var fields = new Mock<IFieldSelectionContext>(MockBehavior.Strict);
    fields.Setup(x => x.GetCurrentFieldIdAsync()).ReturnsAsync(mode == "missing-field" ? null : "FIELD-A");
    var services = new ServiceCollection(); services.AddLogging(); services.AddMudServices(); services.AddAuthorizationCore();
    services.AddSingleton(company); services.AddSingleton(state); services.AddSingleton(creation); services.AddSingleton(fields.Object);
    services.AddSingleton<AuthenticationStateProvider, FixtureAuth>(); services.AddSingleton<IJSRuntime, StaticJs>(); services.AddSingleton<NavigationManager, StaticNavigation>();
    await using var provider = services.BuildServiceProvider();
    await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
    var root = await renderer.Dispatcher.InvokeAsync(() => renderer.BeginRenderingComponent<Beep.OilandGas.Web.LeaseRenderHarness>());
    if (mode != "loading") await root.QuiescenceTask;
    if (mode.StartsWith("full")) {
        api.Setup(x => x.EvaluateLeaseAsync("recorded-id", "FIELD-A", It.IsAny<CancellationToken>())).ReturnsAsync(row);
        await renderer.Dispatcher.InvokeAsync(() => state.EvaluateAsync(state.Leases[0]));
    }
    if (mode == "uncertain-status") {
        api.Setup(x => x.UpdateLeaseStatusAsync("recorded-id", "ACTIVE", "FIELD-A")).ThrowsAsync(new IOException("lost"));
        await renderer.Dispatcher.InvokeAsync(() => state.UpdateStatusAsync(state.Leases[0], "ACTIVE"));
        await renderer.Dispatcher.InvokeAsync(() => state.LoadAsync(fields.Object.GetCurrentFieldIdAsync));
    }
    var html = await renderer.Dispatcher.InvokeAsync(root.ToHtmlString);
    await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory, mode + ".html"), html);
    void Require(bool condition, string message) { if (!condition) throw new Exception(mode + ": " + message); }
    if (mode.StartsWith("full")) {
        Require(html.Contains("recorded-subtype") && html.Contains("recorded-id") && html.Contains("Date zone UTC"), "Complete identity/provenance missing.");
        Require(WebUtility.HtmlDecode(html).Contains(mode == "full-fr" ? "0,25" : "0.25"), "Raw number was not preserved/formatted.");
        Require(html.Contains("COMPANY_REVIEW") && html.Contains("Name&lt;recorded&gt;") && !html.Contains("Name<recorded>"), "Status changed or recorded text was not encoded.");
        Require(html.Contains("Recorded Leases") && !html.Contains("No leases found"), "Loaded evidence is missing.");
    }
    else if (mode == "empty") Require(html.Contains("No leases found") && html.Contains("Recorded Leases"), "Explicit empty state missing.");
    else if (mode == "uncertain-status") Require(html.Contains("A status write for this FIELD is unconfirmed") && state.HasUnconfirmedStatus && !state.CanUpdate(state.Leases[0]), "Reload incorrectly cleared command uncertainty.");
    else {
        Require(!html.Contains("Recorded Leases") && !html.Contains("No leases found") && !html.Contains("recorded-id"), "Unavailable evidence became displayed rows or zero counts.");
        if (mode == "error") Require(html.Contains("Lease records could not be confirmed") && html.Contains("role=\"alert\""), "Failure is not explicit.");
        if (mode == "missing-field") Require(html.Contains("Choose an available FIELD"), "Missing FIELD prompt missing.");
        if (mode == "missing-company") Require(html.Contains("Select an available company"), "Missing company prompt missing.");
        if (mode == "loading") { Require(html.Contains("Loading lease records"), "Loading state missing."); state.Dispose(); completion.SetCanceled(); await root.QuiescenceTask; }
    }
    Console.WriteLine(mode + " exact-page static rendering passed.");
}
foreach (var mode in new[] { "selector-full", "selector-empty", "selector-unavailable", "selector-error", "selector-missing-company", "selector-loading", "selector-confirmed" })
{
    using var company = new CompanyClientContext();
    if (mode != "selector-missing-company" && !company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Reader"], []))) throw new Exception("Selection failed.");
    var review = new FieldSelectionReview(new("tenant", "company"), new("connection", "fingerprint"), "actor", "revision", mode == "selector-empty" ? null : "A");
    var current = new FieldResponse { FieldId = mode is "selector-empty" or "selector-unavailable" ? "" : "A", SelectionContext = review, SelectionAvailable = mode is not ("selector-empty" or "selector-unavailable") };
    var fields = new Mock<IFieldSelectionContext>(MockBehavior.Strict);
    var completion = new TaskCompletionSource<List<FieldListItem>>(TaskCreationOptions.RunContinuationsAsynchronously);
    var setup = fields.Setup(x => x.GetAvailableFieldsAsync(It.IsAny<CancellationToken>()));
    if (mode == "selector-loading") setup.Returns(completion.Task);
    else if (mode == "selector-error") setup.ThrowsAsync(new IOException("fixture"));
    else setup.ReturnsAsync(mode == "selector-empty" ? [] : [new() { FieldId = "A", FieldName = "Name<recorded>" }, new() { FieldId = "B", FieldName = "Beta" }]);
    fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(current);
    fields.Setup(x => x.SetCurrentFieldAsync(It.Is<SetActiveFieldRequest>(request => request.FieldId == "B" && request.Context == review), It.IsAny<CancellationToken>())).ReturnsAsync(true);
    using var state = new FieldSelectionState(company, fields.Object, (_, _, sentence) => sentence);
    var services = new ServiceCollection(); services.AddLogging(); services.AddMudServices(); services.AddSingleton(state); services.AddSingleton<IJSRuntime, StaticJs>();
    await using var provider = services.BuildServiceProvider(); await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
    var root = await renderer.Dispatcher.InvokeAsync(() => renderer.BeginRenderingComponent<Beep.OilandGas.Web.Components.FieldSelector>());
    if (mode != "selector-loading") await root.QuiescenceTask;
    if (mode == "selector-confirmed") await renderer.Dispatcher.InvokeAsync(() => state.SelectAsync("B"));
    var html = await renderer.Dispatcher.InvokeAsync(root.ToHtmlString); await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory, mode + ".html"), html);
    void Require(bool condition, string message) { if (!condition) throw new Exception(mode + ": " + message); }
    Require(html.Contains("Active field") && !html.Contains("Name<recorded>"), "Selector label/encoded rendering failed.");
    if (mode == "selector-full") Require(state.Loaded && state.SelectedId == "A", "Reviewed selection missing.");
    if (mode == "selector-empty") Require(html.Contains("No fields are available") && state.Loaded, "Confirmed empty state missing.");
    if (mode == "selector-unavailable") Require(html.Contains("saved FIELD is no longer available") && state.SelectedId is null, "Saved preference became displayed authority.");
    if (mode == "selector-error") Require(html.Contains("could not be confirmed") && html.Contains("role=\"alert\""), "Explicit failure missing.");
    if (mode == "selector-missing-company") Require(html.Contains("Select an available company") && !state.Loaded, "Company prompt missing.");
    if (mode == "selector-confirmed") Require(html.Contains("FIELD preference saved") && !state.Loaded && state.SelectedId == "B", "Confirmed preference did not require fresh review.");
    if (mode == "selector-loading") { Require(html.Contains("Updating field selection") && !state.Loaded, "Loading state missing."); state.Dispose(); completion.SetCanceled(); await root.QuiescenceTask; }
    Console.WriteLine(mode + " exact-component static rendering passed.");
}
public sealed class FixtureAuth : AuthenticationStateProvider {
    public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Administrator")], "render-fixture"))));
}
public sealed class StaticJs : IJSRuntime {
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => throw new NotSupportedException("Static rendering cannot execute JS.");
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken token, object?[]? args) => throw new NotSupportedException("Static rendering cannot execute JS.");
}
public sealed class StaticNavigation : NavigationManager {
    public StaticNavigation() => Initialize("https://render.test/", "https://render.test/ppdm39/operations/leaseacquisition");
    protected override void NavigateToCore(string uri, bool forceLoad) => throw new NotSupportedException("Static rendering cannot navigate.");
}
'@ | Set-Content -LiteralPath (Join-Path $renderDirectory 'Program.cs') -Encoding utf8
    & dotnet run --project $renderProjectPath --configuration $Configuration --verbosity quiet --nologo
    if ($LASTEXITCODE -ne 0) { throw "Static portfolio rendering failed; inspect $renderDirectory." }
    Write-Output "8 exact-page and 7 exact-selector static rendering checks passed. Host/DI registration/OIDC/live HTTP/browser/provider execution remains unverified. Evidence: $renderDirectory"
}

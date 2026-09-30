using Beep.OilandGas.PPDM39.Core;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using MudBlazor.Services;
using MudBlazor.Translations;
using Blazored.LocalStorage;
using System.Text;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Core.Tree;
using Beep.OilandGas.PPDM39.DataManagement.Core.Tree;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.DataManagement.Repositories;
using Beep.OilandGas.Web.Theme;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Client.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using Beep.Foundation.IdentityServer.Shared.Extensions;
using Beep.Foundation.IdentityServer.Shared.Identity;
using Duende.AccessTokenManagement.OpenIdConnect;
using Beep.OilandGas.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Ensure UTF-8 encoding for proper international text support
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
Console.OutputEncoding = Encoding.UTF8;

// Configure Kestrel
builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

// ============================================
// MUDBLAZOR & UI SERVICES
// ============================================
builder.Services.AddMudServices();
// MudBlazor's own component texts (pager, pickers, data-grid filters) in the request's language: the MudBlazor
// organisation's translations, as MudBlazor's localization documentation recommends. OilGas offers English only today;
// a culture added below is served by MudBlazor's texts without further wiring.
builder.Services.AddMudTranslations();

// Beep.Razor.Components — shared data management UI components (setup wizard, CRUD, drivers, etc.)
Beep.Razor.Components.Extensions.BeepStudioServiceCollectionExtensions.AddBeepBlazorStudio(builder.Services);

builder.Services.AddBlazoredLocalStorage();

// English only (Fahad, 2026-09-23): the application has no resources for another language. It declared ar-SA with no
// localization, so an Arabic browser got English laid out as if nothing was wrong (S3-06 §8).
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-US");
    options.SupportedCultures = [new System.Globalization.CultureInfo("en-US")];
    options.SupportedUICultures = [new System.Globalization.CultureInfo("en-US")];
});

// ============================================
// BLAZOR COMPONENTS
// ============================================
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();

// ============================================
// SIGN-IN (the identity server) AND ROLES (the OilGas repository, through the API)
// ============================================
// The identity server authenticates; everything a person may do is the OilGas repository's, which the API owns — the
// Web holds no user store, so it asks the API with the person's own access token (RepositoryAccountClient). The library
// signs people in (code + PKCE, the cookie, the person's tokens refreshed by Duende's token manager), maps /account/*
// (sign-in, registration, sign-out, back-channel logout, the unavailable page) and refuses an account OilGas has switched
// off at sign-in, on the cookie and on an open circuit (OilGasAccountAdmission).
//
// It replaced a hand-written OpenID Connect setup: a client id that fell back to "beep_oilgas_web" and a secret that fell
// back to empty, the shared "beep-api" scope, a registration call that failed sign-in whenever the API did, a token
// dictionary nothing refreshed, a /authentication/start-login endpoint that passed any return address on, and a sign-out
// link to a route that did not exist (S3-06 §3). The client id and secret are the pair the identity server's console
// generated (IdentityServer:ClientId / ClientSecret); IdentityServer:ApiScopes names the OilGas API's identifier and
// IdentityServer:AccountScopes the scopes /account/manage uses.
builder.AddBeepClientApp(options =>
{
    options.CookieName = ".Beep.OilGas.Auth";
    options.UiLocales = () => System.Globalization.CultureInfo.CurrentUICulture.Name;
});

builder.Services.AddScoped<IAccountAdmission, OilGasAccountAdmission>();
builder.Services.AddChainedClaimsTransformation<OilGasClaimsTransformation>();

// ============================================
// LOG AND FAILURES — every failure logged and stored under a reference the person is shown
// ============================================
// The identity server's client library reports every failure it catches through this, and does not start without it. A
// request that fails is answered by /Error with the reference; a page that fails, by the layout's error boundary; the
// administrator reads them at /admin/failures. The store is the API's too (Diagnostics:Provider /
// Diagnostics:ConnectionString) — the API applies its schema, so this host never migrates it.
builder.Services.AddOilGasDiagnostics(builder.Configuration);

// SEC-BCL-01. The sign-outs the identity server sends (back-channel logout), in the same store: a cookie session keeps no
// state, so this record is what refuses a cookie the identity server ended, and it has to outlive a restart.
builder.Services.AddBeepBackchannelLogoutStore<Beep.OilandGas.Diagnostics.OilGasDiagnosticsDbContext>();

// ============================================
// THE OILGAS API
// ============================================
// Every client carries the signed-in person's access token (the library's token manager, refreshed before it expires).
var api = OilGasApiAddress.Resolve(builder.Configuration);
builder.Services.AddSingleton(api);
builder.Services.AddHttpClient<RepositoryAccountClient>(client => client.BaseAddress = api.Address);
builder.Services.AddScoped<OilGasAccountDeactivation>();
builder.Services.AddScoped<UserAdministrationClient>();
builder.Services.AddScoped<ModuleDatabaseClient>();
builder.Services.AddScoped<PersonaClient>();
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = api.Address;
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
})
.AddUserAccessTokenHandler();

// ============================================
// THEME
// ============================================
// The theme is read once (singleton); light or dark is each person's, on their circuit (scoped).
builder.Services.AddSingleton<IThemeProvider, ThemeProvider>();
builder.Services.AddScoped<ThemeState>();

// ============================================
// APPLICATION SERVICES
// ============================================
// Register the Beep.OilandGas client app (auto-detect local/remote)
// The client library's calls carry the signed-in person's access token (OilGasUserTokenProvider).
builder.Services.AddScoped<Beep.OilandGas.Client.Authentication.IAuthenticationProvider, OilGasUserTokenProvider>();
builder.Services.AddBeepOilandGasAppRemote(builder.Configuration);

    // Keep the Beep.OilandGas app facade registered for legacy flows.
    // Prefer focused typed clients or scoped services in pages and components where an HTTP/API seam already exists.
        
    // Focused web clients and scoped services
        builder.Services.AddScoped<IAccountingServiceClient, AccountingServiceClient>();
        builder.Services.AddScoped<IPersonaContextService, PersonaContextService>();
        builder.Services.AddScoped<IAfeServiceClient, AfeServiceClient>();
        builder.Services.AddScoped<IDataManagementService, DataManagementService>();
        builder.Services.AddScoped<Beep.OilandGas.Web.Services.IConnectionService, Beep.OilandGas.Web.Services.ConnectionService>();
        builder.Services.AddScoped<IBusinessProcessServiceClient, BusinessProcessServiceClient>();
        builder.Services.AddScoped<ICalculationServiceClient, CalculationServiceClient>();
        builder.Services.AddScoped<IComplianceServiceClient, ComplianceServiceClient>();
        builder.Services.AddScoped<IDecommissioningServiceClient, DecommissioningServiceClient>();
        builder.Services.AddScoped<IDevelopmentServiceClient, DevelopmentServiceClient>();
        builder.Services.AddScoped<IDrillingServiceClient, DrillingServiceClient>();
        builder.Services.AddScoped<IEnhancedRecoveryServiceClient, EnhancedRecoveryServiceClient>();
        builder.Services.AddScoped<IExplorationServiceClient, ExplorationServiceClient>();
        builder.Services.AddScoped<IHSEServiceClient, HSEServiceClient>();
        builder.Services.AddScoped<ILeaseServiceClient, LeaseServiceClient>();
        builder.Services.AddScoped<IOperationsServiceClient, OperationsServiceClient>();
        builder.Services.AddScoped<IProductionServiceClient, ProductionServiceClient>();
        builder.Services.AddScoped<IPumpServiceClient, PumpServiceClient>();
        builder.Services.AddScoped<IPropertiesServiceClient, PropertiesServiceClient>();
        builder.Services.AddScoped<IWellLookupServiceClient, WellLookupServiceClient>();
        builder.Services.AddScoped<IWellStatusServiceClient, WellStatusServiceClient>();
        builder.Services.AddScoped<IWorkOrderServiceClient, WorkOrderServiceClient>();
        builder.Services.AddScoped<IPermitServiceClient, PermitServiceClient>();
        builder.Services.AddScoped<IProductionOperationsClient, ProductionOperationsClient>();
        builder.Services.AddScoped<IEnhancedRecoveryClient, EnhancedRecoveryClient>();

        // Progress Tracking Client - SignalR client for real-time progress updates
        builder.Services.AddScoped<IProgressTrackingClient, ProgressTrackingClient>();

        // LifeCycle Service - Client service for lifecycle management operations
        builder.Services.AddScoped<ILifeCycleService, LifeCycleService>();

        // Client-side file export service for JSON/text downloads from workbenches
        builder.Services.AddScoped<IClientFileExportService, ClientFileExportService>();

        // Demo Database Service - Client service for demo database operations
        builder.Services.AddScoped<Beep.OilandGas.Web.Services.IDemoDatabaseService, Beep.OilandGas.Web.Services.DemoDatabaseService>();

        // First-run setup wizard service

        // Multi-page PPDM39 database setup wizard state
        builder.Services.AddScoped<CreateDatabaseWizardState>();

        // ── Phase 5: Workflow Notifications & Task Inbox ─────────────────
        builder.Services.AddScoped<IUnifiedTaskInboxService, UnifiedTaskInboxService>();
        builder.Services.AddScoped<INotificationService, NotificationService>();
        builder.Services.AddSingleton<IEmailNotificationProvider, EmailNotificationProvider>();
        builder.Services.AddSingleton<IExternalWebhookTriggerService>(sp =>
        {
            var httpClient = sp.GetRequiredService<HttpClient>();
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<ExternalWebhookTriggerService>();
            return new ExternalWebhookTriggerService(httpClient, logger);
        });

// PPDM39 Data Management Services

// Common Column Handler
builder.Services.AddSingleton<ICommonColumnHandler, CommonColumnHandler>();

// Metadata Repository
builder.Services.AddSingleton<IPPDMMetadataRepository>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<PPDMMetadataService>>();
    return new PPDMMetadataService(logger);
});

// Tree Builder (depends on metadata repository)
builder.Services.AddScoped<IPPDMTreeBuilder>(sp =>
{
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    return new PPDMTreeBuilder(metadata);
});

// Defaults Repository (optional - only if needed for direct database access)
// Note: This requires IDMEEditor which may not be available in the Web project
// If you need defaults, consider calling the API service instead
// builder.Services.AddScoped<IPPDM39DefaultsRepository>(sp =>
// {
//     var editor = sp.GetRequiredService<IDMEEditor>();
//     var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
//     return new PPDM39DefaultsRepository(editor, "PPDM39", metadata);
// });

var app = builder.Build();

// ============================================
// HTTP PIPELINE — Microsoft's order
// ============================================
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseRequestLocalization();
app.UseHttpsRedirection();

// Files on disk before authentication, so a stylesheet or a script does not ask the API who the person is: authentication
// used to run first, and every asset cost the role bridge an HTTP call (S3-06 §3).
app.UseStaticFiles();

// Authentication, authorization and the library's /account endpoints.
app.UseBeepClientApp();

// After authentication and authorization, as ASP.NET Core orders them: the sign-out form posts with its token.
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<Beep.OilandGas.Web.App>()
    .AddInteractiveServerRenderMode();

app.Run();

/// <summary>The entry point, visible to the test host.</summary>
public partial class Program;

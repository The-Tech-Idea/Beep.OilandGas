using Beep.OilandGas.Models.Data.Company;
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
using Beep.OilandGas.Client.App;
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

// What a page shows of a failure, and of an API answer that was not a success (OILGAS-CATCH-01): the shared page notifier
// (reported, said with its reference) and the one wording of the API's refusals and failures.
builder.Services.AddFailureNotifier();
builder.Services.AddSingleton<Beep.OilandGas.Web.Services.OilGasApiFailures>();
// A service or page state whose contract is a failed result (Success = false, an Error the page renders) words any
// failure through this — the API's answers through the wording above, anything else reported with its reference.
builder.Services.AddSingleton<Beep.OilandGas.Web.Services.OilGasCallFailures>();
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
    .AddOilGasInteractiveServerComponents();

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
builder.Services.AddScoped<CompanyClientContext>();
builder.Services.AddScoped<CompanyEnvironmentChangeSignal>();
builder.Services.AddScoped<CompanyEnvironmentPollClock>();
builder.Services.AddScoped<CompanyAccountingHandoffState>();
builder.Services.AddScoped<CommercialTermsWorkspaceState>();
builder.Services.AddScoped<DeliveryAcceptanceWorkspaceState>();
builder.Services.AddScoped<CompanyBillingPolicyWorkspaceState>();
builder.Services.AddScoped<CompanyLedgerAccountState>();
builder.Services.AddScoped<CompanySalesInvoiceWorkspaceState>();
builder.Services.AddScoped<CompanySalesReceivableWorkspaceState>();
builder.Services.AddScoped<ProductionInterventionDecisionState>();
builder.Services.AddScoped<CompanyMaintenanceHandoffState>();
builder.Services.AddScoped<CompanyMaintenanceReceivingState>();
builder.Services.AddScoped<WorkOrderExecutionPermitState>();
builder.Services.AddScoped<WorkOrderMaterialAllocationState>();
builder.Services.AddScoped<WorkOrderExecutionPermitDecisionState>();
builder.Services.AddScoped<WorkOrderExecutionStartState>();
builder.Services.AddScoped<WorkOrderSubmissionState>();
builder.Services.AddScoped<WorkOrderMaterialUsageState>();
builder.Services.AddScoped<WorkOrderLaborUsageState>();
builder.Services.AddScoped<WorkOrderBudgetState>();
builder.Services.AddScoped<WorkOrderBudgetDecisionState>();
builder.Services.AddScoped<WorkOrderBudgetDecisionStatusState>();
builder.Services.AddScoped<WorkOrderLaborCostState>();
builder.Services.AddScoped<WorkOrderMaterialCostState>();
builder.Services.AddScoped<WorkOrderFinancialWorkspaceState>();
builder.Services.AddScoped<WorkOrderCompletionReviewState>();
builder.Services.AddScoped<WorkOrderCompletionDecisionState>();
builder.Services.AddScoped<WorkOrderReturnHandoffState>();
builder.Services.AddScoped<WorkOrderReturnAcceptanceState>();
builder.Services.AddScoped<MaintenanceClosureState>();
builder.Services.AddScoped<MaintenanceAssetEvidenceState>();
builder.Services.AddScoped<MaintenanceAssetReturnState>();
builder.Services.AddScoped<WorkOrderExecutionHandoffState>();
builder.Services.AddScoped<CompanyPersonnelPolicyState>();
builder.Services.AddScoped<CompanyMaintenanceReturnPolicyState>();
builder.Services.AddScoped<CompanyPersonnelIdentityState>();
builder.Services.AddScoped<CompanySalesInvoiceReviewWorkspaceState>();
builder.Services.AddScoped<CommercialAmendmentWorkspaceState>();
builder.Services.AddScoped<CompanyAccountingStepState>();
builder.Services.AddScoped<APInvoiceWorkspaceState>();
builder.Services.AddScoped<PurchaseOrderWorkspaceState>(services=>new(services.GetRequiredService<CompanyClientContext>(),services.GetRequiredService<InventoryWorkspaceState>()));
builder.Services.AddScoped<InventoryWorkspaceState>();
var originalAuthentication = builder.Services.Last(x => x.ServiceType == typeof(AuthenticationStateProvider));
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
{
    var original = (AuthenticationStateProvider)(originalAuthentication.ImplementationInstance ??
        originalAuthentication.ImplementationFactory?.Invoke(sp) ??
        ActivatorUtilities.CreateInstance(sp, originalAuthentication.ImplementationType!));
    return new CompanyAuthenticationStateProvider(original, sp.GetRequiredService<CompanyClientContext>(),
        sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>(), PartyIdClaims.Find);
});
builder.Services.AddScoped<ICompanyAccessClient, CompanyAccessClient>();
// Per-component state; transport and failure wording retain their existing canonical owners.
builder.Services.AddTransient(sp => new AssetHierarchyState(sp.GetRequiredService<CompanyClientContext>(),
    (path, token) => sp.GetRequiredService<ApiClient>().GetAsync<AssetHierarchyNode>(path, token),
    (failure, operation, sentence) => sp.GetRequiredService<OilGasCallFailures>().Explain(failure, operation, sentence)));
builder.Services.AddTransient(sp => new HierarchyConfigurationState(sp.GetRequiredService<CompanyClientContext>(),
    (path, token) => sp.GetRequiredService<ApiClient>().GetAsync<List<HierarchyConfig>>(path, token),
    (failure, operation, sentence) => sp.GetRequiredService<OilGasCallFailures>().Explain(failure, operation, sentence),
    (path, rows, token) => sp.GetRequiredService<ApiClient>().PutAsync<List<HierarchyConfig>, bool>(path, rows, token)));
builder.Services.AddTransient(sp => new AssetGrantAdministrationState(sp.GetRequiredService<CompanyClientContext>(),
    (path, token) => sp.GetRequiredService<ApiClient>().GetAsync<List<AssetAccess>>(path, token),
    (path, request, token) => sp.GetRequiredService<ApiClient>().PostAsync<Beep.OilandGas.Models.Data.AccessControl.GrantAccessRequest, bool>(path, request, token),
    (path, request, token) => sp.GetRequiredService<ApiClient>().PostAsync<Beep.OilandGas.Models.Data.AccessControl.RevokeAccessRequest, bool>(path, request, token),
    (failure, operation, sentence) => sp.GetRequiredService<OilGasCallFailures>().Explain(failure, operation, sentence),
    sp.GetRequiredService<IBeepOilandGasApp>().AccessControl));

builder.Services.AddScoped(sp => new CompanySelectionService(sp.GetRequiredService<ICompanyAccessClient>(),
    sp.GetRequiredService<AuthenticationStateProvider>(), sp.GetRequiredService<CompanyClientContext>(), PartyIdClaims.Find));

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
builder.Services.AddHttpClient<SetupBootstrapDriverClient>(client => client.BaseAddress = api.Address);
builder.Services.AddScoped<OilGasAccountDeactivation>();
builder.Services.AddScoped<UserAdministrationClient>();
builder.Services.AddScoped<CompanyWellQualityClient>();
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
        builder.Services.AddScoped<ICompanyLedgerBookClient>(sp=>sp.GetRequiredService<IAccountingServiceClient>());
        builder.Services.AddScoped<IPersonaClient>(sp => sp.GetRequiredService<PersonaClient>());
        builder.Services.AddScoped<IPersonaContextService>(sp => new PersonaContextService(
            sp.GetRequiredService<IPersonaClient>(), sp.GetRequiredService<AuthenticationStateProvider>(),
            sp.GetRequiredService<CompanyClientContext>(), PartyIdClaims.Find));
        builder.Services.AddScoped<IAfeServiceClient, AfeServiceClient>();
        builder.Services.AddScoped<IDataManagementService, DataManagementService>();
        builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IFieldSelectionContext>(sp => sp.GetRequiredService<IDataManagementService>());
        builder.Services.AddScoped<Beep.OilandGas.Web.Services.IConnectionService, Beep.OilandGas.Web.Services.ConnectionService>();
        builder.Services.AddScoped<IBusinessProcessServiceClient, BusinessProcessServiceClient>();
        builder.Services.AddScoped<ICompanyMaintenanceHandoffClient>(sp=>sp.GetRequiredService<IBusinessProcessServiceClient>());
        builder.Services.AddScoped<IRecordedProcessClient>(sp=>sp.GetRequiredService<IBusinessProcessServiceClient>());
        builder.Services.AddScoped<ICalculationServiceClient, CalculationServiceClient>();
        builder.Services.AddScoped<IComplianceServiceClient, ComplianceServiceClient>();
        builder.Services.AddScoped<IDecommissioningServiceClient, DecommissioningServiceClient>();
        builder.Services.AddScoped<IDevelopmentServiceClient, DevelopmentServiceClient>();
        builder.Services.AddScoped<Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage.ProtectedSessionStorage>();
        builder.Services.AddScoped<IDevelopmentDraftIntentStore, DevelopmentDraftIntentStore>();
        builder.Services.AddScoped<IDrillingServiceClient, DrillingServiceClient>();
        builder.Services.AddScoped<IEnhancedRecoveryServiceClient, EnhancedRecoveryServiceClient>();
        builder.Services.AddScoped<IExplorationServiceClient, ExplorationServiceClient>();
        builder.Services.AddScoped<Beep.OilandGas.Client.App.Services.Field.IFieldService>(sp => sp.GetRequiredService<IBeepOilandGasApp>().Field);
        builder.Services.AddScoped<IHSEServiceClient, HSEServiceClient>();
        builder.Services.AddScoped<IInvestigatorAssignmentIntentStore, InvestigatorAssignmentIntentStore>();
        builder.Services.AddScoped<IPermitApplicationIntentStore, PermitApplicationIntentStore>();
        builder.Services.AddScoped<IncidentReportIntentStore>();
        builder.Services.AddScoped<IIncidentReportIntentStore>(sp => sp.GetRequiredService<IncidentReportIntentStore>());
        builder.Services.AddScoped<ICompanyHseWorkflowIntentStore>(sp => sp.GetRequiredService<IncidentReportIntentStore>());
        builder.Services.AddScoped<IIncidentRcaIntentStore>(sp => sp.GetRequiredService<IncidentReportIntentStore>());
        builder.Services.AddScoped<IIncidentRcaHandoffIntentStore>(sp => sp.GetRequiredService<IncidentReportIntentStore>());
        builder.Services.AddScoped<IFieldObligationIntentStore, FieldObligationIntentStore>();
        builder.Services.AddScoped<ICorrectivePlanIntentStore, CorrectivePlanIntentStore>();
        builder.Services.AddScoped<ICorrectiveActionIntentStore, CorrectiveActionIntentStore>();
        builder.Services.AddScoped<CompanySetupIntentStore>();
        builder.Services.AddScoped<IGhgSelectionIntentStore>(sp => sp.GetRequiredService<CompanySetupIntentStore>());
        builder.Services.AddScoped<ICompanyEnvironmentIntentStore>(sp => sp.GetRequiredService<CompanySetupIntentStore>());
        builder.Services.AddScoped<ISystemSetupIntentStore>(sp => sp.GetRequiredService<CompanySetupIntentStore>());
        builder.Services.AddScoped<IInitialCompanyIntentStore>(sp => sp.GetRequiredService<CompanySetupIntentStore>());
        builder.Services.AddScoped<ICompanyMembershipIntentStore>(sp => sp.GetRequiredService<CompanySetupIntentStore>());
        builder.Services.AddScoped<ICompanyRoleIntentStore>(sp => sp.GetRequiredService<CompanySetupIntentStore>());
        builder.Services.AddTransient(sp => new CorrectivePlanWorkspaceState(sp.GetRequiredService<CompanyClientContext>(),sp.GetRequiredService<Beep.OilandGas.Models.Core.Interfaces.IFieldSelectionContext>(),sp.GetRequiredService<Beep.OilandGas.Client.App.Services.Field.IFieldService>(),sp.GetRequiredService<ICorrectivePlanIntentStore>(),sp.GetRequiredService<OilGasCallFailures>().Explain));
        builder.Services.AddTransient(sp => new InvestigatorAssignmentState(sp.GetRequiredService<CompanyClientContext>(),
            sp.GetRequiredService<Beep.OilandGas.Models.Core.Interfaces.IFieldSelectionContext>(), sp.GetRequiredService<IHSEServiceClient>(),
            sp.GetRequiredService<OilGasCallFailures>().Explain,sp.GetRequiredService<IInvestigatorAssignmentIntentStore>()));
        builder.Services.AddScoped<ILeaseServiceClient, LeaseServiceClient>();
        builder.Services.AddTransient<LeaseCreationState>();
        builder.Services.AddTransient(sp => new FieldSelectionState(sp.GetRequiredService<CompanyClientContext>(),
            sp.GetRequiredService<Beep.OilandGas.Models.Core.Interfaces.IFieldSelectionContext>(), sp.GetRequiredService<OilGasCallFailures>().Explain));
        builder.Services.AddTransient(sp => new LeasePortfolioState(sp.GetRequiredService<CompanyClientContext>(),
            sp.GetRequiredService<ILeaseServiceClient>(), sp.GetRequiredService<OilGasCallFailures>().Explain));
        builder.Services.AddScoped<IOperationsServiceClient, OperationsServiceClient>();
        builder.Services.AddScoped<IExecutiveServiceClient, ExecutiveServiceClient>();
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

        // ── Phase 5: Workflow Notifications & Task Inbox ─────────────────
        builder.Services.AddScoped<IUnifiedTaskInboxService, UnifiedTaskInboxService>();
        builder.Services.AddScoped<INotificationService, NotificationService>();
        builder.Services.AddSingleton<IEmailNotificationProvider, EmailNotificationProvider>();
        builder.Services.AddSingleton<IExternalWebhookTriggerService>(sp =>
        {
            var httpClient = sp.GetRequiredService<HttpClient>();
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<ExternalWebhookTriggerService>();
            return new ExternalWebhookTriggerService(httpClient, logger, sp.GetRequiredService<TheTechIdeaWeb.Diagnostics.IFailureReporter>());
        });

// PPDM39 Data Management Services

// Common Column Handler
builder.Services.AddSingleton<ICommonColumnHandler, CommonColumnHandler>();

// Metadata Repository
builder.Services.AddSingleton<Beep.OilandGas.PPDM39.Core.Metadata.IPPDMMetadataContributor, Beep.OilandGas.PermitsAndApplications.Metadata.PermitApplicationNativeMetadata>();
builder.Services.AddSingleton<IPPDMMetadataRepository>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<PPDMMetadataService>>();
    return new PPDMMetadataService(logger, contributors: sp.GetServices<Beep.OilandGas.PPDM39.Core.Metadata.IPPDMMetadataContributor>());
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

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MudBlazor.Services;
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
using Beep.Foundation.IdentityServer.Shared.Authentication;
using Beep.Foundation.IdentityServer.Shared.Services;

// ============================================
// OIDC AUTHENTICATION SCHEME
// ============================================
// const string OIDC_SCHEME = "oidc"; // Reserved for future OIDC authentication

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
builder.Services.AddBlazoredLocalStorage();

// Configure request localization
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-US");
    options.SupportedCultures = new[]
    {
        new System.Globalization.CultureInfo("en-US"),
        new System.Globalization.CultureInfo("ar-SA")
    };
    options.SupportedUICultures = new[]
    {
        new System.Globalization.CultureInfo("en-US"),
        new System.Globalization.CultureInfo("ar-SA")
    };
});

// ============================================
// BLAZOR COMPONENTS
// ============================================
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();

// ============================================
// TOKEN MANAGEMENT SERVICES
// ============================================
// TokenProvider is a SINGLETON that stores tokens per-user (using user ID as key)
// This allows tokens to persist across SignalR reconnections for authenticated users
builder.Services.AddSingleton<Beep.Foundation.IdentityServer.Shared.Authentication.TokenProvider>();

// Register TokenHandler for attaching access tokens to outgoing requests
// Microsoft's official pattern uses Scoped (see BlazorWebAppOidcServer sample)
builder.Services.AddScoped<Beep.Foundation.IdentityServer.Shared.Authentication.TokenHandler>();

// Register UserService and UserCircuitHandler for maintaining user state across SignalR connections
builder.Services.AddScoped<Beep.Foundation.IdentityServer.Shared.Authentication.UserService>();
builder.Services.TryAddEnumerable(
    ServiceDescriptor.Scoped<CircuitHandler, Beep.Foundation.IdentityServer.Shared.Authentication.UserCircuitHandler>());

// ============================================
// API CLIENT SERVICES
// ============================================
// ApiClient: Generic HTTP client for calling the API service
var apiServiceUrl = builder.Configuration["ApiService:BaseUrl"] ?? "https://localhost:7001";
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiServiceUrl);
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
})
.AddHttpMessageHandler<Beep.Foundation.IdentityServer.Shared.Authentication.TokenHandler>();

// ============================================
// AUTHENTICATION CONFIGURATION
// ============================================
const string OIDC_SCHEME = "oidc";

// Add cascading authentication state for Blazor
builder.Services.AddCascadingAuthenticationState();

// Register the OIDC-compatible AuthenticationStateProvider
builder.Services.AddScoped<AuthenticationStateProvider, Beep.Foundation.IdentityServer.Shared.Authentication.OidcAuthenticationStateProvider>();

// Get IdentityServer URL for OIDC configuration
// Try Aspire service discovery first, then fallback to config
var identityServerUrl = builder.Configuration["services:identityserver:https:0"] 
    ?? builder.Configuration["IdentityServer:BaseUrl"] 
    ?? "https://localhost:7062/";

// Configure Authentication: Cookie as DEFAULT scheme, OIDC for CHALLENGE
builder.Services.AddAuthentication(options =>
{
    // Cookie is the default - this is what Blazor uses to check if user is authenticated
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    // OIDC is used when we need to challenge (redirect to login)
    options.DefaultChallengeScheme = OIDC_SCHEME;
    options.DefaultSignOutScheme = OIDC_SCHEME;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.Name = ".Beep.OilGas.Auth";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
    
    // These paths trigger OIDC challenge when user is not authenticated
    options.LoginPath = "/authentication/login";
    options.LogoutPath = "/authentication/logout";
    options.AccessDeniedPath = "/access-denied";
})
.AddOpenIdConnect(OIDC_SCHEME, options =>
{
    // After OIDC login, create a cookie
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.SignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    
    options.Authority = identityServerUrl;
    options.ClientId = "beep_oilgas_web";
    options.ClientSecret = "web_secret";
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.UsePkce = true;
    
    // Scopes - offline_access is required for refresh tokens
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Scope.Add("roles");
    options.Scope.Add("beep-api");
    options.Scope.Add("offline_access"); // Required for token refresh
    
    // IMPORTANT: Configure claim mapping for Blazor
    options.MapInboundClaims = false;
    options.TokenValidationParameters.NameClaimType = "name";
    options.TokenValidationParameters.RoleClaimType = "role";
    options.GetClaimsFromUserInfoEndpoint = true;
    options.SaveTokens = true;
    
    // Callback paths
    options.CallbackPath = "/signin-oidc";
    options.SignedOutCallbackPath = "/signout-callback-oidc";
    options.RemoteSignOutPath = "/signout-oidc";
    
    // Development settings
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    
    // Cookie settings to fix correlation issues
    options.CorrelationCookie.SameSite = SameSiteMode.None;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
    options.NonceCookie.SameSite = SameSiteMode.None;
    options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
    
    options.Events = new OpenIdConnectEvents
    {
        OnRedirectToIdentityProvider = context =>
        {
            // Add client_id as a query parameter to the authorization request
            // This allows IdentityServer to detect which client is requesting authentication
            // and apply the appropriate branding (BeepOilGasTheme)
            context.ProtocolMessage.SetParameter("client_id", "beep_oilgas_web");
            return Task.CompletedTask;
        },
        
        // Fired when token response is received from the authorization server
        // This is the BEST place to capture the access token
        OnTokenResponseReceived = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("OIDC.TokenResponse");
            
            var accessToken = context.TokenEndpointResponse?.AccessToken;
            
            logger.LogInformation("OIDC TokenResponseReceived - HasAccessToken: {HasToken}, TokenLength: {Length}", 
                !string.IsNullOrEmpty(accessToken), accessToken?.Length ?? 0);
            
            // Store the raw access token in HttpContext.Items for later use
            if (!string.IsNullOrEmpty(accessToken))
            {
                context.HttpContext.Items["RawAccessToken"] = accessToken;
            }
            
            return Task.CompletedTask;
        },
        
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("OIDC.TokenValidated");
            
            var userId = context.Principal?.FindFirst("sub")?.Value;
            var email = context.Principal?.FindFirst("email")?.Value;
            var name = context.Principal?.FindFirst("name")?.Value;
            
            logger.LogInformation("OIDC Token validated for user: {UserId}, Email: {Email}, Name: {Name}", 
                userId, email, name);
            
            // Try to get access token from multiple sources
            string? accessToken = null;
            
            // Source 1: TokenEndpointResponse (available in authorization code flow)
            if (context.TokenEndpointResponse is not null)
            {
                accessToken = context.TokenEndpointResponse.AccessToken;
                logger.LogDebug("OIDC: Got token from TokenEndpointResponse");
            }
            
            // Source 2: HttpContext.Items (set in OnTokenResponseReceived)
            if (string.IsNullOrEmpty(accessToken) && 
                context.HttpContext.Items.TryGetValue("RawAccessToken", out var rawToken) &&
                rawToken is string token)
            {
                accessToken = token;
                logger.LogDebug("OIDC: Got token from HttpContext.Items[RawAccessToken]");
            }
            
            // Source 3: ProtocolMessage (for implicit flow)
            if (string.IsNullOrEmpty(accessToken))
            {
                accessToken = context.ProtocolMessage?.AccessToken;
                if (!string.IsNullOrEmpty(accessToken))
                {
                    logger.LogDebug("OIDC: Got token from ProtocolMessage");
                }
            }
            
            // Store the token if we have one
            if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(userId))
            {
                var tokenProvider = context.HttpContext.RequestServices.GetRequiredService<Beep.Foundation.IdentityServer.Shared.Authentication.TokenProvider>();
                tokenProvider.SetUserToken(userId, accessToken);
                logger.LogInformation("OIDC: ✓ Stored access token for user {UserId} in TokenProvider (length: {Length})", 
                    userId, accessToken.Length);
            }
            else
            {
                logger.LogWarning("OIDC: ✗ Could not capture access token. UserId: {UserId}, HasToken: {HasToken}", 
                    userId, !string.IsNullOrEmpty(accessToken));
            }
            
            return Task.CompletedTask;
        },
        
        OnRemoteFailure = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("OIDC");
            logger.LogError(context.Failure, "OIDC remote failure: {Message}", context.Failure?.Message);
            
            context.Response.Redirect($"/?error={Uri.EscapeDataString(context.Failure?.Message ?? "Authentication failed")}");
            context.HandleResponse();
            return Task.CompletedTask;
        },
        
        OnAccessDenied = context =>
        {
            context.Response.Redirect("/access-denied");
            context.HandleResponse();
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ============================================
// IDENTITY SERVER HTTP CLIENT
// ============================================
// HttpClient for communicating with Identity Server (for branding registration)
builder.Services.AddHttpClient("IdentityServer", client =>
{
    // Use Aspire service discovery URL if available, otherwise use config
    var identityServerUrl = builder.Configuration["services:identityserver:https:0"] 
        ?? builder.Configuration["IdentityServer:BaseUrl"] 
        ?? "https://localhost:7062/";
    client.BaseAddress = new Uri(identityServerUrl);
    client.Timeout = TimeSpan.FromSeconds(30); // Increase timeout to 30 seconds
})
.ConfigurePrimaryHttpMessageHandler(() => 
{
    var handler = new HttpClientHandler();
    // In development, skip certificate validation for self-signed certs
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
    }
    return handler;
})
.AddHttpMessageHandler<Beep.Foundation.IdentityServer.Shared.Authentication.TokenHandler>();

// ============================================
// THEME PROVIDER
// ============================================
builder.Services.AddSingleton<IThemeProvider, ThemeProvider>();

// ============================================
// BRANDING REGISTRATION SERVICE
// ============================================
// Registers branding with Identity Server on startup so login/register pages show the correct theme
builder.Services.AddHostedService<Beep.OilandGas.Web.Services.BrandingRegistrationService>();

// ============================================
// APPLICATION SERVICES
// ============================================
// Register the Beep.OilandGas client app (auto-detect local/remote)
builder.Services.AddBeepOilandGasAppAuto(builder.Configuration);

        // All services are accessed via IBeepOilandGasApp (registered above).
        // Components should inject IBeepOilandGasApp and use its properties for calculations, data management, etc.
        
        // Legacy service clients kept for backward compatibility (deprecated)
        builder.Services.AddScoped<IDataManagementService, DataManagementService>();
        builder.Services.AddScoped<IConnectionService, ConnectionService>();
        builder.Services.AddScoped<IAccountingServiceClient, AccountingServiceClient>();
        builder.Services.AddScoped<ICalculationServiceClient, CalculationServiceClient>();
        builder.Services.AddScoped<IOperationsServiceClient, OperationsServiceClient>();
        builder.Services.AddScoped<IPumpServiceClient, PumpServiceClient>();
        builder.Services.AddScoped<IPropertiesServiceClient, PropertiesServiceClient>();

        // Progress Tracking Client - SignalR client for real-time progress updates
        builder.Services.AddScoped<IProgressTrackingClient, ProgressTrackingClient>();

        // LifeCycle Service - Client service for lifecycle management operations
        builder.Services.AddScoped<ILifeCycleService, LifeCycleService>();

        // Demo Database Service - Client service for demo database operations
        builder.Services.AddScoped<IDemoDatabaseService, DemoDatabaseService>();

// PPDM39 Data Management Services

// Common Column Handler
builder.Services.AddSingleton<ICommonColumnHandler, CommonColumnHandler>();

// Metadata Repository
builder.Services.AddSingleton<IPPDMMetadataRepository>(sp =>
{
    return PPDMMetadataRepository.FromGeneratedClass();
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

// Theme provider - loads theme from Theme/OilGasTheme.json
builder.Services.AddSingleton<IThemeProvider, Beep.OilandGas.Web.Theme.ThemeProvider>();

// Register ICurrentUser service to get user info from OIDC claims
builder.Services.AddScoped<Beep.Foundation.IdentityServer.Shared.Services.ICurrentUser, Beep.Foundation.IdentityServer.Shared.Services.CurrentUser>();

// Register Account Management service for profile, password, 2FA management (IdentityServer)
builder.Services.AddScoped<Beep.Foundation.IdentityServer.Shared.Services.IAccountManagementService, Beep.Foundation.IdentityServer.Shared.Services.AccountManagementService>();

// Controllers support
builder.Services.AddControllersWithViews();

// Razor Pages support
builder.Services.AddRazorPages();

// Add CORS for static files (so IdentityServer can fetch logos)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ============================================
// HTTP PIPELINE
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

// IMPORTANT: Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

// Capture access token for authenticated users during HTTP request
// This ensures the token is available for API calls during SignalR sessions
app.UseTokenCapture();

app.UseCors();
app.UseStaticFiles();
app.UseRouting();

// IMPORTANT: UseAntiforgery must be after UseRouting and before MapRazorComponents
app.UseAntiforgery();

app.MapRazorComponents<Beep.OilandGas.Web.App>()
    .AddInteractiveServerRenderMode();

// Map authentication endpoints (login/logout)
app.MapGroup("/authentication").MapLoginAndLogout();

app.Run();

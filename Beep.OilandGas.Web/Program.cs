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
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.DataManagement.Repositories;
using Beep.OilandGas.Web.Theme;
using Microsoft.AspNetCore.Routing;

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
// API CLIENT SERVICES
// ============================================
// ApiClient: Generic HTTP client for calling the API service
var apiServiceUrl = builder.Configuration["ApiService:BaseUrl"] ?? "https://localhost:7001";
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiServiceUrl);
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

// ============================================
// AUTHENTICATION CONFIGURATION
// ============================================
const string OIDC_SCHEME = "oidc";

// Add cascading authentication state for Blazor
builder.Services.AddCascadingAuthenticationState();

// Register the OIDC-compatible AuthenticationStateProvider
builder.Services.AddScoped<AuthenticationStateProvider, Beep.OilandGas.Web.Components.Account.OidcAuthenticationStateProvider>();

// Get IdentityServer URL for OIDC configuration
var identityServerUrl = builder.Configuration["IdentityServer:BaseUrl"] ?? "https://localhost:7062/";

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
    
    // Add client_id to the authorization request so IdentityServer can apply the correct branding
    options.Events = new OpenIdConnectEvents
    {
        OnRedirectToIdentityProvider = context =>
        {
            // Add client_id as a query parameter to the authorization request
            // This allows IdentityServer to detect which client is requesting authentication
            // and apply the appropriate branding (BeepOilGasTheme)
            context.ProtocolMessage.SetParameter("client_id", "beep_oilgas_web");
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
});

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

// ============================================
// API CLIENT SERVICE
// ============================================
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;
    
    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("GET {Endpoint}", endpoint);
            var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return System.Text.Json.JsonSerializer.Deserialize<T>(content, JsonOptions);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error on GET {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint, 
        TRequest data, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("POST {Endpoint}", endpoint);
            var json = System.Text.Json.JsonSerializer.Serialize(data, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            return System.Text.Json.JsonSerializer.Deserialize<TResponse>(responseContent, JsonOptions);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error on POST {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<bool> PostAsync<TRequest>(
        string endpoint, 
        TRequest data, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("POST {Endpoint}", endpoint);
            var json = System.Text.Json.JsonSerializer.Serialize(data, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error on POST {Endpoint}", endpoint);
            return false;
        }
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(
        string endpoint, 
        TRequest data, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("PUT {Endpoint}", endpoint);
            var json = System.Text.Json.JsonSerializer.Serialize(data, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync(endpoint, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            return System.Text.Json.JsonSerializer.Deserialize<TResponse>(responseContent, JsonOptions);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error on PUT {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("DELETE {Endpoint}", endpoint);
            var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error on DELETE {Endpoint}", endpoint);
            return false;
        }
    }
}

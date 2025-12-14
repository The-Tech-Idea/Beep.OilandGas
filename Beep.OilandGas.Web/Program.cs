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
// AUTHENTICATION CONFIGURATION (Optional - can be added later)
// ============================================
builder.Services.AddCascadingAuthenticationState();
// TODO: Add authentication if needed
// builder.Services.AddScoped<AuthenticationStateProvider, OidcAuthenticationStateProvider>();

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

// Controllers support
builder.Services.AddControllersWithViews();

// Razor Pages support
builder.Services.AddRazorPages();

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

// Authentication (if configured)
// app.UseAuthentication();
// app.UseAuthorization();

app.UseStaticFiles();
app.UseRouting();

// IMPORTANT: UseAntiforgery must be after UseRouting and before MapRazorComponents
app.UseAntiforgery();

app.MapRazorComponents<Beep.OilandGas.Web.App>()
    .AddInteractiveServerRenderMode();

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

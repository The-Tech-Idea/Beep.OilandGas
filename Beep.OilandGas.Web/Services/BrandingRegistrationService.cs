using Beep.OilandGas.Web.Theme;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace Beep.OilandGas.Web.Services;

/// <summary>
/// Background service that registers the app's branding with Identity Server on startup.
/// This ensures branding is available before any login redirects occur.
/// </summary>
public class BrandingRegistrationService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IThemeProvider _themeProvider;
    private readonly ILogger<BrandingRegistrationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServer _server;
    private string? _lastBrandingHash;

    public BrandingRegistrationService(
        IHttpClientFactory httpClientFactory,
        IThemeProvider themeProvider,
        ILogger<BrandingRegistrationService> logger,
        IConfiguration configuration,
        IServer server)
    {
        _httpClientFactory = httpClientFactory;
        _themeProvider = themeProvider;
        _logger = logger;
        _configuration = configuration;
        _server = server;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait a bit for the app to fully start and Identity Server to be ready
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        
        // Initial registration
        await RegisterBrandingAsync(stoppingToken);
        
        // Periodically check for branding changes and re-register if needed
        // Also acts as a keep-alive to refresh the cache on Identity Server
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(10));
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
                await RegisterBrandingIfChangedAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during periodic branding refresh");
            }
        }
    }

    private async Task RegisterBrandingAsync(CancellationToken cancellationToken)
    {
        var clientId = _configuration["Authentication:ClientId"] ?? "beep_oilgas_web";
        
        try
        {
            var branding = _themeProvider.GetBranding();
            // Get the current server URL dynamically
            var webAppBaseUrl = GetWebAppBaseUrl();
            var brandingRegistration = CreateBrandingRegistration(clientId, branding, webAppBaseUrl);
            
            var httpClient = _httpClientFactory.CreateClient("IdentityServer");
            var json = System.Text.Json.JsonSerializer.Serialize(brandingRegistration);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            // Retry logic for startup when Identity Server might not be ready yet
            var maxRetries = 5;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    // Use a longer timeout for this specific request (30 seconds)
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    cts.CancelAfter(TimeSpan.FromSeconds(30));
                    
                    var response = await httpClient.PostAsync("/api/branding/register-client", content, cts.Token);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        _lastBrandingHash = ComputeBrandingHash(branding, webAppBaseUrl);
                        _logger.LogInformation("✓ Registered branding with Identity Server for client '{ClientId}': {AppName}", 
                            clientId, branding.AppName);
                        return;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync(cts.Token);
                        _logger.LogWarning("Failed to register branding (attempt {Attempt}/{MaxRetries}): {StatusCode} - {Error}", 
                            i + 1, maxRetries, response.StatusCode, errorContent);
                    }
                }
                catch (TaskCanceledException ex) when (i < maxRetries - 1)
                {
                    // Handle timeout or cancellation
                    if (ex.InnerException is TimeoutException)
                    {
                        _logger.LogWarning("Request timeout (attempt {Attempt}/{MaxRetries}): {Message}", 
                            i + 1, maxRetries, ex.Message);
                    }
                    else
                    {
                        _logger.LogWarning("Request canceled (attempt {Attempt}/{MaxRetries}): {Message}", 
                            i + 1, maxRetries, ex.Message);
                    }
                }
                catch (HttpRequestException ex) when (i < maxRetries - 1)
                {
                    _logger.LogWarning("Identity Server not ready yet (attempt {Attempt}/{MaxRetries}): {Message}", 
                        i + 1, maxRetries, ex.Message);
                }
                catch (Exception ex) when (i < maxRetries - 1)
                {
                    _logger.LogWarning("Unexpected error (attempt {Attempt}/{MaxRetries}): {Message}", 
                        i + 1, maxRetries, ex.Message);
                }
                
                // Wait before retry, with exponential backoff (skip delay on last attempt)
                if (i < maxRetries - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)), cancellationToken);
                }
            }
            
            _logger.LogError("Failed to register branding with Identity Server after {MaxRetries} attempts", maxRetries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering branding with Identity Server for client '{ClientId}'", clientId);
        }
    }

    private string GetWebAppBaseUrl()
    {
        // Get the server address from IServer (works in BackgroundService)
        var serverAddressesFeature = _server.Features.Get<IServerAddressesFeature>();
        if (serverAddressesFeature != null && serverAddressesFeature.Addresses.Any())
        {
            // Prefer HTTPS, then HTTP
            var httpsAddress = serverAddressesFeature.Addresses.FirstOrDefault(a => a.StartsWith("https://"));
            if (httpsAddress != null)
            {
                return httpsAddress.TrimEnd('/');
            }
            var httpAddress = serverAddressesFeature.Addresses.FirstOrDefault(a => a.StartsWith("http://"));
            if (httpAddress != null)
            {
                return httpAddress.TrimEnd('/');
            }
            // Use first available address
            return serverAddressesFeature.Addresses.First().TrimEnd('/');
        }
        
        // Fallback to appsettings or default
        return _configuration["WebApp:BaseUrl"] ?? "https://localhost:7066";
    }

    private async Task RegisterBrandingIfChangedAsync(CancellationToken cancellationToken)
    {
        var branding = _themeProvider.GetBranding();
        var webAppBaseUrl = GetWebAppBaseUrl();
        var currentHash = ComputeBrandingHash(branding, webAppBaseUrl);
        
        if (currentHash != _lastBrandingHash)
        {
            _logger.LogInformation("Branding changed, re-registering with Identity Server");
            await RegisterBrandingAsync(cancellationToken);
        }
        else
        {
            // Even if not changed, refresh the cache on Identity Server (it expires after 30 mins)
            await RegisterBrandingAsync(cancellationToken);
        }
    }

    private static object CreateBrandingRegistration(string clientId, BrandingConfig branding, string webAppBaseUrl)
    {
        // Create absolute URL for logo so IdentityServer can fetch it
        // If LogoUrl is already absolute, use it; otherwise build from base URL
        string logoUrl;
        if (!string.IsNullOrEmpty(branding.LogoUrl))
        {
            if (Uri.IsWellFormedUriString(branding.LogoUrl, UriKind.Absolute))
            {
                logoUrl = branding.LogoUrl;
            }
            else
            {
                // Relative path - make it absolute
                logoUrl = $"{webAppBaseUrl.TrimEnd('/')}{branding.LogoUrl.TrimStart('/')}";
            }
        }
        else
        {
            // Fallback to default logo path
            logoUrl = $"{webAppBaseUrl.TrimEnd('/')}/imgs/BeepOGDM.png";
        }
        
        return new
        {
            ClientId = clientId,
            Branding = new
            {
                AppName = branding.AppName ?? "Beep Oil & Gas",
                Tagline = branding.Tagline ?? "Professional Oil & Gas Management Platform",
                LogoUrl = logoUrl,
                PrimaryColor = branding.PrimaryColor ?? "#7B2CBF",
                SecondaryColor = branding.SecondaryColor ?? "#FF6B35",
                BackgroundColor = branding.BackgroundColorDark ?? branding.BackgroundDarkColor ?? branding.BackgroundColor ?? "#0d1117",
                SurfaceColor = branding.SurfaceColor ?? "#161b22",
                TextPrimary = branding.TextPrimaryDark ?? branding.TextPrimary ?? "#e6edf3",
                TextSecondary = branding.TextSecondaryDark ?? branding.TextSecondary ?? "#8b949e",
                BorderColor = branding.BorderColorDark ?? branding.BorderColor ?? "#30363d",
                SuccessColor = branding.SuccessColor ?? "#00ff88",
                WarningColor = branding.WarningColor ?? "#ffd866",
                ErrorColor = branding.ErrorColor ?? "#ff6b9d",
                InfoColor = branding.InfoColor ?? "#00d4ff",
                FontFamily = branding.FontFamily ?? "'Roboto', 'Inter', '-apple-system', 'BlinkMacSystemFont', sans-serif",
                HeadingFontFamily = branding.HeadingFontFamily ?? "'Roboto', 'Inter', sans-serif",
                BorderRadius = branding.BorderRadiusLg ?? branding.BorderRadius ?? "8px",
                // Don't send LoginWelcomeMessage to avoid title duplication - IdentityServer uses AppName
                // Don't include AppName in subtitle since it's already in the header
                LoginSubtitle = "Sign in to access your dashboard and manage your operations",
                ShowSocialLogins = true,
                ShowRememberMe = true
            }
        };
    }

    private static string ComputeBrandingHash(BrandingConfig branding, string webAppBaseUrl)
    {
        var logoUrl = $"{webAppBaseUrl.TrimEnd('/')}/imgs/BeepOGDM.png";
        var key = $"{branding.AppName}|{branding.PrimaryColor}|{branding.SecondaryColor}|{branding.BackgroundColor}|{logoUrl}";
        return Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(key)));
    }
}


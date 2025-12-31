# Beep.OilandGas.Web Authentication Documentation

## Overview

The Blazor web application uses OpenID Connect (OIDC) for authentication with an Identity Server. The application supports cookie-based authentication for Blazor Server and OIDC for external authentication flows.

## Authentication Architecture

### Authentication Flow

```
User → Login Page → Identity Server → OIDC Authentication → 
Callback → Cookie Authentication → Authenticated User
```

### Components

1. **Identity Server**: External authentication server
2. **OIDC Authentication**: OpenID Connect authentication scheme
3. **Cookie Authentication**: Default authentication scheme for Blazor
4. **AuthenticationStateProvider**: Blazor authentication state management

## Configuration

### Program.cs Configuration

```csharp
// OIDC Configuration
const string OIDC_SCHEME = "oidc";
var identityServerUrl = builder.Configuration["IdentityServer:BaseUrl"] ?? "https://localhost:7062/";

// Authentication Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
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
    options.LoginPath = "/authentication/login";
    options.LogoutPath = "/authentication/logout";
    options.AccessDeniedPath = "/access-denied";
})
.AddOpenIdConnect(OIDC_SCHEME, options =>
{
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.SignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.Authority = identityServerUrl;
    options.ClientId = "beep_oilgas_web";
    options.ClientSecret = "web_secret";
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.UsePkce = true;
    
    // Scopes
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Scope.Add("roles");
    options.Scope.Add("beep-api");
    options.Scope.Add("offline_access");
    
    // Claim mapping
    options.MapInboundClaims = false;
    options.TokenValidationParameters.NameClaimType = "name";
    options.TokenValidationParameters.RoleClaimType = "role";
    options.GetClaimsFromUserInfoEndpoint = true;
    options.SaveTokens = true;
    
    // Callback paths
    options.CallbackPath = "/signin-oidc";
    options.SignedOutCallbackPath = "/signout-callback-oidc";
    options.RemoteSignOutPath = "/signout-oidc";
});
```

### AuthenticationStateProvider

Custom `AuthenticationStateProvider` for Blazor:

```csharp
public class OidcAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public OidcAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.User?.Identity?.IsAuthenticated == true)
        {
            return new AuthenticationState(context.User);
        }
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }
}
```

**Registration**:
```csharp
builder.Services.AddScoped<AuthenticationStateProvider, OidcAuthenticationStateProvider>();
```

## Authentication Pages

### Login Page

**Route**: `/login`

Redirects to Identity Server for authentication:

```razor
@page "/login"
@inject NavigationManager NavigationManager

@code {
    protected override void OnInitialized()
    {
        NavigationManager.NavigateTo("/authentication/login", forceLoad: true);
    }
}
```

### Logout

**Route**: `/authentication/logout`

Redirects to Identity Server for logout:

```razor
NavigationManager.NavigateTo("/authentication/logout?returnUrl=/", forceLoad: true);
```

## Authorization

### Role-Based Authorization

#### Page-Level Authorization

```razor
@page "/admin/users"
@using Microsoft.AspNetCore.Authorization
@attribute [Authorize(Roles = "Administrator")]
```

#### Component-Level Authorization

```razor
@inject IAuthorizationService AuthorizationService

@code {
    private bool _canEdit = false;
    
    protected override async Task OnInitializedAsync()
    {
        var authResult = await AuthorizationService.AuthorizeAsync(
            user, null, "EditPolicy");
        _canEdit = authResult.Succeeded;
    }
}
```

### Policy-Based Authorization

Define policies in `Program.cs`:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EditPolicy", policy =>
        policy.RequireRole("Manager", "Administrator"));
    
    options.AddPolicy("ViewOnlyPolicy", policy =>
        policy.RequireRole("Viewer", "Manager", "Administrator"));
});
```

Use in components:

```razor
@inject IAuthorizationService AuthorizationService

@if (await AuthorizationService.AuthorizeAsync(user, "EditPolicy"))
{
    <MudButton OnClick="@Edit">Edit</MudButton>
}
```

## Accessing User Information

### Get Current User

```razor
@inject AuthenticationStateProvider AuthStateProvider

@code {
    private ClaimsPrincipal? _user;
    
    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        _user = authState.User;
    }
}
```

### Get User Claims

```razor
@code {
    private string GetUserName()
    {
        return _user?.Identity?.Name ?? "Unknown";
    }
    
    private bool IsInRole(string role)
    {
        return _user?.IsInRole(role) ?? false;
    }
    
    private string? GetClaim(string claimType)
    {
        return _user?.FindFirst(claimType)?.Value;
    }
}
```

## Role Definitions

### Standard Roles

- **Administrator**: Full system access
- **Manager**: Management-level access
- **PetroleumEngineer**: Petroleum engineering operations
- **ReservoirEngineer**: Reservoir engineering operations
- **Accountant**: Accounting and financial operations
- **Viewer**: Read-only access

### Role-Based Layouts

Different layouts for different roles:

- `AccountantLayout.razor` - For accountants
- `ManagerLayout.razor` - For managers
- `PetroleumEngineerLayout.razor` - For petroleum engineers
- `ReservoirEngineerLayout.razor` - For reservoir engineers

## Protected Routes

### Require Authentication

```razor
@page "/ppdm39/field/dashboard"
@using Microsoft.AspNetCore.Authorization
@attribute [Authorize]
```

### Require Specific Role

```razor
@page "/admin/users"
@attribute [Authorize(Roles = "Administrator")]
```

### Multiple Roles

```razor
@attribute [Authorize(Roles = "Manager,Administrator")]
```

## API Authentication

### Token Management

The API client automatically includes authentication cookies in requests. The API service validates these cookies and extracts user information.

### User Context in API Calls

```csharp
// User ID is passed as query parameter for audit trails
var url = $"/api/ppdm39/data/{tableName}/insert?userId={Uri.EscapeDataString(userId)}";
```

## Logout

### Logout Implementation

```razor
@inject NavigationManager NavigationManager

private async Task Logout()
{
    NavigationManager.NavigateTo("/authentication/logout?returnUrl=/", forceLoad: true);
}
```

### Logout Flow

1. User clicks logout
2. Navigate to `/authentication/logout`
3. Clear local authentication cookie
4. Redirect to Identity Server logout
5. Identity Server clears session
6. Redirect back to application

## Security Considerations

### Cookie Security

- **HttpOnly**: Prevents JavaScript access
- **Secure**: Only sent over HTTPS
- **SameSite**: Lax mode for cross-site requests
- **Expiration**: 14 days with sliding expiration

### Token Security

- Tokens stored in secure cookies
- PKCE (Proof Key for Code Exchange) enabled
- Token validation on every request

### HTTPS

- All communication over HTTPS
- Certificate validation in production
- Self-signed certificates allowed in development

## Best Practices

### 1. Always Check Authentication

```razor
if (!_user?.Identity?.IsAuthenticated ?? false)
{
    NavigationManager.NavigateTo("/login");
    return;
}
```

### 2. Use Authorization Policies

Define reusable authorization policies instead of hardcoding roles.

### 3. Protect API Endpoints

All API endpoints should validate authentication and authorization.

### 4. Audit Trail

Log user actions for audit purposes:

```csharp
var userId = _user?.FindFirst("sub")?.Value ?? "unknown";
await DataManagementService.InsertEntityAsync(tableName, data, userId);
```

## Related Documentation

- [Architecture](beep-oilgas-web-architecture.md)
- [Pages](beep-oilgas-web-pages.md)
- [Connection Management](beep-oilgas-web-connection-management.md)


# Phase 10 — Security Hardening
## OWASP API Top 10 Checklist, JWT Rotation, Field Isolation Pen-Test Plan

---

## OWASP API Top 10 (2023) — Gap Analysis and Mitigations

| # | OWASP Risk | Our Exposure | Mitigation |
|---|---|---|---|
| API1 | Broken Object Level Authorization | Each endpoint must verify `FIELD_ID` matches user's assigned field | Guard in `FieldOrchestrator`; add BOLA unit tests |
| API2 | Broken Authentication | JWT expiry; no refresh rotation | Implement sliding refresh token with 15 min access + 7 day refresh |
| API3 | Broken Object Property Level Auth | PATCH/PUT may expose admin-only columns | Add `[BindRequired]` DTO validation; strip disallowed properties |
| API4 | Unrestricted Resource Consumption | No rate limits currently | Add `AspNetCoreRateLimit` middleware: 1000 req/min per user |
| API5 | Broken Function Level Authorization | Admin endpoints must check role strictly | Audit all `[Authorize]` attributes; ensure admin routes use `Policy="AdminOnly"` |
| API6 | Unrestricted Access to Sensitive Business Flows | Incident escalation may be triggered repeatedly | Idempotency check: guard same-state transitions |
| API7 | SSRF | Integration adapters call external URLs from config | Validate all integration URLs against allow-list at startup |
| API8 | Security Misconfiguration | Development settings in production | Environment-specific config; no `appsettings.Development.json` in container |
| API9 | Improper Inventory Management | Legacy or unused endpoints | Run endpoint inventory; disable or protect any unauthenticated routes |
| API10 | Unsafe Consumption of APIs | Trust WITSML/OSDU responses without validation | Validate JSON schema; max response size limit (10 MB per adapter) |

---

## Security Headers Middleware

```csharp
// SecurityHeadersMiddleware.cs
public class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        ctx.Response.Headers["X-Content-Type-Options"]    = "nosniff";
        ctx.Response.Headers["X-Frame-Options"]           = "DENY";
        ctx.Response.Headers["X-XSS-Protection"]         = "1; mode=block";
        ctx.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        ctx.Response.Headers["Referrer-Policy"]           = "no-referrer";
        ctx.Response.Headers["Permissions-Policy"]        = "camera=(), microphone=(), geolocation=()";
        await next(ctx);
    }
}
// Registered in Program.cs before app.UseAuthentication()
app.UseMiddleware<SecurityHeadersMiddleware>();
```

---

## JWT Rotation Strategy

| Token Type | Lifetime | Storage |
|---|---|---|
| Access token | 15 minutes | Memory / HTTP-only cookie |
| Refresh token | 7 days | `REFRESH_TOKENS` DB table (hashed SHA-256) |
| Refresh rotation | Each use issues a new refresh token; old one invalidated | |
| Revocation | `DELETE FROM REFRESH_TOKENS WHERE USER_ID=?` | Available on logout / password change |

Refresh token is SHA-256 hashed before storage — plaintext never stored.

---

## Rate Limiting Middleware

```csharp
// Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.AddSlidingWindowLimiter("api-limit", o =>
    {
        o.Window           = TimeSpan.FromMinutes(1);
        o.SegmentsPerWindow = 6;
        o.PermitLimit      = 1000;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit       = 50;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
app.UseRateLimiter();
```

---

## Field Isolation Pen-Test Plan

Test that user A cannot access field B data even with a valid JWT:

| Test # | Attack | Expected Result |
|---|---|---|
| PT-01 | User A calls `GET /api/field/current/incidents` with User B's `fieldId` in query | `403 Forbidden` |
| PT-02 | User A manually sets `FIELD_ID` in POST body to another field | Record rejected by `FieldOrchestrator` guard |
| PT-03 | Admin endpoint called with Operator-level token | `403 Forbidden` |
| PT-04 | Expired JWT (5 min old; no refresh) | `401 Unauthorized` |
| PT-05 | Cross-tenant: WITSML sync writes well to another field's partition | FIELD_ID validation in `WitsmlAdapterService.SyncWellAsync` |
| PT-06 | Mass assignment: POST with extra properties not in DTO | Extra properties silently ignored; no DB write |
| PT-07 | SQL injection via `AppFilter.FilterValue` | `PPDMGenericRepository` uses parameterized queries; injection blocked |
| PT-08 | SSRF: Swap WITSML URL in config to internal AWS metadata endpoint | URL allow-list in adapter factory blocks non-whitelisted hosts |

---

## SSRF Integration URL Allow-List

```csharp
// IntegrationAdapterFactory.cs — startup validation
private static readonly HashSet<string> _allowedHosts = new(StringComparer.OrdinalIgnoreCase)
{
    config["Integrations:WITSML:ServerUrl"],
    config["Integrations:PRODML:EndpointUrl"],
    config["Integrations:SAP:BaseUrl"],
    config["Integrations:OSDU:BaseUrl"],
    config["Integrations:SharePoint:SiteUrl"]
};

public static void ValidateAdapterUrls(IConfiguration config)
{
    foreach (var url in GetAllIntegrationUrls(config))
    {
        var uri = new Uri(url);
        if (uri.IsLoopback || IsPrivateIp(uri.Host))
            throw new SecurityException($"Integration URL {url} points to a private address — SSRF risk");
    }
}
```

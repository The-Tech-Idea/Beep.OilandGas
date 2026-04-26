# Beep.OilandGas — Deployment & Environment Configuration Guide

> **Last Updated**: 2026-04-25  
> **Scope**: Production deployment, environment configuration, monitoring setup  
> **Applies To**: `Beep.OilandGas.Web`, `Beep.OilandGas.ApiService`, and all domain projects

---

## Architecture Overview

```
┌─────────────────┐     HTTPS/JSON     ┌──────────────────┐     IDMEEditor     ┌──────────────────┐
│  Blazor Server  │ ──────────────────▶ │  ASP.NET Core    │ ─────────────────▶ │  PPDM39 Database │
│  (Web Layer)    │ ◀────────────────── │  API Service     │ ◀───────────────── │  (SQL Server)    │
│  Port: 5001     │   JWT Bearer Auth   │  Port: 7001      │   Beep Framework   │                  │
└─────────────────┘                    └──────────────────┘                    └──────────────────┘
        │                                       │
        │                                       │
        ▼                                       ▼
┌─────────────────┐                    ┌──────────────────┐
│  IdentityServer │                    │  Azure Monitor   │
│  Port: 7062     │                    │  / Application   │
│  (OIDC/OAuth2)  │                    │  Insights        │
└─────────────────┘                    └──────────────────┘
```

---

## Environment Variables

### Web Application (`Beep.OilandGas.Web`)

| Variable | Required | Description | Default |
|----------|----------|-------------|---------|
| `ApiService:BaseUrl` | Yes | Base URL of the API service | `https://localhost:7001` |
| `IdentityServer:Authority` | Yes | OIDC authority URL | `https://localhost:7062/` |
| `IdentityServer:Audience` | Yes | JWT audience identifier | `beep-api` |
| `Authentication:Schemes:OpenIdConnect:ClientSecret` | Yes | OIDC client secret (from user-secrets or vault) | — |
| `ASPNETCORE_ENVIRONMENT` | No | Environment name (`Development`, `Staging`, `Production`) | `Development` |
| `ASPNETCORE_URLS` | No | Kestrel listen URLs | `https://localhost:5001` |

### API Service (`Beep.OilandGas.ApiService`)

| Variable | Required | Description | Default |
|----------|----------|-------------|---------|
| `IdentityServer:Authority` | Yes | OIDC authority URL for token validation | `https://localhost:7062/` |
| `IdentityServer:Audience` | Yes | JWT audience identifier | `beep-api` |
| `ConnectionStrings:PPDM39` | Yes | PPDM39 database connection string | — |
| `Beep:ConfigPath` | No | Path to Beep configuration directory | `./Beep` |
| `ASPNETCORE_ENVIRONMENT` | No | Environment name | `Development` |
| `ASPNETCORE_URLS` | No | Kestrel listen URLs | `https://localhost:7001` |

### Identity Server (`Beep.Foundation.IdentityServer`)

| Variable | Required | Description | Default |
|----------|----------|-------------|---------|
| `ConnectionStrings:Identity` | Yes | Identity database connection string | — |
| `ASPNETCORE_ENVIRONMENT` | No | Environment name | `Development` |
| `ASPNETCORE_URLS` | No | Kestrel listen URLs | `https://localhost:7062` |

---

## Deployment Steps

### 1. Prerequisites

- .NET 10 SDK installed on target server
- SQL Server 2022+ (or PostgreSQL/SQLite for development)
- Identity Server instance running and accessible
- PPDM39 database schema created and seeded

### 2. Build

```bash
# Build all projects
dotnet build Beep.OilandGas.sln -c Release

# Publish Web
dotnet publish Beep.OilandGas.Web/Beep.OilandGas.Web.csproj -c Release -o ./publish/web

# Publish API
dotnet publish Beep.OilandGas.ApiService/Beep.OilandGas.ApiService.csproj -c Release -o ./publish/api
```

### 3. Configure

Create `appsettings.Production.json` for each service:

**Web (`appsettings.Production.json`):**
```json
{
  "ApiService": {
    "BaseUrl": "https://api.beep-oilgas.example.com"
  },
  "IdentityServer": {
    "Authority": "https://identity.beep-oilgas.example.com",
    "Audience": "beep-api"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Error"
    }
  }
}
```

**API (`appsettings.Production.json`):**
```json
{
  "IdentityServer": {
    "Authority": "https://identity.beep-oilgas.example.com",
    "Audience": "beep-api"
  },
  "ConnectionStrings": {
    "PPDM39": "Server=sql.beep-oilgas.example.com;Database=PPDM39;User Id=beep;Password=***;TrustServerCertificate=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

### 4. Deploy

```bash
# Copy published files to target server
scp -r ./publish/web/* user@web-server:/var/www/beep-oilgas/web/
scp -r ./publish/api/* user@api-server:/var/www/beep-oilgas/api/

# Set up systemd services (Linux)
# See deployment/systemd/ for service unit files
```

### 5. Verify

```bash
# Check API health
curl -k https://api.beep-oilgas.example.com/health

# Check Web availability
curl -k https://web.beep-oilgas.example.com/

# Check logs
tail -f /var/log/beep-oilgas/api/*.txt
tail -f /var/log/beep-oilgas/web/*.txt
```

---

## Monitoring Setup

### Application Insights

Add to both Web and API `appsettings.json`:

```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=xxx;IngestionEndpoint=https://xxx.applicationinsights.azure.com/"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Serilog File Logging

Both services use Serilog with rolling daily files:

- **API logs**: `logs/beep-oilgas-api-YYYYMMDD.txt`
- **Web logs**: `logs/beep-oilgas-web-YYYYMMDD.txt`

Log levels:
- `Information`: Normal operation
- `Warning`: Recoverable issues
- `Error`: Failures requiring attention

### Key Metrics to Monitor

| Metric | Source | Alert Threshold |
|--------|--------|-----------------|
| API response time (p95) | Application Insights | > 2000ms |
| Web circuit disconnects | Application Insights | > 10/min |
| Database connection pool | SQL Server DMVs | > 80% utilized |
| 429 (rate limit) errors | API logs | > 5/min |
| Unhandled exceptions | Application Insights | > 0 |

---

## Security Checklist

- [ ] OIDC client secrets stored in Azure Key Vault or user-secrets (never in source)
- [ ] JWT tokens validated with `SaveToken=false` and explicit lifetime checks
- [ ] HTTPS enforced on all endpoints (`RequireHttpsMetadata=true` in production)
- [ ] CORS configured for specific origins only (not `AllowAnyOrigin` in production)
- [ ] Database connection strings encrypted at rest
- [ ] Serilog logs exclude sensitive data (no token/claim payload logging)
- [ ] `AddServerHeader=false` configured on Kestrel
- [ ] First-run setup gating active on `MainLayout`
- [ ] Role-based access control enforced at API boundary (`[Authorize]` on all controllers)
- [ ] Field-scoped access checks active (`[RequireCurrentFieldAccess]` on field-scoped endpoints)

---

## Troubleshooting

### Common Issues

| Symptom | Likely Cause | Resolution |
|---------|-------------|------------|
| Web shows "Connection refused" on API calls | API service not running or wrong `ApiService:BaseUrl` | Verify API is running and URL matches |
| 401 Unauthorized on all API calls | Identity Server unreachable or token expired | Check `IdentityServer:Authority` and token validity |
| 429 Request Rate Too Large | Cosmos DB RU limit exceeded (if using Cosmos) | Increase RUs or optimize queries |
| Blazor circuit disconnected | SignalR connection lost or server restart | Check network stability; circuit auto-reconnects |
| Drawing sample page errors | `Beep.OilandGas.Drawing` type drift | Known deferred issue; page can be hidden from nav |

### Log Locations

| Service | Log Path |
|---------|----------|
| API | `logs/beep-oilgas-api-*.txt` |
| Web | `logs/beep-oilgas-web-*.txt` |
| Identity Server | Identity Server's configured log path |

### Emergency Access

If locked out due to identity issues:
1. Check Identity Server logs for authentication failures
2. Verify OIDC client configuration matches between Web and Identity Server
3. Check token expiration and clock skew settings
4. Review `RequireCurrentFieldAccessAttribute` logs for field-scoped denials

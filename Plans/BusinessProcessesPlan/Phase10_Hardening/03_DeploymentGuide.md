# Phase 10 — Deployment Guide
## Docker Compose, Kubernetes Helm Chart, Environment Variables, DB Migration

---

## Docker Compose (Development and Test)

`docker-compose.yml` at solution root:

```yaml
version: "3.9"
services:
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "${SA_PASSWORD}"
      ACCEPT_EULA: "Y"
    ports: ["1433:1433"]
    volumes: ["ppdm-data:/var/opt/mssql"]

  identity:
    build: ./Beep.OilandGas.UserManagement.AspNetCore
    ports: ["7062:7062"]
    environment:
      ConnectionStrings__Identity: "Server=db;Database=BeepIdentity;User=sa;Password=${SA_PASSWORD}"

  api:
    build: ./Beep.OilandGas.ApiService
    ports: ["5000:5000"]
    environment:
      ConnectionStrings__PPDM: "Server=db;Database=PPDM39;User=sa;Password=${SA_PASSWORD}"
      JwtSettings__Issuer: "https://identity:7062"
      JwtSettings__Secret: "${JWT_SECRET}"
      Integrations__WITSML__Username: "${WITSML_USER}"
      Integrations__WITSML__Password: "${WITSML_PASS}"
    depends_on: [db, identity]

  web:
    build: ./Beep.OilandGas.Web
    ports: ["5001:5001"]
    environment:
      ApiBaseUrl: "http://api:5000"
      IdentityUrl: "https://identity:7062"
    depends_on: [api]

volumes:
  ppdm-data:
```

All secrets injected via `.env` file (never committed to git; listed in `.gitignore`).

---

## Environment Variable Matrix

| Variable | Used By | Example |
|---|---|---|
| `SA_PASSWORD` | SQL Server; connection strings | `StrongPass123!` |
| `JWT_SECRET` | API JWT signing key (min 32 chars) | `<generated>` |
| `WITSML_USER` / `WITSML_PASS` | WitsmlAdapterService | `witsml_svc` / `<pass>` |
| `SCADA_USER` / `SCADA_PASS` | OpcUaAdapterService | `opc_svc` / `<pass>` |
| `SAP_API_KEY` | SapErpAdapterService | `<key>` |
| `OSDU_TENANT_ID` / `OSDU_CLIENT_ID` / `OSDU_CLIENT_SECRET` | OsduAdapter | Azure AD app registration |
| `SHAREPOINT_TENANT_ID` / `SHAREPOINT_CLIENT_ID` / `SHAREPOINT_CLIENT_SECRET` | DocumentManagementAdapter | Azure AD app registration |

---

## Production Kubernetes Helm Chart

```
helm/beep-oilgas/
├── Chart.yaml
├── values.yaml
├── templates/
│   ├── api-deployment.yaml
│   ├── api-service.yaml
│   ├── web-deployment.yaml
│   ├── web-service.yaml
│   ├── identity-deployment.yaml
│   ├── identity-service.yaml
│   ├── ingress.yaml
│   └── secrets.yaml    (references external secrets store)
```

Secrets loaded from Azure Key Vault via `azure-keyvault-secrets-provider` CSI driver — not stored in Helm values.

---

## Database Migration Strategy

PPDM39 schema migrations managed via SQL scripts under `Scripts/` rather than EF Migrations (PPDM schema is pre-defined):

```
Scripts/
├── SqlServer/
│   ├── TAB/    ← CREATE TABLE scripts (PPDM standard + custom ext cols)
│   ├── PK/     ← PRIMARY KEY constraints
│   └── FK/     ← FOREIGN KEY constraints
└── Common/
    └── SeedData/  ← R_* reference table inserts
```

`PPDMDatabaseCreatorService.CreateDatabaseAsync` runs all scripts in order: TAB → PK → FK → SeedData.

---

## CI/CD Pipeline (GitHub Actions)

`.github/workflows/ci.yml`:

```yaml
on: [push, pull_request]
jobs:
  build-and-test:
    runs-on: ubuntu-latest
    services:
      mssql:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          SA_PASSWORD: CI_Test_1234!
          ACCEPT_EULA: Y
        ports: [1433:1433]
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '9.0.x' }
      - run: dotnet restore Beep.OilandGas.sln
      - run: dotnet build Beep.OilandGas.sln --no-restore
      - run: dotnet test Beep.OilandGas.sln --no-build --logger trx
      - uses: actions/upload-artifact@v4
        with: { name: test-results, path: "**/*.trx" }
```

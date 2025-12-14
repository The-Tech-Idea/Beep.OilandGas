# Beep.OilandGas.API

RESTful API server for managing all 7 stages of the oil and gas field lifecycle.

## Features

- ✅ **Complete API Coverage** - All 7 lifecycle stages
- ✅ **51 REST Endpoints** - Full CRUD operations for all entities
- ✅ **Swagger Documentation** - Interactive API documentation
- ✅ **Validation** - FluentValidation for request validation
- ✅ **Error Handling** - Global exception handling middleware
- ✅ **Logging** - Structured logging with Serilog
- ✅ **Health Checks** - Database and service health monitoring
- ✅ **CORS Support** - Cross-origin resource sharing enabled

## API Endpoints

### Stage 1: Prospect Identification
- `GET /api/v1/prospects` - List all prospects
- `GET /api/v1/prospects/{id}` - Get prospect details
- `POST /api/v1/prospects` - Create new prospect
- `PUT /api/v1/prospects/{id}` - Update prospect
- `DELETE /api/v1/prospects/{id}` - Delete prospect
- `POST /api/v1/prospects/{id}/evaluate` - Evaluate prospect
- `GET /api/v1/prospects/{id}/seismic-data` - Get seismic surveys
- `POST /api/v1/prospects/{id}/seismic-analysis` - Analyze seismic data
- `GET /api/v1/prospects/{id}/risk-assessment` - Get risk assessment

### Stage 2: Lease Acquisition
- `GET /api/v1/leases` - List all leases
- `GET /api/v1/leases/{id}` - Get lease details
- `POST /api/v1/leases` - Create new lease
- `PUT /api/v1/leases/{id}` - Update lease
- `POST /api/v1/leases/{id}/renew` - Renew lease
- `GET /api/v1/leases/expiring` - Get expiring leases
- `GET /api/v1/leases/{id}/land-rights` - Get land rights
- `GET /api/v1/leases/{id}/mineral-rights` - Get mineral rights
- `GET /api/v1/leases/{id}/royalties` - Get royalties

### Stage 3: Development Planning
- `GET /api/v1/development-plans` - List all plans
- `GET /api/v1/development-plans/{id}` - Get plan details
- `POST /api/v1/development-plans` - Create new plan
- `PUT /api/v1/development-plans/{id}` - Update plan
- `POST /api/v1/development-plans/{id}/approve` - Approve plan
- `GET /api/v1/development-plans/{id}/well-plans` - Get well plans
- `GET /api/v1/development-plans/{id}/facility-plans` - Get facility plans
- `GET /api/v1/development-plans/{id}/permits` - Get permit applications

### Stage 4: Drilling & Construction
- `GET /api/v1/drilling-operations` - List all operations
- `GET /api/v1/drilling-operations/{id}` - Get operation details
- `POST /api/v1/drilling-operations` - Create new operation
- `PUT /api/v1/drilling-operations/{id}` - Update operation
- `GET /api/v1/drilling-operations/{id}/reports` - Get drilling reports
- `POST /api/v1/drilling-operations/{id}/reports` - Create drilling report

### Stage 5: Production Operations
- `GET /api/v1/production-operations` - List all operations
- `GET /api/v1/production-operations/{id}` - Get operation details
- `POST /api/v1/production-operations` - Create new operation
- `GET /api/v1/production-operations/reports` - Get production reports
- `GET /api/v1/production-operations/wells/{wellUWI}/operations` - Get well operations
- `GET /api/v1/production-operations/facilities/{facilityId}/operations` - Get facility operations

### Stage 6: Enhanced Recovery
- `GET /api/v1/enhanced-recovery` - List all operations
- `GET /api/v1/enhanced-recovery/{id}` - Get operation details
- `POST /api/v1/enhanced-recovery` - Create new operation
- `GET /api/v1/enhanced-recovery/injection` - Get injection operations
- `GET /api/v1/enhanced-recovery/water-flooding` - Get water flooding operations
- `GET /api/v1/enhanced-recovery/gas-injection` - Get gas injection operations

### Stage 7: Decommissioning
- `GET /api/v1/decommissioning/well-plugging` - List well plugging operations
- `GET /api/v1/decommissioning/well-plugging/{id}` - Get plugging details
- `POST /api/v1/decommissioning/well-plugging` - Create plugging operation
- `POST /api/v1/decommissioning/well-plugging/{id}/verify` - Verify plugging
- `GET /api/v1/decommissioning/facility-decommissioning` - Get facility decommissioning
- `GET /api/v1/decommissioning/site-restoration` - Get site restoration
- `GET /api/v1/decommissioning/abandonment` - Get abandonment operations

### Health & Monitoring
- `GET /health` - Health check endpoint

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code

### Running the API

```bash
cd Beep.OilandGas.API
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

### Configuration

The API uses `appsettings.json` for configuration. Key settings:
- Logging levels
- CORS policies
- Database connection strings (when implemented)

## Architecture

```
API Layer (Controllers)
    ↓
Business Layer (Services)
    ↓
Repository Layer (Data Access)
    ↓
PPDM39 Data Model
```

## Validation

Request validation is handled by FluentValidation. Validators are automatically registered and applied to all requests.

### Example Validators
- `CreateProspectDtoValidator`
- `CreateLeaseDtoValidator`
- `CreateDevelopmentPlanDtoValidator`
- `CreateDrillingOperationDtoValidator`
- `CreateProductionOperationDtoValidator`

## Error Handling

Global exception handling middleware catches all unhandled exceptions and returns standardized error responses:

```json
{
  "error": {
    "message": "Error message",
    "type": "ExceptionType",
    "statusCode": 500,
    "path": "/api/v1/prospects",
    "timestamp": "2024-01-01T00:00:00Z"
  }
}
```

## Logging

Structured logging is configured using Serilog:
- Console output
- File output (daily rolling logs in `logs/` directory)
- Log levels: Information, Warning, Error

## Health Checks

Health check endpoint available at `/health`:
- Database connectivity check
- Service availability check

## Authentication

**Note**: Authentication is handled per platform. This API does not include authentication middleware. Integrate with your platform's authentication system (e.g., Beep.Authentication).

## Next Steps

1. Implement concrete repository classes with actual database access
2. Add more validators for remaining DTOs
3. Add unit and integration tests
4. Configure production logging
5. Add API versioning if needed
6. Add rate limiting
7. Add caching strategies

## Documentation

- Swagger UI: Available at `/swagger` when running in Development mode
- API Documentation: See `FIELD_LIFECYCLE_IMPLEMENTATION_PLAN.md` for detailed API documentation


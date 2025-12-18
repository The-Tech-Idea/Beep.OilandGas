# Beep.OilandGas.ApiService

API service for Beep Oil and Gas PPDM39 data management.

## Setup

1. **Configure IDMEEditor Registration**: Update `Program.cs` to properly register `IDMEEditor` and its dependencies based on your DataManagementEngine setup.

2. **Connection String**: Add connection string to `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "PPDM39": "your-connection-string"
  }
}
```

## API Endpoints

### PPDM39 Controller (`/api/ppdm39`)
- `POST /api/ppdm39/wells/compare` - Compare multiple wells
- `POST /api/ppdm39/wells/compare-multi-source` - Compare wells from different sources
- `GET /api/ppdm39/wells/comparison-fields` - Get available comparison fields
- `POST /api/ppdm39/validate/{tableName}` - Validate an entity
- `GET /api/ppdm39/validate/{tableName}/rules` - Get validation rules
- `GET /api/ppdm39/quality/{tableName}/metrics` - Get quality metrics
- `GET /api/ppdm39/quality/{tableName}/dashboard` - Get quality dashboard
- `GET /api/ppdm39/quality/alerts` - Get quality alerts

### Well Controller (`/api/well`)
- `GET /api/well/uwi/{uwi}` - Get well by UWI
- `GET /api/well/uwi/{uwi}/status` - Get well status
- `POST /api/well` - Create a new well
- `PUT /api/well/uwi/{uwi}` - Update a well

## Dependencies

- Uses `IDMEEditor` and `UnitOfWork` from DataManagementEngine
- All data access goes through PPDM39 services
- Uses AppFilter for all queries (no raw SQL)




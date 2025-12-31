# Mapping Service

## Overview

The `PPDMMappingService` provides centralized conversion between DTOs and PPDM model classes. All conversions use strongly-typed classes - no Dictionary-based operations.

## Key Features

- **DTO to PPDM Model**: Converts DTO Request objects to PPDM model instances
- **PPDM Model to DTO**: Converts PPDM model instances to DTO Response objects
- **Type-Safe**: Uses strongly-typed classes throughout
- **Automatic Property Mapping**: Automatically maps matching properties
- **Property Name Conversion**: Converts property names (e.g., ProspectId -> PROSPECT_ID)

## Key Methods

### ConvertDTOToPPDMModel

Converts a DTO Request to a PPDM model class instance.

```csharp
public TPPDM ConvertDTOToPPDMModel<TPPDM, TDTO>(TDTO dto)
    where TPPDM : class, new()
    where TDTO : class
```

**Example:**
```csharp
var mappingService = new PPDMMappingService();

var prospectRequest = new ProspectRequest
{
    ProspectName = "Eagle Ford Prospect",
    RiskLevel = "Medium",
    EstimatedVolume = 1000000.0m
};

var prospect = mappingService.ConvertDTOToPPDMModel<PROSPECT, ProspectRequest>(prospectRequest);
// prospect.PROSPECT_NAME = "Eagle Ford Prospect"
// prospect.RISK_LEVEL = "Medium"
// prospect.ESTIMATED_VOLUME = 1000000.0m
```

### ConvertPPDMModelToDTO

Converts a PPDM model class instance to a DTO Response.

```csharp
public TDTO ConvertPPDMModelToDTO<TDTO, TPPDM>(TPPDM ppdmEntity)
    where TDTO : class, new()
    where TPPDM : class
```

**Example:**
```csharp
var prospect = await repository.GetByIdAsync("PROSPECT-001");
var prospectResponse = mappingService.ConvertPPDMModelToDTO<ProspectResponse, PROSPECT>(prospect);
```

## Property Name Conversion

The service automatically converts property names:
- `ProspectId` -> `PROSPECT_ID`
- `ProspectName` -> `PROSPECT_NAME`
- `RiskLevel` -> `RISK_LEVEL`

## Best Practices

### 1. Use Type-Safe Conversions

```csharp
// ✅ Good - Use strongly-typed conversions
var prospect = mappingService.ConvertDTOToPPDMModel<PROSPECT, ProspectRequest>(request);

// ❌ Bad - Dictionary-based conversions
var prospect = new Dictionary<string, object>();
prospect["PROSPECT_NAME"] = request.ProspectName;
```

## Related Documentation

- [Overview](beep-dataaccess-overview.md) - Framework overview
- [Usage Examples](beep-dataaccess-examples.md) - Practical examples


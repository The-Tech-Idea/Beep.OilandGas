# Multi-Jurisdiction Support

## Overview

The `Beep.OilandGas.PermitsAndApplications` library supports multiple jurisdictions with country and state/province as key identifiers. This allows the system to handle permits and applications from different regulatory authorities.

## Supported Jurisdictions

### United States - Texas
- **Regulatory Authority**: RRC (Railroad Commission of Texas)
- **Forms**: [RRC Oil & Gas Forms](https://www.rrc.texas.gov/oil-and-gas/oil-and-gas-forms/)
- **Applications**: [RRC Applications and Permits](https://www.rrc.texas.gov/oil-and-gas/applications-and-permits/)

### Canada - Alberta
- **Regulatory Authority**: AER (Alberta Energy Regulator)
- **Forms**: [AER Forms](https://www.aer.ca/applications-and-notices/application-processes/aer-forms)

### United States - Federal
- **Regulatory Authorities**: BLM, USACE, EPA
- **Coverage**: Federal-level permits across multiple states

## Data Model

### Key Fields

Every `PermitApplication` includes:
- `Country` - The country/jurisdiction (UnitedStates, Canada, etc.)
- `StateProvince` - The state or province (Texas, Alberta, etc.)
- `RegulatoryAuthority` - The specific regulatory authority (RRC, AER, etc.)

### Jurisdiction Inference

The system automatically infers jurisdiction from:
1. **Regulatory Authority** - Primary method
   - RRC → United States, Texas
   - AER → Canada, Alberta
   - BLM/USACE/EPA → United States, OtherUS

2. **Stored Data** - If jurisdiction is stored in PPDM39 REMARK field
   - Format: `Jurisdiction: Country|StateProvince`

## Usage Example

```csharp
// Texas RRC Application
var texasApplication = new DrillingPermitApplication
{
    ApplicationId = "DP-TX-2025-001",
    Country = Country.UnitedStates,
    StateProvince = StateProvince.Texas,
    RegulatoryAuthority = RegulatoryAuthority.RRC,
    // ... other properties
};

// Alberta AER Application
var albertaApplication = new DrillingPermitApplication
{
    ApplicationId = "DP-AB-2025-001",
    Country = Country.Canada,
    StateProvince = StateProvince.Alberta,
    RegulatoryAuthority = RegulatoryAuthority.AER,
    // ... other properties
};

// Using JurisdictionHelper
var authority = JurisdictionHelper.GetDefaultRegulatoryAuthority(
    Country.UnitedStates, 
    StateProvince.Texas);
// Returns: RegulatoryAuthority.RRC

var isValid = JurisdictionHelper.IsValidJurisdiction(
    Country.Canada, 
    StateProvince.Alberta);
// Returns: true
```

## Adding New Jurisdictions

To add support for a new jurisdiction:

1. **Add to Enums**:
   - Add country to `Country` enum
   - Add state/province to `StateProvince` enum
   - Add regulatory authority to `RegulatoryAuthority` enum

2. **Update Constants**:
   - Add authority constant to `PermitConstants.cs`

3. **Update Mappers**:
   - Update `InferJurisdiction()` method in `ApplicationMapper.cs`
   - Update `GetDefaultRegulatoryAuthority()` in `JurisdictionHelper`

4. **Add Requirements**:
   - Create `JurisdictionRequirements` for the new jurisdiction
   - Document required forms and regulations

## PPDM39 Storage

Jurisdiction information is stored in the PPDM39 `APPLICATION.REMARK` field in the format:
```
Jurisdiction: Country|StateProvince
```

This allows retrieval of jurisdiction information when mapping from PPDM39 to domain models.

## Future Enhancements

- Database fields for Country and StateProvince in PPDM39 (if schema is extended)
- Jurisdiction-specific validation rules
- Jurisdiction-specific form templates
- Multi-jurisdiction reporting and compliance tracking


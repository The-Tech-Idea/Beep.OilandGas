# Beep.OilandGas.PermitsAndApplications - LifeCycle Integration Guide

## Overview

**Beep.OilandGas.PermitsAndApplications** is a comprehensive library for managing permits, applications, and regulatory compliance throughout the oil and gas field lifecycle, supporting multiple jurisdictions (Texas RRC, TCEQ, Alberta AER, Federal BLM, USACE, EPA).

### Key Capabilities
- **Drilling Permits**: New well drilling, re-entry, horizontal drilling
- **Environmental Permits**: Waste management, pits, discharges, NORM
- **Injection/Storage Permits**: Enhanced recovery, disposal, gas storage, CO2 storage
- **Groundwater Advisory**: Groundwater protection determinations
- **Multi-Jurisdiction Support**: Texas, Alberta, Federal agencies
- **PPDM39 Integration**: Complete mapping to PPDM data model

### Current Status
⚠️ **Not Yet Integrated** - Should be integrated into LifeCycle services for permit management

---

## Key Classes and Interfaces

### Main Classes

#### `PermitApplication`
Base class for all permit applications.

**Key Properties:**
```csharp
public class PermitApplication
{
    public string ApplicationId { get; set; }
    public PermitApplicationType ApplicationType { get; set; }
    public PermitApplicationStatus Status { get; set; }
    public RegulatoryAuthority RegulatoryAuthority { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? DecisionDate { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
```

#### `DrillingPermitApplication`
Drilling permit application.

**Key Properties:**
```csharp
public class DrillingPermitApplication : PermitApplication
{
    public string WellUWI { get; set; }
    public string TargetFormation { get; set; }
    public decimal ProposedDepth { get; set; }
    public string DrillingMethod { get; set; }
    public bool SurfaceOwnerNotified { get; set; }
}
```

#### `EnvironmentalPermitApplication`
Environmental permit application.

**Key Properties:**
```csharp
public class EnvironmentalPermitApplication : PermitApplication
{
    public EnvironmentalPermitType PermitType { get; set; }
    public string WasteType { get; set; }
    public decimal? WasteVolume { get; set; }
    public bool NORMInvolved { get; set; }
}
```

#### `InjectionPermitApplication`
Injection permit application.

**Key Properties:**
```csharp
public class InjectionPermitApplication : PermitApplication
{
    public InjectionType InjectionType { get; set; }
    public string InjectionZone { get; set; }
    public string InjectionFluid { get; set; }
    public decimal? MaxInjectionPressure { get; set; }
    public decimal? MaxInjectionRate { get; set; }
}
```

---

## Integration with LifeCycle Services

### Planned Integration

**Service:** New `PermitManagementService` or integrate into phase services  
**Location:** `Beep.OilandGas.LifeCycle.Services.Permits.PermitManagementService`

### Integration Points

1. **Exploration Service**
   - Exploratory drilling permits
   - Seismic survey permits
   - Environmental permits for exploration

2. **Development Service**
   - Development drilling permits
   - Facility permits
   - Pipeline permits

3. **Production Service**
   - Production permits
   - Injection permits
   - Environmental monitoring

4. **Decommissioning Service**
   - Plugging permits
   - Site restoration permits

### Service Methods (To Be Added)

```csharp
public interface IPermitManagementService
{
    Task<PermitApplication> CreatePermitApplicationAsync(PermitApplicationRequest request);
    Task<PermitApplication> GetPermitApplicationAsync(string applicationId);
    Task<List<PermitApplication>> GetPermitsForWellAsync(string wellId);
    Task<List<PermitApplication>> GetPermitsForFieldAsync(string fieldId);
    Task<PermitApplication> UpdatePermitStatusAsync(string applicationId, PermitApplicationStatus status);
}
```

---

## Usage Examples

### Example 1: Create Drilling Permit Application

```csharp
using Beep.OilandGas.PermitsAndApplications.Models;

var drillingPermit = new DrillingPermitApplication
{
    ApplicationId = "DP-2025-001",
    ApplicationType = PermitApplicationType.Drilling,
    Status = PermitApplicationStatus.Draft,
    RegulatoryAuthority = RegulatoryAuthority.RRC,
    WellUWI = "12345678901234",
    TargetFormation = "Eagle Ford",
    ProposedDepth = 8500m,
    DrillingMethod = "Horizontal",
    SurfaceOwnerNotified = true,
    CreatedDate = DateTime.Now
};
```

### Example 2: Integration with LifeCycle Service (Planned)

```csharp
var permitService = serviceProvider.GetRequiredService<IPermitManagementService>();

var request = new PermitApplicationRequest
{
    ApplicationType = PermitApplicationType.Drilling,
    WellId = "WELL-001",
    RegulatoryAuthority = RegulatoryAuthority.RRC,
    UserId = "user123"
};

var application = await permitService.CreatePermitApplicationAsync(request);
```

---

## Data Storage

### PPDM Tables

#### Existing PPDM Tables (Use These)

**Status:** ✅ **Existing PPDM Tables** - These tables already exist in PPDM39.

1. **`APPLICATION`** - Main application entity
   - Stores application/permit data
   - Links to entities via foreign keys
   - Use for: All permit applications

2. **`APPLIC_ATTACH`** - Application attachments
   - Stores attachment documents
   - Links to `APPLICATION`
   - Use for: Permit application attachments

3. **`APPLIC_AREA`** - Geographic areas
   - Stores geographic area data
   - Links to `APPLICATION`
   - Use for: Application geographic coverage

4. **`APPLICATION_COMPONENT`** - Application components
   - Stores component details
   - Links to `APPLICATION`
   - Use for: Application component breakdown

5. **`WELL_LICENSE`** - Well-specific licenses
   - Stores well license data
   - Links to `WELL`
   - Use for: Well permits and licenses

6. **`WELL_PERMIT_TYPE`** - Permit types
   - Stores permit type reference data
   - Use for: Permit type classification

#### New Tables Needed (To Be Created)

**Status:** ⚠️ **New Tables Needed** - These tables do not exist in PPDM39 and must be created following PPDM patterns.

1. **`PERMIT_STATUS_HISTORY`** - Permit status change history
   ```sql
   CREATE TABLE PERMIT_STATUS_HISTORY (
       PERMIT_STATUS_HISTORY_ID VARCHAR(50) PRIMARY KEY,
       APPLICATION_ID VARCHAR(50) NOT NULL,
       STATUS VARCHAR(50),  -- Draft, Submitted, UnderReview, Approved, etc.
       STATUS_DATE DATETIME,
       STATUS_CHANGED_BY VARCHAR(50),
       COMMENTS VARCHAR(1000),
       -- Standard PPDM columns
       ROW_ID VARCHAR(50),
       ROW_CHANGED_BY VARCHAR(50),
       ROW_CHANGED_DATE DATETIME,
       ROW_CREATED_BY VARCHAR(50),
       ROW_CREATED_DATE DATETIME
   );
   ```

### Relationships

**Existing Tables:**
- `APPLICATION` links to `WELL`, `FIELD`, `FACILITY` via foreign keys
- `APPLIC_ATTACH.APPLICATION_ID` → `APPLICATION.APPLICATION_ID`
- `APPLIC_AREA.APPLICATION_ID` → `APPLICATION.APPLICATION_ID`
- `APPLICATION_COMPONENT.APPLICATION_ID` → `APPLICATION.APPLICATION_ID`
- `WELL_LICENSE.WELL_ID` → `WELL.WELL_ID`

**New Tables:**
- `PERMIT_STATUS_HISTORY.APPLICATION_ID` → `APPLICATION.APPLICATION_ID`

---

## Best Practices

1. **Permit Tracking**
   - Track permit status throughout lifecycle
   - Monitor expiration dates
   - Maintain permit history

2. **Regulatory Compliance**
   - Use correct regulatory authority
   - Follow jurisdiction-specific requirements
   - Maintain compliance documentation

---

## Future Enhancements

### Planned Integrations

1. **Exploration Service Integration**
   - Automatic permit creation for exploratory wells
   - Permit status tracking
   - Integration with prospect evaluation

2. **Development Service Integration**
   - Permit management for development activities
   - Integration with pipeline permits
   - Facility permit tracking

3. **FieldOrchestrator Integration**
   - Field-level permit management
   - Compliance monitoring
   - Permit expiration alerts

---

## References

- **Project Location:** `Beep.OilandGas.PermitsAndApplications`
- **Service Integration:** `Beep.OilandGas.LifeCycle.Services.Permits.PermitManagementService` (planned)
- **Documentation:** `Beep.OilandGas.PermitsAndApplications/README.md`
- **PPDM Tables:** `APPLICATION`, `WELL_LICENSE`, `WELL_PERMIT_TYPE`

---

**Last Updated:** 2024  
**Status:** ⚠️ Not Yet Integrated (Should be integrated)


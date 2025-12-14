# Beep.OilandGas.PermitsAndApplications

A comprehensive library for managing permits, applications, and regulatory compliance throughout the oil and gas field lifecycle, supporting multiple jurisdictions:

- **United States - Texas**: Railroad Commission of Texas (RRC), Texas Commission on Environmental Quality (TCEQ)
- **Canada - Alberta**: Alberta Energy Regulator (AER)
- **United States - Federal**: Bureau of Land Management (BLM), U.S. Army Corps of Engineers (USACE), Environmental Protection Agency (EPA)

All data is mapped to PPDM39 data model with jurisdiction-specific support.

## Overview

This library provides support for the complete field lifecycle from prospect identification through decommissioning, with full permit and application management capabilities.

## Field Lifecycle Stages

### Stage 1: Prospect Identification & Exploration
- Geological surveys
- Seismic data acquisition
- Exploratory drilling permits

### Stage 2: Lease Acquisition & Land Rights
- Lease negotiations
- Land agreements
- Surface access permits

### Stage 3: Development Planning & Permits
- Development plan preparation
- Drilling permits
- Environmental permits
- Facility permits

### Stage 4: Drilling & Construction
- Well drilling permits
- Construction permits
- Environmental compliance

### Stage 5: Production Operations
- Production permits
- Injection/storage permits
- Environmental monitoring

### Stage 6: Enhanced Recovery & Injection
- Enhanced recovery permits
- Injection well permits
- Disposal well permits

### Stage 7: Decommissioning & Site Restoration
- Plugging permits
- Site restoration permits
- Final environmental clearance

## Features

### Permit Types Supported

#### 1. Drilling Permits
- New well drilling
- Re-entry permits
- Reapplication for expired permits
- Horizontal drilling permits

#### 2. Environmental Permits
- Waste Haulers
- Minor Permits (hydrostatic test discharges, domestic wastewater)
- Pits (waste pits, reserve pits)
- Recycling facilities
- Discharges
- Landfarming, Landtreatment, & Land Application Facilities
- Reclamation Plants
- Commercial Waste Separation Facilities
- Commercial Surface Waste Facilities
- Hazardous Waste
- NORM (Naturally Occurring Radioactive Material)
- Monitoring and Reporting for Surface Waste Facilities

#### 3. Injection/Storage Permits
- Enhanced Recovery (EOR)
- Oil and Gas Waste Disposal
- Mechanical Integrity Tests (MITs)
- Reservoir Gas Storage
- Cavern Storage
- Geologic Storage of Carbon Dioxide (CO2)
- Brine Mining & Brine Production (Lithium)
- Geothermal
- TCEQ Class I - Disposal Wells

#### 4. Groundwater Advisory Unit
- Groundwater Protection Determinations
- GAU Payments
- North Central Texas Area Groundwater Protection Determination
- GAU Oil & Gas Waste Disposal Well Letter
- GAU Determination for a Seismic Survey

## Core Models

### PermitApplication
Base class for all permit applications with common properties:
- Application ID, Type, Status
- Regulatory Authority (RRC, TCEQ, etc.)
- Dates (Created, Submitted, Received, Decision, Effective, Expiry)
- Applicant and Operator information
- Attachments, Areas, Components

### DrillingPermitApplication
Extends `PermitApplication` with drilling-specific properties:
- Well UWI
- Legal description
- Target formation
- Proposed depth
- Drilling method
- Surface owner notification
- Environmental assessment

### EnvironmentalPermitApplication
Extends `PermitApplication` with environmental-specific properties:
- Environmental permit type
- Waste type and volume
- Disposal method
- Environmental impact
- Monitoring plan
- NORM involvement

### InjectionPermitApplication
Extends `PermitApplication` with injection-specific properties:
- Injection type
- Injection zone
- Injection fluid
- Maximum injection pressure and rate
- MIT (Mechanical Integrity Test) results
- Monitoring requirements
- CO2/Gas storage flags

## PPDM39 Integration

The library provides complete mapping to PPDM39 entities:

- **APPLICATION**: Main application entity
- **APPLIC_ATTACH**: Application attachments/documents
- **APPLIC_AREA**: Geographic areas covered
- **APPLICATION_COMPONENT**: Application components
- **WELL_LICENSE**: Well-specific licenses
- **WELL_PERMIT_TYPE**: Types of well permits

## Usage Example

```csharp
using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.PermitsAndApplications.DataMapping;

// Create a drilling permit application
var drillingPermit = new DrillingPermitApplication
{
    ApplicationId = "DP-2025-001",
    ApplicationType = PermitApplicationType.Drilling,
    Status = PermitApplicationStatus.Draft,
    RegulatoryAuthority = RegulatoryAuthority.RRC,
    WellUWI = "12345678901234",
    TargetFormation = "Eagle Ford",
    ProposedDepth = 8500,
    DrillingMethod = "Horizontal",
    SurfaceOwnerNotified = true,
    CreatedDate = DateTime.Now
};

// Map to PPDM39
var mapper = new ApplicationMapper();
var ppdmApplication = mapper.MapDrillingPermitToPPDM39(drillingPermit);

// Map from PPDM39
var domainApplication = mapper.MapToDomain(ppdmApplication);
```

## Regulatory Authorities

### United States - Texas
- **RRC**: Railroad Commission of Texas - [Applications and Permits](https://www.rrc.texas.gov/oil-and-gas/applications-and-permits/) | [Forms](https://www.rrc.texas.gov/oil-and-gas/oil-and-gas-forms/)
- **TCEQ**: Texas Commission on Environmental Quality

### Canada - Alberta
- **AER**: Alberta Energy Regulator - [AER Forms](https://www.aer.ca/applications-and-notices/application-processes/aer-forms)

### United States - Federal
- **BLM**: Bureau of Land Management
- **USACE**: U.S. Army Corps of Engineers
- **EPA**: Environmental Protection Agency

## Application Status Workflow

1. **Draft** - Application being prepared
2. **Submitted** - Application submitted to regulatory authority
3. **UnderReview** - Application under regulatory review
4. **AdditionalInformationRequired** - More information needed
5. **Approved** - Application approved
6. **Rejected** - Application rejected
7. **Withdrawn** - Application withdrawn by applicant
8. **Expired** - Permit has expired
9. **Renewed** - Permit has been renewed

## Integration Points

- **PPDM39**: Complete data model mapping
- **FieldManagement**: Integration with field lifecycle management
- **ProductionForecasting**: For development planning
- **EconomicAnalysis**: For permit cost analysis
- **WellTestAnalysis**: For permit requirements

## References

### Texas (RRC)
- [RRC Applications and Permits](https://www.rrc.texas.gov/oil-and-gas/applications-and-permits/)
- [RRC Oil & Gas Forms](https://www.rrc.texas.gov/oil-and-gas/oil-and-gas-forms/)
- Texas Administrative Code

### Alberta (AER)
- [AER Forms](https://www.aer.ca/applications-and-notices/application-processes/aer-forms)
- Environmental Protection and Enhancement Act
- Mines and Minerals Act
- Public Lands Act
- Water Act

### Standards
- PPDM39 Data Model Standard

## License

See main repository license.


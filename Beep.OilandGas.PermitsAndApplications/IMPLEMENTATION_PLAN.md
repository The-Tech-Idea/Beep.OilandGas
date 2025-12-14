# Beep.OilandGas.PermitsAndApplications - Implementation Plan

## Overview

This project provides comprehensive support for managing permits, applications, and regulatory compliance throughout the oil and gas field lifecycle, supporting multiple jurisdictions including:

- **United States - Texas**: Railroad Commission of Texas (RRC), Texas Commission on Environmental Quality (TCEQ)
- **Canada - Alberta**: Alberta Energy Regulator (AER)
- **United States - Federal**: Bureau of Land Management (BLM), U.S. Army Corps of Engineers (USACE), Environmental Protection Agency (EPA)

All data is mapped to PPDM39 data model with jurisdiction-specific support.

## Field Lifecycle Stages

### Stage 1: Prospect Identification & Exploration
- **Objective**: Identify potential oil and gas reserves
- **Activities**: 
  - Geological surveys
  - Seismic data acquisition
  - Exploratory drilling
- **Permits Required**:
  - Seismic survey permits
  - Exploratory drilling permits
  - Environmental assessments

### Stage 2: Lease Acquisition & Land Rights
- **Objective**: Secure mineral rights and surface access
- **Activities**:
  - Lease negotiations
  - Land agreements
  - Surface use agreements
- **Permits Required**:
  - Land use permits
  - Surface access permits

### Stage 3: Development Planning & Permits
- **Objective**: Obtain all necessary permits for field development
- **Activities**:
  - Development plan preparation
  - Permit applications
  - Regulatory submissions
- **Permits Required**:
  - Drilling permits
  - Environmental permits
  - Facility permits

### Stage 4: Drilling & Construction
- **Objective**: Execute drilling and construct facilities
- **Activities**:
  - Well drilling
  - Facility construction
  - Infrastructure development
- **Permits Required**:
  - Drilling permits (active)
  - Construction permits
  - Environmental compliance

### Stage 5: Production Operations
- **Objective**: Optimize production and maintain compliance
- **Activities**:
  - Production operations
  - Well maintenance
  - Regulatory reporting
- **Permits Required**:
  - Production permits
  - Injection/storage permits (if applicable)
  - Environmental monitoring permits

### Stage 6: Enhanced Recovery & Injection
- **Objective**: Maximize recovery through enhanced methods
- **Activities**:
  - Water flooding
  - Gas injection
  - CO2 injection
- **Permits Required**:
  - Enhanced recovery permits
  - Injection well permits
  - Disposal well permits

### Stage 7: Decommissioning & Site Restoration
- **Objective**: Safely close operations and restore site
- **Activities**:
  - Well plugging
  - Facility removal
  - Site remediation
- **Permits Required**:
  - Plugging permits
  - Site restoration permits
  - Final environmental clearance

## Regulatory Authorities and Permit Types

### Texas Railroad Commission (RRC)
**Reference**: [RRC Applications and Permits](https://www.rrc.texas.gov/oil-and-gas/applications-and-permits/)  
**Forms**: [RRC Oil & Gas Forms](https://www.rrc.texas.gov/oil-and-gas/oil-and-gas-forms/)

### 1. Drilling Permits
- New well drilling
- Re-entry permits
- Reapplication for expired permits
- Horizontal drilling permits

### 2. Environmental Permits
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

### 3. Injection/Storage Permits
- Enhanced Recovery (EOR)
- Oil and Gas Waste Disposal
- Mechanical Integrity Tests (MITs)
- Reservoir Gas Storage
- Cavern Storage
- Geologic Storage of Carbon Dioxide (CO2)
- Brine Mining & Brine Production (Lithium)
- Geothermal
- TCEQ Class I - Disposal Wells

### 4. Groundwater Advisory Unit
- Groundwater Protection Determinations
- GAU Payments
- North Central Texas Area Groundwater Protection Determination
- GAU Oil & Gas Waste Disposal Well Letter
- GAU Determination for a Seismic Survey

### Alberta Energy Regulator (AER)
**Reference**: [AER Forms](https://www.aer.ca/applications-and-notices/application-processes/aer-forms)

#### AER Application Types:
1. **Directive Forms**
   - Forms attached to AER directives for specific energy resource activities

2. **Environmental Protection and Enhancement Act Forms**
   - Forms required under the Environmental Protection and Enhancement Act

3. **Liability Management Forms**
   - Licensee capability assessment
   - Closure nomination
   - Transfer forms
   - Financial security

4. **Mines and Minerals Act Forms**
   - Forms required under the Mines and Minerals Act

5. **Public Lands Act Forms**
   - Forms required under the Public Lands Act

6. **Water Act Forms**
   - Water licence applications
   - Water use reporting

7. **Remediation and Reclamation Forms**
   - Reclamation certificate applications
   - Record of Site Condition
   - Contamination management

8. **Release Reporting**
   - Release notification
   - Release reports

9. **Rock-Hosted Mineral Exploration (Manual 030)**
   - Forms for metallic and industrial minerals exploration

10. **Private Surface Agreements Registry Forms**
    - Landowner registry forms

## PPDM39 Entity Mapping

### Core Entities
- **APPLICATION**: Main application entity
  - APPLICATION_ID
  - APPLICATION_TYPE
  - CURRENT_STATUS
  - DECISION
  - DECISION_DATE
  - EFFECTIVE_DATE
  - EXPIRY_DATE
  - SUBMITTED_DATE
  - APPROVED_DATE

- **APPLICATION_COMPONENT**: Components of application
- **APPLIC_AREA**: Geographic areas covered
- **APPLIC_ATTACH**: Attachments/documents
- **APPLIC_BA**: Business associates (applicants, operators)
- **APPLIC_DESC**: Descriptions
- **APPLIC_REMARK**: Remarks/notes

- **WELL_LICENSE**: Well-specific licenses
  - UWI
  - APPLICATION_ID (links to APPLICATION)
  - LICENSE_ID
  - LICENSE_TYPE
  - GRANTED_DATE
  - EXPIRY_DATE

- **WELL_PERMIT_TYPE**: Types of well permits
  - PERMIT_TYPE
  - GRANTED_BY_BA_ID (regulatory authority)

- **BA_PERMIT**: Business associate permits
- **FACILITY_LICENSE**: Facility licenses

## Implementation Phases

### Phase 1: Core Models & PPDM39 Mapping
1. **Models**:
   - `PermitApplication` - Base application model
   - `DrillingPermitApplication` - Drilling permit specific
   - `EnvironmentalPermitApplication` - Environmental permits
   - `InjectionPermitApplication` - Injection/storage permits
   - `PermitStatus` - Status tracking
   - `PermitType` - Enumeration of permit types
   - `RegulatoryAuthority` - RRC, TCEQ, etc.

2. **PPDM39 Mappers**:
   - `ApplicationMapper` - Maps to/from APPLICATION
   - `WellLicenseMapper` - Maps to/from WELL_LICENSE
   - `PermitApplicationMapper` - Maps application models to PPDM39

### Phase 2: Application Management
1. **Services**:
   - `PermitApplicationService` - Create, update, submit applications
   - `PermitStatusService` - Track status and workflow
   - `DocumentManagementService` - Handle attachments
   - `ComplianceService` - Check compliance requirements

2. **Workflow**:
   - Application creation
   - Submission workflow
   - Review process
   - Approval/rejection handling
   - Renewal process

### Phase 3: Jurisdiction-Specific Requirements

#### Texas (RRC/TCEQ) Requirements:
1. **Drilling Permit Requirements**:
   - Well location (legal description)
   - Target formation
   - Drilling method
   - Environmental impact assessment
   - Surface owner notification

2. **Environmental Permit Requirements**:
   - Waste type and volume
   - Disposal method
   - Environmental impact
   - Monitoring plans

3. **Injection Permit Requirements**:
   - Injection fluid type
   - Injection zone
   - Mechanical integrity test results
   - Monitoring requirements

#### Alberta (AER) Requirements:
1. **Well Licence Requirements**:
   - Well location (legal land description)
   - Target formation
   - Drilling method
   - Surface owner consultation
   - Environmental assessment

2. **Environmental Protection Requirements**:
   - Environmental Protection and Enhancement Act compliance
   - Reclamation planning
   - Contamination management
   - Record of Site Condition

3. **Liability Management Requirements**:
   - Licensee capability assessment
   - Financial security
   - Closure planning
   - Closure nomination

4. **Water Act Requirements**:
   - Water licence applications
   - Water use reporting
   - Water source protection

5. **Public Lands Act Requirements**:
   - Public lands access permits
   - Surface disturbance permits

### Phase 4: Compliance & Reporting
1. **Compliance Tracking**:
   - Permit expiration monitoring
   - Renewal reminders
   - Violation tracking
   - Compliance reporting

2. **Reporting**:
   - Application status reports
   - Permit inventory
   - Compliance reports
   - Regulatory submissions

### Phase 5: Integration
1. **Field Management Integration**:
   - Link permits to wells
   - Link permits to facilities
   - Link permits to production operations

2. **Workflow Integration**:
   - Integration with prospect identification
   - Integration with drilling operations
   - Integration with production operations

## Data Model Structure

```csharp
// Core Models
PermitApplication
├── ApplicationId
├── ApplicationType (Drilling, Environmental, Injection, etc.)
├── Status (Draft, Submitted, UnderReview, Approved, Rejected, Expired)
├── SubmittedDate
├── DecisionDate
├── EffectiveDate
├── ExpiryDate
├── RegulatoryAuthority (RRC, TCEQ, etc.)
├── Applicant (Business Associate)
├── Operator (Business Associate)
├── RelatedWell (UWI)
├── RelatedFacility
├── Attachments
└── Components

DrillingPermitApplication : PermitApplication
├── WellLocation (Legal Description)
├── TargetFormation
├── DrillingMethod
├── ProposedDepth
├── SurfaceOwnerNotification
└── EnvironmentalAssessment

EnvironmentalPermitApplication : PermitApplication
├── WasteType
├── WasteVolume
├── DisposalMethod
├── EnvironmentalImpact
└── MonitoringPlan

InjectionPermitApplication : PermitApplication
├── InjectionType (Enhanced Recovery, Disposal, Storage)
├── InjectionZone
├── InjectionFluid
├── MITResults
└── MonitoringRequirements
```

## Integration Points

1. **With PPDM39**:
   - APPLICATION entity
   - WELL_LICENSE entity
   - WELL entity (for well permits)
   - FACILITY entity (for facility permits)
   - BUSINESS_ASSOCIATE (for applicants/operators)

2. **With FieldManagement**:
   - Prospect identification results
   - Well planning data
   - Production operations
   - Facility operations

3. **With Other Modules**:
   - ProductionForecasting (for development planning)
   - EconomicAnalysis (for permit cost analysis)
   - WellTestAnalysis (for permit requirements)

## Success Criteria

- ✅ All RRC permit types supported
- ✅ Complete PPDM39 mapping
- ✅ Application workflow management
- ✅ Compliance tracking
- ✅ Document management
- ✅ Integration with field lifecycle stages
- ✅ Regulatory reporting capabilities


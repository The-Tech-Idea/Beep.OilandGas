# Beep.OilandGas.PermitsAndApplications

A comprehensive library for managing permits, applications, and regulatory compliance throughout the oil and gas field lifecycle, supporting multiple jurisdictions:

- **United States - Texas**: Railroad Commission of Texas (RRC), Texas Commission on Environmental Quality (TCEQ)
- **Canada - Alberta**: Alberta Energy Regulator (AER)
- **United States - Federal**: Bureau of Land Management (BLM), U.S. Army Corps of Engineers (USACE), Environmental Protection Agency (EPA)

All data is mapped to PPDM39 data model with jurisdiction-specific support.

## Planning & enhancements

- **[MASTER-TODO-TRACKER.md](./MASTER-TODO-TRACKER.md)** — phased checklist (restructure + enhancements)  
- **[.plans/permit_applications_restructure/ENHANCEMENT_PLAN.md](./.plans/permit_applications_restructure/ENHANCEMENT_PLAN.md)** — oil & gas regulatory workflow enhancement roadmap  

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

## Application services and ownership

The current delivery status and remaining gates are owned by [Plan 354](../Plans/FrameworkAppEnhancement/Implementation-354-Permit-Application-Workflow.md) and the [company tracker](../Plans/FrameworkAppEnhancement/Company-Plan-Index.md).

`TheTechIdea.Data` owns the canonical `PERMIT_APPLICATION`, supporting rows, history, immutable command receipt and transport contracts. The PERMITS module manifest uses Beep Migration for schema changes; new application schema does not use handwritten SQL or EF migrations. Clients use the typed API clients.

`PermitApplicationLifecycleService.ReviewSourceAsync` captures the exact current company/FIELD header, history and three supporting rows. Supporting schema/read failures refuse review rather than confirming absence. `RecordCommandAsync` records a reviewed original through the existing owned Serializable transaction, history and receipt. `RecoverCommandAsync` reconciles that same original without a new effect.

Author draft/header changes, submission and independent decisions use this command owner. `AMEND_DRILLING` supplies typed `PermitDrillingDraftChanges` for Draft or AdditionalInformationRequired applications with a recorded well. The proposal cannot change parent ownership, identity, classification, well or filing state. Positive depth requires an explicit recorded unit; casing, legal description and supporting notes allow up to4096 characters. The permit screen retains the original before transport and displays its exact proposal and recorded result. `AMEND_ENVIRONMENTAL` and `AMEND_INJECTION` use the same reviewed owner and typed screen controls. Waste volume and injection limits require an explicit paired unit when entered; unknown quantities and indicators remain nullable. Injection uses only canonical maximum pressure/rate/fluid fields and its recorded well; environmental disposal uses the active `DISPOSAL_METHOD`. Each subtype has its own retained proposal and recorded result display. Attachments remain planned work.

`IPermitApplicationWorkflowService` supplies company-bound reads, automated validation and form payload projection; it does not expose raw persistence or unreviewed lifecycle writes. The automated checker and template renderer are not evidence of regulator approval. Supporting values use the exact canonical parent and recorded units; no unit conversion is inferred.

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


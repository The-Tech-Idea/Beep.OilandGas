# Exploration Process - Implementation Plan

## Overview

This document defines the process workflows for the Exploration phase, including Lead management, Prospect evaluation, Discovery recording, and Development decision processes.

## Current State

### What Exists
- ✅ Basic CRUD for PROSPECT table
- ✅ Basic CRUD for SEIS_ACQTN_SURVEY (seismic surveys)
- ✅ Basic CRUD for exploratory wells
- ❌ **Missing**: Lead management, Prospect workflows, Discovery processes, Risk assessment workflows, Volume estimation workflows

### What Needs to Be Built
1. Lead to Prospect workflow
2. Prospect evaluation workflow (Risk → Volume → Economic → Drilling Decision)
3. Discovery recording workflow
4. Discovery to Development decision workflow
5. Prospect portfolio management
6. Exploration program management

## Exploration Process Workflows

### Process 1: Lead to Prospect Workflow

#### Workflow Definition
```
Lead Creation → Lead Evaluation → Lead Approval → Prospect Creation → Prospect Initial Assessment
```

#### Detailed Steps

**Step 1: Lead Creation**
- **Action**: Create LEAD entity
- **Required Data**: Lead name, location, source, initial assessment
- **Validation**: Location must be valid, source must be specified
- **Output**: LEAD entity created

**Step 2: Lead Evaluation**
- **Action**: Evaluate lead potential
- **Required Data**: Geological data, analog data, initial risk assessment
- **Validation**: At least one geological indicator required
- **Approval**: Required from Exploration Manager
- **Output**: Lead evaluation report

**Step 3: Lead Approval**
- **Action**: Approve or reject lead
- **Required Data**: Evaluation results, recommendation
- **Validation**: Evaluation must be completed
- **Approval**: Required from Exploration Manager or higher
- **Output**: Lead status (APPROVED/REJECTED)

**Step 4: Prospect Creation**
- **Action**: Create PROSPECT from approved LEAD
- **Required Data**: Prospect name, location, geological data
- **Validation**: Lead must be approved
- **Output**: PROSPECT entity created, LEAD status updated to PROMOTED

**Step 5: Prospect Initial Assessment**
- **Action**: Initial prospect assessment
- **Required Data**: Basic geological data, initial volume estimate
- **Validation**: Prospect must exist
- **Output**: Prospect assessment record

#### State Machine
```
LEAD_CREATED → LEAD_EVALUATING → LEAD_APPROVED → PROSPECT_CREATED → PROSPECT_ASSESSED
                ↓
            LEAD_REJECTED (terminal)
```

### Process 2: Prospect to Discovery Workflow

#### Workflow Definition
```
Prospect Creation → Risk Assessment → Volume Estimation → Economic Evaluation → Drilling Decision → Discovery Recording
```

#### Detailed Steps

**Step 1: Prospect Creation**
- **Action**: Create or receive PROSPECT
- **Required Data**: Prospect name, location, geological data
- **Validation**: Location must be valid
- **Output**: PROSPECT entity created

**Step 2: Risk Assessment**
- **Action**: Perform geological risk assessment
- **Required Data**: Trap risk, Reservoir risk, Seal risk, Source risk, Timing risk
- **Validation**: All risk components must be assessed
- **Calculation**: Overall geological risk = Trap × Reservoir × Seal × Source × Timing
- **Output**: PROSPECT_RISK_ASSESSMENT record

**Step 3: Volume Estimation**
- **Action**: Estimate recoverable volumes
- **Required Data**: Reservoir parameters, trap parameters, recovery factors
- **Validation**: Risk assessment must be completed
- **Calculation**: P10/P50/P90 probabilistic volumes
- **Output**: PROSPECT_VOLUME_ESTIMATE record

**Step 4: Economic Evaluation**
- **Action**: Perform economic analysis
- **Required Data**: Volume estimates, cost estimates, price forecasts
- **Validation**: Volume estimates must exist
- **Calculation**: NPV, IRR, EMV (Expected Monetary Value)
- **Output**: PROSPECT_ECONOMIC record

**Step 5: Drilling Decision**
- **Action**: Make drilling decision
- **Required Data**: Economic evaluation, risk assessment, strategic fit
- **Validation**: Economic evaluation must be completed
- **Approval**: Required from Exploration Manager and Business Development
- **Output**: Drilling decision (APPROVED/REJECTED/DEFERRED)

**Step 6: Discovery Recording**
- **Action**: Record discovery if drilling successful
- **Required Data**: Discovery date, discovery well, initial reserves
- **Validation**: Drilling must be approved and completed
- **Output**: PROSPECT_DISCOVERY record, PROSPECT status updated to DISCOVERED

#### State Machine
```
PROSPECT_CREATED → RISK_ASSESSED → VOLUME_ESTIMATED → ECONOMIC_EVALUATED → DRILLING_DECIDED → DISCOVERY_RECORDED
                    ↓                    ↓                    ↓
                REJECTED            REJECTED            REJECTED (terminal)
```

### Process 3: Discovery to Development Decision

#### Workflow Definition
```
Discovery Recording → Appraisal → Reserve Estimation → Economic Analysis → Development Approval
```

#### Detailed Steps

**Step 1: Discovery Recording**
- **Action**: Record discovery
- **Required Data**: Discovery date, discovery well, initial reserves
- **Validation**: Discovery well must exist
- **Output**: PROSPECT_DISCOVERY record

**Step 2: Appraisal**
- **Action**: Perform appraisal activities
- **Required Data**: Appraisal wells, additional seismic, reservoir characterization
- **Validation**: Discovery must be recorded
- **Output**: Appraisal data, updated reservoir model

**Step 3: Reserve Estimation**
- **Action**: Estimate proved reserves
- **Required Data**: Appraisal data, reservoir model, recovery factors
- **Validation**: Appraisal must be completed
- **Calculation**: Proved, Probable, Possible reserves
- **Output**: RESERVE_ENTITY records

**Step 4: Economic Analysis**
- **Action**: Perform development economic analysis
- **Required Data**: Reserve estimates, development costs, production forecasts
- **Validation**: Reserve estimates must exist
- **Calculation**: Development NPV, IRR, Payback period
- **Output**: Economic analysis results

**Step 5: Development Approval**
- **Action**: Approve or reject development
- **Required Data**: Economic analysis, strategic fit, resource availability
- **Validation**: Economic analysis must be completed
- **Approval**: Required from Development Manager and Executive approval
- **Output**: Development decision (APPROVED/REJECTED/DEFERRED)

#### State Machine
```
DISCOVERY_RECORDED → APPRAISING → RESERVES_ESTIMATED → ECONOMIC_ANALYZED → DEVELOPMENT_APPROVED
                        ↓                ↓                    ↓
                    REJECTED        REJECTED            REJECTED (terminal)
```

## Service Implementation

### ExplorationProcessService
**Location**: `Beep.OilandGas.LifeCycle/Services/Exploration/Processes/ExplorationProcessService.cs`

#### Methods

```csharp
// Lead to Prospect Process
Task<ProcessInstance> StartLeadToProspectProcessAsync(string leadId, string fieldId, string userId);
Task<bool> EvaluateLeadAsync(string instanceId, LeadEvaluationRequest data, string userId);
Task<bool> ApproveLeadAsync(string instanceId, string userId);
Task<bool> RejectLeadAsync(string instanceId, string reason, string userId);
Task<ProcessInstance> PromoteLeadToProspectAsync(string instanceId, ProspectRequest prospectData, string userId);

// Prospect to Discovery Process
Task<ProcessInstance> StartProspectToDiscoveryProcessAsync(string prospectId, string fieldId, string userId);
Task<bool> PerformRiskAssessmentAsync(string instanceId, RiskAssessmentRequest data, string userId);
Task<bool> PerformVolumeEstimationAsync(string instanceId, VolumeEstimateRequest data, string userId);
Task<bool> PerformEconomicEvaluationAsync(string instanceId, EconomicEvaluationRequest data, string userId);
Task<bool> MakeDrillingDecisionAsync(string instanceId, DrillingDecisionRequest data, string userId);
Task<bool> RecordDiscoveryAsync(string instanceId, DiscoveryRequest data, string userId);

// Discovery to Development Process
Task<ProcessInstance> StartDiscoveryToDevelopmentProcessAsync(string discoveryId, string fieldId, string userId);
Task<bool> PerformAppraisalAsync(string instanceId, AppraisalRequest data, string userId);
Task<bool> EstimateReservesAsync(string instanceId, ReserveEstimateRequest data, string userId);
Task<bool> PerformDevelopmentEconomicAnalysisAsync(string instanceId, EconomicAnalysisRequest data, string userId);
Task<bool> ApproveDevelopmentAsync(string instanceId, string userId);
```

### LeadToProspectProcess
**Location**: `Beep.OilandGas.LifeCycle/Services/Exploration/Processes/LeadToProspectProcess.cs`

Specific implementation for Lead to Prospect workflow.

### ProspectToDiscoveryProcess
**Location**: `Beep.OilandGas.LifeCycle/Services/Exploration/Processes/ProspectToDiscoveryProcess.cs`

Specific implementation for Prospect to Discovery workflow.

### DiscoveryToDevelopmentProcess
**Location**: `Beep.OilandGas.LifeCycle/Services/Exploration/Processes/DiscoveryToDevelopmentProcess.cs`

Specific implementation for Discovery to Development workflow.

## DTOs Required

### Lead Management DTOs
```csharp
public class LeadRequest { }
public class LeadResponse { }
public class LeadEvaluationRequest { }
public class LeadEvaluationResponse { }
public class LeadApprovalRequest { }
```

### Prospect Process DTOs
```csharp
public class ProspectProcessRequest { }
public class RiskAssessmentRequest { }
public class RiskAssessmentResponse { }
public class VolumeEstimateRequest { }
public class VolumeEstimateResponse { }
public class EconomicEvaluationRequest { }
public class EconomicEvaluationResponse { }
public class DrillingDecisionRequest { }
public class DrillingDecisionResponse { }
```

### Discovery Process DTOs
```csharp
public class DiscoveryRequest { }
public class DiscoveryResponse { }
public class AppraisalRequest { }
public class AppraisalResponse { }
public class ReserveEstimateRequest { }
public class ReserveEstimateResponse { }
```

## PPDM Tables

### Existing Tables (Use These)
- `PROSPECT` - Prospect data
- `SEIS_ACQTN_SURVEY` - Seismic surveys
- `WELL` - Exploratory wells
- `APPLICATION` - Drilling permits

### New Tables Needed

#### LEAD Table
```sql
CREATE TABLE LEAD (
    LEAD_ID VARCHAR(50) PRIMARY KEY,
    FIELD_ID VARCHAR(50),
    LEAD_NAME VARCHAR(200),
    LEAD_TYPE VARCHAR(50), -- STRUCTURAL, STRATIGRAPHIC, COMBINATION
    LEAD_STATUS VARCHAR(50), -- CREATED, EVALUATING, APPROVED, REJECTED, PROMOTED
    SOURCE VARCHAR(200), -- Seismic, Geological, Analog, etc.
    LOCATION_DESC TEXT,
    INITIAL_ASSESSMENT TEXT,
    ROW_CREATED_DATE DATETIME,
    ROW_CREATED_BY VARCHAR(50)
);
```

#### PROSPECT_RISK_ASSESSMENT Table
```sql
CREATE TABLE PROSPECT_RISK_ASSESSMENT (
    PROSPECT_RISK_ASSESSMENT_ID VARCHAR(50) PRIMARY KEY,
    PROSPECT_ID VARCHAR(50),
    ASSESSMENT_DATE DATETIME,
    TRAP_RISK DECIMAL(5,2), -- 0-1
    RESERVOIR_RISK DECIMAL(5,2),
    SEAL_RISK DECIMAL(5,2),
    SOURCE_RISK DECIMAL(5,2),
    TIMING_RISK DECIMAL(5,2),
    OVERALL_RISK DECIMAL(5,2), -- Product of all risks
    ASSESSED_BY VARCHAR(50),
    ROW_CREATED_DATE DATETIME
);
```

#### PROSPECT_VOLUME_ESTIMATE Table
```sql
CREATE TABLE PROSPECT_VOLUME_ESTIMATE (
    PROSPECT_VOLUME_ESTIMATE_ID VARCHAR(50) PRIMARY KEY,
    PROSPECT_ID VARCHAR(50),
    ESTIMATE_DATE DATETIME,
    P10_OIL_RESERVES DECIMAL(18,2), -- MMbbl
    P50_OIL_RESERVES DECIMAL(18,2),
    P90_OIL_RESERVES DECIMAL(18,2),
    P10_GAS_RESERVES DECIMAL(18,2), -- Bcf
    P50_GAS_RESERVES DECIMAL(18,2),
    P90_GAS_RESERVES DECIMAL(18,2),
    ESTIMATED_BY VARCHAR(50),
    ROW_CREATED_DATE DATETIME
);
```

#### PROSPECT_ECONOMIC Table
```sql
CREATE TABLE PROSPECT_ECONOMIC (
    PROSPECT_ECONOMIC_ID VARCHAR(50) PRIMARY KEY,
    PROSPECT_ID VARCHAR(50),
    EVALUATION_DATE DATETIME,
    NPV DECIMAL(18,2), -- Net Present Value
    IRR DECIMAL(5,2), -- Internal Rate of Return %
    EMV DECIMAL(18,2), -- Expected Monetary Value
    PAYBACK_PERIOD DECIMAL(10,2), -- Years
    EVALUATED_BY VARCHAR(50),
    ROW_CREATED_DATE DATETIME
);
```

#### PROSPECT_DISCOVERY Table
```sql
CREATE TABLE PROSPECT_DISCOVERY (
    PROSPECT_DISCOVERY_ID VARCHAR(50) PRIMARY KEY,
    PROSPECT_ID VARCHAR(50),
    DISCOVERY_WELL_ID VARCHAR(50),
    DISCOVERY_DATE DATETIME,
    INITIAL_OIL_RESERVES DECIMAL(18,2),
    INITIAL_GAS_RESERVES DECIMAL(18,2),
    DISCOVERY_TYPE VARCHAR(50), -- OIL, GAS, OIL_AND_GAS
    RECORDED_BY VARCHAR(50),
    ROW_CREATED_DATE DATETIME
);
```

## Integration with Existing Services

### Integration with PPDMExplorationService
- ExplorationProcessService will call PPDMExplorationService for CRUD operations
- Process steps will update PROSPECT, WELL, and related tables
- Process history will be tracked in PROCESS_HISTORY table

### Integration with PPDMCalculationService
- Risk assessment calculations
- Volume estimation calculations
- Economic analysis (NPV, IRR, EMV)

### Integration with PermitManagementService
- Drilling permit application as part of Prospect to Discovery workflow
- Permit approval required before drilling decision

## MVVM Structure

### ViewModels (Future Blazor Integration)
**Location**: `Beep.OilandGas.LifeCycle/ViewModels/Exploration/`

- `LeadToProspectViewModel.cs` - Lead promotion process UI
- `ProspectEvaluationViewModel.cs` - Prospect evaluation process UI
- `DiscoveryRecordingViewModel.cs` - Discovery recording UI
- `DevelopmentDecisionViewModel.cs` - Development decision UI

### Process UI Models
- `LeadEvaluationUI.cs` - Lead evaluation form data
- `RiskAssessmentUI.cs` - Risk assessment form data
- `VolumeEstimationUI.cs` - Volume estimation form data
- `EconomicEvaluationUI.cs` - Economic evaluation form data

## Implementation Steps

1. **Create Process Definitions**
   - Define Lead to Prospect process definition
   - Define Prospect to Discovery process definition
   - Define Discovery to Development process definition

2. **Implement ExplorationProcessService**
   - Implement process orchestration methods
   - Integrate with PPDMExplorationService
   - Integrate with PPDMCalculationService

3. **Implement Specific Process Classes**
   - LeadToProspectProcess
   - ProspectToDiscoveryProcess
   - DiscoveryToDevelopmentProcess

4. **Create PPDM Tables**
   - Create LEAD table
   - Create PROSPECT_RISK_ASSESSMENT table
   - Create PROSPECT_VOLUME_ESTIMATE table
   - Create PROSPECT_ECONOMIC table
   - Create PROSPECT_DISCOVERY table

5. **Create DTOs**
   - Lead management DTOs
   - Prospect process DTOs
   - Discovery process DTOs

6. **Add Methods to PPDMExplorationService**
   - Lead management methods
   - Risk assessment methods
   - Volume estimation methods
   - Economic evaluation methods
   - Discovery recording methods

7. **Create ViewModels** (Future)
   - Process ViewModels for Blazor UI

## Testing Strategy

1. Unit tests for each process step
2. Integration tests for complete workflows
3. Validation tests for business rules
4. Approval workflow tests
5. State transition tests


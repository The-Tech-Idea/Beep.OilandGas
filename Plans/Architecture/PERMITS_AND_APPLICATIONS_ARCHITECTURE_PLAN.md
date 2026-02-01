# Beep.OilandGas Permits and Applications - Architecture Plan

## Executive Summary

**Goal**: Provide a PPDM-aligned regulatory permitting platform that manages applications, approvals, compliance obligations, and regulatory closeout across the full asset lifecycle.

**Key Principle**: Use **Data Classes Only** in `Beep.OilandGas.Models.Data.PermitsAndApplications` as the system of record; services orchestrate permit workflows and compliance audits.

**Scope**: Regulatory approvals for drilling, construction, production operations, enhanced recovery, and abandonment.

---

## Architecture Principles

### 1) Regulatory Traceability
- Preserve full audit trails for submissions, approvals, and conditions.
- Store regulator communications, notices, and compliance evidence.

### 2) Lifecycle Continuity
- Permits attach to assets (lease, well, facility, pipeline) with clear status history.
- Handoff requirements flow to DrillingAndConstruction and ProductionOperations.

### 3) PPDM39 Alignment
- Use PPDM-style entities and IDs for all permit records.
- Maintain consistent audit columns and status history tables.

### 4) Cross-Project Integration
- **LeaseAcquisition**: surface use and land access.
- **DevelopmentPlanning**: permit dependencies in schedules.
- **DrillingAndConstruction**: well construction approvals.
- **Decommissioning**: closure approvals and reclamation.

---

## Target Project Structure

```
Beep.OilandGas.PermitsAndApplications/
├── Services/
│   ├── PermittingService.cs (orchestrator)
│   ├── ApplicationService.cs
│   ├── ComplianceService.cs
│   ├── EnvironmentalService.cs
│   ├── InspectionService.cs
│   └── DocumentService.cs
├── Workflows/
│   ├── PermitLifecycleWorkflow.cs
│   └── ComplianceWorkflow.cs
├── Validation/
│   ├── PermitValidator.cs
│   ├── ApplicationValidator.cs
│   └── ComplianceValidator.cs
├── Integration/
│   ├── LeaseAcquisitionBridge.cs
│   ├── DevelopmentPlanningBridge.cs
│   └── DecommissioningBridge.cs
└── Exceptions/
    ├── PermittingException.cs
    └── ComplianceException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.PermitsAndApplications`:

### Core Permitting
- PERMIT_APPLICATION
- PERMIT
- PERMIT_STATUS_HISTORY
- REGULATORY_AGENCY
- PERMIT_CONDITION

### Environmental + Public
- ENVIRONMENTAL_ASSESSMENT
- PUBLIC_NOTICE
- STAKEHOLDER_COMMENT
- MITIGATION_PLAN

### Compliance
- COMPLIANCE_REQUIREMENT
- INSPECTION
- VIOLATION
- CORRECTIVE_ACTION

### Documents
- PERMIT_DOCUMENT
- APPLICATION_ATTACHMENT
- REGULATORY_CORRESPONDENCE

---

## Service Interface Standards

```csharp
public interface IPermittingService
{
    Task<PERMIT_APPLICATION> SubmitApplicationAsync(PERMIT_APPLICATION application, string userId);
    Task<PERMIT> IssuePermitAsync(string applicationId, string userId);
    Task<bool> RecordInspectionAsync(INSPECTION inspection, string userId);
    Task<bool> ClosePermitAsync(string permitId, string userId);
}
```

---

## Implementation Phases

### Phase 1: Data Model + Core Services (Week 1)
- Create permit, application, and status history entities.
- Implement PermittingService and validations.

### Phase 2: Compliance + Inspections (Week 2)
- Implement compliance requirements and inspection tracking.
- Add violation and corrective action workflow.

### Phase 3: Environmental + Public Records (Week 3)
- Environmental assessment, mitigation plans, and public comments.

### Phase 4: Integrations (Week 4)
- Connect with DevelopmentPlanning, DrillingAndConstruction, and Decommissioning.

---

## Best Practices Embedded

- **Auditability**: full submission-to-approval trail.
- **Compliance readiness**: inspection and corrective action workflows.
- **Lifecycle alignment**: permit readiness gates in project workflows.

---

## API Endpoint Sketch

```
/api/permits/
├── /applications
│   ├── POST
│   └── GET /{id}
├── /permits
│   ├── POST /issue/{applicationId}
│   ├── GET /{id}
│   └── POST /close/{id}
├── /inspections
│   ├── POST
│   └── GET /{permitId}
└── /compliance
    ├── POST /violations
    └── POST /corrective-actions
```

---

## Success Criteria

- PPDM-aligned permit entities persist all regulatory records.
- Permit readiness gates integrate with drilling and abandonment workflows.
- Compliance issues are tracked end-to-end with audit trails.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)

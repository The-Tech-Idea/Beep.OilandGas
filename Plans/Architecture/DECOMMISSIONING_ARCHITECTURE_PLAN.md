# Beep.OilandGas Decommissioning - Architecture Plan

## Executive Summary

**Goal**: Provide a PPDM-aligned decommissioning platform that manages well abandonment, facility removal, remediation, and regulatory closeout.

**Key Principle**: Use **Data Classes Only** in `Beep.OilandGas.Models.Data.Decommissioning` as the system of record; services orchestrate execution, cost tracking, and regulatory compliance.

**Scope**: Late-life operations through full abandonment and reclamation.

---

## Architecture Principles

### 1) Regulatory Closeout Discipline
- Track regulatory requirements, approvals, and closure evidence.
- Maintain end-to-end audit trails for abandonment actions.

### 2) Cost and Liability Transparency
- Track ARO and decommissioning costs with clear phase ownership.
- Preserve estimates vs actuals and remediation obligations.

### 3) PPDM39 Alignment
- Store abandonment and remediation records in PPDM-style entities.

### 4) Cross-Project Integration
- **PermitsAndApplications**: abandonment permits and closeout.
- **ProductionAccounting**: ARO and cost reconciliation.
- **ProductionOperations**: asset status handoff.

---

## Target Project Structure

```
Beep.OilandGas.Decommissioning/
├── Services/
│   ├── DecommissioningService.cs (orchestrator)
│   ├── AbandonmentService.cs
│   ├── RemediationService.cs
│   ├── FacilityRemovalService.cs
│   ├── RegulatoryCloseoutService.cs
│   └── CostTrackingService.cs
├── Workflows/
│   ├── AbandonmentWorkflow.cs
│   └── RemediationWorkflow.cs
├── Validation/
│   ├── AbandonmentValidator.cs
│   ├── RemediationValidator.cs
│   └── CloseoutValidator.cs
└── Exceptions/
    ├── DecommissioningException.cs
    └── RemediationException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.Decommissioning`:

### Core Decommissioning
- DECOMMISSIONING_PROJECT
- WELL_ABANDONMENT
- PLUGGING_OPERATION
- ABANDONMENT_STATUS_HISTORY

### Facilities + Remediation
- FACILITY_REMOVAL
- SITE_REMEDIATION
- WASTE_MANIFEST
- REMEDIATION_VERIFICATION

### Regulatory + Cost
- REGULATORY_CLOSURE
- COMPLIANCE_CERTIFICATE
- COST_ESTIMATE
- COST_ACTUAL
- ASSET_RETIREMENT_OBLIGATION

---

## Service Interface Standards

```csharp
public interface IDecommissioningService
{
    Task<DECOMMISSIONING_PROJECT> CreateProjectAsync(DECOMMISSIONING_PROJECT project, string userId);
    Task<WELL_ABANDONMENT> RecordAbandonmentAsync(WELL_ABANDONMENT abandonment, string userId);
    Task<bool> SubmitCloseoutAsync(string projectId, string userId);
    Task<bool> RecordRemediationAsync(SITE_REMEDIATION remediation, string userId);
}
```

---

## Implementation Phases

### Phase 1: Data Model + Core Services (Week 1)
- Implement abandonment and remediation entities.
- Create DecommissioningService and validators.

### Phase 2: Cost Tracking + ARO (Week 2)
- ARO estimates and actuals with approvals.

### Phase 3: Regulatory Closeout (Week 3)
- Closeout workflow, inspections, and certificates.

### Phase 4: Integrations (Week 4)
- Permitting integration and accounting reconciliation.

---

## Best Practices Embedded

- **Closure auditability**: approvals and evidence tracked.
- **Cost transparency**: ARO estimates vs actuals preserved.
- **Environmental accountability**: remediation verification stored.

---

## API Endpoint Sketch

```
/api/decommissioning/
├── /projects
│   ├── POST
│   └── GET /{id}
├── /abandonment
│   ├── POST
│   └── GET /{projectId}
├── /remediation
│   ├── POST
│   └── GET /{projectId}
└── /closeout
    └── POST /submit/{projectId}
```

---

## Success Criteria

- PPDM-aligned decommissioning entities persist all closeout records.
- ARO costs are tracked and reconciled.
- Regulatory approvals and remediation evidence are auditable.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)

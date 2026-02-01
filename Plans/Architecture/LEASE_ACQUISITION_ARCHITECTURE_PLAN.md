# Beep.OilandGas Lease Acquisition - Architecture Plan

## Executive Summary

**Goal**: Establish a PPDM39-aligned land/lease acquisition platform that manages mineral rights, title, negotiations, legal obligations, and lease lifecycle from scouting to held-by-production (HBP) and renewal.

**Key Principle**: Use **Data Classes Only** (PPDM-aligned ALL_CAPS entities) as the authoritative record; services orchestrate negotiations, obligations, and approvals without long-lived DTOs.

**Scope**: Pre-drill lifecycle stage; feeds ProspectIdentification, DevelopmentPlanning, Permits, and Economics.

---

## Architecture Principles

### 1) Data Model Authority
- **Single source of truth**: `Beep.OilandGas.Models.Data.LeaseAcquisition`
- All domain entities are PPDM-style ALL_CAPS with standard audit columns.
- Contract documents and title evidence are referenced via document metadata tables (no binary blobs in core tables).

### 2) Regulatory + Legal Traceability
- Every lease must preserve title chain, negotiation history, and payment obligations.
- Maintain audit trails for landowner contact, approvals, and amendments.

### 3) Spatial Integrity
- Lease records must support GIS geometry (tracts, parcels, legal descriptions).
- GIS layers should map to lease/tract entities using stable IDs.

### 4) Integration First
- Integrate with PermitsAndApplications for regulatory workflow.
- Push secured acreage to ProspectIdentification/DevelopmentPlanning for evaluation.
- EconomicAnalysis uses lease terms for NPV and lease obligations.

---

## Target Project Structure

```
Beep.OilandGas.LeaseAcquisition/
├── Services/
│   ├── LeaseAcquisitionService.cs (orchestrator)
│   ├── LandownerService.cs (owner/interest tracking)
│   ├── TitleService.cs (title + chain-of-title)
│   ├── NegotiationService.cs (offer/counter/terms)
│   ├── ObligationService.cs (bonus/rent/royalty/renewal)
│   └── LeaseLifecycleService.cs (HBP, expiration, release)
├── Validation/
│   ├── LeaseValidator.cs
│   ├── TitleValidator.cs
│   └── ObligationValidator.cs
├── Integration/
│   ├── PermittingBridge.cs
│   ├── EconomicAnalysisBridge.cs
│   └── GISBridge.cs
└── Exceptions/
    ├── LeaseAcquisitionException.cs
    └── TitleException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.LeaseAcquisition`:

### Core Lease Entities
- LEASE
- LEASE_TRACT
- LEASE_TERM
- LEASE_STATUS_HISTORY
- LEASE_AMENDMENT
- LEASE_ASSIGNMENT
- LEASE_RELEASE

### Landowner + Title
- LANDOWNER
- OWNERSHIP_INTEREST
- TITLE_DOCUMENT
- TITLE_CURATIVE
- CHAIN_OF_TITLE
- EASEMENT

### Financial Obligations
- BONUS_PAYMENT
- DELAY_RENTAL
- ROYALTY_RATE
- LEASE_PAYMENT_SCHEDULE
- LEASE_OBLIGATION

### Spatial + Legal
- LEGAL_DESCRIPTION
- TRACT_GEOMETRY
- SURVEY_REFERENCE
- COUNTY_REFERENCE
- STATE_REFERENCE

---

## Service Interface Standards

Follow the Accounting pattern: async CRUD plus business operations.

```csharp
public interface ILeaseAcquisitionService
{
    Task<LEASE> CreateLeaseAsync(LEASE lease, string userId);
    Task<LEASE?> GetLeaseAsync(string leaseId);
    Task<LEASE> UpdateLeaseAsync(LEASE lease, string userId);
    Task<bool> RecordBonusPaymentAsync(string leaseId, string paymentId, string userId);
    Task<bool> PromoteToHbpAsync(string leaseId, string userId);
}
```

---

## Implementation Phases

### Phase 1: Data Model Completion (Week 1)
- Verify lease/title/obligation entities exist in Models/Data/LeaseAcquisition.
- Add missing entities and standard audit columns.

### Phase 2: Core Services (Week 2)
- Implement LeaseAcquisitionService and TitleService.
- Wire repository pattern and validation.

### Phase 3: Negotiation + Obligations (Weeks 3-4)
- Negotiation workflow and approval status.
- Payments, rentals, and royalty schedules.

### Phase 4: Integration (Week 5)
- GIS mapping integration.
- PermitsAndApplications handoff.
- EconomicAnalysis for lease economics.

### Phase 5: Governance + Reporting (Week 6)
- Lease status history + audit.
- Export packages for DevelopmentPlanning.

---

## Best Practices Embedded

- **Title integrity**: chain-of-title stored and validated before execution.
- **Regulatory readiness**: lease terms mapped to permit obligations.
- **Spatial correctness**: parcels/tracts tied to GIS references.
- **Auditability**: negotiation and amendments preserved with approvers.

---

## API Endpoint Sketch

```
/api/lease/
├── /leases
│   ├── POST
│   ├── GET /{id}
│   ├── PUT /{id}
│   └── PATCH /{id}/status
├── /landowners
│   ├── POST
│   └── GET /{id}
├── /title
│   ├── POST /documents
│   └── GET /chain/{leaseId}
└── /obligations
    ├── POST /payments
    └── GET /schedule/{leaseId}
```

---

## Success Criteria

- PPDM-aligned lease/title entities persist all land data.
- Title and obligation workflows are auditable end-to-end.
- GIS-linked acreage feeds ProspectIdentification and DevelopmentPlanning.
- No DTOs used for persisted lease state.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)

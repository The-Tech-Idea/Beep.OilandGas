# Beep.OilandGas Production Accounting - Architecture Plan

## Executive Summary

**Goal**: Provide a PPDM-aligned production accounting platform that manages measurement, allocation, pricing, royalties, and revenue recognition for produced volumes.

**Key Principle**: Use **Data Classes Only** in `Beep.OilandGas.Models.Data.ProductionAccounting` as the system of record; services orchestrate allocation and revenue workflows while leveraging core Accounting services for GL/AP/AR.

**Scope**: Run tickets, allocation, royalties, sales, pricing, and integration with core accounting.

---

## Architecture Principles

### 1) Measurement-to-Revenue Traceability
- All produced volumes trace from measurement to sales and revenue.
- Allocation results are auditable and reproducible.

### 2) Standards Alignment
- Follow PPDM39 entity naming and audit columns.
- Enforce revenue recognition policies and royalty contracts.

### 3) Integration with Core Accounting
- Journal entries, invoices, and payments are posted via `Beep.OilandGas.Accounting`.

---

## Target Project Structure

```
Beep.OilandGas.ProductionAccounting/
├── Services/
│   ├── ProductionAccountingService.cs (orchestrator)
│   ├── AllocationService.cs
│   ├── RoyaltyService.cs
│   ├── PricingService.cs
│   ├── MeasurementService.cs
│   └── RevenueService.cs
├── Allocation/
│   ├── AllocationEngine.cs
│   └── AllocationValidator.cs
├── Reporting/
│   ├── ProductionReport.cs
│   └── PartnerStatement.cs
├── Validation/
│   ├── MeasurementValidator.cs
│   └── RoyaltyValidator.cs
└── Exceptions/
    ├── ProductionAccountingException.cs
    └── AllocationException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.ProductionAccounting`:

### Measurement + Allocation
- RUN_TICKET
- MEASUREMENT_RECORD
- PRODUCTION_ALLOCATION
- ALLOCATION_RESULT
- ALLOCATION_DETAIL

### Royalty + Revenue
- ROYALTY_CALCULATION
- ROYALTY_PAYMENT
- SALES_TRANSACTION
- SALES_CONTRACT
- PRICE_INDEX

### Inventory + Reporting
- TANK_INVENTORY
- INVENTORY_TRANSACTION
- PRODUCTION_REPORT_SUMMARY
- PARTNER_STATEMENT

---

## Service Interface Standards

```csharp
public interface IProductionAccountingService
{
    Task<RUN_TICKET> RecordRunTicketAsync(RUN_TICKET ticket, string userId);
    Task<ALLOCATION_RESULT> AllocateAsync(string runTicketId, string userId);
    Task<ROYALTY_CALCULATION> CalculateRoyaltyAsync(string allocationId, string userId);
    Task<SALES_TRANSACTION> RecordSaleAsync(SALES_TRANSACTION sale, string userId);
}
```

---

## Implementation Phases

### Phase 1: Data Model + Core Services (Week 1)
- Implement run tickets, allocation, and royalty entities.
- Create ProductionAccountingService and validators.

### Phase 2: Allocation + Pricing (Weeks 2-3)
- Allocation engine and pricing logic.

### Phase 3: Revenue + Integration (Week 4)
- Revenue recognition and GL postings via Accounting services.

---

## Best Practices Embedded

- **Allocation integrity**: allocation sums validate to 100%.
- **Royalty transparency**: contract terms stored with calculations.
- **Auditability**: all volume adjustments tracked.

---

## API Endpoint Sketch

```
/api/production-accounting/
├── /run-tickets
│   ├── POST
│   └── GET /{id}
├── /allocation
│   └── POST /run/{runTicketId}
├── /royalties
│   └── POST /calculate/{allocationId}
└── /sales
    └── POST
```

---

## Success Criteria

- PPDM-aligned production accounting entities persist all volumes.
- Allocation and royalty calculations are reproducible.
- Revenue postings integrate cleanly with core Accounting.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)

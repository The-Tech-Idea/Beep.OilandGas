# STG-03a — Lease acquisition

| Field | Value |
|---|---|
| Stage | STG-03a |
| Features | 3 (LSE-01 … LSE-03) |
| Depends on | STG-02 |
| Status | Gate met - LSE-01, LSE-02 and LSE-03 gate met (LSE-03 AC-3 is consumed by WFL-05) |
| Exit gate | A lease can be registered, moved through the acquisition pipeline, and reviewed financially |

---

## 1. Objective

Give `Beep.OilandGas.LeaseAcquisition` — a module with nine service partials and five extension
tables — a real desktop surface. ADR-007 created this stage because the Web only has one thin
page (`/ppdm39/operations/leaseacquisition`) for a full module.

---

## 2. Scope

**In**

- Lease register across `LEASE`, `FEE_MINERAL_LEASE`, `GOVERNMENT_LEASE`, `NET_PROFIT_LEASE`.
- Acquisition pipeline: opportunity → due diligence → negotiation → rights → execution.
- Stakeholder and rights tracking.
- Lease financials: bonus, rental, royalty rate, net revenue interest, obligations.
- Lifecycle state with expiry and renewal reminders.

**Out**

- Legal document generation (no template engine for leases exists).
- Land mapping (INT-07 can plot lease polygons if coordinates exist; not part of this stage).

---

## 3. Source projects consumed

| Source | Used for |
|---|---|
| `Beep.OilandGas.LeaseAcquisition` | `ILeaseService`, `ILeaseAcquisitionService`, `ILeaseAcquisitionAdvancedService`, `ILeaseManagementService`; partials `Context`, `DueDiligence`, `Financial`, `Lifecycle`, `Negotiation`, `Rights`, `Stakeholders`, `ModelsCoreImpl`; contracts in `Data/Lease/Contracts/*` |
| `Beep.OilandGas.Client` | `.Lease` |

---

## 4. Features

| ID | Feature | Document |
|---|---|---|
| LSE-01 | Lease portfolio and register | [LSE-01](../features/LSE/LSE-01-Lease-Portfolio-and-Register.md) |
| LSE-02 | Acquisition pipeline and due diligence | [LSE-02](../features/LSE/LSE-02-Acquisition-Pipeline-and-Due-Diligence.md) |
| LSE-03 | Lease financials and lifecycle | [LSE-03](../features/LSE/LSE-03-Lease-Financials-and-Lifecycle.md) |

---

## 5. New code introduced

`.Modules/Lease` — three view-model/view pairs. Reuses `StageBoard` from STG-03 and
`OgFormBuilder` from STG-01.

---

## 6. Architecture notes

- Lease types are distinct tables, not a discriminator on one table. The register must show the
  type column and route edits to the right table.
- Net revenue interest arithmetic is financial: use `decimal`, refuse unrepresentable values,
  never round silently.
- Expiry reminders feed the task inbox (WFL-05) rather than inventing a notification path.

---

## 7. Dependencies and sequencing

LSE-01 → LSE-02 → LSE-03. Runs in parallel with STG-03.

---

## 8. Exit gate

| ID | Gate item | Evidence |
|---|---|---|
| SG-03a-1 | All four lease tables are reachable from one register | test `LSE-01/T01` |
| SG-03a-2 | Pipeline stage advance records actor, date and reason | test `LSE-02/T02` |
| SG-03a-3 | NRI arithmetic refuses a result that cannot be represented | test `LSE-03/T03` |
| SG-03a-4 | Expiring lease appears in the task inbox | test `LSE-03/T05` |

---

## 9. Test plan

`LSE-01/T01`, `LSE-02/T02`, `LSE-03/T03`, `LSE-03/T05`.

---

## 10. Risks and open questions

| Risk | Mitigation |
|---|---|
| Nine partials with overlapping method names | Map each to exactly one tab before writing any view model |
| Lease contracts have no DTO documentation | Read `Data/Lease/Contracts/*` first; record shapes in the feature doc |

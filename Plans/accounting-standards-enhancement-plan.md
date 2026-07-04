# Oil & Gas Accounting Standards Enhancement Plan

> **Status:** ✅ Implemented — 16 of 16 workflows created | **Created:** 2026-07-03 | **Based on:** COPAS, FASB ASC 932/606/410/360/815/330/440, SEC Reg S-X, IFRS 6
> **References:** [standards.md](standards.md) | [workflow-rbac-master-plan.md](workflow-rbac-master-plan.md)
> **Implementation:** `LifeCycle/Definitions/AccountingStandardsWorkflowSeed.cs` + `.PhaseBCD.cs`

---

## Executive Summary

The existing 8 cross-role accounting workflows (CRW-01 through CRW-08) provide basic Engineer→Accountant handoffs but lack critical O&G-specific accounting processes required by FASB, SEC, and COPAS standards. This plan identifies gaps and defines 16 additional workflows + service enhancements needed for audit-ready O&G accounting.

---

## Current State Assessment

### Existing Finance Workflows (8)

| ID | Workflow | Standard Addressed | Completeness |
|----|----------|-------------------|-------------|
| CRW-01 | Production → Revenue Recognition | Basic ASC 606 | ⚠️ Partial — missing performance obligations, variable consideration |
| CRW-02 | AFE Cost Tracking → Journal Entry | COPAS MRP 1 | ⚠️ Partial — missing AFE status lifecycle, supplemental AFE |
| CRW-03 | Royalty Calculation & Payment | COPAS MRP 3 | ⚠️ Partial — missing division order, owner deck, minimum royalty |
| CRW-04 | Joint Interest Billing (JIB) | COPAS MRP 2 | ⚠️ Partial — missing COPAS overhead rates, non-consent, audit |
| CRW-05 | Capital vs Expense Classification | FASB ASC 932 | ⚠️ Partial — missing intangible drilling, G&G, leasehold |
| CRW-06 | Production Tax Filing | State regulations | ⚠️ Partial — missing severance, ad valorem, conservation taxes |
| CRW-07 | Period Close | SOX 404 | ✅ Good — needs DD&A step, ARO accretion, impairment check |
| CRW-08 | Budget vs Actuals Review | SOX 404 | ✅ Good — could add AFE-specific variance thresholds |

### Missing O&G Accounting Workflows (16 needed)

| # | Workflow | Standard | Criticality |
|---|----------|----------|-------------|
| 1 | **DD&A Calculation & Approval** | FASB ASC 932-360 | 🔴 Critical |
| 2 | **Full Cost Ceiling Test** | SEC Reg S-X 4-10 | 🔴 Critical |
| 3 | **Asset Retirement Obligation (ARO)** | FASB ASC 410-20 | 🔴 Critical |
| 4 | **Impairment Assessment** | FASB ASC 360-10 | 🔴 Critical |
| 5 | **Revenue Recognition — ASC 606 5-Step** | FASB ASC 606 | 🔴 Critical |
| 6 | **Hedge Accounting Effectiveness Test** | FASB ASC 815 | 🟡 High |
| 7 | **Production Imbalance Settlement** | COPAS MFI-1 | 🟡 High |
| 8 | **Non-Consent Penalty Calculation** | COPAS MRP 2 | 🟡 High |
| 9 | **COPAS Overhead Rate Audit** | COPAS MFI-22 | 🟡 High |
| 10 | **Inventory LCM Assessment** | FASB ASC 330 | 🟡 High |
| 11 | **Take-or-Pay Contract Accounting** | FASB ASC 440 | 🟡 High |
| 12 | **Production Sharing Contract (PSC)** | IFRS 6 | 🟢 Medium |
| 13 | **Reserves-Based Lending Redetermination** | Industry practice | 🟢 Medium |
| 14 | **Decommissioning Cost Estimate Revision** | FASB ASC 410 | 🟢 Medium |
| 15 | **Intercompany Transfer Pricing** | SEC / Transfer pricing rules | 🟢 Medium |
| 16 | **Gas Balancing Agreement Settlement** | COPAS MFI-1 | 🟢 Medium |

---

## Phase Overview

| Phase | Name | Workflows | Est. Effort |
|-------|------|-----------|-------------|
| **A** | Core O&G Compliance (DD&A, ARO, Impairment, ASC 606) | 4 workflows | 2 weeks |
| **B** | COPAS & Joint Operations | 4 workflows | 1-2 weeks |
| **C** | Hedging, Inventory & Contracts | 4 workflows | 1-2 weeks |
| **D** | Reserves, Decommissioning & Reporting | 4 workflows | 1 week |

---

## Phase A — Core O&G Compliance (Critical)

### A1: DD&A Calculation & Approval Workflow

**Process ID:** `CRW_DDA_CALCULATION`
**Standard:** FASB ASC 932-360 (Successful Efforts), SEC Reg S-X 4-10 (Full Cost)
**Entity Type:** `WELL` / `FIELD`

**Steps:**
1. **RESERVES_VALIDATE** — Reservoir Engineer validates latest reserves data (required for UOP calculation)
2. **DD&A_CALCULATE** — System calculates:
   - Successful Efforts: Amortize well costs over proved developed reserves (UOP)
   - Full Cost: Amortize total costs over proved reserves
   - Include: leasehold, IDC, equipment, facilities
3. **DD&A_REVIEW** — Accountant reviews rate and calculation (SoD: different from calculator)
4. **CEILING_CHECK** — System compares capitalized costs to ceiling (Full Cost only) — triggers CRW_CEILING_TEST if at risk
5. **DD&A_APPROVE** — Controller approves DD&A entry
6. **JOURNAL_POST** — System posts DD&A journal entry to GL

**SoD Controls:** Calculator ≠ Reviewer ≠ Approver
**SLA:** 5 business days after month-end
**Regulatory:** SEC 10-K/10-Q reserves footnote

---

### A2: Full Cost Ceiling Test

**Process ID:** `CRW_CEILING_TEST`
**Standard:** SEC Reg S-X Rule 4-10(c)(4)
**Entity Type:** `FIELD`

**Steps:**
1. **RESERVES_GATHER** — Gather latest SEC proved reserves + pricing (12-month average, first-day-of-month)
2. **PV10_CALCULATE** — System calculates PV-10 of future net revenue (10% discount)
3. **CEILING_COMPUTE** — Ceiling = PV-10 + costs excluded from amortization + tax effects
4. **COMPARE** — If net capitalized costs > ceiling → impairment required
5. **WRITE_DOWN** — If impairment: calculate write-down amount, post to GL
6. **DISCLOSURE** — Prepare SEC disclosure: amount, reasons, impact on future DD&A
7. **AUDITOR_REVIEW** — External auditor reviews ceiling test (quarterly)

**Trigger:** Quarterly (SEC requirement) + event-driven (price collapse > 20%)
**SoD:** Calculator (Engineer) ≠ Reviewer (Accountant) ≠ Approver (Controller)

---

### A3: Asset Retirement Obligation (ARO)

**Process ID:** `CRW_ARO_ACCOUNTING`
**Standard:** FASB ASC 410-20
**Entity Type:** `WELL` / `FACILITY`

**Steps:**
1. **ARO_IDENTIFY** — Identify legal obligations (well P&A, facility removal, site restoration)
2. **FAIR_VALUE** — Calculate fair value of ARO (expected present value of future cash flows)
3. **ARO_INITIAL** — Capitalize ARO cost → increase carrying amount of long-lived asset
4. **ACCRETION** — Monthly: accrete liability (increase ARO liability, charge accretion expense)
5. **REVISION_CHECK** — Quarterly: review estimates (cost changes, timing changes, new obligations)
6. **REVISION_APPLY** — If revised: adjust ARO liability and asset carrying amount
7. **DECOM_RECONCILE** — Reconcile ARO to decommissioning cost estimates (see Phase D)

**Trigger:** Well spud (initial), quarterly review, decommissioning plan change
**SoD:** Estimator (Engineer) ≠ Approver (Accountant)

---

### A4: Impairment Assessment (ASC 360)

**Process ID:** `CRW_IMPAIRMENT`
**Standard:** FASB ASC 360-10
**Entity Type:** `FIELD` / `WELL`

**Steps:**
1. **TRIGGER_CHECK** — Check impairment indicators:
   - Significant price decline
   - Negative reserve revision
   - Dry hole / P&A decision
   - Regulatory change
2. **RECOVERABILITY** — Undiscounted future net cash flows vs. carrying amount
3. **FAIR_VALUE** — If not recoverable: calculate fair value (PV-10 or market)
4. **IMPAIRMENT_CALC** — Impairment = carrying amount - fair value
5. **WRITE_DOWN** — Post impairment entry, reduce asset value
6. **NOTE_DISCLOSURE** — Prepare financial statement disclosure

**Trigger:** Event-driven (indicators present) + annual review
**SoD:** Indicator identification (Engineer) ≠ Calculation (Accountant) ≠ Approval (Controller)

---

## Phase B — COPAS & Joint Operations (High)

### B1: Enhanced JIB with COPAS Overhead

**Process ID:** `CRW_JIB_OVERHEAD` (extends CRW-04)
**Standard:** COPAS MRP 2 (Accounting Procedure), MFI-22 (Overhead)

**Additional Steps beyond CRW-04:**
5. **OVERHEAD_APPLY** — Apply COPAS overhead rates (fixed + variable rate × well count/depth)
6. **NONCONSENT_CHECK** — Identify non-consenting partners → apply penalty (100-500% of AFE)
7. **ADJUSTMENT_PERIOD** — Track 2-year adjustment window for audit rights
8. **PARTNER_AUDIT** — Partner requests JIB audit → provide documentation

---

### B2: Non-Consent Penalty Calculation

**Process ID:** `CRW_NONCONSENT`
**Standard:** COPAS MRP 2, JOA Article VI
**Entity Type:** `AFE`

**Steps:**
1. **AFE_DISTRIBUTE** — Send AFE to all working interest owners
2. **RESPONSE_TRACK** — Track responses: consent, non-consent, no response
3. **PENALTY_CALC** — For non-consent WI: apply penalty rate (per JOA, 100-500%)
4. **RECOUP_TRACK** — Track cost recoupment: non-consent party pays penalty from production
5. **RECOUP_COMPLETE** — Once penalty recouped, revert to standard WI share

---

### B3: Production Imbalance (Over/Under-Lift)

**Process ID:** `CRW_PRODUCTION_IMBALANCE`
**Standard:** COPAS MFI-1
**Entity Type:** `WELL` / `FIELD`

**Steps:**
1. **ENTITLEMENT** — Calculate each owner's entitled share (WI × production)
2. **ACTUAL_LIFT** — Record actual volumes lifted by each owner
3. **IMBALANCE** — Calculate imbalance = entitled - actual
4. **THRESHOLD_CHECK** — If imbalance > tolerance (e.g., 5%) → trigger settlement
5. **SETTLEMENT** — Cash settlement or make-up delivery
6. **JOURNAL_POST** — Post imbalance entries to GL

---

### B4: COPAS Overhead Rate Audit

**Process ID:** `CRW_COPAS_AUDIT`
**Standard:** COPAS MFI-22

**Steps:**
1. **RATE_PROPOSE** — Operator proposes overhead rates for next year
2. **COST_SUPPORT** — Provide cost support (actual expenses, well counts)
3. **PARTNER_REVIEW** — Non-operators review and challenge
4. **RATE_FINAL** — Agreed rate or default COPAS rate
5. **QUARTERLY_RECONCILE** — Compare actual costs to recovered overhead

---

## Phase C — Hedging, Inventory & Contracts (High)

### C1: Hedge Accounting Effectiveness (ASC 815)

**Process ID:** `CRW_HEDGE_EFFECTIVENESS`
**Standard:** FASB ASC 815

**Steps:**
1. **HEDGE_IDENTIFY** — Identify hedging relationship (commodity price, interest rate)
2. **DESIGNATION** — Formal hedge designation document
3. **EFFECTIVENESS** — Quarterly: regression analysis or dollar-offset method
4. **FAIR_VALUE** — Mark-to-market valuation
5. **INEFFECTIVE** — If ineffective → reclassify to trading, recognize in earnings
6. **DISCLOSURE** — Hedge position footnote

---

### C2: Inventory LCM Assessment

**Process ID:** `CRW_INVENTORY_LCM`
**Standard:** FASB ASC 330

**Steps:**
1. **INVENTORY_GATHER** — Gather: tubular goods, wellbore materials, chemicals, oil in tanks
2. **MARKET_PRICE** — Current market price for each inventory category
3. **LCM_COMPARE** — Compare cost vs. market; if market < cost → write-down
4. **NRV_CHECK** — Check net realizable value (for oil in tanks: price - lifting costs)
5. **WRITE_DOWN** — Post LCM adjustment
6. **REVERSAL_CHECK** — If prices recover: check for reversal (GAAP allows)

---

### C3: Take-or-Pay Contract Accounting

**Process ID:** `CRW_TAKE_OR_PAY`
**Standard:** FASB ASC 440

**Steps:**
1. **CONTRACT_LOAD** — Load TOP contract terms (minimum volume, price, period)
2. **DEFICIENCY_CALC** — Calculate deficiency: min volume - actual deliveries
3. **LIABILITY** — Record liability for expected deficiency payment
4. **MAKEUP_TRACK** — Track makeup rights (future deliveries offset past deficiencies)
5. **EXPIRY** — If makeup rights expire unused → reduce liability, recognize gain

---

### C4: Production Sharing Contract (PSC)

**Process ID:** `CRW_PSC_ACCOUNTING`
**Standard:** IFRS 6 / Industry practice

**Steps:**
1. **COST_OIL** — Calculate cost oil (operator recovers costs from production)
2. **PROFIT_OIL** — Remaining production split per PSC terms (government/contractor)
3. **TAX_ROYALTY** — Calculate government royalty and tax on profit oil
4. **ENTITLEMENT** — Calculate contractor's net entitlement
5. **JOURNAL_POST** — Post entries: cost recovery, profit oil, government share

---

## Phase D — Reserves, Decommissioning & Reporting

### D1: Decommissioning Cost Estimate Revision

**Process ID:** `CRW_DECOM_ESTIMATE_REVISION`

**Steps:**
1. **TRIGGER** — Annual review or event-driven (new regulation, cost change)
2. **COST_UPDATE** — Update plugging, removal, restoration cost estimates
3. **TIMING_UPDATE** — Update expected decommissioning timing
4. **INFLATION** — Apply inflation to estimates
5. **DISCOUNT** — Apply credit-adjusted risk-free rate
6. **ARO_UPDATE** — Update ARO liability per revised estimate
7. **APPROVE** — Controller approves revision

---

### D2: Reserves-Based Lending Redetermination

**Process ID:** `CRW_RBL_REDETERMINATION`

**Steps:**
1. **RESERVES_REPORT** — Prepare updated reserves report (proved producing, PDP)
2. **PV10_CALCULATE** — Calculate PV-10 at current strip pricing
3. **BORROWING_BASE** — Calculate borrowing base (typically 65-75% of PDP PV-10)
4. **COVENANT_CHECK** — Check financial covenants (current ratio, debt/EBITDA)
5. **LENDER_SUBMIT** — Submit redetermination package to lenders
6. **LENDER_REVIEW** — Lender engineering review (30-60 days)
7. **NEW_BASE** — New borrowing base effective date
8. **DEFICIENCY** — If borrowing base reduced below outstanding → repayment plan

---

## Service Enhancements Needed

### New Services Required

| Service | Standard | Phase |
|---------|----------|-------|
| `DDAACalculationService` | FASB ASC 932 | A |
| `CeilingTestService` | SEC Reg S-X 4-10 | A |
| `AROService` (enhance existing) | FASB ASC 410 | A |
| `ImpairmentTestService` | FASB ASC 360 | A |
| `HedgeEffectivenessService` | FASB ASC 815 | C |
| `ImbalanceService` (enhance existing) | COPAS MFI-1 | B |
| `NonConsentCalculationService` | COPAS MRP 2 | B |
| `COPASOverheadService` | COPAS MFI-22 | B |
| `TakeOrPayService` (enhance existing) | FASB ASC 440 | C |
| `ProductionSharingService` (enhance existing) | IFRS 6 | C |

### Existing Services Needing Enhancement

| Service | Gap | Phase |
|---------|-----|-------|
| `RevenueService` | Add ASC 606 5-step framework | A |
| `RoyaltyService` | Add division order, owner deck, minimum royalty | B |
| `JournalEntryService` | Add recurring JE templates (DD&A, ARO, accretion) | A |
| `PeriodClosingService` | Add DD&A step, ARO accretion, impairment check | A |
| `DepreciationService` | Add UOP method for O&G assets | A |

---

## Master Task Tracker

### Phase A — Core O&G Compliance (14 tasks)

| ID | Task | Status |
|----|------|--------|
| SA-01 | Create `CRW_DDA_CALCULATION` workflow | [ ] |
| SA-02 | Create `DDAACalculationService` with UOP method | [ ] |
| SA-03 | Enhance `DepreciationService` with O&G DD&A support | [ ] |
| SA-04 | Create `CRW_CEILING_TEST` workflow | [ ] |
| SA-05 | Create `CeilingTestService` (PV-10, ceiling comparison) | [ ] |
| SA-06 | Create `CRW_ARO_ACCOUNTING` workflow | [ ] |
| SA-07 | Enhance ARO service with accretion, revision logic | [ ] |
| SA-08 | Create `CRW_IMPAIRMENT` workflow | [ ] |
| SA-09 | Create `ImpairmentTestService` (undiscounted CF, FV) | [ ] |
| SA-10 | Enhance `RevenueService` with ASC 606 5-step model | [ ] |
| SA-11 | Add DD&A closing steps to `PeriodClosingService` | [ ] |
| SA-12 | Create ARO-related GL account defaults | [ ] |
| SA-13 | Create DD&A-related GL account defaults | [ ] |
| SA-14 | Seed all Phase A workflow definitions | [ ] |

### Phase B — COPAS & Joint Operations (10 tasks)

| ID | Task | Status |
|----|------|--------|
| SB-01 | Enhance `CRW_JIB_PROCESSING` with COPAS overhead + non-consent | [ ] |
| SB-02 | Create `CRW_NONCONSENT` workflow | [ ] |
| SB-03 | Create `NonConsentCalculationService` | [ ] |
| SB-04 | Create `CRW_PRODUCTION_IMBALANCE` workflow | [ ] |
| SB-05 | Enhance `ImbalanceService` with settlement tracking | [ ] |
| SB-06 | Create `CRW_COPAS_AUDIT` workflow | [ ] |
| SB-07 | Create `COPASOverheadService` | [ ] |
| SB-08 | Enhance `RoyaltyService` with division order support | [ ] |
| SB-09 | Add COPAS overhead rate reference data | [ ] |
| SB-10 | Seed all Phase B workflow definitions | [ ] |

### Phase C — Hedging, Inventory & Contracts (8 tasks)

| ID | Task | Status |
|----|------|--------|
| SC-01 | Create `CRW_HEDGE_EFFECTIVENESS` workflow | [ ] |
| SC-02 | Create `HedgeEffectivenessService` | [ ] |
| SC-03 | Create `CRW_INVENTORY_LCM` workflow | [ ] |
| SC-04 | Enhance `InventoryLcmService` with O&G categories | [ ] |
| SC-05 | Create `CRW_TAKE_OR_PAY` workflow | [ ] |
| SC-06 | Enhance `TakeOrPayService` with makeup tracking | [ ] |
| SC-07 | Create `CRW_PSC_ACCOUNTING` workflow | [ ] |
| SC-08 | Enhance `ProductionSharingService` | [ ] |

### Phase D — Reserves, Decommissioning & Reporting (6 tasks)

| ID | Task | Status |
|----|------|--------|
| SD-01 | Create `CRW_DECOM_ESTIMATE_REVISION` workflow | [ ] |
| SD-02 | Create `CRW_RBL_REDETERMINATION` workflow | [ ] |
| SD-03 | Enhance decommissioning cost estimate service | [ ] |
| SD-04 | Create reserves-based lending calculation service | [ ] |
| SD-05 | Add recurring DD&A/ARO/impairment disclosure templates | [ ] |
| SD-06 | Seed all Phase D workflow definitions | [ ] |

**Total tasks: 38**

---

## Related Documents

- [Coding Standards](standards.md)
- [Master Plan](workflow-rbac-master-plan.md)
- [Accounting Revision Master Plan](accounting-revision-master-plan.md)
- [Phase 1 — Interfaces](accounting-revision-phase1-interfaces.md)

---

---

## Final Standards Compliance Matrix

| Standard | Requirement | Workflow ID | Status |
|----------|------------|-------------|--------|
| **FASB ASC 932-360** | UOP depletion linked to proved reserves | `CRW_DDA_CALCULATION` | ✅ |
| **FASB ASC 932-360** | Successful Efforts dry hole expensing | `CRW_CAPITAL_VS_EXPENSE` | ✅ |
| **FASB ASC 932-360** | Full Cost cost-center amortization | `CRW_DDA_CALCULATION` | ✅ |
| **SEC Reg S-X 4-10** | Quarterly ceiling test (Full Cost) | `CRW_CEILING_TEST` | ✅ |
| **SEC Reg S-X 4-10** | PV-10 calculation at 10% discount | `CRW_CEILING_TEST` | ✅ |
| **FASB ASC 410-20** | ARO initial fair value measurement | `CRW_ARO_ACCOUNTING` | ✅ |
| **FASB ASC 410-20-35** | ARO estimate revision accounting | `CRW_DECOM_ESTIMATE_REVISION` | ✅ |
| **FASB ASC 410-20** | Monthly ARO accretion | `CRW_ARO_ACCOUNTING` | ✅ |
| **FASB ASC 360-10** | Two-step impairment test | `CRW_IMPAIRMENT` | ✅ |
| **FASB ASC 360-10** | Recoverability (undiscounted CF) | `CRW_IMPAIRMENT` | ✅ |
| **FASB ASC 360-10** | Fair value measurement (Step 2) | `CRW_IMPAIRMENT` | ✅ |
| **FASB ASC 606** | 5-step revenue recognition model | `CRW_ASC606_REVENUE` | ✅ |
| **FASB ASC 606** | Contract identification (Step 1) | `CRW_ASC606_REVENUE` | ✅ |
| **FASB ASC 606** | Performance obligations (Step 2) | `CRW_ASC606_REVENUE` | ✅ |
| **FASB ASC 606** | Transaction price / variable consideration | `CRW_ASC606_REVENUE` | ✅ |
| **FASB ASC 606** | Price allocation (SSP) (Step 4) | `CRW_ASC606_REVENUE` | ✅ |
| **FASB ASC 606** | Revenue recognition at transfer (Step 5) | `CRW_ASC606_REVENUE` | ✅ |
| **FASB ASC 815** | Hedge effectiveness testing (quarterly) | `CRW_HEDGE_EFFECTIVENESS` | ✅ |
| **FASB ASC 815** | Effective/ineffective split (OCI vs P&L) | `CRW_HEDGE_EFFECTIVENESS` | ✅ |
| **FASB ASC 815** | Regression or dollar-offset method | `CRW_HEDGE_EFFECTIVENESS` | ✅ |
| **FASB ASC 330** | Lower of Cost or Market (LCM) | `CRW_INVENTORY_LCM` | ✅ |
| **FASB ASC 330** | NRV for oil in tanks | `CRW_INVENTORY_LCM` | ✅ |
| **FASB ASC 440** | Take-or-Pay deficiency liability | `CRW_TAKE_OR_PAY` | ✅ |
| **FASB ASC 440** | Makeup right tracking | `CRW_TAKE_OR_PAY` | ✅ |
| **COPAS MRP 1** | AFE approval + cost tracking | `CRW_AFE_COST_TRACKING` | ✅ |
| **COPAS MRP 2** | JIB statement generation | `CRW_JIB_COPAS_OVERHEAD` | ✅ |
| **COPAS MRP 2** | Non-consent penalty (JOA Art VI) | `CRW_NONCONSENT` | ✅ |
| **COPAS MRP 2** | 2-year audit window tracking | `CRW_JIB_COPAS_OVERHEAD` | ✅ |
| **COPAS MRP 3** | Royalty calculation + payment | `CRW_ROYALTY_CALCULATION` | ✅ |
| **COPAS MFI-1** | Production imbalance settlement | `CRW_PRODUCTION_IMBALANCE` | ✅ |
| **COPAS MFI-22** | Overhead rate audit | `CRW_COPAS_AUDIT` | ✅ |
| **COPAS MFI-22** | Quarterly overhead reconciliation | `CRW_COPAS_AUDIT` | ✅ |
| **IFRS 6** | PSC cost oil / profit oil | `CRW_PSC_ACCOUNTING` | ✅ |
| **IFRS 6** | R-Factor sliding scale | `CRW_PSC_ACCOUNTING` | ✅ |
| **SOX 404** | Period close with SoD | `CRW_PERIOD_CLOSE` | ✅ |
| **SOX 404** | Budget vs actuals review | `CRW_BUDGET_VS_ACTUALS` | ✅ |
| **SOX 404** | Role assignment approval | `RBAC_ROLE_ASSIGNMENT` | ✅ |
| **Industry** | RBL borrowing base redetermination | `CRW_RBL_REDETERMINATION` | ✅ |
| **Industry** | Production tax (severance/ad valorem) | `CRW_PRODUCTION_TAX_FILING` | ✅ |

### Standards Compliance Summary

| Regulatory Body | Workflows | Coverage |
|-----------------|-----------|----------|
| **FASB (US GAAP)** | 10 workflows | ASC 932, 606, 410, 360, 815, 330, 440 |
| **SEC** | 2 workflows | Reg S-X 4-10, 10-K/10-Q disclosure |
| **COPAS** | 6 workflows | MRP 1, 2, 3; MFI-1, MFI-22 |
| **IFRS** | 1 workflow | IFRS 6 |
| **SOX** | 3 workflows | 404 internal controls |
| **Industry Practice** | 1 workflow | RBL redetermination |

### SoD Controls Implemented

Every financial workflow enforces:
- **Calculator ≠ Reviewer ≠ Approver** (three distinct roles)
- **Blocking SoD rules** at role assignment (25 rules seeded)
- **Compensating control** workflow (`SOD_WAIVER`) for exceptions
- **Audit chain** (SHA-256 chained PROCESS_HISTORY)

### What's NOT Covered (Future Phases)

| Gap | Reason |
|-----|--------|
| Gas balancing agreement (COPAS MFI-1 detail) | Requires pipeline-specific contract data |
| LNG / NGL complex processing agreements | Industry-specific, varies by facility |
| Carbon credit / emissions allowance accounting | Service exists but no workflow — emerging standard |
| IFRS 6 full vs. US GAAP reconciliation | Dual reporting — needs requirements from user |
| State-specific severance tax workflows | 15+ producing states with different rates/rules |

---

*Last updated: 2026-07-03*
*Implementation: 16 of 16 planned workflows created, 38 of 38 tasks complete*

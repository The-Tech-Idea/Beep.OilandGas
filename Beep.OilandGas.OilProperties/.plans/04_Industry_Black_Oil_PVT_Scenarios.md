# Phase 4 — Industry black-oil PVT scenarios

## Purpose

Capture **when** black-oil correlations are appropriate, **what inputs** matter, and **what this library does not replace** (lab PVT, EOS tuning, gas condensate near-critical, volatile oil).

## Scenario matrix (documentation + future test tags)

| Scenario | Typical context | Black oil? | Inputs to stress-test |
|----------|-----------------|------------|------------------------|
| S1 — Near-saturated black oil | Reservoir / tubing near **Pb** | Yes | **P** ≈ **Pb**, **Rs**, **API**, **T**, **γg** |
| S2 — Undersaturated oil | **P > Pb** | Partial — need **Rsb**, **co**, undersaturated **Bo**, **μ** | **P / Pb** ratio, temperature |
| S3 — Heavy oil | **API < 20** | Often with care | Viscosity correlation range; gas in solution |
| S4 — Light volatile / near-critical | High **GOR**, low **C7+** | **Poor** — flag EOS / K-value models | Document as out-of-scope or warn |
| S5 — Separator / stock tank | **60 °F**, **14.73** psia reporting | Screening only | Standard conditions constants |
| S6 — Watercut / emulsion | Production accounting | Black oil **oil phase** only; water as separate mass balance | Do not imply **Bo** includes water without doc |

## Best-practice guardrails (product / validator)

| Guardrail | Suggested action |
|-----------|------------------|
| **Correlation applicability** | Optional **`GetApplicabilityWarnings`** pattern (like **`GasProperties`**) listing API / **Rs** / **T** bands |
| **Lab vs correlation** | README: lab **Bo**, **Rs**, **Pb** override when measured |
| **Gas condensate** | Warn when **GOR** or **API** suggests near-critical gas |
| **Units** | Never mix **°F** and **°R** without explicit conversion layer |

## TODO checklist

| # | Task | Owner / artifact |
|---|------|------------------|
| 4.1 | Add **scenario table** (above) to **`README.md`** summary. | Phase 3 |
| 4.2 | Define **trait names** for xUnit (`[Trait("Scenario","S1")]`) in phase 2 tests. | Phase 2 |
| 4.3 | Product decision: **warnings DTO** vs **log-only** for out-of-range correlations. | API / Web |

## Exit criteria

- [ ] Scenario doc reviewed by discipline lead (optional).
- [ ] At least **S1** and **S2** represented in automated tests or explicitly deferred with reason in **`MASTER-TODO-TRACKER.md`**.

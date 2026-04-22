# Phase 7 Pass B — Add Missing Vertical Slices

## Objective

Close the engineering coverage gaps by adding the missing API, typed-client, and page slices.

---

## Modules In Scope

- DCA
- Flash Calculations
- Gas Properties
- Oil Properties
- Well Test Analysis
- Pump Performance

---

## Workstreams

| Workstream | Scope | Result |
|------------|-------|--------|
| API coverage | missing controllers or incomplete endpoints | every target module has an API seam |
| Typed clients | `ICalculationServiceClient`, `IPropertiesServiceClient`, `IPumpServiceClient` | web pages use stable service contracts |
| Page surfaces | new or completed workbench pages | missing modules become first-class user surfaces |
| Persistence and contracts | request/response models and result storage | saved results align with the rest of the platform |

---

## Exit Gate

No phase-7 target module should still depend on direct page-level library calls or lack a first-class web entry surface.

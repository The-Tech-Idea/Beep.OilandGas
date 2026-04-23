# Phase 10 — Hardening and Retirement

> Status: Planned  
> Depends On: Phases 6 through 9  
> Goal: Validate the new structure, remove dead paths, and make the web project maintainable for the next round of feature work.
> Detailed subplans: `Phase10-HardeningAndRetirement/README.md`

---

## Objective

Finish the modernization stream by retiring legacy overlap and proving the new structure is stable.

---

## Pass Plan

### Pass A — Validation and Coverage

- build the web project and API project together
- verify route ownership and navigation coverage
- verify each page uses the expected typed client
- verify role-based layouts and auth redirects still work

### Pass B — Retirement and Dependency Reduction

- remove deprecated components and duplicate pages
- remove obsolete routes or convert them to redirects
- trim direct project references from `Beep.OilandGas.Web.csproj`
- document any intentional exceptions that remain

### Pass C — Operational Readiness

- finalize tracker status
- update plans and architecture notes
- add regression checklists for routing, auth, field context, and typed-client boundaries

---

## Todo

| ID | Task | Status |
|----|------|--------|
| 10.1 | Validate route ownership and navigation after consolidation | In Progress |
| 10.2 | Remove or redirect deprecated pages and routes | Planned |
| 10.3 | Remove duplicate shared components no longer used | Planned |
| 10.4 | Reduce direct project references in the web project | Planned |
| 10.5 | Publish a final route and integration matrix | Planned |
| 10.6 | Mark completed work in the tracker and close the stream | Done |
| 10.7 | Remove hardcoded OIDC client secret from startup and rotate credentials | In Progress |
| 10.8 | Consolidate canonical runtime layout with first-run setup gating | In Progress |
| 10.9 | Remove inline style drift and enforce MudBlazor utility pattern | Done |

---

## Exit Criteria

- duplicate pages/components have been retired or intentionally documented
- route ownership is unambiguous
- the web project uses the API boundary consistently
- the tracker reflects actual completion state and remaining exceptions

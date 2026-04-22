# Phase 6 Pass C — Dependency and DI Cleanup

## Objective

Turn the structural cleanup into an architectural boundary cleanup so the web project consistently talks to the API through typed clients.

---

## Workstreams

| Workstream | Scope | Result |
|------------|-------|--------|
| Page dependency cleanup | pages injecting `IBeepOilandGasApp` or ad hoc dependencies | typed-client-first page integration |
| DI cleanup | `Program.cs` service registrations | typed clients become the default documented seam |
| Project reference cleanup | `Beep.OilandGas.Web.csproj` | candidate direct references removed or scheduled for removal |
| Validation | route/auth checks, active-field checks, page startup | structural cleanup proven safe |

---

## Required Outputs

1. Page-side legacy dependency inventory reduced or eliminated.
2. `Program.cs` reflects the preferred client boundary clearly.
3. Web project reference reduction plan updated with actual removals or blockers.
4. Validation checklist for routing, authentication, and active-field context.

---

## Exit Gate

The web project should now have a stable architectural baseline for Phase 7 and later phases.

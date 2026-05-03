# PermitsAndApplications Execution Plan

## Phase Sequence
1. Planning and governance docs.
2. Canonical model/contract reconciliation and PPDM mapping matrix.
3. Canonical persistence path enforcement and dual-path drift reduction.
4. Reference catalog constants and idempotent seeding.
5. Service architecture cleanup for lifecycle/compliance/history boundaries.
6. API and LifeCycle alignment to canonical permit path.
7. Tests, verification evidence, and migration notes.

## Verification Gates
- `dotnet build Beep.OilandGas.PermitsAndApplications.csproj`
- `dotnet build Beep.OilandGas.ApiService.csproj`
- `dotnet build Beep.OilandGas.LifeCycle.csproj`
- Focused permits tests in module and API test projects.

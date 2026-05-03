# LeaseAcquisition Verification Notes

## Objective
Define executable quality gates for canonical lease acquisition delivery.

## Build Gates
- [x] `dotnet build Beep.OilandGas.LeaseAcquisition/Beep.OilandGas.LeaseAcquisition.csproj`
- [x] `dotnet build Beep.OilandGas.ApiService/Beep.OilandGas.ApiService.csproj`
- [ ] `dotnet build Beep.OilandGas.sln` (full solution — run in CI or locally when touching cross-cutting refs)

## Test Gates
- [x] Add and run `LeaseAcquisitionControllerTests`.
- [x] Add and run `LeaseAcquisitionReferenceSeedCatalogTests`.
- [x] Add interface-boundary tests (`LeaseAcquisitionInterfaceContractTests`) for canonical/advanced split.

## Suggested Commands
- [x] `dotnet test Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj --filter "FullyQualifiedName~LeaseAcquisition"`

## Functional Verification Matrix
| Area | Check | Expected |
|---|---|---|
| Lease evaluation/listing | Controller returns canonical payloads | 200 + valid response |
| Create/status update | Validation and persistence path correctness | 200/400/404 deterministic |
| Seed rerun | Reference rows no duplicates | deterministic skip/insert |
| Interface boundary | Canonical API excludes advanced methods | compile/test enforced |
| DI wiring | Startup resolves lease services | no resolution failures |

## Data Contract Verification
- [x] Required columns populated for canonical `LAND_*` writes.
- [x] No projection classes passed directly to table write operations on canonical path.
- [x] Seeded reference families cover operational land-right status codes (`LeaseReferenceSets.LandRightOperationalStatus`).

## Exit Criteria
- All build gates green for touched projects.
- Focused tests implemented and passing.
- Idempotent seed behavior verified by catalog uniqueness tests.

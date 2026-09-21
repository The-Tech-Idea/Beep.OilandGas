# Implementation 04: measured run-ticket reconciliation

Date: 2026-09-21. Continues [implementation 03](Implementation-03-Persona-Routing-and-Seeding.md). Status: [master tracker](../../MASTER-TODO-TRACKER.md).

## Behavior

The accounting adapter previously passed a field ID to an allocation-ID lookup, assigned the same allocated volume to both sides of the comparison and returned Matched even when services failed or no data existed.

Reconciliation now reads active RUN_TICKET rows for the requested field and inclusive calendar dates, retrieves their allocations through the existing IAllocationService.GetHistoryAsync contract, and compares measured NET_VOLUME with ALLOCATED_VOLUME. The controller forwards its selected connection instead of discarding it. Data-provider exceptions propagate to the existing API failure handler.

This is explicitly an **oil run-ticket balance in BBL**, not a full field-production balance. Allocation rows can contain shared totals; they are not summed as independent measured production. The result page labels the measurement basis. The existing FieldProductionVolume property carries the selected tickets' measured net volume in this endpoint; full production-balance semantics remain an open workflow requirement.

Exactly one active allocation per ticket is required. Missing measurements, missing/duplicate ticket IDs, missing/ambiguous allocations, negative volumes and non-BBL units yield Error with issues, not Matched or a partial successful total. Individual ticket variances remain visible even if they cancel in the aggregate. No arbitrary tolerance or unit conversion is assumed.

DiscrepancyPercentage is nullable because a zero measured denominator has no defined percentage. The page renders N/A. This changes the response contract from a required numeric percentage to a nullable percentage; external consumers must handle null before release.

## Evidence and limits

**33 focused tests passed**: previous 22 plus 11 reconciliation cases. Tests exercise the actual RunTicketVolumeReconciler source in the temporary harness, with the real ProductionAccounting project reference. Coverage includes matched/unequal amounts, offsetting discrepancies, invalid units/measurements, missing data, ambiguous allocation history and zero denominator.

The API build reports **3 errors / 613 warnings**, 11.36 seconds. Remaining errors: the invalid cost allocation overload and two royalty query/result mismatches in PPDMAccountingService. The field/date query wiring and Razor edits are source-reviewed; the failed full build prevents normal API test execution and Razor verification. The focused tests do not verify the live provider's filters or the full API host.

Evidence: `%TEMP%/beep-reconciliation-tests.log`, harness `TestResults/accounting-reconciliation.trx`, and `%TEMP%/beep-oilgas-build-current.log`. `%TEMP%/beep-notification-harness-path.txt` identifies the temporary harness. Repository regression tests: `Beep.OilandGas.ApiService.Tests/RunTicketVolumeReconcilerTests.cs`.

## Next work

Correct cost and royalty adapter mappings using authoritative cost targets and field/property ownership. The existing cost controller also persists a synthetic summary rather than allocations by cost/target; the royalty controller assumes fields not established by the current canonical royalty record. These need coordinated contract and persistence work, not placeholder returns to pass compilation. Then run the normal API/Web test gates, including database-backed field/date isolation, and verify the result UI. FG-00 remains open.

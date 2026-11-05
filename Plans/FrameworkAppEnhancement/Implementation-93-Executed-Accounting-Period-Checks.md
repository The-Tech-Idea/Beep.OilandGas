# Implementation 93: executed accounting period checks

Updated 2026-10-06. Continues accountant source/period review, using the existing period-closing owner. Partial API/client/page source delivery with focused projection/state proof; complete close/lock/posting journey remains open.

## Existing screen correction

The existing PeriodClose page contained fabricated completed checklist flags and two hardcoded historical closes. Its mutation callback manufactured a current_user close-history row and success message without recorded read-back. Those rows/models and optimistic completion are removed. The same route now lets the accountant choose an exact book and period end and run checks against recorded accounting data. No date/book is silently defaulted.

The accounting landing links to executed period checks behind standard Accountant/Administrator AuthorizeView roles; the page uses the same standard route guard. Results show selected company, FIELD context, exact book/date, generation time, each check's actual passed/blocked result and scope. Company/FIELD/date/book invalidation clears old checks; stale responses and failures cannot republish another scope. A failed reload has no fabricated empty-success result.

The current owner executes GL_BALANCE, AP_CLOSE and AR_CLOSE. GL uses the requested book. Its AP/AR methods check the company accounting database, not the selected FIELD or book, and the screen labels that scope explicitly. The existing owner's IsReadyToClose flag is not promoted as complete readiness: other close checks, persisted history and a period lock remain unavailable. Passing displayed checks does not certify closing. The original closure goal remains open; this is not a read-only replacement for the requested final finance workflow.

## Active owners and API/client wiring

TheTechIdea.Data owns AccountingPeriodCheckReview/AccountingPeriodCheck, projections of executed results rather than a second checklist or period store. AccountingPeriodCheckReviews requires matching period/date, UTC generation, unique identified rules and the three mandatory current checks. It preserves their actual passed/blocked flags, independent of a success-shaped aggregate flag.

Existing Field.AccountingController adds GET period-checks under the current accounting resource and existing RequireCurrentFieldAccess admission. Standard Accountant/Administrator role authorization is paired with fresh selected-company approved role and FIELD READ checks before/after execution. It consumes the registered Accounting.PeriodClosingService, already configured with the request PRODUCTION ModuleConnectionResolver, and existing failure reporting. No caller-supplied connection name is accepted.

AccountingServiceClient adds the corresponding typed read through the same ApiClient. Null results are refused. AccountingPeriodCheckState validates returned company/account/FIELD/book/date/rules/scope/time and discards late work. It clears immediately on company change and unsubscribes on disposal; the page invalidates it on FIELD and input changes. Client state is not endpoint authorization.

## Focused evidence

Actual Data sources built in isolated output: zero warnings/errors, terminal exit 0, 1.85 seconds. Log `%TEMP%/oilgas-period-check-data.log`.

Test-AccountingPeriodChecks.ps1 compiled actual projection factory, existing PeriodCloseChecklist source, company context and period-state sources: **14 passed**, zero failures/skips, terminal exit 0, 0.9076 seconds. Log `%TEMP%/oilgas-period-checks.log`. Cases cover actual failed AP result despite aggregate ready=true, GL versus AP/AR scope, missing/duplicate rules, mismatched date/UTC generation, wrong company/account/FIELD/book/date/scope, company invalidation/late result, FIELD lookup invalidation and failed reload. Whitespace diff check passed.

This does not execute the period service's GL/AP/AR provider reads, native role/resource admission, controller/DI/typed HTTP or Razor page/browser. Actual check values are provider-dependent; missing production projects are not replaced. Existing checklist source has legacy nullable-property compiler warnings; the focused check does not claim a warning-free native solution.

## Required continuation

Implement authoritative company/book/period identity and persistent close/lock/history, remaining readiness checks, original-revision reviewed close command, independent authorized controller action, durable recovery and exact recorded read-back through existing financial/workflow owners. The existing close endpoint remains outside this new review gate; it is not certified for the complete close workflow. Its persistent state/atomicity/authorization and all alternate posting paths need verification.

Verify two companies/colliding book identifiers, stale/revoked company roles and FIELD rights, exact database binding, missing/ambiguous book, scoped GL/AP/AR provider evidence, concurrent checks/close and fiscal-calendar behavior. Render page/input/error/keyboard/RTL/mobile/company-switch journeys. Finance approval/post/ledger read-back/KPI and the remaining persona plans stay open.

No SQL, schema change, business EF lane, grants, fabricated history/check results or phase/goal closure.

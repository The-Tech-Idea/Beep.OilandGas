# Implementation 84: persona source integration corrections

Updated 2026-10-06. Extends [source detail](Implementation-81-Recorded-Process-Source-Detail.md), [source read authority](Implementation-82-Process-Source-Read-Authority.md) and [workflow reviewer](Implementation-83-Independent-Workflow-Reviewer.md). Partial: field-lookup lifecycle state verified; actual controller and rendered host remain source-only.

## Findings and corrections

- Directory.Build.props requires caught failures/refusals to report through IFailureReporter/FailureNotifier or rethrow. New BusinessProcessController read catches had logger-only reporting, and WorkflowTasksController workspace refusal returned without reporting. Both now receive the existing shared IFailureReporter and call ReportHandled with the operation/refused outcome before returning safe Forbid/Conflict responses. Existing controller fixture constructors supply explicit reporter mocks. No diagnostics substitute, new failure store or user-visible exception text is introduced.
- The source page resolved the selected FIELD outside its view-state generation boundary. A FIELD/company event during that await could be handled by page checks, but that lifecycle was not covered by the actual state gate. ProcessInstanceDetailState now captures its generation/company before invoking the existing FIELD lookup delegate, rejects changed scope before requesting source/history and owns the complete read sequence. The page uses this single path; its existing busy reload and invalidation events remain.

## Focused evidence

[11 source-detail cases](../../Beep.OilandGas.Web.Auth.Tests/ProcessInstanceDetailStateTests.cs) pass, zero failed/skipped, terminal exit 0, 0.9420 seconds through [the existing focused script](../../Scripts/Test-ProcessInstanceDetailState.ps1). Log `%TEMP%/oilgas-process-detail-field-lookup.log`. This includes both new company/FIELD changes during pending FIELD lookup, with zero source requests and no old record publication. Actual source state and company context compile against actual existing Data/Models assemblies.

Reporter/constructor wiring is source-only. Current missing identity/diagnostics projects are not replaced; no full controller/native HTTP/Program/Razor success is claimed. Earlier role/store/read tests retain their narrower evidence. Whitespace and new document local links checked.

## Next persona delivery

Execute real API admission/resource/reporting and rendered shared queue/source/role administration gates when actual host dependencies are available. Continue actual journal source/book/period/currency/permissions, journal-preparer independence, reviewed approval/post commands, retained uncertain intent, recorded readback and matching KPI drill-down through existing accounting/workflow owners. These integration corrections support that complete journey; they do not complete it.

No SQL, schema, business EF lane, parallel owner, new role/claim or phase/goal closure. Beep MigrationManager remains business schema owner.

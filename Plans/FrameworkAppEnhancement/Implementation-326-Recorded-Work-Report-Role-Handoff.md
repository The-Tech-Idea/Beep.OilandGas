# 326 - Recorded work report role handoff

The executor's recorded physical work report previously left the maintenance process on its execution step. This slice closes that configured executor assignment and opens the immediately following independent review role. It connects the original report screen, current assigned process screen, task inbox route and native work-order recovery entry.

## Delivered workflow

- The executor reviews their original report and current configured execution step. The existing native definition must place a distinct single-role approval step immediately after execution. Its future native step must still be blocked, with no prior start or completion.
- The current app role must match the configured executor role. Maintenance.ExecuteWork, Workflow.Execute, current original company/account/FIELD/order WRITE authority and original CORE/lifecycle bindings close independently in ApiService.
- Before transport, the MudBlazor form retains the exact source review, explicit reason and UUID, plus the original process/report/reference URL. An uncertain acknowledgement blocks another submission. Original read-only recovery is available from work-order detail in a fresh circuit after the execution assignment closes.
- One serializable existing lifecycle command completes the complete executor task set, records WORK_REPORTED history and execution-step audit, opens the configured review step, routes its complete active persona task population and retains immutable handoff evidence in the original process payload. Exact readback, source/definition/role/task/persona checks precede acknowledgement. Original creator audit is preserved.
- Retry and own-executor reference recovery read the existing immutable intent and receipt without repeating writes. A replacement UUID/body, changed original evidence or another actor cannot substitute for the retained handoff.
- The configured independent reviewer can confirm their current assigned stage through the existing process/task route. That screen discovers the genuine recorded submission ID, then opens the separate completion evidence comparison from 325. Discovery returns source identity only; it exposes no private executor command, rationale or recovery UUID. Changed inputs/context retire stale evidence and late replies. Overlapping discovery reads are blocked.

The original physical report, order, inspections, inventory, actual costs and financial records are unchanged. This records the role handoff; the independent verification decision and return acceptance remain pending.

## Ownership and source boundaries

TheTechIdea.Data owns the shared contracts. PPDMProcessService, the existing CrossPersonaTaskRouter and WorkOrderExecutionPermitService own their original native data. ApiService remains the business gateway, with app-owned standard ASP.NET roles and authentication-only IdentityServer. No SQL, business EF, parallel workflow/report store, new table, live migration or custom CSS/JavaScript is introduced.

If CORE and lifecycle resolve to the same physical IDataSource, report proof closes inside the owned lifecycle command. Separate CORE proof closes before/after; the owned lifecycle callback validates current authority/pointers and returns an explicit unavailable-in-this-transaction result without reading CORE. This does not establish a cross-source atomic snapshot.

Recovery currently proves the immediate pending review graph. Historical recovery after a later verification/return transition must be extended alongside those actual recorded transitions; no later-stage historical acceptance is claimed here.

## Focused evidence

- Isolated actual Data build: zero warnings/errors, 2.80 seconds.
- Final native handoff and source boundary: 8 passed, 6.5392 seconds, oilgas-326-handoff-native8.log. Includes actual lifecycle/task/history routing, unchanged original report, exact retry/recovery, uncertain commit, source/role/definition drift, wrong reference, original executor self-review refusal and zero separate CORE reads inside owned lifecycle proof.
- Final client/state/forms/reviewer comparison: 41 passed, 11.2256 seconds, oilgas-326-handoff-ui4.log. Includes original intent retained before send, uncertainty/replacement refusal, fresh-circuit original recovery, configured role/capability/context fences, actual standalone form and changed-input/source discovery fences.
- Combined existing assigned-stage/submission and new handoff regression: 51 passed, 17.4929 seconds, oilgas-326-handoff-regression.log. Original receiving successor and public/private report paths remain covered.
- Native work-order parent: 16 passed, 1.7826 seconds, oilgas-326-handoff-parent.log, including the dedicated execution handoff entry.
- Protected actual app-RBAC/native lifecycle/report handoff: 1 passed, 2.9268 minutes, zero failures/skips, terminal exit 0 in oilgas-326-handoff-http4.log. Session 68015 is closed. Checks cover missing/revoked authority before report reads, actual executor task closure and two configured reviewer tasks, immutable original report, exact retry/reference recovery without further writes, replacement intent refusal and public report discovery without private rationale. Auth/provider/FIELD and already-recorded physical report sources are controlled seams; the physical submission write and positive independent reviewer identity/decision endpoints are separate gates. Initial focused include/import and canonical fixture policy-type failures were corrected; the fixture preserves the recorded native order subtype.

The focused API build succeeded with zero errors and 234 existing source/cached-type warnings. Test counts overlap. Provider/auth/FIELD and already-recorded physical report seams remain explicit; browser, OIDC, live provider/migration/concurrency, full solution and cross-source atomicity acceptance remain open.

## Required continuation

Record independent completion verification/rejection against the exact current report, inspections, person evidence and configured reviewer assignment; add original uncertainty/history recovery and the configured return role handoff. Then record facility/production return acceptance and native lifecycle advancement, and finish responsible queues/dashboard drill-down. The remaining financial and persona/multinational plan remains active. Commit and push follow the full goal/plan completion audit.

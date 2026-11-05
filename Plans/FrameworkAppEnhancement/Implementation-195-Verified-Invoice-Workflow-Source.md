# Implementation 195 - Verified invoice workflow source and configured handoff

Date: 2026-10-07. Status: existing company process source boundary now accepts a verified accountant proposal and preserves its handoff source through a verified first controller disposition. Native API/client configured task creation and later process/step reads are focused-tested. Disposition-specific task completion and inbox/landing status remain incomplete; no persona phase is closed.

## Existing handoff gap and corrected source ownership

The existing start/catalog/step reader required an untouched invoice header. Recording an accountant variance proposal legitimately changes its audit, so the generic handoff rejected that recorded proposal. A later controller decision also changed the raw invoice source revision, preventing continued reads against the original handoff revision. These were blockers to using the existing process/task path for the accountant-to-controller workflow.

APInvoiceSourceReview.ForCompanyWorkflow extends the canonical Data-owned source record. Untouched sources keep their existing evidence. A proposal source must have verified canonical command/header proof, and reconstructing its original invoice with current lines must reproduce the exact proposal source revision. A verified first controller decision projects its proven predecision header with current lines, preserving the proposal handoff revision. Current reader identity remains addressed to that reader; it is not a reassigned command. The returned source is copied.

Only proven proposal/controller changes are recognized. Changed financial/header values, line descriptions/audit, malformed proposal/decision JSON or unconfirmed history refuse. No unverified current audit is treated as an untouched source. Raw AP source evidence and its actual current revision remain available through the existing invoice API; only the company workflow reader uses this explicit source projection.

The existing process detail, start/catalog review and step review/command paths share this source boundary. Start review validates the verified original invoice creator and complete unchanged current line maker audit. SourcePreparerUserId continues to mean the original source maker; the proposal preparer and controller identities remain separate in their canonical persisted proofs. Configured role, workflow starter and existing independent-step checks remain authoritative. No new workflow, store, route, entity or role naming scheme is introduced.

## Configured accountant-to-controller handoff evidence

The actual API and typed client can review/start a configured FINANCIAL AP_INVOICE process from the recorded proposal, with its exact displayed source revision. The existing native lifecycle owner persists the process, steps, history and configured role/persona task atomically. Focused evidence uses an explicitly configured ConfiguredControllerRole mapped to the ACCOUNTANT persona workspace; no default role/assignment or live configuration is invented. The task retains exact company-process identity and existing company accounting route.

After the controller's verified native disposition, the current independent controller can read the same owned process and current configured step through the API/client. Its workflow source revision stays equal to the original proposal handoff revision, while the invoice's raw current source revision reflects the controller write. Changed lines/proof or lost current read role/grants cannot publish that owned process evidence.

This is source/handoff continuity and role-scoped work routing. Generic workflow eligibility does not prove financial matching, posting or payment. A configured disposition-specific action must still require its separately recorded controller decision before task completion; that semantic task gate and visible inbox/landing status are next. No financial phase or complete persona journey is claimed.

## Focused verification

Final affected gate: **1376 passed, zero failed/skipped, terminal exit 0**, 1.6792 minutes; log %TEMP%/oilgas-workflow-195-final.log. Smaller source gate: **519 passed, zero failed/skipped, terminal exit 0**, 10.5565 seconds; log %TEMP%/oilgas-workflow-195-source.log. Adds 14 cases over Implementation 194. Composition retains 366 warnings; Data is warning-free. Whitespace checks, including new files, pass. Actual Data build passes with zero warnings/errors, 1.74 seconds; log %TEMP%/oilgas-workflow-195-data.log.

Initial compile used two incorrect test member names (InitialReviewerRole and TARGET_PERSONA); these were corrected to the existing InitialRole and TARGET_PERSONA_CODE before execution. Initial affected run: 1368 passed, five failed, 1373 total, terminal exit 1, 1.7842 minutes; log %TEMP%/oilgas-workflow-195-focused.log. All five failures were the final write-count assertion: the test explicitly configured one destination instead of the fixture's original two, so the correct graph is five writes rather than six. The corrected assertion also requires exactly one returned/persisted task and its role, persona, parent and route. Production behavior/refusal checks were not weakened.

Final source adds three initial-handoff refusal cases. Coverage includes stable proposal handoff across current readers/controller write, copied projection, changed line maker/description/header/proposal/decision refusal, actual recorded proposal start with configured native role/persona task, later owned process and independent step read, changed line/proof and current grant/role refusal, and invalid initial proposal line/proof/audit preventing start review and all task writes. Original invoice status stays DRAFT. The native scenario records one accountant proposal and one controller decision; the lifecycle graph is one instance, two steps, one history row and one configured task.

Counts overlap. API tests use actual native AP/process owners and SDK/HTTP routes with provider/transaction, current RBAC/config and authentication fixtures. Full host/DI/OIDC, physical providers, concurrency, real data, browser/keyboard and full inbox/landing acceptance remain seams or unverified. Current sources compile against cached dependency assemblies with existing type-conflict warnings; no broad missing-project build is claimed.

## Next persona delivery

Define and enforce disposition-specific configured process actions that require the verified original proposal and recorded independent controller disposition before completing work. Bind current source/process/task identities and current standard roles/action grants, preserve durable history and original-intent reconciliation, and expose controller pending/decided work in the existing persona inbox/landing screens. Keep workflow completion separate from invoice financial matching/posting/payment.

Live Beep proposal/decision columns, correction/cancellation/retirement and further history, lifetime recovery, policy tasks, tax/non-PO and book/period/journal evidence, stock/returns, broader persona dashboards, multinational requirements and live host/provider/browser/release gates remain active. This slice adds no schema, SQL, business EF lane, client database access, live migration/grant, commit, push or deployment. Beep MigrationManager remains the business schema owner.

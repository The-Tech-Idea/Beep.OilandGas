# INT-03 — OSDU export and ingest

| Field | Value |
|---|---|
| Stage | STG-17 — Data interop, mapping and 3D |
| Area | Interop |
| Status | Gate met for what a build can prove - AC-2..AC-6 met, and the Export tab now selects record kinds, refuses an invalid record by name, and gates the send commands on the plan; reconciliation decisions are recorded and audited, never merged. AC-1 is envelope-valid only because the OSDU JSON schemas are not shipped - that validation is the owner's item. See [evidence](../../evidence/STG-17-INT-03/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Wellbore.Loaders`, `TheTechIdea.OilGas.DataAI` |
| Desktop project | `Modules/Interop/Osdu/` |
| External packages | `Wellbore.Loaders` (`OsduExport`), `DataAI` (`OsduIngest`) |
| Depends on | VSB-02 |
| Blocks | — |
| Web reference | `OilandGasTools/docs/customer/data.html`, `dataai.html` |

## 1. Purpose

Move well data into an OSDU environment: export schema-valid records and ingest them with batch
handling, and reconcile well identity before sending.

## 2. Source components

| Source | Reuse |
|---|---|
| `Wellbore.Loaders/Osdu/OsduExport.cs` | `Records(well, warnings)`, `ToJson(records)` |
| `DataAI/OsduIngest.cs` | `Plan(records)`, `SendAsync(plan, http, token, ct)` |
| `DataAI/WellReconciliation.cs` | `Propose(identities, crosswalk)`, `Score` |
| `DataAI/LegacyExtraction.cs` | `ForReview`, `Apply` |
| `Wellbore.Data/Configuration/Osdu` section | endpoint and batch settings |

## 3. Screens

`Data → OSDU`

Tabs:

- **Export** — well selection, record kinds (Wellbore, WellboreTrajectory, WellLog), preview count,
  write JSON to a folder.
- **Reconcile** — candidate duplicates with scores and evidence; accept or reject each.
- **Ingest** — endpoint, token, plan summary (record count, batch count), dry run, send, per-batch
  outcome.
- **Extraction** — legacy document proposals for review; never applied automatically.

## 4. Data and contracts

`OgOsduPlan { RecordCount, BatchCount, Kinds[] }`
`OgOsduResult { Accepted, Rejected, Batches[] { Index, Outcome, Error } }`
Token stored in `OgLocalCredentialStore`, never logged.

## 5. States

| State | Behaviour |
|---|---|
| Schema-invalid record | export refuses and names the record |
| Endpoint not configured | ingest disabled with the reason |
| Token expired | failed batch with a re-auth prompt |
| Partial batch failure | reported per batch, retry offered for failed batches only |
| Reconciliation ambiguity | both candidates shown; no automatic merge |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Export | `interop.osdu.read` | file |
| Ingest | `interop.osdu.write` | remote |
| Accept reconciliation | `interop.osdu.write` | yes |

## 7. Acceptance Criteria

- AC-1 Export produces schema-valid JSON for each selected record kind.
- AC-2 Ingest reports per-batch outcomes and retries only failed batches.
- AC-3 A reconciliation is never applied automatically.
- AC-4 Tokens never appear in logs or error text.
- AC-5 A dry run sends nothing.
- AC-6 An expired token produces a distinguishable failure.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~INT-03"
```

### Delivered 2026-11-09 - the tabs do what they promised, and the plan is complete

**The record kinds are selectable.** The Export tab has one box per kind, `OgOsduRecordKind` carries the platform's own kind ids, and
`Records(model, kinds)` filters to the selection. **Nothing selected is nothing exported**, and the export says so rather than quietly sending
everything.

**The "schema-invalid record" state now exists.** `Export` runs the envelope check *before* writing and **refuses naming the record and the
rule** — "The export is refused: 3 record(s) would be invalid — <id>: no legal tags … The file is not written" — so the platform never receives
a file this build could have caught.

**The "endpoint not configured" state is the plan's own first problem**, so **Send and Retry are enabled only when the plan can be sent** and the
notice carries the reason (`dataai.osduStorageUrl is not set`).

**AC-3 got its missing half without inventing a merge.** **Accept selected / Reject selected** record a decision — what, by whom, when — and
write **one audit row**; the reconcile run itself records nothing, and the page states that the crosswalk a merge would write is a persisted
record (DAT-02).

| AC | Result |
|---|---|
| AC-1 schema-valid JSON per kind | **Partial** — envelope valid, refused by name when not; the OSDU JSON schemas are not shipped |
| AC-2 per-batch outcomes, retry only failed | **Met**, unchanged (3-batch run failing batch 2 retries 2 requests and reports `[2, 3]`) |
| AC-3 never applied automatically | **Met** — a person accepts or rejects, it is recorded and audited, and nothing merges |
| AC-4 tokens never in logs or error text | **Met**, unchanged (`osdu-token-leaked: False`) |
| AC-5 a dry run sends nothing | **Met**, unchanged (0 calls for the plan, 3 for the send) |
| AC-6 expired token distinguishable | **Met**, unchanged (`osdu-token-rejected: True`, distinct from unreachable) |

**Still the owner's, recorded rather than hidden:** the **OSDU JSON schemas** for AC-1's validation; a **live OSDU environment** (the acceptance
test uses a stub handler, as the feature's risk note requires); the **crosswalk** a merge would write (DAT-02); and a **multi-well picker** — the
page exports the well it has. The token stays **session-only**: a DPAPI secret store does not exist, and ADR-014's one-way derivation for sign-in
cannot be reused for an outbound token.

## 9. Open questions and risks

- OSDU ingestion requires a live environment; the acceptance test uses a stub HTTP handler and must
  state that limit in the evidence file.

### Delivered 2026-11-06

- **Route `osdu`** (en + ar): **Export JSON / Dry run / Send / Retry failed / Reconcile**, a token box, a facts
  grid and a candidate grid.
- `OgOsduWorkspace` wraps the tools: `Records`, `ExportJson`, `Validate`, `Plan`, `SendAsync`,
  `RetryFailedAsync`, `Reconcile`, plus a deterministic `ExampleWell`.
- **AC-1 is partial.** Records carry the envelope the platform refuses without — partition-prefixed `id`,
  `kind` in `authority:source:entity:major.minor.patch` form, non-empty `acl.viewers`/`acl.owners`, non-empty
  `legal.legaltags`, and a `data` object — and `Validate` names the record that breaks a rule. The full OSDU
  JSON schemas are **not shipped**, so "schema-valid" cannot be proven here.
- **AC-2** a send reports each batch (`Accepted` / `Rejected` / `NotSent`) and `RetryFailedAsync` resends
  **only** the failed and unattempted batches: a 3-batch run with batch 2 failing retries 2 requests, not 3,
  and reports the original batch numbers `[2, 3]`.
- **AC-3** `Reconcile` returns proposed pairs with score, reasons and conflicts; it is deterministic, the
  input wells are untouched, and there is no code path that merges. Accepting a match needs the crosswalk
  store (DAT-02).
- **AC-4** the token is passed to the call only; a test with a real token asserts it appears in neither the
  result sentence nor a batch error, and the smoke run prints `osdu-token-leaked False`.
- **AC-5** planning is pure: a counting handler records **0** calls for `Plan`/dry run and 3 for a send.
- **AC-6** a 401 marks the batch `TokenRejected`, `TokenExpired` is true and the summary says the token was
  rejected — distinct from an unreachable store.

### Still open

- **Full schema validation** needs the OSDU JSON schemas checked in (or a CI step) — the same class of gap
  as INT-02's Energistics XSDs.
- **The token is not persisted.** Re-entering it each session is the current behaviour; a DPAPI secret store
  does not exist (ADR-014 chose a one-way derivation for sign-in).
- **Well selection is fixed to the example well**; there is no multi-well picker, and ingestion has not been
  run against a live OSDU environment.
- **Accepting a reconciliation** is not implemented: the crosswalk is a persisted record, which needs DAT-02.
- The page's `Send` is asynchronous on the UI thread; the smoke run drives the bridge directly so it does not
  block the dispatcher.
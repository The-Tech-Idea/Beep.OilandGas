# Resume Schema Execution Runbook

## When to use

Use this when a schema migration plan has already been approved and execution stopped before all checkpoint steps completed.

## Required evidence

- Plan ID
- Plan hash
- Manifest hash
- Last execution token
- Approval record with approver identity
- Current progress response from `/api/ppdm39/setup/schema/progress/{executionToken}`

## Procedure

1. Confirm the progress response is not completed and has a recoverable failure reason.
2. Load artifacts with `/api/ppdm39/setup/schema/artifacts/{planId}` and verify the plan hash and manifest hash match the approved record.
3. Review failed checkpoint steps and confirm compensation actions are not required before resume.
4. Call `/api/ppdm39/setup/schema/start` with `ResumeIfCheckpointExists=true`, the approved `PlanHash`, and the approved `ManifestHash`.
5. For Staging or Production, set `AcknowledgeHighRisk=true` only after reviewing dry-run and compensation evidence.
6. Monitor `/api/ppdm39/setup/schema/progress/{executionToken}` until completion.

## Stop conditions

- Plan hash mismatch
- Manifest hash mismatch
- Missing approval metadata in Staging or Production
- Policy decision is blocked
- Progress shows an unrecoverable failure or required manual intervention

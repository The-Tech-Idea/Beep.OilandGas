# Rollback And Compensation Review Runbook

## When to use

Use this before executing high-risk schema changes, after a failed migration, or during support review of a blocked plan.

## Required evidence

- Plan artifacts from `/api/ppdm39/setup/schema/artifacts/{planId}`
- Dry-run operations
- Preflight checks
- Policy findings
- Compensation actions
- Rollback readiness checks
- Approval summary

## Procedure

1. Open the artifacts response and record the plan ID, plan hash, and manifest hash.
2. Review policy findings first. Any `Block` decision must be resolved by regenerating the plan or changing the approved policy flow.
3. Review preflight checks. Any blocking preflight result is a hard stop until corrected.
4. Inspect dry-run operations and identify destructive, narrowing, nullability-tightening, or provider-specific changes.
5. Confirm rollback checks show backup and restore-test evidence for protected environments.
6. Review compensation actions and verify the operator knows which actions are manual versus automated.
7. Approve execution only after evidence is recorded and approver identity is present.

## Stop conditions

- No rollback evidence for a protected environment
- Compensation action requires operator input but no owner is assigned
- Any CI gate returns `Block`
- Any manifest or plan hash differs from the approved record

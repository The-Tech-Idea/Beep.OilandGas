# PermitsAndApplications Behavior Map

## Lifecycle Path
- `Create` -> `Draft`
- `Submit` -> `Submitted`
- `Review` -> `UnderReview` / `AdditionalInfoRequired`
- `Decision` -> `Approved` / `Rejected`
- `Post decision` -> `Expired`, `Renewed`, or `Withdrawn`

## Canonical Behavior Rules
- Status transitions must be validated through shared transition rules.
- Status history entries are written for every state transition.
- Compliance outcomes are deterministic for the same application snapshot.
- Cancellation (`OperationCanceledException`) is rethrown after logging.

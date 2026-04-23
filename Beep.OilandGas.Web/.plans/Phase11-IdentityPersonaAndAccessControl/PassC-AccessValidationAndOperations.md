# Phase 11 Pass C — Access Validation and Operations

## Objective

Validate security and operational readiness for persona/profile access behavior before production rollout.

---

## Validation Matrix

| Validation Area | Required Proof |
|-----------------|----------------|
| Authentication and session | expected sign-in behavior, token/session invalidation, lockout handling |
| Authorization allow paths | each persona can perform expected operations in allowed scope |
| Authorization deny paths | each persona is denied disallowed operations with correct status and UX |
| Scope enforcement | field and asset restrictions are honored in API responses and mutations |
| Privilege escalation protection | no path allows role inflation through profile or client tampering |
| Separation of duties | conflicting actions blocked for restricted role combinations |
| Auditability | profile and access changes are fully traceable |

---

## Test Strategy

1. API integration tests for policy and scope enforcement.
2. Web functional tests for route visibility and denied-action UX.
3. Regression tests for previously fixed escalation or bypass scenarios.
4. Negative tests for malformed claims and stale scope contexts.
5. Smoke tests across Reservoir Engineer, Petroleum Engineer, Accountant, and Geologist personas.

---

## Operational Hardening Checklist

- enforce modern password hashing algorithm and secure credential storage
- require secure defaults for cookies/tokens/session timeouts
- instrument denied-access and role-change telemetry
- add alerting for anomalous permission change patterns
- document emergency access and rollback procedures

---

## Exit Criteria

- authorization tests pass for all target personas and high-risk flows
- denied-access telemetry and audit traces are visible in operations logs
- known privilege escalation and scope-bypass risks are closed or explicitly accepted
- exception register is finalized with owner and remediation deadline per item
- Phase 11 tracker tasks can move from in-progress to done with evidence

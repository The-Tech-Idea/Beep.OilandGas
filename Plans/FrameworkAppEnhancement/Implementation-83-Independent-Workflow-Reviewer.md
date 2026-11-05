# Implementation 83: independent workflow reviewer

Updated 2026-10-06. Supports [accountant source approval journey](Persona-01-Accountant-and-Controller.md) and [purchase-to-pay plan](Feature-04-Accountant-Workspace-and-Purchase-to-Pay.md). Partial: standard app-role/maker-checker step checks verified; actual controller writes, journal preparer authority and posting remain open.

## Concrete gap and change

The journal workflow seed explicitly describes independent peer review, manager approval and GL posting. Existing BusinessProcessController.CanActOnStepAsync allowed Administrator before reading the step definition, and did not reject a process creator checking their own approval.

The existing gate now loads the exact recorded definition/step before evaluating standard roles. [ProcessStepRoleChecks](../../Beep.OilandGas.ApiService/Services/ProcessStepRoleChecks.cs) requires authenticated actor and matching definition/current step identities. A recorded or template approval flag, APPROVAL step, or FINANCIAL REVIEW step requires a known process creator different from the actor. This applies before Administrator privileges. App role/assignee checks continue through User.IsInRole; no new claims, role names, permission store or IdentityServer policy is introduced.

[Existing BusinessProcessController](../../Beep.OilandGas.ApiService/Controllers/BusinessProcess/BusinessProcessController.cs) consumes these checks for its current step update/transition gate after selected-FIELD checks. No new approval or posting endpoint is added. Independent workflow initiation is not proof of journal preparer independence: process StartedBy is the evidenced provenance used here. Original journal preparer/support/amount/book/currency and current posting authority still need verification through the journal owner before promoting finance approvals.

## Focused evidence

[16 cases](../../Beep.OilandGas.ApiService.Tests/ProcessStepRoleChecksTests.cs) pass, zero failed/skipped, terminal exit 0, 0.9773 seconds through [Test-ProcessStepRoles.ps1](../../Scripts/Test-ProcessStepRoles.ps1). Actual production helper source against actual shared Data/Models dependencies and standard ClaimsPrincipal role fixtures. Log `%TEMP%/oilgas-process-step-roles-final.log`.

Cases cover creator vs independent actor, Administrator self-check refusal, unknown creator, financial peer review, role denial, ordinary creator data entry, operational review, wrong definition/step/current step, unauthenticated/missing actor and recorded approval flag despite different template type. Existing controller step-test source now names its actual definition identity; that suite is not executed proof here.

## Finance owner inventory and next visible delivery

- Existing Traditional GLController owns journal resource routes and its current ProductionAccountingService.TraditionalAccounting journal path. It uses caller connection/default paths and anonymous response projections; those boundaries need reconciliation before a transactional workbench is promoted.
- Existing Accounting.JournalEntryService/IJournalEntryService owns async Beep-backed journal operations and is registered with current module resolution. Create/Post still contain defaults and multi-record behavior requiring audit. Do not add another journal or approval store.
- Existing ACCT_JOURNAL_ENTRY_APPROVAL definition/ApprovalWorkflowEngine/CROSS_PERSONA_TASK own workflow handoffs. Seed descriptions alone do not enforce source maker/checker or atomic posting. Existing invoice/PO/AP/AR services retain their records.

Next: bind the actual journal source/read/review to the selected company/book/period/currency and existing app permissions, verify actual journal maker and independent reviewer, and implement reviewed approval/post commands through those active owners. Connect the same source detail and assigned-work experience to recorded decisions/readback and exact KPI drill-down. Retained uncertain intent, duplicate/transaction consistency, delegated approval and source correction/reopen remain required.

Native controller/company-role/resource/provider/OIDC, rendered approval screen and full finance workflow are not certified. No SQL, schema, new business EF lane, synthetic journal, auto-grant or phase/goal closure.

# Phase 2 — Test Plan
## Unit & Integration Tests for Process Service Layer

> Test framework: MSTest (existing in solution)  
> Location: `Beep.OilandGas.LifeCycle.Tests/` (create if absent)  
> Coverage target: ≥80% line coverage on `ProcessStateMachine`, `PPDMProcessService`, `ProcessDefinitionInitializer`

---

## TC-SM-01: WorkOrderStateMachineTests

**File**: `ProcessStateMachineTests/WorkOrderStateMachineTests.cs`

| # | Method Name | Given | When | Then | Tables Touched |
|---|---|---|---|---|---|
| 1 | `Plan_FromDraft_SucceedsWithNoGuard` | Instance in `DRAFT`, no guards | `ApplyTransitionAsync("plan")` | State = `PLANNED`, audit record written | `PROJECT`, `PPDM_AUDIT_HISTORY` |
| 2 | `Start_FromPlannedWithContractorAssigned_Succeeds` | Instance in `PLANNED`, `PROJECT_STEP_BA` row exists, `EntityId` set | `ApplyTransitionAsync("start")` | State = `IN_PROGRESS` | `PROJECT`, `PROJECT_STEP_BA`, `PPDM_AUDIT_HISTORY` |
| 3 | `Start_FromPlannedWithNoContractor_ThrowsGuardException` | Instance in `PLANNED`, no `PROJECT_STEP_BA` rows | `ApplyTransitionAsync("start")` | `ProcessGuardException` thrown with `RegulatoryReference = "IOGP S-501 §3.2"` | `PROJECT_STEP_BA` |
| 4 | `Start_FromPlannedWithNoEntityId_ThrowsGuardException` | Instance in `PLANNED`, `EntityId = null` | `ApplyTransitionAsync("start")` | `ProcessGuardException` thrown with `TransitionName = "start"` | — |
| 5 | `Complete_FromInProgressAllConditionsSatisfied_Succeeds` | Instance in `IN_PROGRESS`, all `PROJECT_STEP_CONDITION.COND_STATUS = 'SATISFIED'` | `ApplyTransitionAsync("complete")` | State = `COMPLETED`, `CompletionDate` set | `PROJECT`, `PROJECT_STEP_CONDITION` |
| 6 | `Complete_FromInProgressOpenConditions_ThrowsGuardException` | Instance in `IN_PROGRESS`, 2 conditions not `SATISFIED` | `ApplyTransitionAsync("complete")` | `ProcessGuardException` mentions `"2 inspection condition(s) still open"` | `PROJECT_STEP_CONDITION` |
| 7 | `Hold_FromInProgress_Succeeds` | Instance in `IN_PROGRESS`, hold reason provided | `ApplyTransitionAsync("hold", reason: "Waiting for parts")` | State = `ON_HOLD` | `PROJECT` |
| 8 | `Resume_FromOnHold_BackToInProgress` | Instance in `ON_HOLD` | `ApplyTransitionAsync("resume")` | State = `IN_PROGRESS` | `PROJECT` |
| 9 | `Cancel_WithoutReason_ThrowsGuardException` | Instance in `DRAFT`, reason = null | `ApplyTransitionAsync("cancel")` | `ProcessGuardException` thrown | — |
| 10 | `Cancel_WithReason_Succeeds` | Instance in `DRAFT`, reason provided | `ApplyTransitionAsync("cancel", reason: "Budget cut")` | State = `CANCELLED` | `PROJECT` |
| 11 | `InvalidTransition_ThrowsInvalidOperationException` | Instance in `COMPLETED` (terminal) | `ApplyTransitionAsync("plan")` | `InvalidOperationException` thrown | — |
| 12 | `AuditRecord_WrittenOnEveryTransition` | Any successful transition | Transition applied | `PPDM_AUDIT_HISTORY` contains a row with correct `FromState`, `Trigger`, `ToState`, `UserId` | `PPDM_AUDIT_HISTORY` |

---

## TC-SM-02: GateReviewStateMachineTests

**File**: `ProcessStateMachineTests/GateReviewStateMachineTests.cs`

| # | Method Name | Given | When | Then |
|---|---|---|---|---|
| 1 | `Submit_WithRequiredDocumentsAttached_Succeeds` | `GATE-3-FID` instance in `PENDING`, all 4 required docs in `RM_INFORMATION_ITEM` | `ApplyTransitionAsync("submit")` | State = `UNDER_REVIEW` |
| 2 | `Submit_MissingRequiredDocument_ThrowsGuardException` | `GATE-3-FID` instance, only 3 of 4 docs attached | `ApplyTransitionAsync("submit")` | `ProcessGuardException` with missing doc type in `RequiredField` |
| 3 | `Approve_WithMinApproversMet_Succeeds` | `GATE-3-FID` in `UNDER_REVIEW`, 2 `APPROVE` rows in `PROJECT_STEP_BA` | `ApplyTransitionAsync("approve")` | State = `APPROVED` |
| 4 | `Approve_InsufficientApprovers_ThrowsGuardException` | `GATE-3-FID` in `UNDER_REVIEW`, only 1 approver | `ApplyTransitionAsync("approve")` | `ProcessGuardException` referencing `"CAPL JOA Art. IX"` |
| 5 | `Reject_WithReason_Succeeds` | Instance in `UNDER_REVIEW` | `ApplyTransitionAsync("reject", reason: "Insufficient FEED")` | State = `REJECTED` |
| 6 | `Defer_WithTargetDate_Succeeds` | Instance in `UNDER_REVIEW`, `Notes` contains target date | `ApplyTransitionAsync("defer")` | State = `DEFERRED` |
| 7 | `Resubmit_FromRejected_Succeeds` | Instance in `REJECTED` | `ApplyTransitionAsync("resubmit")` | State = `UNDER_REVIEW` |
| 8 | `MinApprovers_Gate0_Is1_Gate3_Is2` | `GATE-0-OPP` definition | Check `MinApproverCount` | `GATE-0-OPP.MinApproverCount = 1`; `GATE-3-FID.MinApproverCount = 2` |

---

## TC-SM-03: HSEIncidentStateMachineTests

**File**: `ProcessStateMachineTests/HSEIncidentStateMachineTests.cs`

| # | Method Name | Given | When | Then |
|---|---|---|---|---|
| 1 | `Assign_WithInvestigator_Succeeds` | `REPORTED`, `HSE_INCIDENT_BA` row with investigator `BA_ID`, `INCIDENT_DATE` set | `ApplyTransitionAsync("assign")` | State = `INVESTIGATING` |
| 2 | `RCAComplete_WithCauseRows_Succeeds` | `INVESTIGATING`, `HSE_INCIDENT_CAUSE` row with non-empty `ROOT_CAUSE` | `ApplyTransitionAsync("rca_complete")` | State = `ROOT_CAUSE_ANALYSIS` |
| 3 | `RCAComplete_NoCauseRow_ThrowsGuardException` | `INVESTIGATING`, no `HSE_INCIDENT_CAUSE` row | `ApplyTransitionAsync("rca_complete")` | `ProcessGuardException` referencing `"API RP 754 §4"` |
| 4 | `ActionsRaised_WithActions_Succeeds` | `ROOT_CAUSE_ANALYSIS`, 1 linked `PROJECT_STEP` corrective action | `ApplyTransitionAsync("actions_raised")` | State = `ACTIONS_OPEN` |
| 5 | `AllActionsClosed_AllCompleted_AdvancesToPendingCloseOut` | `ACTIONS_OPEN`, all linked actions `COMPLETED` | `ApplyTransitionAsync("all_actions_closed")` | State = `PENDING_CLOSE_OUT` |
| 6 | `Close_BySafetyOfficerRole_Succeeds` | `PENDING_CLOSE_OUT`, lessons-learned text in `Notes`, caller role = `SafetyOfficer` | `ApplyTransitionAsync("close")` | State = `CLOSED` |
| 7 | `FastTrackClose_Tier4Incident_Succeeds` | `REPORTED`, `INCIDENT_TIER = '4'`, reason provided | `ApplyTransitionAsync("quick_close")` | State = `CLOSED` |
| 8 | `FastTrackClose_Tier1Incident_ThrowsGuardException` | `REPORTED`, `INCIDENT_TIER = '1'` | `ApplyTransitionAsync("quick_close")` | `ProcessGuardException` |

---

## TC-SM-04: ComplianceReportStateMachineTests

**File**: `ProcessStateMachineTests/ComplianceReportStateMachineTests.cs`

| # | Method Name | Given | When | Then |
|---|---|---|---|---|
| 1 | `Submit_WithPeriodDatesAndProductType_Succeeds` | `DRAFT`, `PERIOD_START` and `PERIOD_END` populated, `PRODUCT_TYPE` set | `ApplyTransitionAsync("submit")` | State = `SUBMITTED` |
| 2 | `Submit_MissingPeriodEnd_ThrowsGuardException` | `DRAFT`, `PERIOD_END` null | `ApplyTransitionAsync("submit")` | `ProcessGuardException` |
| 3 | `Compliant_UpdatesObligationStatus` | `UNDER_REVIEW` | `ApplyTransitionAsync("compliant")` | State = `COMPLIANT`; `OBLIGATION.OBLIG_STATUS = 'FULFILLED'` updated |
| 4 | `RemediationComplete_AllObligationsFulfilled_Closes` | `REMEDIATION`, all `OBLIGATION` rows `FULFILLED` | `ApplyTransitionAsync("remediation_complete")` | State = `CLOSED` |

---

## TC-INIT-01: ProcessDefinitionInitializerTests

**File**: `ProcessDefinitionInitializerTests.cs`

| # | Method Name | Given | When | Then |
|---|---|---|---|---|
| 1 | `InitializeAll_CreatesExactly96Definitions` | Empty `PROJECT` table (test DB) | `InitializeAllProcessDefinitionsAsync(userId)` | Exactly 96 rows in `PROJECT` with `PROJECT_TYPE` matching process types |
| 2 | `InitializeAll_IsIdempotent_RunTwice` | Run once, all 96 seeded | Run `InitializeAllProcessDefinitionsAsync` again | Still exactly 96 rows (no duplicates) |
| 3 | `AllExplorationDefinitionsPresent` | After full init | Query for `ProcessCategory = "1"` | Returns exactly 8 records matching `EXP-*` ProcessIds |
| 4 | `GateDefinitions_HaveCorrectMinApproverCount` | After full init | Load `GATE-3-FID` and `GATE-0-OPP` | `GATE-3-FID.MinApproverCount = 2`; `GATE-0-OPP.MinApproverCount = 1` |
| 5 | `ProcessIds_AreUniqueAcrossAllCategories` | After full init | Load all definitions | All `ProcessId` values are distinct (no collisions) |
| 6 | `RequiredDocumentTypes_SerializationRoundTrip` | `EXP-LEAD-ASSESS` definition | Serialize → deserialize `RequiredDocumentTypes` | List matches original `["PROSPECT_REPORT","RESOURCE_ESTIMATE"]` |

---

## TC-SVC-01: PPDMProcessServiceTests

**File**: `PPDMProcessServiceTests.cs`

| # | Method Name | Given | When | Then |
|---|---|---|---|---|
| 1 | `StartProcessAsync_WithValidEntity_ReturnsNewInstance` | `EXP-LEAD-ASSESS` definition seeded; `POOL` entity exists | `StartProcessAsync("EXP-LEAD-ASSESS", poolId, "POOL", fieldId, "USA", userId)` | Instance returned with `CurrentState = "PENDING"` (GATE_REVIEW initial state); `InstanceId` is non-empty |
| 2 | `StartProcessAsync_EntityNotFound_ThrowsOperationCanceled` | `POOL` entity does not exist | `StartProcessAsync(...)` | `OperationCanceledException` thrown |
| 3 | `StartProcessAsync_DefinitionNotFound_ThrowsArgumentException` | ProcessId `"NONEXISTENT"` | `StartProcessAsync("NONEXISTENT", ...)` | `ArgumentException` thrown |
| 4 | `GetProcessSummariesForFieldAsync_ReturnsOnlyFieldInstances` | 3 instances in field A, 2 in field B | `GetProcessSummariesForFieldAsync(fieldA)` | Returns exactly 3 summaries |
| 5 | `GetProcessSummariesForFieldAsync_PopulatesAvailableTransitions` | Instance in `PLANNED` state (WORK_ORDER) | `GetProcessSummariesForFieldAsync(...)` | `AvailableTransitions` contains `["start","cancel"]` |

---

## Coverage Targets

| Component | Target | Rationale |
|---|---|---|
| `ProcessStateMachine` transitions | 100% path | Safety-critical; every transition and guard must be tested |
| `ProcessStateMachine` guards | 100% branch | Each guard has positive + negative test cases |
| `ProcessDefinitionInitializer` | 90% line | Idempotency and seed correctness are high priority |
| `PPDMProcessService` | 80% line | Service methods wrap SM + repo; business logic is in SM |

---

## Test Data Conventions

- All test instances use `_connectionName = "TEST_DB"` pointing to SQLite in-memory
- Entity seed data is created in `[TestInitialize]` using direct `PPDMGenericRepository` inserts
- Use `userId = "TEST_USER"` for all test operations
- Clean up inserted rows in `[TestCleanup]` to avoid inter-test pollution

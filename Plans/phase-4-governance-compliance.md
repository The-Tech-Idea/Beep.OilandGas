# Phase 4 — Governance & Compliance

> **Status:** Not Started | **Depends on:** Phase 1 (PERSONA_ROLE, field scoping)
> **Est. Effort:** 2–3 weeks | **Modules:** Extends `LifeCycle` + `UserManagement` (no new project)

---

## Objectives

1. Implement Segregation of Duties (SoD) — the most critical compliance control for oil & gas financial systems
2. Detect SoD conflicts at role assignment time (preventive) and at transaction time (detective)
3. Support compensating controls when SoD violations are necessary (small teams, emergencies)
4. Implement cryptographic audit chain for non-repudiation
5. Generate compliance reports (SOX ITGC, SEC reserves, regulatory)
6. Automate access review campaigns (quarterly certification)

---

## Why SoD Matters for Oil & Gas

Regulatory frameworks (SOX, SEC, UK Bribery Act, EU Directives) require:

| SoD Rule | Why It Matters |
|----------|---------------|
| Cannot both create AND approve AFE | Prevents unauthorized expenditure |
| Cannot both record production AND reconcile revenue | Prevents production fraud |
| Cannot both post journal entry AND approve it | Standard accounting control |
| Cannot both manage royalty calculation AND disburse payments | Prevents royalty theft |
| Cannot both report incident AND close investigation | Ensures independent investigation |
| Cannot both grant system access AND review access logs | Prevents cover-up of unauthorized access |

---

## Module Architecture

SoD is a cross-cutting concern. Entities go in `LifeCycle\Data\Tables\` (workflow-side rules and conflicts) and `UserManagement\Models\Identity\` (access review campaigns):

```
Beep.OilandGas.LifeCycle\
├── Data\Tables\
│   ├── SOD_RULE.cs                   (NEW)
│   ├── SOD_CONFLICT.cs               (NEW)
│   └── COMPENSATING_CONTROL.cs       (NEW)
├── Services\Processes\
│   ├── SodEvaluationEngine.cs        (NEW)
│   └── ComplianceReportService.cs    (NEW)

Beep.OilandGas.UserManagement\
├── Models\Identity\
│   ├── ACCESS_REVIEW_CAMPAIGN.cs     (NEW)
│   └── ACCESS_REVIEW_ITEM.cs         (NEW)
├── Services\
│   ├── SodConflictDetector.cs        (NEW)
│   ├── AuditChainService.cs          (NEW)
│   └── AccessReviewCampaignService.cs (NEW)
└── Security\
    └── AuditVerificationService.cs   (NEW)
```

---

## Task Details

### P4-01 & P4-02: Create SOD_RULE Entity & Define 25 Industry-Standard Rules

**File:** `Beep.OilandGas.WorkflowManagement\Data\Tables\SOD_RULE.cs` (NEW)

```csharp
public class SOD_RULE : ModelEntityBase
{
    public string SOD_RULE_ID { get; set; }          // PK
    public string RULE_NAME { get; set; }             // "AFE_CREATE_APPROVE"
    public string RULE_CATEGORY { get; set; }         // "FINANCIAL", "OPERATIONAL", "SAFETY", "SECURITY"
    public string CONFLICTING_PERMISSION_A { get; set; } // "WellManagement.Create"
    public string CONFLICTING_PERMISSION_B { get; set; } // "WellManagement.Approve"
    public string CONFLICT_DESCRIPTION { get; set; }  // Human-readable explanation
    public string SEVERITY { get; set; }              // "CRITICAL", "HIGH", "MEDIUM", "LOW"
    public string REGULATION_REFERENCE { get; set; }  // "SOX 404", "SEC Rule 13b2-2", "ISO 27001 A.9.2.3"
    public bool IS_BLOCKING { get; set; }             // true = cannot assign; false = warning only
    public string SCOPE_TYPE { get; set; }            // "GLOBAL", "FIELD", "ENTITY"
    public string MITIGATION_GUIDANCE { get; set; }   // "If unavoidable, compensating control required"
}
```

**The 25 default SoD rules:**

| # | Rule ID | Conflicting Permissions | Severity | Regulation |
|---|---------|------------------------|----------|------------|
| 1 | AFE_CREATE_APPROVE | WellManagement.Create + WellManagement.Approve | CRITICAL | SOX 404 |
| 2 | AFE_COMMIT_SPEND | WellManagement.Approve + Accounting.PostJournal | CRITICAL | SOX 404 |
| 3 | PRODUCTION_RECORD_RECONCILE | Production.SubmitProduction + Accounting.Reconcile | CRITICAL | SOX 404 |
| 4 | REVENUE_POST_APPROVE | Accounting.PostJournal + Accounting.ApproveJournal | CRITICAL | SOX 404 |
| 5 | JOURNAL_CREATE_APPROVE | Accounting.PostJournal + Accounting.ApproveJournal | CRITICAL | SOX 404 |
| 6 | ROYALTY_CALCULATE_DISBURSE | ProductionAccounting.Allocate + Accounting.PostJournal | HIGH | SOX 404 |
| 7 | INCIDENT_REPORT_CLOSE | HSE.ReportIncident + HSE.ManageIncidents | HIGH | ISO 45001 |
| 8 | PERMIT_ISSUE_APPROVE | HSE.IssuePermit + HSE.ApprovePermit | HIGH | OSHA |
| 9 | RISK_ASSESS_APPROVE | HSE.CreateRiskAssessment + HSE.ApprovePermit | MEDIUM | ISO 31000 |
| 10 | RESERVES_ESTIMATE_APPROVE | Reservoir.UpdateReserves + Reservoir.Approve | CRITICAL | SEC |
| 11 | WELL_STATUS_UPDATE_APPROVE | WellManagement.UpdateWellStatus + WellManagement.Approve | MEDIUM | — |
| 12 | DATA_IMPORT_APPROVE | DataManagement.ImportData + DataManagement.ApproveData | HIGH | GDPR/DPA |
| 13 | ACCESS_GRANT_REVIEW | Security.ManagePermissions + Admin.ViewAuditLogs | CRITICAL | SOX 404, ISO 27001 |
| 14 | ROLE_ASSIGN_APPROVE | Admin.AssignRoles + Admin.ManageUsers | CRITICAL | SOX 404 |
| 15 | CONFIG_CHANGE_APPROVE | Admin.ConfigureSystem + Admin.ViewAuditLogs | HIGH | SOX 404 |
| 16 | CONTRACT_CREATE_APPROVE | LeaseAcquisition.Create + LeaseAcquisition.Approve | HIGH | SOX 404 |
| 17 | TAX_CALCULATE_FILE | Tax.Calculate + Regulatory.Submit | HIGH | SOX 404 |
| 18 | EMISSION_REPORT_VERIFY | Environmental.ReportEmissions + Environmental.ViewCompliance | MEDIUM | EPA |
| 19 | SAFETY_DRILL_EVALUATE | HSE.CreateRiskAssessment + HSE.ConductAudit | MEDIUM | ISO 45001 |
| 20 | PURCHASE_ORDER_APPROVE | Facilities.ManageEquipment + WellManagement.Approve | MEDIUM | SOX 404 |
| 21 | DECOMMISSIONING_PLAN_APPROVE | Decommissioning.PlanAbandonment + Decommissioning.Approve | HIGH | Regulatory |
| 22 | PRODUCTION_ALLOCATE_ADJUST | ProductionAccounting.Allocate + ProductionAccounting.Adjust | HIGH | SOX 404 |
| 23 | COST_CLASSIFY_POST | Accounting.PostJournal + Accounting.ManagePeriods | MEDIUM | SOX 404 |
| 24 | USER_CREATE_APPROVE | Admin.ManageUsers + Admin.AssignRoles | CRITICAL | SOX 404 |
| 25 | AUDIT_LOG_VIEW_MODIFY | Admin.ViewAuditLogs + Admin.ConfigureSystem | CRITICAL | ISO 27001 |

---

### P4-03: SodEvaluationEngine

**File:** `Beep.OilandGas.WorkflowManagement\Services\SodEvaluationEngine.cs` (NEW)

```csharp
public interface ISodEvaluationEngine
{
    /// <summary>
    /// Given a set of permissions, find all SoD conflicts.
    /// </summary>
    Task<List<SodConflict>> EvaluatePermissionsAsync(
        List<string> permissionCodes, string scopeContext = null);

    /// <summary>
    /// Given two roles, check if assigning both creates an SoD conflict.
    /// </summary>
    Task<SodCheckResult> CheckRoleCombinationAsync(
        string roleA, string roleB, string scopeContext = null);

    /// <summary>
    /// Given a user's current permissions and a new permission being added,
    /// check if adding it creates any new SoD conflicts.
    /// </summary>
    Task<SodCheckResult> CheckPermissionAdditionAsync(
        string userId, string newPermissionCode);

    /// <summary>
    /// Get all active SoD rules for a category.
    /// </summary>
    Task<List<SOD_RULE>> GetRulesByCategoryAsync(string category);
}

public class SodConflict
{
    public SOD_RULE Rule { get; set; }
    public string PermissionA { get; set; }
    public string PermissionB { get; set; }
    public string RoleA_HoldingPermissionA { get; set; }
    public string RoleB_HoldingPermissionB { get; set; }
    public bool IsBlocking { get; set; }
    public string RecommendedAction { get; set; }
}

public class SodCheckResult
{
    public bool HasConflict { get; set; }
    public List<SodConflict> Conflicts { get; set; }
    public List<SodConflict> Warnings { get; set; } // Non-blocking conflicts
    public bool CanProceed { get; set; }
    public List<string> RequiredMitigations { get; set; }
}
```

**Algorithm:**
1. Load all active SoD rules
2. For each pair of user permissions, check against each rule
3. For role combination check: expand both roles to their full permission sets (including inherited), then run the pair check
4. For permission addition check: expand user's current permissions, add the new one, run pair check on new pairs only

---

### P4-04: SodConflictDetector — At Role Assignment Time

**File:** `Beep.OilandGas.UserManagement\Services\SodConflictDetector.cs` (NEW)

This intercepts role assignment attempts and blocks SoD-violating assignments before they're saved.

```csharp
public interface ISodConflictDetector
{
    /// <summary>
    /// Called BEFORE a role is assigned to a user.
    /// Returns the SoD check result. If blocking, assignment is rejected.
    /// </summary>
    Task<SodCheckResult> PreAssignCheckAsync(
        string userId, string roleId, List<string> newPermissionCodes);

    /// <summary>
    /// Called at transaction time (e.g., when user tries to approve their own AFE).
    /// Checks if the same user performed the prerequisite action.
    /// </summary>
    Task<SodCheckResult> PreTransactionCheckAsync(
        string userId, string action, string entityType, string entityId);

    /// <summary>
    /// Log all SoD evaluations for audit.
    /// </summary>
    Task LogSodEvaluationAsync(SodCheckResult result, string contextUserId);
}
```

**Integration in `RoleAssignmentService.AssignRoleAsync`:**
```csharp
public async Task<Result> AssignRoleAsync(string userId, string roleId, string assignedBy)
{
    // 1. Compute what permissions the user would have after this assignment
    var newPermissions = await ComputeEffectivePermissionsAsync(userId, roleId);
    
    // 2. Run SoD check
    var sodCheck = await _sodDetector.PreAssignCheckAsync(userId, roleId, newPermissions);
    
    // 3. If blocking conflict → reject with explanation
    if (!sodCheck.CanProceed)
    {
        await _sodDetector.LogSodEvaluationAsync(sodCheck, assignedBy);
        return Result.Fail($"SoD conflict: {string.Join("; ", sodCheck.Conflicts.Select(c => c.Rule.RULE_NAME))}");
    }
    
    // 4. If warning only → log and proceed
    if (sodCheck.Warnings.Any())
    {
        await _sodDetector.LogSodEvaluationAsync(sodCheck, assignedBy);
    }
    
    // 5. Proceed with assignment
    return await _roleAssignmentRepo.AssignRoleAsync(userId, roleId, assignedBy);
}
```

---

### P4-05 & P4-06: Compensating Controls

**Entity:** `COMPENSATING_CONTROL`
```csharp
public class COMPENSATING_CONTROL : ModelEntityBase
{
    public string CONTROL_ID { get; set; }           // PK
    public string SOD_CONFLICT_ID { get; set; }      // Which conflict this compensates for
    public string USER_ID { get; set; }              // User with the SoD conflict
    public string CONTROL_TYPE { get; set; }         // "MANAGER_REVIEW", "AUDIT_LOG_REVIEW", "DUAL_APPROVAL"
    public string CONTROL_DESCRIPTION { get; set; }
    public string APPROVED_BY { get; set; }          // Who approved this exception
    public DateTime APPROVED_DATE { get; set; }
    public DateTime EFFECTIVE_FROM { get; set; }
    public DateTime EFFECTIVE_TO { get; set; }       // Compensating controls are TIME-BOUND
    public string REVIEW_FREQUENCY { get; set; }     // "WEEKLY", "MONTHLY", "QUARTERLY"
    public string LAST_REVIEWED_DATE { get; set; }
    public string LAST_REVIEWED_BY { get; set; }
    public string STATUS { get; set; }               // "ACTIVE", "EXPIRED", "REVOKED"
}
```

**Compensating control workflow (process definition `SOD_WAIVER`):**
1. User's manager requests waiver with justification
2. Security Administrator reviews SoD conflict details
3. Independent reviewer (Auditor or Compliance Officer) approves
4. Compensating control documented with expiry date (max 90 days)
5. Auto-expiry: control deactivates, user's conflicting role is suspended
6. Renewal requires new approval

---

### P4-07: Cryptographic Audit Chain

**File:** `Beep.OilandGas.UserManagement\Services\AuditChainService.cs` (NEW)

Each `PROCESS_HISTORY` entry gets a cryptographic hash that includes the previous entry's hash, creating an immutable chain.

```csharp
public interface IAuditChainService
{
    /// <summary>
    /// Compute the hash for a new history entry, chaining from the previous entry.
    /// </summary>
    Task<string> ComputeChainHashAsync(
        string previousEntryHash,
        ProcessHistoryEntry newEntry);

    /// <summary>
    /// Verify the integrity of the entire audit chain for a process instance.
    /// </summary>
    Task<ChainVerificationResult> VerifyChainIntegrityAsync(string processInstanceId);

    /// <summary>
    /// Sign a chain checkpoint with a key (for regulatory submission).
    /// </summary>
    Task<string> SignChainCheckpointAsync(string processInstanceId, string keyId);
}

public class ChainVerificationResult
{
    public bool IsIntact { get; set; }
    public int TotalEntries { get; set; }
    public int VerifiedEntries { get; set; }
    public List<ChainBreakInfo> Breaks { get; set; }
    public DateTime VerificationTimestamp { get; set; }
}

public class ChainBreakInfo
{
    public int EntryIndex { get; set; }
    public string ProcessHistoryId { get; set; }
    public string ExpectedHash { get; set; }
    public string ComputedHash { get; set; }
    public string BreakType { get; set; } // "HASH_MISMATCH", "MISSING_ENTRY", "TIMESTAMP_ANOMALY"
}
```

**Hash algorithm:** SHA-256 over:
```
SHA256(previousHash + "|" + entryId + "|" + eventType + "|" + timestamp + "|" + userId + "|" + fromStatus + "|" + toStatus + "|" + details)
```

**Storage:** Add `CHAIN_HASH` column to `PROCESS_HISTORY` entity (or store in `EVENT_DATA_JSON` for minimal schema change).

---

### P4-08: AuditVerificationService

**File:** `Beep.OilandGas.UserManagement\Security\AuditVerificationService.cs` (NEW)

```csharp
public interface IAuditVerificationService
{
    /// <summary>
    /// Run a full integrity verification on all process instances in a date range.
    /// Used by auditors.
    /// </summary>
    Task<BatchVerificationReport> VerifyAllChainsAsync(DateTime from, DateTime to);

    /// <summary>
    /// Export the audit chain for a process instance in a format acceptable
    /// to external auditors (CSV + hash manifest).
    /// </summary>
    Task<Stream> ExportAuditPackageAsync(string processInstanceId);
}
```

---

### P4-09 & P4-10: Compliance Report Service

**File:** `Beep.OilandGas.WorkflowManagement\Services\ComplianceReportService.cs` (NEW)

```csharp
public interface IComplianceReportService
{
    /// <summary>
    /// Generate a SOX IT General Controls (ITGC) report covering:
    /// - Access control: who has what access, when granted, when reviewed
    /// - Change management: process definition changes, who approved
    /// - Computer operations: system health, backup verification
    /// </summary>
    Task<SoxItgcReport> GenerateSoxItgcReportAsync(DateTime periodStart, DateTime periodEnd);

    /// <summary>
    /// Generate user access summary: all users, roles, permissions,
    /// SoD conflicts, compensating controls.
    /// </summary>
    Task<UserAccessSummaryReport> GenerateUserAccessReportAsync();

    /// <summary>
    /// Generate role-permission matrix for audit review.
    /// </summary>
    Task<Stream> GenerateRolePermissionMatrixAsync();
}
```

**Report templates use existing PPDM data:**
- `PROCESS_HISTORY` for change audit trail
- `ROLE_PERMISSION` + `USER_ROLE` for access matrix
- `SOD_CONFLICT` + `COMPENSATING_CONTROL` for SoD exceptions
- `PROCESS_DEFINITION` + `WORKFLOW_VERSION` for change history

---

### P4-11: SEC Reserves Report Template

```csharp
public class SecReservesReportData
{
    public string FieldId { get; set; }
    public string ReservesCategory { get; set; }    // PROVED, PROBABLE, POSSIBLE
    public decimal OilVolume { get; set; }           // MMbbl
    public decimal GasVolume { get; set; }           // Bcf
    public decimal NglVolume { get; set; }           // MMbbl
    public decimal BOE { get; set; }                 // Barrels of oil equivalent
    public string EvaluatorId { get; set; }
    public string ApproverId { get; set; }
    public DateTime EvaluationDate { get; set; }
    public string AuditChainVerificationId { get; set; } // Links to PROCESS_HISTORY chain
}
```

**Key requirement:** SEC reserves reports must have verifiable audit trail showing:
- Who evaluated the reserves
- Who independently reviewed (SoD check: evaluator ≠ approver)
- When each step occurred
- That no data was altered post-approval (chain hash verification)

---

### P4-12: Access Review Campaign Service

**File:** `Beep.OilandGas.UserManagement\Services\AccessReviewCampaignService.cs` (NEW)

```csharp
public interface IAccessReviewCampaignService
{
    /// <summary>
    /// Start a quarterly access review campaign.
    /// Each manager must certify their team's access is appropriate.
    /// </summary>
    Task<AccessReviewCampaign> StartCampaignAsync(string initiatedBy);

    /// <summary>
    /// Get pending reviews for a manager.
    /// </summary>
    Task<List<AccessReviewItem>> GetPendingReviewsAsync(string managerId);

    /// <summary>
    /// Manager certifies or revokes a user's access.
    /// </summary>
    Task CertifyAccessAsync(string reviewItemId, string decision, string comments, string reviewerId);

    /// <summary>
    /// Generate campaign completion report.
    /// </summary>
    Task<CampaignReport> GetCampaignReportAsync(string campaignId);
}
```

**Entity:** `ACCESS_REVIEW_CAMPAIGN`
```csharp
public class ACCESS_REVIEW_CAMPAIGN : ModelEntityBase
{
    public string CAMPAIGN_ID { get; set; }
    public string CAMPAIGN_NAME { get; set; }         // "Q3 2026 Access Review"
    public DateTime START_DATE { get; set; }
    public DateTime DUE_DATE { get; set; }            // 30 days from start
    public string STATUS { get; set; }                // "ACTIVE", "COMPLETED", "OVERDUE"
    public string INITIATED_BY { get; set; }
}

public class ACCESS_REVIEW_ITEM : ModelEntityBase
{
    public string REVIEW_ITEM_ID { get; set; }
    public string CAMPAIGN_ID { get; set; }
    public string USER_ID { get; set; }
    public string REVIEWER_ID { get; set; }           // User's manager
    public string CURRENT_ROLES_JSON { get; set; }    // Snapshot of roles at campaign start
    public string CURRENT_PERMISSIONS_JSON { get; set; }
    public string DECISION { get; set; }              // "CERTIFIED", "REVOKED", "MODIFIED"
    public string COMMENTS { get; set; }
    public DateTime? REVIEWED_DATE { get; set; }
}
```

---

## Phase 4 Completion Checklist

- [ ] 25 SoD rules defined and seeded
- [ ] SoD check runs at role assignment time, blocks critical conflicts
- [ ] SoD check runs at transaction time (e.g., approving own AFE)
- [ ] Compensating controls can be created for necessary SoD exceptions
- [ ] Compensating controls auto-expire after 90 days
- [ ] PROCESS_HISTORY entries are cryptographically chained
- [ ] Audit chain verification detects tampering
- [ ] SOX ITGC report generates from live data
- [ ] User access report shows all roles, permissions, SoD status
- [ ] SEC reserves report links to verifiable audit chain
- [ ] Quarterly access review campaign can be initiated
- [ ] Manager can certify/revoke team access from campaign

## Phase 4 Deliverables

| # | File | Action |
|---|------|--------|
| 1 | `LifeCycle\Data\Tables\SOD_RULE.cs` | CREATE |
| 2 | `LifeCycle\Data\Tables\SOD_CONFLICT.cs` | CREATE |
| 3 | `LifeCycle\Data\Tables\COMPENSATING_CONTROL.cs` | CREATE |
| 4 | `UserManagement\Models\Identity\ACCESS_REVIEW_CAMPAIGN.cs` | CREATE |
| 5 | `UserManagement\Models\Identity\ACCESS_REVIEW_ITEM.cs` | CREATE |
| 6 | `LifeCycle\Services\Processes\SodEvaluationEngine.cs` | CREATE |
| 7 | `LifeCycle\Services\Processes\ComplianceReportService.cs` | CREATE |
| 8 | `UserManagement\Services\SodConflictDetector.cs` | CREATE |
| 9 | `UserManagement\Services\AuditChainService.cs` | CREATE |
| 10 | `UserManagement\Services\AccessReviewCampaignService.cs` | CREATE |
| 11 | `UserManagement\Security\AuditVerificationService.cs` | CREATE |
| 12 | `UserManagement\Services\RoleAssignmentService.cs` | MODIFY (SoD integration) |
| 13 | `LifeCycle\Data\Tables\PROCESS_HISTORY.cs` | MODIFY (add CHAIN_HASH) |
| 14 | `LifeCycle\Services\Processes\PPDMProcessService.cs` | MODIFY (hash chain on history insert) |
| 15 | `LifeCycle\Modules\LifeCycleModule.cs` | MODIFY (register new entities, seed SoD rules) |
| 16 | `LifeCycle\Definitions\SodRuleSeed.cs` | CREATE |

---

*Previous: [Phase 3 — Cross-Role Orchestration](phase-3-cross-role-orchestration.md)*
*Next: [Phase 5 — Experience & Integration](phase-5-experience-integration.md)*

# Phase 6 — Contractor Management
## PROJECT_STEP_BA Assignments and BA_LICENSE Validation

---

## Contractor Assignment Flow

```
WorkOrder in PLANNED state
    │
    ▼
AssignContractorAsync(instanceId, stepId, baId, role, userId)
    │
    ├─► Get BUSINESS_ASSOCIATE by baId — 404 if not found
    ├─► Validate BA_TYPE is contractor type
    ├─► Validate BA_LICENSE meets WO type requirements (see table below)
    ├─► Check for existing assignment to avoid duplicates
    │
    ▼
Insert PROJECT_STEP_BA row
    │
    ▼
StateM guard check: HasContractorAssigned(instanceId) passes
    │
    ▼
Transition to IN_PROGRESS is now allowed
```

---

## License Validation Rules by WO Type

| WO Type | Required `BA_LICENSE.LICENSE_TYPE` | Minimum Level | Jurisdiction |
|---|---|---|---|
| `WO-PREVENTIVE` | `CONTRACTOR_GENERAL` | 2 | All |
| `WO-CORRECTIVE` | `CONTRACTOR_GENERAL` | 2 | All |
| `WO-SAFETY` | `SAFETY_CRITICAL_CERT` | 3 | USA: `BSEE_CERT`; Canada: `AER_CONTRACTOR_CERT` |
| `WO-ENVIRONMENTAL` | `ENV_REMEDIATION_CERT` | 2 | USA: `EPA_CERT`; Canada: `ECCC_CERT` |
| `WO-REGULATORY` | `REGULATORY_INSPECTION_CERT` | 3 | All |
| `WO-TURNAROUND` | `TURNAROUND_SUPERVISOR` | 3 | All |

---

## IContractorManagementService Interface

```csharp
public interface IContractorManagementService
{
    Task<ProjectStepBARow> AssignContractorAsync(
        string instanceId, string stepId, string baId,
        string roleCode, string userId);

    Task RemoveContractorAsync(string instanceId, string stepId, string baId, string userId);

    Task<List<ContractorAssignment>> GetAssignmentsAsync(string instanceId);

    Task<ContractorQualificationResult> ValidateContractorAsync(
        string baId, string woType, string jurisdiction);
}

public record ContractorQualificationResult(
    bool IsQualified,
    string? FailureReason,
    DateTime? LicenseExpiry);

public record ContractorAssignment(
    string StepId, string BaId, string BaName, string RoleCode, DateTime AssignedDate);
```

---

## ContractorManagementService — ValidateContractorAsync

```csharp
public async Task<ContractorQualificationResult> ValidateContractorAsync(
    string baId, string woType, string jurisdiction)
{
    var licenseRepo = BuildRepo("BA_LICENSE");
    var filters = new List<AppFilter>
    {
        new() { FieldName = "BA_ID",       Operator = "=", FilterValue = baId },
        new() { FieldName = "ACTIVE_IND",  Operator = "=", FilterValue = "Y"  }
    };
    var licenses = await licenseRepo.GetAsync(filters);

    var requiredType  = GetRequiredLicenseType(woType, jurisdiction);
    var requiredLevel = GetRequiredLicenseLevel(woType);

    var matching = licenses
        .Select(l => (dynamic)l)
        .Where(l => (string)l.LICENSE_TYPE == requiredType &&
                    (int)l.LICENSE_LEVEL   >= requiredLevel &&
                    (DateTime)l.EXPIRY_DATE > DateTime.UtcNow)
        .ToList();

    if (matching.Count == 0)
        return new ContractorQualificationResult(false,
            $"No valid {requiredType} (level ≥ {requiredLevel}) license on file", null);

    var expiry = matching.Max(l => (DateTime)l.EXPIRY_DATE);
    return new ContractorQualificationResult(true, null, expiry);
}
```

---

## PPDM Tables Used

| Table | Column | Purpose |
|---|---|---|
| `PROJECT_STEP_BA` | `PROJECT_ID`, `STEP_SEQ`, `BA_ID`, `STEP_BA_TYPE` | Assignment rows |
| `BUSINESS_ASSOCIATE` | `BA_ID`, `BA_TYPE`, `BA_NAME` | Contractor entity |
| `BA_LICENSE` | `BA_ID`, `LICENSE_TYPE`, `LICENSE_LEVEL`, `EXPIRY_DATE` | Qualification records |

---

## Guard Wiring

The `HasContractorAssigned` guard check used in `ProcessStateMachine`:

```csharp
machine.Configure(PLANNED)
    .Permit(START_TRIGGER, IN_PROGRESS)
    .Guard(() => _contractorService.GetAssignmentsAsync(instanceId)
                     .GetAwaiter().GetResult()
                     .Any(a => a.StepId == "EXECUTION_STEP"),
           "Contractor assignment (PROJECT_STEP_BA) is required [IOGP S-501 §3.2]");
```

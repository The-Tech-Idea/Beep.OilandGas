# Phase 10 — Disaster Recovery
## RTO/RPO Targets, Backup Schedule, PPDM Restore Procedure

---

## Recovery Objectives

| Tier | Scenario | RTO | RPO |
|---|---|---|---|
| Tier 1 | API service crash (pod restart) | < 2 minutes | 0 (no data loss) |
| Tier 2 | DB server failure (failover replica) | < 30 minutes | < 5 minutes |
| Tier 3 | Full region failure (restore from backup) | < 4 hours | < 24 hours |
| Tier 4 | Data corruption (point-in-time restore) | < 6 hours | < 1 hour (transaction logs) |

---

## Backup Schedule

| Backup Type | Frequency | Retention | Storage |
|---|---|---|---|
| Full database backup | Daily (01:00 UTC) | 30 days | Azure Blob Storage (geo-redundant) |
| Differential backup | Every 6 hours | 7 days | Same |
| Transaction log backup | Every 15 minutes | 3 days | Same |
| Application config snapshot | On each deployment | 90 days | Azure Blob + Git tag |

Azure SQL or SQL Server AlwaysOn AG handles automatic failover for Tier 1–2.

---

## PPDM Full Restore Procedure (Tier 3)

```
Step 1: Provision new SQL Server instance (Bicep/Terraform or Azure Portal)
         Target SQL Server 2022 or Azure SQL; same collation as original

Step 2: Restore latest full backup from Azure Blob
        RESTORE DATABASE PPDM39
        FROM URL = 'https://storageaccount.blob.core.windows.net/backups/ppdm39_full_YYYYMMDD.bak'
        WITH NORECOVERY

Step 3: Apply latest differential backup
        RESTORE DATABASE PPDM39
        FROM URL = '.../ppdm39_diff_YYYYMMDD_HH.bak'
        WITH NORECOVERY

Step 4: Apply transaction logs in sequence until desired point
        RESTORE LOG PPDM39
        FROM URL = '.../ppdm39_log_YYYYMMDD_HHmm.bak'
        WITH RECOVERY

Step 5: Update connection string in Key Vault / environment variable
         ConnectionStrings__PPDM = "Server=<new-server>;Database=PPDM39;..."

Step 6: Restart API pods / containers
        kubectl rollout restart deployment/beep-oilgas-api

Step 7: Validate: run smoke test suite against restored DB
        dotnet test --filter "Category=SmokeTest"

Step 8: Notify field operations team; re-enable integrations
```

---

## Data Corruption Recovery (Tier 4)

If data corruption is detected (e.g., bulk delete, wrong UPDATE):

```sql
-- Point-in-time restore to 15 minutes before corruption
RESTORE DATABASE PPDM39
FROM URL = '...'
WITH STOPAT = '2025-06-15T14:30:00',
     RECOVERY

-- Re-run any legitimate transactions that occurred in that 15-min window
-- using captured audit log from PPDMDataAccessAuditService
```

---

## Runbook Automation

DR runbook stored as `Scripts/DR/RestoreDatabase.ps1`:

```powershell
param(
    [string]$RestoreToPointInTime,   # ISO 8601; if empty, restore to latest
    [string]$TargetServer,
    [string]$StorageAccountUrl)

# 1. Download backup files
# 2. Execute RESTORE T-SQL via sqlcmd / Invoke-Sqlcmd
# 3. Run smoke tests
# 4. Output restore summary
```

---

## Backup Validation Test (Monthly)

Monthly restore drill:
1. Spin up isolated SQL Server container
2. Restore previous night's full backup
3. Run `dotnet test --filter "Category=SmokeTest"` against restored DB
4. Confirm all smoke tests pass
5. Record result in `DR_TEST_LOG.md` with date + RTO achieved

# Demo Database Cleanup Runbook

## When to use

Use this to remove expired demo databases, recover from a failed demo database creation, or verify local cleanup behavior.

## Required evidence

- Demo database connection name
- Demo database status from `GetDemoDatabaseStatusAsync`
- Creation and expiry timestamps
- Seed stage results
- Local database file path, when applicable

## Procedure

1. Query demo database status and confirm whether the database exists, is expired, and completed required seed stages.
2. If cleanup follows failed creation, remove the Beep connection before deleting the local database file.
3. If cleanup is scheduled retention cleanup, call `CleanupExpiredDatabasesAsync` and record the number of cleaned databases.
4. If cleanup is manual, call `DeleteDemoDatabaseAsync(connectionName)` and verify the connection no longer appears in demo database listings.
5. For local file-backed databases, confirm the file is deleted only after the datasource is closed or removed from configuration.
6. Re-run status lookup to verify `Exists=false` or that the connection is absent.

## Stop conditions

- The connection is still open and the database file cannot be deleted
- The database is not expired and no manual deletion was approved
- Required seed stage failure indicates setup data should be inspected before deletion

# Phase 1: Audit & Remove Duplicate PPDM39 Tables

## Goals
- Identify all local table classes that duplicate PPDM39 canonical tables.
- Remove these files from the project.
- Update project references to use canonical models only.
- Document all removals and any issues encountered.

## Steps
1. List all local table classes in `Data/PermitsAndApplications/Tables`.
2. Cross-reference with `Beep.OilandGas.PPDM.Models/39` to identify duplicates.
3. Delete duplicate files from the local tables folder.
4. Search and update all code references to use canonical models.
5. Commit and document the changes.

## Notes
- Do not remove extension/unique tables.
- Document any business logic that depended on local table customizations.

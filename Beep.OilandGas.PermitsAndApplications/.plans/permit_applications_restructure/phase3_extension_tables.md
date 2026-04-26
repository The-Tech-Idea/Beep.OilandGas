# Phase 3: Redesign Extension Tables & Domain Logic

## Goals
- Review all extension/unique tables and ensure they do not duplicate PPDM39 schema.
- Redesign extension tables to follow best practices (scalar fields, clear foreign keys, no object/list properties).
- Align domain logic to use extension tables only for non-PPDM data.

## Steps
1. List all remaining tables in `Data/PermitsAndApplications/Tables` after duplicate removal.
2. Review schema for each extension table for best practices.
3. Refactor extension tables as needed.
4. Update business logic to use extension tables for custom data only.
5. Document all changes and rationale.

## Notes
- Extension tables should never shadow or overlap canonical PPDM columns.
- Document any new extension table designs.

# PermitsAndApplications Global Enhancement Plan

## Goal
Extend permit and application coverage beyond current US/Canada scope by adding jurisdiction packages (templates, validation rules, mappings, and tests) and hardening the service workflows.

## Current Coverage (Baseline)
- US: RRC, TCEQ, BOEM, BSEE
- Canada: AER, BCER

## Enhancement Phases

### Phase 1: Global Jurisdiction Framework
- Define a standard "jurisdiction package" contract:
  - Form templates and field mappings
  - Validation rule sets
  - Required attachments and fee rules
- Add a registry for available jurisdictions (metadata-driven).
- Add a per-jurisdiction config file format (JSON/YAML) for templates and requirements.

### Phase 2: Additional Jurisdiction Packages (Priority Regions)
- United States: remaining state regulators (COGCC, NDIC, WOGCC, etc.).
- Canada: SER, NLDET and provincial extensions.
- Mexico: CNH, ASEA.
- UK/North Sea: NSTA.
- Norway: NPD.
- Australia: NOPSEMA, QLD_DNRME, WA_DMIRS.
- Brazil: ANP.
- Argentina: Neuquen, Mendoza.
- Indonesia: SKKMigas.
- Kazakhstan: KZ_MOE.

Deliverables per jurisdiction:
- Form templates and field mappings
- Validation rules (required fields, cross-field checks)
- Attachment requirements and fee rules
- Unit tests (templates + validation)

### Phase 3: Mapping Expansion (PPDM39)
- Add mappers for:
  - BA_PERMIT
  - FACILITY_LICENSE
  - WELL_PERMIT_TYPE
  - APPLIC_BA / APPLIC_DESC / APPLIC_REMARK
- Add data access methods for each new entity.
- Add unit tests for each mapper.

### Phase 4: Form Output Pipeline
- JSON payload generation (complete).
- Add PDF-ready rendering templates and storage workflow.
- Add output storage naming + archival strategy.
- Add tests for file output and attachment creation.

### Phase 5: Status and Compliance Hardening
- Status transition validation (complete).
- Status history persistence (complete).
- Add cross-entity validation:
  - Permit type vs. application type
  - Well/facility linkage integrity
  - Jurisdiction-specific required fields
- Add regression tests for status transitions.

## Success Criteria
- 10+ jurisdictions supported with validated templates and rules.
- Full PPDM39 mapping for permit entities.
- JSON/PDF outputs for all supported jurisdictions.
- Coverage: 80%+ unit test coverage for mappings and validation rules.

## Suggested Next Steps
1. Implement jurisdiction config format and registry.
2. Add top 3 new jurisdiction packages (e.g., CNH, NSTA, NOPSEMA).
3. Add mapper test suite for BA_PERMIT/FACILITY_LICENSE/WELL_PERMIT_TYPE.

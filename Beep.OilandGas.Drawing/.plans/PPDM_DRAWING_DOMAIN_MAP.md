# PPDM Drawing Domain Map

## Purpose

This document maps drawing and visualization capabilities to PPDM subject areas and table families. It is a planning reference, not a rigid API contract. The goal is to keep roadmap decisions aligned with the petroleum data model already used across the wider solution.

## Domain Map

| Visualization Domain | PPDM Anchors | Why It Matters To Drawing |
| --- | --- | --- |
| Wells and well structure | `WELL`, `WELL_COMPONENT`, `WELL_ACTIVITY_COMPONENT`, `PROD_STRING_COMPONENT`, `PDEN_WELL`, `PDEN_PROD_STRING` | Core wellbore, completion, production-string, and activity-driven schematic views |
| Reservoir, pool, and field context | `POOL`, `POOL_COMPONENT`, `FIELD`, `FIELD_COMPONENT`, `PDEN`, `PDEN_COMPONENT`, `PDEN_VOL_SUMMARY` | Reservoir maps, field overviews, pool boundaries, volumetric context, and aggregation layers |
| Lithology and stratigraphy | `LITH_LOG`, `LITH_LOG_COMPONENT`, `LITH_INTERVAL`, `LITH_ROCK_TYPE`, `STRAT_UNIT_COMPONENT`, `PALEO_*` | Lithology strips, zonation, section labeling, facies display, and interpreted cross-sections |
| Facilities and equipment | `FACILITY`, `FACILITY_COMPONENT`, `EQUIPMENT`, `EQUIPMENT_COMPONENT`, `SF_COMPONENT` | Surface layout views, equipment overlays, facility schematics, and field system maps |
| Spatial description and location systems | `SP_COMPONENT`, `AREA`, `AREA_COMPONENT`, `AREA_XREF`, `LEGAL_*`, `CS_COORDINATE_SYSTEM`, `CS_GEODETIC_DATUM`, `CS_COORD_TRANSFORM`, `CS_COORD_TRANS_PARM`, `CS_COORD_TRANS_VALUE` | Map geometry, legal boundaries, location conversion, CRS alignment, and plan-view rendering |
| Seismic and interpretation context | `SEIS_SET_COMPONENT`, `SEIS_INSP_COMPONENT`, `SEIS_TRANS_COMPONENT` | Seismic footprints, interpreted overlays, and trace-to-map context |
| Production engineering context | `PROD_STRING_COMPONENT`, `PDEN_*`, `FACILITY_RATE`, `PDEN_VOL_SUMMARY`, `PDEN_FLOW_MEASUREMENT` | Flow path views, production system schematics, surveillance layers, and allocation-aware displays |
| HSE and compliance overlays | `HSE_INCIDENT*`, `CONSENT*`, `NOTIFICATION*`, `LAND_RIGHT*`, `WORK_ORDER_COMPONENT` | Risk overlays, compliance status, operational constraints, and incident visualization on field maps |
| Land and commercial context | `LAND_RIGHT*`, `LAND_UNIT*`, `CONTRACT*`, `OBLIGATION*`, `INTEREST_SET*` | Lease overlays, contract boundaries, ownership, obligations, and commercial annotation |
| Metadata and validation | `PPDM_TABLE`, `PPDM_COLUMN`, `PPDM_RULE_COMPONENT`, `PPDM_METRIC_COMPONENT` | Table discovery, rule-aware validation, and renderer-side schema diagnostics |

## High-Value PPDM Families For Early Delivery

### Phase 01 through Phase 03

- `WELL*`
- `LITH*`
- `STRAT*`
- `PDEN*`
- `POOL*`
- `FIELD*`
- `CS_*`

### Phase 04 and Phase 05

- `SP_*`
- `AREA*`
- `SEIS_*`
- `FACILITY*`
- `EQUIPMENT*`
- `LAND_*`
- `HSE_INCIDENT*`

## Recommended Adapter Strategy

Do not bind renderers directly to PPDM entity classes. Instead:

1. Load PPDM entities into normalized visualization models.
2. Preserve source metadata such as table name, key identifiers, and source timestamps.
3. Keep scene models independent of whether the data came from PPDM, Energistics, LAS, or flat files.

## Immediate Planning Implications

- Contour and reservoir map work should plan against `POOL`, `FIELD`, `PDEN`, `SP_*`, and `AREA*` families.
- Well schematics should plan against `WELL*`, `PROD_STRING_COMPONENT`, and equipment-linked facility data.
- Lithology and stratigraphy displays should plan against `LITH_*`, `STRAT_*`, and `PALEO_*` subject areas.
- Map views must treat `CS_*`, legal location tables, and `SP_COMPONENT` as first-class dependencies rather than optional metadata.
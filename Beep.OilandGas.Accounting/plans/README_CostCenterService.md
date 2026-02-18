# CostCenterService

## Overview

The `CostCenterService` provides Basic CRUD operations for `COST_CENTER` entities.

## Responsibilities

- **Management**: Create, Update, Deactivate Cost Centers.
- **Hierarchy**: Although not fully hierarchical in code yet, it manages the flat list of centers used by the `CostAllocationService`.

## Key Methods

### `CreateCostCenterAsync`
Creates a new cost center definition.

### `UpdateCostCenterAsync`
Updates details (Name, Type, Parent, etc.).

## Data Models
- `COST_CENTER` (Fields: `COST_CENTER_ID`, `COST_CENTER_NAME`, `COST_CENTER_TYPE`, `TOTAL_CAPITALIZED_COSTS`)

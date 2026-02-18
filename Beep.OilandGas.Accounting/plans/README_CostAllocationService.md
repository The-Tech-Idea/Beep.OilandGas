# CostAllocationService

## Overview

The `CostAllocationService` handles the complex task of distributing overhead and support costs to revenue-generating centers. It supports multiple standard accounting methods.

## Allocation Methods

1.  **Direct Allocation**: Allocates support costs directly to revenue centers based on a basis (e.g., headcount, sq ft). Ignores inter-support-department services.
2.  **Step-Down**: Allocates support centers sequentially. Once a support center is allocated, it's closed out. Recognizes some inter-departmental services.
3.  **Reciprocal**: Uses an iterative approach to fully recognize all inter-departmental services.
4.  **Activity-Based Costing (ABC)**: Allocates based on specific activities and drivers.

## Key Methods

### `AllocateCostsAsync`
Main entry point. Takes a list of cost centers and an allocation method. Returns a `CostAllocationResult` with detailed audit trails of every allocation step.

### `GenerateDepartmentalProfitabilityAsync`
Produces a report showing Revenue vs Direct Costs vs Allocated Overhead to show true profitability.

## Models
- `COST_ALLOCATION`
- `COST_CENTER`

# JointArrangementsService

## Overview

The `JointArrangementsService` handles **IFRS 11** joint arrangements, specifically focusing on the recording of cost shares in joint operations.

## Key Functionality

- **Cost Sharing**: Records the entity's share of costs arising from joint operations.
  - GL: Debit Operating Expense (or Asset), Credit Cash (or Payable).

## Key Methods

### `RecordJointOperationCostShareAsync`
Records a cost share entry.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`

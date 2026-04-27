# ProductionAccounting Interface Status Map

## Canonical Contract

- **Primary canonical contract**: `Beep.OilandGas.Models.Core.Interfaces.IProductionAccountingService`
  - `ProcessProductionCycleAsync` -> **implemented**
  - `GetAccountingStatusAsync` -> **implemented**
  - `ClosePeriodAsync` -> **implemented**

## Expanded Local Surface

- `ProductionAccountingService` includes additional methods not yet represented in the canonical interface:
  - `GetRevenueTransactionsAsync` -> **implemented (service-internal + API-used)**
  - compatibility manager properties/factories (`TraditionalAccounting`, `ProductionManager`, etc.) -> **compatibility-active**

## Status Classification Rules

- **Active**: methods used by API controllers or required orchestration workflows.
- **Compatibility**: methods retained for broader legacy surface; keep callable, document behavior constraints.
- **Staged**: methods with placeholder/no-op behavior or in-memory-only persistence that must not be presented as hardened production API.

## Interface Normalization Tasks

- Add/confirm a domain-facing interface for currently API-used methods such as `GetRevenueTransactionsAsync` if they remain public API dependencies.
- Avoid injecting concrete service type in controllers when a stable interface can be used.
- Keep compatibility-manager access clearly marked as compatibility surface and not core domain API.


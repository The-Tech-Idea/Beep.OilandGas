# ProductionAccounting Behavior Map

## Legend

- **Implemented**: concrete persistence/service logic with defined outputs.
- **Compatibility-active**: callable behavior, often with fallback/in-memory path.
- **Staged/placeholder**: partial or no-op behavior requiring hardening.

## Service Behavior Classification

## `ProductionAccountingService` (orchestrator)

- **Implemented**
  - `ProcessProductionCycleAsync`: multi-step orchestration (measurement -> allocation -> royalty -> revenue -> GL -> mark processed).
  - `GetAccountingStatusAsync`: computes aggregate production/revenue/royalty/cost/net + method + period status.
  - `ClosePeriodAsync`: delegates to period-closing service with logging/error wrapping.
  - `GetRevenueTransactionsAsync`: PPDM query path with fallback field/property matching.
- **Partially hardened**
  - Private aggregate helpers catch-and-default on errors (operationally tolerant but can hide root cause).

## `ProductionAccountingService.ControllerFacade`

- **Compatibility-active**
  - Traditional accounting compatibility managers (PO/Invoice/GL/JE/AR/AP).
  - Production manager compatibility (run tickets, tank inventory), pricing manager, ownership manager, royalty manager.
  - Trading/full-cost/successful-efforts compatibility wrappers.
- **Staged/placeholder**
  - Some methods are no-op shells (example: division-order approval path).
  - Several flows mix persistent writes with in-memory dictionaries, creating parity risk.
  - Multiple synchronous `.GetAwaiter().GetResult()` paths need async hardening plan.

## Module Behavior

- `ProductionAccountingModuleSetup`
  - **Implemented**: broad entity registration for schema migration.
  - **Staged**: seed currently skipped (`No reference seed data defined`), leaving reference-data governance gap.

## API Behavior (current accounting field controller)

- `ApiService/Controllers/Field/AccountingController.cs`
  - **Implemented**: recent activities, production summary, revenue lines, close period.
  - **Risk**: direct dependency on concrete `ProductionAccountingService` instead of interface contract for all actions.

## Key Risks to Address in Next Phases

- Compatibility layers with in-memory fallback can diverge from DB truth.
- Missing explicit `R_`/`RA_` production-accounting seed sets for status/code domains.
- Interface/service/controller boundary is not fully normalized around a canonical contract.


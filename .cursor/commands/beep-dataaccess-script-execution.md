# Script Execution Services

## Beep.OilandGas — scope

These services **execute** scripts from a configured directory (discovery, ordering, engine). They do **not** define the workflow for **new feature extension** tables — use **entities + `IModuleSetup` + entity-driven schema creation** instead of adding DDL under `Beep.OilandGas.Models/Scripts/**`. See **`CLAUDE.md`** and [Database Creation](beep-dataaccess-database-creation.md).

## Overview

The script execution services provide comprehensive script management including discovery, categorization, execution ordering, and execution engine capabilities.

## Services

### PPDMScriptDiscoveryService

Discovers database scripts in a directory structure.

**Key Methods:**
- `DiscoverScriptsAsync`: Discovers all scripts in a path

### PPDMScriptCategorizer

Categorizes scripts by type and module.

**Key Methods:**
- `CategorizeScript`: Categorizes a single script
- `CategorizeScripts`: Categorizes multiple scripts

### PPDMScriptExecutionOrderManager

Determines correct execution order based on dependencies.

**Key Methods:**
- `DetermineExecutionOrder`: Determines execution order for scripts

### PPDMScriptExecutionEngine

Executes database scripts with error handling.

**Key Methods:**
- `ExecuteScriptAsync`: Executes a single script
- `ExecuteScriptsAsync`: Executes multiple scripts

## Related Documentation

- [Database Creation](beep-dataaccess-database-creation.md) - Database creation service
- [Overview](beep-dataaccess-overview.md) - Framework overview


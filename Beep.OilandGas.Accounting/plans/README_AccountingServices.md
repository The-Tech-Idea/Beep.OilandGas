# AccountingServices Container

## Overview

`AccountingServices` is the central dependency injection container for the `Beep.OilandGas.Accounting` namespace. It aggregates all individual accounting services into a single interface (`IAccountingServices`), simplifying service consumption in applications.

## Usage

Instead of injecting dozens of individual services, inject `IAccountingServices`.

```csharp
public class MyController
{
    private readonly IAccountingServices _accounting;

    public MyController(IAccountingServices accounting)
    {
        _accounting = accounting;
    }

    public async Task PostInvoice()
    {
        // Access specific services via the container
        await _accounting.AccountsPayableInvoices.CreateInvoiceAsync(...);
    }
}
```

## Included Services

See the full list of properties in `IAccountingServices` for all available modules, covering:
- Core GL & Reporting
- Subledgers (AP, AR, Inventory, Fixed Assets)
- Tax & Compliance
- Specialized IFRS/GAAP Modules
- Presentation & Disclosure

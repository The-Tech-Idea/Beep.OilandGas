# Beep.OilandGas.ProductionAccounting

Comprehensive oil and gas production accounting and operations management system.

## Overview

This library provides complete production accounting functionality covering:
- Crude oil properties and classification
- Lease acquisition and management
- Storage facilities and service units
- Production measurement and allocation
- Crude oil trading and exchanges
- Pricing and valuation
- Ownership and division of interest
- Unitization
- Sales accounting
- Royalty calculations and payments
- Reporting (internal and external)
- Oil imbalance management

## Features

### Phase 1: Foundation (✅ Complete)
- ✅ Crude oil properties and classification
- ✅ Lease management (Fee, Government, Net Profit, Joint Interest)
- ✅ Contractual agreements (Sales, Transportation, Processing, Storage)
- ✅ Validation and calculations

### Phase 2: Storage Facilities (In Progress)
- Storage facilities and tank batteries
- Test separators
- LACT units
- Inventory management

### Phase 3-12: Additional Phases
See `PRODUCTION_ACCOUNTING_IMPLEMENTATION_PLAN.md` for complete roadmap.

## Usage

### Crude Oil Properties

```csharp
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.ProductionAccounting.Calculations;

var properties = new CrudeOilProperties
{
    ApiGravity = 35.5m,
    SulfurContent = 0.8m,
    BSW = 0.5m,
    WaterContent = 0.3m
};

var oilType = properties.GetCrudeOilType(); // Light
var netVolume = CrudeOilCalculations.CalculateNetVolume(1000m, 0.5m);
```

### Lease Management

```csharp
using Beep.OilandGas.ProductionAccounting.Management;
using Beep.OilandGas.ProductionAccounting.Models;

var manager = new LeaseManager();

var lease = new FeeMineralLease
{
    LeaseId = "Lease-001",
    LeaseName = "Smith Ranch",
    EffectiveDate = new DateTime(2023, 1, 1),
    PrimaryTermMonths = 60,
    WorkingInterest = 0.75m,
    NetRevenueInterest = 0.65625m,
    RoyaltyRate = 0.125m
};

manager.RegisterLease(lease);
var retrievedLease = manager.GetLease("Lease-001");
```

### Sales Agreements

```csharp
var salesAgreement = new OilSalesAgreement
{
    AgreementId = "SA-001",
    Seller = "Producer Co",
    Purchaser = "Refiner Co",
    EffectiveDate = new DateTime(2023, 1, 1),
    PricingTerms = new PricingTerms
    {
        PricingMethod = PricingMethod.IndexBased,
        PriceIndex = "WTI",
        Differential = 2.50m
    }
};

manager.RegisterSalesAgreement(salesAgreement);
```

## Project Structure

```
Beep.OilandGas.ProductionAccounting/
├── Models/
│   ├── CrudeOilModels.cs
│   ├── LeaseModels.cs
│   └── AgreementModels.cs
├── Calculations/
│   └── CrudeOilCalculations.cs
├── Validation/
│   ├── CrudeOilValidator.cs
│   └── LeaseValidator.cs
├── Management/
│   └── LeaseManager.cs
├── Constants/
│   └── ProductionAccountingConstants.cs
└── Exceptions/
    └── ProductionAccountingException.cs
```

## Dependencies

- SkiaSharp (for future visualization)
- Beep.OilandGas.Accounting (for accounting integration)

## Documentation

- `PRODUCTION_ACCOUNTING_IMPLEMENTATION_PLAN.md` - Complete implementation roadmap
- `PRODUCTION_ACCOUNTING_QUICK_REFERENCE.md` - Quick reference guide

## Status

**Phase 1: Foundation** - ✅ Complete
**Phase 2-12** - In Progress

---

**Status: Active Development** 🚀


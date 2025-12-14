# Beep.EconomicAnalysis - Economic Evaluation Library

A comprehensive .NET library for economic analysis and financial evaluation of oil and gas projects.

## Features

### Core Functionality
- **NPV Calculation**: Net Present Value analysis
- **IRR Calculation**: Internal Rate of Return
- **Payback Period**: Simple and discounted payback
- **Cash Flow Modeling**: Time-series cash flow analysis
- **DCF Analysis**: Discounted Cash Flow calculations
- **Economic Indicators**: PI, MIRR, ROI, etc.
- **Sensitivity Analysis**: Parameter sensitivity evaluation
- **Scenario Comparison**: Multiple scenario analysis
- **Risk Analysis**: Monte Carlo simulation

### Economic Metrics
- **NPV**: Net Present Value
- **IRR**: Internal Rate of Return
- **MIRR**: Modified Internal Rate of Return
- **PI**: Profitability Index
- **ROI**: Return on Investment
- **Payback Period**: Simple and discounted
- **DPP**: Discounted Payback Period
- **NPV Profile**: NPV vs discount rate

### Analysis Types
- **Project Economics**: Full project evaluation
- **Well Economics**: Individual well economics
- **Portfolio Analysis**: Multiple project comparison
- **Sensitivity Analysis**: Parameter impact analysis
- **Risk Analysis**: Uncertainty quantification
- **Break-even Analysis**: Break-even calculations

### Visualization
- SkiaSharp-based rendering
- Cash flow charts
- NPV profiles
- Sensitivity tornado diagrams
- Risk distribution plots
- Export capabilities

## Quick Start

```csharp
using Beep.EconomicAnalysis;
using Beep.EconomicAnalysis.Calculations;

// Define project cash flows
var cashFlows = new CashFlow[]
{
    new CashFlow { Period = 0, Amount = -1000000 }, // Initial investment
    new CashFlow { Period = 1, Amount = 300000 },
    new CashFlow { Period = 2, Amount = 400000 },
    new CashFlow { Period = 3, Amount = 500000 },
    new CashFlow { Period = 4, Amount = 300000 },
    new CashFlow { Period = 5, Amount = 200000 }
};

// Calculate NPV at 10% discount rate
double discountRate = 0.10;
double npv = EconomicCalculator.CalculateNPV(cashFlows, discountRate);
Console.WriteLine($"NPV: ${npv:N2}");

// Calculate IRR
double irr = EconomicCalculator.CalculateIRR(cashFlows);
Console.WriteLine($"IRR: {irr:P2}");

// Calculate payback period
double payback = EconomicCalculator.CalculatePaybackPeriod(cashFlows);
Console.WriteLine($"Payback Period: {payback:F2} years");

// Perform sensitivity analysis
var sensitivity = EconomicCalculator.SensitivityAnalysis(
    cashFlows,
    discountRate,
    parameters: new[] { "Initial Investment", "Revenue", "Operating Cost" }
);
```

## Installation

```bash
dotnet add package Beep.EconomicAnalysis
```

## Documentation

See the [API Documentation](API.md) for detailed information.

## License

MIT License


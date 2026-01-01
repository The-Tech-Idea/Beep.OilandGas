global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using Beep.OilandGas.ProductionAccounting.Exceptions;
global using Beep.OilandGas.PPDM39.Core.Metadata;
global using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
global using Beep.OilandGas.PPDM39.DataManagement.Core;
global using Microsoft.Extensions.Logging;
global using TheTechIdea.Beep.Editor;
global using Beep.OilandGas.PPDM39.Repositories;
global using Beep.OilandGas.Models.Core.Interfaces;
global using TheTechIdea.Beep.Report;

// Data namespaces
global using Beep.OilandGas.Models.Data;
global using Beep.OilandGas.Models.Data.Accounting;
global using Beep.OilandGas.Models.Data.Pricing;

// DTO namespaces
global using Beep.OilandGas.Models.DTOs.ProductionAccounting;
global using Beep.OilandGas.Models.DTOs.Pricing;

// Local ProductionAccounting namespaces
global using Beep.OilandGas.ProductionAccounting.Production;
global using Beep.OilandGas.ProductionAccounting.Trading;
global using Beep.OilandGas.ProductionAccounting.Models;
global using Beep.OilandGas.ProductionAccounting.Allocation;

// Resolve ambiguous references with aliases
global using RECEIVABLE = Beep.OilandGas.Models.Data.Accounting.RECEIVABLE;
global using PRICE_INDEX = Beep.OilandGas.Models.Data.Pricing.PRICE_INDEX;
global using ProductionDataDto = Beep.OilandGas.Models.DTOs.ProductionAccounting.ProductionDataDto;
global using ValueRunTicketRequest = Beep.OilandGas.Models.DTOs.ProductionAccounting.ValueRunTicketRequest;

// Resolve DTO ambiguities - use types matching interface expectations
global using CreateSalesTransactionRequest = Beep.OilandGas.Models.DTOs.Accounting.CreateSalesTransactionRequest;
global using GenerateOperationalReportRequest = Beep.OilandGas.Models.DTOs.Reporting.GenerateOperationalReportRequest;
global using GenerateJIBStatementRequest = Beep.OilandGas.Models.DTOs.Reporting.GenerateJIBStatementRequest;
global using CreateExchangeContractRequest = Beep.OilandGas.Models.DTOs.Trading.CreateExchangeContractRequest;
global using LeaseType = Beep.OilandGas.Models.DTOs.ProductionAccounting.LeaseType;
global using MeasurementMethod = Beep.OilandGas.Models.DTOs.ProductionAccounting.MeasurementMethod;

// Resolve data model ambiguities - prefer organized subfolders
global using ROYALTY_INTEREST = Beep.OilandGas.Models.Data.Royalty.ROYALTY_INTEREST;
global using ROYALTY_PAYMENT = Beep.OilandGas.Models.Data.Royalty.ROYALTY_PAYMENT;
global using REGULATED_PRICE = Beep.OilandGas.Models.Data.Pricing.REGULATED_PRICE;

// Ownership namespace
global using OwnershipTree = Beep.OilandGas.Models.Data.Ownership.OwnershipTree;
global using OwnershipTreeNode = Beep.OilandGas.Models.Data.Ownership.OwnershipTreeNode;

// Imbalance DTO alias
global using ImbalanceSummary = Beep.OilandGas.Models.DTOs.Imbalance.ImbalanceSummary;

// Resolve DTO type ambiguities for interface implementations
global using ProvedPropertyDto = Beep.OilandGas.Models.DTOs.ProductionAccounting.ProvedPropertyDto;
global using ProvedReservesDto = Beep.OilandGas.Models.DTOs.ProductionAccounting.ProvedReservesDto;
using Beep.OilandGas.Models.Data.ProductionAccounting;
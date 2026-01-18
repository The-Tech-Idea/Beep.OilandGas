global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using Microsoft.Extensions.Logging;
global using TheTechIdea.Beep.Editor;
global using TheTechIdea.Beep.Report;

// Models - Data entities (PPDM39) - USE ONLY THESE
global using Beep.OilandGas.Models.Data.ProductionAccounting;
global using Beep.OilandGas.Models.Data.Accounting;
global using Beep.OilandGas.Models.Data.Common;
global using Beep.OilandGas.Models.Data.Royalty;
global using Beep.OilandGas.Models.Data.Ownership;
global using Beep.OilandGas.Models.Data.Imbalance;
global using Beep.OilandGas.Models.Data.Trading;

// Service interfaces
global using Beep.OilandGas.Models.Core.Interfaces;

// PPDM39 Infrastructure
global using Beep.OilandGas.PPDM39.Repositories;
global using Beep.OilandGas.PPDM39.Core.Metadata;
global using Beep.OilandGas.PPDM39.DataManagement.Core;

// ProductionAccounting Services
global using Beep.OilandGas.ProductionAccounting.Services;

// ProductionAccounting Constants
global using Beep.OilandGas.ProductionAccounting.Constants;

// ProductionAccounting Exceptions
global using Beep.OilandGas.ProductionAccounting.Exceptions;

// Enums
global using Beep.OilandGas.Models.Enums;

global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using Beep.OilandGas.PPDM39.Repositories;
global using Beep.OilandGas.PPDM39.Core.Metadata;
global using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
global using Beep.OilandGas.PPDM39.DataManagement.Core;
global using Beep.OilandGas.GasLift.Calculations;
global using Beep.OilandGas.Models.GasLift;
global using Beep.OilandGas.Models.Core.Interfaces;
global using Beep.OilandGas.Models.DTOs;
global using Microsoft.Extensions.Logging;
global using TheTechIdea.Beep.Editor;
global using TheTechIdea.Beep.DataBase;

// Resolve ambiguous type references - prefer GasLift models namespace
global using GasLiftPerformancePoint = Beep.OilandGas.Models.GasLift.GasLiftPerformancePoint;


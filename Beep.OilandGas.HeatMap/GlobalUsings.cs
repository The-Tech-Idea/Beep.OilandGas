global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using Beep.OilandGas.PPDM39.Repositories;
global using Beep.OilandGas.PPDM39.Core.Metadata;
global using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
global using Beep.OilandGas.PPDM39.DataManagement.Core;
global using Beep.OilandGas.Models.Core.Interfaces;
global using Beep.OilandGas.Models.DTOs;
global using Beep.OilandGas.Models.HeatMap;
global using Microsoft.Extensions.Logging;
global using TheTechIdea.Beep.Editor;
global using TheTechIdea.Beep.DataBase;
global using SkiaSharp;

// Explicit type aliases to avoid conflicts
global using HeatMapConfigurationDto = Beep.OilandGas.Models.DTOs.HeatMapConfigurationDto;
global using HeatMapResultDto = Beep.OilandGas.Models.DTOs.HeatMapResultDto;
global using HeatMapDataPoint = Beep.OilandGas.Models.HeatMap.HeatMapDataPoint;
global using TheTechIdea.Beep.Report;


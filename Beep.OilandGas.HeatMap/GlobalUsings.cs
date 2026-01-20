global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using Beep.OilandGas.PPDM39.Repositories;
global using Beep.OilandGas.PPDM39.Core.Metadata;
global using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
global using Beep.OilandGas.PPDM39.DataManagement.Core;
global using Beep.OilandGas.Models.Core.Interfaces;
global using Beep.OilandGas.Models.Data;
global using Beep.OilandGas.Models.Data.HeatMap;
global using Microsoft.Extensions.Logging;
global using TheTechIdea.Beep.Editor;
global using TheTechIdea.Beep.DataBase;
global using SkiaSharp;

// Explicit type aliases to avoid conflicts
global using HeatMapConfigurationRecord = Beep.OilandGas.Models.Data.HeatMapConfigurationRecord;
global using HeatMapResult = Beep.OilandGas.Models.Data.HeatMapResult;
global using HeatMapDataPoint = Beep.OilandGas.Models.Data.HeatMap.HeatMapDataPoint;
global using TheTechIdea.Beep.Report;


global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using Beep.OilandGas.PPDM39.Repositories;
global using Beep.OilandGas.PPDM39.Core.Metadata;
global using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
global using Beep.OilandGas.PPDM39.DataManagement.Core;
global using Beep.OilandGas.Models.Core.Interfaces;
global using Microsoft.Extensions.Logging;
global using TheTechIdea.Beep.Editor;
global using TheTechIdea.Beep.DataBase;

// Resolve ambiguous type references - use DTO types from Models.DTOs namespace
global using ProspectDto = Beep.OilandGas.Models.DTOs.ProspectDto;
global using ProspectEvaluationDto = Beep.OilandGas.Models.DTOs.ProspectEvaluationDto;
global using ProspectRankingDto = Beep.OilandGas.Models.Core.Interfaces.ProspectRankingDto;
global using TheTechIdea.Beep.Report;


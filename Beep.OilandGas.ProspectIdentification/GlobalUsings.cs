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
global using Prospect = Beep.OilandGas.Models.Data.Prospect;
global using ProspectEvaluation = Beep.OilandGas.Models.Data.ProspectEvaluation;
global using ProspectRanking = Beep.OilandGas.Models.Core.Interfaces.ProspectRanking;
global using TheTechIdea.Beep.Report;


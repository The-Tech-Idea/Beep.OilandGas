using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.Web.Services;

public interface IWellStatusServiceClient
{
    Task<Dictionary<string, WELL_STATUS>> GetCurrentStatusAsync(string uwi, CancellationToken cancellationToken = default);
    Task<List<WellServices.FacetTypeDto>> GetFacetPageDataAsync(string uwi, CancellationToken cancellationToken = default);
    Task<List<WELL_STATUS>> GetStatusHistoryAsync(string uwi, CancellationToken cancellationToken = default);
    Task<List<WellServices.FacetQualifierDto>> GetQualifiersAsync(string statusType, string status, CancellationToken cancellationToken = default);
    Task<WELL_STATUS?> SetFacetAsync(string uwi, WellServices.SetFacetRequest request, CancellationToken cancellationToken = default);
}
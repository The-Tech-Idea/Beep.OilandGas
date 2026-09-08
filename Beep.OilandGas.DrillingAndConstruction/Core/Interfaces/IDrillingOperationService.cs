using Beep.OilandGas.Models.Data.Drilling;

namespace Beep.OilandGas.DrillingAndConstruction.Core.Interfaces;

public interface IDrillingOperationService
{
    Task<DRILLING_OPERATION> CreateDrillingOperationAsync(CREATE_DRILLING_OPERATION createDto, string? fieldId = null, string? userId = null, CancellationToken cancellationToken = default);
}

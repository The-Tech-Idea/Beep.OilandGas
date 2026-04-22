using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Web.Services;

public interface IAfeServiceClient
{
    Task<List<AfeSummaryModel>> GetAfesAsync(CancellationToken cancellationToken = default);
    Task<CreateAfeResponse?> CreateAfeAsync(CreateAFERequest request, CancellationToken cancellationToken = default);
    Task ApproveAfeAsync(string afeId, CancellationToken cancellationToken = default);
    Task RejectAfeAsync(string afeId, CancellationToken cancellationToken = default);
}
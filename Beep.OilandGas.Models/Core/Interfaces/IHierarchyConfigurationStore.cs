using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Core.Interfaces;

public interface IHierarchyConfigurationStore
{
    Task<List<HierarchyConfig>> ReadAsync(string organizationId, bool requireAdministrator = false, CancellationToken token = default);
    Task<bool> UpdateAsync(string organizationId, List<HierarchyConfig> configuration, CancellationToken token = default);
}

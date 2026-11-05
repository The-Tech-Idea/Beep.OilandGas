using global::Beep.OilandGas.Models.Data;

namespace TheTechIdea.Data.OilGas;

// Existing client selection owner, exposed as a narrow contract. A selector is never access authority.
public interface IFieldSelectionContext
{
    Task<string?> GetCurrentFieldIdAsync();
    Task<FieldResponse> GetCurrentFieldSelectionAsync(System.Threading.CancellationToken cancellationToken = default);
    Task<List<FieldListItem>> GetAvailableFieldsAsync(System.Threading.CancellationToken cancellationToken = default);
    Task<bool> SetCurrentFieldAsync(SetActiveFieldRequest request, System.Threading.CancellationToken cancellationToken = default);
    event Action<string>? CurrentFieldChanged;
}

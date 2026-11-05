using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data;

namespace TheTechIdea.Data.OilGas;

// An account preference review, never a FIELD grant. API reads/changes independently require live business authority.
public sealed record FieldSelectionReview(
    [property: JsonRequired] CompanyScope Company,
    [property: JsonRequired] AssetDatabaseScope Database,
    [property: JsonRequired] string ActorUserId,
    [property: JsonRequired] string Revision,
    [property: JsonRequired] string? FieldId);

public interface ICompanyFieldSelectionService
{
    Task<List<FieldListItem>> GetAvailableFieldsAsync(string? connectionName = null);
    Task<FieldResponse> GetCurrentFieldSelectionAsync();
    Task<SetActiveFieldResponse> SetCurrentFieldSelectionAsync(SetActiveFieldRequest request);
}

/// <summary>Revalidates the existing stored app preference without reading a native FIELD during another owner's command.</summary>
public interface ICompanyFieldSelectionAuthority
{
    Task RequireStoredSelectionAsync(FieldSelectionReview review);
}

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Grant admission prerequisite; existence does not establish legal ownership or operator authority.</summary>
public interface IAssetGrantTargetVerifier
{
    Task ValidateAsync(string databaseFingerprint, string assetType, string assetId, string accessLevel, string? facilityType = null);
}


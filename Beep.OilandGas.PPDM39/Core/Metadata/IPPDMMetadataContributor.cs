namespace Beep.OilandGas.PPDM39.Core.Metadata;

/// <summary>Contributes module-owned native metadata to the existing PPDM catalog.</summary>
public interface IPPDMMetadataContributor
{
    void AddTo(Dictionary<string, PPDMTableMetadata> metadata);
}

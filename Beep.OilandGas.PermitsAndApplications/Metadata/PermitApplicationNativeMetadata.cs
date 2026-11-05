using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
namespace Beep.OilandGas.PermitsAndApplications.Metadata;
/// <summary>Canonical identities for the active PERMITS manifest and its native document catalogs.</summary>
public sealed class PermitApplicationNativeMetadata : IPPDMMetadataContributor
{
    void IPPDMMetadataContributor.AddTo(Dictionary<string, PPDMTableMetadata> metadata) => AddTo(metadata);

    public static Dictionary<string,PPDMTableMetadata> AddTo(Dictionary<string,PPDMTableMetadata> metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        foreach(var type in PermitApplicationSchema.Entities)
        {
            if(metadata.ContainsKey(type.Name))throw new InvalidOperationException("PERMITS metadata already has another owner: "+type.Name);
            var parent=type.Name=="MIT_RESULT"?"INJECTION_PERMIT_APPLICATION":type.GetProperty("PERMIT_APPLICATION_ID") is not null && type.Name!="PERMIT_APPLICATION"?"PERMIT_APPLICATION":null;
            metadata.Add(type.Name,new PPDMTableMetadata {TableName=type.Name,EntityTypeName=type.FullName!,PrimaryKeyColumn=string.Join(',',PermitApplicationSchema.Keys(type)),Module="PERMITS",SubjectArea="Permits & Applications",
                CommonColumns=["ACTIVE_IND","ROW_CREATED_BY","ROW_CREATED_DATE","ROW_CHANGED_BY","ROW_CHANGED_DATE","PPDM_GUID","ROW_EFFECTIVE_DATE","ROW_EXPIRY_DATE","ROW_QUALITY"],
                ForeignKeys=parent is null?[]:[new(){ForeignKeyColumn=parent=="INJECTION_PERMIT_APPLICATION"?"INJECTION_PERMIT_APPLICATION_ID":"PERMIT_APPLICATION_ID",ReferencedTable=parent,ReferencedPrimaryKey="PERMIT_APPLICATION_ID"}]});
        }
        foreach(var type in new[]{typeof(SOURCE_DOCUMENT),typeof(RM_INFORMATION_ITEM),typeof(RM_FILE_CONTENT),typeof(R_DOCUMENT_TYPE),typeof(R_LANGUAGE),typeof(R_DIGITAL_FORMAT)})
        {
            if(!metadata.TryGetValue(type.Name,out var definition))continue; // Absent foundation metadata remains unavailable, never invented.
            if(definition.EntityTypeName!=type.Name && definition.EntityTypeName!=type.FullName)
                throw new InvalidOperationException("Native document metadata names a different entity: "+type.Name);
            definition.EntityTypeName=type.FullName!;
        }
        return metadata;
    }
}

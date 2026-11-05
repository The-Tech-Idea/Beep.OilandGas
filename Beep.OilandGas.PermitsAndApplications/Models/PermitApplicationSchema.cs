using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
namespace Beep.OilandGas.PermitsAndApplications.Models;
/// <summary>The module-owned PERMITS manifest shared by setup, runtime metadata and access policy.</summary>
public static class PermitApplicationSchema
{
    public static IReadOnlyList<Type> Entities {get;}=Array.AsReadOnly(new[]{typeof(PERMIT_APPLICATION),typeof(PERMIT_APPLICATION_COMMAND),typeof(PERMIT_DOCUMENT_STAGE),typeof(PERMIT_STATUS_HISTORY),typeof(DRILLING_PERMIT_APPLICATION),typeof(ENVIRONMENTAL_PERMIT_APPLICATION),typeof(INJECTION_PERMIT_APPLICATION),typeof(JURISDICTION_REQUIREMENTS),typeof(MIT_RESULT),typeof(REQUIRED_FORM),typeof(APPLICATION_ATTACHMENT),typeof(R_PERMITS_REFERENCE_CODE)});
    public static IReadOnlyList<string> Keys(Type type)
    {
        if(!Entities.Contains(type))throw new InvalidOperationException("The entity is not owned by the PERMITS manifest.");
        var keys=type.GetProperties().Where(p=>Attribute.IsDefined(p,typeof(KeyAttribute)))
            .OrderBy(p=>p.GetCustomAttributes(typeof(ColumnAttribute),true).Cast<ColumnAttribute>().SingleOrDefault()?.Order??0)
            .ThenBy(p=>p.MetadataToken).Select(p=>p.Name).ToArray();
        if(keys.Length==0)throw new InvalidOperationException("Every PERMITS entity requires explicit canonical keys.");
        return Array.AsReadOnly(keys);
    }
}

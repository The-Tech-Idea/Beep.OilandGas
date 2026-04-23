using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Scope;

public class OrganizationScope : ModelEntityBase
{
    [Key]
    public string ORG_SCOPE_ID { get; set; } = string.Empty;
    public string ORGANIZATION_ID { get; set; } = string.Empty;
    public string? ORG_NAME { get; set; }
    public string? TENANT_ID { get; set; }
    public string? PARENT_ORGANIZATION_ID { get; set; }
    public string? HIERARCHY_TYPE { get; set; }
    public int? HIERARCHY_LEVEL { get; set; }
    public string? ORG_BOUNDARY_TYPE { get; set; }
    public string? ORG_FULL_PATH { get; set; }
}

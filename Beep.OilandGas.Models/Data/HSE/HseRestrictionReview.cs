using Beep.OilandGas.Models.Data.AccessControl;
namespace Beep.OilandGas.Models.Data.HSE;

// Read-only qualified evidence for explicit company configuration, never an automatic grant.
public sealed record HseRestrictionReview(CompanyScope Company, AssetDatabaseScope Database, string FieldId, string IncidentId,
    HseRestrictionDefinitionReview[] Restrictions);
public sealed record HseRestrictionDefinitionReview(string RestrictionId, decimal RestrictionVersion, string DefinitionFingerprint);

using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Validates the physical columns needed for one exact incident report.</summary>
internal static class IncidentReportSchema
{
    public static void Validate(IDataSource source, IncidentReportIntent intent, string sourceName)
    {
        var review = intent.Context; var id = intent.IncidentId(); var actor = review.ActorUserId;
        void Table(Type type, string[] keys, params (string Name, Type Type, int Width)[] required)
        {
            var schema = source.GetEntityStructure(new EntityStructure { EntityName = type.Name, DatasourceEntityName = type.Name }, true);
            if (schema?.Fields is null || !schema.Fields.Where(f => f.IsKey).Select(f => f.FieldName).Order(StringComparer.Ordinal).SequenceEqual(keys.Order(StringComparer.Ordinal)))
                throw new NativeSchemaCompatibilityException($"The {type.Name} primary key is unavailable or differs from its canonical definition.");
            foreach (var (name, fieldType, width) in required.Concat(new[] { ("ROW_CREATED_BY", typeof(string), actor.Length), ("ROW_CREATED_DATE", typeof(DateTime), 0), ("ROW_CHANGED_BY", typeof(string), actor.Length), ("ROW_CHANGED_DATE", typeof(DateTime), 0) }))
            {
                var fields = schema.Fields.Where(f => f.FieldName == name).ToArray();
                if (fields.Length != 1 || fields[0].Fieldtype != fieldType.FullName || width > 0 && Math.Max(fields[0].Size, fields[0].Size1) < width)
                    throw new NativeSchemaCompatibilityException($"The {type.Name}.{name} column cannot preserve this incident report.");
            }
        }
        var text = typeof(string); var number = typeof(decimal); var date = typeof(DateTime);
        Table(typeof(HSE_INCIDENT_REPORT_INTENT), ["HSE_INCIDENT_REPORT_INTENT_ID"], ("HSE_INCIDENT_REPORT_INTENT_ID", text, 32), ("INCIDENT_ID", text, id.Length), ("OWNER_TENANT_ID", text, review.Company.TenantId.Length), ("OWNER_COMPANY_BA_ID", text, review.Company.BusinessAssociateId.Length), ("FIELD_ID", text, review.FieldId!.Length), ("COMMAND_JSON", text, HSE_INCIDENT_REPORT_INTENT.Create(intent).COMMAND_JSON.Length), ("COMMAND_HASH", text, 64), ("ROW_CREATED_BY", text, actor.Length), ("ROW_CREATED_DATE", date, 0), ("ROW_CHANGED_BY", text, actor.Length), ("ROW_CHANGED_DATE", date, 0), ("ACTIVE_IND", text, 1));
        Table(typeof(HSE_INCIDENT), ["INCIDENT_ID"], ("INCIDENT_ID", text, id.Length), ("INCIDENT_DATE", date, 0), ("REMARK", text, intent.Description.Length), ("ACTIVE_IND", text, 1), ("ROW_CREATED_BY", text, actor.Length), ("SOURCE", text, sourceName.Length));
        Table(typeof(HSE_INCIDENT_DETAIL), ["INCIDENT_ID", "DETAIL_OBS_NO"], ("INCIDENT_ID", text, id.Length), ("DETAIL_OBS_NO", number, 0), ("INCIDENT_SET_ID", text, intent.Option.IncidentSetId.Length), ("INCIDENT_TYPE_ID", text, intent.Option.IncidentTypeId.Length), ("INCIDENT_SEVERITY_ID", text, intent.Option.SeverityId.Length), ("DETAIL_TYPE", text, 7), ("REMARK", text, intent.Description.Length), ("INCIDENT_DATE", date, 0), ("ACTIVE_IND", text, 1), ("ROW_CREATED_BY", text, actor.Length), ("SOURCE", text, sourceName.Length));
        Table(typeof(HSE_INCIDENT_COMPONENT), ["INCIDENT_ID", "COMPONENT_OBS_NO"], ("INCIDENT_ID", text, id.Length), ("COMPONENT_OBS_NO", number, 0), ("FIELD_ID", text, review.FieldId!.Length), ("COMPONENT_ROLE", text, HseReferenceCodes.FieldContext.PrimaryRole.Length), ("COMPONENT_TYPE", text, HseReferenceCodes.FieldContext.ComponentType.Length), ("REMARK", text, intent.Location.Length), ("JURISDICTION", text, 0), ("ACTIVE_IND", text, 1), ("ROW_CREATED_BY", text, actor.Length));
        Table(typeof(HSE_INCIDENT_RESPONSE), ["INCIDENT_ID", "RESPONSE_OBS_NO"], ("INCIDENT_ID", text, id.Length), ("RESPONSE_OBS_NO", number, 0), ("RESPONSE_TYPE", text, 6), ("RESPONSE_RESULT", text, IncidentState.Reported.Length), ("EFFECTIVE_DATE", date, 0), ("ACTIVE_IND", text, 1), ("ROW_CREATED_BY", text, actor.Length), ("SOURCE", text, sourceName.Length));
    }
}

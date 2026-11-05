using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ProductionOperations.Services;

public sealed partial class FacilityManagementService
{
    public Task<CompanyMaintenanceQualifiedLicensing> ReadMaintenanceQualifiedLicensingAsync(FieldSelectionReview context,WorkOrderDetailModel order,CompanyMaintenanceQualifiedAssets identity,Func<Task> authority) =>
        ReadMaintenanceQualifiedLicensingAsync(context,order,identity,authority,false);

    internal async Task<CompanyMaintenanceQualifiedLicensing> ReadMaintenanceQualifiedLicensingAsync(FieldSelectionReview context,WorkOrderDetailModel order,CompanyMaintenanceQualifiedAssets identity,Func<Task> authority,bool owned)
    {
        identity.Require(order);if(identity.Context!=context || context.Database.ConnectionName!=_connectionName || _maintenanceAssetAccess is null)throw new UnauthorizedAccessException("Original qualified native identities and parent READ authority are required before licensing evidence.");
        var baseline=identity with{Operating=null,Licensing=null};var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original native licensing source is unavailable.");
        async Task Guard(){RequireMaintenanceEvidenceLane(source,owned);await authority();RequireMaintenanceEvidenceLane(source,owned);if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)) || CompanyWorkOrderExecutionRequirements.OrderRevision(order)!=baseline.OrderRevision)throw new InvalidOperationException("Original licensing source/component scope changed.");}
        async Task Parents(){await Guard();var current=await ReadMaintenanceQualifiedAssetsAsync(context,order,authority,owned);if(current.IdentityRevision()!=baseline.IdentityRevision())throw new InvalidOperationException("Original native parent ownership/installation/authority changed around licensing evidence.");await Guard();}
        async Task<CompanyMaintenanceQualifiedLicensing> Observe()
        {
            await Parents();var definitions=new Dictionary<Type,(string[] Keys,(string Column,string Table,string Key)[] Relationships)>();
            async Task Definitions(IReadOnlyList<MaintenanceAssetDefinition> values)
            {
                await MaintenanceAssetDefinitionsAsync(source,Guard,values);
                foreach(var value in values)definitions[value.Type]=(value.Keys,value.Relationships);
            }
            Task Definition(Type type,string[] keys,params (string Column,string Table,string Key)[] relationships)=>Definitions([new(type,keys,relationships)]);
            Task<T[]> Rows<T>(string[] required,params AppFilter[] filters) where T:class=>MaintenanceAssetRowsCoreAsync<T>(Guard,required,owned,filters);
            static AppFilter Equal(string column,string value)=>new(){FieldName=column,Operator="=",FilterValue=value};
            var licenses=new List<CompanyMaintenanceFacilityLicense>();var statuses=new List<CompanyMaintenanceLicenseStatus>();var conditions=new List<CompanyMaintenanceLicenseCondition>();var violations=new List<CompanyMaintenanceLicenseViolation>();var references=new List<CompanyMaintenanceLicenseReference>();var referenceKeys=new HashSet<(string,string,string?)>();
            var areas=new List<CompanyMaintenanceLicenseArea>();var areaReferences=new Dictionary<(string,string),CompanyMaintenanceLicenseAreaReference?>();
            var units=new Dictionary<string,CompanyMaintenanceLicenseUnitReference?>();var restrictions=new Dictionary<(string,decimal),CompanyMaintenanceLicenseRestriction?>();
            void Bound(){if(licenses.Count+statuses.Count+conditions.Count+violations.Count+references.Count+areas.Count+areas.Count(row=>row.Area is not null)+conditions.Sum(row=>(row.Terms?.ValueUnit is null?0:1)+(row.Terms?.DueTermUnit is null?0:1)+(row.Terms?.Restriction is null?0:1))>2048)throw new InvalidOperationException("Native licensing scope exceeds this complete bounded review; no rows are truncated.");}
            DateTime? Utc(DateTime? value,ref bool confirmed){try{return MaintenanceIdentityUtc(value);}catch(InvalidOperationException){confirmed=false;return null;}}
            async Task Reference<T>(string[] keys,string? first,string? second=null,params (string Column,string Table,string Key)[] relationships) where T:class
            {
                if(!LeaseSummaryValidator.Identifier(first) || keys.Length==2 && !LeaseSummaryValidator.Identifier(second) || !referenceKeys.Add((typeof(T).Name,first!,second)))return;
                await Definition(typeof(T),keys,relationships);var filters=new List<AppFilter>{Equal(keys[0],first!)};if(keys.Length==2)filters.Add(Equal(keys[1],second!));var rows=await Rows<T>([..keys,"ACTIVE_IND"],filters.ToArray());if(rows.Length>1)throw new InvalidOperationException("Native licensing reference compound identity is ambiguous.");
                foreach(var row in rows){var temporal=true;var from=Utc((DateTime?)typeof(T).GetProperty("EFFECTIVE_DATE")!.GetValue(row),ref temporal);var to=Utc((DateTime?)typeof(T).GetProperty("EXPIRY_DATE")!.GetValue(row),ref temporal);references.Add(new(typeof(T).Name,first!,second,(string)typeof(T).GetProperty("ACTIVE_IND")!.GetValue(row)!,from,to,temporal,MaintenanceIdentityRevision(row)));Bound();}
            }
            async Task<CompanyMaintenanceLicenseUnitReference?> Unit(string? key)
            {
                if(!LeaseSummaryValidator.Identifier(key))return null;if(units.TryGetValue(key!,out var found))return found;
                await Definition(typeof(PPDM_UNIT_OF_MEASURE),["UOM_ID"],("UOM_QUANTITY_TYPE","PPDM_QUANTITY","QUANTITY_TYPE"),("UOM_SYSTEM_ID","PPDM_MEASUREMENT_SYSTEM","UOM_SYSTEM_ID"),("UOM_USAGE_TYPE","R_PPDM_UOM_USAGE","UOM_USAGE_TYPE"));
                var rows=await Rows<PPDM_UNIT_OF_MEASURE>(["UOM_ID","ACTIVE_IND"],Equal("UOM_ID",key!));if(rows.Length>1)throw new InvalidOperationException("Native condition unit identity is ambiguous.");
                if(rows.Length==1)
                {
                    var row=rows[0];var valid=true;var from=Utc(row.EFFECTIVE_DATE,ref valid);var to=Utc(row.EXPIRY_DATE,ref valid);found=new(row.UOM_ID,row.UOM_NAME,row.UOM_QUANTITY_TYPE,row.UOM_SYSTEM_ID,row.UOM_USAGE_TYPE,row.ACTIVE_IND,from,to,row.SOURCE,row.SOURCE_DOCUMENT_ID,valid,MaintenanceIdentityRevision(row));
                    await Reference<PPDM_QUANTITY>(["QUANTITY_TYPE"],row.UOM_QUANTITY_TYPE);await Reference<PPDM_MEASUREMENT_SYSTEM>(["UOM_SYSTEM_ID"],row.UOM_SYSTEM_ID);await Reference<R_PPDM_UOM_USAGE>(["UOM_USAGE_TYPE"],row.UOM_USAGE_TYPE);
                }
                units.Add(key!,found);return found;
            }
            async Task<CompanyMaintenanceLicenseRestriction?> Restriction(string? key,decimal? version)
            {
                if(!LeaseSummaryValidator.Identifier(key) || version is null)return null;if(restrictions.TryGetValue((key!,version.Value),out var found))return found;
                await Definition(typeof(RESTRICTION),["RESTRICTION_ID","RESTRICTION_VERSION"],("RESTRICTION_CLASS","REST_CLASS","RESTRICTION_CLASS_ID"),("RESTRICTION_TYPE","R_REST_TYPE","RESTRICTION_TYPE"),("REST_DURATION_TYPE","R_REST_DURATION","REST_DURATION_TYPE"));
                var rows=await Rows<RESTRICTION>(["RESTRICTION_ID","RESTRICTION_VERSION","ACTIVE_IND"],Equal("RESTRICTION_ID",key!),Equal("RESTRICTION_VERSION",version.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)));if(rows.Length>1)throw new InvalidOperationException("Native condition restriction compound identity is ambiguous.");
                if(rows.Length==1)
                {
                    var row=rows[0];var valid=true;var from=Utc(row.EFFECTIVE_DATE,ref valid);var to=Utc(row.EXPIRY_DATE,ref valid);var start=Utc(row.START_DATE,ref valid);var end=Utc(row.END_DATE,ref valid);var startEvent=Utc(row.START_DATE_EVENT,ref valid);var endEvent=Utc(row.END_DATE_EVENT,ref valid);found=new(row.RESTRICTION_ID,row.RESTRICTION_VERSION,row.RESTRICTION_NAME,row.DESCRIPTION,row.REGULATORY_ACT,row.RESTRICTION_CLASS,row.RESTRICTION_TYPE,row.REST_DURATION_TYPE,row.ACTIVE_IND,from,to,start,end,startEvent,endEvent,row.SOURCE,row.SOURCE_DOCUMENT_ID,valid,MaintenanceIdentityRevision(row));
                    await Reference<REST_CLASS>(["RESTRICTION_CLASS_ID"],row.RESTRICTION_CLASS);await Reference<R_REST_TYPE>(["RESTRICTION_TYPE"],row.RESTRICTION_TYPE);await Reference<R_REST_DURATION>(["REST_DURATION_TYPE"],row.REST_DURATION_TYPE);
                }
                restrictions.Add((key!,version.Value),found);return found;
            }
            if(baseline.Facilities.Count>0)
            {
                await Definitions([
                    new(typeof(FACILITY_LICENSE),["FACILITY_ID","FACILITY_TYPE","LICENSE_ID"],[("FACILITY_ID","FACILITY","FACILITY_ID"),("GRANTED_BY_BA_ID","BUSINESS_ASSOCIATE","BUSINESS_ASSOCIATE_ID"),("GRANTED_TO_BA_ID","BUSINESS_ASSOCIATE","BUSINESS_ASSOCIATE_ID"),("ASSIGNED_FIELD_ID","FIELD","FIELD_ID")]),
                    new(typeof(FACILITY_LIC_STATUS),["FACILITY_ID","FACILITY_TYPE","LICENSE_ID","STATUS_ID"],[("FACILITY_ID","FACILITY_LICENSE","FACILITY_ID"),("LICENSE_STATUS_TYPE","R_LICENSE_STATUS","LICENSE_STATUS_TYPE")]),
                    new(typeof(FACILITY_LIC_COND),["FACILITY_ID","FACILITY_TYPE","LICENSE_ID","CONDITION_ID"],[("FACILITY_ID","FACILITY_LICENSE","FACILITY_ID"),("CONDITION_TYPE","R_FAC_LIC_COND","CONDITION_TYPE"),("RESTRICTION_ID","RESTRICTION","RESTRICTION_ID"),("PRODUCT_TYPE","SUBSTANCE","SUBSTANCE_ID"),("FULFILLED_BY_BA_ID","BUSINESS_ASSOCIATE","BUSINESS_ASSOCIATE_ID")]),
                    new(typeof(FACILITY_LIC_VIOLATION),["FACILITY_ID","FACILITY_TYPE","LICENSE_ID","VIOLATION_ID"],[("FACILITY_ID","FACILITY_LICENSE","FACILITY_ID"),("VIOLATION_TYPE","R_FAC_LIC_VIOLATION_TYPE","VIOLATION_TYPE")]),
                    new(typeof(FACILITY_LIC_AREA),["FACILITY_ID","FACILITY_TYPE","LICENSE_ID","AREA_ID","AREA_TYPE"],[("FACILITY_ID","FACILITY_LICENSE","FACILITY_ID"),("AREA_ID","AREA","AREA_ID")])]);
            }
            foreach(var facility in baseline.Facilities)
            {
                var parent=await Rows<FACILITY>(["FACILITY_ID","FACILITY_TYPE","CURRENT_OPERATOR","PRIMARY_FIELD_ID"],Equal("FACILITY_ID",facility.FacilityId),Equal("FACILITY_TYPE",facility.FacilityType),Equal("ACTIVE_IND","Y"));if(parent.Length!=1 || MaintenanceIdentityRevision(parent[0])!=facility.NativeRevision || await _maintenanceAssetAccess.GetEffectiveAccessLevelAsync(typeof(FACILITY),"FACILITY",parent[0],parent[0].CURRENT_OPERATOR==context.Company.BusinessAssociateId)!=facility.AccessLevel)throw new UnauthorizedAccessException("Original qualified parent source/READ authority changed before licence reads.");await Guard();
                var native=await Rows<FACILITY_LICENSE>(["FACILITY_ID","FACILITY_TYPE","LICENSE_ID","ACTIVE_IND"],Equal("FACILITY_ID",facility.FacilityId),Equal("FACILITY_TYPE",facility.FacilityType));
                foreach(var row in native.OrderBy(row=>row.LICENSE_ID,StringComparer.Ordinal))
                {
                    var temporal=true;var granted=Utc(row.GRANTED_DATE,ref temporal);var from=Utc(row.EFFECTIVE_DATE,ref temporal);var expiry=Utc(row.EXPIRY_DATE,ref temporal);licenses.Add(new(row.FACILITY_ID,row.FACILITY_TYPE,row.LICENSE_ID,row.LICENSE_NUM,row.FACILITY_LICENSE_TYPE,row.GRANTED_BY_BA_ID,row.GRANTED_TO_BA_ID,row.ASSIGNED_FIELD_ID,row.ACTIVE_IND,granted,from,expiry,row.VIOLATION_IND,row.FEES_PAID_IND,row.SOURCE,temporal,MaintenanceIdentityRevision(row)));Bound();
                    await Reference<BUSINESS_ASSOCIATE>(["BUSINESS_ASSOCIATE_ID"],row.GRANTED_BY_BA_ID);await Reference<BUSINESS_ASSOCIATE>(["BUSINESS_ASSOCIATE_ID"],row.GRANTED_TO_BA_ID);await Reference<FACILITY_LIC_TYPE>(["GRANTED_BY_BA_ID","FACILITY_LICENSE_TYPE"],row.GRANTED_BY_BA_ID,row.FACILITY_LICENSE_TYPE,("GRANTED_BY_BA_ID","BUSINESS_ASSOCIATE","BUSINESS_ASSOCIATE_ID"));
                    AppFilter[] filters=[Equal("FACILITY_ID",row.FACILITY_ID),Equal("FACILITY_TYPE",row.FACILITY_TYPE),Equal("LICENSE_ID",row.LICENSE_ID)];
                    foreach(var assignment in await Rows<FACILITY_LIC_AREA>(["FACILITY_ID","FACILITY_TYPE","LICENSE_ID","AREA_ID","AREA_TYPE","ACTIVE_IND"],filters))
                    {
                        if(!LeaseSummaryValidator.Identifier(assignment.AREA_ID) || !LeaseSummaryValidator.Identifier(assignment.AREA_TYPE))throw new InvalidOperationException("Exact native licence-area keys are required before referenced area reads.");
                        if(!areaReferences.TryGetValue((assignment.AREA_ID,assignment.AREA_TYPE),out var area))
                        {
                            await Definition(typeof(AREA),["AREA_ID","AREA_TYPE"],("AREA_TYPE","R_AREA_TYPE","AREA_TYPE"),("SOURCE_DOCUMENT_ID","SOURCE_DOCUMENT","SOURCE_DOCUMENT_ID"));
                            var nativeAreas=await Rows<AREA>(["AREA_ID","AREA_TYPE","ACTIVE_IND"],Equal("AREA_ID",assignment.AREA_ID),Equal("AREA_TYPE",assignment.AREA_TYPE));if(nativeAreas.Length>1)throw new InvalidOperationException("Native licence area compound identity is ambiguous.");
                            if(nativeAreas.Length==1){var nativeArea=nativeAreas[0];var confirmed=true;var effective=Utc(nativeArea.EFFECTIVE_DATE,ref confirmed);var expires=Utc(nativeArea.EXPIRY_DATE,ref confirmed);area=new(nativeArea.AREA_ID,nativeArea.AREA_TYPE,nativeArea.PREFERRED_NAME,nativeArea.ACTIVE_IND,effective,expires,nativeArea.SOURCE,nativeArea.SOURCE_DOCUMENT_ID,confirmed,MaintenanceIdentityRevision(nativeArea));}
                            areaReferences.Add((assignment.AREA_ID,assignment.AREA_TYPE),area);
                            await Reference<R_AREA_TYPE>(["AREA_TYPE"],assignment.AREA_TYPE);
                        }
                        var valid=true;var fromArea=Utc(assignment.EFFECTIVE_DATE,ref valid);var toArea=Utc(assignment.EXPIRY_DATE,ref valid);areas.Add(new(assignment.FACILITY_ID,assignment.FACILITY_TYPE,assignment.LICENSE_ID,assignment.AREA_ID,assignment.AREA_TYPE,assignment.DESCRIPTION,assignment.ACTIVE_IND,fromArea,toArea,assignment.SOURCE,valid,MaintenanceIdentityRevision(assignment),area));Bound();
                    }
                    foreach(var status in await Rows<FACILITY_LIC_STATUS>(["FACILITY_ID","FACILITY_TYPE","LICENSE_ID","STATUS_ID","LICENSE_STATUS_TYPE","LICENSE_STATUS","ACTIVE_IND"],filters))
                    {
                        var valid=true;var effective=Utc(status.EFFECTIVE_DATE,ref valid);var expires=Utc(status.EXPIRY_DATE,ref valid);statuses.Add(new(status.FACILITY_ID,status.FACILITY_TYPE,status.LICENSE_ID,status.STATUS_ID,status.LICENSE_STATUS_TYPE,status.LICENSE_STATUS,status.ACTIVE_IND,effective,expires,status.SOURCE,valid,MaintenanceIdentityRevision(status)));Bound();
                        await Reference<R_LIC_STATUS_TYPE>(["LICENSE_STATUS_TYPE"],status.LICENSE_STATUS_TYPE);await Reference<R_LICENSE_STATUS>(["LICENSE_STATUS_TYPE","LICENSE_STATUS"],status.LICENSE_STATUS_TYPE,status.LICENSE_STATUS,("LICENSE_STATUS_TYPE","R_LIC_STATUS_TYPE","LICENSE_STATUS_TYPE"));
                    }
                    foreach(var condition in await Rows<FACILITY_LIC_COND>(["FACILITY_ID","FACILITY_TYPE","LICENSE_ID","CONDITION_ID","ACTIVE_IND"],filters))
                    {
                        var terms=new CompanyMaintenanceLicenseConditionTerms(condition.CONDITION_VALUE,condition.CONDITION_VALUE_UOM,condition.DUE_TERM,condition.DUE_TERM_UOM,condition.DUE_FREQUENCY,condition.PRODUCT_TYPE,condition.RESTRICTION_ID,condition.RESTRICTION_VERSION,await Unit(condition.CONDITION_VALUE_UOM),await Unit(condition.DUE_TERM_UOM),await Restriction(condition.RESTRICTION_ID,condition.RESTRICTION_VERSION));
                        var valid=true;var due=Utc(condition.DUE_DATE,ref valid);var fulfilled=Utc(condition.FULFILLED_DATE,ref valid);var effective=Utc(condition.EFFECTIVE_DATE,ref valid);var expires=Utc(condition.EXPIRY_DATE,ref valid);conditions.Add(new(condition.FACILITY_ID,condition.FACILITY_TYPE,condition.LICENSE_ID,condition.CONDITION_ID,condition.CONDITION_TYPE,condition.CONDITION_CODE,condition.DESCRIPTION,condition.DUE_CONDITION,condition.FULFILLED_IND,condition.EXEMPT_IND,due,fulfilled,effective,expires,condition.ACTIVE_IND,condition.SOURCE,valid,MaintenanceIdentityRevision(condition)){Terms=terms,FulfilledByBaId=condition.FULFILLED_BY_BA_ID});Bound();
                        await Reference<R_FAC_LIC_COND>(["CONDITION_TYPE"],condition.CONDITION_TYPE);await Reference<R_FAC_LIC_COND_CODE>(["CONDITION_TYPE","CONDITION_CODE"],condition.CONDITION_TYPE,condition.CONDITION_CODE);await Reference<R_FAC_LIC_DUE_CONDITION>(["DUE_CONDITION"],condition.DUE_CONDITION);
                        await Reference<SUBSTANCE>(["SUBSTANCE_ID"],condition.PRODUCT_TYPE);await Reference<BUSINESS_ASSOCIATE>(["BUSINESS_ASSOCIATE_ID"],condition.FULFILLED_BY_BA_ID);
                    }
                    foreach(var violation in await Rows<FACILITY_LIC_VIOLATION>(["FACILITY_ID","FACILITY_TYPE","LICENSE_ID","VIOLATION_ID","ACTIVE_IND"],filters))
                    {
                        var valid=true;var occurred=Utc(violation.VIOLATION_DATE,ref valid);var resolved=Utc(violation.RESOLVE_DATE,ref valid);var effective=Utc(violation.EFFECTIVE_DATE,ref valid);var expires=Utc(violation.EXPIRY_DATE,ref valid);violations.Add(new(violation.FACILITY_ID,violation.FACILITY_TYPE,violation.LICENSE_ID,violation.VIOLATION_ID,violation.CONDITION_ID,violation.VIOLATION_TYPE,violation.RESOLVE_TYPE,occurred,resolved,effective,expires,violation.ACTIVE_IND,violation.SOURCE,valid,MaintenanceIdentityRevision(violation)));Bound();
                        await Reference<R_FAC_LIC_VIOLATION_TYPE>(["VIOLATION_TYPE"],violation.VIOLATION_TYPE);await Reference<R_FAC_LIC_VIOL_RESOL>(["RESOLVE_TYPE"],violation.RESOLVE_TYPE);
                    }
                }
            }
            // The next observation's opening Parents(), or the final Parents(), closes this complete observation.
            await MaintenanceAssetDefinitionsAsync(source,Guard,definitions.Select(definition=>new MaintenanceAssetDefinition(definition.Key,definition.Value.Keys,definition.Value.Relationships)).ToArray());
            var utc=DateTime.UtcNow;var result=new CompanyMaintenanceQualifiedLicensing(context,baseline.IdentityRevision(),licenses,statuses,conditions,violations,references,utc,CompanyMaintenanceQualifiedLicensing.GetBlockers(baseline,licenses,statuses,conditions,violations,references,utc,areas)){Areas=areas};result.Require(baseline);await Guard();return result;
        }
        var first=await Observe();var closing=await Observe();if(first.SourceRevision()!=closing.SourceRevision())throw new InvalidOperationException("Complete qualified licensing/status/condition/violation/reference evidence changed during review.");await Parents();await Guard();return closing;
    }
}

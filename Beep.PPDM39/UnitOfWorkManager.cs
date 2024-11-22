using System;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using Beep.PPDM39.Models;
namespace Beep.PPDM39.unitofwork
{
    public partial class UnitOfWorkManager
    {
        public IUnitofWork<Entity> GetEntityUnitofWork(string entityname, IDMEEditor editor, string datasourceName)
        {
            //var ef =new  UnitofWork<ANL_ACCURACY>(editor, entityname, datasourceName);
            
            Type type;
            switch (entityname)
            {

                case "ANL_ACCURACY":
                    type = typeof(ANL_ACCURACY);
                    break;


                case "ANL_ANALYSIS_BATCH":
                    type = typeof(ANL_ANALYSIS_BATCH);
                    break;


                case "ANL_ANALYSIS_REPORT":
                    type = typeof(ANL_ANALYSIS_REPORT);
                    break;


                case "ANL_ANALYSIS_STEP":
                    type = typeof(ANL_ANALYSIS_STEP);
                    break;


                case "ANL_BA":
                    type = typeof(ANL_BA);
                    break;


                case "ANL_CALC_ALIAS":
                    type = typeof(ANL_CALC_ALIAS);
                    break;


                case "ANL_CALC_EQUIV":
                    type = typeof(ANL_CALC_EQUIV);
                    break;


                case "ANL_CALC_FORMULA":
                    type = typeof(ANL_CALC_FORMULA);
                    break;


                case "ANL_CALC_METHOD":
                    type = typeof(ANL_CALC_METHOD);
                    break;


                case "ANL_CALC_SET":
                    type = typeof(ANL_CALC_SET);
                    break;


                case "ANL_COAL_RANK":
                    type = typeof(ANL_COAL_RANK);
                    break;


                case "ANL_COAL_RANK_SCHEME":
                    type = typeof(ANL_COAL_RANK_SCHEME);
                    break;


                case "ANL_COMPONENT":
                    type = typeof(ANL_COMPONENT);
                    break;


                case "ANL_DETAIL":
                    type = typeof(ANL_DETAIL);
                    break;


                case "ANL_ELEMENTAL":
                    type = typeof(ANL_ELEMENTAL);
                    break;


                case "ANL_ELEMENTAL_DETAIL":
                    type = typeof(ANL_ELEMENTAL_DETAIL);
                    break;


                case "ANL_EQUIP":
                    type = typeof(ANL_EQUIP);
                    break;


                case "ANL_FLUOR":
                    type = typeof(ANL_FLUOR);
                    break;


                case "ANL_GAS_ANALYSIS":
                    type = typeof(ANL_GAS_ANALYSIS);
                    break;


                case "ANL_GAS_CHRO":
                    type = typeof(ANL_GAS_CHRO);
                    break;


                case "ANL_GAS_COMPOSITION":
                    type = typeof(ANL_GAS_COMPOSITION);
                    break;


                case "ANL_GAS_DETAIL":
                    type = typeof(ANL_GAS_DETAIL);
                    break;


                case "ANL_GAS_HEAT":
                    type = typeof(ANL_GAS_HEAT);
                    break;


                case "ANL_GAS_PRESS":
                    type = typeof(ANL_GAS_PRESS);
                    break;


                case "ANL_ISOTOPE":
                    type = typeof(ANL_ISOTOPE);
                    break;


                case "ANL_ISOTOPE_STD":
                    type = typeof(ANL_ISOTOPE_STD);
                    break;


                case "ANL_LIQUID_CHRO":
                    type = typeof(ANL_LIQUID_CHRO);
                    break;


                case "ANL_LIQUID_CHRO_DETAIL":
                    type = typeof(ANL_LIQUID_CHRO_DETAIL);
                    break;


                case "ANL_MACERAL":
                    type = typeof(ANL_MACERAL);
                    break;


                case "ANL_MACERAL_MATURITY":
                    type = typeof(ANL_MACERAL_MATURITY);
                    break;


                case "ANL_METHOD":
                    type = typeof(ANL_METHOD);
                    break;


                case "ANL_METHOD_ALIAS":
                    type = typeof(ANL_METHOD_ALIAS);
                    break;


                case "ANL_METHOD_EQUIV":
                    type = typeof(ANL_METHOD_EQUIV);
                    break;


                case "ANL_METHOD_PARM":
                    type = typeof(ANL_METHOD_PARM);
                    break;


                case "ANL_METHOD_SET":
                    type = typeof(ANL_METHOD_SET);
                    break;


                case "ANL_OIL_ANALYSIS":
                    type = typeof(ANL_OIL_ANALYSIS);
                    break;


                case "ANL_OIL_DETAIL":
                    type = typeof(ANL_OIL_DETAIL);
                    break;


                case "ANL_OIL_DISTILL":
                    type = typeof(ANL_OIL_DISTILL);
                    break;


                case "ANL_OIL_FRACTION":
                    type = typeof(ANL_OIL_FRACTION);
                    break;


                case "ANL_OIL_VISCOSITY":
                    type = typeof(ANL_OIL_VISCOSITY);
                    break;


                case "ANL_PALEO":
                    type = typeof(ANL_PALEO);
                    break;


                case "ANL_PALEO_MATURITY":
                    type = typeof(ANL_PALEO_MATURITY);
                    break;


                case "ANL_PARM":
                    type = typeof(ANL_PARM);
                    break;


                case "ANL_PROBLEM":
                    type = typeof(ANL_PROBLEM);
                    break;


                case "ANL_PYROLYSIS":
                    type = typeof(ANL_PYROLYSIS);
                    break;


                case "ANL_QGF_TSF":
                    type = typeof(ANL_QGF_TSF);
                    break;


                case "ANL_REMARK":
                    type = typeof(ANL_REMARK);
                    break;


                case "ANL_REPORT_ALIAS":
                    type = typeof(ANL_REPORT_ALIAS);
                    break;


                case "ANL_SAMPLE":
                    type = typeof(ANL_SAMPLE);
                    break;


                case "ANL_STEP_XREF":
                    type = typeof(ANL_STEP_XREF);
                    break;


                case "ANL_TABLE_RESULT":
                    type = typeof(ANL_TABLE_RESULT);
                    break;


                case "ANL_VALID_BA":
                    type = typeof(ANL_VALID_BA);
                    break;


                case "ANL_VALID_EQUIP":
                    type = typeof(ANL_VALID_EQUIP);
                    break;


                case "ANL_VALID_MEASURE":
                    type = typeof(ANL_VALID_MEASURE);
                    break;


                case "ANL_VALID_PROBLEM":
                    type = typeof(ANL_VALID_PROBLEM);
                    break;


                case "ANL_VALID_TABLE_RESULT":
                    type = typeof(ANL_VALID_TABLE_RESULT);
                    break;


                case "ANL_VALID_TOLERANCE":
                    type = typeof(ANL_VALID_TOLERANCE);
                    break;


                case "ANL_WATER_ANALYSIS":
                    type = typeof(ANL_WATER_ANALYSIS);
                    break;


                case "ANL_WATER_DETAIL":
                    type = typeof(ANL_WATER_DETAIL);
                    break;


                case "ANL_WATER_SALINITY":
                    type = typeof(ANL_WATER_SALINITY);
                    break;


                case "APPLIC_ALIAS":
                    type = typeof(APPLIC_ALIAS);
                    break;


                case "APPLIC_AREA":
                    type = typeof(APPLIC_AREA);
                    break;


                case "APPLIC_ATTACH":
                    type = typeof(APPLIC_ATTACH);
                    break;


                case "APPLIC_BA":
                    type = typeof(APPLIC_BA);
                    break;


                case "APPLIC_DESC":
                    type = typeof(APPLIC_DESC);
                    break;


                case "APPLIC_REMARK":
                    type = typeof(APPLIC_REMARK);
                    break;


                case "APPLICATION":
                    type = typeof(APPLICATION);
                    break;


                case "APPLICATION_COMPONENT":
                    type = typeof(APPLICATION_COMPONENT);
                    break;


                case "AREA":
                    type = typeof(AREA);
                    break;


                case "AREA_ALIAS":
                    type = typeof(AREA_ALIAS);
                    break;


                case "AREA_CLASS":
                    type = typeof(AREA_CLASS);
                    break;


                case "AREA_COMPONENT":
                    type = typeof(AREA_COMPONENT);
                    break;


                case "AREA_CONTAIN":
                    type = typeof(AREA_CONTAIN);
                    break;


                case "AREA_DESCRIPTION":
                    type = typeof(AREA_DESCRIPTION);
                    break;


                case "AREA_HIER_DETAIL":
                    type = typeof(AREA_HIER_DETAIL);
                    break;


                case "AREA_HIERARCHY":
                    type = typeof(AREA_HIERARCHY);
                    break;


                case "AREA_XREF":
                    type = typeof(AREA_XREF);
                    break;


                case "BA_ADDRESS":
                    type = typeof(BA_ADDRESS);
                    break;


                case "BA_ALIAS":
                    type = typeof(BA_ALIAS);
                    break;


                case "BA_AUTHORITY":
                    type = typeof(BA_AUTHORITY);
                    break;


                case "BA_AUTHORITY_COMP":
                    type = typeof(BA_AUTHORITY_COMP);
                    break;


                case "BA_COMPONENT":
                    type = typeof(BA_COMPONENT);
                    break;


                case "BA_CONSORTIUM_SERVICE":
                    type = typeof(BA_CONSORTIUM_SERVICE);
                    break;


                case "BA_CONTACT_INFO":
                    type = typeof(BA_CONTACT_INFO);
                    break;


                case "BA_CREW":
                    type = typeof(BA_CREW);
                    break;


                case "BA_CREW_MEMBER":
                    type = typeof(BA_CREW_MEMBER);
                    break;


                case "BA_DESCRIPTION":
                    type = typeof(BA_DESCRIPTION);
                    break;


                case "BA_EMPLOYEE":
                    type = typeof(BA_EMPLOYEE);
                    break;


                case "BA_LICENSE":
                    type = typeof(BA_LICENSE);
                    break;


                case "BA_LICENSE_ALIAS":
                    type = typeof(BA_LICENSE_ALIAS);
                    break;


                case "BA_LICENSE_AREA":
                    type = typeof(BA_LICENSE_AREA);
                    break;


                case "BA_LICENSE_COND":
                    type = typeof(BA_LICENSE_COND);
                    break;


                case "BA_LICENSE_COND_CODE":
                    type = typeof(BA_LICENSE_COND_CODE);
                    break;


                case "BA_LICENSE_COND_TYPE":
                    type = typeof(BA_LICENSE_COND_TYPE);
                    break;


                case "BA_LICENSE_REMARK":
                    type = typeof(BA_LICENSE_REMARK);
                    break;


                case "BA_LICENSE_STATUS":
                    type = typeof(BA_LICENSE_STATUS);
                    break;


                case "BA_LICENSE_TYPE":
                    type = typeof(BA_LICENSE_TYPE);
                    break;


                case "BA_LICENSE_VIOLATION":
                    type = typeof(BA_LICENSE_VIOLATION);
                    break;


                case "BA_ORGANIZATION":
                    type = typeof(BA_ORGANIZATION);
                    break;


                case "BA_ORGANIZATION_COMP":
                    type = typeof(BA_ORGANIZATION_COMP);
                    break;


                case "BA_PERMIT":
                    type = typeof(BA_PERMIT);
                    break;


                case "BA_PREFERENCE":
                    type = typeof(BA_PREFERENCE);
                    break;


                case "BA_PREFERENCE_LEVEL":
                    type = typeof(BA_PREFERENCE_LEVEL);
                    break;


                case "BA_SERVICE":
                    type = typeof(BA_SERVICE);
                    break;


                case "BA_SERVICE_ADDRESS":
                    type = typeof(BA_SERVICE_ADDRESS);
                    break;


                case "BA_XREF":
                    type = typeof(BA_XREF);
                    break;


                case "BUSINESS_ASSOCIATE":
                    type = typeof(BUSINESS_ASSOCIATE);
                    break;


                case "CAT_ADDITIVE":
                    type = typeof(CAT_ADDITIVE);
                    break;


                case "CAT_ADDITIVE_ALIAS":
                    type = typeof(CAT_ADDITIVE_ALIAS);
                    break;


                case "CAT_ADDITIVE_ALLOWANCE":
                    type = typeof(CAT_ADDITIVE_ALLOWANCE);
                    break;


                case "CAT_ADDITIVE_GROUP":
                    type = typeof(CAT_ADDITIVE_GROUP);
                    break;


                case "CAT_ADDITIVE_GROUP_PART":
                    type = typeof(CAT_ADDITIVE_GROUP_PART);
                    break;


                case "CAT_ADDITIVE_SPEC":
                    type = typeof(CAT_ADDITIVE_SPEC);
                    break;


                case "CAT_ADDITIVE_TYPE":
                    type = typeof(CAT_ADDITIVE_TYPE);
                    break;


                case "CAT_ADDITIVE_XREF":
                    type = typeof(CAT_ADDITIVE_XREF);
                    break;


                case "CAT_EQUIP_ALIAS":
                    type = typeof(CAT_EQUIP_ALIAS);
                    break;


                case "CAT_EQUIP_SPEC":
                    type = typeof(CAT_EQUIP_SPEC);
                    break;


                case "CAT_EQUIPMENT":
                    type = typeof(CAT_EQUIPMENT);
                    break;


                case "CLASS_LEVEL":
                    type = typeof(CLASS_LEVEL);
                    break;


                case "CLASS_LEVEL_ALIAS":
                    type = typeof(CLASS_LEVEL_ALIAS);
                    break;


                case "CLASS_LEVEL_COMPONENT":
                    type = typeof(CLASS_LEVEL_COMPONENT);
                    break;


                case "CLASS_LEVEL_DESC":
                    type = typeof(CLASS_LEVEL_DESC);
                    break;


                case "CLASS_LEVEL_TYPE":
                    type = typeof(CLASS_LEVEL_TYPE);
                    break;


                case "CLASS_LEVEL_XREF":
                    type = typeof(CLASS_LEVEL_XREF);
                    break;


                case "CLASS_SYSTEM":
                    type = typeof(CLASS_SYSTEM);
                    break;


                case "CLASS_SYSTEM_ALIAS":
                    type = typeof(CLASS_SYSTEM_ALIAS);
                    break;


                case "CLASS_SYSTEM_XREF":
                    type = typeof(CLASS_SYSTEM_XREF);
                    break;


                case "CONSENT":
                    type = typeof(CONSENT);
                    break;


                case "CONSENT_BA":
                    type = typeof(CONSENT_BA);
                    break;


                case "CONSENT_COMPONENT":
                    type = typeof(CONSENT_COMPONENT);
                    break;


                case "CONSENT_COND":
                    type = typeof(CONSENT_COND);
                    break;


                case "CONSENT_REMARK":
                    type = typeof(CONSENT_REMARK);
                    break;


                case "CONSULT":
                    type = typeof(CONSULT);
                    break;


                case "CONSULT_BA":
                    type = typeof(CONSULT_BA);
                    break;


                case "CONSULT_COMPONENT":
                    type = typeof(CONSULT_COMPONENT);
                    break;


                case "CONSULT_DISC":
                    type = typeof(CONSULT_DISC);
                    break;


                case "CONSULT_DISC_BA":
                    type = typeof(CONSULT_DISC_BA);
                    break;


                case "CONSULT_DISC_ISSUE":
                    type = typeof(CONSULT_DISC_ISSUE);
                    break;


                case "CONSULT_ISSUE":
                    type = typeof(CONSULT_ISSUE);
                    break;


                case "CONSULT_XREF":
                    type = typeof(CONSULT_XREF);
                    break;


                case "CONT_ACCOUNT_PROC":
                    type = typeof(CONT_ACCOUNT_PROC);
                    break;


                case "CONT_ALIAS":
                    type = typeof(CONT_ALIAS);
                    break;


                case "CONT_ALLOW_EXPENSE":
                    type = typeof(CONT_ALLOW_EXPENSE);
                    break;


                case "CONT_AREA":
                    type = typeof(CONT_AREA);
                    break;


                case "CONT_BA":
                    type = typeof(CONT_BA);
                    break;


                case "CONT_BA_SERVICE":
                    type = typeof(CONT_BA_SERVICE);
                    break;


                case "CONT_EXEMPTION":
                    type = typeof(CONT_EXEMPTION);
                    break;


                case "CONT_EXTENSION":
                    type = typeof(CONT_EXTENSION);
                    break;


                case "CONT_JURISDICTION":
                    type = typeof(CONT_JURISDICTION);
                    break;


                case "CONT_KEY_WORD":
                    type = typeof(CONT_KEY_WORD);
                    break;


                case "CONT_MKTG_ELECT_SUBST":
                    type = typeof(CONT_MKTG_ELECT_SUBST);
                    break;


                case "CONT_OPER_PROC":
                    type = typeof(CONT_OPER_PROC);
                    break;


                case "CONT_PROVISION":
                    type = typeof(CONT_PROVISION);
                    break;


                case "CONT_PROVISION_TEXT":
                    type = typeof(CONT_PROVISION_TEXT);
                    break;


                case "CONT_PROVISION_XREF":
                    type = typeof(CONT_PROVISION_XREF);
                    break;


                case "CONT_REMARK":
                    type = typeof(CONT_REMARK);
                    break;


                case "CONT_STATUS":
                    type = typeof(CONT_STATUS);
                    break;


                case "CONT_TYPE":
                    type = typeof(CONT_TYPE);
                    break;


                case "CONT_VOTING_PROC":
                    type = typeof(CONT_VOTING_PROC);
                    break;


                case "CONT_XREF":
                    type = typeof(CONT_XREF);
                    break;


                case "CONTEST":
                    type = typeof(CONTEST);
                    break;


                case "CONTEST_COMPONENT":
                    type = typeof(CONTEST_COMPONENT);
                    break;


                case "CONTEST_PARTY":
                    type = typeof(CONTEST_PARTY);
                    break;


                case "CONTEST_REMARK":
                    type = typeof(CONTEST_REMARK);
                    break;


                case "CONTRACT":
                    type = typeof(CONTRACT);
                    break;


                case "CONTRACT_COMPONENT":
                    type = typeof(CONTRACT_COMPONENT);
                    break;


                case "CS_ALIAS":
                    type = typeof(CS_ALIAS);
                    break;


                case "CS_COORD_ACQUISITION":
                    type = typeof(CS_COORD_ACQUISITION);
                    break;


                case "CS_COORD_TRANS_PARM":
                    type = typeof(CS_COORD_TRANS_PARM);
                    break;


                case "CS_COORD_TRANS_VALUE":
                    type = typeof(CS_COORD_TRANS_VALUE);
                    break;


                case "CS_COORD_TRANSFORM":
                    type = typeof(CS_COORD_TRANSFORM);
                    break;


                case "CS_COORDINATE_SYSTEM":
                    type = typeof(CS_COORDINATE_SYSTEM);
                    break;


                case "CS_ELLIPSOID":
                    type = typeof(CS_ELLIPSOID);
                    break;


                case "CS_GEODETIC_DATUM":
                    type = typeof(CS_GEODETIC_DATUM);
                    break;


                case "CS_PRIME_MERIDIAN":
                    type = typeof(CS_PRIME_MERIDIAN);
                    break;


                case "CS_PRINCIPAL_MERIDIAN":
                    type = typeof(CS_PRINCIPAL_MERIDIAN);
                    break;


                case "ECOZONE":
                    type = typeof(ECOZONE);
                    break;


                case "ECOZONE_ALIAS":
                    type = typeof(ECOZONE_ALIAS);
                    break;


                case "ECOZONE_HIERARCHY":
                    type = typeof(ECOZONE_HIERARCHY);
                    break;


                case "ECOZONE_SET":
                    type = typeof(ECOZONE_SET);
                    break;


                case "ECOZONE_SET_PART":
                    type = typeof(ECOZONE_SET_PART);
                    break;


                case "ECOZONE_XREF":
                    type = typeof(ECOZONE_XREF);
                    break;


                case "ENT_COMPONENT":
                    type = typeof(ENT_COMPONENT);
                    break;


                case "ENT_GROUP":
                    type = typeof(ENT_GROUP);
                    break;


                case "ENT_SECURITY_BA":
                    type = typeof(ENT_SECURITY_BA);
                    break;


                case "ENT_SECURITY_GROUP":
                    type = typeof(ENT_SECURITY_GROUP);
                    break;


                case "ENT_SECURITY_GROUP_XREF":
                    type = typeof(ENT_SECURITY_GROUP_XREF);
                    break;


                case "ENTITLEMENT":
                    type = typeof(ENTITLEMENT);
                    break;


                case "EQUIPMENT":
                    type = typeof(EQUIPMENT);
                    break;


                case "EQUIPMENT_ALIAS":
                    type = typeof(EQUIPMENT_ALIAS);
                    break;


                case "EQUIPMENT_BA":
                    type = typeof(EQUIPMENT_BA);
                    break;


                case "EQUIPMENT_COMPONENT":
                    type = typeof(EQUIPMENT_COMPONENT);
                    break;


                case "EQUIPMENT_MAINT_STATUS":
                    type = typeof(EQUIPMENT_MAINT_STATUS);
                    break;


                case "EQUIPMENT_MAINT_TYPE":
                    type = typeof(EQUIPMENT_MAINT_TYPE);
                    break;


                case "EQUIPMENT_MAINTAIN":
                    type = typeof(EQUIPMENT_MAINTAIN);
                    break;


                case "EQUIPMENT_SPEC":
                    type = typeof(EQUIPMENT_SPEC);
                    break;


                case "EQUIPMENT_SPEC_SET":
                    type = typeof(EQUIPMENT_SPEC_SET);
                    break;


                case "EQUIPMENT_SPEC_SET_SPEC":
                    type = typeof(EQUIPMENT_SPEC_SET_SPEC);
                    break;


                case "EQUIPMENT_STATUS":
                    type = typeof(EQUIPMENT_STATUS);
                    break;


                case "EQUIPMENT_USE_STAT":
                    type = typeof(EQUIPMENT_USE_STAT);
                    break;


                case "EQUIPMENT_XREF":
                    type = typeof(EQUIPMENT_XREF);
                    break;


                case "FACILITY":
                    type = typeof(FACILITY);
                    break;


                case "FACILITY_ALIAS":
                    type = typeof(FACILITY_ALIAS);
                    break;


                case "FACILITY_AREA":
                    type = typeof(FACILITY_AREA);
                    break;


                case "FACILITY_BA_SERVICE":
                    type = typeof(FACILITY_BA_SERVICE);
                    break;


                case "FACILITY_CLASS":
                    type = typeof(FACILITY_CLASS);
                    break;


                case "FACILITY_COMPONENT":
                    type = typeof(FACILITY_COMPONENT);
                    break;


                case "FACILITY_DESCRIPTION":
                    type = typeof(FACILITY_DESCRIPTION);
                    break;


                case "FACILITY_EQUIPMENT":
                    type = typeof(FACILITY_EQUIPMENT);
                    break;


                case "FACILITY_FIELD":
                    type = typeof(FACILITY_FIELD);
                    break;


                case "FACILITY_LIC_ALIAS":
                    type = typeof(FACILITY_LIC_ALIAS);
                    break;


                case "FACILITY_LIC_AREA":
                    type = typeof(FACILITY_LIC_AREA);
                    break;


                case "FACILITY_LIC_COND":
                    type = typeof(FACILITY_LIC_COND);
                    break;


                case "FACILITY_LIC_REMARK":
                    type = typeof(FACILITY_LIC_REMARK);
                    break;


                case "FACILITY_LIC_STATUS":
                    type = typeof(FACILITY_LIC_STATUS);
                    break;


                case "FACILITY_LIC_TYPE":
                    type = typeof(FACILITY_LIC_TYPE);
                    break;


                case "FACILITY_LIC_VIOLATION":
                    type = typeof(FACILITY_LIC_VIOLATION);
                    break;


                case "FACILITY_LICENSE":
                    type = typeof(FACILITY_LICENSE);
                    break;


                case "FACILITY_MAINT_STATUS":
                    type = typeof(FACILITY_MAINT_STATUS);
                    break;


                case "FACILITY_MAINTAIN":
                    type = typeof(FACILITY_MAINTAIN);
                    break;


                case "FACILITY_RATE":
                    type = typeof(FACILITY_RATE);
                    break;


                case "FACILITY_RESTRICTION":
                    type = typeof(FACILITY_RESTRICTION);
                    break;


                case "FACILITY_STATUS":
                    type = typeof(FACILITY_STATUS);
                    break;


                case "FACILITY_SUBSTANCE":
                    type = typeof(FACILITY_SUBSTANCE);
                    break;


                case "FACILITY_VERSION":
                    type = typeof(FACILITY_VERSION);
                    break;


                case "FACILITY_XREF":
                    type = typeof(FACILITY_XREF);
                    break;


                case "FIELD":
                    type = typeof(FIELD);
                    break;


                case "FIELD_ALIAS":
                    type = typeof(FIELD_ALIAS);
                    break;


                case "FIELD_AREA":
                    type = typeof(FIELD_AREA);
                    break;


                case "FIELD_COMPONENT":
                    type = typeof(FIELD_COMPONENT);
                    break;


                case "FIELD_INSTRUMENT":
                    type = typeof(FIELD_INSTRUMENT);
                    break;


                case "FIELD_VERSION":
                    type = typeof(FIELD_VERSION);
                    break;


                case "FIN_COMPONENT":
                    type = typeof(FIN_COMPONENT);
                    break;


                case "FIN_COST_SUMMARY":
                    type = typeof(FIN_COST_SUMMARY);
                    break;


                case "FIN_XREF":
                    type = typeof(FIN_XREF);
                    break;


                case "FINANCE":
                    type = typeof(FINANCE);
                    break;


                case "FOSSIL":
                    type = typeof(FOSSIL);
                    break;


                case "FOSSIL_AGE":
                    type = typeof(FOSSIL_AGE);
                    break;


                case "FOSSIL_ASSEMBLAGE":
                    type = typeof(FOSSIL_ASSEMBLAGE);
                    break;


                case "FOSSIL_DESC":
                    type = typeof(FOSSIL_DESC);
                    break;


                case "FOSSIL_DOCUMENT":
                    type = typeof(FOSSIL_DOCUMENT);
                    break;


                case "FOSSIL_EQUIVALENCE":
                    type = typeof(FOSSIL_EQUIVALENCE);
                    break;


                case "FOSSIL_NAME_SET":
                    type = typeof(FOSSIL_NAME_SET);
                    break;


                case "FOSSIL_NAME_SET_FOSSIL":
                    type = typeof(FOSSIL_NAME_SET_FOSSIL);
                    break;


                case "FOSSIL_TAXON_ALIAS":
                    type = typeof(FOSSIL_TAXON_ALIAS);
                    break;


                case "FOSSIL_TAXON_HIER":
                    type = typeof(FOSSIL_TAXON_HIER);
                    break;


                case "FOSSIL_TAXON_LEAF":
                    type = typeof(FOSSIL_TAXON_LEAF);
                    break;


                case "FOSSIL_XREF":
                    type = typeof(FOSSIL_XREF);
                    break;


                case "HSE_INCIDENT":
                    type = typeof(HSE_INCIDENT);
                    break;


                case "HSE_INCIDENT_BA":
                    type = typeof(HSE_INCIDENT_BA);
                    break;


                case "HSE_INCIDENT_CAUSE":
                    type = typeof(HSE_INCIDENT_CAUSE);
                    break;


                case "HSE_INCIDENT_CLASS":
                    type = typeof(HSE_INCIDENT_CLASS);
                    break;


                case "HSE_INCIDENT_CLASS_ALIAS":
                    type = typeof(HSE_INCIDENT_CLASS_ALIAS);
                    break;


                case "HSE_INCIDENT_COMPONENT":
                    type = typeof(HSE_INCIDENT_COMPONENT);
                    break;


                case "HSE_INCIDENT_DETAIL":
                    type = typeof(HSE_INCIDENT_DETAIL);
                    break;


                case "HSE_INCIDENT_EQUIP":
                    type = typeof(HSE_INCIDENT_EQUIP);
                    break;


                case "HSE_INCIDENT_EQUIV":
                    type = typeof(HSE_INCIDENT_EQUIV);
                    break;


                case "HSE_INCIDENT_INTERACTION":
                    type = typeof(HSE_INCIDENT_INTERACTION);
                    break;


                case "HSE_INCIDENT_REMARK":
                    type = typeof(HSE_INCIDENT_REMARK);
                    break;


                case "HSE_INCIDENT_RESPONSE":
                    type = typeof(HSE_INCIDENT_RESPONSE);
                    break;


                case "HSE_INCIDENT_SET":
                    type = typeof(HSE_INCIDENT_SET);
                    break;


                case "HSE_INCIDENT_SEV_ALIAS":
                    type = typeof(HSE_INCIDENT_SEV_ALIAS);
                    break;


                case "HSE_INCIDENT_SEVERITY":
                    type = typeof(HSE_INCIDENT_SEVERITY);
                    break;


                case "HSE_INCIDENT_SUBSTANCE":
                    type = typeof(HSE_INCIDENT_SUBSTANCE);
                    break;


                case "HSE_INCIDENT_TYPE":
                    type = typeof(HSE_INCIDENT_TYPE);
                    break;


                case "HSE_INCIDENT_TYPE_ALIAS":
                    type = typeof(HSE_INCIDENT_TYPE_ALIAS);
                    break;


                case "HSE_INCIDENT_WEATHER":
                    type = typeof(HSE_INCIDENT_WEATHER);
                    break;


                case "INSTRUMENT":
                    type = typeof(INSTRUMENT);
                    break;


                case "INSTRUMENT_AREA":
                    type = typeof(INSTRUMENT_AREA);
                    break;


                case "INSTRUMENT_COMPONENT":
                    type = typeof(INSTRUMENT_COMPONENT);
                    break;


                case "INSTRUMENT_DETAIL":
                    type = typeof(INSTRUMENT_DETAIL);
                    break;


                case "INSTRUMENT_XREF":
                    type = typeof(INSTRUMENT_XREF);
                    break;


                case "INT_SET_COMPONENT":
                    type = typeof(INT_SET_COMPONENT);
                    break;


                case "INT_SET_PARTNER":
                    type = typeof(INT_SET_PARTNER);
                    break;


                case "INT_SET_PARTNER_CONT":
                    type = typeof(INT_SET_PARTNER_CONT);
                    break;


                case "INT_SET_STATUS":
                    type = typeof(INT_SET_STATUS);
                    break;


                case "INT_SET_XREF":
                    type = typeof(INT_SET_XREF);
                    break;


                case "INTEREST_SET":
                    type = typeof(INTEREST_SET);
                    break;


                case "LAND_AGREE_PART":
                    type = typeof(LAND_AGREE_PART);
                    break;


                case "LAND_AGREEMENT":
                    type = typeof(LAND_AGREEMENT);
                    break;


                case "LAND_ALIAS":
                    type = typeof(LAND_ALIAS);
                    break;


                case "LAND_AREA":
                    type = typeof(LAND_AREA);
                    break;


                case "LAND_BA_SERVICE":
                    type = typeof(LAND_BA_SERVICE);
                    break;


                case "LAND_OCCUPANT":
                    type = typeof(LAND_OCCUPANT);
                    break;


                case "LAND_REMARK":
                    type = typeof(LAND_REMARK);
                    break;


                case "LAND_RIGHT":
                    type = typeof(LAND_RIGHT);
                    break;


                case "LAND_RIGHT_APPLIC":
                    type = typeof(LAND_RIGHT_APPLIC);
                    break;


                case "LAND_RIGHT_BA_LIC":
                    type = typeof(LAND_RIGHT_BA_LIC);
                    break;


                case "LAND_RIGHT_COMPONENT":
                    type = typeof(LAND_RIGHT_COMPONENT);
                    break;


                case "LAND_RIGHT_FACILITY":
                    type = typeof(LAND_RIGHT_FACILITY);
                    break;


                case "LAND_RIGHT_FIELD":
                    type = typeof(LAND_RIGHT_FIELD);
                    break;


                case "LAND_RIGHT_INSTRUMENT":
                    type = typeof(LAND_RIGHT_INSTRUMENT);
                    break;


                case "LAND_RIGHT_POOL":
                    type = typeof(LAND_RIGHT_POOL);
                    break;


                case "LAND_RIGHT_REST":
                    type = typeof(LAND_RIGHT_REST);
                    break;


                case "LAND_RIGHT_REST_REM":
                    type = typeof(LAND_RIGHT_REST_REM);
                    break;


                case "LAND_RIGHT_WELL":
                    type = typeof(LAND_RIGHT_WELL);
                    break;


                case "LAND_RIGHT_WELL_SUBST":
                    type = typeof(LAND_RIGHT_WELL_SUBST);
                    break;


                case "LAND_SALE":
                    type = typeof(LAND_SALE);
                    break;


                case "LAND_SALE_BA_SERVICE":
                    type = typeof(LAND_SALE_BA_SERVICE);
                    break;


                case "LAND_SALE_BID":
                    type = typeof(LAND_SALE_BID);
                    break;


                case "LAND_SALE_BID_SET":
                    type = typeof(LAND_SALE_BID_SET);
                    break;


                case "LAND_SALE_BID_SET_BID":
                    type = typeof(LAND_SALE_BID_SET_BID);
                    break;


                case "LAND_SALE_FEE":
                    type = typeof(LAND_SALE_FEE);
                    break;


                case "LAND_SALE_OFFERING":
                    type = typeof(LAND_SALE_OFFERING);
                    break;


                case "LAND_SALE_OFFERING_AREA":
                    type = typeof(LAND_SALE_OFFERING_AREA);
                    break;


                case "LAND_SALE_REQUEST":
                    type = typeof(LAND_SALE_REQUEST);
                    break;


                case "LAND_SALE_REST_REMARK":
                    type = typeof(LAND_SALE_REST_REMARK);
                    break;


                case "LAND_SALE_RESTRICTION":
                    type = typeof(LAND_SALE_RESTRICTION);
                    break;


                case "LAND_SALE_WORK_BID":
                    type = typeof(LAND_SALE_WORK_BID);
                    break;


                case "LAND_SIZE":
                    type = typeof(LAND_SIZE);
                    break;


                case "LAND_STATUS":
                    type = typeof(LAND_STATUS);
                    break;


                case "LAND_TERMINATION":
                    type = typeof(LAND_TERMINATION);
                    break;


                case "LAND_TITLE":
                    type = typeof(LAND_TITLE);
                    break;


                case "LAND_TRACT_FACTOR":
                    type = typeof(LAND_TRACT_FACTOR);
                    break;


                case "LAND_UNIT":
                    type = typeof(LAND_UNIT);
                    break;


                case "LAND_UNIT_TRACT":
                    type = typeof(LAND_UNIT_TRACT);
                    break;


                case "LAND_XREF":
                    type = typeof(LAND_XREF);
                    break;


                case "LEGAL_CARTER_LOC":
                    type = typeof(LEGAL_CARTER_LOC);
                    break;


                case "LEGAL_CONGRESS_LOC":
                    type = typeof(LEGAL_CONGRESS_LOC);
                    break;


                case "LEGAL_DLS_LOC":
                    type = typeof(LEGAL_DLS_LOC);
                    break;


                case "LEGAL_FPS_LOC":
                    type = typeof(LEGAL_FPS_LOC);
                    break;


                case "LEGAL_GEODETIC_LOC":
                    type = typeof(LEGAL_GEODETIC_LOC);
                    break;


                case "LEGAL_LOC_AREA":
                    type = typeof(LEGAL_LOC_AREA);
                    break;


                case "LEGAL_LOC_REMARK":
                    type = typeof(LEGAL_LOC_REMARK);
                    break;


                case "LEGAL_NE_LOC":
                    type = typeof(LEGAL_NE_LOC);
                    break;


                case "LEGAL_NORTH_SEA_LOC":
                    type = typeof(LEGAL_NORTH_SEA_LOC);
                    break;


                case "LEGAL_NTS_LOC":
                    type = typeof(LEGAL_NTS_LOC);
                    break;


                case "LEGAL_OFFSHORE_LOC":
                    type = typeof(LEGAL_OFFSHORE_LOC);
                    break;


                case "LEGAL_OHIO_LOC":
                    type = typeof(LEGAL_OHIO_LOC);
                    break;


                case "LEGAL_TEXAS_LOC":
                    type = typeof(LEGAL_TEXAS_LOC);
                    break;


                case "LITH_DEP_ENV_INT":
                    type = typeof(LITH_DEP_ENV_INT);
                    break;


                case "LITH_DIAGENESIS":
                    type = typeof(LITH_DIAGENESIS);
                    break;


                case "LITH_GRAIN_SIZE":
                    type = typeof(LITH_GRAIN_SIZE);
                    break;


                case "LITH_INTERVAL":
                    type = typeof(LITH_INTERVAL);
                    break;


                case "LITH_LOG":
                    type = typeof(LITH_LOG);
                    break;


                case "LITH_LOG_BA_SERVICE":
                    type = typeof(LITH_LOG_BA_SERVICE);
                    break;


                case "LITH_LOG_COMPONENT":
                    type = typeof(LITH_LOG_COMPONENT);
                    break;


                case "LITH_LOG_REMARK":
                    type = typeof(LITH_LOG_REMARK);
                    break;


                case "LITH_MEASURED_SEC":
                    type = typeof(LITH_MEASURED_SEC);
                    break;


                case "LITH_POROSITY":
                    type = typeof(LITH_POROSITY);
                    break;


                case "LITH_ROCK_COLOR":
                    type = typeof(LITH_ROCK_COLOR);
                    break;


                case "LITH_ROCK_STRUCTURE":
                    type = typeof(LITH_ROCK_STRUCTURE);
                    break;


                case "LITH_ROCK_TYPE":
                    type = typeof(LITH_ROCK_TYPE);
                    break;


                case "LITH_ROCKPART":
                    type = typeof(LITH_ROCKPART);
                    break;


                case "LITH_ROCKPART_COLOR":
                    type = typeof(LITH_ROCKPART_COLOR);
                    break;


                case "LITH_ROCKPART_GRAIN_SIZE":
                    type = typeof(LITH_ROCKPART_GRAIN_SIZE);
                    break;


                case "LITH_STRUCTURE":
                    type = typeof(LITH_STRUCTURE);
                    break;


                case "NOTIF_BA":
                    type = typeof(NOTIF_BA);
                    break;


                case "NOTIFICATION":
                    type = typeof(NOTIFICATION);
                    break;


                case "NOTIFICATION_COMPONENT":
                    type = typeof(NOTIFICATION_COMPONENT);
                    break;


                case "OBLIG_ALLOW_DEDUCTION":
                    type = typeof(OBLIG_ALLOW_DEDUCTION);
                    break;


                case "OBLIG_BA_SERVICE":
                    type = typeof(OBLIG_BA_SERVICE);
                    break;


                case "OBLIG_CALC":
                    type = typeof(OBLIG_CALC);
                    break;


                case "OBLIG_DEDUCT_CALC":
                    type = typeof(OBLIG_DEDUCT_CALC);
                    break;


                case "OBLIG_DEDUCTION":
                    type = typeof(OBLIG_DEDUCTION);
                    break;


                case "OBLIG_PAY_DETAIL":
                    type = typeof(OBLIG_PAY_DETAIL);
                    break;


                case "OBLIG_PAYMENT":
                    type = typeof(OBLIG_PAYMENT);
                    break;


                case "OBLIG_PAYMENT_INSTR":
                    type = typeof(OBLIG_PAYMENT_INSTR);
                    break;


                case "OBLIG_PAYMENT_RATE":
                    type = typeof(OBLIG_PAYMENT_RATE);
                    break;


                case "OBLIG_REMARK":
                    type = typeof(OBLIG_REMARK);
                    break;


                case "OBLIG_SUBSTANCE":
                    type = typeof(OBLIG_SUBSTANCE);
                    break;


                case "OBLIG_TYPE":
                    type = typeof(OBLIG_TYPE);
                    break;


                case "OBLIG_XREF":
                    type = typeof(OBLIG_XREF);
                    break;


                case "OBLIGATION":
                    type = typeof(OBLIGATION);
                    break;


                case "OBLIGATION_COMPONENT":
                    type = typeof(OBLIGATION_COMPONENT);
                    break;


                case "PALEO_ABUND_QUALIFIER":
                    type = typeof(PALEO_ABUND_QUALIFIER);
                    break;


                case "PALEO_ABUND_SCHEME":
                    type = typeof(PALEO_ABUND_SCHEME);
                    break;


                case "PALEO_CLIMATE":
                    type = typeof(PALEO_CLIMATE);
                    break;


                case "PALEO_CONFIDENCE":
                    type = typeof(PALEO_CONFIDENCE);
                    break;


                case "PALEO_FOSSIL_IND":
                    type = typeof(PALEO_FOSSIL_IND);
                    break;


                case "PALEO_FOSSIL_INTERP":
                    type = typeof(PALEO_FOSSIL_INTERP);
                    break;


                case "PALEO_FOSSIL_LIST":
                    type = typeof(PALEO_FOSSIL_LIST);
                    break;


                case "PALEO_FOSSIL_OBS":
                    type = typeof(PALEO_FOSSIL_OBS);
                    break;


                case "PALEO_INTERP":
                    type = typeof(PALEO_INTERP);
                    break;


                case "PALEO_OBS_QUALIFIER":
                    type = typeof(PALEO_OBS_QUALIFIER);
                    break;


                case "PALEO_SUM_AUTHOR":
                    type = typeof(PALEO_SUM_AUTHOR);
                    break;


                case "PALEO_SUM_COMP":
                    type = typeof(PALEO_SUM_COMP);
                    break;


                case "PALEO_SUM_INTERVAL":
                    type = typeof(PALEO_SUM_INTERVAL);
                    break;


                case "PALEO_SUM_SAMPLE":
                    type = typeof(PALEO_SUM_SAMPLE);
                    break;


                case "PALEO_SUM_XREF":
                    type = typeof(PALEO_SUM_XREF);
                    break;


                case "PALEO_SUMMARY":
                    type = typeof(PALEO_SUMMARY);
                    break;


                case "PDEN":
                    type = typeof(PDEN);
                    break;


                case "PDEN_ALLOC_FACTOR":
                    type = typeof(PDEN_ALLOC_FACTOR);
                    break;


                case "PDEN_AREA":
                    type = typeof(PDEN_AREA);
                    break;


                case "PDEN_BUSINESS_ASSOC":
                    type = typeof(PDEN_BUSINESS_ASSOC);
                    break;


                case "PDEN_COMPONENT":
                    type = typeof(PDEN_COMPONENT);
                    break;


                case "PDEN_DECLINE_CASE":
                    type = typeof(PDEN_DECLINE_CASE);
                    break;


                case "PDEN_DECLINE_CONDITION":
                    type = typeof(PDEN_DECLINE_CONDITION);
                    break;


                case "PDEN_DECLINE_SEGMENT":
                    type = typeof(PDEN_DECLINE_SEGMENT);
                    break;


                case "PDEN_FACILITY":
                    type = typeof(PDEN_FACILITY);
                    break;


                case "PDEN_FIELD":
                    type = typeof(PDEN_FIELD);
                    break;


                case "PDEN_FLOW_MEASUREMENT":
                    type = typeof(PDEN_FLOW_MEASUREMENT);
                    break;


                case "PDEN_IN_AREA":
                    type = typeof(PDEN_IN_AREA);
                    break;


                case "PDEN_LAND_RIGHT":
                    type = typeof(PDEN_LAND_RIGHT);
                    break;


                case "PDEN_LEASE_UNIT":
                    type = typeof(PDEN_LEASE_UNIT);
                    break;


                case "PDEN_MATERIAL_BAL":
                    type = typeof(PDEN_MATERIAL_BAL);
                    break;


                case "PDEN_OPER_HIST":
                    type = typeof(PDEN_OPER_HIST);
                    break;


                case "PDEN_OTHER":
                    type = typeof(PDEN_OTHER);
                    break;


                case "PDEN_POOL":
                    type = typeof(PDEN_POOL);
                    break;


                case "PDEN_PR_STR_ALLOWABLE":
                    type = typeof(PDEN_PR_STR_ALLOWABLE);
                    break;


                case "PDEN_PR_STR_FORM":
                    type = typeof(PDEN_PR_STR_FORM);
                    break;


                case "PDEN_PROD_STRING":
                    type = typeof(PDEN_PROD_STRING);
                    break;


                case "PDEN_PROD_STRING_XREF":
                    type = typeof(PDEN_PROD_STRING_XREF);
                    break;


                case "PDEN_RESENT":
                    type = typeof(PDEN_RESENT);
                    break;


                case "PDEN_RESENT_CLASS":
                    type = typeof(PDEN_RESENT_CLASS);
                    break;


                case "PDEN_STATUS_HIST":
                    type = typeof(PDEN_STATUS_HIST);
                    break;


                case "PDEN_VOL_DISPOSITION":
                    type = typeof(PDEN_VOL_DISPOSITION);
                    break;


                case "PDEN_VOL_REGIME":
                    type = typeof(PDEN_VOL_REGIME);
                    break;


                case "PDEN_VOL_SUMM_OTHER":
                    type = typeof(PDEN_VOL_SUMM_OTHER);
                    break;


                case "PDEN_VOL_SUMMARY":
                    type = typeof(PDEN_VOL_SUMMARY);
                    break;


                case "PDEN_VOLUME_ANALYSIS":
                    type = typeof(PDEN_VOLUME_ANALYSIS);
                    break;


                case "PDEN_WELL":
                    type = typeof(PDEN_WELL);
                    break;


                case "PDEN_WELL_REPORT_STREAM":
                    type = typeof(PDEN_WELL_REPORT_STREAM);
                    break;


                case "PDEN_XREF":
                    type = typeof(PDEN_XREF);
                    break;


                case "POOL":
                    type = typeof(POOL);
                    break;


                case "POOL_ALIAS":
                    type = typeof(POOL_ALIAS);
                    break;


                case "POOL_AREA":
                    type = typeof(POOL_AREA);
                    break;


                case "POOL_COMPONENT":
                    type = typeof(POOL_COMPONENT);
                    break;


                case "POOL_INSTRUMENT":
                    type = typeof(POOL_INSTRUMENT);
                    break;


                case "POOL_VERSION":
                    type = typeof(POOL_VERSION);
                    break;


                case "POOL_VERSION_AREA":
                    type = typeof(POOL_VERSION_AREA);
                    break;


                case "PPDM_AUDIT_HISTORY":
                    type = typeof(PPDM_AUDIT_HISTORY);
                    break;


                case "PPDM_AUDIT_HISTORY_REM":
                    type = typeof(PPDM_AUDIT_HISTORY_REM);
                    break;


                case "PPDM_CHECK_CONS_VALUE":
                    type = typeof(PPDM_CHECK_CONS_VALUE);
                    break;


                case "PPDM_CODE_VERSION":
                    type = typeof(PPDM_CODE_VERSION);
                    break;


                case "PPDM_CODE_VERSION_COLUMN":
                    type = typeof(PPDM_CODE_VERSION_COLUMN);
                    break;


                case "PPDM_CODE_VERSION_USE":
                    type = typeof(PPDM_CODE_VERSION_USE);
                    break;


                case "PPDM_CODE_VERSION_XREF":
                    type = typeof(PPDM_CODE_VERSION_XREF);
                    break;


                case "PPDM_COLUMN":
                    type = typeof(PPDM_COLUMN);
                    break;


                case "PPDM_COLUMN_ALIAS":
                    type = typeof(PPDM_COLUMN_ALIAS);
                    break;


                case "PPDM_CONS_COLUMN":
                    type = typeof(PPDM_CONS_COLUMN);
                    break;


                case "PPDM_CONSTRAINT":
                    type = typeof(PPDM_CONSTRAINT);
                    break;


                case "PPDM_DATA_STORE":
                    type = typeof(PPDM_DATA_STORE);
                    break;


                case "PPDM_DOMAIN":
                    type = typeof(PPDM_DOMAIN);
                    break;


                case "PPDM_EXCEPTION":
                    type = typeof(PPDM_EXCEPTION);
                    break;


                case "PPDM_GROUP":
                    type = typeof(PPDM_GROUP);
                    break;


                case "PPDM_GROUP_OBJECT":
                    type = typeof(PPDM_GROUP_OBJECT);
                    break;


                case "PPDM_GROUP_OWNER":
                    type = typeof(PPDM_GROUP_OWNER);
                    break;


                case "PPDM_GROUP_REMARK":
                    type = typeof(PPDM_GROUP_REMARK);
                    break;


                case "PPDM_GROUP_XREF":
                    type = typeof(PPDM_GROUP_XREF);
                    break;


                case "PPDM_INDEX":
                    type = typeof(PPDM_INDEX);
                    break;


                case "PPDM_INDEX_COLUMN":
                    type = typeof(PPDM_INDEX_COLUMN);
                    break;


                case "PPDM_MAP_DETAIL":
                    type = typeof(PPDM_MAP_DETAIL);
                    break;


                case "PPDM_MAP_LOAD":
                    type = typeof(PPDM_MAP_LOAD);
                    break;


                case "PPDM_MAP_LOAD_ERROR":
                    type = typeof(PPDM_MAP_LOAD_ERROR);
                    break;


                case "PPDM_MAP_RULE":
                    type = typeof(PPDM_MAP_RULE);
                    break;


                case "PPDM_MEASUREMENT_SYSTEM":
                    type = typeof(PPDM_MEASUREMENT_SYSTEM);
                    break;


                case "PPDM_METRIC":
                    type = typeof(PPDM_METRIC);
                    break;


                case "PPDM_METRIC_COMPONENT":
                    type = typeof(PPDM_METRIC_COMPONENT);
                    break;


                case "PPDM_METRIC_VALUE":
                    type = typeof(PPDM_METRIC_VALUE);
                    break;


                case "PPDM_OBJECT_STATUS":
                    type = typeof(PPDM_OBJECT_STATUS);
                    break;


                case "PPDM_PROCEDURE":
                    type = typeof(PPDM_PROCEDURE);
                    break;


                case "PPDM_PROPERTY_COLUMN":
                    type = typeof(PPDM_PROPERTY_COLUMN);
                    break;


                case "PPDM_PROPERTY_SET":
                    type = typeof(PPDM_PROPERTY_SET);
                    break;


                case "PPDM_QUALITY_CONTROL":
                    type = typeof(PPDM_QUALITY_CONTROL);
                    break;


                case "PPDM_QUANTITY":
                    type = typeof(PPDM_QUANTITY);
                    break;


                case "PPDM_QUANTITY_ALIAS":
                    type = typeof(PPDM_QUANTITY_ALIAS);
                    break;


                case "PPDM_RULE":
                    type = typeof(PPDM_RULE);
                    break;


                case "PPDM_RULE_ALIAS":
                    type = typeof(PPDM_RULE_ALIAS);
                    break;


                case "PPDM_RULE_COMPONENT":
                    type = typeof(PPDM_RULE_COMPONENT);
                    break;


                case "PPDM_RULE_DETAIL":
                    type = typeof(PPDM_RULE_DETAIL);
                    break;


                case "PPDM_RULE_ENFORCEMENT":
                    type = typeof(PPDM_RULE_ENFORCEMENT);
                    break;


                case "PPDM_RULE_REMARK":
                    type = typeof(PPDM_RULE_REMARK);
                    break;


                case "PPDM_RULE_XREF":
                    type = typeof(PPDM_RULE_XREF);
                    break;


                case "PPDM_SCHEMA_ENTITY":
                    type = typeof(PPDM_SCHEMA_ENTITY);
                    break;


                case "PPDM_SCHEMA_ENTITY_ALIAS":
                    type = typeof(PPDM_SCHEMA_ENTITY_ALIAS);
                    break;


                case "PPDM_SCHEMA_GROUP":
                    type = typeof(PPDM_SCHEMA_GROUP);
                    break;


                case "PPDM_SW_APP_BA":
                    type = typeof(PPDM_SW_APP_BA);
                    break;


                case "PPDM_SW_APP_FUNCTION":
                    type = typeof(PPDM_SW_APP_FUNCTION);
                    break;


                case "PPDM_SW_APP_XREF":
                    type = typeof(PPDM_SW_APP_XREF);
                    break;


                case "PPDM_SW_APPLIC_ALIAS":
                    type = typeof(PPDM_SW_APPLIC_ALIAS);
                    break;


                case "PPDM_SW_APPLIC_COMP":
                    type = typeof(PPDM_SW_APPLIC_COMP);
                    break;


                case "PPDM_SW_APPLICATION":
                    type = typeof(PPDM_SW_APPLICATION);
                    break;


                case "PPDM_SYSTEM":
                    type = typeof(PPDM_SYSTEM);
                    break;


                case "PPDM_SYSTEM_ALIAS":
                    type = typeof(PPDM_SYSTEM_ALIAS);
                    break;


                case "PPDM_SYSTEM_APPLICATION":
                    type = typeof(PPDM_SYSTEM_APPLICATION);
                    break;


                case "PPDM_SYSTEM_MAP":
                    type = typeof(PPDM_SYSTEM_MAP);
                    break;


                case "PPDM_TABLE":
                    type = typeof(PPDM_TABLE);
                    break;


                case "PPDM_TABLE_ALIAS":
                    type = typeof(PPDM_TABLE_ALIAS);
                    break;


                case "PPDM_TABLE_HISTORY":
                    type = typeof(PPDM_TABLE_HISTORY);
                    break;


                case "PPDM_UNIT_CONVERSION":
                    type = typeof(PPDM_UNIT_CONVERSION);
                    break;


                case "PPDM_UNIT_OF_MEASURE":
                    type = typeof(PPDM_UNIT_OF_MEASURE);
                    break;


                case "PPDM_UOM_ALIAS":
                    type = typeof(PPDM_UOM_ALIAS);
                    break;


                case "PPDM_VOL_MEAS_CONV":
                    type = typeof(PPDM_VOL_MEAS_CONV);
                    break;


                case "PPDM_VOL_MEAS_REGIME":
                    type = typeof(PPDM_VOL_MEAS_REGIME);
                    break;


                case "PPDM_VOL_MEAS_USE":
                    type = typeof(PPDM_VOL_MEAS_USE);
                    break;


                case "PR_LSE_UNIT_STR_HIST":
                    type = typeof(PR_LSE_UNIT_STR_HIST);
                    break;


                case "PR_STR_FORM_COMPLETION":
                    type = typeof(PR_STR_FORM_COMPLETION);
                    break;


                case "PR_STR_FORM_STAT_HIST":
                    type = typeof(PR_STR_FORM_STAT_HIST);
                    break;


                case "PROD_LEASE_UNIT":
                    type = typeof(PROD_LEASE_UNIT);
                    break;


                case "PROD_LEASE_UNIT_ALIAS":
                    type = typeof(PROD_LEASE_UNIT_ALIAS);
                    break;


                case "PROD_LEASE_UNIT_AREA":
                    type = typeof(PROD_LEASE_UNIT_AREA);
                    break;


                case "PROD_LEASE_UNIT_VER_AREA":
                    type = typeof(PROD_LEASE_UNIT_VER_AREA);
                    break;


                case "PROD_LEASE_UNIT_VERSION":
                    type = typeof(PROD_LEASE_UNIT_VERSION);
                    break;


                case "PROD_STR_STAT_HIST":
                    type = typeof(PROD_STR_STAT_HIST);
                    break;


                case "PROD_STRING":
                    type = typeof(PROD_STRING);
                    break;


                case "PROD_STRING_ALIAS":
                    type = typeof(PROD_STRING_ALIAS);
                    break;


                case "PROD_STRING_COMPONENT":
                    type = typeof(PROD_STRING_COMPONENT);
                    break;


                case "PROD_STRING_FORM_ALIAS":
                    type = typeof(PROD_STRING_FORM_ALIAS);
                    break;


                case "PROD_STRING_FORMATION":
                    type = typeof(PROD_STRING_FORMATION);
                    break;


                case "PROJ_STEP_CONDITION":
                    type = typeof(PROJ_STEP_CONDITION);
                    break;


                case "PROJECT":
                    type = typeof(PROJECT);
                    break;


                case "PROJECT_ALIAS":
                    type = typeof(PROJECT_ALIAS);
                    break;


                case "PROJECT_BA":
                    type = typeof(PROJECT_BA);
                    break;


                case "PROJECT_BA_ROLE":
                    type = typeof(PROJECT_BA_ROLE);
                    break;


                case "PROJECT_COMPONENT":
                    type = typeof(PROJECT_COMPONENT);
                    break;


                case "PROJECT_CONDITION":
                    type = typeof(PROJECT_CONDITION);
                    break;


                case "PROJECT_EQUIP_ROLE":
                    type = typeof(PROJECT_EQUIP_ROLE);
                    break;


                case "PROJECT_EQUIPMENT":
                    type = typeof(PROJECT_EQUIPMENT);
                    break;


                case "PROJECT_PLAN":
                    type = typeof(PROJECT_PLAN);
                    break;


                case "PROJECT_PLAN_STEP":
                    type = typeof(PROJECT_PLAN_STEP);
                    break;


                case "PROJECT_PLAN_STEP_XREF":
                    type = typeof(PROJECT_PLAN_STEP_XREF);
                    break;


                case "PROJECT_STATUS":
                    type = typeof(PROJECT_STATUS);
                    break;


                case "PROJECT_STEP":
                    type = typeof(PROJECT_STEP);
                    break;


                case "PROJECT_STEP_BA":
                    type = typeof(PROJECT_STEP_BA);
                    break;


                case "PROJECT_STEP_EQUIP":
                    type = typeof(PROJECT_STEP_EQUIP);
                    break;


                case "PROJECT_STEP_TIME":
                    type = typeof(PROJECT_STEP_TIME);
                    break;


                case "PROJECT_STEP_XREF":
                    type = typeof(PROJECT_STEP_XREF);
                    break;


                case "R_ACCESS_CONDITION":
                    type = typeof(R_ACCESS_CONDITION);
                    break;


                case "R_ACCOUNT_PROC_TYPE":
                    type = typeof(R_ACCOUNT_PROC_TYPE);
                    break;


                case "R_ACTIVITY_SET_TYPE":
                    type = typeof(R_ACTIVITY_SET_TYPE);
                    break;


                case "R_ACTIVITY_TYPE":
                    type = typeof(R_ACTIVITY_TYPE);
                    break;


                case "R_ADDITIVE_METHOD":
                    type = typeof(R_ADDITIVE_METHOD);
                    break;


                case "R_ADDITIVE_TYPE":
                    type = typeof(R_ADDITIVE_TYPE);
                    break;


                case "R_ADDRESS_TYPE":
                    type = typeof(R_ADDRESS_TYPE);
                    break;


                case "R_AIR_GAS_CODE":
                    type = typeof(R_AIR_GAS_CODE);
                    break;


                case "R_AIRCRAFT_TYPE":
                    type = typeof(R_AIRCRAFT_TYPE);
                    break;


                case "R_ALIAS_REASON_TYPE":
                    type = typeof(R_ALIAS_REASON_TYPE);
                    break;


                case "R_ALIAS_TYPE":
                    type = typeof(R_ALIAS_TYPE);
                    break;


                case "R_ALLOCATION_TYPE":
                    type = typeof(R_ALLOCATION_TYPE);
                    break;


                case "R_ALLOWABLE_EXPENSE":
                    type = typeof(R_ALLOWABLE_EXPENSE);
                    break;


                case "R_ANALYSIS_PROPERTY":
                    type = typeof(R_ANALYSIS_PROPERTY);
                    break;


                case "R_ANL_ACCURACY_TYPE":
                    type = typeof(R_ANL_ACCURACY_TYPE);
                    break;


                case "R_ANL_BA_ROLE_TYPE":
                    type = typeof(R_ANL_BA_ROLE_TYPE);
                    break;


                case "R_ANL_CALC_EQUIV_TYPE":
                    type = typeof(R_ANL_CALC_EQUIV_TYPE);
                    break;


                case "R_ANL_CHRO_PROPERTY":
                    type = typeof(R_ANL_CHRO_PROPERTY);
                    break;


                case "R_ANL_COMP_TYPE":
                    type = typeof(R_ANL_COMP_TYPE);
                    break;


                case "R_ANL_CONFIDENCE_TYPE":
                    type = typeof(R_ANL_CONFIDENCE_TYPE);
                    break;


                case "R_ANL_DETAIL_REF_VALUE":
                    type = typeof(R_ANL_DETAIL_REF_VALUE);
                    break;


                case "R_ANL_DETAIL_TYPE":
                    type = typeof(R_ANL_DETAIL_TYPE);
                    break;


                case "R_ANL_ELEMENT_VALUE_CODE":
                    type = typeof(R_ANL_ELEMENT_VALUE_CODE);
                    break;


                case "R_ANL_ELEMENT_VALUE_TYPE":
                    type = typeof(R_ANL_ELEMENT_VALUE_TYPE);
                    break;


                case "R_ANL_EQUIP_ROLE":
                    type = typeof(R_ANL_EQUIP_ROLE);
                    break;


                case "R_ANL_FORMULA_TYPE":
                    type = typeof(R_ANL_FORMULA_TYPE);
                    break;


                case "R_ANL_GAS_CHRO_VALUE":
                    type = typeof(R_ANL_GAS_CHRO_VALUE);
                    break;


                case "R_ANL_GAS_PROPERTY":
                    type = typeof(R_ANL_GAS_PROPERTY);
                    break;


                case "R_ANL_GAS_PROPERTY_CODE":
                    type = typeof(R_ANL_GAS_PROPERTY_CODE);
                    break;


                case "R_ANL_METHOD_EQUIV_TYPE":
                    type = typeof(R_ANL_METHOD_EQUIV_TYPE);
                    break;


                case "R_ANL_METHOD_SET_TYPE":
                    type = typeof(R_ANL_METHOD_SET_TYPE);
                    break;


                case "R_ANL_MISSING_REP":
                    type = typeof(R_ANL_MISSING_REP);
                    break;


                case "R_ANL_NULL_REP":
                    type = typeof(R_ANL_NULL_REP);
                    break;


                case "R_ANL_OIL_PROPERTY_CODE":
                    type = typeof(R_ANL_OIL_PROPERTY_CODE);
                    break;


                case "R_ANL_PARAMETER_TYPE":
                    type = typeof(R_ANL_PARAMETER_TYPE);
                    break;


                case "R_ANL_PROBLEM_RESOLUTION":
                    type = typeof(R_ANL_PROBLEM_RESOLUTION);
                    break;


                case "R_ANL_PROBLEM_RESULT":
                    type = typeof(R_ANL_PROBLEM_RESULT);
                    break;


                case "R_ANL_PROBLEM_SEVERITY":
                    type = typeof(R_ANL_PROBLEM_SEVERITY);
                    break;


                case "R_ANL_PROBLEM_TYPE":
                    type = typeof(R_ANL_PROBLEM_TYPE);
                    break;


                case "R_ANL_REF_VALUE":
                    type = typeof(R_ANL_REF_VALUE);
                    break;


                case "R_ANL_REMARK_TYPE":
                    type = typeof(R_ANL_REMARK_TYPE);
                    break;


                case "R_ANL_REPEATABILITY":
                    type = typeof(R_ANL_REPEATABILITY);
                    break;


                case "R_ANL_STEP_XREF":
                    type = typeof(R_ANL_STEP_XREF);
                    break;


                case "R_ANL_TOLERANCE_TYPE":
                    type = typeof(R_ANL_TOLERANCE_TYPE);
                    break;


                case "R_ANL_VALID_MEAS_VALUE":
                    type = typeof(R_ANL_VALID_MEAS_VALUE);
                    break;


                case "R_ANL_VALID_MEASUREMENT":
                    type = typeof(R_ANL_VALID_MEASUREMENT);
                    break;


                case "R_ANL_VALID_PROBLEM":
                    type = typeof(R_ANL_VALID_PROBLEM);
                    break;


                case "R_ANL_WATER_PROPERTY":
                    type = typeof(R_ANL_WATER_PROPERTY);
                    break;


                case "R_AOF_ANALYSIS_TYPE":
                    type = typeof(R_AOF_ANALYSIS_TYPE);
                    break;


                case "R_AOF_CALC_METHOD":
                    type = typeof(R_AOF_CALC_METHOD);
                    break;


                case "R_API_LOG_SYSTEM":
                    type = typeof(R_API_LOG_SYSTEM);
                    break;


                case "R_APPLIC_ATTACHMENT":
                    type = typeof(R_APPLIC_ATTACHMENT);
                    break;


                case "R_APPLIC_BA_ROLE":
                    type = typeof(R_APPLIC_BA_ROLE);
                    break;


                case "R_APPLIC_DECISION":
                    type = typeof(R_APPLIC_DECISION);
                    break;


                case "R_APPLIC_DESC":
                    type = typeof(R_APPLIC_DESC);
                    break;


                case "R_APPLIC_REMARK_TYPE":
                    type = typeof(R_APPLIC_REMARK_TYPE);
                    break;


                case "R_APPLIC_STATUS":
                    type = typeof(R_APPLIC_STATUS);
                    break;


                case "R_APPLIC_TYPE":
                    type = typeof(R_APPLIC_TYPE);
                    break;


                case "R_APPLICATION_COMP_TYPE":
                    type = typeof(R_APPLICATION_COMP_TYPE);
                    break;


                case "R_AREA_COMPONENT_TYPE":
                    type = typeof(R_AREA_COMPONENT_TYPE);
                    break;


                case "R_AREA_CONTAIN_TYPE":
                    type = typeof(R_AREA_CONTAIN_TYPE);
                    break;


                case "R_AREA_DESC_CODE":
                    type = typeof(R_AREA_DESC_CODE);
                    break;


                case "R_AREA_DESC_TYPE":
                    type = typeof(R_AREA_DESC_TYPE);
                    break;


                case "R_AREA_TYPE":
                    type = typeof(R_AREA_TYPE);
                    break;


                case "R_AREA_XREF_TYPE":
                    type = typeof(R_AREA_XREF_TYPE);
                    break;


                case "R_AUTHOR_TYPE":
                    type = typeof(R_AUTHOR_TYPE);
                    break;


                case "R_AUTHORITY_TYPE":
                    type = typeof(R_AUTHORITY_TYPE);
                    break;


                case "R_BA_AUTHORITY_COMP_TYPE":
                    type = typeof(R_BA_AUTHORITY_COMP_TYPE);
                    break;


                case "R_BA_CATEGORY":
                    type = typeof(R_BA_CATEGORY);
                    break;


                case "R_BA_COMPONENT_TYPE":
                    type = typeof(R_BA_COMPONENT_TYPE);
                    break;


                case "R_BA_CONTACT_LOC_TYPE":
                    type = typeof(R_BA_CONTACT_LOC_TYPE);
                    break;


                case "R_BA_CREW_OVERHEAD_TYPE":
                    type = typeof(R_BA_CREW_OVERHEAD_TYPE);
                    break;


                case "R_BA_CREW_TYPE":
                    type = typeof(R_BA_CREW_TYPE);
                    break;


                case "R_BA_DESC_CODE":
                    type = typeof(R_BA_DESC_CODE);
                    break;


                case "R_BA_DESC_REF_VALUE":
                    type = typeof(R_BA_DESC_REF_VALUE);
                    break;


                case "R_BA_DESC_TYPE":
                    type = typeof(R_BA_DESC_TYPE);
                    break;


                case "R_BA_LIC_DUE_CONDITION":
                    type = typeof(R_BA_LIC_DUE_CONDITION);
                    break;


                case "R_BA_LIC_VIOL_RESOL":
                    type = typeof(R_BA_LIC_VIOL_RESOL);
                    break;


                case "R_BA_LIC_VIOLATION_TYPE":
                    type = typeof(R_BA_LIC_VIOLATION_TYPE);
                    break;


                case "R_BA_ORGANIZATION_COMP_TYPE":
                    type = typeof(R_BA_ORGANIZATION_COMP_TYPE);
                    break;


                case "R_BA_ORGANIZATION_TYPE":
                    type = typeof(R_BA_ORGANIZATION_TYPE);
                    break;


                case "R_BA_PERMIT_TYPE":
                    type = typeof(R_BA_PERMIT_TYPE);
                    break;


                case "R_BA_PREF_TYPE":
                    type = typeof(R_BA_PREF_TYPE);
                    break;


                case "R_BA_SERVICE_TYPE":
                    type = typeof(R_BA_SERVICE_TYPE);
                    break;


                case "R_BA_STATUS":
                    type = typeof(R_BA_STATUS);
                    break;


                case "R_BA_TYPE":
                    type = typeof(R_BA_TYPE);
                    break;


                case "R_BA_XREF_TYPE":
                    type = typeof(R_BA_XREF_TYPE);
                    break;


                case "R_BH_PRESS_TEST_TYPE":
                    type = typeof(R_BH_PRESS_TEST_TYPE);
                    break;


                case "R_BHP_METHOD":
                    type = typeof(R_BHP_METHOD);
                    break;


                case "R_BIT_BEARING_CONDITION":
                    type = typeof(R_BIT_BEARING_CONDITION);
                    break;


                case "R_BIT_CUT_STRUCT_DULL":
                    type = typeof(R_BIT_CUT_STRUCT_DULL);
                    break;


                case "R_BIT_CUT_STRUCT_INNER":
                    type = typeof(R_BIT_CUT_STRUCT_INNER);
                    break;


                case "R_BIT_CUT_STRUCT_LOC":
                    type = typeof(R_BIT_CUT_STRUCT_LOC);
                    break;


                case "R_BIT_CUT_STRUCT_OUTER":
                    type = typeof(R_BIT_CUT_STRUCT_OUTER);
                    break;


                case "R_BIT_REASON_PULLED":
                    type = typeof(R_BIT_REASON_PULLED);
                    break;


                case "R_BLOWOUT_FLUID":
                    type = typeof(R_BLOWOUT_FLUID);
                    break;


                case "R_BUILDUP_RADIUS_TYPE":
                    type = typeof(R_BUILDUP_RADIUS_TYPE);
                    break;


                case "R_CAT_ADDITIVE_GROUP":
                    type = typeof(R_CAT_ADDITIVE_GROUP);
                    break;


                case "R_CAT_ADDITIVE_QUANTITY":
                    type = typeof(R_CAT_ADDITIVE_QUANTITY);
                    break;


                case "R_CAT_ADDITIVE_SPEC":
                    type = typeof(R_CAT_ADDITIVE_SPEC);
                    break;


                case "R_CAT_ADDITIVE_XREF":
                    type = typeof(R_CAT_ADDITIVE_XREF);
                    break;


                case "R_CAT_EQUIP_GROUP":
                    type = typeof(R_CAT_EQUIP_GROUP);
                    break;


                case "R_CAT_EQUIP_SPEC":
                    type = typeof(R_CAT_EQUIP_SPEC);
                    break;


                case "R_CAT_EQUIP_SPEC_CODE":
                    type = typeof(R_CAT_EQUIP_SPEC_CODE);
                    break;


                case "R_CAT_EQUIP_TYPE":
                    type = typeof(R_CAT_EQUIP_TYPE);
                    break;


                case "R_CEMENT_TYPE":
                    type = typeof(R_CEMENT_TYPE);
                    break;


                case "R_CHECKSHOT_SRVY_TYPE":
                    type = typeof(R_CHECKSHOT_SRVY_TYPE);
                    break;


                case "R_CLASS_DESC_PROPERTY":
                    type = typeof(R_CLASS_DESC_PROPERTY);
                    break;


                case "R_CLASS_LEV_COMP_TYPE":
                    type = typeof(R_CLASS_LEV_COMP_TYPE);
                    break;


                case "R_CLASS_LEV_XREF_TYPE":
                    type = typeof(R_CLASS_LEV_XREF_TYPE);
                    break;


                case "R_CLASS_SYST_XREF_TYPE":
                    type = typeof(R_CLASS_SYST_XREF_TYPE);
                    break;


                case "R_CLASS_SYSTEM_DIMENSION":
                    type = typeof(R_CLASS_SYSTEM_DIMENSION);
                    break;


                case "R_CLIMATE":
                    type = typeof(R_CLIMATE);
                    break;


                case "R_COAL_RANK_SCHEME_TYPE":
                    type = typeof(R_COAL_RANK_SCHEME_TYPE);
                    break;


                case "R_CODE_VERSION_XREF_TYPE":
                    type = typeof(R_CODE_VERSION_XREF_TYPE);
                    break;


                case "R_COLLAR_TYPE":
                    type = typeof(R_COLLAR_TYPE);
                    break;


                case "R_COLOR":
                    type = typeof(R_COLOR);
                    break;


                case "R_COLOR_EQUIV":
                    type = typeof(R_COLOR_EQUIV);
                    break;


                case "R_COLOR_FORMAT":
                    type = typeof(R_COLOR_FORMAT);
                    break;


                case "R_COLOR_PALETTE":
                    type = typeof(R_COLOR_PALETTE);
                    break;


                case "R_COMPLETION_METHOD":
                    type = typeof(R_COMPLETION_METHOD);
                    break;


                case "R_COMPLETION_STATUS":
                    type = typeof(R_COMPLETION_STATUS);
                    break;


                case "R_COMPLETION_STATUS_TYPE":
                    type = typeof(R_COMPLETION_STATUS_TYPE);
                    break;


                case "R_COMPLETION_TYPE":
                    type = typeof(R_COMPLETION_TYPE);
                    break;


                case "R_CONDITION_TYPE":
                    type = typeof(R_CONDITION_TYPE);
                    break;


                case "R_CONFIDENCE_TYPE":
                    type = typeof(R_CONFIDENCE_TYPE);
                    break;


                case "R_CONFIDENTIAL_REASON":
                    type = typeof(R_CONFIDENTIAL_REASON);
                    break;


                case "R_CONFIDENTIAL_TYPE":
                    type = typeof(R_CONFIDENTIAL_TYPE);
                    break;


                case "R_CONFORMITY_RELATION":
                    type = typeof(R_CONFORMITY_RELATION);
                    break;


                case "R_CONSENT_BA_ROLE":
                    type = typeof(R_CONSENT_BA_ROLE);
                    break;


                case "R_CONSENT_COMP_TYPE":
                    type = typeof(R_CONSENT_COMP_TYPE);
                    break;


                case "R_CONSENT_CONDITION":
                    type = typeof(R_CONSENT_CONDITION);
                    break;


                case "R_CONSENT_REMARK":
                    type = typeof(R_CONSENT_REMARK);
                    break;


                case "R_CONSENT_STATUS":
                    type = typeof(R_CONSENT_STATUS);
                    break;


                case "R_CONSENT_TYPE":
                    type = typeof(R_CONSENT_TYPE);
                    break;


                case "R_CONSULT_ATTEND_TYPE":
                    type = typeof(R_CONSULT_ATTEND_TYPE);
                    break;


                case "R_CONSULT_COMP_TYPE":
                    type = typeof(R_CONSULT_COMP_TYPE);
                    break;


                case "R_CONSULT_DISC_TYPE":
                    type = typeof(R_CONSULT_DISC_TYPE);
                    break;


                case "R_CONSULT_ISSUE_TYPE":
                    type = typeof(R_CONSULT_ISSUE_TYPE);
                    break;


                case "R_CONSULT_REASON":
                    type = typeof(R_CONSULT_REASON);
                    break;


                case "R_CONSULT_RESOLUTION":
                    type = typeof(R_CONSULT_RESOLUTION);
                    break;


                case "R_CONSULT_ROLE":
                    type = typeof(R_CONSULT_ROLE);
                    break;


                case "R_CONSULT_TYPE":
                    type = typeof(R_CONSULT_TYPE);
                    break;


                case "R_CONSULT_XREF_TYPE":
                    type = typeof(R_CONSULT_XREF_TYPE);
                    break;


                case "R_CONT_BA_ROLE":
                    type = typeof(R_CONT_BA_ROLE);
                    break;


                case "R_CONT_COMP_REASON":
                    type = typeof(R_CONT_COMP_REASON);
                    break;


                case "R_CONT_EXTEND_COND":
                    type = typeof(R_CONT_EXTEND_COND);
                    break;


                case "R_CONT_EXTEND_TYPE":
                    type = typeof(R_CONT_EXTEND_TYPE);
                    break;


                case "R_CONT_INSUR_ELECT":
                    type = typeof(R_CONT_INSUR_ELECT);
                    break;


                case "R_CONT_OPERATING_PROC":
                    type = typeof(R_CONT_OPERATING_PROC);
                    break;


                case "R_CONT_PROV_XREF_TYPE":
                    type = typeof(R_CONT_PROV_XREF_TYPE);
                    break;


                case "R_CONT_PROVISION_TYPE":
                    type = typeof(R_CONT_PROVISION_TYPE);
                    break;


                case "R_CONT_STATUS":
                    type = typeof(R_CONT_STATUS);
                    break;


                case "R_CONT_STATUS_TYPE":
                    type = typeof(R_CONT_STATUS_TYPE);
                    break;


                case "R_CONT_TYPE":
                    type = typeof(R_CONT_TYPE);
                    break;


                case "R_CONT_VOTE_RESPONSE":
                    type = typeof(R_CONT_VOTE_RESPONSE);
                    break;


                case "R_CONT_VOTE_TYPE":
                    type = typeof(R_CONT_VOTE_TYPE);
                    break;


                case "R_CONT_XREF_TYPE":
                    type = typeof(R_CONT_XREF_TYPE);
                    break;


                case "R_CONTACT_ROLE":
                    type = typeof(R_CONTACT_ROLE);
                    break;


                case "R_CONTAMINANT_TYPE":
                    type = typeof(R_CONTAMINANT_TYPE);
                    break;


                case "R_CONTEST_COMP_TYPE":
                    type = typeof(R_CONTEST_COMP_TYPE);
                    break;


                case "R_CONTEST_PARTY_ROLE":
                    type = typeof(R_CONTEST_PARTY_ROLE);
                    break;


                case "R_CONTEST_RESOLUTION":
                    type = typeof(R_CONTEST_RESOLUTION);
                    break;


                case "R_CONTEST_TYPE":
                    type = typeof(R_CONTEST_TYPE);
                    break;


                case "R_CONTRACT_COMP_TYPE":
                    type = typeof(R_CONTRACT_COMP_TYPE);
                    break;


                case "R_COORD_CAPTURE":
                    type = typeof(R_COORD_CAPTURE);
                    break;


                case "R_COORD_COMPUTE":
                    type = typeof(R_COORD_COMPUTE);
                    break;


                case "R_COORD_QUALITY":
                    type = typeof(R_COORD_QUALITY);
                    break;


                case "R_COORD_SYSTEM_TYPE":
                    type = typeof(R_COORD_SYSTEM_TYPE);
                    break;


                case "R_CORE_HANDLING":
                    type = typeof(R_CORE_HANDLING);
                    break;


                case "R_CORE_RECOVERY_TYPE":
                    type = typeof(R_CORE_RECOVERY_TYPE);
                    break;


                case "R_CORE_SAMPLE_TYPE":
                    type = typeof(R_CORE_SAMPLE_TYPE);
                    break;


                case "R_CORE_SHIFT_METHOD":
                    type = typeof(R_CORE_SHIFT_METHOD);
                    break;


                case "R_CORE_SOLVENT":
                    type = typeof(R_CORE_SOLVENT);
                    break;


                case "R_CORE_TYPE":
                    type = typeof(R_CORE_TYPE);
                    break;


                case "R_CORRECTION_METHOD":
                    type = typeof(R_CORRECTION_METHOD);
                    break;


                case "R_COUPLING_TYPE":
                    type = typeof(R_COUPLING_TYPE);
                    break;


                case "R_CREATOR_TYPE":
                    type = typeof(R_CREATOR_TYPE);
                    break;


                case "R_CS_TRANSFORM_PARM":
                    type = typeof(R_CS_TRANSFORM_PARM);
                    break;


                case "R_CS_TRANSFORM_TYPE":
                    type = typeof(R_CS_TRANSFORM_TYPE);
                    break;


                case "R_CURVE_SCALE":
                    type = typeof(R_CURVE_SCALE);
                    break;


                case "R_CURVE_TYPE":
                    type = typeof(R_CURVE_TYPE);
                    break;


                case "R_CURVE_XREF_TYPE":
                    type = typeof(R_CURVE_XREF_TYPE);
                    break;


                case "R_CUSHION_TYPE":
                    type = typeof(R_CUSHION_TYPE);
                    break;


                case "R_CUTTING_FLUID":
                    type = typeof(R_CUTTING_FLUID);
                    break;


                case "R_DATA_CIRC_PROCESS":
                    type = typeof(R_DATA_CIRC_PROCESS);
                    break;


                case "R_DATA_CIRC_STATUS":
                    type = typeof(R_DATA_CIRC_STATUS);
                    break;


                case "R_DATA_STORE_TYPE":
                    type = typeof(R_DATA_STORE_TYPE);
                    break;


                case "R_DATE_FORMAT_TYPE":
                    type = typeof(R_DATE_FORMAT_TYPE);
                    break;


                case "R_DATUM_ORIGIN":
                    type = typeof(R_DATUM_ORIGIN);
                    break;


                case "R_DECLINE_COND_CODE":
                    type = typeof(R_DECLINE_COND_CODE);
                    break;


                case "R_DECLINE_COND_TYPE":
                    type = typeof(R_DECLINE_COND_TYPE);
                    break;


                case "R_DECLINE_CURVE_TYPE":
                    type = typeof(R_DECLINE_CURVE_TYPE);
                    break;


                case "R_DECLINE_TYPE":
                    type = typeof(R_DECLINE_TYPE);
                    break;


                case "R_DECRYPT_TYPE":
                    type = typeof(R_DECRYPT_TYPE);
                    break;


                case "R_DEDUCT_TYPE":
                    type = typeof(R_DEDUCT_TYPE);
                    break;


                case "R_DIGITAL_FORMAT":
                    type = typeof(R_DIGITAL_FORMAT);
                    break;


                case "R_DIGITAL_OUTPUT":
                    type = typeof(R_DIGITAL_OUTPUT);
                    break;


                case "R_DIR_SRVY_ACC_REASON":
                    type = typeof(R_DIR_SRVY_ACC_REASON);
                    break;


                case "R_DIR_SRVY_CLASS":
                    type = typeof(R_DIR_SRVY_CLASS);
                    break;


                case "R_DIR_SRVY_COMPUTE":
                    type = typeof(R_DIR_SRVY_COMPUTE);
                    break;


                case "R_DIR_SRVY_CORR_ANGLE_TYPE":
                    type = typeof(R_DIR_SRVY_CORR_ANGLE_TYPE);
                    break;


                case "R_DIR_SRVY_POINT_TYPE":
                    type = typeof(R_DIR_SRVY_POINT_TYPE);
                    break;


                case "R_DIR_SRVY_PROCESS_METH":
                    type = typeof(R_DIR_SRVY_PROCESS_METH);
                    break;


                case "R_DIR_SRVY_RAD_UNCERT":
                    type = typeof(R_DIR_SRVY_RAD_UNCERT);
                    break;


                case "R_DIR_SRVY_RECORD":
                    type = typeof(R_DIR_SRVY_RECORD);
                    break;


                case "R_DIR_SRVY_REPORT_TYPE":
                    type = typeof(R_DIR_SRVY_REPORT_TYPE);
                    break;


                case "R_DIR_SRVY_TYPE":
                    type = typeof(R_DIR_SRVY_TYPE);
                    break;


                case "R_DIRECTION":
                    type = typeof(R_DIRECTION);
                    break;


                case "R_DIST_REF_PT":
                    type = typeof(R_DIST_REF_PT);
                    break;


                case "R_DOC_STATUS":
                    type = typeof(R_DOC_STATUS);
                    break;


                case "R_DOCUMENT_TYPE":
                    type = typeof(R_DOCUMENT_TYPE);
                    break;


                case "R_DRILL_ASSEMBLY_COMP":
                    type = typeof(R_DRILL_ASSEMBLY_COMP);
                    break;


                case "R_DRILL_BIT_CONDITION":
                    type = typeof(R_DRILL_BIT_CONDITION);
                    break;


                case "R_DRILL_BIT_DETAIL_CODE":
                    type = typeof(R_DRILL_BIT_DETAIL_CODE);
                    break;


                case "R_DRILL_BIT_DETAIL_TYPE":
                    type = typeof(R_DRILL_BIT_DETAIL_TYPE);
                    break;


                case "R_DRILL_BIT_JET_TYPE":
                    type = typeof(R_DRILL_BIT_JET_TYPE);
                    break;


                case "R_DRILL_BIT_TYPE":
                    type = typeof(R_DRILL_BIT_TYPE);
                    break;


                case "R_DRILL_HOLE_POSITION":
                    type = typeof(R_DRILL_HOLE_POSITION);
                    break;


                case "R_DRILL_REPORT_TIME":
                    type = typeof(R_DRILL_REPORT_TIME);
                    break;


                case "R_DRILL_STAT_CODE":
                    type = typeof(R_DRILL_STAT_CODE);
                    break;


                case "R_DRILL_STAT_TYPE":
                    type = typeof(R_DRILL_STAT_TYPE);
                    break;


                case "R_DRILL_TOOL_TYPE":
                    type = typeof(R_DRILL_TOOL_TYPE);
                    break;


                case "R_DRILLING_MEDIA":
                    type = typeof(R_DRILLING_MEDIA);
                    break;


                case "R_ECONOMIC_SCENARIO":
                    type = typeof(R_ECONOMIC_SCENARIO);
                    break;


                case "R_ECONOMIC_SCHEDULE":
                    type = typeof(R_ECONOMIC_SCHEDULE);
                    break;


                case "R_ECOZONE_HIER_LEVEL":
                    type = typeof(R_ECOZONE_HIER_LEVEL);
                    break;


                case "R_ECOZONE_TYPE":
                    type = typeof(R_ECOZONE_TYPE);
                    break;


                case "R_ECOZONE_XREF":
                    type = typeof(R_ECOZONE_XREF);
                    break;


                case "R_EMPLOYEE_POSITION":
                    type = typeof(R_EMPLOYEE_POSITION);
                    break;


                case "R_EMPLOYEE_STATUS":
                    type = typeof(R_EMPLOYEE_STATUS);
                    break;


                case "R_ENCODING_TYPE":
                    type = typeof(R_ENCODING_TYPE);
                    break;


                case "R_ENHANCED_REC_TYPE":
                    type = typeof(R_ENHANCED_REC_TYPE);
                    break;


                case "R_ENT_ACCESS_TYPE":
                    type = typeof(R_ENT_ACCESS_TYPE);
                    break;


                case "R_ENT_COMPONENT_TYPE":
                    type = typeof(R_ENT_COMPONENT_TYPE);
                    break;


                case "R_ENT_EXPIRY_ACTION":
                    type = typeof(R_ENT_EXPIRY_ACTION);
                    break;


                case "R_ENT_SEC_GROUP_TYPE":
                    type = typeof(R_ENT_SEC_GROUP_TYPE);
                    break;


                case "R_ENT_SEC_GROUP_XREF":
                    type = typeof(R_ENT_SEC_GROUP_XREF);
                    break;


                case "R_ENT_TYPE":
                    type = typeof(R_ENT_TYPE);
                    break;


                case "R_ENVIRONMENT":
                    type = typeof(R_ENVIRONMENT);
                    break;


                case "R_EQUIP_BA_ROLE_TYPE":
                    type = typeof(R_EQUIP_BA_ROLE_TYPE);
                    break;


                case "R_EQUIP_COMPONENT_TYPE":
                    type = typeof(R_EQUIP_COMPONENT_TYPE);
                    break;


                case "R_EQUIP_INSTALL_LOC":
                    type = typeof(R_EQUIP_INSTALL_LOC);
                    break;


                case "R_EQUIP_MAINT_LOC":
                    type = typeof(R_EQUIP_MAINT_LOC);
                    break;


                case "R_EQUIP_MAINT_REASON":
                    type = typeof(R_EQUIP_MAINT_REASON);
                    break;


                case "R_EQUIP_MAINT_STAT_TYPE":
                    type = typeof(R_EQUIP_MAINT_STAT_TYPE);
                    break;


                case "R_EQUIP_MAINT_STATUS":
                    type = typeof(R_EQUIP_MAINT_STATUS);
                    break;


                case "R_EQUIP_REMOVE_REASON":
                    type = typeof(R_EQUIP_REMOVE_REASON);
                    break;


                case "R_EQUIP_SPEC":
                    type = typeof(R_EQUIP_SPEC);
                    break;


                case "R_EQUIP_SPEC_REF_TYPE":
                    type = typeof(R_EQUIP_SPEC_REF_TYPE);
                    break;


                case "R_EQUIP_SPEC_SET_TYPE":
                    type = typeof(R_EQUIP_SPEC_SET_TYPE);
                    break;


                case "R_EQUIP_STATUS":
                    type = typeof(R_EQUIP_STATUS);
                    break;


                case "R_EQUIP_STATUS_TYPE":
                    type = typeof(R_EQUIP_STATUS_TYPE);
                    break;


                case "R_EQUIP_SYSTEM_CONDITION":
                    type = typeof(R_EQUIP_SYSTEM_CONDITION);
                    break;


                case "R_EQUIP_USE_STAT_TYPE":
                    type = typeof(R_EQUIP_USE_STAT_TYPE);
                    break;


                case "R_EQUIP_XREF_TYPE":
                    type = typeof(R_EQUIP_XREF_TYPE);
                    break;


                case "R_EW_DIRECTION":
                    type = typeof(R_EW_DIRECTION);
                    break;


                case "R_EW_START_LINE":
                    type = typeof(R_EW_START_LINE);
                    break;


                case "R_FAC_FUNCTION":
                    type = typeof(R_FAC_FUNCTION);
                    break;


                case "R_FAC_LIC_COND":
                    type = typeof(R_FAC_LIC_COND);
                    break;


                case "R_FAC_LIC_COND_CODE":
                    type = typeof(R_FAC_LIC_COND_CODE);
                    break;


                case "R_FAC_LIC_DUE_CONDITION":
                    type = typeof(R_FAC_LIC_DUE_CONDITION);
                    break;


                case "R_FAC_LIC_EXTEND_TYPE":
                    type = typeof(R_FAC_LIC_EXTEND_TYPE);
                    break;


                case "R_FAC_LIC_VIOL_RESOL":
                    type = typeof(R_FAC_LIC_VIOL_RESOL);
                    break;


                case "R_FAC_LIC_VIOLATION_TYPE":
                    type = typeof(R_FAC_LIC_VIOLATION_TYPE);
                    break;


                case "R_FAC_MAINT_STATUS":
                    type = typeof(R_FAC_MAINT_STATUS);
                    break;


                case "R_FAC_MAINT_STATUS_TYPE":
                    type = typeof(R_FAC_MAINT_STATUS_TYPE);
                    break;


                case "R_FAC_MAINTAIN_TYPE":
                    type = typeof(R_FAC_MAINTAIN_TYPE);
                    break;


                case "R_FAC_PIPE_COVER":
                    type = typeof(R_FAC_PIPE_COVER);
                    break;


                case "R_FAC_PIPE_MATERIAL":
                    type = typeof(R_FAC_PIPE_MATERIAL);
                    break;


                case "R_FAC_PIPE_TYPE":
                    type = typeof(R_FAC_PIPE_TYPE);
                    break;


                case "R_FAC_SPEC_REFERENCE":
                    type = typeof(R_FAC_SPEC_REFERENCE);
                    break;


                case "R_FAC_STATUS_TYPE":
                    type = typeof(R_FAC_STATUS_TYPE);
                    break;


                case "R_FACILITY_CLASS":
                    type = typeof(R_FACILITY_CLASS);
                    break;


                case "R_FACILITY_COMP_TYPE":
                    type = typeof(R_FACILITY_COMP_TYPE);
                    break;


                case "R_FACILITY_SPEC_CODE":
                    type = typeof(R_FACILITY_SPEC_CODE);
                    break;


                case "R_FACILITY_SPEC_TYPE":
                    type = typeof(R_FACILITY_SPEC_TYPE);
                    break;


                case "R_FACILITY_STATUS":
                    type = typeof(R_FACILITY_STATUS);
                    break;


                case "R_FACILITY_TYPE":
                    type = typeof(R_FACILITY_TYPE);
                    break;


                case "R_FACILITY_XREF_TYPE":
                    type = typeof(R_FACILITY_XREF_TYPE);
                    break;


                case "R_FAULT_TYPE":
                    type = typeof(R_FAULT_TYPE);
                    break;


                case "R_FIELD_COMPONENT_TYPE":
                    type = typeof(R_FIELD_COMPONENT_TYPE);
                    break;


                case "R_FIELD_STATION_TYPE":
                    type = typeof(R_FIELD_STATION_TYPE);
                    break;


                case "R_FIELD_TYPE":
                    type = typeof(R_FIELD_TYPE);
                    break;


                case "R_FIN_COMPONENT_TYPE":
                    type = typeof(R_FIN_COMPONENT_TYPE);
                    break;


                case "R_FIN_COST_TYPE":
                    type = typeof(R_FIN_COST_TYPE);
                    break;


                case "R_FIN_STATUS":
                    type = typeof(R_FIN_STATUS);
                    break;


                case "R_FIN_TYPE":
                    type = typeof(R_FIN_TYPE);
                    break;


                case "R_FIN_XREF_TYPE":
                    type = typeof(R_FIN_XREF_TYPE);
                    break;


                case "R_FLUID_TYPE":
                    type = typeof(R_FLUID_TYPE);
                    break;


                case "R_FONT":
                    type = typeof(R_FONT);
                    break;


                case "R_FONT_EFFECT":
                    type = typeof(R_FONT_EFFECT);
                    break;


                case "R_FOOTAGE_ORIGIN":
                    type = typeof(R_FOOTAGE_ORIGIN);
                    break;


                case "R_FOS_ALIAS_TYPE":
                    type = typeof(R_FOS_ALIAS_TYPE);
                    break;


                case "R_FOS_ASSEMBLAGE_TYPE":
                    type = typeof(R_FOS_ASSEMBLAGE_TYPE);
                    break;


                case "R_FOS_DESC_CODE":
                    type = typeof(R_FOS_DESC_CODE);
                    break;


                case "R_FOS_DESC_TYPE":
                    type = typeof(R_FOS_DESC_TYPE);
                    break;


                case "R_FOS_LIFE_HABIT":
                    type = typeof(R_FOS_LIFE_HABIT);
                    break;


                case "R_FOS_NAME_SET_TYPE":
                    type = typeof(R_FOS_NAME_SET_TYPE);
                    break;


                case "R_FOS_OBS_TYPE":
                    type = typeof(R_FOS_OBS_TYPE);
                    break;


                case "R_FOS_TAXON_GROUP":
                    type = typeof(R_FOS_TAXON_GROUP);
                    break;


                case "R_FOS_TAXON_LEVEL":
                    type = typeof(R_FOS_TAXON_LEVEL);
                    break;


                case "R_FOS_XREF":
                    type = typeof(R_FOS_XREF);
                    break;


                case "R_GAS_ANL_VALUE_CODE":
                    type = typeof(R_GAS_ANL_VALUE_CODE);
                    break;


                case "R_GAS_ANL_VALUE_TYPE":
                    type = typeof(R_GAS_ANL_VALUE_TYPE);
                    break;


                case "R_GRANTED_RIGHT_TYPE":
                    type = typeof(R_GRANTED_RIGHT_TYPE);
                    break;


                case "R_HEAT_CONTENT_METHOD":
                    type = typeof(R_HEAT_CONTENT_METHOD);
                    break;


                case "R_HOLE_CONDITION":
                    type = typeof(R_HOLE_CONDITION);
                    break;


                case "R_HORIZ_DRILL_REASON":
                    type = typeof(R_HORIZ_DRILL_REASON);
                    break;


                case "R_HORIZ_DRILL_TYPE":
                    type = typeof(R_HORIZ_DRILL_TYPE);
                    break;


                case "R_HSE_COMP_ROLE":
                    type = typeof(R_HSE_COMP_ROLE);
                    break;


                case "R_HSE_INCIDENT_COMP_TYPE":
                    type = typeof(R_HSE_INCIDENT_COMP_TYPE);
                    break;


                case "R_HSE_INCIDENT_DETAIL":
                    type = typeof(R_HSE_INCIDENT_DETAIL);
                    break;


                case "R_HSE_RESPONSE_TYPE":
                    type = typeof(R_HSE_RESPONSE_TYPE);
                    break;


                case "R_IMAGE_CALIBRATE_METHOD":
                    type = typeof(R_IMAGE_CALIBRATE_METHOD);
                    break;


                case "R_IMAGE_SECTION_TYPE":
                    type = typeof(R_IMAGE_SECTION_TYPE);
                    break;


                case "R_INCIDENT_BA_ROLE":
                    type = typeof(R_INCIDENT_BA_ROLE);
                    break;


                case "R_INCIDENT_CAUSE_CODE":
                    type = typeof(R_INCIDENT_CAUSE_CODE);
                    break;


                case "R_INCIDENT_CAUSE_TYPE":
                    type = typeof(R_INCIDENT_CAUSE_TYPE);
                    break;


                case "R_INCIDENT_INTERACT_TYPE":
                    type = typeof(R_INCIDENT_INTERACT_TYPE);
                    break;


                case "R_INCIDENT_RESP_RESULT":
                    type = typeof(R_INCIDENT_RESP_RESULT);
                    break;


                case "R_INCIDENT_SUBST_ROLE":
                    type = typeof(R_INCIDENT_SUBST_ROLE);
                    break;


                case "R_INCIDENT_SUBSTANCE":
                    type = typeof(R_INCIDENT_SUBSTANCE);
                    break;


                case "R_INFORMATION_PROCESS":
                    type = typeof(R_INFORMATION_PROCESS);
                    break;


                case "R_INPUT_TYPE":
                    type = typeof(R_INPUT_TYPE);
                    break;


                case "R_INSP_COMP_TYPE":
                    type = typeof(R_INSP_COMP_TYPE);
                    break;


                case "R_INSP_STATUS":
                    type = typeof(R_INSP_STATUS);
                    break;


                case "R_INST_DETAIL_CODE":
                    type = typeof(R_INST_DETAIL_CODE);
                    break;


                case "R_INST_DETAIL_REF_VALUE":
                    type = typeof(R_INST_DETAIL_REF_VALUE);
                    break;


                case "R_INST_DETAIL_TYPE":
                    type = typeof(R_INST_DETAIL_TYPE);
                    break;


                case "R_INSTRUMENT_COMP_TYPE":
                    type = typeof(R_INSTRUMENT_COMP_TYPE);
                    break;


                case "R_INSTRUMENT_TYPE":
                    type = typeof(R_INSTRUMENT_TYPE);
                    break;


                case "R_INT_SET_COMPONENT":
                    type = typeof(R_INT_SET_COMPONENT);
                    break;


                case "R_INT_SET_ROLE":
                    type = typeof(R_INT_SET_ROLE);
                    break;


                case "R_INT_SET_STATUS":
                    type = typeof(R_INT_SET_STATUS);
                    break;


                case "R_INT_SET_STATUS_TYPE":
                    type = typeof(R_INT_SET_STATUS_TYPE);
                    break;


                case "R_INT_SET_TRIGGER":
                    type = typeof(R_INT_SET_TRIGGER);
                    break;


                case "R_INT_SET_TYPE":
                    type = typeof(R_INT_SET_TYPE);
                    break;


                case "R_INT_SET_XREF_TYPE":
                    type = typeof(R_INT_SET_XREF_TYPE);
                    break;


                case "R_INTERP_ORIGIN_TYPE":
                    type = typeof(R_INTERP_ORIGIN_TYPE);
                    break;


                case "R_INTERP_TYPE":
                    type = typeof(R_INTERP_TYPE);
                    break;


                case "R_INV_MATERIAL_TYPE":
                    type = typeof(R_INV_MATERIAL_TYPE);
                    break;


                case "R_ITEM_CATEGORY":
                    type = typeof(R_ITEM_CATEGORY);
                    break;


                case "R_ITEM_SUB_CATEGORY":
                    type = typeof(R_ITEM_SUB_CATEGORY);
                    break;


                case "R_L_OFFR_CANCEL_RSN":
                    type = typeof(R_L_OFFR_CANCEL_RSN);
                    break;


                case "R_LAND_ACQTN_METHOD":
                    type = typeof(R_LAND_ACQTN_METHOD);
                    break;


                case "R_LAND_AGREE_TYPE":
                    type = typeof(R_LAND_AGREE_TYPE);
                    break;


                case "R_LAND_BID_STATUS":
                    type = typeof(R_LAND_BID_STATUS);
                    break;


                case "R_LAND_BID_TYPE":
                    type = typeof(R_LAND_BID_TYPE);
                    break;


                case "R_LAND_BIDDER_TYPE":
                    type = typeof(R_LAND_BIDDER_TYPE);
                    break;


                case "R_LAND_CASE_ACTION":
                    type = typeof(R_LAND_CASE_ACTION);
                    break;


                case "R_LAND_CASE_TYPE":
                    type = typeof(R_LAND_CASE_TYPE);
                    break;


                case "R_LAND_CASH_BID_TYPE":
                    type = typeof(R_LAND_CASH_BID_TYPE);
                    break;


                case "R_LAND_COMPONENT_TYPE":
                    type = typeof(R_LAND_COMPONENT_TYPE);
                    break;


                case "R_LAND_LESSOR_TYPE":
                    type = typeof(R_LAND_LESSOR_TYPE);
                    break;


                case "R_LAND_OFFRING_STATUS":
                    type = typeof(R_LAND_OFFRING_STATUS);
                    break;


                case "R_LAND_OFFRING_TYPE":
                    type = typeof(R_LAND_OFFRING_TYPE);
                    break;


                case "R_LAND_PROPERTY_TYPE":
                    type = typeof(R_LAND_PROPERTY_TYPE);
                    break;


                case "R_LAND_REF_NUM_TYPE":
                    type = typeof(R_LAND_REF_NUM_TYPE);
                    break;


                case "R_LAND_RENTAL_TYPE":
                    type = typeof(R_LAND_RENTAL_TYPE);
                    break;


                case "R_LAND_REQ_STATUS":
                    type = typeof(R_LAND_REQ_STATUS);
                    break;


                case "R_LAND_REQUEST_TYPE":
                    type = typeof(R_LAND_REQUEST_TYPE);
                    break;


                case "R_LAND_RIGHT_CATEGORY":
                    type = typeof(R_LAND_RIGHT_CATEGORY);
                    break;


                case "R_LAND_RIGHT_STATUS":
                    type = typeof(R_LAND_RIGHT_STATUS);
                    break;


                case "R_LAND_STATUS_TYPE":
                    type = typeof(R_LAND_STATUS_TYPE);
                    break;


                case "R_LAND_TITLE_CHG_RSN":
                    type = typeof(R_LAND_TITLE_CHG_RSN);
                    break;


                case "R_LAND_TITLE_TYPE":
                    type = typeof(R_LAND_TITLE_TYPE);
                    break;


                case "R_LAND_TRACT_TYPE":
                    type = typeof(R_LAND_TRACT_TYPE);
                    break;


                case "R_LAND_UNIT_TYPE":
                    type = typeof(R_LAND_UNIT_TYPE);
                    break;


                case "R_LAND_WELL_REL_TYPE":
                    type = typeof(R_LAND_WELL_REL_TYPE);
                    break;


                case "R_LANGUAGE":
                    type = typeof(R_LANGUAGE);
                    break;


                case "R_LEASE_UNIT_STATUS":
                    type = typeof(R_LEASE_UNIT_STATUS);
                    break;


                case "R_LEASE_UNIT_TYPE":
                    type = typeof(R_LEASE_UNIT_TYPE);
                    break;


                case "R_LEGAL_SURVEY_TYPE":
                    type = typeof(R_LEGAL_SURVEY_TYPE);
                    break;


                case "R_LIC_STATUS_TYPE":
                    type = typeof(R_LIC_STATUS_TYPE);
                    break;


                case "R_LICENSE_STATUS":
                    type = typeof(R_LICENSE_STATUS);
                    break;


                case "R_LINER_TYPE":
                    type = typeof(R_LINER_TYPE);
                    break;


                case "R_LITH_ABUNDANCE":
                    type = typeof(R_LITH_ABUNDANCE);
                    break;


                case "R_LITH_BOUNDARY_TYPE":
                    type = typeof(R_LITH_BOUNDARY_TYPE);
                    break;


                case "R_LITH_COLOR":
                    type = typeof(R_LITH_COLOR);
                    break;


                case "R_LITH_CONSOLIDATION":
                    type = typeof(R_LITH_CONSOLIDATION);
                    break;


                case "R_LITH_CYCLE_BED":
                    type = typeof(R_LITH_CYCLE_BED);
                    break;


                case "R_LITH_DEP_ENV":
                    type = typeof(R_LITH_DEP_ENV);
                    break;


                case "R_LITH_DIAGENESIS":
                    type = typeof(R_LITH_DIAGENESIS);
                    break;


                case "R_LITH_DISTRIBUTION":
                    type = typeof(R_LITH_DISTRIBUTION);
                    break;


                case "R_LITH_INTENSITY":
                    type = typeof(R_LITH_INTENSITY);
                    break;


                case "R_LITH_LOG_COMP_TYPE":
                    type = typeof(R_LITH_LOG_COMP_TYPE);
                    break;


                case "R_LITH_LOG_TYPE":
                    type = typeof(R_LITH_LOG_TYPE);
                    break;


                case "R_LITH_OIL_STAIN":
                    type = typeof(R_LITH_OIL_STAIN);
                    break;


                case "R_LITH_PATTERN_CODE":
                    type = typeof(R_LITH_PATTERN_CODE);
                    break;


                case "R_LITH_ROCK_MATRIX":
                    type = typeof(R_LITH_ROCK_MATRIX);
                    break;


                case "R_LITH_ROCK_PROFILE":
                    type = typeof(R_LITH_ROCK_PROFILE);
                    break;


                case "R_LITH_ROCK_TYPE":
                    type = typeof(R_LITH_ROCK_TYPE);
                    break;


                case "R_LITH_ROCKPART":
                    type = typeof(R_LITH_ROCKPART);
                    break;


                case "R_LITH_ROUNDING":
                    type = typeof(R_LITH_ROUNDING);
                    break;


                case "R_LITH_SCALE_SCHEME":
                    type = typeof(R_LITH_SCALE_SCHEME);
                    break;


                case "R_LITH_SORTING":
                    type = typeof(R_LITH_SORTING);
                    break;


                case "R_LITH_STRUCTURE":
                    type = typeof(R_LITH_STRUCTURE);
                    break;


                case "R_LITHOLOGY":
                    type = typeof(R_LITHOLOGY);
                    break;


                case "R_LOCATION_DESC_TYPE":
                    type = typeof(R_LOCATION_DESC_TYPE);
                    break;


                case "R_LOCATION_QUALIFIER":
                    type = typeof(R_LOCATION_QUALIFIER);
                    break;


                case "R_LOCATION_QUALITY":
                    type = typeof(R_LOCATION_QUALITY);
                    break;


                case "R_LOCATION_TYPE":
                    type = typeof(R_LOCATION_TYPE);
                    break;


                case "R_LOG_ARRAY_DIMENSION":
                    type = typeof(R_LOG_ARRAY_DIMENSION);
                    break;


                case "R_LOG_CORRECT_METHOD":
                    type = typeof(R_LOG_CORRECT_METHOD);
                    break;


                case "R_LOG_CRV_CLASS_SYSTEM":
                    type = typeof(R_LOG_CRV_CLASS_SYSTEM);
                    break;


                case "R_LOG_DEPTH_TYPE":
                    type = typeof(R_LOG_DEPTH_TYPE);
                    break;


                case "R_LOG_DIRECTION":
                    type = typeof(R_LOG_DIRECTION);
                    break;


                case "R_LOG_GOOD_VALUE_TYPE":
                    type = typeof(R_LOG_GOOD_VALUE_TYPE);
                    break;


                case "R_LOG_INDEX_TYPE":
                    type = typeof(R_LOG_INDEX_TYPE);
                    break;


                case "R_LOG_MATRIX":
                    type = typeof(R_LOG_MATRIX);
                    break;


                case "R_LOG_POSITION_TYPE":
                    type = typeof(R_LOG_POSITION_TYPE);
                    break;


                case "R_LOG_TOOL_TYPE":
                    type = typeof(R_LOG_TOOL_TYPE);
                    break;


                case "R_LOST_MATERIAL_TYPE":
                    type = typeof(R_LOST_MATERIAL_TYPE);
                    break;


                case "R_LR_FACILITY_XREF":
                    type = typeof(R_LR_FACILITY_XREF);
                    break;


                case "R_LR_FIELD_XREF":
                    type = typeof(R_LR_FIELD_XREF);
                    break;


                case "R_LR_SIZE_TYPE":
                    type = typeof(R_LR_SIZE_TYPE);
                    break;


                case "R_LR_TERMIN_REQMT":
                    type = typeof(R_LR_TERMIN_REQMT);
                    break;


                case "R_LR_TERMIN_TYPE":
                    type = typeof(R_LR_TERMIN_TYPE);
                    break;


                case "R_LR_XREF_TYPE":
                    type = typeof(R_LR_XREF_TYPE);
                    break;


                case "R_MACERAL_AMOUNT_TYPE":
                    type = typeof(R_MACERAL_AMOUNT_TYPE);
                    break;


                case "R_MAINT_PROCESS":
                    type = typeof(R_MAINT_PROCESS);
                    break;


                case "R_MATURATION_TYPE":
                    type = typeof(R_MATURATION_TYPE);
                    break;


                case "R_MATURITY_METHOD":
                    type = typeof(R_MATURITY_METHOD);
                    break;


                case "R_MBAL_COMPRESS_TYPE":
                    type = typeof(R_MBAL_COMPRESS_TYPE);
                    break;


                case "R_MBAL_CURVE_TYPE":
                    type = typeof(R_MBAL_CURVE_TYPE);
                    break;


                case "R_MEASURE_TECHNIQUE":
                    type = typeof(R_MEASURE_TECHNIQUE);
                    break;


                case "R_MEASUREMENT_LOC":
                    type = typeof(R_MEASUREMENT_LOC);
                    break;


                case "R_MEASUREMENT_TYPE":
                    type = typeof(R_MEASUREMENT_TYPE);
                    break;


                case "R_MEDIA_TYPE":
                    type = typeof(R_MEDIA_TYPE);
                    break;


                case "R_METHOD_TYPE":
                    type = typeof(R_METHOD_TYPE);
                    break;


                case "R_MISC_DATA_CODE":
                    type = typeof(R_MISC_DATA_CODE);
                    break;


                case "R_MISC_DATA_TYPE":
                    type = typeof(R_MISC_DATA_TYPE);
                    break;


                case "R_MISSING_STRAT_TYPE":
                    type = typeof(R_MISSING_STRAT_TYPE);
                    break;


                case "R_MOBILITY_TYPE":
                    type = typeof(R_MOBILITY_TYPE);
                    break;


                case "R_MONTH":
                    type = typeof(R_MONTH);
                    break;


                case "R_MUD_COLLECT_REASON":
                    type = typeof(R_MUD_COLLECT_REASON);
                    break;


                case "R_MUD_LOG_COLOR":
                    type = typeof(R_MUD_LOG_COLOR);
                    break;


                case "R_MUD_PROPERTY_CODE":
                    type = typeof(R_MUD_PROPERTY_CODE);
                    break;


                case "R_MUD_PROPERTY_REF":
                    type = typeof(R_MUD_PROPERTY_REF);
                    break;


                case "R_MUD_PROPERTY_TYPE":
                    type = typeof(R_MUD_PROPERTY_TYPE);
                    break;


                case "R_MUD_SAMPLE_TYPE":
                    type = typeof(R_MUD_SAMPLE_TYPE);
                    break;


                case "R_MUNICIPALITY":
                    type = typeof(R_MUNICIPALITY);
                    break;


                case "R_NAME_SET_XREF_TYPE":
                    type = typeof(R_NAME_SET_XREF_TYPE);
                    break;


                case "R_NODE_POSITION":
                    type = typeof(R_NODE_POSITION);
                    break;


                case "R_NORTH":
                    type = typeof(R_NORTH);
                    break;


                case "R_NOTIFICATION_COMP_TYPE":
                    type = typeof(R_NOTIFICATION_COMP_TYPE);
                    break;


                case "R_NOTIFICATION_TYPE":
                    type = typeof(R_NOTIFICATION_TYPE);
                    break;


                case "R_NS_DIRECTION":
                    type = typeof(R_NS_DIRECTION);
                    break;


                case "R_NS_START_LINE":
                    type = typeof(R_NS_START_LINE);
                    break;


                case "R_OBLIG_CALC_METHOD":
                    type = typeof(R_OBLIG_CALC_METHOD);
                    break;


                case "R_OBLIG_CALC_POINT":
                    type = typeof(R_OBLIG_CALC_POINT);
                    break;


                case "R_OBLIG_CATEGORY":
                    type = typeof(R_OBLIG_CATEGORY);
                    break;


                case "R_OBLIG_COMP_REASON":
                    type = typeof(R_OBLIG_COMP_REASON);
                    break;


                case "R_OBLIG_COMPONENT_TYPE":
                    type = typeof(R_OBLIG_COMPONENT_TYPE);
                    break;


                case "R_OBLIG_DEDUCT_CALC":
                    type = typeof(R_OBLIG_DEDUCT_CALC);
                    break;


                case "R_OBLIG_PARTY_TYPE":
                    type = typeof(R_OBLIG_PARTY_TYPE);
                    break;


                case "R_OBLIG_PAY_RESP":
                    type = typeof(R_OBLIG_PAY_RESP);
                    break;


                case "R_OBLIG_REMARK_TYPE":
                    type = typeof(R_OBLIG_REMARK_TYPE);
                    break;


                case "R_OBLIG_SUSPEND_PAY":
                    type = typeof(R_OBLIG_SUSPEND_PAY);
                    break;


                case "R_OBLIG_TRIGGER":
                    type = typeof(R_OBLIG_TRIGGER);
                    break;


                case "R_OBLIG_TYPE":
                    type = typeof(R_OBLIG_TYPE);
                    break;


                case "R_OBLIG_XREF_TYPE":
                    type = typeof(R_OBLIG_XREF_TYPE);
                    break;


                case "R_OFFSHORE_AREA_CODE":
                    type = typeof(R_OFFSHORE_AREA_CODE);
                    break;


                case "R_OFFSHORE_COMP_TYPE":
                    type = typeof(R_OFFSHORE_COMP_TYPE);
                    break;


                case "R_OIL_VALUE_CODE":
                    type = typeof(R_OIL_VALUE_CODE);
                    break;


                case "R_ONTOGENY_TYPE":
                    type = typeof(R_ONTOGENY_TYPE);
                    break;


                case "R_OPERAND_QUALIFIER":
                    type = typeof(R_OPERAND_QUALIFIER);
                    break;


                case "R_ORIENTATION":
                    type = typeof(R_ORIENTATION);
                    break;


                case "R_PAL_SUM_COMP_TYPE":
                    type = typeof(R_PAL_SUM_COMP_TYPE);
                    break;


                case "R_PAL_SUM_XREF_TYPE":
                    type = typeof(R_PAL_SUM_XREF_TYPE);
                    break;


                case "R_PALEO_AMOUNT_TYPE":
                    type = typeof(R_PALEO_AMOUNT_TYPE);
                    break;


                case "R_PALEO_IND_TYPE":
                    type = typeof(R_PALEO_IND_TYPE);
                    break;


                case "R_PALEO_INTERP_TYPE":
                    type = typeof(R_PALEO_INTERP_TYPE);
                    break;


                case "R_PALEO_TYPE_FOSSIL":
                    type = typeof(R_PALEO_TYPE_FOSSIL);
                    break;


                case "R_PARCEL_LOT_TYPE":
                    type = typeof(R_PARCEL_LOT_TYPE);
                    break;


                case "R_PARCEL_TYPE":
                    type = typeof(R_PARCEL_TYPE);
                    break;


                case "R_PAY_DETAIL_TYPE":
                    type = typeof(R_PAY_DETAIL_TYPE);
                    break;


                case "R_PAY_METHOD":
                    type = typeof(R_PAY_METHOD);
                    break;


                case "R_PAY_RATE_TYPE":
                    type = typeof(R_PAY_RATE_TYPE);
                    break;


                case "R_PAYMENT_TYPE":
                    type = typeof(R_PAYMENT_TYPE);
                    break;


                case "R_PAYZONE_TYPE":
                    type = typeof(R_PAYZONE_TYPE);
                    break;


                case "R_PDEN_AMEND_REASON":
                    type = typeof(R_PDEN_AMEND_REASON);
                    break;


                case "R_PDEN_COMPONENT_TYPE":
                    type = typeof(R_PDEN_COMPONENT_TYPE);
                    break;


                case "R_PDEN_STATUS":
                    type = typeof(R_PDEN_STATUS);
                    break;


                case "R_PDEN_STATUS_TYPE":
                    type = typeof(R_PDEN_STATUS_TYPE);
                    break;


                case "R_PDEN_XREF_TYPE":
                    type = typeof(R_PDEN_XREF_TYPE);
                    break;


                case "R_PERFORATION_METHOD":
                    type = typeof(R_PERFORATION_METHOD);
                    break;


                case "R_PERFORATION_TYPE":
                    type = typeof(R_PERFORATION_TYPE);
                    break;


                case "R_PERIOD_TYPE":
                    type = typeof(R_PERIOD_TYPE);
                    break;


                case "R_PERMEABILITY_TYPE":
                    type = typeof(R_PERMEABILITY_TYPE);
                    break;


                case "R_PHYS_ITEM_GROUP_TYPE":
                    type = typeof(R_PHYS_ITEM_GROUP_TYPE);
                    break;


                case "R_PHYSICAL_ITEM_STATUS":
                    type = typeof(R_PHYSICAL_ITEM_STATUS);
                    break;


                case "R_PHYSICAL_PROCESS":
                    type = typeof(R_PHYSICAL_PROCESS);
                    break;


                case "R_PICK_LOCATION":
                    type = typeof(R_PICK_LOCATION);
                    break;


                case "R_PICK_QUALIF_REASON":
                    type = typeof(R_PICK_QUALIF_REASON);
                    break;


                case "R_PICK_QUALIFIER":
                    type = typeof(R_PICK_QUALIFIER);
                    break;


                case "R_PICK_QUALITY":
                    type = typeof(R_PICK_QUALITY);
                    break;


                case "R_PICK_VERSION_TYPE":
                    type = typeof(R_PICK_VERSION_TYPE);
                    break;


                case "R_PLATFORM_TYPE":
                    type = typeof(R_PLATFORM_TYPE);
                    break;


                case "R_PLOT_SYMBOL":
                    type = typeof(R_PLOT_SYMBOL);
                    break;


                case "R_PLUG_TYPE":
                    type = typeof(R_PLUG_TYPE);
                    break;


                case "R_POOL_COMPONENT_TYPE":
                    type = typeof(R_POOL_COMPONENT_TYPE);
                    break;


                case "R_POOL_STATUS":
                    type = typeof(R_POOL_STATUS);
                    break;


                case "R_POOL_TYPE":
                    type = typeof(R_POOL_TYPE);
                    break;


                case "R_POROSITY_TYPE":
                    type = typeof(R_POROSITY_TYPE);
                    break;


                case "R_PPDM_AUDIT_REASON":
                    type = typeof(R_PPDM_AUDIT_REASON);
                    break;


                case "R_PPDM_AUDIT_TYPE":
                    type = typeof(R_PPDM_AUDIT_TYPE);
                    break;


                case "R_PPDM_BOOLEAN_RULE":
                    type = typeof(R_PPDM_BOOLEAN_RULE);
                    break;


                case "R_PPDM_CREATE_METHOD":
                    type = typeof(R_PPDM_CREATE_METHOD);
                    break;


                case "R_PPDM_DATA_TYPE":
                    type = typeof(R_PPDM_DATA_TYPE);
                    break;


                case "R_PPDM_DEFAULT_VALUE":
                    type = typeof(R_PPDM_DEFAULT_VALUE);
                    break;


                case "R_PPDM_ENFORCE_METHOD":
                    type = typeof(R_PPDM_ENFORCE_METHOD);
                    break;


                case "R_PPDM_FAIL_RESULT":
                    type = typeof(R_PPDM_FAIL_RESULT);
                    break;


                case "R_PPDM_GROUP_TYPE":
                    type = typeof(R_PPDM_GROUP_TYPE);
                    break;


                case "R_PPDM_GROUP_USE":
                    type = typeof(R_PPDM_GROUP_USE);
                    break;


                case "R_PPDM_GROUP_XREF_TYPE":
                    type = typeof(R_PPDM_GROUP_XREF_TYPE);
                    break;


                case "R_PPDM_INDEX_CATEGORY":
                    type = typeof(R_PPDM_INDEX_CATEGORY);
                    break;


                case "R_PPDM_MAP_RULE_TYPE":
                    type = typeof(R_PPDM_MAP_RULE_TYPE);
                    break;


                case "R_PPDM_MAP_TYPE":
                    type = typeof(R_PPDM_MAP_TYPE);
                    break;


                case "R_PPDM_METRIC_CODE":
                    type = typeof(R_PPDM_METRIC_CODE);
                    break;


                case "R_PPDM_METRIC_COMP_TYPE":
                    type = typeof(R_PPDM_METRIC_COMP_TYPE);
                    break;


                case "R_PPDM_METRIC_REF_VALUE":
                    type = typeof(R_PPDM_METRIC_REF_VALUE);
                    break;


                case "R_PPDM_METRIC_TYPE":
                    type = typeof(R_PPDM_METRIC_TYPE);
                    break;


                case "R_PPDM_OBJECT_STATUS":
                    type = typeof(R_PPDM_OBJECT_STATUS);
                    break;


                case "R_PPDM_OBJECT_TYPE":
                    type = typeof(R_PPDM_OBJECT_TYPE);
                    break;


                case "R_PPDM_OPERATING_SYSTEM":
                    type = typeof(R_PPDM_OPERATING_SYSTEM);
                    break;


                case "R_PPDM_OWNER_ROLE":
                    type = typeof(R_PPDM_OWNER_ROLE);
                    break;


                case "R_PPDM_PROC_TYPE":
                    type = typeof(R_PPDM_PROC_TYPE);
                    break;


                case "R_PPDM_QC_QUALITY":
                    type = typeof(R_PPDM_QC_QUALITY);
                    break;


                case "R_PPDM_QC_STATUS":
                    type = typeof(R_PPDM_QC_STATUS);
                    break;


                case "R_PPDM_QC_TYPE":
                    type = typeof(R_PPDM_QC_TYPE);
                    break;


                case "R_PPDM_RDBMS":
                    type = typeof(R_PPDM_RDBMS);
                    break;


                case "R_PPDM_REF_VALUE_TYPE":
                    type = typeof(R_PPDM_REF_VALUE_TYPE);
                    break;


                case "R_PPDM_ROW_QUALITY":
                    type = typeof(R_PPDM_ROW_QUALITY);
                    break;


                case "R_PPDM_RULE_CLASS":
                    type = typeof(R_PPDM_RULE_CLASS);
                    break;


                case "R_PPDM_RULE_COMP_TYPE":
                    type = typeof(R_PPDM_RULE_COMP_TYPE);
                    break;


                case "R_PPDM_RULE_DETAIL_TYPE":
                    type = typeof(R_PPDM_RULE_DETAIL_TYPE);
                    break;


                case "R_PPDM_RULE_PURPOSE":
                    type = typeof(R_PPDM_RULE_PURPOSE);
                    break;


                case "R_PPDM_RULE_STATUS":
                    type = typeof(R_PPDM_RULE_STATUS);
                    break;


                case "R_PPDM_RULE_USE_COND":
                    type = typeof(R_PPDM_RULE_USE_COND);
                    break;


                case "R_PPDM_RULE_XREF_COND":
                    type = typeof(R_PPDM_RULE_XREF_COND);
                    break;


                case "R_PPDM_RULE_XREF_TYPE":
                    type = typeof(R_PPDM_RULE_XREF_TYPE);
                    break;


                case "R_PPDM_SCHEMA_ENTITY":
                    type = typeof(R_PPDM_SCHEMA_ENTITY);
                    break;


                case "R_PPDM_SCHEMA_GROUP":
                    type = typeof(R_PPDM_SCHEMA_GROUP);
                    break;


                case "R_PPDM_SYSTEM_TYPE":
                    type = typeof(R_PPDM_SYSTEM_TYPE);
                    break;


                case "R_PPDM_TABLE_TYPE":
                    type = typeof(R_PPDM_TABLE_TYPE);
                    break;


                case "R_PPDM_UOM_ALIAS_TYPE":
                    type = typeof(R_PPDM_UOM_ALIAS_TYPE);
                    break;


                case "R_PPDM_UOM_USAGE":
                    type = typeof(R_PPDM_UOM_USAGE);
                    break;


                case "R_PRESERVE_QUALITY":
                    type = typeof(R_PRESERVE_QUALITY);
                    break;


                case "R_PRESERVE_TYPE":
                    type = typeof(R_PRESERVE_TYPE);
                    break;


                case "R_PROD_STR_FM_STAT_TYPE":
                    type = typeof(R_PROD_STR_FM_STAT_TYPE);
                    break;


                case "R_PROD_STR_FM_STATUS":
                    type = typeof(R_PROD_STR_FM_STATUS);
                    break;


                case "R_PROD_STRING_COMP_TYPE":
                    type = typeof(R_PROD_STRING_COMP_TYPE);
                    break;


                case "R_PROD_STRING_STAT_TYPE":
                    type = typeof(R_PROD_STRING_STAT_TYPE);
                    break;


                case "R_PROD_STRING_STATUS":
                    type = typeof(R_PROD_STRING_STATUS);
                    break;


                case "R_PROD_STRING_TYPE":
                    type = typeof(R_PROD_STRING_TYPE);
                    break;


                case "R_PRODUCT_COMPONENT_TYPE":
                    type = typeof(R_PRODUCT_COMPONENT_TYPE);
                    break;


                case "R_PRODUCTION_METHOD":
                    type = typeof(R_PRODUCTION_METHOD);
                    break;


                case "R_PROJ_STEP_TYPE":
                    type = typeof(R_PROJ_STEP_TYPE);
                    break;


                case "R_PROJ_STEP_XREF_TYPE":
                    type = typeof(R_PROJ_STEP_XREF_TYPE);
                    break;


                case "R_PROJECT_ALIAS_TYPE":
                    type = typeof(R_PROJECT_ALIAS_TYPE);
                    break;


                case "R_PROJECT_BA_ROLE":
                    type = typeof(R_PROJECT_BA_ROLE);
                    break;


                case "R_PROJECT_COMP_REASON":
                    type = typeof(R_PROJECT_COMP_REASON);
                    break;


                case "R_PROJECT_COMP_TYPE":
                    type = typeof(R_PROJECT_COMP_TYPE);
                    break;


                case "R_PROJECT_CONDITION":
                    type = typeof(R_PROJECT_CONDITION);
                    break;


                case "R_PROJECT_EQUIP_ROLE":
                    type = typeof(R_PROJECT_EQUIP_ROLE);
                    break;


                case "R_PROJECT_STATUS":
                    type = typeof(R_PROJECT_STATUS);
                    break;


                case "R_PROJECT_STATUS_TYPE":
                    type = typeof(R_PROJECT_STATUS_TYPE);
                    break;


                case "R_PROJECT_TYPE":
                    type = typeof(R_PROJECT_TYPE);
                    break;


                case "R_PROJECTION_TYPE":
                    type = typeof(R_PROJECTION_TYPE);
                    break;


                case "R_PROPPANT_TYPE":
                    type = typeof(R_PROPPANT_TYPE);
                    break;


                case "R_PUBLICATION_NAME":
                    type = typeof(R_PUBLICATION_NAME);
                    break;


                case "R_QUALIFIER_TYPE":
                    type = typeof(R_QUALIFIER_TYPE);
                    break;


                case "R_QUALITY":
                    type = typeof(R_QUALITY);
                    break;


                case "R_RATE_CONDITION":
                    type = typeof(R_RATE_CONDITION);
                    break;


                case "R_RATE_SCHED_XREF":
                    type = typeof(R_RATE_SCHED_XREF);
                    break;


                case "R_RATE_SCHEDULE":
                    type = typeof(R_RATE_SCHEDULE);
                    break;


                case "R_RATE_SCHEDULE_COMP_TYPE":
                    type = typeof(R_RATE_SCHEDULE_COMP_TYPE);
                    break;


                case "R_RATE_TYPE":
                    type = typeof(R_RATE_TYPE);
                    break;


                case "R_RATIO_CURVE_TYPE":
                    type = typeof(R_RATIO_CURVE_TYPE);
                    break;


                case "R_RATIO_FLUID_TYPE":
                    type = typeof(R_RATIO_FLUID_TYPE);
                    break;


                case "R_RECORDER_POSITION":
                    type = typeof(R_RECORDER_POSITION);
                    break;


                case "R_RECORDER_TYPE":
                    type = typeof(R_RECORDER_TYPE);
                    break;


                case "R_REMARK_TYPE":
                    type = typeof(R_REMARK_TYPE);
                    break;


                case "R_REMARK_USE_TYPE":
                    type = typeof(R_REMARK_USE_TYPE);
                    break;


                case "R_REP_HIER_ALIAS_TYPE":
                    type = typeof(R_REP_HIER_ALIAS_TYPE);
                    break;


                case "R_REPEAT_STRAT_TYPE":
                    type = typeof(R_REPEAT_STRAT_TYPE);
                    break;


                case "R_REPORT_HIER_COMP":
                    type = typeof(R_REPORT_HIER_COMP);
                    break;


                case "R_REPORT_HIER_LEVEL":
                    type = typeof(R_REPORT_HIER_LEVEL);
                    break;


                case "R_REPORT_HIER_TYPE":
                    type = typeof(R_REPORT_HIER_TYPE);
                    break;


                case "R_RESENT_COMP_TYPE":
                    type = typeof(R_RESENT_COMP_TYPE);
                    break;


                case "R_RESENT_CONFIDENCE":
                    type = typeof(R_RESENT_CONFIDENCE);
                    break;


                case "R_RESENT_GROUP_TYPE":
                    type = typeof(R_RESENT_GROUP_TYPE);
                    break;


                case "R_RESENT_REV_CAT":
                    type = typeof(R_RESENT_REV_CAT);
                    break;


                case "R_RESENT_USE_TYPE":
                    type = typeof(R_RESENT_USE_TYPE);
                    break;


                case "R_RESENT_VOL_METHOD":
                    type = typeof(R_RESENT_VOL_METHOD);
                    break;


                case "R_RESENT_XREF_TYPE":
                    type = typeof(R_RESENT_XREF_TYPE);
                    break;


                case "R_REST_ACTIVITY":
                    type = typeof(R_REST_ACTIVITY);
                    break;


                case "R_REST_DURATION":
                    type = typeof(R_REST_DURATION);
                    break;


                case "R_REST_REMARK":
                    type = typeof(R_REST_REMARK);
                    break;


                case "R_REST_TYPE":
                    type = typeof(R_REST_TYPE);
                    break;


                case "R_RETENTION_PERIOD":
                    type = typeof(R_RETENTION_PERIOD);
                    break;


                case "R_REVISION_METHOD":
                    type = typeof(R_REVISION_METHOD);
                    break;


                case "R_RIG_BLOWOUT_PREVENTER":
                    type = typeof(R_RIG_BLOWOUT_PREVENTER);
                    break;


                case "R_RIG_CATEGORY":
                    type = typeof(R_RIG_CATEGORY);
                    break;


                case "R_RIG_CLASS":
                    type = typeof(R_RIG_CLASS);
                    break;


                case "R_RIG_CODE":
                    type = typeof(R_RIG_CODE);
                    break;


                case "R_RIG_DESANDER_TYPE":
                    type = typeof(R_RIG_DESANDER_TYPE);
                    break;


                case "R_RIG_DESILTER_TYPE":
                    type = typeof(R_RIG_DESILTER_TYPE);
                    break;


                case "R_RIG_DRAWWORKS":
                    type = typeof(R_RIG_DRAWWORKS);
                    break;


                case "R_RIG_GENERATOR_TYPE":
                    type = typeof(R_RIG_GENERATOR_TYPE);
                    break;


                case "R_RIG_HOOKBLOCK_TYPE":
                    type = typeof(R_RIG_HOOKBLOCK_TYPE);
                    break;


                case "R_RIG_MAST":
                    type = typeof(R_RIG_MAST);
                    break;


                case "R_RIG_OVERHEAD_CAPACITY":
                    type = typeof(R_RIG_OVERHEAD_CAPACITY);
                    break;


                case "R_RIG_OVERHEAD_TYPE":
                    type = typeof(R_RIG_OVERHEAD_TYPE);
                    break;


                case "R_RIG_PUMP":
                    type = typeof(R_RIG_PUMP);
                    break;


                case "R_RIG_PUMP_FUNCTION":
                    type = typeof(R_RIG_PUMP_FUNCTION);
                    break;


                case "R_RIG_SUBSTRUCTURE":
                    type = typeof(R_RIG_SUBSTRUCTURE);
                    break;


                case "R_RIG_SWIVEL_TYPE":
                    type = typeof(R_RIG_SWIVEL_TYPE);
                    break;


                case "R_RIG_TYPE":
                    type = typeof(R_RIG_TYPE);
                    break;


                case "R_RM_THESAURUS_XREF":
                    type = typeof(R_RM_THESAURUS_XREF);
                    break;


                case "R_RMII_CONTACT_TYPE":
                    type = typeof(R_RMII_CONTACT_TYPE);
                    break;


                case "R_RMII_CONTENT_TYPE":
                    type = typeof(R_RMII_CONTENT_TYPE);
                    break;


                case "R_RMII_DEFICIENCY":
                    type = typeof(R_RMII_DEFICIENCY);
                    break;


                case "R_RMII_DESC_TYPE":
                    type = typeof(R_RMII_DESC_TYPE);
                    break;


                case "R_RMII_GROUP_TYPE":
                    type = typeof(R_RMII_GROUP_TYPE);
                    break;


                case "R_RMII_METADATA_CODE":
                    type = typeof(R_RMII_METADATA_CODE);
                    break;


                case "R_RMII_METADATA_TYPE":
                    type = typeof(R_RMII_METADATA_TYPE);
                    break;


                case "R_RMII_QUALITY_CODE":
                    type = typeof(R_RMII_QUALITY_CODE);
                    break;


                case "R_RMII_STATUS":
                    type = typeof(R_RMII_STATUS);
                    break;


                case "R_RMII_STATUS_TYPE":
                    type = typeof(R_RMII_STATUS_TYPE);
                    break;


                case "R_ROAD_CONDITION":
                    type = typeof(R_ROAD_CONDITION);
                    break;


                case "R_ROAD_CONTROL_TYPE":
                    type = typeof(R_ROAD_CONTROL_TYPE);
                    break;


                case "R_ROAD_DRIVING_SIDE":
                    type = typeof(R_ROAD_DRIVING_SIDE);
                    break;


                case "R_ROAD_TRAFFIC_FLOW_TYPE":
                    type = typeof(R_ROAD_TRAFFIC_FLOW_TYPE);
                    break;


                case "R_ROCK_CLASS_SCHEME":
                    type = typeof(R_ROCK_CLASS_SCHEME);
                    break;


                case "R_ROLL_ALONG_METHOD":
                    type = typeof(R_ROLL_ALONG_METHOD);
                    break;


                case "R_ROYALTY_TYPE":
                    type = typeof(R_ROYALTY_TYPE);
                    break;


                case "R_SALINITY_TYPE":
                    type = typeof(R_SALINITY_TYPE);
                    break;


                case "R_SAMPLE_COLLECT_METHOD":
                    type = typeof(R_SAMPLE_COLLECT_METHOD);
                    break;


                case "R_SAMPLE_COLLECTION_TYPE":
                    type = typeof(R_SAMPLE_COLLECTION_TYPE);
                    break;


                case "R_SAMPLE_COMP_TYPE":
                    type = typeof(R_SAMPLE_COMP_TYPE);
                    break;


                case "R_SAMPLE_CONTAMINANT":
                    type = typeof(R_SAMPLE_CONTAMINANT);
                    break;


                case "R_SAMPLE_DESC_CODE":
                    type = typeof(R_SAMPLE_DESC_CODE);
                    break;


                case "R_SAMPLE_DESC_TYPE":
                    type = typeof(R_SAMPLE_DESC_TYPE);
                    break;


                case "R_SAMPLE_FRACTION_TYPE":
                    type = typeof(R_SAMPLE_FRACTION_TYPE);
                    break;


                case "R_SAMPLE_LOCATION":
                    type = typeof(R_SAMPLE_LOCATION);
                    break;


                case "R_SAMPLE_PHASE":
                    type = typeof(R_SAMPLE_PHASE);
                    break;


                case "R_SAMPLE_PREP_CLASS":
                    type = typeof(R_SAMPLE_PREP_CLASS);
                    break;


                case "R_SAMPLE_REF_VALUE_TYPE":
                    type = typeof(R_SAMPLE_REF_VALUE_TYPE);
                    break;


                case "R_SAMPLE_SHAPE":
                    type = typeof(R_SAMPLE_SHAPE);
                    break;


                case "R_SAMPLE_TYPE":
                    type = typeof(R_SAMPLE_TYPE);
                    break;


                case "R_SCALE_TRANSFORM":
                    type = typeof(R_SCALE_TRANSFORM);
                    break;


                case "R_SCREEN_LOCATION":
                    type = typeof(R_SCREEN_LOCATION);
                    break;


                case "R_SECTION_TYPE":
                    type = typeof(R_SECTION_TYPE);
                    break;


                case "R_SEIS_3D_TYPE":
                    type = typeof(R_SEIS_3D_TYPE);
                    break;


                case "R_SEIS_ACTIVITY_CLASS":
                    type = typeof(R_SEIS_ACTIVITY_CLASS);
                    break;


                case "R_SEIS_ACTIVITY_TYPE":
                    type = typeof(R_SEIS_ACTIVITY_TYPE);
                    break;


                case "R_SEIS_AUTHORIZE_LIMIT":
                    type = typeof(R_SEIS_AUTHORIZE_LIMIT);
                    break;


                case "R_SEIS_AUTHORIZE_REASON":
                    type = typeof(R_SEIS_AUTHORIZE_REASON);
                    break;


                case "R_SEIS_AUTHORIZE_TYPE":
                    type = typeof(R_SEIS_AUTHORIZE_TYPE);
                    break;


                case "R_SEIS_BIN_METHOD":
                    type = typeof(R_SEIS_BIN_METHOD);
                    break;


                case "R_SEIS_BIN_OUTLINE_TYPE":
                    type = typeof(R_SEIS_BIN_OUTLINE_TYPE);
                    break;


                case "R_SEIS_CABLE_MAKE":
                    type = typeof(R_SEIS_CABLE_MAKE);
                    break;


                case "R_SEIS_CHANNEL_TYPE":
                    type = typeof(R_SEIS_CHANNEL_TYPE);
                    break;


                case "R_SEIS_DIMENSION":
                    type = typeof(R_SEIS_DIMENSION);
                    break;


                case "R_SEIS_ENERGY_TYPE":
                    type = typeof(R_SEIS_ENERGY_TYPE);
                    break;


                case "R_SEIS_FLOW_DESC_TYPE":
                    type = typeof(R_SEIS_FLOW_DESC_TYPE);
                    break;


                case "R_SEIS_GROUP_TYPE":
                    type = typeof(R_SEIS_GROUP_TYPE);
                    break;


                case "R_SEIS_INSP_COMPONENT_TYPE":
                    type = typeof(R_SEIS_INSP_COMPONENT_TYPE);
                    break;


                case "R_SEIS_LIC_COND":
                    type = typeof(R_SEIS_LIC_COND);
                    break;


                case "R_SEIS_LIC_COND_CODE":
                    type = typeof(R_SEIS_LIC_COND_CODE);
                    break;


                case "R_SEIS_LIC_DUE_CONDITION":
                    type = typeof(R_SEIS_LIC_DUE_CONDITION);
                    break;


                case "R_SEIS_LIC_VIOL_RESOL":
                    type = typeof(R_SEIS_LIC_VIOL_RESOL);
                    break;


                case "R_SEIS_LIC_VIOL_TYPE":
                    type = typeof(R_SEIS_LIC_VIOL_TYPE);
                    break;


                case "R_SEIS_PARM_ORIGIN":
                    type = typeof(R_SEIS_PARM_ORIGIN);
                    break;


                case "R_SEIS_PATCH_TYPE":
                    type = typeof(R_SEIS_PATCH_TYPE);
                    break;


                case "R_SEIS_PICK_METHOD":
                    type = typeof(R_SEIS_PICK_METHOD);
                    break;


                case "R_SEIS_PROC_COMP_TYPE":
                    type = typeof(R_SEIS_PROC_COMP_TYPE);
                    break;


                case "R_SEIS_PROC_PARM":
                    type = typeof(R_SEIS_PROC_PARM);
                    break;


                case "R_SEIS_PROC_SET_TYPE":
                    type = typeof(R_SEIS_PROC_SET_TYPE);
                    break;


                case "R_SEIS_PROC_STATUS":
                    type = typeof(R_SEIS_PROC_STATUS);
                    break;


                case "R_SEIS_PROC_STEP_NAME":
                    type = typeof(R_SEIS_PROC_STEP_NAME);
                    break;


                case "R_SEIS_PROC_STEP_TYPE":
                    type = typeof(R_SEIS_PROC_STEP_TYPE);
                    break;


                case "R_SEIS_RCRD_FMT_TYPE":
                    type = typeof(R_SEIS_RCRD_FMT_TYPE);
                    break;


                case "R_SEIS_RCRD_MAKE":
                    type = typeof(R_SEIS_RCRD_MAKE);
                    break;


                case "R_SEIS_RCVR_ARRY_TYPE":
                    type = typeof(R_SEIS_RCVR_ARRY_TYPE);
                    break;


                case "R_SEIS_RCVR_TYPE":
                    type = typeof(R_SEIS_RCVR_TYPE);
                    break;


                case "R_SEIS_RECORD_TYPE":
                    type = typeof(R_SEIS_RECORD_TYPE);
                    break;


                case "R_SEIS_REF_DATUM":
                    type = typeof(R_SEIS_REF_DATUM);
                    break;


                case "R_SEIS_REF_NUM_TYPE":
                    type = typeof(R_SEIS_REF_NUM_TYPE);
                    break;


                case "R_SEIS_SAMPLE_TYPE":
                    type = typeof(R_SEIS_SAMPLE_TYPE);
                    break;


                case "R_SEIS_SEGMENT_REASON":
                    type = typeof(R_SEIS_SEGMENT_REASON);
                    break;


                case "R_SEIS_SET_COMPONENT_TYPE":
                    type = typeof(R_SEIS_SET_COMPONENT_TYPE);
                    break;


                case "R_SEIS_SPECTRUM_TYPE":
                    type = typeof(R_SEIS_SPECTRUM_TYPE);
                    break;


                case "R_SEIS_SRC_ARRAY_TYPE":
                    type = typeof(R_SEIS_SRC_ARRAY_TYPE);
                    break;


                case "R_SEIS_SRC_MAKE":
                    type = typeof(R_SEIS_SRC_MAKE);
                    break;


                case "R_SEIS_STATION_TYPE":
                    type = typeof(R_SEIS_STATION_TYPE);
                    break;


                case "R_SEIS_STATUS":
                    type = typeof(R_SEIS_STATUS);
                    break;


                case "R_SEIS_STATUS_TYPE":
                    type = typeof(R_SEIS_STATUS_TYPE);
                    break;


                case "R_SEIS_SUMMARY_TYPE":
                    type = typeof(R_SEIS_SUMMARY_TYPE);
                    break;


                case "R_SEIS_SWEEP_TYPE":
                    type = typeof(R_SEIS_SWEEP_TYPE);
                    break;


                case "R_SEIS_TRANS_COMP_TYPE":
                    type = typeof(R_SEIS_TRANS_COMP_TYPE);
                    break;


                case "R_SEISMIC_PATH":
                    type = typeof(R_SEISMIC_PATH);
                    break;


                case "R_SEND_METHOD":
                    type = typeof(R_SEND_METHOD);
                    break;


                case "R_SERVICE_QUALITY":
                    type = typeof(R_SERVICE_QUALITY);
                    break;


                case "R_SEVERITY":
                    type = typeof(R_SEVERITY);
                    break;


                case "R_SF_AIRSTRIP_TYPE":
                    type = typeof(R_SF_AIRSTRIP_TYPE);
                    break;


                case "R_SF_BRIDGE_TYPE":
                    type = typeof(R_SF_BRIDGE_TYPE);
                    break;


                case "R_SF_COMPONENT_TYPE":
                    type = typeof(R_SF_COMPONENT_TYPE);
                    break;


                case "R_SF_DESC_TYPE":
                    type = typeof(R_SF_DESC_TYPE);
                    break;


                case "R_SF_DESC_VALUE":
                    type = typeof(R_SF_DESC_VALUE);
                    break;


                case "R_SF_DOCK_TYPE":
                    type = typeof(R_SF_DOCK_TYPE);
                    break;


                case "R_SF_ELECTRIC_TYPE":
                    type = typeof(R_SF_ELECTRIC_TYPE);
                    break;


                case "R_SF_LANDING_TYPE":
                    type = typeof(R_SF_LANDING_TYPE);
                    break;


                case "R_SF_MAINTAIN_TYPE":
                    type = typeof(R_SF_MAINTAIN_TYPE);
                    break;


                case "R_SF_PAD_TYPE":
                    type = typeof(R_SF_PAD_TYPE);
                    break;


                case "R_SF_ROAD_TYPE":
                    type = typeof(R_SF_ROAD_TYPE);
                    break;


                case "R_SF_STATUS":
                    type = typeof(R_SF_STATUS);
                    break;


                case "R_SF_STATUS_TYPE":
                    type = typeof(R_SF_STATUS_TYPE);
                    break;


                case "R_SF_SURFACE_TYPE":
                    type = typeof(R_SF_SURFACE_TYPE);
                    break;


                case "R_SF_TOWER_TYPE":
                    type = typeof(R_SF_TOWER_TYPE);
                    break;


                case "R_SF_VEHICLE_TYPE":
                    type = typeof(R_SF_VEHICLE_TYPE);
                    break;


                case "R_SF_VESSEL_ROLE":
                    type = typeof(R_SF_VESSEL_ROLE);
                    break;


                case "R_SF_VESSEL_TYPE":
                    type = typeof(R_SF_VESSEL_TYPE);
                    break;


                case "R_SF_XREF_TYPE":
                    type = typeof(R_SF_XREF_TYPE);
                    break;


                case "R_SHOW_TYPE":
                    type = typeof(R_SHOW_TYPE);
                    break;


                case "R_SHUTIN_PRESS_TYPE":
                    type = typeof(R_SHUTIN_PRESS_TYPE);
                    break;


                case "R_SOURCE":
                    type = typeof(R_SOURCE);
                    break;


                case "R_SOURCE_ORIGIN":
                    type = typeof(R_SOURCE_ORIGIN);
                    break;


                case "R_SP_POINT_VERSION_TYPE":
                    type = typeof(R_SP_POINT_VERSION_TYPE);
                    break;


                case "R_SP_ZONE_DEFIN_XREF":
                    type = typeof(R_SP_ZONE_DEFIN_XREF);
                    break;


                case "R_SP_ZONE_DERIV":
                    type = typeof(R_SP_ZONE_DERIV);
                    break;


                case "R_SP_ZONE_TYPE":
                    type = typeof(R_SP_ZONE_TYPE);
                    break;


                case "R_SPACING_UNIT_TYPE":
                    type = typeof(R_SPACING_UNIT_TYPE);
                    break;


                case "R_SPATIAL_DESC_COMP_TYPE":
                    type = typeof(R_SPATIAL_DESC_COMP_TYPE);
                    break;


                case "R_SPATIAL_DESC_TYPE":
                    type = typeof(R_SPATIAL_DESC_TYPE);
                    break;


                case "R_SPATIAL_XREF_TYPE":
                    type = typeof(R_SPATIAL_XREF_TYPE);
                    break;


                case "R_STATUS_GROUP":
                    type = typeof(R_STATUS_GROUP);
                    break;


                case "R_STORE_STATUS":
                    type = typeof(R_STORE_STATUS);
                    break;


                case "R_STRAT_ACQTN_METHOD":
                    type = typeof(R_STRAT_ACQTN_METHOD);
                    break;


                case "R_STRAT_AGE_METHOD":
                    type = typeof(R_STRAT_AGE_METHOD);
                    break;


                case "R_STRAT_ALIAS_TYPE":
                    type = typeof(R_STRAT_ALIAS_TYPE);
                    break;


                case "R_STRAT_COL_XREF_TYPE":
                    type = typeof(R_STRAT_COL_XREF_TYPE);
                    break;


                case "R_STRAT_COLUMN_TYPE":
                    type = typeof(R_STRAT_COLUMN_TYPE);
                    break;


                case "R_STRAT_CORR_CRITERIA":
                    type = typeof(R_STRAT_CORR_CRITERIA);
                    break;


                case "R_STRAT_CORR_TYPE":
                    type = typeof(R_STRAT_CORR_TYPE);
                    break;


                case "R_STRAT_DESC_TYPE":
                    type = typeof(R_STRAT_DESC_TYPE);
                    break;


                case "R_STRAT_EQUIV_DIRECT":
                    type = typeof(R_STRAT_EQUIV_DIRECT);
                    break;


                case "R_STRAT_EQUIV_TYPE":
                    type = typeof(R_STRAT_EQUIV_TYPE);
                    break;


                case "R_STRAT_FLD_NODE_LOC":
                    type = typeof(R_STRAT_FLD_NODE_LOC);
                    break;


                case "R_STRAT_HIERARCHY":
                    type = typeof(R_STRAT_HIERARCHY);
                    break;


                case "R_STRAT_INTERP_METHOD":
                    type = typeof(R_STRAT_INTERP_METHOD);
                    break;


                case "R_STRAT_NAME_SET_TYPE":
                    type = typeof(R_STRAT_NAME_SET_TYPE);
                    break;


                case "R_STRAT_OCCURRENCE_TYPE":
                    type = typeof(R_STRAT_OCCURRENCE_TYPE);
                    break;


                case "R_STRAT_STATUS":
                    type = typeof(R_STRAT_STATUS);
                    break;


                case "R_STRAT_TOPO_RELATION":
                    type = typeof(R_STRAT_TOPO_RELATION);
                    break;


                case "R_STRAT_TYPE":
                    type = typeof(R_STRAT_TYPE);
                    break;


                case "R_STRAT_UNIT_COMP_TYPE":
                    type = typeof(R_STRAT_UNIT_COMP_TYPE);
                    break;


                case "R_STRAT_UNIT_DESC":
                    type = typeof(R_STRAT_UNIT_DESC);
                    break;


                case "R_STRAT_UNIT_QUALIFIER":
                    type = typeof(R_STRAT_UNIT_QUALIFIER);
                    break;


                case "R_STRAT_UNIT_TYPE":
                    type = typeof(R_STRAT_UNIT_TYPE);
                    break;


                case "R_STREAMER_COMP":
                    type = typeof(R_STREAMER_COMP);
                    break;


                case "R_STREAMER_POSITION":
                    type = typeof(R_STREAMER_POSITION);
                    break;


                case "R_STUDY_TYPE":
                    type = typeof(R_STUDY_TYPE);
                    break;


                case "R_SUBSTANCE_COMP_TYPE":
                    type = typeof(R_SUBSTANCE_COMP_TYPE);
                    break;


                case "R_SUBSTANCE_PROPERTY":
                    type = typeof(R_SUBSTANCE_PROPERTY);
                    break;


                case "R_SUBSTANCE_USE_RULE":
                    type = typeof(R_SUBSTANCE_USE_RULE);
                    break;


                case "R_SUBSTANCE_XREF_TYPE":
                    type = typeof(R_SUBSTANCE_XREF_TYPE);
                    break;


                case "R_SW_APP_BA_ROLE":
                    type = typeof(R_SW_APP_BA_ROLE);
                    break;


                case "R_SW_APP_FUNCTION":
                    type = typeof(R_SW_APP_FUNCTION);
                    break;


                case "R_SW_APP_FUNCTION_TYPE":
                    type = typeof(R_SW_APP_FUNCTION_TYPE);
                    break;


                case "R_SW_APP_XREF_TYPE":
                    type = typeof(R_SW_APP_XREF_TYPE);
                    break;


                case "R_TAX_CREDIT_CODE":
                    type = typeof(R_TAX_CREDIT_CODE);
                    break;


                case "R_TEST_EQUIPMENT":
                    type = typeof(R_TEST_EQUIPMENT);
                    break;


                case "R_TEST_PERIOD_TYPE":
                    type = typeof(R_TEST_PERIOD_TYPE);
                    break;


                case "R_TEST_RECOV_METHOD":
                    type = typeof(R_TEST_RECOV_METHOD);
                    break;


                case "R_TEST_RESULT":
                    type = typeof(R_TEST_RESULT);
                    break;


                case "R_TEST_SHUTOFF_TYPE":
                    type = typeof(R_TEST_SHUTOFF_TYPE);
                    break;


                case "R_TEST_SUBTYPE":
                    type = typeof(R_TEST_SUBTYPE);
                    break;


                case "R_TIMEZONE":
                    type = typeof(R_TIMEZONE);
                    break;


                case "R_TITLE_OWN_TYPE":
                    type = typeof(R_TITLE_OWN_TYPE);
                    break;


                case "R_TOUR_OCCURRENCE_TYPE":
                    type = typeof(R_TOUR_OCCURRENCE_TYPE);
                    break;


                case "R_TRACE_HEADER_FORMAT":
                    type = typeof(R_TRACE_HEADER_FORMAT);
                    break;


                case "R_TRACE_HEADER_WORD":
                    type = typeof(R_TRACE_HEADER_WORD);
                    break;


                case "R_TRANS_COMP_TYPE":
                    type = typeof(R_TRANS_COMP_TYPE);
                    break;


                case "R_TRANS_STATUS":
                    type = typeof(R_TRANS_STATUS);
                    break;


                case "R_TRANS_TYPE":
                    type = typeof(R_TRANS_TYPE);
                    break;


                case "R_TREATMENT_FLUID":
                    type = typeof(R_TREATMENT_FLUID);
                    break;


                case "R_TREATMENT_TYPE":
                    type = typeof(R_TREATMENT_TYPE);
                    break;


                case "R_TUBING_GRADE":
                    type = typeof(R_TUBING_GRADE);
                    break;


                case "R_TUBING_TYPE":
                    type = typeof(R_TUBING_TYPE);
                    break;


                case "R_TVD_METHOD":
                    type = typeof(R_TVD_METHOD);
                    break;


                case "R_VALUE_QUALITY":
                    type = typeof(R_VALUE_QUALITY);
                    break;


                case "R_VELOCITY_COMPUTE":
                    type = typeof(R_VELOCITY_COMPUTE);
                    break;


                case "R_VELOCITY_DIMENSION":
                    type = typeof(R_VELOCITY_DIMENSION);
                    break;


                case "R_VELOCITY_TYPE":
                    type = typeof(R_VELOCITY_TYPE);
                    break;


                case "R_VERTICAL_DATUM_TYPE":
                    type = typeof(R_VERTICAL_DATUM_TYPE);
                    break;


                case "R_VESSEL_REFERENCE":
                    type = typeof(R_VESSEL_REFERENCE);
                    break;


                case "R_VESSEL_SIZE":
                    type = typeof(R_VESSEL_SIZE);
                    break;


                case "R_VOLUME_FRACTION":
                    type = typeof(R_VOLUME_FRACTION);
                    break;


                case "R_VOLUME_METHOD":
                    type = typeof(R_VOLUME_METHOD);
                    break;


                case "R_VSP_TYPE":
                    type = typeof(R_VSP_TYPE);
                    break;


                case "R_WASTE_ADJUST_REASON":
                    type = typeof(R_WASTE_ADJUST_REASON);
                    break;


                case "R_WASTE_FACILITY_TYPE":
                    type = typeof(R_WASTE_FACILITY_TYPE);
                    break;


                case "R_WASTE_HANDLING":
                    type = typeof(R_WASTE_HANDLING);
                    break;


                case "R_WASTE_HAZARD_TYPE":
                    type = typeof(R_WASTE_HAZARD_TYPE);
                    break;


                case "R_WASTE_MATERIAL":
                    type = typeof(R_WASTE_MATERIAL);
                    break;


                case "R_WASTE_ORIGIN":
                    type = typeof(R_WASTE_ORIGIN);
                    break;


                case "R_WATER_BOTTOM_ZONE":
                    type = typeof(R_WATER_BOTTOM_ZONE);
                    break;


                case "R_WATER_CONDITION":
                    type = typeof(R_WATER_CONDITION);
                    break;


                case "R_WATER_DATUM":
                    type = typeof(R_WATER_DATUM);
                    break;


                case "R_WATER_PROPERTY_CODE":
                    type = typeof(R_WATER_PROPERTY_CODE);
                    break;


                case "R_WEATHER_CONDITION":
                    type = typeof(R_WEATHER_CONDITION);
                    break;


                case "R_WELL_ACT_TYPE_EQUIV":
                    type = typeof(R_WELL_ACT_TYPE_EQUIV);
                    break;


                case "R_WELL_ACTIVITY_CAUSE":
                    type = typeof(R_WELL_ACTIVITY_CAUSE);
                    break;


                case "R_WELL_ACTIVITY_COMP_TYPE":
                    type = typeof(R_WELL_ACTIVITY_COMP_TYPE);
                    break;


                case "R_WELL_ALIAS_LOCATION":
                    type = typeof(R_WELL_ALIAS_LOCATION);
                    break;


                case "R_WELL_CIRC_PRESS_TYPE":
                    type = typeof(R_WELL_CIRC_PRESS_TYPE);
                    break;


                case "R_WELL_CLASS":
                    type = typeof(R_WELL_CLASS);
                    break;


                case "R_WELL_COMPONENT_TYPE":
                    type = typeof(R_WELL_COMPONENT_TYPE);
                    break;


                case "R_WELL_DATUM_TYPE":
                    type = typeof(R_WELL_DATUM_TYPE);
                    break;


                case "R_WELL_DOWNTIME_TYPE":
                    type = typeof(R_WELL_DOWNTIME_TYPE);
                    break;


                case "R_WELL_DRILL_OP_TYPE":
                    type = typeof(R_WELL_DRILL_OP_TYPE);
                    break;


                case "R_WELL_FACILITY_USE_TYPE":
                    type = typeof(R_WELL_FACILITY_USE_TYPE);
                    break;


                case "R_WELL_LEVEL_TYPE":
                    type = typeof(R_WELL_LEVEL_TYPE);
                    break;


                case "R_WELL_LIC_COND":
                    type = typeof(R_WELL_LIC_COND);
                    break;


                case "R_WELL_LIC_COND_CODE":
                    type = typeof(R_WELL_LIC_COND_CODE);
                    break;


                case "R_WELL_LIC_DUE_CONDITION":
                    type = typeof(R_WELL_LIC_DUE_CONDITION);
                    break;


                case "R_WELL_LIC_VIOL_RESOL":
                    type = typeof(R_WELL_LIC_VIOL_RESOL);
                    break;


                case "R_WELL_LIC_VIOL_TYPE":
                    type = typeof(R_WELL_LIC_VIOL_TYPE);
                    break;


                case "R_WELL_LOG_CLASS":
                    type = typeof(R_WELL_LOG_CLASS);
                    break;


                case "R_WELL_NODE_PICK_METHOD":
                    type = typeof(R_WELL_NODE_PICK_METHOD);
                    break;


                case "R_WELL_PROFILE_TYPE":
                    type = typeof(R_WELL_PROFILE_TYPE);
                    break;


                case "R_WELL_QUALIFIC_TYPE":
                    type = typeof(R_WELL_QUALIFIC_TYPE);
                    break;


                case "R_WELL_REF_VALUE_TYPE":
                    type = typeof(R_WELL_REF_VALUE_TYPE);
                    break;


                case "R_WELL_RELATIONSHIP":
                    type = typeof(R_WELL_RELATIONSHIP);
                    break;


                case "R_WELL_SERV_METRIC_CODE":
                    type = typeof(R_WELL_SERV_METRIC_CODE);
                    break;


                case "R_WELL_SERVICE_METRIC":
                    type = typeof(R_WELL_SERVICE_METRIC);
                    break;


                case "R_WELL_SET_ROLE":
                    type = typeof(R_WELL_SET_ROLE);
                    break;


                case "R_WELL_SET_TYPE":
                    type = typeof(R_WELL_SET_TYPE);
                    break;


                case "R_WELL_SF_USE_TYPE":
                    type = typeof(R_WELL_SF_USE_TYPE);
                    break;


                case "R_WELL_STATUS":
                    type = typeof(R_WELL_STATUS);
                    break;


                case "R_WELL_STATUS_QUAL":
                    type = typeof(R_WELL_STATUS_QUAL);
                    break;


                case "R_WELL_STATUS_QUAL_VALUE":
                    type = typeof(R_WELL_STATUS_QUAL_VALUE);
                    break;


                case "R_WELL_STATUS_SYMBOL":
                    type = typeof(R_WELL_STATUS_SYMBOL);
                    break;


                case "R_WELL_STATUS_TYPE":
                    type = typeof(R_WELL_STATUS_TYPE);
                    break;


                case "R_WELL_STATUS_XREF":
                    type = typeof(R_WELL_STATUS_XREF);
                    break;


                case "R_WELL_TEST_TYPE":
                    type = typeof(R_WELL_TEST_TYPE);
                    break;


                case "R_WELL_XREF_TYPE":
                    type = typeof(R_WELL_XREF_TYPE);
                    break;


                case "R_WELL_ZONE_INT_VALUE":
                    type = typeof(R_WELL_ZONE_INT_VALUE);
                    break;


                case "R_WIND_STRENGTH":
                    type = typeof(R_WIND_STRENGTH);
                    break;


                case "R_WO_BA_ROLE":
                    type = typeof(R_WO_BA_ROLE);
                    break;


                case "R_WO_COMPONENT_TYPE":
                    type = typeof(R_WO_COMPONENT_TYPE);
                    break;


                case "R_WO_CONDITION":
                    type = typeof(R_WO_CONDITION);
                    break;


                case "R_WO_DELIVERY_TYPE":
                    type = typeof(R_WO_DELIVERY_TYPE);
                    break;


                case "R_WO_INSTRUCTION":
                    type = typeof(R_WO_INSTRUCTION);
                    break;


                case "R_WO_TYPE":
                    type = typeof(R_WO_TYPE);
                    break;


                case "R_WO_XREF_TYPE":
                    type = typeof(R_WO_XREF_TYPE);
                    break;


                case "R_WORK_BID_TYPE":
                    type = typeof(R_WORK_BID_TYPE);
                    break;


                case "RA_ACCESS_CONDITION":
                    type = typeof(RA_ACCESS_CONDITION);
                    break;


                case "RA_ACCOUNT_PROC_TYPE":
                    type = typeof(RA_ACCOUNT_PROC_TYPE);
                    break;


                case "RA_ACTIVITY_SET_TYPE":
                    type = typeof(RA_ACTIVITY_SET_TYPE);
                    break;


                case "RA_ACTIVITY_TYPE":
                    type = typeof(RA_ACTIVITY_TYPE);
                    break;


                case "RA_ADDITIVE_METHOD":
                    type = typeof(RA_ADDITIVE_METHOD);
                    break;


                case "RA_ADDITIVE_TYPE":
                    type = typeof(RA_ADDITIVE_TYPE);
                    break;


                case "RA_ADDRESS_TYPE":
                    type = typeof(RA_ADDRESS_TYPE);
                    break;


                case "RA_AIR_GAS_CODE":
                    type = typeof(RA_AIR_GAS_CODE);
                    break;


                case "RA_AIRCRAFT_TYPE":
                    type = typeof(RA_AIRCRAFT_TYPE);
                    break;


                case "RA_ALIAS_REASON_TYPE":
                    type = typeof(RA_ALIAS_REASON_TYPE);
                    break;


                case "RA_ALIAS_TYPE":
                    type = typeof(RA_ALIAS_TYPE);
                    break;


                case "RA_ALLOCATION_TYPE":
                    type = typeof(RA_ALLOCATION_TYPE);
                    break;


                case "RA_ALLOWABLE_EXPENSE":
                    type = typeof(RA_ALLOWABLE_EXPENSE);
                    break;


                case "RA_ANALYSIS_PROPERTY":
                    type = typeof(RA_ANALYSIS_PROPERTY);
                    break;


                case "RA_ANL_ACCURACY_TYPE":
                    type = typeof(RA_ANL_ACCURACY_TYPE);
                    break;


                case "RA_ANL_BA_ROLE_TYPE":
                    type = typeof(RA_ANL_BA_ROLE_TYPE);
                    break;


                case "RA_ANL_CALC_EQUIV_TYPE":
                    type = typeof(RA_ANL_CALC_EQUIV_TYPE);
                    break;


                case "RA_ANL_CHRO_PROPERTY":
                    type = typeof(RA_ANL_CHRO_PROPERTY);
                    break;


                case "RA_ANL_COMP_TYPE":
                    type = typeof(RA_ANL_COMP_TYPE);
                    break;


                case "RA_ANL_CONFIDENCE_TYPE":
                    type = typeof(RA_ANL_CONFIDENCE_TYPE);
                    break;


                case "RA_ANL_DETAIL_REF_VALUE":
                    type = typeof(RA_ANL_DETAIL_REF_VALUE);
                    break;


                case "RA_ANL_DETAIL_TYPE":
                    type = typeof(RA_ANL_DETAIL_TYPE);
                    break;


                case "RA_ANL_ELEMENT_VALUE_CODE":
                    type = typeof(RA_ANL_ELEMENT_VALUE_CODE);
                    break;


                case "RA_ANL_ELEMENT_VALUE_TYPE":
                    type = typeof(RA_ANL_ELEMENT_VALUE_TYPE);
                    break;


                case "RA_ANL_EQUIP_ROLE":
                    type = typeof(RA_ANL_EQUIP_ROLE);
                    break;


                case "RA_ANL_FORMULA_TYPE":
                    type = typeof(RA_ANL_FORMULA_TYPE);
                    break;


                case "RA_ANL_GAS_CHRO_VALUE":
                    type = typeof(RA_ANL_GAS_CHRO_VALUE);
                    break;


                case "RA_ANL_GAS_PROPERTY":
                    type = typeof(RA_ANL_GAS_PROPERTY);
                    break;


                case "RA_ANL_GAS_PROPERTY_CODE":
                    type = typeof(RA_ANL_GAS_PROPERTY_CODE);
                    break;


                case "RA_ANL_METHOD_EQUIV_TYPE":
                    type = typeof(RA_ANL_METHOD_EQUIV_TYPE);
                    break;


                case "RA_ANL_METHOD_SET_TYPE":
                    type = typeof(RA_ANL_METHOD_SET_TYPE);
                    break;


                case "RA_ANL_MISSING_REP":
                    type = typeof(RA_ANL_MISSING_REP);
                    break;


                case "RA_ANL_NULL_REP":
                    type = typeof(RA_ANL_NULL_REP);
                    break;


                case "RA_ANL_OIL_PROPERTY_CODE":
                    type = typeof(RA_ANL_OIL_PROPERTY_CODE);
                    break;


                case "RA_ANL_PARAMETER_TYPE":
                    type = typeof(RA_ANL_PARAMETER_TYPE);
                    break;


                case "RA_ANL_PROBLEM_RESOLUTION":
                    type = typeof(RA_ANL_PROBLEM_RESOLUTION);
                    break;


                case "RA_ANL_PROBLEM_RESULT":
                    type = typeof(RA_ANL_PROBLEM_RESULT);
                    break;


                case "RA_ANL_PROBLEM_SEVERITY":
                    type = typeof(RA_ANL_PROBLEM_SEVERITY);
                    break;


                case "RA_ANL_PROBLEM_TYPE":
                    type = typeof(RA_ANL_PROBLEM_TYPE);
                    break;


                case "RA_ANL_REF_VALUE":
                    type = typeof(RA_ANL_REF_VALUE);
                    break;


                case "RA_ANL_REMARK_TYPE":
                    type = typeof(RA_ANL_REMARK_TYPE);
                    break;


                case "RA_ANL_REPEATABILITY":
                    type = typeof(RA_ANL_REPEATABILITY);
                    break;


                case "RA_ANL_STEP_XREF":
                    type = typeof(RA_ANL_STEP_XREF);
                    break;


                case "RA_ANL_TOLERANCE_TYPE":
                    type = typeof(RA_ANL_TOLERANCE_TYPE);
                    break;


                case "RA_ANL_VALID_MEAS_VALUE":
                    type = typeof(RA_ANL_VALID_MEAS_VALUE);
                    break;


                case "RA_ANL_VALID_MEASUREMENT":
                    type = typeof(RA_ANL_VALID_MEASUREMENT);
                    break;


                case "RA_ANL_VALID_PROBLEM":
                    type = typeof(RA_ANL_VALID_PROBLEM);
                    break;


                case "RA_ANL_WATER_PROPERTY":
                    type = typeof(RA_ANL_WATER_PROPERTY);
                    break;


                case "RA_AOF_ANALYSIS_TYPE":
                    type = typeof(RA_AOF_ANALYSIS_TYPE);
                    break;


                case "RA_AOF_CALC_METHOD":
                    type = typeof(RA_AOF_CALC_METHOD);
                    break;


                case "RA_API_LOG_SYSTEM":
                    type = typeof(RA_API_LOG_SYSTEM);
                    break;


                case "RA_APPLIC_ATTACHMENT":
                    type = typeof(RA_APPLIC_ATTACHMENT);
                    break;


                case "RA_APPLIC_BA_ROLE":
                    type = typeof(RA_APPLIC_BA_ROLE);
                    break;


                case "RA_APPLIC_DECISION":
                    type = typeof(RA_APPLIC_DECISION);
                    break;


                case "RA_APPLIC_DESC":
                    type = typeof(RA_APPLIC_DESC);
                    break;


                case "RA_APPLIC_REMARK_TYPE":
                    type = typeof(RA_APPLIC_REMARK_TYPE);
                    break;


                case "RA_APPLIC_STATUS":
                    type = typeof(RA_APPLIC_STATUS);
                    break;


                case "RA_APPLIC_TYPE":
                    type = typeof(RA_APPLIC_TYPE);
                    break;


                case "RA_APPLICATION_COMP_TYPE":
                    type = typeof(RA_APPLICATION_COMP_TYPE);
                    break;


                case "RA_AREA_COMPONENT_TYPE":
                    type = typeof(RA_AREA_COMPONENT_TYPE);
                    break;


                case "RA_AREA_CONTAIN_TYPE":
                    type = typeof(RA_AREA_CONTAIN_TYPE);
                    break;


                case "RA_AREA_DESC_CODE":
                    type = typeof(RA_AREA_DESC_CODE);
                    break;


                case "RA_AREA_DESC_TYPE":
                    type = typeof(RA_AREA_DESC_TYPE);
                    break;


                case "RA_AREA_TYPE":
                    type = typeof(RA_AREA_TYPE);
                    break;


                case "RA_AREA_XREF_TYPE":
                    type = typeof(RA_AREA_XREF_TYPE);
                    break;


                case "RA_AUTHOR_TYPE":
                    type = typeof(RA_AUTHOR_TYPE);
                    break;


                case "RA_AUTHORITY_TYPE":
                    type = typeof(RA_AUTHORITY_TYPE);
                    break;


                case "RA_BA_AUTHORITY_COMP_TYPE":
                    type = typeof(RA_BA_AUTHORITY_COMP_TYPE);
                    break;


                case "RA_BA_CATEGORY":
                    type = typeof(RA_BA_CATEGORY);
                    break;


                case "RA_BA_COMPONENT_TYPE":
                    type = typeof(RA_BA_COMPONENT_TYPE);
                    break;


                case "RA_BA_CONTACT_LOC_TYPE":
                    type = typeof(RA_BA_CONTACT_LOC_TYPE);
                    break;


                case "RA_BA_CREW_OVERHEAD_TYPE":
                    type = typeof(RA_BA_CREW_OVERHEAD_TYPE);
                    break;


                case "RA_BA_CREW_TYPE":
                    type = typeof(RA_BA_CREW_TYPE);
                    break;


                case "RA_BA_DESC_CODE":
                    type = typeof(RA_BA_DESC_CODE);
                    break;


                case "RA_BA_DESC_REF_VALUE":
                    type = typeof(RA_BA_DESC_REF_VALUE);
                    break;


                case "RA_BA_DESC_TYPE":
                    type = typeof(RA_BA_DESC_TYPE);
                    break;


                case "RA_BA_LIC_DUE_CONDITION":
                    type = typeof(RA_BA_LIC_DUE_CONDITION);
                    break;


                case "RA_BA_LIC_VIOL_RESOL":
                    type = typeof(RA_BA_LIC_VIOL_RESOL);
                    break;


                case "RA_BA_LIC_VIOLATION_TYPE":
                    type = typeof(RA_BA_LIC_VIOLATION_TYPE);
                    break;


                case "RA_BA_ORGANIZATION_COMP_TYPE":
                    type = typeof(RA_BA_ORGANIZATION_COMP_TYPE);
                    break;


                case "RA_BA_ORGANIZATION_TYPE":
                    type = typeof(RA_BA_ORGANIZATION_TYPE);
                    break;


                case "RA_BA_PERMIT_TYPE":
                    type = typeof(RA_BA_PERMIT_TYPE);
                    break;


                case "RA_BA_PREF_TYPE":
                    type = typeof(RA_BA_PREF_TYPE);
                    break;


                case "RA_BA_SERVICE_TYPE":
                    type = typeof(RA_BA_SERVICE_TYPE);
                    break;


                case "RA_BA_STATUS":
                    type = typeof(RA_BA_STATUS);
                    break;


                case "RA_BA_TYPE":
                    type = typeof(RA_BA_TYPE);
                    break;


                case "RA_BA_XREF_TYPE":
                    type = typeof(RA_BA_XREF_TYPE);
                    break;


                case "RA_BH_PRESS_TEST_TYPE":
                    type = typeof(RA_BH_PRESS_TEST_TYPE);
                    break;


                case "RA_BHP_METHOD":
                    type = typeof(RA_BHP_METHOD);
                    break;


                case "RA_BIT_BEARING_CONDITION":
                    type = typeof(RA_BIT_BEARING_CONDITION);
                    break;


                case "RA_BIT_CUT_STRUCT_DULL":
                    type = typeof(RA_BIT_CUT_STRUCT_DULL);
                    break;


                case "RA_BIT_CUT_STRUCT_INNER":
                    type = typeof(RA_BIT_CUT_STRUCT_INNER);
                    break;


                case "RA_BIT_CUT_STRUCT_LOC":
                    type = typeof(RA_BIT_CUT_STRUCT_LOC);
                    break;


                case "RA_BIT_CUT_STRUCT_OUTER":
                    type = typeof(RA_BIT_CUT_STRUCT_OUTER);
                    break;


                case "RA_BIT_REASON_PULLED":
                    type = typeof(RA_BIT_REASON_PULLED);
                    break;


                case "RA_BLOWOUT_FLUID":
                    type = typeof(RA_BLOWOUT_FLUID);
                    break;


                case "RA_BUILDUP_RADIUS_TYPE":
                    type = typeof(RA_BUILDUP_RADIUS_TYPE);
                    break;


                case "RA_CAT_ADDITIVE_GROUP":
                    type = typeof(RA_CAT_ADDITIVE_GROUP);
                    break;


                case "RA_CAT_ADDITIVE_QUANTITY":
                    type = typeof(RA_CAT_ADDITIVE_QUANTITY);
                    break;


                case "RA_CAT_ADDITIVE_SPEC":
                    type = typeof(RA_CAT_ADDITIVE_SPEC);
                    break;


                case "RA_CAT_ADDITIVE_XREF":
                    type = typeof(RA_CAT_ADDITIVE_XREF);
                    break;


                case "RA_CAT_EQUIP_GROUP":
                    type = typeof(RA_CAT_EQUIP_GROUP);
                    break;


                case "RA_CAT_EQUIP_SPEC":
                    type = typeof(RA_CAT_EQUIP_SPEC);
                    break;


                case "RA_CAT_EQUIP_SPEC_CODE":
                    type = typeof(RA_CAT_EQUIP_SPEC_CODE);
                    break;


                case "RA_CAT_EQUIP_TYPE":
                    type = typeof(RA_CAT_EQUIP_TYPE);
                    break;


                case "RA_CEMENT_TYPE":
                    type = typeof(RA_CEMENT_TYPE);
                    break;


                case "RA_CHECKSHOT_SRVY_TYPE":
                    type = typeof(RA_CHECKSHOT_SRVY_TYPE);
                    break;


                case "RA_CLASS_DESC_PROPERTY":
                    type = typeof(RA_CLASS_DESC_PROPERTY);
                    break;


                case "RA_CLASS_LEV_COMP_TYPE":
                    type = typeof(RA_CLASS_LEV_COMP_TYPE);
                    break;


                case "RA_CLASS_LEV_XREF_TYPE":
                    type = typeof(RA_CLASS_LEV_XREF_TYPE);
                    break;


                case "RA_CLASS_SYST_XREF_TYPE":
                    type = typeof(RA_CLASS_SYST_XREF_TYPE);
                    break;


                case "RA_CLASS_SYSTEM_DIMENSION":
                    type = typeof(RA_CLASS_SYSTEM_DIMENSION);
                    break;


                case "RA_CLIMATE":
                    type = typeof(RA_CLIMATE);
                    break;


                case "RA_COAL_RANK_SCHEME_TYPE":
                    type = typeof(RA_COAL_RANK_SCHEME_TYPE);
                    break;


                case "RA_CODE_VERSION_XREF_TYPE":
                    type = typeof(RA_CODE_VERSION_XREF_TYPE);
                    break;


                case "RA_COLLAR_TYPE":
                    type = typeof(RA_COLLAR_TYPE);
                    break;


                case "RA_COLOR":
                    type = typeof(RA_COLOR);
                    break;


                case "RA_COLOR_EQUIV":
                    type = typeof(RA_COLOR_EQUIV);
                    break;


                case "RA_COLOR_FORMAT":
                    type = typeof(RA_COLOR_FORMAT);
                    break;


                case "RA_COLOR_PALETTE":
                    type = typeof(RA_COLOR_PALETTE);
                    break;


                case "RA_COMPLETION_METHOD":
                    type = typeof(RA_COMPLETION_METHOD);
                    break;


                case "RA_COMPLETION_STATUS":
                    type = typeof(RA_COMPLETION_STATUS);
                    break;


                case "RA_COMPLETION_STATUS_TYPE":
                    type = typeof(RA_COMPLETION_STATUS_TYPE);
                    break;


                case "RA_COMPLETION_TYPE":
                    type = typeof(RA_COMPLETION_TYPE);
                    break;


                case "RA_CONDITION_TYPE":
                    type = typeof(RA_CONDITION_TYPE);
                    break;


                case "RA_CONFIDENCE_TYPE":
                    type = typeof(RA_CONFIDENCE_TYPE);
                    break;


                case "RA_CONFIDENTIAL_REASON":
                    type = typeof(RA_CONFIDENTIAL_REASON);
                    break;


                case "RA_CONFIDENTIAL_TYPE":
                    type = typeof(RA_CONFIDENTIAL_TYPE);
                    break;


                case "RA_CONFORMITY_RELATION":
                    type = typeof(RA_CONFORMITY_RELATION);
                    break;


                case "RA_CONSENT_BA_ROLE":
                    type = typeof(RA_CONSENT_BA_ROLE);
                    break;


                case "RA_CONSENT_COMP_TYPE":
                    type = typeof(RA_CONSENT_COMP_TYPE);
                    break;


                case "RA_CONSENT_CONDITION":
                    type = typeof(RA_CONSENT_CONDITION);
                    break;


                case "RA_CONSENT_REMARK":
                    type = typeof(RA_CONSENT_REMARK);
                    break;


                case "RA_CONSENT_STATUS":
                    type = typeof(RA_CONSENT_STATUS);
                    break;


                case "RA_CONSENT_TYPE":
                    type = typeof(RA_CONSENT_TYPE);
                    break;


                case "RA_CONSULT_ATTEND_TYPE":
                    type = typeof(RA_CONSULT_ATTEND_TYPE);
                    break;


                case "RA_CONSULT_COMP_TYPE":
                    type = typeof(RA_CONSULT_COMP_TYPE);
                    break;


                case "RA_CONSULT_DISC_TYPE":
                    type = typeof(RA_CONSULT_DISC_TYPE);
                    break;


                case "RA_CONSULT_ISSUE_TYPE":
                    type = typeof(RA_CONSULT_ISSUE_TYPE);
                    break;


                case "RA_CONSULT_REASON":
                    type = typeof(RA_CONSULT_REASON);
                    break;


                case "RA_CONSULT_RESOLUTION":
                    type = typeof(RA_CONSULT_RESOLUTION);
                    break;


                case "RA_CONSULT_ROLE":
                    type = typeof(RA_CONSULT_ROLE);
                    break;


                case "RA_CONSULT_TYPE":
                    type = typeof(RA_CONSULT_TYPE);
                    break;


                case "RA_CONSULT_XREF_TYPE":
                    type = typeof(RA_CONSULT_XREF_TYPE);
                    break;


                case "RA_CONT_BA_ROLE":
                    type = typeof(RA_CONT_BA_ROLE);
                    break;


                case "RA_CONT_COMP_REASON":
                    type = typeof(RA_CONT_COMP_REASON);
                    break;


                case "RA_CONT_EXTEND_COND":
                    type = typeof(RA_CONT_EXTEND_COND);
                    break;


                case "RA_CONT_EXTEND_TYPE":
                    type = typeof(RA_CONT_EXTEND_TYPE);
                    break;


                case "RA_CONT_INSUR_ELECT":
                    type = typeof(RA_CONT_INSUR_ELECT);
                    break;


                case "RA_CONT_OPERATING_PROC":
                    type = typeof(RA_CONT_OPERATING_PROC);
                    break;


                case "RA_CONT_PROV_XREF_TYPE":
                    type = typeof(RA_CONT_PROV_XREF_TYPE);
                    break;


                case "RA_CONT_PROVISION_TYPE":
                    type = typeof(RA_CONT_PROVISION_TYPE);
                    break;


                case "RA_CONT_STATUS":
                    type = typeof(RA_CONT_STATUS);
                    break;


                case "RA_CONT_STATUS_TYPE":
                    type = typeof(RA_CONT_STATUS_TYPE);
                    break;


                case "RA_CONT_TYPE":
                    type = typeof(RA_CONT_TYPE);
                    break;


                case "RA_CONT_VOTE_RESPONSE":
                    type = typeof(RA_CONT_VOTE_RESPONSE);
                    break;


                case "RA_CONT_VOTE_TYPE":
                    type = typeof(RA_CONT_VOTE_TYPE);
                    break;


                case "RA_CONT_XREF_TYPE":
                    type = typeof(RA_CONT_XREF_TYPE);
                    break;


                case "RA_CONTACT_ROLE":
                    type = typeof(RA_CONTACT_ROLE);
                    break;


                case "RA_CONTAMINANT_TYPE":
                    type = typeof(RA_CONTAMINANT_TYPE);
                    break;


                case "RA_CONTEST_COMP_TYPE":
                    type = typeof(RA_CONTEST_COMP_TYPE);
                    break;


                case "RA_CONTEST_PARTY_ROLE":
                    type = typeof(RA_CONTEST_PARTY_ROLE);
                    break;


                case "RA_CONTEST_RESOLUTION":
                    type = typeof(RA_CONTEST_RESOLUTION);
                    break;


                case "RA_CONTEST_TYPE":
                    type = typeof(RA_CONTEST_TYPE);
                    break;


                case "RA_CONTRACT_COMP_TYPE":
                    type = typeof(RA_CONTRACT_COMP_TYPE);
                    break;


                case "RA_COORD_CAPTURE":
                    type = typeof(RA_COORD_CAPTURE);
                    break;


                case "RA_COORD_COMPUTE":
                    type = typeof(RA_COORD_COMPUTE);
                    break;


                case "RA_COORD_QUALITY":
                    type = typeof(RA_COORD_QUALITY);
                    break;


                case "RA_COORD_SYSTEM_TYPE":
                    type = typeof(RA_COORD_SYSTEM_TYPE);
                    break;


                case "RA_CORE_HANDLING":
                    type = typeof(RA_CORE_HANDLING);
                    break;


                case "RA_CORE_RECOVERY_TYPE":
                    type = typeof(RA_CORE_RECOVERY_TYPE);
                    break;


                case "RA_CORE_SAMPLE_TYPE":
                    type = typeof(RA_CORE_SAMPLE_TYPE);
                    break;


                case "RA_CORE_SHIFT_METHOD":
                    type = typeof(RA_CORE_SHIFT_METHOD);
                    break;


                case "RA_CORE_SOLVENT":
                    type = typeof(RA_CORE_SOLVENT);
                    break;


                case "RA_CORE_TYPE":
                    type = typeof(RA_CORE_TYPE);
                    break;


                case "RA_CORRECTION_METHOD":
                    type = typeof(RA_CORRECTION_METHOD);
                    break;


                case "RA_COUPLING_TYPE":
                    type = typeof(RA_COUPLING_TYPE);
                    break;


                case "RA_CREATOR_TYPE":
                    type = typeof(RA_CREATOR_TYPE);
                    break;


                case "RA_CS_TRANSFORM_PARM":
                    type = typeof(RA_CS_TRANSFORM_PARM);
                    break;


                case "RA_CS_TRANSFORM_TYPE":
                    type = typeof(RA_CS_TRANSFORM_TYPE);
                    break;


                case "RA_CURVE_SCALE":
                    type = typeof(RA_CURVE_SCALE);
                    break;


                case "RA_CURVE_TYPE":
                    type = typeof(RA_CURVE_TYPE);
                    break;


                case "RA_CURVE_XREF_TYPE":
                    type = typeof(RA_CURVE_XREF_TYPE);
                    break;


                case "RA_CUSHION_TYPE":
                    type = typeof(RA_CUSHION_TYPE);
                    break;


                case "RA_CUTTING_FLUID":
                    type = typeof(RA_CUTTING_FLUID);
                    break;


                case "RA_DATA_CIRC_PROCESS":
                    type = typeof(RA_DATA_CIRC_PROCESS);
                    break;


                case "RA_DATA_CIRC_STATUS":
                    type = typeof(RA_DATA_CIRC_STATUS);
                    break;


                case "RA_DATA_STORE_TYPE":
                    type = typeof(RA_DATA_STORE_TYPE);
                    break;


                case "RA_DATE_FORMAT_TYPE":
                    type = typeof(RA_DATE_FORMAT_TYPE);
                    break;


                case "RA_DATUM_ORIGIN":
                    type = typeof(RA_DATUM_ORIGIN);
                    break;


                case "RA_DECLINE_COND_CODE":
                    type = typeof(RA_DECLINE_COND_CODE);
                    break;


                case "RA_DECLINE_COND_TYPE":
                    type = typeof(RA_DECLINE_COND_TYPE);
                    break;


                case "RA_DECLINE_CURVE_TYPE":
                    type = typeof(RA_DECLINE_CURVE_TYPE);
                    break;


                case "RA_DECLINE_TYPE":
                    type = typeof(RA_DECLINE_TYPE);
                    break;


                case "RA_DECRYPT_TYPE":
                    type = typeof(RA_DECRYPT_TYPE);
                    break;


                case "RA_DEDUCT_TYPE":
                    type = typeof(RA_DEDUCT_TYPE);
                    break;


                case "RA_DIGITAL_FORMAT":
                    type = typeof(RA_DIGITAL_FORMAT);
                    break;


                case "RA_DIGITAL_OUTPUT":
                    type = typeof(RA_DIGITAL_OUTPUT);
                    break;


                case "RA_DIR_SRVY_ACC_REASON":
                    type = typeof(RA_DIR_SRVY_ACC_REASON);
                    break;


                case "RA_DIR_SRVY_CLASS":
                    type = typeof(RA_DIR_SRVY_CLASS);
                    break;


                case "RA_DIR_SRVY_COMPUTE":
                    type = typeof(RA_DIR_SRVY_COMPUTE);
                    break;


                case "RA_DIR_SRVY_CORR_ANGLE_TYPE":
                    type = typeof(RA_DIR_SRVY_CORR_ANGLE_TYPE);
                    break;


                case "RA_DIR_SRVY_POINT_TYPE":
                    type = typeof(RA_DIR_SRVY_POINT_TYPE);
                    break;


                case "RA_DIR_SRVY_PROCESS_METH":
                    type = typeof(RA_DIR_SRVY_PROCESS_METH);
                    break;


                case "RA_DIR_SRVY_RAD_UNCERT":
                    type = typeof(RA_DIR_SRVY_RAD_UNCERT);
                    break;


                case "RA_DIR_SRVY_RECORD":
                    type = typeof(RA_DIR_SRVY_RECORD);
                    break;


                case "RA_DIR_SRVY_REPORT_TYPE":
                    type = typeof(RA_DIR_SRVY_REPORT_TYPE);
                    break;


                case "RA_DIR_SRVY_TYPE":
                    type = typeof(RA_DIR_SRVY_TYPE);
                    break;


                case "RA_DIRECTION":
                    type = typeof(RA_DIRECTION);
                    break;


                case "RA_DIST_REF_PT":
                    type = typeof(RA_DIST_REF_PT);
                    break;


                case "RA_DOC_STATUS":
                    type = typeof(RA_DOC_STATUS);
                    break;


                case "RA_DOCUMENT_TYPE":
                    type = typeof(RA_DOCUMENT_TYPE);
                    break;


                case "RA_DRILL_ASSEMBLY_COMP":
                    type = typeof(RA_DRILL_ASSEMBLY_COMP);
                    break;


                case "RA_DRILL_BIT_CONDITION":
                    type = typeof(RA_DRILL_BIT_CONDITION);
                    break;


                case "RA_DRILL_BIT_DETAIL_CODE":
                    type = typeof(RA_DRILL_BIT_DETAIL_CODE);
                    break;


                case "RA_DRILL_BIT_DETAIL_TYPE":
                    type = typeof(RA_DRILL_BIT_DETAIL_TYPE);
                    break;


                case "RA_DRILL_BIT_JET_TYPE":
                    type = typeof(RA_DRILL_BIT_JET_TYPE);
                    break;


                case "RA_DRILL_BIT_TYPE":
                    type = typeof(RA_DRILL_BIT_TYPE);
                    break;


                case "RA_DRILL_HOLE_POSITION":
                    type = typeof(RA_DRILL_HOLE_POSITION);
                    break;


                case "RA_DRILL_REPORT_TIME":
                    type = typeof(RA_DRILL_REPORT_TIME);
                    break;


                case "RA_DRILL_STAT_CODE":
                    type = typeof(RA_DRILL_STAT_CODE);
                    break;


                case "RA_DRILL_STAT_TYPE":
                    type = typeof(RA_DRILL_STAT_TYPE);
                    break;


                case "RA_DRILL_TOOL_TYPE":
                    type = typeof(RA_DRILL_TOOL_TYPE);
                    break;


                case "RA_DRILLING_MEDIA":
                    type = typeof(RA_DRILLING_MEDIA);
                    break;


                case "RA_ECONOMIC_SCENARIO":
                    type = typeof(RA_ECONOMIC_SCENARIO);
                    break;


                case "RA_ECONOMIC_SCHEDULE":
                    type = typeof(RA_ECONOMIC_SCHEDULE);
                    break;


                case "RA_ECOZONE_HIER_LEVEL":
                    type = typeof(RA_ECOZONE_HIER_LEVEL);
                    break;


                case "RA_ECOZONE_TYPE":
                    type = typeof(RA_ECOZONE_TYPE);
                    break;


                case "RA_ECOZONE_XREF":
                    type = typeof(RA_ECOZONE_XREF);
                    break;


                case "RA_EMPLOYEE_POSITION":
                    type = typeof(RA_EMPLOYEE_POSITION);
                    break;


                case "RA_EMPLOYEE_STATUS":
                    type = typeof(RA_EMPLOYEE_STATUS);
                    break;


                case "RA_ENCODING_TYPE":
                    type = typeof(RA_ENCODING_TYPE);
                    break;


                case "RA_ENHANCED_REC_TYPE":
                    type = typeof(RA_ENHANCED_REC_TYPE);
                    break;


                case "RA_ENT_ACCESS_TYPE":
                    type = typeof(RA_ENT_ACCESS_TYPE);
                    break;


                case "RA_ENT_COMPONENT_TYPE":
                    type = typeof(RA_ENT_COMPONENT_TYPE);
                    break;


                case "RA_ENT_EXPIRY_ACTION":
                    type = typeof(RA_ENT_EXPIRY_ACTION);
                    break;


                case "RA_ENT_SEC_GROUP_TYPE":
                    type = typeof(RA_ENT_SEC_GROUP_TYPE);
                    break;


                case "RA_ENT_SEC_GROUP_XREF":
                    type = typeof(RA_ENT_SEC_GROUP_XREF);
                    break;


                case "RA_ENT_TYPE":
                    type = typeof(RA_ENT_TYPE);
                    break;


                case "RA_ENVIRONMENT":
                    type = typeof(RA_ENVIRONMENT);
                    break;


                case "RA_EQUIP_BA_ROLE_TYPE":
                    type = typeof(RA_EQUIP_BA_ROLE_TYPE);
                    break;


                case "RA_EQUIP_COMPONENT_TYPE":
                    type = typeof(RA_EQUIP_COMPONENT_TYPE);
                    break;


                case "RA_EQUIP_INSTALL_LOC":
                    type = typeof(RA_EQUIP_INSTALL_LOC);
                    break;


                case "RA_EQUIP_MAINT_LOC":
                    type = typeof(RA_EQUIP_MAINT_LOC);
                    break;


                case "RA_EQUIP_MAINT_REASON":
                    type = typeof(RA_EQUIP_MAINT_REASON);
                    break;


                case "RA_EQUIP_MAINT_STAT_TYPE":
                    type = typeof(RA_EQUIP_MAINT_STAT_TYPE);
                    break;


                case "RA_EQUIP_MAINT_STATUS":
                    type = typeof(RA_EQUIP_MAINT_STATUS);
                    break;


                case "RA_EQUIP_REMOVE_REASON":
                    type = typeof(RA_EQUIP_REMOVE_REASON);
                    break;


                case "RA_EQUIP_SPEC":
                    type = typeof(RA_EQUIP_SPEC);
                    break;


                case "RA_EQUIP_SPEC_REF_TYPE":
                    type = typeof(RA_EQUIP_SPEC_REF_TYPE);
                    break;


                case "RA_EQUIP_SPEC_SET_TYPE":
                    type = typeof(RA_EQUIP_SPEC_SET_TYPE);
                    break;


                case "RA_EQUIP_STATUS":
                    type = typeof(RA_EQUIP_STATUS);
                    break;


                case "RA_EQUIP_STATUS_TYPE":
                    type = typeof(RA_EQUIP_STATUS_TYPE);
                    break;


                case "RA_EQUIP_SYSTEM_CONDITION":
                    type = typeof(RA_EQUIP_SYSTEM_CONDITION);
                    break;


                case "RA_EQUIP_USE_STAT_TYPE":
                    type = typeof(RA_EQUIP_USE_STAT_TYPE);
                    break;


                case "RA_EQUIP_XREF_TYPE":
                    type = typeof(RA_EQUIP_XREF_TYPE);
                    break;


                case "RA_EW_DIRECTION":
                    type = typeof(RA_EW_DIRECTION);
                    break;


                case "RA_EW_START_LINE":
                    type = typeof(RA_EW_START_LINE);
                    break;


                case "RA_FAC_FUNCTION":
                    type = typeof(RA_FAC_FUNCTION);
                    break;


                case "RA_FAC_LIC_COND":
                    type = typeof(RA_FAC_LIC_COND);
                    break;


                case "RA_FAC_LIC_COND_CODE":
                    type = typeof(RA_FAC_LIC_COND_CODE);
                    break;


                case "RA_FAC_LIC_DUE_CONDITION":
                    type = typeof(RA_FAC_LIC_DUE_CONDITION);
                    break;


                case "RA_FAC_LIC_EXTEND_TYPE":
                    type = typeof(RA_FAC_LIC_EXTEND_TYPE);
                    break;


                case "RA_FAC_LIC_VIOL_RESOL":
                    type = typeof(RA_FAC_LIC_VIOL_RESOL);
                    break;


                case "RA_FAC_LIC_VIOLATION_TYPE":
                    type = typeof(RA_FAC_LIC_VIOLATION_TYPE);
                    break;


                case "RA_FAC_MAINT_STATUS":
                    type = typeof(RA_FAC_MAINT_STATUS);
                    break;


                case "RA_FAC_MAINT_STATUS_TYPE":
                    type = typeof(RA_FAC_MAINT_STATUS_TYPE);
                    break;


                case "RA_FAC_MAINTAIN_TYPE":
                    type = typeof(RA_FAC_MAINTAIN_TYPE);
                    break;


                case "RA_FAC_PIPE_COVER":
                    type = typeof(RA_FAC_PIPE_COVER);
                    break;


                case "RA_FAC_PIPE_MATERIAL":
                    type = typeof(RA_FAC_PIPE_MATERIAL);
                    break;


                case "RA_FAC_PIPE_TYPE":
                    type = typeof(RA_FAC_PIPE_TYPE);
                    break;


                case "RA_FAC_SPEC_REFERENCE":
                    type = typeof(RA_FAC_SPEC_REFERENCE);
                    break;


                case "RA_FAC_STATUS_TYPE":
                    type = typeof(RA_FAC_STATUS_TYPE);
                    break;


                case "RA_FACILITY_CLASS":
                    type = typeof(RA_FACILITY_CLASS);
                    break;


                case "RA_FACILITY_COMP_TYPE":
                    type = typeof(RA_FACILITY_COMP_TYPE);
                    break;


                case "RA_FACILITY_SPEC_CODE":
                    type = typeof(RA_FACILITY_SPEC_CODE);
                    break;


                case "RA_FACILITY_SPEC_TYPE":
                    type = typeof(RA_FACILITY_SPEC_TYPE);
                    break;


                case "RA_FACILITY_STATUS":
                    type = typeof(RA_FACILITY_STATUS);
                    break;


                case "RA_FACILITY_TYPE":
                    type = typeof(RA_FACILITY_TYPE);
                    break;


                case "RA_FACILITY_XREF_TYPE":
                    type = typeof(RA_FACILITY_XREF_TYPE);
                    break;


                case "RA_FAULT_TYPE":
                    type = typeof(RA_FAULT_TYPE);
                    break;


                case "RA_FIELD_COMPONENT_TYPE":
                    type = typeof(RA_FIELD_COMPONENT_TYPE);
                    break;


                case "RA_FIELD_STATION_TYPE":
                    type = typeof(RA_FIELD_STATION_TYPE);
                    break;


                case "RA_FIELD_TYPE":
                    type = typeof(RA_FIELD_TYPE);
                    break;


                case "RA_FIN_COMPONENT_TYPE":
                    type = typeof(RA_FIN_COMPONENT_TYPE);
                    break;


                case "RA_FIN_COST_TYPE":
                    type = typeof(RA_FIN_COST_TYPE);
                    break;


                case "RA_FIN_STATUS":
                    type = typeof(RA_FIN_STATUS);
                    break;


                case "RA_FIN_TYPE":
                    type = typeof(RA_FIN_TYPE);
                    break;


                case "RA_FIN_XREF_TYPE":
                    type = typeof(RA_FIN_XREF_TYPE);
                    break;


                case "RA_FLUID_TYPE":
                    type = typeof(RA_FLUID_TYPE);
                    break;


                case "RA_FONT":
                    type = typeof(RA_FONT);
                    break;


                case "RA_FONT_EFFECT":
                    type = typeof(RA_FONT_EFFECT);
                    break;


                case "RA_FOOTAGE_ORIGIN":
                    type = typeof(RA_FOOTAGE_ORIGIN);
                    break;


                case "RA_FOS_ALIAS_TYPE":
                    type = typeof(RA_FOS_ALIAS_TYPE);
                    break;


                case "RA_FOS_ASSEMBLAGE_TYPE":
                    type = typeof(RA_FOS_ASSEMBLAGE_TYPE);
                    break;


                case "RA_FOS_DESC_CODE":
                    type = typeof(RA_FOS_DESC_CODE);
                    break;


                case "RA_FOS_DESC_TYPE":
                    type = typeof(RA_FOS_DESC_TYPE);
                    break;


                case "RA_FOS_LIFE_HABIT":
                    type = typeof(RA_FOS_LIFE_HABIT);
                    break;


                case "RA_FOS_NAME_SET_TYPE":
                    type = typeof(RA_FOS_NAME_SET_TYPE);
                    break;


                case "RA_FOS_OBS_TYPE":
                    type = typeof(RA_FOS_OBS_TYPE);
                    break;


                case "RA_FOS_TAXON_GROUP":
                    type = typeof(RA_FOS_TAXON_GROUP);
                    break;


                case "RA_FOS_TAXON_LEVEL":
                    type = typeof(RA_FOS_TAXON_LEVEL);
                    break;


                case "RA_FOS_XREF":
                    type = typeof(RA_FOS_XREF);
                    break;


                case "RA_GAS_ANL_VALUE_CODE":
                    type = typeof(RA_GAS_ANL_VALUE_CODE);
                    break;


                case "RA_GAS_ANL_VALUE_TYPE":
                    type = typeof(RA_GAS_ANL_VALUE_TYPE);
                    break;


                case "RA_GRANTED_RIGHT_TYPE":
                    type = typeof(RA_GRANTED_RIGHT_TYPE);
                    break;


                case "RA_HEAT_CONTENT_METHOD":
                    type = typeof(RA_HEAT_CONTENT_METHOD);
                    break;


                case "RA_HOLE_CONDITION":
                    type = typeof(RA_HOLE_CONDITION);
                    break;


                case "RA_HORIZ_DRILL_REASON":
                    type = typeof(RA_HORIZ_DRILL_REASON);
                    break;


                case "RA_HORIZ_DRILL_TYPE":
                    type = typeof(RA_HORIZ_DRILL_TYPE);
                    break;


                case "RA_HSE_COMP_ROLE":
                    type = typeof(RA_HSE_COMP_ROLE);
                    break;


                case "RA_HSE_INCIDENT_COMP_TYPE":
                    type = typeof(RA_HSE_INCIDENT_COMP_TYPE);
                    break;


                case "RA_HSE_INCIDENT_DETAIL":
                    type = typeof(RA_HSE_INCIDENT_DETAIL);
                    break;


                case "RA_HSE_RESPONSE_TYPE":
                    type = typeof(RA_HSE_RESPONSE_TYPE);
                    break;


                case "RA_IMAGE_CALIBRATE_METHOD":
                    type = typeof(RA_IMAGE_CALIBRATE_METHOD);
                    break;


                case "RA_IMAGE_SECTION_TYPE":
                    type = typeof(RA_IMAGE_SECTION_TYPE);
                    break;


                case "RA_INCIDENT_BA_ROLE":
                    type = typeof(RA_INCIDENT_BA_ROLE);
                    break;


                case "RA_INCIDENT_CAUSE_CODE":
                    type = typeof(RA_INCIDENT_CAUSE_CODE);
                    break;


                case "RA_INCIDENT_CAUSE_TYPE":
                    type = typeof(RA_INCIDENT_CAUSE_TYPE);
                    break;


                case "RA_INCIDENT_INTERACT_TYPE":
                    type = typeof(RA_INCIDENT_INTERACT_TYPE);
                    break;


                case "RA_INCIDENT_RESP_RESULT":
                    type = typeof(RA_INCIDENT_RESP_RESULT);
                    break;


                case "RA_INCIDENT_SUBST_ROLE":
                    type = typeof(RA_INCIDENT_SUBST_ROLE);
                    break;


                case "RA_INCIDENT_SUBSTANCE":
                    type = typeof(RA_INCIDENT_SUBSTANCE);
                    break;


                case "RA_INFORMATION_PROCESS":
                    type = typeof(RA_INFORMATION_PROCESS);
                    break;


                case "RA_INPUT_TYPE":
                    type = typeof(RA_INPUT_TYPE);
                    break;


                case "RA_INSP_COMP_TYPE":
                    type = typeof(RA_INSP_COMP_TYPE);
                    break;


                case "RA_INSP_STATUS":
                    type = typeof(RA_INSP_STATUS);
                    break;


                case "RA_INST_DETAIL_CODE":
                    type = typeof(RA_INST_DETAIL_CODE);
                    break;


                case "RA_INST_DETAIL_REF_VALUE":
                    type = typeof(RA_INST_DETAIL_REF_VALUE);
                    break;


                case "RA_INST_DETAIL_TYPE":
                    type = typeof(RA_INST_DETAIL_TYPE);
                    break;


                case "RA_INSTRUMENT_COMP_TYPE":
                    type = typeof(RA_INSTRUMENT_COMP_TYPE);
                    break;


                case "RA_INSTRUMENT_TYPE":
                    type = typeof(RA_INSTRUMENT_TYPE);
                    break;


                case "RA_INT_SET_COMPONENT":
                    type = typeof(RA_INT_SET_COMPONENT);
                    break;


                case "RA_INT_SET_ROLE":
                    type = typeof(RA_INT_SET_ROLE);
                    break;


                case "RA_INT_SET_STATUS":
                    type = typeof(RA_INT_SET_STATUS);
                    break;


                case "RA_INT_SET_STATUS_TYPE":
                    type = typeof(RA_INT_SET_STATUS_TYPE);
                    break;


                case "RA_INT_SET_TRIGGER":
                    type = typeof(RA_INT_SET_TRIGGER);
                    break;


                case "RA_INT_SET_TYPE":
                    type = typeof(RA_INT_SET_TYPE);
                    break;


                case "RA_INT_SET_XREF_TYPE":
                    type = typeof(RA_INT_SET_XREF_TYPE);
                    break;


                case "RA_INTERP_ORIGIN_TYPE":
                    type = typeof(RA_INTERP_ORIGIN_TYPE);
                    break;


                case "RA_INTERP_TYPE":
                    type = typeof(RA_INTERP_TYPE);
                    break;


                case "RA_INV_MATERIAL_TYPE":
                    type = typeof(RA_INV_MATERIAL_TYPE);
                    break;


                case "RA_ITEM_CATEGORY":
                    type = typeof(RA_ITEM_CATEGORY);
                    break;


                case "RA_ITEM_SUB_CATEGORY":
                    type = typeof(RA_ITEM_SUB_CATEGORY);
                    break;


                case "RA_L_OFFR_CANCEL_RSN":
                    type = typeof(RA_L_OFFR_CANCEL_RSN);
                    break;


                case "RA_LAND_ACQTN_METHOD":
                    type = typeof(RA_LAND_ACQTN_METHOD);
                    break;


                case "RA_LAND_AGREE_TYPE":
                    type = typeof(RA_LAND_AGREE_TYPE);
                    break;


                case "RA_LAND_BID_STATUS":
                    type = typeof(RA_LAND_BID_STATUS);
                    break;


                case "RA_LAND_BID_TYPE":
                    type = typeof(RA_LAND_BID_TYPE);
                    break;


                case "RA_LAND_BIDDER_TYPE":
                    type = typeof(RA_LAND_BIDDER_TYPE);
                    break;


                case "RA_LAND_CASE_ACTION":
                    type = typeof(RA_LAND_CASE_ACTION);
                    break;


                case "RA_LAND_CASE_TYPE":
                    type = typeof(RA_LAND_CASE_TYPE);
                    break;


                case "RA_LAND_CASH_BID_TYPE":
                    type = typeof(RA_LAND_CASH_BID_TYPE);
                    break;


                case "RA_LAND_COMPONENT_TYPE":
                    type = typeof(RA_LAND_COMPONENT_TYPE);
                    break;


                case "RA_LAND_LESSOR_TYPE":
                    type = typeof(RA_LAND_LESSOR_TYPE);
                    break;


                case "RA_LAND_OFFRING_STATUS":
                    type = typeof(RA_LAND_OFFRING_STATUS);
                    break;


                case "RA_LAND_OFFRING_TYPE":
                    type = typeof(RA_LAND_OFFRING_TYPE);
                    break;


                case "RA_LAND_PROPERTY_TYPE":
                    type = typeof(RA_LAND_PROPERTY_TYPE);
                    break;


                case "RA_LAND_REF_NUM_TYPE":
                    type = typeof(RA_LAND_REF_NUM_TYPE);
                    break;


                case "RA_LAND_RENTAL_TYPE":
                    type = typeof(RA_LAND_RENTAL_TYPE);
                    break;


                case "RA_LAND_REQ_STATUS":
                    type = typeof(RA_LAND_REQ_STATUS);
                    break;


                case "RA_LAND_REQUEST_TYPE":
                    type = typeof(RA_LAND_REQUEST_TYPE);
                    break;


                case "RA_LAND_RIGHT_CATEGORY":
                    type = typeof(RA_LAND_RIGHT_CATEGORY);
                    break;


                case "RA_LAND_RIGHT_STATUS":
                    type = typeof(RA_LAND_RIGHT_STATUS);
                    break;


                case "RA_LAND_STATUS_TYPE":
                    type = typeof(RA_LAND_STATUS_TYPE);
                    break;


                case "RA_LAND_TITLE_CHG_RSN":
                    type = typeof(RA_LAND_TITLE_CHG_RSN);
                    break;


                case "RA_LAND_TITLE_TYPE":
                    type = typeof(RA_LAND_TITLE_TYPE);
                    break;


                case "RA_LAND_TRACT_TYPE":
                    type = typeof(RA_LAND_TRACT_TYPE);
                    break;


                case "RA_LAND_UNIT_TYPE":
                    type = typeof(RA_LAND_UNIT_TYPE);
                    break;


                case "RA_LAND_WELL_REL_TYPE":
                    type = typeof(RA_LAND_WELL_REL_TYPE);
                    break;


                case "RA_LANGUAGE":
                    type = typeof(RA_LANGUAGE);
                    break;


                case "RA_LEASE_UNIT_STATUS":
                    type = typeof(RA_LEASE_UNIT_STATUS);
                    break;


                case "RA_LEASE_UNIT_TYPE":
                    type = typeof(RA_LEASE_UNIT_TYPE);
                    break;


                case "RA_LEGAL_SURVEY_TYPE":
                    type = typeof(RA_LEGAL_SURVEY_TYPE);
                    break;


                case "RA_LIC_STATUS_TYPE":
                    type = typeof(RA_LIC_STATUS_TYPE);
                    break;


                case "RA_LICENSE_STATUS":
                    type = typeof(RA_LICENSE_STATUS);
                    break;


                case "RA_LINER_TYPE":
                    type = typeof(RA_LINER_TYPE);
                    break;


                case "RA_LITH_ABUNDANCE":
                    type = typeof(RA_LITH_ABUNDANCE);
                    break;


                case "RA_LITH_BOUNDARY_TYPE":
                    type = typeof(RA_LITH_BOUNDARY_TYPE);
                    break;


                case "RA_LITH_COLOR":
                    type = typeof(RA_LITH_COLOR);
                    break;


                case "RA_LITH_CONSOLIDATION":
                    type = typeof(RA_LITH_CONSOLIDATION);
                    break;


                case "RA_LITH_CYCLE_BED":
                    type = typeof(RA_LITH_CYCLE_BED);
                    break;


                case "RA_LITH_DEP_ENV":
                    type = typeof(RA_LITH_DEP_ENV);
                    break;


                case "RA_LITH_DIAGENESIS":
                    type = typeof(RA_LITH_DIAGENESIS);
                    break;


                case "RA_LITH_DISTRIBUTION":
                    type = typeof(RA_LITH_DISTRIBUTION);
                    break;


                case "RA_LITH_INTENSITY":
                    type = typeof(RA_LITH_INTENSITY);
                    break;


                case "RA_LITH_LOG_COMP_TYPE":
                    type = typeof(RA_LITH_LOG_COMP_TYPE);
                    break;


                case "RA_LITH_LOG_TYPE":
                    type = typeof(RA_LITH_LOG_TYPE);
                    break;


                case "RA_LITH_OIL_STAIN":
                    type = typeof(RA_LITH_OIL_STAIN);
                    break;


                case "RA_LITH_PATTERN_CODE":
                    type = typeof(RA_LITH_PATTERN_CODE);
                    break;


                case "RA_LITH_ROCK_MATRIX":
                    type = typeof(RA_LITH_ROCK_MATRIX);
                    break;


                case "RA_LITH_ROCK_PROFILE":
                    type = typeof(RA_LITH_ROCK_PROFILE);
                    break;


                case "RA_LITH_ROCK_TYPE":
                    type = typeof(RA_LITH_ROCK_TYPE);
                    break;


                case "RA_LITH_ROCKPART":
                    type = typeof(RA_LITH_ROCKPART);
                    break;


                case "RA_LITH_ROUNDING":
                    type = typeof(RA_LITH_ROUNDING);
                    break;


                case "RA_LITH_SCALE_SCHEME":
                    type = typeof(RA_LITH_SCALE_SCHEME);
                    break;


                case "RA_LITH_SORTING":
                    type = typeof(RA_LITH_SORTING);
                    break;


                case "RA_LITH_STRUCTURE":
                    type = typeof(RA_LITH_STRUCTURE);
                    break;


                case "RA_LITHOLOGY":
                    type = typeof(RA_LITHOLOGY);
                    break;


                case "RA_LOCATION_DESC_TYPE":
                    type = typeof(RA_LOCATION_DESC_TYPE);
                    break;


                case "RA_LOCATION_QUALIFIER":
                    type = typeof(RA_LOCATION_QUALIFIER);
                    break;


                case "RA_LOCATION_QUALITY":
                    type = typeof(RA_LOCATION_QUALITY);
                    break;


                case "RA_LOCATION_TYPE":
                    type = typeof(RA_LOCATION_TYPE);
                    break;


                case "RA_LOG_ARRAY_DIMENSION":
                    type = typeof(RA_LOG_ARRAY_DIMENSION);
                    break;


                case "RA_LOG_CORRECT_METHOD":
                    type = typeof(RA_LOG_CORRECT_METHOD);
                    break;


                case "RA_LOG_CRV_CLASS_SYSTEM":
                    type = typeof(RA_LOG_CRV_CLASS_SYSTEM);
                    break;


                case "RA_LOG_DEPTH_TYPE":
                    type = typeof(RA_LOG_DEPTH_TYPE);
                    break;


                case "RA_LOG_DIRECTION":
                    type = typeof(RA_LOG_DIRECTION);
                    break;


                case "RA_LOG_GOOD_VALUE_TYPE":
                    type = typeof(RA_LOG_GOOD_VALUE_TYPE);
                    break;


                case "RA_LOG_INDEX_TYPE":
                    type = typeof(RA_LOG_INDEX_TYPE);
                    break;


                case "RA_LOG_MATRIX":
                    type = typeof(RA_LOG_MATRIX);
                    break;


                case "RA_LOG_POSITION_TYPE":
                    type = typeof(RA_LOG_POSITION_TYPE);
                    break;


                case "RA_LOG_TOOL_TYPE":
                    type = typeof(RA_LOG_TOOL_TYPE);
                    break;


                case "RA_LOST_MATERIAL_TYPE":
                    type = typeof(RA_LOST_MATERIAL_TYPE);
                    break;


                case "RA_LR_FACILITY_XREF":
                    type = typeof(RA_LR_FACILITY_XREF);
                    break;


                case "RA_LR_FIELD_XREF":
                    type = typeof(RA_LR_FIELD_XREF);
                    break;


                case "RA_LR_SIZE_TYPE":
                    type = typeof(RA_LR_SIZE_TYPE);
                    break;


                case "RA_LR_TERMIN_REQMT":
                    type = typeof(RA_LR_TERMIN_REQMT);
                    break;


                case "RA_LR_TERMIN_TYPE":
                    type = typeof(RA_LR_TERMIN_TYPE);
                    break;


                case "RA_LR_XREF_TYPE":
                    type = typeof(RA_LR_XREF_TYPE);
                    break;


                case "RA_MACERAL_AMOUNT_TYPE":
                    type = typeof(RA_MACERAL_AMOUNT_TYPE);
                    break;


                case "RA_MAINT_PROCESS":
                    type = typeof(RA_MAINT_PROCESS);
                    break;


                case "RA_MATURATION_TYPE":
                    type = typeof(RA_MATURATION_TYPE);
                    break;


                case "RA_MATURITY_METHOD":
                    type = typeof(RA_MATURITY_METHOD);
                    break;


                case "RA_MBAL_COMPRESS_TYPE":
                    type = typeof(RA_MBAL_COMPRESS_TYPE);
                    break;


                case "RA_MBAL_CURVE_TYPE":
                    type = typeof(RA_MBAL_CURVE_TYPE);
                    break;


                case "RA_MEASURE_TECHNIQUE":
                    type = typeof(RA_MEASURE_TECHNIQUE);
                    break;


                case "RA_MEASUREMENT_LOC":
                    type = typeof(RA_MEASUREMENT_LOC);
                    break;


                case "RA_MEASUREMENT_TYPE":
                    type = typeof(RA_MEASUREMENT_TYPE);
                    break;


                case "RA_MEDIA_TYPE":
                    type = typeof(RA_MEDIA_TYPE);
                    break;


                case "RA_METHOD_TYPE":
                    type = typeof(RA_METHOD_TYPE);
                    break;


                case "RA_MISC_DATA_CODE":
                    type = typeof(RA_MISC_DATA_CODE);
                    break;


                case "RA_MISC_DATA_TYPE":
                    type = typeof(RA_MISC_DATA_TYPE);
                    break;


                case "RA_MISSING_STRAT_TYPE":
                    type = typeof(RA_MISSING_STRAT_TYPE);
                    break;


                case "RA_MOBILITY_TYPE":
                    type = typeof(RA_MOBILITY_TYPE);
                    break;


                case "RA_MONTH":
                    type = typeof(RA_MONTH);
                    break;


                case "RA_MUD_COLLECT_REASON":
                    type = typeof(RA_MUD_COLLECT_REASON);
                    break;


                case "RA_MUD_LOG_COLOR":
                    type = typeof(RA_MUD_LOG_COLOR);
                    break;


                case "RA_MUD_PROPERTY_CODE":
                    type = typeof(RA_MUD_PROPERTY_CODE);
                    break;


                case "RA_MUD_PROPERTY_REF":
                    type = typeof(RA_MUD_PROPERTY_REF);
                    break;


                case "RA_MUD_PROPERTY_TYPE":
                    type = typeof(RA_MUD_PROPERTY_TYPE);
                    break;


                case "RA_MUD_SAMPLE_TYPE":
                    type = typeof(RA_MUD_SAMPLE_TYPE);
                    break;


                case "RA_MUNICIPALITY":
                    type = typeof(RA_MUNICIPALITY);
                    break;


                case "RA_NAME_SET_XREF_TYPE":
                    type = typeof(RA_NAME_SET_XREF_TYPE);
                    break;


                case "RA_NODE_POSITION":
                    type = typeof(RA_NODE_POSITION);
                    break;


                case "RA_NORTH":
                    type = typeof(RA_NORTH);
                    break;


                case "RA_NOTIFICATION_COMP_TYPE":
                    type = typeof(RA_NOTIFICATION_COMP_TYPE);
                    break;


                case "RA_NOTIFICATION_TYPE":
                    type = typeof(RA_NOTIFICATION_TYPE);
                    break;


                case "RA_NS_DIRECTION":
                    type = typeof(RA_NS_DIRECTION);
                    break;


                case "RA_NS_START_LINE":
                    type = typeof(RA_NS_START_LINE);
                    break;


                case "RA_OBLIG_CALC_METHOD":
                    type = typeof(RA_OBLIG_CALC_METHOD);
                    break;


                case "RA_OBLIG_CALC_POINT":
                    type = typeof(RA_OBLIG_CALC_POINT);
                    break;


                case "RA_OBLIG_CATEGORY":
                    type = typeof(RA_OBLIG_CATEGORY);
                    break;


                case "RA_OBLIG_COMP_REASON":
                    type = typeof(RA_OBLIG_COMP_REASON);
                    break;


                case "RA_OBLIG_COMPONENT_TYPE":
                    type = typeof(RA_OBLIG_COMPONENT_TYPE);
                    break;


                case "RA_OBLIG_DEDUCT_CALC":
                    type = typeof(RA_OBLIG_DEDUCT_CALC);
                    break;


                case "RA_OBLIG_PARTY_TYPE":
                    type = typeof(RA_OBLIG_PARTY_TYPE);
                    break;


                case "RA_OBLIG_PAY_RESP":
                    type = typeof(RA_OBLIG_PAY_RESP);
                    break;


                case "RA_OBLIG_REMARK_TYPE":
                    type = typeof(RA_OBLIG_REMARK_TYPE);
                    break;


                case "RA_OBLIG_SUSPEND_PAY":
                    type = typeof(RA_OBLIG_SUSPEND_PAY);
                    break;


                case "RA_OBLIG_TRIGGER":
                    type = typeof(RA_OBLIG_TRIGGER);
                    break;


                case "RA_OBLIG_TYPE":
                    type = typeof(RA_OBLIG_TYPE);
                    break;


                case "RA_OBLIG_XREF_TYPE":
                    type = typeof(RA_OBLIG_XREF_TYPE);
                    break;


                case "RA_OFFSHORE_AREA_CODE":
                    type = typeof(RA_OFFSHORE_AREA_CODE);
                    break;


                case "RA_OFFSHORE_COMP_TYPE":
                    type = typeof(RA_OFFSHORE_COMP_TYPE);
                    break;


                case "RA_OIL_VALUE_CODE":
                    type = typeof(RA_OIL_VALUE_CODE);
                    break;


                case "RA_ONTOGENY_TYPE":
                    type = typeof(RA_ONTOGENY_TYPE);
                    break;


                case "RA_OPERAND_QUALIFIER":
                    type = typeof(RA_OPERAND_QUALIFIER);
                    break;


                case "RA_ORIENTATION":
                    type = typeof(RA_ORIENTATION);
                    break;


                case "RA_PAL_SUM_COMP_TYPE":
                    type = typeof(RA_PAL_SUM_COMP_TYPE);
                    break;


                case "RA_PAL_SUM_XREF_TYPE":
                    type = typeof(RA_PAL_SUM_XREF_TYPE);
                    break;


                case "RA_PALEO_AMOUNT_TYPE":
                    type = typeof(RA_PALEO_AMOUNT_TYPE);
                    break;


                case "RA_PALEO_IND_TYPE":
                    type = typeof(RA_PALEO_IND_TYPE);
                    break;


                case "RA_PALEO_INTERP_TYPE":
                    type = typeof(RA_PALEO_INTERP_TYPE);
                    break;


                case "RA_PALEO_TYPE_FOSSIL":
                    type = typeof(RA_PALEO_TYPE_FOSSIL);
                    break;


                case "RA_PARCEL_LOT_TYPE":
                    type = typeof(RA_PARCEL_LOT_TYPE);
                    break;


                case "RA_PARCEL_TYPE":
                    type = typeof(RA_PARCEL_TYPE);
                    break;


                case "RA_PAY_DETAIL_TYPE":
                    type = typeof(RA_PAY_DETAIL_TYPE);
                    break;


                case "RA_PAY_METHOD":
                    type = typeof(RA_PAY_METHOD);
                    break;


                case "RA_PAY_RATE_TYPE":
                    type = typeof(RA_PAY_RATE_TYPE);
                    break;


                case "RA_PAYMENT_TYPE":
                    type = typeof(RA_PAYMENT_TYPE);
                    break;


                case "RA_PAYZONE_TYPE":
                    type = typeof(RA_PAYZONE_TYPE);
                    break;


                case "RA_PDEN_AMEND_REASON":
                    type = typeof(RA_PDEN_AMEND_REASON);
                    break;


                case "RA_PDEN_COMPONENT_TYPE":
                    type = typeof(RA_PDEN_COMPONENT_TYPE);
                    break;


                case "RA_PDEN_STATUS":
                    type = typeof(RA_PDEN_STATUS);
                    break;


                case "RA_PDEN_STATUS_TYPE":
                    type = typeof(RA_PDEN_STATUS_TYPE);
                    break;


                case "RA_PDEN_XREF_TYPE":
                    type = typeof(RA_PDEN_XREF_TYPE);
                    break;


                case "RA_PERFORATION_METHOD":
                    type = typeof(RA_PERFORATION_METHOD);
                    break;


                case "RA_PERFORATION_TYPE":
                    type = typeof(RA_PERFORATION_TYPE);
                    break;


                case "RA_PERIOD_TYPE":
                    type = typeof(RA_PERIOD_TYPE);
                    break;


                case "RA_PERMEABILITY_TYPE":
                    type = typeof(RA_PERMEABILITY_TYPE);
                    break;


                case "RA_PHYS_ITEM_GROUP_TYPE":
                    type = typeof(RA_PHYS_ITEM_GROUP_TYPE);
                    break;


                case "RA_PHYSICAL_ITEM_STATUS":
                    type = typeof(RA_PHYSICAL_ITEM_STATUS);
                    break;


                case "RA_PHYSICAL_PROCESS":
                    type = typeof(RA_PHYSICAL_PROCESS);
                    break;


                case "RA_PICK_LOCATION":
                    type = typeof(RA_PICK_LOCATION);
                    break;


                case "RA_PICK_QUALIF_REASON":
                    type = typeof(RA_PICK_QUALIF_REASON);
                    break;


                case "RA_PICK_QUALIFIER":
                    type = typeof(RA_PICK_QUALIFIER);
                    break;


                case "RA_PICK_QUALITY":
                    type = typeof(RA_PICK_QUALITY);
                    break;


                case "RA_PICK_VERSION_TYPE":
                    type = typeof(RA_PICK_VERSION_TYPE);
                    break;


                case "RA_PLATFORM_TYPE":
                    type = typeof(RA_PLATFORM_TYPE);
                    break;


                case "RA_PLOT_SYMBOL":
                    type = typeof(RA_PLOT_SYMBOL);
                    break;


                case "RA_PLUG_TYPE":
                    type = typeof(RA_PLUG_TYPE);
                    break;


                case "RA_POOL_COMPONENT_TYPE":
                    type = typeof(RA_POOL_COMPONENT_TYPE);
                    break;


                case "RA_POOL_STATUS":
                    type = typeof(RA_POOL_STATUS);
                    break;


                case "RA_POOL_TYPE":
                    type = typeof(RA_POOL_TYPE);
                    break;


                case "RA_POROSITY_TYPE":
                    type = typeof(RA_POROSITY_TYPE);
                    break;


                case "RA_PPDM_AUDIT_REASON":
                    type = typeof(RA_PPDM_AUDIT_REASON);
                    break;


                case "RA_PPDM_AUDIT_TYPE":
                    type = typeof(RA_PPDM_AUDIT_TYPE);
                    break;


                case "RA_PPDM_BOOLEAN_RULE":
                    type = typeof(RA_PPDM_BOOLEAN_RULE);
                    break;


                case "RA_PPDM_CREATE_METHOD":
                    type = typeof(RA_PPDM_CREATE_METHOD);
                    break;


                case "RA_PPDM_DATA_TYPE":
                    type = typeof(RA_PPDM_DATA_TYPE);
                    break;


                case "RA_PPDM_DEFAULT_VALUE":
                    type = typeof(RA_PPDM_DEFAULT_VALUE);
                    break;


                case "RA_PPDM_ENFORCE_METHOD":
                    type = typeof(RA_PPDM_ENFORCE_METHOD);
                    break;


                case "RA_PPDM_FAIL_RESULT":
                    type = typeof(RA_PPDM_FAIL_RESULT);
                    break;


                case "RA_PPDM_GROUP_TYPE":
                    type = typeof(RA_PPDM_GROUP_TYPE);
                    break;


                case "RA_PPDM_GROUP_USE":
                    type = typeof(RA_PPDM_GROUP_USE);
                    break;


                case "RA_PPDM_GROUP_XREF_TYPE":
                    type = typeof(RA_PPDM_GROUP_XREF_TYPE);
                    break;


                case "RA_PPDM_INDEX_CATEGORY":
                    type = typeof(RA_PPDM_INDEX_CATEGORY);
                    break;


                case "RA_PPDM_MAP_RULE_TYPE":
                    type = typeof(RA_PPDM_MAP_RULE_TYPE);
                    break;


                case "RA_PPDM_MAP_TYPE":
                    type = typeof(RA_PPDM_MAP_TYPE);
                    break;


                case "RA_PPDM_METRIC_CODE":
                    type = typeof(RA_PPDM_METRIC_CODE);
                    break;


                case "RA_PPDM_METRIC_COMP_TYPE":
                    type = typeof(RA_PPDM_METRIC_COMP_TYPE);
                    break;


                case "RA_PPDM_METRIC_REF_VALUE":
                    type = typeof(RA_PPDM_METRIC_REF_VALUE);
                    break;


                case "RA_PPDM_METRIC_TYPE":
                    type = typeof(RA_PPDM_METRIC_TYPE);
                    break;


                case "RA_PPDM_OBJECT_STATUS":
                    type = typeof(RA_PPDM_OBJECT_STATUS);
                    break;


                case "RA_PPDM_OBJECT_TYPE":
                    type = typeof(RA_PPDM_OBJECT_TYPE);
                    break;


                case "RA_PPDM_OPERATING_SYSTEM":
                    type = typeof(RA_PPDM_OPERATING_SYSTEM);
                    break;


                case "RA_PPDM_OWNER_ROLE":
                    type = typeof(RA_PPDM_OWNER_ROLE);
                    break;


                case "RA_PPDM_PROC_TYPE":
                    type = typeof(RA_PPDM_PROC_TYPE);
                    break;


                case "RA_PPDM_QC_QUALITY":
                    type = typeof(RA_PPDM_QC_QUALITY);
                    break;


                case "RA_PPDM_QC_STATUS":
                    type = typeof(RA_PPDM_QC_STATUS);
                    break;


                case "RA_PPDM_QC_TYPE":
                    type = typeof(RA_PPDM_QC_TYPE);
                    break;


                case "RA_PPDM_RDBMS":
                    type = typeof(RA_PPDM_RDBMS);
                    break;


                case "RA_PPDM_REF_VALUE_TYPE":
                    type = typeof(RA_PPDM_REF_VALUE_TYPE);
                    break;


                case "RA_PPDM_ROW_QUALITY":
                    type = typeof(RA_PPDM_ROW_QUALITY);
                    break;


                case "RA_PPDM_RULE_CLASS":
                    type = typeof(RA_PPDM_RULE_CLASS);
                    break;


                case "RA_PPDM_RULE_COMP_TYPE":
                    type = typeof(RA_PPDM_RULE_COMP_TYPE);
                    break;


                case "RA_PPDM_RULE_DETAIL_TYPE":
                    type = typeof(RA_PPDM_RULE_DETAIL_TYPE);
                    break;


                case "RA_PPDM_RULE_PURPOSE":
                    type = typeof(RA_PPDM_RULE_PURPOSE);
                    break;


                case "RA_PPDM_RULE_STATUS":
                    type = typeof(RA_PPDM_RULE_STATUS);
                    break;


                case "RA_PPDM_RULE_USE_COND":
                    type = typeof(RA_PPDM_RULE_USE_COND);
                    break;


                case "RA_PPDM_RULE_XREF_COND":
                    type = typeof(RA_PPDM_RULE_XREF_COND);
                    break;


                case "RA_PPDM_RULE_XREF_TYPE":
                    type = typeof(RA_PPDM_RULE_XREF_TYPE);
                    break;


                case "RA_PPDM_SCHEMA_ENTITY":
                    type = typeof(RA_PPDM_SCHEMA_ENTITY);
                    break;


                case "RA_PPDM_SCHEMA_GROUP":
                    type = typeof(RA_PPDM_SCHEMA_GROUP);
                    break;


                case "RA_PPDM_SYSTEM_TYPE":
                    type = typeof(RA_PPDM_SYSTEM_TYPE);
                    break;


                case "RA_PPDM_TABLE_TYPE":
                    type = typeof(RA_PPDM_TABLE_TYPE);
                    break;


                case "RA_PPDM_UOM_ALIAS_TYPE":
                    type = typeof(RA_PPDM_UOM_ALIAS_TYPE);
                    break;


                case "RA_PPDM_UOM_USAGE":
                    type = typeof(RA_PPDM_UOM_USAGE);
                    break;


                case "RA_PRESERVE_QUALITY":
                    type = typeof(RA_PRESERVE_QUALITY);
                    break;


                case "RA_PRESERVE_TYPE":
                    type = typeof(RA_PRESERVE_TYPE);
                    break;


                case "RA_PROD_STR_FM_STAT_TYPE":
                    type = typeof(RA_PROD_STR_FM_STAT_TYPE);
                    break;


                case "RA_PROD_STR_FM_STATUS":
                    type = typeof(RA_PROD_STR_FM_STATUS);
                    break;


                case "RA_PROD_STRING_COMP_TYPE":
                    type = typeof(RA_PROD_STRING_COMP_TYPE);
                    break;


                case "RA_PROD_STRING_STAT_TYPE":
                    type = typeof(RA_PROD_STRING_STAT_TYPE);
                    break;


                case "RA_PROD_STRING_STATUS":
                    type = typeof(RA_PROD_STRING_STATUS);
                    break;


                case "RA_PROD_STRING_TYPE":
                    type = typeof(RA_PROD_STRING_TYPE);
                    break;


                case "RA_PRODUCT_COMPONENT_TYPE":
                    type = typeof(RA_PRODUCT_COMPONENT_TYPE);
                    break;


                case "RA_PRODUCTION_METHOD":
                    type = typeof(RA_PRODUCTION_METHOD);
                    break;


                case "RA_PROJ_STEP_TYPE":
                    type = typeof(RA_PROJ_STEP_TYPE);
                    break;


                case "RA_PROJ_STEP_XREF_TYPE":
                    type = typeof(RA_PROJ_STEP_XREF_TYPE);
                    break;


                case "RA_PROJECT_ALIAS_TYPE":
                    type = typeof(RA_PROJECT_ALIAS_TYPE);
                    break;


                case "RA_PROJECT_BA_ROLE":
                    type = typeof(RA_PROJECT_BA_ROLE);
                    break;


                case "RA_PROJECT_COMP_REASON":
                    type = typeof(RA_PROJECT_COMP_REASON);
                    break;


                case "RA_PROJECT_COMP_TYPE":
                    type = typeof(RA_PROJECT_COMP_TYPE);
                    break;


                case "RA_PROJECT_CONDITION":
                    type = typeof(RA_PROJECT_CONDITION);
                    break;


                case "RA_PROJECT_EQUIP_ROLE":
                    type = typeof(RA_PROJECT_EQUIP_ROLE);
                    break;


                case "RA_PROJECT_STATUS":
                    type = typeof(RA_PROJECT_STATUS);
                    break;


                case "RA_PROJECT_STATUS_TYPE":
                    type = typeof(RA_PROJECT_STATUS_TYPE);
                    break;


                case "RA_PROJECT_TYPE":
                    type = typeof(RA_PROJECT_TYPE);
                    break;


                case "RA_PROJECTION_TYPE":
                    type = typeof(RA_PROJECTION_TYPE);
                    break;


                case "RA_PROPPANT_TYPE":
                    type = typeof(RA_PROPPANT_TYPE);
                    break;


                case "RA_PUBLICATION_NAME":
                    type = typeof(RA_PUBLICATION_NAME);
                    break;


                case "RA_QUALIFIER_TYPE":
                    type = typeof(RA_QUALIFIER_TYPE);
                    break;


                case "RA_QUALITY":
                    type = typeof(RA_QUALITY);
                    break;


                case "RA_RATE_CONDITION":
                    type = typeof(RA_RATE_CONDITION);
                    break;


                case "RA_RATE_SCHED_XREF":
                    type = typeof(RA_RATE_SCHED_XREF);
                    break;


                case "RA_RATE_SCHEDULE":
                    type = typeof(RA_RATE_SCHEDULE);
                    break;


                case "RA_RATE_SCHEDULE_COMP_TYPE":
                    type = typeof(RA_RATE_SCHEDULE_COMP_TYPE);
                    break;


                case "RA_RATE_TYPE":
                    type = typeof(RA_RATE_TYPE);
                    break;


                case "RA_RATIO_CURVE_TYPE":
                    type = typeof(RA_RATIO_CURVE_TYPE);
                    break;


                case "RA_RATIO_FLUID_TYPE":
                    type = typeof(RA_RATIO_FLUID_TYPE);
                    break;


                case "RA_RECORDER_POSITION":
                    type = typeof(RA_RECORDER_POSITION);
                    break;


                case "RA_RECORDER_TYPE":
                    type = typeof(RA_RECORDER_TYPE);
                    break;


                case "RA_REMARK_TYPE":
                    type = typeof(RA_REMARK_TYPE);
                    break;


                case "RA_REMARK_USE_TYPE":
                    type = typeof(RA_REMARK_USE_TYPE);
                    break;


                case "RA_REP_HIER_ALIAS_TYPE":
                    type = typeof(RA_REP_HIER_ALIAS_TYPE);
                    break;


                case "RA_REPEAT_STRAT_TYPE":
                    type = typeof(RA_REPEAT_STRAT_TYPE);
                    break;


                case "RA_REPORT_HIER_COMP":
                    type = typeof(RA_REPORT_HIER_COMP);
                    break;


                case "RA_REPORT_HIER_LEVEL":
                    type = typeof(RA_REPORT_HIER_LEVEL);
                    break;


                case "RA_REPORT_HIER_TYPE":
                    type = typeof(RA_REPORT_HIER_TYPE);
                    break;


                case "RA_RESENT_COMP_TYPE":
                    type = typeof(RA_RESENT_COMP_TYPE);
                    break;


                case "RA_RESENT_CONFIDENCE":
                    type = typeof(RA_RESENT_CONFIDENCE);
                    break;


                case "RA_RESENT_GROUP_TYPE":
                    type = typeof(RA_RESENT_GROUP_TYPE);
                    break;


                case "RA_RESENT_REV_CAT":
                    type = typeof(RA_RESENT_REV_CAT);
                    break;


                case "RA_RESENT_USE_TYPE":
                    type = typeof(RA_RESENT_USE_TYPE);
                    break;


                case "RA_RESENT_VOL_METHOD":
                    type = typeof(RA_RESENT_VOL_METHOD);
                    break;


                case "RA_RESENT_XREF_TYPE":
                    type = typeof(RA_RESENT_XREF_TYPE);
                    break;


                case "RA_REST_ACTIVITY":
                    type = typeof(RA_REST_ACTIVITY);
                    break;


                case "RA_REST_DURATION":
                    type = typeof(RA_REST_DURATION);
                    break;


                case "RA_REST_REMARK":
                    type = typeof(RA_REST_REMARK);
                    break;


                case "RA_REST_TYPE":
                    type = typeof(RA_REST_TYPE);
                    break;


                case "RA_RETENTION_PERIOD":
                    type = typeof(RA_RETENTION_PERIOD);
                    break;


                case "RA_REVISION_METHOD":
                    type = typeof(RA_REVISION_METHOD);
                    break;


                case "RA_RIG_BLOWOUT_PREVENTER":
                    type = typeof(RA_RIG_BLOWOUT_PREVENTER);
                    break;


                case "RA_RIG_CATEGORY":
                    type = typeof(RA_RIG_CATEGORY);
                    break;


                case "RA_RIG_CLASS":
                    type = typeof(RA_RIG_CLASS);
                    break;


                case "RA_RIG_CODE":
                    type = typeof(RA_RIG_CODE);
                    break;


                case "RA_RIG_DESANDER_TYPE":
                    type = typeof(RA_RIG_DESANDER_TYPE);
                    break;


                case "RA_RIG_DESILTER_TYPE":
                    type = typeof(RA_RIG_DESILTER_TYPE);
                    break;


                case "RA_RIG_DRAWWORKS":
                    type = typeof(RA_RIG_DRAWWORKS);
                    break;


                case "RA_RIG_GENERATOR_TYPE":
                    type = typeof(RA_RIG_GENERATOR_TYPE);
                    break;


                case "RA_RIG_HOOKBLOCK_TYPE":
                    type = typeof(RA_RIG_HOOKBLOCK_TYPE);
                    break;


                case "RA_RIG_MAST":
                    type = typeof(RA_RIG_MAST);
                    break;


                case "RA_RIG_OVERHEAD_CAPACITY":
                    type = typeof(RA_RIG_OVERHEAD_CAPACITY);
                    break;


                case "RA_RIG_OVERHEAD_TYPE":
                    type = typeof(RA_RIG_OVERHEAD_TYPE);
                    break;


                case "RA_RIG_PUMP":
                    type = typeof(RA_RIG_PUMP);
                    break;


                case "RA_RIG_PUMP_FUNCTION":
                    type = typeof(RA_RIG_PUMP_FUNCTION);
                    break;


                case "RA_RIG_SUBSTRUCTURE":
                    type = typeof(RA_RIG_SUBSTRUCTURE);
                    break;


                case "RA_RIG_SWIVEL_TYPE":
                    type = typeof(RA_RIG_SWIVEL_TYPE);
                    break;


                case "RA_RIG_TYPE":
                    type = typeof(RA_RIG_TYPE);
                    break;


                case "RA_RM_THESAURUS_XREF":
                    type = typeof(RA_RM_THESAURUS_XREF);
                    break;


                case "RA_RMII_CONTACT_TYPE":
                    type = typeof(RA_RMII_CONTACT_TYPE);
                    break;


                case "RA_RMII_CONTENT_TYPE":
                    type = typeof(RA_RMII_CONTENT_TYPE);
                    break;


                case "RA_RMII_DEFICIENCY":
                    type = typeof(RA_RMII_DEFICIENCY);
                    break;


                case "RA_RMII_DESC_TYPE":
                    type = typeof(RA_RMII_DESC_TYPE);
                    break;


                case "RA_RMII_GROUP_TYPE":
                    type = typeof(RA_RMII_GROUP_TYPE);
                    break;


                case "RA_RMII_METADATA_CODE":
                    type = typeof(RA_RMII_METADATA_CODE);
                    break;


                case "RA_RMII_METADATA_TYPE":
                    type = typeof(RA_RMII_METADATA_TYPE);
                    break;


                case "RA_RMII_QUALITY_CODE":
                    type = typeof(RA_RMII_QUALITY_CODE);
                    break;


                case "RA_RMII_STATUS":
                    type = typeof(RA_RMII_STATUS);
                    break;


                case "RA_RMII_STATUS_TYPE":
                    type = typeof(RA_RMII_STATUS_TYPE);
                    break;


                case "RA_ROAD_CONDITION":
                    type = typeof(RA_ROAD_CONDITION);
                    break;


                case "RA_ROAD_CONTROL_TYPE":
                    type = typeof(RA_ROAD_CONTROL_TYPE);
                    break;


                case "RA_ROAD_DRIVING_SIDE":
                    type = typeof(RA_ROAD_DRIVING_SIDE);
                    break;


                case "RA_ROAD_TRAFFIC_FLOW_TYPE":
                    type = typeof(RA_ROAD_TRAFFIC_FLOW_TYPE);
                    break;


                case "RA_ROCK_CLASS_SCHEME":
                    type = typeof(RA_ROCK_CLASS_SCHEME);
                    break;


                case "RA_ROLL_ALONG_METHOD":
                    type = typeof(RA_ROLL_ALONG_METHOD);
                    break;


                case "RA_ROYALTY_TYPE":
                    type = typeof(RA_ROYALTY_TYPE);
                    break;


                case "RA_SALINITY_TYPE":
                    type = typeof(RA_SALINITY_TYPE);
                    break;


                case "RA_SAMPLE_COLLECT_METHOD":
                    type = typeof(RA_SAMPLE_COLLECT_METHOD);
                    break;


                case "RA_SAMPLE_COLLECTION_TYPE":
                    type = typeof(RA_SAMPLE_COLLECTION_TYPE);
                    break;


                case "RA_SAMPLE_COMP_TYPE":
                    type = typeof(RA_SAMPLE_COMP_TYPE);
                    break;


                case "RA_SAMPLE_CONTAMINANT":
                    type = typeof(RA_SAMPLE_CONTAMINANT);
                    break;


                case "RA_SAMPLE_DESC_CODE":
                    type = typeof(RA_SAMPLE_DESC_CODE);
                    break;


                case "RA_SAMPLE_DESC_TYPE":
                    type = typeof(RA_SAMPLE_DESC_TYPE);
                    break;


                case "RA_SAMPLE_FRACTION_TYPE":
                    type = typeof(RA_SAMPLE_FRACTION_TYPE);
                    break;


                case "RA_SAMPLE_LOCATION":
                    type = typeof(RA_SAMPLE_LOCATION);
                    break;


                case "RA_SAMPLE_PHASE":
                    type = typeof(RA_SAMPLE_PHASE);
                    break;


                case "RA_SAMPLE_PREP_CLASS":
                    type = typeof(RA_SAMPLE_PREP_CLASS);
                    break;


                case "RA_SAMPLE_REF_VALUE_TYPE":
                    type = typeof(RA_SAMPLE_REF_VALUE_TYPE);
                    break;


                case "RA_SAMPLE_SHAPE":
                    type = typeof(RA_SAMPLE_SHAPE);
                    break;


                case "RA_SAMPLE_TYPE":
                    type = typeof(RA_SAMPLE_TYPE);
                    break;


                case "RA_SCALE_TRANSFORM":
                    type = typeof(RA_SCALE_TRANSFORM);
                    break;


                case "RA_SCREEN_LOCATION":
                    type = typeof(RA_SCREEN_LOCATION);
                    break;


                case "RA_SECTION_TYPE":
                    type = typeof(RA_SECTION_TYPE);
                    break;


                case "RA_SEIS_3D_TYPE":
                    type = typeof(RA_SEIS_3D_TYPE);
                    break;


                case "RA_SEIS_ACTIVITY_CLASS":
                    type = typeof(RA_SEIS_ACTIVITY_CLASS);
                    break;


                case "RA_SEIS_ACTIVITY_TYPE":
                    type = typeof(RA_SEIS_ACTIVITY_TYPE);
                    break;


                case "RA_SEIS_AUTHORIZE_LIMIT":
                    type = typeof(RA_SEIS_AUTHORIZE_LIMIT);
                    break;


                case "RA_SEIS_AUTHORIZE_REASON":
                    type = typeof(RA_SEIS_AUTHORIZE_REASON);
                    break;


                case "RA_SEIS_AUTHORIZE_TYPE":
                    type = typeof(RA_SEIS_AUTHORIZE_TYPE);
                    break;


                case "RA_SEIS_BIN_METHOD":
                    type = typeof(RA_SEIS_BIN_METHOD);
                    break;


                case "RA_SEIS_BIN_OUTLINE_TYPE":
                    type = typeof(RA_SEIS_BIN_OUTLINE_TYPE);
                    break;


                case "RA_SEIS_CABLE_MAKE":
                    type = typeof(RA_SEIS_CABLE_MAKE);
                    break;


                case "RA_SEIS_CHANNEL_TYPE":
                    type = typeof(RA_SEIS_CHANNEL_TYPE);
                    break;


                case "RA_SEIS_DIMENSION":
                    type = typeof(RA_SEIS_DIMENSION);
                    break;


                case "RA_SEIS_ENERGY_TYPE":
                    type = typeof(RA_SEIS_ENERGY_TYPE);
                    break;


                case "RA_SEIS_FLOW_DESC_TYPE":
                    type = typeof(RA_SEIS_FLOW_DESC_TYPE);
                    break;


                case "RA_SEIS_GROUP_TYPE":
                    type = typeof(RA_SEIS_GROUP_TYPE);
                    break;


                case "RA_SEIS_INSP_COMPONENT_TYPE":
                    type = typeof(RA_SEIS_INSP_COMPONENT_TYPE);
                    break;


                case "RA_SEIS_LIC_COND":
                    type = typeof(RA_SEIS_LIC_COND);
                    break;


                case "RA_SEIS_LIC_COND_CODE":
                    type = typeof(RA_SEIS_LIC_COND_CODE);
                    break;


                case "RA_SEIS_LIC_DUE_CONDITION":
                    type = typeof(RA_SEIS_LIC_DUE_CONDITION);
                    break;


                case "RA_SEIS_LIC_VIOL_RESOL":
                    type = typeof(RA_SEIS_LIC_VIOL_RESOL);
                    break;


                case "RA_SEIS_LIC_VIOL_TYPE":
                    type = typeof(RA_SEIS_LIC_VIOL_TYPE);
                    break;


                case "RA_SEIS_PARM_ORIGIN":
                    type = typeof(RA_SEIS_PARM_ORIGIN);
                    break;


                case "RA_SEIS_PATCH_TYPE":
                    type = typeof(RA_SEIS_PATCH_TYPE);
                    break;


                case "RA_SEIS_PICK_METHOD":
                    type = typeof(RA_SEIS_PICK_METHOD);
                    break;


                case "RA_SEIS_PROC_COMP_TYPE":
                    type = typeof(RA_SEIS_PROC_COMP_TYPE);
                    break;


                case "RA_SEIS_PROC_PARM":
                    type = typeof(RA_SEIS_PROC_PARM);
                    break;


                case "RA_SEIS_PROC_SET_TYPE":
                    type = typeof(RA_SEIS_PROC_SET_TYPE);
                    break;


                case "RA_SEIS_PROC_STATUS":
                    type = typeof(RA_SEIS_PROC_STATUS);
                    break;


                case "RA_SEIS_PROC_STEP_NAME":
                    type = typeof(RA_SEIS_PROC_STEP_NAME);
                    break;


                case "RA_SEIS_PROC_STEP_TYPE":
                    type = typeof(RA_SEIS_PROC_STEP_TYPE);
                    break;


                case "RA_SEIS_RCRD_FMT_TYPE":
                    type = typeof(RA_SEIS_RCRD_FMT_TYPE);
                    break;


                case "RA_SEIS_RCRD_MAKE":
                    type = typeof(RA_SEIS_RCRD_MAKE);
                    break;


                case "RA_SEIS_RCVR_ARRY_TYPE":
                    type = typeof(RA_SEIS_RCVR_ARRY_TYPE);
                    break;


                case "RA_SEIS_RCVR_TYPE":
                    type = typeof(RA_SEIS_RCVR_TYPE);
                    break;


                case "RA_SEIS_RECORD_TYPE":
                    type = typeof(RA_SEIS_RECORD_TYPE);
                    break;


                case "RA_SEIS_REF_DATUM":
                    type = typeof(RA_SEIS_REF_DATUM);
                    break;


                case "RA_SEIS_REF_NUM_TYPE":
                    type = typeof(RA_SEIS_REF_NUM_TYPE);
                    break;


                case "RA_SEIS_SAMPLE_TYPE":
                    type = typeof(RA_SEIS_SAMPLE_TYPE);
                    break;


                case "RA_SEIS_SEGMENT_REASON":
                    type = typeof(RA_SEIS_SEGMENT_REASON);
                    break;


                case "RA_SEIS_SET_COMPONENT_TYPE":
                    type = typeof(RA_SEIS_SET_COMPONENT_TYPE);
                    break;


                case "RA_SEIS_SPECTRUM_TYPE":
                    type = typeof(RA_SEIS_SPECTRUM_TYPE);
                    break;


                case "RA_SEIS_SRC_ARRAY_TYPE":
                    type = typeof(RA_SEIS_SRC_ARRAY_TYPE);
                    break;


                case "RA_SEIS_SRC_MAKE":
                    type = typeof(RA_SEIS_SRC_MAKE);
                    break;


                case "RA_SEIS_STATION_TYPE":
                    type = typeof(RA_SEIS_STATION_TYPE);
                    break;


                case "RA_SEIS_STATUS":
                    type = typeof(RA_SEIS_STATUS);
                    break;


                case "RA_SEIS_STATUS_TYPE":
                    type = typeof(RA_SEIS_STATUS_TYPE);
                    break;


                case "RA_SEIS_SUMMARY_TYPE":
                    type = typeof(RA_SEIS_SUMMARY_TYPE);
                    break;


                case "RA_SEIS_SWEEP_TYPE":
                    type = typeof(RA_SEIS_SWEEP_TYPE);
                    break;


                case "RA_SEIS_TRANS_COMP_TYPE":
                    type = typeof(RA_SEIS_TRANS_COMP_TYPE);
                    break;


                case "RA_SEISMIC_PATH":
                    type = typeof(RA_SEISMIC_PATH);
                    break;


                case "RA_SEND_METHOD":
                    type = typeof(RA_SEND_METHOD);
                    break;


                case "RA_SERVICE_QUALITY":
                    type = typeof(RA_SERVICE_QUALITY);
                    break;


                case "RA_SEVERITY":
                    type = typeof(RA_SEVERITY);
                    break;


                case "RA_SF_AIRSTRIP_TYPE":
                    type = typeof(RA_SF_AIRSTRIP_TYPE);
                    break;


                case "RA_SF_BRIDGE_TYPE":
                    type = typeof(RA_SF_BRIDGE_TYPE);
                    break;


                case "RA_SF_COMPONENT_TYPE":
                    type = typeof(RA_SF_COMPONENT_TYPE);
                    break;


                case "RA_SF_DESC_TYPE":
                    type = typeof(RA_SF_DESC_TYPE);
                    break;


                case "RA_SF_DESC_VALUE":
                    type = typeof(RA_SF_DESC_VALUE);
                    break;


                case "RA_SF_DOCK_TYPE":
                    type = typeof(RA_SF_DOCK_TYPE);
                    break;


                case "RA_SF_ELECTRIC_TYPE":
                    type = typeof(RA_SF_ELECTRIC_TYPE);
                    break;


                case "RA_SF_LANDING_TYPE":
                    type = typeof(RA_SF_LANDING_TYPE);
                    break;


                case "RA_SF_MAINTAIN_TYPE":
                    type = typeof(RA_SF_MAINTAIN_TYPE);
                    break;


                case "RA_SF_PAD_TYPE":
                    type = typeof(RA_SF_PAD_TYPE);
                    break;


                case "RA_SF_ROAD_TYPE":
                    type = typeof(RA_SF_ROAD_TYPE);
                    break;


                case "RA_SF_STATUS":
                    type = typeof(RA_SF_STATUS);
                    break;


                case "RA_SF_STATUS_TYPE":
                    type = typeof(RA_SF_STATUS_TYPE);
                    break;


                case "RA_SF_SURFACE_TYPE":
                    type = typeof(RA_SF_SURFACE_TYPE);
                    break;


                case "RA_SF_TOWER_TYPE":
                    type = typeof(RA_SF_TOWER_TYPE);
                    break;


                case "RA_SF_VEHICLE_TYPE":
                    type = typeof(RA_SF_VEHICLE_TYPE);
                    break;


                case "RA_SF_VESSEL_ROLE":
                    type = typeof(RA_SF_VESSEL_ROLE);
                    break;


                case "RA_SF_VESSEL_TYPE":
                    type = typeof(RA_SF_VESSEL_TYPE);
                    break;


                case "RA_SF_XREF_TYPE":
                    type = typeof(RA_SF_XREF_TYPE);
                    break;


                case "RA_SHOW_TYPE":
                    type = typeof(RA_SHOW_TYPE);
                    break;


                case "RA_SHUTIN_PRESS_TYPE":
                    type = typeof(RA_SHUTIN_PRESS_TYPE);
                    break;


                case "RA_SOURCE":
                    type = typeof(RA_SOURCE);
                    break;


                case "RA_SOURCE_ORIGIN":
                    type = typeof(RA_SOURCE_ORIGIN);
                    break;


                case "RA_SP_POINT_VERSION_TYPE":
                    type = typeof(RA_SP_POINT_VERSION_TYPE);
                    break;


                case "RA_SP_ZONE_DEFIN_XREF":
                    type = typeof(RA_SP_ZONE_DEFIN_XREF);
                    break;


                case "RA_SP_ZONE_DERIV":
                    type = typeof(RA_SP_ZONE_DERIV);
                    break;


                case "RA_SP_ZONE_TYPE":
                    type = typeof(RA_SP_ZONE_TYPE);
                    break;


                case "RA_SPACING_UNIT_TYPE":
                    type = typeof(RA_SPACING_UNIT_TYPE);
                    break;


                case "RA_SPATIAL_DESC_COMP_TYPE":
                    type = typeof(RA_SPATIAL_DESC_COMP_TYPE);
                    break;


                case "RA_SPATIAL_DESC_TYPE":
                    type = typeof(RA_SPATIAL_DESC_TYPE);
                    break;


                case "RA_SPATIAL_XREF_TYPE":
                    type = typeof(RA_SPATIAL_XREF_TYPE);
                    break;


                case "RA_STATUS_GROUP":
                    type = typeof(RA_STATUS_GROUP);
                    break;


                case "RA_STORE_STATUS":
                    type = typeof(RA_STORE_STATUS);
                    break;


                case "RA_STRAT_ACQTN_METHOD":
                    type = typeof(RA_STRAT_ACQTN_METHOD);
                    break;


                case "RA_STRAT_AGE_METHOD":
                    type = typeof(RA_STRAT_AGE_METHOD);
                    break;


                case "RA_STRAT_ALIAS_TYPE":
                    type = typeof(RA_STRAT_ALIAS_TYPE);
                    break;


                case "RA_STRAT_COL_XREF_TYPE":
                    type = typeof(RA_STRAT_COL_XREF_TYPE);
                    break;


                case "RA_STRAT_COLUMN_TYPE":
                    type = typeof(RA_STRAT_COLUMN_TYPE);
                    break;


                case "RA_STRAT_CORR_CRITERIA":
                    type = typeof(RA_STRAT_CORR_CRITERIA);
                    break;


                case "RA_STRAT_CORR_TYPE":
                    type = typeof(RA_STRAT_CORR_TYPE);
                    break;


                case "RA_STRAT_DESC_TYPE":
                    type = typeof(RA_STRAT_DESC_TYPE);
                    break;


                case "RA_STRAT_EQUIV_DIRECT":
                    type = typeof(RA_STRAT_EQUIV_DIRECT);
                    break;


                case "RA_STRAT_EQUIV_TYPE":
                    type = typeof(RA_STRAT_EQUIV_TYPE);
                    break;


                case "RA_STRAT_FLD_NODE_LOC":
                    type = typeof(RA_STRAT_FLD_NODE_LOC);
                    break;


                case "RA_STRAT_HIERARCHY":
                    type = typeof(RA_STRAT_HIERARCHY);
                    break;


                case "RA_STRAT_INTERP_METHOD":
                    type = typeof(RA_STRAT_INTERP_METHOD);
                    break;


                case "RA_STRAT_NAME_SET_TYPE":
                    type = typeof(RA_STRAT_NAME_SET_TYPE);
                    break;


                case "RA_STRAT_OCCURRENCE_TYPE":
                    type = typeof(RA_STRAT_OCCURRENCE_TYPE);
                    break;


                case "RA_STRAT_STATUS":
                    type = typeof(RA_STRAT_STATUS);
                    break;


                case "RA_STRAT_TOPO_RELATION":
                    type = typeof(RA_STRAT_TOPO_RELATION);
                    break;


                case "RA_STRAT_TYPE":
                    type = typeof(RA_STRAT_TYPE);
                    break;


                case "RA_STRAT_UNIT_COMP_TYPE":
                    type = typeof(RA_STRAT_UNIT_COMP_TYPE);
                    break;


                case "RA_STRAT_UNIT_DESC":
                    type = typeof(RA_STRAT_UNIT_DESC);
                    break;


                case "RA_STRAT_UNIT_QUALIFIER":
                    type = typeof(RA_STRAT_UNIT_QUALIFIER);
                    break;


                case "RA_STRAT_UNIT_TYPE":
                    type = typeof(RA_STRAT_UNIT_TYPE);
                    break;


                case "RA_STREAMER_COMP":
                    type = typeof(RA_STREAMER_COMP);
                    break;


                case "RA_STREAMER_POSITION":
                    type = typeof(RA_STREAMER_POSITION);
                    break;


                case "RA_STUDY_TYPE":
                    type = typeof(RA_STUDY_TYPE);
                    break;


                case "RA_SUBSTANCE_COMP_TYPE":
                    type = typeof(RA_SUBSTANCE_COMP_TYPE);
                    break;


                case "RA_SUBSTANCE_PROPERTY":
                    type = typeof(RA_SUBSTANCE_PROPERTY);
                    break;


                case "RA_SUBSTANCE_USE_RULE":
                    type = typeof(RA_SUBSTANCE_USE_RULE);
                    break;


                case "RA_SUBSTANCE_XREF_TYPE":
                    type = typeof(RA_SUBSTANCE_XREF_TYPE);
                    break;


                case "RA_SW_APP_BA_ROLE":
                    type = typeof(RA_SW_APP_BA_ROLE);
                    break;


                case "RA_SW_APP_FUNCTION":
                    type = typeof(RA_SW_APP_FUNCTION);
                    break;


                case "RA_SW_APP_FUNCTION_TYPE":
                    type = typeof(RA_SW_APP_FUNCTION_TYPE);
                    break;


                case "RA_SW_APP_XREF_TYPE":
                    type = typeof(RA_SW_APP_XREF_TYPE);
                    break;


                case "RA_TAX_CREDIT_CODE":
                    type = typeof(RA_TAX_CREDIT_CODE);
                    break;


                case "RA_TEST_EQUIPMENT":
                    type = typeof(RA_TEST_EQUIPMENT);
                    break;


                case "RA_TEST_PERIOD_TYPE":
                    type = typeof(RA_TEST_PERIOD_TYPE);
                    break;


                case "RA_TEST_RECOV_METHOD":
                    type = typeof(RA_TEST_RECOV_METHOD);
                    break;


                case "RA_TEST_RESULT":
                    type = typeof(RA_TEST_RESULT);
                    break;


                case "RA_TEST_SHUTOFF_TYPE":
                    type = typeof(RA_TEST_SHUTOFF_TYPE);
                    break;


                case "RA_TEST_SUBTYPE":
                    type = typeof(RA_TEST_SUBTYPE);
                    break;


                case "RA_TIMEZONE":
                    type = typeof(RA_TIMEZONE);
                    break;


                case "RA_TITLE_OWN_TYPE":
                    type = typeof(RA_TITLE_OWN_TYPE);
                    break;


                case "RA_TOUR_OCCURRENCE_TYPE":
                    type = typeof(RA_TOUR_OCCURRENCE_TYPE);
                    break;


                case "RA_TRACE_HEADER_FORMAT":
                    type = typeof(RA_TRACE_HEADER_FORMAT);
                    break;


                case "RA_TRACE_HEADER_WORD":
                    type = typeof(RA_TRACE_HEADER_WORD);
                    break;


                case "RA_TRANS_COMP_TYPE":
                    type = typeof(RA_TRANS_COMP_TYPE);
                    break;


                case "RA_TRANS_STATUS":
                    type = typeof(RA_TRANS_STATUS);
                    break;


                case "RA_TRANS_TYPE":
                    type = typeof(RA_TRANS_TYPE);
                    break;


                case "RA_TREATMENT_FLUID":
                    type = typeof(RA_TREATMENT_FLUID);
                    break;


                case "RA_TREATMENT_TYPE":
                    type = typeof(RA_TREATMENT_TYPE);
                    break;


                case "RA_TUBING_GRADE":
                    type = typeof(RA_TUBING_GRADE);
                    break;


                case "RA_TUBING_TYPE":
                    type = typeof(RA_TUBING_TYPE);
                    break;


                case "RA_TVD_METHOD":
                    type = typeof(RA_TVD_METHOD);
                    break;


                case "RA_VALUE_QUALITY":
                    type = typeof(RA_VALUE_QUALITY);
                    break;


                case "RA_VELOCITY_COMPUTE":
                    type = typeof(RA_VELOCITY_COMPUTE);
                    break;


                case "RA_VELOCITY_DIMENSION":
                    type = typeof(RA_VELOCITY_DIMENSION);
                    break;


                case "RA_VELOCITY_TYPE":
                    type = typeof(RA_VELOCITY_TYPE);
                    break;


                case "RA_VERTICAL_DATUM_TYPE":
                    type = typeof(RA_VERTICAL_DATUM_TYPE);
                    break;


                case "RA_VESSEL_REFERENCE":
                    type = typeof(RA_VESSEL_REFERENCE);
                    break;


                case "RA_VESSEL_SIZE":
                    type = typeof(RA_VESSEL_SIZE);
                    break;


                case "RA_VOLUME_FRACTION":
                    type = typeof(RA_VOLUME_FRACTION);
                    break;


                case "RA_VOLUME_METHOD":
                    type = typeof(RA_VOLUME_METHOD);
                    break;


                case "RA_VSP_TYPE":
                    type = typeof(RA_VSP_TYPE);
                    break;


                case "RA_WASTE_ADJUST_REASON":
                    type = typeof(RA_WASTE_ADJUST_REASON);
                    break;


                case "RA_WASTE_FACILITY_TYPE":
                    type = typeof(RA_WASTE_FACILITY_TYPE);
                    break;


                case "RA_WASTE_HANDLING":
                    type = typeof(RA_WASTE_HANDLING);
                    break;


                case "RA_WASTE_HAZARD_TYPE":
                    type = typeof(RA_WASTE_HAZARD_TYPE);
                    break;


                case "RA_WASTE_MATERIAL":
                    type = typeof(RA_WASTE_MATERIAL);
                    break;


                case "RA_WASTE_ORIGIN":
                    type = typeof(RA_WASTE_ORIGIN);
                    break;


                case "RA_WATER_BOTTOM_ZONE":
                    type = typeof(RA_WATER_BOTTOM_ZONE);
                    break;


                case "RA_WATER_CONDITION":
                    type = typeof(RA_WATER_CONDITION);
                    break;


                case "RA_WATER_DATUM":
                    type = typeof(RA_WATER_DATUM);
                    break;


                case "RA_WATER_PROPERTY_CODE":
                    type = typeof(RA_WATER_PROPERTY_CODE);
                    break;


                case "RA_WEATHER_CONDITION":
                    type = typeof(RA_WEATHER_CONDITION);
                    break;


                case "RA_WELL_ACT_TYPE_EQUIV":
                    type = typeof(RA_WELL_ACT_TYPE_EQUIV);
                    break;


                case "RA_WELL_ACTIVITY_CAUSE":
                    type = typeof(RA_WELL_ACTIVITY_CAUSE);
                    break;


                case "RA_WELL_ACTIVITY_COMP_TYPE":
                    type = typeof(RA_WELL_ACTIVITY_COMP_TYPE);
                    break;


                case "RA_WELL_ALIAS_LOCATION":
                    type = typeof(RA_WELL_ALIAS_LOCATION);
                    break;


                case "RA_WELL_CIRC_PRESS_TYPE":
                    type = typeof(RA_WELL_CIRC_PRESS_TYPE);
                    break;


                case "RA_WELL_CLASS":
                    type = typeof(RA_WELL_CLASS);
                    break;


                case "RA_WELL_COMPONENT_TYPE":
                    type = typeof(RA_WELL_COMPONENT_TYPE);
                    break;


                case "RA_WELL_DATUM_TYPE":
                    type = typeof(RA_WELL_DATUM_TYPE);
                    break;


                case "RA_WELL_DOWNTIME_TYPE":
                    type = typeof(RA_WELL_DOWNTIME_TYPE);
                    break;


                case "RA_WELL_DRILL_OP_TYPE":
                    type = typeof(RA_WELL_DRILL_OP_TYPE);
                    break;


                case "RA_WELL_FACILITY_USE_TYPE":
                    type = typeof(RA_WELL_FACILITY_USE_TYPE);
                    break;


                case "RA_WELL_LEVEL_TYPE":
                    type = typeof(RA_WELL_LEVEL_TYPE);
                    break;


                case "RA_WELL_LIC_COND":
                    type = typeof(RA_WELL_LIC_COND);
                    break;


                case "RA_WELL_LIC_COND_CODE":
                    type = typeof(RA_WELL_LIC_COND_CODE);
                    break;


                case "RA_WELL_LIC_DUE_CONDITION":
                    type = typeof(RA_WELL_LIC_DUE_CONDITION);
                    break;


                case "RA_WELL_LIC_VIOL_RESOL":
                    type = typeof(RA_WELL_LIC_VIOL_RESOL);
                    break;


                case "RA_WELL_LIC_VIOL_TYPE":
                    type = typeof(RA_WELL_LIC_VIOL_TYPE);
                    break;


                case "RA_WELL_LOG_CLASS":
                    type = typeof(RA_WELL_LOG_CLASS);
                    break;


                case "RA_WELL_NODE_PICK_METHOD":
                    type = typeof(RA_WELL_NODE_PICK_METHOD);
                    break;


                case "RA_WELL_PROFILE_TYPE":
                    type = typeof(RA_WELL_PROFILE_TYPE);
                    break;


                case "RA_WELL_QUALIFIC_TYPE":
                    type = typeof(RA_WELL_QUALIFIC_TYPE);
                    break;


                case "RA_WELL_REF_VALUE_TYPE":
                    type = typeof(RA_WELL_REF_VALUE_TYPE);
                    break;


                case "RA_WELL_RELATIONSHIP":
                    type = typeof(RA_WELL_RELATIONSHIP);
                    break;


                case "RA_WELL_SERV_METRIC_CODE":
                    type = typeof(RA_WELL_SERV_METRIC_CODE);
                    break;


                case "RA_WELL_SERVICE_METRIC":
                    type = typeof(RA_WELL_SERVICE_METRIC);
                    break;


                case "RA_WELL_SET_ROLE":
                    type = typeof(RA_WELL_SET_ROLE);
                    break;


                case "RA_WELL_SET_TYPE":
                    type = typeof(RA_WELL_SET_TYPE);
                    break;


                case "RA_WELL_SF_USE_TYPE":
                    type = typeof(RA_WELL_SF_USE_TYPE);
                    break;


                case "RA_WELL_STATUS":
                    type = typeof(RA_WELL_STATUS);
                    break;


                case "RA_WELL_STATUS_QUAL":
                    type = typeof(RA_WELL_STATUS_QUAL);
                    break;


                case "RA_WELL_STATUS_QUAL_VALUE":
                    type = typeof(RA_WELL_STATUS_QUAL_VALUE);
                    break;


                case "RA_WELL_STATUS_SYMBOL":
                    type = typeof(RA_WELL_STATUS_SYMBOL);
                    break;


                case "RA_WELL_STATUS_TYPE":
                    type = typeof(RA_WELL_STATUS_TYPE);
                    break;


                case "RA_WELL_STATUS_XREF":
                    type = typeof(RA_WELL_STATUS_XREF);
                    break;


                case "RA_WELL_TEST_TYPE":
                    type = typeof(RA_WELL_TEST_TYPE);
                    break;


                case "RA_WELL_XREF_TYPE":
                    type = typeof(RA_WELL_XREF_TYPE);
                    break;


                case "RA_WELL_ZONE_INT_VALUE":
                    type = typeof(RA_WELL_ZONE_INT_VALUE);
                    break;


                case "RA_WIND_STRENGTH":
                    type = typeof(RA_WIND_STRENGTH);
                    break;


                case "RA_WO_BA_ROLE":
                    type = typeof(RA_WO_BA_ROLE);
                    break;


                case "RA_WO_COMPONENT_TYPE":
                    type = typeof(RA_WO_COMPONENT_TYPE);
                    break;


                case "RA_WO_CONDITION":
                    type = typeof(RA_WO_CONDITION);
                    break;


                case "RA_WO_DELIVERY_TYPE":
                    type = typeof(RA_WO_DELIVERY_TYPE);
                    break;


                case "RA_WO_INSTRUCTION":
                    type = typeof(RA_WO_INSTRUCTION);
                    break;


                case "RA_WO_TYPE":
                    type = typeof(RA_WO_TYPE);
                    break;


                case "RA_WO_XREF_TYPE":
                    type = typeof(RA_WO_XREF_TYPE);
                    break;


                case "RA_WORK_BID_TYPE":
                    type = typeof(RA_WORK_BID_TYPE);
                    break;


                case "RATE_AREA":
                    type = typeof(RATE_AREA);
                    break;


                case "RATE_SCHED_DETAIL":
                    type = typeof(RATE_SCHED_DETAIL);
                    break;


                case "RATE_SCHEDULE":
                    type = typeof(RATE_SCHEDULE);
                    break;


                case "RATE_SCHEDULE_COMPONENT":
                    type = typeof(RATE_SCHEDULE_COMPONENT);
                    break;


                case "RATE_SCHEDULE_XREF":
                    type = typeof(RATE_SCHEDULE_XREF);
                    break;


                case "REPORT_HIER":
                    type = typeof(REPORT_HIER);
                    break;


                case "REPORT_HIER_ALIAS":
                    type = typeof(REPORT_HIER_ALIAS);
                    break;


                case "REPORT_HIER_DESC":
                    type = typeof(REPORT_HIER_DESC);
                    break;


                case "REPORT_HIER_LEVEL":
                    type = typeof(REPORT_HIER_LEVEL);
                    break;


                case "REPORT_HIER_TYPE":
                    type = typeof(REPORT_HIER_TYPE);
                    break;


                case "REPORT_HIER_USE":
                    type = typeof(REPORT_HIER_USE);
                    break;


                case "RESENT_CLASS":
                    type = typeof(RESENT_CLASS);
                    break;


                case "RESENT_COMPONENT":
                    type = typeof(RESENT_COMPONENT);
                    break;


                case "RESENT_ECO_RUN":
                    type = typeof(RESENT_ECO_RUN);
                    break;


                case "RESENT_ECO_SCHEDULE":
                    type = typeof(RESENT_ECO_SCHEDULE);
                    break;


                case "RESENT_ECO_VOLUME":
                    type = typeof(RESENT_ECO_VOLUME);
                    break;


                case "RESENT_PROD_PROPERTY":
                    type = typeof(RESENT_PROD_PROPERTY);
                    break;


                case "RESENT_PRODUCT":
                    type = typeof(RESENT_PRODUCT);
                    break;


                case "RESENT_REVISION_CAT":
                    type = typeof(RESENT_REVISION_CAT);
                    break;


                case "RESENT_VOL_REGIME":
                    type = typeof(RESENT_VOL_REGIME);
                    break;


                case "RESENT_VOL_REVISION":
                    type = typeof(RESENT_VOL_REVISION);
                    break;


                case "RESENT_VOL_SUMMARY":
                    type = typeof(RESENT_VOL_SUMMARY);
                    break;


                case "RESENT_XREF":
                    type = typeof(RESENT_XREF);
                    break;


                case "RESERVE_CLASS":
                    type = typeof(RESERVE_CLASS);
                    break;


                case "RESERVE_CLASS_CALC":
                    type = typeof(RESERVE_CLASS_CALC);
                    break;


                case "RESERVE_CLASS_FORMULA":
                    type = typeof(RESERVE_CLASS_FORMULA);
                    break;


                case "RESERVE_ENTITY":
                    type = typeof(RESERVE_ENTITY);
                    break;


                case "REST_ACTIVITY":
                    type = typeof(REST_ACTIVITY);
                    break;


                case "REST_CLASS":
                    type = typeof(REST_CLASS);
                    break;


                case "REST_CONTACT":
                    type = typeof(REST_CONTACT);
                    break;


                case "REST_REMARK":
                    type = typeof(REST_REMARK);
                    break;


                case "REST_TEXT":
                    type = typeof(REST_TEXT);
                    break;


                case "RESTRICTION":
                    type = typeof(RESTRICTION);
                    break;


                case "RM_AUX_CHANNEL":
                    type = typeof(RM_AUX_CHANNEL);
                    break;


                case "RM_CIRC_PROCESS":
                    type = typeof(RM_CIRC_PROCESS);
                    break;


                case "RM_CIRCULATION":
                    type = typeof(RM_CIRCULATION);
                    break;


                case "RM_COMPOSITE":
                    type = typeof(RM_COMPOSITE);
                    break;


                case "RM_COPY_RECORD":
                    type = typeof(RM_COPY_RECORD);
                    break;


                case "RM_CREATOR":
                    type = typeof(RM_CREATOR);
                    break;


                case "RM_CUSTODY":
                    type = typeof(RM_CUSTODY);
                    break;


                case "RM_DATA_CONTENT":
                    type = typeof(RM_DATA_CONTENT);
                    break;


                case "RM_DATA_STORE":
                    type = typeof(RM_DATA_STORE);
                    break;


                case "RM_DATA_STORE_HIER":
                    type = typeof(RM_DATA_STORE_HIER);
                    break;


                case "RM_DATA_STORE_HIER_LEVEL":
                    type = typeof(RM_DATA_STORE_HIER_LEVEL);
                    break;


                case "RM_DATA_STORE_ITEM":
                    type = typeof(RM_DATA_STORE_ITEM);
                    break;


                case "RM_DATA_STORE_MEDIA":
                    type = typeof(RM_DATA_STORE_MEDIA);
                    break;


                case "RM_DATA_STORE_STRUCTURE":
                    type = typeof(RM_DATA_STORE_STRUCTURE);
                    break;


                case "RM_DECRYPT_KEY":
                    type = typeof(RM_DECRYPT_KEY);
                    break;


                case "RM_DOCUMENT":
                    type = typeof(RM_DOCUMENT);
                    break;


                case "RM_ENCODING":
                    type = typeof(RM_ENCODING);
                    break;


                case "RM_EQUIPMENT":
                    type = typeof(RM_EQUIPMENT);
                    break;


                case "RM_FILE_CONTENT":
                    type = typeof(RM_FILE_CONTENT);
                    break;


                case "RM_FOSSIL":
                    type = typeof(RM_FOSSIL);
                    break;


                case "RM_IMAGE_COMP":
                    type = typeof(RM_IMAGE_COMP);
                    break;


                case "RM_IMAGE_LOC":
                    type = typeof(RM_IMAGE_LOC);
                    break;


                case "RM_IMAGE_SECT":
                    type = typeof(RM_IMAGE_SECT);
                    break;


                case "RM_INFO_COORD_QUALITY":
                    type = typeof(RM_INFO_COORD_QUALITY);
                    break;


                case "RM_INFO_DATA_QUALITY":
                    type = typeof(RM_INFO_DATA_QUALITY);
                    break;


                case "RM_INFO_ITEM_ALIAS":
                    type = typeof(RM_INFO_ITEM_ALIAS);
                    break;


                case "RM_INFO_ITEM_BA":
                    type = typeof(RM_INFO_ITEM_BA);
                    break;


                case "RM_INFO_ITEM_CONTENT":
                    type = typeof(RM_INFO_ITEM_CONTENT);
                    break;


                case "RM_INFO_ITEM_DESC":
                    type = typeof(RM_INFO_ITEM_DESC);
                    break;


                case "RM_INFO_ITEM_GROUP":
                    type = typeof(RM_INFO_ITEM_GROUP);
                    break;


                case "RM_INFO_ITEM_MAINT":
                    type = typeof(RM_INFO_ITEM_MAINT);
                    break;


                case "RM_INFO_ITEM_ORIGIN":
                    type = typeof(RM_INFO_ITEM_ORIGIN);
                    break;


                case "RM_INFO_ITEM_STATUS":
                    type = typeof(RM_INFO_ITEM_STATUS);
                    break;


                case "RM_INFORMATION_ITEM":
                    type = typeof(RM_INFORMATION_ITEM);
                    break;


                case "RM_KEYWORD":
                    type = typeof(RM_KEYWORD);
                    break;


                case "RM_LITH_SAMPLE":
                    type = typeof(RM_LITH_SAMPLE);
                    break;


                case "RM_MAP":
                    type = typeof(RM_MAP);
                    break;


                case "RM_PHYS_ITEM_CONDITION":
                    type = typeof(RM_PHYS_ITEM_CONDITION);
                    break;


                case "RM_PHYS_ITEM_GROUP":
                    type = typeof(RM_PHYS_ITEM_GROUP);
                    break;


                case "RM_PHYS_ITEM_MAINT":
                    type = typeof(RM_PHYS_ITEM_MAINT);
                    break;


                case "RM_PHYS_ITEM_ORIGIN":
                    type = typeof(RM_PHYS_ITEM_ORIGIN);
                    break;


                case "RM_PHYS_ITEM_STORE":
                    type = typeof(RM_PHYS_ITEM_STORE);
                    break;


                case "RM_PHYSICAL_ITEM":
                    type = typeof(RM_PHYSICAL_ITEM);
                    break;


                case "RM_SEIS_TRACE":
                    type = typeof(RM_SEIS_TRACE);
                    break;


                case "RM_SPATIAL_DATASET":
                    type = typeof(RM_SPATIAL_DATASET);
                    break;


                case "RM_THESAURUS":
                    type = typeof(RM_THESAURUS);
                    break;


                case "RM_THESAURUS_GLOSSARY":
                    type = typeof(RM_THESAURUS_GLOSSARY);
                    break;


                case "RM_THESAURUS_WORD":
                    type = typeof(RM_THESAURUS_WORD);
                    break;


                case "RM_THESAURUS_WORD_XREF":
                    type = typeof(RM_THESAURUS_WORD_XREF);
                    break;


                case "RM_TRACE_HEADER":
                    type = typeof(RM_TRACE_HEADER);
                    break;


                case "RM_WELL_LOG":
                    type = typeof(RM_WELL_LOG);
                    break;


                case "SAMPLE":
                    type = typeof(SAMPLE);
                    break;


                case "SAMPLE_ALIAS":
                    type = typeof(SAMPLE_ALIAS);
                    break;


                case "SAMPLE_COMPONENT":
                    type = typeof(SAMPLE_COMPONENT);
                    break;


                case "SAMPLE_DESC_OTHER":
                    type = typeof(SAMPLE_DESC_OTHER);
                    break;


                case "SAMPLE_LITH_DESC":
                    type = typeof(SAMPLE_LITH_DESC);
                    break;


                case "SAMPLE_ORIGIN":
                    type = typeof(SAMPLE_ORIGIN);
                    break;


                case "SEIS_3D":
                    type = typeof(SEIS_3D);
                    break;


                case "SEIS_ACQTN_DESIGN":
                    type = typeof(SEIS_ACQTN_DESIGN);
                    break;


                case "SEIS_ACQTN_SPECTRUM":
                    type = typeof(SEIS_ACQTN_SPECTRUM);
                    break;


                case "SEIS_ACQTN_SURVEY":
                    type = typeof(SEIS_ACQTN_SURVEY);
                    break;


                case "SEIS_ACTIVITY":
                    type = typeof(SEIS_ACTIVITY);
                    break;


                case "SEIS_ALIAS":
                    type = typeof(SEIS_ALIAS);
                    break;


                case "SEIS_BA_SERVICE":
                    type = typeof(SEIS_BA_SERVICE);
                    break;


                case "SEIS_BIN_GRID":
                    type = typeof(SEIS_BIN_GRID);
                    break;


                case "SEIS_BIN_ORIGIN":
                    type = typeof(SEIS_BIN_ORIGIN);
                    break;


                case "SEIS_BIN_OUTLINE":
                    type = typeof(SEIS_BIN_OUTLINE);
                    break;


                case "SEIS_BIN_POINT":
                    type = typeof(SEIS_BIN_POINT);
                    break;


                case "SEIS_BIN_POINT_TRACE":
                    type = typeof(SEIS_BIN_POINT_TRACE);
                    break;


                case "SEIS_BIN_POINT_VERSION":
                    type = typeof(SEIS_BIN_POINT_VERSION);
                    break;


                case "SEIS_CHANNEL":
                    type = typeof(SEIS_CHANNEL);
                    break;


                case "SEIS_GROUP_COMP":
                    type = typeof(SEIS_GROUP_COMP);
                    break;


                case "SEIS_INSP_COMPONENT":
                    type = typeof(SEIS_INSP_COMPONENT);
                    break;


                case "SEIS_INSPECTION":
                    type = typeof(SEIS_INSPECTION);
                    break;


                case "SEIS_INTERP_COMP":
                    type = typeof(SEIS_INTERP_COMP);
                    break;


                case "SEIS_INTERP_LOAD":
                    type = typeof(SEIS_INTERP_LOAD);
                    break;


                case "SEIS_INTERP_LOAD_PARM":
                    type = typeof(SEIS_INTERP_LOAD_PARM);
                    break;


                case "SEIS_INTERP_SET":
                    type = typeof(SEIS_INTERP_SET);
                    break;


                case "SEIS_INTERP_SURFACE":
                    type = typeof(SEIS_INTERP_SURFACE);
                    break;


                case "SEIS_LICENSE":
                    type = typeof(SEIS_LICENSE);
                    break;


                case "SEIS_LICENSE_ALIAS":
                    type = typeof(SEIS_LICENSE_ALIAS);
                    break;


                case "SEIS_LICENSE_AREA":
                    type = typeof(SEIS_LICENSE_AREA);
                    break;


                case "SEIS_LICENSE_COND":
                    type = typeof(SEIS_LICENSE_COND);
                    break;


                case "SEIS_LICENSE_REMARK":
                    type = typeof(SEIS_LICENSE_REMARK);
                    break;


                case "SEIS_LICENSE_STATUS":
                    type = typeof(SEIS_LICENSE_STATUS);
                    break;


                case "SEIS_LICENSE_TYPE":
                    type = typeof(SEIS_LICENSE_TYPE);
                    break;


                case "SEIS_LICENSE_VIOLATION":
                    type = typeof(SEIS_LICENSE_VIOLATION);
                    break;


                case "SEIS_LINE":
                    type = typeof(SEIS_LINE);
                    break;


                case "SEIS_PATCH":
                    type = typeof(SEIS_PATCH);
                    break;


                case "SEIS_PATCH_DESC":
                    type = typeof(SEIS_PATCH_DESC);
                    break;


                case "SEIS_PICK":
                    type = typeof(SEIS_PICK);
                    break;


                case "SEIS_POINT":
                    type = typeof(SEIS_POINT);
                    break;


                case "SEIS_POINT_FLOW":
                    type = typeof(SEIS_POINT_FLOW);
                    break;


                case "SEIS_POINT_FLOW_DESC":
                    type = typeof(SEIS_POINT_FLOW_DESC);
                    break;


                case "SEIS_POINT_SUMMARY":
                    type = typeof(SEIS_POINT_SUMMARY);
                    break;


                case "SEIS_POINT_VERSION":
                    type = typeof(SEIS_POINT_VERSION);
                    break;


                case "SEIS_PROC_COMPONENT":
                    type = typeof(SEIS_PROC_COMPONENT);
                    break;


                case "SEIS_PROC_PARM":
                    type = typeof(SEIS_PROC_PARM);
                    break;


                case "SEIS_PROC_PLAN":
                    type = typeof(SEIS_PROC_PLAN);
                    break;


                case "SEIS_PROC_PLAN_PARM":
                    type = typeof(SEIS_PROC_PLAN_PARM);
                    break;


                case "SEIS_PROC_PLAN_STEP":
                    type = typeof(SEIS_PROC_PLAN_STEP);
                    break;


                case "SEIS_PROC_SET":
                    type = typeof(SEIS_PROC_SET);
                    break;


                case "SEIS_PROC_STEP":
                    type = typeof(SEIS_PROC_STEP);
                    break;


                case "SEIS_PROC_STEP_COMPONENT":
                    type = typeof(SEIS_PROC_STEP_COMPONENT);
                    break;


                case "SEIS_RECORD":
                    type = typeof(SEIS_RECORD);
                    break;


                case "SEIS_RECVR_MAKE":
                    type = typeof(SEIS_RECVR_MAKE);
                    break;


                case "SEIS_RECVR_SETUP":
                    type = typeof(SEIS_RECVR_SETUP);
                    break;


                case "SEIS_SEGMENT":
                    type = typeof(SEIS_SEGMENT);
                    break;


                case "SEIS_SET":
                    type = typeof(SEIS_SET);
                    break;


                case "SEIS_SET_AREA":
                    type = typeof(SEIS_SET_AREA);
                    break;


                case "SEIS_SET_AUTHORIZE":
                    type = typeof(SEIS_SET_AUTHORIZE);
                    break;


                case "SEIS_SET_COMPONENT":
                    type = typeof(SEIS_SET_COMPONENT);
                    break;


                case "SEIS_SET_JURISDICTION":
                    type = typeof(SEIS_SET_JURISDICTION);
                    break;


                case "SEIS_SET_PLAN":
                    type = typeof(SEIS_SET_PLAN);
                    break;


                case "SEIS_SET_STATUS":
                    type = typeof(SEIS_SET_STATUS);
                    break;


                case "SEIS_SP_SURVEY":
                    type = typeof(SEIS_SP_SURVEY);
                    break;


                case "SEIS_STREAMER":
                    type = typeof(SEIS_STREAMER);
                    break;


                case "SEIS_STREAMER_BUILD":
                    type = typeof(SEIS_STREAMER_BUILD);
                    break;


                case "SEIS_STREAMER_COMP":
                    type = typeof(SEIS_STREAMER_COMP);
                    break;


                case "SEIS_TRANS_COMPONENT":
                    type = typeof(SEIS_TRANS_COMPONENT);
                    break;


                case "SEIS_TRANSACTION":
                    type = typeof(SEIS_TRANSACTION);
                    break;


                case "SEIS_VELOCITY":
                    type = typeof(SEIS_VELOCITY);
                    break;


                case "SEIS_VELOCITY_INTERVAL":
                    type = typeof(SEIS_VELOCITY_INTERVAL);
                    break;


                case "SEIS_VELOCITY_VOLUME":
                    type = typeof(SEIS_VELOCITY_VOLUME);
                    break;


                case "SEIS_VESSEL":
                    type = typeof(SEIS_VESSEL);
                    break;


                case "SEIS_WELL":
                    type = typeof(SEIS_WELL);
                    break;


                case "SF_AIRCRAFT":
                    type = typeof(SF_AIRCRAFT);
                    break;


                case "SF_AIRSTRIP":
                    type = typeof(SF_AIRSTRIP);
                    break;


                case "SF_ALIAS":
                    type = typeof(SF_ALIAS);
                    break;


                case "SF_AREA":
                    type = typeof(SF_AREA);
                    break;


                case "SF_BA_CREW":
                    type = typeof(SF_BA_CREW);
                    break;


                case "SF_BA_SERVICE":
                    type = typeof(SF_BA_SERVICE);
                    break;


                case "SF_BRIDGE":
                    type = typeof(SF_BRIDGE);
                    break;


                case "SF_COMPONENT":
                    type = typeof(SF_COMPONENT);
                    break;


                case "SF_DESCRIPTION":
                    type = typeof(SF_DESCRIPTION);
                    break;


                case "SF_DISPOSAL":
                    type = typeof(SF_DISPOSAL);
                    break;


                case "SF_DOCK":
                    type = typeof(SF_DOCK);
                    break;


                case "SF_ELECTRIC":
                    type = typeof(SF_ELECTRIC);
                    break;


                case "SF_EQUIPMENT":
                    type = typeof(SF_EQUIPMENT);
                    break;


                case "SF_HABITAT":
                    type = typeof(SF_HABITAT);
                    break;


                case "SF_LANDING":
                    type = typeof(SF_LANDING);
                    break;


                case "SF_MAINTAIN":
                    type = typeof(SF_MAINTAIN);
                    break;


                case "SF_MONUMENT":
                    type = typeof(SF_MONUMENT);
                    break;


                case "SF_OTHER":
                    type = typeof(SF_OTHER);
                    break;


                case "SF_PAD":
                    type = typeof(SF_PAD);
                    break;


                case "SF_PLATFORM":
                    type = typeof(SF_PLATFORM);
                    break;


                case "SF_PORT":
                    type = typeof(SF_PORT);
                    break;


                case "SF_RAILWAY":
                    type = typeof(SF_RAILWAY);
                    break;


                case "SF_REST_REMARK":
                    type = typeof(SF_REST_REMARK);
                    break;


                case "SF_RESTRICTION":
                    type = typeof(SF_RESTRICTION);
                    break;


                case "SF_RIG":
                    type = typeof(SF_RIG);
                    break;


                case "SF_RIG_BOP":
                    type = typeof(SF_RIG_BOP);
                    break;


                case "SF_RIG_GENERATOR":
                    type = typeof(SF_RIG_GENERATOR);
                    break;


                case "SF_RIG_OVERHEAD_EQUIP":
                    type = typeof(SF_RIG_OVERHEAD_EQUIP);
                    break;


                case "SF_RIG_PUMP":
                    type = typeof(SF_RIG_PUMP);
                    break;


                case "SF_RIG_SHAKER":
                    type = typeof(SF_RIG_SHAKER);
                    break;


                case "SF_ROAD":
                    type = typeof(SF_ROAD);
                    break;


                case "SF_STATUS":
                    type = typeof(SF_STATUS);
                    break;


                case "SF_SUPPORT_FACILITY":
                    type = typeof(SF_SUPPORT_FACILITY);
                    break;


                case "SF_TOWER":
                    type = typeof(SF_TOWER);
                    break;


                case "SF_VEHICLE":
                    type = typeof(SF_VEHICLE);
                    break;


                case "SF_VESSEL":
                    type = typeof(SF_VESSEL);
                    break;


                case "SF_WASTE":
                    type = typeof(SF_WASTE);
                    break;


                case "SF_WASTE_DISPOSAL":
                    type = typeof(SF_WASTE_DISPOSAL);
                    break;


                case "SF_XREF":
                    type = typeof(SF_XREF);
                    break;


                case "SOURCE_DOC_AUTHOR":
                    type = typeof(SOURCE_DOC_AUTHOR);
                    break;


                case "SOURCE_DOC_BIBLIO":
                    type = typeof(SOURCE_DOC_BIBLIO);
                    break;


                case "SOURCE_DOCUMENT":
                    type = typeof(SOURCE_DOCUMENT);
                    break;


                case "SP_BOUNDARY":
                    type = typeof(SP_BOUNDARY);
                    break;


                case "SP_BOUNDARY_VERSION":
                    type = typeof(SP_BOUNDARY_VERSION);
                    break;


                case "SP_COMPONENT":
                    type = typeof(SP_COMPONENT);
                    break;


                case "SP_DESC_TEXT":
                    type = typeof(SP_DESC_TEXT);
                    break;


                case "SP_DESC_XREF":
                    type = typeof(SP_DESC_XREF);
                    break;


                case "SP_LINE":
                    type = typeof(SP_LINE);
                    break;


                case "SP_LINE_POINT":
                    type = typeof(SP_LINE_POINT);
                    break;


                case "SP_LINE_POINT_VERSION":
                    type = typeof(SP_LINE_POINT_VERSION);
                    break;


                case "SP_MINERAL_ZONE":
                    type = typeof(SP_MINERAL_ZONE);
                    break;


                case "SP_PARCEL":
                    type = typeof(SP_PARCEL);
                    break;


                case "SP_PARCEL_AREA":
                    type = typeof(SP_PARCEL_AREA);
                    break;


                case "SP_PARCEL_CARTER":
                    type = typeof(SP_PARCEL_CARTER);
                    break;


                case "SP_PARCEL_CONGRESS":
                    type = typeof(SP_PARCEL_CONGRESS);
                    break;


                case "SP_PARCEL_DLS":
                    type = typeof(SP_PARCEL_DLS);
                    break;


                case "SP_PARCEL_DLS_ROAD":
                    type = typeof(SP_PARCEL_DLS_ROAD);
                    break;


                case "SP_PARCEL_FPS":
                    type = typeof(SP_PARCEL_FPS);
                    break;


                case "SP_PARCEL_LIBYA":
                    type = typeof(SP_PARCEL_LIBYA);
                    break;


                case "SP_PARCEL_LOT":
                    type = typeof(SP_PARCEL_LOT);
                    break;


                case "SP_PARCEL_M_B":
                    type = typeof(SP_PARCEL_M_B);
                    break;


                case "SP_PARCEL_NE_LOC":
                    type = typeof(SP_PARCEL_NE_LOC);
                    break;


                case "SP_PARCEL_NORTH_SEA":
                    type = typeof(SP_PARCEL_NORTH_SEA);
                    break;


                case "SP_PARCEL_NTS":
                    type = typeof(SP_PARCEL_NTS);
                    break;


                case "SP_PARCEL_OFFSHORE":
                    type = typeof(SP_PARCEL_OFFSHORE);
                    break;


                case "SP_PARCEL_OHIO":
                    type = typeof(SP_PARCEL_OHIO);
                    break;


                case "SP_PARCEL_PBL":
                    type = typeof(SP_PARCEL_PBL);
                    break;


                case "SP_PARCEL_REMARK":
                    type = typeof(SP_PARCEL_REMARK);
                    break;


                case "SP_PARCEL_TEXAS":
                    type = typeof(SP_PARCEL_TEXAS);
                    break;


                case "SP_POINT":
                    type = typeof(SP_POINT);
                    break;


                case "SP_POINT_VERSION":
                    type = typeof(SP_POINT_VERSION);
                    break;


                case "SP_POLYGON":
                    type = typeof(SP_POLYGON);
                    break;


                case "SP_ZONE_DEFIN_XREF":
                    type = typeof(SP_ZONE_DEFIN_XREF);
                    break;


                case "SP_ZONE_DEFINITION":
                    type = typeof(SP_ZONE_DEFINITION);
                    break;


                case "SP_ZONE_SUBSTANCE":
                    type = typeof(SP_ZONE_SUBSTANCE);
                    break;


                case "SPACING_UNIT":
                    type = typeof(SPACING_UNIT);
                    break;


                case "SPACING_UNIT_INST":
                    type = typeof(SPACING_UNIT_INST);
                    break;


                case "SPATIAL_DESCRIPTION":
                    type = typeof(SPATIAL_DESCRIPTION);
                    break;


                case "STRAT_ACQTN_METHOD":
                    type = typeof(STRAT_ACQTN_METHOD);
                    break;


                case "STRAT_ALIAS":
                    type = typeof(STRAT_ALIAS);
                    break;


                case "STRAT_COL_UNIT_AGE":
                    type = typeof(STRAT_COL_UNIT_AGE);
                    break;


                case "STRAT_COLUMN":
                    type = typeof(STRAT_COLUMN);
                    break;


                case "STRAT_COLUMN_ACQTN":
                    type = typeof(STRAT_COLUMN_ACQTN);
                    break;


                case "STRAT_COLUMN_UNIT":
                    type = typeof(STRAT_COLUMN_UNIT);
                    break;


                case "STRAT_COLUMN_XREF":
                    type = typeof(STRAT_COLUMN_XREF);
                    break;


                case "STRAT_EQUIVALENCE":
                    type = typeof(STRAT_EQUIVALENCE);
                    break;


                case "STRAT_FIELD_ACQTN":
                    type = typeof(STRAT_FIELD_ACQTN);
                    break;


                case "STRAT_FIELD_NODE":
                    type = typeof(STRAT_FIELD_NODE);
                    break;


                case "STRAT_FIELD_SECTION":
                    type = typeof(STRAT_FIELD_SECTION);
                    break;


                case "STRAT_FIELD_STATION":
                    type = typeof(STRAT_FIELD_STATION);
                    break;


                case "STRAT_FLD_INTERP_AGE":
                    type = typeof(STRAT_FLD_INTERP_AGE);
                    break;


                case "STRAT_HIERARCHY":
                    type = typeof(STRAT_HIERARCHY);
                    break;


                case "STRAT_HIERARCHY_DESC":
                    type = typeof(STRAT_HIERARCHY_DESC);
                    break;


                case "STRAT_INTERP_CORR":
                    type = typeof(STRAT_INTERP_CORR);
                    break;


                case "STRAT_NAME_SET":
                    type = typeof(STRAT_NAME_SET);
                    break;


                case "STRAT_NAME_SET_XREF":
                    type = typeof(STRAT_NAME_SET_XREF);
                    break;


                case "STRAT_NODE_VERSION":
                    type = typeof(STRAT_NODE_VERSION);
                    break;


                case "STRAT_TOPO_RELATION":
                    type = typeof(STRAT_TOPO_RELATION);
                    break;


                case "STRAT_UNIT":
                    type = typeof(STRAT_UNIT);
                    break;


                case "STRAT_UNIT_AGE":
                    type = typeof(STRAT_UNIT_AGE);
                    break;


                case "STRAT_UNIT_COMPONENT":
                    type = typeof(STRAT_UNIT_COMPONENT);
                    break;


                case "STRAT_UNIT_DESCRIPTION":
                    type = typeof(STRAT_UNIT_DESCRIPTION);
                    break;


                case "STRAT_WELL_ACQTN":
                    type = typeof(STRAT_WELL_ACQTN);
                    break;


                case "STRAT_WELL_INTERP_AGE":
                    type = typeof(STRAT_WELL_INTERP_AGE);
                    break;


                case "STRAT_WELL_SECTION":
                    type = typeof(STRAT_WELL_SECTION);
                    break;


                case "SUBSTANCE":
                    type = typeof(SUBSTANCE);
                    break;


                case "SUBSTANCE_ALIAS":
                    type = typeof(SUBSTANCE_ALIAS);
                    break;


                case "SUBSTANCE_BA":
                    type = typeof(SUBSTANCE_BA);
                    break;


                case "SUBSTANCE_BEHAVIOR":
                    type = typeof(SUBSTANCE_BEHAVIOR);
                    break;


                case "SUBSTANCE_COMPOSITION":
                    type = typeof(SUBSTANCE_COMPOSITION);
                    break;


                case "SUBSTANCE_PROPERTY_DETAIL":
                    type = typeof(SUBSTANCE_PROPERTY_DETAIL);
                    break;


                case "SUBSTANCE_USE":
                    type = typeof(SUBSTANCE_USE);
                    break;


                case "SUBSTANCE_XREF":
                    type = typeof(SUBSTANCE_XREF);
                    break;


                case "WELL":
                    type = typeof(WELL);
                    break;


                case "WELL_ACTIVITY":
                    type = typeof(WELL_ACTIVITY);
                    break;


                case "WELL_ACTIVITY_CAUSE":
                    type = typeof(WELL_ACTIVITY_CAUSE);
                    break;


                case "WELL_ACTIVITY_COMPONENT":
                    type = typeof(WELL_ACTIVITY_COMPONENT);
                    break;


                case "WELL_ACTIVITY_DURATION":
                    type = typeof(WELL_ACTIVITY_DURATION);
                    break;


                case "WELL_ACTIVITY_SET":
                    type = typeof(WELL_ACTIVITY_SET);
                    break;


                case "WELL_ACTIVITY_TYPE":
                    type = typeof(WELL_ACTIVITY_TYPE);
                    break;


                case "WELL_ACTIVITY_TYPE_ALIAS":
                    type = typeof(WELL_ACTIVITY_TYPE_ALIAS);
                    break;


                case "WELL_ACTIVITY_TYPE_EQUIV":
                    type = typeof(WELL_ACTIVITY_TYPE_EQUIV);
                    break;


                case "WELL_AIR_DRILL":
                    type = typeof(WELL_AIR_DRILL);
                    break;


                case "WELL_AIR_DRILL_INTERVAL":
                    type = typeof(WELL_AIR_DRILL_INTERVAL);
                    break;


                case "WELL_AIR_DRILL_PERIOD":
                    type = typeof(WELL_AIR_DRILL_PERIOD);
                    break;


                case "WELL_ALIAS":
                    type = typeof(WELL_ALIAS);
                    break;


                case "WELL_AREA":
                    type = typeof(WELL_AREA);
                    break;


                case "WELL_BA_SERVICE":
                    type = typeof(WELL_BA_SERVICE);
                    break;


                case "WELL_CEMENT":
                    type = typeof(WELL_CEMENT);
                    break;


                case "WELL_COMPLETION":
                    type = typeof(WELL_COMPLETION);
                    break;


                case "WELL_COMPONENT":
                    type = typeof(WELL_COMPONENT);
                    break;


                case "WELL_CORE":
                    type = typeof(WELL_CORE);
                    break;


                case "WELL_CORE_ANAL_METHOD":
                    type = typeof(WELL_CORE_ANAL_METHOD);
                    break;


                case "WELL_CORE_ANAL_REMARK":
                    type = typeof(WELL_CORE_ANAL_REMARK);
                    break;


                case "WELL_CORE_ANALYSIS":
                    type = typeof(WELL_CORE_ANALYSIS);
                    break;


                case "WELL_CORE_DESCRIPTION":
                    type = typeof(WELL_CORE_DESCRIPTION);
                    break;


                case "WELL_CORE_REMARK":
                    type = typeof(WELL_CORE_REMARK);
                    break;


                case "WELL_CORE_SAMPLE_ANAL":
                    type = typeof(WELL_CORE_SAMPLE_ANAL);
                    break;


                case "WELL_CORE_SAMPLE_DESC":
                    type = typeof(WELL_CORE_SAMPLE_DESC);
                    break;


                case "WELL_CORE_SAMPLE_RMK":
                    type = typeof(WELL_CORE_SAMPLE_RMK);
                    break;


                case "WELL_CORE_SHIFT":
                    type = typeof(WELL_CORE_SHIFT);
                    break;


                case "WELL_CORE_STRAT_UNIT":
                    type = typeof(WELL_CORE_STRAT_UNIT);
                    break;


                case "WELL_DIR_SRVY":
                    type = typeof(WELL_DIR_SRVY);
                    break;


                case "WELL_DIR_SRVY_COMPOSITE":
                    type = typeof(WELL_DIR_SRVY_COMPOSITE);
                    break;


                case "WELL_DIR_SRVY_ST_VERSION":
                    type = typeof(WELL_DIR_SRVY_ST_VERSION);
                    break;


                case "WELL_DIR_SRVY_STATION":
                    type = typeof(WELL_DIR_SRVY_STATION);
                    break;


                case "WELL_DIR_SRVY_VERSION":
                    type = typeof(WELL_DIR_SRVY_VERSION);
                    break;


                case "WELL_DRILL_ADD_INV":
                    type = typeof(WELL_DRILL_ADD_INV);
                    break;


                case "WELL_DRILL_ASSEMBLY":
                    type = typeof(WELL_DRILL_ASSEMBLY);
                    break;


                case "WELL_DRILL_ASSEMBLY_COMP":
                    type = typeof(WELL_DRILL_ASSEMBLY_COMP);
                    break;


                case "WELL_DRILL_ASSEMBLY_PER":
                    type = typeof(WELL_DRILL_ASSEMBLY_PER);
                    break;


                case "WELL_DRILL_BIT_CONDITION":
                    type = typeof(WELL_DRILL_BIT_CONDITION);
                    break;


                case "WELL_DRILL_BIT_INTERVAL":
                    type = typeof(WELL_DRILL_BIT_INTERVAL);
                    break;


                case "WELL_DRILL_BIT_JET":
                    type = typeof(WELL_DRILL_BIT_JET);
                    break;


                case "WELL_DRILL_BIT_PERIOD":
                    type = typeof(WELL_DRILL_BIT_PERIOD);
                    break;


                case "WELL_DRILL_CHECK":
                    type = typeof(WELL_DRILL_CHECK);
                    break;


                case "WELL_DRILL_CHECK_SET":
                    type = typeof(WELL_DRILL_CHECK_SET);
                    break;


                case "WELL_DRILL_CHECK_TYPE":
                    type = typeof(WELL_DRILL_CHECK_TYPE);
                    break;


                case "WELL_DRILL_EQUIPMENT":
                    type = typeof(WELL_DRILL_EQUIPMENT);
                    break;


                case "WELL_DRILL_INT_DETAIL":
                    type = typeof(WELL_DRILL_INT_DETAIL);
                    break;


                case "WELL_DRILL_MUD_ADDITIVE":
                    type = typeof(WELL_DRILL_MUD_ADDITIVE);
                    break;


                case "WELL_DRILL_MUD_INTRVL":
                    type = typeof(WELL_DRILL_MUD_INTRVL);
                    break;


                case "WELL_DRILL_MUD_WEIGHT":
                    type = typeof(WELL_DRILL_MUD_WEIGHT);
                    break;


                case "WELL_DRILL_PERIOD":
                    type = typeof(WELL_DRILL_PERIOD);
                    break;


                case "WELL_DRILL_PERIOD_CREW":
                    type = typeof(WELL_DRILL_PERIOD_CREW);
                    break;


                case "WELL_DRILL_PERIOD_EQUIP":
                    type = typeof(WELL_DRILL_PERIOD_EQUIP);
                    break;


                case "WELL_DRILL_PERIOD_INV":
                    type = typeof(WELL_DRILL_PERIOD_INV);
                    break;


                case "WELL_DRILL_PERIOD_SERV":
                    type = typeof(WELL_DRILL_PERIOD_SERV);
                    break;


                case "WELL_DRILL_PERIOD_VESSEL":
                    type = typeof(WELL_DRILL_PERIOD_VESSEL);
                    break;


                case "WELL_DRILL_PIPE_INV":
                    type = typeof(WELL_DRILL_PIPE_INV);
                    break;


                case "WELL_DRILL_REMARK":
                    type = typeof(WELL_DRILL_REMARK);
                    break;


                case "WELL_DRILL_REPORT":
                    type = typeof(WELL_DRILL_REPORT);
                    break;


                case "WELL_DRILL_SHAKER":
                    type = typeof(WELL_DRILL_SHAKER);
                    break;


                case "WELL_DRILL_STATISTIC":
                    type = typeof(WELL_DRILL_STATISTIC);
                    break;


                case "WELL_DRILL_WEATHER":
                    type = typeof(WELL_DRILL_WEATHER);
                    break;


                case "WELL_EQUIPMENT":
                    type = typeof(WELL_EQUIPMENT);
                    break;


                case "WELL_FACILITY":
                    type = typeof(WELL_FACILITY);
                    break;


                case "WELL_HORIZ_DRILL":
                    type = typeof(WELL_HORIZ_DRILL);
                    break;


                case "WELL_HORIZ_DRILL_KOP":
                    type = typeof(WELL_HORIZ_DRILL_KOP);
                    break;


                case "WELL_HORIZ_DRILL_POE":
                    type = typeof(WELL_HORIZ_DRILL_POE);
                    break;


                case "WELL_HORIZ_DRILL_SPOKE":
                    type = typeof(WELL_HORIZ_DRILL_SPOKE);
                    break;


                case "WELL_LICENSE":
                    type = typeof(WELL_LICENSE);
                    break;


                case "WELL_LICENSE_ALIAS":
                    type = typeof(WELL_LICENSE_ALIAS);
                    break;


                case "WELL_LICENSE_AREA":
                    type = typeof(WELL_LICENSE_AREA);
                    break;


                case "WELL_LICENSE_COND":
                    type = typeof(WELL_LICENSE_COND);
                    break;


                case "WELL_LICENSE_REMARK":
                    type = typeof(WELL_LICENSE_REMARK);
                    break;


                case "WELL_LICENSE_STATUS":
                    type = typeof(WELL_LICENSE_STATUS);
                    break;


                case "WELL_LICENSE_VIOLATION":
                    type = typeof(WELL_LICENSE_VIOLATION);
                    break;


                case "WELL_LOG":
                    type = typeof(WELL_LOG);
                    break;


                case "WELL_LOG_AXIS_COORD":
                    type = typeof(WELL_LOG_AXIS_COORD);
                    break;


                case "WELL_LOG_CLASS":
                    type = typeof(WELL_LOG_CLASS);
                    break;


                case "WELL_LOG_CLS_CRV_CLS":
                    type = typeof(WELL_LOG_CLS_CRV_CLS);
                    break;


                case "WELL_LOG_CRV_CLS_XREF":
                    type = typeof(WELL_LOG_CRV_CLS_XREF);
                    break;


                case "WELL_LOG_CURVE":
                    type = typeof(WELL_LOG_CURVE);
                    break;


                case "WELL_LOG_CURVE_AXIS":
                    type = typeof(WELL_LOG_CURVE_AXIS);
                    break;


                case "WELL_LOG_CURVE_CLASS":
                    type = typeof(WELL_LOG_CURVE_CLASS);
                    break;


                case "WELL_LOG_CURVE_FRAME":
                    type = typeof(WELL_LOG_CURVE_FRAME);
                    break;


                case "WELL_LOG_CURVE_PROC":
                    type = typeof(WELL_LOG_CURVE_PROC);
                    break;


                case "WELL_LOG_CURVE_REMARK":
                    type = typeof(WELL_LOG_CURVE_REMARK);
                    break;


                case "WELL_LOG_CURVE_SCALE":
                    type = typeof(WELL_LOG_CURVE_SCALE);
                    break;


                case "WELL_LOG_CURVE_SPLICE":
                    type = typeof(WELL_LOG_CURVE_SPLICE);
                    break;


                case "WELL_LOG_CURVE_VALUE":
                    type = typeof(WELL_LOG_CURVE_VALUE);
                    break;


                case "WELL_LOG_DGTZ_CURVE":
                    type = typeof(WELL_LOG_DGTZ_CURVE);
                    break;


                case "WELL_LOG_DICT_ALIAS":
                    type = typeof(WELL_LOG_DICT_ALIAS);
                    break;


                case "WELL_LOG_DICT_BA":
                    type = typeof(WELL_LOG_DICT_BA);
                    break;


                case "WELL_LOG_DICT_CRV_CLS":
                    type = typeof(WELL_LOG_DICT_CRV_CLS);
                    break;


                case "WELL_LOG_DICT_CURVE":
                    type = typeof(WELL_LOG_DICT_CURVE);
                    break;


                case "WELL_LOG_DICT_PARM":
                    type = typeof(WELL_LOG_DICT_PARM);
                    break;


                case "WELL_LOG_DICT_PARM_CLS":
                    type = typeof(WELL_LOG_DICT_PARM_CLS);
                    break;


                case "WELL_LOG_DICT_PARM_VAL":
                    type = typeof(WELL_LOG_DICT_PARM_VAL);
                    break;


                case "WELL_LOG_DICT_PROC":
                    type = typeof(WELL_LOG_DICT_PROC);
                    break;


                case "WELL_LOG_DICTIONARY":
                    type = typeof(WELL_LOG_DICTIONARY);
                    break;


                case "WELL_LOG_JOB":
                    type = typeof(WELL_LOG_JOB);
                    break;


                case "WELL_LOG_PARM":
                    type = typeof(WELL_LOG_PARM);
                    break;


                case "WELL_LOG_PARM_ARRAY":
                    type = typeof(WELL_LOG_PARM_ARRAY);
                    break;


                case "WELL_LOG_PARM_CLASS":
                    type = typeof(WELL_LOG_PARM_CLASS);
                    break;


                case "WELL_LOG_PASS":
                    type = typeof(WELL_LOG_PASS);
                    break;


                case "WELL_LOG_REMARK":
                    type = typeof(WELL_LOG_REMARK);
                    break;


                case "WELL_LOG_TRIP":
                    type = typeof(WELL_LOG_TRIP);
                    break;


                case "WELL_LOG_TRIP_REMARK":
                    type = typeof(WELL_LOG_TRIP_REMARK);
                    break;


                case "WELL_MISC_DATA":
                    type = typeof(WELL_MISC_DATA);
                    break;


                case "WELL_MUD_PROPERTY":
                    type = typeof(WELL_MUD_PROPERTY);
                    break;


                case "WELL_MUD_RESISTIVITY":
                    type = typeof(WELL_MUD_RESISTIVITY);
                    break;


                case "WELL_MUD_SAMPLE":
                    type = typeof(WELL_MUD_SAMPLE);
                    break;


                case "WELL_NODE":
                    type = typeof(WELL_NODE);
                    break;


                case "WELL_NODE_AREA":
                    type = typeof(WELL_NODE_AREA);
                    break;


                case "WELL_NODE_M_B":
                    type = typeof(WELL_NODE_M_B);
                    break;


                case "WELL_NODE_STRAT_UNIT":
                    type = typeof(WELL_NODE_STRAT_UNIT);
                    break;


                case "WELL_NODE_VERSION":
                    type = typeof(WELL_NODE_VERSION);
                    break;


                case "WELL_PAYZONE":
                    type = typeof(WELL_PAYZONE);
                    break;


                case "WELL_PERF_REMARK":
                    type = typeof(WELL_PERF_REMARK);
                    break;


                case "WELL_PERFORATION":
                    type = typeof(WELL_PERFORATION);
                    break;


                case "WELL_PERMIT_TYPE":
                    type = typeof(WELL_PERMIT_TYPE);
                    break;


                case "WELL_PLUGBACK":
                    type = typeof(WELL_PLUGBACK);
                    break;


                case "WELL_POROUS_INTERVAL":
                    type = typeof(WELL_POROUS_INTERVAL);
                    break;


                case "WELL_PRESSURE":
                    type = typeof(WELL_PRESSURE);
                    break;


                case "WELL_PRESSURE_AOF":
                    type = typeof(WELL_PRESSURE_AOF);
                    break;


                case "WELL_PRESSURE_AOF_4PT":
                    type = typeof(WELL_PRESSURE_AOF_4PT);
                    break;


                case "WELL_PRESSURE_BH":
                    type = typeof(WELL_PRESSURE_BH);
                    break;


                case "WELL_REMARK":
                    type = typeof(WELL_REMARK);
                    break;


                case "WELL_SET":
                    type = typeof(WELL_SET);
                    break;


                case "WELL_SET_WELL":
                    type = typeof(WELL_SET_WELL);
                    break;


                case "WELL_SHOW":
                    type = typeof(WELL_SHOW);
                    break;


                case "WELL_SHOW_REMARK":
                    type = typeof(WELL_SHOW_REMARK);
                    break;


                case "WELL_SIEVE_ANALYSIS":
                    type = typeof(WELL_SIEVE_ANALYSIS);
                    break;


                case "WELL_SIEVE_SCREEN":
                    type = typeof(WELL_SIEVE_SCREEN);
                    break;


                case "WELL_STATUS":
                    type = typeof(WELL_STATUS);
                    break;


                case "WELL_SUPPORT_FACILITY":
                    type = typeof(WELL_SUPPORT_FACILITY);
                    break;


                case "WELL_TEST":
                    type = typeof(WELL_TEST);
                    break;


                case "WELL_TEST_ANALYSIS":
                    type = typeof(WELL_TEST_ANALYSIS);
                    break;


                case "WELL_TEST_COMPUT_ANAL":
                    type = typeof(WELL_TEST_COMPUT_ANAL);
                    break;


                case "WELL_TEST_CONTAMINANT":
                    type = typeof(WELL_TEST_CONTAMINANT);
                    break;


                case "WELL_TEST_CUSHION":
                    type = typeof(WELL_TEST_CUSHION);
                    break;


                case "WELL_TEST_EQUIPMENT":
                    type = typeof(WELL_TEST_EQUIPMENT);
                    break;


                case "WELL_TEST_FLOW":
                    type = typeof(WELL_TEST_FLOW);
                    break;


                case "WELL_TEST_FLOW_MEAS":
                    type = typeof(WELL_TEST_FLOW_MEAS);
                    break;


                case "WELL_TEST_MUD":
                    type = typeof(WELL_TEST_MUD);
                    break;


                case "WELL_TEST_PERIOD":
                    type = typeof(WELL_TEST_PERIOD);
                    break;


                case "WELL_TEST_PRESS_MEAS":
                    type = typeof(WELL_TEST_PRESS_MEAS);
                    break;


                case "WELL_TEST_PRESSURE":
                    type = typeof(WELL_TEST_PRESSURE);
                    break;


                case "WELL_TEST_RECORDER":
                    type = typeof(WELL_TEST_RECORDER);
                    break;


                case "WELL_TEST_RECOVERY":
                    type = typeof(WELL_TEST_RECOVERY);
                    break;


                case "WELL_TEST_REMARK":
                    type = typeof(WELL_TEST_REMARK);
                    break;


                case "WELL_TEST_SHUTOFF":
                    type = typeof(WELL_TEST_SHUTOFF);
                    break;


                case "WELL_TEST_STRAT_UNIT":
                    type = typeof(WELL_TEST_STRAT_UNIT);
                    break;


                case "WELL_TREATMENT":
                    type = typeof(WELL_TREATMENT);
                    break;


                case "WELL_TUBULAR":
                    type = typeof(WELL_TUBULAR);
                    break;


                case "WELL_VERSION":
                    type = typeof(WELL_VERSION);
                    break;


                case "WELL_VERSION_AREA":
                    type = typeof(WELL_VERSION_AREA);
                    break;


                case "WELL_XREF":
                    type = typeof(WELL_XREF);
                    break;


                case "WELL_ZONE_INTERVAL":
                    type = typeof(WELL_ZONE_INTERVAL);
                    break;


                case "WELL_ZONE_INTRVL_VALUE":
                    type = typeof(WELL_ZONE_INTRVL_VALUE);
                    break;


                case "WORK_ORDER":
                    type = typeof(WORK_ORDER);
                    break;


                case "WORK_ORDER_ALIAS":
                    type = typeof(WORK_ORDER_ALIAS);
                    break;


                case "WORK_ORDER_BA":
                    type = typeof(WORK_ORDER_BA);
                    break;


                case "WORK_ORDER_COMPONENT":
                    type = typeof(WORK_ORDER_COMPONENT);
                    break;


                case "WORK_ORDER_CONDITION":
                    type = typeof(WORK_ORDER_CONDITION);
                    break;


                case "WORK_ORDER_DELIVERY":
                    type = typeof(WORK_ORDER_DELIVERY);
                    break;


                case "WORK_ORDER_DELIVERY_COMP":
                    type = typeof(WORK_ORDER_DELIVERY_COMP);
                    break;


                case "WORK_ORDER_INST_COMP":
                    type = typeof(WORK_ORDER_INST_COMP);
                    break;


                case "WORK_ORDER_INSTRUCTION":
                    type = typeof(WORK_ORDER_INSTRUCTION);
                    break;


                case "WORK_ORDER_XREF":
                    type = typeof(WORK_ORDER_XREF);
                    break;


                case "Z_PRODUCT":
                    type = typeof(Z_PRODUCT);
                    break;


                case "Z_PRODUCT_COMPOSITION":
                    type = typeof(Z_PRODUCT_COMPOSITION);
                    break;


                case "Z_R_OIL_BASE_TYPE":
                    type = typeof(Z_R_OIL_BASE_TYPE);
                    break;


                case "Z_R_OIL_TYPE":
                    type = typeof(Z_R_OIL_TYPE);
                    break;


                case "Z_R_SAMPLE_WATER_TYPE":
                    type = typeof(Z_R_SAMPLE_WATER_TYPE);
                    break;


                case "Z_R_WATER_TYPE":
                    type = typeof(Z_R_WATER_TYPE);
                    break;


                case "ZONE":
                    type = typeof(ZONE);
                    break;




                default:
                    throw new ArgumentException("Invalid type name", nameof(entityname));
            }
            Type genericClass = typeof(UnitofWork<>);
            Type constructedClass = genericClass.MakeGenericType(type);

            return (IUnitofWork<Entity>)Activator.CreateInstance(constructedClass, new object[] { editor, datasourceName, entityname });

        }
    }
}


using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Beep.OilandGas.Branchs;

public static class PPDM39TableMapping
{
    private static readonly Dictionary<string, int> TableToCategoryMap;

    /// <summary>Maps each child table to the unique set of parent tables it references via foreign keys.</summary>
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> TableParents;

    /// <summary>Maps each parent table to the unique set of child tables that foreign-key-reference it.</summary>
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> TableChildren;

    static PPDM39TableMapping()
    {
        TableToCategoryMap = new Dictionary<string, int>
        {
            // ========== 1. Additives ==========
            { "CAT_ADDITIVE", 1 }, { "CAT_ADDITIVE_ALIAS", 1 }, { "CAT_ADDITIVE_ALLOWANCE", 1 },
            { "CAT_ADDITIVE_GROUP", 1 }, { "CAT_ADDITIVE_GROUP_PART", 1 }, { "CAT_ADDITIVE_SPEC", 1 },
            { "CAT_ADDITIVE_TYPE", 1 }, { "CAT_ADDITIVE_XREF", 1 },

            // ========== 2. Applications ==========
            { "APPLICATION", 2 }, { "APPLIC_AREA", 2 }, { "APPLIC_ATTACH", 2 },
            { "APPLIC_ALIAS", 2 }, { "APPLIC_BA", 2 }, { "APPLIC_DESC", 2 },
            { "APPLIC_REMARK", 2 }, { "APPLIC_COMPONENT", 2 },

            // ========== 3. Areas ==========
            { "AREA", 3 }, { "AREA_ALIAS", 3 }, { "AREA_CLASS", 3 }, { "AREA_COMPONENT", 3 },
            { "AREA_CONTAIN", 3 }, { "AREA_DESCRIPTION", 3 }, { "AREA_HIERARCHY", 3 },
            { "AREA_HIER_DETAIL", 3 }, { "AREA_XREF", 3 },

            // ========== 4. Business Associates ==========
            { "BUSINESS_ASSOCIATE", 4 }, { "BA_ALIAS", 4 }, { "BA_AUTHORITY", 4 },
            { "BA_AUTHORITY_COMP", 4 }, { "BA_CONSORTIUM_SERVICE", 4 }, { "BA_COMPONENT", 4 },
            { "BA_DESCRIPTION", 4 }, { "BA_PERMIT", 4 }, { "BA_PREFERENCE", 4 },
            { "BA_PREFERENCE_LEVEL", 4 }, { "BA_ADDRESS", 4 }, { "BA_SERVICE_ADDRESS", 4 },
            { "BA_SERVICE", 4 }, { "BA_CONTACT_INFO", 4 }, { "BA_LICENSE", 4 },
            { "BA_LICENSE_ALIAS", 4 }, { "BA_LICENSE_AREA", 4 }, { "BA_LICENSE_COND", 4 },
            { "BA_LICENSE_COND_CODE", 4 }, { "BA_LICENSE_COND_TYPE", 4 }, { "BA_LICENSE_REMARK", 4 },
            { "BA_LICENSE_STATUS", 4 }, { "BA_LICENSE_TYPE", 4 }, { "BA_LICENSE_VIOLATION", 4 },
            { "BA_CREW", 4 }, { "BA_CREW_MEMBER", 4 }, { "BA_EMPLOYEE", 4 },
            { "BA_ORGANIZATION", 4 }, { "BA_ORGANIZATION_COMP", 4 }, { "BA_XREF", 4 },

            // ========== 5. Classification / Taxonomy ==========
            { "CLASS_SYSTEM", 5 }, { "CLASS_SYSTEM_ALIAS", 5 }, { "CLASS_SYSTEM_XREF", 5 },
            { "CLASS_LEVEL_TYPE", 5 }, { "CLASS_LEVEL", 5 }, { "CLASS_LEVEL_ALIAS", 5 },
            { "CLASS_LEVEL_COMPONENT", 5 }, { "CLASS_LEVEL_DESC", 5 }, { "CLASS_LEVEL_XREF", 5 },

            // ========== 6. Consents ==========
            { "CONSENT", 6 }, { "CONSENT_BA", 6 }, { "CONSENT_COMPONENT", 6 },
            { "CONSENT_COND", 6 }, { "CONSENT_REMARK", 6 },

            // ========== 7. Consultations ==========
            { "CONSULT", 7 }, { "CONSULT_BA", 7 }, { "CONSULT_COMPONENT", 7 },
            { "CONSULT_DISC", 7 }, { "CONSULT_DISC_BA", 7 }, { "CONSULT_DISC_ISSUE", 7 },
            { "CONSULT_ISSUE", 7 }, { "CONSULT_XREF", 7 },

            // ========== 7. Contests / Disputes (Operations Support – same group) ==========
            { "CONTEST", 7 }, { "CONTEST_COMPONENT", 7 }, { "CONTEST_PARTY", 7 }, { "CONTEST_REMARK", 7 },

            // ========== 8. Contracts / Agreements ==========
            { "CONTRACT", 8 }, { "CONT_ALIAS", 8 }, { "CONT_AREA", 8 }, { "CONT_BA", 8 },
            { "CONT_BA_SERVICE", 8 }, { "CONT_EXTENSION", 8 }, { "CONT_REMARK", 8 },
            { "CONT_STATUS", 8 }, { "CONT_TYPE", 8 }, { "CONT_XREF", 8 },
            { "CONTRACT_COMPONENT", 8 }, { "CONT_ACCOUNT_PROC", 8 }, { "CONT_ALLOW_EXPENSE", 8 },
            { "CONT_OPER_PROC", 8 }, { "CONT_MKTG", 8 }, { "CONT_MKTG_ELECT_SUBST", 8 },
            { "CONT_VOTING_PROC", 8 }, { "CONT_PROVISION", 8 }, { "CONT_KEY_WORD", 8 },
            { "CONT_EXEMPTION", 8 }, { "CONT_PROVISION_TEXT", 8 }, { "CONT_PROVISION_XREF", 8 },
            { "CONT_JURISDICTION", 8 },

            // ========== 9. Coordinate Reference Systems ==========
            { "CS_COORDINATE_SYSTEM", 9 }, { "CS_ALIAS", 9 }, { "CS_COORD_ACQUISITION", 9 },
            { "CS_COORD_TRANSFORM", 9 }, { "CS_COORD_TRANS_PARM", 9 }, { "CS_COORD_TRANS_VALUE", 9 },
            { "CS_GEODETIC_DATUM", 9 }, { "CS_ELLIPSOID", 9 }, { "CS_PRIME_MERIDIAN", 9 },
            { "CS_PRINCIPAL_MERIDIAN", 9 },

            // ========== 10. Ecozones ==========
            { "ECOZONE_SET", 10 }, { "ECOZONE_SET_PART", 10 }, { "ECOZONE", 10 },
            { "ECOZONE_ALIAS", 10 }, { "ECOZONE_HIERARCHY", 10 }, { "ECOZONE_XREF", 10 },
            { "PALEO_CLIMATE", 10 },

            // ========== 11. Entitlements / Security ==========
            { "ENTITLEMENT", 11 }, { "ENT_COMPONENT", 11 }, { "ENT_GROUP", 11 },
            { "ENT_SECURITY_GROUP", 11 }, { "ENT_SECURITY_GROUP_XREF", 11 }, { "ENT_SECURITY_BA", 11 },
            { "LAND_RIGHT", 11 }, { "LAND_AGREE_PART", 11 }, { "LAND_AGREEMENT", 11 },
            { "LAND_TITLE", 11 }, { "LAND_UNIT", 11 }, { "LAND_UNIT_TRACT", 11 },
            { "LAND_TRACT_FACTOR", 11 }, { "LAND_STATUS", 11 }, { "LAND_TERMINATION", 11 },
            { "LAND_SALE", 11 }, { "LAND_SALE_OFFERING", 11 }, { "LAND_SALE_BID", 11 },
            { "LAND_SALE_WORK_BID", 11 }, { "LAND_SALE_BID_SET_BID", 11 }, { "LAND_SALE_BID_SET", 11 },
            { "LAND_SALE_BA_SERVICE", 11 }, { "LAND_SALE_OFFERING_AREA", 11 }, { "LAND_SALE_FEE", 11 },
            { "LAND_SALE_REQUEST", 11 }, { "LAND_SALE_RESTRICTION", 11 }, { "LAND_SALE_REST_REMARK", 11 },
            { "LAND_RIGHT_COMPONENT", 11 }, { "LAND_RIGHT_APPLIC", 11 }, { "LAND_RIGHT_BALIC", 11 },
            { "LAND_AREA", 11 }, { "LAND_RIGHT_FACILITY", 11 }, { "LAND_RIGHT_FIELD", 11 },
            { "LAND_RIGHT_INSTRUMENT", 11 }, { "LAND_RIGHT_POOL", 11 }, { "LAND_RIGHT_REST", 11 },
            { "LAND_RIGHT_REST_REM", 11 }, { "LAND", 11 }, { "LAND_ALIAS", 11 },
            { "LAND_BA_SERVICE", 11 }, { "LAND_OCCUPANT", 11 }, { "LAND_REMARK", 11 },
            { "LAND_SIZE", 11 }, { "LAND_XREF", 11 },

            // ========== 12. Equipment ==========
            { "EQUIPMENT", 12 }, { "EQUIPMENT_ALIAS", 12 }, { "EQUIPMENT_BA", 12 },
            { "EQUIPMENT_COMPONENT", 12 }, { "EQUIPMENT_STATUS", 12 }, { "EQUIPMENT_USE_STAT", 12 },
            { "EQUIPMENT_XREF", 12 }, { "CAT_EQUIPMENT", 12 }, { "CAT_EQUIP_ALIAS", 12 },
            { "CAT_EQUIP_SPEC", 12 }, { "EQUIPMENT_MAINTAIN", 12 }, { "EQUIPMENT_MAINT_STATUS", 12 },
            { "EQUIPMENT_MAINT_TYPE", 12 }, { "EQUIPMENT_SPEC", 12 }, { "EQUIPMENT_SPEC_SET", 12 },
            { "EQUIPMENT_SPEC_SET_SPEC", 12 },

            // ========== 13. Facilities ==========
            { "FACILITY", 13 }, { "FACILITY_BA_SERVICE", 13 }, { "FACILITY_EQUIPMENT", 13 },
            { "FACILITY_MAINTAIN", 13 }, { "FACILITY_MAINT_STATUS", 13 }, { "FACILITY_STATUS", 13 },
            { "FACILITY_SUBSTANCE", 13 }, { "FACILITY_ALIAS", 13 }, { "FACILITY_AREA", 13 },
            { "FACILITY_CLASS", 13 }, { "FACILITY_COMPONENT", 13 }, { "FACILITY_DESCRIPTION", 13 },
            { "FACILITY_FIELD", 13 }, { "FACILITY_RATE", 13 }, { "FACILITY_RESTRICTION", 13 },
            { "FACILITY_VERSION", 13 }, { "FACILITY_XREF", 13 }, { "FACILITY_LICENSE", 13 },
            { "FACILITY_LIC_ALIAS", 13 }, { "FACILITY_LIC_AREA", 13 }, { "FACILITY_LIC_COND", 13 },
            { "FACILITY_LIC_REMARK", 13 }, { "FACILITY_LIC_STATUS", 13 }, { "FACILITY_LIC_TYPE", 13 },
            { "FACILITY_LIC_VIOLATION", 13 },

            // ========== 14. Fields ==========
            { "FIELD", 14 }, { "FIELD_ALIAS", 14 }, { "FIELD_AREA", 14 },
            { "FIELD_COMPONENT", 14 }, { "FIELD_INSTRUMENT", 14 }, { "FIELD_VERSION", 14 },

            // ========== 15. Financial ==========
            { "FINANCE", 15 }, { "FIN_COMPONENT", 15 }, { "FIN_COST_SUMMARY", 15 }, { "FIN_XREF", 15 },

            // ========== 16. Fossils ==========
            // (FOSSIL tables are under Paleontology)

            // ========== 17. HSE / Incidents ==========
            { "HSE_INCIDENT", 17 }, { "HSE_INCIDENT_CLASS", 17 }, { "HSE_INCIDENT_CLASS_ALAIS", 17 },
            { "HSE_INCIDENT_BA", 17 }, { "HSE_INCIDENT_CAUSE", 17 }, { "HSE_INCIDENT_DETAIL", 17 },
            { "HSE_INCIDENT_EQUIP", 17 }, { "HSE_INCIDENT_EQUIV", 17 }, { "HSE_INCIDENT_INTERACTION", 17 },
            { "HSE_INCIDENT_REMARK", 17 }, { "HSE_INCIDENT_RESPONCE", 17 }, { "HSE_INCIDENT_SUBSTANCE", 17 },
            { "HSE_INCIDENT_WEATHER", 17 }, { "HSE_INCIDENT_COMPONENT", 17 }, { "HSE_INCIDENT_SET", 17 },
            { "HSE_INCIDENT_TYPE", 17 }, { "HSE_INCIDENT_TYPE_ALIAS", 17 }, { "HSE_INCIDENT_SEVERITY", 17 },
            { "HSE_INCIDENT_SEV_ALIAS", 17 },

            // ========== 18. Instruments ==========
            { "INSTRUMENT", 18 }, { "INSTRUMENT_AREA", 18 }, { "INSTRUMENT_COMPONENT", 18 },
            { "INSTRUMENT_DETAIL", 18 }, { "INSTRUMENT_XREF", 18 },

            // ========== 19. Interest Sets / Partnerships ==========
            { "INTEREST_SET", 19 }, { "INT_SET_PARTNER", 19 }, { "INT_SET_PARTNER_CONT", 19 },
            { "INT_SET_STATUS", 19 }, { "INT_SET_XREF", 19 }, { "INT_SET_COMPONENT", 19 },

            // ========== 20. Lithology ==========
            { "LITH_MEASURED_SEC", 20 }, { "LITH_LOG", 20 }, { "LITH_LOG_BA_SERVICE", 20 },
            { "LITH_DEP_ENV_INT", 20 }, { "LITH_LOG_REMARK", 20 }, { "LITH_LOG_COMPONENT", 20 },
            { "LITH_INTERVAL", 20 }, { "LITH_STRUCTURE", 20 }, { "LITH_ROCK_TYPE", 20 },
            { "LITH_DIAGENESIS", 20 }, { "LITH_GRAIN_SIZE", 20 }, { "LITH_POROSITY", 20 },
            { "LITH_ROCK_COLOR", 20 }, { "LITH_ROCK_STRUCTURE", 20 }, { "LITH_ROCKPART", 20 },
            { "LITH_ROCKPART_COLOR", 20 }, { "LITH_ROCKPART_GRAIN_SIZE", 20 },

            // ========== 21. Notifications ==========
            { "NOTIFICATION", 21 }, { "NOTIF_BA", 21 }, { "NOTIFICATION_COMPONENT", 21 },

            // ========== 22. Obligations ==========
            { "OBLIGATION", 22 }, { "OBLIG_TYPE", 22 }, { "OBLIG_BA_SERVICE", 22 }, { "OBLIG_CALC", 22 },
            { "OBLIG_SUBSTANCE", 22 }, { "OBLIG_DEDUCTION", 22 }, { "OBLIG_ALLOW_DEDUCTION", 22 },
            { "OBLIG_PAYMENT", 22 }, { "OBLIG_PAY_DETAIL", 22 }, { "OBLIG_PAYMENT_INSTR", 22 },
            { "OBLIG_PAYMENT_RATE", 22 }, { "OBLIG_REMARK", 22 }, { "OBLIG_XREF", 22 },
            { "OBLIGATION_TYPE", 22 },

            // ========== 23. Other ==========
            // (General uncategorized tables - no specific mapping)

            // ========== 24. PPDM Data Management ==========
            { "PPDM_SYSTEM", 24 }, { "PPDM_SYSTEM_APPLICATION", 24 }, { "PPDM_SYSTEM_ATLIS", 24 },
            { "PPDM_SYSTEM_MAP", 24 }, { "PPDM_TABLE", 24 }, { "PPDM_TABLE_ALIAS", 24 },
            { "PPDM_PROCEDURE", 24 }, { "PPDM_METRIC", 24 }, { "PPDM_METRIC_COMPONENT", 24 },
            { "PPDM_METRIC_VALUE", 24 }, { "PPDM_PROPERTY_SET", 24 }, { "PPDM_PROPERTY_COLUMN", 24 },
            { "PPDM_AUDIT_HISTORY", 24 }, { "PPDM_AUDIT_HISTORY_REM", 24 }, { "PPDM_QUALITY_CONTROL", 24 },
            { "PPDM_TABLE_HISTORY", 24 }, { "PPDM_MAP_DETAIL", 24 }, { "PPDM_MAP_LOAD", 24 },
            { "PPDM_MAP_LOAD_ERROR", 24 }, { "PPDM_EXCEPTION", 24 }, { "PPDM_MAP_RULE", 24 },
            { "PPDM_OBJECT_STATUS", 24 }, { "PPDM_GROUP", 24 }, { "PPDM_GROUP_REMARK", 24 },
            { "PPDM_GROUP_OBJECT", 24 }, { "PPDM_GROUP_OWNER", 24 }, { "PPDM_GROUP_XREF", 24 },
            { "PPDM_CONSTRAINT", 24 }, { "PPDM_CONS_COLUMN", 24 }, { "PPDM_INDEX", 24 },
            { "PPDM_INDEX_COLUMN", 24 }, { "PPDM_RULE", 24 }, { "PPDM_RULE_ALIAS", 24 },
            { "PPDM_RULE_DETAIL", 24 }, { "PPDM_RULE_ENFORCEMENT", 24 }, { "PPDM_RULE_XREF", 24 },
            { "PPDM_RULE_REMARK", 24 }, { "PPDM_RULE_COMPONENT", 24 }, { "PPDM_SW_APPLICATION", 24 },
            { "PPDM_SW_APPLIC_ALIAS", 24 }, { "PPDM_SW_APPLIC_COMP", 24 }, { "PPDM_SW_APP_BA", 24 },
            { "PPDM_SW_APP_FUNCTION", 24 }, { "PPDM_SW_APP_XREF", 24 }, { "PPDM_COLUMN", 24 },
            { "PPDM_COLUMN_ALIAS", 24 }, { "PPDM_CHECK_CONS_VALUE", 24 }, { "PPDM_DOMAIN", 24 },
            { "PPDM_QUANTITY", 24 }, { "PPDM_QUANTITY_ALIAS", 24 }, { "PPDM_SCHEMA_ENTITY", 24 },
            { "PPDM_SCHEMA_ENTITY_ALIAS", 24 }, { "PPDM_SCHEMA_GROUP", 24 }, { "PPDM_CODE_VERSION", 24 },
            { "PPDM_CODE_VERSION_COLUMN", 24 }, { "PPDM_CODE_VERSION_USE", 24 }, { "PPDM_CODE_VERSION_XREF", 24 },

            // ========== 25. Paleontology ==========
            { "FOSSIL_NAME_SET", 25 }, { "FOSSIL_NAME_SET_FOSSIL", 25 }, { "FOSSIL", 25 },
            { "FOSSIL_TAXON_LEAF", 25 }, { "FOSSIL_TAXON_ALIAS", 25 }, { "FOSSIL_TAXON_HEIR", 25 },
            { "FOSSIL_AGE", 25 }, { "FOSSIL_DESC", 25 }, { "FOSSIL_DOCUMENT", 25 },
            { "FOSSIL_ASSEMBLAGE", 25 }, { "FOSSIL_EQUIVALENCE", 25 }, { "FOSSIL_XREF", 25 },
            { "PALEO_SUMMARY", 25 }, { "PALEO_INTERP", 25 }, { "PALEO_FOSSIL_INTERP", 25 },
            { "PALEO_SUM_AUTHOR", 25 }, { "PALEO_SUM_COMP", 25 }, { "PALEO_SUM_INTERVAL", 25 },
            { "PALEO_SUM_SAMPLE", 25 }, { "PALEO_SUM_XREF", 25 }, { "PALEO_FOSSIL_IND", 25 },
            { "PALEO_FOSSIL_OBS", 25 }, { "PALEO_OBS_QUALIFIER", 25 }, { "PALEO_CONFIDENCE", 25 },
            { "ANL_PALEOMATURITY", 25 }, { "PALEO_ABUND_SCHEME", 25 }, { "PALEO_ABUND_QUALIFIER", 25 },

            // ========== 26. Pools ==========
            { "POOL", 26 }, { "POOL_ALIAS", 26 }, { "POOL_AREA", 26 },
            { "POOL_COMPONENT", 26 }, { "POOL_INSTRUMENT", 26 }, { "POOL_VERSION", 26 },
            { "POOL_VERSION_AREA", 26 },

            // ========== 27. Production Entities ==========
            { "PDEN", 27 }, { "PDEN_IN_AREA", 27 }, { "PDEN_XREF", 27 }, { "PDEN_VOL_REGIME", 27 },
            { "PDEN_OPER_HIST", 27 }, { "PDEN_STATUS_HIST", 27 }, { "PDEN_COUPONET", 27 },
            { "PDEN_AREA", 27 }, { "PDEN_BUSINESS_ASSOC", 27 }, { "PDEN_FACILITY", 27 },
            { "PDEN_FIELD", 27 }, { "PDEN_LAND_RIGHT", 27 }, { "PDEN_LEASE_UNIT", 27 },
            { "PDEN_OTHER", 27 }, { "PDEN_POOL", 27 }, { "PDEN_PR_STR_FORM", 27 },
            { "PDEN_PROD_STRING", 27 }, { "PDEN_PROD_STRING_XREF", 27 }, { "PDEN_PR_STR_ALLOWABLE", 27 },
            { "PDEN_RESENT", 27 }, { "PDEN_REGENT_CLASS", 27 }, { "PDEN_WELL", 27 },
            { "PDEN_WELL_REPORT_STREAM", 27 }, { "PDEN_VOL_SUMMARY", 27 }, { "PDEN_VOL_DISPOSITION", 27 },
            { "PDEN_ALLOC_FACTOR", 27 }, { "PDEN_FLOW_MEASUREMENT", 27 }, { "PDEN_DECLINE_CAGE", 27 },
            { "PDEN_DECLINE_CONDITION", 27 }, { "PDEN_DECLINE_SEGMENT", 27 }, { "PDEN_MATERIAL_BAL", 27 },
            { "PDEN_VOLUME_ANALYSIS", 27 },

            // ========== 28. Production Lease Units ==========
            { "PROD_LEASE_UNIT", 28 }, { "PROD_LEASE_UNIT_ALIAS", 28 }, { "PROD_LEASE_UNIT_AREA", 28 },
            { "PRLSE_UNIT_STR_HIST", 28 }, { "PROD_LEASE_UNIT_VERSION", 28 }, { "PROD_LEASE_UNIT_VERSION_AREA", 28 },

            // ========== 29. Production Strings ==========
            { "PROD_STRING", 29 }, { "PROD_STRING_ALIAS", 29 }, { "PROD_STR_STAT_HIST", 29 },
            { "PROD_STRING_FORMATION", 29 }, { "PROD_STRING_FORM_ALIAS", 29 }, { "PR_STR_FORM_COMPLETION", 29 },
            { "PR_STR_FORM_STAT_HIST", 29 }, { "PROD_STR_STAT_COMPONENT", 29 },

            // ========== 30. Projects ==========
            { "PROJECT", 30 }, { "PROJECT_ALIAS", 30 }, { "PROJECT_STATUS", 30 },
            { "PROJECT_COMPONENT", 30 }, { "PROJECT_BA", 30 }, { "PROJECT_BA_ROLE", 30 },
            { "PROJECT_EQUIPMENT", 30 }, { "PROJECT_EQUIP_ROLE", 30 }, { "PROJECT_STEP_EQUIP", 30 },
            { "PROJECT_PLAN", 30 }, { "PROJECT_PLAN_STEP", 30 }, { "PROJECT_PLAN_STEP_XREF", 30 },
            { "PROJECT_STEP", 30 }, { "PROJECT_STEP_BA", 30 }, { "PROJECT_STEP_XREF", 30 },
            { "PROJECT_STEP_CONDITION", 30 }, { "PROJECT_STEP_TIME", 30 }, { "PROJECT_CONDITION", 30 },

            // ========== 31. Rate Schedules ==========
            { "RATE_SCHEDULE", 31 }, { "RATE_AREA", 31 }, { "RATE_SCHEDULE_COMPONENT", 31 },
            { "RATE_SCHEDULE_XREF", 31 }, { "RATE_SCHED_DETAIL", 31 },

            // ========== 32. Records Management ==========
            { "RM_INFORMATION_ITEM", 32 }, { "RM_CREATOR", 32 }, { "RM_CUSTODY", 32 },
            { "RM_INFO_COORD_QUALITY", 32 }, { "RM_INFO_DATA_QUALITY", 32 }, { "RM_INFO_ITEM_ALIAS", 32 },
            { "RM_INFO_ITEM_BA", 32 }, { "RM_INFO_ITEM_CONTENT", 32 }, { "RM_INFO_ITEM_DESC", 32 },
            { "RM_INFO_ITEM_GEOMETRY", 32 }, { "RM_INFO_ITEM_GROUP", 32 }, { "RM_INFO_ITEM_MAINT", 32 },
            { "RM_INFO_ITEM_ORIGIN", 32 }, { "RM_INFO_ITEM_STATUS", 32 }, { "RM_THESAURUS", 32 },
            { "RM_THESAURUS_WORD", 32 }, { "RM_THESAURUS_WORD_XREF", 32 }, { "RM_KEYWORD", 32 },
            { "RM_THESAURUS_GLOSSARY", 32 }, { "RM_FILE_CONTENT", 32 }, { "RM_DATA_CONTENT", 32 },
            { "RM_PHYSICAL_ITEM", 32 }, { "RM_IMAGE_SECT", 32 }, { "RM_IMAGE_LOC", 32 },
            { "RM_IMAGE_COMP", 32 }, { "RM_PHYS_ITEM_STORE", 32 }, { "RM_DATA_STORE", 32 },
            { "RM_DATA_STORE_STRUCTURE", 32 }, { "RM_DATA_STORE_ITEM", 32 }, { "RM_DATA_STORE_MEDIA", 32 },
            { "RM_DATA_STORE_HIER", 32 }, { "RM_DATA_STORE_HIER_LEVEL", 32 }, { "RM_ENCODING", 32 },
            { "RM_DECRYPT_KEY", 32 }, { "RM_CIRCULATION", 32 }, { "RM_CIRC_PROCESS", 32 },
            { "RM_COMPOSITE", 32 }, { "RM_DOCUMENT", 32 }, { "RM_EQUIPMENT", 32 },
            { "RM_FOSSIL", 32 }, { "RM_LITH_SAMPLE", 32 }, { "RM_MAP", 32 },
            { "RM_SEIS_TRACE", 32 }, { "RM_TRACE_HEADER", 32 }, { "RM_AUX_CHANNEL", 32 },
            { "RM_WELL_LOG", 32 }, { "RM_SPATIAL_DATA_SET", 32 },

            // ========== 33. Reference Values ==========
            // Standard PPDM39 Reference Tables
            { "CODE_LIBRARY", 33 }, { "CODE_VALUE", 33 }, { "REFERENCE_TABLE", 33 },
            { "REFERENCE_VALUE", 33 }, { "ALIAS_TYPE", 33 }, { "AREA_STATUS", 33 },
            { "AREA_TYPE", 33 }, { "AUTHORITY_TYPE", 33 }, { "BA_ROLE_TYPE", 33 },
            { "BA_STATUS", 33 }, { "BA_TYPE", 33 }, { "CATEGORY_TYPE", 33 },
            { "CONTACT_TYPE", 33 }, { "COORD_ACQN_CODE", 33 }, { "COORD_OBSOLETE_CODE", 33 },
            { "COORD_SYS_TYPE", 33 }, { "COUNTRY", 33 }, { "COUNTY", 33 },
            { "DATA_STORE_TYPE", 33 }, { "DATUM_TYPE", 33 }, { "DESCRIPTION_TYPE", 33 },
            { "DOCUMENT_TYPE", 33 }, { "ELEVATION_TYPE", 33 }, { "EMPLOYEE_TYPE", 33 },
            { "ENTITLEMENT_TYPE", 33 }, { "EQUIPMENT_STATUS", 33 }, { "EQUIPMENT_TYPE", 33 },
            { "FACILITY_STATUS", 33 }, { "FACILITY_TYPE", 33 }, { "FIELD_STATION_TYPE", 33 },
            { "FINANCE_TYPE", 33 }, { "FOSSIL_GROUP", 33 }, { "FOSSIL_NAME_TYPE", 33 },
            { "GEODETIC_DATUM_TYPE", 33 }, { "INSTRUMENT_TYPE", 33 }, { "INTEREST_TYPE", 33 },
            { "JURISDICTION_TYPE", 33 }, { "LICENSE_STATUS", 33 }, { "LICENSE_TYPE", 33 },
            { "LINE_TYPE", 33 }, { "LITHOLOGY_TYPE", 33 }, { "LOCATION_TYPE", 33 },
            { "MEASUREMENT_TYPE", 33 }, { "MERIDIAN_TYPE", 33 }, { "METHOD_TYPE", 33 },
            { "NOTIFICATION_TYPE", 33 }, { "OBJECT_STATUS_TYPE", 33 }, { "OBLIGATION_STATUS", 33 },
            { "OBLIGATION_TYPE", 33 }, { "ORGANIZATION_TYPE", 33 }, { "ORIGIN_TYPE", 33 },
            { "PARCEL_TYPE", 33 }, { "PHYSICAL_STATUS", 33 }, { "POOL_STATUS", 33 },
            { "POOL_TYPE", 33 }, { "PREFERRED_ID_TYPE", 33 }, { "PRIVILEGE_TYPE", 33 },
            { "PROC_STATUS_TYPE", 33 }, { "PROCESS_TYPE", 33 }, { "PRODUCT_TYPE", 33 },
            { "PROJECT_STATUS", 33 }, { "PROJECT_TYPE", 33 }, { "QUALIFIER_TYPE", 33 },
            { "QUANTITY_TYPE", 33 }, { "RATE_SCHEDULE_TYPE", 33 }, { "RATE_TYPE", 33 },
            { "RECOURSE_TYPE", 33 }, { "REFERENCE_ORIGIN_TYPE", 33 }, { "REGULATION_TYPE", 33 },
            { "REMARK_TYPE", 33 }, { "RESENT_TYPE", 33 }, { "RESTRICTION_TYPE", 33 },
            { "RIGHT_TYPE", 33 }, { "ROLE_TYPE", 33 }, { "SAMPLE_TYPE", 33 },
            { "SEIS_3D_TYPE", 33 }, { "SEIS_ACQTN_METHOD", 33 }, { "SEIS_DATA_TYPE", 33 },
            { "SEIS_DIMENSION", 33 }, { "SEIS_LINE_TYPE", 33 }, { "SEIS_PROCESS_STATUS", 33 },
            { "SEIS_RECVR_TYPE", 33 }, { "SEIS_SOURCE_TYPE", 33 }, { "SEIS_SP_TYPE", 33 },
            { "SEIS_SURVEY_TYPE", 33 }, { "SEIS_TRANSPORT_TYPE", 33 }, { "SERVICE_TYPE", 33 },
            { "SPECIAL_CONDITION", 33 }, { "STATE", 33 }, { "STATUS_TYPE", 33 },
            { "STRAT_HIER_TYPE", 33 }, { "STRAT_INTERP_TYPE", 33 }, { "STRAT_STATUS", 33 },
            { "STRAT_TYPE", 33 }, { "STRING_STATUS", 33 }, { "STRING_TYPE", 33 },
            { "SUBSTANCE_TYPE", 33 }, { "SUPPORT_FACILITY_TYPE", 33 }, { "SURVEY_TYPE", 33 },
            { "TAX_CREDIT_TYPE", 33 }, { "TEST_STATUS", 33 }, { "TEST_TYPE", 33 },
            { "TRACKING_STATUS", 33 }, { "UNIT_OF_MEASURE_TYPE", 33 }, { "UOM_SYSTEM", 33 },
            { "VOLUME_REGIME", 33 }, { "WELL_STATUS", 33 }, { "WELL_TYPE", 33 },
            { "WORK_ORDER_STATUS", 33 }, { "WORK_ORDER_TYPE", 33 }, { "ZONE_TYPE", 33 },
            // Legal Location tables (from JSON)
            { "LEGAL_LOC", 33 }, { "LEGAL_LOC_AREA", 33 }, { "LEGAL_LOC_REMARK", 33 },
            { "LEGAL_GEODETIC_LOC", 33 }, { "LEGAL_CARTER_LOC", 33 }, { "LEGAL_CONGRESS_LOC", 33 },
            { "LEGAL_DLS_LOC", 33 }, { "LEGAL_FIPS_LOC", 33 }, { "LEGAL_NORTH_SEA_LOC", 33 },
            { "LEGAL_NTS_LOC", 33 }, { "LEGAL_OFFSHORE_LOC", 33 }, { "LEGAL_OHIO_LOC", 33 },
            { "LEGAL_TEXAS_LOC", 33 },
            // R_ LOV / Picklist tables (PPDM standard reference tables)
            { "R_WELL_STATUS", 33 }, { "R_WELL_CLASS", 33 }, { "R_WELL_PROFILE_TYPE", 33 },
            { "R_FLUID_TYPE", 33 }, { "R_SEVERITY", 33 }, { "R_COMPLETION_STATUS", 33 },
            { "R_COMPLETION_TYPE", 33 }, { "R_PRODUCTION_METHOD", 33 }, { "R_ALLOCATION_TYPE", 33 },
            { "R_WELL_QUALIFIC_TYPE", 33 }, { "R_WELL_COMPONENT_TYPE", 33 },
            { "R_PLATFORM_TYPE", 33 }, { "R_LOCATION_TYPE", 33 }, { "R_AREA_TYPE", 33 },
            { "R_WELL_ACTIVITY_CAUSE", 33 }, { "R_FIN_STATUS", 33 }, { "R_BA_PERMIT_TYPE", 33 },
            { "R_LICENSE_STATUS", 33 }, { "R_CONDITION_TYPE", 33 }, { "R_DIRECTION", 33 },
            { "R_WELL_TEST_TYPE", 33 }, { "R_TEST_EQUIPMENT", 33 }, { "R_TEST_RESULT", 33 },
            { "R_CAT_EQUIP_TYPE", 33 }, { "R_CAT_EQUIP_SPEC", 33 },
            { "R_PROJECT_STATUS", 33 }, { "R_PROJECT_TYPE", 33 },
            { "R_PRODUCTION_ENTITY_TYPE", 33 }, { "R_PROD_STRING_STATUS", 33 },
            { "R_WELL_ACTIVITY_TYPE", 33 }, { "R_STRAT_UNIT_TYPE", 33 },
            { "R_SEIS_SET_TYPE", 33 }, { "R_SEIS_LINE_TYPE", 33 },
            { "R_SAMPLE_TYPE", 33 }, { "R_ANL_METHOD_TYPE", 33 },
            { "R_OBLIGATION_TYPE", 33 }, { "R_OBLIGATION_STATUS", 33 },
            { "R_RESTRICTION_TYPE", 33 }, { "R_ENTITLEMENT_TYPE", 33 },
            { "R_ECOZONE_TYPE", 33 }, { "R_PALEO_CONFIDENCE", 33 },

            // ========== 34. Reporting Hierarchies ==========
            { "REPORT_HIER_TYPE", 34 }, { "REPORT_HIER_DESC", 34 }, { "REPORT_HIER", 34 },
            { "REPORT_HIER_ALIAS", 34 }, { "REPORT_HIER_LEVEL", 34 }, { "REPORT_HIER_USE", 34 },

            // ========== 35. Reserves ==========
            { "RESERVE_CLASS", 35 }, { "RESERVE_ENTITY", 35 }, { "RESERVE_CLASS_FORMULA", 35 },
            { "RESERVE_CLASS_CALC", 35 }, { "RESENT_CLASS", 35 }, { "RESENT_VOL_SUMMARY", 35 },
            { "RESENT_VOL_REVISION", 35 }, { "RESENT_REVISION_CAT", 35 }, { "RESENT_ECO_RUN", 35 },
            { "RESENT_ECO_SCHEDULE", 35 }, { "RESENT_ECO_VOLUME", 35 }, { "RESENT_PRODUCT", 35 },
            { "RESENT_PROD_PROPERTY", 35 }, { "RESENT_COMPONENT", 35 }, { "RESENT_XREF", 35 },
            { "RESENT_VOL_REGIME", 35 },

            // ========== 36. Restrictions / Environment ==========
            { "RESTRICTION", 36 }, { "REST_ACTIVITY", 36 }, { "REST_CLASS", 36 },
            { "REST_CONTACT", 36 }, { "REST_REMARK", 36 }, { "REST_TEXT", 36 },

            // ========== 37. Sample Analysis ==========
            { "ANL_SAMPLE", 37 }, { "ANL_ANALYSIS_REPORT", 37 }, { "ANL_REPORT_ALIAS", 37 },
            { "ANL_COMPONENT", 37 }, { "ANL_ANALYSIS_STEP", 37 }, { "ANL_STEP_XREF", 37 },
            { "ANL_BA", 37 }, { "ANL_EQUIP", 37 }, { "ANL_PARM", 37 },
            { "ANL_ANALYSIS_BATCH", 37 }, { "ANL_METHOD_SET", 37 }, { "ANL_METHOD", 37 },
            { "ANL_METHOD_ALIAS", 37 }, { "ANL_METHOD_EQUIV", 37 }, { "ANL_METHOD_PARM", 37 },
            { "ANL_ACCURACY", 37 }, { "ANL_DETAIL", 37 }, { "ANL_PROBLEM", 37 },
            { "ANL_REMARK", 37 }, { "ANL_TABLE_RESULT", 37 }, { "ANL_VALID_TABLE_RESULT", 37 },
            { "ANL_VALID_BA", 37 }, { "ANL_VALID_EQUIP", 37 }, { "ANL_VALID_TOLERANCE", 37 },
            { "ANL_VALID_MEASURE", 37 }, { "ANL_VALID_PROBLEM", 37 }, { "ANL_CALC_SET", 37 },
            { "ANL_CALC_ALIAS", 37 }, { "ANL_CALC_EQUIV", 37 }, { "ANL_CALC_FORMULA", 37 },
            { "ANL_CALC_METHOD", 37 }, { "ANL_ELEMENTAL", 37 }, { "ANL_ELEMENTAL_DETAIL", 37 },
            { "ANL_GAS_ANALYSIS", 37 }, { "ANL_GAS_COMPOSITION", 37 }, { "ANL_GAS_DETAIL", 37 },
            { "ANL_GAS_HEAT", 37 }, { "ANL_GAS_PRESS", 37 }, { "ANL_OIL_FRACTION", 37 },
            { "ANL_OIL_ANALYSIS", 37 }, { "ANL_OIL_DETAIL", 37 }, { "ANL_OIL_DISTILL", 37 },
            { "ANL_OIL_VISCOSITY", 37 }, { "ANL_PYROLYSIS", 37 }, { "ANL_WATER_ANALYSIS", 37 },
            { "ANL_WATER_DETAIL", 37 }, { "ANL_WATER_SALINITY", 37 }, { "ANL_ISOTOPE", 37 },
            { "ANL_ISOTOPE_STD", 37 }, { "ANL_MACERAL", 37 }, { "ANL_MACERAL_MATURITY", 37 },
            { "ANL_PALEO", 37 }, { "ANL_PALEO_MATURITY", 37 }, { "ANL_COAL_RANK", 37 },
            { "ANL_COAL_RANK_SCHEME", 37 }, { "ANL_FLUOR", 37 }, { "ANL_OGF_TSF", 37 },
            { "ANL_LIQUID_CHRO", 37 }, { "ANL_LIQUID_CHRO_DETAIL", 37 }, { "ANL_GAS_CHRO", 37 },

            // ========== 38. Sample Masters ==========
            { "SAMPLE", 38 }, { "SAMPLE_ALIAS", 38 }, { "SAMPLE_COMPONENT", 38 },
            { "SAMPLE_DESC_OTHER", 38 }, { "SAMPLE_LITH_DESC", 38 }, { "SAMPLE_ORIGIN", 38 },

            // ========== 39. Seismic ==========
            { "SEIS_SET", 39 }, { "SEIS_ACTIVITY", 39 }, { "SEIS_ALIAS", 39 },
            { "SEIS_BA_SERVICE", 39 }, { "SEIS_SET_AREA", 39 }, { "SEIS_SET_JURISDICTION", 39 },
            { "SEIS_SET_STATUS", 39 }, { "SEIS_SET_COMPONENT", 39 }, { "SEIS_SET_AUTHORIZE", 39 },
            { "SEIS_SET_PLAN", 39 }, { "SEIS_3D", 39 }, { "SEIS_ACQTN_SURVEY", 39 },
            { "SEIS_INTERP_SET", 39 }, { "SEIS_LINE", 39 }, { "SEIS_PROC_SET", 39 },
            { "SEIS_SEGMENT", 39 }, { "SEIS_WELL", 39 }, { "SEIS_GROUP_COMP", 39 },
            { "SEIS_ACQTN_DESIGN", 39 }, { "SEIS_ACQTN_SPECTRUM", 39 }, { "SEIS_RECVR_SETUP", 39 },
            { "SEIS_RECVR_MAKE", 39 }, { "SEIS_PATCH", 39 }, { "SEIS_PATCH_DESC", 39 },
            { "SEIS_RECORD", 39 }, { "SEIS_CHANNEL", 39 }, { "SEIS_VESSEL", 39 },
            { "SEIS_STREAMER", 39 }, { "SEIS_STREAMER_BUILD", 39 }, { "SEIS_STREAMER_COMP", 39 },
            { "SEIS_LICENSE", 39 }, { "SEIS_LICENSE_ALIAS", 39 }, { "SEIS_LICENSE_AREA", 39 },
            { "SEIS_LICENSE_COND", 39 }, { "SEIS_LICENSE_REMARK", 39 }, { "SEIS_LICENSE_STATUS", 39 },
            { "SEIS_LICENSE_TYPE", 39 }, { "SEIS_LICENSE_VIOLATION", 39 }, { "SEIS_POINT", 39 },
            { "SEIS_POINT_SUMMARY", 39 }, { "SEIS_POINT_VERSION", 39 }, { "SEIS_SP_SURVEY", 39 },
            { "SEIS_POINT_FLOW", 39 }, { "SEIS_POINT_FLOW_DESC", 39 }, { "SEIS_BIN_GRID", 39 },
            { "SEIS_BIN_OUTLINE", 39 }, { "SEIS_BIN_POINT", 39 }, { "SEIS_BIN_POINT_VERSION", 39 },
            { "SEIS_BIN_POINT_TRACE", 39 }, { "SEIS_BIN_ORIGIN", 39 }, { "Z_SEIS_SET_GEOMETRY", 39 },
            { "SEIS_INTERP_COMP", 39 }, { "SEIS_INTERP_LOAD", 39 }, { "SEIS_INTERP_LOAD_PARM", 39 },
            { "SEIS_INTERP_SURFACE", 39 }, { "SEIS_PICK", 39 }, { "SEIS_VELOCITY_VOLUME", 39 },
            { "SEIS_VELOCITY", 39 }, { "SEIS_VELOCITY_INTERVAL", 39 }, { "SEIS_PROC_PLAN", 39 },
            { "SEIS_PROC_PLAN_STEP", 39 }, { "SEIS_PROC_PLAN_PARM", 39 }, { "SEIS_PROC_COMPONENT", 39 },
            { "SEIS_PROC_STEP", 39 }, { "SEIS_PROC_STEP_COMP", 39 }, { "SEIS_PROC_PARM", 39 },
            { "SEIS_INSPECTION", 39 }, { "SEIS_INSP_COMPONENT", 39 }, { "SEIS_TRANSACTION", 39 },
            { "SEIS_TRANS_COMPONENT", 39 },

            // ========== 40. Sources ==========
            { "SOURCE_DOCUMENT", 40 }, { "SOURCE_DOC_AUTHOR", 40 }, { "SOURCE_DOC_BIBLIO", 40 },

            // ========== 41. Spacing Units ==========
            { "SPACING_UNIT", 41 }, { "SPACING_UNIT_INST", 41 },

            // ========== 42. Spatial Descriptions ==========
            { "SPATIAL_DESCRIPTION", 42 }, { "SP_COMPONENT", 42 }, { "SP_DESC_TEXT", 42 },
            { "SP_DESC_XREF", 42 }, { "SP_LINE", 42 }, { "SP_LINE_POINT", 42 },
            { "SP_LINE_POINT_VERSION", 42 }, { "SP_ZONE_DEFINITION", 42 }, { "SP_ZONE_DEFIN_XREF", 42 },
            { "SP_MINERAL_ZONE", 42 }, { "SP_ZONE_SUBSTANCE", 42 }, { "SP_POINT", 42 },
            { "SP_POINT_VERSION", 42 }, { "SP_POLYGON", 42 }, { "SP_BOUNDARY", 42 },
            { "SP_BOUNDARY_VERSION", 42 }, { "Z_SP_GEOMETRY", 42 },

            // ========== 43. Spatial Parcels ==========
            { "SP_PARCEL", 43 }, { "SP_PARCEL_AREA", 43 }, { "SP_PARCEL_CARTER", 43 },
            { "SP_PARCEL_CONGRESS", 43 }, { "SP_PARCEL_DLS", 43 }, { "SP_PARCEL_DLS_ROAD", 43 },
            { "SP_PARCEL_FPS", 43 }, { "SP_PARCEL_NE_LOC", 43 }, { "SP_PARCEL_NORTH_SEA", 43 },
            { "SP_PARCEL_NTS", 43 }, { "SP_PARCEL_OFFSHORE", 43 }, { "SP_PARCEL_OHIO", 43 },
            { "SP_PARCEL_POL", 43 }, { "SP_PARCEL_TEXAS", 43 }, { "SP_PARCEL_LIBYA", 43 },
            { "SP_PARCEL_M_B", 43 }, { "SP_PARCEL_LOT", 43 }, { "SP_PARCEL_REMARK", 43 },

            // ========== 44. Stratigraphy ==========
            { "STRAT", 44 }, { "STRAT_UNIT", 44 }, { "STRAT_NAME_SET", 44 }, { "STRAT_NAME_SET_XREF", 44 },
            { "STRAT_ALIAS", 44 }, { "STRAT_EQUIVALENCE", 44 }, { "STRAT_HIERARCHY", 44 },
            { "STRAT_HIERARCHY_DESC", 44 }, { "STRAT_TOPO", 44 }, { "STRAT_TOPO_RELATION", 44 },
            { "STRAT_UNIT_DESCRIPTION", 44 }, { "STRAT_UNIT_AGE", 44 }, { "STRAT_UNIT_COMPONENT", 44 },
            { "STRAT_COLUMN", 44 }, { "STRAT_COLUMN_UNIT", 44 }, { "STRAT_COLUMN_XREF", 44 },
            { "STRAT_COLUMN_ACQTN", 44 }, { "STRAT_COL_UNIT_AGE", 44 }, { "STRAT_FIELD_STATION", 44 },
            { "STRAT_FIELD_NODE", 44 }, { "STRAT_NODE_VERSION", 44 }, { "Z_STRAT_FIELD", 44 },
            { "Z_STRAT_FIELD_GEOMETRY", 44 }, { "STRAT_FIELD_SECTION", 44 }, { "STRAT_FIELD_ACQTN", 44 },
            { "STRAT_FLD_INTERP", 44 }, { "STRAT_FLD_INTERP_AGE", 44 }, { "STRAT_WELL_SECTION", 44 },
            { "STRAT_WELL_ACQTN", 44 }, { "STRAT_WELL_INTERP", 44 }, { "STRAT_WELL_INTERPAGE", 44 },
            { "STRAT_ACQTN_METHOD", 44 }, { "STRAT_INTERP", 44 }, { "STRAT_INTERP_CORR", 44 },

            // ========== 45. Substances / Products ==========
            { "SUBSTANCE", 45 }, { "SUBSTANCE_ALIAS", 45 }, { "SUBSTANCE_BA", 45 },
            { "SUBSTANCE_BEHAVIOR", 45 }, { "SUBSTANCE_COMPOSITION", 45 }, { "SUBSTANCE_PROPERTY_DETAIL", 45 },
            { "SUBSTANCE_USE", 45 }, { "SUBSTANCE_XREF", 45 }, { "Z_PRODUCT", 45 },
            { "Z_PRODUCT_COMPOSITION", 45 },

            // ========== 46. Support Facilities ==========
            { "SF_SUPPORT_FACILITY", 46 }, { "SF_ALIAS", 46 }, { "SF_AREA", 46 },
            { "SF_BA_CREW", 46 }, { "SF_BA_SERVICE", 46 }, { "SF_DESCRIPTION", 46 },
            { "SF_EQUIPMENT", 46 }, { "SF_MAINTAIN", 46 }, { "SF_STATUS", 46 },
            { "SF_RESTRICTION", 46 }, { "SF_REST_REMARK", 46 }, { "SF_COMPONENT", 46 },
            { "SF_XREF", 46 }, { "Z_SF_GEOMETRY", 46 }, { "SF_AIRCRAFT", 46 },
            { "SF_AIRSTRIP", 46 }, { "SF_BRIDGE", 46 }, { "SF_DISPOSAL", 46 },
            { "SF_DOCK", 46 }, { "SF_ELECTRIC", 46 }, { "SF_HABITAT", 46 },
            { "SF_LANDING", 46 }, { "SF_MONUMENT", 46 }, { "SF_OTHER", 46 },
            { "SF_PAD", 46 }, { "SF_PLATFORM", 46 }, { "SF_PORT", 46 },
            { "SF_RAILWAY", 46 }, { "SF_RIG", 46 }, { "SF_RIG_BOP", 46 },
            { "SF_RIG_GENERATOR", 46 }, { "SF_RIG_OVERHEAD_EQUIP", 46 }, { "SF_RIG_PUMP", 46 },
            { "SF_RIG_SHAKER", 46 }, { "SF_ROAD", 46 }, { "SF_TOWER", 46 },
            { "SF_VEHICLE", 46 }, { "SF_VESSEL", 46 }, { "SF_WASTE", 46 },
            { "SF_WASTE_DISPOSAL", 46 },

            // ========== 47. Units of Measure ==========
            { "PPDM_MEASUREMENT_SYSTEM", 47 }, { "PPDM_UNIT_OF_MEASURE", 47 }, { "PPDM_UOM_ALIAS", 47 },
            { "PPDM_UNIT_CONVERSION", 47 }, { "PPDM_DATA_STORE", 47 },

            // ========== 48. Volume Conversions ==========
            { "PPDM_VOL_MEAS_REGIME", 48 }, { "PPDM_VOL_MEAS_USE", 48 }, { "PPDM_VOL_MEAS_CONV", 48 },

            // ========== 49. Well Logs ==========
            { "WELL_LOG", 49 }, { "WELL_LOG_AXIS_COORD", 49 }, { "WELL_LOG_REMARK", 49 },
            { "WELL_LOG_PARM", 49 }, { "WELL_LOG_PARM_ARRAY", 49 }, { "WELL_LOG_PARM_CLASS", 49 },
            { "WELL_LOG_JOB", 49 }, { "WELL_LOG_PASS", 49 }, { "WELL_LOG_TRIP", 49 },
            { "WELL_LOG_TRIP_REMARK", 49 }, { "WELL_LOG_CLASS", 49 }, { "WELL_LOG_CLS_CRV_CLS", 49 },
            { "WELL_LOG_CRV_CLS_XREF", 49 }, { "WELL_LOG_CURVE_SPLICE", 49 }, { "WELL_LOG_DGTZ_CURVE", 49 },
            { "WELL_LOG_CURVE_SCALE", 49 }, { "WELL_LOG_CURVE", 49 }, { "WELL_LOG_CURVE_AXIS", 49 },
            { "WELL_LOG_CURVE_CLASS", 49 }, { "WELL_LOG_CURVE_FRAME", 49 }, { "WELL_LOG_CURVE_PROC", 49 },
            { "WELL_LOG_CURVE_REMARK", 49 }, { "WELL_LOG_CURVE_VALUE", 49 }, { "WELL_LOG_DICTIONARY", 49 },
            { "WELL_LOG_DICT_ALIAS", 49 }, { "WELL_LOG_DICT_BA", 49 }, { "WELL_LOG_DICT_CRV_CLS", 49 },
            { "WELL_LOG_DICT_CURVE", 49 }, { "WELL_LOG_DICT_PARM", 49 }, { "WELL_LOG_DICT_PARM_CLS", 49 },
            { "WELL_LOG_DICT_PARM_VAL", 49 }, { "WELL_LOG_DICT_PROC", 49 }, { "WELL_MUD_SAMPLE", 49 },
            { "WELL_MUD_PROPERTY", 49 }, { "WELL_MUD_RESISTIVITY", 49 },

            // ========== 50. Wells ==========
            { "WELL", 50 }, { "WELL_ALIAS", 50 }, { "WELL_AREA", 50 }, { "WELL_FACILITY", 50 },
            { "WELL_MISC_DATA", 50 }, { "WELL_SUPPORT_FACILITY", 50 }, { "WELL_REMARK", 50 },
            { "WELL_VERSION", 50 }, { "WELL_VERSION_AREA", 50 }, { "WELL_REF", 50 },
            { "WELL_COMPONENT", 50 }, { "WELL_TEST", 50 }, { "WELL_TEST_CUSHION", 50 },
            { "WELL_TEST_REMARK", 50 }, { "WELL_TEST_EQUIPMENT", 50 }, { "WELL_TEST_MUD", 50 },
            { "WELL_TEST_STRAT_UNIT", 50 }, { "WELL_TEST_PERIOD", 50 }, { "WELL_TEST_PRESS_MEAS", 50 },
            { "WELL_TEST_PRESSURE", 50 }, { "WELL_TEST_FLOW", 50 }, { "WELL_TEST_FLOW_MEAS", 50 },
            { "WELL_TEST_ANALYSIS", 50 }, { "WELL_TEST_RECORDER", 50 }, { "WELL_TEST_RECOVERY", 50 },
            { "WELL_TEST_CONTAMINANT", 50 }, { "WELL_TEST_COMPUT_ANAL", 50 }, { "WELL_DRILL_SHAKER", 50 },
            { "WELL_DRILL_ASSEMBLY", 50 }, { "WELL_DRILL_ASSEMBLY_COMP", 50 }, { "WELL_DRILL_ASSEMBLY_PER", 50 },
            { "WELL_EQUIPMENT", 50 }, { "WELL_DRILL_PIPE_INV", 50 }, { "WELL_TUBULAR", 50 },
            { "WELL_CEMENT", 50 }, { "WELL_AIR_DRILL", 50 }, { "WELL_AIR_DRILL_INTERVAL", 50 },
            { "WELL_AIR_DRILL_PERIOD", 50 }, { "WELL_HORIZ_DRILL", 50 }, { "WELL_HORIZ_DRILL_KOP", 50 },
            { "WELL_HORIZ_DRILL_POE", 50 }, { "WELL_HORIZ_DRILL_SPOKE", 50 }, { "WELL_SHOW", 50 },
            { "WELL_SHOW_REMARK", 50 }, { "WELL_DRILL_MUD", 50 }, { "WELL_DRILL_MUD_ADDITIVE", 50 },
            { "WELL_DRILL_MUD_INTERVAL", 50 }, { "WELL_DRILL_MUD_WEIGHT", 50 }, { "WELL_COMPLETION", 50 },
            { "WELL_PERFORATION", 50 }, { "WELL_PERF_REMARK", 50 }, { "WELL_TREATMENT", 50 },
            { "WELL_PLUGBACK", 50 }, { "WELL_DRILL_REPORT", 50 }, { "WELL_DRILL_PERIOD", 50 },
            { "WELL_DRILL_PERIOD_CREW", 50 }, { "WELL_DRILL_PERIOD_EQUIP", 50 }, { "WELL_DRILL_PERIOD_INV", 50 },
            { "WELL_DRILL_PERIOD_SERV", 50 }, { "WELL_DRILL_PERIOD_VESSEL", 50 }, { "WELL_DRILL_STATISTIC", 50 },
            { "WELL_DRILL_WEATHER", 50 }, { "WELL_DRILL_INT_DETAIL", 50 }, { "WELL_DRILL_REMARK", 50 },
            { "WELL_SIEVE_SCREEN", 50 }, { "WELL_SIEVE_ANALYSIS", 50 }, { "WELL_DRILL_CHECK", 50 },
            { "WELL_DRILL_CHECK_SET", 50 }, { "WELL_DRILL_CHECK_TYPE", 50 }, { "WELL_BA_SERVICE", 50 },
            { "LAND_RIGHT_WELL", 50 }, { "LAND_RIGHT_WELL_SUBST", 50 }, { "WELL_ACTIVITY", 50 },
            { "WELL_ACTIVITY_CAUSE", 50 }, { "WELL_ACTIVITY_DURATION", 50 }, { "WELL_ACTIVITY_COMPONENT", 50 },
            { "WELL_ACTIVITY_TIME", 50 }, { "WELL_ACTIVITY_TYPE", 50 }, { "WELL_ACTIVITY_TYPE_ALIAS", 50 },
            { "WELL_ACTIVITY_TYPE_EQUIN", 50 }, { "WELL_STATUS", 50 }, { "WELL_PRESSURE", 50 },
            { "WELL_PRESSURE_ADP", 50 }, { "WELL_PRESSURE_ADP_ART", 50 }, { "WELL_PRESSURE_BH", 50 },
            { "WELL_DRILL_BIT", 50 }, { "WELL_DRILL_BIT_CONDITION", 50 }, { "WELL_DRILL_BIT_INTERVAL", 50 },
            { "WELL_DRILL_BIT_JET", 50 }, { "WELL_DRILL_BIT_PERIOD", 50 }, { "WELL_LICENSE", 50 },
            { "WELL_LICENSE_ALIAS", 50 }, { "WELL_LICENSE_AREA", 50 }, { "WELL_LICENSE_COND", 50 },
            { "WELL_LICENSE_REMARK", 50 }, { "WELL_LICENSE_STATUS", 50 }, { "WELL_LICENSE_VIOLATION", 50 },
            { "WELL_CORE", 50 }, { "WELL_CORE_DESCRIPTION", 50 }, { "WELL_CORE_STRAT_UNIT", 50 },
            { "WELL_CORE_REMARK", 50 }, { "WELL_CORE_SAMPLE_ANAL", 50 }, { "WELL_CORE_SAMPLE_DESC", 50 },
            { "WELL_CORE_SHIFT", 50 }, { "WELL_CORE_ANALYSIS", 50 }, { "WELL_CORE_ANAL_REMARK", 50 },
            { "WELL_CORE_ANAL_METHOD", 50 }, { "WELL_NODE", 50 }, { "WELL_NODE_AREA", 50 },
            { "WELL_NODE_MB", 50 }, { "WELL_NODE_STRATUNIT", 50 }, { "WELL_NODE_VERSION", 50 },
            { "Z_WELL_NODE_GEOMETRY", 50 }, { "Z_WELL_GEOMETRY", 50 }, { "WELL_DIR_SRVY", 50 },
            { "WELL_DIR_SRVY_STATION", 50 }, { "WELL_DIR_SRVY_VERSION", 50 }, { "WELL_DIR_SRVY_COMPOSITE", 50 },
            { "WELL_POROUS_INTERVAL", 50 }, { "WELL_PAYZONE", 50 },
            { "ZONE", 50 }, { "WELL_ZONE_INTERVAL", 50 },
            { "WELL_ZONE_INTRVL_VALUE", 50 },

            // ========== 51. Work Orders ==========
            { "WORK_ORDER", 51 }, { "WORK_ORDER_DELIVERY", 51 }, { "WORK_ORDER_DELIVERY_COMP", 51 },
            { "WORK_ORDER_INSTRUCTION", 51 }, { "WORK_ORDER_INST_COMP", 51 }, { "WORK_ORDER_XREF", 51 },
            { "WORK_ORDER_ALIAS", 51 }, { "WORK_ORDER_BA", 51 }, { "WORK_ORDER_CONDITION", 51 },
            { "WORK_ORDER_COMPONENT", 51 },

            };

        // Build FK hierarchy from embedded PPDM39Metadata.json
        var parents = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var children = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        var assembly = typeof(PPDM39TableMapping).Assembly;
        using var stream = assembly.GetManifestResourceStream(
            "Beep.OilandGas.Branchs.Data.PPDM39Metadata.json");

        if (stream != null)
        {
            using var doc = JsonDocument.Parse(stream);
            foreach (var tableEntry in doc.RootElement.EnumerateObject())
            {
                string childTable = tableEntry.Name;
                if (!tableEntry.Value.TryGetProperty("ForeignKeys", out var fkArray))
                    continue;

                foreach (var fk in fkArray.EnumerateArray())
                {
                    if (!fk.TryGetProperty("ReferencedTable", out var parentProp))
                        continue;

                    string parentTable = parentProp.GetString() ?? string.Empty;
                    if (string.IsNullOrEmpty(parentTable) ||
                        string.Equals(parentTable, childTable, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!parents.ContainsKey(childTable))
                        parents[childTable] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    parents[childTable].Add(parentTable);

                    if (!children.ContainsKey(parentTable))
                        children[parentTable] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    children[parentTable].Add(childTable);
                }
            }
        }

        TableParents = parents.ToDictionary(
            k => k.Key,
            v => (IReadOnlyList<string>)v.Value.OrderBy(x => x).ToList(),
            StringComparer.OrdinalIgnoreCase);

        TableChildren = children.ToDictionary(
            k => k.Key,
            v => (IReadOnlyList<string>)v.Value.OrderBy(x => x).ToList(),
            StringComparer.OrdinalIgnoreCase);
    }

    public static int? GetCategoryIdForTable(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            return null;

        if (TableToCategoryMap.TryGetValue(tableName.ToUpperInvariant(), out int categoryId))
            return categoryId;

        return null;
    }

    public static PPDM39Category? GetCategoryForTable(string tableName)
    {
        int? categoryId = GetCategoryIdForTable(tableName);
        if (categoryId.HasValue)
        {
            return PPDM39Categories.GetAll().FirstOrDefault(c => c.Id == categoryId.Value);
        }
        return null;
    }

    public static List<string> GetTablesForCategory(int categoryId)
    {
        return TableToCategoryMap
            .Where(kvp => kvp.Value == categoryId)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    public static List<string> GetTablesForCategory(string categoryName)
    {
        var category = PPDM39Categories.GetAll()
            .FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.InvariantCultureIgnoreCase));
        
        if (category == null)
            return new List<string>();

        return GetTablesForCategory(category.Id);
    }

    public static Dictionary<string, int> GetAllTableMappings() => TableToCategoryMap;

    public static int TotalTables => TableToCategoryMap.Count;

    public static List<PPDM39Category> GetCategoriesWithTables()
    {
        return PPDM39Categories.GetAll()
            .Where(c => GetTablesForCategory(c.Id).Count > 0)
            .ToList();
    }

    public static List<PPDM39Category> GetCategoriesWithoutTables()
    {
        return PPDM39Categories.GetAll()
            .Where(c => GetTablesForCategory(c.Id).Count == 0)
            .ToList();
    }

    /// <summary>Returns the unique parent tables this table references via foreign keys.</summary>
    public static IReadOnlyList<string> GetParents(string tableName) =>
        !string.IsNullOrWhiteSpace(tableName) && TableParents.TryGetValue(tableName, out var p)
            ? p
            : Array.Empty<string>();

    /// <summary>Returns the tables that foreign-key-reference this table (its children).</summary>
    public static IReadOnlyList<string> GetChildren(string tableName) =>
        !string.IsNullOrWhiteSpace(tableName) && TableChildren.TryGetValue(tableName, out var c)
            ? c
            : Array.Empty<string>();

    /// <summary>Returns true when this table has no FK parents — it is a root entity.</summary>
    public static bool IsRootTable(string tableName) =>
        string.IsNullOrWhiteSpace(tableName) || !TableParents.ContainsKey(tableName);

    /// <summary>Returns true when no other table FK-references this table — it is a leaf node.</summary>
    public static bool IsLeafTable(string tableName) =>
        string.IsNullOrWhiteSpace(tableName) || !TableChildren.ContainsKey(tableName);
}

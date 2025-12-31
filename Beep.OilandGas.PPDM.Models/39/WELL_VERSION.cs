using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_VERSION: Entity,IPPDMEntity

{

private  System.String UWIValue; 
 public System.String UWI
        {  
            get  
            {  
                return this.UWIValue;  
            }  

          set { SetProperty(ref  UWIValue, value); }
        } 
private  System.String SOURCEValue; 
 public System.String SOURCE
        {  
            get  
            {  
                return this.SOURCEValue;  
            }  

          set { SetProperty(ref  SOURCEValue, value); }
        } 
private  System.DateTime? ABANDONMENT_DATEValue; 
 public System.DateTime? ABANDONMENT_DATE
        {  
            get  
            {  
                return this.ABANDONMENT_DATEValue;  
            }  

          set { SetProperty(ref  ABANDONMENT_DATEValue, value); }
        } 
private  System.String ACTIVE_INDValue; 
 public System.String ACTIVE_IND
        {  
            get  
            {  
                return this.ACTIVE_INDValue;  
            }  

          set { SetProperty(ref  ACTIVE_INDValue, value); }
        } 
private  System.String ASSIGNED_FIELDValue; 
 public System.String ASSIGNED_FIELD
        {  
            get  
            {  
                return this.ASSIGNED_FIELDValue;  
            }  

          set { SetProperty(ref  ASSIGNED_FIELDValue, value); }
        } 
private  System.Decimal BASE_DEPTHValue; 
 public System.Decimal BASE_DEPTH
        {  
            get  
            {  
                return this.BASE_DEPTHValue;  
            }  

          set { SetProperty(ref  BASE_DEPTHValue, value); }
        } 
private  System.String BASE_DEPTH_OUOMValue; 
 public System.String BASE_DEPTH_OUOM
        {  
            get  
            {  
                return this.BASE_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  BASE_DEPTH_OUOMValue, value); }
        } 
private  System.String BASE_NODE_IDValue; 
 public System.String BASE_NODE_ID
        {  
            get  
            {  
                return this.BASE_NODE_IDValue;  
            }  

          set { SetProperty(ref  BASE_NODE_IDValue, value); }
        } 
private  System.String BASE_STRAT_NAME_SET_IDValue; 
 public System.String BASE_STRAT_NAME_SET_ID
        {  
            get  
            {  
                return this.BASE_STRAT_NAME_SET_IDValue;  
            }  

          set { SetProperty(ref  BASE_STRAT_NAME_SET_IDValue, value); }
        } 
private  System.String BASE_STRAT_UNIT_IDValue; 
 public System.String BASE_STRAT_UNIT_ID
        {  
            get  
            {  
                return this.BASE_STRAT_UNIT_IDValue;  
            }  

          set { SetProperty(ref  BASE_STRAT_UNIT_IDValue, value); }
        } 
private  System.Decimal BOTTOM_HOLE_LATITUDEValue; 
 public System.Decimal BOTTOM_HOLE_LATITUDE
        {  
            get  
            {  
                return this.BOTTOM_HOLE_LATITUDEValue;  
            }  

          set { SetProperty(ref  BOTTOM_HOLE_LATITUDEValue, value); }
        } 
private  System.Decimal BOTTOM_HOLE_LONGITUDEValue; 
 public System.Decimal BOTTOM_HOLE_LONGITUDE
        {  
            get  
            {  
                return this.BOTTOM_HOLE_LONGITUDEValue;  
            }  

          set { SetProperty(ref  BOTTOM_HOLE_LONGITUDEValue, value); }
        } 
private  System.Decimal CASING_FLANGE_ELEVValue; 
 public System.Decimal CASING_FLANGE_ELEV
        {  
            get  
            {  
                return this.CASING_FLANGE_ELEVValue;  
            }  

          set { SetProperty(ref  CASING_FLANGE_ELEVValue, value); }
        } 
private  System.String CASING_FLANGE_ELEV_OUOMValue; 
 public System.String CASING_FLANGE_ELEV_OUOM
        {  
            get  
            {  
                return this.CASING_FLANGE_ELEV_OUOMValue;  
            }  

          set { SetProperty(ref  CASING_FLANGE_ELEV_OUOMValue, value); }
        } 
private  System.DateTime? COMPLETION_DATEValue; 
 public System.DateTime? COMPLETION_DATE
        {  
            get  
            {  
                return this.COMPLETION_DATEValue;  
            }  

          set { SetProperty(ref  COMPLETION_DATEValue, value); }
        } 
private  System.DateTime? CONFIDENTIAL_DATEValue; 
 public System.DateTime? CONFIDENTIAL_DATE
        {  
            get  
            {  
                return this.CONFIDENTIAL_DATEValue;  
            }  

          set { SetProperty(ref  CONFIDENTIAL_DATEValue, value); }
        } 
private  System.Decimal CONFIDENTIAL_DEPTHValue; 
 public System.Decimal CONFIDENTIAL_DEPTH
        {  
            get  
            {  
                return this.CONFIDENTIAL_DEPTHValue;  
            }  

          set { SetProperty(ref  CONFIDENTIAL_DEPTHValue, value); }
        } 
private  System.String CONFIDENTIAL_DEPTH_OUOMValue; 
 public System.String CONFIDENTIAL_DEPTH_OUOM
        {  
            get  
            {  
                return this.CONFIDENTIAL_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  CONFIDENTIAL_DEPTH_OUOMValue, value); }
        } 
private  System.String CONFIDENTIAL_TYPEValue; 
 public System.String CONFIDENTIAL_TYPE
        {  
            get  
            {  
                return this.CONFIDENTIAL_TYPEValue;  
            }  

          set { SetProperty(ref  CONFIDENTIAL_TYPEValue, value); }
        } 
private  System.String CONFID_STRAT_NAME_SET_IDValue; 
 public System.String CONFID_STRAT_NAME_SET_ID
        {  
            get  
            {  
                return this.CONFID_STRAT_NAME_SET_IDValue;  
            }  

          set { SetProperty(ref  CONFID_STRAT_NAME_SET_IDValue, value); }
        } 
private  System.String CONFID_STRAT_UNIT_IDValue; 
 public System.String CONFID_STRAT_UNIT_ID
        {  
            get  
            {  
                return this.CONFID_STRAT_UNIT_IDValue;  
            }  

          set { SetProperty(ref  CONFID_STRAT_UNIT_IDValue, value); }
        } 
private  System.String CURRENT_CLASSValue; 
 public System.String CURRENT_CLASS
        {  
            get  
            {  
                return this.CURRENT_CLASSValue;  
            }  

          set { SetProperty(ref  CURRENT_CLASSValue, value); }
        } 
private  System.String CURRENT_STATUSValue; 
 public System.String CURRENT_STATUS
        {  
            get  
            {  
                return this.CURRENT_STATUSValue;  
            }  

          set { SetProperty(ref  CURRENT_STATUSValue, value); }
        } 
private  System.DateTime? CURRENT_STATUS_DATEValue; 
 public System.DateTime? CURRENT_STATUS_DATE
        {  
            get  
            {  
                return this.CURRENT_STATUS_DATEValue;  
            }  

          set { SetProperty(ref  CURRENT_STATUS_DATEValue, value); }
        } 
private  System.Decimal DEEPEST_DEPTHValue; 
 public System.Decimal DEEPEST_DEPTH
        {  
            get  
            {  
                return this.DEEPEST_DEPTHValue;  
            }  

          set { SetProperty(ref  DEEPEST_DEPTHValue, value); }
        } 
private  System.String DEEPEST_DEPTH_OUOMValue; 
 public System.String DEEPEST_DEPTH_OUOM
        {  
            get  
            {  
                return this.DEEPEST_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  DEEPEST_DEPTH_OUOMValue, value); }
        } 
private  System.String DEPTH_DATUMValue; 
 public System.String DEPTH_DATUM
        {  
            get  
            {  
                return this.DEPTH_DATUMValue;  
            }  

          set { SetProperty(ref  DEPTH_DATUMValue, value); }
        } 
private  System.Decimal DEPTH_DATUM_ELEVValue; 
 public System.Decimal DEPTH_DATUM_ELEV
        {  
            get  
            {  
                return this.DEPTH_DATUM_ELEVValue;  
            }  

          set { SetProperty(ref  DEPTH_DATUM_ELEVValue, value); }
        } 
private  System.String DEPTH_DATUM_ELEV_OUOMValue; 
 public System.String DEPTH_DATUM_ELEV_OUOM
        {  
            get  
            {  
                return this.DEPTH_DATUM_ELEV_OUOMValue;  
            }  

          set { SetProperty(ref  DEPTH_DATUM_ELEV_OUOMValue, value); }
        } 
private  System.Decimal DERRICK_FLOOR_ELEVValue; 
 public System.Decimal DERRICK_FLOOR_ELEV
        {  
            get  
            {  
                return this.DERRICK_FLOOR_ELEVValue;  
            }  

          set { SetProperty(ref  DERRICK_FLOOR_ELEVValue, value); }
        } 
private  System.String DERRICK_FLOOR_ELEV_OUOMValue; 
 public System.String DERRICK_FLOOR_ELEV_OUOM
        {  
            get  
            {  
                return this.DERRICK_FLOOR_ELEV_OUOMValue;  
            }  

          set { SetProperty(ref  DERRICK_FLOOR_ELEV_OUOMValue, value); }
        } 
private  System.Decimal DIFFERENCE_LAT_MSLValue; 
 public System.Decimal DIFFERENCE_LAT_MSL
        {  
            get  
            {  
                return this.DIFFERENCE_LAT_MSLValue;  
            }  

          set { SetProperty(ref  DIFFERENCE_LAT_MSLValue, value); }
        } 
private  System.String DISCOVERY_INDValue; 
 public System.String DISCOVERY_IND
        {  
            get  
            {  
                return this.DISCOVERY_INDValue;  
            }  

          set { SetProperty(ref  DISCOVERY_INDValue, value); }
        } 
private  System.Decimal DRILL_TDValue; 
 public System.Decimal DRILL_TD
        {  
            get  
            {  
                return this.DRILL_TDValue;  
            }  

          set { SetProperty(ref  DRILL_TDValue, value); }
        } 
private  System.String DRILL_TD_OUOMValue; 
 public System.String DRILL_TD_OUOM
        {  
            get  
            {  
                return this.DRILL_TD_OUOMValue;  
            }  

          set { SetProperty(ref  DRILL_TD_OUOMValue, value); }
        } 
private  System.DateTime? EFFECTIVE_DATEValue; 
 public System.DateTime? EFFECTIVE_DATE
        {  
            get  
            {  
                return this.EFFECTIVE_DATEValue;  
            }  

          set { SetProperty(ref  EFFECTIVE_DATEValue, value); }
        } 
private  System.String ELEV_REF_DATUMValue; 
 public System.String ELEV_REF_DATUM
        {  
            get  
            {  
                return this.ELEV_REF_DATUMValue;  
            }  

          set { SetProperty(ref  ELEV_REF_DATUMValue, value); }
        } 
private  System.String ENVIRONMENT_TYPEValue; 
 public System.String ENVIRONMENT_TYPE
        {  
            get  
            {  
                return this.ENVIRONMENT_TYPEValue;  
            }  

          set { SetProperty(ref  ENVIRONMENT_TYPEValue, value); }
        } 
private  System.DateTime? EXPIRY_DATEValue; 
 public System.DateTime? EXPIRY_DATE
        {  
            get  
            {  
                return this.EXPIRY_DATEValue;  
            }  

          set { SetProperty(ref  EXPIRY_DATEValue, value); }
        } 
private  System.String FAULTED_INDValue; 
 public System.String FAULTED_IND
        {  
            get  
            {  
                return this.FAULTED_INDValue;  
            }  

          set { SetProperty(ref  FAULTED_INDValue, value); }
        } 
private  System.DateTime? FINAL_DRILL_DATEValue; 
 public System.DateTime? FINAL_DRILL_DATE
        {  
            get  
            {  
                return this.FINAL_DRILL_DATEValue;  
            }  

          set { SetProperty(ref  FINAL_DRILL_DATEValue, value); }
        } 
private  System.Decimal FINAL_TDValue; 
 public System.Decimal FINAL_TD
        {  
            get  
            {  
                return this.FINAL_TDValue;  
            }  

          set { SetProperty(ref  FINAL_TDValue, value); }
        } 
private  System.String FINAL_TD_OUOMValue; 
 public System.String FINAL_TD_OUOM
        {  
            get  
            {  
                return this.FINAL_TD_OUOMValue;  
            }  

          set { SetProperty(ref  FINAL_TD_OUOMValue, value); }
        } 
private  System.Decimal GROUND_ELEVValue; 
 public System.Decimal GROUND_ELEV
        {  
            get  
            {  
                return this.GROUND_ELEVValue;  
            }  

          set { SetProperty(ref  GROUND_ELEVValue, value); }
        } 
private  System.String GROUND_ELEV_OUOMValue; 
 public System.String GROUND_ELEV_OUOM
        {  
            get  
            {  
                return this.GROUND_ELEV_OUOMValue;  
            }  

          set { SetProperty(ref  GROUND_ELEV_OUOMValue, value); }
        } 
private  System.String GROUND_ELEV_TYPEValue; 
 public System.String GROUND_ELEV_TYPE
        {  
            get  
            {  
                return this.GROUND_ELEV_TYPEValue;  
            }  

          set { SetProperty(ref  GROUND_ELEV_TYPEValue, value); }
        } 
private  System.String INITIAL_CLASSValue; 
 public System.String INITIAL_CLASS
        {  
            get  
            {  
                return this.INITIAL_CLASSValue;  
            }  

          set { SetProperty(ref  INITIAL_CLASSValue, value); }
        } 
private  System.Decimal KB_ELEVValue; 
 public System.Decimal KB_ELEV
        {  
            get  
            {  
                return this.KB_ELEVValue;  
            }  

          set { SetProperty(ref  KB_ELEVValue, value); }
        } 
private  System.String KB_ELEV_OUOMValue; 
 public System.String KB_ELEV_OUOM
        {  
            get  
            {  
                return this.KB_ELEV_OUOMValue;  
            }  

          set { SetProperty(ref  KB_ELEV_OUOMValue, value); }
        } 
private  System.String LEASE_NAMEValue; 
 public System.String LEASE_NAME
        {  
            get  
            {  
                return this.LEASE_NAMEValue;  
            }  

          set { SetProperty(ref  LEASE_NAMEValue, value); }
        } 
private  System.String LEASE_NUMValue; 
 public System.String LEASE_NUM
        {  
            get  
            {  
                return this.LEASE_NUMValue;  
            }  

          set { SetProperty(ref  LEASE_NUMValue, value); }
        } 
private  System.String LEGAL_SURVEY_TYPEValue; 
 public System.String LEGAL_SURVEY_TYPE
        {  
            get  
            {  
                return this.LEGAL_SURVEY_TYPEValue;  
            }  

          set { SetProperty(ref  LEGAL_SURVEY_TYPEValue, value); }
        } 
private  System.String LOCATION_TYPEValue; 
 public System.String LOCATION_TYPE
        {  
            get  
            {  
                return this.LOCATION_TYPEValue;  
            }  

          set { SetProperty(ref  LOCATION_TYPEValue, value); }
        } 
private  System.Decimal LOG_TDValue; 
 public System.Decimal LOG_TD
        {  
            get  
            {  
                return this.LOG_TDValue;  
            }  

          set { SetProperty(ref  LOG_TDValue, value); }
        } 
private  System.String LOG_TD_OUOMValue; 
 public System.String LOG_TD_OUOM
        {  
            get  
            {  
                return this.LOG_TD_OUOMValue;  
            }  

          set { SetProperty(ref  LOG_TD_OUOMValue, value); }
        } 
private  System.Decimal MAX_TVDValue; 
 public System.Decimal MAX_TVD
        {  
            get  
            {  
                return this.MAX_TVDValue;  
            }  

          set { SetProperty(ref  MAX_TVDValue, value); }
        } 
private  System.String MAX_TVD_OUOMValue; 
 public System.String MAX_TVD_OUOM
        {  
            get  
            {  
                return this.MAX_TVD_OUOMValue;  
            }  

          set { SetProperty(ref  MAX_TVD_OUOMValue, value); }
        } 
private  System.Decimal NET_PAYValue; 
 public System.Decimal NET_PAY
        {  
            get  
            {  
                return this.NET_PAYValue;  
            }  

          set { SetProperty(ref  NET_PAYValue, value); }
        } 
private  System.String NET_PAY_OUOMValue; 
 public System.String NET_PAY_OUOM
        {  
            get  
            {  
                return this.NET_PAY_OUOMValue;  
            }  

          set { SetProperty(ref  NET_PAY_OUOMValue, value); }
        } 
private  System.String OLDEST_STRAT_AGEValue; 
 public System.String OLDEST_STRAT_AGE
        {  
            get  
            {  
                return this.OLDEST_STRAT_AGEValue;  
            }  

          set { SetProperty(ref  OLDEST_STRAT_AGEValue, value); }
        } 
private  System.String OLDEST_STRAT_NAME_SET_AGEValue; 
 public System.String OLDEST_STRAT_NAME_SET_AGE
        {  
            get  
            {  
                return this.OLDEST_STRAT_NAME_SET_AGEValue;  
            }  

          set { SetProperty(ref  OLDEST_STRAT_NAME_SET_AGEValue, value); }
        } 
private  System.String OLDEST_STRAT_NAME_SET_IDValue; 
 public System.String OLDEST_STRAT_NAME_SET_ID
        {  
            get  
            {  
                return this.OLDEST_STRAT_NAME_SET_IDValue;  
            }  

          set { SetProperty(ref  OLDEST_STRAT_NAME_SET_IDValue, value); }
        } 
private  System.String OLDEST_STRAT_UNIT_IDValue; 
 public System.String OLDEST_STRAT_UNIT_ID
        {  
            get  
            {  
                return this.OLDEST_STRAT_UNIT_IDValue;  
            }  

          set { SetProperty(ref  OLDEST_STRAT_UNIT_IDValue, value); }
        } 
private  System.String OPERATORValue; 
 public System.String OPERATOR
        {  
            get  
            {  
                return this.OPERATORValue;  
            }  

          set { SetProperty(ref  OPERATORValue, value); }
        } 
private  System.String PLATFORM_IDValue; 
 public System.String PLATFORM_ID
        {  
            get  
            {  
                return this.PLATFORM_IDValue;  
            }  

          set { SetProperty(ref  PLATFORM_IDValue, value); }
        } 
private  System.String PLATFORM_SF_SUBTYPEValue; 
 public System.String PLATFORM_SF_SUBTYPE
        {  
            get  
            {  
                return this.PLATFORM_SF_SUBTYPEValue;  
            }  

          set { SetProperty(ref  PLATFORM_SF_SUBTYPEValue, value); }
        } 
private  System.String PLOT_NAMEValue; 
 public System.String PLOT_NAME
        {  
            get  
            {  
                return this.PLOT_NAMEValue;  
            }  

          set { SetProperty(ref  PLOT_NAMEValue, value); }
        } 
private  System.String PLOT_SYMBOLValue; 
 public System.String PLOT_SYMBOL
        {  
            get  
            {  
                return this.PLOT_SYMBOLValue;  
            }  

          set { SetProperty(ref  PLOT_SYMBOLValue, value); }
        } 
private  System.Decimal PLUGBACK_DEPTHValue; 
 public System.Decimal PLUGBACK_DEPTH
        {  
            get  
            {  
                return this.PLUGBACK_DEPTHValue;  
            }  

          set { SetProperty(ref  PLUGBACK_DEPTHValue, value); }
        } 
private  System.String PLUGBACK_DEPTH_OUOMValue; 
 public System.String PLUGBACK_DEPTH_OUOM
        {  
            get  
            {  
                return this.PLUGBACK_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  PLUGBACK_DEPTH_OUOMValue, value); }
        } 
private  System.String PPDM_GUIDValue; 
 public System.String PPDM_GUID
        {  
            get  
            {  
                return this.PPDM_GUIDValue;  
            }  

          set { SetProperty(ref  PPDM_GUIDValue, value); }
        } 
private  System.String PROFILE_TYPEValue; 
 public System.String PROFILE_TYPE
        {  
            get  
            {  
                return this.PROFILE_TYPEValue;  
            }  

          set { SetProperty(ref  PROFILE_TYPEValue, value); }
        } 
private  System.String REGULATORY_AGENCYValue; 
 public System.String REGULATORY_AGENCY
        {  
            get  
            {  
                return this.REGULATORY_AGENCYValue;  
            }  

          set { SetProperty(ref  REGULATORY_AGENCYValue, value); }
        } 
private  System.String REMARKValue; 
 public System.String REMARK
        {  
            get  
            {  
                return this.REMARKValue;  
            }  

          set { SetProperty(ref  REMARKValue, value); }
        } 
private  System.DateTime? RIG_ON_SITE_DATEValue; 
 public System.DateTime? RIG_ON_SITE_DATE
        {  
            get  
            {  
                return this.RIG_ON_SITE_DATEValue;  
            }  

          set { SetProperty(ref  RIG_ON_SITE_DATEValue, value); }
        } 
private  System.DateTime? RIG_RELEASE_DATEValue; 
 public System.DateTime? RIG_RELEASE_DATE
        {  
            get  
            {  
                return this.RIG_RELEASE_DATEValue;  
            }  

          set { SetProperty(ref  RIG_RELEASE_DATEValue, value); }
        } 
private  System.Decimal ROTARY_TABLE_ELEVValue; 
 public System.Decimal ROTARY_TABLE_ELEV
        {  
            get  
            {  
                return this.ROTARY_TABLE_ELEVValue;  
            }  

          set { SetProperty(ref  ROTARY_TABLE_ELEVValue, value); }
        } 
private  System.String ROTARY_TABLE_ELEV_OUOMValue; 
 public System.String ROTARY_TABLE_ELEV_OUOM
        {  
            get  
            {  
                return this.ROTARY_TABLE_ELEV_OUOMValue;  
            }  

          set { SetProperty(ref  ROTARY_TABLE_ELEV_OUOMValue, value); }
        } 
private  System.String SOURCE_DOCUMENT_IDValue; 
 public System.String SOURCE_DOCUMENT_ID
        {  
            get  
            {  
                return this.SOURCE_DOCUMENT_IDValue;  
            }  

          set { SetProperty(ref  SOURCE_DOCUMENT_IDValue, value); }
        } 
private  System.DateTime? SPUD_DATEValue; 
 public System.DateTime? SPUD_DATE
        {  
            get  
            {  
                return this.SPUD_DATEValue;  
            }  

          set { SetProperty(ref  SPUD_DATEValue, value); }
        } 
private  System.String STATUS_TYPEValue; 
 public System.String STATUS_TYPE
        {  
            get  
            {  
                return this.STATUS_TYPEValue;  
            }  

          set { SetProperty(ref  STATUS_TYPEValue, value); }
        } 
private  System.String SUBSEA_ELEV_REF_TYPEValue; 
 public System.String SUBSEA_ELEV_REF_TYPE
        {  
            get  
            {  
                return this.SUBSEA_ELEV_REF_TYPEValue;  
            }  

          set { SetProperty(ref  SUBSEA_ELEV_REF_TYPEValue, value); }
        } 
private  System.Decimal SURFACE_LATITUDEValue; 
 public System.Decimal SURFACE_LATITUDE
        {  
            get  
            {  
                return this.SURFACE_LATITUDEValue;  
            }  

          set { SetProperty(ref  SURFACE_LATITUDEValue, value); }
        } 
private  System.Decimal SURFACE_LONGITUDEValue; 
 public System.Decimal SURFACE_LONGITUDE
        {  
            get  
            {  
                return this.SURFACE_LONGITUDEValue;  
            }  

          set { SetProperty(ref  SURFACE_LONGITUDEValue, value); }
        } 
private  System.String SURFACE_NODE_IDValue; 
 public System.String SURFACE_NODE_ID
        {  
            get  
            {  
                return this.SURFACE_NODE_IDValue;  
            }  

          set { SetProperty(ref  SURFACE_NODE_IDValue, value); }
        } 
private  System.String TAX_CREDIT_CODEValue; 
 public System.String TAX_CREDIT_CODE
        {  
            get  
            {  
                return this.TAX_CREDIT_CODEValue;  
            }  

          set { SetProperty(ref  TAX_CREDIT_CODEValue, value); }
        } 
private  System.String TD_STRAT_AGEValue; 
 public System.String TD_STRAT_AGE
        {  
            get  
            {  
                return this.TD_STRAT_AGEValue;  
            }  

          set { SetProperty(ref  TD_STRAT_AGEValue, value); }
        } 
private  System.String TD_STRAT_NAME_SET_AGEValue; 
 public System.String TD_STRAT_NAME_SET_AGE
        {  
            get  
            {  
                return this.TD_STRAT_NAME_SET_AGEValue;  
            }  

          set { SetProperty(ref  TD_STRAT_NAME_SET_AGEValue, value); }
        } 
private  System.String TD_STRAT_NAME_SET_IDValue; 
 public System.String TD_STRAT_NAME_SET_ID
        {  
            get  
            {  
                return this.TD_STRAT_NAME_SET_IDValue;  
            }  

          set { SetProperty(ref  TD_STRAT_NAME_SET_IDValue, value); }
        } 
private  System.String TD_STRAT_UNIT_IDValue; 
 public System.String TD_STRAT_UNIT_ID
        {  
            get  
            {  
                return this.TD_STRAT_UNIT_IDValue;  
            }  

          set { SetProperty(ref  TD_STRAT_UNIT_IDValue, value); }
        } 
private  System.Decimal TOP_DEPTHValue; 
 public System.Decimal TOP_DEPTH
        {  
            get  
            {  
                return this.TOP_DEPTHValue;  
            }  

          set { SetProperty(ref  TOP_DEPTHValue, value); }
        } 
private  System.String TOP_DEPTH_OUOMValue; 
 public System.String TOP_DEPTH_OUOM
        {  
            get  
            {  
                return this.TOP_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  TOP_DEPTH_OUOMValue, value); }
        } 
private  System.String TOP_STRAT_NAME_SET_IDValue; 
 public System.String TOP_STRAT_NAME_SET_ID
        {  
            get  
            {  
                return this.TOP_STRAT_NAME_SET_IDValue;  
            }  

          set { SetProperty(ref  TOP_STRAT_NAME_SET_IDValue, value); }
        } 
private  System.String TOP_STRAT_UNIT_IDValue; 
 public System.String TOP_STRAT_UNIT_ID
        {  
            get  
            {  
                return this.TOP_STRAT_UNIT_IDValue;  
            }  

          set { SetProperty(ref  TOP_STRAT_UNIT_IDValue, value); }
        } 
private  System.Decimal WATER_ACOUSTIC_VELValue; 
 public System.Decimal WATER_ACOUSTIC_VEL
        {  
            get  
            {  
                return this.WATER_ACOUSTIC_VELValue;  
            }  

          set { SetProperty(ref  WATER_ACOUSTIC_VELValue, value); }
        } 
private  System.String WATER_ACOUSTIC_VEL_OUOMValue; 
 public System.String WATER_ACOUSTIC_VEL_OUOM
        {  
            get  
            {  
                return this.WATER_ACOUSTIC_VEL_OUOMValue;  
            }  

          set { SetProperty(ref  WATER_ACOUSTIC_VEL_OUOMValue, value); }
        } 
private  System.Decimal WATER_DEPTHValue; 
 public System.Decimal WATER_DEPTH
        {  
            get  
            {  
                return this.WATER_DEPTHValue;  
            }  

          set { SetProperty(ref  WATER_DEPTHValue, value); }
        } 
private  System.String WATER_DEPTH_DATUMValue; 
 public System.String WATER_DEPTH_DATUM
        {  
            get  
            {  
                return this.WATER_DEPTH_DATUMValue;  
            }  

          set { SetProperty(ref  WATER_DEPTH_DATUMValue, value); }
        } 
private  System.String WATER_DEPTH_OUOMValue; 
 public System.String WATER_DEPTH_OUOM
        {  
            get  
            {  
                return this.WATER_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  WATER_DEPTH_OUOMValue, value); }
        } 
private  System.String WELL_EVENT_NUMValue; 
 public System.String WELL_EVENT_NUM
        {  
            get  
            {  
                return this.WELL_EVENT_NUMValue;  
            }  

          set { SetProperty(ref  WELL_EVENT_NUMValue, value); }
        } 
private  System.String WELL_GOVERNMENT_IDValue; 
 public System.String WELL_GOVERNMENT_ID
        {  
            get  
            {  
                return this.WELL_GOVERNMENT_IDValue;  
            }  

          set { SetProperty(ref  WELL_GOVERNMENT_IDValue, value); }
        } 
private  System.Decimal WELL_INTERSECT_MDValue; 
 public System.Decimal WELL_INTERSECT_MD
        {  
            get  
            {  
                return this.WELL_INTERSECT_MDValue;  
            }  

          set { SetProperty(ref  WELL_INTERSECT_MDValue, value); }
        } 
private  System.String WELL_LEVEL_TYPEValue; 
 public System.String WELL_LEVEL_TYPE
        {  
            get  
            {  
                return this.WELL_LEVEL_TYPEValue;  
            }  

          set { SetProperty(ref  WELL_LEVEL_TYPEValue, value); }
        } 
private  System.String WELL_NAMEValue; 
 public System.String WELL_NAME
        {  
            get  
            {  
                return this.WELL_NAMEValue;  
            }  

          set { SetProperty(ref  WELL_NAMEValue, value); }
        } 
private  System.String WELL_NUMValue; 
 public System.String WELL_NUM
        {  
            get  
            {  
                return this.WELL_NUMValue;  
            }  

          set { SetProperty(ref  WELL_NUMValue, value); }
        } 
private  System.Decimal WHIPSTOCK_DEPTHValue; 
 public System.Decimal WHIPSTOCK_DEPTH
        {  
            get  
            {  
                return this.WHIPSTOCK_DEPTHValue;  
            }  

          set { SetProperty(ref  WHIPSTOCK_DEPTHValue, value); }
        } 
private  System.String WHIPSTOCK_DEPTH_OUOMValue; 
 public System.String WHIPSTOCK_DEPTH_OUOM
        {  
            get  
            {  
                return this.WHIPSTOCK_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  WHIPSTOCK_DEPTH_OUOMValue, value); }
        } 
private  System.String ROW_CHANGED_BYValue; 
 public System.String ROW_CHANGED_BY
        {  
            get  
            {  
                return this.ROW_CHANGED_BYValue;  
            }  

          set { SetProperty(ref  ROW_CHANGED_BYValue, value); }
        } 
private  System.DateTime? ROW_CHANGED_DATEValue; 
 public System.DateTime? ROW_CHANGED_DATE
        {  
            get  
            {  
                return this.ROW_CHANGED_DATEValue;  
            }  

          set { SetProperty(ref  ROW_CHANGED_DATEValue, value); }
        } 
private  System.String ROW_CREATED_BYValue; 
 public System.String ROW_CREATED_BY
        {  
            get  
            {  
                return this.ROW_CREATED_BYValue;  
            }  

          set { SetProperty(ref  ROW_CREATED_BYValue, value); }
        } 
private  System.DateTime? ROW_CREATED_DATEValue; 
 public System.DateTime? ROW_CREATED_DATE
        {  
            get  
            {  
                return this.ROW_CREATED_DATEValue;  
            }  

          set { SetProperty(ref  ROW_CREATED_DATEValue, value); }
        } 
private  System.DateTime? ROW_EFFECTIVE_DATEValue; 
 public System.DateTime? ROW_EFFECTIVE_DATE
        {  
            get  
            {  
                return this.ROW_EFFECTIVE_DATEValue;  
            }  

          set { SetProperty(ref  ROW_EFFECTIVE_DATEValue, value); }
        } 
private  System.DateTime? ROW_EXPIRY_DATEValue; 
 public System.DateTime? ROW_EXPIRY_DATE
        {  
            get  
            {  
                return this.ROW_EXPIRY_DATEValue;  
            }  

          set { SetProperty(ref  ROW_EXPIRY_DATEValue, value); }
        } 
private  System.String ROW_QUALITYValue; 
 public System.String ROW_QUALITY
        {  
            get  
            {  
                return this.ROW_QUALITYValue;  
            }  

          set { SetProperty(ref  ROW_QUALITYValue, value); }
        } 


    public WELL_VERSION () { }

  }
}


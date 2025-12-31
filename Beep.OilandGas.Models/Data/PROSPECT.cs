using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.Models.Data 
{
public partial class PROSPECT: Entity,IPPDMEntity

{

private  System.String PROSPECT_IDValue; 
 public System.String PROSPECT_ID
        {  
            get  
            {  
                return this.PROSPECT_IDValue;  
            }  

          set { SetProperty(ref  PROSPECT_IDValue, value); }
        } 
private  System.String PROSPECT_NAMEValue; 
 public System.String PROSPECT_NAME
        {  
            get  
            {  
                return this.PROSPECT_NAMEValue;  
            }  

          set { SetProperty(ref  PROSPECT_NAMEValue, value); }
        } 
private  System.String PROSPECT_SHORT_NAMEValue; 
 public System.String PROSPECT_SHORT_NAME
        {  
            get  
            {  
                return this.PROSPECT_SHORT_NAMEValue;  
            }  

          set { SetProperty(ref  PROSPECT_SHORT_NAMEValue, value); }
        } 
private  System.String PRIMARY_FIELD_IDValue; 
 public System.String PRIMARY_FIELD_ID
        {  
            get  
            {  
                return this.PRIMARY_FIELD_IDValue;  
            }  

          set { SetProperty(ref  PRIMARY_FIELD_IDValue, value); }
        } 
private  System.String PLAY_IDValue; 
 public System.String PLAY_ID
        {  
            get  
            {  
                return this.PLAY_IDValue;  
            }  

          set { SetProperty(ref  PLAY_IDValue, value); }
        } 
private  System.String PROSPECT_TYPEValue; 
 public System.String PROSPECT_TYPE
        {  
            get  
            {  
                return this.PROSPECT_TYPEValue;  
            }  

          set { SetProperty(ref  PROSPECT_TYPEValue, value); }
        } 
private  System.String PROSPECT_STATUSValue; 
 public System.String PROSPECT_STATUS
        {  
            get  
            {  
                return this.PROSPECT_STATUSValue;  
            }  

          set { SetProperty(ref  PROSPECT_STATUSValue, value); }
        } 
private  System.String RISK_LEVELValue; 
 public System.String RISK_LEVEL
        {  
            get  
            {  
                return this.RISK_LEVELValue;  
            }  

          set { SetProperty(ref  RISK_LEVELValue, value); }
        } 
private  System.String CURRENT_OPERATORValue; 
 public System.String CURRENT_OPERATOR
        {  
            get  
            {  
                return this.CURRENT_OPERATORValue;  
            }  

          set { SetProperty(ref  CURRENT_OPERATORValue, value); }
        } 
private  System.String DESCRIPTIONValue; 
 public System.String DESCRIPTION
        {  
            get  
            {  
                return this.DESCRIPTIONValue;  
            }  

          set { SetProperty(ref  DESCRIPTIONValue, value); }
        } 
private  System.DateTime? DISCOVERY_DATEValue; 
 public System.DateTime? DISCOVERY_DATE
        {  
            get  
            {  
                return this.DISCOVERY_DATEValue;  
            }  

          set { SetProperty(ref  DISCOVERY_DATEValue, value); }
        } 
private  System.DateTime? FIRST_DRILL_DATEValue; 
 public System.DateTime? FIRST_DRILL_DATE
        {  
            get  
            {  
                return this.FIRST_DRILL_DATEValue;  
            }  

          set { SetProperty(ref  FIRST_DRILL_DATEValue, value); }
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
private  System.Decimal? LATITUDEValue; 
 public System.Decimal? LATITUDE
        {  
            get  
            {  
                return this.LATITUDEValue;  
            }  

          set { SetProperty(ref  LATITUDEValue, value); }
        } 
private  System.Decimal? LONGITUDEValue; 
 public System.Decimal? LONGITUDE
        {  
            get  
            {  
                return this.LONGITUDEValue;  
            }  

          set { SetProperty(ref  LONGITUDEValue, value); }
        } 
private  System.Decimal? ELEVATIONValue; 
 public System.Decimal? ELEVATION
        {  
            get  
            {  
                return this.ELEVATIONValue;  
            }  

          set { SetProperty(ref  ELEVATIONValue, value); }
        } 
private  System.String ELEVATION_OUOMValue; 
 public System.String ELEVATION_OUOM
        {  
            get  
            {  
                return this.ELEVATION_OUOMValue;  
            }  

          set { SetProperty(ref  ELEVATION_OUOMValue, value); }
        } 
private  System.String LOCATION_DESCRIPTIONValue; 
 public System.String LOCATION_DESCRIPTION
        {  
            get  
            {  
                return this.LOCATION_DESCRIPTIONValue;  
            }  

          set { SetProperty(ref  LOCATION_DESCRIPTIONValue, value); }
        } 
private  System.Decimal? TOP_DEPTHValue; 
 public System.Decimal? TOP_DEPTH
        {  
            get  
            {  
                return this.TOP_DEPTHValue;  
            }  

          set { SetProperty(ref  TOP_DEPTHValue, value); }
        } 
private  System.Decimal? BASE_DEPTHValue; 
 public System.Decimal? BASE_DEPTH
        {  
            get  
            {  
                return this.BASE_DEPTHValue;  
            }  

          set { SetProperty(ref  BASE_DEPTHValue, value); }
        } 
private  System.String DEPTH_OUOMValue; 
 public System.String DEPTH_OUOM
        {  
            get  
            {  
                return this.DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  DEPTH_OUOMValue, value); }
        } 
private  System.Decimal? ESTIMATED_OIL_VOLUMEValue; 
 public System.Decimal? ESTIMATED_OIL_VOLUME
        {  
            get  
            {  
                return this.ESTIMATED_OIL_VOLUMEValue;  
            }  

          set { SetProperty(ref  ESTIMATED_OIL_VOLUMEValue, value); }
        } 
private  System.Decimal? ESTIMATED_GAS_VOLUMEValue; 
 public System.Decimal? ESTIMATED_GAS_VOLUME
        {  
            get  
            {  
                return this.ESTIMATED_GAS_VOLUMEValue;  
            }  

          set { SetProperty(ref  ESTIMATED_GAS_VOLUMEValue, value); }
        } 
private  System.String ESTIMATED_VOLUME_OUOMValue; 
 public System.String ESTIMATED_VOLUME_OUOM
        {  
            get  
            {  
                return this.ESTIMATED_VOLUME_OUOMValue;  
            }  

          set { SetProperty(ref  ESTIMATED_VOLUME_OUOMValue, value); }
        } 
private  System.Decimal? ESTIMATED_VALUEValue; 
 public System.Decimal? ESTIMATED_VALUE
        {  
            get  
            {  
                return this.ESTIMATED_VALUEValue;  
            }  

          set { SetProperty(ref  ESTIMATED_VALUEValue, value); }
        } 
private  System.String ESTIMATED_VALUE_CURRENCYValue; 
 public System.String ESTIMATED_VALUE_CURRENCY
        {  
            get  
            {  
                return this.ESTIMATED_VALUE_CURRENCYValue;  
            }  

          set { SetProperty(ref  ESTIMATED_VALUE_CURRENCYValue, value); }
        } 
private  System.String FORMATION_NAMEValue; 
 public System.String FORMATION_NAME
        {  
            get  
            {  
                return this.FORMATION_NAMEValue;  
            }  

          set { SetProperty(ref  FORMATION_NAMEValue, value); }
        } 
private  System.String STRAT_UNIT_IDValue; 
 public System.String STRAT_UNIT_ID
        {  
            get  
            {  
                return this.STRAT_UNIT_IDValue;  
            }  

          set { SetProperty(ref  STRAT_UNIT_IDValue, value); }
        } 
private  System.Decimal? POROSITYValue; 
 public System.Decimal? POROSITY
        {  
            get  
            {  
                return this.POROSITYValue;  
            }  

          set { SetProperty(ref  POROSITYValue, value); }
        } 
private  System.Decimal? PERMEABILITYValue; 
 public System.Decimal? PERMEABILITY
        {  
            get  
            {  
                return this.PERMEABILITYValue;  
            }  

          set { SetProperty(ref  PERMEABILITYValue, value); }
        } 
private  System.Decimal? NET_PAYValue; 
 public System.Decimal? NET_PAY
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
private  System.String COORD_SYSTEM_IDValue; 
 public System.String COORD_SYSTEM_ID
        {  
            get  
            {  
                return this.COORD_SYSTEM_IDValue;  
            }  

          set { SetProperty(ref  COORD_SYSTEM_IDValue, value); }
        } 
private  System.String LOCAL_COORD_SYSTEM_IDValue; 
 public System.String LOCAL_COORD_SYSTEM_ID
        {  
            get  
            {  
                return this.LOCAL_COORD_SYSTEM_IDValue;  
            }  

          set { SetProperty(ref  LOCAL_COORD_SYSTEM_IDValue, value); }
        } 
private  System.String COORD_ACQUISITION_IDValue; 
 public System.String COORD_ACQUISITION_ID
        {  
            get  
            {  
                return this.COORD_ACQUISITION_IDValue;  
            }  

          set { SetProperty(ref  COORD_ACQUISITION_IDValue, value); }
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
private  System.DateTime? EFFECTIVE_DATEValue; 
 public System.DateTime? EFFECTIVE_DATE
        {  
            get  
            {  
                return this.EFFECTIVE_DATEValue;  
            }  

          set { SetProperty(ref  EFFECTIVE_DATEValue, value); }
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
private  System.String PPDM_GUIDValue; 
 public System.String PPDM_GUID
        {  
            get  
            {  
                return this.PPDM_GUIDValue;  
            }  

          set { SetProperty(ref  PPDM_GUIDValue, value); }
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
private  System.String SOURCEValue; 
 public System.String SOURCE
        {  
            get  
            {  
                return this.SOURCEValue;  
            }  

          set { SetProperty(ref  SOURCEValue, value); }
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


    public PROSPECT () { }

  }
}

using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class PALEO_INTERP: Entity,IPPDMEntity

{

private  System.String PALEO_SUMMARY_IDValue; 
 public System.String PALEO_SUMMARY_ID
        {  
            get  
            {  
                return this.PALEO_SUMMARY_IDValue;  
            }  

          set { SetProperty(ref  PALEO_SUMMARY_IDValue, value); }
        } 
private  System.String DETAIL_IDValue; 
 public System.String DETAIL_ID
        {  
            get  
            {  
                return this.DETAIL_IDValue;  
            }  

          set { SetProperty(ref  DETAIL_IDValue, value); }
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
private  System.String ANALYSIS_IDValue; 
 public System.String ANALYSIS_ID
        {  
            get  
            {  
                return this.ANALYSIS_IDValue;  
            }  

          set { SetProperty(ref  ANALYSIS_IDValue, value); }
        } 
private  System.String ANL_SOURCEValue; 
 public System.String ANL_SOURCE
        {  
            get  
            {  
                return this.ANL_SOURCEValue;  
            }  

          set { SetProperty(ref  ANL_SOURCEValue, value); }
        } 
private  System.Decimal BASE_MDValue; 
 public System.Decimal BASE_MD
        {  
            get  
            {  
                return this.BASE_MDValue;  
            }  

          set { SetProperty(ref  BASE_MDValue, value); }
        } 
private  System.String BASE_MD_OUOMValue; 
 public System.String BASE_MD_OUOM
        {  
            get  
            {  
                return this.BASE_MD_OUOMValue;  
            }  

          set { SetProperty(ref  BASE_MD_OUOMValue, value); }
        } 
private  System.String CLIMATE_IDValue; 
 public System.String CLIMATE_ID
        {  
            get  
            {  
                return this.CLIMATE_IDValue;  
            }  

          set { SetProperty(ref  CLIMATE_IDValue, value); }
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
private  System.String ECOZONE_CONFIDENCE_IDValue; 
 public System.String ECOZONE_CONFIDENCE_ID
        {  
            get  
            {  
                return this.ECOZONE_CONFIDENCE_IDValue;  
            }  

          set { SetProperty(ref  ECOZONE_CONFIDENCE_IDValue, value); }
        } 
private  System.String ECOZONE_IDValue; 
 public System.String ECOZONE_ID
        {  
            get  
            {  
                return this.ECOZONE_IDValue;  
            }  

          set { SetProperty(ref  ECOZONE_IDValue, value); }
        } 
private  System.String ECOZONE_QUALIFIER_IDValue; 
 public System.String ECOZONE_QUALIFIER_ID
        {  
            get  
            {  
                return this.ECOZONE_QUALIFIER_IDValue;  
            }  

          set { SetProperty(ref  ECOZONE_QUALIFIER_IDValue, value); }
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
private  System.String FROM_REL_STRAT_NAME_SET_IDValue; 
 public System.String FROM_REL_STRAT_NAME_SET_ID
        {  
            get  
            {  
                return this.FROM_REL_STRAT_NAME_SET_IDValue;  
            }  

          set { SetProperty(ref  FROM_REL_STRAT_NAME_SET_IDValue, value); }
        } 
private  System.String FROM_REL_STRAT_UNIT_IDValue; 
 public System.String FROM_REL_STRAT_UNIT_ID
        {  
            get  
            {  
                return this.FROM_REL_STRAT_UNIT_IDValue;  
            }  

          set { SetProperty(ref  FROM_REL_STRAT_UNIT_IDValue, value); }
        } 
private  System.String FROM_STRAT_NAME_SET_IDValue; 
 public System.String FROM_STRAT_NAME_SET_ID
        {  
            get  
            {  
                return this.FROM_STRAT_NAME_SET_IDValue;  
            }  

          set { SetProperty(ref  FROM_STRAT_NAME_SET_IDValue, value); }
        } 
private  System.String FROM_STRAT_UNIT_IDValue; 
 public System.String FROM_STRAT_UNIT_ID
        {  
            get  
            {  
                return this.FROM_STRAT_UNIT_IDValue;  
            }  

          set { SetProperty(ref  FROM_STRAT_UNIT_IDValue, value); }
        } 
private  System.String INTERP_TYPEValue; 
 public System.String INTERP_TYPE
        {  
            get  
            {  
                return this.INTERP_TYPEValue;  
            }  

          set { SetProperty(ref  INTERP_TYPEValue, value); }
        } 
private  System.String LITHOLOGY_TYPEValue; 
 public System.String LITHOLOGY_TYPE
        {  
            get  
            {  
                return this.LITHOLOGY_TYPEValue;  
            }  

          set { SetProperty(ref  LITHOLOGY_TYPEValue, value); }
        } 
private  System.String LITH_STRUCTURE_IDValue; 
 public System.String LITH_STRUCTURE_ID
        {  
            get  
            {  
                return this.LITH_STRUCTURE_IDValue;  
            }  

          set { SetProperty(ref  LITH_STRUCTURE_IDValue, value); }
        } 
private  System.Decimal MATURATION_OBS_NOValue; 
 public System.Decimal MATURATION_OBS_NO
        {  
            get  
            {  
                return this.MATURATION_OBS_NOValue;  
            }  

          set { SetProperty(ref  MATURATION_OBS_NOValue, value); }
        } 
private  System.String PALEO_CONFIDENCE_IDValue; 
 public System.String PALEO_CONFIDENCE_ID
        {  
            get  
            {  
                return this.PALEO_CONFIDENCE_IDValue;  
            }  

          set { SetProperty(ref  PALEO_CONFIDENCE_IDValue, value); }
        } 
private  System.String PALEO_QUALIFIER_IDValue; 
 public System.String PALEO_QUALIFIER_ID
        {  
            get  
            {  
                return this.PALEO_QUALIFIER_IDValue;  
            }  

          set { SetProperty(ref  PALEO_QUALIFIER_IDValue, value); }
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
private  System.String PREFERRED_INDValue; 
 public System.String PREFERRED_IND
        {  
            get  
            {  
                return this.PREFERRED_INDValue;  
            }  

          set { SetProperty(ref  PREFERRED_INDValue, value); }
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
private  System.String TAI_COLORValue; 
 public System.String TAI_COLOR
        {  
            get  
            {  
                return this.TAI_COLORValue;  
            }  

          set { SetProperty(ref  TAI_COLORValue, value); }
        } 
private  System.Decimal TOP_MDValue; 
 public System.Decimal TOP_MD
        {  
            get  
            {  
                return this.TOP_MDValue;  
            }  

          set { SetProperty(ref  TOP_MDValue, value); }
        } 
private  System.String TOP_MD_OUOMValue; 
 public System.String TOP_MD_OUOM
        {  
            get  
            {  
                return this.TOP_MD_OUOMValue;  
            }  

          set { SetProperty(ref  TOP_MD_OUOMValue, value); }
        } 
private  System.String TO_REL_STRAT_NAME_SET_IDValue; 
 public System.String TO_REL_STRAT_NAME_SET_ID
        {  
            get  
            {  
                return this.TO_REL_STRAT_NAME_SET_IDValue;  
            }  

          set { SetProperty(ref  TO_REL_STRAT_NAME_SET_IDValue, value); }
        } 
private  System.String TO_REL_STRAT_UNIT_IDValue; 
 public System.String TO_REL_STRAT_UNIT_ID
        {  
            get  
            {  
                return this.TO_REL_STRAT_UNIT_IDValue;  
            }  

          set { SetProperty(ref  TO_REL_STRAT_UNIT_IDValue, value); }
        } 
private  System.String TO_STRAT_NAME_SET_IDValue; 
 public System.String TO_STRAT_NAME_SET_ID
        {  
            get  
            {  
                return this.TO_STRAT_NAME_SET_IDValue;  
            }  

          set { SetProperty(ref  TO_STRAT_NAME_SET_IDValue, value); }
        } 
private  System.String TO_STRAT_UNIT_IDValue; 
 public System.String TO_STRAT_UNIT_ID
        {  
            get  
            {  
                return this.TO_STRAT_UNIT_IDValue;  
            }  

          set { SetProperty(ref  TO_STRAT_UNIT_IDValue, value); }
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


    public PALEO_INTERP () { }

  }
}


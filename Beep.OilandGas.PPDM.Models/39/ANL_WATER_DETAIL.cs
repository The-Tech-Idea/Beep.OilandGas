using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class ANL_WATER_DETAIL: Entity,IPPDMEntity

{

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
private  System.Decimal DETAIL_OBS_NOValue; 
 public System.Decimal DETAIL_OBS_NO
        {  
            get  
            {  
                return this.DETAIL_OBS_NOValue;  
            }  

          set { SetProperty(ref  DETAIL_OBS_NOValue, value); }
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
private  System.Decimal ANALYSIS_PRESSValue; 
 public System.Decimal ANALYSIS_PRESS
        {  
            get  
            {  
                return this.ANALYSIS_PRESSValue;  
            }  

          set { SetProperty(ref  ANALYSIS_PRESSValue, value); }
        } 
private  System.String ANALYSIS_PRESS_OUOMValue; 
 public System.String ANALYSIS_PRESS_OUOM
        {  
            get  
            {  
                return this.ANALYSIS_PRESS_OUOMValue;  
            }  

          set { SetProperty(ref  ANALYSIS_PRESS_OUOMValue, value); }
        } 
private  System.String ANALYSIS_PRESS_UOMValue; 
 public System.String ANALYSIS_PRESS_UOM
        {  
            get  
            {  
                return this.ANALYSIS_PRESS_UOMValue;  
            }  

          set { SetProperty(ref  ANALYSIS_PRESS_UOMValue, value); }
        } 
private  System.Decimal ANALYSIS_TEMPValue; 
 public System.Decimal ANALYSIS_TEMP
        {  
            get  
            {  
                return this.ANALYSIS_TEMPValue;  
            }  

          set { SetProperty(ref  ANALYSIS_TEMPValue, value); }
        } 
private  System.String ANALYSIS_TEMP_OUOMValue; 
 public System.String ANALYSIS_TEMP_OUOM
        {  
            get  
            {  
                return this.ANALYSIS_TEMP_OUOMValue;  
            }  

          set { SetProperty(ref  ANALYSIS_TEMP_OUOMValue, value); }
        } 
private  System.String ANALYSIS_TEMP_UOMValue; 
 public System.String ANALYSIS_TEMP_UOM
        {  
            get  
            {  
                return this.ANALYSIS_TEMP_UOMValue;  
            }  

          set { SetProperty(ref  ANALYSIS_TEMP_UOMValue, value); }
        } 
private  System.Decimal ANALYSIS_VALUEValue; 
 public System.Decimal ANALYSIS_VALUE
        {  
            get  
            {  
                return this.ANALYSIS_VALUEValue;  
            }  

          set { SetProperty(ref  ANALYSIS_VALUEValue, value); }
        } 
private  System.String ANALYSIS_VALUE_OUOMValue; 
 public System.String ANALYSIS_VALUE_OUOM
        {  
            get  
            {  
                return this.ANALYSIS_VALUE_OUOMValue;  
            }  

          set { SetProperty(ref  ANALYSIS_VALUE_OUOMValue, value); }
        } 
private  System.String ANALYSIS_VALUE_TYPEValue; 
 public System.String ANALYSIS_VALUE_TYPE
        {  
            get  
            {  
                return this.ANALYSIS_VALUE_TYPEValue;  
            }  

          set { SetProperty(ref  ANALYSIS_VALUE_TYPEValue, value); }
        } 
private  System.String ANALYSIS_VALUE_UOMValue; 
 public System.String ANALYSIS_VALUE_UOM
        {  
            get  
            {  
                return this.ANALYSIS_VALUE_UOMValue;  
            }  

          set { SetProperty(ref  ANALYSIS_VALUE_UOMValue, value); }
        } 
private  System.String ANL_VALUE_REMARKValue; 
 public System.String ANL_VALUE_REMARK
        {  
            get  
            {  
                return this.ANL_VALUE_REMARKValue;  
            }  

          set { SetProperty(ref  ANL_VALUE_REMARKValue, value); }
        } 
private  System.String CALCULATED_VALUE_INDValue; 
 public System.String CALCULATED_VALUE_IND
        {  
            get  
            {  
                return this.CALCULATED_VALUE_INDValue;  
            }  

          set { SetProperty(ref  CALCULATED_VALUE_INDValue, value); }
        } 
private  System.String CALCULATE_METHOD_IDValue; 
 public System.String CALCULATE_METHOD_ID
        {  
            get  
            {  
                return this.CALCULATE_METHOD_IDValue;  
            }  

          set { SetProperty(ref  CALCULATE_METHOD_IDValue, value); }
        } 
private  System.String DATE_FORMAT_DESCValue; 
 public System.String DATE_FORMAT_DESC
        {  
            get  
            {  
                return this.DATE_FORMAT_DESCValue;  
            }  

          set { SetProperty(ref  DATE_FORMAT_DESCValue, value); }
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
private  System.Decimal MAX_VALUEValue; 
 public System.Decimal MAX_VALUE
        {  
            get  
            {  
                return this.MAX_VALUEValue;  
            }  

          set { SetProperty(ref  MAX_VALUEValue, value); }
        } 
private  System.String MAX_VALUE_OUOMValue; 
 public System.String MAX_VALUE_OUOM
        {  
            get  
            {  
                return this.MAX_VALUE_OUOMValue;  
            }  

          set { SetProperty(ref  MAX_VALUE_OUOMValue, value); }
        } 
private  System.String MAX_VALUE_UOMValue; 
 public System.String MAX_VALUE_UOM
        {  
            get  
            {  
                return this.MAX_VALUE_UOMValue;  
            }  

          set { SetProperty(ref  MAX_VALUE_UOMValue, value); }
        } 
private  System.String MEASURED_VALUE_INDValue; 
 public System.String MEASURED_VALUE_IND
        {  
            get  
            {  
                return this.MEASURED_VALUE_INDValue;  
            }  

          set { SetProperty(ref  MEASURED_VALUE_INDValue, value); }
        } 
private  System.String MEASUREMENT_TYPEValue; 
 public System.String MEASUREMENT_TYPE
        {  
            get  
            {  
                return this.MEASUREMENT_TYPEValue;  
            }  

          set { SetProperty(ref  MEASUREMENT_TYPEValue, value); }
        } 
private  System.Decimal MIN_VALUEValue; 
 public System.Decimal MIN_VALUE
        {  
            get  
            {  
                return this.MIN_VALUEValue;  
            }  

          set { SetProperty(ref  MIN_VALUEValue, value); }
        } 
private  System.String MIN_VALUE_OUOMValue; 
 public System.String MIN_VALUE_OUOM
        {  
            get  
            {  
                return this.MIN_VALUE_OUOMValue;  
            }  

          set { SetProperty(ref  MIN_VALUE_OUOMValue, value); }
        } 
private  System.String MIN_VALUE_UOMValue; 
 public System.String MIN_VALUE_UOM
        {  
            get  
            {  
                return this.MIN_VALUE_UOMValue;  
            }  

          set { SetProperty(ref  MIN_VALUE_UOMValue, value); }
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
private  System.String PROBLEM_INDValue; 
 public System.String PROBLEM_IND
        {  
            get  
            {  
                return this.PROBLEM_INDValue;  
            }  

          set { SetProperty(ref  PROBLEM_INDValue, value); }
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
private  System.String REPORTED_VALUE_INDValue; 
 public System.String REPORTED_VALUE_IND
        {  
            get  
            {  
                return this.REPORTED_VALUE_INDValue;  
            }  

          set { SetProperty(ref  REPORTED_VALUE_INDValue, value); }
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
private  System.Decimal STEP_SEQ_NOValue; 
 public System.Decimal STEP_SEQ_NO
        {  
            get  
            {  
                return this.STEP_SEQ_NOValue;  
            }  

          set { SetProperty(ref  STEP_SEQ_NOValue, value); }
        } 
private  System.String SUBSTANCE_IDValue; 
 public System.String SUBSTANCE_ID
        {  
            get  
            {  
                return this.SUBSTANCE_IDValue;  
            }  

          set { SetProperty(ref  SUBSTANCE_IDValue, value); }
        } 
private  System.String WATER_PROPERTYValue; 
 public System.String WATER_PROPERTY
        {  
            get  
            {  
                return this.WATER_PROPERTYValue;  
            }  

          set { SetProperty(ref  WATER_PROPERTYValue, value); }
        } 
private  System.String WATER_PROPERTY_CODEValue; 
 public System.String WATER_PROPERTY_CODE
        {  
            get  
            {  
                return this.WATER_PROPERTY_CODEValue;  
            }  

          set { SetProperty(ref  WATER_PROPERTY_CODEValue, value); }
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


    public ANL_WATER_DETAIL () { }

  }
}


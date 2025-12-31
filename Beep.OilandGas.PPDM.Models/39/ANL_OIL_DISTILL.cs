using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class ANL_OIL_DISTILL: Entity,IPPDMEntity

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
private  System.Decimal DSTL_SUMMARY_OBS_NOValue; 
 public System.Decimal DSTL_SUMMARY_OBS_NO
        {  
            get  
            {  
                return this.DSTL_SUMMARY_OBS_NOValue;  
            }  

          set { SetProperty(ref  DSTL_SUMMARY_OBS_NOValue, value); }
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
private  System.Decimal ATMOSP_DSTL_PRESSValue; 
 public System.Decimal ATMOSP_DSTL_PRESS
        {  
            get  
            {  
                return this.ATMOSP_DSTL_PRESSValue;  
            }  

          set { SetProperty(ref  ATMOSP_DSTL_PRESSValue, value); }
        } 
private  System.String ATMOSP_DSTL_PRESS_OUOMValue; 
 public System.String ATMOSP_DSTL_PRESS_OUOM
        {  
            get  
            {  
                return this.ATMOSP_DSTL_PRESS_OUOMValue;  
            }  

          set { SetProperty(ref  ATMOSP_DSTL_PRESS_OUOMValue, value); }
        } 
private  System.Decimal ATMOSP_DSTL_TEMPValue; 
 public System.Decimal ATMOSP_DSTL_TEMP
        {  
            get  
            {  
                return this.ATMOSP_DSTL_TEMPValue;  
            }  

          set { SetProperty(ref  ATMOSP_DSTL_TEMPValue, value); }
        } 
private  System.String ATMOSP_DSTL_TEMP_OUOMValue; 
 public System.String ATMOSP_DSTL_TEMP_OUOM
        {  
            get  
            {  
                return this.ATMOSP_DSTL_TEMP_OUOMValue;  
            }  

          set { SetProperty(ref  ATMOSP_DSTL_TEMP_OUOMValue, value); }
        } 
private  System.String CALCULATED_INDValue; 
 public System.String CALCULATED_IND
        {  
            get  
            {  
                return this.CALCULATED_INDValue;  
            }  

          set { SetProperty(ref  CALCULATED_INDValue, value); }
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
private  System.Decimal FINAL_BOIL_PT_TEMPValue; 
 public System.Decimal FINAL_BOIL_PT_TEMP
        {  
            get  
            {  
                return this.FINAL_BOIL_PT_TEMPValue;  
            }  

          set { SetProperty(ref  FINAL_BOIL_PT_TEMPValue, value); }
        } 
private  System.String FINAL_BOIL_PT_TEMP_OUOMValue; 
 public System.String FINAL_BOIL_PT_TEMP_OUOM
        {  
            get  
            {  
                return this.FINAL_BOIL_PT_TEMP_OUOMValue;  
            }  

          set { SetProperty(ref  FINAL_BOIL_PT_TEMP_OUOMValue, value); }
        } 
private  System.Decimal MEASUREMENT_TEMPValue; 
 public System.Decimal MEASUREMENT_TEMP
        {  
            get  
            {  
                return this.MEASUREMENT_TEMPValue;  
            }  

          set { SetProperty(ref  MEASUREMENT_TEMPValue, value); }
        } 
private  System.String MEASUREMENT_TEMP_OUOMValue; 
 public System.String MEASUREMENT_TEMP_OUOM
        {  
            get  
            {  
                return this.MEASUREMENT_TEMP_OUOMValue;  
            }  

          set { SetProperty(ref  MEASUREMENT_TEMP_OUOMValue, value); }
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
private  System.String REPORTED_INDValue; 
 public System.String REPORTED_IND
        {  
            get  
            {  
                return this.REPORTED_INDValue;  
            }  

          set { SetProperty(ref  REPORTED_INDValue, value); }
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
private  System.Decimal START_BOIL_PT_TEMPValue; 
 public System.Decimal START_BOIL_PT_TEMP
        {  
            get  
            {  
                return this.START_BOIL_PT_TEMPValue;  
            }  

          set { SetProperty(ref  START_BOIL_PT_TEMPValue, value); }
        } 
private  System.String START_BOIL_PT_TEMP_OUOMValue; 
 public System.String START_BOIL_PT_TEMP_OUOM
        {  
            get  
            {  
                return this.START_BOIL_PT_TEMP_OUOMValue;  
            }  

          set { SetProperty(ref  START_BOIL_PT_TEMP_OUOMValue, value); }
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
private  System.String VOLUME_FRACTION_TYPEValue; 
 public System.String VOLUME_FRACTION_TYPE
        {  
            get  
            {  
                return this.VOLUME_FRACTION_TYPEValue;  
            }  

          set { SetProperty(ref  VOLUME_FRACTION_TYPEValue, value); }
        } 
private  System.Decimal VOL_FRACTION_TEMPValue; 
 public System.Decimal VOL_FRACTION_TEMP
        {  
            get  
            {  
                return this.VOL_FRACTION_TEMPValue;  
            }  

          set { SetProperty(ref  VOL_FRACTION_TEMPValue, value); }
        } 
private  System.String VOL_FRACTION_TEMP_OUOMValue; 
 public System.String VOL_FRACTION_TEMP_OUOM
        {  
            get  
            {  
                return this.VOL_FRACTION_TEMP_OUOMValue;  
            }  

          set { SetProperty(ref  VOL_FRACTION_TEMP_OUOMValue, value); }
        } 
private  System.Decimal VOL_FRACTION_VALUEValue; 
 public System.Decimal VOL_FRACTION_VALUE
        {  
            get  
            {  
                return this.VOL_FRACTION_VALUEValue;  
            }  

          set { SetProperty(ref  VOL_FRACTION_VALUEValue, value); }
        } 
private  System.String VOL_FRACTION_VALUE_OUOMValue; 
 public System.String VOL_FRACTION_VALUE_OUOM
        {  
            get  
            {  
                return this.VOL_FRACTION_VALUE_OUOMValue;  
            }  

          set { SetProperty(ref  VOL_FRACTION_VALUE_OUOMValue, value); }
        } 
private  System.Decimal WEIGHT_CUTValue; 
 public System.Decimal WEIGHT_CUT
        {  
            get  
            {  
                return this.WEIGHT_CUTValue;  
            }  

          set { SetProperty(ref  WEIGHT_CUTValue, value); }
        } 
private  System.String WEIGHT_CUT_OUOMValue; 
 public System.String WEIGHT_CUT_OUOM
        {  
            get  
            {  
                return this.WEIGHT_CUT_OUOMValue;  
            }  

          set { SetProperty(ref  WEIGHT_CUT_OUOMValue, value); }
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


    public ANL_OIL_DISTILL () { }

  }
}


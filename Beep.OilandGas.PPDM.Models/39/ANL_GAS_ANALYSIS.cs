using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class ANL_GAS_ANALYSIS: Entity,IPPDMEntity

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
private  System.Decimal GAS_ANL_OBS_NOValue; 
 public System.Decimal GAS_ANL_OBS_NO
        {  
            get  
            {  
                return this.GAS_ANL_OBS_NOValue;  
            }  

          set { SetProperty(ref  GAS_ANL_OBS_NOValue, value); }
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
private  System.String FLUID_TYPEValue; 
 public System.String FLUID_TYPE
        {  
            get  
            {  
                return this.FLUID_TYPEValue;  
            }  

          set { SetProperty(ref  FLUID_TYPEValue, value); }
        } 
private  System.Decimal GAS_GRAVITYValue; 
 public System.Decimal GAS_GRAVITY
        {  
            get  
            {  
                return this.GAS_GRAVITYValue;  
            }  

          set { SetProperty(ref  GAS_GRAVITYValue, value); }
        } 
private  System.String GAS_GRAVITY_OUOMValue; 
 public System.String GAS_GRAVITY_OUOM
        {  
            get  
            {  
                return this.GAS_GRAVITY_OUOMValue;  
            }  

          set { SetProperty(ref  GAS_GRAVITY_OUOMValue, value); }
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
private  System.Decimal PSEUDO_CRITICAL_PRESSValue; 
 public System.Decimal PSEUDO_CRITICAL_PRESS
        {  
            get  
            {  
                return this.PSEUDO_CRITICAL_PRESSValue;  
            }  

          set { SetProperty(ref  PSEUDO_CRITICAL_PRESSValue, value); }
        } 
private  System.String PSEUDO_CRITICAL_PRESS_OUOMValue; 
 public System.String PSEUDO_CRITICAL_PRESS_OUOM
        {  
            get  
            {  
                return this.PSEUDO_CRITICAL_PRESS_OUOMValue;  
            }  

          set { SetProperty(ref  PSEUDO_CRITICAL_PRESS_OUOMValue, value); }
        } 
private  System.Decimal PSEUDO_CRITICAL_TEMPValue; 
 public System.Decimal PSEUDO_CRITICAL_TEMP
        {  
            get  
            {  
                return this.PSEUDO_CRITICAL_TEMPValue;  
            }  

          set { SetProperty(ref  PSEUDO_CRITICAL_TEMPValue, value); }
        } 
private  System.String PSEUDO_CRITICAL_TEMP_OUOMValue; 
 public System.String PSEUDO_CRITICAL_TEMP_OUOM
        {  
            get  
            {  
                return this.PSEUDO_CRITICAL_TEMP_OUOMValue;  
            }  

          set { SetProperty(ref  PSEUDO_CRITICAL_TEMP_OUOMValue, value); }
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
private  System.Decimal STEP_SEQ_NOValue; 
 public System.Decimal STEP_SEQ_NO
        {  
            get  
            {  
                return this.STEP_SEQ_NOValue;  
            }  

          set { SetProperty(ref  STEP_SEQ_NOValue, value); }
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


    public ANL_GAS_ANALYSIS () { }

  }
}


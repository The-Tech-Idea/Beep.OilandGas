using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class ANL_GAS_COMPOSITION: Entity

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
private  System.Decimal GAS_ANL_COMPOSITION_OBS_NOValue; 
 public System.Decimal GAS_ANL_COMPOSITION_OBS_NO
        {  
            get  
            {  
                return this.GAS_ANL_COMPOSITION_OBS_NOValue;  
            }  

          set { SetProperty(ref  GAS_ANL_COMPOSITION_OBS_NOValue, value); }
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
private  System.DateTime EFFECTIVE_DATEValue; 
 public System.DateTime EFFECTIVE_DATE
        {  
            get  
            {  
                return this.EFFECTIVE_DATEValue;  
            }  

          set { SetProperty(ref  EFFECTIVE_DATEValue, value); }
        } 
private  System.DateTime EXPIRY_DATEValue; 
 public System.DateTime EXPIRY_DATE
        {  
            get  
            {  
                return this.EXPIRY_DATEValue;  
            }  

          set { SetProperty(ref  EXPIRY_DATEValue, value); }
        } 
private  System.String NOT_PRESENT_INDValue; 
 public System.String NOT_PRESENT_IND
        {  
            get  
            {  
                return this.NOT_PRESENT_INDValue;  
            }  

          set { SetProperty(ref  NOT_PRESENT_INDValue, value); }
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
private  System.Decimal STEP_SEQ_NOValue; 
 public System.Decimal STEP_SEQ_NO
        {  
            get  
            {  
                return this.STEP_SEQ_NOValue;  
            }  

          set { SetProperty(ref  STEP_SEQ_NOValue, value); }
        } 
private  System.Decimal SUBSTANCE_AMOUNTValue; 
 public System.Decimal SUBSTANCE_AMOUNT
        {  
            get  
            {  
                return this.SUBSTANCE_AMOUNTValue;  
            }  

          set { SetProperty(ref  SUBSTANCE_AMOUNTValue, value); }
        } 
private  System.String SUBSTANCE_AMOUNT_OUOMValue; 
 public System.String SUBSTANCE_AMOUNT_OUOM
        {  
            get  
            {  
                return this.SUBSTANCE_AMOUNT_OUOMValue;  
            }  

          set { SetProperty(ref  SUBSTANCE_AMOUNT_OUOMValue, value); }
        } 
private  System.String SUBSTANCE_AMOUNT_UOMValue; 
 public System.String SUBSTANCE_AMOUNT_UOM
        {  
            get  
            {  
                return this.SUBSTANCE_AMOUNT_UOMValue;  
            }  

          set { SetProperty(ref  SUBSTANCE_AMOUNT_UOMValue, value); }
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
private  System.Decimal SUBSTANCE_PERCENTValue; 
 public System.Decimal SUBSTANCE_PERCENT
        {  
            get  
            {  
                return this.SUBSTANCE_PERCENTValue;  
            }  

          set { SetProperty(ref  SUBSTANCE_PERCENTValue, value); }
        } 
private  System.String TRACE_INDValue; 
 public System.String TRACE_IND
        {  
            get  
            {  
                return this.TRACE_INDValue;  
            }  

          set { SetProperty(ref  TRACE_INDValue, value); }
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
private  System.DateTime ROW_CHANGED_DATEValue; 
 public System.DateTime ROW_CHANGED_DATE
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
private  System.DateTime ROW_CREATED_DATEValue; 
 public System.DateTime ROW_CREATED_DATE
        {  
            get  
            {  
                return this.ROW_CREATED_DATEValue;  
            }  

          set { SetProperty(ref  ROW_CREATED_DATEValue, value); }
        } 
private  System.DateTime ROW_EFFECTIVE_DATEValue; 
 public System.DateTime ROW_EFFECTIVE_DATE
        {  
            get  
            {  
                return this.ROW_EFFECTIVE_DATEValue;  
            }  

          set { SetProperty(ref  ROW_EFFECTIVE_DATEValue, value); }
        } 
private  System.DateTime ROW_EXPIRY_DATEValue; 
 public System.DateTime ROW_EXPIRY_DATE
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


    public ANL_GAS_COMPOSITION () { }

  }
}


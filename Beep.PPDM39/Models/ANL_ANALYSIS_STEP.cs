using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.PPDM39.Models 
{
public partial class ANL_ANALYSIS_STEP: Entity

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
private  System.Decimal STEP_SEQ_NOValue; 
 public System.Decimal STEP_SEQ_NO
        {  
            get  
            {  
                return this.STEP_SEQ_NOValue;  
            }  

          set { SetProperty(ref  STEP_SEQ_NOValue, value); }
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
private  System.String ANALYSIS_PHASEValue; 
 public System.String ANALYSIS_PHASE
        {  
            get  
            {  
                return this.ANALYSIS_PHASEValue;  
            }  

          set { SetProperty(ref  ANALYSIS_PHASEValue, value); }
        } 
private  System.DateTime ANL_DATEValue; 
 public System.DateTime ANL_DATE
        {  
            get  
            {  
                return this.ANL_DATEValue;  
            }  

          set { SetProperty(ref  ANL_DATEValue, value); }
        } 
private  System.DateTime COMPLETE_DATEValue; 
 public System.DateTime COMPLETE_DATE
        {  
            get  
            {  
                return this.COMPLETE_DATEValue;  
            }  

          set { SetProperty(ref  COMPLETE_DATEValue, value); }
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
private  System.DateTime END_DATEValue; 
 public System.DateTime END_DATE
        {  
            get  
            {  
                return this.END_DATEValue;  
            }  

          set { SetProperty(ref  END_DATEValue, value); }
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
private  System.Decimal FINAL_VOLUMEValue; 
 public System.Decimal FINAL_VOLUME
        {  
            get  
            {  
                return this.FINAL_VOLUMEValue;  
            }  

          set { SetProperty(ref  FINAL_VOLUMEValue, value); }
        } 
private  System.String FINAL_VOLUME_OUOMValue; 
 public System.String FINAL_VOLUME_OUOM
        {  
            get  
            {  
                return this.FINAL_VOLUME_OUOMValue;  
            }  

          set { SetProperty(ref  FINAL_VOLUME_OUOMValue, value); }
        } 
private  System.Decimal FINAL_VOLUME_PERCENTValue; 
 public System.Decimal FINAL_VOLUME_PERCENT
        {  
            get  
            {  
                return this.FINAL_VOLUME_PERCENTValue;  
            }  

          set { SetProperty(ref  FINAL_VOLUME_PERCENTValue, value); }
        } 
private  System.Decimal FINAL_WEIGHTValue; 
 public System.Decimal FINAL_WEIGHT
        {  
            get  
            {  
                return this.FINAL_WEIGHTValue;  
            }  

          set { SetProperty(ref  FINAL_WEIGHTValue, value); }
        } 
private  System.String FINAL_WEIGHT_OUOMValue; 
 public System.String FINAL_WEIGHT_OUOM
        {  
            get  
            {  
                return this.FINAL_WEIGHT_OUOMValue;  
            }  

          set { SetProperty(ref  FINAL_WEIGHT_OUOMValue, value); }
        } 
private  System.String METHOD_IDValue; 
 public System.String METHOD_ID
        {  
            get  
            {  
                return this.METHOD_IDValue;  
            }  

          set { SetProperty(ref  METHOD_IDValue, value); }
        } 
private  System.String METHOD_SET_IDValue; 
 public System.String METHOD_SET_ID
        {  
            get  
            {  
                return this.METHOD_SET_IDValue;  
            }  

          set { SetProperty(ref  METHOD_SET_IDValue, value); }
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
private  System.DateTime RECEIVED_DATEValue; 
 public System.DateTime RECEIVED_DATE
        {  
            get  
            {  
                return this.RECEIVED_DATEValue;  
            }  

          set { SetProperty(ref  RECEIVED_DATEValue, value); }
        } 
private  System.Decimal RECOVERED_PERCENTValue; 
 public System.Decimal RECOVERED_PERCENT
        {  
            get  
            {  
                return this.RECOVERED_PERCENTValue;  
            }  

          set { SetProperty(ref  RECOVERED_PERCENTValue, value); }
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
private  System.DateTime REPORTED_DATEValue; 
 public System.DateTime REPORTED_DATE
        {  
            get  
            {  
                return this.REPORTED_DATEValue;  
            }  

          set { SetProperty(ref  REPORTED_DATEValue, value); }
        } 
private  System.DateTime RESULTS_RECEIVED_DATEValue; 
 public System.DateTime RESULTS_RECEIVED_DATE
        {  
            get  
            {  
                return this.RESULTS_RECEIVED_DATEValue;  
            }  

          set { SetProperty(ref  RESULTS_RECEIVED_DATEValue, value); }
        } 
private  System.String RESULTS_RECEIVED_INDValue; 
 public System.String RESULTS_RECEIVED_IND
        {  
            get  
            {  
                return this.RESULTS_RECEIVED_INDValue;  
            }  

          set { SetProperty(ref  RESULTS_RECEIVED_INDValue, value); }
        } 
private  System.String SAMPLE_FRACTION_TYPEValue; 
 public System.String SAMPLE_FRACTION_TYPE
        {  
            get  
            {  
                return this.SAMPLE_FRACTION_TYPEValue;  
            }  

          set { SetProperty(ref  SAMPLE_FRACTION_TYPEValue, value); }
        } 
private  System.String SAMPLE_QUALITYValue; 
 public System.String SAMPLE_QUALITY
        {  
            get  
            {  
                return this.SAMPLE_QUALITYValue;  
            }  

          set { SetProperty(ref  SAMPLE_QUALITYValue, value); }
        } 
private  System.String SAMPLE_QUALITY_DESCValue; 
 public System.String SAMPLE_QUALITY_DESC
        {  
            get  
            {  
                return this.SAMPLE_QUALITY_DESCValue;  
            }  

          set { SetProperty(ref  SAMPLE_QUALITY_DESCValue, value); }
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
private  System.DateTime START_DATEValue; 
 public System.DateTime START_DATE
        {  
            get  
            {  
                return this.START_DATEValue;  
            }  

          set { SetProperty(ref  START_DATEValue, value); }
        } 
private  System.String STEP_COMPLETED_INDValue; 
 public System.String STEP_COMPLETED_IND
        {  
            get  
            {  
                return this.STEP_COMPLETED_INDValue;  
            }  

          set { SetProperty(ref  STEP_COMPLETED_INDValue, value); }
        } 
private  System.String STEP_DESCValue; 
 public System.String STEP_DESC
        {  
            get  
            {  
                return this.STEP_DESCValue;  
            }  

          set { SetProperty(ref  STEP_DESCValue, value); }
        } 
private  System.String STEP_QUALITY_DESCValue; 
 public System.String STEP_QUALITY_DESC
        {  
            get  
            {  
                return this.STEP_QUALITY_DESCValue;  
            }  

          set { SetProperty(ref  STEP_QUALITY_DESCValue, value); }
        } 
private  System.String STEP_QUALITY_TYPEValue; 
 public System.String STEP_QUALITY_TYPE
        {  
            get  
            {  
                return this.STEP_QUALITY_TYPEValue;  
            }  

          set { SetProperty(ref  STEP_QUALITY_TYPEValue, value); }
        } 
private  System.String STEP_REQUESTED_INDValue; 
 public System.String STEP_REQUESTED_IND
        {  
            get  
            {  
                return this.STEP_REQUESTED_INDValue;  
            }  

          set { SetProperty(ref  STEP_REQUESTED_INDValue, value); }
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


    public ANL_ANALYSIS_STEP () { }

  }
}


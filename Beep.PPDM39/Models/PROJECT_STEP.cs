using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.PPDM39.Models 
{
public partial class PROJECT_STEP: Entity

{

private  System.String PROJECT_IDValue; 
 public System.String PROJECT_ID
        {  
            get  
            {  
                return this.PROJECT_IDValue;  
            }  

          set { SetProperty(ref  PROJECT_IDValue, value); }
        } 
private  System.String STEP_IDValue; 
 public System.String STEP_ID
        {  
            get  
            {  
                return this.STEP_IDValue;  
            }  

          set { SetProperty(ref  STEP_IDValue, value); }
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
private  System.DateTime ACTUAL_END_DATEValue; 
 public System.DateTime ACTUAL_END_DATE
        {  
            get  
            {  
                return this.ACTUAL_END_DATEValue;  
            }  

          set { SetProperty(ref  ACTUAL_END_DATEValue, value); }
        } 
private  System.DateTime ACTUAL_START_DATEValue; 
 public System.DateTime ACTUAL_START_DATE
        {  
            get  
            {  
                return this.ACTUAL_START_DATEValue;  
            }  

          set { SetProperty(ref  ACTUAL_START_DATEValue, value); }
        } 
private  System.Decimal ACTUAL_TIME_ELAPSEDValue; 
 public System.Decimal ACTUAL_TIME_ELAPSED
        {  
            get  
            {  
                return this.ACTUAL_TIME_ELAPSEDValue;  
            }  

          set { SetProperty(ref  ACTUAL_TIME_ELAPSEDValue, value); }
        } 
private  System.String ACTUAL_TIME_ELAPSED_OUOMValue; 
 public System.String ACTUAL_TIME_ELAPSED_OUOM
        {  
            get  
            {  
                return this.ACTUAL_TIME_ELAPSED_OUOMValue;  
            }  

          set { SetProperty(ref  ACTUAL_TIME_ELAPSED_OUOMValue, value); }
        } 
private  System.String ACTUAL_TIME_ELAPSED_UOMValue; 
 public System.String ACTUAL_TIME_ELAPSED_UOM
        {  
            get  
            {  
                return this.ACTUAL_TIME_ELAPSED_UOMValue;  
            }  

          set { SetProperty(ref  ACTUAL_TIME_ELAPSED_UOMValue, value); }
        } 
private  System.DateTime CRITICAL_DATEValue; 
 public System.DateTime CRITICAL_DATE
        {  
            get  
            {  
                return this.CRITICAL_DATEValue;  
            }  

          set { SetProperty(ref  CRITICAL_DATEValue, value); }
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
private  System.DateTime DUE_DATEValue; 
 public System.DateTime DUE_DATE
        {  
            get  
            {  
                return this.DUE_DATEValue;  
            }  

          set { SetProperty(ref  DUE_DATEValue, value); }
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
private  System.Decimal ESTIMATED_TIME_ELAPSEDValue; 
 public System.Decimal ESTIMATED_TIME_ELAPSED
        {  
            get  
            {  
                return this.ESTIMATED_TIME_ELAPSEDValue;  
            }  

          set { SetProperty(ref  ESTIMATED_TIME_ELAPSEDValue, value); }
        } 
private  System.String ESTIMATED_TIME_ELAPSED_OUOMValue; 
 public System.String ESTIMATED_TIME_ELAPSED_OUOM
        {  
            get  
            {  
                return this.ESTIMATED_TIME_ELAPSED_OUOMValue;  
            }  

          set { SetProperty(ref  ESTIMATED_TIME_ELAPSED_OUOMValue, value); }
        } 
private  System.String ESTIMATED_TIME_ELAPSED_UOMValue; 
 public System.String ESTIMATED_TIME_ELAPSED_UOM
        {  
            get  
            {  
                return this.ESTIMATED_TIME_ELAPSED_UOMValue;  
            }  

          set { SetProperty(ref  ESTIMATED_TIME_ELAPSED_UOMValue, value); }
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
private  System.DateTime PLAN_END_DATEValue; 
 public System.DateTime PLAN_END_DATE
        {  
            get  
            {  
                return this.PLAN_END_DATEValue;  
            }  

          set { SetProperty(ref  PLAN_END_DATEValue, value); }
        } 
private  System.DateTime PLAN_START_DATEValue; 
 public System.DateTime PLAN_START_DATE
        {  
            get  
            {  
                return this.PLAN_START_DATEValue;  
            }  

          set { SetProperty(ref  PLAN_START_DATEValue, value); }
        } 
private  System.String PLAN_STEP_IDValue; 
 public System.String PLAN_STEP_ID
        {  
            get  
            {  
                return this.PLAN_STEP_IDValue;  
            }  

          set { SetProperty(ref  PLAN_STEP_IDValue, value); }
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
private  System.String PROJECT_PLAN_IDValue; 
 public System.String PROJECT_PLAN_ID
        {  
            get  
            {  
                return this.PROJECT_PLAN_IDValue;  
            }  

          set { SetProperty(ref  PROJECT_PLAN_IDValue, value); }
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
private  System.String STEP_NAMEValue; 
 public System.String STEP_NAME
        {  
            get  
            {  
                return this.STEP_NAMEValue;  
            }  

          set { SetProperty(ref  STEP_NAMEValue, value); }
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
private  System.String STEP_TYPEValue; 
 public System.String STEP_TYPE
        {  
            get  
            {  
                return this.STEP_TYPEValue;  
            }  

          set { SetProperty(ref  STEP_TYPEValue, value); }
        } 
private  System.String WHERE_COMPLETEDValue; 
 public System.String WHERE_COMPLETED
        {  
            get  
            {  
                return this.WHERE_COMPLETEDValue;  
            }  

          set { SetProperty(ref  WHERE_COMPLETEDValue, value); }
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


    public PROJECT_STEP () { }

  }
}


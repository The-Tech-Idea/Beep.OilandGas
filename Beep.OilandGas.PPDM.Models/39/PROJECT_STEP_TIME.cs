using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class PROJECT_STEP_TIME: Entity,IPPDMEntity

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
private  System.Decimal TIME_OBS_NOValue; 
 public System.Decimal TIME_OBS_NO
        {  
            get  
            {  
                return this.TIME_OBS_NOValue;  
            }  

          set { SetProperty(ref  TIME_OBS_NOValue, value); }
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
private  System.String BUSINESS_ASSOCIATE_IDValue; 
 public System.String BUSINESS_ASSOCIATE_ID
        {  
            get  
            {  
                return this.BUSINESS_ASSOCIATE_IDValue;  
            }  

          set { SetProperty(ref  BUSINESS_ASSOCIATE_IDValue, value); }
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
private  System.DateTime? END_DATEValue; 
 public System.DateTime? END_DATE
        {  
            get  
            {  
                return this.END_DATEValue;  
            }  

          set { SetProperty(ref  END_DATEValue, value); }
        } 
private  System.DateTime? END_TIMEValue; 
 public System.DateTime? END_TIME
        {  
            get  
            {  
                return this.END_TIMEValue;  
            }  

          set { SetProperty(ref  END_TIMEValue, value); }
        } 
private  System.String END_TIMEZONEValue; 
 public System.String END_TIMEZONE
        {  
            get  
            {  
                return this.END_TIMEZONEValue;  
            }  

          set { SetProperty(ref  END_TIMEZONEValue, value); }
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
private  System.String PLAN_INDValue; 
 public System.String PLAN_IND
        {  
            get  
            {  
                return this.PLAN_INDValue;  
            }  

          set { SetProperty(ref  PLAN_INDValue, value); }
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
private  System.DateTime? START_DATEValue; 
 public System.DateTime? START_DATE
        {  
            get  
            {  
                return this.START_DATEValue;  
            }  

          set { SetProperty(ref  START_DATEValue, value); }
        } 
private  System.DateTime? START_TIMEValue; 
 public System.DateTime? START_TIME
        {  
            get  
            {  
                return this.START_TIMEValue;  
            }  

          set { SetProperty(ref  START_TIMEValue, value); }
        } 
private  System.String START_TIMEZONEValue; 
 public System.String START_TIMEZONE
        {  
            get  
            {  
                return this.START_TIMEZONEValue;  
            }  

          set { SetProperty(ref  START_TIMEZONEValue, value); }
        } 
private  System.Decimal TOTAL_TIME_ELAPSEDValue; 
 public System.Decimal TOTAL_TIME_ELAPSED
        {  
            get  
            {  
                return this.TOTAL_TIME_ELAPSEDValue;  
            }  

          set { SetProperty(ref  TOTAL_TIME_ELAPSEDValue, value); }
        } 
private  System.String TOTAL_TIME_ELAPSED_OUOMValue; 
 public System.String TOTAL_TIME_ELAPSED_OUOM
        {  
            get  
            {  
                return this.TOTAL_TIME_ELAPSED_OUOMValue;  
            }  

          set { SetProperty(ref  TOTAL_TIME_ELAPSED_OUOMValue, value); }
        } 
private  System.String TOTAL_TIME_ELAPSED_UOMValue; 
 public System.String TOTAL_TIME_ELAPSED_UOM
        {  
            get  
            {  
                return this.TOTAL_TIME_ELAPSED_UOMValue;  
            }  

          set { SetProperty(ref  TOTAL_TIME_ELAPSED_UOMValue, value); }
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


    public PROJECT_STEP_TIME () { }

  }
}


using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_ACTIVITY_DURATION: Entity,IPPDMEntity

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
private  System.Decimal ACTIVITY_OBS_NOValue; 
 public System.Decimal ACTIVITY_OBS_NO
        {  
            get  
            {  
                return this.ACTIVITY_OBS_NOValue;  
            }  

          set { SetProperty(ref  ACTIVITY_OBS_NOValue, value); }
        } 
private  System.Decimal DURATION_OBS_NOValue; 
 public System.Decimal DURATION_OBS_NO
        {  
            get  
            {  
                return this.DURATION_OBS_NOValue;  
            }  

          set { SetProperty(ref  DURATION_OBS_NOValue, value); }
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
private  System.Decimal ACTIVITY_DURATIONValue; 
 public System.Decimal ACTIVITY_DURATION
        {  
            get  
            {  
                return this.ACTIVITY_DURATIONValue;  
            }  

          set { SetProperty(ref  ACTIVITY_DURATIONValue, value); }
        } 
private  System.String ACTIVITY_DURATION_OUOMValue; 
 public System.String ACTIVITY_DURATION_OUOM
        {  
            get  
            {  
                return this.ACTIVITY_DURATION_OUOMValue;  
            }  

          set { SetProperty(ref  ACTIVITY_DURATION_OUOMValue, value); }
        } 
private  System.String DOWNTIME_TYPEValue; 
 public System.String DOWNTIME_TYPE
        {  
            get  
            {  
                return this.DOWNTIME_TYPEValue;  
            }  

          set { SetProperty(ref  DOWNTIME_TYPEValue, value); }
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
private  System.DateTime? EVENT_DATEValue; 
 public System.DateTime? EVENT_DATE
        {  
            get  
            {  
                return this.EVENT_DATEValue;  
            }  

          set { SetProperty(ref  EVENT_DATEValue, value); }
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
private  System.Decimal PERIOD_OBS_NOValue; 
 public System.Decimal PERIOD_OBS_NO
        {  
            get  
            {  
                return this.PERIOD_OBS_NOValue;  
            }  

          set { SetProperty(ref  PERIOD_OBS_NOValue, value); }
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


    public WELL_ACTIVITY_DURATION () { }

  }
}


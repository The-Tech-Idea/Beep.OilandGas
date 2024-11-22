using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.PPDM39.Models 
{
public partial class WELL_BA_SERVICE: Entity

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
private  System.String PROVIDED_BYValue; 
 public System.String PROVIDED_BY
        {  
            get  
            {  
                return this.PROVIDED_BYValue;  
            }  

          set { SetProperty(ref  PROVIDED_BYValue, value); }
        } 
private  System.Decimal SERVICE_SEQ_NOValue; 
 public System.Decimal SERVICE_SEQ_NO
        {  
            get  
            {  
                return this.SERVICE_SEQ_NOValue;  
            }  

          set { SetProperty(ref  SERVICE_SEQ_NOValue, value); }
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
private  System.String CONTACT_BA_IDValue; 
 public System.String CONTACT_BA_ID
        {  
            get  
            {  
                return this.CONTACT_BA_IDValue;  
            }  

          set { SetProperty(ref  CONTACT_BA_IDValue, value); }
        } 
private  System.String CONTRACT_IDValue; 
 public System.String CONTRACT_ID
        {  
            get  
            {  
                return this.CONTRACT_IDValue;  
            }  

          set { SetProperty(ref  CONTRACT_IDValue, value); }
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
private  System.String PPDM_GUIDValue; 
 public System.String PPDM_GUID
        {  
            get  
            {  
                return this.PPDM_GUIDValue;  
            }  

          set { SetProperty(ref  PPDM_GUIDValue, value); }
        } 
private  System.String PROVIDED_FORValue; 
 public System.String PROVIDED_FOR
        {  
            get  
            {  
                return this.PROVIDED_FORValue;  
            }  

          set { SetProperty(ref  PROVIDED_FORValue, value); }
        } 
private  System.String PROVISION_IDValue; 
 public System.String PROVISION_ID
        {  
            get  
            {  
                return this.PROVISION_IDValue;  
            }  

          set { SetProperty(ref  PROVISION_IDValue, value); }
        } 
private  System.String RATE_SCHEDULE_IDValue; 
 public System.String RATE_SCHEDULE_ID
        {  
            get  
            {  
                return this.RATE_SCHEDULE_IDValue;  
            }  

          set { SetProperty(ref  RATE_SCHEDULE_IDValue, value); }
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
private  System.String REPRESENTED_BA_IDValue; 
 public System.String REPRESENTED_BA_ID
        {  
            get  
            {  
                return this.REPRESENTED_BA_IDValue;  
            }  

          set { SetProperty(ref  REPRESENTED_BA_IDValue, value); }
        } 
private  System.DateTime SERVICE_DATEValue; 
 public System.DateTime SERVICE_DATE
        {  
            get  
            {  
                return this.SERVICE_DATEValue;  
            }  

          set { SetProperty(ref  SERVICE_DATEValue, value); }
        } 
private  System.Decimal SERVICE_PERIODValue; 
 public System.Decimal SERVICE_PERIOD
        {  
            get  
            {  
                return this.SERVICE_PERIODValue;  
            }  

          set { SetProperty(ref  SERVICE_PERIODValue, value); }
        } 
private  System.String SERVICE_PERIOD_UOMValue; 
 public System.String SERVICE_PERIOD_UOM
        {  
            get  
            {  
                return this.SERVICE_PERIOD_UOMValue;  
            }  

          set { SetProperty(ref  SERVICE_PERIOD_UOMValue, value); }
        } 
private  System.String SERVICE_QUALITYValue; 
 public System.String SERVICE_QUALITY
        {  
            get  
            {  
                return this.SERVICE_QUALITYValue;  
            }  

          set { SetProperty(ref  SERVICE_QUALITYValue, value); }
        } 
private  System.DateTime SERVICE_TIMEValue; 
 public System.DateTime SERVICE_TIME
        {  
            get  
            {  
                return this.SERVICE_TIMEValue;  
            }  

          set { SetProperty(ref  SERVICE_TIMEValue, value); }
        } 
private  System.String SERVICE_TIMEZONEValue; 
 public System.String SERVICE_TIMEZONE
        {  
            get  
            {  
                return this.SERVICE_TIMEZONEValue;  
            }  

          set { SetProperty(ref  SERVICE_TIMEZONEValue, value); }
        } 
private  System.String SERVICE_TIME_DESCValue; 
 public System.String SERVICE_TIME_DESC
        {  
            get  
            {  
                return this.SERVICE_TIME_DESCValue;  
            }  

          set { SetProperty(ref  SERVICE_TIME_DESCValue, value); }
        } 
private  System.String SERVICE_TYPEValue; 
 public System.String SERVICE_TYPE
        {  
            get  
            {  
                return this.SERVICE_TYPEValue;  
            }  

          set { SetProperty(ref  SERVICE_TYPEValue, value); }
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
private  System.Decimal WELL_ACTIVITY_OBS_NOValue; 
 public System.Decimal WELL_ACTIVITY_OBS_NO
        {  
            get  
            {  
                return this.WELL_ACTIVITY_OBS_NOValue;  
            }  

          set { SetProperty(ref  WELL_ACTIVITY_OBS_NOValue, value); }
        } 
private  System.String WELL_ACTIVITY_SOURCEValue; 
 public System.String WELL_ACTIVITY_SOURCE
        {  
            get  
            {  
                return this.WELL_ACTIVITY_SOURCEValue;  
            }  

          set { SetProperty(ref  WELL_ACTIVITY_SOURCEValue, value); }
        } 
private  System.Decimal WELL_DRILL_PERIOD_OBS_NOValue; 
 public System.Decimal WELL_DRILL_PERIOD_OBS_NO
        {  
            get  
            {  
                return this.WELL_DRILL_PERIOD_OBS_NOValue;  
            }  

          set { SetProperty(ref  WELL_DRILL_PERIOD_OBS_NOValue, value); }
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


    public WELL_BA_SERVICE () { }

  }
}


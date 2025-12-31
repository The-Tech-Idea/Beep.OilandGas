using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class HSE_INCIDENT: Entity,IPPDMEntity

{

private  System.String INCIDENT_IDValue; 
 public System.String INCIDENT_ID
        {  
            get  
            {  
                return this.INCIDENT_IDValue;  
            }  

          set { SetProperty(ref  INCIDENT_IDValue, value); }
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
private  System.String INCIDENT_CLASS_IDValue; 
 public System.String INCIDENT_CLASS_ID
        {  
            get  
            {  
                return this.INCIDENT_CLASS_IDValue;  
            }  

          set { SetProperty(ref  INCIDENT_CLASS_IDValue, value); }
        } 
private  System.DateTime? INCIDENT_DATEValue; 
 public System.DateTime? INCIDENT_DATE
        {  
            get  
            {  
                return this.INCIDENT_DATEValue;  
            }  

          set { SetProperty(ref  INCIDENT_DATEValue, value); }
        } 
private  System.Decimal INCIDENT_DURATIONValue; 
 public System.Decimal INCIDENT_DURATION
        {  
            get  
            {  
                return this.INCIDENT_DURATIONValue;  
            }  

          set { SetProperty(ref  INCIDENT_DURATIONValue, value); }
        } 
private  System.String INCIDENT_DURATION_OUOMValue; 
 public System.String INCIDENT_DURATION_OUOM
        {  
            get  
            {  
                return this.INCIDENT_DURATION_OUOMValue;  
            }  

          set { SetProperty(ref  INCIDENT_DURATION_OUOMValue, value); }
        } 
private  System.String INCIDENT_DURATION_UOMValue; 
 public System.String INCIDENT_DURATION_UOM
        {  
            get  
            {  
                return this.INCIDENT_DURATION_UOMValue;  
            }  

          set { SetProperty(ref  INCIDENT_DURATION_UOMValue, value); }
        } 
private  System.String LOST_TIME_INDValue; 
 public System.String LOST_TIME_IND
        {  
            get  
            {  
                return this.LOST_TIME_INDValue;  
            }  

          set { SetProperty(ref  LOST_TIME_INDValue, value); }
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
private  System.DateTime? RECORDED_TIMEValue; 
 public System.DateTime? RECORDED_TIME
        {  
            get  
            {  
                return this.RECORDED_TIMEValue;  
            }  

          set { SetProperty(ref  RECORDED_TIMEValue, value); }
        } 
private  System.String RECORDED_TIMEZONEValue; 
 public System.String RECORDED_TIMEZONE
        {  
            get  
            {  
                return this.RECORDED_TIMEZONEValue;  
            }  

          set { SetProperty(ref  RECORDED_TIMEZONEValue, value); }
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
private  System.String REPORTED_BY_BA_IDValue; 
 public System.String REPORTED_BY_BA_ID
        {  
            get  
            {  
                return this.REPORTED_BY_BA_IDValue;  
            }  

          set { SetProperty(ref  REPORTED_BY_BA_IDValue, value); }
        } 
private  System.String REPORTED_BY_NAMEValue; 
 public System.String REPORTED_BY_NAME
        {  
            get  
            {  
                return this.REPORTED_BY_NAMEValue;  
            }  

          set { SetProperty(ref  REPORTED_BY_NAMEValue, value); }
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
private  System.String WORK_RELATED_INDValue; 
 public System.String WORK_RELATED_IND
        {  
            get  
            {  
                return this.WORK_RELATED_INDValue;  
            }  

          set { SetProperty(ref  WORK_RELATED_INDValue, value); }
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


    public HSE_INCIDENT () { }

  }
}


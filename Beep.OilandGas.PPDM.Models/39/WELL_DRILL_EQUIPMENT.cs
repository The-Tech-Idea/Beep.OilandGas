using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_DRILL_EQUIPMENT: Entity,IPPDMEntity

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
private  System.String EQUIPMENT_IDValue; 
 public System.String EQUIPMENT_ID
        {  
            get  
            {  
                return this.EQUIPMENT_IDValue;  
            }  

          set { SetProperty(ref  EQUIPMENT_IDValue, value); }
        } 
private  System.Decimal EQUIPMENT_OBS_NOValue; 
 public System.Decimal EQUIPMENT_OBS_NO
        {  
            get  
            {  
                return this.EQUIPMENT_OBS_NOValue;  
            }  

          set { SetProperty(ref  EQUIPMENT_OBS_NOValue, value); }
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
private  System.DateTime? OFFSITE_DATEValue; 
 public System.DateTime? OFFSITE_DATE
        {  
            get  
            {  
                return this.OFFSITE_DATEValue;  
            }  

          set { SetProperty(ref  OFFSITE_DATEValue, value); }
        } 
private  System.DateTime? OFFSITE_TIMEValue; 
 public System.DateTime? OFFSITE_TIME
        {  
            get  
            {  
                return this.OFFSITE_TIMEValue;  
            }  

          set { SetProperty(ref  OFFSITE_TIMEValue, value); }
        } 
private  System.DateTime? ONSITE_DATEValue; 
 public System.DateTime? ONSITE_DATE
        {  
            get  
            {  
                return this.ONSITE_DATEValue;  
            }  

          set { SetProperty(ref  ONSITE_DATEValue, value); }
        } 
private  System.DateTime? ONSITE_TIMEValue; 
 public System.DateTime? ONSITE_TIME
        {  
            get  
            {  
                return this.ONSITE_TIMEValue;  
            }  

          set { SetProperty(ref  ONSITE_TIMEValue, value); }
        } 
private  System.String OPERATED_BY_BA_IDValue; 
 public System.String OPERATED_BY_BA_ID
        {  
            get  
            {  
                return this.OPERATED_BY_BA_IDValue;  
            }  

          set { SetProperty(ref  OPERATED_BY_BA_IDValue, value); }
        } 
private  System.String PARENT_EQUIPMENT_IDValue; 
 public System.String PARENT_EQUIPMENT_ID
        {  
            get  
            {  
                return this.PARENT_EQUIPMENT_IDValue;  
            }  

          set { SetProperty(ref  PARENT_EQUIPMENT_IDValue, value); }
        } 
private  System.Decimal PERIOD_ON_WELLValue; 
 public System.Decimal PERIOD_ON_WELL
        {  
            get  
            {  
                return this.PERIOD_ON_WELLValue;  
            }  

          set { SetProperty(ref  PERIOD_ON_WELLValue, value); }
        } 
private  System.String PERIOD_ON_WELL_OUOMValue; 
 public System.String PERIOD_ON_WELL_OUOM
        {  
            get  
            {  
                return this.PERIOD_ON_WELL_OUOMValue;  
            }  

          set { SetProperty(ref  PERIOD_ON_WELL_OUOMValue, value); }
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
private  System.String REFERENCE_NUMValue; 
 public System.String REFERENCE_NUM
        {  
            get  
            {  
                return this.REFERENCE_NUMValue;  
            }  

          set { SetProperty(ref  REFERENCE_NUMValue, value); }
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
private  System.String RENTED_INDValue; 
 public System.String RENTED_IND
        {  
            get  
            {  
                return this.RENTED_INDValue;  
            }  

          set { SetProperty(ref  RENTED_INDValue, value); }
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
private  System.String TIMEZONEValue; 
 public System.String TIMEZONE
        {  
            get  
            {  
                return this.TIMEZONEValue;  
            }  

          set { SetProperty(ref  TIMEZONEValue, value); }
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


    public WELL_DRILL_EQUIPMENT () { }

  }
}


using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class HSE_INCIDENT_EQUIP: Entity,IPPDMEntity

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
private  System.Decimal EQUIP_OBS_NOValue; 
 public System.Decimal EQUIP_OBS_NO
        {  
            get  
            {  
                return this.EQUIP_OBS_NOValue;  
            }  

          set { SetProperty(ref  EQUIP_OBS_NOValue, value); }
        } 
private  System.Decimal EQUIP_ROLE_OBS_NOValue; 
 public System.Decimal EQUIP_ROLE_OBS_NO
        {  
            get  
            {  
                return this.EQUIP_ROLE_OBS_NOValue;  
            }  

          set { SetProperty(ref  EQUIP_ROLE_OBS_NOValue, value); }
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
private  System.String CREW_ROLEValue; 
 public System.String CREW_ROLE
        {  
            get  
            {  
                return this.CREW_ROLEValue;  
            }  

          set { SetProperty(ref  CREW_ROLEValue, value); }
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
private  System.String EQUIPMENT_GROUPValue; 
 public System.String EQUIPMENT_GROUP
        {  
            get  
            {  
                return this.EQUIPMENT_GROUPValue;  
            }  

          set { SetProperty(ref  EQUIPMENT_GROUPValue, value); }
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
private  System.String EQUIPMENT_TYPEValue; 
 public System.String EQUIPMENT_TYPE
        {  
            get  
            {  
                return this.EQUIPMENT_TYPEValue;  
            }  

          set { SetProperty(ref  EQUIPMENT_TYPEValue, value); }
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
private  System.String SOURCEValue; 
 public System.String SOURCE
        {  
            get  
            {  
                return this.SOURCEValue;  
            }  

          set { SetProperty(ref  SOURCEValue, value); }
        } 
private  System.String UWIValue; 
 public System.String UWI
        {  
            get  
            {  
                return this.UWIValue;  
            }  

          set { SetProperty(ref  UWIValue, value); }
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


    public HSE_INCIDENT_EQUIP () { }

  }
}


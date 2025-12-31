using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class EQUIPMENT_MAINTAIN: Entity,IPPDMEntity

{

private  System.String EQUIPMENT_IDValue; 
 public System.String EQUIPMENT_ID
        {  
            get  
            {  
                return this.EQUIPMENT_IDValue;  
            }  

          set { SetProperty(ref  EQUIPMENT_IDValue, value); }
        } 
private  System.String EQUIP_MAINT_IDValue; 
 public System.String EQUIP_MAINT_ID
        {  
            get  
            {  
                return this.EQUIP_MAINT_IDValue;  
            }  

          set { SetProperty(ref  EQUIP_MAINT_IDValue, value); }
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
private  System.DateTime? ACTUAL_END_DATEValue; 
 public System.DateTime? ACTUAL_END_DATE
        {  
            get  
            {  
                return this.ACTUAL_END_DATEValue;  
            }  

          set { SetProperty(ref  ACTUAL_END_DATEValue, value); }
        } 
private  System.DateTime? ACTUAL_START_DATEValue; 
 public System.DateTime? ACTUAL_START_DATE
        {  
            get  
            {  
                return this.ACTUAL_START_DATEValue;  
            }  

          set { SetProperty(ref  ACTUAL_START_DATEValue, value); }
        } 
private  System.String CATALOGUE_EQUIP_IDValue; 
 public System.String CATALOGUE_EQUIP_ID
        {  
            get  
            {  
                return this.CATALOGUE_EQUIP_IDValue;  
            }  

          set { SetProperty(ref  CATALOGUE_EQUIP_IDValue, value); }
        } 
private  System.String COMPLETED_BY_BA_IDValue; 
 public System.String COMPLETED_BY_BA_ID
        {  
            get  
            {  
                return this.COMPLETED_BY_BA_IDValue;  
            }  

          set { SetProperty(ref  COMPLETED_BY_BA_IDValue, value); }
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
private  System.DateTime? EXPIRY_DATEValue; 
 public System.DateTime? EXPIRY_DATE
        {  
            get  
            {  
                return this.EXPIRY_DATEValue;  
            }  

          set { SetProperty(ref  EXPIRY_DATEValue, value); }
        } 
private  System.String FAILURE_INDValue; 
 public System.String FAILURE_IND
        {  
            get  
            {  
                return this.FAILURE_INDValue;  
            }  

          set { SetProperty(ref  FAILURE_INDValue, value); }
        } 
private  System.Decimal LOCATION_BA_ADDRESS_OBS_NOValue; 
 public System.Decimal LOCATION_BA_ADDRESS_OBS_NO
        {  
            get  
            {  
                return this.LOCATION_BA_ADDRESS_OBS_NOValue;  
            }  

          set { SetProperty(ref  LOCATION_BA_ADDRESS_OBS_NOValue, value); }
        } 
private  System.String LOCATION_BA_IDValue; 
 public System.String LOCATION_BA_ID
        {  
            get  
            {  
                return this.LOCATION_BA_IDValue;  
            }  

          set { SetProperty(ref  LOCATION_BA_IDValue, value); }
        } 
private  System.String LOCATION_BA_SOURCEValue; 
 public System.String LOCATION_BA_SOURCE
        {  
            get  
            {  
                return this.LOCATION_BA_SOURCEValue;  
            }  

          set { SetProperty(ref  LOCATION_BA_SOURCEValue, value); }
        } 
private  System.String MAINT_LOCATION_TYPEValue; 
 public System.String MAINT_LOCATION_TYPE
        {  
            get  
            {  
                return this.MAINT_LOCATION_TYPEValue;  
            }  

          set { SetProperty(ref  MAINT_LOCATION_TYPEValue, value); }
        } 
private  System.String MAINT_REASONValue; 
 public System.String MAINT_REASON
        {  
            get  
            {  
                return this.MAINT_REASONValue;  
            }  

          set { SetProperty(ref  MAINT_REASONValue, value); }
        } 
private  System.String MAINT_TYPEValue; 
 public System.String MAINT_TYPE
        {  
            get  
            {  
                return this.MAINT_TYPEValue;  
            }  

          set { SetProperty(ref  MAINT_TYPEValue, value); }
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
private  System.String PROJECT_IDValue; 
 public System.String PROJECT_ID
        {  
            get  
            {  
                return this.PROJECT_IDValue;  
            }  

          set { SetProperty(ref  PROJECT_IDValue, value); }
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
private  System.DateTime? SCHEDULED_DATEValue; 
 public System.DateTime? SCHEDULED_DATE
        {  
            get  
            {  
                return this.SCHEDULED_DATEValue;  
            }  

          set { SetProperty(ref  SCHEDULED_DATEValue, value); }
        } 
private  System.String SCHEDULED_INDValue; 
 public System.String SCHEDULED_IND
        {  
            get  
            {  
                return this.SCHEDULED_INDValue;  
            }  

          set { SetProperty(ref  SCHEDULED_INDValue, value); }
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
private  System.String SYSTEM_CONDITIONValue; 
 public System.String SYSTEM_CONDITION
        {  
            get  
            {  
                return this.SYSTEM_CONDITIONValue;  
            }  

          set { SetProperty(ref  SYSTEM_CONDITIONValue, value); }
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


    public EQUIPMENT_MAINTAIN () { }

  }
}


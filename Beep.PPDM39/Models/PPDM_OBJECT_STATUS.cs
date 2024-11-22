using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.PPDM39.Models 
{
public partial class PPDM_OBJECT_STATUS: Entity

{

private  System.String STATUS_IDValue; 
 public System.String STATUS_ID
        {  
            get  
            {  
                return this.STATUS_IDValue;  
            }  

          set { SetProperty(ref  STATUS_IDValue, value); }
        } 
private  System.Decimal STATUS_OBS_NOValue; 
 public System.Decimal STATUS_OBS_NO
        {  
            get  
            {  
                return this.STATUS_OBS_NOValue;  
            }  

          set { SetProperty(ref  STATUS_OBS_NOValue, value); }
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
private  System.Decimal CODE_VERSION_OBS_NOValue; 
 public System.Decimal CODE_VERSION_OBS_NO
        {  
            get  
            {  
                return this.CODE_VERSION_OBS_NOValue;  
            }  

          set { SetProperty(ref  CODE_VERSION_OBS_NOValue, value); }
        } 
private  System.String CODE_VERSION_SOURCEValue; 
 public System.String CODE_VERSION_SOURCE
        {  
            get  
            {  
                return this.CODE_VERSION_SOURCEValue;  
            }  

          set { SetProperty(ref  CODE_VERSION_SOURCEValue, value); }
        } 
private  System.String COLUMN_NAMEValue; 
 public System.String COLUMN_NAME
        {  
            get  
            {  
                return this.COLUMN_NAMEValue;  
            }  

          set { SetProperty(ref  COLUMN_NAMEValue, value); }
        } 
private  System.String CONSTRAINT_NAMEValue; 
 public System.String CONSTRAINT_NAME
        {  
            get  
            {  
                return this.CONSTRAINT_NAMEValue;  
            }  

          set { SetProperty(ref  CONSTRAINT_NAMEValue, value); }
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
private  System.String INDEX_IDValue; 
 public System.String INDEX_ID
        {  
            get  
            {  
                return this.INDEX_IDValue;  
            }  

          set { SetProperty(ref  INDEX_IDValue, value); }
        } 
private  System.String OBJECT_NAMEValue; 
 public System.String OBJECT_NAME
        {  
            get  
            {  
                return this.OBJECT_NAMEValue;  
            }  

          set { SetProperty(ref  OBJECT_NAMEValue, value); }
        } 
private  System.String OBJECT_STATUSValue; 
 public System.String OBJECT_STATUS
        {  
            get  
            {  
                return this.OBJECT_STATUSValue;  
            }  

          set { SetProperty(ref  OBJECT_STATUSValue, value); }
        } 
private  System.String OBJECT_TYPEValue; 
 public System.String OBJECT_TYPE
        {  
            get  
            {  
                return this.OBJECT_TYPEValue;  
            }  

          set { SetProperty(ref  OBJECT_TYPEValue, value); }
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
private  System.String PROCEDURE_IDValue; 
 public System.String PROCEDURE_ID
        {  
            get  
            {  
                return this.PROCEDURE_IDValue;  
            }  

          set { SetProperty(ref  PROCEDURE_IDValue, value); }
        } 
private  System.String PROPERTY_SET_IDValue; 
 public System.String PROPERTY_SET_ID
        {  
            get  
            {  
                return this.PROPERTY_SET_IDValue;  
            }  

          set { SetProperty(ref  PROPERTY_SET_IDValue, value); }
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
private  System.String RULE_IDValue; 
 public System.String RULE_ID
        {  
            get  
            {  
                return this.RULE_IDValue;  
            }  

          set { SetProperty(ref  RULE_IDValue, value); }
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
private  System.String SYSTEM_IDValue; 
 public System.String SYSTEM_ID
        {  
            get  
            {  
                return this.SYSTEM_IDValue;  
            }  

          set { SetProperty(ref  SYSTEM_IDValue, value); }
        } 
private  System.String TABLE_NAMEValue; 
 public System.String TABLE_NAME
        {  
            get  
            {  
                return this.TABLE_NAMEValue;  
            }  

          set { SetProperty(ref  TABLE_NAMEValue, value); }
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


    public PPDM_OBJECT_STATUS () { }

  }
}

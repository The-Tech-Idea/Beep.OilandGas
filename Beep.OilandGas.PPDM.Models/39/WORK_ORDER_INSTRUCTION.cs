using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WORK_ORDER_INSTRUCTION: Entity,IPPDMEntity

{

private  System.String WORK_ORDER_IDValue; 
 public System.String WORK_ORDER_ID
        {  
            get  
            {  
                return this.WORK_ORDER_IDValue;  
            }  

          set { SetProperty(ref  WORK_ORDER_IDValue, value); }
        } 
private  System.String INSTRUCTION_IDValue; 
 public System.String INSTRUCTION_ID
        {  
            get  
            {  
                return this.INSTRUCTION_IDValue;  
            }  

          set { SetProperty(ref  INSTRUCTION_IDValue, value); }
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
private  System.Decimal BA_ADDRESS_OBS_NOValue; 
 public System.Decimal BA_ADDRESS_OBS_NO
        {  
            get  
            {  
                return this.BA_ADDRESS_OBS_NOValue;  
            }  

          set { SetProperty(ref  BA_ADDRESS_OBS_NOValue, value); }
        } 
private  System.String BA_ADDRESS_SOURCEValue; 
 public System.String BA_ADDRESS_SOURCE
        {  
            get  
            {  
                return this.BA_ADDRESS_SOURCEValue;  
            }  

          set { SetProperty(ref  BA_ADDRESS_SOURCEValue, value); }
        } 
private  System.Decimal BA_OBS_NOValue; 
 public System.Decimal BA_OBS_NO
        {  
            get  
            {  
                return this.BA_OBS_NOValue;  
            }  

          set { SetProperty(ref  BA_OBS_NOValue, value); }
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
private  System.DateTime? EXPIRY_DATEValue; 
 public System.DateTime? EXPIRY_DATE
        {  
            get  
            {  
                return this.EXPIRY_DATEValue;  
            }  

          set { SetProperty(ref  EXPIRY_DATEValue, value); }
        } 
private  System.String INSTRUCTION_DESCValue; 
 public System.String INSTRUCTION_DESC
        {  
            get  
            {  
                return this.INSTRUCTION_DESCValue;  
            }  

          set { SetProperty(ref  INSTRUCTION_DESCValue, value); }
        } 
private  System.String INSTRUCTION_TYPEValue; 
 public System.String INSTRUCTION_TYPE
        {  
            get  
            {  
                return this.INSTRUCTION_TYPEValue;  
            }  

          set { SetProperty(ref  INSTRUCTION_TYPEValue, value); }
        } 
private  System.Decimal INSTRUCTION_VALUEValue; 
 public System.Decimal INSTRUCTION_VALUE
        {  
            get  
            {  
                return this.INSTRUCTION_VALUEValue;  
            }  

          set { SetProperty(ref  INSTRUCTION_VALUEValue, value); }
        } 
private  System.String INSTRUCTION_VALUE_OUOMValue; 
 public System.String INSTRUCTION_VALUE_OUOM
        {  
            get  
            {  
                return this.INSTRUCTION_VALUE_OUOMValue;  
            }  

          set { SetProperty(ref  INSTRUCTION_VALUE_OUOMValue, value); }
        } 
private  System.String INSTRUCTION_VALUE_UOMValue; 
 public System.String INSTRUCTION_VALUE_UOM
        {  
            get  
            {  
                return this.INSTRUCTION_VALUE_UOMValue;  
            }  

          set { SetProperty(ref  INSTRUCTION_VALUE_UOMValue, value); }
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


    public WORK_ORDER_INSTRUCTION () { }

  }
}


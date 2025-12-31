using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class PPDM_SCHEMA_GROUP: Entity,IPPDMEntity

{

private  System.String GROUP_SYSTEM_IDValue; 
 public System.String GROUP_SYSTEM_ID
        {  
            get  
            {  
                return this.GROUP_SYSTEM_IDValue;  
            }  

          set { SetProperty(ref  GROUP_SYSTEM_IDValue, value); }
        } 
private  System.String GROUP_SCHEMA_ENTITY_IDValue; 
 public System.String GROUP_SCHEMA_ENTITY_ID
        {  
            get  
            {  
                return this.GROUP_SCHEMA_ENTITY_IDValue;  
            }  

          set { SetProperty(ref  GROUP_SCHEMA_ENTITY_IDValue, value); }
        } 
private  System.String COMP_SCHEMA_ENTITY_IDValue; 
 public System.String COMP_SCHEMA_ENTITY_ID
        {  
            get  
            {  
                return this.COMP_SCHEMA_ENTITY_IDValue;  
            }  

          set { SetProperty(ref  COMP_SCHEMA_ENTITY_IDValue, value); }
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
private  System.Decimal ENTITY_SEQ_NOValue; 
 public System.Decimal ENTITY_SEQ_NO
        {  
            get  
            {  
                return this.ENTITY_SEQ_NOValue;  
            }  

          set { SetProperty(ref  ENTITY_SEQ_NOValue, value); }
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
private  System.String EXTENSION_INDValue; 
 public System.String EXTENSION_IND
        {  
            get  
            {  
                return this.EXTENSION_INDValue;  
            }  

          set { SetProperty(ref  EXTENSION_INDValue, value); }
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
private  System.String SCHEMA_GROUP_TYPEValue; 
 public System.String SCHEMA_GROUP_TYPE
        {  
            get  
            {  
                return this.SCHEMA_GROUP_TYPEValue;  
            }  

          set { SetProperty(ref  SCHEMA_GROUP_TYPEValue, value); }
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
private  System.String SURROGATE_INDValue; 
 public System.String SURROGATE_IND
        {  
            get  
            {  
                return this.SURROGATE_INDValue;  
            }  

          set { SetProperty(ref  SURROGATE_INDValue, value); }
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


    public PPDM_SCHEMA_GROUP () { }

  }
}


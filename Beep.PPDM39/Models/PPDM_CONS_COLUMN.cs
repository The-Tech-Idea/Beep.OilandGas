using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.PPDM39.Models 
{
public partial class PPDM_CONS_COLUMN: Entity

{

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
private  System.String CONSTRAINT_NAMEValue; 
 public System.String CONSTRAINT_NAME
        {  
            get  
            {  
                return this.CONSTRAINT_NAMEValue;  
            }  

          set { SetProperty(ref  CONSTRAINT_NAMEValue, value); }
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
private  System.String ACTIVE_INDValue; 
 public System.String ACTIVE_IND
        {  
            get  
            {  
                return this.ACTIVE_INDValue;  
            }  

          set { SetProperty(ref  ACTIVE_INDValue, value); }
        } 
private  System.String CONDITIONValue; 
 public System.String CONDITION
        {  
            get  
            {  
                return this.CONDITIONValue;  
            }  

          set { SetProperty(ref  CONDITIONValue, value); }
        } 
private  System.Decimal CONSTRAINT_SEQ_NOValue; 
 public System.Decimal CONSTRAINT_SEQ_NO
        {  
            get  
            {  
                return this.CONSTRAINT_SEQ_NOValue;  
            }  

          set { SetProperty(ref  CONSTRAINT_SEQ_NOValue, value); }
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
private  System.String REFERENCED_COLUMN_NAMEValue; 
 public System.String REFERENCED_COLUMN_NAME
        {  
            get  
            {  
                return this.REFERENCED_COLUMN_NAMEValue;  
            }  

          set { SetProperty(ref  REFERENCED_COLUMN_NAMEValue, value); }
        } 
private  System.String REFERENCED_CONSTRAINT_NAMEValue; 
 public System.String REFERENCED_CONSTRAINT_NAME
        {  
            get  
            {  
                return this.REFERENCED_CONSTRAINT_NAMEValue;  
            }  

          set { SetProperty(ref  REFERENCED_CONSTRAINT_NAMEValue, value); }
        } 
private  System.String REFERENCED_SYSTEM_IDValue; 
 public System.String REFERENCED_SYSTEM_ID
        {  
            get  
            {  
                return this.REFERENCED_SYSTEM_IDValue;  
            }  

          set { SetProperty(ref  REFERENCED_SYSTEM_IDValue, value); }
        } 
private  System.String REFERENCED_TABLE_NAMEValue; 
 public System.String REFERENCED_TABLE_NAME
        {  
            get  
            {  
                return this.REFERENCED_TABLE_NAMEValue;  
            }  

          set { SetProperty(ref  REFERENCED_TABLE_NAMEValue, value); }
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


    public PPDM_CONS_COLUMN () { }

  }
}


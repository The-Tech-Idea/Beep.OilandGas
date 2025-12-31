using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class PPDM_MAP_LOAD: Entity,IPPDMEntity

{

private  System.String MAP_IDValue; 
 public System.String MAP_ID
        {  
            get  
            {  
                return this.MAP_IDValue;  
            }  

          set { SetProperty(ref  MAP_IDValue, value); }
        } 
private  System.String LOAD_PROCESS_IDValue; 
 public System.String LOAD_PROCESS_ID
        {  
            get  
            {  
                return this.LOAD_PROCESS_IDValue;  
            }  

          set { SetProperty(ref  LOAD_PROCESS_IDValue, value); }
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
private  System.String DELETE_ALLOWED_INDValue; 
 public System.String DELETE_ALLOWED_IND
        {  
            get  
            {  
                return this.DELETE_ALLOWED_INDValue;  
            }  

          set { SetProperty(ref  DELETE_ALLOWED_INDValue, value); }
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
private  System.String INSERT_ALLOWED_INDValue; 
 public System.String INSERT_ALLOWED_IND
        {  
            get  
            {  
                return this.INSERT_ALLOWED_INDValue;  
            }  

          set { SetProperty(ref  INSERT_ALLOWED_INDValue, value); }
        } 
private  System.String PPDM_GROUP_IDValue; 
 public System.String PPDM_GROUP_ID
        {  
            get  
            {  
                return this.PPDM_GROUP_IDValue;  
            }  

          set { SetProperty(ref  PPDM_GROUP_IDValue, value); }
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
private  System.String PROCEDURE_NAMEValue; 
 public System.String PROCEDURE_NAME
        {  
            get  
            {  
                return this.PROCEDURE_NAMEValue;  
            }  

          set { SetProperty(ref  PROCEDURE_NAMEValue, value); }
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
private  System.String SOURCE_SYSTEM_IDValue; 
 public System.String SOURCE_SYSTEM_ID
        {  
            get  
            {  
                return this.SOURCE_SYSTEM_IDValue;  
            }  

          set { SetProperty(ref  SOURCE_SYSTEM_IDValue, value); }
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
private  System.String SW_APPLICATION_IDValue; 
 public System.String SW_APPLICATION_ID
        {  
            get  
            {  
                return this.SW_APPLICATION_IDValue;  
            }  

          set { SetProperty(ref  SW_APPLICATION_IDValue, value); }
        } 
private  System.String TARGET_SYSTEM_IDValue; 
 public System.String TARGET_SYSTEM_ID
        {  
            get  
            {  
                return this.TARGET_SYSTEM_IDValue;  
            }  

          set { SetProperty(ref  TARGET_SYSTEM_IDValue, value); }
        } 
private  System.String UPDATE_ALLOWED_INDValue; 
 public System.String UPDATE_ALLOWED_IND
        {  
            get  
            {  
                return this.UPDATE_ALLOWED_INDValue;  
            }  

          set { SetProperty(ref  UPDATE_ALLOWED_INDValue, value); }
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


    public PPDM_MAP_LOAD () { }

  }
}


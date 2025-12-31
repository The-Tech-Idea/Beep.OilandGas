using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class SEIS_LICENSE_VIOLATION: Entity,IPPDMEntity

{

private  System.String SEIS_SET_SUBTYPEValue; 
 public System.String SEIS_SET_SUBTYPE
        {  
            get  
            {  
                return this.SEIS_SET_SUBTYPEValue;  
            }  

          set { SetProperty(ref  SEIS_SET_SUBTYPEValue, value); }
        } 
private  System.String SEIS_SET_IDValue; 
 public System.String SEIS_SET_ID
        {  
            get  
            {  
                return this.SEIS_SET_IDValue;  
            }  

          set { SetProperty(ref  SEIS_SET_IDValue, value); }
        } 
private  System.String LICENSE_IDValue; 
 public System.String LICENSE_ID
        {  
            get  
            {  
                return this.LICENSE_IDValue;  
            }  

          set { SetProperty(ref  LICENSE_IDValue, value); }
        } 
private  System.String VIOLATION_IDValue; 
 public System.String VIOLATION_ID
        {  
            get  
            {  
                return this.VIOLATION_IDValue;  
            }  

          set { SetProperty(ref  VIOLATION_IDValue, value); }
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
private  System.String CONDITION_IDValue; 
 public System.String CONDITION_ID
        {  
            get  
            {  
                return this.CONDITION_IDValue;  
            }  

          set { SetProperty(ref  CONDITION_IDValue, value); }
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
private  System.DateTime? RESOLVE_DATEValue; 
 public System.DateTime? RESOLVE_DATE
        {  
            get  
            {  
                return this.RESOLVE_DATEValue;  
            }  

          set { SetProperty(ref  RESOLVE_DATEValue, value); }
        } 
private  System.String RESOLVE_DESCValue; 
 public System.String RESOLVE_DESC
        {  
            get  
            {  
                return this.RESOLVE_DESCValue;  
            }  

          set { SetProperty(ref  RESOLVE_DESCValue, value); }
        } 
private  System.String RESOLVE_TYPEValue; 
 public System.String RESOLVE_TYPE
        {  
            get  
            {  
                return this.RESOLVE_TYPEValue;  
            }  

          set { SetProperty(ref  RESOLVE_TYPEValue, value); }
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
private  System.DateTime? VIOLATION_DATEValue; 
 public System.DateTime? VIOLATION_DATE
        {  
            get  
            {  
                return this.VIOLATION_DATEValue;  
            }  

          set { SetProperty(ref  VIOLATION_DATEValue, value); }
        } 
private  System.String VIOLATION_DESCValue; 
 public System.String VIOLATION_DESC
        {  
            get  
            {  
                return this.VIOLATION_DESCValue;  
            }  

          set { SetProperty(ref  VIOLATION_DESCValue, value); }
        } 
private  System.String VIOLATION_TYPEValue; 
 public System.String VIOLATION_TYPE
        {  
            get  
            {  
                return this.VIOLATION_TYPEValue;  
            }  

          set { SetProperty(ref  VIOLATION_TYPEValue, value); }
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


    public SEIS_LICENSE_VIOLATION () { }

  }
}


using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class PPDM_RULE: Entity,IPPDMEntity

{

private  System.String RULE_IDValue; 
 public System.String RULE_ID
        {  
            get  
            {  
                return this.RULE_IDValue;  
            }  

          set { SetProperty(ref  RULE_IDValue, value); }
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
private  System.String CURRENT_STATUSValue; 
 public System.String CURRENT_STATUS
        {  
            get  
            {  
                return this.CURRENT_STATUSValue;  
            }  

          set { SetProperty(ref  CURRENT_STATUSValue, value); }
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
private  System.String RULE_CLASSValue; 
 public System.String RULE_CLASS
        {  
            get  
            {  
                return this.RULE_CLASSValue;  
            }  

          set { SetProperty(ref  RULE_CLASSValue, value); }
        } 
private  System.String RULE_DESCValue; 
 public System.String RULE_DESC
        {  
            get  
            {  
                return this.RULE_DESCValue;  
            }  

          set { SetProperty(ref  RULE_DESCValue, value); }
        } 
private  System.String RULE_PURPOSEValue; 
 public System.String RULE_PURPOSE
        {  
            get  
            {  
                return this.RULE_PURPOSEValue;  
            }  

          set { SetProperty(ref  RULE_PURPOSEValue, value); }
        } 
private  System.String RULE_QUERYValue; 
 public System.String RULE_QUERY
        {  
            get  
            {  
                return this.RULE_QUERYValue;  
            }  

          set { SetProperty(ref  RULE_QUERYValue, value); }
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
private  System.String USE_CONDITION_DESCValue; 
 public System.String USE_CONDITION_DESC
        {  
            get  
            {  
                return this.USE_CONDITION_DESCValue;  
            }  

          set { SetProperty(ref  USE_CONDITION_DESCValue, value); }
        } 
private  System.String USE_CONDITION_TYPEValue; 
 public System.String USE_CONDITION_TYPE
        {  
            get  
            {  
                return this.USE_CONDITION_TYPEValue;  
            }  

          set { SetProperty(ref  USE_CONDITION_TYPEValue, value); }
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


    public PPDM_RULE () { }

  }
}


using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class PDEN_DECLINE_CONDITION: Entity,IPPDMEntity

{

private  System.String PDEN_IDValue; 
 public System.String PDEN_ID
        {  
            get  
            {  
                return this.PDEN_IDValue;  
            }  

          set { SetProperty(ref  PDEN_IDValue, value); }
        } 
private  System.String PDEN_SUBTYPEValue; 
 public System.String PDEN_SUBTYPE
        {  
            get  
            {  
                return this.PDEN_SUBTYPEValue;  
            }  

          set { SetProperty(ref  PDEN_SUBTYPEValue, value); }
        } 
private  System.String PDEN_SOURCEValue; 
 public System.String PDEN_SOURCE
        {  
            get  
            {  
                return this.PDEN_SOURCEValue;  
            }  

          set { SetProperty(ref  PDEN_SOURCEValue, value); }
        } 
private  System.String PRODUCT_TYPEValue; 
 public System.String PRODUCT_TYPE
        {  
            get  
            {  
                return this.PRODUCT_TYPEValue;  
            }  

          set { SetProperty(ref  PRODUCT_TYPEValue, value); }
        } 
private  System.String CASE_IDValue; 
 public System.String CASE_ID
        {  
            get  
            {  
                return this.CASE_IDValue;  
            }  

          set { SetProperty(ref  CASE_IDValue, value); }
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
private  System.String PERIOD_TYPEValue; 
 public System.String PERIOD_TYPE
        {  
            get  
            {  
                return this.PERIOD_TYPEValue;  
            }  

          set { SetProperty(ref  PERIOD_TYPEValue, value); }
        } 
private  System.String PERIOD_IDValue; 
 public System.String PERIOD_ID
        {  
            get  
            {  
                return this.PERIOD_IDValue;  
            }  

          set { SetProperty(ref  PERIOD_IDValue, value); }
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
private  System.String CONDITION_CODEValue; 
 public System.String CONDITION_CODE
        {  
            get  
            {  
                return this.CONDITION_CODEValue;  
            }  

          set { SetProperty(ref  CONDITION_CODEValue, value); }
        } 
private  System.String CONDITION_DESCValue; 
 public System.String CONDITION_DESC
        {  
            get  
            {  
                return this.CONDITION_DESCValue;  
            }  

          set { SetProperty(ref  CONDITION_DESCValue, value); }
        } 
private  System.String CONDITION_TYPEValue; 
 public System.String CONDITION_TYPE
        {  
            get  
            {  
                return this.CONDITION_TYPEValue;  
            }  

          set { SetProperty(ref  CONDITION_TYPEValue, value); }
        } 
private  System.Decimal CONDITION_VALUEValue; 
 public System.Decimal CONDITION_VALUE
        {  
            get  
            {  
                return this.CONDITION_VALUEValue;  
            }  

          set { SetProperty(ref  CONDITION_VALUEValue, value); }
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
private  System.String SOURCEValue; 
 public System.String SOURCE
        {  
            get  
            {  
                return this.SOURCEValue;  
            }  

          set { SetProperty(ref  SOURCEValue, value); }
        } 
private  System.DateTime? VOLUME_DATEValue; 
 public System.DateTime? VOLUME_DATE
        {  
            get  
            {  
                return this.VOLUME_DATEValue;  
            }  

          set { SetProperty(ref  VOLUME_DATEValue, value); }
        } 
private  System.String VOLUME_DATE_DESCValue; 
 public System.String VOLUME_DATE_DESC
        {  
            get  
            {  
                return this.VOLUME_DATE_DESCValue;  
            }  

          set { SetProperty(ref  VOLUME_DATE_DESCValue, value); }
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


    public PDEN_DECLINE_CONDITION () { }

  }
}


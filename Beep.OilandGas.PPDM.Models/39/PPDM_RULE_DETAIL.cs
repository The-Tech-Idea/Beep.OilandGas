using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class PPDM_RULE_DETAIL: Entity,IPPDMEntity

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
private  System.Decimal DETAIL_SEQ_NOValue; 
 public System.Decimal DETAIL_SEQ_NO
        {  
            get  
            {  
                return this.DETAIL_SEQ_NOValue;  
            }  

          set { SetProperty(ref  DETAIL_SEQ_NOValue, value); }
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
private  System.Decimal AVERAGE_VALUEValue; 
 public System.Decimal AVERAGE_VALUE
        {  
            get  
            {  
                return this.AVERAGE_VALUEValue;  
            }  

          set { SetProperty(ref  AVERAGE_VALUEValue, value); }
        } 
private  System.String AVERAGE_VALUE_OUOMValue; 
 public System.String AVERAGE_VALUE_OUOM
        {  
            get  
            {  
                return this.AVERAGE_VALUE_OUOMValue;  
            }  

          set { SetProperty(ref  AVERAGE_VALUE_OUOMValue, value); }
        } 
private  System.String AVERAGE_VALUE_UOMValue; 
 public System.String AVERAGE_VALUE_UOM
        {  
            get  
            {  
                return this.AVERAGE_VALUE_UOMValue;  
            }  

          set { SetProperty(ref  AVERAGE_VALUE_UOMValue, value); }
        } 
private  System.String BOOLEAN_RULEValue; 
 public System.String BOOLEAN_RULE
        {  
            get  
            {  
                return this.BOOLEAN_RULEValue;  
            }  

          set { SetProperty(ref  BOOLEAN_RULEValue, value); }
        } 
private  System.String BUSINESS_RULEValue; 
 public System.String BUSINESS_RULE
        {  
            get  
            {  
                return this.BUSINESS_RULEValue;  
            }  

          set { SetProperty(ref  BUSINESS_RULEValue, value); }
        } 
private  System.String DATE_FORMAT_DESCValue; 
 public System.String DATE_FORMAT_DESC
        {  
            get  
            {  
                return this.DATE_FORMAT_DESCValue;  
            }  

          set { SetProperty(ref  DATE_FORMAT_DESCValue, value); }
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
private  System.DateTime? MAX_DATEValue; 
 public System.DateTime? MAX_DATE
        {  
            get  
            {  
                return this.MAX_DATEValue;  
            }  

          set { SetProperty(ref  MAX_DATEValue, value); }
        } 
private  System.Decimal MAX_VALUEValue; 
 public System.Decimal MAX_VALUE
        {  
            get  
            {  
                return this.MAX_VALUEValue;  
            }  

          set { SetProperty(ref  MAX_VALUEValue, value); }
        } 
private  System.String MAX_VALUE_OUOMValue; 
 public System.String MAX_VALUE_OUOM
        {  
            get  
            {  
                return this.MAX_VALUE_OUOMValue;  
            }  

          set { SetProperty(ref  MAX_VALUE_OUOMValue, value); }
        } 
private  System.String MAX_VALUE_UOMValue; 
 public System.String MAX_VALUE_UOM
        {  
            get  
            {  
                return this.MAX_VALUE_UOMValue;  
            }  

          set { SetProperty(ref  MAX_VALUE_UOMValue, value); }
        } 
private  System.DateTime? MIN_DATEValue; 
 public System.DateTime? MIN_DATE
        {  
            get  
            {  
                return this.MIN_DATEValue;  
            }  

          set { SetProperty(ref  MIN_DATEValue, value); }
        } 
private  System.Decimal MIN_VALUEValue; 
 public System.Decimal MIN_VALUE
        {  
            get  
            {  
                return this.MIN_VALUEValue;  
            }  

          set { SetProperty(ref  MIN_VALUEValue, value); }
        } 
private  System.String MIN_VALUE_OUOMValue; 
 public System.String MIN_VALUE_OUOM
        {  
            get  
            {  
                return this.MIN_VALUE_OUOMValue;  
            }  

          set { SetProperty(ref  MIN_VALUE_OUOMValue, value); }
        } 
private  System.String MIN_VALUE_UOMValue; 
 public System.String MIN_VALUE_UOM
        {  
            get  
            {  
                return this.MIN_VALUE_UOMValue;  
            }  

          set { SetProperty(ref  MIN_VALUE_UOMValue, value); }
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
private  System.String REFERENCE_COLUMN_NAME2Value; 
 public System.String REFERENCE_COLUMN_NAME2
        {  
            get  
            {  
                return this.REFERENCE_COLUMN_NAME2Value;  
            }  

          set { SetProperty(ref  REFERENCE_COLUMN_NAME2Value, value); }
        } 
private  System.String REFERENCE_SYSTEM_IDValue; 
 public System.String REFERENCE_SYSTEM_ID
        {  
            get  
            {  
                return this.REFERENCE_SYSTEM_IDValue;  
            }  

          set { SetProperty(ref  REFERENCE_SYSTEM_IDValue, value); }
        } 
private  System.String REFERENCE_TABLE_NAMEValue; 
 public System.String REFERENCE_TABLE_NAME
        {  
            get  
            {  
                return this.REFERENCE_TABLE_NAMEValue;  
            }  

          set { SetProperty(ref  REFERENCE_TABLE_NAMEValue, value); }
        } 
private  System.String REFERENCE_TABLE_NAME2Value; 
 public System.String REFERENCE_TABLE_NAME2
        {  
            get  
            {  
                return this.REFERENCE_TABLE_NAME2Value;  
            }  

          set { SetProperty(ref  REFERENCE_TABLE_NAME2Value, value); }
        } 
private  System.Decimal REFERENCE_VALUEValue; 
 public System.Decimal REFERENCE_VALUE
        {  
            get  
            {  
                return this.REFERENCE_VALUEValue;  
            }  

          set { SetProperty(ref  REFERENCE_VALUEValue, value); }
        } 
private  System.String REFERENCE_VALUE_OUOMValue; 
 public System.String REFERENCE_VALUE_OUOM
        {  
            get  
            {  
                return this.REFERENCE_VALUE_OUOMValue;  
            }  

          set { SetProperty(ref  REFERENCE_VALUE_OUOMValue, value); }
        } 
private  System.String REFERENCE_VALUE_TYPEValue; 
 public System.String REFERENCE_VALUE_TYPE
        {  
            get  
            {  
                return this.REFERENCE_VALUE_TYPEValue;  
            }  

          set { SetProperty(ref  REFERENCE_VALUE_TYPEValue, value); }
        } 
private  System.String REFERENCE_VALUE_UOMValue; 
 public System.String REFERENCE_VALUE_UOM
        {  
            get  
            {  
                return this.REFERENCE_VALUE_UOMValue;  
            }  

          set { SetProperty(ref  REFERENCE_VALUE_UOMValue, value); }
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
private  System.String RULE_DESCValue; 
 public System.String RULE_DESC
        {  
            get  
            {  
                return this.RULE_DESCValue;  
            }  

          set { SetProperty(ref  RULE_DESCValue, value); }
        } 
private  System.String RULE_DETAIL_TYPEValue; 
 public System.String RULE_DETAIL_TYPE
        {  
            get  
            {  
                return this.RULE_DETAIL_TYPEValue;  
            }  

          set { SetProperty(ref  RULE_DETAIL_TYPEValue, value); }
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


    public PPDM_RULE_DETAIL () { }

  }
}


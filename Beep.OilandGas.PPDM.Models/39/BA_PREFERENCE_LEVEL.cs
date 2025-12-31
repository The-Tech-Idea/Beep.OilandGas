using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class BA_PREFERENCE_LEVEL: Entity,IPPDMEntity

{

private  System.String BUSINESS_ASSOCIATE_IDValue; 
 public System.String BUSINESS_ASSOCIATE_ID
        {  
            get  
            {  
                return this.BUSINESS_ASSOCIATE_IDValue;  
            }  

          set { SetProperty(ref  BUSINESS_ASSOCIATE_IDValue, value); }
        } 
private  System.String PREFERENCE_TYPEValue; 
 public System.String PREFERENCE_TYPE
        {  
            get  
            {  
                return this.PREFERENCE_TYPEValue;  
            }  

          set { SetProperty(ref  PREFERENCE_TYPEValue, value); }
        } 
private  System.Decimal PREFERENCE_OBS_NOValue; 
 public System.Decimal PREFERENCE_OBS_NO
        {  
            get  
            {  
                return this.PREFERENCE_OBS_NOValue;  
            }  

          set { SetProperty(ref  PREFERENCE_OBS_NOValue, value); }
        } 
private  System.String LEVEL_IDValue; 
 public System.String LEVEL_ID
        {  
            get  
            {  
                return this.LEVEL_IDValue;  
            }  

          set { SetProperty(ref  LEVEL_IDValue, value); }
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
private  System.String CURRENCY_UOMValue; 
 public System.String CURRENCY_UOM
        {  
            get  
            {  
                return this.CURRENCY_UOMValue;  
            }  

          set { SetProperty(ref  CURRENCY_UOMValue, value); }
        } 
private  System.String DESCRIPTIONValue; 
 public System.String DESCRIPTION
        {  
            get  
            {  
                return this.DESCRIPTIONValue;  
            }  

          set { SetProperty(ref  DESCRIPTIONValue, value); }
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
private  System.Decimal PREFERENCE_LEVELValue; 
 public System.Decimal PREFERENCE_LEVEL
        {  
            get  
            {  
                return this.PREFERENCE_LEVELValue;  
            }  

          set { SetProperty(ref  PREFERENCE_LEVELValue, value); }
        } 
private  System.String PREFERENCE_RULEValue; 
 public System.String PREFERENCE_RULE
        {  
            get  
            {  
                return this.PREFERENCE_RULEValue;  
            }  

          set { SetProperty(ref  PREFERENCE_RULEValue, value); }
        } 
private  System.String PREFERRED_BA_IDValue; 
 public System.String PREFERRED_BA_ID
        {  
            get  
            {  
                return this.PREFERRED_BA_IDValue;  
            }  

          set { SetProperty(ref  PREFERRED_BA_IDValue, value); }
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
private  System.String WL_DICTIONARY_IDValue; 
 public System.String WL_DICTIONARY_ID
        {  
            get  
            {  
                return this.WL_DICTIONARY_IDValue;  
            }  

          set { SetProperty(ref  WL_DICTIONARY_IDValue, value); }
        } 
private  System.String WL_DICT_CURVE_IDValue; 
 public System.String WL_DICT_CURVE_ID
        {  
            get  
            {  
                return this.WL_DICT_CURVE_IDValue;  
            }  

          set { SetProperty(ref  WL_DICT_CURVE_IDValue, value); }
        } 
private  System.String WL_PARAMETER_IDValue; 
 public System.String WL_PARAMETER_ID
        {  
            get  
            {  
                return this.WL_PARAMETER_IDValue;  
            }  

          set { SetProperty(ref  WL_PARAMETER_IDValue, value); }
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


    public BA_PREFERENCE_LEVEL () { }

  }
}


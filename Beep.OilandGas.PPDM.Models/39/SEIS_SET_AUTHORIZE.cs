using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class SEIS_SET_AUTHORIZE: Entity,IPPDMEntity

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
private  System.String AUTHORIZE_IDValue; 
 public System.String AUTHORIZE_ID
        {  
            get  
            {  
                return this.AUTHORIZE_IDValue;  
            }  

          set { SetProperty(ref  AUTHORIZE_IDValue, value); }
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
private  System.String AUTHORIZED_BYValue; 
 public System.String AUTHORIZED_BY
        {  
            get  
            {  
                return this.AUTHORIZED_BYValue;  
            }  

          set { SetProperty(ref  AUTHORIZED_BYValue, value); }
        } 
private  System.String AUTHORIZE_TYPEValue; 
 public System.String AUTHORIZE_TYPE
        {  
            get  
            {  
                return this.AUTHORIZE_TYPEValue;  
            }  

          set { SetProperty(ref  AUTHORIZE_TYPEValue, value); }
        } 
private  System.Decimal CURRENCY_CONVERSIONValue; 
 public System.Decimal CURRENCY_CONVERSION
        {  
            get  
            {  
                return this.CURRENCY_CONVERSIONValue;  
            }  

          set { SetProperty(ref  CURRENCY_CONVERSIONValue, value); }
        } 
private  System.String CURRENCY_OUOMValue; 
 public System.String CURRENCY_OUOM
        {  
            get  
            {  
                return this.CURRENCY_OUOMValue;  
            }  

          set { SetProperty(ref  CURRENCY_OUOMValue, value); }
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
private  System.String LIMIT_DESCValue; 
 public System.String LIMIT_DESC
        {  
            get  
            {  
                return this.LIMIT_DESCValue;  
            }  

          set { SetProperty(ref  LIMIT_DESCValue, value); }
        } 
private  System.String LIMIT_TYPEValue; 
 public System.String LIMIT_TYPE
        {  
            get  
            {  
                return this.LIMIT_TYPEValue;  
            }  

          set { SetProperty(ref  LIMIT_TYPEValue, value); }
        } 
private  System.String PARTNER_BA_IDValue; 
 public System.String PARTNER_BA_ID
        {  
            get  
            {  
                return this.PARTNER_BA_IDValue;  
            }  

          set { SetProperty(ref  PARTNER_BA_IDValue, value); }
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
private  System.Decimal PRICE_PER_LENGTHValue; 
 public System.Decimal PRICE_PER_LENGTH
        {  
            get  
            {  
                return this.PRICE_PER_LENGTHValue;  
            }  

          set { SetProperty(ref  PRICE_PER_LENGTHValue, value); }
        } 
private  System.String REASON_TYPEValue; 
 public System.String REASON_TYPE
        {  
            get  
            {  
                return this.REASON_TYPEValue;  
            }  

          set { SetProperty(ref  REASON_TYPEValue, value); }
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


    public SEIS_SET_AUTHORIZE () { }

  }
}


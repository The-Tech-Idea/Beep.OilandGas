using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.PPDM39.Models 
{
public partial class SEIS_TRANSACTION: Entity

{

private  System.String SEIS_TRANSACTION_IDValue; 
 public System.String SEIS_TRANSACTION_ID
        {  
            get  
            {  
                return this.SEIS_TRANSACTION_IDValue;  
            }  

          set { SetProperty(ref  SEIS_TRANSACTION_IDValue, value); }
        } 
private  System.String TRANSACTION_TYPEValue; 
 public System.String TRANSACTION_TYPE
        {  
            get  
            {  
                return this.TRANSACTION_TYPEValue;  
            }  

          set { SetProperty(ref  TRANSACTION_TYPEValue, value); }
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
private  System.Decimal LENGTHValue; 
 public System.Decimal LENGTH
        {  
            get  
            {  
                return this.LENGTHValue;  
            }  

          set { SetProperty(ref  LENGTHValue, value); }
        } 
private  System.String LENGTH_OUOMValue; 
 public System.String LENGTH_OUOM
        {  
            get  
            {  
                return this.LENGTH_OUOMValue;  
            }  

          set { SetProperty(ref  LENGTH_OUOMValue, value); }
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
private  System.String REFERENCE_NUMValue; 
 public System.String REFERENCE_NUM
        {  
            get  
            {  
                return this.REFERENCE_NUMValue;  
            }  

          set { SetProperty(ref  REFERENCE_NUMValue, value); }
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
private  System.Decimal TOTAL_PRICEValue; 
 public System.Decimal TOTAL_PRICE
        {  
            get  
            {  
                return this.TOTAL_PRICEValue;  
            }  

          set { SetProperty(ref  TOTAL_PRICEValue, value); }
        } 
private  System.DateTime TRANSACTION_DATEValue; 
 public System.DateTime TRANSACTION_DATE
        {  
            get  
            {  
                return this.TRANSACTION_DATEValue;  
            }  

          set { SetProperty(ref  TRANSACTION_DATEValue, value); }
        } 
private  System.String TRANSACTION_STATUSValue; 
 public System.String TRANSACTION_STATUS
        {  
            get  
            {  
                return this.TRANSACTION_STATUSValue;  
            }  

          set { SetProperty(ref  TRANSACTION_STATUSValue, value); }
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


    public SEIS_TRANSACTION () { }

  }
}


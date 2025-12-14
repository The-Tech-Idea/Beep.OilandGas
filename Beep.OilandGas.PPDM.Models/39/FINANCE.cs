using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class FINANCE: Entity

{

private  System.String FINANCE_IDValue; 
 public System.String FINANCE_ID
        {  
            get  
            {  
                return this.FINANCE_IDValue;  
            }  

          set { SetProperty(ref  FINANCE_IDValue, value); }
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
private  System.Decimal ACTUAL_COSTValue; 
 public System.Decimal ACTUAL_COST
        {  
            get  
            {  
                return this.ACTUAL_COSTValue;  
            }  

          set { SetProperty(ref  ACTUAL_COSTValue, value); }
        } 
private  System.String AUTHORIZED_BY_BA_IDValue; 
 public System.String AUTHORIZED_BY_BA_ID
        {  
            get  
            {  
                return this.AUTHORIZED_BY_BA_IDValue;  
            }  

          set { SetProperty(ref  AUTHORIZED_BY_BA_IDValue, value); }
        } 
private  System.Decimal BUDGET_COSTValue; 
 public System.Decimal BUDGET_COST
        {  
            get  
            {  
                return this.BUDGET_COSTValue;  
            }  

          set { SetProperty(ref  BUDGET_COSTValue, value); }
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
private  System.String FINANCE_TYPEValue; 
 public System.String FINANCE_TYPE
        {  
            get  
            {  
                return this.FINANCE_TYPEValue;  
            }  

          set { SetProperty(ref  FINANCE_TYPEValue, value); }
        } 
private  System.String FIN_STATUSValue; 
 public System.String FIN_STATUS
        {  
            get  
            {  
                return this.FIN_STATUSValue;  
            }  

          set { SetProperty(ref  FIN_STATUSValue, value); }
        } 
private  System.DateTime ISSUE_DATEValue; 
 public System.DateTime ISSUE_DATE
        {  
            get  
            {  
                return this.ISSUE_DATEValue;  
            }  

          set { SetProperty(ref  ISSUE_DATEValue, value); }
        } 
private  System.Decimal LIMIT_AMOUNTValue; 
 public System.Decimal LIMIT_AMOUNT
        {  
            get  
            {  
                return this.LIMIT_AMOUNTValue;  
            }  

          set { SetProperty(ref  LIMIT_AMOUNTValue, value); }
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
private  System.String REFERENCE_NUMBERValue; 
 public System.String REFERENCE_NUMBER
        {  
            get  
            {  
                return this.REFERENCE_NUMBERValue;  
            }  

          set { SetProperty(ref  REFERENCE_NUMBERValue, value); }
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
private  System.String TAX_CREDIT_CODEValue; 
 public System.String TAX_CREDIT_CODE
        {  
            get  
            {  
                return this.TAX_CREDIT_CODEValue;  
            }  

          set { SetProperty(ref  TAX_CREDIT_CODEValue, value); }
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


    public FINANCE () { }

  }
}


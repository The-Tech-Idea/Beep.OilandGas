using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class OBLIG_PAYMENT_INSTR: Entity

{

private  System.String PAYEE_BA_IDValue; 
 public System.String PAYEE_BA_ID
        {  
            get  
            {  
                return this.PAYEE_BA_IDValue;  
            }  

          set { SetProperty(ref  PAYEE_BA_IDValue, value); }
        } 
private  System.String PAYMENT_INSTRUCTION_IDValue; 
 public System.String PAYMENT_INSTRUCTION_ID
        {  
            get  
            {  
                return this.PAYMENT_INSTRUCTION_IDValue;  
            }  

          set { SetProperty(ref  PAYMENT_INSTRUCTION_IDValue, value); }
        } 
private  System.String ABA_NUMBERValue; 
 public System.String ABA_NUMBER
        {  
            get  
            {  
                return this.ABA_NUMBERValue;  
            }  

          set { SetProperty(ref  ABA_NUMBERValue, value); }
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
private  System.Decimal BANK_ADDRESS_OBS_NOValue; 
 public System.Decimal BANK_ADDRESS_OBS_NO
        {  
            get  
            {  
                return this.BANK_ADDRESS_OBS_NOValue;  
            }  

          set { SetProperty(ref  BANK_ADDRESS_OBS_NOValue, value); }
        } 
private  System.String BANK_ADDRESS_SOURCEValue; 
 public System.String BANK_ADDRESS_SOURCE
        {  
            get  
            {  
                return this.BANK_ADDRESS_SOURCEValue;  
            }  

          set { SetProperty(ref  BANK_ADDRESS_SOURCEValue, value); }
        } 
private  System.String BANK_BA_IDValue; 
 public System.String BANK_BA_ID
        {  
            get  
            {  
                return this.BANK_BA_IDValue;  
            }  

          set { SetProperty(ref  BANK_BA_IDValue, value); }
        } 
private  System.Decimal BANK_FEEValue; 
 public System.Decimal BANK_FEE
        {  
            get  
            {  
                return this.BANK_FEEValue;  
            }  

          set { SetProperty(ref  BANK_FEEValue, value); }
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
private  System.String DEPOSITORY_NUMValue; 
 public System.String DEPOSITORY_NUM
        {  
            get  
            {  
                return this.DEPOSITORY_NUMValue;  
            }  

          set { SetProperty(ref  DEPOSITORY_NUMValue, value); }
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
private  System.DateTime INVALID_DATEValue; 
 public System.DateTime INVALID_DATE
        {  
            get  
            {  
                return this.INVALID_DATEValue;  
            }  

          set { SetProperty(ref  INVALID_DATEValue, value); }
        } 
private  System.String PAY_METHODValue; 
 public System.String PAY_METHOD
        {  
            get  
            {  
                return this.PAY_METHODValue;  
            }  

          set { SetProperty(ref  PAY_METHODValue, value); }
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
private  System.String SUSPEND_PAYMENT_INDValue; 
 public System.String SUSPEND_PAYMENT_IND
        {  
            get  
            {  
                return this.SUSPEND_PAYMENT_INDValue;  
            }  

          set { SetProperty(ref  SUSPEND_PAYMENT_INDValue, value); }
        } 
private  System.String SUSPEND_PAYMENT_REASONValue; 
 public System.String SUSPEND_PAYMENT_REASON
        {  
            get  
            {  
                return this.SUSPEND_PAYMENT_REASONValue;  
            }  

          set { SetProperty(ref  SUSPEND_PAYMENT_REASONValue, value); }
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


    public OBLIG_PAYMENT_INSTR () { }

  }
}


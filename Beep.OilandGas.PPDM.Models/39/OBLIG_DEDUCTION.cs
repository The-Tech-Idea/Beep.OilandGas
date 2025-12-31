using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class OBLIG_DEDUCTION: Entity,IPPDMEntity

{

private  System.String OBLIGATION_IDValue; 
 public System.String OBLIGATION_ID
        {  
            get  
            {  
                return this.OBLIGATION_IDValue;  
            }  

          set { SetProperty(ref  OBLIGATION_IDValue, value); }
        } 
private  System.Decimal OBLIGATION_SEQ_NOValue; 
 public System.Decimal OBLIGATION_SEQ_NO
        {  
            get  
            {  
                return this.OBLIGATION_SEQ_NOValue;  
            }  

          set { SetProperty(ref  OBLIGATION_SEQ_NOValue, value); }
        } 
private  System.String DEDUCTION_IDValue; 
 public System.String DEDUCTION_ID
        {  
            get  
            {  
                return this.DEDUCTION_IDValue;  
            }  

          set { SetProperty(ref  DEDUCTION_IDValue, value); }
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
private  System.String ALLOW_DEDUCTION_IDValue; 
 public System.String ALLOW_DEDUCTION_ID
        {  
            get  
            {  
                return this.ALLOW_DEDUCTION_IDValue;  
            }  

          set { SetProperty(ref  ALLOW_DEDUCTION_IDValue, value); }
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
private  System.Decimal DEDUCTION_AMOUNTValue; 
 public System.Decimal DEDUCTION_AMOUNT
        {  
            get  
            {  
                return this.DEDUCTION_AMOUNTValue;  
            }  

          set { SetProperty(ref  DEDUCTION_AMOUNTValue, value); }
        } 
private  System.Decimal DEDUCTION_PERCENTValue; 
 public System.Decimal DEDUCTION_PERCENT
        {  
            get  
            {  
                return this.DEDUCTION_PERCENTValue;  
            }  

          set { SetProperty(ref  DEDUCTION_PERCENTValue, value); }
        } 
private  System.String DEDUCT_TYPEValue; 
 public System.String DEDUCT_TYPE
        {  
            get  
            {  
                return this.DEDUCT_TYPEValue;  
            }  

          set { SetProperty(ref  DEDUCT_TYPEValue, value); }
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
private  System.Decimal MAX_DEDUCTION_ALLOWEDValue; 
 public System.Decimal MAX_DEDUCTION_ALLOWED
        {  
            get  
            {  
                return this.MAX_DEDUCTION_ALLOWEDValue;  
            }  

          set { SetProperty(ref  MAX_DEDUCTION_ALLOWEDValue, value); }
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
private  System.Decimal ROYALTY_AMOUNTValue; 
 public System.Decimal ROYALTY_AMOUNT
        {  
            get  
            {  
                return this.ROYALTY_AMOUNTValue;  
            }  

          set { SetProperty(ref  ROYALTY_AMOUNTValue, value); }
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


    public OBLIG_DEDUCTION () { }

  }
}


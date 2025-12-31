using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class FIN_COST_SUMMARY: Entity,IPPDMEntity

{

private  System.String COST_IDValue; 
 public System.String COST_ID
        {  
            get  
            {  
                return this.COST_IDValue;  
            }  

          set { SetProperty(ref  COST_IDValue, value); }
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
private  System.String AMI_INDValue; 
 public System.String AMI_IND
        {  
            get  
            {  
                return this.AMI_INDValue;  
            }  

          set { SetProperty(ref  AMI_INDValue, value); }
        } 
private  System.String CONFIDENTIAL_INDValue; 
 public System.String CONFIDENTIAL_IND
        {  
            get  
            {  
                return this.CONFIDENTIAL_INDValue;  
            }  

          set { SetProperty(ref  CONFIDENTIAL_INDValue, value); }
        } 
private  System.Decimal COST_AMOUNTValue; 
 public System.Decimal COST_AMOUNT
        {  
            get  
            {  
                return this.COST_AMOUNTValue;  
            }  

          set { SetProperty(ref  COST_AMOUNTValue, value); }
        } 
private  System.Decimal COST_PER_SIZEValue; 
 public System.Decimal COST_PER_SIZE
        {  
            get  
            {  
                return this.COST_PER_SIZEValue;  
            }  

          set { SetProperty(ref  COST_PER_SIZEValue, value); }
        } 
private  System.String COST_PER_SIZE_OUOMValue; 
 public System.String COST_PER_SIZE_OUOM
        {  
            get  
            {  
                return this.COST_PER_SIZE_OUOMValue;  
            }  

          set { SetProperty(ref  COST_PER_SIZE_OUOMValue, value); }
        } 
private  System.String COST_TYPEValue; 
 public System.String COST_TYPE
        {  
            get  
            {  
                return this.COST_TYPEValue;  
            }  

          set { SetProperty(ref  COST_TYPEValue, value); }
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
private  System.String FINANCE_COMPONENT_IDValue; 
 public System.String FINANCE_COMPONENT_ID
        {  
            get  
            {  
                return this.FINANCE_COMPONENT_IDValue;  
            }  

          set { SetProperty(ref  FINANCE_COMPONENT_IDValue, value); }
        } 
private  System.String FINANCE_IDValue; 
 public System.String FINANCE_ID
        {  
            get  
            {  
                return this.FINANCE_IDValue;  
            }  

          set { SetProperty(ref  FINANCE_IDValue, value); }
        } 
private  System.DateTime? PAID_DATEValue; 
 public System.DateTime? PAID_DATE
        {  
            get  
            {  
                return this.PAID_DATEValue;  
            }  

          set { SetProperty(ref  PAID_DATEValue, value); }
        } 
private  System.String PARENT_COST_IDValue; 
 public System.String PARENT_COST_ID
        {  
            get  
            {  
                return this.PARENT_COST_IDValue;  
            }  

          set { SetProperty(ref  PARENT_COST_IDValue, value); }
        } 
private  System.Decimal PERCENT_OF_PARENTValue; 
 public System.Decimal PERCENT_OF_PARENT
        {  
            get  
            {  
                return this.PERCENT_OF_PARENTValue;  
            }  

          set { SetProperty(ref  PERCENT_OF_PARENTValue, value); }
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
private  System.String REPORT_COST_INDValue; 
 public System.String REPORT_COST_IND
        {  
            get  
            {  
                return this.REPORT_COST_INDValue;  
            }  

          set { SetProperty(ref  REPORT_COST_INDValue, value); }
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
private  System.DateTime? SUBMIT_DATEValue; 
 public System.DateTime? SUBMIT_DATE
        {  
            get  
            {  
                return this.SUBMIT_DATEValue;  
            }  

          set { SetProperty(ref  SUBMIT_DATEValue, value); }
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


    public FIN_COST_SUMMARY () { }

  }
}


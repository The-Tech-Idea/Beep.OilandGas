using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class LAND_SALE_OFFERING: Entity,IPPDMEntity

{

private  System.String JURISDICTIONValue; 
 public System.String JURISDICTION
        {  
            get  
            {  
                return this.JURISDICTIONValue;  
            }  

          set { SetProperty(ref  JURISDICTIONValue, value); }
        } 
private  System.String LAND_SALE_NUMBERValue; 
 public System.String LAND_SALE_NUMBER
        {  
            get  
            {  
                return this.LAND_SALE_NUMBERValue;  
            }  

          set { SetProperty(ref  LAND_SALE_NUMBERValue, value); }
        } 
private  System.String LAND_SALE_OFFERING_IDValue; 
 public System.String LAND_SALE_OFFERING_ID
        {  
            get  
            {  
                return this.LAND_SALE_OFFERING_IDValue;  
            }  

          set { SetProperty(ref  LAND_SALE_OFFERING_IDValue, value); }
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
private  System.String CANCEL_REASONValue; 
 public System.String CANCEL_REASON
        {  
            get  
            {  
                return this.CANCEL_REASONValue;  
            }  

          set { SetProperty(ref  CANCEL_REASONValue, value); }
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
private  System.Decimal GROSS_SIZEValue; 
 public System.Decimal GROSS_SIZE
        {  
            get  
            {  
                return this.GROSS_SIZEValue;  
            }  

          set { SetProperty(ref  GROSS_SIZEValue, value); }
        } 
private  System.String GROSS_SIZE_OUOMValue; 
 public System.String GROSS_SIZE_OUOM
        {  
            get  
            {  
                return this.GROSS_SIZE_OUOMValue;  
            }  

          set { SetProperty(ref  GROSS_SIZE_OUOMValue, value); }
        } 
private  System.DateTime? INACTIVATION_DATEValue; 
 public System.DateTime? INACTIVATION_DATE
        {  
            get  
            {  
                return this.INACTIVATION_DATEValue;  
            }  

          set { SetProperty(ref  INACTIVATION_DATEValue, value); }
        } 
private  System.String LAND_OFFERING_STATUSValue; 
 public System.String LAND_OFFERING_STATUS
        {  
            get  
            {  
                return this.LAND_OFFERING_STATUSValue;  
            }  

          set { SetProperty(ref  LAND_OFFERING_STATUSValue, value); }
        } 
private  System.String LAND_OFFERING_TYPEValue; 
 public System.String LAND_OFFERING_TYPE
        {  
            get  
            {  
                return this.LAND_OFFERING_TYPEValue;  
            }  

          set { SetProperty(ref  LAND_OFFERING_TYPEValue, value); }
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
private  System.DateTime? STATUS_DATEValue; 
 public System.DateTime? STATUS_DATE
        {  
            get  
            {  
                return this.STATUS_DATEValue;  
            }  

          set { SetProperty(ref  STATUS_DATEValue, value); }
        } 
private  System.Decimal TERM_DURATIONValue; 
 public System.Decimal TERM_DURATION
        {  
            get  
            {  
                return this.TERM_DURATIONValue;  
            }  

          set { SetProperty(ref  TERM_DURATIONValue, value); }
        } 
private  System.String TERM_DURATION_OUOMValue; 
 public System.String TERM_DURATION_OUOM
        {  
            get  
            {  
                return this.TERM_DURATION_OUOMValue;  
            }  

          set { SetProperty(ref  TERM_DURATION_OUOMValue, value); }
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


    public LAND_SALE_OFFERING () { }

  }
}


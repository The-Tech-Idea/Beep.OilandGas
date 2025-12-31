using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class LAND_SALE: Entity,IPPDMEntity

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
private  System.String ACTIVE_INDValue; 
 public System.String ACTIVE_IND
        {  
            get  
            {  
                return this.ACTIVE_INDValue;  
            }  

          set { SetProperty(ref  ACTIVE_INDValue, value); }
        } 
private  System.DateTime? CLOSE_DATEValue; 
 public System.DateTime? CLOSE_DATE
        {  
            get  
            {  
                return this.CLOSE_DATEValue;  
            }  

          set { SetProperty(ref  CLOSE_DATEValue, value); }
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
private  System.String LAND_SALE_NAMEValue; 
 public System.String LAND_SALE_NAME
        {  
            get  
            {  
                return this.LAND_SALE_NAMEValue;  
            }  

          set { SetProperty(ref  LAND_SALE_NAMEValue, value); }
        } 
private  System.DateTime? OPEN_DATEValue; 
 public System.DateTime? OPEN_DATE
        {  
            get  
            {  
                return this.OPEN_DATEValue;  
            }  

          set { SetProperty(ref  OPEN_DATEValue, value); }
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
private  System.DateTime? PUBLISH_DATEValue; 
 public System.DateTime? PUBLISH_DATE
        {  
            get  
            {  
                return this.PUBLISH_DATEValue;  
            }  

          set { SetProperty(ref  PUBLISH_DATEValue, value); }
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
private  System.DateTime? SALE_DATEValue; 
 public System.DateTime? SALE_DATE
        {  
            get  
            {  
                return this.SALE_DATEValue;  
            }  

          set { SetProperty(ref  SALE_DATEValue, value); }
        } 
private  System.Decimal SOLD_SIZEValue; 
 public System.Decimal SOLD_SIZE
        {  
            get  
            {  
                return this.SOLD_SIZEValue;  
            }  

          set { SetProperty(ref  SOLD_SIZEValue, value); }
        } 
private  System.String SOLD_SIZE_OUOMValue; 
 public System.String SOLD_SIZE_OUOM
        {  
            get  
            {  
                return this.SOLD_SIZE_OUOMValue;  
            }  

          set { SetProperty(ref  SOLD_SIZE_OUOMValue, value); }
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
private  System.Decimal TOTAL_BONUSValue; 
 public System.Decimal TOTAL_BONUS
        {  
            get  
            {  
                return this.TOTAL_BONUSValue;  
            }  

          set { SetProperty(ref  TOTAL_BONUSValue, value); }
        } 
private  System.String TOTAL_BONUS_OUOMValue; 
 public System.String TOTAL_BONUS_OUOM
        {  
            get  
            {  
                return this.TOTAL_BONUS_OUOMValue;  
            }  

          set { SetProperty(ref  TOTAL_BONUS_OUOMValue, value); }
        } 
private  System.Decimal TOTAL_SIZEValue; 
 public System.Decimal TOTAL_SIZE
        {  
            get  
            {  
                return this.TOTAL_SIZEValue;  
            }  

          set { SetProperty(ref  TOTAL_SIZEValue, value); }
        } 
private  System.String TOTAL_SIZE_OUOMValue; 
 public System.String TOTAL_SIZE_OUOM
        {  
            get  
            {  
                return this.TOTAL_SIZE_OUOMValue;  
            }  

          set { SetProperty(ref  TOTAL_SIZE_OUOMValue, value); }
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


    public LAND_SALE () { }

  }
}


using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class SF_RIG_SHAKER: Entity,IPPDMEntity

{

private  System.String SF_SUBTYPEValue; 
 public System.String SF_SUBTYPE
        {  
            get  
            {  
                return this.SF_SUBTYPEValue;  
            }  

          set { SetProperty(ref  SF_SUBTYPEValue, value); }
        } 
private  System.String RIG_IDValue; 
 public System.String RIG_ID
        {  
            get  
            {  
                return this.RIG_IDValue;  
            }  

          set { SetProperty(ref  RIG_IDValue, value); }
        } 
private  System.String SHAKER_IDValue; 
 public System.String SHAKER_ID
        {  
            get  
            {  
                return this.SHAKER_IDValue;  
            }  

          set { SetProperty(ref  SHAKER_IDValue, value); }
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
private  System.Decimal CAPACITYValue; 
 public System.Decimal CAPACITY
        {  
            get  
            {  
                return this.CAPACITYValue;  
            }  

          set { SetProperty(ref  CAPACITYValue, value); }
        } 
private  System.String CAPACITY_OUOMValue; 
 public System.String CAPACITY_OUOM
        {  
            get  
            {  
                return this.CAPACITY_OUOMValue;  
            }  

          set { SetProperty(ref  CAPACITY_OUOMValue, value); }
        } 
private  System.String CATALOGUE_EQUIP_IDValue; 
 public System.String CATALOGUE_EQUIP_ID
        {  
            get  
            {  
                return this.CATALOGUE_EQUIP_IDValue;  
            }  

          set { SetProperty(ref  CATALOGUE_EQUIP_IDValue, value); }
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
private  System.String EQUIPMENT_IDValue; 
 public System.String EQUIPMENT_ID
        {  
            get  
            {  
                return this.EQUIPMENT_IDValue;  
            }  

          set { SetProperty(ref  EQUIPMENT_IDValue, value); }
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
private  System.String INPUT_TYPEValue; 
 public System.String INPUT_TYPE
        {  
            get  
            {  
                return this.INPUT_TYPEValue;  
            }  

          set { SetProperty(ref  INPUT_TYPEValue, value); }
        } 
private  System.DateTime? INSTALL_DATEValue; 
 public System.DateTime? INSTALL_DATE
        {  
            get  
            {  
                return this.INSTALL_DATEValue;  
            }  

          set { SetProperty(ref  INSTALL_DATEValue, value); }
        } 
private  System.String POSITION_DESCValue; 
 public System.String POSITION_DESC
        {  
            get  
            {  
                return this.POSITION_DESCValue;  
            }  

          set { SetProperty(ref  POSITION_DESCValue, value); }
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
private  System.DateTime? REMOVE_DATEValue; 
 public System.DateTime? REMOVE_DATE
        {  
            get  
            {  
                return this.REMOVE_DATEValue;  
            }  

          set { SetProperty(ref  REMOVE_DATEValue, value); }
        } 
private  System.Decimal SHAKER_COUNTValue; 
 public System.Decimal SHAKER_COUNT
        {  
            get  
            {  
                return this.SHAKER_COUNTValue;  
            }  

          set { SetProperty(ref  SHAKER_COUNTValue, value); }
        } 
private  System.Decimal SHAKER_SIZEValue; 
 public System.Decimal SHAKER_SIZE
        {  
            get  
            {  
                return this.SHAKER_SIZEValue;  
            }  

          set { SetProperty(ref  SHAKER_SIZEValue, value); }
        } 
private  System.String SHAKER_SIZE_DESCValue; 
 public System.String SHAKER_SIZE_DESC
        {  
            get  
            {  
                return this.SHAKER_SIZE_DESCValue;  
            }  

          set { SetProperty(ref  SHAKER_SIZE_DESCValue, value); }
        } 
private  System.String SHAKER_SIZE_OUOMValue; 
 public System.String SHAKER_SIZE_OUOM
        {  
            get  
            {  
                return this.SHAKER_SIZE_OUOMValue;  
            }  

          set { SetProperty(ref  SHAKER_SIZE_OUOMValue, value); }
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


    public SF_RIG_SHAKER () { }

  }
}


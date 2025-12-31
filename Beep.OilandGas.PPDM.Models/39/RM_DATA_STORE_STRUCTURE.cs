using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class RM_DATA_STORE_STRUCTURE: Entity,IPPDMEntity

{

private  System.String STORE_IDValue; 
 public System.String STORE_ID
        {  
            get  
            {  
                return this.STORE_IDValue;  
            }  

          set { SetProperty(ref  STORE_IDValue, value); }
        } 
private  System.Decimal STRUCTURE_OBS_NOValue; 
 public System.Decimal STRUCTURE_OBS_NO
        {  
            get  
            {  
                return this.STRUCTURE_OBS_NOValue;  
            }  

          set { SetProperty(ref  STRUCTURE_OBS_NOValue, value); }
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
private  System.Decimal DIM_HEIGHTValue; 
 public System.Decimal DIM_HEIGHT
        {  
            get  
            {  
                return this.DIM_HEIGHTValue;  
            }  

          set { SetProperty(ref  DIM_HEIGHTValue, value); }
        } 
private  System.String DIM_HEIGHT_OUOMValue; 
 public System.String DIM_HEIGHT_OUOM
        {  
            get  
            {  
                return this.DIM_HEIGHT_OUOMValue;  
            }  

          set { SetProperty(ref  DIM_HEIGHT_OUOMValue, value); }
        } 
private  System.String DIM_HEIGHT_UOMValue; 
 public System.String DIM_HEIGHT_UOM
        {  
            get  
            {  
                return this.DIM_HEIGHT_UOMValue;  
            }  

          set { SetProperty(ref  DIM_HEIGHT_UOMValue, value); }
        } 
private  System.Decimal DIM_LENGTHValue; 
 public System.Decimal DIM_LENGTH
        {  
            get  
            {  
                return this.DIM_LENGTHValue;  
            }  

          set { SetProperty(ref  DIM_LENGTHValue, value); }
        } 
private  System.String DIM_LENGTH_OUOMValue; 
 public System.String DIM_LENGTH_OUOM
        {  
            get  
            {  
                return this.DIM_LENGTH_OUOMValue;  
            }  

          set { SetProperty(ref  DIM_LENGTH_OUOMValue, value); }
        } 
private  System.String DIM_LENGTH_UOMValue; 
 public System.String DIM_LENGTH_UOM
        {  
            get  
            {  
                return this.DIM_LENGTH_UOMValue;  
            }  

          set { SetProperty(ref  DIM_LENGTH_UOMValue, value); }
        } 
private  System.Decimal DIM_WEIGHTValue; 
 public System.Decimal DIM_WEIGHT
        {  
            get  
            {  
                return this.DIM_WEIGHTValue;  
            }  

          set { SetProperty(ref  DIM_WEIGHTValue, value); }
        } 
private  System.String DIM_WEIGHT_OUOMValue; 
 public System.String DIM_WEIGHT_OUOM
        {  
            get  
            {  
                return this.DIM_WEIGHT_OUOMValue;  
            }  

          set { SetProperty(ref  DIM_WEIGHT_OUOMValue, value); }
        } 
private  System.String DIM_WEIGHT_UOMValue; 
 public System.String DIM_WEIGHT_UOM
        {  
            get  
            {  
                return this.DIM_WEIGHT_UOMValue;  
            }  

          set { SetProperty(ref  DIM_WEIGHT_UOMValue, value); }
        } 
private  System.Decimal DIM_WIDTHValue; 
 public System.Decimal DIM_WIDTH
        {  
            get  
            {  
                return this.DIM_WIDTHValue;  
            }  

          set { SetProperty(ref  DIM_WIDTHValue, value); }
        } 
private  System.String DIM_WIDTH_OUOMValue; 
 public System.String DIM_WIDTH_OUOM
        {  
            get  
            {  
                return this.DIM_WIDTH_OUOMValue;  
            }  

          set { SetProperty(ref  DIM_WIDTH_OUOMValue, value); }
        } 
private  System.String DIM_WIDTH_UOMValue; 
 public System.String DIM_WIDTH_UOM
        {  
            get  
            {  
                return this.DIM_WIDTH_UOMValue;  
            }  

          set { SetProperty(ref  DIM_WIDTH_UOMValue, value); }
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
private  System.String LOWER_CONTAINED_STORE_IDValue; 
 public System.String LOWER_CONTAINED_STORE_ID
        {  
            get  
            {  
                return this.LOWER_CONTAINED_STORE_IDValue;  
            }  

          set { SetProperty(ref  LOWER_CONTAINED_STORE_IDValue, value); }
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
private  System.Decimal TOTAL_CAPACITYValue; 
 public System.Decimal TOTAL_CAPACITY
        {  
            get  
            {  
                return this.TOTAL_CAPACITYValue;  
            }  

          set { SetProperty(ref  TOTAL_CAPACITYValue, value); }
        } 
private  System.String UPPER_CONTAINED_STORE_IDValue; 
 public System.String UPPER_CONTAINED_STORE_ID
        {  
            get  
            {  
                return this.UPPER_CONTAINED_STORE_IDValue;  
            }  

          set { SetProperty(ref  UPPER_CONTAINED_STORE_IDValue, value); }
        } 
private  System.Decimal USED_CAPACITYValue; 
 public System.Decimal USED_CAPACITY
        {  
            get  
            {  
                return this.USED_CAPACITYValue;  
            }  

          set { SetProperty(ref  USED_CAPACITYValue, value); }
        } 
private  System.DateTime? USED_CAPACITY_DATEValue; 
 public System.DateTime? USED_CAPACITY_DATE
        {  
            get  
            {  
                return this.USED_CAPACITY_DATEValue;  
            }  

          set { SetProperty(ref  USED_CAPACITY_DATEValue, value); }
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


    public RM_DATA_STORE_STRUCTURE () { }

  }
}


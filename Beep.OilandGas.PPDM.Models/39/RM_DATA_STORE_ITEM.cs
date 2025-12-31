using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class RM_DATA_STORE_ITEM: Entity,IPPDMEntity

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
private  System.Decimal ITEM_OBS_NOValue; 
 public System.Decimal ITEM_OBS_NO
        {  
            get  
            {  
                return this.ITEM_OBS_NOValue;  
            }  

          set { SetProperty(ref  ITEM_OBS_NOValue, value); }
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
private  System.String ITEM_CATEGORYValue; 
 public System.String ITEM_CATEGORY
        {  
            get  
            {  
                return this.ITEM_CATEGORYValue;  
            }  

          set { SetProperty(ref  ITEM_CATEGORYValue, value); }
        } 
private  System.String ITEM_SUB_CATEGORYValue; 
 public System.String ITEM_SUB_CATEGORY
        {  
            get  
            {  
                return this.ITEM_SUB_CATEGORYValue;  
            }  

          set { SetProperty(ref  ITEM_SUB_CATEGORYValue, value); }
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


    public RM_DATA_STORE_ITEM () { }

  }
}


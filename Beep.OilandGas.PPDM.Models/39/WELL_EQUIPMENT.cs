using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_EQUIPMENT: Entity,IPPDMEntity

{

private  System.String UWIValue; 
 public System.String UWI
        {  
            get  
            {  
                return this.UWIValue;  
            }  

          set { SetProperty(ref  UWIValue, value); }
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
private  System.String EQUIPMENT_IDValue; 
 public System.String EQUIPMENT_ID
        {  
            get  
            {  
                return this.EQUIPMENT_IDValue;  
            }  

          set { SetProperty(ref  EQUIPMENT_IDValue, value); }
        } 
private  System.Decimal EQUIP_OBS_NOValue; 
 public System.Decimal EQUIP_OBS_NO
        {  
            get  
            {  
                return this.EQUIP_OBS_NOValue;  
            }  

          set { SetProperty(ref  EQUIP_OBS_NOValue, value); }
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
private  System.Decimal INSTALL_BASE_DEPTHValue; 
 public System.Decimal INSTALL_BASE_DEPTH
        {  
            get  
            {  
                return this.INSTALL_BASE_DEPTHValue;  
            }  

          set { SetProperty(ref  INSTALL_BASE_DEPTHValue, value); }
        } 
private  System.String INSTALL_BASE_DEPTH_OUOMValue; 
 public System.String INSTALL_BASE_DEPTH_OUOM
        {  
            get  
            {  
                return this.INSTALL_BASE_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  INSTALL_BASE_DEPTH_OUOMValue, value); }
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
private  System.Decimal INSTALL_SEQ_NOValue; 
 public System.Decimal INSTALL_SEQ_NO
        {  
            get  
            {  
                return this.INSTALL_SEQ_NOValue;  
            }  

          set { SetProperty(ref  INSTALL_SEQ_NOValue, value); }
        } 
private  System.Decimal INSTALL_TOP_DEPTHValue; 
 public System.Decimal INSTALL_TOP_DEPTH
        {  
            get  
            {  
                return this.INSTALL_TOP_DEPTHValue;  
            }  

          set { SetProperty(ref  INSTALL_TOP_DEPTHValue, value); }
        } 
private  System.String INSTALL_TOP_DEPTH_OUOMValue; 
 public System.String INSTALL_TOP_DEPTH_OUOM
        {  
            get  
            {  
                return this.INSTALL_TOP_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  INSTALL_TOP_DEPTH_OUOMValue, value); }
        } 
private  System.String PARENT_EQUIPMENT_IDValue; 
 public System.String PARENT_EQUIPMENT_ID
        {  
            get  
            {  
                return this.PARENT_EQUIPMENT_IDValue;  
            }  

          set { SetProperty(ref  PARENT_EQUIPMENT_IDValue, value); }
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
private  System.DateTime? REMOVAL_DATEValue; 
 public System.DateTime? REMOVAL_DATE
        {  
            get  
            {  
                return this.REMOVAL_DATEValue;  
            }  

          set { SetProperty(ref  REMOVAL_DATEValue, value); }
        } 
private  System.String STRING_IDValue; 
 public System.String STRING_ID
        {  
            get  
            {  
                return this.STRING_IDValue;  
            }  

          set { SetProperty(ref  STRING_IDValue, value); }
        } 
private  System.String STRING_SOURCEValue; 
 public System.String STRING_SOURCE
        {  
            get  
            {  
                return this.STRING_SOURCEValue;  
            }  

          set { SetProperty(ref  STRING_SOURCEValue, value); }
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


    public WELL_EQUIPMENT () { }

  }
}


using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class SF_DOCK: Entity,IPPDMEntity

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
private  System.String DOCK_IDValue; 
 public System.String DOCK_ID
        {  
            get  
            {  
                return this.DOCK_IDValue;  
            }  

          set { SetProperty(ref  DOCK_IDValue, value); }
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
private  System.Decimal DOCK_LENGTHValue; 
 public System.Decimal DOCK_LENGTH
        {  
            get  
            {  
                return this.DOCK_LENGTHValue;  
            }  

          set { SetProperty(ref  DOCK_LENGTHValue, value); }
        } 
private  System.String DOCK_LENGTH_OUOMValue; 
 public System.String DOCK_LENGTH_OUOM
        {  
            get  
            {  
                return this.DOCK_LENGTH_OUOMValue;  
            }  

          set { SetProperty(ref  DOCK_LENGTH_OUOMValue, value); }
        } 
private  System.String DOCK_TYPEValue; 
 public System.String DOCK_TYPE
        {  
            get  
            {  
                return this.DOCK_TYPEValue;  
            }  

          set { SetProperty(ref  DOCK_TYPEValue, value); }
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
private  System.String SURFACE_TYPEValue; 
 public System.String SURFACE_TYPE
        {  
            get  
            {  
                return this.SURFACE_TYPEValue;  
            }  

          set { SetProperty(ref  SURFACE_TYPEValue, value); }
        } 
private  System.Decimal WATER_DEPTHValue; 
 public System.Decimal WATER_DEPTH
        {  
            get  
            {  
                return this.WATER_DEPTHValue;  
            }  

          set { SetProperty(ref  WATER_DEPTHValue, value); }
        } 
private  System.String WATER_DEPTH_OUOMValue; 
 public System.String WATER_DEPTH_OUOM
        {  
            get  
            {  
                return this.WATER_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  WATER_DEPTH_OUOMValue, value); }
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


    public SF_DOCK () { }

  }
}


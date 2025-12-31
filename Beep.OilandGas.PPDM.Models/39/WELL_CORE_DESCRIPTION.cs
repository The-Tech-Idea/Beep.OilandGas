using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_CORE_DESCRIPTION: Entity,IPPDMEntity

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
private  System.String CORE_IDValue; 
 public System.String CORE_ID
        {  
            get  
            {  
                return this.CORE_IDValue;  
            }  

          set { SetProperty(ref  CORE_IDValue, value); }
        } 
private  System.Decimal DESCRIPTION_OBS_NOValue; 
 public System.Decimal DESCRIPTION_OBS_NO
        {  
            get  
            {  
                return this.DESCRIPTION_OBS_NOValue;  
            }  

          set { SetProperty(ref  DESCRIPTION_OBS_NOValue, value); }
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
private  System.Decimal BASE_DEPTHValue; 
 public System.Decimal BASE_DEPTH
        {  
            get  
            {  
                return this.BASE_DEPTHValue;  
            }  

          set { SetProperty(ref  BASE_DEPTHValue, value); }
        } 
private  System.String BASE_DEPTH_OUOMValue; 
 public System.String BASE_DEPTH_OUOM
        {  
            get  
            {  
                return this.BASE_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  BASE_DEPTH_OUOMValue, value); }
        } 
private  System.String CORE_DESCRIPTION_COMPANYValue; 
 public System.String CORE_DESCRIPTION_COMPANY
        {  
            get  
            {  
                return this.CORE_DESCRIPTION_COMPANYValue;  
            }  

          set { SetProperty(ref  CORE_DESCRIPTION_COMPANYValue, value); }
        } 
private  System.DateTime? DESCRIPTION_DATEValue; 
 public System.DateTime? DESCRIPTION_DATE
        {  
            get  
            {  
                return this.DESCRIPTION_DATEValue;  
            }  

          set { SetProperty(ref  DESCRIPTION_DATEValue, value); }
        } 
private  System.Decimal DIP_ANGLEValue; 
 public System.Decimal DIP_ANGLE
        {  
            get  
            {  
                return this.DIP_ANGLEValue;  
            }  

          set { SetProperty(ref  DIP_ANGLEValue, value); }
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
private  System.Decimal INTERVAL_THICKNESSValue; 
 public System.Decimal INTERVAL_THICKNESS
        {  
            get  
            {  
                return this.INTERVAL_THICKNESSValue;  
            }  

          set { SetProperty(ref  INTERVAL_THICKNESSValue, value); }
        } 
private  System.String INTERVAL_THICKNESS_OUOMValue; 
 public System.String INTERVAL_THICKNESS_OUOM
        {  
            get  
            {  
                return this.INTERVAL_THICKNESS_OUOMValue;  
            }  

          set { SetProperty(ref  INTERVAL_THICKNESS_OUOMValue, value); }
        } 
private  System.String LITHOLOGY_DESCValue; 
 public System.String LITHOLOGY_DESC
        {  
            get  
            {  
                return this.LITHOLOGY_DESCValue;  
            }  

          set { SetProperty(ref  LITHOLOGY_DESCValue, value); }
        } 
private  System.Decimal POROSITY_LENGTHValue; 
 public System.Decimal POROSITY_LENGTH
        {  
            get  
            {  
                return this.POROSITY_LENGTHValue;  
            }  

          set { SetProperty(ref  POROSITY_LENGTHValue, value); }
        } 
private  System.String POROSITY_LENGTH_OUOMValue; 
 public System.String POROSITY_LENGTH_OUOM
        {  
            get  
            {  
                return this.POROSITY_LENGTH_OUOMValue;  
            }  

          set { SetProperty(ref  POROSITY_LENGTH_OUOMValue, value); }
        } 
private  System.String POROSITY_QUALITYValue; 
 public System.String POROSITY_QUALITY
        {  
            get  
            {  
                return this.POROSITY_QUALITYValue;  
            }  

          set { SetProperty(ref  POROSITY_QUALITYValue, value); }
        } 
private  System.String POROSITY_TYPEValue; 
 public System.String POROSITY_TYPE
        {  
            get  
            {  
                return this.POROSITY_TYPEValue;  
            }  

          set { SetProperty(ref  POROSITY_TYPEValue, value); }
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
private  System.Decimal SHOW_LENGTHValue; 
 public System.Decimal SHOW_LENGTH
        {  
            get  
            {  
                return this.SHOW_LENGTHValue;  
            }  

          set { SetProperty(ref  SHOW_LENGTHValue, value); }
        } 
private  System.String SHOW_LENGTH_OUOMValue; 
 public System.String SHOW_LENGTH_OUOM
        {  
            get  
            {  
                return this.SHOW_LENGTH_OUOMValue;  
            }  

          set { SetProperty(ref  SHOW_LENGTH_OUOMValue, value); }
        } 
private  System.String SHOW_TYPEValue; 
 public System.String SHOW_TYPE
        {  
            get  
            {  
                return this.SHOW_TYPEValue;  
            }  

          set { SetProperty(ref  SHOW_TYPEValue, value); }
        } 
private  System.Decimal TOP_DEPTHValue; 
 public System.Decimal TOP_DEPTH
        {  
            get  
            {  
                return this.TOP_DEPTHValue;  
            }  

          set { SetProperty(ref  TOP_DEPTHValue, value); }
        } 
private  System.String TOP_DEPTH_OUOMValue; 
 public System.String TOP_DEPTH_OUOM
        {  
            get  
            {  
                return this.TOP_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  TOP_DEPTH_OUOMValue, value); }
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


    public WELL_CORE_DESCRIPTION () { }

  }
}


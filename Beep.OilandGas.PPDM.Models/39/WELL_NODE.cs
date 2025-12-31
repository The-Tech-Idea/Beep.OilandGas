using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_NODE: Entity,IPPDMEntity

{

private  System.String NODE_IDValue; 
 public System.String NODE_ID
        {  
            get  
            {  
                return this.NODE_IDValue;  
            }  

          set { SetProperty(ref  NODE_IDValue, value); }
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
private  System.String COORDINATE_QUALITYValue; 
 public System.String COORDINATE_QUALITY
        {  
            get  
            {  
                return this.COORDINATE_QUALITYValue;  
            }  

          set { SetProperty(ref  COORDINATE_QUALITYValue, value); }
        } 
private  System.String COORD_ACQUISITION_IDValue; 
 public System.String COORD_ACQUISITION_ID
        {  
            get  
            {  
                return this.COORD_ACQUISITION_IDValue;  
            }  

          set { SetProperty(ref  COORD_ACQUISITION_IDValue, value); }
        } 
private  System.String COORD_SYSTEM_IDValue; 
 public System.String COORD_SYSTEM_ID
        {  
            get  
            {  
                return this.COORD_SYSTEM_IDValue;  
            }  

          set { SetProperty(ref  COORD_SYSTEM_IDValue, value); }
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
private  System.Decimal LATITUDEValue; 
 public System.Decimal LATITUDE
        {  
            get  
            {  
                return this.LATITUDEValue;  
            }  

          set { SetProperty(ref  LATITUDEValue, value); }
        } 
private  System.String LEGAL_SURVEY_TYPEValue; 
 public System.String LEGAL_SURVEY_TYPE
        {  
            get  
            {  
                return this.LEGAL_SURVEY_TYPEValue;  
            }  

          set { SetProperty(ref  LEGAL_SURVEY_TYPEValue, value); }
        } 
private  System.String LOCAL_COORD_SYSTEM_IDValue; 
 public System.String LOCAL_COORD_SYSTEM_ID
        {  
            get  
            {  
                return this.LOCAL_COORD_SYSTEM_IDValue;  
            }  

          set { SetProperty(ref  LOCAL_COORD_SYSTEM_IDValue, value); }
        } 
private  System.String LOCATION_QUALITYValue; 
 public System.String LOCATION_QUALITY
        {  
            get  
            {  
                return this.LOCATION_QUALITYValue;  
            }  

          set { SetProperty(ref  LOCATION_QUALITYValue, value); }
        } 
private  System.String LOCATION_TYPEValue; 
 public System.String LOCATION_TYPE
        {  
            get  
            {  
                return this.LOCATION_TYPEValue;  
            }  

          set { SetProperty(ref  LOCATION_TYPEValue, value); }
        } 
private  System.Decimal LONGITUDEValue; 
 public System.Decimal LONGITUDE
        {  
            get  
            {  
                return this.LONGITUDEValue;  
            }  

          set { SetProperty(ref  LONGITUDEValue, value); }
        } 
private  System.String NODE_POSITIONValue; 
 public System.String NODE_POSITION
        {  
            get  
            {  
                return this.NODE_POSITIONValue;  
            }  

          set { SetProperty(ref  NODE_POSITIONValue, value); }
        } 
private  System.Decimal ORIGINAL_OBS_NOValue; 
 public System.Decimal ORIGINAL_OBS_NO
        {  
            get  
            {  
                return this.ORIGINAL_OBS_NOValue;  
            }  

          set { SetProperty(ref  ORIGINAL_OBS_NOValue, value); }
        } 
private  System.String ORIGINAL_XY_UOMValue; 
 public System.String ORIGINAL_XY_UOM
        {  
            get  
            {  
                return this.ORIGINAL_XY_UOMValue;  
            }  

          set { SetProperty(ref  ORIGINAL_XY_UOMValue, value); }
        } 
private  System.String PLATFORM_IDValue; 
 public System.String PLATFORM_ID
        {  
            get  
            {  
                return this.PLATFORM_IDValue;  
            }  

          set { SetProperty(ref  PLATFORM_IDValue, value); }
        } 
private  System.String PLATFORM_SF_SUBTYPEValue; 
 public System.String PLATFORM_SF_SUBTYPE
        {  
            get  
            {  
                return this.PLATFORM_SF_SUBTYPEValue;  
            }  

          set { SetProperty(ref  PLATFORM_SF_SUBTYPEValue, value); }
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
private  System.String PREFERRED_INDValue; 
 public System.String PREFERRED_IND
        {  
            get  
            {  
                return this.PREFERRED_INDValue;  
            }  

          set { SetProperty(ref  PREFERRED_INDValue, value); }
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
private  System.Decimal SELECTED_OBS_NOValue; 
 public System.Decimal SELECTED_OBS_NO
        {  
            get  
            {  
                return this.SELECTED_OBS_NOValue;  
            }  

          set { SetProperty(ref  SELECTED_OBS_NOValue, value); }
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
private  System.String UWIValue; 
 public System.String UWI
        {  
            get  
            {  
                return this.UWIValue;  
            }  

          set { SetProperty(ref  UWIValue, value); }
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


    public WELL_NODE () { }

  }
}


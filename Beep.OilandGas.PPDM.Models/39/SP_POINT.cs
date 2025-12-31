using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class SP_POINT: Entity,IPPDMEntity

{

private  System.String POINT_IDValue; 
 public System.String POINT_ID
        {  
            get  
            {  
                return this.POINT_IDValue;  
            }  

          set { SetProperty(ref  POINT_IDValue, value); }
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
private  System.Decimal DATUM_ELEVValue; 
 public System.Decimal DATUM_ELEV
        {  
            get  
            {  
                return this.DATUM_ELEVValue;  
            }  

          set { SetProperty(ref  DATUM_ELEVValue, value); }
        } 
private  System.String DATUM_ELEV_OUOMValue; 
 public System.String DATUM_ELEV_OUOM
        {  
            get  
            {  
                return this.DATUM_ELEV_OUOMValue;  
            }  

          set { SetProperty(ref  DATUM_ELEV_OUOMValue, value); }
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
private  System.Decimal ELEVATIONValue; 
 public System.Decimal ELEVATION
        {  
            get  
            {  
                return this.ELEVATIONValue;  
            }  

          set { SetProperty(ref  ELEVATIONValue, value); }
        } 
private  System.String ELEVATION_OUOMValue; 
 public System.String ELEVATION_OUOM
        {  
            get  
            {  
                return this.ELEVATION_OUOMValue;  
            }  

          set { SetProperty(ref  ELEVATION_OUOMValue, value); }
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
private  System.Decimal MEASURED_DEPTHValue; 
 public System.Decimal MEASURED_DEPTH
        {  
            get  
            {  
                return this.MEASURED_DEPTHValue;  
            }  

          set { SetProperty(ref  MEASURED_DEPTHValue, value); }
        } 
private  System.String MEASURED_DEPTH_OUOMValue; 
 public System.String MEASURED_DEPTH_OUOM
        {  
            get  
            {  
                return this.MEASURED_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  MEASURED_DEPTH_OUOMValue, value); }
        } 
private  System.String POINT_LABELValue; 
 public System.String POINT_LABEL
        {  
            get  
            {  
                return this.POINT_LABELValue;  
            }  

          set { SetProperty(ref  POINT_LABELValue, value); }
        } 
private  System.Decimal POINT_NOValue; 
 public System.Decimal POINT_NO
        {  
            get  
            {  
                return this.POINT_NOValue;  
            }  

          set { SetProperty(ref  POINT_NOValue, value); }
        } 
private  System.String POINT_POSITIONValue; 
 public System.String POINT_POSITION
        {  
            get  
            {  
                return this.POINT_POSITIONValue;  
            }  

          set { SetProperty(ref  POINT_POSITIONValue, value); }
        } 
private  System.Decimal POINT_SEQ_NOValue; 
 public System.Decimal POINT_SEQ_NO
        {  
            get  
            {  
                return this.POINT_SEQ_NOValue;  
            }  

          set { SetProperty(ref  POINT_SEQ_NOValue, value); }
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
private  System.String REFERENCE_DATUMValue; 
 public System.String REFERENCE_DATUM
        {  
            get  
            {  
                return this.REFERENCE_DATUMValue;  
            }  

          set { SetProperty(ref  REFERENCE_DATUMValue, value); }
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
private  System.String SPATIAL_DESCRIPTION_IDValue; 
 public System.String SPATIAL_DESCRIPTION_ID
        {  
            get  
            {  
                return this.SPATIAL_DESCRIPTION_IDValue;  
            }  

          set { SetProperty(ref  SPATIAL_DESCRIPTION_IDValue, value); }
        } 
private  System.Decimal SPATIAL_OBS_NOValue; 
 public System.Decimal SPATIAL_OBS_NO
        {  
            get  
            {  
                return this.SPATIAL_OBS_NOValue;  
            }  

          set { SetProperty(ref  SPATIAL_OBS_NOValue, value); }
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


    public SP_POINT () { }

  }
}


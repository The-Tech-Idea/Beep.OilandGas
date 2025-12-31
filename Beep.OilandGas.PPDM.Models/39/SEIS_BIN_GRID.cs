using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class SEIS_BIN_GRID: Entity,IPPDMEntity

{

private  System.String SEIS_SET_SUBTYPEValue; 
 public System.String SEIS_SET_SUBTYPE
        {  
            get  
            {  
                return this.SEIS_SET_SUBTYPEValue;  
            }  

          set { SetProperty(ref  SEIS_SET_SUBTYPEValue, value); }
        } 
private  System.String SEIS_SET_IDValue; 
 public System.String SEIS_SET_ID
        {  
            get  
            {  
                return this.SEIS_SET_IDValue;  
            }  

          set { SetProperty(ref  SEIS_SET_IDValue, value); }
        } 
private  System.String BIN_GRID_IDValue; 
 public System.String BIN_GRID_ID
        {  
            get  
            {  
                return this.BIN_GRID_IDValue;  
            }  

          set { SetProperty(ref  BIN_GRID_IDValue, value); }
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
private  System.String ACTIVE_INDValue; 
 public System.String ACTIVE_IND
        {  
            get  
            {  
                return this.ACTIVE_INDValue;  
            }  

          set { SetProperty(ref  ACTIVE_INDValue, value); }
        } 
private  System.Decimal ANGLE_ROTATIONValue; 
 public System.Decimal ANGLE_ROTATION
        {  
            get  
            {  
                return this.ANGLE_ROTATIONValue;  
            }  

          set { SetProperty(ref  ANGLE_ROTATIONValue, value); }
        } 
private  System.String BIN_GRID_TYPEValue; 
 public System.String BIN_GRID_TYPE
        {  
            get  
            {  
                return this.BIN_GRID_TYPEValue;  
            }  

          set { SetProperty(ref  BIN_GRID_TYPEValue, value); }
        } 
private  System.Decimal BIN_GRID_VERSIONValue; 
 public System.Decimal BIN_GRID_VERSION
        {  
            get  
            {  
                return this.BIN_GRID_VERSIONValue;  
            }  

          set { SetProperty(ref  BIN_GRID_VERSIONValue, value); }
        } 
private  System.String BIN_METHODValue; 
 public System.String BIN_METHOD
        {  
            get  
            {  
                return this.BIN_METHODValue;  
            }  

          set { SetProperty(ref  BIN_METHODValue, value); }
        } 
private  System.String BIN_POINT_OUOMValue; 
 public System.String BIN_POINT_OUOM
        {  
            get  
            {  
                return this.BIN_POINT_OUOMValue;  
            }  

          set { SetProperty(ref  BIN_POINT_OUOMValue, value); }
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
private  System.Decimal CORNER1_LATValue; 
 public System.Decimal CORNER1_LAT
        {  
            get  
            {  
                return this.CORNER1_LATValue;  
            }  

          set { SetProperty(ref  CORNER1_LATValue, value); }
        } 
private  System.Decimal CORNER1_LONGValue; 
 public System.Decimal CORNER1_LONG
        {  
            get  
            {  
                return this.CORNER1_LONGValue;  
            }  

          set { SetProperty(ref  CORNER1_LONGValue, value); }
        } 
private  System.Decimal CORNER2_LATValue; 
 public System.Decimal CORNER2_LAT
        {  
            get  
            {  
                return this.CORNER2_LATValue;  
            }  

          set { SetProperty(ref  CORNER2_LATValue, value); }
        } 
private  System.Decimal CORNER2_LONGValue; 
 public System.Decimal CORNER2_LONG
        {  
            get  
            {  
                return this.CORNER2_LONGValue;  
            }  

          set { SetProperty(ref  CORNER2_LONGValue, value); }
        } 
private  System.Decimal CORNER3_LATValue; 
 public System.Decimal CORNER3_LAT
        {  
            get  
            {  
                return this.CORNER3_LATValue;  
            }  

          set { SetProperty(ref  CORNER3_LATValue, value); }
        } 
private  System.Decimal CORNER3_LONGValue; 
 public System.Decimal CORNER3_LONG
        {  
            get  
            {  
                return this.CORNER3_LONGValue;  
            }  

          set { SetProperty(ref  CORNER3_LONGValue, value); }
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
private  System.Decimal FOLD_COVERAGEValue; 
 public System.Decimal FOLD_COVERAGE
        {  
            get  
            {  
                return this.FOLD_COVERAGEValue;  
            }  

          set { SetProperty(ref  FOLD_COVERAGEValue, value); }
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
private  System.Decimal NLINE_AZIMUTHValue; 
 public System.Decimal NLINE_AZIMUTH
        {  
            get  
            {  
                return this.NLINE_AZIMUTHValue;  
            }  

          set { SetProperty(ref  NLINE_AZIMUTHValue, value); }
        } 
private  System.Decimal NLINE_COUNTValue; 
 public System.Decimal NLINE_COUNT
        {  
            get  
            {  
                return this.NLINE_COUNTValue;  
            }  

          set { SetProperty(ref  NLINE_COUNTValue, value); }
        } 
private  System.Decimal NLINE_MAX_NOValue; 
 public System.Decimal NLINE_MAX_NO
        {  
            get  
            {  
                return this.NLINE_MAX_NOValue;  
            }  

          set { SetProperty(ref  NLINE_MAX_NOValue, value); }
        } 
private  System.Decimal NLINE_MIN_NOValue; 
 public System.Decimal NLINE_MIN_NO
        {  
            get  
            {  
                return this.NLINE_MIN_NOValue;  
            }  

          set { SetProperty(ref  NLINE_MIN_NOValue, value); }
        } 
private  System.Decimal NLINE_SPACINGValue; 
 public System.Decimal NLINE_SPACING
        {  
            get  
            {  
                return this.NLINE_SPACINGValue;  
            }  

          set { SetProperty(ref  NLINE_SPACINGValue, value); }
        } 
private  System.String NORTH_TYPEValue; 
 public System.String NORTH_TYPE
        {  
            get  
            {  
                return this.NORTH_TYPEValue;  
            }  

          set { SetProperty(ref  NORTH_TYPEValue, value); }
        } 
private  System.Decimal POINT_ORIGIN_EASTINGValue; 
 public System.Decimal POINT_ORIGIN_EASTING
        {  
            get  
            {  
                return this.POINT_ORIGIN_EASTINGValue;  
            }  

          set { SetProperty(ref  POINT_ORIGIN_EASTINGValue, value); }
        } 
private  System.Decimal POINT_ORIGIN_LATITUDEValue; 
 public System.Decimal POINT_ORIGIN_LATITUDE
        {  
            get  
            {  
                return this.POINT_ORIGIN_LATITUDEValue;  
            }  

          set { SetProperty(ref  POINT_ORIGIN_LATITUDEValue, value); }
        } 
private  System.Decimal POINT_ORIGIN_LONGITUDEValue; 
 public System.Decimal POINT_ORIGIN_LONGITUDE
        {  
            get  
            {  
                return this.POINT_ORIGIN_LONGITUDEValue;  
            }  

          set { SetProperty(ref  POINT_ORIGIN_LONGITUDEValue, value); }
        } 
private  System.Decimal POINT_ORIGIN_NORTHINGValue; 
 public System.Decimal POINT_ORIGIN_NORTHING
        {  
            get  
            {  
                return this.POINT_ORIGIN_NORTHINGValue;  
            }  

          set { SetProperty(ref  POINT_ORIGIN_NORTHINGValue, value); }
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
private  System.Decimal XLINE_AZIMUTHValue; 
 public System.Decimal XLINE_AZIMUTH
        {  
            get  
            {  
                return this.XLINE_AZIMUTHValue;  
            }  

          set { SetProperty(ref  XLINE_AZIMUTHValue, value); }
        } 
private  System.Decimal XLINE_COUNTValue; 
 public System.Decimal XLINE_COUNT
        {  
            get  
            {  
                return this.XLINE_COUNTValue;  
            }  

          set { SetProperty(ref  XLINE_COUNTValue, value); }
        } 
private  System.Decimal XLINE_MAX_NOValue; 
 public System.Decimal XLINE_MAX_NO
        {  
            get  
            {  
                return this.XLINE_MAX_NOValue;  
            }  

          set { SetProperty(ref  XLINE_MAX_NOValue, value); }
        } 
private  System.Decimal XLINE_MIN_NOValue; 
 public System.Decimal XLINE_MIN_NO
        {  
            get  
            {  
                return this.XLINE_MIN_NOValue;  
            }  

          set { SetProperty(ref  XLINE_MIN_NOValue, value); }
        } 
private  System.Decimal XLINE_SPACINGValue; 
 public System.Decimal XLINE_SPACING
        {  
            get  
            {  
                return this.XLINE_SPACINGValue;  
            }  

          set { SetProperty(ref  XLINE_SPACINGValue, value); }
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


    public SEIS_BIN_GRID () { }

  }
}


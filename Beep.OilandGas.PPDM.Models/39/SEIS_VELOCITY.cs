using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class SEIS_VELOCITY: Entity,IPPDMEntity

{

private  System.String VELOCITY_VOLUME_IDValue; 
 public System.String VELOCITY_VOLUME_ID
        {  
            get  
            {  
                return this.VELOCITY_VOLUME_IDValue;  
            }  

          set { SetProperty(ref  VELOCITY_VOLUME_IDValue, value); }
        } 
private  System.String VOLUME_POINTValue; 
 public System.String VOLUME_POINT
        {  
            get  
            {  
                return this.VOLUME_POINTValue;  
            }  

          set { SetProperty(ref  VOLUME_POINTValue, value); }
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
private  System.String BIN_GRID_IDValue; 
 public System.String BIN_GRID_ID
        {  
            get  
            {  
                return this.BIN_GRID_IDValue;  
            }  

          set { SetProperty(ref  BIN_GRID_IDValue, value); }
        } 
private  System.String BIN_POINT_IDValue; 
 public System.String BIN_POINT_ID
        {  
            get  
            {  
                return this.BIN_POINT_IDValue;  
            }  

          set { SetProperty(ref  BIN_POINT_IDValue, value); }
        } 
private  System.String BIN_SOURCEValue; 
 public System.String BIN_SOURCE
        {  
            get  
            {  
                return this.BIN_SOURCEValue;  
            }  

          set { SetProperty(ref  BIN_SOURCEValue, value); }
        } 
private  System.String COMPUTE_METHODValue; 
 public System.String COMPUTE_METHOD
        {  
            get  
            {  
                return this.COMPUTE_METHODValue;  
            }  

          set { SetProperty(ref  COMPUTE_METHODValue, value); }
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
private  System.Decimal LONGITUDEValue; 
 public System.Decimal LONGITUDE
        {  
            get  
            {  
                return this.LONGITUDEValue;  
            }  

          set { SetProperty(ref  LONGITUDEValue, value); }
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
private  System.Decimal RECEIVER_MDValue; 
 public System.Decimal RECEIVER_MD
        {  
            get  
            {  
                return this.RECEIVER_MDValue;  
            }  

          set { SetProperty(ref  RECEIVER_MDValue, value); }
        } 
private  System.String RECEIVER_MD_OUOMValue; 
 public System.String RECEIVER_MD_OUOM
        {  
            get  
            {  
                return this.RECEIVER_MD_OUOMValue;  
            }  

          set { SetProperty(ref  RECEIVER_MD_OUOMValue, value); }
        } 
private  System.Decimal RECEIVER_VERT_DEPTHValue; 
 public System.Decimal RECEIVER_VERT_DEPTH
        {  
            get  
            {  
                return this.RECEIVER_VERT_DEPTHValue;  
            }  

          set { SetProperty(ref  RECEIVER_VERT_DEPTHValue, value); }
        } 
private  System.String RECEIVER_VERT_DEPTH_OUOMValue; 
 public System.String RECEIVER_VERT_DEPTH_OUOM
        {  
            get  
            {  
                return this.RECEIVER_VERT_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  RECEIVER_VERT_DEPTH_OUOMValue, value); }
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
private  System.String SEIS_POINT_IDValue; 
 public System.String SEIS_POINT_ID
        {  
            get  
            {  
                return this.SEIS_POINT_IDValue;  
            }  

          set { SetProperty(ref  SEIS_POINT_IDValue, value); }
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
private  System.String SEIS_SET_SUBTYPEValue; 
 public System.String SEIS_SET_SUBTYPE
        {  
            get  
            {  
                return this.SEIS_SET_SUBTYPEValue;  
            }  

          set { SetProperty(ref  SEIS_SET_SUBTYPEValue, value); }
        } 
private  System.Decimal SEIS_TIMEValue; 
 public System.Decimal SEIS_TIME
        {  
            get  
            {  
                return this.SEIS_TIMEValue;  
            }  

          set { SetProperty(ref  SEIS_TIMEValue, value); }
        } 
private  System.String SEIS_TIME_OUOMValue; 
 public System.String SEIS_TIME_OUOM
        {  
            get  
            {  
                return this.SEIS_TIME_OUOMValue;  
            }  

          set { SetProperty(ref  SEIS_TIME_OUOMValue, value); }
        } 
private  System.String SEIS_WELL_SET_IDValue; 
 public System.String SEIS_WELL_SET_ID
        {  
            get  
            {  
                return this.SEIS_WELL_SET_IDValue;  
            }  

          set { SetProperty(ref  SEIS_WELL_SET_IDValue, value); }
        } 
private  System.String SEIS_WELL_SET_SUBTYPEValue; 
 public System.String SEIS_WELL_SET_SUBTYPE
        {  
            get  
            {  
                return this.SEIS_WELL_SET_SUBTYPEValue;  
            }  

          set { SetProperty(ref  SEIS_WELL_SET_SUBTYPEValue, value); }
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
private  System.Decimal SOURCE_MDValue; 
 public System.Decimal SOURCE_MD
        {  
            get  
            {  
                return this.SOURCE_MDValue;  
            }  

          set { SetProperty(ref  SOURCE_MDValue, value); }
        } 
private  System.String SOURCE_MD_OUOMValue; 
 public System.String SOURCE_MD_OUOM
        {  
            get  
            {  
                return this.SOURCE_MD_OUOMValue;  
            }  

          set { SetProperty(ref  SOURCE_MD_OUOMValue, value); }
        } 
private  System.Decimal SOURCE_VERT_DEPTHValue; 
 public System.Decimal SOURCE_VERT_DEPTH
        {  
            get  
            {  
                return this.SOURCE_VERT_DEPTHValue;  
            }  

          set { SetProperty(ref  SOURCE_VERT_DEPTHValue, value); }
        } 
private  System.String SOURCE_VERT_DEPTH_OUOMValue; 
 public System.String SOURCE_VERT_DEPTH_OUOM
        {  
            get  
            {  
                return this.SOURCE_VERT_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  SOURCE_VERT_DEPTH_OUOMValue, value); }
        } 
private  System.Decimal VELOCITYValue; 
 public System.Decimal VELOCITY
        {  
            get  
            {  
                return this.VELOCITYValue;  
            }  

          set { SetProperty(ref  VELOCITYValue, value); }
        } 
private  System.Decimal VELOCITY_AZIMUTHValue; 
 public System.Decimal VELOCITY_AZIMUTH
        {  
            get  
            {  
                return this.VELOCITY_AZIMUTHValue;  
            }  

          set { SetProperty(ref  VELOCITY_AZIMUTHValue, value); }
        } 
private  System.Decimal VELOCITY_DEPTHValue; 
 public System.Decimal VELOCITY_DEPTH
        {  
            get  
            {  
                return this.VELOCITY_DEPTHValue;  
            }  

          set { SetProperty(ref  VELOCITY_DEPTHValue, value); }
        } 
private  System.Decimal VELOCITY_INCLINATIONValue; 
 public System.Decimal VELOCITY_INCLINATION
        {  
            get  
            {  
                return this.VELOCITY_INCLINATIONValue;  
            }  

          set { SetProperty(ref  VELOCITY_INCLINATIONValue, value); }
        } 
private  System.String VELOCITY_INCLINATION_OUOMValue; 
 public System.String VELOCITY_INCLINATION_OUOM
        {  
            get  
            {  
                return this.VELOCITY_INCLINATION_OUOMValue;  
            }  

          set { SetProperty(ref  VELOCITY_INCLINATION_OUOMValue, value); }
        } 
private  System.String VELOCITY_OUOMValue; 
 public System.String VELOCITY_OUOM
        {  
            get  
            {  
                return this.VELOCITY_OUOMValue;  
            }  

          set { SetProperty(ref  VELOCITY_OUOMValue, value); }
        } 
private  System.String VELOCITY_TYPEValue; 
 public System.String VELOCITY_TYPE
        {  
            get  
            {  
                return this.VELOCITY_TYPEValue;  
            }  

          set { SetProperty(ref  VELOCITY_TYPEValue, value); }
        } 
private  System.Decimal VELOCITY_XValue; 
 public System.Decimal VELOCITY_X
        {  
            get  
            {  
                return this.VELOCITY_XValue;  
            }  

          set { SetProperty(ref  VELOCITY_XValue, value); }
        } 
private  System.Decimal VELOCITY_YValue; 
 public System.Decimal VELOCITY_Y
        {  
            get  
            {  
                return this.VELOCITY_YValue;  
            }  

          set { SetProperty(ref  VELOCITY_YValue, value); }
        } 
private  System.Decimal VELOCITY_ZValue; 
 public System.Decimal VELOCITY_Z
        {  
            get  
            {  
                return this.VELOCITY_ZValue;  
            }  

          set { SetProperty(ref  VELOCITY_ZValue, value); }
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


    public SEIS_VELOCITY () { }

  }
}


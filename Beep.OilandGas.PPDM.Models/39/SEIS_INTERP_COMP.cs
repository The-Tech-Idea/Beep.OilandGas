using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class SEIS_INTERP_COMP: Entity,IPPDMEntity

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
private  System.String INTERP_SET_IDValue; 
 public System.String INTERP_SET_ID
        {  
            get  
            {  
                return this.INTERP_SET_IDValue;  
            }  

          set { SetProperty(ref  INTERP_SET_IDValue, value); }
        } 
private  System.String COMPONENT_IDValue; 
 public System.String COMPONENT_ID
        {  
            get  
            {  
                return this.COMPONENT_IDValue;  
            }  

          set { SetProperty(ref  COMPONENT_IDValue, value); }
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
private  System.String AREA_IDValue; 
 public System.String AREA_ID
        {  
            get  
            {  
                return this.AREA_IDValue;  
            }  

          set { SetProperty(ref  AREA_IDValue, value); }
        } 
private  System.String AREA_TYPEValue; 
 public System.String AREA_TYPE
        {  
            get  
            {  
                return this.AREA_TYPEValue;  
            }  

          set { SetProperty(ref  AREA_TYPEValue, value); }
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
private  System.Decimal DATA_SAMPLE_SIZEValue; 
 public System.Decimal DATA_SAMPLE_SIZE
        {  
            get  
            {  
                return this.DATA_SAMPLE_SIZEValue;  
            }  

          set { SetProperty(ref  DATA_SAMPLE_SIZEValue, value); }
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
private  System.String INFORMATION_ITEM_IDValue; 
 public System.String INFORMATION_ITEM_ID
        {  
            get  
            {  
                return this.INFORMATION_ITEM_IDValue;  
            }  

          set { SetProperty(ref  INFORMATION_ITEM_IDValue, value); }
        } 
private  System.String INFO_ITEM_SUBTYPEValue; 
 public System.String INFO_ITEM_SUBTYPE
        {  
            get  
            {  
                return this.INFO_ITEM_SUBTYPEValue;  
            }  

          set { SetProperty(ref  INFO_ITEM_SUBTYPEValue, value); }
        } 
private  System.String INPUT_INDValue; 
 public System.String INPUT_IND
        {  
            get  
            {  
                return this.INPUT_INDValue;  
            }  

          set { SetProperty(ref  INPUT_INDValue, value); }
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
private  System.String ORIGIN_TYPEValue; 
 public System.String ORIGIN_TYPE
        {  
            get  
            {  
                return this.ORIGIN_TYPEValue;  
            }  

          set { SetProperty(ref  ORIGIN_TYPEValue, value); }
        } 
private  System.String OUTPUT_INDValue; 
 public System.String OUTPUT_IND
        {  
            get  
            {  
                return this.OUTPUT_INDValue;  
            }  

          set { SetProperty(ref  OUTPUT_INDValue, value); }
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
private  System.String PROCESS_STEP_IDValue; 
 public System.String PROCESS_STEP_ID
        {  
            get  
            {  
                return this.PROCESS_STEP_IDValue;  
            }  

          set { SetProperty(ref  PROCESS_STEP_IDValue, value); }
        } 
private  System.String PROC_COMPONENT_IDValue; 
 public System.String PROC_COMPONENT_ID
        {  
            get  
            {  
                return this.PROC_COMPONENT_IDValue;  
            }  

          set { SetProperty(ref  PROC_COMPONENT_IDValue, value); }
        } 
private  System.String PROC_SET_IDValue; 
 public System.String PROC_SET_ID
        {  
            get  
            {  
                return this.PROC_SET_IDValue;  
            }  

          set { SetProperty(ref  PROC_SET_IDValue, value); }
        } 
private  System.String PROC_SET_SUBTYPEValue; 
 public System.String PROC_SET_SUBTYPE
        {  
            get  
            {  
                return this.PROC_SET_SUBTYPEValue;  
            }  

          set { SetProperty(ref  PROC_SET_SUBTYPEValue, value); }
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
private  System.String UWIValue; 
 public System.String UWI
        {  
            get  
            {  
                return this.UWIValue;  
            }  

          set { SetProperty(ref  UWIValue, value); }
        } 
private  System.String VELOCITY_VOLUME_IDValue; 
 public System.String VELOCITY_VOLUME_ID
        {  
            get  
            {  
                return this.VELOCITY_VOLUME_IDValue;  
            }  

          set { SetProperty(ref  VELOCITY_VOLUME_IDValue, value); }
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


    public SEIS_INTERP_COMP () { }

  }
}


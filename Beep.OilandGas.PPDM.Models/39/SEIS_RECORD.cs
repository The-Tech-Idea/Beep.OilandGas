using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class SEIS_RECORD: Entity,IPPDMEntity

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
private  System.String RECORD_IDValue; 
 public System.String RECORD_ID
        {  
            get  
            {  
                return this.RECORD_IDValue;  
            }  

          set { SetProperty(ref  RECORD_IDValue, value); }
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
private  System.String ACTUAL_ACQTN_DESIGN_IDValue; 
 public System.String ACTUAL_ACQTN_DESIGN_ID
        {  
            get  
            {  
                return this.ACTUAL_ACQTN_DESIGN_IDValue;  
            }  

          set { SetProperty(ref  ACTUAL_ACQTN_DESIGN_IDValue, value); }
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
private  System.String FIELD_FILE_NUMBERValue; 
 public System.String FIELD_FILE_NUMBER
        {  
            get  
            {  
                return this.FIELD_FILE_NUMBERValue;  
            }  

          set { SetProperty(ref  FIELD_FILE_NUMBERValue, value); }
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
private  System.String LOGICAL_RECORD_NUMBERValue; 
 public System.String LOGICAL_RECORD_NUMBER
        {  
            get  
            {  
                return this.LOGICAL_RECORD_NUMBERValue;  
            }  

          set { SetProperty(ref  LOGICAL_RECORD_NUMBERValue, value); }
        } 
private  System.String PATCH_IDValue; 
 public System.String PATCH_ID
        {  
            get  
            {  
                return this.PATCH_IDValue;  
            }  

          set { SetProperty(ref  PATCH_IDValue, value); }
        } 
private  System.String PATCH_USED_INDValue; 
 public System.String PATCH_USED_IND
        {  
            get  
            {  
                return this.PATCH_USED_INDValue;  
            }  

          set { SetProperty(ref  PATCH_USED_INDValue, value); }
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
private  System.Decimal RCRD_CHANNEL_COUNTValue; 
 public System.Decimal RCRD_CHANNEL_COUNT
        {  
            get  
            {  
                return this.RCRD_CHANNEL_COUNTValue;  
            }  

          set { SetProperty(ref  RCRD_CHANNEL_COUNTValue, value); }
        } 
private  System.String RECORDING_REMARKValue; 
 public System.String RECORDING_REMARK
        {  
            get  
            {  
                return this.RECORDING_REMARKValue;  
            }  

          set { SetProperty(ref  RECORDING_REMARKValue, value); }
        } 
private  System.String RECORD_NUMBERValue; 
 public System.String RECORD_NUMBER
        {  
            get  
            {  
                return this.RECORD_NUMBERValue;  
            }  

          set { SetProperty(ref  RECORD_NUMBERValue, value); }
        } 
private  System.String RECORD_QUALITYValue; 
 public System.String RECORD_QUALITY
        {  
            get  
            {  
                return this.RECORD_QUALITYValue;  
            }  

          set { SetProperty(ref  RECORD_QUALITYValue, value); }
        } 
private  System.String RECORD_TYPEValue; 
 public System.String RECORD_TYPE
        {  
            get  
            {  
                return this.RECORD_TYPEValue;  
            }  

          set { SetProperty(ref  RECORD_TYPEValue, value); }
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
private  System.String SEIS_SHOT_POINT_IDValue; 
 public System.String SEIS_SHOT_POINT_ID
        {  
            get  
            {  
                return this.SEIS_SHOT_POINT_IDValue;  
            }  

          set { SetProperty(ref  SEIS_SHOT_POINT_IDValue, value); }
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
private  System.String TAPE_NUMBERValue; 
 public System.String TAPE_NUMBER
        {  
            get  
            {  
                return this.TAPE_NUMBERValue;  
            }  

          set { SetProperty(ref  TAPE_NUMBERValue, value); }
        } 
private  System.Decimal TIME_DELAYValue; 
 public System.Decimal TIME_DELAY
        {  
            get  
            {  
                return this.TIME_DELAYValue;  
            }  

          set { SetProperty(ref  TIME_DELAYValue, value); }
        } 
private  System.String TIME_DELAY_OUOMValue; 
 public System.String TIME_DELAY_OUOM
        {  
            get  
            {  
                return this.TIME_DELAY_OUOMValue;  
            }  

          set { SetProperty(ref  TIME_DELAY_OUOMValue, value); }
        } 
private  System.Decimal UPHOLE_TIMEValue; 
 public System.Decimal UPHOLE_TIME
        {  
            get  
            {  
                return this.UPHOLE_TIMEValue;  
            }  

          set { SetProperty(ref  UPHOLE_TIMEValue, value); }
        } 
private  System.String UPHOLE_TIME_OUOMValue; 
 public System.String UPHOLE_TIME_OUOM
        {  
            get  
            {  
                return this.UPHOLE_TIME_OUOMValue;  
            }  

          set { SetProperty(ref  UPHOLE_TIME_OUOMValue, value); }
        } 
private  System.Decimal VESSEL_CONFIG_OBS_NOValue; 
 public System.Decimal VESSEL_CONFIG_OBS_NO
        {  
            get  
            {  
                return this.VESSEL_CONFIG_OBS_NOValue;  
            }  

          set { SetProperty(ref  VESSEL_CONFIG_OBS_NOValue, value); }
        } 
private  System.String VESSEL_IDValue; 
 public System.String VESSEL_ID
        {  
            get  
            {  
                return this.VESSEL_IDValue;  
            }  

          set { SetProperty(ref  VESSEL_IDValue, value); }
        } 
private  System.String VESSEL_SF_SUBTYPEValue; 
 public System.String VESSEL_SF_SUBTYPE
        {  
            get  
            {  
                return this.VESSEL_SF_SUBTYPEValue;  
            }  

          set { SetProperty(ref  VESSEL_SF_SUBTYPEValue, value); }
        } 
private  System.Decimal X_OFFSETValue; 
 public System.Decimal X_OFFSET
        {  
            get  
            {  
                return this.X_OFFSETValue;  
            }  

          set { SetProperty(ref  X_OFFSETValue, value); }
        } 
private  System.Decimal Y_OFFSETValue; 
 public System.Decimal Y_OFFSET
        {  
            get  
            {  
                return this.Y_OFFSETValue;  
            }  

          set { SetProperty(ref  Y_OFFSETValue, value); }
        } 
private  System.Decimal Z_OFFSETValue; 
 public System.Decimal Z_OFFSET
        {  
            get  
            {  
                return this.Z_OFFSETValue;  
            }  

          set { SetProperty(ref  Z_OFFSETValue, value); }
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


    public SEIS_RECORD () { }

  }
}


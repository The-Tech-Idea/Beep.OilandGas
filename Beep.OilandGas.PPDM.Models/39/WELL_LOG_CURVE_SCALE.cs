using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_LOG_CURVE_SCALE: Entity,IPPDMEntity

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
private  System.String CURVE_IDValue; 
 public System.String CURVE_ID
        {  
            get  
            {  
                return this.CURVE_IDValue;  
            }  

          set { SetProperty(ref  CURVE_IDValue, value); }
        } 
private  System.String DIGITAL_CURVE_IDValue; 
 public System.String DIGITAL_CURVE_ID
        {  
            get  
            {  
                return this.DIGITAL_CURVE_IDValue;  
            }  

          set { SetProperty(ref  DIGITAL_CURVE_IDValue, value); }
        } 
private  System.String CURVE_SCALE_IDValue; 
 public System.String CURVE_SCALE_ID
        {  
            get  
            {  
                return this.CURVE_SCALE_IDValue;  
            }  

          set { SetProperty(ref  CURVE_SCALE_IDValue, value); }
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
private  System.String BACKUP_CURVE_SCALEValue; 
 public System.String BACKUP_CURVE_SCALE
        {  
            get  
            {  
                return this.BACKUP_CURVE_SCALEValue;  
            }  

          set { SetProperty(ref  BACKUP_CURVE_SCALEValue, value); }
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
private  System.Decimal LEFT_SCALE_VALUEValue; 
 public System.Decimal LEFT_SCALE_VALUE
        {  
            get  
            {  
                return this.LEFT_SCALE_VALUEValue;  
            }  

          set { SetProperty(ref  LEFT_SCALE_VALUEValue, value); }
        } 
private  System.String MATRIX_LITHOLOGY_SETTINGValue; 
 public System.String MATRIX_LITHOLOGY_SETTING
        {  
            get  
            {  
                return this.MATRIX_LITHOLOGY_SETTINGValue;  
            }  

          set { SetProperty(ref  MATRIX_LITHOLOGY_SETTINGValue, value); }
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
private  System.Decimal RIGHT_SCALE_VALUEValue; 
 public System.Decimal RIGHT_SCALE_VALUE
        {  
            get  
            {  
                return this.RIGHT_SCALE_VALUEValue;  
            }  

          set { SetProperty(ref  RIGHT_SCALE_VALUEValue, value); }
        } 
private  System.String SCALE_TRANSFORM_TYPEValue; 
 public System.String SCALE_TRANSFORM_TYPE
        {  
            get  
            {  
                return this.SCALE_TRANSFORM_TYPEValue;  
            }  

          set { SetProperty(ref  SCALE_TRANSFORM_TYPEValue, value); }
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
private  System.String TRACK_NUMValue; 
 public System.String TRACK_NUM
        {  
            get  
            {  
                return this.TRACK_NUMValue;  
            }  

          set { SetProperty(ref  TRACK_NUMValue, value); }
        } 
private  System.Decimal TRACK_WIDTHValue; 
 public System.Decimal TRACK_WIDTH
        {  
            get  
            {  
                return this.TRACK_WIDTHValue;  
            }  

          set { SetProperty(ref  TRACK_WIDTHValue, value); }
        } 
private  System.String TRACK_WIDTH_OUOMValue; 
 public System.String TRACK_WIDTH_OUOM
        {  
            get  
            {  
                return this.TRACK_WIDTH_OUOMValue;  
            }  

          set { SetProperty(ref  TRACK_WIDTH_OUOMValue, value); }
        } 
private  System.String TRACK_WIDTH_UOMValue; 
 public System.String TRACK_WIDTH_UOM
        {  
            get  
            {  
                return this.TRACK_WIDTH_UOMValue;  
            }  

          set { SetProperty(ref  TRACK_WIDTH_UOMValue, value); }
        } 
private  System.String VERTICAL_SCALE_RATIOValue; 
 public System.String VERTICAL_SCALE_RATIO
        {  
            get  
            {  
                return this.VERTICAL_SCALE_RATIOValue;  
            }  

          set { SetProperty(ref  VERTICAL_SCALE_RATIOValue, value); }
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


    public WELL_LOG_CURVE_SCALE () { }

  }
}


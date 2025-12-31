using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_LOG_CURVE_FRAME: Entity,IPPDMEntity

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
private  System.String WELL_LOG_IDValue; 
 public System.String WELL_LOG_ID
        {  
            get  
            {  
                return this.WELL_LOG_IDValue;  
            }  

          set { SetProperty(ref  WELL_LOG_IDValue, value); }
        } 
private  System.String WELL_LOG_SOURCEValue; 
 public System.String WELL_LOG_SOURCE
        {  
            get  
            {  
                return this.WELL_LOG_SOURCEValue;  
            }  

          set { SetProperty(ref  WELL_LOG_SOURCEValue, value); }
        } 
private  System.String FRAME_IDValue; 
 public System.String FRAME_ID
        {  
            get  
            {  
                return this.FRAME_IDValue;  
            }  

          set { SetProperty(ref  FRAME_IDValue, value); }
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
private  System.String DEPTH_OUOMValue; 
 public System.String DEPTH_OUOM
        {  
            get  
            {  
                return this.DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  DEPTH_OUOMValue, value); }
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
private  System.String FRAME_NAMEValue; 
 public System.String FRAME_NAME
        {  
            get  
            {  
                return this.FRAME_NAMEValue;  
            }  

          set { SetProperty(ref  FRAME_NAMEValue, value); }
        } 
private  System.Decimal FRAME_SPACINGValue; 
 public System.Decimal FRAME_SPACING
        {  
            get  
            {  
                return this.FRAME_SPACINGValue;  
            }  

          set { SetProperty(ref  FRAME_SPACINGValue, value); }
        } 
private  System.String FRAME_SPACING_OUOMValue; 
 public System.String FRAME_SPACING_OUOM
        {  
            get  
            {  
                return this.FRAME_SPACING_OUOMValue;  
            }  

          set { SetProperty(ref  FRAME_SPACING_OUOMValue, value); }
        } 
private  System.String FRAME_SPACING_UOMValue; 
 public System.String FRAME_SPACING_UOM
        {  
            get  
            {  
                return this.FRAME_SPACING_UOMValue;  
            }  

          set { SetProperty(ref  FRAME_SPACING_UOMValue, value); }
        } 
private  System.String GAPS_INDValue; 
 public System.String GAPS_IND
        {  
            get  
            {  
                return this.GAPS_INDValue;  
            }  

          set { SetProperty(ref  GAPS_INDValue, value); }
        } 
private  System.String INDEX_OUOMValue; 
 public System.String INDEX_OUOM
        {  
            get  
            {  
                return this.INDEX_OUOMValue;  
            }  

          set { SetProperty(ref  INDEX_OUOMValue, value); }
        } 
private  System.String INDEX_UOMValue; 
 public System.String INDEX_UOM
        {  
            get  
            {  
                return this.INDEX_UOMValue;  
            }  

          set { SetProperty(ref  INDEX_UOMValue, value); }
        } 
private  System.String IRREGULAR_DESCValue; 
 public System.String IRREGULAR_DESC
        {  
            get  
            {  
                return this.IRREGULAR_DESCValue;  
            }  

          set { SetProperty(ref  IRREGULAR_DESCValue, value); }
        } 
private  System.String IRREGULAR_INDValue; 
 public System.String IRREGULAR_IND
        {  
            get  
            {  
                return this.IRREGULAR_INDValue;  
            }  

          set { SetProperty(ref  IRREGULAR_INDValue, value); }
        } 
private  System.String LOG_DIRECTIONValue; 
 public System.String LOG_DIRECTION
        {  
            get  
            {  
                return this.LOG_DIRECTIONValue;  
            }  

          set { SetProperty(ref  LOG_DIRECTIONValue, value); }
        } 
private  System.Decimal MAX_INDEXValue; 
 public System.Decimal MAX_INDEX
        {  
            get  
            {  
                return this.MAX_INDEXValue;  
            }  

          set { SetProperty(ref  MAX_INDEXValue, value); }
        } 
private  System.Decimal MIN_INDEXValue; 
 public System.Decimal MIN_INDEX
        {  
            get  
            {  
                return this.MIN_INDEXValue;  
            }  

          set { SetProperty(ref  MIN_INDEXValue, value); }
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
private  System.String PRIMARY_INDEX_TYPEValue; 
 public System.String PRIMARY_INDEX_TYPE
        {  
            get  
            {  
                return this.PRIMARY_INDEX_TYPEValue;  
            }  

          set { SetProperty(ref  PRIMARY_INDEX_TYPEValue, value); }
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
private  System.Decimal TOP_DEPTHValue; 
 public System.Decimal TOP_DEPTH
        {  
            get  
            {  
                return this.TOP_DEPTHValue;  
            }  

          set { SetProperty(ref  TOP_DEPTHValue, value); }
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


    public WELL_LOG_CURVE_FRAME () { }

  }
}


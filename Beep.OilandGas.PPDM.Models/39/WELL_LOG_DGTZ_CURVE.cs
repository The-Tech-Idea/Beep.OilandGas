using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_LOG_DGTZ_CURVE: Entity,IPPDMEntity

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
private  System.String CURVE_QUALITYValue; 
 public System.String CURVE_QUALITY
        {  
            get  
            {  
                return this.CURVE_QUALITYValue;  
            }  

          set { SetProperty(ref  CURVE_QUALITYValue, value); }
        } 
private  System.String DEPTH_CORRECTION_METHODValue; 
 public System.String DEPTH_CORRECTION_METHOD
        {  
            get  
            {  
                return this.DEPTH_CORRECTION_METHODValue;  
            }  

          set { SetProperty(ref  DEPTH_CORRECTION_METHODValue, value); }
        } 
private  System.Decimal DEPTH_INCREMENTValue; 
 public System.Decimal DEPTH_INCREMENT
        {  
            get  
            {  
                return this.DEPTH_INCREMENTValue;  
            }  

          set { SetProperty(ref  DEPTH_INCREMENTValue, value); }
        } 
private  System.String DEPTH_INCREMENT_OUOMValue; 
 public System.String DEPTH_INCREMENT_OUOM
        {  
            get  
            {  
                return this.DEPTH_INCREMENT_OUOMValue;  
            }  

          set { SetProperty(ref  DEPTH_INCREMENT_OUOMValue, value); }
        } 
private  System.DateTime? DIGITIZED_DATEValue; 
 public System.DateTime? DIGITIZED_DATE
        {  
            get  
            {  
                return this.DIGITIZED_DATEValue;  
            }  

          set { SetProperty(ref  DIGITIZED_DATEValue, value); }
        } 
private  System.String DIGITIZERValue; 
 public System.String DIGITIZER
        {  
            get  
            {  
                return this.DIGITIZERValue;  
            }  

          set { SetProperty(ref  DIGITIZERValue, value); }
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
private  System.Decimal NULL_VALUEValue; 
 public System.Decimal NULL_VALUE
        {  
            get  
            {  
                return this.NULL_VALUEValue;  
            }  

          set { SetProperty(ref  NULL_VALUEValue, value); }
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


    public WELL_LOG_DGTZ_CURVE () { }

  }
}


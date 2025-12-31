using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_DRILL_PERIOD_VESSEL: Entity,IPPDMEntity

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
private  System.Decimal PERIOD_OBS_NOValue; 
 public System.Decimal PERIOD_OBS_NO
        {  
            get  
            {  
                return this.PERIOD_OBS_NOValue;  
            }  

          set { SetProperty(ref  PERIOD_OBS_NOValue, value); }
        } 
private  System.String SF_SUBTYPEValue; 
 public System.String SF_SUBTYPE
        {  
            get  
            {  
                return this.SF_SUBTYPEValue;  
            }  

          set { SetProperty(ref  SF_SUBTYPEValue, value); }
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
private  System.String ACTIVE_INDValue; 
 public System.String ACTIVE_IND
        {  
            get  
            {  
                return this.ACTIVE_INDValue;  
            }  

          set { SetProperty(ref  ACTIVE_INDValue, value); }
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
private  System.Decimal HEADINGValue; 
 public System.Decimal HEADING
        {  
            get  
            {  
                return this.HEADINGValue;  
            }  

          set { SetProperty(ref  HEADINGValue, value); }
        } 
private  System.String HEADING_NORTH_TYPEValue; 
 public System.String HEADING_NORTH_TYPE
        {  
            get  
            {  
                return this.HEADING_NORTH_TYPEValue;  
            }  

          set { SetProperty(ref  HEADING_NORTH_TYPEValue, value); }
        } 
private  System.Decimal PASSENGERS_OFFValue; 
 public System.Decimal PASSENGERS_OFF
        {  
            get  
            {  
                return this.PASSENGERS_OFFValue;  
            }  

          set { SetProperty(ref  PASSENGERS_OFFValue, value); }
        } 
private  System.Decimal PASSENGERS_ONValue; 
 public System.Decimal PASSENGERS_ON
        {  
            get  
            {  
                return this.PASSENGERS_ONValue;  
            }  

          set { SetProperty(ref  PASSENGERS_ONValue, value); }
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
private  System.Decimal RISER_ANGLEValue; 
 public System.Decimal RISER_ANGLE
        {  
            get  
            {  
                return this.RISER_ANGLEValue;  
            }  

          set { SetProperty(ref  RISER_ANGLEValue, value); }
        } 
private  System.Decimal RISER_TENSIONValue; 
 public System.Decimal RISER_TENSION
        {  
            get  
            {  
                return this.RISER_TENSIONValue;  
            }  

          set { SetProperty(ref  RISER_TENSIONValue, value); }
        } 
private  System.String RISER_TENSION_OUOMValue; 
 public System.String RISER_TENSION_OUOM
        {  
            get  
            {  
                return this.RISER_TENSION_OUOMValue;  
            }  

          set { SetProperty(ref  RISER_TENSION_OUOMValue, value); }
        } 
private  System.String RISER_TENSION_UOMValue; 
 public System.String RISER_TENSION_UOM
        {  
            get  
            {  
                return this.RISER_TENSION_UOMValue;  
            }  

          set { SetProperty(ref  RISER_TENSION_UOMValue, value); }
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
private  System.String VESSEL_ROLEValue; 
 public System.String VESSEL_ROLE
        {  
            get  
            {  
                return this.VESSEL_ROLEValue;  
            }  

          set { SetProperty(ref  VESSEL_ROLEValue, value); }
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


    public WELL_DRILL_PERIOD_VESSEL () { }

  }
}


using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_DRILL_MUD_WEIGHT: Entity,IPPDMEntity

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
private  System.String SOURCEValue; 
 public System.String SOURCE
        {  
            get  
            {  
                return this.SOURCEValue;  
            }  

          set { SetProperty(ref  SOURCEValue, value); }
        } 
private  System.Decimal DEPTH_OBS_NOValue; 
 public System.Decimal DEPTH_OBS_NO
        {  
            get  
            {  
                return this.DEPTH_OBS_NOValue;  
            }  

          set { SetProperty(ref  DEPTH_OBS_NOValue, value); }
        } 
private  System.Decimal MEDIA_OBS_NOValue; 
 public System.Decimal MEDIA_OBS_NO
        {  
            get  
            {  
                return this.MEDIA_OBS_NOValue;  
            }  

          set { SetProperty(ref  MEDIA_OBS_NOValue, value); }
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
private  System.DateTime? END_DATEValue; 
 public System.DateTime? END_DATE
        {  
            get  
            {  
                return this.END_DATEValue;  
            }  

          set { SetProperty(ref  END_DATEValue, value); }
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
private  System.Decimal MUD_DEPTHValue; 
 public System.Decimal MUD_DEPTH
        {  
            get  
            {  
                return this.MUD_DEPTHValue;  
            }  

          set { SetProperty(ref  MUD_DEPTHValue, value); }
        } 
private  System.String MUD_DEPTH_OUOMValue; 
 public System.String MUD_DEPTH_OUOM
        {  
            get  
            {  
                return this.MUD_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  MUD_DEPTH_OUOMValue, value); }
        } 
private  System.Decimal MUD_WEIGHTValue; 
 public System.Decimal MUD_WEIGHT
        {  
            get  
            {  
                return this.MUD_WEIGHTValue;  
            }  

          set { SetProperty(ref  MUD_WEIGHTValue, value); }
        } 
private  System.String MUD_WEIGHT_OUOMValue; 
 public System.String MUD_WEIGHT_OUOM
        {  
            get  
            {  
                return this.MUD_WEIGHT_OUOMValue;  
            }  

          set { SetProperty(ref  MUD_WEIGHT_OUOMValue, value); }
        } 
private  System.String MUD_WEIGHT_UOMValue; 
 public System.String MUD_WEIGHT_UOM
        {  
            get  
            {  
                return this.MUD_WEIGHT_UOMValue;  
            }  

          set { SetProperty(ref  MUD_WEIGHT_UOMValue, value); }
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
private  System.Decimal REPORTED_TVDValue; 
 public System.Decimal REPORTED_TVD
        {  
            get  
            {  
                return this.REPORTED_TVDValue;  
            }  

          set { SetProperty(ref  REPORTED_TVDValue, value); }
        } 
private  System.String REPORTED_TVD_OUOMValue; 
 public System.String REPORTED_TVD_OUOM
        {  
            get  
            {  
                return this.REPORTED_TVD_OUOMValue;  
            }  

          set { SetProperty(ref  REPORTED_TVD_OUOMValue, value); }
        } 
private  System.DateTime? START_DATEValue; 
 public System.DateTime? START_DATE
        {  
            get  
            {  
                return this.START_DATEValue;  
            }  

          set { SetProperty(ref  START_DATEValue, value); }
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


    public WELL_DRILL_MUD_WEIGHT () { }

  }
}


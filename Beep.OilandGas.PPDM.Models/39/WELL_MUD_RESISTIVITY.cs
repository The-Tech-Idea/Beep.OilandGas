using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_MUD_RESISTIVITY: Entity,IPPDMEntity

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
private  System.String SAMPLE_IDValue; 
 public System.String SAMPLE_ID
        {  
            get  
            {  
                return this.SAMPLE_IDValue;  
            }  

          set { SetProperty(ref  SAMPLE_IDValue, value); }
        } 
private  System.Decimal RESISTIVITY_OBS_NOValue; 
 public System.Decimal RESISTIVITY_OBS_NO
        {  
            get  
            {  
                return this.RESISTIVITY_OBS_NOValue;  
            }  

          set { SetProperty(ref  RESISTIVITY_OBS_NOValue, value); }
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
private  System.Decimal MUD_RESISTIVITYValue; 
 public System.Decimal MUD_RESISTIVITY
        {  
            get  
            {  
                return this.MUD_RESISTIVITYValue;  
            }  

          set { SetProperty(ref  MUD_RESISTIVITYValue, value); }
        } 
private  System.String MUD_RESISTIVITY_OUOMValue; 
 public System.String MUD_RESISTIVITY_OUOM
        {  
            get  
            {  
                return this.MUD_RESISTIVITY_OUOMValue;  
            }  

          set { SetProperty(ref  MUD_RESISTIVITY_OUOMValue, value); }
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
private  System.Decimal RESISTIVITY_TEMPERATUREValue; 
 public System.Decimal RESISTIVITY_TEMPERATURE
        {  
            get  
            {  
                return this.RESISTIVITY_TEMPERATUREValue;  
            }  

          set { SetProperty(ref  RESISTIVITY_TEMPERATUREValue, value); }
        } 
private  System.String RESISTIVITY_TEMPERATURE_OUOMValue; 
 public System.String RESISTIVITY_TEMPERATURE_OUOM
        {  
            get  
            {  
                return this.RESISTIVITY_TEMPERATURE_OUOMValue;  
            }  

          set { SetProperty(ref  RESISTIVITY_TEMPERATURE_OUOMValue, value); }
        } 
private  System.String SAMPLE_TYPEValue; 
 public System.String SAMPLE_TYPE
        {  
            get  
            {  
                return this.SAMPLE_TYPEValue;  
            }  

          set { SetProperty(ref  SAMPLE_TYPEValue, value); }
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


    public WELL_MUD_RESISTIVITY () { }

  }
}


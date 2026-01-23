using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
 namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
public partial class PIPELINE_INCIDENT : ModelEntityBase {

private  System.String PIPELINE_IDValue; 
 public System.String PIPELINE_ID
        {  
            get  
            {  
                return this.PIPELINE_IDValue;  
            }  

          set { SetProperty(ref  PIPELINE_IDValue, value); }
        } 
private  System.String INCIDENT_IDValue; 
 public System.String INCIDENT_ID
        {  
            get  
            {  
                return this.INCIDENT_IDValue;  
            }  

          set { SetProperty(ref  INCIDENT_IDValue, value); }
        } 
private  System.DateTime? INCIDENT_DATEValue; 
 public System.DateTime? INCIDENT_DATE
        {  
            get  
            {  
                return this.INCIDENT_DATEValue;  
            }  

          set { SetProperty(ref  INCIDENT_DATEValue, value); }
        } 
private  System.String INCIDENT_TYPEValue; 
 public System.String INCIDENT_TYPE
        {  
            get  
            {  
                return this.INCIDENT_TYPEValue;  
            }  

          set { SetProperty(ref  INCIDENT_TYPEValue, value); }
        } 
private  System.String SEVERITYValue; 
 public System.String SEVERITY
        {  
            get  
            {  
                return this.SEVERITYValue;  
            }  

          set { SetProperty(ref  SEVERITYValue, value); }
        } 
private  System.String CAUSEValue; 
 public System.String CAUSE
        {  
            get  
            {  
                return this.CAUSEValue;  
            }  

          set { SetProperty(ref  CAUSEValue, value); }
        } 
private  System.Decimal VOLUME_RELEASEDValue; 
 public System.Decimal VOLUME_RELEASED
        {  
            get  
            {  
                return this.VOLUME_RELEASEDValue;  
            }  

          set { SetProperty(ref  VOLUME_RELEASEDValue, value); }
        } 
private  System.String VOLUME_RELEASED_OUOMValue; 
 public System.String VOLUME_RELEASED_OUOM
        {  
            get  
            {  
                return this.VOLUME_RELEASED_OUOMValue;  
            }  

          set { SetProperty(ref  VOLUME_RELEASED_OUOMValue, value); }
        } 
private  System.String ENVIRONMENTAL_IMPACTValue; 
 public System.String ENVIRONMENTAL_IMPACT
        {  
            get  
            {  
                return this.ENVIRONMENTAL_IMPACTValue;  
            }  

          set { SetProperty(ref  ENVIRONMENTAL_IMPACTValue, value); }
        } 
private  System.String DESCRIPTIONValue; 
 public System.String DESCRIPTION
        {  
            get  
            {  
                return this.DESCRIPTIONValue;  
            }  

          set { SetProperty(ref  DESCRIPTIONValue, value); }
        } 
private  System.DateTime? EFFECTIVE_DATEValue; 

private  System.DateTime? EXPIRY_DATEValue; 

private  System.String REMARKValue; 

private  System.String SOURCEValue; 

    public PIPELINE_INCIDENT () { }

  }
}



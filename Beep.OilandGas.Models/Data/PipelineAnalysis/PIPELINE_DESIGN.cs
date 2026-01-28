using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
public partial class PIPELINE_DESIGN : ModelEntityBase {

private  System.String PIPELINE_IDValue; 
 public System.String PIPELINE_ID
        {  
            get  
            {  
                return this.PIPELINE_IDValue;  
            }  

          set { SetProperty(ref  PIPELINE_IDValue, value); }
        } 
private  System.String DESIGN_VERSION_NOValue; 
 public System.String DESIGN_VERSION_NO
        {  
            get  
            {  
                return this.DESIGN_VERSION_NOValue;  
            }  

          set { SetProperty(ref  DESIGN_VERSION_NOValue, value); }
        } 
private  System.Decimal DESIGN_PRESSUREValue; 
 public System.Decimal DESIGN_PRESSURE
        {  
            get  
            {  
                return this.DESIGN_PRESSUREValue;  
            }  

          set { SetProperty(ref  DESIGN_PRESSUREValue, value); }
        } 
private  System.String DESIGN_PRESSURE_OUOMValue; 
 public System.String DESIGN_PRESSURE_OUOM
        {  
            get  
            {  
                return this.DESIGN_PRESSURE_OUOMValue;  
            }  

          set { SetProperty(ref  DESIGN_PRESSURE_OUOMValue, value); }
        } 
private  System.Decimal DESIGN_TEMPERATUREValue; 
 public System.Decimal DESIGN_TEMPERATURE
        {  
            get  
            {  
                return this.DESIGN_TEMPERATUREValue;  
            }  

          set { SetProperty(ref  DESIGN_TEMPERATUREValue, value); }
        } 
private  System.String DESIGN_TEMPERATURE_OUOMValue; 
 public System.String DESIGN_TEMPERATURE_OUOM
        {  
            get  
            {  
                return this.DESIGN_TEMPERATURE_OUOMValue;  
            }  

          set { SetProperty(ref  DESIGN_TEMPERATURE_OUOMValue, value); }
        } 
private  System.String MATERIAL_GRADEValue; 
 public System.String MATERIAL_GRADE
        {  
            get  
            {  
                return this.MATERIAL_GRADEValue;  
            }  

          set { SetProperty(ref  MATERIAL_GRADEValue, value); }
        } 
private  System.String COATING_TYPEValue; 
 public System.String COATING_TYPE
        {  
            get  
            {  
                return this.COATING_TYPEValue;  
            }  

          set { SetProperty(ref  COATING_TYPEValue, value); }
        } 
private  System.String CATHODIC_PROTECTION_TYPEValue; 
 public System.String CATHODIC_PROTECTION_TYPE
        {  
            get  
            {  
                return this.CATHODIC_PROTECTION_TYPEValue;  
            }  

          set { SetProperty(ref  CATHODIC_PROTECTION_TYPEValue, value); }
        } 
private  System.String CP_SYSTEM_TYPEValue; 
 public System.String CP_SYSTEM_TYPE
        {  
            get  
            {  
                return this.CP_SYSTEM_TYPEValue;  
            }  

          set { SetProperty(ref  CP_SYSTEM_TYPEValue, value); }
        } 
private  System.String DESIGN_SPECIFICATIONValue; 
 public System.String DESIGN_SPECIFICATION
        {  
            get  
            {  
                return this.DESIGN_SPECIFICATIONValue;  
            }  

          set { SetProperty(ref  DESIGN_SPECIFICATIONValue, value); }
        } 
private  System.String DESIGN_STANDARDValue; 
 public System.String DESIGN_STANDARD
        {  
            get  
            {  
                return this.DESIGN_STANDARDValue;  
            }  

          set { SetProperty(ref  DESIGN_STANDARDValue, value); }
        } 
private  System.DateTime? DESIGN_DATEValue; 
 public System.DateTime? DESIGN_DATE
        {  
            get  
            {  
                return this.DESIGN_DATEValue;  
            }  

          set { SetProperty(ref  DESIGN_DATEValue, value); }
        } 
private  System.String DESIGNERValue; 
 public System.String DESIGNER
        {  
            get  
            {  
                return this.DESIGNERValue;  
            }  

          set { SetProperty(ref  DESIGNERValue, value); }
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

    public PIPELINE_DESIGN () { }

  }
}

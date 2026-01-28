using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
public partial class PIPELINE_MATERIAL : ModelEntityBase {

private  System.String PIPELINE_IDValue; 
 public System.String PIPELINE_ID
        {  
            get  
            {  
                return this.PIPELINE_IDValue;  
            }  

          set { SetProperty(ref  PIPELINE_IDValue, value); }
        } 
private  System.String MATERIAL_SEQ_NOValue; 
 public System.String MATERIAL_SEQ_NO
        {  
            get  
            {  
                return this.MATERIAL_SEQ_NOValue;  
            }  

          set { SetProperty(ref  MATERIAL_SEQ_NOValue, value); }
        } 
private  System.String MATERIAL_TYPEValue; 
 public System.String MATERIAL_TYPE
        {  
            get  
            {  
                return this.MATERIAL_TYPEValue;  
            }  

          set { SetProperty(ref  MATERIAL_TYPEValue, value); }
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
private  System.String MATERIAL_SPECIFICATIONValue; 
 public System.String MATERIAL_SPECIFICATION
        {  
            get  
            {  
                return this.MATERIAL_SPECIFICATIONValue;  
            }  

          set { SetProperty(ref  MATERIAL_SPECIFICATIONValue, value); }
        } 
private  System.String MATERIAL_STANDARDValue; 
 public System.String MATERIAL_STANDARD
        {  
            get  
            {  
                return this.MATERIAL_STANDARDValue;  
            }  

          set { SetProperty(ref  MATERIAL_STANDARDValue, value); }
        } 
private  System.String MANUFACTURERValue; 
 public System.String MANUFACTURER
        {  
            get  
            {  
                return this.MANUFACTURERValue;  
            }  

          set { SetProperty(ref  MANUFACTURERValue, value); }
        } 
private  System.String HEAT_NUMBERValue; 
 public System.String HEAT_NUMBER
        {  
            get  
            {  
                return this.HEAT_NUMBERValue;  
            }  

          set { SetProperty(ref  HEAT_NUMBERValue, value); }
        } 
private  System.String BATCH_NUMBERValue; 
 public System.String BATCH_NUMBER
        {  
            get  
            {  
                return this.BATCH_NUMBERValue;  
            }  

          set { SetProperty(ref  BATCH_NUMBERValue, value); }
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

    public PIPELINE_MATERIAL () { }

  }
}

using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
 namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
public partial class PIPELINE_COMPONENT : ModelEntityBase {

private  System.String PIPELINE_IDValue; 
 public System.String PIPELINE_ID
        {  
            get  
            {  
                return this.PIPELINE_IDValue;  
            }  

          set { SetProperty(ref  PIPELINE_IDValue, value); }
        } 
private  System.String COMPONENT_IDValue; 
 public System.String COMPONENT_ID
        {  
            get  
            {  
                return this.COMPONENT_IDValue;  
            }  

          set { SetProperty(ref  COMPONENT_IDValue, value); }
        } 
private  System.String COMPONENT_TYPEValue; 
 public System.String COMPONENT_TYPE
        {  
            get  
            {  
                return this.COMPONENT_TYPEValue;  
            }  

          set { SetProperty(ref  COMPONENT_TYPEValue, value); }
        } 
private  System.String PART_NUMBERValue; 
 public System.String PART_NUMBER
        {  
            get  
            {  
                return this.PART_NUMBERValue;  
            }  

          set { SetProperty(ref  PART_NUMBERValue, value); }
        } 
private  System.String SEGMENT_IDValue; 
 public System.String SEGMENT_ID
        {  
            get  
            {  
                return this.SEGMENT_IDValue;  
            }  

          set { SetProperty(ref  SEGMENT_IDValue, value); }
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
private  System.String MANUFACTURERValue; 
 public System.String MANUFACTURER
        {  
            get  
            {  
                return this.MANUFACTURERValue;  
            }  

          set { SetProperty(ref  MANUFACTURERValue, value); }
        } 
private  System.DateTime? INSTALLATION_DATEValue; 
 public System.DateTime? INSTALLATION_DATE
        {  
            get  
            {  
                return this.INSTALLATION_DATEValue;  
            }  

          set { SetProperty(ref  INSTALLATION_DATEValue, value); }
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

    public PIPELINE_COMPONENT () { }

  }
}



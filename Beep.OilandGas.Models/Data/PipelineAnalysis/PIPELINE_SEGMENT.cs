using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
public partial class PIPELINE_SEGMENT : ModelEntityBase {

private  System.String PIPELINE_IDValue; 
 public System.String PIPELINE_ID
        {  
            get  
            {  
                return this.PIPELINE_IDValue;  
            }  

          set { SetProperty(ref  PIPELINE_IDValue, value); }
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
private  System.String SEGMENT_NAMEValue; 
 public System.String SEGMENT_NAME
        {  
            get  
            {  
                return this.SEGMENT_NAMEValue;  
            }  

          set { SetProperty(ref  SEGMENT_NAMEValue, value); }
        } 
private  System.String SEGMENT_SHORT_NAMEValue; 
 public System.String SEGMENT_SHORT_NAME
        {  
            get  
            {  
                return this.SEGMENT_SHORT_NAMEValue;  
            }  

          set { SetProperty(ref  SEGMENT_SHORT_NAMEValue, value); }
        } 
private  System.String START_STATION_IDValue; 
 public System.String START_STATION_ID
        {  
            get  
            {  
                return this.START_STATION_IDValue;  
            }  

          set { SetProperty(ref  START_STATION_IDValue, value); }
        } 
private  System.String START_STATION_TYPEValue; 
 public System.String START_STATION_TYPE
        {  
            get  
            {  
                return this.START_STATION_TYPEValue;  
            }  

          set { SetProperty(ref  START_STATION_TYPEValue, value); }
        } 
private  System.String END_STATION_IDValue; 
 public System.String END_STATION_ID
        {  
            get  
            {  
                return this.END_STATION_IDValue;  
            }  

          set { SetProperty(ref  END_STATION_IDValue, value); }
        } 
private  System.String END_STATION_TYPEValue; 
 public System.String END_STATION_TYPE
        {  
            get  
            {  
                return this.END_STATION_TYPEValue;  
            }  

          set { SetProperty(ref  END_STATION_TYPEValue, value); }
        } 
private  System.Decimal SEGMENT_LENGTHValue; 
 public System.Decimal SEGMENT_LENGTH
        {  
            get  
            {  
                return this.SEGMENT_LENGTHValue;  
            }  

          set { SetProperty(ref  SEGMENT_LENGTHValue, value); }
        } 
private  System.String SEGMENT_LENGTH_OUOMValue; 
 public System.String SEGMENT_LENGTH_OUOM
        {  
            get  
            {  
                return this.SEGMENT_LENGTH_OUOMValue;  
            }  

          set { SetProperty(ref  SEGMENT_LENGTH_OUOMValue, value); }
        } 
private  System.Decimal SEGMENT_DIAMETERValue; 
 public System.Decimal SEGMENT_DIAMETER
        {  
            get  
            {  
                return this.SEGMENT_DIAMETERValue;  
            }  

          set { SetProperty(ref  SEGMENT_DIAMETERValue, value); }
        } 
private  System.String SEGMENT_DIAMETER_OUOMValue; 
 public System.String SEGMENT_DIAMETER_OUOM
        {  
            get  
            {  
                return this.SEGMENT_DIAMETER_OUOMValue;  
            }  

          set { SetProperty(ref  SEGMENT_DIAMETER_OUOMValue, value); }
        } 
private  System.Decimal WALL_THICKNESSValue; 
 public System.Decimal WALL_THICKNESS
        {  
            get  
            {  
                return this.WALL_THICKNESSValue;  
            }  

          set { SetProperty(ref  WALL_THICKNESSValue, value); }
        } 
private  System.String WALL_THICKNESS_OUOMValue; 
 public System.String WALL_THICKNESS_OUOM
        {  
            get  
            {  
                return this.WALL_THICKNESS_OUOMValue;  
            }  

          set { SetProperty(ref  WALL_THICKNESS_OUOMValue, value); }
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
private  System.String DESCRIPTIONValue; 
 public System.String DESCRIPTION
        {  
            get  
            {  
                return this.DESCRIPTIONValue;  
            }  

          set { SetProperty(ref  DESCRIPTIONValue, value); }
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
private  System.DateTime? EFFECTIVE_DATEValue; 

private  System.DateTime? EXPIRY_DATEValue; 

private  System.String REMARKValue; 

private  System.String SOURCEValue; 

    public PIPELINE_SEGMENT () { }

  }
}

using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
public partial class PIPELINE_SEGMENT: Entity,IPPDMEntity

{

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


    public PIPELINE_SEGMENT () { }

  }
}




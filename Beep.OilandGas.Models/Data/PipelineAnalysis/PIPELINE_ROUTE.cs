using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
 namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
public partial class PIPELINE_ROUTE : ModelEntityBase {

private  System.String PIPELINE_IDValue; 
 public System.String PIPELINE_ID
        {  
            get  
            {  
                return this.PIPELINE_IDValue;  
            }  

          set { SetProperty(ref  PIPELINE_IDValue, value); }
        } 
private  System.Decimal ROUTE_SEQ_NOValue; 
 public System.Decimal ROUTE_SEQ_NO
        {  
            get  
            {  
                return this.ROUTE_SEQ_NOValue;  
            }  

          set { SetProperty(ref  ROUTE_SEQ_NOValue, value); }
        } 
private  System.Decimal LATITUDEValue; 
 public System.Decimal LATITUDE
        {  
            get  
            {  
                return this.LATITUDEValue;  
            }  

          set { SetProperty(ref  LATITUDEValue, value); }
        } 
private  System.Decimal LONGITUDEValue; 
 public System.Decimal LONGITUDE
        {  
            get  
            {  
                return this.LONGITUDEValue;  
            }  

          set { SetProperty(ref  LONGITUDEValue, value); }
        } 
private  System.Decimal ELEVATIONValue; 
 public System.Decimal ELEVATION
        {  
            get  
            {  
                return this.ELEVATIONValue;  
            }  

          set { SetProperty(ref  ELEVATIONValue, value); }
        } 
private  System.String ELEVATION_OUOMValue; 
 public System.String ELEVATION_OUOM
        {  
            get  
            {  
                return this.ELEVATION_OUOMValue;  
            }  

          set { SetProperty(ref  ELEVATION_OUOMValue, value); }
        } 
private  System.Decimal STATION_MILEPOSTValue; 
 public System.Decimal STATION_MILEPOST
        {  
            get  
            {  
                return this.STATION_MILEPOSTValue;  
            }  

          set { SetProperty(ref  STATION_MILEPOSTValue, value); }
        } 
private  System.String STATION_MILEPOST_OUOMValue; 
 public System.String STATION_MILEPOST_OUOM
        {  
            get  
            {  
                return this.STATION_MILEPOST_OUOMValue;  
            }  

          set { SetProperty(ref  STATION_MILEPOST_OUOMValue, value); }
        } 
private  System.Decimal LINEAR_REFERENCEValue; 
 public System.Decimal LINEAR_REFERENCE
        {  
            get  
            {  
                return this.LINEAR_REFERENCEValue;  
            }  

          set { SetProperty(ref  LINEAR_REFERENCEValue, value); }
        } 
private  System.String COORD_SYSTEM_IDValue; 
 public System.String COORD_SYSTEM_ID
        {  
            get  
            {  
                return this.COORD_SYSTEM_IDValue;  
            }  

          set { SetProperty(ref  COORD_SYSTEM_IDValue, value); }
        } 
private  System.String LOCAL_COORD_SYSTEM_IDValue; 
 public System.String LOCAL_COORD_SYSTEM_ID
        {  
            get  
            {  
                return this.LOCAL_COORD_SYSTEM_IDValue;  
            }  

          set { SetProperty(ref  LOCAL_COORD_SYSTEM_IDValue, value); }
        } 
private  System.String COORD_ACQUISITION_IDValue; 
 public System.String COORD_ACQUISITION_ID
        {  
            get  
            {  
                return this.COORD_ACQUISITION_IDValue;  
            }  

          set { SetProperty(ref  COORD_ACQUISITION_IDValue, value); }
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

    public PIPELINE_ROUTE () { }

  }
}



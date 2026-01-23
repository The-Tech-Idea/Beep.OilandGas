using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
 namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
public partial class PROSPECT_SOURCE_ROCK : ModelEntityBase

{

private  System.String PROSPECT_IDValue; 
 public System.String PROSPECT_ID
        {  
            get  
            {  
                return this.PROSPECT_IDValue;  
            }  

          set { SetProperty(ref  PROSPECT_IDValue, value); }
        } 
private  System.String SOURCE_IDValue; 
 public System.String SOURCE_ID
        {  
            get  
            {  
                return this.SOURCE_IDValue;  
            }  

          set { SetProperty(ref  SOURCE_IDValue, value); }
        } 
private  System.String STRAT_UNIT_IDValue; 
 public System.String STRAT_UNIT_ID
        {  
            get  
            {  
                return this.STRAT_UNIT_IDValue;  
            }  

          set { SetProperty(ref  STRAT_UNIT_IDValue, value); }
        } 
private  System.Decimal? TOCValue; 
 public System.Decimal? TOC
        {  
            get  
            {  
                return this.TOCValue;  
            }  

          set { SetProperty(ref  TOCValue, value); }
        } 
private  System.Decimal? HIValue; 
 public System.Decimal? HI
        {  
            get  
            {  
                return this.HIValue;  
            }  

          set { SetProperty(ref  HIValue, value); }
        } 
private  System.String KEROGEN_TYPEValue; 
 public System.String KEROGEN_TYPE
        {  
            get  
            {  
                return this.KEROGEN_TYPEValue;  
            }  

          set { SetProperty(ref  KEROGEN_TYPEValue, value); }
        } 
private  System.Decimal? VITRINITE_REFLECTANCEValue; 
 public System.Decimal? VITRINITE_REFLECTANCE
        {  
            get  
            {  
                return this.VITRINITE_REFLECTANCEValue;  
            }  

          set { SetProperty(ref  VITRINITE_REFLECTANCEValue, value); }
        } 
private  System.Decimal? TMAXValue; 
 public System.Decimal? TMAX
        {  
            get  
            {  
                return this.TMAXValue;  
            }  

          set { SetProperty(ref  TMAXValue, value); }
        } 
private  System.String MATURITY_STAGEValue; 
 public System.String MATURITY_STAGE
        {  
            get  
            {  
                return this.MATURITY_STAGEValue;  
            }  

          set { SetProperty(ref  MATURITY_STAGEValue, value); }
        } 
private  System.Decimal? GENERATION_POTENTIALValue; 
 public System.Decimal? GENERATION_POTENTIAL
        {  
            get  
            {  
                return this.GENERATION_POTENTIALValue;  
            }  

          set { SetProperty(ref  GENERATION_POTENTIALValue, value); }
        } 
private  System.Decimal? EXPULSION_EFFICIENCYValue; 
 public System.Decimal? EXPULSION_EFFICIENCY
        {  
            get  
            {  
                return this.EXPULSION_EFFICIENCYValue;  
            }  

          set { SetProperty(ref  EXPULSION_EFFICIENCYValue, value); }
        } 
private  System.Decimal? MIGRATION_DISTANCEValue; 
 public System.Decimal? MIGRATION_DISTANCE
        {  
            get  
            {  
                return this.MIGRATION_DISTANCEValue;  
            }  

          set { SetProperty(ref  MIGRATION_DISTANCEValue, value); }
        } 
private  System.String MIGRATION_DISTANCE_OUOMValue; 
 public System.String MIGRATION_DISTANCE_OUOM
        {  
            get  
            {  
                return this.MIGRATION_DISTANCE_OUOMValue;  
            }  

          set { SetProperty(ref  MIGRATION_DISTANCE_OUOMValue, value); }
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

    public PROSPECT_SOURCE_ROCK () { }

  }
}



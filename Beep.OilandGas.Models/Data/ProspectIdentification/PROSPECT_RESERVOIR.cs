using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
public partial class PROSPECT_RESERVOIR : ModelEntityBase

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
private  System.String RESERVOIR_IDValue; 
 public System.String RESERVOIR_ID
        {  
            get  
            {  
                return this.RESERVOIR_IDValue;  
            }  

          set { SetProperty(ref  RESERVOIR_IDValue, value); }
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
private  System.Decimal? POROSITYValue; 
 public System.Decimal? POROSITY
        {  
            get  
            {  
                return this.POROSITYValue;  
            }  

          set { SetProperty(ref  POROSITYValue, value); }
        } 
private  System.Decimal? PERMEABILITYValue; 
 public System.Decimal? PERMEABILITY
        {  
            get  
            {  
                return this.PERMEABILITYValue;  
            }  

          set { SetProperty(ref  PERMEABILITYValue, value); }
        } 
private  System.Decimal? NET_PAYValue; 
 public System.Decimal? NET_PAY
        {  
            get  
            {  
                return this.NET_PAYValue;  
            }  

          set { SetProperty(ref  NET_PAYValue, value); }
        } 
private  System.String NET_PAY_OUOMValue; 
 public System.String NET_PAY_OUOM
        {  
            get  
            {  
                return this.NET_PAY_OUOMValue;  
            }  

          set { SetProperty(ref  NET_PAY_OUOMValue, value); }
        } 
private  System.Decimal? GROSS_PAYValue; 
 public System.Decimal? GROSS_PAY
        {  
            get  
            {  
                return this.GROSS_PAYValue;  
            }  

          set { SetProperty(ref  GROSS_PAYValue, value); }
        } 
private  System.String GROSS_PAY_OUOMValue; 
 public System.String GROSS_PAY_OUOM
        {  
            get  
            {  
                return this.GROSS_PAY_OUOMValue;  
            }  

          set { SetProperty(ref  GROSS_PAY_OUOMValue, value); }
        } 
private  System.Decimal? NET_TO_GROSS_RATIOValue; 
 public System.Decimal? NET_TO_GROSS_RATIO
        {  
            get  
            {  
                return this.NET_TO_GROSS_RATIOValue;  
            }  

          set { SetProperty(ref  NET_TO_GROSS_RATIOValue, value); }
        } 
private  System.Decimal? OWCValue; 
 public System.Decimal? OWC
        {  
            get  
            {  
                return this.OWCValue;  
            }  

          set { SetProperty(ref  OWCValue, value); }
        } 
private  System.String OWC_OUOMValue; 
 public System.String OWC_OUOM
        {  
            get  
            {  
                return this.OWC_OUOMValue;  
            }  

          set { SetProperty(ref  OWC_OUOMValue, value); }
        } 
private  System.Decimal? GOCValue; 
 public System.Decimal? GOC
        {  
            get  
            {  
                return this.GOCValue;  
            }  

          set { SetProperty(ref  GOCValue, value); }
        } 
private  System.String GOC_OUOMValue; 
 public System.String GOC_OUOM
        {  
            get  
            {  
                return this.GOC_OUOMValue;  
            }  

          set { SetProperty(ref  GOC_OUOMValue, value); }
        } 
private  System.Decimal? FWLValue; 
 public System.Decimal? FWL
        {  
            get  
            {  
                return this.FWLValue;  
            }  

          set { SetProperty(ref  FWLValue, value); }
        } 
private  System.String FWL_OUOMValue; 
 public System.String FWL_OUOM
        {  
            get  
            {  
                return this.FWL_OUOMValue;  
            }  

          set { SetProperty(ref  FWL_OUOMValue, value); }
        } 
private  System.String FACIESValue; 
 public System.String FACIES
        {  
            get  
            {  
                return this.FACIESValue;  
            }  

          set { SetProperty(ref  FACIESValue, value); }
        } 
private  System.String LITHOLOGYValue; 
 public System.String LITHOLOGY
        {  
            get  
            {  
                return this.LITHOLOGYValue;  
            }  

          set { SetProperty(ref  LITHOLOGYValue, value); }
        } 
private  System.String DIAGENESISValue; 
 public System.String DIAGENESIS
        {  
            get  
            {  
                return this.DIAGENESISValue;  
            }  

          set { SetProperty(ref  DIAGENESISValue, value); }
        } 
private  System.Decimal? RESERVOIR_AREAValue; 
 public System.Decimal? RESERVOIR_AREA
        {  
            get  
            {  
                return this.RESERVOIR_AREAValue;  
            }  

          set { SetProperty(ref  RESERVOIR_AREAValue, value); }
        } 
private  System.String RESERVOIR_AREA_OUOMValue; 
 public System.String RESERVOIR_AREA_OUOM
        {  
            get  
            {  
                return this.RESERVOIR_AREA_OUOMValue;  
            }  

          set { SetProperty(ref  RESERVOIR_AREA_OUOMValue, value); }
        } 
private  System.Decimal? RESERVOIR_THICKNESSValue; 
 public System.Decimal? RESERVOIR_THICKNESS
        {  
            get  
            {  
                return this.RESERVOIR_THICKNESSValue;  
            }  

          set { SetProperty(ref  RESERVOIR_THICKNESSValue, value); }
        } 
private  System.String RESERVOIR_THICKNESS_OUOMValue; 
 public System.String RESERVOIR_THICKNESS_OUOM
        {  
            get  
            {  
                return this.RESERVOIR_THICKNESS_OUOMValue;  
            }  

          set { SetProperty(ref  RESERVOIR_THICKNESS_OUOMValue, value); }
        } 
private  System.Decimal? RESERVOIR_VOLUMEValue; 
 public System.Decimal? RESERVOIR_VOLUME
        {  
            get  
            {  
                return this.RESERVOIR_VOLUMEValue;  
            }  

          set { SetProperty(ref  RESERVOIR_VOLUMEValue, value); }
        } 
private  System.String RESERVOIR_VOLUME_OUOMValue; 
 public System.String RESERVOIR_VOLUME_OUOM
        {  
            get  
            {  
                return this.RESERVOIR_VOLUME_OUOMValue;  
            }  

          set { SetProperty(ref  RESERVOIR_VOLUME_OUOMValue, value); }
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

    public PROSPECT_RESERVOIR () { }

  }
}

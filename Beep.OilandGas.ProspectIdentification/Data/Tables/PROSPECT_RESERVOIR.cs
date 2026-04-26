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

        // --- O&G best-practice additions (PVT / structural depth) ---

        private System.String RESERVOIR_NAMEValue;
        /// <summary>Descriptive reservoir name (e.g. Lower Cretaceous Sandstone).</summary>
        public System.String RESERVOIR_NAME
        {
            get { return this.RESERVOIR_NAMEValue; }
            set { SetProperty(ref RESERVOIR_NAMEValue, value); }
        }

        private System.String FLUID_TYPEValue;
        /// <summary>Primary fluid phase in reservoir: OIL / GAS / CONDENSATE / CBM.</summary>
        public System.String FLUID_TYPE
        {
            get { return this.FLUID_TYPEValue; }
            set { SetProperty(ref FLUID_TYPEValue, value); }
        }

        private System.Decimal? TEMPERATUREValue;
        /// <summary>Reservoir temperature (bottom-hole static temperature).</summary>
        public System.Decimal? TEMPERATURE
        {
            get { return this.TEMPERATUREValue; }
            set { SetProperty(ref TEMPERATUREValue, value); }
        }

        private System.String TEMPERATURE_OUOMValue;
        public System.String TEMPERATURE_OUOM
        {
            get { return this.TEMPERATURE_OUOMValue; }
            set { SetProperty(ref TEMPERATURE_OUOMValue, value); }
        }

        private System.Decimal? PRESSUREValue;
        /// <summary>Initial reservoir pressure (BHSP).</summary>
        public System.Decimal? PRESSURE
        {
            get { return this.PRESSUREValue; }
            set { SetProperty(ref PRESSUREValue, value); }
        }

        private System.String PRESSURE_OUOMValue;
        public System.String PRESSURE_OUOM
        {
            get { return this.PRESSURE_OUOMValue; }
            set { SetProperty(ref PRESSURE_OUOMValue, value); }
        }

        private System.Decimal? WATER_SATURATIONValue;
        /// <summary>Connate water saturation Sw (fraction 0–1).</summary>
        public System.Decimal? WATER_SATURATION
        {
            get { return this.WATER_SATURATIONValue; }
            set { SetProperty(ref WATER_SATURATIONValue, value); }
        }

        private System.Decimal? OIL_API_GRAVITYValue;
        /// <summary>Oil API gravity (°API).</summary>
        public System.Decimal? OIL_API_GRAVITY
        {
            get { return this.OIL_API_GRAVITYValue; }
            set { SetProperty(ref OIL_API_GRAVITYValue, value); }
        }

        private System.Decimal? GAS_GRAVITYValue;
        /// <summary>Gas specific gravity (air = 1.0).</summary>
        public System.Decimal? GAS_GRAVITY
        {
            get { return this.GAS_GRAVITYValue; }
            set { SetProperty(ref GAS_GRAVITYValue, value); }
        }

        private System.Decimal? DEPTH_TO_TOPValue;
        /// <summary>Depth to top of reservoir (subsea or subsurface).</summary>
        public System.Decimal? DEPTH_TO_TOP
        {
            get { return this.DEPTH_TO_TOPValue; }
            set { SetProperty(ref DEPTH_TO_TOPValue, value); }
        }

        private System.Decimal? DEPTH_TO_BASEValue;
        public System.Decimal? DEPTH_TO_BASE
        {
            get { return this.DEPTH_TO_BASEValue; }
            set { SetProperty(ref DEPTH_TO_BASEValue, value); }
        }

        private System.String DEPTH_OUOMValue;
        public System.String DEPTH_OUOM
        {
            get { return this.DEPTH_OUOMValue; }
            set { SetProperty(ref DEPTH_OUOMValue, value); }
        }

        private System.Decimal? GAS_OIL_RATIOValue;
        /// <summary>Solution gas-oil ratio GOR (scf/bbl or m³/m³).</summary>
        public System.Decimal? GAS_OIL_RATIO
        {
            get { return this.GAS_OIL_RATIOValue; }
            set { SetProperty(ref GAS_OIL_RATIOValue, value); }
        }

        private System.String GOR_OUOMValue;
        public System.String GOR_OUOM
        {
            get { return this.GOR_OUOMValue; }
            set { SetProperty(ref GOR_OUOMValue, value); }
        }

        private System.Decimal? FORMATION_VOLUME_FACTORValue;
        /// <summary>Oil formation volume factor Bo (res bbl/STB) or gas Bg (res cf/SCF).</summary>
        public System.Decimal? FORMATION_VOLUME_FACTOR
        {
            get { return this.FORMATION_VOLUME_FACTORValue; }
            set { SetProperty(ref FORMATION_VOLUME_FACTORValue, value); }
        }

    public PROSPECT_RESERVOIR () { }

  }
}

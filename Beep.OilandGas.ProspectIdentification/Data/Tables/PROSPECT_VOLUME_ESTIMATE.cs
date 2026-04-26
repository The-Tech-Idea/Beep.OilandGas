using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
public partial class PROSPECT_VOLUME_ESTIMATE : ModelEntityBase

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
private  System.String ESTIMATE_IDValue; 
 public System.String ESTIMATE_ID
        {  
            get  
            {  
                return this.ESTIMATE_IDValue;  
            }  

          set { SetProperty(ref  ESTIMATE_IDValue, value); }
        } 
private  System.Decimal  OIL_VOLUME_P10Value; 
 public System.Decimal  OIL_VOLUME_P10
        {  
            get  
            {  
                return this.OIL_VOLUME_P10Value;  
            }  

          set { SetProperty(ref  OIL_VOLUME_P10Value, value); }
        } 
private  System.Decimal  OIL_VOLUME_P50Value; 
 public System.Decimal  OIL_VOLUME_P50
        {  
            get  
            {  
                return this.OIL_VOLUME_P50Value;  
            }  

          set { SetProperty(ref  OIL_VOLUME_P50Value, value); }
        } 
private  System.Decimal  OIL_VOLUME_P90Value; 
 public System.Decimal  OIL_VOLUME_P90
        {  
            get  
            {  
                return this.OIL_VOLUME_P90Value;  
            }  

          set { SetProperty(ref  OIL_VOLUME_P90Value, value); }
        } 
private  System.Decimal  GAS_VOLUME_P10Value; 
 public System.Decimal  GAS_VOLUME_P10
        {  
            get  
            {  
                return this.GAS_VOLUME_P10Value;  
            }  

          set { SetProperty(ref  GAS_VOLUME_P10Value, value); }
        } 
private  System.Decimal  GAS_VOLUME_P50Value; 
 public System.Decimal  GAS_VOLUME_P50
        {  
            get  
            {  
                return this.GAS_VOLUME_P50Value;  
            }  

          set { SetProperty(ref  GAS_VOLUME_P50Value, value); }
        } 
private  System.Decimal  GAS_VOLUME_P90Value; 
 public System.Decimal  GAS_VOLUME_P90
        {  
            get  
            {  
                return this.GAS_VOLUME_P90Value;  
            }  

          set { SetProperty(ref  GAS_VOLUME_P90Value, value); }
        } 
private  System.String VOLUME_OUOMValue; 
 public System.String VOLUME_OUOM
        {  
            get  
            {  
                return this.VOLUME_OUOMValue;  
            }  

          set { SetProperty(ref  VOLUME_OUOMValue, value); }
        } 
private  System.Decimal  UNRISKED_OIL_VOLUMEValue; 
 public System.Decimal  UNRISKED_OIL_VOLUME
        {  
            get  
            {  
                return this.UNRISKED_OIL_VOLUMEValue;  
            }  

          set { SetProperty(ref  UNRISKED_OIL_VOLUMEValue, value); }
        } 
private  System.Decimal  UNRISKED_GAS_VOLUMEValue; 
 public System.Decimal  UNRISKED_GAS_VOLUME
        {  
            get  
            {  
                return this.UNRISKED_GAS_VOLUMEValue;  
            }  

          set { SetProperty(ref  UNRISKED_GAS_VOLUMEValue, value); }
        } 
private  System.Decimal  RISKED_OIL_VOLUMEValue; 
 public System.Decimal  RISKED_OIL_VOLUME
        {  
            get  
            {  
                return this.RISKED_OIL_VOLUMEValue;  
            }  

          set { SetProperty(ref  RISKED_OIL_VOLUMEValue, value); }
        } 
private  System.Decimal  RISKED_GAS_VOLUMEValue; 
 public System.Decimal  RISKED_GAS_VOLUME
        {  
            get  
            {  
                return this.RISKED_GAS_VOLUMEValue;  
            }  

          set { SetProperty(ref  RISKED_GAS_VOLUMEValue, value); }
        } 
private  System.Decimal  RECOVERY_FACTORValue; 
 public System.Decimal  RECOVERY_FACTOR
        {  
            get  
            {  
                return this.RECOVERY_FACTORValue;  
            }  

          set { SetProperty(ref  RECOVERY_FACTORValue, value); }
        } 
private  System.Decimal  FORMATION_VOLUME_FACTORValue; 
 public System.Decimal  FORMATION_VOLUME_FACTOR
        {  
            get  
            {  
                return this.FORMATION_VOLUME_FACTORValue;  
            }  

          set { SetProperty(ref  FORMATION_VOLUME_FACTORValue, value); }
        } 
private  System.String ESTIMATE_METHODValue; 
 public System.String ESTIMATE_METHOD
        {  
            get  
            {  
                return this.ESTIMATE_METHODValue;  
            }  

          set { SetProperty(ref  ESTIMATE_METHODValue, value); }
        } 
private  System.DateTime? ESTIMATE_DATEValue; 
 public System.DateTime? ESTIMATE_DATE
        {  
            get  
            {  
                return this.ESTIMATE_DATEValue;  
            }  

          set { SetProperty(ref  ESTIMATE_DATEValue, value); }
        } 
private  System.String ESTIMATORValue; 
 public System.String ESTIMATOR
        {  
            get  
            {  
                return this.ESTIMATORValue;  
            }  

          set { SetProperty(ref  ESTIMATORValue, value); }
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

        // --- O&G best-practice additions (SPE PRMS 2018 resource classification) ---

        private System.String ESTIMATE_TYPEValue;
        /// <summary>SPE PRMS resource category: GRV / STOIIP / GIIP / RECOVERABLE / EUR / CONTINGENT.</summary>
        public System.String ESTIMATE_TYPE
        {
            get { return this.ESTIMATE_TYPEValue; }
            set { SetProperty(ref ESTIMATE_TYPEValue, value); }
        }

        private System.String FLUID_TYPEValue;
        /// <summary>Fluid type these volumes relate to: OIL / GAS / CONDENSATE / NGL / MIXED.</summary>
        public System.String FLUID_TYPE
        {
            get { return this.FLUID_TYPEValue; }
            set { SetProperty(ref FLUID_TYPEValue, value); }
        }

        private System.Decimal? OIL_VOLUME_MEANValue;
        /// <summary>Mean (expected value) oil volume estimate alongside probabilistic P10/P50/P90 set.</summary>
        public System.Decimal? OIL_VOLUME_MEAN
        {
            get { return this.OIL_VOLUME_MEANValue; }
            set { SetProperty(ref OIL_VOLUME_MEANValue, value); }
        }

        private System.Decimal? GAS_VOLUME_MEANValue;
        /// <summary>Mean (expected value) gas volume estimate.</summary>
        public System.Decimal? GAS_VOLUME_MEAN
        {
            get { return this.GAS_VOLUME_MEANValue; }
            set { SetProperty(ref GAS_VOLUME_MEANValue, value); }
        }

        private System.Decimal? CONDENSATE_VOLUME_P50Value;
        /// <summary>P50 condensate/NGL volume estimate (important for wet-gas plays).</summary>
        public System.Decimal? CONDENSATE_VOLUME_P50
        {
            get { return this.CONDENSATE_VOLUME_P50Value; }
            set { SetProperty(ref CONDENSATE_VOLUME_P50Value, value); }
        }

        private System.Decimal? BOE_EQUIVALENT_P50Value;
        /// <summary>P50 BOE equivalent volume for portfolio comparison (oil + gas/6 + condensate).</summary>
        public System.Decimal? BOE_EQUIVALENT_P50
        {
            get { return this.BOE_EQUIVALENT_P50Value; }
            set { SetProperty(ref BOE_EQUIVALENT_P50Value, value); }
        }

    public PROSPECT_VOLUME_ESTIMATE () { }

  }
}

using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
public partial class PLAY : ModelEntityBase

{

private  System.String PLAY_IDValue; 
 public System.String PLAY_ID
        {  
            get  
            {  
                return this.PLAY_IDValue;  
            }  

          set { SetProperty(ref  PLAY_IDValue, value); }
        } 
private  System.String PLAY_NAMEValue; 
 public System.String PLAY_NAME
        {  
            get  
            {  
                return this.PLAY_NAMEValue;  
            }  

          set { SetProperty(ref  PLAY_NAMEValue, value); }
        } 
private  System.String PLAY_SHORT_NAMEValue; 
 public System.String PLAY_SHORT_NAME
        {  
            get  
            {  
                return this.PLAY_SHORT_NAMEValue;  
            }  

          set { SetProperty(ref  PLAY_SHORT_NAMEValue, value); }
        } 
private  System.String FIELD_IDValue; 
 public System.String FIELD_ID
        {  
            get  
            {  
                return this.FIELD_IDValue;  
            }  

          set { SetProperty(ref  FIELD_IDValue, value); }
        } 
private  System.String PLAY_TYPEValue; 
 public System.String PLAY_TYPE
        {  
            get  
            {  
                return this.PLAY_TYPEValue;  
            }  

          set { SetProperty(ref  PLAY_TYPEValue, value); }
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
private  System.String GEOLOGICAL_SETTINGValue; 
 public System.String GEOLOGICAL_SETTING
        {  
            get  
            {  
                return this.GEOLOGICAL_SETTINGValue;  
            }  

          set { SetProperty(ref  GEOLOGICAL_SETTINGValue, value); }
        } 
private  System.String GEOLOGICAL_AGEValue; 
 public System.String GEOLOGICAL_AGE
        {  
            get  
            {  
                return this.GEOLOGICAL_AGEValue;  
            }  

          set { SetProperty(ref  GEOLOGICAL_AGEValue, value); }
        } 
private  System.Decimal? PLAY_EXTENT_AREAValue; 
 public System.Decimal? PLAY_EXTENT_AREA
        {  
            get  
            {  
                return this.PLAY_EXTENT_AREAValue;  
            }  

          set { SetProperty(ref  PLAY_EXTENT_AREAValue, value); }
        } 
private  System.String PLAY_EXTENT_AREA_OUOMValue; 
 public System.String PLAY_EXTENT_AREA_OUOM
        {  
            get  
            {  
                return this.PLAY_EXTENT_AREA_OUOMValue;  
            }  

          set { SetProperty(ref  PLAY_EXTENT_AREA_OUOMValue, value); }
        } 
private  System.Decimal? PROVEN_RESERVES_OILValue; 
 public System.Decimal? PROVEN_RESERVES_OIL
        {  
            get  
            {  
                return this.PROVEN_RESERVES_OILValue;  
            }  

          set { SetProperty(ref  PROVEN_RESERVES_OILValue, value); }
        } 
private  System.Decimal? PROVEN_RESERVES_GASValue; 
 public System.Decimal? PROVEN_RESERVES_GAS
        {  
            get  
            {  
                return this.PROVEN_RESERVES_GASValue;  
            }  

          set { SetProperty(ref  PROVEN_RESERVES_GASValue, value); }
        } 
private  System.String PROVEN_RESERVES_OUOMValue; 
 public System.String PROVEN_RESERVES_OUOM
        {  
            get  
            {  
                return this.PROVEN_RESERVES_OUOMValue;  
            }  

          set { SetProperty(ref  PROVEN_RESERVES_OUOMValue, value); }
        } 
private  System.Decimal? LATITUDEValue; 
 public System.Decimal? LATITUDE
        {  
            get  
            {  
                return this.LATITUDEValue;  
            }  

          set { SetProperty(ref  LATITUDEValue, value); }
        } 
private  System.Decimal? LONGITUDEValue; 
 public System.Decimal? LONGITUDE
        {  
            get  
            {  
                return this.LONGITUDEValue;  
            }  

          set { SetProperty(ref  LONGITUDEValue, value); }
        } 
private  System.Decimal? ELEVATIONValue; 
 public System.Decimal? ELEVATION
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
private  System.DateTime? EFFECTIVE_DATEValue; 

private  System.DateTime? EXPIRY_DATEValue; 

private  System.String REMARKValue; 

private  System.String SOURCEValue; 

        // --- O&G best-practice additions (PPDM 3.9 / play analysis) ---

        private System.String BASIN_IDValue;
        /// <summary>Basin identifier — links play to basin context for analogy work.</summary>
        public System.String BASIN_ID
        {
            get { return this.BASIN_IDValue; }
            set { SetProperty(ref BASIN_IDValue, value); }
        }

        private System.String PROVINCE_NAMEValue;
        /// <summary>Geological province name (e.g. Permian Basin, North Sea Central Graben).</summary>
        public System.String PROVINCE_NAME
        {
            get { return this.PROVINCE_NAMEValue; }
            set { SetProperty(ref PROVINCE_NAMEValue, value); }
        }

        private System.String SEAL_TYPEValue;
        /// <summary>Dominant seal type: EVAPORITE / SHALE / TIGHT_SANDSTONE / FAULT / COMBINATION.</summary>
        public System.String SEAL_TYPE
        {
            get { return this.SEAL_TYPEValue; }
            set { SetProperty(ref SEAL_TYPEValue, value); }
        }

        private System.String FLUID_TYPEValue;
        /// <summary>Expected primary fluid: OIL / GAS / CONDENSATE / CBM.</summary>
        public System.String FLUID_TYPE
        {
            get { return this.FLUID_TYPEValue; }
            set { SetProperty(ref FLUID_TYPEValue, value); }
        }

        private System.String MATURITY_STATUSValue;
        /// <summary>Play lifecycle maturity: EMERGING / IMMATURE / MATURE / DECLINING / ABANDONED.</summary>
        public System.String MATURITY_STATUS
        {
            get { return this.MATURITY_STATUSValue; }
            set { SetProperty(ref MATURITY_STATUSValue, value); }
        }

        private System.Decimal? PLAY_RISKValue;
        /// <summary>Overall play-level risk factor (0.0 = no risk, 1.0 = maximum risk).</summary>
        public System.Decimal? PLAY_RISK
        {
            get { return this.PLAY_RISKValue; }
            set { SetProperty(ref PLAY_RISKValue, value); }
        }

    public PLAY () { }

  }
}

using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
public partial class PROSPECT : ModelEntityBase

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
private  System.String PROSPECT_NAMEValue; 
 public System.String PROSPECT_NAME
        {  
            get  
            {  
                return this.PROSPECT_NAMEValue;  
            }  

          set { SetProperty(ref  PROSPECT_NAMEValue, value); }
        } 
private  System.String PROSPECT_SHORT_NAMEValue; 
 public System.String PROSPECT_SHORT_NAME
        {  
            get  
            {  
                return this.PROSPECT_SHORT_NAMEValue;  
            }  

          set { SetProperty(ref  PROSPECT_SHORT_NAMEValue, value); }
        } 
private  System.String PRIMARY_FIELD_IDValue; 
 public System.String PRIMARY_FIELD_ID
        {  
            get  
            {  
                return this.PRIMARY_FIELD_IDValue;  
            }  

          set { SetProperty(ref  PRIMARY_FIELD_IDValue, value); }
        } 
private  System.String PLAY_IDValue; 
 public System.String PLAY_ID
        {  
            get  
            {  
                return this.PLAY_IDValue;  
            }  

          set { SetProperty(ref  PLAY_IDValue, value); }
        } 
private  System.String PROSPECT_TYPEValue; 
 public System.String PROSPECT_TYPE
        {  
            get  
            {  
                return this.PROSPECT_TYPEValue;  
            }  

          set { SetProperty(ref  PROSPECT_TYPEValue, value); }
        } 
private  System.String PROSPECT_STATUSValue; 
 public System.String PROSPECT_STATUS
        {  
            get  
            {  
                return this.PROSPECT_STATUSValue;  
            }  

          set { SetProperty(ref  PROSPECT_STATUSValue, value); }
        } 
private  System.String RISK_LEVELValue; 
 public System.String RISK_LEVEL
        {  
            get  
            {  
                return this.RISK_LEVELValue;  
            }  

          set { SetProperty(ref  RISK_LEVELValue, value); }
        } 
private  System.String CURRENT_OPERATORValue; 
 public System.String CURRENT_OPERATOR
        {  
            get  
            {  
                return this.CURRENT_OPERATORValue;  
            }  

          set { SetProperty(ref  CURRENT_OPERATORValue, value); }
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
private  System.DateTime? DISCOVERY_DATEValue; 
 public System.DateTime? DISCOVERY_DATE
        {  
            get  
            {  
                return this.DISCOVERY_DATEValue;  
            }  

          set { SetProperty(ref  DISCOVERY_DATEValue, value); }
        } 
private  System.DateTime? FIRST_DRILL_DATEValue; 
 public System.DateTime? FIRST_DRILL_DATE
        {  
            get  
            {  
                return this.FIRST_DRILL_DATEValue;  
            }  

          set { SetProperty(ref  FIRST_DRILL_DATEValue, value); }
        } 
private  System.DateTime? COMPLETION_DATEValue; 
 public System.DateTime? COMPLETION_DATE
        {  
            get  
            {  
                return this.COMPLETION_DATEValue;  
            }  

          set { SetProperty(ref  COMPLETION_DATEValue, value); }
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
private  System.String LOCATION_DESCRIPTIONValue; 
 public System.String LOCATION_DESCRIPTION
        {  
            get  
            {  
                return this.LOCATION_DESCRIPTIONValue;  
            }  

          set { SetProperty(ref  LOCATION_DESCRIPTIONValue, value); }
        } 
private  System.Decimal? TOP_DEPTHValue; 
 public System.Decimal? TOP_DEPTH
        {  
            get  
            {  
                return this.TOP_DEPTHValue;  
            }  

          set { SetProperty(ref  TOP_DEPTHValue, value); }
        } 
private  System.Decimal? BASE_DEPTHValue; 
 public System.Decimal? BASE_DEPTH
        {  
            get  
            {  
                return this.BASE_DEPTHValue;  
            }  

          set { SetProperty(ref  BASE_DEPTHValue, value); }
        } 
private  System.String DEPTH_OUOMValue; 
 public System.String DEPTH_OUOM
        {  
            get  
            {  
                return this.DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  DEPTH_OUOMValue, value); }
        } 
private  System.Decimal? ESTIMATED_OIL_VOLUMEValue; 
 public System.Decimal? ESTIMATED_OIL_VOLUME
        {  
            get  
            {  
                return this.ESTIMATED_OIL_VOLUMEValue;  
            }  

          set { SetProperty(ref  ESTIMATED_OIL_VOLUMEValue, value); }
        } 
private  System.Decimal? ESTIMATED_GAS_VOLUMEValue; 
 public System.Decimal? ESTIMATED_GAS_VOLUME
        {  
            get  
            {  
                return this.ESTIMATED_GAS_VOLUMEValue;  
            }  

          set { SetProperty(ref  ESTIMATED_GAS_VOLUMEValue, value); }
        } 
private  System.String ESTIMATED_VOLUME_OUOMValue; 
 public System.String ESTIMATED_VOLUME_OUOM
        {  
            get  
            {  
                return this.ESTIMATED_VOLUME_OUOMValue;  
            }  

          set { SetProperty(ref  ESTIMATED_VOLUME_OUOMValue, value); }
        } 
private  System.Decimal? ESTIMATED_VALUEValue; 
 public System.Decimal? ESTIMATED_VALUE
        {  
            get  
            {  
                return this.ESTIMATED_VALUEValue;  
            }  

          set { SetProperty(ref  ESTIMATED_VALUEValue, value); }
        } 
private  System.String ESTIMATED_VALUE_CURRENCYValue; 
 public System.String ESTIMATED_VALUE_CURRENCY
        {  
            get  
            {  
                return this.ESTIMATED_VALUE_CURRENCYValue;  
            }  

          set { SetProperty(ref  ESTIMATED_VALUE_CURRENCYValue, value); }
        } 
private  System.String FORMATION_NAMEValue; 
 public System.String FORMATION_NAME
        {  
            get  
            {  
                return this.FORMATION_NAMEValue;  
            }  

          set { SetProperty(ref  FORMATION_NAMEValue, value); }
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
private  System.DateTime? EFFECTIVE_DATEValue; 

private  System.DateTime? EXPIRY_DATEValue; 

private  System.String REMARKValue; 

private  System.String SOURCEValue; 

        private System.DateTime? EVALUATION_DATEValue;
        public System.DateTime? EVALUATION_DATE
        {
            get { return this.EVALUATION_DATEValue; }
            set { SetProperty(ref EVALUATION_DATEValue, value); }
        }

        private System.String FIELD_IDValue = string.Empty;
        public System.String FIELD_ID
        {
            get { return this.FIELD_IDValue; }
            set { SetProperty(ref FIELD_IDValue, value); }
        }

        private System.Decimal? ESTIMATED_RESERVESValue;
        public System.Decimal? ESTIMATED_RESERVES
        {
            get { return this.ESTIMATED_RESERVESValue; }
            set { SetProperty(ref ESTIMATED_RESERVESValue, value); }
        }

        private System.Decimal? RISK_FACTORValue;
        public System.Decimal? RISK_FACTOR
        {
            get { return this.RISK_FACTORValue; }
            set { SetProperty(ref RISK_FACTORValue, value); }
        }

        private System.String STATUSValue = string.Empty;
        public System.String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private System.Decimal? AREA_SIZEValue;
        public System.Decimal? AREA_SIZE
        {
            get { return this.AREA_SIZEValue; }
            set { SetProperty(ref AREA_SIZEValue, value); }
        }

        private System.String AREA_SIZE_OUOMValue = string.Empty;
        public System.String AREA_SIZE_OUOM
        {
            get { return this.AREA_SIZE_OUOMValue; }
            set { SetProperty(ref AREA_SIZE_OUOMValue, value); }
        }

        // --- O&G best-practice additions (SPE PRMS / PPDM 3.9 / Lahee) ---

        private System.String LEAD_IDValue = string.Empty;
        /// <summary>FK to LEAD that was promoted to this prospect (lifecycle traceability).</summary>
        public System.String LEAD_ID
        {
            get { return this.LEAD_IDValue; }
            set { SetProperty(ref LEAD_IDValue, value); }
        }

        private System.String PROSPECT_CATEGORYValue = string.Empty;
        /// <summary>SPE PRMS lifecycle category: LEAD / PROSPECT / DISCOVERY / DEVELOPMENT.</summary>
        public System.String PROSPECT_CATEGORY
        {
            get { return this.PROSPECT_CATEGORYValue; }
            set { SetProperty(ref PROSPECT_CATEGORYValue, value); }
        }

        private System.String PRIMARY_FLUID_TYPEValue = string.Empty;
        /// <summary>Expected primary fluid: OIL / GAS / CONDENSATE / CBM / MIXED.</summary>
        public System.String PRIMARY_FLUID_TYPE
        {
            get { return this.PRIMARY_FLUID_TYPEValue; }
            set { SetProperty(ref PRIMARY_FLUID_TYPEValue, value); }
        }

        private System.String PROSPECT_CLASSIFICATIONValue = string.Empty;
        /// <summary>Lahee classification: A / B1 / B2 / C1 / C2 / D1 / D2.</summary>
        public System.String PROSPECT_CLASSIFICATION
        {
            get { return this.PROSPECT_CLASSIFICATIONValue; }
            set { SetProperty(ref PROSPECT_CLASSIFICATIONValue, value); }
        }

        private System.String SEISMIC_COVERAGE_INDValue = string.Empty;
        /// <summary>Y/N — seismic data supports prospect definition.</summary>
        public System.String SEISMIC_COVERAGE_IND
        {
            get { return this.SEISMIC_COVERAGE_INDValue; }
            set { SetProperty(ref SEISMIC_COVERAGE_INDValue, value); }
        }

        private System.Decimal? RANK_VALUEValue;
        /// <summary>Numeric portfolio ranking score (higher = higher priority).</summary>
        public System.Decimal? RANK_VALUE
        {
            get { return this.RANK_VALUEValue; }
            set { SetProperty(ref RANK_VALUEValue, value); }
        }

        private System.String RESPONSIBLE_BA_IDValue;
        /// <summary>PPDM 3.9: Business associate responsible for this prospect (e.g. geologist, geophysicist).</summary>
        public System.String RESPONSIBLE_BA_ID
        {
            get { return this.RESPONSIBLE_BA_IDValue; }
            set { SetProperty(ref RESPONSIBLE_BA_IDValue, value); }
        }

        public PROSPECT() { }
  }
}

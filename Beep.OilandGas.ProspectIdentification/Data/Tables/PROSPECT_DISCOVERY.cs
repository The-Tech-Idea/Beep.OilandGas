using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
public partial class PROSPECT_DISCOVERY : ModelEntityBase

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
private  System.String DISCOVERY_IDValue; 
 public System.String DISCOVERY_ID
        {  
            get  
            {  
                return this.DISCOVERY_IDValue;  
            }  

          set { SetProperty(ref  DISCOVERY_IDValue, value); }
        } 
private  System.String DISCOVERY_TYPEValue; 
 public System.String DISCOVERY_TYPE
        {  
            get  
            {  
                return this.DISCOVERY_TYPEValue;  
            }  

          set { SetProperty(ref  DISCOVERY_TYPEValue, value); }
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
private  System.String DISCOVERY_WELLValue; 
 public System.String DISCOVERY_WELL
        {  
            get  
            {  
                return this.DISCOVERY_WELLValue;  
            }  

          set { SetProperty(ref  DISCOVERY_WELLValue, value); }
        } 
private  System.Decimal? VOLUMES_DISCOVERED_OILValue; 
 public System.Decimal? VOLUMES_DISCOVERED_OIL
        {  
            get  
            {  
                return this.VOLUMES_DISCOVERED_OILValue;  
            }  

          set { SetProperty(ref  VOLUMES_DISCOVERED_OILValue, value); }
        } 
private  System.Decimal? VOLUMES_DISCOVERED_GASValue; 
 public System.Decimal? VOLUMES_DISCOVERED_GAS
        {  
            get  
            {  
                return this.VOLUMES_DISCOVERED_GASValue;  
            }  

          set { SetProperty(ref  VOLUMES_DISCOVERED_GASValue, value); }
        } 
private  System.String VOLUMES_DISCOVERED_OUOMValue; 
 public System.String VOLUMES_DISCOVERED_OUOM
        {  
            get  
            {  
                return this.VOLUMES_DISCOVERED_OUOMValue;  
            }  

          set { SetProperty(ref  VOLUMES_DISCOVERED_OUOMValue, value); }
        } 
private  System.String SIGNIFICANCEValue; 
 public System.String SIGNIFICANCE
        {  
            get  
            {  
                return this.SIGNIFICANCEValue;  
            }  

          set { SetProperty(ref  SIGNIFICANCEValue, value); }
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

        // --- O&G best-practice additions (discovery well test / PRMS classification) ---

        private System.String DISCOVERY_WELL_UWIValue;
        /// <summary>UWI of the discovery well (structured link vs free-text DISCOVERY_WELL).</summary>
        public System.String DISCOVERY_WELL_UWI
        {
            get { return this.DISCOVERY_WELL_UWIValue; }
            set { SetProperty(ref DISCOVERY_WELL_UWIValue, value); }
        }

        private System.Decimal? VOLUMES_DISCOVERED_CONDENSATEValue;
        /// <summary>Condensate/NGL volumes discovered (same unit as OIL/GAS volumes).</summary>
        public System.Decimal? VOLUMES_DISCOVERED_CONDENSATE
        {
            get { return this.VOLUMES_DISCOVERED_CONDENSATEValue; }
            set { SetProperty(ref VOLUMES_DISCOVERED_CONDENSATEValue, value); }
        }

        private System.Decimal? FLOW_RATE_OILValue;
        /// <summary>Discovery well test oil flow rate (production test result).</summary>
        public System.Decimal? FLOW_RATE_OIL
        {
            get { return this.FLOW_RATE_OILValue; }
            set { SetProperty(ref FLOW_RATE_OILValue, value); }
        }

        private System.Decimal? FLOW_RATE_GASValue;
        public System.Decimal? FLOW_RATE_GAS
        {
            get { return this.FLOW_RATE_GASValue; }
            set { SetProperty(ref FLOW_RATE_GASValue, value); }
        }

        private System.String FLOW_RATE_OUOMValue;
        public System.String FLOW_RATE_OUOM
        {
            get { return this.FLOW_RATE_OUOMValue; }
            set { SetProperty(ref FLOW_RATE_OUOMValue, value); }
        }

        private System.String RESOURCE_CLASSValue;
        /// <summary>SPE PRMS resource class: PROSPECTIVE / CONTINGENT / RESERVES / UNDISCOVERED.</summary>
        public System.String RESOURCE_CLASS
        {
            get { return this.RESOURCE_CLASSValue; }
            set { SetProperty(ref RESOURCE_CLASSValue, value); }
        }

    public PROSPECT_DISCOVERY () { }

  }
}

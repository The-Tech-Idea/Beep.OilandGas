using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
public partial class PROSPECT_MIGRATION : ModelEntityBase

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
private  System.String MIGRATION_IDValue; 
 public System.String MIGRATION_ID
        {  
            get  
            {  
                return this.MIGRATION_IDValue;  
            }  

          set { SetProperty(ref  MIGRATION_IDValue, value); }
        } 
private  System.String MIGRATION_PATHWAYValue; 
 public System.String MIGRATION_PATHWAY
        {  
            get  
            {  
                return this.MIGRATION_PATHWAYValue;  
            }  

          set { SetProperty(ref  MIGRATION_PATHWAYValue, value); }
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
private  System.Decimal? MIGRATION_EFFICIENCYValue; 
 public System.Decimal? MIGRATION_EFFICIENCY
        {  
            get  
            {  
                return this.MIGRATION_EFFICIENCYValue;  
            }  

          set { SetProperty(ref  MIGRATION_EFFICIENCYValue, value); }
        } 
private  System.String MIGRATION_TIMINGValue; 
 public System.String MIGRATION_TIMING
        {  
            get  
            {  
                return this.MIGRATION_TIMINGValue;  
            }  

          set { SetProperty(ref  MIGRATION_TIMINGValue, value); }
        } 
private  System.String CARRIER_BEDValue; 
 public System.String CARRIER_BED
        {  
            get  
            {  
                return this.CARRIER_BEDValue;  
            }  

          set { SetProperty(ref  CARRIER_BEDValue, value); }
        } 
private  System.Decimal? SEAL_BREACH_RISKValue; 
 public System.Decimal? SEAL_BREACH_RISK
        {  
            get  
            {  
                return this.SEAL_BREACH_RISKValue;  
            }  

          set { SetProperty(ref  SEAL_BREACH_RISKValue, value); }
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

    public PROSPECT_MIGRATION () { }

  }
}

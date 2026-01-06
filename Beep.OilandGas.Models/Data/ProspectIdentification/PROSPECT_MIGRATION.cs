using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
public partial class PROSPECT_MIGRATION: Entity,IPPDMEntity

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


    public PROSPECT_MIGRATION () { }

  }
}




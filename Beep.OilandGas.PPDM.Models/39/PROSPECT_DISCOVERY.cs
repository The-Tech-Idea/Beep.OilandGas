using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class PROSPECT_DISCOVERY: Entity

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


    public PROSPECT_DISCOVERY () { }

  }
}

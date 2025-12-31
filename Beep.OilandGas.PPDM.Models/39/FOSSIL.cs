using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class FOSSIL: Entity,IPPDMEntity

{

private  System.String FOSSIL_IDValue; 
 public System.String FOSSIL_ID
        {  
            get  
            {  
                return this.FOSSIL_IDValue;  
            }  

          set { SetProperty(ref  FOSSIL_IDValue, value); }
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
private  System.String CLIMATE_TYPEValue; 
 public System.String CLIMATE_TYPE
        {  
            get  
            {  
                return this.CLIMATE_TYPEValue;  
            }  

          set { SetProperty(ref  CLIMATE_TYPEValue, value); }
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
private  System.String LIFE_HABITValue; 
 public System.String LIFE_HABIT
        {  
            get  
            {  
                return this.LIFE_HABITValue;  
            }  

          set { SetProperty(ref  LIFE_HABITValue, value); }
        } 
private  System.String LOWER_ECOZONE_IDValue; 
 public System.String LOWER_ECOZONE_ID
        {  
            get  
            {  
                return this.LOWER_ECOZONE_IDValue;  
            }  

          set { SetProperty(ref  LOWER_ECOZONE_IDValue, value); }
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
private  System.String PREFERRED_INDValue; 
 public System.String PREFERRED_IND
        {  
            get  
            {  
                return this.PREFERRED_INDValue;  
            }  

          set { SetProperty(ref  PREFERRED_INDValue, value); }
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
private  System.String TAXON_GROUPValue; 
 public System.String TAXON_GROUP
        {  
            get  
            {  
                return this.TAXON_GROUPValue;  
            }  

          set { SetProperty(ref  TAXON_GROUPValue, value); }
        } 
private  System.String TAXON_LEAF_IDValue; 
 public System.String TAXON_LEAF_ID
        {  
            get  
            {  
                return this.TAXON_LEAF_IDValue;  
            }  

          set { SetProperty(ref  TAXON_LEAF_IDValue, value); }
        } 
private  System.String UPPER_ECOZONE_IDValue; 
 public System.String UPPER_ECOZONE_ID
        {  
            get  
            {  
                return this.UPPER_ECOZONE_IDValue;  
            }  

          set { SetProperty(ref  UPPER_ECOZONE_IDValue, value); }
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


    public FOSSIL () { }

  }
}


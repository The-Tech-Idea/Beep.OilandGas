using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.Models.Data 
{
public partial class PROSPECT_TRAP: Entity,IPPDMEntity

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
private  System.String TRAP_IDValue; 
 public System.String TRAP_ID
        {  
            get  
            {  
                return this.TRAP_IDValue;  
            }  

          set { SetProperty(ref  TRAP_IDValue, value); }
        } 
private  System.String TRAP_TYPEValue; 
 public System.String TRAP_TYPE
        {  
            get  
            {  
                return this.TRAP_TYPEValue;  
            }  

          set { SetProperty(ref  TRAP_TYPEValue, value); }
        } 
private  System.String TRAP_GEOMETRYValue; 
 public System.String TRAP_GEOMETRY
        {  
            get  
            {  
                return this.TRAP_GEOMETRYValue;  
            }  

          set { SetProperty(ref  TRAP_GEOMETRYValue, value); }
        } 
private  System.Decimal? CLOSURE_AREAValue; 
 public System.Decimal? CLOSURE_AREA
        {  
            get  
            {  
                return this.CLOSURE_AREAValue;  
            }  

          set { SetProperty(ref  CLOSURE_AREAValue, value); }
        } 
private  System.String CLOSURE_AREA_OUOMValue; 
 public System.String CLOSURE_AREA_OUOM
        {  
            get  
            {  
                return this.CLOSURE_AREA_OUOMValue;  
            }  

          set { SetProperty(ref  CLOSURE_AREA_OUOMValue, value); }
        } 
private  System.Decimal? CLOSURE_HEIGHTValue; 
 public System.Decimal? CLOSURE_HEIGHT
        {  
            get  
            {  
                return this.CLOSURE_HEIGHTValue;  
            }  

          set { SetProperty(ref  CLOSURE_HEIGHTValue, value); }
        } 
private  System.String CLOSURE_HEIGHT_OUOMValue; 
 public System.String CLOSURE_HEIGHT_OUOM
        {  
            get  
            {  
                return this.CLOSURE_HEIGHT_OUOMValue;  
            }  

          set { SetProperty(ref  CLOSURE_HEIGHT_OUOMValue, value); }
        } 
private  System.String SEAL_QUALITYValue; 
 public System.String SEAL_QUALITY
        {  
            get  
            {  
                return this.SEAL_QUALITYValue;  
            }  

          set { SetProperty(ref  SEAL_QUALITYValue, value); }
        } 
private  System.Decimal? SEAL_THICKNESSValue; 
 public System.Decimal? SEAL_THICKNESS
        {  
            get  
            {  
                return this.SEAL_THICKNESSValue;  
            }  

          set { SetProperty(ref  SEAL_THICKNESSValue, value); }
        } 
private  System.String SEAL_THICKNESS_OUOMValue; 
 public System.String SEAL_THICKNESS_OUOM
        {  
            get  
            {  
                return this.SEAL_THICKNESS_OUOMValue;  
            }  

          set { SetProperty(ref  SEAL_THICKNESS_OUOMValue, value); }
        } 
private  System.Decimal? TRAP_EFFECTIVENESSValue; 
 public System.Decimal? TRAP_EFFECTIVENESS
        {  
            get  
            {  
                return this.TRAP_EFFECTIVENESSValue;  
            }  

          set { SetProperty(ref  TRAP_EFFECTIVENESSValue, value); }
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


    public PROSPECT_TRAP () { }

  }
}

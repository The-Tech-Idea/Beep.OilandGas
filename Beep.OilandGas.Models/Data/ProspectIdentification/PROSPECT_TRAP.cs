using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
 namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
public partial class PROSPECT_TRAP : ModelEntityBase

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
private  System.DateTime? EFFECTIVE_DATEValue; 

private  System.DateTime? EXPIRY_DATEValue; 

private  System.String REMARKValue; 

private  System.String SOURCEValue; 

    public PROSPECT_TRAP () { }

  }
}



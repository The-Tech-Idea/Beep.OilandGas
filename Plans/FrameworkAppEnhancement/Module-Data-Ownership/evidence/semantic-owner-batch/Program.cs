using Beep.OilandGas.Accounting.Models;
using Beep.OilandGas.Repository.Models;
using Beep.OilandGas.HSE.Models;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
void Require(bool ok,string message){if(!ok)throw new Exception(message);}
foreach(var type in new[]{typeof(CreateInvoiceRequest),typeof(ARAgingSummary),typeof(OilGasUser),typeof(GhgArithmeticRequest)})
{bool refused=false;try{ModuleSchemaBoundary.Validate(new[]{type});}catch(ArgumentException){refused=true;}Require(refused,"DTO/identity admitted: "+type.FullName);}
ModuleSchemaBoundary.Validate(new[]{typeof(AFE_LINE_ITEM)});
Require(typeof(AFE_LINE_ITEM).Assembly.GetName().Name=="Beep.OilandGas.ProductionAccounting","Wrong native owner");
Require(typeof(AFE_LINE_ITEM).GetProperty("AFE_LINE_ITEM_ID")!.IsDefined(typeof(KeyAttribute),true),"Native key missing");
var relation=new PPDMForeignKey{ForeignKeyColumn="FINANCE_ID",ReferencedTable="FINANCE",ReferencedPrimaryKey="FINANCE_ID"};
var rows=new Dictionary<string,PPDMTableMetadata>(StringComparer.OrdinalIgnoreCase){{"ACCOUNTING_COST",new(){TableName="accounting_cost",EntityTypeName="ACCOUNTING_COST",PrimaryKeyColumn="ACCOUNTING_COST_ID",Module="Accounting",ForeignKeys=new(){relation}}}};
WorkOrderFinancialMetadata.AddTo(rows);Require(rows["ACCOUNTING_COST"].ForeignKeys.Single()==relation,"Relationship replaced");Require(rows["AFE_LINE_ITEM"].EntityTypeName==typeof(AFE_LINE_ITEM).FullName,"Native metadata changed");
var foreign=new Dictionary<string,PPDMTableMetadata>{{"ACCOUNTING_COST",new(){TableName="ACCOUNTING_COST",EntityTypeName="foreign.Entity",PrimaryKeyColumn="ACCOUNTING_COST_ID",Module="Accounting"}}};bool rejected=false;try{WorkOrderFinancialMetadata.AddTo(foreign);}catch(InvalidOperationException){rejected=true;}Require(rejected && foreign.Count==1 && !foreign.ContainsKey("AFE"),"Partial metadata mutation");
var json=JsonSerializer.SerializeToElement(new CreateInvoiceRequest{InvoiceNumber="original",CustomerBaId="customer"},new JsonSerializerOptions(JsonSerializerDefaults.Web));Require(json.GetProperty("invoiceNumber").GetString()=="original" && json.GetProperty("customerBaId").GetString()=="customer","Wire fields changed");
foreach(var type in new[]{typeof(Beep.OilandGas.Models.Data.Company.CompanyRequestHeaders),typeof(Beep.OilandGas.Models.Data.DataManagement.FileImportResult),typeof(Beep.OilandGas.Models.Data.HSE.HseIncidentPage)})
{bool refused=false;try{ModuleSchemaBoundary.Validate(new[]{type});}catch(ArgumentException){refused=true;}Require(refused,"Relocated contract admitted: "+type.FullName);}
var import=new Beep.OilandGas.Models.Data.DataManagement.FileImportResult{FilePath="review.csv",TotalRows=2,SuccessCount=1,ErrorCount=1,Errors=new(){new(){RowNumber=2,Message="Invalid input",ColumnName="Value"}}};
Require(!import.IsSuccess,"Import errors shown as success");var importJson=JsonSerializer.SerializeToElement(import,new JsonSerializerOptions(JsonSerializerDefaults.Web));Require(importJson.GetProperty("filePath").GetString()=="review.csv" && !importJson.GetProperty("isSuccess").GetBoolean() && !importJson.TryGetProperty("active_IND",out _),"Import transport fields changed");
Require(typeof(Beep.OilandGas.Models.Data.DataManagement.FileImportResult).Assembly.GetName().Name=="Beep.OilandGas.Models","Wrong import contract owner");
ModuleSchemaBoundary.Validate(new[]{typeof(Beep.OilandGas.Models.Data.HSE.HSE_INCIDENT_CLOSURE)});
Require(typeof(Beep.OilandGas.Models.Data.HSE.HSE_INCIDENT_CLOSURE).Assembly.GetName().Name=="Beep.OilandGas.Models","Wrong core HSE closure contract owner");
var protectedNames=(HashSet<string>)typeof(Beep.OilandGas.PPDM39.Core.PPDMGenericRepository).GetField("ControlledPermitTables",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic)!.GetValue(null)!;
Require(protectedNames.SetEquals(TheTechIdea.Data.OilGas.PermitApplicationSchema.Entities.Select(t=>t.Name)),"Protected permit identities differ from actual manifest");
var policy=typeof(Beep.OilandGas.PPDM39.Core.PPDMGenericRepository).GetMethod("RequiresControlledMutation",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic)!;
foreach(var type in TheTechIdea.Data.OilGas.PermitApplicationSchema.Entities)Require((bool)policy.Invoke(null,new object[]{type,type.Name})!,"Permit table lost controlled mutation policy");
Require(!(bool)policy.Invoke(null,new object[]{typeof(KeyAttribute),"KeyAttribute"})!,"Foreign attribute name treated as permit table");
Console.WriteLine("PASS: 7 contract/identity refusals; native owner/key; metadata preservation/refusal before mutation; invoice wire fields; consolidated import owner/error/wire behavior; core HSE closure owner; exact 12-table permit protection and foreign-name refusal.");

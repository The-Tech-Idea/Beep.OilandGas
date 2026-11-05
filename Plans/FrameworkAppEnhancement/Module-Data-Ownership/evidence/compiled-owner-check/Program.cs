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
Console.WriteLine("PASS: 4 contract/identity refusals; native owner/key; metadata preservation and refusal before mutation; retained invoice wire fields.");
